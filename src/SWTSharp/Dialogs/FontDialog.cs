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

        var result = PlatformFactory.Instance.ShowFontDialog(
            Parent?.Handle ?? IntPtr.Zero,
            Text,
            _fontData,
            _rgb);

        if (result.FontData != null)
        {
            _fontData = result.FontData;
            _rgb = result.Color;
            return _fontData;
        }

        return null;
    }
}
