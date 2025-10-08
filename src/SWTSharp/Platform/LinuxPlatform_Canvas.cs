using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Canvas widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK Drawing Area widget imports for Canvas
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_drawing_area_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_queue_draw(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_queue_draw_area(IntPtr widget, int x, int y, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_override_background_color(IntPtr widget, int state, ref GdkRGBA color);

    // GdkRGBA struct for color representation
    [StructLayout(LayoutKind.Sequential)]
    private struct GdkRGBA
    {
        public double Red;
        public double Green;
        public double Blue;
        public double Alpha;

        public GdkRGBA(Graphics.RGB rgb)
        {
            Red = rgb.Red / 255.0;
            Green = rgb.Green / 255.0;
            Blue = rgb.Blue / 255.0;
            Alpha = 1.0;
        }
    }

    // Canvas data storage
    private sealed class CanvasData
    {
        public GdkRGBA BackgroundColor { get; set; }
        public Action<int, int, int, int, object?>? PaintCallback { get; set; }
    }

    // Track canvas widgets and their data
    private readonly Dictionary<IntPtr, CanvasData> _canvasData = new Dictionary<IntPtr, CanvasData>();
    private readonly Dictionary<GtkSignalFunc, object> _canvasDelegateReferences = new Dictionary<GtkSignalFunc, object>();

    // Canvas operations
    public IntPtr CreateCanvas(IntPtr parent, int style)
    {
        // Create GtkDrawingArea widget
        IntPtr canvas = gtk_drawing_area_new();

        if (canvas == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK drawing area");
        }

        // Initialize canvas data with default white background
        var backgroundColor = new GdkRGBA(new Graphics.RGB(255, 255, 255));
        var canvasData = new CanvasData
        {
            BackgroundColor = backgroundColor
        };
        _canvasData[canvas] = canvasData;

        // Set default background color
        gtk_widget_override_background_color(canvas, 0, ref backgroundColor);

        // Connect "draw" signal for paint events
        GtkSignalFunc drawHandler = (widget, data) =>
        {
            if (_canvasData.TryGetValue(widget, out var canvasInfo) && canvasInfo.PaintCallback != null)
            {
                // Get widget dimensions for full redraw
                // For now, pass dummy values - proper implementation would query widget allocation
                canvasInfo.PaintCallback(0, 0, 0, 0, data);
            }
        };

        // Store delegate reference to prevent GC collection
        _canvasDelegateReferences[drawHandler] = canvas;

        // Connect the "draw" signal
        IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(drawHandler);
        g_signal_connect_data(canvas, "draw", funcPtr, IntPtr.Zero, IntPtr.Zero, 0);

        // Add to parent if provided (use helper to handle GtkWindow containers)
        if (parent != IntPtr.Zero)
        {
            AddChildToParent(parent, canvas);
        }

        gtk_widget_show(canvas);

        return canvas;
    }

    public void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid canvas handle", nameof(handle));

        if (!_canvasData.TryGetValue(handle, out var canvasData))
        {
            throw new ArgumentException("Canvas handle not found", nameof(handle));
        }

        // Store the paint callback
        canvasData.PaintCallback = paintCallback;
    }

    public void RedrawCanvas(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid canvas handle", nameof(handle));

        // Queue a full redraw of the entire widget
        gtk_widget_queue_draw(handle);
    }

    public void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid canvas handle", nameof(handle));

        // Queue a partial redraw of the specified area
        gtk_widget_queue_draw_area(handle, x, y, width, height);
    }
}
