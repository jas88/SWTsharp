using System.Runtime.InteropServices;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// xUnit collection definition to ensure all tests using Display run serially.
/// This is necessary because Display is a singleton that can only be associated with one thread.
/// </summary>
[CollectionDefinition("Display Tests", DisableParallelization = true)]
public class DisplayCollection : ICollectionFixture<DisplayFixture>
{
}

/// <summary>
/// Shared fixture that creates a single UI thread and Display for all tests.
/// </summary>
public class DisplayFixture : IDisposable
{
    private Thread _uiThread = null!;
    private bool _disposed;

    public Display Display { get; private set; } = null!;

    public DisplayFixture()
    {
        // CRITICAL: macOS requires ALL UI operations on the process's FIRST thread
        // We must use MainThreadDispatcher for macOS compatibility
        Console.WriteLine($"DisplayFixture: Current thread = {Thread.CurrentThread.ManagedThreadId}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Initialize MainThreadDispatcher if not already done
            if (!MainThreadDispatcher.IsInitialized)
            {
                // Create a completion signal
                var initCompleted = new ManualResetEventSlim(false);

                // Start dispatcher on a dedicated thread that becomes the "main" thread
                var dispatcherThread = new Thread(() =>
                {
                    MainThreadDispatcher.Initialize();

                    // Initialize Display on THIS thread (Thread 8) BEFORE starting RunLoop
                    Console.WriteLine($"DisplayFixture.Initialize: Running on Thread {Thread.CurrentThread.ManagedThreadId}");
                    Display = Display.Default;
                    _uiThread = Thread.CurrentThread;

                    // Override Display.AsyncExec to use MainThreadDispatcher
                    Console.WriteLine("DisplayFixture: Setting custom async executor to use MainThreadDispatcher");
                    Display.SetAsyncExecutor(action =>
                    {
                        MainThreadDispatcher.Invoke(action);
                    });

                    // Signal that initialization is complete
                    initCompleted.Set();

                    // Now start the dispatch loop
                    MainThreadDispatcher.RunLoop();
                })
                {
                    Name = "Main Thread Dispatcher",
                    IsBackground = false
                };
                dispatcherThread.Start();

                // Wait for Display initialization to complete
                initCompleted.Wait();
            }
            else
            {
                // Dispatcher already running, use it to initialize Display
                MainThreadDispatcher.Invoke(() =>
                {
                    if (Display == null)
                    {
                        Console.WriteLine($"DisplayFixture.Initialize: Running on Thread {Thread.CurrentThread.ManagedThreadId}");
                        Display = Display.Default;
                        _uiThread = Thread.CurrentThread;

                        // Override Display.AsyncExec to use MainThreadDispatcher
                        Console.WriteLine("DisplayFixture: Setting custom async executor to use MainThreadDispatcher");
                        Display.SetAsyncExecutor(action =>
                        {
                            MainThreadDispatcher.Invoke(action);
                        });
                    }
                });
            }
        }
        else
        {
            // On other platforms, initialize directly
            Display = Display.Default;
            _uiThread = Thread.CurrentThread;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            // Cleanup all shells directly on current thread
            // Since DisplayFixture runs on xUnit's thread, and that's where we initialized Display,
            // we're already on the correct thread for macOS
            try
            {
                var shells = Display.GetShells();
                foreach (var shell in shells)
                {
                    shell?.Dispose();
                }
            }
            catch
            {
                // Swallow exceptions during disposal
            }
        }
    }
}
