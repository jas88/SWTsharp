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
    protected Display Display { get; private set; }
    protected IPlatform MockPlatform { get; private set; } = null!;
    private bool _disposed;

    protected TestBase(DisplayFixture displayFixture)
    {
        // Use shared Display from fixture
        Display = displayFixture.Display;

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
    /// Uses Display.SyncExec to ensure creation happens on the UI thread.
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
    /// Executes an action on the UI thread.
    /// Use this to wrap all widget operations in tests.
    /// </summary>
    protected void RunOnUIThread(Action action)
    {
        Display.SyncExec(action);
    }

    /// <summary>
    /// Executes a function on the UI thread and returns the result.
    /// </summary>
    protected T RunOnUIThread<T>(Func<T> func)
    {
        T? result = default;
        Display.SyncExec(() =>
        {
            result = func();
        });
        return result!;
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
                // Cleanup shells created by this test
                // We're already on the main thread, no need for ExecuteOnMainThread
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

            _disposed = true;
        }
    }
}
