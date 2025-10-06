namespace SWTSharp.Events;

/// <summary>
/// Instances of this class are sent as a result of widgets being disposed.
/// </summary>
public class DisposeEvent : Event
{
    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    /// <param name="e">The event to copy information from</param>
    public DisposeEvent(Event e)
    {
        Widget = e.Widget;
        Display = e.Display;
        Type = e.Type;
        Time = e.Time;
        Data = e.Data;
    }
}
