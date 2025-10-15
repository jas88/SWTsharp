using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.TestHost;

namespace SWTSharp.Tests;

/// <summary>
/// Test runner entry point that executes all xUnit tests with macOS Thread 1 support.
/// Reports failures to STDERR and exits with appropriate code for CI.
/// </summary>
public class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine($"SWTSharp Test Runner");
        Console.WriteLine($"Platform: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}");
        Console.WriteLine();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RunTestsOnMacOS();
        }
        else
        {
            return RunTests();
        }
    }

    private static int RunTestsOnMacOS()
    {
        Console.WriteLine("macOS detected - initializing Thread 1 dispatcher...");

        // Initialize MainThreadDispatcher on Thread 1
        MainThreadDispatcher.Initialize();

        // Hook into MacOSPlatform to route ExecuteOnMainThread through our dispatcher
        SWTSharp.Platform.MacOSPlatform.CustomMainThreadExecutor = MainThreadDispatcher.Invoke;

        var exitCode = 0;
        var completionSignal = new ManualResetEventSlim(false);

        // Run tests on background thread
        var testThread = new Thread(() =>
        {
            try
            {
                exitCode = RunTests();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FATAL: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                exitCode = 1;
            }
            finally
            {
                // Signal completion FIRST, then stop the run loop
                completionSignal.Set();

                // Stop the main thread run loop so RunLoop() can exit
                MainThreadDispatcher.Stop();
            }
        })
        {
            Name = "Test Execution Thread",
            IsBackground = false
        };

        testThread.Start();

        // Run dispatch loop on Thread 1 (blocks until Stop() is called)
        Console.WriteLine("Starting main thread dispatch loop...");
        MainThreadDispatcher.RunLoop();

        // RunLoop() has exited, wait for test thread to finish cleanup
        if (!completionSignal.Wait(TimeSpan.FromSeconds(5)))
        {
            Console.Error.WriteLine("FATAL: Test thread did not complete cleanup within 5 seconds after RunLoop stopped");
            return 1;
        }

        Console.WriteLine("Tests completed successfully, exiting...");
        return exitCode;
    }

    private static int RunTests()
    {
        try
        {
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            Console.WriteLine($"Running tests in: {assemblyPath}");
            Console.WriteLine();

            // Use xUnit to discover and run tests
            using var controller = new XunitFrontController(
                AppDomainSupport.Denied,
                assemblyPath,
                configFileName: null,
                shadowCopy: false,
                diagnosticMessageSink: new DiagnosticMessageSink());

            // Discover tests
            var discoveryVisitor = new TestDiscoveryVisitor();
            controller.Find(
                includeSourceInformation: false,
                messageSink: discoveryVisitor,
                discoveryOptions: TestFrameworkOptions.ForDiscovery());

            if (!discoveryVisitor.Finished.WaitOne(TimeSpan.FromSeconds(30)))
            {
                Console.Error.WriteLine("FATAL: Test discovery timed out");
                return 1;
            }

            Console.WriteLine($"Discovered {discoveryVisitor.TestCases.Count} tests");
            Console.WriteLine();

            // Run tests
            var executionVisitor = new TestExecutionVisitor();
            controller.RunTests(
                discoveryVisitor.TestCases,
                executionVisitor,
                TestFrameworkOptions.ForExecution());

            if (!executionVisitor.Finished.WaitOne(TimeSpan.FromMinutes(5)))
            {
                Console.Error.WriteLine("FATAL: Test execution timed out");
                return 1;
            }

            // Report results
            Console.WriteLine();
            Console.WriteLine("=== Test Results ===");
            Console.WriteLine($"Passed:  {executionVisitor.PassedTests}");
            Console.WriteLine($"Failed:  {executionVisitor.FailedTests}");
            Console.WriteLine($"Skipped: {executionVisitor.SkippedTests}");
            Console.WriteLine($"Total:   {executionVisitor.PassedTests + executionVisitor.FailedTests + executionVisitor.SkippedTests}");

            return executionVisitor.FailedTests > 0 ? 1 : 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex.Message}");
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

        public bool OnMessage(IMessageSinkMessage message)
        {
            switch (message)
            {
                case ITestStarting starting:
                    _testStopwatch.Restart();
                    Console.Write($"  {starting.Test.DisplayName} ... ");
                    break;

                case ITestPassed passed:
                    PassedTests++;
                    _testStopwatch.Stop();
                    Console.WriteLine($"PASSED ({_testStopwatch.ElapsedMilliseconds}ms)");
                    break;

                case ITestFailed failed:
                    FailedTests++;
                    _testStopwatch.Stop();
                    Console.WriteLine($"FAILED ({_testStopwatch.ElapsedMilliseconds}ms)");
                    Console.Error.WriteLine($"FAILED: {failed.Test.DisplayName}");
                    Console.Error.WriteLine($"  {string.Join("\n  ", failed.Messages)}");
                    if (failed.StackTraces.Any())
                    {
                        Console.Error.WriteLine($"  {string.Join("\n  ", failed.StackTraces)}");
                    }
                    break;

                case ITestSkipped skipped:
                    SkippedTests++;
                    Console.WriteLine($"SKIPPED ({skipped.Reason})");
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
                Console.WriteLine($"[xUnit] {diagnostic.Message}");
            }
            return true;
        }

        public void Dispose() { }
    }
}
