using SWTSharp.Platform;
using SWTSharp.Events;

namespace SWTSharp;

/// <summary>
/// Represents a date/time picker control with calendar dropdown.
/// Supports DATE, TIME, and CALENDAR display modes.
/// </summary>
public class DateTime : Control
{
    private System.DateTime _value = System.DateTime.Now;
    private int _year, _month, _day;
    private int _hours, _minutes, _seconds;

    /// <summary>
    /// Gets or sets the year value.
    /// </summary>
    public int Year
    {
        get
        {
            CheckWidget();
            return _year;
        }
        set
        {
            CheckWidget();
            if (value < 1752 || value > 9999)
                throw new ArgumentOutOfRangeException(nameof(value), "Year must be between 1752 and 9999");
            _year = value;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Gets or sets the month value (0-11).
    /// </summary>
    public int Month
    {
        get
        {
            CheckWidget();
            return _month;
        }
        set
        {
            CheckWidget();
            if (value < 0 || value > 11)
                throw new ArgumentOutOfRangeException(nameof(value), "Month must be between 0 and 11");
            _month = value;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Gets or sets the day of month value (1-31).
    /// </summary>
    public int Day
    {
        get
        {
            CheckWidget();
            return _day;
        }
        set
        {
            CheckWidget();
            if (value < 1 || value > 31)
                throw new ArgumentOutOfRangeException(nameof(value), "Day must be between 1 and 31");
            _day = value;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Gets or sets the hours value (0-23).
    /// </summary>
    public int Hours
    {
        get
        {
            CheckWidget();
            return _hours;
        }
        set
        {
            CheckWidget();
            if (value < 0 || value > 23)
                throw new ArgumentOutOfRangeException(nameof(value), "Hours must be between 0 and 23");
            _hours = value;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Gets or sets the minutes value (0-59).
    /// </summary>
    public int Minutes
    {
        get
        {
            CheckWidget();
            return _minutes;
        }
        set
        {
            CheckWidget();
            if (value < 0 || value > 59)
                throw new ArgumentOutOfRangeException(nameof(value), "Minutes must be between 0 and 59");
            _minutes = value;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Gets or sets the seconds value (0-59).
    /// </summary>
    public int Seconds
    {
        get
        {
            CheckWidget();
            return _seconds;
        }
        set
        {
            CheckWidget();
            if (value < 0 || value > 59)
                throw new ArgumentOutOfRangeException(nameof(value), "Seconds must be between 0 and 59");
            _seconds = value;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Gets or sets the DateTime value.
    /// </summary>
    public System.DateTime Value
    {
        get
        {
            CheckWidget();
            return _value;
        }
        set
        {
            CheckWidget();
            _value = value;
            _year = value.Year;
            _month = value.Month - 1; // SWT uses 0-based months
            _day = value.Day;
            _hours = value.Hour;
            _minutes = value.Minute;
            _seconds = value.Second;
            UpdateDateTime();
        }
    }

    /// <summary>
    /// Occurs when the date/time selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a new DateTime control.
    /// </summary>
    /// <param name="parent">Parent control</param>
    /// <param name="style">Style flags (SWT.DATE, SWT.TIME, SWT.CALENDAR, SWT.SHORT, SWT.MEDIUM, SWT.LONG)</param>
    public DateTime(Control parent, int style) : base(parent, style)
    {
        // Initialize with current date/time
        var now = System.DateTime.Now;
        _year = now.Year;
        _month = now.Month - 1; // SWT uses 0-based months
        _day = now.Day;
        _hours = now.Hour;
        _minutes = now.Minute;
        _seconds = now.Second;
        _value = now;

        CreateWidget();
    }

    private void CreateWidget()
    {
        // Use platform widget
        var widget = Platform.PlatformFactory.Instance.CreateDateTimeWidget(
            Parent?.PlatformWidget,
            Style
        );

        // Only assign after successful creation
        PlatformWidget = widget;

        // Set initial date/time value via platform widget interface
        if (PlatformWidget is IPlatformDateTime dateTime)
        {
            dateTime.SetDate(_year, _month, _day);
            dateTime.SetTime(_hours, _minutes, _seconds);

            // Subscribe to selection changed events
            dateTime.SelectionChanged += OnPlatformSelectionChanged;
        }
    }

    /// <summary>
    /// Sets the date portion of the DateTime.
    /// </summary>
    /// <param name="year">Year (1752-9999)</param>
    /// <param name="month">Month (0-11)</param>
    /// <param name="day">Day (1-31)</param>
    public void SetDate(int year, int month, int day)
    {
        CheckWidget();
        if (year < 1752 || year > 9999)
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1752 and 9999");
        if (month < 0 || month > 11)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 0 and 11");
        if (day < 1 || day > 31)
            throw new ArgumentOutOfRangeException(nameof(day), "Day must be between 1 and 31");

        _year = year;
        _month = month;
        _day = day;
        UpdateDateTime();
    }

    /// <summary>
    /// Sets the time portion of the DateTime.
    /// </summary>
    /// <param name="hours">Hours (0-23)</param>
    /// <param name="minutes">Minutes (0-59)</param>
    /// <param name="seconds">Seconds (0-59)</param>
    public void SetTime(int hours, int minutes, int seconds)
    {
        CheckWidget();
        if (hours < 0 || hours > 23)
            throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be between 0 and 23");
        if (minutes < 0 || minutes > 59)
            throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be between 0 and 59");
        if (seconds < 0 || seconds > 59)
            throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be between 0 and 59");

        _hours = hours;
        _minutes = minutes;
        _seconds = seconds;
        UpdateDateTime();
    }

    /// <summary>
    /// Gets the current date values.
    /// </summary>
    /// <returns>Tuple of (year, month, day)</returns>
    public (int Year, int Month, int Day) GetDate()
    {
        CheckWidget();
        if (PlatformWidget is IPlatformDateTime dateTime)
        {
            return dateTime.GetDate();
        }
        return (_year, _month, _day);
    }

    /// <summary>
    /// Gets the current time values.
    /// </summary>
    /// <returns>Tuple of (hours, minutes, seconds)</returns>
    public (int Hours, int Minutes, int Seconds) GetTime()
    {
        CheckWidget();
        if (PlatformWidget is IPlatformDateTime dateTime)
        {
            return dateTime.GetTime();
        }
        return (_hours, _minutes, _seconds);
    }

    private void UpdateDateTime()
    {
        if (PlatformWidget is IPlatformDateTime dateTime)
        {
            dateTime.SetDate(_year, _month, _day);
            dateTime.SetTime(_hours, _minutes, _seconds);

            // Update the combined DateTime value
            try
            {
                _value = new System.DateTime(_year, _month + 1, _day, _hours, _minutes, _seconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Invalid date combination, keep previous value
            }

            OnSelectionChanged(EventArgs.Empty);
        }
    }

    /// <summary>
    /// Handles platform widget selection changed events.
    /// </summary>
    private void OnPlatformSelectionChanged(object? sender, EventArgs e)
    {
        CheckWidget();

        if (PlatformWidget is IPlatformDateTime dateTime)
        {
            var (year, month, day) = dateTime.GetDate();
            var (hours, minutes, seconds) = dateTime.GetTime();

            _year = year;
            _month = month;
            _day = day;
            _hours = hours;
            _minutes = minutes;
            _seconds = seconds;

            try
            {
                _value = new System.DateTime(year, month + 1, day, hours, minutes, seconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Invalid date combination
            }

            // Create SWT Selection event
            var selectionEvent = new Event
            {
                Time = Environment.TickCount
            };
            NotifyListeners(SWT.Selection, selectionEvent);

            // Raise the SelectionChanged event
            OnSelectionChanged(EventArgs.Empty);
        }
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }

    protected override void ReleaseWidget()
    {
        // Unsubscribe from platform widget events
        if (PlatformWidget is IPlatformDateTime dateTime)
        {
            dateTime.SelectionChanged -= OnPlatformSelectionChanged;
        }

        SelectionChanged = null;
        base.ReleaseWidget();
    }
}
