namespace SWTSharp;

/// <summary>
/// Represents a selectable user interface table with rows and columns.
/// Supports single or multiple selection, checkboxes, and customizable columns.
/// </summary>
public class Table : Composite
{
    private readonly System.Collections.Generic.List<TableColumn> _columns = new();
    private readonly System.Collections.Generic.List<TableItem> _items = new();
    private readonly System.Collections.Generic.List<int> _selectedIndices = new();
    private bool _headerVisible = true;
    private bool _linesVisible = true;

    /// <summary>
    /// Gets the number of columns in the table.
    /// </summary>
    public int ColumnCount
    {
        get
        {
            CheckWidget();
            return _columns.Count;
        }
    }

    /// <summary>
    /// Gets the number of items (rows) in the table.
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
    /// Gets or sets whether the table header is visible.
    /// </summary>
    public bool HeaderVisible
    {
        get
        {
            CheckWidget();
            return _headerVisible;
        }
        set
        {
            CheckWidget();
            if (_headerVisible != value)
            {
                _headerVisible = value;
                // TODO: Implement platform widget interface for SetTableHeaderVisible
                // Platform.PlatformFactory.Instance.SetTableHeaderVisible(Handle, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether grid lines are visible.
    /// </summary>
    public bool LinesVisible
    {
        get
        {
            CheckWidget();
            return _linesVisible;
        }
        set
        {
            CheckWidget();
            if (_linesVisible != value)
            {
                _linesVisible = value;
                // TODO: Implement platform widget interface for SetTableLinesVisible
                // Platform.PlatformFactory.Instance.SetTableLinesVisible(Handle, value);
            }
        }
    }

    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    public TableItem[] Selection
    {
        get
        {
            CheckWidget();
            return _selectedIndices.Select(i => _items[i]).ToArray();
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
            return _selectedIndices.Count;
        }
    }

    /// <summary>
    /// Gets or sets the index of the selected item for single-selection tables.
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
            SetSelection(value);
        }
    }

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a new table control.
    /// </summary>
    /// <param name="parent">Parent composite</param>
    /// <param name="style">Style flags (SWT.SINGLE, SWT.MULTI, SWT.CHECK, SWT.FULL_SELECTION, SWT.HIDE_SELECTION)</param>
    public Table(Composite parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific table widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // TODO: Implement platform widget interface for CreateTable
        // TODO: Create IPlatformTable widget here
        // PlatformWidget = Platform.PlatformFactory.Instance.CreateTableWidget(Parent?.PlatformWidget, Style);

        // Set initial properties
        // TODO: Implement platform widget interface for SetTableHeaderVisible
        // TODO: Implement platform widget interface for SetTableLinesVisible
    }

    /// <summary>
    /// Gets all columns in the table.
    /// </summary>
    /// <returns>Array of table columns</returns>
    public TableColumn[] GetColumns()
    {
        CheckWidget();
        lock (_columns)
        {
            return _columns.ToArray();
        }
    }

    /// <summary>
    /// Gets the column at the specified index.
    /// </summary>
    /// <param name="index">Column index</param>
    /// <returns>The table column</returns>
    public TableColumn GetColumn(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _columns.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        lock (_columns)
        {
            return _columns[index];
        }
    }

    /// <summary>
    /// Gets all items (rows) in the table.
    /// </summary>
    /// <returns>Array of table items</returns>
    public TableItem[] GetItems()
    {
        CheckWidget();
        lock (_items)
        {
            return _items.ToArray();
        }
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>The table item</returns>
    public TableItem GetItem(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        lock (_items)
        {
            return _items[index];
        }
    }

    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    /// <returns>Array of selected items</returns>
    public TableItem[] GetSelection()
    {
        CheckWidget();
        return Selection;
    }

    /// <summary>
    /// Sets the table's selection to the specified items.
    /// </summary>
    /// <param name="items">Items to select</param>
    public void SetSelection(TableItem[] items)
    {
        CheckWidget();
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        _selectedIndices.Clear();
        foreach (var item in items)
        {
            lock (_items)
            {
                int index = _items.IndexOf(item);
                if (index >= 0)
                {
                    _selectedIndices.Add(index);
                }
            }
        }
        UpdateSelection();
    }

    /// <summary>
    /// Sets the table's selection to a single item.
    /// </summary>
    /// <param name="item">Item to select</param>
    public void SetSelection(TableItem item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        SetSelection(new[] { item });
    }

    /// <summary>
    /// Sets the table's selection to the item at the specified index.
    /// </summary>
    /// <param name="index">Index to select</param>
    public void SetSelection(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            return;
        }
        _selectedIndices.Clear();
        _selectedIndices.Add(index);
        UpdateSelection();
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

        lock (_items)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            item.Dispose();

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
    /// Removes all items from the table.
    /// </summary>
    public void RemoveAll()
    {
        CheckWidget();
        lock (_items)
        {
            foreach (var item in _items.ToArray())
            {
                item.Dispose();
            }
            _items.Clear();
            _selectedIndices.Clear();
        }
        // TODO: Implement platform widget interface for ClearTableItems
        // Platform.PlatformFactory.Instance.ClearTableItems(Handle);
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
    /// Scrolls the table to show the specified item.
    /// </summary>
    /// <param name="item">Item to show</param>
    public void ShowItem(TableItem item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            int index = _items.IndexOf(item);
            if (index >= 0)
            {
                // TODO: Implement platform widget interface for ShowTableItem
                // Platform.PlatformFactory.Instance.ShowTableItem(Handle, index);
            }
        }
    }

    /// <summary>
    /// Adds a column to this table.
    /// This method is called internally when a TableColumn is created.
    /// </summary>
    /// <param name="column">The column to add</param>
    internal void AddColumn(TableColumn column)
    {
        if (column == null)
        {
            throw new ArgumentNullException(nameof(column));
        }

        lock (_columns)
        {
            _columns.Add(column);
        }
    }

    /// <summary>
    /// Adds a column at the specified index.
    /// </summary>
    /// <param name="column">The column to add</param>
    /// <param name="index">Index at which to insert</param>
    internal void AddColumn(TableColumn column, int index)
    {
        if (column == null)
        {
            throw new ArgumentNullException(nameof(column));
        }

        lock (_columns)
        {
            if (index < 0 || index > _columns.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _columns.Insert(index, column);
        }
    }

    /// <summary>
    /// Removes a column from this table.
    /// </summary>
    /// <param name="column">The column to remove</param>
    internal void RemoveColumn(TableColumn column)
    {
        if (column == null)
        {
            return;
        }

        lock (_columns)
        {
            _columns.Remove(column);
        }
    }

    /// <summary>
    /// Adds an item to this table.
    /// This method is called internally when a TableItem is created.
    /// </summary>
    /// <param name="item">The item to add</param>
    internal void AddItem(TableItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            _items.Add(item);
        }
    }

    /// <summary>
    /// Adds an item at the specified index.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <param name="index">Index at which to insert</param>
    internal void AddItem(TableItem item, int index)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            if (index < 0 || index > _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _items.Insert(index, item);

            // Update selected indices
            for (int i = 0; i < _selectedIndices.Count; i++)
            {
                if (_selectedIndices[i] >= index)
                {
                    _selectedIndices[i]++;
                }
            }
        }
    }

    /// <summary>
    /// Removes an item from this table.
    /// </summary>
    /// <param name="item">The item to remove</param>
    internal void RemoveItem(TableItem item)
    {
        if (item == null)
        {
            return;
        }

        lock (_items)
        {
            int index = _items.IndexOf(item);
            if (index >= 0)
            {
                _items.RemoveAt(index);

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
        }
    }

    /// <summary>
    /// Gets the index of a table item.
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The item's index, or -1 if not found</returns>
    internal int GetItemIndex(TableItem item)
    {
        lock (_items)
        {
            return _items.IndexOf(item);
        }
    }

    /// <summary>
    /// Gets the index of a table column.
    /// </summary>
    /// <param name="column">The column</param>
    /// <returns>The column's index, or -1 if not found</returns>
    internal int GetColumnIndex(TableColumn column)
    {
        lock (_columns)
        {
            return _columns.IndexOf(column);
        }
    }

    private void UpdateSelection()
    {
        // TODO: Implement platform widget interface for SetTableSelection
        // Platform.PlatformFactory.Instance.SetTableSelection(Handle, _selectedIndices.ToArray());
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
        // Dispose all columns
        lock (_columns)
        {
            foreach (var column in _columns.ToArray())
            {
                column.Dispose();
            }
            _columns.Clear();
        }

        // Dispose all items
        lock (_items)
        {
            foreach (var item in _items.ToArray())
            {
                item.Dispose();
            }
            _items.Clear();
        }

        _selectedIndices.Clear();

        base.ReleaseWidget();
    }
}
