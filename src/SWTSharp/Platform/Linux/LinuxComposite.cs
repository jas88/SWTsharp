using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a composite platform widget.
/// Uses GtkFixed container to allow absolute positioning of child widgets.
/// </summary>
internal class LinuxComposite : LinuxWidget, IPlatformComposite
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _gtkFixedHandle;
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    public LinuxComposite(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxComposite] Creating composite. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create GtkFixed container - allows absolute positioning of children
        _gtkFixedHandle = gtk_fixed_new();

        if (_gtkFixedHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK fixed container");
        }

        // Add border if requested
        if ((style & SWT.BORDER) != 0)
        {
            // Create a GtkFrame to provide border
            IntPtr frame = gtk_frame_new(null);
            gtk_container_add(frame, _gtkFixedHandle);

            // Add frame to parent
            if (parentHandle != IntPtr.Zero)
            {
                gtk_container_add(parentHandle, frame);
            }

            gtk_widget_show(frame);
        }
        else
        {
            // Add directly to parent
            if (parentHandle != IntPtr.Zero)
            {
                gtk_container_add(parentHandle, _gtkFixedHandle);
            }
        }

        // Show the widget
        gtk_widget_show(_gtkFixedHandle);

        if (enableLogging)
            Console.WriteLine($"[LinuxComposite] Composite created successfully. Handle: 0x{_gtkFixedHandle:X}");
    }

    public override IntPtr GetNativeHandle()
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

        // If it's a Linux widget, add it to the fixed container
        if (child is LinuxWidget linuxWidget)
        {
            IntPtr childHandle = linuxWidget.GetNativeHandle();
            if (childHandle != IntPtr.Zero && _gtkFixedHandle != IntPtr.Zero)
            {
                // gtk_fixed_put adds the child at position (0, 0) by default
                // The child's position will be set via SetBounds
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

        // If it's a Linux widget, remove it from the container
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

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _gtkFixedHandle == IntPtr.Zero) return;

        gtk_widget_set_size_request(_gtkFixedHandle, width, height);

        // Note: Position would require gtk_fixed_move if parent is also GtkFixed
        // For now, just set the size request

        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _gtkFixedHandle == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_gtkFixedHandle, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _gtkFixedHandle == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_gtkFixedHandle);
        else
            gtk_widget_hide(_gtkFixedHandle);
    }

    public bool GetVisible()
    {
        if (_disposed || _gtkFixedHandle == IntPtr.Zero) return false;

        return gtk_widget_get_visible(_gtkFixedHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _gtkFixedHandle == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_gtkFixedHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _gtkFixedHandle == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_gtkFixedHandle);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via CSS provider
        // This requires GtkCssProvider and gtk_style_context APIs
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

        // Destroy the widget
        if (_gtkFixedHandle != IntPtr.Zero)
        {
            gtk_widget_destroy(_gtkFixedHandle);
            _gtkFixedHandle = IntPtr.Zero;
        }
    }

    // GTK3 P/Invoke declarations

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_fixed_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_put(IntPtr @fixed, IntPtr widget, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_move(IntPtr @fixed, IntPtr widget, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_frame_new(string? label);

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
