using System.Runtime.InteropServices;
using SWTSharp.TestHost;
using Xunit.Runners;

namespace SWTSharp.Tests;

/// <summary>
/// Custom test runner entry point that handles macOS threading while still allowing VSTest discovery.
/// This is a WRAPPER, not a replacement for the test discoverer.
///
/// Usage: dotnet run --project tests/SWTSharp.Tests -- [xunit args]
/// Example: dotnet run --project tests/SWTSharp.Tests -- --list
///
/// For VSTest (with coverage): dotnet test (uses library mode, tests run on VSTest threads)
/// For macOS compatibility: Use this runner directly
/// </summary>
public static class TestRunner
{
    public static int Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("TestRunner: Running on macOS, using MainThreadDispatcher");
            return RunWithMainThreadDispatcher(args).GetAwaiter().GetResult();
        }
        else
        {
            Console.WriteLine("TestRunner: Running on Windows/Linux, direct execution");
            return RunXUnit(args).GetAwaiter().GetResult();
        }
    }

    private static Task<int> RunWithMainThreadDispatcher(string[] args)
    {
        // Initialize MainThreadDispatcher on Thread 1
        MainThreadDispatcher.Initialize();

        var exitCode = 0;
        var completionSignal = new ManualResetEventSlim(false);

        // Run xUnit on a background thread
        var xunitThread = new Thread(() =>
        {
            try
            {
                exitCode = RunXUnit(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestRunner: xUnit execution failed: {ex}");
                exitCode = -1;
            }
            finally
            {
                completionSignal.Set();
                MainThreadDispatcher.Stop();
            }
        })
        {
            Name = "xUnit Runner Thread",
            IsBackground = true
        };

        xunitThread.Start();

        // Start the main thread dispatcher loop (blocks until tests complete)
        Console.WriteLine("TestRunner: Starting MainThreadDispatcher on Thread 1");
        MainThreadDispatcher.RunLoop();

        // Wait for xUnit to finish
        completionSignal.Wait();
        return Task.FromResult(exitCode);
    }

    private static async Task<int> RunXUnit(string[] args)
    {
        Console.WriteLine($"TestRunner: Running xUnit on Thread {Thread.CurrentThread.ManagedThreadId}");

        var assembly = typeof(TestRunner).Assembly;
        Console.WriteLine($"TestRunner: Executing tests from {assembly.Location}");

        using var runner = Xunit.Runners.AssemblyRunner.WithoutAppDomain(assembly.Location);

        var completionSource = new TaskCompletionSource<int>();
        var totalTests = 0;
        var failedTests = 0;
        var skippedTests = 0;

        runner.OnDiscoveryComplete = info =>
        {
            Console.WriteLine($"TestRunner: Discovered {info.TestCasesToRun} test(s)");
        };

        runner.OnExecutionComplete = info =>
        {
            Console.WriteLine($"TestRunner: Execution complete - {info.TotalTests} tests, {info.TestsFailed} failed, {info.TestsSkipped} skipped");
            totalTests = info.TotalTests;
            failedTests = info.TestsFailed;
            skippedTests = info.TestsSkipped;
            completionSource.SetResult(info.TestsFailed > 0 ? 1 : 0);
        };

        runner.OnTestFailed = info =>
        {
            Console.WriteLine($"  [FAIL] {info.TestDisplayName}");
            Console.WriteLine($"    {info.ExceptionMessage}");
        };

        runner.OnTestSkipped = info =>
        {
            Console.WriteLine($"  [SKIP] {info.TestDisplayName}: {info.SkipReason}");
        };

        Console.WriteLine("TestRunner: Starting test discovery and execution...");
        runner.Start();

        var exitCode = await completionSource.Task;
        Console.WriteLine($"TestRunner: Exit code {exitCode}");
        return exitCode;
    }
}
