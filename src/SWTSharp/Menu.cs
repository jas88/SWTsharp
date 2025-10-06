namespace SWTSharp;

/// <summary>
/// Represents a menu that can be a menu bar, popup menu, or drop-down menu.
/// Menus contain menu items and can be attached to shells or controls.
/// </summary>
public class Menu : Widget
{
    private Widget? _parent;
    private MenuItem? _parentItem;
    private Menu? _parentMenu;
    private bool _visible;
    private readonly List<MenuItem> _items = new();
    private IntPtr _handle;

    /// <summary>
    /// Gets the parent widget of this menu.
    /// </summary>
    public Widget? Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets the parent menu item if this menu is a cascade submenu.
    /// </summary>
    public MenuItem? ParentItem
    {
        get
        {
            CheckWidget();
            return _parentItem;
        }
    }

    /// <summary>
    /// Gets the parent menu if this menu is a submenu.
    /// </summary>
    public Menu? ParentMenu
    {
        get
        {
            CheckWidget();
            return _parentMenu;
        }
    }

    /// <summary>
    /// Gets or sets whether the menu is visible.
    /// </summary>
    public bool Visible
    {
        get
        {
            CheckWidget();
            return _visible;
        }
        set
        {
            CheckWidget();
            if (_visible != value)
            {
                _visible = value;
                UpdateVisible();
            }
        }
    }

    /// <summary>
    /// Gets or sets the platform-specific menu handle.
    /// </summary>
    public override IntPtr Handle
    {
        get => _handle;
        protected set => _handle = value;
    }

    /// <summary>
    /// Gets the array of menu items contained in this menu.
    /// </summary>
    public MenuItem[] Items
    {
        get
        {
            CheckWidget();
            lock (_items)
            {
                return _items.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the number of items in this menu.
    /// </summary>
    public int ItemCount
    {
        get
        {
            CheckWidget();
            lock (_items)
            {
                return _items.Count;
            }
        }
    }

    /// <summary>
    /// Creates a menu bar for a Shell.
    /// </summary>
    /// <param name="parent">The shell that will contain this menu bar</param>
    public Menu(Shell parent) : this(parent, SWT.BAR)
    {
    }

    /// <summary>
    /// Creates a menu with the specified style for a Control.
    /// </summary>
    /// <param name="parent">The control that will own this menu</param>
    /// <param name="style">Menu style (BAR, DROP_DOWN, or POP_UP)</param>
    public Menu(Control parent, int style) : base(parent, CheckStyle(style))
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        CreateWidget();
    }

    /// <summary>
    /// Creates a menu bar for a Shell with the specified style.
    /// </summary>
    /// <param name="parent">The shell that will contain this menu</param>
    /// <param name="style">Menu style (typically BAR for menu bars)</param>
    public Menu(Shell parent, int style) : base(parent, CheckStyle(style))
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        CreateWidget();

        // If this is a menu bar, attach it to the shell
        if ((style & SWT.BAR) != 0)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetShellMenuBar(parent.Handle, _handle);
        }
    }

    /// <summary>
    /// Creates a submenu that will be attached to a menu item.
    /// </summary>
    /// <param name="parentItem">The cascade menu item that will contain this submenu</param>
    public Menu(MenuItem parentItem) : base(parentItem, SWT.DROP_DOWN)
    {
        if (parentItem == null)
        {
            throw new ArgumentNullException(nameof(parentItem));
        }

        _parentItem = parentItem;
        _parentMenu = parentItem.Parent;
        _parent = _parentMenu?._parent;
        CreateWidget();
    }

    /// <summary>
    /// Creates a submenu for a Menu.
    /// </summary>
    /// <param name="parentMenu">The parent menu</param>
    public Menu(Menu parentMenu) : base(parentMenu, SWT.DROP_DOWN)
    {
        if (parentMenu == null)
        {
            throw new ArgumentNullException(nameof(parentMenu));
        }

        _parentMenu = parentMenu;
        _parent = parentMenu._parent;
        CreateWidget();
    }

    private static int CheckStyle(int style)
    {
        // Ensure at least one valid menu style is set
        int mask = SWT.BAR | SWT.DROP_DOWN | SWT.POP_UP;
        if ((style & mask) == 0)
        {
            return style | SWT.POP_UP;
        }
        return style;
    }

    private void CreateWidget()
    {
        // Create platform-specific menu handle
        Handle = SWTSharp.Platform.PlatformFactory.Instance.CreateMenu(Style);
    }

    /// <summary>
    /// Gets the menu item at the specified index.
    /// </summary>
    public MenuItem GetItem(int index)
    {
        CheckWidget();
        lock (_items)
        {
            if (index < 0 || index >= _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _items[index];
        }
    }

    /// <summary>
    /// Gets the index of the specified menu item.
    /// </summary>
    public int IndexOf(MenuItem item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            return _items.IndexOf(item);
        }
    }

    /// <summary>
    /// Returns whether this is a menu bar.
    /// </summary>
    public bool IsMenuBar => (Style & SWT.BAR) != 0;

    /// <summary>
    /// Returns whether this is a popup menu.
    /// </summary>
    public bool IsPopupMenu => (Style & SWT.POP_UP) != 0;

    /// <summary>
    /// Returns whether this is a drop-down menu.
    /// </summary>
    public bool IsDropDown => (Style & SWT.DROP_DOWN) != 0;

    /// <summary>
    /// Sets the menu visible or hidden.
    /// </summary>
    public void SetVisible(bool visible)
    {
        Visible = visible;
    }

    /// <summary>
    /// Shows the popup menu at the specified location.
    /// </summary>
    /// <param name="x">X coordinate in screen coordinates</param>
    /// <param name="y">Y coordinate in screen coordinates</param>
    public void SetLocation(int x, int y)
    {
        CheckWidget();
        if (!IsPopupMenu)
        {
            throw new SWTException(SWT.ERROR_MENU_NOT_POP_UP);
        }

        SWTSharp.Platform.PlatformFactory.Instance.ShowPopupMenu(_handle, x, y);
    }

    /// <summary>
    /// Adds a menu item to this menu's internal list.
    /// Called by MenuItem during construction.
    /// </summary>
    internal void AddItem(MenuItem item)
    {
        lock (_items)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
            }
        }
    }

    /// <summary>
    /// Removes a menu item from this menu's internal list.
    /// Called by MenuItem during disposal.
    /// </summary>
    internal void RemoveItem(MenuItem item)
    {
        lock (_items)
        {
            _items.Remove(item);
        }
    }

    private void UpdateVisible()
    {
        if (_handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetMenuVisible(_handle, _visible);
        }
    }

    protected override void ReleaseWidget()
    {
        // Dispose all items
        lock (_items)
        {
            foreach (var item in _items.ToArray())
            {
                item.Dispose();
            }
            _items.Clear();
        }

        // Destroy platform menu
        if (_handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.DestroyMenu(_handle);
            _handle = IntPtr.Zero;
        }

        _parent = null;
        _parentItem = null;
        _parentMenu = null;

        base.ReleaseWidget();
    }
}
