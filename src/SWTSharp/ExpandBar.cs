using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a collapsible accordion-style container control with expandable sections.
/// Each section consists of a header and content area controlled by an ExpandItem.
/// </summary>
public class ExpandBar : Composite
{
    private readonly List<ExpandItem> _items = new();
    private int _spacing = 4;

    /// <summary>
    /// Gets the number of expand items in the bar.
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
    /// Gets all expand items in the bar.
    /// </summary>
    public ExpandItem[] Items
    {
        get
        {
            CheckWidget();
            return _items.ToArray();
        }
    }

    /// <summary>
    /// Gets or sets the vertical spacing between expand items in pixels.
    /// </summary>
    public int Spacing
    {
        get
        {
            CheckWidget();
            return _spacing;
        }
        set
        {
            CheckWidget();
            _spacing = Math.Max(0, value);
            if (PlatformWidget is IPlatformExpandBar expandBar)
            {
                expandBar.SetSpacing(_spacing);
            }
            Layout();
        }
    }

    /// <summary>
    /// Creates a new expand bar.
    /// </summary>
    /// <param name="parent">Parent control</param>
    /// <param name="style">Style flags (SWT.V_SCROLL for vertical scrolling)</param>
    public ExpandBar(Composite parent, int style) : base(parent, style)
    {
        // CreateWidget is called by base constructor
    }

    /// <summary>
    /// Creates the platform-specific expand bar control.
    /// </summary>
    protected override void CreateWidget()
    {
        // Use platform widget
        var widget = PlatformFactory.Instance.CreateExpandBarWidget(
            Parent?.PlatformWidget,
            Style
        );

        // Only assign after successful creation
        PlatformWidget = widget;

        // Set initial spacing
        if (widget is IPlatformExpandBar expandBar)
        {
            expandBar.SetSpacing(_spacing);
        }
    }

    /// <summary>
    /// Gets the expand item at the specified index.
    /// </summary>
    /// <param name="index">Index of the expand item</param>
    /// <returns>The expand item at the specified index</returns>
    public ExpandItem GetItem(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        }
        return _items[index];
    }

    /// <summary>
    /// Gets the number of expand items in the bar.
    /// </summary>
    /// <returns>The number of expand items</returns>
    public int GetItemCount()
    {
        CheckWidget();
        return _items.Count;
    }

    /// <summary>
    /// Gets all expand items in the bar.
    /// </summary>
    /// <returns>Array of all expand items</returns>
    public ExpandItem[] GetItems()
    {
        CheckWidget();
        return _items.ToArray();
    }

    /// <summary>
    /// Gets the index of the specified expand item.
    /// </summary>
    /// <param name="item">The expand item to find</param>
    /// <returns>The index of the item, or -1 if not found</returns>
    public int IndexOf(ExpandItem item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        return _items.IndexOf(item);
    }

    /// <summary>
    /// Gets the spacing between expand items.
    /// </summary>
    /// <returns>The spacing in pixels</returns>
    public int GetSpacing()
    {
        CheckWidget();
        return _spacing;
    }

    /// <summary>
    /// Sets the spacing between expand items.
    /// </summary>
    /// <param name="spacing">The spacing in pixels</param>
    public void SetSpacing(int spacing)
    {
        CheckWidget();
        Spacing = spacing;
    }

    /// <summary>
    /// Adds an expand item to this bar.
    /// This method is called internally by ExpandItem.
    /// </summary>
    internal void AddItem(ExpandItem item, int index)
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

        // Request layout when an item is added
        Layout();
    }

    /// <summary>
    /// Removes an expand item from this bar.
    /// This method is called internally by ExpandItem.
    /// </summary>
    internal void RemoveItem(ExpandItem item)
    {
        if (item == null)
        {
            return;
        }

        if (_items.Remove(item))
        {
            // Request layout when an item is removed
            Layout();
        }
    }

    /// <summary>
    /// Called when an expand item's expanded state changes.
    /// This method is invoked by platform-specific event handlers.
    /// </summary>
    internal void HandleItemExpanded(int itemIndex, bool expanded)
    {
        if (itemIndex >= 0 && itemIndex < _items.Count)
        {
            var item = _items[itemIndex];
            item.NotifyExpanded(expanded);

            // Request layout to adjust item positions
            Layout();
        }
    }

    protected override void ReleaseWidget()
    {
        // Dispose all expand items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        base.ReleaseWidget();
    }
}
