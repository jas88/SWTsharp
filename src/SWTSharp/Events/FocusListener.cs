namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide methods that deal with the events
/// that are generated as controls gain and lose focus.
/// </summary>
public interface IFocusListener
{
    /// <summary>
    /// Sent when a control gets focus.
    /// </summary>
    /// <param name="e">An event containing information about the focus change</param>
    void FocusGained(FocusEvent e);

    /// <summary>
    /// Sent when a control loses focus.
    /// </summary>
    /// <param name="e">An event containing information about the focus change</param>
    void FocusLost(FocusEvent e);
}

/// <summary>
/// This adapter class provides default implementations for the methods described by the
/// FocusListener interface.
/// </summary>
public abstract class FocusAdapter : IFocusListener
{
    /// <summary>
    /// Sent when a control gets focus.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void FocusGained(FocusEvent e) { }

    /// <summary>
    /// Sent when a control loses focus.
    /// The default behavior is to do nothing.
    /// </summary>
    public virtual void FocusLost(FocusEvent e) { }
}
