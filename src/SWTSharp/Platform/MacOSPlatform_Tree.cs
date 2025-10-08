using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Tree widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    private readonly Dictionary<IntPtr, TreeData> _treeData = new();
    private readonly Dictionary<IntPtr, TreeNode> _treeNodes = new();
    private IntPtr _nsOutlineViewClass;
    private IntPtr _selExpandItem;
    private IntPtr _selCollapseItem;
    private IntPtr _selIsItemExpanded;
    private IntPtr _selReloadItem;
    private IntPtr _selRowForItem;
    private IntPtr _selItemAtRow;

    private sealed class TreeData
    {
        public IntPtr ScrollView { get; set; }
        public IntPtr OutlineView { get; set; }
        public List<TreeNode> RootNodes { get; set; } = new();
        public bool MultiSelect { get; set; }
        public bool HasCheck { get; set; }
    }

    private sealed class TreeNode
    {
        public IntPtr Handle { get; set; }
        public string Text { get; set; } = string.Empty;
        public IntPtr ImageHandle { get; set; }
        public bool Checked { get; set; }
        public bool Expanded { get; set; }
        public TreeNode? Parent { get; set; }
        public List<TreeNode> Children { get; set; } = new();
        public IntPtr TreeScrollView { get; set; }
    }

    private void InitializeTreeSelectors()
    {
        // Reuse list selectors for common operations
        InitializeListSelectors();

        if (_nsOutlineViewClass == IntPtr.Zero)
        {
            _nsOutlineViewClass = objc_getClass("NSOutlineView");
            _selExpandItem = sel_registerName("expandItem:");
            _selCollapseItem = sel_registerName("collapseItem:");
            _selIsItemExpanded = sel_registerName("isItemExpanded:");
            _selReloadItem = sel_registerName("reloadItem:reloadChildren:");
            _selRowForItem = sel_registerName("rowForItem:");
            _selItemAtRow = sel_registerName("itemAtRow:");
        }
    }

    public IntPtr CreateTree(IntPtr parent, int style)
    {
        var objc = ObjCRuntime.Instance;
        InitializeTreeSelectors();

        // Create NSScrollView to contain the outline view
        IntPtr scrollView = objc_msgSend(objc.NSScrollView, objc.SelAlloc);
        scrollView = objc_msgSend(scrollView, objc.SelInit);

        // Create NSOutlineView
        IntPtr outlineView = objc_msgSend(_nsOutlineViewClass, _selAlloc);
        outlineView = objc_msgSend(outlineView, _selInit);

        // Set selection mode
        bool multiSelect = (style & SWT.MULTI) != 0;
        objc_msgSend_void(outlineView, _selSetAllowsMultipleSelection, multiSelect);

        // Configure scroll view
        objc_msgSend_void(scrollView, _selSetHasVerticalScroller, true);
        objc_msgSend_void(scrollView, _selSetAutohidesScrollers, true);
        objc_msgSend(scrollView, _selSetDocumentView, outlineView);

        // Set default frame
        var frame = new CGRect(0, 0, 300, 400);
        objc_msgSend_rect(scrollView, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, scrollView);
        }

        // Initialize tree data
        _treeData[scrollView] = new TreeData
        {
            ScrollView = scrollView,
            OutlineView = outlineView,
            MultiSelect = multiSelect,
            HasCheck = (style & SWT.CHECK) != 0
        };

        return scrollView;
    }

    public IntPtr[] GetTreeSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_treeData.TryGetValue(handle, out var data))
            return Array.Empty<IntPtr>();

        InitializeTreeSelectors();

        // Get selected row indexes
        IntPtr indexSet = objc_msgSend(data.OutlineView, _selSelectedRowIndexes);

        // Convert NSIndexSet to array
        var selectedHandles = new List<IntPtr>();
        IntPtr selFirstIndex = sel_registerName("firstIndex");
        IntPtr selIndexGreaterThanIndex = sel_registerName("indexGreaterThanIndex:");

        nint index = (nint)objc_msgSend(indexSet, selFirstIndex);
        const nint NSNotFound = -1; // NSNotFound constant in macOS

        while (index != NSNotFound)
        {
            // Get item at this row
            IntPtr item = objc_msgSend(data.OutlineView, _selItemAtRow, new IntPtr(index));

            // Find the TreeNode for this item
            foreach (var node in _treeNodes.Values)
            {
                if (node.TreeScrollView == handle)
                {
                    // In a real implementation, we'd need proper item-to-node mapping
                    // For now, we'll return the node handle if we can identify it
                    selectedHandles.Add(node.Handle);
                    break;
                }
            }

            index = (nint)objc_msgSend(indexSet, selIndexGreaterThanIndex, new IntPtr(index));
        }

        return selectedHandles.ToArray();
    }

    public void SetTreeSelection(IntPtr handle, IntPtr[] items)
    {
        if (handle == IntPtr.Zero || !_treeData.TryGetValue(handle, out var data))
            return;

        InitializeTreeSelectors();
        IntPtr selSelectRowIndexes = sel_registerName("selectRowIndexes:byExtendingSelection:");

        if (items == null || items.Length == 0)
        {
            // Clear selection
            IntPtr emptyIndexSet = objc_msgSend(_nsIndexSetClass, _selAlloc);
            emptyIndexSet = objc_msgSend(emptyIndexSet, _selInit);
            objc_msgSend(data.OutlineView, selSelectRowIndexes, emptyIndexSet, new IntPtr(0));
            return;
        }

        // Create index set for selected rows
        IntPtr nsMutableIndexSetClass = objc_getClass("NSMutableIndexSet");
        IntPtr indexSet = objc_msgSend(nsMutableIndexSetClass, _selAlloc);
        indexSet = objc_msgSend(indexSet, _selInit);
        IntPtr selAddIndex = sel_registerName("addIndex:");

        foreach (var itemHandle in items)
        {
            if (_treeNodes.TryGetValue(itemHandle, out var node))
            {
                // Get row index for this item
                // Note: This requires the item to be visible (parents expanded)
                nint row = (nint)objc_msgSend(data.OutlineView, _selRowForItem, itemHandle);
                if (row >= 0)
                {
                    objc_msgSend_ulong(indexSet, selAddIndex, (nuint)row);
                }
            }
        }

        objc_msgSend(data.OutlineView, selSelectRowIndexes, indexSet, new IntPtr(0));
    }

    public void ClearTreeItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_treeData.TryGetValue(handle, out var data))
            return;

        // Remove all root nodes
        var nodesToRemove = data.RootNodes.ToList();
        foreach (var node in nodesToRemove)
        {
            DestroyTreeNodeRecursive(node);
        }
        data.RootNodes.Clear();

        // Reload the outline view
        InitializeTreeSelectors();
        IntPtr selReloadData = sel_registerName("reloadData");
        objc_msgSend(data.OutlineView, selReloadData);
    }

    public void ShowTreeItem(IntPtr handle, IntPtr item)
    {
        if (handle == IntPtr.Zero || !_treeData.TryGetValue(handle, out var data))
            return;

        if (!_treeNodes.TryGetValue(item, out var node))
            return;

        InitializeTreeSelectors();

        // Expand all parent nodes first
        var parent = node.Parent;
        while (parent != null)
        {
            if (!parent.Expanded)
            {
                objc_msgSend(data.OutlineView, _selExpandItem, parent.Handle);
                parent.Expanded = true;
            }
            parent = parent.Parent;
        }

        // Get row index and scroll to it
        nint row = (nint)objc_msgSend(data.OutlineView, _selRowForItem, item);
        if (row >= 0)
        {
            objc_msgSend(data.OutlineView, _selScrollRowToVisible, new IntPtr(row));
        }
    }

    // TreeItem operations

    public IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index)
    {
        if (treeHandle == IntPtr.Zero || !_treeData.TryGetValue(treeHandle, out var data))
            return IntPtr.Zero;

        InitializeTreeSelectors();

        // Create new tree node with a pseudo-handle
        int itemId = _nextItemId++;
        IntPtr itemHandle = new IntPtr(0x20000000 + itemId); // Use different bit pattern from table items

        var newNode = new TreeNode
        {
            Handle = itemHandle,
            TreeScrollView = treeHandle,
            Expanded = false
        };

        if (parentItemHandle == IntPtr.Zero)
        {
            // Root item
            if (index < 0 || index >= data.RootNodes.Count)
            {
                data.RootNodes.Add(newNode);
            }
            else
            {
                data.RootNodes.Insert(index, newNode);
            }
        }
        else
        {
            // Child item
            if (_treeNodes.TryGetValue(parentItemHandle, out var parentNode))
            {
                newNode.Parent = parentNode;
                if (index < 0 || index >= parentNode.Children.Count)
                {
                    parentNode.Children.Add(newNode);
                }
                else
                {
                    parentNode.Children.Insert(index, newNode);
                }
            }
            else
            {
                return IntPtr.Zero;
            }
        }

        // Store node mapping
        _treeNodes[newNode.Handle] = newNode;

        // Reload the tree
        IntPtr selReloadData = sel_registerName("reloadData");
        objc_msgSend(data.OutlineView, selReloadData);

        return newNode.Handle;
    }

    public void DestroyTreeItem(IntPtr handle)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return;

        InitializeTreeSelectors();

        // Remove from parent or root list
        if (node.Parent != null)
        {
            node.Parent.Children.Remove(node);
        }
        else
        {
            data.RootNodes.Remove(node);
        }

        // Recursively destroy this node and all children
        DestroyTreeNodeRecursive(node);

        // Reload the tree
        IntPtr selReloadData = sel_registerName("reloadData");
        objc_msgSend(data.OutlineView, selReloadData);
    }

    private void DestroyTreeNodeRecursive(TreeNode node)
    {
        // Recursively destroy children
        foreach (var child in node.Children.ToList())
        {
            DestroyTreeNodeRecursive(child);
        }

        // Remove from mapping
        _treeNodes.Remove(node.Handle);
    }

    public void SetTreeItemText(IntPtr handle, string text)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return;

        node.Text = text ?? string.Empty;

        // Reload this item
        InitializeTreeSelectors();
        objc_msgSend(data.OutlineView, _selReloadItem, handle, new IntPtr(0));
    }

    public void SetTreeItemImage(IntPtr handle, IntPtr image)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return;

        node.ImageHandle = image;

        // Reload this item
        InitializeTreeSelectors();
        objc_msgSend(data.OutlineView, _selReloadItem, handle, new IntPtr(0));
    }

    public void SetTreeItemChecked(IntPtr handle, bool checked_)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return;

        if (!data.HasCheck)
            return;

        node.Checked = checked_;

        // Reload this item
        InitializeTreeSelectors();
        objc_msgSend(data.OutlineView, _selReloadItem, handle, new IntPtr(0));
    }

    public bool GetTreeItemChecked(IntPtr handle)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return false;

        return node.Checked;
    }

    public void SetTreeItemExpanded(IntPtr handle, bool expanded)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return;

        InitializeTreeSelectors();

        if (expanded)
        {
            objc_msgSend(data.OutlineView, _selExpandItem, handle);
        }
        else
        {
            objc_msgSend(data.OutlineView, _selCollapseItem, handle);
        }

        node.Expanded = expanded;
    }

    public bool GetTreeItemExpanded(IntPtr handle)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return false;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return false;

        InitializeTreeSelectors();

        bool expanded = objc_msgSend_bool(data.OutlineView, _selIsItemExpanded, handle);
        node.Expanded = expanded;

        return expanded;
    }

    public void ClearTreeItemChildren(IntPtr handle)
    {
        if (!_treeNodes.TryGetValue(handle, out var node))
            return;

        if (!_treeData.TryGetValue(node.TreeScrollView, out var data))
            return;

        // Destroy all children recursively
        foreach (var child in node.Children.ToList())
        {
            DestroyTreeNodeRecursive(child);
        }
        node.Children.Clear();

        // Reload this item and its children
        InitializeTreeSelectors();
        objc_msgSend(data.OutlineView, _selReloadItem, handle, new IntPtr(1));
    }

    public void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index)
    {
        if (treeHandle == IntPtr.Zero || !_treeData.TryGetValue(treeHandle, out var data))
            return;

        if (!_treeNodes.TryGetValue(itemHandle, out var node))
            return;

        InitializeTreeSelectors();

        // Remove from current parent if any
        if (node.Parent != null)
        {
            node.Parent.Children.Remove(node);
        }
        else
        {
            data.RootNodes.Remove(node);
        }

        // Add to new parent
        if (parentItemHandle == IntPtr.Zero)
        {
            // Add as root item
            node.Parent = null;
            if (index < 0 || index >= data.RootNodes.Count)
            {
                data.RootNodes.Add(node);
            }
            else
            {
                data.RootNodes.Insert(index, node);
            }
        }
        else
        {
            // Add as child item
            if (_treeNodes.TryGetValue(parentItemHandle, out var parentNode))
            {
                node.Parent = parentNode;
                if (index < 0 || index >= parentNode.Children.Count)
                {
                    parentNode.Children.Add(node);
                }
                else
                {
                    parentNode.Children.Insert(index, node);
                }
            }
        }

        // Reload the tree
        IntPtr selReloadData = sel_registerName("reloadData");
        objc_msgSend(data.OutlineView, selReloadData);
    }

    // Table operations - using NSTableView for multi-column grid display
}
