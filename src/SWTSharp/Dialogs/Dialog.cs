namespace SWTSharp.Dialogs;

/// <summary>
/// Base class for all dialog windows.
/// Dialogs are modal or modeless windows that are used to interact with the user.
/// </summary>
public abstract class Dialog
{
    private Shell? _parent;
    private int _style;
    private string _text = string.Empty;

    /// <summary>
    /// Gets the parent shell of this dialog.
    /// </summary>
    public Shell? Parent
    {
        get => _parent;
    }

    /// <summary>
    /// Gets the style bits for this dialog.
    /// </summary>
    public int Style
    {
        get => _style;
    }

    /// <summary>
    /// Gets or sets the title text of the dialog.
    /// </summary>
    public string Text
    {
        get => _text;
        set => _text = value ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the Dialog class.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    protected Dialog(Shell? parent) : this(parent, SWT.NONE)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Dialog class with a style.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    /// <param name="style">Dialog style bits</param>
    protected Dialog(Shell? parent, int style)
    {
        _parent = parent;
        _style = style;
    }

    /// <summary>
    /// Validates that the dialog has not been disposed.
    /// </summary>
    protected void CheckWidget()
    {
        if (_parent != null && _parent.IsDisposed)
        {
            throw new SWTException(SWT.ERROR_WIDGET_DISPOSED);
        }
    }
}
