using System.Runtime.InteropServices;
using System.Drawing;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of a window platform widget.
/// Encapsulates NSWindow and provides IPlatformWindow functionality.
/// </summary>
internal class MacOSWindow : MacOSWidget, IPlatformWindow
{
    private IntPtr _nsWindowHandle;
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;

    // Event handling
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    public MacOSWindow(int style, string title)
    {
        // Create NSWindow using objc_msgSend
        _nsWindowHandle = CreateNSWindow(style, title);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, setFrame:, NSMakeRect(x, y, width, height), display:YES)
        var rectClass = objc_getClass("NSValue");
        var selector = sel_registerName("valueWithRect:");
        var rect = new NSRect { x = x, y = y, width = width, height = height };
        var rectValue = objc_msgSend(rectClass, selector, rect);

        var setFrameSelector = sel_registerName("setFrame:display:");
        // setFrame:display: takes NSRect* and BOOL parameters
        objc_msgSend(_nsWindowHandle, setFrameSelector, rectValue, true, 0); // backing parameter defaults to 0

        // Fire LayoutRequested event when bounds change
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public SWTSharp.Graphics.Rectangle GetBounds()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return default(SWTSharp.Graphics.Rectangle);

        // objc_msgSend(_nsWindowHandle, frame)
        var selector = sel_registerName("frame");
        var frameValue = objc_msgSend(_nsWindowHandle, selector);

        // Extract NSRect from NSValue
        var rectSelector = sel_registerName("rectValue");
        var rect = Marshal.PtrToStructure<NSRect>(objc_msgSend(frameValue, rectSelector));

        return new SWTSharp.Graphics.Rectangle((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        if (visible)
        {
            // objc_msgSend(_nsWindowHandle, makeKeyAndOrderFront:)
            var selector = sel_registerName("makeKeyAndOrderFront:");
            objc_msgSend(_nsWindowHandle, selector, IntPtr.Zero);
        }
        else
        {
            // objc_msgSend(_nsWindowHandle, orderOut:)
            var selector = sel_registerName("orderOut:");
            objc_msgSend(_nsWindowHandle, selector, IntPtr.Zero);
        }
    }

    public bool GetVisible()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return false;

        // objc_msgSend(_nsWindowHandle, isVisible)
        var selector = sel_registerName("isVisible");
        var result = objc_msgSend(_nsWindowHandle, selector);
        return result != IntPtr.Zero;
    }

    public void SetEnabled(bool enabled)
    {
        // NSWindow doesn't have enabled state like individual widgets
        // We can store this for future use if needed
    }

    public bool GetEnabled()
    {
        // NSWindow doesn't have enabled state
        return true;
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // TODO: Implement background color setting
        // This would require NSColor handling
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color getting
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // NSWindow doesn't have foreground color in the traditional sense
        // This would affect content view colors
    }

    public RGB GetForeground()
    {
        // NSWindow doesn't have foreground color
        return new RGB(0, 0, 0); // Default black
    }

    public void SetTitle(string title)
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, setTitle:, NSString stringWithString:title)
        var strClass = objc_getClass("NSString");
        var selector = sel_registerName("stringWithString:");
        var textPtr = Marshal.StringToHGlobalAuto(title);
        try
        {
            var nsString = objc_msgSend(strClass, selector, textPtr);
            var setTitleSelector = sel_registerName("setTitle:");
            objc_msgSend(_nsWindowHandle, setTitleSelector, nsString);
        }
        finally
        {
            Marshal.FreeHGlobal(textPtr);
        }
    }

    public string GetTitle()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return "";

        // objc_msgSend(_nsWindowHandle, title)
        var selector = sel_registerName("title");
        var nsString = objc_msgSend(_nsWindowHandle, selector);
        return NSStringToString(nsString);
    }

    public void Maximize()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, zoom:)
        var selector = sel_registerName("zoom:");
        objc_msgSend(_nsWindowHandle, selector, IntPtr.Zero);
    }

    public void Minimize()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, miniaturize:)
        var selector = sel_registerName("miniaturize:");
        objc_msgSend(_nsWindowHandle, selector, IntPtr.Zero);
    }

    public void Restore()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, deminiaturize:)
        var selector = sel_registerName("deminiaturize:");
        objc_msgSend(_nsWindowHandle, selector, IntPtr.Zero);
    }

    public void Close()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, close)
        var selector = sel_registerName("close");
        objc_msgSend(_nsWindowHandle, selector);
    }

    public bool IsDisposed => _disposed;

    public void Open()
    {
        if (_disposed || _nsWindowHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsWindowHandle, makeKeyAndOrderFront:)
        var selector = sel_registerName("makeKeyAndOrderFront:");
        objc_msgSend(_nsWindowHandle, selector, IntPtr.Zero);
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        if (child is MacOSWidget macOSChild)
        {
            var childHandle = macOSChild.GetNativeHandle();

            // Add child to window's content view
            var contentViewSelector = sel_registerName("contentView");
            var contentView = objc_msgSend(_nsWindowHandle, contentViewSelector);

            if (contentView != IntPtr.Zero)
            {
                var addSubViewSelector = sel_registerName("addSubview:");
                objc_msgSend(contentView, addSubViewSelector, childHandle);
            }

            _platformChildren.Add(child);

            // Fire ChildAdded event
            ChildAdded?.Invoke(this, child);
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        if (child is MacOSWidget macOSChild)
        {
            var childHandle = macOSChild.GetNativeHandle();

            // objc_msgSend(childHandle, removeFromSuperview)
            var selector = sel_registerName("removeFromSuperview");
            objc_msgSend(childHandle, selector);

            _platformChildren.Remove(child);

            // Fire ChildRemoved event
            ChildRemoved?.Invoke(this, child);
        }
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return _platformChildren.AsReadOnly();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Remove all children first
            foreach (var child in _platformChildren.ToArray())
            {
                RemoveChild(child);
            }

            if (_nsWindowHandle != IntPtr.Zero)
            {
                // objc_msgSend(_nsWindowHandle, release)
                var selector = sel_registerName("release");
                objc_msgSend(_nsWindowHandle, selector);
                _nsWindowHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsWindowHandle;
    }

    private IntPtr CreateNSWindow(int style, string title)
    {
        // Implementation should create NSWindow with proper configuration
        var windowClass = objc_getClass("NSWindow");
        var allocSelector = sel_registerName("alloc");

        var window = objc_msgSend(windowClass, allocSelector);

        // NSMakeRect(x, y, width, height) - default size
        var rect = new NSRect { x = 100, y = 100, width = 400, height = 300 };

        // Initialize NSWindow with content rect
        var initSelector = sel_registerName("initWithContentRect:styleMask:backing:defer:");
        var styleMask = GetStyleMask(style);
        var initializedWindow = objc_msgSend(window, initSelector, rect, styleMask, 2, false);

        // Set title
        if (!string.IsNullOrEmpty(title))
        {
            var strClass = objc_getClass("NSString");
            var stringSelector = sel_registerName("stringWithString:");
            var textPtr = Marshal.StringToHGlobalAuto(title);
            try
            {
                var nsString = objc_msgSend(strClass, stringSelector, textPtr);
                var setTitleSelector = sel_registerName("setTitle:");
                objc_msgSend(initializedWindow, setTitleSelector, nsString);
            }
            finally
            {
                Marshal.FreeHGlobal(textPtr);
            }
        }

        return initializedWindow;
    }

    private static int GetStyleMask(int style)
    {
        int mask = 0;

        // Basic window styles
        if ((style & SWT.TITLE) != 0 || (style & SWT.CLOSE) != 0 || (style & SWT.MIN) != 0 || (style & SWT.MAX) != 0)
            mask |= 1; // NSTitledWindowMask

        if ((style & SWT.CLOSE) != 0)
            mask |= 2; // NSClosableWindowMask

        if ((style & SWT.MIN) != 0)
            mask |= 4; // NSMiniaturizableWindowMask

        if ((style & SWT.MAX) != 0)
            mask |= 8; // NSResizableWindowMask

        if ((style & SWT.BORDER) != 0)
            mask |= 1; // NSTitledWindowMask (border implies title)

        if ((style & SWT.RESIZE) != 0)
            mask |= 8; // NSResizableWindowMask

        return mask;
    }

    // Native method declarations
    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg, int mask, int backing, bool defer);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg, bool display, int backing);

    private static string NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero) return "";

        var selector = sel_registerName("UTF8String");
        var utf8Ptr = objc_msgSend(nsString, selector);
        return Marshal.PtrToStringAuto(utf8Ptr) ?? "";
    }

    // Event handling methods
    private void OnChildAdded(IPlatformWidget child)
    {
        if (_disposed) return;
        ChildAdded?.Invoke(this, child);
    }

    private void OnChildRemoved(IPlatformWidget child)
    {
        if (_disposed) return;
        ChildRemoved?.Invoke(this, child);
    }

    private void OnLayoutRequested()
    {
        if (_disposed) return;
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnClick()
    {
        if (_disposed) return;
        Click?.Invoke(this, 0);
    }

    private void OnFocusGained()
    {
        if (_disposed) return;
        FocusGained?.Invoke(this, 0);
    }

    private void OnFocusLost()
    {
        if (_disposed) return;
        FocusLost?.Invoke(this, 0);
    }

    private void OnKeyDown(PlatformKeyEventArgs args)
    {
        if (_disposed) return;
        KeyDown?.Invoke(this, args);
    }

    private void OnKeyUp(PlatformKeyEventArgs args)
    {
        if (_disposed) return;
        KeyUp?.Invoke(this, args);
    }
}