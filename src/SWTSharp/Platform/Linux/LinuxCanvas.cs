using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of Canvas widget using GtkDrawingArea with Cairo drawing context.
/// A Canvas is a drawable composite widget that supports Cairo 2D graphics operations.
/// </summary>
internal class LinuxCanvas : LinuxWidget, IPlatformComposite
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";

    private IntPtr _gtkDrawingAreaHandle;
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private ulong _drawSignalId;
    private GtkDrawCallback? _drawCallback; // Keep reference to prevent GC

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // Delegate for GTK "draw" signal callback
    private delegate bool GtkDrawCallback(IntPtr widget, IntPtr cr, IntPtr userData);

    public LinuxCanvas(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxCanvas] Creating canvas. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create GtkDrawingArea - a widget for custom drawing
        _gtkDrawingAreaHandle = gtk_drawing_area_new();

        if (_gtkDrawingAreaHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK drawing area");
        }

        // Set default size
        gtk_widget_set_size_request(_gtkDrawingAreaHandle, 100, 100);

        // Add border if requested
        if ((style & SWT.BORDER) != 0)
        {
            // Create a GtkFrame to provide border
            IntPtr frame = gtk_frame_new(null);
            gtk_container_add(frame, _gtkDrawingAreaHandle);

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
                gtk_container_add(parentHandle, _gtkDrawingAreaHandle);
            }
        }

        // Connect to "draw" signal for Cairo drawing
        // The draw signal provides a Cairo context for custom rendering
        _drawCallback = OnDraw;
        _drawSignalId = g_signal_connect_data(
            _gtkDrawingAreaHandle,
            "draw",
            Marshal.GetFunctionPointerForDelegate(_drawCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0);

        // Show the widget
        gtk_widget_show(_gtkDrawingAreaHandle);

        if (enableLogging)
            Console.WriteLine($"[LinuxCanvas] Canvas created successfully. Handle: 0x{_gtkDrawingAreaHandle:X}");
    }

    /// <summary>
    /// GTK "draw" signal callback - invoked when widget needs to be redrawn.
    /// Receives a Cairo context (cr) for 2D drawing operations.
    /// </summary>
    private bool OnDraw(IntPtr widget, IntPtr cr, IntPtr userData)
    {
        // Get widget allocation to know the drawing area size
        GtkAllocation allocation;
        gtk_widget_get_allocation(widget, out allocation);

        // Set background color using Cairo
        cairo_set_source_rgb(cr, _background.Red / 255.0, _background.Green / 255.0, _background.Blue / 255.0);
        cairo_paint(cr);

        // TODO: Future enhancement - expose Cairo context to application for custom drawing
        // For now, just render the background color

        return false; // Return false to propagate event
    }

    /// <summary>
    /// Forces a repaint of the canvas by queuing a draw request.
    /// </summary>
    public void Redraw()
    {
        if (_gtkDrawingAreaHandle != IntPtr.Zero)
        {
            gtk_widget_queue_draw(_gtkDrawingAreaHandle);
        }
    }

    /// <summary>
    /// Forces a repaint of a specific region of the canvas.
    /// </summary>
    public void Redraw(int x, int y, int width, int height)
    {
        if (_gtkDrawingAreaHandle != IntPtr.Zero)
        {
            gtk_widget_queue_draw_area(_gtkDrawingAreaHandle, x, y, width, height);
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _gtkDrawingAreaHandle;
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

        // Note: GtkDrawingArea is not a container, so we can't add child widgets directly
        // Child widgets would need to be overlaid using GtkOverlay or similar
        // For now, we just track them logically
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
        if (_disposed || _gtkDrawingAreaHandle == IntPtr.Zero) return;

        gtk_widget_set_size_request(_gtkDrawingAreaHandle, width, height);
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _gtkDrawingAreaHandle == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_gtkDrawingAreaHandle, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _gtkDrawingAreaHandle == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_gtkDrawingAreaHandle);
        else
            gtk_widget_hide(_gtkDrawingAreaHandle);
    }

    public bool GetVisible()
    {
        if (_disposed || _gtkDrawingAreaHandle == IntPtr.Zero) return false;

        return gtk_widget_get_visible(_gtkDrawingAreaHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _gtkDrawingAreaHandle == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_gtkDrawingAreaHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _gtkDrawingAreaHandle == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_gtkDrawingAreaHandle);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // Trigger repaint to show new background
        Redraw();
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Disconnect signal handler
        if (_drawSignalId != 0 && _gtkDrawingAreaHandle != IntPtr.Zero)
        {
            g_signal_handler_disconnect(_gtkDrawingAreaHandle, _drawSignalId);
            _drawSignalId = 0;
        }

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
        if (_gtkDrawingAreaHandle != IntPtr.Zero)
        {
            gtk_widget_destroy(_gtkDrawingAreaHandle);
            _gtkDrawingAreaHandle = IntPtr.Zero;
        }

        // Clear delegate reference
        _drawCallback = null;
    }

    #region GTK3 and Cairo P/Invoke

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_drawing_area_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_frame_new(string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

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
    private static extern void gtk_widget_queue_draw(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_queue_draw_area(IntPtr widget, int x, int y, int width, int height);

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong g_signal_connect_data(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string detailed_signal,
        IntPtr callback,
        IntPtr data,
        IntPtr destroy_data,
        int connect_flags);

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_signal_handler_disconnect(IntPtr instance, ulong handler_id);

    // Cairo drawing functions
    [DllImport("libcairo.so.2", CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_source_rgb(IntPtr cr, double red, double green, double blue);

    [DllImport("libcairo.so.2", CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_paint(IntPtr cr);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    #endregion
}
