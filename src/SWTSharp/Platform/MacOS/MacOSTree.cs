using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of Tree widget using NSOutlineView with NSTreeController.
/// NSOutlineView displays hierarchical tree data with support for single/multi selection.
/// </summary>
internal class MacOSTree : IPlatformComposite
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _outlineView;
    private IntPtr _scrollView;
    private readonly int _style;
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private readonly bool _multiSelect;
    private readonly bool _check;

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // Cached selectors
    private static IntPtr _selAlloc;
    private static IntPtr _selInit;
    private static IntPtr _selSetFrame;
    private static IntPtr _selAddSubview;
    private static IntPtr _selSetHidden;
    private static IntPtr _selIsHidden;
    private static IntPtr _selSetEnabled;
    private static IntPtr _selIsEnabled;
    private static IntPtr _selAddTableColumn;
    private static IntPtr _selSetDocumentView;
    private static IntPtr _selSetHasVerticalScroller;
    private static IntPtr _selSetHasHorizontalScroller;

    // Cached classes
    private static IntPtr _nsOutlineViewClass;
    private static IntPtr _nsScrollViewClass;
    private static IntPtr _nsTableColumnClass;

    static MacOSTree()
    {
        // Initialize selectors
        _selAlloc = sel_registerName("alloc");
        _selInit = sel_registerName("init");
        _selSetFrame = sel_registerName("setFrame:");
        _selAddSubview = sel_registerName("addSubview:");
        _selSetHidden = sel_registerName("setHidden:");
        _selIsHidden = sel_registerName("isHidden");
        _selSetEnabled = sel_registerName("setEnabled:");
        _selIsEnabled = sel_registerName("isEnabled");
        _selAddTableColumn = sel_registerName("addTableColumn:");
        _selSetDocumentView = sel_registerName("setDocumentView:");
        _selSetHasVerticalScroller = sel_registerName("setHasVerticalScroller:");
        _selSetHasHorizontalScroller = sel_registerName("setHasHorizontalScroller:");

        // Initialize classes
        _nsOutlineViewClass = objc_getClass("NSOutlineView");
        _nsScrollViewClass = objc_getClass("NSScrollView");
        _nsTableColumnClass = objc_getClass("NSTableColumn");
    }

    public MacOSTree(IntPtr parentHandle, int style)
    {
        _style = style;
        _multiSelect = (style & SWT.MULTI) != 0;
        _check = (style & SWT.CHECK) != 0;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSTree] Creating tree. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSOutlineView
        _outlineView = objc_msgSend(_nsOutlineViewClass, _selAlloc);
        _outlineView = objc_msgSend(_outlineView, _selInit);

        // Set initial frame
        var frame = new CGRect(0, 0, 100, 100);
        objc_msgSend_rect(_outlineView, _selSetFrame, frame);

        // Create table column for text
        IntPtr textColumn = objc_msgSend(_nsTableColumnClass, _selAlloc);
        textColumn = objc_msgSend(textColumn, _selInit);
        objc_msgSend(_outlineView, _selAddTableColumn, textColumn);

        // Create scroll view
        _scrollView = objc_msgSend(_nsScrollViewClass, _selAlloc);
        _scrollView = objc_msgSend(_scrollView, _selInit);
        objc_msgSend_rect(_scrollView, _selSetFrame, frame);

        // Configure scroll view
        objc_msgSend(_scrollView, _selSetDocumentView, _outlineView);
        objc_msgSend_void(_scrollView, _selSetHasVerticalScroller, true);
        objc_msgSend_void(_scrollView, _selSetHasHorizontalScroller, true);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            AddToParent(parentHandle, _scrollView);
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSTree] Tree created successfully. OutlineView: 0x{_outlineView:X}");
    }

    private void AddToParent(IntPtr parent, IntPtr child)
    {
        // Get class of parent object
        IntPtr selClass = sel_registerName("class");
        IntPtr parentClass = objc_msgSend(parent, selClass);

        // Check if it's NSWindow
        IntPtr nsWindowClass = objc_getClass("NSWindow");
        IntPtr targetView = parent;

        if (parentClass == nsWindowClass)
        {
            // Parent is NSWindow, get contentView
            IntPtr selContentView = sel_registerName("contentView");
            targetView = objc_msgSend(parent, selContentView);
        }

        // Add child to target view
        objc_msgSend(targetView, _selAddSubview, child);
    }

    /// <summary>
    /// Gets the native NSOutlineView handle.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _scrollView; // Return scroll view as main handle
    }

    public void AddChild(IPlatformWidget child)
    {
        // Tree items are data, not child widgets - no-op
    }

    public void RemoveChild(IPlatformWidget child)
    {
        // Tree items are data, not child widgets - no-op
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return Array.Empty<IPlatformWidget>();
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_scrollView == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        objc_msgSend_rect(_scrollView, _selSetFrame, frame);
    }

    public Rectangle GetBounds()
    {
        if (_scrollView == IntPtr.Zero) return default;

        CGRect frame;
        IntPtr selFrame = sel_registerName("frame");
        objc_msgSend_stret(out frame, _scrollView, selFrame);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_scrollView == IntPtr.Zero) return;
        objc_msgSend_void(_scrollView, _selSetHidden, !visible);
    }

    public bool GetVisible()
    {
        if (_scrollView == IntPtr.Zero) return false;
        return !objc_msgSend_bool(_scrollView, _selIsHidden);
    }

    public void SetEnabled(bool enabled)
    {
        if (_outlineView == IntPtr.Zero) return;
        objc_msgSend_void(_outlineView, _selSetEnabled, enabled);
    }

    public bool GetEnabled()
    {
        if (_outlineView == IntPtr.Zero) return false;
        return objc_msgSend_bool(_outlineView, _selIsEnabled);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via NSColor
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color via NSColor
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_outlineView != IntPtr.Zero)
        {
            // NSOutlineView will be released when scroll view is released
            _outlineView = IntPtr.Zero;
        }

        if (_scrollView != IntPtr.Zero)
        {
            IntPtr selRemoveFromSuperview = sel_registerName("removeFromSuperview");
            objc_msgSend(_scrollView, selRemoveFromSuperview);
            _scrollView = IntPtr.Zero;
        }
    }

    #region Objective-C Runtime

    [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

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

    #endregion
}
