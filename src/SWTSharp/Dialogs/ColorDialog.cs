using SWTSharp.Graphics;
using SWTSharp.Platform;

namespace SWTSharp.Dialogs;

/// <summary>
/// Color selection dialog.
/// Allows the user to select a color from a color picker.
/// </summary>
public class ColorDialog : Dialog
{
    private RGB _rgb;
    private RGB[]? _rgbs;

    /// <summary>
    /// Gets or sets the selected color.
    /// </summary>
    public RGB RGB
    {
        get => _rgb;
        set => _rgb = value;
    }

    /// <summary>
    /// Gets or sets the custom colors palette (platform-specific).
    /// </summary>
    public RGB[]? RGBs
    {
        get => _rgbs;
        set => _rgbs = value;
    }

    /// <summary>
    /// Creates a new color dialog with the specified parent and style.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    /// <param name="style">Style bits</param>
    public ColorDialog(Shell? parent, int style) : base(parent, style)
    {
        _rgb = new RGB(255, 255, 255); // Default to white
    }

    /// <summary>
    /// Creates a new color dialog with the specified parent.
    /// </summary>
    /// <param name="parent">Parent shell (can be null)</param>
    public ColorDialog(Shell? parent) : this(parent, SWT.NONE)
    {
    }

    /// <summary>
    /// Opens the color dialog and returns the selected color.
    /// </summary>
    /// <returns>Selected color, or null if cancelled</returns>
    public RGB? Open()
    {
        CheckWidget();

        // Get parent window handle
        IntPtr parentHandle = IntPtr.Zero;
        if (Parent?.PlatformWidget is IPlatformWindow window)
        {
            parentHandle = window.GetNativeHandle();
        }

        // Call platform color dialog
        RGB? selectedColor = PlatformFactory.Instance.ShowColorDialog(
            parentHandle,
            Text,
            _rgb,
            _rgbs);

        // User cancelled
        if (!selectedColor.HasValue)
        {
            return null;
        }

        // Update RGB from result
        _rgb = selectedColor.Value;

        return selectedColor;
    }
}
