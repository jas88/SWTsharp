using Xunit;
using NSubstitute;
using SWTSharp;
using SWTSharp.Platform;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Base class for all SWTSharp tests providing common setup and teardown.
/// </summary>
public abstract class TestBase : IDisposable
{
    protected Display Display { get; private set; }
    protected IPlatform MockPlatform { get; private set; }
    private bool _disposed;

    protected TestBase()
    {
        // Initialize display on UI thread
        Display = Display.Default;

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
        return new Shell(Display);
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
            if (disposing)
            {
                // Cleanup display and shells on UI thread
                Display?.SyncExec(() =>
                {
                    var shells = Display.GetShells();
                    foreach (var shell in shells)
                    {
                        shell?.Dispose();
                    }
                });

                Display?.Dispose();
            }
            _disposed = true;
        }
    }
}
