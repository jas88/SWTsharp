using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SWTSharp.Tests;

/// <summary>
/// Dispatcher that runs code on the macOS main thread (thread 1).
/// This solves the NSWindow "must be created on main thread" requirement.
/// </summary>
public static class MainThreadDispatcher
{
    private static readonly BlockingCollection<Action> _workQueue = new();
    private static readonly Thread _mainThread;
    private static bool _running = true;
    private static readonly ManualResetEventSlim _initialized = new(false);

    static MainThreadDispatcher()
    {
        // On macOS, THIS thread (Module Initializer thread, which is Thread 1)
        // must become the main dispatcher thread
        _mainThread = Thread.CurrentThread;
    }

    public static int MainThreadId => _mainThread.ManagedThreadId;

    /// <summary>
    /// Starts the dispatcher loop on the CURRENT thread.
    /// This will block until the dispatcher is stopped.
    /// Only call this from the main thread (Thread 1).
    /// </summary>
    public static void RunLoop()
    {
        if (Thread.CurrentThread != _mainThread)
        {
            throw new InvalidOperationException("RunLoop() must be called from the main thread");
        }

        Console.WriteLine($"MainThreadDispatcher.RunLoop: Starting dispatch loop on Thread {Thread.CurrentThread.ManagedThreadId}");

        // Run the dispatch loop on THIS thread
        DispatchLoop();
    }

    /// <summary>
    /// Initializes macOS AppKit without starting the loop.
    /// Call this before spawning xUnit runner on a background thread.
    /// </summary>
    public static void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine($"MainThreadDispatcher.Initialize: Calling NSApplicationLoad on Thread {Thread.CurrentThread.ManagedThreadId}...");
            try
            {
                NSApplicationLoad();
                Console.WriteLine("MainThreadDispatcher.Initialize: NSApplicationLoad completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainThreadDispatcher.Initialize: NSApplicationLoad failed: {ex.Message}");
            }
        }

        _initialized.Set();
        Console.WriteLine("MainThreadDispatcher.Initialize: Dispatcher initialized");
    }

    public static bool IsInitialized => _initialized.IsSet;

    public static void Stop()
    {
        _running = false;
        _workQueue.CompleteAdding();
    }

    /// <summary>
    /// Executes an action synchronously on the main thread.
    /// If already on the main thread, executes immediately.
    /// Otherwise, blocks until execution completes.
    /// </summary>
    public static void Invoke(Action action)
    {
        if (!_initialized.IsSet)
        {
            throw new InvalidOperationException("MainThreadDispatcher has not been started. Call Start() first.");
        }

        if (Thread.CurrentThread.ManagedThreadId == _mainThread.ManagedThreadId)
        {
            // Already on main thread, execute directly
            action();
            return;
        }

        // Marshal to main thread
        Exception? exception = null;
        var completed = new ManualResetEventSlim(false);

        _workQueue.Add(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        completed.Wait();

        if (exception != null)
        {
            throw new InvalidOperationException("Exception on main thread", exception);
        }
    }

    /// <summary>
    /// Executes a function synchronously on the main thread and returns the result.
    /// </summary>
    public static T Invoke<T>(Func<T> func)
    {
        if (Thread.CurrentThread.ManagedThreadId == _mainThread.ManagedThreadId)
        {
            // Already on main thread, execute directly
            return func();
        }

        // Marshal to main thread
        T? result = default;
        Exception? exception = null;
        var completed = new ManualResetEventSlim(false);

        _workQueue.Add(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                completed.Set();
            }
        });

        completed.Wait();

        if (exception != null)
        {
            throw new InvalidOperationException("Exception on main thread", exception);
        }

        return result!;
    }

    private static void DispatchLoop()
    {
        Console.WriteLine($"MainThreadDispatcher: Dispatch loop started on Thread {Thread.CurrentThread.ManagedThreadId}");

        if (!_initialized.IsSet)
        {
            throw new InvalidOperationException("Dispatcher not initialized. Call Initialize() first.");
        }

        // Dispatch loop
        while (_running)
        {
            try
            {
                if (_workQueue.TryTake(out var action, 100))
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainThreadDispatcher: Unhandled exception: {ex}");
            }
        }

        Console.WriteLine("MainThreadDispatcher: Dispatch loop stopped");
    }

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern void NSApplicationLoad();
}
