using SWTSharp.Platform;
using SWTSharp.Platform.MacOS;

namespace SWTSharp;

/// <summary>
/// Represents an item in a Tree control.
/// Tree items can contain other tree items, forming a hierarchy.
/// </summary>
public class TreeItem : Widget
{
    private string _text = string.Empty;
    private Graphics.Image? _image;
    private bool _checked;
    private bool _expanded;
    private readonly Tree? _parentTree;
    private readonly TreeItem? _parentItem;
    private readonly System.Collections.Generic.List<TreeItem> _items = new();
    private IPlatformTreeItem? _platformTreeItem;

    /// <summary>
    /// Gets or sets the text displayed in the tree item.
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
            if (_text != value)
            {
                _text = value ?? string.Empty;
                // Use platform widget
                if (_platformTreeItem != null)
                {
                    _platformTreeItem.SetText(_text);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the image displayed in the tree item.
    /// </summary>
    public Graphics.Image? Image
    {
        get
        {
            CheckWidget();
            return _image;
        }
        set
        {
            CheckWidget();
            if (_image != value)
            {
                _image = value;
                // Use platform widget
                if (_platformTreeItem != null && _image != null)
                {
                    var imageAdapter = new MacOSImage(_image);
                    _platformTreeItem.SetImage(imageAdapter);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the item is checked (only valid if tree has SWT.CHECK style).
    /// </summary>
    public bool Checked
    {
        get
        {
            CheckWidget();
            if (!ParentTree.IsCheckStyle)
            {
                throw new SWTException(SWT.ERROR_INVALID_ARGUMENT, "Tree does not have CHECK style");
            }
            return _checked;
        }
        set
        {
            CheckWidget();
            if (!ParentTree.IsCheckStyle)
            {
                throw new SWTException(SWT.ERROR_INVALID_ARGUMENT, "Tree does not have CHECK style");
            }
            if (_checked != value)
            {
                _checked = value;
                // Use platform widget
                if (_platformTreeItem != null)
                {
                    _platformTreeItem.SetChecked(_checked);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the item is expanded.
    /// </summary>
    public bool Expanded
    {
        get
        {
            CheckWidget();
            return _expanded;
        }
        set
        {
            CheckWidget();
            if (_expanded != value)
            {
                _expanded = value;
                // Use platform widget
                if (_platformTreeItem != null)
                {
                    _platformTreeItem.SetExpanded(_expanded);
                }

                if (_expanded)
                {
                    ParentTree.OnExpand(this);
                }
                else
                {
                    ParentTree.OnCollapse(this);
                }
            }
        }
    }

    /// <summary>
    /// Gets the number of child items.
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
    /// Gets the child items of this tree item.
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
    /// Gets the parent tree that owns this item.
    /// </summary>
    public Tree ParentTree
    {
        get
        {
            CheckWidget();
            return _parentTree ?? _parentItem?.ParentTree!;
        }
    }

    /// <summary>
    /// Gets the parent tree item (null for root items).
    /// </summary>
    public TreeItem? ParentItem
    {
        get
        {
            CheckWidget();
            return _parentItem;
        }
    }

    /// <summary>
    /// Creates a new root tree item.
    /// </summary>
    /// <param name="parent">Parent tree</param>
    /// <param name="style">Style flags</param>
    public TreeItem(Tree parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a new root tree item at the specified index.
    /// </summary>
    /// <param name="parent">Parent tree</param>
    /// <param name="style">Style flags</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    public TreeItem(Tree parent, int style, int index) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }
        _parentTree = parent;
        _parentItem = null;
        CreateWidget(parent.PlatformWidget, null, index);
        parent.AddItem(this, index >= 0 ? index : parent.ItemCount);
    }

    /// <summary>
    /// Creates a new child tree item.
    /// </summary>
    /// <param name="parent">Parent tree item</param>
    /// <param name="style">Style flags</param>
    public TreeItem(TreeItem parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a new child tree item at the specified index.
    /// </summary>
    /// <param name="parent">Parent tree item</param>
    /// <param name="style">Style flags</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    public TreeItem(TreeItem parent, int style, int index) : base(parent.ParentTree, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }
        _parentTree = null;
        _parentItem = parent;
        CreateWidget(parent.ParentTree.PlatformWidget, parent.PlatformWidget, index);
        parent.AddItem(this, index >= 0 ? index : parent.ItemCount);
    }

    /// <summary>
    /// Creates the platform-specific tree item.
    /// </summary>
    private void CreateWidget(IPlatformWidget? treeWidget, IPlatformWidget? parentItemWidget, int index)
    {
        // Create platform tree item if platform factory supports it
        if (Platform.PlatformFactory.Instance is MacOSPlatform macOSPlatform)
        {
            // TODO: Implement proper platform tree item creation without pseudo-handles
            // TODO: Create IPlatformTreeItem widget here through platform widget interface

            // Create platform adapter (temporary workaround)
            _platformTreeItem = new MacOSTreeItem(macOSPlatform, IntPtr.Zero);
        }
        else
        {
            // Fallback for other platforms - TODO: Implement direct platform widget creation
            throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Direct platform widget creation not implemented for this platform");
        }

        // Set initial properties using platform widget
        if (_platformTreeItem != null)
        {
            _platformTreeItem.SetText(_text);
            if (_image != null)
            {
                var imageAdapter = new MacOSImage(_image);
                _platformTreeItem.SetImage(imageAdapter);
            }
            _platformTreeItem.SetExpanded(_expanded);
            _platformTreeItem.SetChecked(_checked);
        }
    }

    /// <summary>
    /// Gets all child items.
    /// </summary>
    /// <returns>Array of child tree items</returns>
    public TreeItem[] GetItems()
    {
        CheckWidget();
        return _items.ToArray();
    }

    /// <summary>
    /// Gets the child item at the specified index.
    /// </summary>
    /// <param name="index">Child item index</param>
    /// <returns>The child tree item</returns>
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
    /// Gets the number of child items.
    /// </summary>
    /// <returns>The number of child items</returns>
    public int GetItemCount()
    {
        CheckWidget();
        return _items.Count;
    }

    /// <summary>
    /// Sets the text of the tree item.
    /// </summary>
    /// <param name="text">Text to set</param>
    public void SetText(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Sets the image of the tree item.
    /// </summary>
    /// <param name="image">Image to set</param>
    public void SetImage(Graphics.Image? image)
    {
        Image = image;
    }

    /// <summary>
    /// Sets the checked state of the tree item (only valid if tree has SWT.CHECK style).
    /// </summary>
    /// <param name="checked">True to check, false to uncheck</param>
    public void SetChecked(bool @checked)
    {
        Checked = @checked;
    }

    /// <summary>
    /// Sets the expanded state of the tree item.
    /// </summary>
    /// <param name="expanded">True to expand, false to collapse</param>
    public void SetExpanded(bool expanded)
    {
        Expanded = expanded;
    }

    /// <summary>
    /// Removes all child items from this tree item.
    /// </summary>
    public void RemoveAll()
    {
        CheckWidget();
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();
        // Clear children handled by platform widget
    }

    /// <summary>
    /// Gets the index of this item within its parent.
    /// </summary>
    /// <returns>The index, or -1 if not found</returns>
    public int GetIndex()
    {
        CheckWidget();
        if (_parentItem != null)
        {
            return _parentItem._items.IndexOf(this);
        }
        else if (_parentTree != null)
        {
            return _parentTree.Items.ToList().IndexOf(this);
        }
        return -1;
    }

    /// <summary>
    /// Adds a child item to this tree item.
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
    /// Removes a child item from this tree item.
    /// Called internally when a child TreeItem is disposed.
    /// </summary>
    internal void RemoveItem(TreeItem item)
    {
        if (item != null)
        {
            _items.Remove(item);
        }
    }

    /// <summary>
    /// Gets the internal list of child items (for tree selection operations).
    /// </summary>
    internal System.Collections.Generic.List<TreeItem> GetItemsList()
    {
        return _items;
    }

    protected override void ReleaseWidget()
    {
        // Remove from parent
        if (_parentItem != null)
        {
            _parentItem.RemoveItem(this);
        }
        else if (_parentTree != null)
        {
            _parentTree.RemoveItem(this);
        }

        // Dispose all child items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        // Dispose platform widget
        if (_platformTreeItem != null)
        {
            _platformTreeItem.Dispose();
            _platformTreeItem = null;
        }

        _image = null;
        base.ReleaseWidget();
    }

    /// <summary>
    /// Returns a string representation of this tree item.
    /// </summary>
    public override string ToString()
    {
        if (IsDisposed)
        {
            return "TreeItem {disposed}";
        }
        return $"TreeItem {{text={_text}, children={_items.Count}}}";
    }
}
