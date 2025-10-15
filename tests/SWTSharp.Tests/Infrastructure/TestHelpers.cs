using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Helper utilities for common test operations.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Creates a test shell with automatic disposal tracking.
    /// </summary>
    public static Shell CreateTestShell(Display display, Action<Shell>? configure = null)
    {
        Shell? shell = null;
        display.SyncExec(() =>
        {
            shell = new Shell(display);
            configure?.Invoke(shell);
        });
        return shell!;
    }

    /// <summary>
    /// Creates a test button with automatic disposal tracking.
    /// </summary>
    public static Button CreateTestButton(Composite parent, string text = "Test", int style = SWT.PUSH)
    {
        Button? button = null;
        parent.Display.SyncExec(() =>
        {
            button = new Button(parent, style);
            button.Text = text;
        });
        if (button == null)
            throw new InvalidOperationException("Failed to create Button on UI thread");
        return button;
    }

    /// <summary>
    /// Creates a test label with automatic disposal tracking.
    /// </summary>
    public static Label CreateTestLabel(Composite parent, string text = "Test", int style = SWT.NONE)
    {
        Label? label = null;
        parent.Display.SyncExec(() =>
        {
            label = new Label(parent, style);
            label.Text = text;
        });
        if (label == null)
            throw new InvalidOperationException("Failed to create Label on UI thread");
        return label;
    }

    /// <summary>
    /// Creates a test composite with automatic disposal tracking.
    /// </summary>
    public static Composite CreateTestComposite(Composite parent, int style = SWT.NONE)
    {
        Composite? composite = null;
        parent.Display.SyncExec(() =>
        {
            composite = new Composite(parent, style);
        });
        if (composite == null)
            throw new InvalidOperationException("Failed to create Composite on UI thread");
        return composite;
    }

    /// <summary>
    /// Asserts that a widget is properly disposed.
    /// </summary>
    public static void AssertDisposed(Widget widget)
    {
        Assert.True(widget.IsDisposed, "Widget should be disposed");
    }

    /// <summary>
    /// Asserts that a widget is not disposed.
    /// </summary>
    public static void AssertNotDisposed(Widget widget)
    {
        Assert.False(widget.IsDisposed, "Widget should not be disposed");
    }

    /// <summary>
    /// Asserts that an event was fired with the expected parameters.
    /// </summary>
    public static void AssertEventFired<TEventArgs>(
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe,
        Action trigger,
        Action<TEventArgs>? validate = null)
        where TEventArgs : EventArgs
    {
        bool eventFired = false;
        TEventArgs? capturedArgs = default;

        EventHandler<TEventArgs> handler = (sender, args) =>
        {
            eventFired = true;
            capturedArgs = args;
        };

        subscribe(handler);
        trigger();
        unsubscribe(handler);

        Assert.True(eventFired, "Event should have been fired");
        if (validate != null && capturedArgs != null)
        {
            validate(capturedArgs);
        }
    }

    /// <summary>
    /// Asserts that an event was not fired.
    /// </summary>
    public static void AssertEventNotFired<TEventArgs>(
        Action<EventHandler<TEventArgs>> subscribe,
        Action<EventHandler<TEventArgs>> unsubscribe,
        Action trigger)
        where TEventArgs : EventArgs
    {
        bool eventFired = false;

        EventHandler<TEventArgs> handler = (sender, args) =>
        {
            eventFired = true;
        };

        subscribe(handler);
        trigger();
        unsubscribe(handler);

        Assert.False(eventFired, "Event should not have been fired");
    }

    /// <summary>
    /// Waits for a condition to become true with a timeout.
    /// </summary>
    public static bool WaitFor(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < timeout)
        {
            if (condition())
            {
                return true;
            }
            System.Threading.Thread.Sleep(10);
        }
        return false;
    }

    /// <summary>
    /// Waits for a condition to become true with a default timeout of 5 seconds.
    /// </summary>
    public static bool WaitFor(Func<bool> condition)
    {
        return WaitFor(condition, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Asserts that a condition becomes true within the specified timeout.
    /// </summary>
    public static void AssertCondition(Func<bool> condition, TimeSpan timeout, string message)
    {
        Assert.True(WaitFor(condition, timeout), message);
    }

    /// <summary>
    /// Asserts that a condition becomes true within 5 seconds.
    /// </summary>
    public static void AssertCondition(Func<bool> condition, string message)
    {
        AssertCondition(condition, TimeSpan.FromSeconds(5), message);
    }

    /// <summary>
    /// Measures the execution time of an action.
    /// </summary>
    public static TimeSpan MeasureTime(Action action)
    {
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    /// <summary>
    /// Runs an action on the UI thread and returns the execution time.
    /// </summary>
    public static TimeSpan MeasureUITime(Display display, Action action)
    {
        var elapsed = TimeSpan.Zero;
        display.SyncExec(() =>
        {
            elapsed = MeasureTime(action);
        });
        return elapsed;
    }

    /// <summary>
    /// Creates a disposable scope that ensures all widgets created within it are disposed.
    /// </summary>
    public static IDisposable CreateWidgetScope(Display display)
    {
        return new WidgetScope(display);
    }

    private sealed class WidgetScope : IDisposable
    {
        private readonly Display _display;
        private readonly List<Widget> _widgets = new();
        private bool _disposed;

        public WidgetScope(Display display)
        {
            _display = display;
        }

        public void Track(Widget widget)
        {
            _widgets.Add(widget);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _display.SyncExec(() =>
                {
                    foreach (var widget in _widgets)
                    {
                        try
                        {
                            if (!widget.IsDisposed)
                            {
                                widget.Dispose();
                            }
                        }
                        catch
                        {
                            // Swallow disposal exceptions
                        }
                    }
                });
                _widgets.Clear();
            }
        }
    }
}
