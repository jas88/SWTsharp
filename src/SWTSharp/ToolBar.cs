namespace SWTSharp;

/// <summary>
/// Represents a toolbar container widget.
/// A toolbar is a selectable user interface component that displays a row or column of items,
/// where each item is either a button (with text and/or an image) or a separator.
/// </summary>
public class ToolBar : Composite
{
    private readonly List<ToolItem> _items = new();
    private IntPtr _toolBarHandle;

    /// <summary>
    /// Gets an array of all items in the toolbar.
    /// </summary>
    public ToolItem[] Items
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
    /// Gets the number of items in the toolbar.
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
    /// Creates a new toolbar with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite (cannot be null).</param>
    /// <param name="style">The toolbar style bits (FLAT, WRAP, RIGHT, HORIZONTAL, VERTICAL, SHADOW_OUT).</param>
    public ToolBar(Composite parent, int style) : base(parent, CheckStyle(style))
    {
        // CreateWidget is already called by base Composite constructor
    }

    private static int CheckStyle(int style)
    {
        // Ensure default orientation is HORIZONTAL if not specified
        if ((style & (SWT.HORIZONTAL | SWT.VERTICAL)) == 0)
        {
            style |= SWT.HORIZONTAL;
        }

        // FLAT is the default if no border style is specified
        if ((style & (SWT.FLAT | SWT.SHADOW_OUT)) == 0)
        {
            style |= SWT.FLAT;
        }

        return style;
    }

    /// <summary>
    /// Gets or sets the platform-specific toolbar handle.
    /// Note: ToolBar uses its own internal handle instead of the inherited Handle property
    /// to avoid conflicts with the Composite disposal chain.
    /// </summary>
    internal override IntPtr Handle
    {
        get => IntPtr.Zero; // Return Zero to prevent Composite.ReleaseWidget from destroying it
        set { } // Ignore sets from base class
    }

    /// <summary>
    /// Gets the actual platform-specific toolbar handle for use by ToolItem.
    /// </summary>
    internal IntPtr ToolBarHandle => _toolBarHandle;

    /// <summary>
    /// Creates the platform-specific toolbar widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // Create platform-specific toolbar handle (stored separately from inherited Handle)
        _toolBarHandle = Platform.PlatformFactory.Instance.CreateToolBar(Parent!.Handle, Style);
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <returns>The item at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When index is out of range.</exception>
    public ToolItem GetItem(int index)
    {
        CheckWidget();
        lock (_items)
        {
            if (index < 0 || index >= _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
            }
            return _items[index];
        }
    }

    /// <summary>
    /// Gets the number of items in the toolbar.
    /// </summary>
    /// <returns>The number of items.</returns>
    public int GetItemCount()
    {
        return ItemCount;
    }

    /// <summary>
    /// Returns the zero-based index of the item in the toolbar.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(ToolItem item)
    {
        CheckWidget();
        if (item == null)
        {
            return -1;
        }

        lock (_items)
        {
            return _items.IndexOf(item);
        }
    }

    /// <summary>
    /// Adds a tool item to the toolbar.
    /// This method is called internally when a ToolItem is created with this toolbar as parent.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="index">The index at which to insert the item, or -1 to append.</param>
    internal void AddItem(ToolItem item, int index)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            if (index < 0 || index >= _items.Count)
            {
                _items.Add(item);
            }
            else
            {
                _items.Insert(index, item);
            }
        }
    }

    /// <summary>
    /// Removes a tool item from the toolbar.
    /// This method is called internally when a ToolItem is disposed.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    internal void RemoveItem(ToolItem item)
    {
        if (item == null)
        {
            return;
        }

        lock (_items)
        {
            _items.Remove(item);
        }
    }

    /// <summary>
    /// Returns whether this is a FLAT toolbar.
    /// </summary>
    public bool IsFlat => (Style & SWT.FLAT) != 0;

    /// <summary>
    /// Returns whether this is a WRAP toolbar (items wrap to next line when needed).
    /// </summary>
    public bool IsWrap => (Style & SWT.WRAP) != 0;

    /// <summary>
    /// Returns whether text appears to the RIGHT of images.
    /// </summary>
    public bool IsRight => (Style & SWT.RIGHT) != 0;

    /// <summary>
    /// Returns whether this is a HORIZONTAL toolbar.
    /// </summary>
    public bool IsHorizontal => (Style & SWT.HORIZONTAL) != 0;

    /// <summary>
    /// Returns whether this is a VERTICAL toolbar.
    /// </summary>
    public bool IsVertical => (Style & SWT.VERTICAL) != 0;

    /// <summary>
    /// Returns whether this toolbar has SHADOW_OUT style.
    /// </summary>
    public bool IsShadowOut => (Style & SWT.SHADOW_OUT) != 0;

    protected override void UpdateVisible()
    {
        if (_toolBarHandle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetControlVisible(_toolBarHandle, Visible);
        }
    }

    protected override void UpdateEnabled()
    {
        if (_toolBarHandle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetControlEnabled(_toolBarHandle, Enabled);
        }
    }

    protected override void ReleaseWidget()
    {
        // Dispose all items
        lock (_items)
        {
            foreach (var item in _items.ToArray())
            {
                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _items.Clear();
        }

        // Destroy platform toolbar handle
        if (_toolBarHandle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.DestroyToolBar(_toolBarHandle);
            _toolBarHandle = IntPtr.Zero;
        }

        base.ReleaseWidget();
    }
}
