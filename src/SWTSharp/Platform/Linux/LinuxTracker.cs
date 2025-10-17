using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux (GTK) implementation of tracker widget.
/// Uses GDK drawing with cairo for rubber-band rectangle feedback.
/// </summary>
internal class LinuxTracker : IPlatformTracker
{
    private Rectangle[] _rectangles = Array.Empty<Rectangle>();
    private bool _stippled;
    private int _cursorType;
    private readonly IntPtr _parentWidget;
    private readonly int _style;
    private bool _disposed;
    private bool _tracking;
    private IntPtr _window;
    private IntPtr _cr;

    public event EventHandler<TrackerEventArgs>? MouseMove;
    public event EventHandler<TrackerEventArgs>? Resize;

    // GTK/GDK constants
    private const int GDK_BUTTON_RELEASE_MASK = 1 << 3;
    private const int GDK_POINTER_MOTION_MASK = 1 << 2;
    private const int GDK_KEY_PRESS_MASK = 1 << 10;
    private const int GDK_OWNERSHIP_NONE = 0;

    public LinuxTracker(IntPtr parentWidget, int style)
    {
        _parentWidget = parentWidget;
        _style = style;
    }

    public bool Open()
    {
        if (_tracking || _rectangles.Length == 0)
            return false;

        _tracking = true;

        try
        {
            // Get the GDK window
            _window = _parentWidget != IntPtr.Zero
                ? gtk_widget_get_window(_parentWidget)
                : gdk_get_default_root_window();

            if (_window == IntPtr.Zero)
                return false;

            // Create cairo context for drawing
            _cr = gdk_cairo_create(_window);
            if (_cr == IntPtr.Zero)
                return false;

            // Set up cairo for XOR-like drawing
            cairo_set_operator(_cr, CAIRO_OPERATOR_DIFFERENCE);
            cairo_set_line_width(_cr, 2.0);

            if (_stippled)
            {
                double[] dashes = { 5.0, 5.0 };
                cairo_set_dash(_cr, dashes, 2, 0);
            }

            cairo_set_source_rgb(_cr, 1.0, 1.0, 1.0);

            // Draw initial rectangles
            DrawRectangles();

            // Grab pointer
            IntPtr cursor = IntPtr.Zero;
            IntPtr device = gdk_device_manager_get_client_pointer(
                gdk_display_get_device_manager(gdk_display_get_default()));

            int grabResult = gdk_device_grab(
                device,
                _window,
                GDK_OWNERSHIP_NONE,
                true,
                GDK_BUTTON_RELEASE_MASK | GDK_POINTER_MOTION_MASK | GDK_KEY_PRESS_MASK,
                cursor,
                GDK_CURRENT_TIME
            );

            if (grabResult != 0)
            {
                cairo_destroy(_cr);
                _cr = IntPtr.Zero;
                return false;
            }

            // Track events
            bool completed = TrackEvents(device);

            // Erase rectangles (draw them again with XOR)
            DrawRectangles();

            // Release grab
            gdk_device_ungrab(device, GDK_CURRENT_TIME);

            return completed;
        }
        finally
        {
            if (_cr != IntPtr.Zero)
            {
                cairo_destroy(_cr);
                _cr = IntPtr.Zero;
            }
            _tracking = false;
        }
    }

    public void Close()
    {
        _tracking = false;
    }

    public void SetRectangles(Rectangle[] rectangles)
    {
        _rectangles = rectangles ?? Array.Empty<Rectangle>();
    }

    public Rectangle[] GetRectangles()
    {
        return _rectangles;
    }

    public void SetStippled(bool stippled)
    {
        _stippled = stippled;
    }

    public bool GetStippled()
    {
        return _stippled;
    }

    public void SetCursor(int cursorType)
    {
        _cursorType = cursorType;
    }

    private void DrawRectangles()
    {
        foreach (var rect in _rectangles)
        {
            cairo_rectangle(_cr, rect.X, rect.Y, rect.Width, rect.Height);
            cairo_stroke(_cr);
        }
    }

    private bool TrackEvents(IntPtr device)
    {
        int startX = 0, startY = 0;
        gdk_device_get_position(device, IntPtr.Zero, out startX, out startY);

        Rectangle[] originalRects = (Rectangle[])_rectangles.Clone();

        while (_tracking)
        {
            // Process GTK events
            while (gtk_events_pending())
            {
                IntPtr eventPtr = gdk_event_get();
                if (eventPtr == IntPtr.Zero)
                    break;

                int eventType = gdk_event_get_event_type(eventPtr);

                if (eventType == 5) // GDK_BUTTON_RELEASE
                {
                    gdk_event_free(eventPtr);
                    return true; // Completed
                }
                else if (eventType == 9) // GDK_KEY_PRESS
                {
                    uint keyval = gdk_event_get_keyval(eventPtr);
                    if (keyval == 0xFF1B) // GDK_KEY_Escape
                    {
                        DrawRectangles(); // Erase current
                        _rectangles = originalRects;
                        DrawRectangles(); // Draw original
                        gdk_event_free(eventPtr);
                        return false; // Cancelled
                    }
                }
                else if (eventType == 3) // GDK_MOTION_NOTIFY
                {
                    int currentX, currentY;
                    gdk_device_get_position(device, IntPtr.Zero, out currentX, out currentY);

                    int dx = currentX - startX;
                    int dy = currentY - startY;

                    // Erase old rectangles
                    DrawRectangles();

                    // Update rectangles
                    for (int i = 0; i < _rectangles.Length; i++)
                    {
                        if ((_style & SWT.RESIZE) != 0)
                        {
                            _rectangles[i] = new Rectangle(
                                originalRects[i].X,
                                originalRects[i].Y,
                                originalRects[i].Width + dx,
                                originalRects[i].Height + dy
                            );

                            var args = new TrackerEventArgs
                            {
                                X = currentX,
                                Y = currentY,
                                Bounds = _rectangles[i]
                            };
                            Resize?.Invoke(this, args);
                        }
                        else
                        {
                            _rectangles[i] = new Rectangle(
                                originalRects[i].X + dx,
                                originalRects[i].Y + dy,
                                originalRects[i].Width,
                                originalRects[i].Height
                            );
                        }
                    }

                    // Draw new rectangles
                    DrawRectangles();

                    var moveArgs = new TrackerEventArgs
                    {
                        X = currentX,
                        Y = currentY,
                        Bounds = _rectangles.Length > 0 ? _rectangles[0] : new Rectangle()
                    };
                    MouseMove?.Invoke(this, moveArgs);
                }

                gdk_event_free(eventPtr);
            }

            Thread.Sleep(10); // Small delay to prevent CPU spinning
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Close();
        System.GC.SuppressFinalize(this);
    }

    // GTK/GDK/Cairo API declarations
    [DllImport("gtk-3")]
    private static extern IntPtr gtk_widget_get_window(IntPtr widget);

    [DllImport("gdk-3")]
    private static extern IntPtr gdk_get_default_root_window();

    [DllImport("gdk-3")]
    private static extern IntPtr gdk_cairo_create(IntPtr window);

    [DllImport("cairo")]
    private static extern void cairo_set_operator(IntPtr cr, int op);

    [DllImport("cairo")]
    private static extern void cairo_set_line_width(IntPtr cr, double width);

    [DllImport("cairo")]
    private static extern void cairo_set_dash(IntPtr cr, double[] dashes, int num_dashes, double offset);

    [DllImport("cairo")]
    private static extern void cairo_set_source_rgb(IntPtr cr, double r, double g, double b);

    [DllImport("cairo")]
    private static extern void cairo_rectangle(IntPtr cr, double x, double y, double width, double height);

    [DllImport("cairo")]
    private static extern void cairo_stroke(IntPtr cr);

    [DllImport("cairo")]
    private static extern void cairo_destroy(IntPtr cr);

    [DllImport("gdk-3")]
    private static extern IntPtr gdk_display_get_default();

    [DllImport("gdk-3")]
    private static extern IntPtr gdk_display_get_device_manager(IntPtr display);

    [DllImport("gdk-3")]
    private static extern IntPtr gdk_device_manager_get_client_pointer(IntPtr deviceManager);

    [DllImport("gdk-3")]
    private static extern int gdk_device_grab(IntPtr device, IntPtr window, int grab_ownership, bool owner_events, int event_mask, IntPtr cursor, uint time);

    [DllImport("gdk-3")]
    private static extern void gdk_device_ungrab(IntPtr device, uint time);

    [DllImport("gdk-3")]
    private static extern void gdk_device_get_position(IntPtr device, IntPtr screen, out int x, out int y);

    [DllImport("gtk-3")]
    private static extern bool gtk_events_pending();

    [DllImport("gdk-3")]
    private static extern IntPtr gdk_event_get();

    [DllImport("gdk-3")]
    private static extern int gdk_event_get_event_type(IntPtr eventPtr);

    [DllImport("gdk-3")]
    private static extern uint gdk_event_get_keyval(IntPtr eventPtr);

    [DllImport("gdk-3")]
    private static extern void gdk_event_free(IntPtr eventPtr);

    private const int CAIRO_OPERATOR_DIFFERENCE = 16;
    private const uint GDK_CURRENT_TIME = 0;
}
