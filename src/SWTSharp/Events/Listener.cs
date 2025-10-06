namespace SWTSharp.Events;

/// <summary>
/// Implementers of Listener provide a simple handleEvent() method that is used internally
/// by SWT to dispatch events.
/// </summary>
/// <remarks>
/// After creating an instance of a class that implements this interface it can be added
/// to a widget using the addListener(int eventType, Listener handler) method and removed
/// using the removeListener(int eventType, Listener handler) method. When the specified
/// event occurs, handleEvent(...) will be sent to the instance.
/// </remarks>
public interface IListener
{
    /// <summary>
    /// Sent when an event that the receiver has registered for occurs.
    /// </summary>
    /// <param name="e">The event which occurred</param>
    void HandleEvent(Event e);
}
