using SWTSharp.Platform;
using SWTSharp.Events;

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

        // Use IPlatformList interface to add item
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.AddItem(item);
        }
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

        // For IPlatformList interface, we need to refresh all items since it doesn't support insert at index
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.ClearItems();
            for (int i = 0; i < _items.Count; i++)
            {
                listWidget.AddItem(_items[i]);
            }
        }

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

        // Use IPlatformList interface to refresh items after removal
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.ClearItems();
            for (int i = 0; i < _items.Count; i++)
            {
                listWidget.AddItem(_items[i]);
            }
        }

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

        // Use IPlatformList interface to clear items
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.ClearItems();
        }
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
        // TODO: Implement platform widget interface for getting list top index
        // return Platform.PlatformFactory.Instance.GetListTopIndex(Handle);
        return 0;
    }

    /// <summary>
    /// Sets the zero-relative index of the item which is currently at the top of the list.
    /// </summary>
    public void SetTopIndex(int index)
    {
        CheckWidget();
        if (index >= 0 && index < _items.Count)
        {
            // TODO: Implement platform widget interface for setting list top index
            // Platform.PlatformFactory.Instance.SetListTopIndex(Handle, index);
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
        // Create IPlatformList widget using platform widget interface
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateListWidget(parentWidget, Style);

        // Subscribe to platform widget events
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.SelectionChanged += OnPlatformSelectionChanged;
            listWidget.ItemDoubleClick += OnPlatformItemDoubleClick;
        }

        // Connect standard widget events
        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.Click += OnPlatformClick;
            eventHandling.FocusGained += OnPlatformFocusGained;
            eventHandling.FocusLost += OnPlatformFocusLost;
            eventHandling.KeyDown += OnPlatformKeyDown;
            eventHandling.KeyUp += OnPlatformKeyUp;
        }
    }

    private void UpdateItems()
    {
        // Use IPlatformList interface to update items
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.ClearItems();
            for (int i = 0; i < _items.Count; i++)
            {
                listWidget.AddItem(_items[i]);
            }
        }
    }

    private void UpdateSelection()
    {
        // Use IPlatformList interface to update selection
        if (PlatformWidget is IPlatformList listWidget)
        {
            if (_multiSelect)
            {
                listWidget.SelectionIndices = _selectedIndices.ToArray();
            }
            else
            {
                listWidget.SelectionIndex = _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
            }
        }
        OnSelectionChanged(EventArgs.Empty);
    }

    /// <summary>
    /// Handles platform widget selection changed events.
    /// </summary>
    private void OnPlatformSelectionChanged(object? sender, int selectedIndex)
    {
        CheckWidget();

        // Handle single selection mode
        if (!_multiSelect)
        {
            if (_selectedIndices.Count != 1 || _selectedIndices[0] != selectedIndex)
            {
                _selectedIndices.Clear();
                if (selectedIndex >= 0 && selectedIndex < _items.Count)
                {
                    _selectedIndices.Add(selectedIndex);
                }

                // Create SWT Selection event
                var selectionEvent = new Event
                {
                    Index = selectedIndex,
                    Time = Environment.TickCount,
                    Item = selectedIndex >= 0 ? _items[selectedIndex] : null
                };
                NotifyListeners(SWT.Selection, selectionEvent);

                // Raise the legacy SelectionChanged event for backwards compatibility
                OnSelectionChanged(EventArgs.Empty);
            }
        }
        else
        {
            // Multi-selection mode - toggle selection
            if (selectedIndex >= 0 && selectedIndex < _items.Count)
            {
                if (!_selectedIndices.Remove(selectedIndex))
                {
                    _selectedIndices.Add(selectedIndex);
                }

                // Create SWT Selection event
                var selectionEvent = new Event
                {
                    Index = selectedIndex,
                    Time = Environment.TickCount,
                    Item = _items[selectedIndex],
                    Detail = _selectedIndices.Contains(selectedIndex) ? SWT.NONE : SWT.NONE
                };
                NotifyListeners(SWT.Selection, selectionEvent);

                // Raise the legacy SelectionChanged event for backwards compatibility
                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Handles platform widget item double-click events.
    /// </summary>
    private void OnPlatformItemDoubleClick(object? sender, int itemIndex)
    {
        CheckWidget();

        var doubleClickEvent = new Event
        {
            Index = itemIndex,
            Time = Environment.TickCount,
            Item = itemIndex >= 0 ? _items[itemIndex] : null
        };
        NotifyListeners(SWT.DefaultSelection, doubleClickEvent);
    }

    /// <summary>
    /// Handles platform widget click events.
    /// </summary>
    private void OnPlatformClick(object? sender, int button)
    {
        CheckWidget();

        // Create SWT MouseDown event
        var mouseDownEvent = new Event
        {
            Button = button,
            Time = Environment.TickCount,
            StateMask = GetCurrentStateMask()
        };
        NotifyListeners(SWT.MouseDown, mouseDownEvent);

        // Create SWT MouseUp event
        var mouseUpEvent = new Event
        {
            Button = button,
            Time = Environment.TickCount,
            StateMask = GetCurrentStateMask()
        };
        NotifyListeners(SWT.MouseUp, mouseUpEvent);
    }

    /// <summary>
    /// Handles platform widget focus gained events.
    /// </summary>
    private void OnPlatformFocusGained(object? sender, int detail)
    {
        CheckWidget();

        var focusEvent = new Event
        {
            Detail = detail,
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.FocusIn, focusEvent);
    }

    /// <summary>
    /// Handles platform widget focus lost events.
    /// </summary>
    private void OnPlatformFocusLost(object? sender, int detail)
    {
        CheckWidget();

        var focusEvent = new Event
        {
            Detail = detail,
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.FocusOut, focusEvent);
    }

    /// <summary>
    /// Handles platform widget key down events.
    /// </summary>
    private void OnPlatformKeyDown(object? sender, PlatformKeyEventArgs e)
    {
        CheckWidget();

        var keyEvent = new Event
        {
            KeyCode = e.KeyCode,
            Character = e.Character,
            StateMask = GetStateMaskFromPlatformArgs(e),
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.KeyDown, keyEvent);

        // Handle keyboard navigation
        if (e.KeyCode == SWT.ARROW_UP || e.KeyCode == SWT.ARROW_LEFT)
        {
            NavigateSelection(-1);
        }
        else if (e.KeyCode == SWT.ARROW_DOWN || e.KeyCode == SWT.ARROW_RIGHT)
        {
            NavigateSelection(1);
        }
        else if (e.KeyCode == SWT.PAGE_UP)
        {
            NavigateSelection(-10);
        }
        else if (e.KeyCode == SWT.PAGE_DOWN)
        {
            NavigateSelection(10);
        }
        else if (e.KeyCode == SWT.HOME)
        {
            Select(0);
        }
        else if (e.KeyCode == SWT.END)
        {
            Select(_items.Count - 1);
        }
        else if (e.KeyCode == SWT.CR || e.KeyCode == SWT.LF)
        {
            // Enter key - trigger default selection
            int currentIndex = GetSelectionIndex();
            if (currentIndex >= 0)
            {
                OnPlatformItemDoubleClick(this, currentIndex);
            }
        }
    }

    /// <summary>
    /// Handles platform widget key up events.
    /// </summary>
    private void OnPlatformKeyUp(object? sender, PlatformKeyEventArgs e)
    {
        CheckWidget();

        var keyEvent = new Event
        {
            KeyCode = e.KeyCode,
            Character = e.Character,
            StateMask = GetStateMaskFromPlatformArgs(e),
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.KeyUp, keyEvent);
    }

    /// <summary>
    /// Navigates selection by the specified amount.
    /// </summary>
    private void NavigateSelection(int delta)
    {
        if (_items.Count == 0) return;

        int currentIndex = GetSelectionIndex();
        int newIndex = currentIndex + delta;

        // Clamp to valid range
        if (newIndex < 0) newIndex = 0;
        if (newIndex >= _items.Count) newIndex = _items.Count - 1;

        if (newIndex != currentIndex)
        {
            if (!_multiSelect)
            {
                Select(newIndex);
            }
            else
            {
                // In multi-select mode, move focus without changing selection
                // This would require additional platform support
                Select(newIndex);
            }
        }
    }

    /// <summary>
    /// Gets the current keyboard and mouse state mask.
    /// </summary>
    private int GetCurrentStateMask()
    {
        int stateMask = 0;
        // TODO: Implement platform-specific state detection
        // For now, return 0 as placeholder
        return stateMask;
    }

    /// <summary>
    /// Converts platform key event arguments to SWT state mask.
    /// </summary>
    private int GetStateMaskFromPlatformArgs(PlatformKeyEventArgs e)
    {
        int stateMask = 0;
        if (e.Shift) stateMask |= SWT.SHIFT;
        if (e.Control) stateMask |= SWT.CTRL;
        if (e.Alt) stateMask |= SWT.ALT;
        // TODO: Add Command key detection on macOS
        return stateMask;
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
        // Unsubscribe from platform widget events to prevent memory leaks
        if (PlatformWidget is IPlatformList listWidget)
        {
            listWidget.SelectionChanged -= OnPlatformSelectionChanged;
            listWidget.ItemDoubleClick -= OnPlatformItemDoubleClick;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.Click -= OnPlatformClick;
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        _items.Clear();
        _selectedIndices.Clear();
        base.ReleaseWidget();
    }
}
