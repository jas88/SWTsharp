using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a scrollbar platform widget.
/// Uses GtkScrollbar with GtkAdjustment for value management.
/// </summary>
internal class LinuxScrollBar : LinuxWidget, IPlatformScrollBar
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _scrollBar;
    private readonly IntPtr _adjustment;
    private bool _disposed;
    private int _minimum;
    private int _maximum = 100;
    private int _value;
    private int _increment = 1;
    private int _pageIncrement = 10;
    private int _thumb = 10;
    private Rectangle _requestedBounds;

    public event EventHandler<int>? ValueChanged;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    public LinuxScrollBar(IntPtr parentHandle, int style)
    {
        // Create adjustment (value, lower, upper, step_increment, page_increment, page_size)
        _adjustment = gtk_adjustment_new(0, 0, 100, 1, 10, 10);
        if (_adjustment == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK adjustment");

        // Determine orientation from style
        int orientation = (style & SWT.VERTICAL) != 0 ? GTK_ORIENTATION_VERTICAL : GTK_ORIENTATION_HORIZONTAL;

        // Create scrollbar with orientation and adjustment
        _scrollBar = gtk_scrollbar_new(orientation, _adjustment);
        if (_scrollBar == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK scrollbar");

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
            gtk_container_add(parentHandle, _scrollBar);

        gtk_widget_show(_scrollBar);
    }

    public override IntPtr GetNativeHandle() => _scrollBar;

    public int Value
    {
        get => _value;
        set
        {
            if (_disposed || _scrollBar == IntPtr.Zero) return;
            value = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != value)
            {
                _value = value;
                UpdateAdjustment();
                ValueChanged?.Invoke(this, value);
            }
        }
    }

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (_disposed || _scrollBar == IntPtr.Zero) return;
            if (_minimum != value)
            {
                _minimum = value;
                UpdateAdjustment();
            }
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            if (_disposed || _scrollBar == IntPtr.Zero) return;
            if (_maximum != value)
            {
                _maximum = value;
                UpdateAdjustment();
            }
        }
    }

    public int Increment
    {
        get => _increment;
        set
        {
            if (_disposed || _scrollBar == IntPtr.Zero) return;
            if (_increment != value)
            {
                _increment = value;
                UpdateAdjustment();
            }
        }
    }

    public int PageIncrement
    {
        get => _pageIncrement;
        set
        {
            if (_disposed || _scrollBar == IntPtr.Zero) return;
            if (_pageIncrement != value)
            {
                _pageIncrement = value;
                UpdateAdjustment();
            }
        }
    }

    public int Thumb
    {
        get => _thumb;
        set
        {
            if (_disposed || _scrollBar == IntPtr.Zero) return;
            if (_thumb != value)
            {
                _thumb = value;
                UpdateAdjustment();
            }
        }
    }

    private void UpdateAdjustment()
    {
        if (_adjustment == IntPtr.Zero) return;

        // Clamp value to current range before passing to GTK
        _value = Math.Max(_minimum, Math.Min(_maximum, _value));

        gtk_adjustment_configure(
            _adjustment,
            _value,             // value
            _minimum,           // lower
            _maximum,           // upper
            _increment,         // step_increment
            _pageIncrement,     // page_increment
            _thumb);            // page_size
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _scrollBar == IntPtr.Zero) return;
        _requestedBounds = new Rectangle(x, y, width, height);
        gtk_widget_set_size_request(_scrollBar, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _scrollBar == IntPtr.Zero)
            return default;

        // Return requested bounds since GTK allocation may not reflect size request
        // until widget is realized and laid out
        return _requestedBounds;
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _scrollBar == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_scrollBar);
        else
            gtk_widget_hide(_scrollBar);
    }

    public bool GetVisible()
    {
        if (_disposed || _scrollBar == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_scrollBar);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _scrollBar == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_scrollBar, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _scrollBar == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_scrollBar);
    }

    public void SetBackground(RGB color)
    {
        // GTK3 scrollbar background is controlled via CSS/theming
        // Custom colors would require CSS provider setup
    }

    public RGB GetBackground()
    {
        return new RGB(200, 200, 200); // Default GTK scrollbar background
    }

    public void SetForeground(RGB color)
    {
        // Not applicable for scrollbar
    }

    public RGB GetForeground()
    {
        return new RGB(100, 100, 100); // Default GTK scrollbar foreground
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DetachFromParent();

        if (_scrollBar != IntPtr.Zero)
        {
            gtk_widget_destroy(_scrollBar);
            _scrollBar = IntPtr.Zero;
        }
    }

    // GTK orientation constants
    private const int GTK_ORIENTATION_HORIZONTAL = 0;
    private const int GTK_ORIENTATION_VERTICAL = 1;

    // GtkAllocation structure
    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x, y, width, height;
    }

    // GTK scrollbar functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrollbar_new(int orientation, IntPtr adjustment);

    // GTK adjustment functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_adjustment_new(double value, double lower, double upper,
        double step_increment, double page_increment, double page_size);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_configure(IntPtr adjustment, double value, double lower,
        double upper, double step_increment, double page_increment, double page_size);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern double gtk_adjustment_get_value(IntPtr adjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_set_value(IntPtr adjustment, double value);

    // Common widget functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);
}
