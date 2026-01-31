using SWTSharp.Graphics;
using SWTSharp.Platform;

namespace SWTSharp.Dialogs;

/// <summary>
/// Font selection dialog.
/// Allows the user to select a font with name, size, and style.
/// </summary>
public class FontDialog : Dialog
{
    private FontData? _fontData;
    private FontData[]? _fontList;
    private RGB? _rgb;

    /// <summary>
    /// Gets or sets the selected font data.
    /// </summary>
    public FontData? FontData
    {
        get => _fontData;
        set => _fontData = value;
    }

    /// <summary>
    /// Gets or sets the list of available fonts (platform-specific).
    /// </summary>
    public FontData[]? FontList
    {
        get => _fontList;
        set => _fontList = value;
    }

    /// <summary>
    /// Gets or sets the selected font color.
    /// </summary>
    public RGB? RGB
    {
        get => _rgb;
        set => _rgb = value;
    }

    /// <summary>
    /// Creates a new font dialog with the specified parent and style.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    /// <param name="style">Style bits</param>
    public FontDialog(Shell? parent, int style) : base(parent, style)
    {
        // Initialize with default font data
        _fontData = new FontData("Arial", 12, SWT.NORMAL);
    }

    /// <summary>
    /// Creates a new font dialog with the specified parent.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    public FontDialog(Shell? parent) : this(parent, SWT.NONE)
    {
    }

    /// <summary>
    /// Opens the font dialog and returns the selected font data.
    /// </summary>
    /// <returns>Selected font data, or null if cancelled</returns>
    public FontData? Open()
    {
        CheckWidget();

        // Get parent window handle
        IntPtr parentHandle = IntPtr.Zero;
        if (Parent?.PlatformWidget is IPlatformWindow window)
        {
            parentHandle = window.GetNativeHandle();
        }

        // Call platform font dialog
        var result = PlatformFactory.Instance.ShowFontDialog(
            parentHandle,
            Text,
            _fontData,
            _rgb);

        // User cancelled
        if (result.FontData == null)
        {
            return null;
        }

        // Update state from result
        _fontData = result.FontData;

        if (result.Color.HasValue)
        {
            _rgb = result.Color;
        }

        return _fontData;
    }
}
