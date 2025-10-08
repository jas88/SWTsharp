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

        var exitCode = 0;
        var completionSignal = new ManualResetEventSlim(false);

        // Run tests on background thread
        var testThread = new Thread(() =>
        {
            try
            {
                exitCode = RunTests(testAssemblyPath, testFilter);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] SWTSharp TestHost: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                exitCode = 1;
            }
            finally
            {
                Console.WriteLine("[INFO] SWTSharp TestHost: Test thread completing...");
                // Signal dispatcher to stop FIRST
                MainThreadDispatcher.Stop();
                // Then signal completion
                completionSignal.Set();
            }
        })
        {
            Name = "Test Execution Thread",
            IsBackground = false // Foreground thread to ensure it completes
        };

        testThread.Start();

        // Run the dispatch loop on Thread 1
        // This will block until tests complete and call Stop()
        Console.WriteLine("[INFO] SWTSharp TestHost: Starting main thread dispatch loop...");
        MainThreadDispatcher.RunLoop();

        // Wait for test thread to finish cleanup (with timeout)
        Console.WriteLine("[INFO] SWTSharp TestHost: Waiting for test thread completion...");
        if (!completionSignal.Wait(TimeSpan.FromSeconds(10)))
        {
            Console.Error.WriteLine("[ERROR] SWTSharp TestHost: Test thread did not complete in time");
            return 1;
        }

        Console.WriteLine("[INFO] SWTSharp TestHost: Exiting with code " + exitCode);
        return exitCode;
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

            // Run tests
            var executionVisitor = new TestExecutionVisitor();
            var executionOptions = TestFrameworkOptions.ForExecution();
            controller.RunTests(testsToRun, executionVisitor, executionOptions);

            if (!executionVisitor.Finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Console.Error.WriteLine("[ERROR] SWTSharp TestHost: Test execution timed out");
                return 1;
            }

            // Report results
            Console.WriteLine($"[INFO] SWTSharp TestHost: Tests completed");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Passed: {executionVisitor.PassedTests}");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Failed: {executionVisitor.FailedTests}");
            Console.WriteLine($"[INFO] SWTSharp TestHost: Skipped: {executionVisitor.SkippedTests}");

            return executionVisitor.FailedTests > 0 ? 1 : 0;
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

    private class TestExecutionVisitor : IMessageSink
    {
        public int PassedTests { get; private set; }
        public int FailedTests { get; private set; }
        public int SkippedTests { get; private set; }
        public ManualResetEvent Finished { get; } = new(false);

        private readonly Stopwatch _testStopwatch = new();
        private string? _currentTest;

        public bool OnMessage(IMessageSinkMessage message)
        {
            switch (message)
            {
                case ITestStarting starting:
                    _currentTest = starting.Test.DisplayName;
                    _testStopwatch.Restart();
                    Console.WriteLine($"[START] {_currentTest}");
                    break;

                case ITestPassed passed:
                    PassedTests++;
                    _testStopwatch.Stop();
                    Console.WriteLine($"[RESULT] {passed.Test.DisplayName}: Passed {_testStopwatch.Elapsed}");
                    break;

                case ITestFailed failed:
                    FailedTests++;
                    _testStopwatch.Stop();
                    Console.WriteLine($"[RESULT] {failed.Test.DisplayName}: Failed {_testStopwatch.Elapsed} {failed.Messages[0]}");
                    Console.Error.WriteLine($"[ERROR] {failed.Test.DisplayName}:");
                    Console.Error.WriteLine($"  {string.Join("\n  ", failed.Messages)}");
                    Console.Error.WriteLine($"  {string.Join("\n  ", failed.StackTraces)}");
                    break;

                case ITestSkipped skipped:
                    SkippedTests++;
                    Console.WriteLine($"[RESULT] {skipped.Test.DisplayName}: Skipped 0 {skipped.Reason}");
                    break;

                case ITestAssemblyFinished:
                    Finished.Set();
                    break;
            }

            return true;
        }

        public void Dispose()
        {
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
