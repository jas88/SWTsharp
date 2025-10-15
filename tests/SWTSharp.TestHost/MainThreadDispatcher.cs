using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SWTSharp.TestHost;

/// <summary>
/// Dispatcher that runs code on the macOS main thread (thread 1).
/// This solves the NSWindow "must be created on main thread" requirement.
/// Uses CFRunLoop on macOS, custom queue on other platforms.
/// </summary>
public static class MainThreadDispatcher
{
    private static readonly BlockingCollection<Action> _workQueue = new();
    private static Thread? _mainThread;
    private static bool _running = true;
    private static readonly ManualResetEventSlim _initialized = new(false);
    private static readonly ManualResetEventSlim _runLoopReady = new(false);
    private static IntPtr _mainQueue = IntPtr.Zero;
    private static IntPtr _mainRunLoop = IntPtr.Zero;

    public static int MainThreadId => _mainThread?.ManagedThreadId ?? -1;
    public static bool IsInitialized => _initialized.IsSet;

    /// <summary>
    /// Starts a dedicated UI thread that runs the dispatch loop.
    /// On macOS, runs CFRunLoop. On Windows/Linux, runs custom queue.
    /// This method returns immediately - the UI thread runs in the background.
    /// </summary>
    public static void StartUIThread()
    {
        if (_initialized.IsSet)
        {
            Console.WriteLine("[INFO] MainThreadDispatcher: Already initialized");
            return;
        }

        // Start UI thread
        var uiThread = new Thread(() =>
        {
            // Initialize on THIS thread (the UI thread)
            Initialize();

            // Run the dispatch loop (blocks until Stop() is called)
            RunLoop();
        })
        {
            Name = "UI Thread",
            IsBackground = true // Background thread - exits when main app exits
        };

        uiThread.Start();

        // Wait for initialization to complete
        if (!_initialized.Wait(TimeSpan.FromSeconds(5)))
        {
            throw new TimeoutException("UI thread failed to initialize within 5 seconds");
        }

        // On macOS, also wait for run loop handles to be ready
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (!_runLoopReady.Wait(TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException("UI thread run loop failed to initialize within 5 seconds");
            }
        }

        Console.WriteLine($"[INFO] MainThreadDispatcher: UI thread started on Thread {MainThreadId}");
    }

    // Objective-C runtime
    private const string LibObjC = "/usr/lib/libobjc.A.dylib";

    [DllImport(LibObjC, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(LibObjC, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    // GCD functions
    private const string LibSystem = "/usr/lib/libSystem.dylib";

    [DllImport(LibSystem, EntryPoint = "dispatch_async_f")]
    private static extern void dispatch_async_f(IntPtr queue, IntPtr context, IntPtr work);

    [DllImport(LibSystem, EntryPoint = "dispatch_main")]
    private static extern void dispatch_main();

    // Core Foundation run loop
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    [DllImport(CoreFoundation, EntryPoint = "CFRunLoopGetMain")]
    private static extern IntPtr CFRunLoopGetMain();

    [DllImport(CoreFoundation, EntryPoint = "CFRunLoopGetCurrent")]
    private static extern IntPtr CFRunLoopGetCurrent();

    [DllImport(CoreFoundation, EntryPoint = "CFRunLoopRun")]
    private static extern void CFRunLoopRun();

    [DllImport(CoreFoundation, EntryPoint = "CFRunLoopStop")]
    private static extern void CFRunLoopStop(IntPtr rl);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void WorkDelegate(IntPtr context);

    private static readonly WorkDelegate _workCallback = WorkCallback;

    private static void WorkCallback(IntPtr context)
    {
        try
        {
            var handle = GCHandle.FromIntPtr(context);
            var wrapper = (WorkContext)handle.Target!;
            wrapper.Action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] MainThreadDispatcher callback failed: {ex}");
        }
    }

    private class WorkContext
    {
        public required Action Action { get; init; }
    }

    /// <summary>
    /// Get GCD main queue via NSOperationQueue.mainQueue.underlyingQueue
    /// </summary>
    private static IntPtr GetMainQueue()
    {
        var nsOperationQueueClass = objc_getClass("NSOperationQueue");
        var mainQueueSelector = sel_registerName("mainQueue");
        var mainOperationQueue = objc_msgSend(nsOperationQueueClass, mainQueueSelector);
        var underlyingQueueSelector = sel_registerName("underlyingQueue");
        var mainDispatchQueue = objc_msgSend(mainOperationQueue, underlyingQueueSelector);
        return mainDispatchQueue;
    }

    /// <summary>
    /// Initializes the dispatcher with the current thread as main thread.
    /// On macOS, MUST be called from the process's main thread (Thread 1)
    /// because NSApplicationLoad() requires the main thread.
    /// RunLoop() must then be called from this same thread.
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
    /// On macOS, runs CFRunLoop which processes GCD main queue.
    /// On other platforms, uses custom blocking queue.
    /// IMPORTANT: On macOS, MUST be called from the same thread where Initialize() was called
    /// (the thread where NSApplicationLoad() ran).
    /// </summary>
    public static void RunLoop()
    {
        if (!_initialized.IsSet)
        {
            throw new InvalidOperationException("MainThreadDispatcher not initialized. Call Initialize() first.");
        }

        // Validate we're on the same thread as Initialize() was called on
        if (Thread.CurrentThread.ManagedThreadId != _mainThread?.ManagedThreadId)
        {
            throw new InvalidOperationException(
                $"RunLoop() must be called from the same thread as Initialize(). " +
                $"Expected Thread {_mainThread?.ManagedThreadId}, but called from Thread {Thread.CurrentThread.ManagedThreadId}. " +
                $"On macOS, CFRunLoop must run on the thread where NSApplicationLoad() was called.");
        }

        Console.WriteLine($"[INFO] MainThreadDispatcher: Starting dispatch loop on Thread {Thread.CurrentThread.ManagedThreadId}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: Run CFRunLoop on Thread 1
            _mainRunLoop = CFRunLoopGetCurrent();
            Console.WriteLine($"[INFO] MainThreadDispatcher: Running CFRunLoop on Thread 1 (handle: 0x{_mainRunLoop:X})");

            // Signal that run loop is ready BEFORE starting CFRunLoop
            _runLoopReady.Set();

            // This blocks until CFRunLoopStop() is called
            CFRunLoopRun();

            Console.WriteLine("[INFO] MainThreadDispatcher: CFRunLoop stopped");
        }
        else
        {
            // Windows/Linux: use custom dispatch loop
            Console.WriteLine("[INFO] MainThreadDispatcher: Running custom dispatch loop");
            _runLoopReady.Set();

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
            Console.WriteLine("[INFO] MainThreadDispatcher: Custom dispatch loop stopped");
        }
    }

    public static void Stop()
    {
        _running = false;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && _mainRunLoop != IntPtr.Zero)
        {
            // Stop CFRunLoop
            CFRunLoopStop(_mainRunLoop);
        }
        else
        {
            // Post a dummy action to wake up the custom loop
            try
            {
                _workQueue.Add(() => { });
            }
            catch (InvalidOperationException)
            {
                // Already completed
            }
        }
    }

    /// <summary>
    /// Executes an action synchronously on the main thread.
    /// If already on the main thread, executes immediately.
    /// Otherwise, blocks until execution completes.
    /// On macOS, uses GCD dispatch_async_f to submit to CFRunLoop.
    /// On other platforms, uses custom blocking queue.
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: Use GCD dispatch_async_f to submit to CFRunLoop
            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Dispatching from Thread {Thread.CurrentThread.ManagedThreadId} to UI Thread {MainThreadId} via GCD");

            Exception? exception = null;
            var completed = new ManualResetEventSlim(false);

            var wrapper = new WorkContext
            {
                Action = () =>
                {
                    Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Executing on Thread {Thread.CurrentThread.ManagedThreadId}");
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"[ERROR] MainThreadDispatcher.Invoke: Exception: {ex}");
                        exception = ex;
                    }
                    finally
                    {
                        Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Completed on Thread {Thread.CurrentThread.ManagedThreadId}");
                        completed.Set();
                    }
                }
            };

            var handle = GCHandle.Alloc(wrapper);
            var mainQueue = GetMainQueue();
            var callbackPtr = Marshal.GetFunctionPointerForDelegate(_workCallback);

            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Dispatching to GCD main queue (handle: 0x{GCHandle.ToIntPtr(handle):X})");
            dispatch_async_f(mainQueue, GCHandle.ToIntPtr(handle), callbackPtr);

            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Waiting for completion...");
            completed.Wait();
            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Wait completed");

            if (exception != null)
            {
                throw new InvalidOperationException("Exception on main thread", exception);
            }
        }
        else
        {
            // Windows/Linux: Use custom BlockingCollection queue
            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Dispatching from Thread {Thread.CurrentThread.ManagedThreadId} to UI Thread {MainThreadId}");
            Exception? exception = null;
            var completed = new ManualResetEventSlim(false);

            _workQueue.Add(() =>
            {
                Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Executing on Thread {Thread.CurrentThread.ManagedThreadId}");
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[ERROR] MainThreadDispatcher.Invoke: Exception: {ex}");
                    exception = ex;
                }
                finally
                {
                    Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Completed on Thread {Thread.CurrentThread.ManagedThreadId}");
                    completed.Set();
                }
            });

            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Waiting for completion...");
            completed.Wait();
            Console.WriteLine($"[INFO] MainThreadDispatcher.Invoke: Wait completed");

            if (exception != null)
            {
                throw new InvalidOperationException("Exception on main thread", exception);
            }
        }
    }

    /// <summary>
    /// Executes a function synchronously on the main thread and returns the result.
    /// On macOS, uses GCD dispatch_async_f to submit to CFRunLoop.
    /// On other platforms, uses custom BlockingCollection queue.
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: Use GCD dispatch_async_f
            T? result = default;
            Exception? exception = null;
            var completed = new ManualResetEventSlim(false);

            var wrapper = new WorkContext
            {
                Action = () =>
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
                }
            };

            var handle = GCHandle.Alloc(wrapper);
            var mainQueue = GetMainQueue();
            var callbackPtr = Marshal.GetFunctionPointerForDelegate(_workCallback);

            dispatch_async_f(mainQueue, GCHandle.ToIntPtr(handle), callbackPtr);

            completed.Wait();

            if (exception != null)
            {
                throw new InvalidOperationException("Exception on main thread", exception);
            }

            return result!;
        }
        else
        {
            // Windows/Linux: Use custom BlockingCollection queue
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
    }

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern void NSApplicationLoad();
}
