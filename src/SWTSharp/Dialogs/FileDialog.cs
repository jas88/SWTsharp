using SWTSharp.Platform;

namespace SWTSharp.Dialogs;

/// <summary>
/// File selection dialog (open or save).
/// Allows the user to select one or more files.
/// </summary>
public class FileDialog : Dialog
{
    private string[] _filterExtensions = Array.Empty<string>();
    private string[] _filterNames = Array.Empty<string>();
    private string _filterPath = string.Empty;
    private string _fileName = string.Empty;
    private string[] _fileNames = Array.Empty<string>();
    private bool _overwrite = true;

    /// <summary>
    /// Gets or sets the file filter extensions (e.g., "*.txt", "*.jpg;*.png").
    /// </summary>
    public string[] FilterExtensions
    {
        get => _filterExtensions;
        set => _filterExtensions = value ?? Array.Empty<string>();
    }

    /// <summary>
    /// Gets or sets the file filter names (e.g., "Text Files", "Image Files").
    /// Should have the same length as FilterExtensions.
    /// </summary>
    public string[] FilterNames
    {
        get => _filterNames;
        set => _filterNames = value ?? Array.Empty<string>();
    }

    /// <summary>
    /// Gets or sets the initial directory path.
    /// </summary>
    public string FilterPath
    {
        get => _filterPath;
        set => _filterPath = value ?? string.Empty;
    }

    /// <summary>
    /// Gets or sets the initial or selected file name.
    /// </summary>
    public string FileName
    {
        get => _fileName;
        set => _fileName = value ?? string.Empty;
    }

    /// <summary>
    /// Gets the selected file names (for MULTI style).
    /// </summary>
    public string[] FileNames
    {
        get => _fileNames;
    }

    /// <summary>
    /// Gets or sets whether to prompt when overwriting files (for SAVE style).
    /// </summary>
    public bool Overwrite
    {
        get => _overwrite;
        set => _overwrite = value;
    }

    /// <summary>
    /// Creates a new file dialog with the specified parent and style.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    /// <param name="style">Style bits (SWT.OPEN, SWT.SAVE, SWT.MULTI)</param>
    public FileDialog(Shell? parent, int style) : base(parent, style)
    {
    }

    /// <summary>
    /// Creates a new file dialog with the specified parent.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    public FileDialog(Shell? parent) : this(parent, SWT.OPEN)
    {
    }

    /// <summary>
    /// Opens the file dialog and returns the selected file path.
    /// </summary>
    /// <returns>Selected file path, or null if cancelled</returns>
    public string? Open()
    {
        CheckWidget();

        var result = PlatformFactory.Instance.ShowFileDialog(
            Parent?.Handle ?? IntPtr.Zero,
            Text,
            _filterPath,
            _fileName,
            _filterNames,
            _filterExtensions,
            Style,
            _overwrite);

        if (result.SelectedFiles != null && result.SelectedFiles.Length > 0)
        {
            _fileName = result.SelectedFiles[0];
            _fileNames = result.SelectedFiles;
            _filterPath = result.FilterPath ?? _filterPath;
            return _fileName;
        }

        return null;
    }
}
