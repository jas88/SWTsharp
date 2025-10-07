using Xunit;
using NSubstitute;
using SWTSharp;
using SWTSharp.Platform;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Base class for all SWTSharp tests providing common setup and teardown.
/// </summary>
[Collection("Display Tests")]
public abstract class TestBase : IDisposable
{
    protected Display Display { get; private set; } = null!;
    protected IPlatform MockPlatform { get; private set; } = null!;
    private bool _disposed;
    private Thread? _uiThread;
    private bool _eventLoopStarted;

    protected TestBase()
    {
        // Start UI thread with event loop
        var displayReady = new ManualResetEventSlim(false);
        _uiThread = new Thread(() =>
        {
            // Initialize display on UI thread
            Display = Display.Default;
            displayReady.Set();

            // Run event loop - process async actions but don't block
            while (!_disposed)
            {
                Display.ReadAndDispatch();
                Thread.Sleep(10);
            }
        });
        _uiThread.IsBackground = true;
        _uiThread.Start();

        // Wait for display to be initialized
        displayReady.Wait();
        _eventLoopStarted = true;

        // Create mock platform for testing
        MockPlatform = Substitute.For<IPlatform>();

        // Set up default mock behaviors
        SetupDefaultMockBehaviors();
    }

    /// <summary>
    /// Sets up default behaviors for the mock platform.
    /// Override this method in derived classes to customize mock behavior.
    /// </summary>
    protected virtual void SetupDefaultMockBehaviors()
    {
        // Default mock behaviors can be set here
        // Platform-specific test classes can override this
    }

    /// <summary>
    /// Creates a test shell for widget testing.
    /// </summary>
    protected Shell CreateTestShell()
    {
        Shell? shell = null;
        Display.SyncExec(() =>
        {
            shell = new Shell(Display);
        });
        return shell!;
    }

    /// <summary>
    /// Verifies that a widget is properly disposed.
    /// </summary>
    protected void AssertDisposed(Widget widget)
    {
        Assert.True(widget.IsDisposed, "Widget should be disposed");
    }

    /// <summary>
    /// Verifies that a widget is not disposed.
    /// </summary>
    protected void AssertNotDisposed(Widget widget)
    {
        Assert.False(widget.IsDisposed, "Widget should not be disposed");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _eventLoopStarted)
            {
                // Cleanup display and shells on UI thread FIRST, before signaling exit
                try
                {
                    Display?.SyncExec(() =>
                    {
                        var shells = Display.GetShells();
                        foreach (var shell in shells)
                        {
                            shell?.Dispose();
                        }

                        // Dispose display on UI thread
                        Display?.Dispose();
                    });
                }
                catch
                {
                    // Swallow exceptions during disposal
                }

                // NOW signal event loop to exit
                _disposed = true;

                // Wait for UI thread to finish
                _uiThread?.Join(1000);
            }
            else
            {
                _disposed = true;
            }
        }
    }
}
