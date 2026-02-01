using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace SWTSharp.TestHost;

/// <summary>
/// Test host process that ensures macOS Thread 1 is available for UI operations.
/// This host is launched by the SWTSharp.TestAdapter to run tests in isolation.
///
/// Usage: dotnet SWTSharp.TestHost.dll &lt;test-assembly&gt; [test-filter]
/// </summary>
public class Program
{
    /// <summary>
    /// Per-test timeout in seconds. Tests that exceed this timeout are considered deadlocked.
    /// </summary>
    private const int TestTimeoutSeconds = 30;

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: SWTSharp.TestHost <test-assembly> [test-filter]");
            return 1;
        }

        var testAssemblyPath = args[0];
        var testFilter = args.Length > 1 ? args[1].Split(';') : Array.Empty<string>();

        Console.WriteLine($"[INFO] SWTSharp TestHost: Loading test assembly: {testAssemblyPath}");
        Console.WriteLine($"[INFO] SWTSharp TestHost: Platform: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"[INFO] SWTSharp TestHost: Thread {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine($"[INFO] SWTSharp TestHost: Per-test timeout: {TestTimeoutSeconds}s");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RunTestsOnMacOS(testAssemblyPath, testFilter);
        }
        else
        {
            return RunTestsDefault(testAssemblyPath, testFilter);
        }
    }

    private static int RunTestsOnMacOS(string testAssemblyPath, string[] testFilter)
    {
        Console.WriteLine("[INFO] SWTSharp TestHost: Using macOS Thread 1 dispatcher");

        // Initialize MainThreadDispatcher on Thread 1 (the main process thread)
        MainThreadDispatcher.Initialize();

        // Hook into SWTSharp's MacOSPlatform to route ExecuteOnMainThread through our dispatcher
        SWTSharp.Platform.MacOSPlatform.CustomMainThreadExecutor = MainThreadDispatcher.Invoke;

        // Run tests DIRECTLY on Thread 1 (no background thread needed)
        // This allows SyncExec to execute immediately when already on UI thread
        Console.WriteLine("[INFO] SWTSharp TestHost: Running tests on Thread 1...");
        return RunTests(testAssemblyPath, testFilter);
    }

    private static int RunTestsDefault(string testAssemblyPath, string[] testFilter)
    {
        Console.WriteLine("[INFO] SWTSharp TestHost: Using default execution (Windows/Linux)");
        return RunTests(testAssemblyPath, testFilter);
    }

    private static int RunTests(string testAssemblyPath, string[] testFilter)
    {
        try
        {
            // Load test assembly
            var assembly = Assembly.LoadFrom(testAssemblyPath);
            Console.WriteLine($"[INFO] SWTSharp TestHost: Loaded assembly: {assembly.FullName}");

            // Use xUnit to discover and run tests via XunitFrontController
            using var controller = new XunitFrontController(
                AppDomainSupport.Denied,
                testAssemblyPath,
                configFileName: null,
                shadowCopy: false,
                diagnosticMessageSink: new DiagnosticMessageSink());

            // Discover tests
            var discoveryVisitor = new TestDiscoveryVisitor();
            var discoveryOptions = TestFrameworkOptions.ForDiscovery();
            controller.Find(
                includeSourceInformation: false,
                messageSink: discoveryVisitor,
                discoveryOptions: discoveryOptions);

            if (!discoveryVisitor.Finished.WaitOne(TimeSpan.FromSeconds(30)))
            {
                Console.Error.WriteLine("[ERROR] SWTSharp TestHost: Test discovery timed out");
                return 1;
            }

            Console.WriteLine($"[INFO] SWTSharp TestHost: Discovered {discoveryVisitor.TestCases.Count} tests");

            // Filter tests if filter provided
            var testsToRun = testFilter.Length > 0
                ? discoveryVisitor.TestCases.Where(t => testFilter.Any(f => t.DisplayName.Contains(f))).ToList()
                : discoveryVisitor.TestCases;

            if (testsToRun.Count == 0)
            {
                Console.WriteLine("[WARN] SWTSharp TestHost: No tests matched filter");
                return 0;
            }

            Console.WriteLine($"[INFO] SWTSharp TestHost: Running {testsToRun.Count} tests");

            // Run tests with timeout monitoring
            var executionVisitor = new TestExecutionVisitor(TestTimeoutSeconds);
            var executionOptions = TestFrameworkOptions.ForExecution();

            // Run tests one at a time to enable per-test timeout
            executionOptions.SetValue("xunit.execution.MaxParallelThreads", 1);
            executionOptions.SetValue("xunit.execution.DisableParallelization", true);

            controller.RunTests(testsToRun, executionVisitor, executionOptions);

            // Wait for completion with overall timeout
            var overallTimeout = TimeSpan.FromSeconds(testsToRun.Count * TestTimeoutSeconds + 60);
            if (!executionVisitor.Finished.WaitOne(overallTimeout))
            {
                Console.Error.WriteLine("[ERROR] SWTSharp TestHost: Test execution timed out (overall)");
                executionVisitor.FailTimedOutTest();
                return 1;
            }

            // Report results
            Console.WriteLine($"[INFO] SWTSharp TestHost: Tests completed");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Passed: {executionVisitor.PassedTests}");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Failed: {executionVisitor.FailedTests}");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Skipped: {executionVisitor.SkippedTests}");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Timed out: {executionVisitor.TimedOutTests}");

            return executionVisitor.FailedTests > 0 || executionVisitor.TimedOutTests > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] SWTSharp TestHost: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private class TestDiscoveryVisitor : IMessageSink
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
    /// Test execution visitor with per-test timeout detection and deadlock handling.
    /// </summary>
    private class TestExecutionVisitor : IMessageSink
    {
        public int PassedTests { get; private set; }
        public int FailedTests { get; private set; }
        public int SkippedTests { get; private set; }
        public int TimedOutTests { get; private set; }
        public ManualResetEvent Finished { get; } = new(false);

        private readonly int _timeoutSeconds;
        private readonly Stopwatch _testStopwatch = new();
        private readonly object _lock = new();
        private string? _currentTest;
        private CancellationTokenSource? _timeoutCts;
        private bool _testTimedOut;

        public TestExecutionVisitor(int timeoutSeconds)
        {
            _timeoutSeconds = timeoutSeconds;
        }

        public bool OnMessage(IMessageSinkMessage message)
        {
            lock (_lock)
            {
                switch (message)
                {
                    case ITestStarting starting:
                        _currentTest = starting.Test.DisplayName;
                        _testTimedOut = false;
                        _testStopwatch.Restart();
                        StartTimeoutMonitor();
                        Console.WriteLine($"[START] {_currentTest}");
                        break;

                    case ITestPassed passed:
                        CancelTimeoutMonitor();
                        if (!_testTimedOut)
                        {
                            PassedTests++;
                            _testStopwatch.Stop();
                            Console.WriteLine($"[RESULT] {passed.Test.DisplayName}: Passed {_testStopwatch.Elapsed}");
                        }
                        break;

                    case ITestFailed failed:
                        CancelTimeoutMonitor();
                        if (!_testTimedOut)
                        {
                            FailedTests++;
                            _testStopwatch.Stop();
                            Console.WriteLine($"[RESULT] {failed.Test.DisplayName}: Failed {_testStopwatch.Elapsed} {failed.Messages[0]}");
                            Console.Error.WriteLine($"[ERROR] {failed.Test.DisplayName}:");
                            Console.Error.WriteLine($"  {string.Join("\n  ", failed.Messages)}");
                            Console.Error.WriteLine($"  {string.Join("\n  ", failed.StackTraces)}");
                        }
                        break;

                    case ITestSkipped skipped:
                        CancelTimeoutMonitor();
                        if (!_testTimedOut)
                        {
                            SkippedTests++;
                            Console.WriteLine($"[RESULT] {skipped.Test.DisplayName}: Skipped 0 {skipped.Reason}");
                        }
                        break;

                    case ITestAssemblyFinished:
                        CancelTimeoutMonitor();
                        Finished.Set();
                        break;
                }
            }

            return true;
        }

        private void StartTimeoutMonitor()
        {
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();
            _timeoutCts = new CancellationTokenSource();

            var testName = _currentTest;
            var cts = _timeoutCts;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_timeoutSeconds), cts.Token);

                    // Timeout fired - test is likely deadlocked
                    lock (_lock)
                    {
                        if (!cts.IsCancellationRequested && _currentTest == testName && !_testTimedOut)
                        {
                            _testTimedOut = true;
                            TimedOutTests++;
                            FailedTests++;
                            _testStopwatch.Stop();

                            Console.Error.WriteLine($"[TIMEOUT] Test '{testName}' deadlocked after {_timeoutSeconds}s - possible Thread 1 dispatch issue");
                            Console.WriteLine($"[RESULT] {testName}: Failed {_testStopwatch.Elapsed} Deadlock timeout after {_timeoutSeconds} seconds");
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Test completed before timeout - normal behavior
                }
            });
        }

        private void CancelTimeoutMonitor()
        {
            _timeoutCts?.Cancel();
        }

        /// <summary>
        /// Called when the overall test execution times out to mark the current test as failed.
        /// </summary>
        public void FailTimedOutTest()
        {
            lock (_lock)
            {
                if (_currentTest != null && !_testTimedOut)
                {
                    _testTimedOut = true;
                    TimedOutTests++;
                    FailedTests++;
                    Console.Error.WriteLine($"[TIMEOUT] Test '{_currentTest}' deadlocked after {_timeoutSeconds}s - possible Thread 1 dispatch issue");
                    Console.WriteLine($"[RESULT] {_currentTest}: Failed {_testStopwatch.Elapsed} Deadlock timeout after {_timeoutSeconds} seconds");
                }
                Finished.Set();
            }
        }

        public void Dispose()
        {
            _timeoutCts?.Cancel();
            _timeoutCts?.Dispose();
            Finished?.Dispose();
        }
    }

    private class DiagnosticMessageSink : IMessageSink
    {
        public bool OnMessage(IMessageSinkMessage message)
        {
            if (message is IDiagnosticMessage diagnostic)
            {
                Console.WriteLine($"[DIAG] {diagnostic.Message}");
            }

            return true;
        }

        public void Dispose() { }
    }

    private class NullSourceInformationProvider : ISourceInformationProvider
    {
        public ISourceInformation GetSourceInformation(ITestCase testCase) => null!;
        public void Dispose() { }
    }
}
