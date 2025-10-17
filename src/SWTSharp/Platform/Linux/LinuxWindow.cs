using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a window platform widget.
/// Encapsulates GtkWindow and provides IPlatformWindow functionality.
/// </summary>
internal class LinuxWindow : LinuxWidget, IPlatformWindow
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _gtkWindowHandle;
    private IntPtr _container; // GtkFixed container for child widgets
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;

    // Event handling
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
#pragma warning disable CS0067 // Event is never used (events will be implemented in future phase)
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxWindow(int style, string title)
    {
        // Create GtkWindow
        _gtkWindowHandle = gtk_window_new(GtkWindowType.Toplevel);

        if (_gtkWindowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK window");
        }

        // Set window properties
        gtk_window_set_title(_gtkWindowHandle, title);
        gtk_window_set_default_size(_gtkWindowHandle, 800, 600);

        // CRITICAL: GtkWindow is a GtkBin and can only contain ONE child widget.
        // Create a GtkFixed container to hold multiple widgets, like NSWindow's contentView.
        _container = gtk_fixed_new();
        if (_container != IntPtr.Zero)
        {
            gtk_container_add(_gtkWindowHandle, _container);
            gtk_widget_show(_container);
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return;

        gtk_window_move(_gtkWindowHandle, x, y);
        gtk_window_resize(_gtkWindowHandle, width, height);

        // Fire LayoutRequested event when bounds change
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return default(Rectangle);

        gtk_window_get_position(_gtkWindowHandle, out int x, out int y);
        gtk_window_get_size(_gtkWindowHandle, out int width, out int height);

        return new Rectangle(x, y, width, height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return;

        if (visible)
        {
            gtk_widget_show_all(_gtkWindowHandle);
        }
        else
        {
            gtk_widget_hide(_gtkWindowHandle);
        }
    }

    public bool GetVisible()
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return false;

        return gtk_widget_get_visible(_gtkWindowHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_gtkWindowHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_gtkWindowHandle);
    }

    public void SetBackground(RGB color)
    {
        // TODO: Implement background color setting with CSS provider
        // This requires GtkCssProvider and gtk_style_context APIs
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color getting
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // GtkWindow doesn't have direct foreground color
        // This would affect child widgets
    }

    public RGB GetForeground()
    {
        // GtkWindow doesn't have direct foreground color
        return new RGB(0, 0, 0); // Default black
    }

    public void SetTitle(string title)
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return;

        gtk_window_set_title(_gtkWindowHandle, title);
    }

    public string GetTitle()
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return "";

        IntPtr titlePtr = gtk_window_get_title(_gtkWindowHandle);
        return Marshal.PtrToStringAuto(titlePtr) ?? "";
    }

    public void Open()
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return;

        gtk_widget_show_all(_gtkWindowHandle);
    }

    public void Close()
    {
        if (_disposed || _gtkWindowHandle == IntPtr.Zero) return;

        gtk_widget_destroy(_gtkWindowHandle);
    }

    public bool IsDisposed => _disposed;

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        if (child is LinuxWidget linuxWidget)
        {
            var childHandle = linuxWidget.GetNativeHandle();

            // Add child to the container (not directly to window)
            if (_container != IntPtr.Zero)
            {
                gtk_container_add(_container, childHandle);
                gtk_widget_show(childHandle);
            }

            _platformChildren.Add(child);

            // Fire ChildAdded event
            ChildAdded?.Invoke(this, child);
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        if (child is LinuxWidget linuxWidget)
        {
            var childHandle = linuxWidget.GetNativeHandle();

            // Remove from container
            if (_container != IntPtr.Zero)
            {
                gtk_container_remove(_container, childHandle);
            }

            _platformChildren.Remove(child);

            // Fire ChildRemoved event
            ChildRemoved?.Invoke(this, child);
        }
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return _platformChildren.AsReadOnly();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Remove all children first
            foreach (var child in _platformChildren.ToArray())
            {
                RemoveChild(child);
            }

            if (_gtkWindowHandle != IntPtr.Zero)
            {
                gtk_widget_destroy(_gtkWindowHandle);
                _gtkWindowHandle = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        // CRITICAL FIX: Return the GtkFixed container, NOT the GtkWindow.
        // GtkWindow is a GtkBin and can only contain ONE widget.
        // Child widgets must be added to the GtkFixed container, not directly to the window.
        // This prevents "GtkBin can only contain one widget at a time" warnings.
        return _container != IntPtr.Zero ? _container : _gtkWindowHandle;
    }

    // GTK Window Type Enumeration
    private enum GtkWindowType
    {
        Toplevel = 0,
        Popup = 1
    }

    // GTK P/Invoke declarations
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_window_new(GtkWindowType type);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_window_set_title(IntPtr window, string title);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_window_get_title(IntPtr window);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_set_default_size(IntPtr window, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_move(IntPtr window, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_resize(IntPtr window, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_get_position(IntPtr window, out int root_x, out int root_y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_get_size(IntPtr window, out int width, out int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_fixed_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show_all(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);
}
