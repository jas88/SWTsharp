using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Sash widget methods.
/// Uses NSSplitView for resizable divider functionality.
/// </summary>
internal partial class MacOSPlatform
{
    private class MacOSSash : IPlatformSash
    {
        private readonly MacOSPlatform _platform;
        private readonly IntPtr _handle;
        private int _position;
        private bool _disposed;

        public event EventHandler<int>? PositionChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public MacOSSash(MacOSPlatform platform, IntPtr handle)
        {
            _platform = platform;
            _handle = handle;
        }

        public void SetPosition(int position)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            _position = position;
            // NSSplitView doesn't have direct position setting - would need to adjust subview frames
        }

        public int GetPosition()
        {
            return _position;
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            CGRect frame = new CGRect(x, y, width, height);
            objc_msgSend_rect(_handle, sel_registerName("setFrame:"), frame);
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _handle == IntPtr.Zero) return default;
            CGRect frame;
            objc_msgSend_stret(out frame, _handle, sel_registerName("frame"));
            return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            objc_msgSend_void(_handle, sel_registerName("setHidden:"), !visible);
        }

        public bool GetVisible()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return !objc_msgSend_bool(_handle, sel_registerName("isHidden"), IntPtr.Zero);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            objc_msgSend_void(_handle, sel_registerName("setEnabled:"), enabled);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return objc_msgSend_bool(_handle, sel_registerName("isEnabled"), IntPtr.Zero);
        }

        public void SetBackground(RGB color)
        {
            // NSSplitView background typically not customized
        }

        public RGB GetBackground()
        {
            return new RGB(200, 200, 200); // Default gray
        }

        public void SetForeground(RGB color)
        {
            // Not applicable for sash
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 0);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle != IntPtr.Zero)
                {
                    objc_msgSend(_handle, sel_registerName("release"));
                }
                _disposed = true;
            }
        }

        internal void OnPositionChanged(int newPosition)
        {
            if (_position != newPosition)
            {
                _position = newPosition;
                PositionChanged?.Invoke(this, _position);
            }
        }
    }

    public IPlatformSash CreateSashWidget(IPlatformWidget? parent, int style)
    {
        bool isVertical = (style & SWT.VERTICAL) != 0;

        IntPtr splitView = objc_msgSend(objc_msgSend(objc_getClass("NSSplitView"), sel_registerName("alloc")), sel_registerName("init"));

        // Set orientation
        objc_msgSend_void(splitView, sel_registerName("setVertical:"), !isVertical);

        // Add two empty views to the split view
        IntPtr view1 = objc_msgSend(objc_msgSend(objc_getClass("NSView"), sel_registerName("alloc")), sel_registerName("init"));
        IntPtr view2 = objc_msgSend(objc_msgSend(objc_getClass("NSView"), sel_registerName("alloc")), sel_registerName("init"));
        objc_msgSend(splitView, sel_registerName("addSubview:"), view1);
        objc_msgSend(splitView, sel_registerName("addSubview:"), view2);

        var sashWidget = new MacOSSash(this, splitView);
        _sashWidgets[splitView] = sashWidget;

        if (parent != null)
        {
            IntPtr parentHandle = MacOSPlatformHelpers.GetParentHandle(parent);
            if (parentHandle != IntPtr.Zero)
            {
                objc_msgSend(parentHandle, sel_registerName("addSubview:"), splitView);
            }
        }

        return sashWidget;
    }

    private Dictionary<IntPtr, MacOSSash> _sashWidgets = new Dictionary<IntPtr, MacOSSash>();
}
