using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using System.Collections.Concurrent;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Base class for all Linux/GTK platform widgets.
/// Provides common functionality and native handle access.
/// </summary>
internal abstract class LinuxWidget
{
    /// <summary>
    /// Gets the native GtkWidget handle.
    /// </summary>
    public abstract IntPtr GetNativeHandle();
}

/// <summary>
/// Linux/GTK implementation of a button platform widget.
/// Encapsulates GtkButton/GtkToggleButton/GtkRadioButton/GtkCheckButton.
/// </summary>
internal class LinuxButton : LinuxWidget, IPlatformTextWidget
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";
    private const string GLibLib = "libglib-2.0.so.0";

    private IntPtr _gtkButtonHandle;
    private bool _disposed;
    private readonly int _style;

    // Static mapping of button handles to instances for callback routing
    private static readonly ConcurrentDictionary<IntPtr, LinuxButton> _buttonInstances = new();

    // GSignal callback delegate
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GtkSignalFunc(IntPtr widget, IntPtr data);
    private readonly GtkSignalFunc _clickCallback;

    // Event handling
    public event EventHandler<int>? Click;
#pragma warning disable CS0067 // Event is never used (events are used via interface by Button.cs)
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? TextCommitted;
#pragma warning restore CS0067

    public LinuxButton(IntPtr parentHandle, int style)
    {
        _style = style;
        _clickCallback = OnButtonClickedCallback;

        // Create appropriate GTK button based on style
        _gtkButtonHandle = CreateGtkButton(style);

        if (_gtkButtonHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK button");
        }

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkButtonHandle);
        }

        // Show the widget
        gtk_widget_show(_gtkButtonHandle);

        // Setup event handlers
        SetupEventHandlers();
    }

    public void SetText(string text)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        gtk_button_set_label(_gtkButtonHandle, text ?? string.Empty);
        TextChanged?.Invoke(this, text ?? string.Empty);
    }

    public string GetText()
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return string.Empty;

        IntPtr labelPtr = gtk_button_get_label(_gtkButtonHandle);
        if (labelPtr == IntPtr.Zero) return string.Empty;

#if NETSTANDARD2_0
        return GetUtf8String(labelPtr);
#else
        return Marshal.PtrToStringUTF8(labelPtr) ?? string.Empty;
#endif
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        gtk_widget_set_size_request(_gtkButtonHandle, width, height);
        // Note: Position would require gtk_fixed_put if parent is GtkFixed
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_gtkButtonHandle, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_gtkButtonHandle);
        else
            gtk_widget_hide(_gtkButtonHandle);
    }

    public bool GetVisible()
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return false;

        return gtk_widget_get_visible(_gtkButtonHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_gtkButtonHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_gtkButtonHandle);
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        // TODO: Implement background color via CSS provider
        // This requires GtkCssProvider and gtk_style_context APIs
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color retrieval
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        // TODO: Implement foreground color via CSS provider
    }

    public RGB GetForeground()
    {
        // TODO: Implement foreground color retrieval
        return new RGB(0, 0, 0); // Default black
    }

    public bool GetSelection()
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return false;

        // Only valid for toggle-style buttons
        if ((_style & (SWT.CHECK | SWT.RADIO | SWT.TOGGLE)) != 0)
        {
            return gtk_toggle_button_get_active(_gtkButtonHandle);
        }

        return false;
    }

    public void SetSelection(bool selected)
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        // Only valid for toggle-style buttons
        if ((_style & (SWT.CHECK | SWT.RADIO | SWT.TOGGLE)) != 0)
        {
            gtk_toggle_button_set_active(_gtkButtonHandle, selected);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_gtkButtonHandle != IntPtr.Zero)
            {
                // Remove from instance mapping
                _buttonInstances.TryRemove(_gtkButtonHandle, out _);

                // Destroy widget
                gtk_widget_destroy(_gtkButtonHandle);
                _gtkButtonHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _gtkButtonHandle;
    }

    private IntPtr CreateGtkButton(int style)
    {
        IntPtr button;

        if ((style & SWT.CHECK) != 0)
        {
            button = gtk_check_button_new_with_label(string.Empty);
        }
        else if ((style & SWT.RADIO) != 0)
        {
            button = gtk_radio_button_new_with_label(IntPtr.Zero, string.Empty);
        }
        else if ((style & SWT.TOGGLE) != 0)
        {
            button = gtk_toggle_button_new_with_label(string.Empty);
        }
        else if ((style & SWT.ARROW) != 0)
        {
            // Create button with arrow (use gtk_button_new and add arrow icon)
            button = gtk_button_new();
        }
        else
        {
            // Default: PUSH button
            button = gtk_button_new_with_label(string.Empty);
        }

        return button;
    }

    private void SetupEventHandlers()
    {
        if (_disposed || _gtkButtonHandle == IntPtr.Zero) return;

        // Register this button instance for callback routing
        _buttonInstances[_gtkButtonHandle] = this;

        // Connect click signal
        g_signal_connect_data(
            _gtkButtonHandle,
            "clicked",
            Marshal.GetFunctionPointerForDelegate(_clickCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );
    }

    private void OnButtonClickedCallback(IntPtr widget, IntPtr data)
    {
        // Verify this is our button and invoke click event
        if (_buttonInstances.TryGetValue(widget, out var button) && button == this)
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        if (_disposed) return;
        Click?.Invoke(this, 1); // Button 1 = left mouse button
    }

#if NETSTANDARD2_0
    /// <summary>
    /// Helper method to convert UTF-8 pointer to string for .NET Standard 2.0.
    /// Marshal.PtrToStringUTF8 was added in .NET Core 2.1+
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

    // GTK3 P/Invoke declarations
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_button_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_button_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_button_set_label(IntPtr button, string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_button_get_label(IntPtr button);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_check_button_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_radio_button_new_with_label(IntPtr group, string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_toggle_button_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toggle_button_set_active(IntPtr toggle_button, bool is_active);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_toggle_button_get_active(IntPtr toggle_button);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

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
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
