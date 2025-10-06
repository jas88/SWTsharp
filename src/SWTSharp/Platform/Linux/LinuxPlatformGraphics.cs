using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux Graphics Platform Implementation using Cairo
/// </summary>
internal partial class LinuxPlatform : IPlatformGraphics
{
    private const string CairoLib = "libcairo.so.2";
    private const string PangoLib = "libpango-1.0.so.0";
    private const string PangoCairoLib = "libpangocairo-1.0.so.0";
    private const string GdkPixbufLib = "libgdk_pixbuf-2.0.so.0";

    // Cairo imports
    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr cairo_create(IntPtr target);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_destroy(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_source_rgb(IntPtr cr, double red, double green, double blue);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_source_rgba(IntPtr cr, double red, double green, double blue, double alpha);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_line_width(IntPtr cr, double width);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_move_to(IntPtr cr, double x, double y);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_line_to(IntPtr cr, double x, double y);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_rectangle(IntPtr cr, double x, double y, double width, double height);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_arc(IntPtr cr, double xc, double yc, double radius, double angle1, double angle2);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_curve_to(IntPtr cr, double x1, double y1, double x2, double y2, double x3, double y3);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_stroke(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_fill(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_stroke_preserve(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_fill_preserve(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_dash(IntPtr cr, double[] dashes, int num_dashes, double offset);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_save(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_restore(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_clip(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_reset_clip(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_show_text(IntPtr cr, string utf8);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_text_extents(IntPtr cr, string utf8, IntPtr extents);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_select_font_face(IntPtr cr, string family, int slant, int weight);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_font_size(IntPtr cr, double size);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr cairo_image_surface_create(int format, int width, int height);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_surface_destroy(IntPtr surface);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_set_source_surface(IntPtr cr, IntPtr surface, double x, double y);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_paint(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_scale(IntPtr cr, double sx, double sy);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_close_path(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_new_path(IntPtr cr);

    // Pango imports for text rendering
    [DllImport(PangoCairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr pango_cairo_create_layout(IntPtr cr);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void pango_layout_set_text(IntPtr layout, string text, int length);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_layout_get_pixel_size(IntPtr layout, out int width, out int height);

    [DllImport(PangoCairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_cairo_show_layout(IntPtr cr, IntPtr layout);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr pango_font_description_new();

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void pango_font_description_set_family(IntPtr desc, string family);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_font_description_set_size(IntPtr desc, int size);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_font_description_set_weight(IntPtr desc, int weight);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_font_description_set_style(IntPtr desc, int style);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_font_description_free(IntPtr desc);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_layout_set_font_description(IntPtr layout, IntPtr desc);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(IntPtr obj);

    // GdkPixbuf imports for image loading
    [DllImport(GdkPixbufLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gdk_pixbuf_new_from_file(string filename, out IntPtr error);

    [DllImport(GdkPixbufLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gdk_pixbuf_get_width(IntPtr pixbuf);

    [DllImport(GdkPixbufLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gdk_pixbuf_get_height(IntPtr pixbuf);

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gdk_cairo_set_source_pixbuf(IntPtr cr, IntPtr pixbuf, double x, double y);

    // Graphics state tracking
    private sealed class GraphicsState
    {
        public IntPtr CairoContext;
        public IntPtr Surface;
        public double ForegroundR, ForegroundG, ForegroundB;
        public double BackgroundR, BackgroundG, BackgroundB;
        public double Alpha = 1.0;
        public int LineWidth = 1;
        public int LineStyle = SWT.LINE_SOLID;
        public IntPtr FontDescription;
    }

    private readonly Dictionary<IntPtr, GraphicsState> _graphicsContexts = new Dictionary<IntPtr, GraphicsState>();
    private readonly Dictionary<IntPtr, (int R, int G, int B)> _colors = new Dictionary<IntPtr, (int, int, int)>();
    private readonly Dictionary<IntPtr, IntPtr> _fonts = new Dictionary<IntPtr, IntPtr>(); // Handle -> PangoFontDescription
    private readonly Dictionary<IntPtr, IntPtr> _images = new Dictionary<IntPtr, IntPtr>(); // Handle -> Cairo surface

    // Color operations
    public IntPtr CreateColor(int red, int green, int blue)
    {
        IntPtr handle = new IntPtr(_colors.Count + 1);
        _colors[handle] = (red, green, blue);
        return handle;
    }

    public void DestroyColor(IntPtr handle)
    {
        _colors.Remove(handle);
    }

    // Font operations
    public IntPtr CreateFont(string name, int height, int style)
    {
        IntPtr desc = pango_font_description_new();
        pango_font_description_set_family(desc, name);
        pango_font_description_set_size(desc, height * 1024); // Pango uses 1/1024th of a point

        // Set weight (bold)
        if ((style & SWT.BOLD) != 0)
        {
            pango_font_description_set_weight(desc, 700); // PANGO_WEIGHT_BOLD
        }
        else
        {
            pango_font_description_set_weight(desc, 400); // PANGO_WEIGHT_NORMAL
        }

        // Set style (italic)
        if ((style & SWT.ITALIC) != 0)
        {
            pango_font_description_set_style(desc, 2); // PANGO_STYLE_ITALIC
        }
        else
        {
            pango_font_description_set_style(desc, 0); // PANGO_STYLE_NORMAL
        }

        IntPtr handle = new IntPtr(_fonts.Count + 1);
        _fonts[handle] = desc;
        return handle;
    }

    public void DestroyFont(IntPtr handle)
    {
        if (_fonts.TryGetValue(handle, out IntPtr desc))
        {
            pango_font_description_free(desc);
            _fonts.Remove(handle);
        }
    }

    // Image operations
    public IntPtr CreateImage(int width, int height)
    {
        // CAIRO_FORMAT_ARGB32 = 0
        IntPtr surface = cairo_image_surface_create(0, width, height);
        IntPtr handle = new IntPtr(_images.Count + 1);
        _images[handle] = surface;
        return handle;
    }

    public (IntPtr Handle, int Width, int Height) LoadImage(string filename)
    {
        IntPtr error = IntPtr.Zero;
        IntPtr pixbuf = gdk_pixbuf_new_from_file(filename, out error);

        if (pixbuf == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to load image: {filename}");
        }

        int width = gdk_pixbuf_get_width(pixbuf);
        int height = gdk_pixbuf_get_height(pixbuf);

        // Create Cairo surface and draw pixbuf to it
        IntPtr surface = cairo_image_surface_create(0, width, height);
        IntPtr cr = cairo_create(surface);
        gdk_cairo_set_source_pixbuf(cr, pixbuf, 0, 0);
        cairo_paint(cr);
        cairo_destroy(cr);

        // Free pixbuf
        g_object_unref(pixbuf);

        IntPtr handle = new IntPtr(_images.Count + 1);
        _images[handle] = surface;

        return (handle, width, height);
    }

    public void DestroyImage(IntPtr handle)
    {
        if (_images.TryGetValue(handle, out IntPtr surface))
        {
            cairo_surface_destroy(surface);
            _images.Remove(handle);
        }
    }

    // Graphics Context operations
    public IntPtr CreateGraphicsContext(IntPtr drawable)
    {
        // Get GdkWindow from widget
        IntPtr gdkWindow = gtk_widget_get_window(drawable);
        if (gdkWindow == IntPtr.Zero)
        {
            throw new InvalidOperationException("Cannot get GdkWindow from widget");
        }

        // Create Cairo context from GdkWindow
        IntPtr cr = gdk_cairo_create(gdkWindow);

        var state = new GraphicsState
        {
            CairoContext = cr,
            Surface = IntPtr.Zero,
            ForegroundR = 0, ForegroundG = 0, ForegroundB = 0,
            BackgroundR = 1, BackgroundG = 1, BackgroundB = 1
        };

        IntPtr handle = new IntPtr(_graphicsContexts.Count + 1);
        _graphicsContexts[handle] = state;
        return handle;
    }

    public IntPtr CreateGraphicsContextForImage(IntPtr imageHandle)
    {
        if (!_images.TryGetValue(imageHandle, out IntPtr surface))
        {
            throw new ArgumentException("Invalid image handle", nameof(imageHandle));
        }

        IntPtr cr = cairo_create(surface);

        var state = new GraphicsState
        {
            CairoContext = cr,
            Surface = surface,
            ForegroundR = 0, ForegroundG = 0, ForegroundB = 0,
            BackgroundR = 1, BackgroundG = 1, BackgroundB = 1
        };

        IntPtr handle = new IntPtr(_graphicsContexts.Count + 1);
        _graphicsContexts[handle] = state;
        return handle;
    }

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gdk_cairo_create(IntPtr window);

    public void DestroyGraphicsContext(IntPtr gcHandle)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            if (state.CairoContext != IntPtr.Zero)
            {
                cairo_destroy(state.CairoContext);
            }
            _graphicsContexts.Remove(gcHandle);
        }
    }

    // GC state operations
    public void SetGCForeground(IntPtr gcHandle, IntPtr colorHandle)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        if (_colors.TryGetValue(colorHandle, out var rgb))
        {
            state.ForegroundR = rgb.R / 255.0;
            state.ForegroundG = rgb.G / 255.0;
            state.ForegroundB = rgb.B / 255.0;
        }
    }

    public void SetGCBackground(IntPtr gcHandle, IntPtr colorHandle)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        if (_colors.TryGetValue(colorHandle, out var rgb))
        {
            state.BackgroundR = rgb.R / 255.0;
            state.BackgroundG = rgb.G / 255.0;
            state.BackgroundB = rgb.B / 255.0;
        }
    }

    public void SetGCFont(IntPtr gcHandle, IntPtr fontHandle)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        if (_fonts.TryGetValue(fontHandle, out IntPtr desc))
        {
            state.FontDescription = desc;
        }
    }

    public void SetGCLineWidth(IntPtr gcHandle, int width)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        state.LineWidth = width;
        cairo_set_line_width(state.CairoContext, width);
    }

    public void SetGCLineStyle(IntPtr gcHandle, int style)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        state.LineStyle = style;

        double[] dashes;
        switch (style)
        {
            case SWT.LINE_DASH:
                dashes = new double[] { 8, 4 };
                cairo_set_dash(state.CairoContext, dashes, dashes.Length, 0);
                break;
            case SWT.LINE_DOT:
                dashes = new double[] { 2, 2 };
                cairo_set_dash(state.CairoContext, dashes, dashes.Length, 0);
                break;
            case SWT.LINE_DASHDOT:
                dashes = new double[] { 8, 4, 2, 4 };
                cairo_set_dash(state.CairoContext, dashes, dashes.Length, 0);
                break;
            case SWT.LINE_DASHDOTDOT:
                dashes = new double[] { 8, 4, 2, 4, 2, 4 };
                cairo_set_dash(state.CairoContext, dashes, dashes.Length, 0);
                break;
            case SWT.LINE_SOLID:
            default:
                cairo_set_dash(state.CairoContext, null!, 0, 0);
                break;
        }
    }

    public void SetGCAlpha(IntPtr gcHandle, int alpha)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        state.Alpha = alpha / 255.0;
    }

    public void SetGCClipping(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        cairo_reset_clip(state.CairoContext);
        cairo_rectangle(state.CairoContext, x, y, width, height);
        cairo_clip(state.CairoContext);
    }

    public void ResetGCClipping(IntPtr gcHandle)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        cairo_reset_clip(state.CairoContext);
    }

    // Drawing operations
    private void ApplyForegroundColor(GraphicsState state)
    {
        cairo_set_source_rgba(state.CairoContext, state.ForegroundR, state.ForegroundG, state.ForegroundB, state.Alpha);
    }

    public void DrawLine(IntPtr gcHandle, int x1, int y1, int x2, int y2)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        cairo_move_to(state.CairoContext, x1 + 0.5, y1 + 0.5);
        cairo_line_to(state.CairoContext, x2 + 0.5, y2 + 0.5);
        cairo_stroke(state.CairoContext);
    }

    public void DrawRectangle(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        cairo_rectangle(state.CairoContext, x + 0.5, y + 0.5, width, height);
        cairo_stroke(state.CairoContext);
    }

    public void FillRectangle(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        cairo_rectangle(state.CairoContext, x, y, width, height);
        cairo_fill(state.CairoContext);
    }

    public void DrawOval(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        cairo_save(state.CairoContext);
        cairo_move_to(state.CairoContext, x + width / 2.0, y + height / 2.0);
        cairo_scale(state.CairoContext, width / 2.0, height / 2.0);
        cairo_arc(state.CairoContext, (x + width / 2.0) / (width / 2.0), (y + height / 2.0) / (height / 2.0), 1, 0, 2 * Math.PI);
        cairo_restore(state.CairoContext);
        cairo_stroke(state.CairoContext);
    }

    public void FillOval(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        cairo_save(state.CairoContext);
        cairo_move_to(state.CairoContext, x + width / 2.0, y + height / 2.0);
        cairo_scale(state.CairoContext, width / 2.0, height / 2.0);
        cairo_arc(state.CairoContext, (x + width / 2.0) / (width / 2.0), (y + height / 2.0) / (height / 2.0), 1, 0, 2 * Math.PI);
        cairo_restore(state.CairoContext);
        cairo_fill(state.CairoContext);
    }

    public void DrawPolygon(IntPtr gcHandle, int[] pointArray)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state) || pointArray.Length < 4)
            return;

        ApplyForegroundColor(state);
        cairo_new_path(state.CairoContext);
        cairo_move_to(state.CairoContext, pointArray[0] + 0.5, pointArray[1] + 0.5);

        for (int i = 2; i < pointArray.Length; i += 2)
        {
            cairo_line_to(state.CairoContext, pointArray[i] + 0.5, pointArray[i + 1] + 0.5);
        }

        cairo_close_path(state.CairoContext);
        cairo_stroke(state.CairoContext);
    }

    public void FillPolygon(IntPtr gcHandle, int[] pointArray)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state) || pointArray.Length < 4)
            return;

        ApplyForegroundColor(state);
        cairo_new_path(state.CairoContext);
        cairo_move_to(state.CairoContext, pointArray[0], pointArray[1]);

        for (int i = 2; i < pointArray.Length; i += 2)
        {
            cairo_line_to(state.CairoContext, pointArray[i], pointArray[i + 1]);
        }

        cairo_close_path(state.CairoContext);
        cairo_fill(state.CairoContext);
    }

    public void DrawPolyline(IntPtr gcHandle, int[] pointArray)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state) || pointArray.Length < 4)
            return;

        ApplyForegroundColor(state);
        cairo_new_path(state.CairoContext);
        cairo_move_to(state.CairoContext, pointArray[0] + 0.5, pointArray[1] + 0.5);

        for (int i = 2; i < pointArray.Length; i += 2)
        {
            cairo_line_to(state.CairoContext, pointArray[i] + 0.5, pointArray[i + 1] + 0.5);
        }

        cairo_stroke(state.CairoContext);
    }

    public void DrawArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        double centerX = x + width / 2.0;
        double centerY = y + height / 2.0;
        double radiusX = width / 2.0;
        double radiusY = height / 2.0;

        double start = -startAngle * Math.PI / 180.0;
        double end = -(startAngle + arcAngle) * Math.PI / 180.0;

        cairo_save(state.CairoContext);
        cairo_translate(state.CairoContext, centerX, centerY);
        cairo_scale(state.CairoContext, radiusX, radiusY);

        if (arcAngle >= 0)
            cairo_arc(state.CairoContext, 0, 0, 1, start, end);
        else
            cairo_arc(state.CairoContext, 0, 0, 1, end, start);

        cairo_restore(state.CairoContext);
        cairo_stroke(state.CairoContext);
    }

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_translate(IntPtr cr, double tx, double ty);

    public void FillArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        double centerX = x + width / 2.0;
        double centerY = y + height / 2.0;
        double radiusX = width / 2.0;
        double radiusY = height / 2.0;

        double start = -startAngle * Math.PI / 180.0;
        double end = -(startAngle + arcAngle) * Math.PI / 180.0;

        cairo_save(state.CairoContext);
        cairo_translate(state.CairoContext, centerX, centerY);
        cairo_scale(state.CairoContext, radiusX, radiusY);
        cairo_move_to(state.CairoContext, 0, 0);

        if (arcAngle >= 0)
            cairo_arc(state.CairoContext, 0, 0, 1, start, end);
        else
            cairo_arc(state.CairoContext, 0, 0, 1, end, start);

        cairo_close_path(state.CairoContext);
        cairo_restore(state.CairoContext);
        cairo_fill(state.CairoContext);
    }

    public void DrawRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        double rx = arcWidth / 2.0;
        double ry = arcHeight / 2.0;

        cairo_new_path(state.CairoContext);
        cairo_move_to(state.CairoContext, x + rx, y);
        cairo_line_to(state.CairoContext, x + width - rx, y);
        cairo_arc(state.CairoContext, x + width - rx, y + ry, rx, -Math.PI / 2, 0);
        cairo_line_to(state.CairoContext, x + width, y + height - ry);
        cairo_arc(state.CairoContext, x + width - rx, y + height - ry, rx, 0, Math.PI / 2);
        cairo_line_to(state.CairoContext, x + rx, y + height);
        cairo_arc(state.CairoContext, x + rx, y + height - ry, rx, Math.PI / 2, Math.PI);
        cairo_line_to(state.CairoContext, x, y + ry);
        cairo_arc(state.CairoContext, x + rx, y + ry, rx, Math.PI, 3 * Math.PI / 2);
        cairo_close_path(state.CairoContext);
        cairo_stroke(state.CairoContext);
    }

    public void FillRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);
        double rx = arcWidth / 2.0;
        double ry = arcHeight / 2.0;

        cairo_new_path(state.CairoContext);
        cairo_move_to(state.CairoContext, x + rx, y);
        cairo_line_to(state.CairoContext, x + width - rx, y);
        cairo_arc(state.CairoContext, x + width - rx, y + ry, rx, -Math.PI / 2, 0);
        cairo_line_to(state.CairoContext, x + width, y + height - ry);
        cairo_arc(state.CairoContext, x + width - rx, y + height - ry, rx, 0, Math.PI / 2);
        cairo_line_to(state.CairoContext, x + rx, y + height);
        cairo_arc(state.CairoContext, x + rx, y + height - ry, rx, Math.PI / 2, Math.PI);
        cairo_line_to(state.CairoContext, x, y + ry);
        cairo_arc(state.CairoContext, x + rx, y + ry, rx, Math.PI, 3 * Math.PI / 2);
        cairo_close_path(state.CairoContext);
        cairo_fill(state.CairoContext);
    }

    // Text operations
    public void DrawText(IntPtr gcHandle, string text, int x, int y, bool isTransparent)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        ApplyForegroundColor(state);

        // Use Pango for better text rendering
        IntPtr layout = pango_cairo_create_layout(state.CairoContext);

        if (state.FontDescription != IntPtr.Zero)
        {
            pango_layout_set_font_description(layout, state.FontDescription);
        }

        pango_layout_set_text(layout, text, -1);
        cairo_move_to(state.CairoContext, x, y);
        pango_cairo_show_layout(state.CairoContext, layout);

        g_object_unref(layout);
    }

    public (int Width, int Height) GetTextExtent(IntPtr gcHandle, string text)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return (0, 0);

        IntPtr layout = pango_cairo_create_layout(state.CairoContext);

        if (state.FontDescription != IntPtr.Zero)
        {
            pango_layout_set_font_description(layout, state.FontDescription);
        }

        pango_layout_set_text(layout, text, -1);
        pango_layout_get_pixel_size(layout, out int width, out int height);

        g_object_unref(layout);

        return (width, height);
    }

    public int GetCharWidth(IntPtr gcHandle, char ch)
    {
        return GetTextExtent(gcHandle, ch.ToString()).Width;
    }

    // Image operations
    public void DrawImage(IntPtr gcHandle, IntPtr imageHandle, int x, int y)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        if (!_images.TryGetValue(imageHandle, out IntPtr surface))
            return;

        cairo_set_source_surface(state.CairoContext, surface, x, y);
        cairo_paint(state.CairoContext);
    }

    public void DrawImageScaled(IntPtr gcHandle, IntPtr imageHandle,
        int srcX, int srcY, int srcWidth, int srcHeight,
        int destX, int destY, int destWidth, int destHeight)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        if (!_images.TryGetValue(imageHandle, out IntPtr surface))
            return;

        cairo_save(state.CairoContext);

        // Translate to destination position
        cairo_translate(state.CairoContext, destX, destY);

        // Scale to destination size
        double scaleX = (double)destWidth / srcWidth;
        double scaleY = (double)destHeight / srcHeight;
        cairo_scale(state.CairoContext, scaleX, scaleY);

        // Set source with offset for source rectangle
        cairo_set_source_surface(state.CairoContext, surface, -srcX, -srcY);

        // Set clipping to source rectangle
        cairo_rectangle(state.CairoContext, 0, 0, srcWidth, srcHeight);
        cairo_clip(state.CairoContext);

        cairo_paint(state.CairoContext);
        cairo_restore(state.CairoContext);
    }

    public void CopyArea(IntPtr gcHandle, int srcX, int srcY, int width, int height, int destX, int destY)
    {
        if (!_graphicsContexts.TryGetValue(gcHandle, out var state))
            return;

        // Create a temporary surface to copy the area
        IntPtr tempSurface = cairo_image_surface_create(0, width, height);
        IntPtr tempCr = cairo_create(tempSurface);

        // Copy source area to temporary surface
        cairo_set_source_surface(tempCr, state.Surface, -srcX, -srcY);
        cairo_rectangle(tempCr, 0, 0, width, height);
        cairo_fill(tempCr);
        cairo_destroy(tempCr);

        // Draw temporary surface to destination
        cairo_set_source_surface(state.CairoContext, tempSurface, destX, destY);
        cairo_paint(state.CairoContext);

        cairo_surface_destroy(tempSurface);
    }
}
