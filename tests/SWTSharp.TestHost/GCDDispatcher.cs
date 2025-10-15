using System.Runtime.InteropServices;

namespace SWTSharp.TestHost;

/// <summary>
/// Grand Central Dispatch (GCD) based dispatcher for macOS.
/// Uses native macOS libdispatch for main thread execution.
/// This is more efficient and integrates better with AppKit than a custom loop.
/// </summary>
public static class GCDDispatcher
{
    private const string LibSystem = "/usr/lib/libSystem.dylib";

    [StructLayout(LayoutKind.Sequential)]
    private struct dispatch_queue_t
    {
        public IntPtr Handle;

        public static implicit operator IntPtr(dispatch_queue_t queue) => queue.Handle;
        public static implicit operator dispatch_queue_t(IntPtr handle) => new dispatch_queue_t { Handle = handle };
    }

    [DllImport(LibSystem, EntryPoint = "dispatch_get_main_queue")]
    private static extern dispatch_queue_t dispatch_get_main_queue();

    [DllImport(LibSystem, EntryPoint = "dispatch_async_f")]
    private static extern void dispatch_async_f(
        dispatch_queue_t queue,
        IntPtr context,
        IntPtr work);

    [DllImport(LibSystem, EntryPoint = "dispatch_sync_f")]
    private static extern void dispatch_sync_f(
        dispatch_queue_t queue,
        IntPtr context,
        IntPtr work);

    [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
    private static extern void NSApplicationLoad();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void WorkDelegate(IntPtr context);

    private static readonly dispatch_queue_t _mainQueue;
    private static readonly WorkDelegate _asyncWorkCallback;
    private static readonly WorkDelegate _syncWorkCallback;
    private static bool _initialized = false;
    private static readonly object _initLock = new object();

    static GCDDispatcher()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _mainQueue = dispatch_get_main_queue();
            _asyncWorkCallback = ExecuteAsyncWork;
            _syncWorkCallback = ExecuteSyncWork;
        }
    }

    public static bool IsInitialized => _initialized;

    /// <summary>
    /// Initializes GCD dispatcher and loads NSApplication.
    /// Should be called from main thread.
    /// </summary>
    public static void Initialize()
    {
        lock (_initLock)
        {
            if (_initialized)
            {
                Console.WriteLine("[INFO] GCDDispatcher: Already initialized");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.WriteLine($"[INFO] GCDDispatcher: Initializing on Thread {Thread.CurrentThread.ManagedThreadId}");
                Console.WriteLine("[INFO] GCDDispatcher: Calling NSApplicationLoad...");

                try
                {
                    NSApplicationLoad();
                    Console.WriteLine("[INFO] GCDDispatcher: NSApplicationLoad completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] GCDDispatcher: NSApplicationLoad failed: {ex.Message}");
                }
            }

            _initialized = true;
        }
    }

    /// <summary>
    /// Executes an action asynchronously on the main thread.
    /// Returns immediately without waiting for completion.
    /// </summary>
    public static void InvokeAsync(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            action();
            return;
        }

        var handle = GCHandle.Alloc(action);
        dispatch_async_f(
            _mainQueue,
            GCHandle.ToIntPtr(handle),
            Marshal.GetFunctionPointerForDelegate(_asyncWorkCallback));
    }

    /// <summary>
    /// Executes an action synchronously on the main thread.
    /// Blocks until the action completes.
    /// </summary>
    public static void Invoke(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            action();
            return;
        }

        // TODO: Check if we're already on main thread to avoid deadlock
        // For now, wrap in exception handler
        var wrapper = new SyncWorkContext { Action = action };
        var handle = GCHandle.Alloc(wrapper);

        try
        {
            dispatch_sync_f(
                _mainQueue,
                GCHandle.ToIntPtr(handle),
                Marshal.GetFunctionPointerForDelegate(_syncWorkCallback));

            if (wrapper.Exception != null)
            {
                throw new InvalidOperationException("Exception on main thread", wrapper.Exception);
            }
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// Executes a function synchronously on the main thread and returns the result.
    /// </summary>
    public static T Invoke<T>(Func<T> func)
    {
        if (func == null)
            throw new ArgumentNullException(nameof(func));

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return func();
        }

        T? result = default;
        Exception? exception = null;

        Invoke(() =>
        {
            try
            {
                result = func();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        if (exception != null)
        {
            throw new InvalidOperationException("Exception on main thread", exception);
        }

        return result!;
    }

    private static void ExecuteAsyncWork(IntPtr context)
    {
        var handle = GCHandle.FromIntPtr(context);
        try
        {
            var action = (Action)handle.Target!;
            action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] GCDDispatcher: Unhandled exception in async work: {ex}");
        }
        finally
        {
            handle.Free();
        }
    }

    private static void ExecuteSyncWork(IntPtr context)
    {
        var handle = GCHandle.FromIntPtr(context);
        var wrapper = (SyncWorkContext)handle.Target!;

        try
        {
            wrapper.Action();
        }
        catch (Exception ex)
        {
            wrapper.Exception = ex;
        }
    }

    private class SyncWorkContext
    {
        public Action Action { get; set; } = null!;
        public Exception? Exception { get; set; }
    }
}
