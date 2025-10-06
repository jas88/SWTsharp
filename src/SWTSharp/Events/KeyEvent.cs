namespace SWTSharp.Events;

/// <summary>
/// Instances of this class are sent as a result of keys being pressed and released.
/// </summary>
public class KeyEvent : Event
{
    /// <summary>
    /// The character represented by the key that was typed.
    /// This is the final character that results after all modifiers have been applied.
    /// </summary>
    public new char Character { get; set; }

    /// <summary>
    /// The key code of the key that was typed, as defined by the key code constants in SWT.
    /// When the character field of the event is ambiguous, this field contains the
    /// unaffected value of the original character.
    /// </summary>
    public new int KeyCode { get; set; }

    /// <summary>
    /// The state of the keyboard and mouse at the time the event was generated.
    /// </summary>
    public new int StateMask { get; set; }

    /// <summary>
    /// A flag indicating whether the operation should be allowed.
    /// </summary>
    public new bool Doit { get; set; } = true;

    /// <summary>
    /// Constructs a new instance of this class.
    /// </summary>
    /// <param name="e">The event to copy information from</param>
    public KeyEvent(Event e)
    {
        Widget = e.Widget;
        Display = e.Display;
        Type = e.Type;
        Character = e.Character;
        KeyCode = e.KeyCode;
        StateMask = e.StateMask;
        Time = e.Time;
        Data = e.Data;
        Doit = e.Doit;
    }
}
