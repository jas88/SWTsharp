namespace SWTSharp;

/// <summary>
/// Represents a hierarchical tree control that displays a tree of items.
/// Supports single or multiple selection modes and optional checkboxes.
/// </summary>
public class Tree : Composite
{
    private readonly System.Collections.Generic.List<TreeItem> _items = new();
    private readonly System.Collections.Generic.List<TreeItem> _selection = new();
    private bool _multiSelect;
    private bool _check;

    /// <summary>
    /// Gets the number of root items in the tree.
    /// </summary>
    public int ItemCount
    {
        get
        {
            CheckWidget();
            return _items.Count;
        }
    }

    /// <summary>
    /// Gets the root items in the tree.
    /// </summary>
    public TreeItem[] Items
    {
        get
        {
            CheckWidget();
            return _items.ToArray();
        }
    }

    /// <summary>
    /// Gets or sets the selected items in the tree.
    /// </summary>
    public TreeItem[] Selection
    {
        get
        {
            CheckWidget();
            return _selection.ToArray();
        }
        set
        {
            CheckWidget();
            _selection.Clear();
            if (value != null)
            {
                foreach (var item in value)
                {
                    if (item != null && item.ParentTree == this && !_selection.Contains(item))
                    {
                        if (!_multiSelect && _selection.Count > 0)
                        {
                            break;
                        }
                        _selection.Add(item);
                    }
                }
                UpdateSelection();
            }
        }
    }

    /// <summary>
    /// Gets the number of selected items.
    /// </summary>
    public int SelectionCount
    {
        get
        {
            CheckWidget();
            return _selection.Count;
        }
    }

    /// <summary>
    /// Gets or sets the index of the selected item for single-selection trees.
    /// Returns -1 if no item is selected.
    /// </summary>
    public int SelectionIndex
    {
        get
        {
            CheckWidget();
            if (_selection.Count == 0)
            {
                return -1;
            }
            return GetItemIndex(_selection[0]);
        }
        set
        {
            CheckWidget();
            if (value < 0)
            {
                _selection.Clear();
                UpdateSelection();
                return;
            }

            var item = FindItemAtIndex(value);
            if (item != null)
            {
                SetSelection(item);
            }
        }
    }

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Occurs when a tree item is expanded.
    /// </summary>
    public event EventHandler<TreeEventArgs>? Expand;

    /// <summary>
    /// Occurs when a tree item is collapsed.
    /// </summary>
    public event EventHandler<TreeEventArgs>? Collapse;

    /// <summary>
    /// Creates a new tree control.
    /// </summary>
    /// <param name="parent">Parent composite</param>
    /// <param name="style">Style flags (SWT.SINGLE, SWT.MULTI, SWT.CHECK)</param>
    public Tree(Composite parent, int style) : base(parent, style)
    {
        _multiSelect = (style & SWT.MULTI) != 0;
        _check = (style & SWT.CHECK) != 0;
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific tree control.
    /// </summary>
    protected override void CreateWidget()
    {
        Handle = Platform.PlatformFactory.Instance.CreateTree(Parent?.Handle ?? IntPtr.Zero, Style);
        if (Handle == IntPtr.Zero)
        {
            throw new SWTException(SWT.ERROR_NO_HANDLES, "Failed to create tree control");
        }
    }

    /// <summary>
    /// Gets the root items in the tree.
    /// </summary>
    /// <returns>Array of root tree items</returns>
    public TreeItem[] GetItems()
    {
        CheckWidget();
        return _items.ToArray();
    }

    /// <summary>
    /// Gets the root item at the specified index.
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>The tree item at the specified index</returns>
    public TreeItem GetItem(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        return _items[index];
    }

    /// <summary>
    /// Gets the number of root items in the tree.
    /// </summary>
    /// <returns>The number of root items</returns>
    public int GetItemCount()
    {
        CheckWidget();
        return _items.Count;
    }

    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    /// <returns>Array of selected items</returns>
    public TreeItem[] GetSelection()
    {
        CheckWidget();
        return _selection.ToArray();
    }

    /// <summary>
    /// Gets the number of selected items.
    /// </summary>
    /// <returns>The number of selected items</returns>
    public int GetSelectionCount()
    {
        CheckWidget();
        return _selection.Count;
    }

    /// <summary>
    /// Sets the selection to the specified items.
    /// </summary>
    /// <param name="items">Items to select</param>
    public void SetSelection(TreeItem[] items)
    {
        CheckWidget();
        _selection.Clear();
        if (items != null)
        {
            foreach (var item in items)
            {
                if (item != null && !_selection.Contains(item))
                {
                    if (!_multiSelect && _selection.Count > 0)
                    {
                        break;
                    }
                    _selection.Add(item);
                }
            }
            UpdateSelection();
        }
    }

    /// <summary>
    /// Sets the selection to a single item.
    /// </summary>
    /// <param name="item">Item to select</param>
    public void SetSelection(TreeItem item)
    {
        CheckWidget();
        if (item == null)
        {
            _selection.Clear();
        }
        else
        {
            SetSelection(new TreeItem[] { item });
        }
    }

    /// <summary>
    /// Deselects all items in the tree.
    /// </summary>
    public void DeselectAll()
    {
        CheckWidget();
        _selection.Clear();
        UpdateSelection();
    }

    /// <summary>
    /// Selects all items in the tree (multi-selection only).
    /// </summary>
    public void SelectAll()
    {
        CheckWidget();
        if (!_multiSelect)
        {
            return;
        }
        _selection.Clear();
        AddAllItemsToSelection(_items);
        UpdateSelection();
    }

    /// <summary>
    /// Recursively adds all items to the selection.
    /// </summary>
    private void AddAllItemsToSelection(System.Collections.Generic.List<TreeItem> items)
    {
        foreach (var item in items)
        {
            if (!_selection.Contains(item))
            {
                _selection.Add(item);
            }
            if (item.ItemCount > 0)
            {
                AddAllItemsToSelection(item.GetItemsList());
            }
        }
    }

    /// <summary>
    /// Shows the specified item, scrolling if necessary.
    /// </summary>
    /// <param name="item">Item to show</param>
    public void ShowItem(TreeItem item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        Platform.PlatformFactory.Instance.ShowTreeItem(Handle, item.Handle);
    }

    /// <summary>
    /// Removes all items from the tree.
    /// </summary>
    public void RemoveAll()
    {
        CheckWidget();
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();
        _selection.Clear();
        Platform.PlatformFactory.Instance.ClearTreeItems(Handle);
    }

    /// <summary>
    /// Adds a root item to the tree.
    /// Called internally by TreeItem constructors.
    /// </summary>
    internal void AddItem(TreeItem item, int index)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (index < 0 || index > _items.Count)
        {
            _items.Add(item);
        }
        else
        {
            _items.Insert(index, item);
        }
    }

    /// <summary>
    /// Removes a root item from the tree.
    /// Called internally when a TreeItem is disposed.
    /// </summary>
    internal void RemoveItem(TreeItem item)
    {
        if (item != null)
        {
            _items.Remove(item);
            _selection.Remove(item);
        }
    }

    /// <summary>
    /// Adds an item to the current selection.
    /// Called internally by TreeItem when selection changes.
    /// </summary>
    internal void AddToSelection(TreeItem item)
    {
        if (item == null)
        {
            return;
        }

        if (!_multiSelect)
        {
            _selection.Clear();
        }

        if (!_selection.Contains(item))
        {
            _selection.Add(item);
            UpdateSelection();
        }
    }

    /// <summary>
    /// Removes an item from the current selection.
    /// Called internally by TreeItem when selection changes.
    /// </summary>
    internal void RemoveFromSelection(TreeItem item)
    {
        if (item != null && _selection.Remove(item))
        {
            UpdateSelection();
        }
    }

    /// <summary>
    /// Updates the platform selection state.
    /// </summary>
    private void UpdateSelection()
    {
        var handles = _selection.Select(item => item.Handle).ToArray();
        Platform.PlatformFactory.Instance.SetTreeSelection(Handle, handles);
        OnSelectionChanged(EventArgs.Empty);
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
        NotifyListeners(SWT.Selection, new Events.Event { Widget = this });
    }

    /// <summary>
    /// Raises the Expand event.
    /// </summary>
    internal void OnExpand(TreeItem item)
    {
        var args = new TreeEventArgs(item);
        Expand?.Invoke(this, args);
        NotifyListeners(SWT.Expand, new Events.Event { Widget = this, Item = item });
    }

    /// <summary>
    /// Raises the Collapse event.
    /// </summary>
    internal void OnCollapse(TreeItem item)
    {
        var args = new TreeEventArgs(item);
        Collapse?.Invoke(this, args);
        NotifyListeners(SWT.Collapse, new Events.Event { Widget = this, Item = item });
    }

    /// <summary>
    /// Returns whether the tree is in check mode.
    /// </summary>
    internal bool IsCheckStyle => _check;

    /// <summary>
    /// Gets the flattened index of a tree item.
    /// </summary>
    private int GetItemIndex(TreeItem item)
    {
        int index = 0;
        foreach (var rootItem in _items)
        {
            if (rootItem == item)
            {
                return index;
            }
            index++;

            index = GetItemIndexRecursive(rootItem, item, ref index);
            if (index >= 0)
            {
                return index;
            }
        }
        return -1;
    }

    /// <summary>
    /// Recursively searches for an item and returns its flattened index.
    /// </summary>
    private int GetItemIndexRecursive(TreeItem parent, TreeItem target, ref int currentIndex)
    {
        for (int i = 0; i < parent.ItemCount; i++)
        {
            var child = parent.GetItem(i);
            if (child == target)
            {
                return currentIndex;
            }
            currentIndex++;

            int found = GetItemIndexRecursive(child, target, ref currentIndex);
            if (found >= 0)
            {
                return found;
            }
        }
        return -1;
    }

    /// <summary>
    /// Finds a tree item at the specified flattened index.
    /// </summary>
    private TreeItem? FindItemAtIndex(int targetIndex)
    {
        int currentIndex = 0;
        foreach (var rootItem in _items)
        {
            if (currentIndex == targetIndex)
            {
                return rootItem;
            }
            currentIndex++;

            var found = FindItemAtIndexRecursive(rootItem, targetIndex, ref currentIndex);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    /// <summary>
    /// Recursively searches for an item at a flattened index.
    /// </summary>
    private TreeItem? FindItemAtIndexRecursive(TreeItem parent, int targetIndex, ref int currentIndex)
    {
        for (int i = 0; i < parent.ItemCount; i++)
        {
            var child = parent.GetItem(i);
            if (currentIndex == targetIndex)
            {
                return child;
            }
            currentIndex++;

            var found = FindItemAtIndexRecursive(child, targetIndex, ref currentIndex);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    protected override void ReleaseWidget()
    {
        // Dispose all items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();
        _selection.Clear();

        base.ReleaseWidget();
    }
}

/// <summary>
/// Event arguments for tree expand/collapse events.
/// </summary>
public class TreeEventArgs : EventArgs
{
    /// <summary>
    /// Gets the tree item that was expanded or collapsed.
    /// </summary>
    public TreeItem Item { get; }

    /// <summary>
    /// Creates new tree event arguments.
    /// </summary>
    /// <param name="item">The tree item</param>
    public TreeEventArgs(TreeItem item)
    {
        Item = item;
    }
}
