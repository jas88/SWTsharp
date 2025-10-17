using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a tabbed container control that displays one or more pages.
/// Each page consists of a tab label and content area controlled by a TabItem.
/// </summary>
public class TabFolder : Composite
{
    private readonly System.Collections.Generic.List<TabItem> _items = new();
    private int _selectionIndex = -1;

    /// <summary>
    /// Gets the number of tabs in the folder.
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
    /// Gets all tab items in the folder.
    /// </summary>
    public TabItem[] Items
    {
        get
        {
            CheckWidget();
            return _items.ToArray();
        }
    }

    /// <summary>
    /// Gets or sets the currently selected tab item.
    /// Returns null if no tab is selected.
    /// </summary>
    public TabItem? Selection
    {
        get
        {
            CheckWidget();
            return _selectionIndex >= 0 && _selectionIndex < _items.Count
                ? _items[_selectionIndex]
                : null;
        }
        set
        {
            CheckWidget();
            if (value == null)
            {
                SelectionIndex = -1;
            }
            else
            {
                int index = _items.IndexOf(value);
                if (index >= 0)
                {
                    SelectionIndex = index;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the currently selected tab.
    /// Returns -1 if no tab is selected.
    /// </summary>
    public int SelectionIndex
    {
        get
        {
            CheckWidget();
            // TODO: Implement platform widget interface call to get tab selection in Phase 5.8
            return _selectionIndex;
        }
        set
        {
            CheckWidget();
            if (value < -1 || value >= _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Selection index out of range");
            }

            if (_selectionIndex != value)
            {
                int oldIndex = _selectionIndex;
                _selectionIndex = value;

                // Use IPlatformTabFolder interface to set tab selection
                // TODO: Implement tab selection through platform widget interface in Phase 5.8
                // if (PlatformWidget is IPlatformTabFolder tabFolderWidget)
                // {
                //     tabFolderWidget.SelectionIndex = value;
                // }

                // Hide old tab's control
                if (oldIndex >= 0 && oldIndex < _items.Count)
                {
                    var oldItem = _items[oldIndex];
                    if (oldItem.Control != null)
                    {
                        oldItem.Control.Visible = false;
                    }
                }

                // Show new tab's control
                if (value >= 0 && value < _items.Count)
                {
                    var newItem = _items[value];
                    if (newItem.Control != null)
                    {
                        newItem.Control.Visible = true;
                        newItem.Control.SetBounds(0, 0, GetBounds().Width, GetBounds().Height);
                    }
                }

                OnSelectionChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Occurs when the selected tab changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a new tab folder.
    /// </summary>
    /// <param name="parent">Parent control</param>
    /// <param name="style">Style flags (SWT.TOP or SWT.BOTTOM for tab position)</param>
    public TabFolder(Composite parent, int style) : base(parent, style)
    {
        // CreateWidget is called by base constructor
    }

    /// <summary>
    /// Creates the platform-specific tab folder control.
    /// </summary>
    protected override void CreateWidget()
    {
        // Use platform widget - must complete before any event subscriptions
        var widget = PlatformFactory.Instance.CreateTabFolderWidget(
            Parent?.PlatformWidget,
            Style
        );

        // Only assign after successful creation
        PlatformWidget = widget;
    }

    /// <summary>
    /// Gets the tab item at the specified index.
    /// </summary>
    /// <param name="index">Index of the tab item</param>
    /// <returns>The tab item at the specified index</returns>
    public TabItem GetItem(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        }
        return _items[index];
    }

    /// <summary>
    /// Gets the number of tabs in the folder.
    /// </summary>
    /// <returns>The number of tab items</returns>
    public int GetItemCount()
    {
        CheckWidget();
        return _items.Count;
    }

    /// <summary>
    /// Gets all tab items in the folder.
    /// </summary>
    /// <returns>Array of all tab items</returns>
    public TabItem[] GetItems()
    {
        CheckWidget();
        return _items.ToArray();
    }

    /// <summary>
    /// Gets the index of the specified tab item.
    /// </summary>
    /// <param name="item">The tab item to find</param>
    /// <returns>The index of the item, or -1 if not found</returns>
    public int IndexOf(TabItem item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        return _items.IndexOf(item);
    }

    /// <summary>
    /// Adds a tab item to this folder.
    /// This method is called internally by TabItem.
    /// </summary>
    internal void AddItem(TabItem item, int index)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        if (index == -1)
        {
            _items.Add(item);
        }
        else
        {
            if (index < 0 || index > _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
            }
            _items.Insert(index, item);
        }

        // If this is the first item, select it
        if (_items.Count == 1)
        {
            SelectionIndex = 0;
        }
    }

    /// <summary>
    /// Removes a tab item from this folder.
    /// This method is called internally by TabItem.
    /// </summary>
    internal void RemoveItem(TabItem item)
    {
        if (item == null)
        {
            return;
        }

        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            _items.RemoveAt(index);

            // Adjust selection if needed
            if (_selectionIndex == index)
            {
                if (_items.Count > 0)
                {
                    SelectionIndex = Math.Min(index, _items.Count - 1);
                }
                else
                {
                    _selectionIndex = -1;
                }
            }
            else if (_selectionIndex > index)
            {
                _selectionIndex--;
            }
        }
    }

    /// <summary>
    /// Gets the currently selected tab item.
    /// </summary>
    /// <returns>The selected tab item, or null if no tab is selected</returns>
    public TabItem? GetSelection()
    {
        CheckWidget();
        return Selection;
    }

    /// <summary>
    /// Gets the index of the currently selected tab.
    /// </summary>
    /// <returns>The index of the selected tab, or -1 if no tab is selected</returns>
    public int GetSelectionIndex()
    {
        CheckWidget();
        return _selectionIndex;
    }

    /// <summary>
    /// Sets the currently selected tab to the specified item.
    /// </summary>
    /// <param name="item">The tab item to select</param>
    public void SetSelection(TabItem item)
    {
        CheckWidget();
        Selection = item;
    }

    /// <summary>
    /// Sets the currently selected tab by index.
    /// </summary>
    /// <param name="index">The index of the tab to select</param>
    public void SetSelection(int index)
    {
        CheckWidget();
        SelectionIndex = index;
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);

        // Also notify via SWT event system
        var evt = new Events.Event
        {
            Widget = this,
            Type = SWT.Selection
        };
        NotifyListeners(SWT.Selection, evt);
    }

    /// <summary>
    /// Called by the platform when tab selection changes.
    /// This method is invoked by platform-specific event handlers.
    /// </summary>
    internal void HandleSelectionChanged(int newIndex)
    {
        if (newIndex != _selectionIndex)
        {
            SelectionIndex = newIndex;
        }
    }

    protected override void ReleaseWidget()
    {
        // Dispose all tab items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        base.ReleaseWidget();
    }
}
