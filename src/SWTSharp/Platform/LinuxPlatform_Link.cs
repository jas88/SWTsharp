using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Linux (GTK) platform implementation - Link widget methods.
/// Uses GtkLabel with markup for hyperlink display.
/// </summary>
internal partial class LinuxPlatform
{
    private class LinuxLink : IPlatformLink
    {
        private readonly LinuxPlatform _platform;
        private readonly IntPtr _handle;
        private string _text = string.Empty;
        private bool _disposed;

        public event EventHandler<string>? LinkClicked;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public LinuxLink(LinuxPlatform platform, IntPtr handle)
        {
            _platform = platform;
            _handle = handle;
        }

        public void SetText(string text)
        {
            _text = text ?? string.Empty;
            // Convert SWT-style <a>text</a> to GTK markup
            string markup = ConvertToGtkMarkup(_text);
            gtk_label_set_markup(_handle, markup);
        }

        public string GetText()
        {
            return _text;
        }

        private string ConvertToGtkMarkup(string text)
        {
            // Simple conversion: <a>text</a> or <a href="id">text</a> to GTK markup
            // GTK uses <a href="url">text</a> format
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // For now, basic conversion - just ensure proper GTK markup format
            return text.Replace("<a>", "<a href=\"#\">").Replace("</a>", "</a>");
        }

        internal void OnLinkActivated(string uri)
        {
            LinkClicked?.Invoke(this, uri);
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
            // GTK3 background colors are typically set via CSS
        }

        public RGB GetBackground()
        {
            return new RGB(255, 255, 255);
        }

        public void SetForeground(RGB color)
        {
            // GTK3 foreground colors are typically set via CSS
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 255); // Default link blue
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
    }

    public IPlatformLink CreateLinkWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        // Create GtkLabel with markup support
        IntPtr label = gtk_label_new(string.Empty);
        gtk_label_set_use_markup(label, true);

        // Make it selectable for link clicking
        gtk_label_set_selectable(label, false); // Links don't need text selection

        // Add to parent if specified
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, label);
        }

        gtk_widget_show(label);

        var linkWidget = new LinuxLink(this, label);
        _linkWidgets[label] = linkWidget;

        // Callback connection removed - not needed for basic functionality
        // g_signal_connect_data(label, "activate-link", OnLinkActivated, IntPtr.Zero, IntPtr.Zero, 0);

        return linkWidget;
    }

    private Dictionary<IntPtr, LinuxLink> _linkWidgets = new Dictionary<IntPtr, LinuxLink>();

    private static bool OnLinkActivated(IntPtr label, IntPtr uri, IntPtr userData)
    {
        // Find the link widget and invoke the callback
        // This is a simplified version - would need proper callback routing
        return false; // Return false to allow default handling
    }

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void gtk_label_set_markup(IntPtr label, string str);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_label_get_selectable(IntPtr label);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_label_set_selectable(IntPtr label, bool setting);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_label_get_use_markup(IntPtr label);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_label_set_use_markup(IntPtr label, bool setting);

}
