using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace SWTSharp.TestAdapter;

/// <summary>
/// Custom VSTest executor that runs tests in a separate process with macOS Thread 1 support.
/// This executor launches a test host process that ensures UI operations run on the main thread.
/// </summary>
[ExtensionUri(ExecutorUri)]
public class SWTSharpTestExecutor : ITestExecutor
{
    public const string ExecutorUri = "executor://SWTSharpTestExecutor";

    private bool _cancelled;

    public void Cancel()
    {
        _cancelled = true;
    }

    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (sources == null || frameworkHandle == null)
            return;

        frameworkHandle.SendMessage(TestMessageLevel.Informational,
            "SWTSharp TestAdapter: Starting test run from sources...");

        // Discover tests first, then run them
        var testCases = new List<TestCase>();
        var discoverer = new SWTSharpTestDiscoverer();

        foreach (var source in sources)
        {
            var sink = new TestCaseCollector();
            discoverer.DiscoverTests(
                new[] { source },
                runContext!,
                frameworkHandle,
                sink);

            testCases.AddRange(sink.TestCases);
        }

        RunTests(testCases, runContext, frameworkHandle);
    }

    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        if (tests == null || frameworkHandle == null)
            return;

        var testList = tests.ToList();
        if (testList.Count == 0)
            return;

        frameworkHandle.SendMessage(TestMessageLevel.Informational,
            $"SWTSharp TestAdapter: Running {testList.Count} tests in separate process...");

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                RunTestsInMacOSHost(testList, frameworkHandle);
            }
            else
            {
                RunTestsInDefaultHost(testList, frameworkHandle);
            }
        }
        catch (Exception ex)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error,
                $"SWTSharp TestAdapter: Failed to run tests: {ex.Message}");

            // Report all tests as failed
            foreach (var test in testList)
            {
                frameworkHandle.RecordResult(new TestResult(test)
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = $"Test host failed to start: {ex.Message}"
                });
            }
        }
    }

    private void RunTestsInMacOSHost(List<TestCase> tests, IFrameworkHandle frameworkHandle)
    {
        frameworkHandle.SendMessage(TestMessageLevel.Informational,
            "SWTSharp TestAdapter: Using macOS test host with Thread 1 support");

        // Launch test host process
        var testHostPath = GetTestHostPath();
        var testAssembly = tests.First().Source;

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{testHostPath}\" \"{testAssembly}\" {GetTestFilter(tests)}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        // Capture output for result parsing
        var output = new StringBuilder();
        var errors = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
                ParseTestResult(e.Data, tests, frameworkHandle);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                errors.AppendLine(e.Data);
                frameworkHandle.SendMessage(TestMessageLevel.Error, e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for completion or cancellation
        while (!process.HasExited)
        {
            if (_cancelled)
            {
                process.Kill();
                frameworkHandle.SendMessage(TestMessageLevel.Warning,
                    "SWTSharp TestAdapter: Test run cancelled");
                break;
            }

            Thread.Sleep(100);
        }

        process.WaitForExit();

        if (process.ExitCode != 0 && !_cancelled)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error,
                $"SWTSharp TestAdapter: Test host exited with code {process.ExitCode}");
        }
    }

    private void RunTestsInDefaultHost(List<TestCase> tests, IFrameworkHandle frameworkHandle)
    {
        frameworkHandle.SendMessage(TestMessageLevel.Informational,
            "SWTSharp TestAdapter: Using default test host (Windows/Linux)");

        // Group tests by source assembly
        var testsBySource = tests.GroupBy(t => t.Source).ToList();

        foreach (var sourceGroup in testsBySource)
        {
            if (_cancelled)
                break;

            var source = sourceGroup.Key;
            var sourceTests = sourceGroup.ToList();

            frameworkHandle.SendMessage(TestMessageLevel.Informational,
                $"SWTSharp TestAdapter: Running {sourceTests.Count} tests from {Path.GetFileName(source)}");

            try
            {
                // Use XunitFrontController to discover and run tests in-process
                using var controller = new XunitFrontController(
                    AppDomainSupport.Denied,
                    source,
                    configFileName: null,
                    shadowCopy: false,
                    diagnosticMessageSink: new NullMessageSink());

                // Discover tests
                using var discoveryVisitor = new TestDiscoveryVisitor();
                var discoveryOptions = TestFrameworkOptions.ForDiscovery();
                controller.Find(
                    includeSourceInformation: false,
                    messageSink: discoveryVisitor,
                    discoveryOptions: discoveryOptions);

                if (!discoveryVisitor.Finished.WaitOne(TimeSpan.FromSeconds(30)))
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Error,
                        "SWTSharp TestAdapter: Test discovery timed out");
                    MarkTestsAsFailed(sourceTests, frameworkHandle, "Test discovery timed out");
                    continue;
                }

                // Filter discovered tests to match requested test cases
                var testNamesToRun = new HashSet<string>(sourceTests.Select(t => t.FullyQualifiedName));
                var testsToRun = discoveryVisitor.TestCases
                    .Where(tc => testNamesToRun.Contains(tc.TestMethod.TestClass.Class.Name + "." + tc.TestMethod.Method.Name)
                              || testNamesToRun.Any(n => n.Contains(tc.DisplayName)))
                    .ToList();

                if (testsToRun.Count == 0)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Warning,
                        "SWTSharp TestAdapter: No tests matched filter, running all discovered tests");
                    testsToRun = discoveryVisitor.TestCases;
                }

                // Run tests
                using var executionVisitor = new TestExecutionVisitor(sourceTests, frameworkHandle, () => _cancelled);
                var executionOptions = TestFrameworkOptions.ForExecution();
                executionOptions.SetValue("xunit.execution.MaxParallelThreads", 1);

                controller.RunTests(testsToRun, executionVisitor, executionOptions);

                if (!executionVisitor.Finished.WaitOne(TimeSpan.FromMinutes(5)))
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Error,
                        "SWTSharp TestAdapter: Test execution timed out");
                    executionVisitor.MarkRemainingAsFailed("Test execution timed out");
                }
            }
            catch (Exception ex)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error,
                    $"SWTSharp TestAdapter: Failed to run tests from {source}: {ex.Message}");
                MarkTestsAsFailed(sourceTests, frameworkHandle, $"Test host error: {ex.Message}");
            }
        }
    }

    private void MarkTestsAsFailed(List<TestCase> tests, IFrameworkHandle frameworkHandle, string message)
    {
        foreach (var test in tests)
        {
            frameworkHandle.RecordResult(new TestResult(test)
            {
                Outcome = TestOutcome.Failed,
                ErrorMessage = message
            });
        }
    }

    private string GetTestHostPath()
    {
        // Find the SWTSharp.TestHost executable
        var adapterPath = Path.GetDirectoryName(GetType().Assembly.Location)!;
        var testHostPath = Path.Combine(adapterPath, "SWTSharp.TestHost.dll");

        if (!File.Exists(testHostPath))
        {
            throw new FileNotFoundException(
                $"SWTSharp TestHost not found at: {testHostPath}. " +
                "Ensure SWTSharp.TestHost is packaged with the test adapter.");
        }

        return testHostPath;
    }

    private string GetTestFilter(List<TestCase> tests)
    {
        // Create a filter string for the test host
        // Format: test1;test2;test3
        return string.Join(";", tests.Select(t => t.FullyQualifiedName));
    }

    private void ParseTestResult(string line, List<TestCase> tests, IFrameworkHandle frameworkHandle)
    {
        // Parse test results from test host output
        // Format: [RESULT] TestName: Passed|Failed|Skipped [duration] [message]

        if (!line.StartsWith("[RESULT]"))
            return;

        try
        {
            var parts = line.Substring(8).Split(new[] { ':' }, 2);
            if (parts.Length < 2)
                return;

            var testName = parts[0].Trim();
            var resultParts = parts[1].Trim().Split(new[] { ' ' }, 3);

            var outcome = resultParts[0] switch
            {
                "Passed" => TestOutcome.Passed,
                "Failed" => TestOutcome.Failed,
                "Skipped" => TestOutcome.Skipped,
                _ => TestOutcome.None
            };

            var duration = resultParts.Length > 1 && TimeSpan.TryParse(resultParts[1], out var d)
                ? d
                : TimeSpan.Zero;

            var message = resultParts.Length > 2 ? resultParts[2] : null;

            var test = tests.FirstOrDefault(t => t.DisplayName == testName || t.FullyQualifiedName == testName);
            if (test != null)
            {
                var result = new TestResult(test)
                {
                    Outcome = outcome,
                    Duration = duration,
                    ErrorMessage = outcome == TestOutcome.Failed ? message : null
                };

                frameworkHandle.RecordResult(result);
            }
        }
        catch (Exception ex)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Warning,
                $"SWTSharp TestAdapter: Failed to parse test result: {line} - {ex.Message}");
        }
    }

    private class TestCaseCollector : ITestCaseDiscoverySink
    {
        public List<TestCase> TestCases { get; } = new();

        public void SendTestCase(TestCase discoveredTest)
        {
            TestCases.Add(discoveredTest);
        }
    }

    /// <summary>
    /// Collects discovered xUnit test cases.
    /// </summary>
    private class TestDiscoveryVisitor : IMessageSink, IDisposable
    {
        public List<ITestCase> TestCases { get; } = new();
        public ManualResetEvent Finished { get; } = new(false);

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is ITestCaseDiscoveryMessage discoveryMessage)
            {
                TestCases.Add(discoveryMessage.TestCase);
            }
            else if (message is IDiscoveryCompleteMessage)
            {
                Finished.Set();
            }

            return true;
        }

        public void Dispose()
        {
            Finished?.Dispose();
        }
    }

    /// <summary>
    /// Handles xUnit test execution messages and reports results via VSTest IFrameworkHandle.
    /// </summary>
    private class TestExecutionVisitor : IMessageSink, IDisposable
    {
        private readonly List<TestCase> _testCases;
        private readonly IFrameworkHandle _frameworkHandle;
        private readonly Func<bool> _isCancelled;
        private readonly HashSet<string> _reportedTests = new();
        private readonly Stopwatch _testStopwatch = new();
        private readonly object _lock = new();
        public ManualResetEvent Finished { get; } = new(false);

        public TestExecutionVisitor(List<TestCase> testCases, IFrameworkHandle frameworkHandle, Func<bool> isCancelled)
        {
            _testCases = testCases;
            _frameworkHandle = frameworkHandle;
            _isCancelled = isCancelled;
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            if (_isCancelled())
            {
                return false; // Stop processing
            }

            switch (message)
            {
                case ITestStarting starting:
                    var startTest = FindTestCase(starting.Test.DisplayName);
                    if (startTest != null)
                    {
                        _frameworkHandle.RecordStart(startTest);
                        _testStopwatch.Restart();
                    }
                    break;

                case ITestPassed passed:
                    RecordResult(passed.Test.DisplayName, TestOutcome.Passed, passed.ExecutionTime, null, null);
                    break;

                case ITestFailed failed:
                    var errorMessage = failed.Messages.Length > 0 ? string.Join(Environment.NewLine, failed.Messages) : "Test failed";
                    var stackTrace = failed.StackTraces.Length > 0 ? string.Join(Environment.NewLine, failed.StackTraces) : null;
                    RecordResult(failed.Test.DisplayName, TestOutcome.Failed, failed.ExecutionTime, errorMessage, stackTrace);
                    break;

                case ITestSkipped skipped:
                    RecordResult(skipped.Test.DisplayName, TestOutcome.Skipped, 0, skipped.Reason, null);
                    break;

                case ITestAssemblyFinished:
                    Finished.Set();
                    break;
            }

            return true;
        }

        private void RecordResult(string testName, TestOutcome outcome, decimal executionTime, string? errorMessage, string? stackTrace)
        {
            lock (_lock)
            {
                if (_reportedTests.Contains(testName))
                    return;

                var testCase = FindTestCase(testName);
                if (testCase == null)
                    return;

                _reportedTests.Add(testName);

                var result = new TestResult(testCase)
                {
                    Outcome = outcome,
                    Duration = TimeSpan.FromSeconds((double)executionTime),
                    ErrorMessage = errorMessage,
                    ErrorStackTrace = stackTrace
                };

                _frameworkHandle.RecordResult(result);
                _frameworkHandle.RecordEnd(testCase, outcome);
            }
        }

        private TestCase? FindTestCase(string displayName)
        {
            // Try exact match on DisplayName
            var testCase = _testCases.FirstOrDefault(t => t.DisplayName == displayName);
            if (testCase != null)
                return testCase;

            // Try matching on FullyQualifiedName containing the display name
            testCase = _testCases.FirstOrDefault(t => t.FullyQualifiedName.EndsWith("." + displayName));
            if (testCase != null)
                return testCase;

            // Try partial match
            return _testCases.FirstOrDefault(t =>
                t.FullyQualifiedName.Contains(displayName) || displayName.Contains(t.DisplayName));
        }

        public void MarkRemainingAsFailed(string message)
        {
            lock (_lock)
            {
                foreach (var testCase in _testCases)
                {
                    if (!_reportedTests.Contains(testCase.DisplayName))
                    {
                        _reportedTests.Add(testCase.DisplayName);
                        _frameworkHandle.RecordResult(new TestResult(testCase)
                        {
                            Outcome = TestOutcome.Failed,
                            ErrorMessage = message
                        });
                    }
                }
                Finished.Set();
            }
        }

        public void Dispose()
        {
            Finished?.Dispose();
        }
    }

    /// <summary>
    /// Null message sink that ignores all messages (for diagnostics).
    /// </summary>
    private class NullMessageSink : IMessageSink
    {
        public bool OnMessage(IMessageSinkMessage message) => true;
        public void Dispose() { }
    }
}
