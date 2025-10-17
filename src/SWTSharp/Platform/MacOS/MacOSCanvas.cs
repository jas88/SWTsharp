using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of Canvas widget using custom NSView with drawRect: override.
/// A Canvas is a drawable composite widget that supports Quartz 2D drawing operations.
/// </summary>
internal class MacOSCanvas : MacOSWidget, IPlatformComposite
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

    public MacOSCanvas(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSCanvas] Creating canvas. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create custom NSView for drawing
        // In a full implementation, we would create a custom NSView subclass with drawRect: override
        // For now, we use a standard NSView and rely on layer-backed drawing
        _nsViewHandle = CreateCustomView(parentHandle, style);

        if (_nsViewHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSView for canvas");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSCanvas] Canvas created successfully. Handle: 0x{_nsViewHandle:X}");
    }

    private IntPtr CreateCustomView(IntPtr parentHandle, int style)
    {
        // Get NSView class
        IntPtr nsViewClass = objc_getClass("NSView");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr view = objc_msgSend(nsViewClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        view = objc_msgSend(view, initSelector);

        // Enable layer-backing for better drawing performance
        // This allows the view to have a CALayer backing store
        IntPtr setWantsLayerSelector = sel_registerName("setWantsLayer:");
        objc_msgSend_void(view, setWantsLayerSelector, true);

        // Set background color via layer
        SetViewBackgroundColor(view, _background);

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

    /// <summary>
    /// Sets the background color of the view using its layer.
    /// </summary>
    private void SetViewBackgroundColor(IntPtr view, RGB color)
    {
        // Get the view's layer
        IntPtr layerSelector = sel_registerName("layer");
        IntPtr layer = objc_msgSend(view, layerSelector);

        if (layer == IntPtr.Zero)
            return;

        // Create CGColor from RGB values
        // We need to use NSColor and convert to CGColor
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr colorWithRGBSelector = sel_registerName("colorWithRed:green:blue:alpha:");

        // Call [NSColor colorWithRed:green:blue:alpha:]
        IntPtr nsColor = objc_msgSend_rgba(nsColorClass, colorWithRGBSelector,
            color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);

        // Convert NSColor to CGColor
        IntPtr cgColorSelector = sel_registerName("CGColor");
        IntPtr cgColor = objc_msgSend(nsColor, cgColorSelector);

        // Set layer background color
        IntPtr setBackgroundColorSelector = sel_registerName("setBackgroundColor:");
        objc_msgSend(layer, setBackgroundColorSelector, cgColor);
    }

    /// <summary>
    /// Forces a repaint of the canvas by marking it as needing display.
    /// </summary>
    public void Redraw()
    {
        if (_nsViewHandle != IntPtr.Zero)
        {
            IntPtr setNeedsDisplaySelector = sel_registerName("setNeedsDisplay:");
            objc_msgSend_void(_nsViewHandle, setNeedsDisplaySelector, true);
        }
    }

    /// <summary>
    /// Forces a repaint of a specific region of the canvas.
    /// </summary>
    public void Redraw(int x, int y, int width, int height)
    {
        if (_nsViewHandle == IntPtr.Zero) return;

        var rect = new CGRect(x, y, width, height);
        IntPtr setNeedsDisplayInRectSelector = sel_registerName("setNeedsDisplayInRect:");
        objc_msgSend_rect(_nsViewHandle, setNeedsDisplayInRectSelector, rect);
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
        // NSView doesn't have enabled state - this is a no-op for canvas
    }

    public bool GetEnabled()
    {
        // NSView doesn't have enabled state - canvas is always "enabled"
        return true;
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        if (_nsViewHandle != IntPtr.Zero)
        {
            SetViewBackgroundColor(_nsViewHandle, color);
            Redraw();
        }
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
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

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_rgba(IntPtr receiver, IntPtr selector,
        double red, double green, double blue, double alpha);

    #endregion
}
