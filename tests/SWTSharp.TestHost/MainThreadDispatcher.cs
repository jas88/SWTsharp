using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SWTSharp.TestHost;

/// <summary>
/// Dispatcher that runs code on the macOS main thread (thread 1).
/// This solves the NSWindow "must be created on main thread" requirement.
/// </summary>
public static class MainThreadDispatcher
{
    private static readonly BlockingCollection<Action> _workQueue = new();
    private static Thread? _mainThread;
    private static bool _running = true;
    private static readonly ManualResetEventSlim _initialized = new(false);

    public static int MainThreadId => _mainThread?.ManagedThreadId ?? -1;

    /// <summary>
    /// Initializes the dispatcher with the current thread as main thread.
    /// Must be called from Thread 1 on macOS.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized.IsSet)
        {
            Console.WriteLine("[INFO] MainThreadDispatcher: Already initialized");
            return;
        }

        _mainThread = Thread.CurrentThread;
        Console.WriteLine($"[INFO] MainThreadDispatcher: Initialized on Thread {_mainThread.ManagedThreadId}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("[INFO] MainThreadDispatcher: Calling NSApplicationLoad...");
            try
            {
                NSApplicationLoad();
                Console.WriteLine("[INFO] MainThreadDispatcher: NSApplicationLoad completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] MainThreadDispatcher: NSApplicationLoad failed: {ex.Message}");
            }
        }

        _initialized.Set();
    }

    /// <summary>
    /// Starts the dispatcher loop on the CURRENT thread.
    /// This will block until Stop() is called.
    /// </summary>
    public static void RunLoop()
    {
        if (!_initialized.IsSet)
        {
            throw new InvalidOperationException("MainThreadDispatcher not initialized. Call Initialize() first.");
        }

        Console.WriteLine($"[INFO] MainThreadDispatcher: Starting dispatch loop on Thread {Thread.CurrentThread.ManagedThreadId}");

        // Dispatch loop
        while (_running)
        {
            try
            {
                // Block for up to 100ms waiting for work
                if (_workQueue.TryTake(out var action, 100))
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] MainThreadDispatcher: Unhandled exception: {ex}");
            }
        }

        Console.WriteLine("[INFO] MainThreadDispatcher: Dispatch loop stopped");
    }

    public static void Stop()
    {
        _running = false;
        // Post a dummy action to wake up the loop
        try
        {
            _workQueue.Add(() => { });
        }
        catch (InvalidOperationException)
        {
            // Already completed
        }
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
            throw new InvalidOperationException("MainThreadDispatcher not initialized. Call Initialize() first.");
        }

        if (Thread.CurrentThread.ManagedThreadId == _mainThread?.ManagedThreadId)
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
        if (!_initialized.IsSet)
        {
            throw new InvalidOperationException("MainThreadDispatcher not initialized. Call Initialize() first.");
        }

        if (Thread.CurrentThread.ManagedThreadId == _mainThread?.ManagedThreadId)
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

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern void NSApplicationLoad();
}
