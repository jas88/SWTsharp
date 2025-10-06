namespace SWTSharp.Events;

/// <summary>
/// Instances of this class are sent as a result of operations being performed on widgets.
/// </summary>
public class Event
{
    /// <summary>
    /// The widget that issued the event.
    /// </summary>
    public Widget? Widget { get; set; }

    /// <summary>
    /// The display where the event occurred.
    /// </summary>
    public Display? Display { get; set; }

    /// <summary>
    /// The type of event, as defined by the event type constants in SWT.
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// The event specific detail field, as defined by the detail constants in SWT.
    /// </summary>
    public int Detail { get; set; }

    /// <summary>
    /// The x location of the event.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The y location of the event.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The width of the bounding rectangle.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The height of the bounding rectangle.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// The button that was pressed or released; 1 for the first button, 2 for the second, etc.
    /// </summary>
    public int Button { get; set; }

    /// <summary>
    /// The state of the keyboard modifier keys and mouse masks at the time the event was generated.
    /// </summary>
    public int StateMask { get; set; }

    /// <summary>
    /// The character represented by the key that was typed.
    /// </summary>
    public char Character { get; set; }

    /// <summary>
    /// The key code of the key that was typed.
    /// </summary>
    public int KeyCode { get; set; }

    /// <summary>
    /// The time that the event occurred.
    /// </summary>
    public int Time { get; set; }

    /// <summary>
    /// A field for application use.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// The item that the event occurred in.
    /// </summary>
    public object? Item { get; set; }

    /// <summary>
    /// The index of the item where the event occurred.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The text associated with the event.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// A flag indicating whether the operation should be allowed.
    /// Setting this field to false will cancel the operation.
    /// </summary>
    public bool Doit { get; set; } = true;

    /// <summary>
    /// Gets the bounds as a Graphics.Rectangle.
    /// </summary>
    public Graphics.Rectangle GetBounds() => new(X, Y, Width, Height);

    /// <summary>
    /// Returns a string containing a concise, human-readable description of the receiver.
    /// </summary>
    public override string ToString()
    {
        return $"Event {{type={Type} widget={Widget} time={Time} data={Data}}}";
    }
}
