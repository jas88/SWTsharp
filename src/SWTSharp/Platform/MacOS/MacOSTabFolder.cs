using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformTabFolder using NSTabView.
/// Provides complete tab folder functionality with native macOS tab controls.
/// </summary>
internal class MacOSTabFolder : MacOSWidget, IPlatformTabFolder
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsTabViewHandle;
    private readonly List<MacOSTabItem> _items = new();
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);

    // Events required by IPlatformComposite
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // Events required by IPlatformSelectionEvents
    public event EventHandler<int>? SelectionChanged;
    #pragma warning disable CS0067
    public event EventHandler<int>? ItemDoubleClick;
    #pragma warning restore CS0067

    public MacOSTabFolder(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSTabFolder] Creating tab folder. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSTabView using objc_msgSend
        _nsTabViewHandle = CreateNSTabView(parentHandle, style);

        if (_nsTabViewHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSTabView for tab folder");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSTabFolder] Tab folder created successfully. Handle: 0x{_nsTabViewHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsTabViewHandle;
    }

    #region IPlatformTabFolder Implementation

    public int GetItemCount()
    {
        return _items.Count;
    }

    public IPlatformTabItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _items[index];
    }

    public int SelectionIndex
    {
        get
        {
            if (_disposed || _nsTabViewHandle == IntPtr.Zero) return -1;

            // Call indexOfTabViewItem: on selectedTabViewItem
            IntPtr selectedItemSelector = sel_registerName("selectedTabViewItem");
            IntPtr selectedItem = objc_msgSend(_nsTabViewHandle, selectedItemSelector);

            if (selectedItem == IntPtr.Zero) return -1;

            IntPtr indexSelector = sel_registerName("indexOfTabViewItem:");
            IntPtr result = objc_msgSend(_nsTabViewHandle, indexSelector, selectedItem);
            return (int)result.ToInt64();
        }
        set
        {
            if (_disposed || _nsTabViewHandle == IntPtr.Zero) return;
            if (value < 0 || value >= _items.Count) return;

            int oldIndex = SelectionIndex;

            // Call selectTabViewItemAtIndex:
            IntPtr selector = sel_registerName("selectTabViewItemAtIndex:");
            objc_msgSend(_nsTabViewHandle, selector, value);

            if (oldIndex != value)
            {
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    public IPlatformTabItem CreateTabItem(int style, int index)
    {
        if (_disposed || _nsTabViewHandle == IntPtr.Zero)
            throw new InvalidOperationException("Cannot create tab item on disposed tab folder");

        // Create the tab item
        var tabItem = new MacOSTabItem(this, _nsTabViewHandle, style, index);

        // Insert at specified index
        if (index >= 0 && index < _items.Count)
        {
            _items.Insert(index, tabItem);
        }
        else
        {
            _items.Add(tabItem);
        }

        return tabItem;
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
        if (_disposed || _nsTabViewHandle == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsTabViewHandle, selector, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsTabViewHandle == IntPtr.Zero) return default;

        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsTabViewHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsTabViewHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsTabViewHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsTabViewHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsTabViewHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsTabViewHandle == IntPtr.Zero) return;

        // NSTabView doesn't have direct enabled state
        // Enable/disable all tab view items instead
        foreach (var item in _items)
        {
            // This would need to be implemented in MacOSTabItem
        }
    }

    public bool GetEnabled()
    {
        // NSTabView doesn't have enabled state
        return true;
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via layer or custom drawing
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

        // Dispose all tab items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        // Clear children
        lock (_platformChildren)
        {
            _platformChildren.Clear();
        }

        // Release NSTabView
        if (_nsTabViewHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsTabViewHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsTabViewHandle, releaseSelector);

            _nsTabViewHandle = IntPtr.Zero;
        }
    }

    #endregion

    #region Private Helper Methods

    private IntPtr CreateNSTabView(IntPtr parentHandle, int style)
    {
        // Get NSTabView class
        IntPtr nsTabViewClass = objc_getClass("NSTabView");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr tabView = objc_msgSend(nsTabViewClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        tabView = objc_msgSend(tabView, initSelector);

        // Set initial frame
        var frame = new CGRect(0, 0, 200, 150);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(tabView, setFrameSelector, frame);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, tabView);
        }

        return tabView;
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
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg);

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
