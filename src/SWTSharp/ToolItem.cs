using SWTSharp.Graphics;
using SWTSharp.Platform;
using SWTSharp.Platform.MacOS;

namespace SWTSharp;

/// <summary>
/// Represents an individual item (button or control) in a toolbar.
/// Tool items can be push buttons, check buttons, radio buttons, drop-down buttons, separators,
/// or custom controls.
/// </summary>
public class ToolItem : Widget
{
    private ToolBar? _parent;
    private string _text = string.Empty;
    private Image? _image;
    private string _toolTipText = string.Empty;
    private bool _enabled = true;
    private bool _selection;
    private int _width;
    private Control? _control;
    private IPlatformToolItem? _platformToolItem;
    // Handle property removed - replaced with PlatformWidget
    private int _id;
    private static int _nextId = 2000;

    /// <summary>
    /// Event raised when the tool item is selected (clicked).
    /// </summary>
    public event EventHandler? Selection;

    /// <summary>
    /// Gets the parent toolbar of this tool item.
    /// </summary>
    public ToolBar? Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the tool item's text.
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
    /// Gets or sets the tool item's image.
    /// </summary>
    public Image? Image
    {
        get
        {
            CheckWidget();
            return _image;
        }
        set
        {
            CheckWidget();
            _image = value;
            UpdateImage();
        }
    }

    /// <summary>
    /// Gets or sets the tool item's tooltip text.
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
            UpdateToolTipText();
        }
    }

    /// <summary>
    /// Gets or sets whether the tool item is enabled.
    /// </summary>
    public bool Enabled
    {
        get
        {
            CheckWidget();
            return _enabled;
        }
        set
        {
            CheckWidget();
            if (_enabled != value)
            {
                _enabled = value;
                UpdateEnabled();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selection state for CHECK and RADIO tool items.
    /// </summary>
    public bool IsSelected
    {
        get
        {
            CheckWidget();
            if (!IsCheck && !IsRadio)
            {
                return false;
            }
            return _selection;
        }
        set
        {
            CheckWidget();
            if (!IsCheck && !IsRadio)
            {
                return;
            }

            if (_selection != value)
            {
                _selection = value;
                UpdateSelection();

                // If this is a radio item being selected, deselect siblings
                if (IsRadio && value && _parent != null)
                {
                    foreach (var item in _parent.Items)
                    {
                        if (item != this && item.IsRadio)
                        {
                            item.IsSelected = false;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the width for SEPARATOR tool items.
    /// </summary>
    public int Width
    {
        get
        {
            CheckWidget();
            if (!IsSeparator)
            {
                return 0;
            }
            return _width;
        }
        set
        {
            CheckWidget();
            if (!IsSeparator)
            {
                return;
            }

            if (_width != value)
            {
                _width = value;
                UpdateWidth();
            }
        }
    }

    /// <summary>
    /// Gets or sets the control for custom tool items.
    /// Only applicable for tool items created to hold a control.
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
            if (_control != value)
            {
                _control = value;
                UpdateControl();
            }
        }
    }

    // Handle property removed - replaced with PlatformWidget

    /// <summary>
    /// Gets the unique ID for this tool item.
    /// </summary>
    internal int Id => _id;

    /// <summary>
    /// Creates a tool item with the specified style, appending it to the toolbar.
    /// </summary>
    /// <param name="parent">The parent toolbar</param>
    /// <param name="style">The tool item style (PUSH, CHECK, RADIO, DROP_DOWN, SEPARATOR)</param>
    public ToolItem(ToolBar parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a tool item with the specified style at the specified index.
    /// </summary>
    /// <param name="parent">The parent toolbar</param>
    /// <param name="style">The tool item style (PUSH, CHECK, RADIO, DROP_DOWN, SEPARATOR)</param>
    /// <param name="index">The index at which to insert the item, or -1 to append</param>
    public ToolItem(ToolBar parent, int style, int index) : base(parent, CheckStyle(style))
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        _id = _nextId++;
        CreateWidget(index);
        _parent.AddItem(this, index);
    }

    private static int CheckStyle(int style)
    {
        // Ensure at least one valid tool item style is set
        int mask = SWT.PUSH | SWT.CHECK | SWT.RADIO | SWT.DROP_DOWN | SWT.SEPARATOR;
        if ((style & mask) == 0)
        {
            return style | SWT.PUSH;
        }
        return style;
    }

    private void CreateWidget(int index)
    {
        if (_parent == null)
        {
            return;
        }

        // NOTE: ToolItem creation now happens through ToolBar which creates
        // platform-specific tool items automatically. This CreateWidget method
        // is a placeholder - actual platform items are created by ToolBar.AddItem()
        // which internally calls the platform-specific IPlatformToolBar.AddItem()

        // Platform-specific ToolItem creation happens automatically when ToolBar
        // creates items, so this method is intentionally minimal.
        // The _platformToolItem field is populated by the ToolBar when it creates items.
    }

    /// <summary>
    /// Returns whether this is a PUSH tool item.
    /// </summary>
    public bool IsPush => (Style & SWT.PUSH) != 0;

    /// <summary>
    /// Returns whether this is a CHECK tool item.
    /// </summary>
    public bool IsCheck => (Style & SWT.CHECK) != 0;

    /// <summary>
    /// Returns whether this is a RADIO tool item.
    /// </summary>
    public bool IsRadio => (Style & SWT.RADIO) != 0;

    /// <summary>
    /// Returns whether this is a DROP_DOWN tool item.
    /// </summary>
    public bool IsDropDown => (Style & SWT.DROP_DOWN) != 0;

    /// <summary>
    /// Returns whether this is a SEPARATOR tool item.
    /// </summary>
    public bool IsSeparator => (Style & SWT.SEPARATOR) != 0;

    /// <summary>
    /// Sets the enabled state of this tool item.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Sets the selection state for CHECK and RADIO tool items.
    /// </summary>
    public void SetSelection(bool selected)
    {
        IsSelected = selected;
    }

    /// <summary>
    /// Sets the text of this tool item.
    /// </summary>
    public void SetText(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Sets the image of this tool item.
    /// </summary>
    public void SetImage(Image? image)
    {
        Image = image;
    }

    /// <summary>
    /// Sets the tooltip text of this tool item.
    /// </summary>
    public void SetToolTipText(string text)
    {
        ToolTipText = text;
    }

    /// <summary>
    /// Sets the control for this tool item.
    /// </summary>
    public void SetControl(Control? control)
    {
        Control = control;
    }

    /// <summary>
    /// Sets the width for SEPARATOR tool items.
    /// </summary>
    public void SetWidth(int width)
    {
        Width = width;
    }

    /// <summary>
    /// Raises the Selection event.
    /// Called by the platform when the tool item is clicked.
    /// </summary>
    internal void OnSelection()
    {
        if (IsCheck || IsRadio)
        {
            _selection = !_selection;
            UpdateSelection();
        }

        Selection?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateText()
    {
        // Use IPlatformToolItem interface to update text
        if (_platformToolItem != null)
        {
            _platformToolItem.SetText(_text);
        }
    }

    private void UpdateImage()
    {
        // Use IPlatformToolItem interface to update image
        if (_platformToolItem != null && _image != null)
        {
            var imageAdapter = new MacOSImage(_image);
            _platformToolItem.SetImage(imageAdapter);
        }
    }

    private void UpdateToolTipText()
    {
        // TODO: Add tooltip support to IPlatformToolItem interface
        // For now, tooltip text is stored but not implemented in platform widget
        // if (_platformToolItem != null)
        // {
        //     _platformToolItem.SetToolTip(_toolTipText);
        // }
    }

    private void UpdateSelection()
    {
        // TODO: Add selection support to IPlatformToolItem interface for CHECK/RADIO items
        // For now, selection state is stored but not implemented in platform widget
        // if (_platformToolItem != null && (IsCheck || IsRadio))
        // {
        //     _platformToolItem.SetSelected(_selection);
        // }
    }

    private void UpdateEnabled()
    {
        // Use IPlatformToolItem interface to update enabled state
        if (_platformToolItem != null)
        {
            _platformToolItem.SetEnabled(_enabled);
        }
    }

    private void UpdateWidth()
    {
        // TODO: Add width support to IPlatformToolItem interface for SEPARATOR items
        // For now, width is stored but not implemented in platform widget
        // if (_platformToolItem != null && IsSeparator)
        // {
        //     _platformToolItem.SetWidth(_width);
        // }
    }

    private void UpdateControl()
    {
        // TODO: Add control support to IPlatformToolItem interface for custom tool items
        // For now, control is stored but not implemented in platform widget
        // if (_platformToolItem != null && _control != null)
        // {
        //     _platformToolItem.SetControl(_control.PlatformWidget);
        // }
    }

    protected override void ReleaseWidget()
    {
        // Remove from parent toolbar
        _parent?.RemoveItem(this);

        // Release control reference (don't dispose it, it belongs to the user)
        _control = null;

        // Release image reference (don't dispose it, it may be shared)
        _image = null;

        // Dispose platform tool item
        _platformToolItem?.Dispose();
        _platformToolItem = null;

        _parent = null;

        base.ReleaseWidget();
    }
}
