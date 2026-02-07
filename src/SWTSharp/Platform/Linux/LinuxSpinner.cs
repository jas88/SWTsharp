using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a spinner platform widget.
/// Encapsulates GtkSpinButton control for numeric entry with up/down buttons.
/// </summary>
internal class LinuxSpinner : LinuxWidget, IPlatformSpinner
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";

    private IntPtr _spinButton;
    private bool _disposed;
    private int _minimum = 0;
    private int _maximum = 100;
    private int _value = 0;
    private int _increment = 1;
    private int _digits = 0;
    private int _textLimit = 0;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);

    // GSignal callback delegates - must be stored to prevent GC
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GtkValueChangedFunc(IntPtr spinButton, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GtkChangedFunc(IntPtr editable, IntPtr data);

    private readonly GtkValueChangedFunc _valueChangedCallback;
    private readonly GtkChangedFunc _textChangedCallback;
    private GCHandle _valueCallbackHandle;
    private GCHandle _textCallbackHandle;

    // Event handling
    public event EventHandler<int>? ValueChanged;
    public event EventHandler<string>? TextChanged;

    // Suppress unused event warnings - these are part of the interface contract
#pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxSpinner(IntPtr parentHandle, int style)
    {
        // Create callbacks and prevent GC collection
        _valueChangedCallback = OnValueChangedCallback;
        _textChangedCallback = OnTextChangedCallback;
        _valueCallbackHandle = GCHandle.Alloc(_valueChangedCallback);
        _textCallbackHandle = GCHandle.Alloc(_textChangedCallback);

        // Create GtkSpinButton with range
        _spinButton = gtk_spin_button_new_with_range(_minimum, _maximum, _increment);
        if (_spinButton == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK spin button");

        // Set to integer mode (0 decimal places)
        gtk_spin_button_set_digits(_spinButton, 0);

        // Handle SWT.WRAP style
        if ((style & SWT.WRAP) != 0)
        {
            gtk_spin_button_set_wrap(_spinButton, true);
        }

        // Handle SWT.READ_ONLY style
        if ((style & SWT.READ_ONLY) != 0)
        {
            gtk_editable_set_editable(_spinButton, false);
        }

        // Add to parent
        if (parentHandle != IntPtr.Zero)
            gtk_container_add(parentHandle, _spinButton);

        gtk_widget_show(_spinButton);

        // Setup event handlers
        SetupEventHandlers();
    }

    public int Value
    {
        get => _value;
        set
        {
            if (_disposed || _spinButton == IntPtr.Zero) return;
            value = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != value)
            {
                _value = value;
                gtk_spin_button_set_value(_spinButton, value);
            }
        }
    }

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (_disposed || _spinButton == IntPtr.Zero) return;
            if (_minimum != value)
            {
                _minimum = value;
                UpdateRange();
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
            if (_disposed || _spinButton == IntPtr.Zero) return;
            if (_maximum != value)
            {
                _maximum = value;
                UpdateRange();
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
            if (_disposed || _spinButton == IntPtr.Zero) return;
            if (value > 0 && _increment != value)
            {
                _increment = value;
                // Page increment is typically 10x line increment
                gtk_spin_button_set_increments(_spinButton, _increment, _increment * 10);
            }
        }
    }

    public int Digits
    {
        get => _digits;
        set
        {
            if (_disposed || _spinButton == IntPtr.Zero) return;
            if (value >= 0 && _digits != value)
            {
                _digits = value;
                gtk_spin_button_set_digits(_spinButton, (uint)_digits);
            }
        }
    }

    public int TextLimit
    {
        get => _textLimit;
        set
        {
            if (_disposed || _spinButton == IntPtr.Zero) return;
            _textLimit = value >= 0 ? value : 0;
            gtk_entry_set_max_length(_spinButton, _textLimit);
        }
    }

    public override IntPtr GetNativeHandle() => _spinButton;

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;
        gtk_widget_set_size_request(_spinButton, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _spinButton == IntPtr.Zero)
            return default;

        gtk_widget_get_allocation(_spinButton, out GtkAllocation allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_spinButton);
        else
            gtk_widget_hide(_spinButton);
    }

    public bool GetVisible()
    {
        if (_disposed || _spinButton == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_spinButton);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_spinButton, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _spinButton == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_spinButton);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // GTK3 widgets are styled via CSS - would need CSS provider for custom colors
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // GTK3 widgets are styled via CSS - would need CSS provider for custom colors
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

        // Free the GCHandles to allow callback collection
        if (_valueCallbackHandle.IsAllocated)
        {
            _valueCallbackHandle.Free();
        }
        if (_textCallbackHandle.IsAllocated)
        {
            _textCallbackHandle.Free();
        }

        // Do NOT call gtk_widget_destroy -- parent window destruction handles cleanup.
        _spinButton = IntPtr.Zero;
    }

    private void UpdateRange()
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;

        IntPtr adjustment = gtk_spin_button_get_adjustment(_spinButton);
        if (adjustment != IntPtr.Zero)
        {
            gtk_adjustment_set_lower(adjustment, _minimum);
            gtk_adjustment_set_upper(adjustment, _maximum);
        }
    }

    private void SetupEventHandlers()
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;

        // Connect value-changed signal (fired when value changes via buttons or programmatically)
        g_signal_connect_data(
            _spinButton,
            "value-changed",
            Marshal.GetFunctionPointerForDelegate(_valueChangedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );

        // Connect changed signal (fired when text in entry changes - for SWT.Modify events)
        g_signal_connect_data(
            _spinButton,
            "changed",
            Marshal.GetFunctionPointerForDelegate(_textChangedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );
    }

    private void OnValueChangedCallback(IntPtr spinButton, IntPtr data)
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;

        double rawValue = gtk_spin_button_get_value(_spinButton);
        int newValue = (int)Math.Round(rawValue);

        if (_value != newValue)
        {
            _value = newValue;
            ValueChanged?.Invoke(this, _value);
        }
    }

    private void OnTextChangedCallback(IntPtr editable, IntPtr data)
    {
        if (_disposed || _spinButton == IntPtr.Zero) return;

        // Get the current text from the spin button entry
        IntPtr textPtr = gtk_entry_get_text(_spinButton);
        string text = string.Empty;
        if (textPtr != IntPtr.Zero)
        {
#if NET5_0_OR_GREATER
            text = Marshal.PtrToStringUTF8(textPtr) ?? string.Empty;
#else
            text = GetUtf8String(textPtr);
#endif
        }

        TextChanged?.Invoke(this, text);
    }

#if !NET5_0_OR_GREATER
    /// <summary>
    /// Helper method to convert UTF-8 pointer to string for older .NET versions.
    /// </summary>
    private static string GetUtf8String(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

        // Find null terminator
        int length = 0;
        unsafe
        {
            byte* p = (byte*)ptr;
            while (p[length] != 0)
                length++;
        }

        if (length == 0)
            return string.Empty;

        // Copy to managed array and decode
        byte[] bytes = new byte[length];
        Marshal.Copy(ptr, bytes, 0, length);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
#endif

    // SpinButton functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_spin_button_new_with_range(double min, double max, double step);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_spin_button_set_value(IntPtr spin_button, double value);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern double gtk_spin_button_get_value(IntPtr spin_button);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_spin_button_set_digits(IntPtr spin_button, uint digits);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_spin_button_set_wrap(IntPtr spin_button, bool wrap);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_spin_button_set_increments(IntPtr spin_button, double step, double page);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_spin_button_get_adjustment(IntPtr spin_button);

    // Adjustment functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_set_lower(IntPtr adjustment, double lower);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_set_upper(IntPtr adjustment, double upper);

    // Entry functions (GtkSpinButton inherits from GtkEntry)
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_entry_get_text(IntPtr entry);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_entry_set_max_length(IntPtr entry, int max);

    // Editable functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_editable_set_editable(IntPtr editable, bool is_editable);

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
