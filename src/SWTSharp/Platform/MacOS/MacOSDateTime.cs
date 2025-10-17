using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of DateTime widget using NSDatePicker.
/// </summary>
internal class MacOSDateTime : IPlatformDateTime
{
    private IntPtr _datePicker;
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

    public MacOSDateTime(IntPtr parentView, int style)
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

        // Create NSDatePicker
        IntPtr datePickerClass = objc_getClass("NSDatePicker");
        _datePicker = objc_msgSend(datePickerClass, sel_registerName("alloc"));
        _datePicker = objc_msgSend(_datePicker, sel_registerName("init"));

        if (_datePicker == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSDatePicker");
        }

        // Set date picker style based on SWT style
        IntPtr datePickerStyle;
        IntPtr datePickerElements;

        if ((style & SWT.CALENDAR) != 0)
        {
            // NSDatePickerStyleClockAndCalendar = 1
            datePickerStyle = new IntPtr(1);
            // NSDatePickerElementFlagYearMonthDay = 0x0E
            datePickerElements = new IntPtr(0x0E);
        }
        else if ((style & SWT.TIME) != 0)
        {
            // NSDatePickerStyleTextFieldAndStepper = 0
            datePickerStyle = IntPtr.Zero;
            // NSDatePickerElementFlagHourMinuteSecond = 0x70
            datePickerElements = new IntPtr(0x70);
        }
        else // DATE
        {
            // NSDatePickerStyleTextFieldAndStepper = 0
            datePickerStyle = IntPtr.Zero;
            // NSDatePickerElementFlagYearMonthDay = 0x0E
            datePickerElements = new IntPtr(0x0E);
        }

        objc_msgSend(_datePicker, sel_registerName("setDatePickerStyle:"), datePickerStyle);
        objc_msgSend(_datePicker, sel_registerName("setDatePickerElements:"), datePickerElements);

        // Set initial date
        SetDate(_year, _month, _day);
        SetTime(_hours, _minutes, _seconds);

        // Add to parent view if present
        if (parentView != IntPtr.Zero)
        {
            objc_msgSend(parentView, sel_registerName("addSubview:"), _datePicker);
        }
    }

    public void SetDate(int year, int month, int day)
    {
        if (_disposed) return;

        _year = year;
        _month = month;
        _day = day;

        if (_datePicker != IntPtr.Zero)
        {
            // Create NSDateComponents
            IntPtr componentsClass = objc_getClass("NSDateComponents");
            IntPtr components = objc_msgSend(componentsClass, sel_registerName("alloc"));
            components = objc_msgSend(components, sel_registerName("init"));

            objc_msgSend(components, sel_registerName("setYear:"), new IntPtr(year));
            objc_msgSend(components, sel_registerName("setMonth:"), new IntPtr(month + 1)); // NSDateComponents uses 1-based months
            objc_msgSend(components, sel_registerName("setDay:"), new IntPtr(day));
            objc_msgSend(components, sel_registerName("setHour:"), new IntPtr(_hours));
            objc_msgSend(components, sel_registerName("setMinute:"), new IntPtr(_minutes));
            objc_msgSend(components, sel_registerName("setSecond:"), new IntPtr(_seconds));

            // Get NSCalendar
            IntPtr calendarClass = objc_getClass("NSCalendar");
            IntPtr calendar = objc_msgSend(calendarClass, sel_registerName("currentCalendar"));

            // Create NSDate from components
            IntPtr date = objc_msgSend(calendar, sel_registerName("dateFromComponents:"), components);

            // Set date on picker
            objc_msgSend(_datePicker, sel_registerName("setDateValue:"), date);

            objc_msgSend(components, sel_registerName("release"));
        }
    }

    public (int Year, int Month, int Day) GetDate()
    {
        if (_disposed) return (_year, _month, _day);

        if (_datePicker != IntPtr.Zero)
        {
            // Get current date
            IntPtr date = objc_msgSend(_datePicker, sel_registerName("dateValue"));

            // Get calendar
            IntPtr calendarClass = objc_getClass("NSCalendar");
            IntPtr calendar = objc_msgSend(calendarClass, sel_registerName("currentCalendar"));

            // Get components
            // NSCalendarUnitYear | NSCalendarUnitMonth | NSCalendarUnitDay = 0x0E
            IntPtr components = objc_msgSend(calendar, sel_registerName("components:fromDate:"),
                new IntPtr(0x0E), date);

            _year = (int)objc_msgSend(components, sel_registerName("year"));
            _month = (int)objc_msgSend(components, sel_registerName("month")) - 1; // Convert to 0-based
            _day = (int)objc_msgSend(components, sel_registerName("day"));
        }

        return (_year, _month, _day);
    }

    public void SetTime(int hours, int minutes, int seconds)
    {
        if (_disposed) return;

        _hours = hours;
        _minutes = minutes;
        _seconds = seconds;

        // Update the date picker with new time
        SetDate(_year, _month, _day);
    }

    public (int Hours, int Minutes, int Seconds) GetTime()
    {
        if (_disposed) return (_hours, _minutes, _seconds);

        if (_datePicker != IntPtr.Zero)
        {
            // Get current date
            IntPtr date = objc_msgSend(_datePicker, sel_registerName("dateValue"));

            // Get calendar
            IntPtr calendarClass = objc_getClass("NSCalendar");
            IntPtr calendar = objc_msgSend(calendarClass, sel_registerName("currentCalendar"));

            // Get time components
            // NSCalendarUnitHour | NSCalendarUnitMinute | NSCalendarUnitSecond = 0x70
            IntPtr components = objc_msgSend(calendar, sel_registerName("components:fromDate:"),
                new IntPtr(0x70), date);

            _hours = (int)objc_msgSend(components, sel_registerName("hour"));
            _minutes = (int)objc_msgSend(components, sel_registerName("minute"));
            _seconds = (int)objc_msgSend(components, sel_registerName("second"));
        }

        return (_hours, _minutes, _seconds);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _datePicker == IntPtr.Zero) return;

        // Create NSRect
        CGRect frame = new CGRect { origin = new CGPoint { x = x, y = y }, size = new CGSize { width = width, height = height } };
        objc_msgSend_stret(out frame, _datePicker, sel_registerName("setFrame:"), frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _datePicker == IntPtr.Zero) return default;

        CGRect frame;
        objc_msgSend_stret(out frame, _datePicker, sel_registerName("frame"));

        return new Rectangle((int)frame.origin.x, (int)frame.origin.y,
            (int)frame.size.width, (int)frame.size.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _datePicker == IntPtr.Zero) return;

        objc_msgSend(_datePicker, sel_registerName("setHidden:"), visible ? IntPtr.Zero : new IntPtr(1));
    }

    public bool GetVisible()
    {
        if (_disposed || _datePicker == IntPtr.Zero) return false;

        IntPtr hidden = objc_msgSend(_datePicker, sel_registerName("isHidden"));
        return hidden == IntPtr.Zero;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _datePicker == IntPtr.Zero) return;

        objc_msgSend(_datePicker, sel_registerName("setEnabled:"), enabled ? new IntPtr(1) : IntPtr.Zero);
    }

    public bool GetEnabled()
    {
        if (_disposed || _datePicker == IntPtr.Zero) return false;

        IntPtr enabled = objc_msgSend(_datePicker, sel_registerName("isEnabled"));
        return enabled != IntPtr.Zero;
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _datePicker == IntPtr.Zero) return;

        // Create NSColor
        IntPtr colorClass = objc_getClass("NSColor");
        IntPtr nsColor = objc_msgSend(colorClass, sel_registerName("colorWithCalibratedRed:green:blue:alpha:"),
            color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);

        objc_msgSend(_datePicker, sel_registerName("setBackgroundColor:"), nsColor);
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255);
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _datePicker == IntPtr.Zero) return;

        // Create NSColor
        IntPtr colorClass = objc_getClass("NSColor");
        IntPtr nsColor = objc_msgSend(colorClass, sel_registerName("colorWithCalibratedRed:green:blue:alpha:"),
            color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);

        objc_msgSend(_datePicker, sel_registerName("setTextColor:"), nsColor);
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0);
    }

    internal IntPtr GetNativeHandle()
    {
        return _datePicker;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_datePicker != IntPtr.Zero)
            {
                objc_msgSend(_datePicker, sel_registerName("removeFromSuperview"));
                objc_msgSend(_datePicker, sel_registerName("release"));
                _datePicker = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    // Objective-C runtime P/Invoke declarations

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, double arg1, double arg2, double arg3, double arg4);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector, CGRect arg1);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double x;
        public double y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double width;
        public double height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public CGPoint origin;
        public CGSize size;
    }
}
