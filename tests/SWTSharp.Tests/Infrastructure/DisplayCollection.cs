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
        // Use MainThreadDispatcher to execute on the correct thread
        Console.WriteLine($"DisplayFixture: Current thread = {Thread.CurrentThread.ManagedThreadId}, Main thread = {MainThreadDispatcher.MainThreadId}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Initialize Display on the main thread via dispatcher
            MainThreadDispatcher.Invoke(() =>
            {
                Console.WriteLine($"DisplayFixture.Initialize: Running on Thread {Thread.CurrentThread.ManagedThreadId}");
                Display = Display.Default;
                _uiThread = Thread.CurrentThread;

                // CRITICAL: Override Display.AsyncExec to use MainThreadDispatcher
                // This makes Display.SyncExec() work correctly with our test dispatcher
                Console.WriteLine("DisplayFixture: Setting custom async executor to use MainThreadDispatcher");
                Display.SetAsyncExecutor(action =>
                {
                    Console.WriteLine($"CustomAsyncExecutor: Marshaling action to Thread {MainThreadDispatcher.MainThreadId} from Thread {Thread.CurrentThread.ManagedThreadId}");
                    MainThreadDispatcher.Invoke(action);
                    Console.WriteLine("CustomAsyncExecutor: Action completed");
                });
            });
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
