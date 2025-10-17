using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a single expandable section within an ExpandBar.
/// ExpandItems are not standalone controls but rather define the section header and associated content control.
/// </summary>
public class ExpandItem : Widget
{
    private ExpandBar _parent;
    private string _text = string.Empty;
    private bool _expanded;
    private int _height = 100;
    private Control? _control;
    private IPlatformExpandItem? _platformExpandItem;

    /// <summary>
    /// Gets the parent ExpandBar.
    /// </summary>
    public ExpandBar Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the item's header text.
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
    /// Gets or sets whether the item is expanded.
    /// </summary>
    public bool Expanded
    {
        get
        {
            CheckWidget();
            return _expanded;
        }
        set
        {
            CheckWidget();
            if (_expanded != value)
            {
                _expanded = value;
                UpdateExpanded();

                // Update control visibility
                if (_control != null)
                {
                    _control.Visible = _expanded;
                }

                OnExpandedChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the height of the item's content area when expanded.
    /// </summary>
    public int Height
    {
        get
        {
            CheckWidget();
            return _height;
        }
        set
        {
            CheckWidget();
            _height = Math.Max(0, value);
            UpdateHeight();
        }
    }

    /// <summary>
    /// Gets or sets the control displayed when this item is expanded.
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

    /// <summary>
    /// Occurs when the expanded state changes.
    /// </summary>
    public event EventHandler? ExpandedChanged;

    /// <summary>
    /// Creates a new expand item and appends it to the parent ExpandBar.
    /// </summary>
    /// <param name="parent">The parent ExpandBar</param>
    /// <param name="style">Style flags (currently unused, reserved for future use)</param>
    public ExpandItem(ExpandBar parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a new expand item at the specified index in the parent ExpandBar.
    /// </summary>
    /// <param name="parent">The parent ExpandBar</param>
    /// <param name="style">Style flags (currently unused, reserved for future use)</param>
    /// <param name="index">The index at which to insert the item, or -1 to append</param>
    public ExpandItem(ExpandBar parent, int style, int index) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        CreateWidget(index);
    }

    /// <summary>
    /// Creates the platform-specific expand item.
    /// </summary>
    private void CreateWidget(int index)
    {
        // ExpandItem delegates to parent ExpandBar's platform widget
        if (_parent?.PlatformWidget is IPlatformExpandBar platformExpandBar)
        {
            // Create expand item through platform expand bar
            _platformExpandItem = platformExpandBar.CreateExpandItem(Style, index);

            // Set initial properties
            if (_platformExpandItem != null)
            {
                _platformExpandItem.SetText(_text);
                _platformExpandItem.SetExpanded(_expanded);
                _platformExpandItem.SetHeight(_height);
            }
        }

        // Add this item to parent's collection
        _parent?.AddItem(this, index);
    }

    /// <summary>
    /// Sets the control to be displayed when this item is expanded.
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

        // Update platform widget
        if (_platformExpandItem != null && _control != null)
        {
            _platformExpandItem.SetControl(_control.PlatformWidget);

            // Show control only if expanded
            _control.Visible = _expanded;
        }
    }

    /// <summary>
    /// Sets the text displayed on the item header.
    /// </summary>
    /// <param name="text">The text to display</param>
    public void SetText(string text)
    {
        CheckWidget();
        Text = text;
    }

    /// <summary>
    /// Sets whether the item is expanded.
    /// </summary>
    /// <param name="expanded">True to expand, false to collapse</param>
    public void SetExpanded(bool expanded)
    {
        CheckWidget();
        Expanded = expanded;
    }

    /// <summary>
    /// Sets the height of the content area when expanded.
    /// </summary>
    /// <param name="height">The height in pixels</param>
    public void SetHeight(int height)
    {
        CheckWidget();
        Height = height;
    }

    /// <summary>
    /// Gets the control associated with this item.
    /// </summary>
    /// <returns>The control, or null if no control is set</returns>
    public Control? GetControl()
    {
        CheckWidget();
        return _control;
    }

    /// <summary>
    /// Gets the parent ExpandBar.
    /// </summary>
    /// <returns>The parent ExpandBar</returns>
    public ExpandBar GetParent()
    {
        CheckWidget();
        return _parent;
    }

    /// <summary>
    /// Gets the display text of the item header.
    /// </summary>
    /// <returns>The item header text</returns>
    public string GetText()
    {
        CheckWidget();
        return _text;
    }

    /// <summary>
    /// Gets whether the item is expanded.
    /// </summary>
    /// <returns>True if expanded, false if collapsed</returns>
    public bool GetExpanded()
    {
        CheckWidget();
        return _expanded;
    }

    /// <summary>
    /// Gets the height of the content area when expanded.
    /// </summary>
    /// <returns>The height in pixels</returns>
    public int GetHeight()
    {
        CheckWidget();
        return _height;
    }

    /// <summary>
    /// Updates the item's text in the platform control.
    /// </summary>
    private void UpdateText()
    {
        if (_platformExpandItem != null)
        {
            _platformExpandItem.SetText(_text);
        }
    }

    /// <summary>
    /// Updates the item's expanded state in the platform control.
    /// </summary>
    private void UpdateExpanded()
    {
        if (_platformExpandItem != null)
        {
            _platformExpandItem.SetExpanded(_expanded);
        }
    }

    /// <summary>
    /// Updates the item's height in the platform control.
    /// </summary>
    private void UpdateHeight()
    {
        if (_platformExpandItem != null)
        {
            _platformExpandItem.SetHeight(_height);
        }
    }

    /// <summary>
    /// Raises the ExpandedChanged event.
    /// </summary>
    protected virtual void OnExpandedChanged(EventArgs e)
    {
        ExpandedChanged?.Invoke(this, e);

        // Also notify via SWT event system
        var evt = new Events.Event
        {
            Widget = this,
            Type = _expanded ? SWT.Expand : SWT.Collapse
        };
        NotifyListeners(_expanded ? SWT.Expand : SWT.Collapse, evt);
    }

    /// <summary>
    /// Called by the parent when expanded state changes from platform.
    /// </summary>
    internal void NotifyExpanded(bool expanded)
    {
        if (_expanded != expanded)
        {
            _expanded = expanded;

            // Update control visibility
            if (_control != null)
            {
                _control.Visible = _expanded;
            }

            OnExpandedChanged(EventArgs.Empty);
        }
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

        // Dispose platform expand item
        _platformExpandItem?.Dispose();
        _platformExpandItem = null;

        base.ReleaseWidget();
    }
}
