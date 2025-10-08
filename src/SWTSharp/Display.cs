using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("SWTSharp.Tests")]

namespace SWTSharp;

/// <summary>
/// Represents a connection to the underlying platform's display system.
/// Each application has one or more displays, and each display has one UI thread.
/// </summary>
public class Display : IDisposable
{
    private static Display? _default;
#if NET9_0_OR_GREATER
    private static readonly Lock _lock = new Lock();
#else
    private static readonly object _lock = new object();
#endif

    private bool _disposed;
    private Thread? _thread;
    private readonly List<Shell> _shells = [];
    private bool _running;
    private readonly Queue<Action> _asyncActions = [];
    private Action<Action>? _customAsyncExecutor = null;

    /// <summary>
    /// Gets the default display for the application.
    /// </summary>
    public static Display Default
    {
        get
        {
            lock (_lock)
            {
                if (_default == null || _default._disposed)
                {
                    _default = new Display();
                }
                return _default;
            }
        }
    }

    /// <summary>
    /// Gets the current display for the calling thread.
    /// </summary>
    public static Display? Current
    {
        get
        {
            lock (_lock)
            {
                if (_default != null && _default._thread == Thread.CurrentThread)
                {
                    return _default;
                }
                return null;
            }
        }
    }

    /// <summary>
    /// Creates a new display.
    /// </summary>
    public Display()
    {
        _thread = Thread.CurrentThread;
        InitializeDisplay();
    }

    /// <summary>
    /// Checks if the calling thread is the display's UI thread.
    /// </summary>
    public bool IsValidThread()
    {
        return Thread.CurrentThread == _thread;
    }

    /// <summary>
    /// Throws an exception if called from the wrong thread.
    /// </summary>
    protected void CheckThread()
    {
        if (!IsValidThread())
        {
            throw new SWTInvalidThreadException("Invalid thread access");
        }
    }

    /// <summary>
    /// Throws an exception if the display has been disposed.
    /// </summary>
    protected void CheckDisplay()
    {
        if (_disposed)
        {
            throw new SWTDisposedException("Display has been disposed");
        }
        CheckThread();
    }

    /// <summary>
    /// Platform-specific display initialization.
    /// </summary>
    private void InitializeDisplay()
    {
        SWTSharp.Platform.PlatformFactory.Instance.Initialize();
    }

    /// <summary>
    /// Runs the event loop until all shells are closed.
    /// </summary>
    public void Run()
    {
        CheckDisplay();
        _running = true;

        while (_running && _shells.Count > 0)
        {
            if (!ReadAndDispatch())
            {
                Sleep();
            }
        }
    }

    /// <summary>
    /// Reads and dispatches a single event from the event queue.
    /// Returns false if there are no events to process.
    /// </summary>
    public bool ReadAndDispatch()
    {
        CheckDisplay();
        ProcessAsyncActions();
        return SWTSharp.Platform.PlatformFactory.Instance.ProcessEvent();
    }

    /// <summary>
    /// Sleeps until an event is available.
    /// </summary>
    public void Sleep()
    {
        CheckDisplay();
        SWTSharp.Platform.PlatformFactory.Instance.WaitForEvent();
    }

    /// <summary>
    /// Wakes up the display's event loop.
    /// </summary>
    public void Wake()
    {
        SWTSharp.Platform.PlatformFactory.Instance.WakeEventLoop();
    }

    /// <summary>
    /// Executes the specified action synchronously on the UI thread.
    /// If called from the UI thread, executes immediately.
    /// If called from another thread, blocks until execution completes.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread</param>
    /// <exception cref="ArgumentNullException">If action is null</exception>
    public void SyncExec(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (IsValidThread())
        {
            // Already on UI thread, execute immediately
            action();
        }
        else
        {
            // On different thread, need to marshal to UI thread
            Exception? exception = null;
            var done = new ManualResetEventSlim(false);

            AsyncExec(() =>
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
                    done.Set();
                }
            });

            done.Wait();
            if (exception != null)
                throw new InvalidOperationException("Exception occurred during SyncExec", exception);
        }
    }

    /// <summary>
    /// Executes the specified action asynchronously on the UI thread.
    /// Returns immediately without waiting for execution to complete.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread</param>
    /// <exception cref="ArgumentNullException">If action is null</exception>
    public void AsyncExec(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (IsValidThread())
        {
            // Already on UI thread, execute immediately
            action();
        }
        else
        {
            // If a custom async executor is registered (for testing), use it
            if (_customAsyncExecutor != null)
            {
                _customAsyncExecutor(action);
            }
            else
            {
                // Queue for execution on UI thread
                lock (_asyncActions)
                {
                    _asyncActions.Enqueue(action);
                }
                Wake();
            }
        }
    }

    /// <summary>
    /// Sets a custom async executor for testing purposes.
    /// This allows tests to override the default async execution mechanism.
    /// </summary>
    internal void SetAsyncExecutor(Action<Action> executor)
    {
        _customAsyncExecutor = executor;
    }

    /// <summary>
    /// Executes an action on the platform's main thread.
    /// On macOS, this uses Grand Central Dispatch to execute on the actual process main thread,
    /// which is required for NSWindow and other AppKit operations.
    /// On other platforms, this may execute directly or on the UI thread.
    /// Note: This does NOT check if you're on the Display's UI thread, because the whole point
    /// is to execute on a different thread (the process main thread on macOS).
    /// </summary>
    /// <param name="action">The action to execute on the main thread</param>
    /// <exception cref="ArgumentNullException">If action is null</exception>
    public void ExecuteOnMainThread(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        // Don't call CheckDisplay() - we might be calling from a different thread
        if (_disposed)
            throw new SWTDisposedException("Display has been disposed");

        SWTSharp.Platform.PlatformFactory.Instance.ExecuteOnMainThread(action);
    }

    /// <summary>
    /// Processes queued async actions. Called by the event loop.
    /// </summary>
    private void ProcessAsyncActions()
    {
        Action[] actions;
        lock (_asyncActions)
        {
            if (_asyncActions.Count == 0)
                return;
            actions = _asyncActions.ToArray();
            _asyncActions.Clear();
        }

        foreach (var action in actions)
        {
            try
            {
                action();
            }
            catch
            {
                // Swallow exceptions from async actions
            }
        }
    }

    /// <summary>
    /// Stops the event loop.
    /// </summary>
    public void Stop()
    {
        CheckDisplay();
        _running = false;
    }

    /// <summary>
    /// Registers a shell with this display.
    /// </summary>
    internal void AddShell(Shell shell)
    {
        lock (_lock)
        {
            if (!_shells.Contains(shell))
            {
                _shells.Add(shell);
            }
        }
    }

    /// <summary>
    /// Unregisters a shell from this display.
    /// </summary>
    internal void RemoveShell(Shell shell)
    {
        lock (_lock)
        {
            _shells.Remove(shell);
        }
    }

    /// <summary>
    /// Gets all shells associated with this display.
    /// </summary>
    public Shell[] GetShells()
    {
        CheckDisplay();
        lock (_lock)
        {
            return _shells.ToArray();
        }
    }

    /// <summary>
    /// Gets the active shell (the one with focus).
    /// </summary>
    public Shell? ActiveShell
    {
        get
        {
            CheckDisplay();
            lock (_lock)
            {
                return _shells.FirstOrDefault(s => s.IsActive);
            }
        }
    }

    /// <summary>
    /// Gets the platform name (win32, macosx, or linux).
    /// </summary>
    public static string Platform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return SWT.PLATFORM_WIN32;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return SWT.PLATFORM_MACOSX;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return SWT.PLATFORM_LINUX;

            throw new PlatformNotSupportedException("Unsupported platform");
        }
    }

    /// <summary>
    /// Disposes the display and all associated resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the display.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    // Dispose all shells
#if NET8_0_OR_GREATER
                    // Use CollectionsMarshal to avoid allocating ToArray()
                    var shellsSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_shells);
                    for (int i = 0; i < shellsSpan.Length; i++)
                    {
                        shellsSpan[i].Dispose();
                    }
#else
                    foreach (var shell in _shells.ToArray())
                    {
                        shell.Dispose();
                    }
#endif
                    _shells.Clear();
                }

                if (_default == this)
                {
                    _default = null;
                }
            }
            _disposed = true;
        }
    }
}
