namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide methods that deal with the events
/// that are generated as keys are pressed on the system keyboard.
/// </summary>
public interface IKeyListener
{
    /// <summary>
    /// Sent when a key is pressed on the system keyboard.
    /// </summary>
    /// <param name="e">An event containing information about the key press</param>
    void KeyPressed(KeyEvent e);

    /// <summary>
    /// Sent when a key is released on the system keyboard.
    /// </summary>
    /// <param name="e">An event containing information about the key release</param>
    void KeyReleased(KeyEvent e);
}

/// <summary>
/// This adapter class provides default implementations for the methods described by the
/// KeyListener interface.
/// </summary>
public abstract class KeyAdapter : IKeyListener
{
    /// <summary>
    /// Sent when a key is pressed on the system keyboard.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void KeyPressed(KeyEvent e) { }

    /// <summary>
    /// Sent when a key is released on the system keyboard.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void KeyReleased(KeyEvent e) { }
}
