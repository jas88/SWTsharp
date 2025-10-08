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

            System.Threading.Thread.Sleep(100);
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

        // On Windows/Linux, we can run tests in-process
        // This is a simplified implementation - in production, you'd use xUnit's execution engine
        foreach (var test in tests)
        {
            if (_cancelled)
                break;

            frameworkHandle.RecordStart(test);

            try
            {
                // TODO: Implement actual test execution via xUnit
                // For now, mark as skipped with a message
                frameworkHandle.RecordResult(new TestResult(test)
                {
                    Outcome = TestOutcome.Skipped,
                    ErrorMessage = "SWTSharp TestAdapter: Default host implementation pending"
                });
            }
            catch (Exception ex)
            {
                frameworkHandle.RecordResult(new TestResult(test)
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace
                });
            }
            finally
            {
                frameworkHandle.RecordEnd(test, TestOutcome.Skipped);
            }
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
}
