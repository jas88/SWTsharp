namespace SWTSharp.Events;

/// <summary>
/// This adapter class provides default implementations for the methods described by the
/// SelectionListener interface.
/// </summary>
/// <remarks>
/// Classes that wish to deal with SelectionEvents can extend this class and override
/// only the methods which they are interested in.
/// </remarks>
public abstract class SelectionAdapter : ISelectionListener
{
    /// <summary>
    /// Sent when selection occurs in the control.
    /// The default behavior is to do nothing.
    /// </summary>
    /// <param name="e">An event containing information about the selection</param>
    public virtual void WidgetSelected(SelectionEvent e)
    {
    }

    /// <summary>
    /// Sent when default selection occurs in the control.
    /// The default behavior is to do nothing.
    /// </summary>
    /// <param name="e">An event containing information about the default selection</param>
    public virtual void WidgetDefaultSelected(SelectionEvent e)
    {
    }
}
