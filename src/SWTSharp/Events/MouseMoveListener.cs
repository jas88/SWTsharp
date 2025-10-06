namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide a method that deals with the events
/// that are generated as the mouse pointer moves.
/// </summary>
public interface IMouseMoveListener
{
    /// <summary>
    /// Sent when the mouse moves.
    /// </summary>
    /// <param name="e">An event containing information about the mouse move</param>
    void MouseMove(MouseEvent e);
}
