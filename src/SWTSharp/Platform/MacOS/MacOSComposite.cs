using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of a composite platform widget.
/// Encapsulates NSView and provides IPlatformComposite functionality.
/// </summary>
internal class MacOSComposite : MacOSWidget, IPlatformComposite
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsViewHandle;
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    public MacOSComposite(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSComposite] Creating composite. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSView using objc_msgSend
        _nsViewHandle = CreateNSView(parentHandle, style);

        if (_nsViewHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSView for composite");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSComposite] Composite created successfully. Handle: 0x{_nsViewHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsViewHandle;
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        lock (_platformChildren)
        {
            if (!_platformChildren.Contains(child))
            {
                _platformChildren.Add(child);
            }
        }

        if (child is MacOSWidget macOSChild)
        {
            IntPtr childHandle = macOSChild.GetNativeHandle();
            if (childHandle != IntPtr.Zero && _nsViewHandle != IntPtr.Zero)
            {
                // objc_msgSend(_nsViewHandle, addSubview:, childHandle)
                IntPtr selector = sel_registerName("addSubview:");
                objc_msgSend(_nsViewHandle, selector, childHandle);
            }
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        lock (_platformChildren)
        {
            _platformChildren.Remove(child);
        }

        if (child is MacOSWidget macOSChild)
        {
            IntPtr childHandle = macOSChild.GetNativeHandle();
            if (childHandle != IntPtr.Zero)
            {
                // objc_msgSend(childHandle, removeFromSuperview)
                IntPtr selector = sel_registerName("removeFromSuperview");
                objc_msgSend(childHandle, selector);
            }
        }
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        lock (_platformChildren)
        {
            return _platformChildren.ToArray();
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

        // Create NSRect and call setFrame:
        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsViewHandle, selector, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return default;

        // Call frame method to get NSRect
        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsViewHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

        // NSView uses setHidden: (inverted from visible)
        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsViewHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return false;

        // NSView uses isHidden (inverted from visible)
        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsViewHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;
        // NSView doesn't have enabled state - this is a no-op for containers
    }

    public bool GetEnabled()
    {
        // NSView doesn't have enabled state - containers are always "enabled"
        return true;
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via layer or wantsLayer/backgroundColor
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Dispose children first
        lock (_platformChildren)
        {
            foreach (var child in _platformChildren.ToArray())
            {
                if (child is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _platformChildren.Clear();
        }

        // Remove from superview and release
        if (_nsViewHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsViewHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsViewHandle, releaseSelector);

            _nsViewHandle = IntPtr.Zero;
        }
    }

    private IntPtr CreateNSView(IntPtr parentHandle, int style)
    {
        // Get NSView class
        IntPtr nsViewClass = objc_getClass("NSView");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr view = objc_msgSend(nsViewClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        view = objc_msgSend(view, initSelector);

        // Set a default frame
        var frame = new CGRect(0, 0, 100, 100);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(view, setFrameSelector, frame);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, view);
        }

        return view;
    }

    #region ObjC P/Invoke

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public CGRect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_getClass(string className);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport(ObjCLibrary)]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    #endregion
}