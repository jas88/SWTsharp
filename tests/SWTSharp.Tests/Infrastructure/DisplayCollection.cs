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
    private readonly Thread _uiThread;
    private bool _disposed;

    public Display Display { get; private set; } = null!;

    public DisplayFixture()
    {
        // Create ONE UI thread for ALL tests
        var displayReady = new ManualResetEventSlim(false);
        _uiThread = new Thread(() =>
        {
            Display = Display.Default;
            displayReady.Set();

            // Run event loop until disposed
            while (!_disposed)
            {
                try
                {
                    Display.ReadAndDispatch();
                }
                catch
                {
                    // Swallow exceptions in event loop
                }
                Thread.Sleep(10);
            }
        });
        _uiThread.IsBackground = true;
        _uiThread.Start();

        displayReady.Wait();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Cleanup all shells
            try
            {
                Display?.SyncExec(() =>
                {
                    var shells = Display.GetShells();
                    foreach (var shell in shells)
                    {
                        shell?.Dispose();
                    }
                });
            }
            catch
            {
                // Swallow exceptions during disposal
            }

            _disposed = true;
            _uiThread?.Join(1000);
        }
    }
}
