using SWTSharp.Platform;

namespace SWTSharp.Dialogs;

/// <summary>
/// Directory (folder) selection dialog.
/// Allows the user to select a directory from the file system.
/// </summary>
public class DirectoryDialog : Dialog
{
    private string _filterPath = string.Empty;
    private string _message = string.Empty;

    /// <summary>
    /// Gets or sets the initial directory path.
    /// </summary>
    public string FilterPath
    {
        get => _filterPath;
        set => _filterPath = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the message to display in the dialog.
    /// </summary>
    public string Message
    {
        get => _message;
        set => _message = value ?? string.Empty;
    }

    /// <summary>
    /// Creates a new directory dialog with the specified parent and style.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    /// <param name="style">Style bits</param>
    public DirectoryDialog(Shell? parent, int style) : base(parent, style)
    {
    }

    /// <summary>
    /// Creates a new directory dialog with the specified parent.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    public DirectoryDialog(Shell? parent) : this(parent, SWT.NONE)
    {
    }

    /// <summary>
    /// Opens the directory dialog and returns the selected directory path.
    /// </summary>
    /// <returns>Selected directory path, or null if cancelled</returns>
    public string? Open()
    {
        CheckWidget();

        // Get parent window handle
        IntPtr parentHandle = IntPtr.Zero;
        if (Parent?.PlatformWidget is IPlatformWindow window)
        {
            parentHandle = window.GetNativeHandle();
        }

        // Call platform directory dialog
        string? selectedPath = PlatformFactory.Instance.ShowDirectoryDialog(
            parentHandle,
            Text,
            _message,
            _filterPath);

        // User cancelled
        if (selectedPath == null)
        {
            return null;
        }

        // Update filter path from result
        _filterPath = selectedPath;

        return selectedPath;
    }
}
