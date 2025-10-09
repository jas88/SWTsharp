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
        Console.WriteLine($"DisplayFixture: Current thread = {Thread.CurrentThread.ManagedThreadId}");

        // On macOS, Display must be created on Thread 1 (the main thread)
        // Use MainThreadDispatcher to ensure Display is created on Thread 1
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SWTSharp.TestHost.MainThreadDispatcher.Invoke(() =>
            {
                Display = Display.Default;
                _uiThread = Thread.CurrentThread;
                Console.WriteLine($"DisplayFixture: Display created on Thread {Thread.CurrentThread.ManagedThreadId}");
            });

            // Hook Display.AsyncExec to use MainThreadDispatcher
            Display.SetAsyncExecutor(SWTSharp.TestHost.MainThreadDispatcher.Invoke);
            Console.WriteLine("DisplayFixture: Set custom async executor to use MainThreadDispatcher");
        }
        else
        {
            Display = Display.Default;
            _uiThread = Thread.CurrentThread;
        }

        var displayThread = Display.GetType().GetField("_thread", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(Display) as Thread;
        Console.WriteLine($"DisplayFixture: Display thread = {displayThread?.ManagedThreadId ?? -1}");
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
