using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a single tab page within a TabFolder.
/// TabItems are not standalone controls but rather define the tab label and associated content control.
/// </summary>
public class TabItem : Widget
{
    private TabFolder _parent;
    private string _text = string.Empty;
    private string _toolTipText = string.Empty;
    private Control? _control;
    private IPlatformTabItem? _platformTabItem;

    /// <summary>
    /// Gets the parent TabFolder.
    /// </summary>
    public TabFolder Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the tab's display text.
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
    /// Gets or sets the tooltip text for this tab.
    /// </summary>
    public string ToolTipText
    {
        get
        {
            CheckWidget();
            return _toolTipText;
        }
        set
        {
            CheckWidget();
            _toolTipText = value ?? string.Empty;
            UpdateToolTip();
        }
    }

    /// <summary>
    /// Gets or sets the control displayed when this tab is selected.
    /// </summary>
    public Control? Control
    {
        get
        {
            CheckWidget();
            return _control;
        }
        set
        {
            CheckWidget();
            SetControl(value);
        }
    }

    // Handle property removed - replaced with PlatformWidget

    /// <summary>
    /// Creates a new tab item and appends it to the parent TabFolder.
    /// </summary>
    /// <param name="parent">The parent TabFolder</param>
    /// <param name="style">Style flags (currently unused, reserved for future use)</param>
    public TabItem(TabFolder parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a new tab item at the specified index in the parent TabFolder.
    /// </summary>
    /// <param name="parent">The parent TabFolder</param>
    /// <param name="style">Style flags (currently unused, reserved for future use)</param>
    /// <param name="index">The index at which to insert the tab, or -1 to append</param>
    public TabItem(TabFolder parent, int style, int index) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        CreateWidget(index);
    }

    /// <summary>
    /// Creates the platform-specific tab item.
    /// </summary>
    private void CreateWidget(int index)
    {
        // TabItem delegates to parent TabFolder's platform widget
        if (_parent?.PlatformWidget is IPlatformTabFolder platformTabFolder)
        {
            // Create tab item through platform tab folder
            _platformTabItem = platformTabFolder.CreateTabItem(Style, index);

            // Set initial text
            if (_platformTabItem != null)
            {
                _platformTabItem.SetText(_text);
            }
        }

        // Add this item to parent's collection
        _parent?.AddItem(this, index);
    }

    /// <summary>
    /// Sets the control to be displayed when this tab is selected.
    /// </summary>
    /// <param name="control">The control to display, or null to remove the current control</param>
    public void SetControl(Control? control)
    {
        CheckWidget();

        // Remove old control if any
        if (_control != null && _control != control)
        {
            _control.Visible = false;
        }

        _control = control;

        // TODO: Update platform via IPlatformWidget interface
        // Update platform using new interface
        // if (_handle != IntPtr.Zero)
        // {
        //     Platform.PlatformFactory.Instance.SetTabItemControl(PlatformWidget, control?.PlatformWidget ?? IntPtr.Zero);
        // }

        // Show control if this tab is currently selected
        if (_control != null && _parent.Selection == this)
        {
            _control.Visible = true;
            var bounds = _parent.GetBounds();
            _control.SetBounds(0, 0, bounds.Width, bounds.Height);
        }
        else if (_control != null)
        {
            _control.Visible = false;
        }
    }

    /// <summary>
    /// Sets the text displayed on the tab.
    /// </summary>
    /// <param name="text">The text to display</param>
    public void SetText(string text)
    {
        CheckWidget();
        Text = text;
    }

    /// <summary>
    /// Gets the control associated with this tab.
    /// </summary>
    /// <returns>The control, or null if no control is set</returns>
    public Control? GetControl()
    {
        CheckWidget();
        return _control;
    }

    /// <summary>
    /// Gets the parent TabFolder.
    /// </summary>
    /// <returns>The parent TabFolder</returns>
    public TabFolder GetParent()
    {
        CheckWidget();
        return _parent;
    }

    /// <summary>
    /// Gets the display text of the tab.
    /// </summary>
    /// <returns>The tab text</returns>
    public string GetText()
    {
        CheckWidget();
        return _text;
    }

    /// <summary>
    /// Gets the tooltip text for the tab.
    /// </summary>
    /// <returns>The tooltip text</returns>
    public string GetToolTipText()
    {
        CheckWidget();
        return _toolTipText;
    }

    /// <summary>
    /// Sets the tooltip text for the tab.
    /// </summary>
    /// <param name="text">The tooltip text</param>
    public void SetToolTipText(string text)
    {
        CheckWidget();
        ToolTipText = text;
    }

    /// <summary>
    /// Updates the tab's text in the platform control.
    /// </summary>
    private void UpdateText()
    {
        // Use IPlatformTabItem interface to update text
        if (_platformTabItem != null)
        {
            _platformTabItem.SetText(_text);
        }
    }

    /// <summary>
    /// Updates the tab's tooltip in the platform control.
    /// </summary>
    private void UpdateToolTip()
    {
        // TODO: Add tooltip support to IPlatformTabItem interface
        // For now, tooltip text is stored but not implemented in platform widget
        // if (_platformTabItem != null)
        // {
        //     _platformTabItem.SetToolTip(_toolTipText);
        // }
    }

    protected override void ReleaseWidget()
    {
        // Remove from parent
        if (_parent != null)
        {
            _parent.RemoveItem(this);
        }

        // Don't dispose the control - it's owned by the application
        _control = null;

        // Dispose platform tab item
        _platformTabItem?.Dispose();
        _platformTabItem = null;

        base.ReleaseWidget();
    }
}
