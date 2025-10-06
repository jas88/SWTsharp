namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide a method that deals with the event
/// that is generated when a widget is disposed.
/// </summary>
public interface IDisposeListener
{
    /// <summary>
    /// Sent when the widget is disposed.
    /// </summary>
    /// <param name="e">An event containing information about the dispose</param>
    void WidgetDisposed(DisposeEvent e);
}
