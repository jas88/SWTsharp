namespace SWTSharp.Events;

/// <summary>
/// Instances of this class are sent whenever mouse related actions occur.
/// </summary>
public class MouseEvent : Event
{
    /// <summary>
    /// The button that was pressed or released.
    /// </summary>
    public new int Button { get; set; }

    /// <summary>
    /// The state of the keyboard and mouse at the time the event was generated.
    /// </summary>
    public new int StateMask { get; set; }

    /// <summary>
    /// The x coordinate of the mouse at the time the event was generated.
    /// </summary>
    public new int X { get; set; }

    /// <summary>
    /// The y coordinate of the mouse at the time the event was generated.
    /// </summary>
    public new int Y { get; set; }

    /// <summary>
    /// The number of times the mouse was clicked.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    /// <param name="e">The event to copy information from</param>
    public MouseEvent(Event e)
    {
        Widget = e.Widget;
        Display = e.Display;
        Type = e.Type;
        Button = e.Button;
        X = e.X;
        Y = e.Y;
        StateMask = e.StateMask;
        Time = e.Time;
        Data = e.Data;
        Count = e.Detail;
    }
}
