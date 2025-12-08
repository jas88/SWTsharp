using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using System.Collections.Concurrent;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a sash (resizable divider) platform widget.
/// Uses GtkEventBox with custom drag handling for a draggable divider.
/// </summary>
internal class LinuxSash : LinuxWidget, IPlatformSash
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";
    private const string GdkLib = "libgdk-3.so.0";

    private IntPtr _sashHandle;
    private bool _disposed;
    private readonly int _style;
    private readonly bool _isVertical;
    private int _position;
    private bool _isDragging;
    private int _dragStartX;
    private int _dragStartY;
    private Rectangle _requestedBounds;

    // Static mapping of sash handles to instances for callback routing
    private static readonly ConcurrentDictionary<IntPtr, LinuxSash> _sashInstances = new();

    // GSignal callback delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool GtkButtonPressFunc(IntPtr widget, IntPtr eventPtr, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool GtkButtonReleaseFunc(IntPtr widget, IntPtr eventPtr, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool GtkMotionNotifyFunc(IntPtr widget, IntPtr eventPtr, IntPtr data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void GtkRealizeFunc(IntPtr widget, IntPtr data);

    private readonly GtkButtonPressFunc _buttonPressCallback;
    private readonly GtkButtonReleaseFunc _buttonReleaseCallback;
    private readonly GtkMotionNotifyFunc _motionNotifyCallback;
    private readonly GtkRealizeFunc _realizeCallback;

    // Event handling
    public event EventHandler<int>? PositionChanged;
#pragma warning disable CS0067 // Events are used via interface
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxSash(IntPtr parentHandle, int style)
    {
        _style = style;
        _isVertical = (style & SWT.VERTICAL) != 0;

        // Create callbacks
        _buttonPressCallback = OnButtonPress;
        _buttonReleaseCallback = OnButtonRelease;
        _motionNotifyCallback = OnMotionNotify;
        _realizeCallback = OnRealize;

        // Create GtkEventBox for event handling
        _sashHandle = gtk_event_box_new();
        if (_sashHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK event box for sash");
        }

        // Create a drawing area for visual representation
        IntPtr drawingArea = gtk_drawing_area_new();
        gtk_container_add(_sashHandle, drawingArea);

        // Set default size based on orientation
        int width = _isVertical ? 100 : 5;
        int height = _isVertical ? 5 : 100;
        gtk_widget_set_size_request(_sashHandle, width, height);
        _requestedBounds = new Rectangle(0, 0, width, height);

        // Note: SetCursor is called in OnRealize callback after widget is realized

        // Enable events on the event box
        gtk_widget_add_events(_sashHandle,
            GdkEventMask.GDK_BUTTON_PRESS_MASK |
            GdkEventMask.GDK_BUTTON_RELEASE_MASK |
            GdkEventMask.GDK_POINTER_MOTION_MASK);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _sashHandle);
        }

        // Show the widget and its child
        gtk_widget_show_all(_sashHandle);

        // Setup event handlers
        SetupEventHandlers();

        // Connect to 'realize' signal to set cursor after widget is realized
        g_signal_connect_data(
            _sashHandle,
            "realize",
            Marshal.GetFunctionPointerForDelegate(_realizeCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );
    }

    public void SetPosition(int position)
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;
        _position = position;
    }

    public int GetPosition()
    {
        return _position;
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;

        _requestedBounds = new Rectangle(x, y, width, height);
        gtk_widget_set_size_request(_sashHandle, width, height);
        // Position is typically controlled by parent layout in GTK
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return default;

        // Return requested bounds since GTK allocation may not reflect size request
        // until widget is realized and laid out
        return _requestedBounds;
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_sashHandle);
        else
            gtk_widget_hide(_sashHandle);
    }

    public bool GetVisible()
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_sashHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_sashHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_sashHandle);
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;
        // GTK3 theming - would need CSS provider for custom colors
    }

    public RGB GetBackground()
    {
        return new RGB(200, 200, 200); // Default gray
    }

    public void SetForeground(RGB color)
    {
        // Not applicable for sash
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_sashHandle != IntPtr.Zero)
            {
                // Remove from instance mapping
                _sashInstances.TryRemove(_sashHandle, out _);

                // Destroy widget
                gtk_widget_destroy(_sashHandle);
                _sashHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _sashHandle;
    }

    private void SetCursor()
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;

        // Get the GdkWindow from the widget
        IntPtr gdkWindow = gtk_widget_get_window(_sashHandle);
        if (gdkWindow == IntPtr.Zero) return;

        // Get display
        IntPtr display = gdk_window_get_display(gdkWindow);
        if (display == IntPtr.Zero) return;

        // Create cursor - use appropriate resize cursor based on orientation
        string cursorName = _isVertical ? "ns-resize" : "ew-resize";
        IntPtr cursor = gdk_cursor_new_from_name(display, cursorName);

        if (cursor != IntPtr.Zero)
        {
            gdk_window_set_cursor(gdkWindow, cursor);
            g_object_unref(cursor);
        }
    }

    private void OnRealize(IntPtr widget, IntPtr data)
    {
        // Called when widget is realized and has a GdkWindow
        SetCursor();
    }

    private void SetupEventHandlers()
    {
        if (_disposed || _sashHandle == IntPtr.Zero) return;

        // Register this sash instance for callback routing
        _sashInstances[_sashHandle] = this;

        // Connect button press signal
        g_signal_connect_data(
            _sashHandle,
            "button-press-event",
            Marshal.GetFunctionPointerForDelegate(_buttonPressCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );

        // Connect button release signal
        g_signal_connect_data(
            _sashHandle,
            "button-release-event",
            Marshal.GetFunctionPointerForDelegate(_buttonReleaseCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );

        // Connect motion notify signal
        g_signal_connect_data(
            _sashHandle,
            "motion-notify-event",
            Marshal.GetFunctionPointerForDelegate(_motionNotifyCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );
    }

    private bool OnButtonPress(IntPtr widget, IntPtr eventPtr, IntPtr data)
    {
        if (_disposed || !_sashInstances.TryGetValue(widget, out var sash) || sash != this)
            return false;

        // Read event structure
        GdkEventButton eventButton = Marshal.PtrToStructure<GdkEventButton>(eventPtr);

        if (eventButton.button == 1) // Left mouse button
        {
            _isDragging = true;
            _dragStartX = (int)eventButton.x;
            _dragStartY = (int)eventButton.y;
            return true; // Event handled
        }

        return false;
    }

    private bool OnButtonRelease(IntPtr widget, IntPtr eventPtr, IntPtr data)
    {
        if (_disposed || !_sashInstances.TryGetValue(widget, out var sash) || sash != this)
            return false;

        GdkEventButton eventButton = Marshal.PtrToStructure<GdkEventButton>(eventPtr);

        if (eventButton.button == 1 && _isDragging)
        {
            _isDragging = false;
            return true;
        }

        return false;
    }

    private bool OnMotionNotify(IntPtr widget, IntPtr eventPtr, IntPtr data)
    {
        if (_disposed || !_sashInstances.TryGetValue(widget, out var sash) || sash != this)
            return false;

        if (!_isDragging)
            return false;

        GdkEventMotion eventMotion = Marshal.PtrToStructure<GdkEventMotion>(eventPtr);

        int currentX = (int)eventMotion.x;
        int currentY = (int)eventMotion.y;

        int delta = _isVertical ? (currentY - _dragStartY) : (currentX - _dragStartX);

        if (delta != 0)
        {
            _position += delta;
            _dragStartX = currentX;
            _dragStartY = currentY;

            // Fire position changed event
            PositionChanged?.Invoke(this, _position);
        }

        return true;
    }

    // GTK3 P/Invoke declarations
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_event_box_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_drawing_area_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_add_events(IntPtr widget, GdkEventMask events);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show_all(IntPtr widget);

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

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_window(IntPtr widget);

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gdk_window_get_display(IntPtr window);

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gdk_cursor_new_from_name(IntPtr display, string name);

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gdk_window_set_cursor(IntPtr window, IntPtr cursor);

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(IntPtr obj);

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

    [StructLayout(LayoutKind.Sequential)]
    private struct GdkEventButton
    {
        public GdkEventType type;
        public IntPtr window;
        public byte send_event;
        public uint time;
        public double x;
        public double y;
        public IntPtr axes;
        public uint state;
        public uint button;
        public IntPtr device;
        public double x_root;
        public double y_root;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GdkEventMotion
    {
        public GdkEventType type;
        public IntPtr window;
        public byte send_event;
        public uint time;
        public double x;
        public double y;
        public IntPtr axes;
        public uint state;
        public short is_hint;
        public IntPtr device;
        public double x_root;
        public double y_root;
    }

    private enum GdkEventType
    {
        GDK_NOTHING = -1,
        GDK_DELETE = 0,
        GDK_DESTROY = 1,
        GDK_EXPOSE = 2,
        GDK_MOTION_NOTIFY = 3,
        GDK_BUTTON_PRESS = 4,
        GDK_2BUTTON_PRESS = 5,
        GDK_3BUTTON_PRESS = 6,
        GDK_BUTTON_RELEASE = 7
    }

    [Flags]
    private enum GdkEventMask
    {
        GDK_EXPOSURE_MASK = 1 << 1,
        GDK_POINTER_MOTION_MASK = 1 << 2,
        GDK_BUTTON_PRESS_MASK = 1 << 8,
        GDK_BUTTON_RELEASE_MASK = 1 << 9,
        GDK_KEY_PRESS_MASK = 1 << 10,
        GDK_KEY_RELEASE_MASK = 1 << 11
    }
}
