namespace SWTSharp.Events;

/// <summary>
/// Instances of this class are sent as a result of widgets being selected.
/// </summary>
public class SelectionEvent : Event
{
    /// <summary>
    /// Extra detail information about the selection, typically application specific.
    /// </summary>
    public new int Detail { get; set; }

    /// <summary>
    /// A flag indicating whether the operation should be allowed.
    /// </summary>
    public new bool Doit { get; set; } = true;

    /// <summary>
    /// The item that was selected.
    /// </summary>
    public new object? Item { get; set; }

    /// <summary>
    /// The text associated with the selection.
    /// </summary>
    public new string? Text { get; set; }

    /// <summary>
    /// The state of the keyboard and mouse at the time the event was generated.
    /// </summary>
    public new int StateMask { get; set; }

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    /// <param name="e">The event to copy information from</param>
    public SelectionEvent(Event e)
    {
        Widget = e.Widget;
        Display = e.Display;
        Type = e.Type;
        Detail = e.Detail;
        X = e.X;
        Y = e.Y;
        Width = e.Width;
        Height = e.Height;
        StateMask = e.StateMask;
        Time = e.Time;
        Data = e.Data;
        Item = e.Item;
        Text = e.Text;
        Doit = e.Doit;
    }
}
