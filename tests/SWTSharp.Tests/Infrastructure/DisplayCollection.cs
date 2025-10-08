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
        // xUnit.StaFact's [UIFact] attribute handles macOS main thread requirements automatically
        // Tests using [UIFact] will run on the UI thread
        Console.WriteLine($"DisplayFixture: Current thread = {Thread.CurrentThread.ManagedThreadId}");

        // Initialize Display directly - tests with [UIFact] will already be on the correct thread
        Display = Display.Default;
        _uiThread = Thread.CurrentThread;
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
