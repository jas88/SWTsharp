namespace SWTSharp.Events;

/// <summary>
/// Classes which implement this interface provide methods that deal with the events
/// that are generated when selection occurs in a control.
/// </summary>
public interface ISelectionListener
{
    /// <summary>
    /// Sent when selection occurs in the control.
    /// </summary>
    /// <param name="e">An event containing information about the selection</param>
    void WidgetSelected(SelectionEvent e);

    /// <summary>
    /// Sent when default selection occurs in the control.
    /// </summary>
    /// <param name="e">An event containing information about the default selection</param>
    void WidgetDefaultSelected(SelectionEvent e);
}
