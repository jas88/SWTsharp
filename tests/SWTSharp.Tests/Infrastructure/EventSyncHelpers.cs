using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// CA1031: Generic catch is intentional for async error propagation via TCS
#pragma warning disable CA1031

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Provides TaskCompletionSource-based event synchronization helpers for async-friendly testing.
/// These methods avoid polling loops (Thread.Sleep in while loop) and use event-based signaling instead.
/// </summary>
/// <remarks>
/// CRITICAL: All TaskCompletionSource instances use TaskCreationOptions.RunContinuationsAsynchronously
/// to prevent deadlocks. Without this flag, continuations may run synchronously on the thread calling
/// SetResult, which can cause deadlocks if that thread holds locks the continuation needs.
/// </remarks>
public static class EventSyncHelpers
{
    /// <summary>
    /// Default timeout for synchronization operations (5 seconds per CONTEXT.md).
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Waits asynchronously for an event to fire.
    /// </summary>
    /// <typeparam name="T">The type of the event argument.</typeparam>
    /// <param name="subscribe">Action that subscribes a handler to the event.</param>
    /// <param name="unsubscribe">Action that unsubscribes a handler from the event.</param>
    /// <param name="timeout">Optional timeout. Defaults to 5 seconds.</param>
    /// <returns>A Task that completes with the event argument when the event fires.</returns>
    /// <exception cref="TimeoutException">Thrown if the event does not fire within the timeout period.</exception>
    /// <exception cref="ArgumentNullException">Thrown if subscribe or unsubscribe is null.</exception>
    /// <example>
    /// <code>
    /// var args = await EventSyncHelpers.WaitForEvent&lt;SelectionEventArgs&gt;(
    ///     handler => button.Selection += handler,
    ///     handler => button.Selection -= handler);
    /// </code>
    /// </example>
    public static async Task<T> WaitForEvent<T>(
        Action<Action<T>> subscribe,
        Action<Action<T>> unsubscribe,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);

        var effectiveTimeout = timeout ?? DefaultTimeout;
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = new CancellationTokenSource(effectiveTimeout);

        Action<T>? handler = null;
        handler = (args) =>
        {
            // Only set result once (first event wins)
            tcs.TrySetResult(args);
        };

        // Register cancellation to timeout the TCS
        using var registration = cts.Token.Register(() =>
        {
            tcs.TrySetException(new TimeoutException(
                $"Event did not fire within {effectiveTimeout.TotalSeconds} seconds"));
        });

        try
        {
            subscribe(handler);
            return await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            unsubscribe(handler);
        }
    }

    /// <summary>
    /// Waits asynchronously for a condition to become true without polling.
    /// Uses a periodic timer to check the condition at regular intervals.
    /// </summary>
    /// <param name="condition">The condition to wait for.</param>
    /// <param name="timeout">Optional timeout. Defaults to 5 seconds.</param>
    /// <param name="pollInterval">Optional interval between condition checks. Defaults to 50ms.</param>
    /// <returns>A Task that completes when the condition becomes true.</returns>
    /// <exception cref="TimeoutException">Thrown if the condition does not become true within the timeout period.</exception>
    /// <exception cref="ArgumentNullException">Thrown if condition is null.</exception>
    /// <remarks>
    /// While this method does use periodic checking internally, it does so via async timers
    /// rather than Thread.Sleep, avoiding blocking threads and allowing proper async/await composition.
    /// </remarks>
    public static async Task WaitForCondition(
        Func<bool> condition,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var effectiveTimeout = timeout ?? DefaultTimeout;
        var effectivePollInterval = pollInterval ?? TimeSpan.FromMilliseconds(50);
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = new CancellationTokenSource(effectiveTimeout);

        // Check condition immediately
        if (condition())
        {
            return;
        }

        Timer? timer = null;
        timer = new Timer(_ =>
        {
            try
            {
                if (condition())
                {
                    tcs.TrySetResult(true);
                    timer?.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
                timer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }, null, effectivePollInterval, effectivePollInterval);

        // Register cancellation to timeout the TCS
        using var registration = cts.Token.Register(() =>
        {
            timer?.Change(Timeout.Infinite, Timeout.Infinite);
            tcs.TrySetException(new TimeoutException(
                $"Condition did not become true within {effectiveTimeout.TotalSeconds} seconds"));
        });

        try
        {
            await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            timer?.Dispose();
        }
    }

    /// <summary>
    /// Waits asynchronously for an event to fire a specified number of times.
    /// </summary>
    /// <typeparam name="T">The type of the event argument.</typeparam>
    /// <param name="subscribe">Action that subscribes a handler to the event.</param>
    /// <param name="unsubscribe">Action that unsubscribes a handler from the event.</param>
    /// <param name="count">The number of times the event must fire.</param>
    /// <param name="timeout">Optional timeout. Defaults to 5 seconds.</param>
    /// <returns>A Task that completes with a list of all captured event arguments.</returns>
    /// <exception cref="TimeoutException">Thrown if the required number of events do not fire within the timeout period.</exception>
    /// <exception cref="ArgumentNullException">Thrown if subscribe or unsubscribe is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if count is less than 1.</exception>
    /// <example>
    /// <code>
    /// // Wait for 3 selection events
    /// var allArgs = await EventSyncHelpers.WaitForEventCount&lt;SelectionEventArgs&gt;(
    ///     handler => button.Selection += handler,
    ///     handler => button.Selection -= handler,
    ///     count: 3);
    /// </code>
    /// </example>
    public static async Task<List<T>> WaitForEventCount<T>(
        Action<Action<T>> subscribe,
        Action<Action<T>> unsubscribe,
        int count,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);
        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count must be at least 1");
        }

        var effectiveTimeout = timeout ?? DefaultTimeout;
        var tcs = new TaskCompletionSource<List<T>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var capturedArgs = new List<T>();
        var lockObj = new object();
        using var cts = new CancellationTokenSource(effectiveTimeout);

        Action<T>? handler = null;
        handler = (args) =>
        {
            lock (lockObj)
            {
                capturedArgs.Add(args);
                if (capturedArgs.Count >= count)
                {
                    tcs.TrySetResult(new List<T>(capturedArgs));
                }
            }
        };

        // Register cancellation to timeout the TCS
        using var registration = cts.Token.Register(() =>
        {
            lock (lockObj)
            {
                tcs.TrySetException(new TimeoutException(
                    $"Only {capturedArgs.Count} of {count} events fired within {effectiveTimeout.TotalSeconds} seconds"));
            }
        });

        try
        {
            subscribe(handler);
            return await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            unsubscribe(handler);
        }
    }

    /// <summary>
    /// Waits asynchronously for an event to fire, executing a trigger action before waiting.
    /// This is useful when you need to trigger an event and wait for the result.
    /// </summary>
    /// <typeparam name="T">The type of the event argument.</typeparam>
    /// <param name="subscribe">Action that subscribes a handler to the event.</param>
    /// <param name="unsubscribe">Action that unsubscribes a handler from the event.</param>
    /// <param name="trigger">Action to trigger the event.</param>
    /// <param name="timeout">Optional timeout. Defaults to 5 seconds.</param>
    /// <returns>A Task that completes with the event argument when the event fires.</returns>
    /// <exception cref="TimeoutException">Thrown if the event does not fire within the timeout period.</exception>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public static async Task<T> WaitForEventWithTrigger<T>(
        Action<Action<T>> subscribe,
        Action<Action<T>> unsubscribe,
        Action trigger,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(subscribe);
        ArgumentNullException.ThrowIfNull(unsubscribe);
        ArgumentNullException.ThrowIfNull(trigger);

        var effectiveTimeout = timeout ?? DefaultTimeout;
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cts = new CancellationTokenSource(effectiveTimeout);

        Action<T>? handler = null;
        handler = (args) =>
        {
            tcs.TrySetResult(args);
        };

        // Register cancellation to timeout the TCS
        using var registration = cts.Token.Register(() =>
        {
            tcs.TrySetException(new TimeoutException(
                $"Event did not fire within {effectiveTimeout.TotalSeconds} seconds after trigger"));
        });

        try
        {
            subscribe(handler);
            trigger();
            return await tcs.Task.ConfigureAwait(false);
        }
        finally
        {
            unsubscribe(handler);
        }
    }
}
