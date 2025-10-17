using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - ScrollBar widget methods.
/// Uses NSScroller for standalone scrollbar control.
/// </summary>
internal partial class MacOSPlatform
{
    private class MacOSScrollBar : IPlatformScrollBar
    {
        private readonly MacOSPlatform _platform;
        private readonly IntPtr _handle;
        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private int _increment = 1;
        private int _pageIncrement = 10;
        private int _thumb = 10;
        private bool _disposed;

        public event EventHandler<int>? ValueChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public MacOSScrollBar(MacOSPlatform platform, IntPtr handle)
        {
            _platform = platform;
            _handle = handle;
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(_minimum, Math.Min(_maximum, value));
                UpdateScroller();
            }
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                UpdateScroller();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                UpdateScroller();
            }
        }

        public int Increment
        {
            get => _increment;
            set => _increment = value;
        }

        public int PageIncrement
        {
            get => _pageIncrement;
            set => _pageIncrement = value;
        }

        public int Thumb
        {
            get => _thumb;
            set
            {
                _thumb = value;
                UpdateScroller();
            }
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
            // NSScroller background typically not customized
        }

        public RGB GetBackground()
        {
            return new RGB(200, 200, 200);
        }

        public void SetForeground(RGB color)
        {
            // Not applicable for scrollbar
        }

        public RGB GetForeground()
        {
            return new RGB(100, 100, 100);
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

        public void UpdateScroller()
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            double range = _maximum - _minimum;
            double proportion = range > 0 ? _thumb / range : 0;
            double position = range > 0 ? (_value - _minimum) / range : 0;

            objc_msgSend_double(_handle, sel_registerName("setKnobProportion:"), proportion);
            objc_msgSend_double(_handle, sel_registerName("setDoubleValue:"), position);
        }

        internal void OnValueChanged()
        {
            double position = objc_msgSend_double(_handle, sel_registerName("doubleValue"));
            int newValue = _minimum + (int)(((_maximum - _minimum) * position));
            if (_value != newValue)
            {
                _value = newValue;
                ValueChanged?.Invoke(this, _value);
            }
        }
    }

    public IPlatformScrollBar CreateScrollBarWidget(IPlatformWidget? parent, int style)
    {
        bool isVertical = (style & SWT.VERTICAL) != 0;

        IntPtr scroller = objc_msgSend(objc_msgSend(objc_getClass("NSScroller"), sel_registerName("alloc")), sel_registerName("init"));

        // Set scroller style
        IntPtr scrollerStyle = isVertical ? new IntPtr(0) : new IntPtr(1); // NSScrollerStyleOverlay
        objc_msgSend(scroller, sel_registerName("setScrollerStyle:"), scrollerStyle);

        var scrollBarWidget = new MacOSScrollBar(this, scroller);
        _scrollBarWidgets[scroller] = scrollBarWidget;
        scrollBarWidget.UpdateScroller();

        if (parent != null)
        {
            IntPtr parentHandle = MacOSPlatformHelpers.GetParentHandle(parent);
            if (parentHandle != IntPtr.Zero)
            {
                objc_msgSend(parentHandle, sel_registerName("addSubview:"), scroller);
            }
        }

        // Set up action target
        objc_msgSend(scroller, sel_registerName("setTarget:"), scroller);
        objc_msgSend(scroller, sel_registerName("setAction:"), sel_registerName("scrollerAction:"));

        return scrollBarWidget;
    }

    private Dictionary<IntPtr, MacOSScrollBar> _scrollBarWidgets = new Dictionary<IntPtr, MacOSScrollBar>();
}
