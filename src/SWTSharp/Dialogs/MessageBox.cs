using SWTSharp.Platform;

namespace SWTSharp.Dialogs;

/// <summary>
/// Standard message dialog box.
/// Displays a message with an icon and buttons for user response.
/// </summary>
public class MessageBox : Dialog
{
    private string _message = string.Empty;

    /// <summary>
    /// Gets or sets the message to display in the dialog.
    /// </summary>
    public string Message
    {
        get => _message;
        set => _message = value ?? string.Empty;
    }

    /// <summary>
    /// Creates a new message box with the specified parent and style.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    /// <param name="style">Style bits (icons and buttons)</param>
    public MessageBox(Shell? parent, int style) : base(parent, style)
    {
    }

    /// <summary>
    /// Creates a new message box with the specified parent.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    public MessageBox(Shell? parent) : this(parent, SWT.OK | SWT.ICON_INFORMATION)
    {
    }

    /// <summary>
    /// Opens the message box and returns the button clicked by the user.
    /// </summary>
    /// <returns>Button constant (SWT.OK, SWT.CANCEL, SWT.YES, SWT.NO, SWT.RETRY, SWT.ABORT, SWT.IGNORE)</returns>
    public int Open()
    {
        CheckWidget();
        // TODO: Implement message box through platform widget interface
        return SWT.OK; // Placeholder
    }
}
