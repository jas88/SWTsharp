using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// A Composite with a border and optional title label.
/// Groups are used to organize related controls and visually separate them with a labeled border.
/// </summary>
public class Group : Composite
{
    private string _text = string.Empty;

    /// <summary>
    /// Gets or sets the group's title text.
    /// The text is displayed in the group's border.
    /// </summary>
    public string Text
    {
        get
        {
            CheckWidget();
            return _text;
        }
        set
        {
            CheckWidget();
            _text = value ?? string.Empty;
            UpdateText();
        }
    }

    /// <summary>
    /// Creates a new group with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite (cannot be null).</param>
    /// <param name="style">The widget style bits. Can include:
    /// SHADOW_IN, SHADOW_OUT, SHADOW_ETCHED_IN, SHADOW_ETCHED_OUT, SHADOW_NONE</param>
    public Group(Composite parent, int style) : base(parent, style)
    {
        // Parent constructor already calls CreateWidget for Composite
        // We need to override the creation
    }

    /// <summary>
    /// Creates the platform-specific group box widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // Create a group widget through the platform factory
        // IPlatform.CreateGroupWidget returns IPlatformComposite for group boxes
        // (GROUPBOX on Windows, GtkFrame on Linux, NSBox on macOS)
        PlatformWidget = PlatformFactory.Instance.CreateGroupWidget(
            Parent?.PlatformWidget,
            Style,
            _text
        );
    }

    /// <summary>
    /// Updates the group's title text on the platform widget.
    /// </summary>
    private void UpdateText()
    {
        // Group widgets store text in the border label
        // IPlatformGroup interface extends IPlatformComposite with SetText support
        if (PlatformWidget is IPlatformGroup groupWidget)
        {
            groupWidget.SetText(_text);
        }
    }

    /// <summary>
    /// Updates the group's visibility state.
    /// </summary>
    protected override void UpdateVisible()
    {
        // Use IPlatformWidget.SetVisible method from base interface
        PlatformWidget?.SetVisible(Visible);
    }

    /// <summary>
    /// Updates the group's enabled state.
    /// </summary>
    protected override void UpdateEnabled()
    {
        // Use IPlatformWidget.SetEnabled method from base interface
        PlatformWidget?.SetEnabled(Enabled);
    }

    /// <summary>
    /// Updates the group's bounds (position and size).
    /// </summary>
    protected override void UpdateBounds()
    {
        // Delegate to base class implementation which calls PlatformWidget.SetBounds
        base.UpdateBounds();
    }

    /// <summary>
    /// Gets the client area rectangle, accounting for the title height and border.
    /// </summary>
    /// <returns>The client area as (x, y, width, height) relative to the group.</returns>
    public (int X, int Y, int Width, int Height) GetClientArea()
    {
        CheckWidget();

        // Get client area from platform widget if available
        if (PlatformWidget is IPlatformGroup groupWidget)
        {
            return groupWidget.GetClientArea();
        }

        // Default: full bounds minus estimated border/title space
        var (_, _, width, height) = GetBounds();
        const int borderWidth = 2;
        const int titleHeight = 16; // Approximate title height

        return (
            borderWidth,
            titleHeight + borderWidth,
            Math.Max(0, width - 2 * borderWidth),
            Math.Max(0, height - titleHeight - 2 * borderWidth)
        );
    }

    protected override void ReleaseWidget()
    {
        // Platform handles cleanup via parent destruction
        base.ReleaseWidget();
    }
}
