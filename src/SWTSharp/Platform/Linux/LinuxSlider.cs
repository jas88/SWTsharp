using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a slider platform widget.
/// Encapsulates GtkScale control without tick marks or value display.
/// </summary>
internal class LinuxSlider : LinuxWidget, IPlatformSlider
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";

    private IntPtr _scale;
    private bool _disposed;
    private int _minimum = 0;
    private int _maximum = 100;
    private int _value = 0;
    private int _increment = 1;
    private int _pageIncrement = 10;
    private RGB _background = new RGB(240, 240, 240);
    private RGB _foreground = new RGB(0, 120, 215);

    // GSignal callback delegate - must be stored to prevent GC
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GtkValueChangedFunc(IntPtr range, IntPtr data);
    private readonly GtkValueChangedFunc _valueChangedCallback;
    private GCHandle _callbackHandle;

    // Event handling
    public event EventHandler<int>? ValueChanged;

    // Suppress unused event warnings - these are part of the interface contract
#pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxSlider(IntPtr parentHandle, int style)
    {
        // Create callback and prevent GC collection
        _valueChangedCallback = OnValueChangedCallback;
        _callbackHandle = GCHandle.Alloc(_valueChangedCallback);

        // Determine orientation
        int orientation = ((style & SWT.VERTICAL) != 0)
            ? GTK_ORIENTATION_VERTICAL
            : GTK_ORIENTATION_HORIZONTAL;

        // Create GtkScale with range
        _scale = gtk_scale_new_with_range(orientation, _minimum, _maximum, _increment);
        if (_scale == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK scale");

        // Configure as slider (no value display)
        gtk_scale_set_draw_value(_scale, false);

        // For vertical sliders, invert so values increase upward (SWT convention)
        if ((style & SWT.VERTICAL) != 0)
        {
            gtk_range_set_inverted(_scale, true);
        }

        // Add to parent
        if (parentHandle != IntPtr.Zero)
            gtk_container_add(parentHandle, _scale);

        gtk_widget_show(_scale);

        // Setup event handlers
        SetupEventHandlers();
    }

    public int Value
    {
        get => _value;
        set
        {
            if (_disposed || _scale == IntPtr.Zero) return;
            value = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != value)
            {
                _value = value;
                gtk_range_set_value(_scale, value);
            }
        }
    }

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (_disposed || _scale == IntPtr.Zero) return;
            if (_minimum != value)
            {
                _minimum = value;
                gtk_range_set_range(_scale, _minimum, _maximum);
                // Ensure value is still valid
                if (_value < _minimum)
                {
                    Value = _minimum;
                }
            }
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            if (_disposed || _scale == IntPtr.Zero) return;
            if (_maximum != value)
            {
                _maximum = value;
                gtk_range_set_range(_scale, _minimum, _maximum);
                // Ensure value is still valid
                if (_value > _maximum)
                {
                    Value = _maximum;
                }
            }
        }
    }

    public int Increment
    {
        get => _increment;
        set
        {
            if (_disposed || _scale == IntPtr.Zero) return;
            if (value > 0 && _increment != value)
            {
                _increment = value;
                gtk_range_set_increments(_scale, _increment, _pageIncrement);
            }
        }
    }

    public int PageIncrement
    {
        get => _pageIncrement;
        set
        {
            if (_disposed || _scale == IntPtr.Zero) return;
            if (value > 0 && _pageIncrement != value)
            {
                _pageIncrement = value;
                gtk_range_set_increments(_scale, _increment, _pageIncrement);
            }
        }
    }

    public override IntPtr GetNativeHandle() => _scale;

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _scale == IntPtr.Zero) return;
        gtk_widget_set_size_request(_scale, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _scale == IntPtr.Zero)
            return default;

        gtk_widget_get_allocation(_scale, out GtkAllocation allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _scale == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_scale);
        else
            gtk_widget_hide(_scale);
    }

    public bool GetVisible()
    {
        if (_disposed || _scale == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_scale);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _scale == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_scale, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _scale == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_scale);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // GTK3 scales are styled via CSS - would need CSS provider for custom colors
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // GTK3 scales are styled via CSS - would need CSS provider for custom colors
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Free the GCHandle to allow callback collection
        if (_callbackHandle.IsAllocated)
        {
            _callbackHandle.Free();
        }

        if (_scale != IntPtr.Zero)
        {
            gtk_widget_destroy(_scale);
            _scale = IntPtr.Zero;
        }
    }

    private void SetupEventHandlers()
    {
        if (_disposed || _scale == IntPtr.Zero) return;

        // Connect value-changed signal
        g_signal_connect_data(
            _scale,
            "value-changed",
            Marshal.GetFunctionPointerForDelegate(_valueChangedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );
    }

    private void OnValueChangedCallback(IntPtr range, IntPtr data)
    {
        if (_disposed || _scale == IntPtr.Zero) return;

        double rawValue = gtk_range_get_value(_scale);
        int newValue = (int)Math.Round(rawValue);

        if (_value != newValue)
        {
            _value = newValue;
            ValueChanged?.Invoke(this, _value);
        }
    }

    // GTK3 Orientation constants
    private const int GTK_ORIENTATION_HORIZONTAL = 0;
    private const int GTK_ORIENTATION_VERTICAL = 1;

    // Scale functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scale_new_with_range(int orientation, double min, double max, double step);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scale_set_draw_value(IntPtr scale, bool draw_value);

    // Range functions (GtkScale is a GtkRange)
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_range_set_value(IntPtr range, double value);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern double gtk_range_get_value(IntPtr range);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_range_set_range(IntPtr range, double min, double max);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_range_set_increments(IntPtr range, double step, double page);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_range_set_inverted(IntPtr range, bool setting);

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

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern ulong g_signal_connect_data(
        IntPtr instance,
        string detailed_signal,
        IntPtr c_handler,
        IntPtr data,
        IntPtr destroy_data,
        int connect_flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x, y, width, height;
    }
}
