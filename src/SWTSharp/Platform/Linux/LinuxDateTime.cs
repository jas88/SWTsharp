using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux GTK implementation of DateTime widget using GtkCalendar.
/// </summary>
internal class LinuxDateTime : IPlatformDateTime
{
    private IntPtr _widget;
    private IntPtr _calendar;
    private bool _disposed;
    private int _year, _month, _day;
    private int _hours, _minutes, _seconds;
    private readonly int _style;

    // Event handling
    public event EventHandler? SelectionChanged;
#pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxDateTime(IntPtr parentWidget, int style)
    {
        _style = style;

        // Initialize with current date/time
        var now = System.DateTime.Now;
        _year = now.Year;
        _month = now.Month - 1;
        _day = now.Day;
        _hours = now.Hour;
        _minutes = now.Minute;
        _seconds = now.Second;

        if ((style & SWT.CALENDAR) != 0)
        {
            // Create GtkCalendar for calendar view
            _calendar = gtk_calendar_new();
            _widget = _calendar;
        }
        else if ((style & SWT.TIME) != 0)
        {
            // Create horizontal box for time spinners
            _widget = gtk_box_new(0, 5); // GTK_ORIENTATION_HORIZONTAL = 0, spacing = 5

            // TODO: Add hour, minute, second spin buttons
            // For now, use a simple entry
            var entry = gtk_entry_new();
            gtk_box_pack_start(_widget, entry, true, true, 0);
        }
        else // DATE
        {
            // Create horizontal box for date spinners or entry
            _widget = gtk_box_new(0, 5);

            // For DATE mode, we could use GtkCalendar in a popup or spin buttons
            // For simplicity, use entry with calendar button
            var entry = gtk_entry_new();
            gtk_box_pack_start(_widget, entry, true, true, 0);
        }

        if (_widget == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK DateTime widget");
        }

        gtk_widget_show_all(_widget);

        // Set initial date on calendar if applicable
        if (_calendar != IntPtr.Zero)
        {
            gtk_calendar_select_month(_calendar, (uint)_month, (uint)_year);
            gtk_calendar_select_day(_calendar, (uint)_day);
        }
    }

    public void SetDate(int year, int month, int day)
    {
        if (_disposed) return;

        _year = year;
        _month = month;
        _day = day;

        if (_calendar != IntPtr.Zero)
        {
            gtk_calendar_select_month(_calendar, (uint)month, (uint)year);
            gtk_calendar_select_day(_calendar, (uint)day);
        }
    }

    public (int Year, int Month, int Day) GetDate()
    {
        if (_disposed) return (_year, _month, _day);

        if (_calendar != IntPtr.Zero)
        {
            uint year = 0, month = 0, day = 0;
            gtk_calendar_get_date(_calendar, out year, out month, out day);

            _year = (int)year;
            _month = (int)month;
            _day = (int)day;
        }

        return (_year, _month, _day);
    }

    public void SetTime(int hours, int minutes, int seconds)
    {
        if (_disposed) return;

        _hours = hours;
        _minutes = minutes;
        _seconds = seconds;

        // Update time spinners if applicable
    }

    public (int Hours, int Minutes, int Seconds) GetTime()
    {
        if (_disposed) return (_hours, _minutes, _seconds);

        // Get time from spinners if applicable

        return (_hours, _minutes, _seconds);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _widget == IntPtr.Zero) return;

        gtk_widget_set_size_request(_widget, width, height);

        if (gtk_widget_get_parent(_widget) != IntPtr.Zero)
        {
            // Position is managed by parent container in GTK
        }
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _widget == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_widget, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _widget == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_widget);
        else
            gtk_widget_hide(_widget);
    }

    public bool GetVisible()
    {
        if (_disposed || _widget == IntPtr.Zero) return false;

        return gtk_widget_get_visible(_widget);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _widget == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_widget, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _widget == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_widget);
    }

    public void SetBackground(RGB color)
    {
        // GTK3/4 styling handled via CSS
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255);
    }

    public void SetForeground(RGB color)
    {
        // GTK3/4 styling handled via CSS
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0);
    }

    internal IntPtr GetNativeHandle()
    {
        return _widget;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_widget != IntPtr.Zero)
            {
                gtk_widget_destroy(_widget);
                _widget = IntPtr.Zero;
            }
            _calendar = IntPtr.Zero;
            _disposed = true;
        }
    }

    // GTK P/Invoke declarations

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_calendar_new();

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_calendar_select_month(IntPtr calendar, uint month, uint year);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_calendar_select_day(IntPtr calendar, uint day);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_calendar_get_date(IntPtr calendar, out uint year, out uint month, out uint day);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_box_new(int orientation, int spacing);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_box_pack_start(IntPtr box, IntPtr child, bool expand, bool fill, uint padding);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_entry_new();

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show_all(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_parent(IntPtr widget);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
