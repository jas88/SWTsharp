namespace SWTSharp.Events;

/// <summary>
/// Base class for SWT event arguments.
/// </summary>
public class SWTEventArgs : EventArgs
{
    /// <summary>
    /// The widget that generated the event.
    /// </summary>
    public Widget? Widget { get; set; }

    /// <summary>
    /// The time the event occurred.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Application-defined event data.
    /// </summary>
    public object? Data { get; set; }

    public SWTEventArgs()
    {
        // Convert System.DateTime to SWTSharp.DateTime
        var now = System.DateTime.Now;
        Time = new DateTime(null!, SWT.TIME | SWT.DATE);
        Time.SetDate(now.Year, now.Month - 1, now.Day);
        Time.SetTime(now.Hour, now.Minute, now.Second);
    }
}

/// <summary>
/// Event arguments for selection events.
/// </summary>
public class SelectionEventArgs : SWTEventArgs
{
    /// <summary>
    /// The selected item.
    /// </summary>
    public object? Item { get; set; }

    /// <summary>
    /// The selection state (true if selected).
    /// </summary>
    public bool Selected { get; set; }
}

/// <summary>
/// Event arguments for key events.
/// </summary>
public class KeyEventArgs : SWTEventArgs
{
    /// <summary>
    /// The character that was typed.
    /// </summary>
    public char Character { get; set; }

    /// <summary>
    /// The key code.
    /// </summary>
    public int KeyCode { get; set; }

    /// <summary>
    /// The state of modifier keys (Shift, Ctrl, Alt).
    /// </summary>
    public int StateMask { get; set; }
}

/// <summary>
/// Event arguments for mouse events.
/// </summary>
public class MouseEventArgs : SWTEventArgs
{
    /// <summary>
    /// The mouse button that was pressed (1=left, 2=middle, 3=right).
    /// </summary>
    public int Button { get; set; }

    /// <summary>
    /// The x coordinate of the mouse.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The y coordinate of the mouse.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The state of modifier keys and mouse buttons.
    /// </summary>
    public int StateMask { get; set; }

    /// <summary>
    /// The number of clicks (1=single, 2=double, etc.).
    /// </summary>
    public int ClickCount { get; set; }
}

/// <summary>
/// Event arguments for paint events.
/// </summary>
public class PaintEventArgs : SWTEventArgs
{
    /// <summary>
    /// The area that needs to be painted.
    /// </summary>
    public (int X, int Y, int Width, int Height) Bounds { get; set; }

    /// <summary>
    /// The graphics context for painting.
    /// </summary>
    public object? GC { get; set; }
}
