using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS threading utilities using Grand Central Dispatch (GCD).
/// Provides automatic main thread dispatch for UI operations that require it.
/// </summary>
public static class MacOSThreading
{
    private const string LibSystem = "/usr/lib/libSystem.dylib";

    [StructLayout(LayoutKind.Sequential)]
    private struct dispatch_queue_t
    {
        public IntPtr Handle;
    }

    [DllImport(LibSystem, EntryPoint = "dispatch_get_main_queue")]
    private static extern dispatch_queue_t dispatch_get_main_queue();

    [DllImport(LibSystem, EntryPoint = "dispatch_sync_f")]
    private static extern void dispatch_sync_f(
        dispatch_queue_t queue,
        IntPtr context,
        IntPtr work);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void WorkDelegate(IntPtr context);

    private static readonly dispatch_queue_t _mainQueue;
    private static readonly WorkDelegate? _workCallback;
    private static readonly bool _isMacOS;

    static MacOSThreading()
    {
        _isMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        if (_isMacOS)
        {
            _mainQueue = dispatch_get_main_queue();
            _workCallback = ExecuteWork;
        }
    }

    /// <summary>
    /// Executes an action on the macOS main thread if needed.
    /// On other platforms, executes directly.
    /// </summary>
    public static void EnsureMainThread(Action action)
    {
        if (!_isMacOS)
        {
            action();
            return;
        }

        // Use GCD to execute on main thread
        var wrapper = new WorkContext { Action = action };
        var handle = GCHandle.Alloc(wrapper);

        try
        {
            dispatch_sync_f(
                _mainQueue,
                GCHandle.ToIntPtr(handle),
                Marshal.GetFunctionPointerForDelegate(_workCallback!));

            if (wrapper.Exception != null)
            {
                throw wrapper.Exception;
            }
        }
        finally
        {
            handle.Free();
        }
    }

    /// <summary>
    /// Executes a function on the macOS main thread and returns the result.
    /// On other platforms, executes directly.
    /// </summary>
    public static T EnsureMainThread<T>(Func<T> func)
    {
        if (!_isMacOS)
        {
            return func();
        }

        T? result = default;
        Exception? exception = null;

        EnsureMainThread(() =>
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
            throw exception;
        }

        return result!;
    }

    private static void ExecuteWork(IntPtr context)
    {
        var handle = GCHandle.FromIntPtr(context);
        var wrapper = (WorkContext)handle.Target!;

        try
        {
            wrapper.Action();
        }
        catch (Exception ex)
        {
            wrapper.Exception = ex;
        }
    }

    private class WorkContext
    {
        public Action Action { get; set; } = null!;
        public Exception? Exception { get; set; }
    }
}
