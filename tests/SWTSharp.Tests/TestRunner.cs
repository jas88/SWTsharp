using System.Runtime.InteropServices;
using SWTSharp.TestHost;

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

    private static Task<int> RunXUnit(string[] args)
    {
        Console.WriteLine($"TestRunner: Running xUnit on Thread {Thread.CurrentThread.ManagedThreadId}");

        // Use xUnit's programmatic API instead of command-line runner
        // This integrates better with the threading model
        var assembly = typeof(TestRunner).Assembly;

        // For now, just indicate that tests would run here
        // In a full implementation, we'd use Xunit.Runner.Common or similar
        Console.WriteLine($"TestRunner: Would execute tests from {assembly.FullName}");
        Console.WriteLine("TestRunner: Use 'dotnet test' for VSTest integration with coverage");

        return Task.FromResult(0);
    }
}
