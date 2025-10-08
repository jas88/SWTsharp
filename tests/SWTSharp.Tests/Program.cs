using System.Runtime.InteropServices;

namespace SWTSharp.Tests;

/// <summary>
/// Custom entry point that keeps Thread 1 available for macOS UI operations.
/// xUnit tests run on a background thread and marshal UI work back to Thread 1.
/// </summary>
public class Program
{
    public static int Main(string[] args)
    {
        Console.WriteLine($"Program.Main: Running on Thread {Thread.CurrentThread.ManagedThreadId}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RunWithMainThreadDispatcher(args).GetAwaiter().GetResult();
        }
        else
        {
            // On other platforms, just run xUnit normally
            return RunXUnit(args).GetAwaiter().GetResult();
        }
    }

    private static async Task<int> RunWithMainThreadDispatcher(string[] args)
    {
        Console.WriteLine("Program: Initializing Main Thread Dispatcher...");

        // Initialize AppKit on Thread 1 (this thread)
        MainThreadDispatcher.Initialize();

        Console.WriteLine("Program: Starting xUnit on background thread...");

        // Create a task completion source to get the exit code from xUnit
        var exitCodeTaskSource = new TaskCompletionSource<int>();

        // Run xUnit on a background thread
        var xunitThread = new Thread(async () =>
        {
            try
            {
                Console.WriteLine($"xUnit thread: Running on Thread {Thread.CurrentThread.ManagedThreadId}");
                var exitCode = await RunXUnit(args);
                Console.WriteLine($"xUnit thread: Tests completed with exit code {exitCode}");

                // Signal dispatcher to stop
                MainThreadDispatcher.Stop();

                exitCodeTaskSource.SetResult(exitCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"xUnit thread: FATAL ERROR: {ex}");
                MainThreadDispatcher.Stop();
                exitCodeTaskSource.SetResult(1);
            }
        })
        {
            Name = "xUnit Runner Thread",
            IsBackground = false
        };

        xunitThread.Start();

        Console.WriteLine("Program: Running dispatch loop on main thread (Thread 1)...");

        // Run dispatch loop on THIS thread (Thread 1)
        // This will block until tests complete and call MainThreadDispatcher.Stop()
        try
        {
            MainThreadDispatcher.RunLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Dispatcher: ERROR: {ex}");
        }

        Console.WriteLine("Program: Dispatch loop completed, waiting for test results...");

        // Wait for xUnit thread to finish and get exit code
        var exitCode = await exitCodeTaskSource.Task;

        Console.WriteLine("Program: Tests completed");

        return exitCode;
    }

    private static async Task<int> RunXUnit(string[] args)
    {
        // Use xUnit v3's in-process system console runner
        // The ConsoleRunner.Run() method is provided by xunit.v3 package
        return await Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args);
    }
}
