namespace SWTSharp;

/// <summary>
/// Represents an item in a menu.
/// Menu items can be push buttons, check boxes, radio buttons, cascade menus, or separators.
/// </summary>
public class MenuItem : Widget
{
    private Menu? _parent;
    private string _text = string.Empty;
    private bool _selection;
    private bool _enabled = true;
    private Menu? _menu;
    // Handle property removed - replaced with PlatformWidget
    private int _id;
    private static int _nextId = 1000;

    /// <summary>
    /// Event raised when the menu item is selected (clicked).
    /// </summary>
    public event EventHandler? Selection;

    /// <summary>
    /// Gets the parent menu of this menu item.
    /// </summary>
    public Menu? Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the menu item's text.
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
            // TODO: Implement text updates through platform widget interface
        }
    }

    /// <summary>
    /// Gets or sets the selection state for CHECK and RADIO menu items.
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
                // TODO: Implement selection updates through platform widget interface

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
    /// Gets or sets whether the menu item is enabled.
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
                // TODO: Implement enabled state updates through platform widget interface
            }
        }
    }

    /// <summary>
    /// Gets or sets the submenu for CASCADE menu items.
    /// </summary>
    public Menu? Menu
    {
        get
        {
            CheckWidget();
            return _menu;
        }
        set
        {
            CheckWidget();
            if (!IsCascade)
            {
                throw new SWTException(SWT.ERROR_MENUITEM_NOT_CASCADE);
            }

            if (_menu != value)
            {
                _menu = value;
                // TODO: Implement submenu updates through platform widget interface
            }
        }
    }

    // Handle property removed - replaced with PlatformWidget

    /// <summary>
    /// Gets the unique ID for this menu item.
    /// </summary>
    internal int Id => _id;

    /// <summary>
    /// Creates a menu item with the specified style.
    /// </summary>
    /// <param name="parent">The parent menu</param>
    /// <param name="style">The menu item style (PUSH, CHECK, RADIO, CASCADE, SEPARATOR)</param>
    public MenuItem(Menu parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a menu item with the specified style at the specified index.
    /// </summary>
    /// <param name="parent">The parent menu</param>
    /// <param name="style">The menu item style (PUSH, CHECK, RADIO, CASCADE, SEPARATOR)</param>
    /// <param name="index">The index at which to insert the item, or -1 to append</param>
    public MenuItem(Menu parent, int style, int index) : base(parent, CheckStyle(style))
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        _id = _nextId++;
        CreateWidget(index);
        _parent.AddItem(this);
    }

    private static int CheckStyle(int style)
    {
        // Ensure at least one valid menu item style is set
        int mask = SWT.PUSH | SWT.CHECK | SWT.RADIO | SWT.CASCADE | SWT.SEPARATOR;
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

        // TODO: Implement menu item creation through platform widget interface
        // Handle property removed - replaced with PlatformWidget
    }

    /// <summary>
    /// Returns whether this is a PUSH menu item.
    /// </summary>
    public bool IsPush => (Style & SWT.PUSH) != 0;

    /// <summary>
    /// Returns whether this is a CHECK menu item.
    /// </summary>
    public bool IsCheck => (Style & SWT.CHECK) != 0;

    /// <summary>
    /// Returns whether this is a RADIO menu item.
    /// </summary>
    public bool IsRadio => (Style & SWT.RADIO) != 0;

    /// <summary>
    /// Returns whether this is a CASCADE menu item.
    /// </summary>
    public bool IsCascade => (Style & SWT.CASCADE) != 0;

    /// <summary>
    /// Returns whether this is a SEPARATOR menu item.
    /// </summary>
    public bool IsSeparator => (Style & SWT.SEPARATOR) != 0;

    /// <summary>
    /// Sets the submenu for this CASCADE menu item.
    /// </summary>
    public void SetMenu(Menu menu)
    {
        Menu = menu;
    }

    /// <summary>
    /// Sets the enabled state of this menu item.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }

    /// <summary>
    /// Sets the selection state for CHECK and RADIO menu items.
    /// </summary>
    public void SetSelection(bool selected)
    {
        IsSelected = selected;
    }

    /// <summary>
    /// Sets the text of this menu item.
    /// </summary>
    public void SetText(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Raises the Selection event.
    /// Called by the platform when the menu item is clicked.
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

    /// <summary>
    /// Updates the selection state of the menu item.
    /// </summary>
    private void UpdateSelection()
    {
        // TODO: Implement selection updates through platform widget interface
        // Platform.PlatformFactory.Instance.SetMenuItemSelection(_handle, _selection);
    }

    protected override void ReleaseWidget()
    {
        // Remove from parent menu
        _parent?.RemoveItem(this);

        // Dispose submenu if cascade
        if (_menu != null)
        {
            _menu.Dispose();
            _menu = null;
        }

        // TODO: Implement menu item disposal through platform widget interface

        _parent = null;

        base.ReleaseWidget();
    }
}
