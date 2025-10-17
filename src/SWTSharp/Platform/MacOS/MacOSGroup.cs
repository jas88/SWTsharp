using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of a Group widget (titled border container).
/// Uses NSBox with title for the border and title, and contains child widgets.
/// </summary>
internal class MacOSGroup : MacOSWidget, IPlatformComposite, IPlatformTextWidget
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsBoxHandle;
    private IntPtr _contentViewHandle;  // NSBox's content view for children
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private string _text = string.Empty;

    // Events required by IPlatformComposite
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // Events required by IPlatformTextWidget
    #pragma warning disable CS0067
    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? TextCommitted;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public MacOSGroup(IntPtr parentHandle, int style, string text)
    {
        _text = text ?? string.Empty;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSGroup] Creating group. Parent: 0x{parentHandle:X}, Style: 0x{style:X}, Text: {text}");

        // Create NSBox using objc_msgSend
        _nsBoxHandle = CreateNSBox(parentHandle, style, _text);

        if (_nsBoxHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSBox for Group");
        }

        // Get the content view for adding children
        IntPtr contentViewSelector = sel_registerName("contentView");
        _contentViewHandle = objc_msgSend(_nsBoxHandle, contentViewSelector);

        if (enableLogging)
            Console.WriteLine($"[MacOSGroup] Group created successfully. NSBox: 0x{_nsBoxHandle:X}, ContentView: 0x{_contentViewHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsBoxHandle;
    }

    /// <summary>
    /// Gets the content view handle for adding children.
    /// </summary>
    internal IntPtr GetContentViewHandle()
    {
        return _contentViewHandle;
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        lock (_platformChildren)
        {
            if (!_platformChildren.Contains(child))
            {
                _platformChildren.Add(child);
                ChildAdded?.Invoke(this, child);
            }
        }

        // Add child to the NSBox's content view
        if (child is MacOSWidget macOSChild)
        {
            IntPtr childHandle = macOSChild.GetNativeHandle();
            if (childHandle != IntPtr.Zero && _contentViewHandle != IntPtr.Zero)
            {
                // objc_msgSend(_contentViewHandle, addSubview:, childHandle)
                IntPtr selector = sel_registerName("addSubview:");
                objc_msgSend(_contentViewHandle, selector, childHandle);
            }
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        lock (_platformChildren)
        {
            if (_platformChildren.Remove(child))
            {
                ChildRemoved?.Invoke(this, child);
            }
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

    public void SetText(string text)
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return;

        _text = text ?? string.Empty;

        // Create NSString
        IntPtr nsString = CreateNSString(_text);

        // Call setTitle:
        IntPtr selector = sel_registerName("setTitle:");
        objc_msgSend(_nsBoxHandle, selector, nsString);

        // Release NSString
        IntPtr releaseSelector = sel_registerName("release");
        objc_msgSend(nsString, releaseSelector);

        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return string.Empty;

        // Call title method
        IntPtr selector = sel_registerName("title");
        IntPtr nsString = objc_msgSend(_nsBoxHandle, selector);

        if (nsString == IntPtr.Zero) return string.Empty;

        // Convert NSString to C# string
        IntPtr utf8Selector = sel_registerName("UTF8String");
        IntPtr utf8Ptr = objc_msgSend(nsString, utf8Selector);

        if (utf8Ptr == IntPtr.Zero) return string.Empty;

        return Marshal.PtrToStringAnsi(utf8Ptr) ?? string.Empty;
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return;

        // Create NSRect and call setFrame:
        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsBoxHandle, selector, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return default;

        // Call frame method to get NSRect
        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsBoxHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return;

        // NSView uses setHidden: (inverted from visible)
        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsBoxHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return false;

        // NSView uses isHidden (inverted from visible)
        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsBoxHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsBoxHandle == IntPtr.Zero) return;
        // NSBox doesn't have enabled state - this is a no-op for containers
    }

    public bool GetEnabled()
    {
        // NSBox doesn't have enabled state - containers are always "enabled"
        return true;
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via fillColor or contentViewMargins
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color for title
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
        if (_nsBoxHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsBoxHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsBoxHandle, releaseSelector);

            _nsBoxHandle = IntPtr.Zero;
            _contentViewHandle = IntPtr.Zero;
        }
    }

    private IntPtr CreateNSBox(IntPtr parentHandle, int style, string title)
    {
        // Get NSBox class
        IntPtr nsBoxClass = objc_getClass("NSBox");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr box = objc_msgSend(nsBoxClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        box = objc_msgSend(box, initSelector);

        // Set box type to NSBoxPrimary (standard bordered box)
        IntPtr setBoxTypeSelector = sel_registerName("setBoxType:");
        objc_msgSend_int(box, setBoxTypeSelector, 0); // NSBoxPrimary = 0

        // Set title position to NSAtTop
        IntPtr setTitlePositionSelector = sel_registerName("setTitlePosition:");
        objc_msgSend_int(box, setTitlePositionSelector, 2); // NSAtTop = 2

        // Set title
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsString = CreateNSString(title);
            IntPtr setTitleSelector = sel_registerName("setTitle:");
            objc_msgSend(box, setTitleSelector, nsString);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(nsString, releaseSelector);
        }

        // Set a default frame
        var frame = new CGRect(0, 0, 100, 100);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(box, setFrameSelector, frame);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, box);
        }

        return box;
    }

    private IntPtr CreateNSString(string str)
    {
        // Get NSString class
        IntPtr nsStringClass = objc_getClass("NSString");

        // Call stringWithUTF8String:
        IntPtr selector = sel_registerName("stringWithUTF8String:");

        // Marshal string to UTF8
        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(str);
        IntPtr nsString = objc_msgSend(nsStringClass, selector, utf8Ptr);
        Marshal.FreeHGlobal(utf8Ptr);

        return nsString;
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
    private static extern void objc_msgSend_int(IntPtr receiver, IntPtr selector, int arg);

    [DllImport(ObjCLibrary)]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    #endregion
}
