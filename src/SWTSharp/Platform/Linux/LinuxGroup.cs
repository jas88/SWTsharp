using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a Group widget (titled border container).
/// Uses GtkFrame with label for the title, and GtkFixed as internal container for children.
/// </summary>
internal class LinuxGroup : LinuxWidget, IPlatformComposite, IPlatformTextWidget
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _gtkFrameHandle;
    private IntPtr _gtkFixedHandle;  // Internal container for children
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private string _text = string.Empty;

    // Events required by IPlatformComposite
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // Events required by IPlatformTextWidget
    #pragma warning disable CS0067
    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? TextCommitted;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public LinuxGroup(IntPtr parentHandle, int style, string text)
    {
        _text = text ?? string.Empty;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxGroup] Creating group. Parent: 0x{parentHandle:X}, Style: 0x{style:X}, Text: {text}");

        // Create GtkFrame with label (the outer container with title)
        _gtkFrameHandle = gtk_frame_new(_text);

        if (_gtkFrameHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK frame for Group");
        }

        // Create GtkFixed as internal container for children - allows absolute positioning
        _gtkFixedHandle = gtk_fixed_new();

        if (_gtkFixedHandle == IntPtr.Zero)
        {
            gtk_widget_destroy(_gtkFrameHandle);
            throw new InvalidOperationException("Failed to create GTK fixed container for Group");
        }

        // Add the fixed container to the frame
        gtk_container_add(_gtkFrameHandle, _gtkFixedHandle);

        // Add frame to parent
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkFrameHandle);
        }

        // Show both widgets
        gtk_widget_show(_gtkFixedHandle);
        gtk_widget_show(_gtkFrameHandle);

        if (enableLogging)
            Console.WriteLine($"[LinuxGroup] Group created successfully. Frame: 0x{_gtkFrameHandle:X}, Fixed: 0x{_gtkFixedHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        // Return the frame handle as the main widget handle
        return _gtkFrameHandle;
    }

    /// <summary>
    /// Gets the internal container handle for adding children.
    /// </summary>
    internal IntPtr GetContainerHandle()
    {
        return _gtkFixedHandle;
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        lock (_children)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
                ChildAdded?.Invoke(this, child);
            }
        }

        // Add child to the internal fixed container, not the frame
        if (child is LinuxWidget linuxWidget)
        {
            IntPtr childHandle = linuxWidget.GetNativeHandle();
            if (childHandle != IntPtr.Zero && _gtkFixedHandle != IntPtr.Zero)
            {
                gtk_fixed_put(_gtkFixedHandle, childHandle, 0, 0);
            }
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        lock (_children)
        {
            if (_children.Remove(child))
            {
                ChildRemoved?.Invoke(this, child);
            }
        }

        // Remove from the internal container
        if (child is LinuxWidget linuxWidget)
        {
            IntPtr childHandle = linuxWidget.GetNativeHandle();
            if (childHandle != IntPtr.Zero && _gtkFixedHandle != IntPtr.Zero)
            {
                gtk_container_remove(_gtkFixedHandle, childHandle);
            }
        }
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        lock (_children)
        {
            return _children.ToArray();
        }
    }

    public void SetText(string text)
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return;

        _text = text ?? string.Empty;
        gtk_frame_set_label(_gtkFrameHandle, _text);
        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return string.Empty;

        IntPtr labelPtr = gtk_frame_get_label(_gtkFrameHandle);
        if (labelPtr == IntPtr.Zero) return string.Empty;

        return Marshal.PtrToStringAnsi(labelPtr) ?? string.Empty;
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return;

        gtk_widget_set_size_request(_gtkFrameHandle, width, height);
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_gtkFrameHandle, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_gtkFrameHandle);
        else
            gtk_widget_hide(_gtkFrameHandle);
    }

    public bool GetVisible()
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return false;

        return gtk_widget_get_visible(_gtkFrameHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_gtkFrameHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _gtkFrameHandle == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_gtkFrameHandle);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via CSS provider
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color via CSS provider
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Dispose children first
        lock (_children)
        {
            foreach (var child in _children.ToArray())
            {
                if (child is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _children.Clear();
        }

        // Destroy the widgets (destroying frame will destroy fixed automatically)
        if (_gtkFrameHandle != IntPtr.Zero)
        {
            gtk_widget_destroy(_gtkFrameHandle);
            _gtkFrameHandle = IntPtr.Zero;
            _gtkFixedHandle = IntPtr.Zero;
        }
    }

    // GTK3 P/Invoke declarations

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_frame_new(string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void gtk_frame_set_label(IntPtr frame, string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_frame_get_label(IntPtr frame);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_fixed_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_put(IntPtr @fixed, IntPtr widget, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

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

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
