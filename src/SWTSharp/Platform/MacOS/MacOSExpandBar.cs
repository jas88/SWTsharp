using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformExpandBar using custom NSView with disclosure triangles.
/// Provides native macOS accordion-style expand/collapse functionality.
/// </summary>
internal class MacOSExpandBar : MacOSWidget, IPlatformExpandBar
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsScrollView;
    private IntPtr _nsContentView;
    private readonly List<MacOSExpandItem> _items = new();
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private int _spacing = 4;

    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    public event EventHandler<int>? ItemExpanded;
    public event EventHandler<int>? ItemCollapsed;
    #pragma warning restore CS0067

    public MacOSExpandBar(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSExpandBar] Creating expand bar. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSScrollView for scrolling support
        IntPtr nsScrollViewClass = objc_getClass("NSScrollView");
        _nsScrollView = objc_msgSend(nsScrollViewClass, sel_registerName("alloc"));
        _nsScrollView = objc_msgSend(_nsScrollView, sel_registerName("init"));

        // Create content view (NSView)
        IntPtr nsViewClass = objc_getClass("NSView");
        _nsContentView = objc_msgSend(nsViewClass, sel_registerName("alloc"));
        _nsContentView = objc_msgSend(_nsContentView, sel_registerName("init"));

        // Set content view as document view
        IntPtr setDocumentViewSelector = sel_registerName("setDocumentView:");
        objc_msgSend(_nsScrollView, setDocumentViewSelector, _nsContentView);

        // Configure scroll view
        if ((style & SWT.V_SCROLL) != 0)
        {
            IntPtr setHasVerticalScrollerSelector = sel_registerName("setHasVerticalScroller:");
            objc_msgSend_void(_nsScrollView, setHasVerticalScrollerSelector, true);
        }

        // Set initial frame
        var frame = new CGRect(0, 0, 200, 300);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsScrollView, setFrameSelector, frame);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, _nsScrollView);
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSExpandBar] ExpandBar created successfully. Handle: 0x{_nsScrollView:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsScrollView;
    }

    #region IPlatformExpandBar Implementation

    public int GetItemCount()
    {
        return _items.Count;
    }

    public IPlatformExpandItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _items[index];
    }

    public void SetSpacing(int spacing)
    {
        _spacing = Math.Max(0, spacing);
        LayoutItems();
    }

    public int GetSpacing()
    {
        return _spacing;
    }

    public IPlatformExpandItem CreateExpandItem(int style, int index)
    {
        if (_disposed || _nsContentView == IntPtr.Zero)
            throw new InvalidOperationException("Cannot create expand item on disposed expand bar");

        var item = new MacOSExpandItem(this, _nsContentView, style, index);

        if (index >= 0 && index < _items.Count)
        {
            _items.Insert(index, item);
        }
        else
        {
            _items.Add(item);
        }

        LayoutItems();
        return item;
    }

    private void LayoutItems()
    {
        if (_disposed || _nsContentView == IntPtr.Zero) return;

        double y = 0;
        CGRect contentFrame = GetContentFrame();
        double width = contentFrame.width;

        foreach (var item in _items)
        {
            double itemHeight = item.GetTotalHeight();
            item.SetBounds(0, (int)y, (int)width, (int)itemHeight);
            y += itemHeight + _spacing;
        }

        // Update content view size
        var newFrame = new CGRect(0, 0, width, y);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsContentView, setFrameSelector, newFrame);
    }

    private CGRect GetContentFrame()
    {
        if (_nsScrollView == IntPtr.Zero) return new CGRect(0, 0, 200, 300);

        IntPtr frameSelector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsScrollView, frameSelector);
        return frame;
    }

    internal void OnItemExpandedChanged(MacOSExpandItem item, bool expanded)
    {
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            if (expanded)
            {
                ItemExpanded?.Invoke(this, index);
            }
            else
            {
                ItemCollapsed?.Invoke(this, index);
            }

            LayoutItems();
        }
    }

    #endregion

    #region IPlatformComposite Implementation

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
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        lock (_platformChildren)
        {
            return _platformChildren.ToArray();
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsScrollView == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsScrollView, selector, frame);

        LayoutItems();
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsScrollView == IntPtr.Zero) return default;

        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsScrollView, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsScrollView == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsScrollView, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsScrollView == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsScrollView, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        // NSScrollView doesn't have direct enabled state
        foreach (var item in _items)
        {
            // Items will handle their own enabled state
        }
    }

    public bool GetEnabled()
    {
        return true;
    }

    public void SetBackground(RGB color)
    {
        _background = color;
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

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        lock (_platformChildren)
        {
            _platformChildren.Clear();
        }

        if (_nsContentView != IntPtr.Zero)
        {
            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsContentView, releaseSelector);
            _nsContentView = IntPtr.Zero;
        }

        if (_nsScrollView != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsScrollView, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsScrollView, releaseSelector);
            _nsScrollView = IntPtr.Zero;
        }
    }

    #endregion

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

/// <summary>
/// macOS implementation of IPlatformExpandItem using custom NSView with disclosure triangle.
/// </summary>
internal class MacOSExpandItem : MacOSWidget, IPlatformExpandItem
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const double HEADER_HEIGHT = 24.0;

    private readonly MacOSExpandBar _expandBar;
    private readonly IntPtr _containerView;
    private readonly IntPtr _headerView;
    private readonly IntPtr _contentView;
    private string _text = string.Empty;
    private bool _expanded;
    private int _height = 100;
    private IPlatformWidget? _control;
    private bool _disposed;

    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public MacOSExpandItem(MacOSExpandBar expandBar, IntPtr parentView, int style, int index)
    {
        _expandBar = expandBar;

        // Create container view
        IntPtr nsViewClass = objc_getClass("NSView");
        _containerView = objc_msgSend(nsViewClass, sel_registerName("alloc"));
        _containerView = objc_msgSend(_containerView, sel_registerName("init"));

        // Create header view (button with disclosure triangle)
        IntPtr nsButtonClass = objc_getClass("NSButton");
        _headerView = objc_msgSend(nsButtonClass, sel_registerName("alloc"));
        _headerView = objc_msgSend(_headerView, sel_registerName("init"));

        // Create content view
        _contentView = objc_msgSend(nsViewClass, sel_registerName("alloc"));
        _contentView = objc_msgSend(_contentView, sel_registerName("init"));

        // Add header and content to container
        IntPtr addSubviewSelector = sel_registerName("addSubview:");
        objc_msgSend(parentView, addSubviewSelector, _containerView);
        objc_msgSend(_containerView, addSubviewSelector, _headerView);
        objc_msgSend(_containerView, addSubviewSelector, _contentView);

        // Initially hide content
        IntPtr setHiddenSelector = sel_registerName("setHidden:");
        objc_msgSend_void(_contentView, setHiddenSelector, true);
    }

    public override IntPtr GetNativeHandle()
    {
        return _containerView;
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _containerView == IntPtr.Zero) return;

        // Set container bounds
        var containerFrame = new CGRect(x, y, width, height);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(_containerView, setFrameSelector, containerFrame);

        // Layout header
        var headerFrame = new CGRect(0, 0, width, HEADER_HEIGHT);
        objc_msgSend_rect(_headerView, setFrameSelector, headerFrame);

        // Layout content if expanded
        if (_expanded)
        {
            var contentFrame = new CGRect(0, HEADER_HEIGHT, width, _height);
            objc_msgSend_rect(_contentView, setFrameSelector, contentFrame);
        }
    }

    public double GetTotalHeight()
    {
        return HEADER_HEIGHT + (_expanded ? _height : 0);
    }

    public void SetText(string text)
    {
        _text = text ?? string.Empty;
        if (_headerView != IntPtr.Zero)
        {
            string displayText = (_expanded ? "▼ " : "► ") + _text;
            IntPtr nsString = CreateNSString(displayText);
            IntPtr setTitleSelector = sel_registerName("setTitle:");
            objc_msgSend(_headerView, setTitleSelector, nsString);
            objc_msgSend(nsString, sel_registerName("release"));
        }
    }

    public string GetText()
    {
        return _text;
    }

    public void SetExpanded(bool expanded)
    {
        if (_expanded != expanded)
        {
            _expanded = expanded;

            if (_contentView != IntPtr.Zero)
            {
                IntPtr setHiddenSelector = sel_registerName("setHidden:");
                objc_msgSend_void(_contentView, setHiddenSelector, !_expanded);
            }

            SetText(_text); // Update disclosure triangle
            _expandBar.OnItemExpandedChanged(this, _expanded);
        }
    }

    public bool GetExpanded()
    {
        return _expanded;
    }

    public void SetHeight(int height)
    {
        _height = Math.Max(0, height);
        if (_expanded && _contentView != IntPtr.Zero)
        {
            CGRect frame;
            IntPtr frameSelector = sel_registerName("frame");
            objc_msgSend_stret(out frame, _contentView, frameSelector);

            frame.height = _height;
            IntPtr setFrameSelector = sel_registerName("setFrame:");
            objc_msgSend_rect(_contentView, setFrameSelector, frame);
        }
    }

    public int GetHeight()
    {
        return _height;
    }

    public void SetControl(IPlatformWidget? control)
    {
        _control = control;

        if (_control != null && _contentView != IntPtr.Zero)
        {
            if (_control is MacOSWidget macWidget)
            {
                IntPtr controlHandle = macWidget.GetNativeHandle();
                if (controlHandle != IntPtr.Zero)
                {
                    IntPtr addSubviewSelector = sel_registerName("addSubview:");
                    objc_msgSend(_contentView, addSubviewSelector, controlHandle);

                    var frame = new CGRect(0, 0, 200, _height);
                    IntPtr setFrameSelector = sel_registerName("setFrame:");
                    objc_msgSend_rect(controlHandle, setFrameSelector, frame);
                }
            }
        }
    }

    private IntPtr CreateNSString(string text)
    {
        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr initWithUTF8StringSelector = sel_registerName("initWithUTF8String:");

        IntPtr nsString = objc_msgSend(nsStringClass, allocSelector);

        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(text);
        nsString = objc_msgSend(nsString, initWithUTF8StringSelector, utf8Ptr);
        Marshal.FreeHGlobal(utf8Ptr);

        return nsString;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        IntPtr releaseSelector = sel_registerName("release");

        if (_contentView != IntPtr.Zero)
        {
            objc_msgSend(_contentView, releaseSelector);
        }

        if (_headerView != IntPtr.Zero)
        {
            objc_msgSend(_headerView, releaseSelector);
        }

        if (_containerView != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_containerView, removeSelector);
            objc_msgSend(_containerView, releaseSelector);
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
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    #endregion
}
