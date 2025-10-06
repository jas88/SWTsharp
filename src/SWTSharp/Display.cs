using System.Runtime.InteropServices;

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
    private readonly List<Shell> _shells = new();
    private bool _running;

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
                    foreach (var shell in _shells.ToArray())
                    {
                        shell.Dispose();
                    }
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
