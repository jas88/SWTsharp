using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// xUnit collection definition for GUI tests.
/// All GUI tests run serially to avoid threading issues with Display singleton.
/// </summary>
[CollectionDefinition("GUI Tests", DisableParallelization = true)]
public class GUITestCollection : ICollectionFixture<DisplayFixture>
{
}

/// <summary>
/// Base class for GUI tests with Display management, strict disposal checking,
/// event queue verification, and diagnostic capture on failure.
/// </summary>
/// <remarks>
/// GUI tests should inherit from GUITestBase instead of TestBase.
/// This class provides:
/// - Display lifecycle management (shared or isolated)
/// - Strict disposal checking: tests fail if widgets not disposed
/// - Event queue checking: tests fail if event queue left dirty
/// - Failure diagnostics: capture widget tree on failure for debugging
/// </remarks>
[Collection("GUI Tests")]
[Trait("Category", "GUI")]
public abstract class GUITestBase : IAsyncLifetime, IDisposable
{
    private readonly DisplayFixture _displayFixture;
    private readonly bool _useIsolatedDisplay;
    private readonly List<Shell> _trackedShells = [];
    private bool _disposed;

    /// <summary>
    /// Gets the Display for this test.
    /// </summary>
    protected Display Display { get; private set; } = null!;

    /// <summary>
    /// Gets the path to the TestResults folder for storing diagnostics.
    /// </summary>
    protected static string TestResultsFolder
    {
        get
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "TestResults");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return folder;
        }
    }

    /// <summary>
    /// Creates a new GUITestBase using the shared Display from the fixture.
    /// </summary>
    /// <param name="displayFixture">The shared display fixture.</param>
    protected GUITestBase(DisplayFixture displayFixture)
        : this(displayFixture, useIsolatedDisplay: false)
    {
    }

    /// <summary>
    /// Creates a new GUITestBase with optional isolated Display.
    /// </summary>
    /// <param name="displayFixture">The shared display fixture.</param>
    /// <param name="useIsolatedDisplay">If true, creates a fresh Display for isolation.</param>
    protected GUITestBase(DisplayFixture displayFixture, bool useIsolatedDisplay)
    {
        _displayFixture = displayFixture;
        _useIsolatedDisplay = useIsolatedDisplay;
    }

    /// <summary>
    /// Initializes the test by acquiring or creating a Display.
    /// </summary>
    public virtual Task InitializeAsync()
    {
        if (_useIsolatedDisplay)
        {
            // Create fresh Display for isolation (rare, opt-in)
            // WARNING: On macOS, Display must be created on Thread 1 (main thread) for Cocoa.
            // This isolated display path bypasses MainThreadDispatcher and may fail on macOS.
            // Only use isolated displays on Windows/Linux, or ensure tests run via custom runner.
            Display = new Display();
        }
        else
        {
            // Use shared Display from fixture for speed (default)
            Display = _displayFixture.Display;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleans up after the test, verifying disposal and event queue state.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        try
        {
            // Check for undisposed shells (strict disposal checking)
            await VerifyDisposalAsync();

            // Check for dirty event queue
            await VerifyEventQueueCleanAsync();
        }
        catch (Exception ex)
        {
            await CaptureFailureDiagnosticsAsync(ex);
            throw;
        }
        finally
        {
            // Clean up tracked shells
            CleanupTrackedShells();

            // If using isolated Display, dispose it
            if (_useIsolatedDisplay && Display != null)
            {
                Display.Dispose();
            }
        }
    }

    /// <summary>
    /// Creates a Shell with auto-tracking for disposal verification.
    /// </summary>
    /// <returns>A new Shell instance.</returns>
    protected Shell CreateShell()
    {
        Shell? shell = null;
        RunOnUIThread(() =>
        {
            shell = new Shell(Display);
        });

        if (shell == null)
            throw new InvalidOperationException("Failed to create Shell on UI thread");

        _trackedShells.Add(shell);
        return shell;
    }

    /// <summary>
    /// Creates a Shell with the specified style, with auto-tracking.
    /// </summary>
    /// <param name="style">The style bits for the Shell.</param>
    /// <returns>A new Shell instance.</returns>
    protected Shell CreateShell(int style)
    {
        Shell? shell = null;
        RunOnUIThread(() =>
        {
            shell = new Shell(Display, style);
        });

        if (shell == null)
            throw new InvalidOperationException("Failed to create Shell on UI thread");

        _trackedShells.Add(shell);
        return shell;
    }

    /// <summary>
    /// Executes an action on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    protected void RunOnUIThread(Action action)
    {
        Display.SyncExec(action);
    }

    /// <summary>
    /// Executes a function on the UI thread and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
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
    /// Verifies that all tracked shells and their children are disposed.
    /// Throws if any widgets remain undisposed.
    /// </summary>
    private Task VerifyDisposalAsync()
    {
        var undisposedWidgets = new List<string>();

        foreach (var shell in _trackedShells)
        {
            if (!shell.IsDisposed)
            {
                undisposedWidgets.Add($"Shell: {shell}");

                // Check children recursively
                try
                {
                    var children = GetUndisposedChildren(shell);
                    undisposedWidgets.AddRange(children);
                }
                catch
                {
                    // Shell may be in inconsistent state
                }
            }
        }

        if (undisposedWidgets.Count > 0)
        {
            var message = $"Test left {undisposedWidgets.Count} widget(s) undisposed:\n" +
                         string.Join("\n", undisposedWidgets);
            throw new InvalidOperationException(message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a list of undisposed children from a composite.
    /// </summary>
    private List<string> GetUndisposedChildren(Composite composite)
    {
        var result = new List<string>();

        Control[]? children = null;
        try
        {
            RunOnUIThread(() =>
            {
                children = composite.Children;
            });
        }
        catch
        {
            return result;
        }

        if (children == null) return result;

        foreach (var child in children)
        {
            if (!child.IsDisposed)
            {
                result.Add($"  {child.GetType().Name}: {child}");

                if (child is Composite childComposite)
                {
                    result.AddRange(GetUndisposedChildren(childComposite));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Verifies that the event queue is clean (no pending events).
    /// Throws if events are still pending after draining attempts.
    /// </summary>
    private Task VerifyEventQueueCleanAsync()
    {
        // Drain the event queue
        int pendingEvents = 0;
        const int maxDrainAttempts = 100;

        RunOnUIThread(() =>
        {
            for (int i = 0; i < maxDrainAttempts; i++)
            {
                if (!Display.ReadAndDispatch())
                {
                    break;
                }
                pendingEvents++;
            }
        });

        // If we hit the max, there might be an infinite event loop
        if (pendingEvents >= maxDrainAttempts)
        {
            throw new InvalidOperationException(
                $"Event queue still has pending events after {maxDrainAttempts} dispatch attempts. " +
                "Possible infinite event loop or dirty queue.");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Captures diagnostic information when a test fails.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    private Task CaptureFailureDiagnosticsAsync(Exception exception)
    {
        try
        {
            var testName = GetType().Name + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var diagnosticsPath = Path.Combine(TestResultsFolder, $"{testName}_diagnostics.txt");

            var diagnostics = new List<string>
            {
                $"Test: {GetType().FullName}",
                $"Time: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Exception: {exception.GetType().Name}: {exception.Message}",
                "",
                "Stack Trace:",
                exception.StackTrace ?? "(none)",
                "",
                "Widget Tree:"
            };

            // Capture widget tree
            foreach (var shell in _trackedShells)
            {
                diagnostics.Add($"Shell: {shell} (Disposed: {shell.IsDisposed})");
                if (!shell.IsDisposed)
                {
                    try
                    {
                        diagnostics.AddRange(GetWidgetTree(shell, "  "));
                    }
                    catch (Exception treeEx)
                    {
                        diagnostics.Add($"  (Error capturing tree: {treeEx.Message})");
                    }
                }
            }

            File.WriteAllLines(diagnosticsPath, diagnostics);
        }
        catch
        {
            // Don't let diagnostic capture failures mask the original error
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a text representation of the widget tree for diagnostics.
    /// </summary>
    private List<string> GetWidgetTree(Composite composite, string indent)
    {
        var result = new List<string>();

        Control[]? children = null;
        try
        {
            RunOnUIThread(() =>
            {
                children = composite.Children;
            });
        }
        catch
        {
            return result;
        }

        if (children == null) return result;

        foreach (var child in children)
        {
            result.Add($"{indent}{child.GetType().Name}: Disposed={child.IsDisposed}");

            if (child is Composite childComposite && !child.IsDisposed)
            {
                result.AddRange(GetWidgetTree(childComposite, indent + "  "));
            }
        }

        return result;
    }

    /// <summary>
    /// Cleans up all tracked shells.
    /// </summary>
    private void CleanupTrackedShells()
    {
        RunOnUIThread(() =>
        {
            foreach (var shell in _trackedShells)
            {
                try
                {
                    if (!shell.IsDisposed)
                    {
                        shell.Dispose();
                    }
                }
                catch
                {
                    // Swallow disposal exceptions during cleanup
                }
            }
        });

        _trackedShells.Clear();
    }

    /// <summary>
    /// IDisposable implementation for synchronous disposal.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Cleanup is handled in DisposeAsync
            }
            _disposed = true;
        }
    }
}
