namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide methods that deal with the events
/// that are generated as mouse buttons are pressed.
/// </summary>
public interface IMouseListener
{
    /// <summary>
    /// Sent when a mouse button is pressed.
    /// </summary>
    /// <param name="e">An event containing information about the mouse button press</param>
    void MouseDown(MouseEvent e);

    /// <summary>
    /// Sent when a mouse button is released.
    /// </summary>
    /// <param name="e">An event containing information about the mouse button release</param>
    void MouseUp(MouseEvent e);

    /// <summary>
    /// Sent when a mouse button is double clicked.
    /// </summary>
    /// <param name="e">An event containing information about the mouse double click</param>
    void MouseDoubleClick(MouseEvent e);
}

/// <summary>
/// This adapter class provides default implementations for the methods described by the
/// MouseListener interface.
/// </summary>
public abstract class MouseAdapter : IMouseListener
{
    /// <summary>
    /// Sent when a mouse button is pressed.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void MouseDown(MouseEvent e) { }

    /// <summary>
    /// Sent when a mouse button is released.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void MouseUp(MouseEvent e) { }

    /// <summary>
    /// Sent when a mouse button is double clicked.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void MouseDoubleClick(MouseEvent e) { }
}
