using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of a composite platform widget.
/// Encapsulates NSView and provides IPlatformComposite functionality.
/// </summary>
internal class MacOSComposite : MacOSWidget, IPlatformComposite
{
    private IntPtr _nsViewHandle;
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;

    // Event handling
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;

    public MacOSComposite(IntPtr parentHandle, int style)
    {
        // Create NSView using objc_msgSend
        _nsViewHandle = CreateNSView(parentHandle, style);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsViewHandle, setFrame:, NSMakeRect(x, y, width, height))
        var rectClass = objc_getClass("NSValue");
        var selector = sel_registerName("valueWithRect:");
        var rect = new NSRect { x = x, y = y, width = width, height = height };
        var rectValue = objc_msgSend(rectClass, selector, rect);

        var setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend(_nsViewHandle, setFrameSelector, rectValue);

        // Fire LayoutRequested event when bounds change
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public SWTSharp.Graphics.Rectangle GetBounds()
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return default(SWTSharp.Graphics.Rectangle);

        // objc_msgSend(_nsViewHandle, frame)
        var selector = sel_registerName("frame");
        var frameValue = objc_msgSend(_nsViewHandle, selector);

        // Extract NSRect from NSValue
        var rectSelector = sel_registerName("rectValue");
        var rect = Marshal.PtrToStructure<NSRect>(objc_msgSend(frameValue, rectSelector));

        return new SWTSharp.Graphics.Rectangle((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsViewHandle, setHidden:, !visible)
        var selector = sel_registerName("setHidden:");
        objc_msgSend(_nsViewHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return false;

        // objc_msgSend(_nsViewHandle, isHidden)
        var selector = sel_registerName("isHidden");
        var result = objc_msgSend(_nsViewHandle, selector);
        return result != IntPtr.Zero;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

        // NSView doesn't have enabled state, but we can store it for child widgets
        // TODO: Store enabled state for child management
    }

    public bool GetEnabled()
    {
        // NSView doesn't have enabled state
        return true;
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

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
        if (_disposed || _nsViewHandle == IntPtr.Zero) return;

        // TODO: Implement foreground color setting
        // This would require NSColor handling
    }

    public RGB GetForeground()
    {
        // TODO: Implement foreground color getting
        return new RGB(0, 0, 0); // Default black
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        if (child is MacOSWidget macOSChild)
        {
            var childHandle = macOSChild.GetNativeHandle();

            // objc_msgSend(_nsViewHandle, addSubview:, childHandle)
            var selector = sel_registerName("addSubview:");
            objc_msgSend(_nsViewHandle, selector, childHandle);

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

            if (_nsViewHandle != IntPtr.Zero)
            {
                // objc_msgSend(_nsViewHandle, release)
                var selector = sel_registerName("release");
                objc_msgSend(_nsViewHandle, selector);
                _nsViewHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsViewHandle;
    }

    private IntPtr CreateNSView(IntPtr parentHandle, int style)
    {
        // Implementation should create NSView with proper configuration
        var viewClass = objc_getClass("NSView");
        var allocSelector = sel_registerName("alloc");
        var initSelector = sel_registerName("init");

        var view = objc_msgSend(viewClass, allocSelector);
        var initializedView = objc_msgSend(view, initSelector);

        // Add to parent view if provided
        if (parentHandle != IntPtr.Zero)
        {
            var addSubViewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubViewSelector, initializedView);
        }

        return initializedView;
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
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg);
}