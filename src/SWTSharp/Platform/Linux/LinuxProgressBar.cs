using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

internal class LinuxProgressBar : LinuxWidget, IPlatformProgressBar
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _progressBar;
    private bool _disposed;
    private int _minimum = 0;
    private int _maximum = 100;
    private int _value = 0;
    private int _state = ProgressBarState.NORMAL;
    private RGB _background = new RGB(240, 240, 240);
    private RGB _foreground = new RGB(0, 120, 215);

    public event EventHandler<int>? ValueChanged;

    public LinuxProgressBar(IntPtr parentHandle, int style)
    {
        _progressBar = gtk_progress_bar_new();
        if (_progressBar == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK progress bar");

        // Handle vertical orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            gtk_orientable_set_orientation(_progressBar, GTK_ORIENTATION_VERTICAL);
            // Invert for vertical (progress goes up, not down)
            gtk_progress_bar_set_inverted(_progressBar, true);
        }

        // Add to parent
        if (parentHandle != IntPtr.Zero)
            gtk_container_add(parentHandle, _progressBar);

        gtk_widget_show(_progressBar);
    }

    public int Value
    {
        get => _value;
        set
        {
            if (_disposed || _progressBar == IntPtr.Zero) return;
            value = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != value)
            {
                _value = value;
                double fraction = (_maximum > _minimum)
                    ? (double)(_value - _minimum) / (_maximum - _minimum)
                    : 0.0;
                gtk_progress_bar_set_fraction(_progressBar, fraction);
                ValueChanged?.Invoke(this, value);
            }
        }
    }

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (_disposed || _progressBar == IntPtr.Zero) return;
            if (_minimum != value)
            {
                _minimum = value;
                // Clamp value to new range before recalculating fraction
                _value = Math.Max(_minimum, Math.Min(_maximum, _value));
                if (_maximum > _minimum)
                {
                    double fraction = (double)(_value - _minimum) / (_maximum - _minimum);
                    gtk_progress_bar_set_fraction(_progressBar, fraction);
                }
                else
                {
                    // Collapsed range - set fraction to 0
                    gtk_progress_bar_set_fraction(_progressBar, 0.0);
                }
            }
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            if (_disposed || _progressBar == IntPtr.Zero) return;
            if (_maximum != value)
            {
                _maximum = value;
                // Clamp value to new range before recalculating fraction
                _value = Math.Max(_minimum, Math.Min(_maximum, _value));
                if (_maximum > _minimum)
                {
                    double fraction = (double)(_value - _minimum) / (_maximum - _minimum);
                    gtk_progress_bar_set_fraction(_progressBar, fraction);
                }
                else
                {
                    // Collapsed range - set fraction to 0
                    gtk_progress_bar_set_fraction(_progressBar, 0.0);
                }
            }
        }
    }

    public int State
    {
        get => _state;
        set
        {
            _state = value;
            // GTK doesn't have native progress bar states (normal/error/paused)
            // Could use CSS classes for visual feedback, but keep it simple for now
        }
    }

    public override IntPtr GetNativeHandle() => _progressBar;

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _progressBar == IntPtr.Zero) return;

        // GTK uses size-allocate signal for positioning
        // Set size request for minimum size
        gtk_widget_set_size_request(_progressBar, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _progressBar == IntPtr.Zero)
            return default;

        gtk_widget_get_allocation(_progressBar, out GtkAllocation allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _progressBar == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_progressBar);
        else
            gtk_widget_hide(_progressBar);
    }

    public bool GetVisible()
    {
        if (_disposed || _progressBar == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_progressBar);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _progressBar == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_progressBar, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _progressBar == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_progressBar);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // GTK3 progress bars are styled via CSS
        // Would need CSS provider for custom colors
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // GTK3 progress bars are styled via CSS
        // Would need CSS provider for custom colors
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DetachFromParent();

        // Do NOT call gtk_widget_destroy -- parent window destruction handles cleanup.
        _progressBar = IntPtr.Zero;
    }

    // Progress bar functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_progress_bar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_set_fraction(IntPtr pbar, double fraction);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern double gtk_progress_bar_get_fraction(IntPtr pbar);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_set_inverted(IntPtr pbar, bool inverted);

    // Orientation
    private const int GTK_ORIENTATION_VERTICAL = 1;

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_orientable_set_orientation(IntPtr orientable, int orientation);

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

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x, y, width, height;
    }
}
