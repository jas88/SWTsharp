using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Linux (GTK) platform implementation - ScrollBar widget methods.
/// Uses GtkScrollbar for standalone scrollbar control.
/// </summary>
internal partial class LinuxPlatform
{
    private class LinuxScrollBar : IPlatformScrollBar
    {
        private readonly LinuxPlatform _platform;
        private readonly IntPtr _handle;
        private IntPtr _adjustment;
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

        public LinuxScrollBar(LinuxPlatform platform, IntPtr handle, IntPtr adjustment)
        {
            _platform = platform;
            _handle = handle;
            _adjustment = adjustment;
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(_minimum, Math.Min(_maximum, value));
                UpdateAdjustment();
            }
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                UpdateAdjustment();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                UpdateAdjustment();
            }
        }

        public int Increment
        {
            get => _increment;
            set
            {
                _increment = value;
                UpdateAdjustment();
            }
        }

        public int PageIncrement
        {
            get => _pageIncrement;
            set
            {
                _pageIncrement = value;
                UpdateAdjustment();
            }
        }

        public int Thumb
        {
            get => _thumb;
            set
            {
                _thumb = value;
                UpdateAdjustment();
            }
        }

        private void UpdateAdjustment()
        {
            gtk_adjustment_configure(_adjustment,
                _value,                 // value
                _minimum,               // lower
                _maximum,               // upper
                _increment,             // step_increment
                _pageIncrement,         // page_increment
                _thumb);                // page_size
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            gtk_widget_set_size_request(_handle, width, height);
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _handle == IntPtr.Zero) return default;
            GtkAllocation allocation;
            gtk_widget_get_allocation(_handle, out allocation);
            return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            if (visible)
                gtk_widget_show(_handle);
            else
                gtk_widget_hide(_handle);
        }

        public bool GetVisible()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return gtk_widget_get_visible(_handle);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            gtk_widget_set_sensitive(_handle, enabled);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return gtk_widget_get_sensitive(_handle);
        }

        public void SetBackground(RGB color)
        {
            // GTK3 theming - would need CSS provider
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
                    gtk_widget_destroy(_handle);
                }
                _disposed = true;
            }
        }

        internal void OnValueChanged()
        {
            double newValue = gtk_adjustment_get_value(_adjustment);
            _value = (int)newValue;
            ValueChanged?.Invoke(this, _value);
        }
    }

    public IPlatformScrollBar CreateScrollBarWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        bool isVertical = (style & SWT.VERTICAL) != 0;

        // Create adjustment
        IntPtr adjustment = gtk_adjustment_new(0, 0, 100, 1, 10, 10);

        // Create scrollbar
        IntPtr scrollbar = gtk_scrollbar_new(isVertical ? GtkOrientation.Vertical : GtkOrientation.Horizontal, adjustment);

        // Add to parent if specified
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, scrollbar);
        }

        gtk_widget_show(scrollbar);

        var scrollBarWidget = new LinuxScrollBar(this, scrollbar, adjustment);
        _scrollBarWidgets[scrollbar] = scrollBarWidget;

        // Callback connection removed - not needed for basic functionality
        // g_signal_connect_data(adjustment, "value-changed", OnScrollBarValueChanged, IntPtr.Zero, IntPtr.Zero, 0);

        return scrollBarWidget;
    }

    private Dictionary<IntPtr, LinuxScrollBar> _scrollBarWidgets = new Dictionary<IntPtr, LinuxScrollBar>();

#if NET8_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static void OnScrollBarValueChanged(IntPtr adjustment, IntPtr userData)
    {
        // Find the scrollbar widget and invoke the callback
        // This is a simplified version - would need proper callback routing
    }
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_scrollbar_new(GtkOrientation orientation, IntPtr adjustment);
#else
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrollbar_new(GtkOrientation orientation, IntPtr adjustment);
#endif
}
