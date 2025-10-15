using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux implementation of a label platform widget using GtkLabel.
/// </summary>
internal class LinuxLabel : IPlatformTextWidget
{
    private IntPtr _gtkLabel;
    private bool _disposed;
    private string _text = string.Empty;

    // Event handling
    public event EventHandler<string>? TextChanged;
#pragma warning disable CS0067 // Event is never used
    public event EventHandler<string>? TextCommitted;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxLabel(IntPtr parentHandle, int style)
    {
        _gtkLabel = CreateGtkLabel(style);

        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkLabel);
        }

        gtk_widget_show(_gtkLabel);
    }

    public void SetText(string text)
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return;

        _text = text ?? string.Empty;
        gtk_label_set_text(_gtkLabel, _text);
        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return _text;

        IntPtr textPtr = gtk_label_get_text(_gtkLabel);
        return PtrToStringUTF8(textPtr);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return;

        gtk_widget_set_size_request(_gtkLabel, width, height);
        // Note: Absolute positioning requires fixed container
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_gtkLabel, out allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_gtkLabel);
        else
            gtk_widget_hide(_gtkLabel);
    }

    public bool GetVisible()
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_gtkLabel);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_gtkLabel, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _gtkLabel == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_gtkLabel);
    }

    public void SetBackground(RGB color)
    {
        // Background color handled by CSS in GTK3+
    }

    public RGB GetBackground()
    {
        return new RGB(240, 240, 240);
    }

    public void SetForeground(RGB color)
    {
        // Text color handled by CSS in GTK3+
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0);
    }

    public IntPtr GetNativeHandle()
    {
        return _gtkLabel;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_gtkLabel != IntPtr.Zero)
            {
                gtk_widget_destroy(_gtkLabel);
                _gtkLabel = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    private static string PtrToStringUTF8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

#if NETSTANDARD2_0
        // Manual UTF-8 decoding for netstandard2.0
        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0)
            length++;

        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
#else
        // Use built-in method for .NET Core 2.1+
        return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
#endif
    }

    private IntPtr CreateGtkLabel(int style)
    {
        IntPtr label;

        // Handle separator
        if ((style & SWT.SEPARATOR) != 0)
        {
            if ((style & SWT.VERTICAL) != 0)
                label = gtk_separator_new(GTK_ORIENTATION_VERTICAL);
            else
                label = gtk_separator_new(GTK_ORIENTATION_HORIZONTAL);
        }
        else
        {
            label = gtk_label_new("");

            // Handle alignment
            float xalign = 0.0f; // Left
            if ((style & SWT.CENTER) != 0)
                xalign = 0.5f;
            else if ((style & SWT.RIGHT) != 0)
                xalign = 1.0f;

            gtk_label_set_xalign(label, xalign);

            // Handle wrap
            if ((style & SWT.WRAP) != 0)
            {
                gtk_label_set_line_wrap(label, true);
            }
        }

        return label;
    }

    // GTK3 Constants
    private const int GTK_ORIENTATION_HORIZONTAL = 0;
    private const int GTK_ORIENTATION_VERTICAL = 1;

    // GTK3 P/Invoke
    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_label_new(string text);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_label_set_text(IntPtr label, string text);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_label_get_text(IntPtr label);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_label_set_xalign(IntPtr label, float xalign);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_label_set_line_wrap(IntPtr label, bool wrap);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_separator_new(int orientation);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport("libgtk-3.so.0")]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
