namespace SWTSharp;

/// <summary>
/// Represents a selectable list of string items.
/// Supports single or multiple selection modes.
/// </summary>
public class List : Control
{
    private readonly System.Collections.Generic.List<string> _items = new();
    private readonly System.Collections.Generic.List<int> _selectedIndices = new();
    private bool _multiSelect;

    /// <summary>
    /// Gets the number of items in the list.
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
    /// Gets or sets all items in the list.
    /// </summary>
    public string[] Items
    {
        get
        {
            CheckWidget();
            return _items.ToArray();
        }
        set
        {
            CheckWidget();
            _items.Clear();
            _selectedIndices.Clear();
            if (value != null)
            {
                _items.AddRange(value);
                UpdateItems();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected items.
    /// </summary>
    public string[] Selection
    {
        get
        {
            CheckWidget();
            return _selectedIndices.Select(i => _items[i]).ToArray();
        }
        set
        {
            CheckWidget();
            _selectedIndices.Clear();
            if (value != null)
            {
                foreach (var item in value)
                {
                    int index = _items.IndexOf(item);
                    if (index >= 0)
                    {
                        _selectedIndices.Add(index);
                    }
                }
                UpdateSelection();
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the selected item (for single selection).
    /// Returns -1 if no item is selected.
    /// </summary>
    public int SelectionIndex
    {
        get
        {
            CheckWidget();
            return _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
        }
        set
        {
            CheckWidget();
            if (value < -1 || value >= _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Selection index out of range");
            }
            _selectedIndices.Clear();
            if (value >= 0)
            {
                _selectedIndices.Add(value);
            }
            UpdateSelection();
        }
    }

    /// <summary>
    /// Gets the selected indices.
    /// </summary>
    public int[] SelectionIndices
    {
        get
        {
            CheckWidget();
            return _selectedIndices.ToArray();
        }
        set
        {
            CheckWidget();
            _selectedIndices.Clear();
            if (value != null)
            {
                foreach (int index in value)
                {
                    if (index >= 0 && index < _items.Count)
                    {
                        if (_multiSelect || _selectedIndices.Count == 0)
                        {
                            _selectedIndices.Add(index);
                        }
                    }
                }
                UpdateSelection();
            }
        }
    }

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a new list control.
    /// </summary>
    /// <param name="parent">Parent control</param>
    /// <param name="style">Style flags (SWT.SINGLE or SWT.MULTI)</param>
    public List(Control parent, int style) : base(parent, style)
    {
        _multiSelect = (style & SWT.MULTI) != 0;
        CreateWidget();
    }

    /// <summary>
    /// Adds an item to the end of the list.
    /// </summary>
    /// <param name="item">Item to add</param>
    public void Add(string item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        _items.Add(item);
        Platform.PlatformFactory.Instance.AddListItem(Handle, item, _items.Count - 1);
    }

    /// <summary>
    /// Adds an item at a specific index.
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="index">Index where to insert the item</param>
    public void Add(string item, int index)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        if (index < 0 || index > _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        _items.Insert(index, item);
        Platform.PlatformFactory.Instance.AddListItem(Handle, item, index);

        // Update selected indices
        for (int i = 0; i < _selectedIndices.Count; i++)
        {
            if (_selectedIndices[i] >= index)
            {
                _selectedIndices[i]++;
            }
        }
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    public void Remove(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        _items.RemoveAt(index);
        Platform.PlatformFactory.Instance.RemoveListItem(Handle, index);

        // Update selected indices
        _selectedIndices.Remove(index);
        for (int i = 0; i < _selectedIndices.Count; i++)
        {
            if (_selectedIndices[i] > index)
            {
                _selectedIndices[i]--;
            }
        }
    }

    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">Item to remove</param>
    public void Remove(string item)
    {
        CheckWidget();
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            Remove(index);
        }
    }

    /// <summary>
    /// Removes a range of items.
    /// </summary>
    /// <param name="start">Start index</param>
    /// <param name="end">End index (inclusive)</param>
    public void Remove(int start, int end)
    {
        CheckWidget();
        if (start < 0 || start >= _items.Count || end < 0 || end >= _items.Count || start > end)
        {
            throw new ArgumentOutOfRangeException("Invalid range");
        }
        for (int i = end; i >= start; i--)
        {
            Remove(i);
        }
    }

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void RemoveAll()
    {
        CheckWidget();
        _items.Clear();
        _selectedIndices.Clear();
        Platform.PlatformFactory.Instance.ClearListItems(Handle);
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>The item string</returns>
    public string GetItem(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        return _items[index];
    }

    /// <summary>
    /// Gets all items in the list.
    /// </summary>
    /// <returns>Array of all items</returns>
    public string[] GetItems()
    {
        CheckWidget();
        return _items.ToArray();
    }

    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    /// <returns>Array of selected items</returns>
    public string[] GetSelection()
    {
        CheckWidget();
        return _selectedIndices.Select(i => _items[i]).ToArray();
    }

    /// <summary>
    /// Gets the index of the first selected item.
    /// </summary>
    /// <returns>Index of selected item, or -1 if none selected</returns>
    public int GetSelectionIndex()
    {
        CheckWidget();
        return SelectionIndex;
    }

    /// <summary>
    /// Gets all selected indices.
    /// </summary>
    /// <returns>Array of selected indices</returns>
    public int[] GetSelectionIndices()
    {
        CheckWidget();
        return _selectedIndices.ToArray();
    }

    /// <summary>
    /// Selects the item at the specified index.
    /// </summary>
    /// <param name="index">Index to select</param>
    public void Select(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            return;
        }
        if (!_multiSelect)
        {
            _selectedIndices.Clear();
        }
        if (!_selectedIndices.Contains(index))
        {
            _selectedIndices.Add(index);
        }
        UpdateSelection();
    }

    /// <summary>
    /// Selects a range of items.
    /// </summary>
    /// <param name="start">Start index</param>
    /// <param name="end">End index (inclusive)</param>
    public void Select(int start, int end)
    {
        CheckWidget();
        if (!_multiSelect || start < 0 || end >= _items.Count || start > end)
        {
            return;
        }
        for (int i = start; i <= end; i++)
        {
            if (!_selectedIndices.Contains(i))
            {
                _selectedIndices.Add(i);
            }
        }
        UpdateSelection();
    }

    /// <summary>
    /// Selects all items (multi-selection only).
    /// </summary>
    public void SelectAll()
    {
        CheckWidget();
        if (!_multiSelect)
        {
            return;
        }
        _selectedIndices.Clear();
        for (int i = 0; i < _items.Count; i++)
        {
            _selectedIndices.Add(i);
        }
        UpdateSelection();
    }

    /// <summary>
    /// Deselects the item at the specified index.
    /// </summary>
    /// <param name="index">Index to deselect</param>
    public void Deselect(int index)
    {
        CheckWidget();
        _selectedIndices.Remove(index);
        UpdateSelection();
    }

    /// <summary>
    /// Deselects all items.
    /// </summary>
    public void DeselectAll()
    {
        CheckWidget();
        _selectedIndices.Clear();
        UpdateSelection();
    }

    /// <summary>
    /// Returns the zero-relative index of the item which is currently at the top of the list.
    /// </summary>
    public int GetTopIndex()
    {
        CheckWidget();
        return Platform.PlatformFactory.Instance.GetListTopIndex(Handle);
    }

    /// <summary>
    /// Sets the zero-relative index of the item which is currently at the top of the list.
    /// </summary>
    public void SetTopIndex(int index)
    {
        CheckWidget();
        if (index >= 0 && index < _items.Count)
        {
            Platform.PlatformFactory.Instance.SetListTopIndex(Handle, index);
        }
    }

    /// <summary>
    /// Searches the list for an item starting at the specified index.
    /// </summary>
    public int IndexOf(string item, int start = 0)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        return _items.IndexOf(item, start);
    }

    private void CreateWidget()
    {
        Handle = Platform.PlatformFactory.Instance.CreateList(Parent?.Handle ?? IntPtr.Zero, Style);
        if (Handle == IntPtr.Zero)
        {
            throw new SWTException(SWT.ERROR_NO_HANDLES, "Failed to create list control");
        }
    }

    private void UpdateItems()
    {
        Platform.PlatformFactory.Instance.ClearListItems(Handle);
        for (int i = 0; i < _items.Count; i++)
        {
            Platform.PlatformFactory.Instance.AddListItem(Handle, _items[i], i);
        }
    }

    private void UpdateSelection()
    {
        Platform.PlatformFactory.Instance.SetListSelection(Handle, _selectedIndices.ToArray());
        OnSelectionChanged(EventArgs.Empty);
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }

    protected override void ReleaseWidget()
    {
        _items.Clear();
        _selectedIndices.Clear();
        base.ReleaseWidget();
    }
}
