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
        // Get parent handle
        IntPtr parentHandle = IntPtr.Zero;
        if (Parent != null)
        {
            parentHandle = Parent.Handle;
        }

        // Create platform-specific group box
        Handle = Platform.PlatformFactory.Instance.CreateGroup(parentHandle, Style, _text);
    }

    /// <summary>
    /// Updates the group's title text on the platform widget.
    /// </summary>
    private void UpdateText()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetGroupText(Handle, _text);
        }
    }

    protected override void UpdateVisible()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetControlVisible(Handle, Visible);
        }
    }

    protected override void UpdateEnabled()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetControlEnabled(Handle, Enabled);
        }
    }

    protected override void UpdateBounds()
    {
        if (Handle != IntPtr.Zero)
        {
            var (x, y, width, height) = GetBounds();
            Platform.PlatformFactory.Instance.SetControlBounds(Handle, x, y, width, height);
        }
    }

    protected override void ReleaseWidget()
    {
        // Platform handles cleanup via parent destruction
        base.ReleaseWidget();
    }
}
