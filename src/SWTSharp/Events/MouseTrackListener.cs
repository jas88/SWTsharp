namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide methods that deal with the events
/// that are generated as the mouse pointer passes (or hovers) over controls.
/// </summary>
public interface IMouseTrackListener
{
    /// <summary>
    /// Sent when the mouse pointer enters the area of the control.
    /// </summary>
    /// <param name="e">An event containing information about the mouse enter</param>
    void MouseEnter(MouseEvent e);

    /// <summary>
    /// Sent when the mouse pointer exits the area of the control.
    /// </summary>
    /// <param name="e">An event containing information about the mouse exit</param>
    void MouseExit(MouseEvent e);

    /// <summary>
    /// Sent when the mouse pointer hovers (that is, stops moving for an extended period)
    /// over a control.
    /// </summary>
    /// <param name="e">An event containing information about the hover</param>
    void MouseHover(MouseEvent e);
}

/// <summary>
/// This adapter class provides default implementations for the methods described by the
/// MouseTrackListener interface.
/// </summary>
public abstract class MouseTrackAdapter : IMouseTrackListener
{
    /// <summary>
    /// Sent when the mouse pointer enters the area of the control.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void MouseEnter(MouseEvent e) { }

    /// <summary>
    /// Sent when the mouse pointer exits the area of the control.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void MouseExit(MouseEvent e) { }

    /// <summary>
    /// Sent when the mouse pointer hovers over a control.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void MouseHover(MouseEvent e) { }
}
