using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Tree widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK TreeView/TreeStore P/Invoke declarations for Tree widget
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_store_newv(int n_columns, IntPtr[] types);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_store_append(IntPtr tree_store, out GtkTreeIter iter, IntPtr parent);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_store_insert(IntPtr tree_store, out GtkTreeIter iter, IntPtr parent, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_store_remove(IntPtr tree_store, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_store_set(IntPtr tree_store, ref GtkTreeIter iter, int column, string val, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_iter_parent(IntPtr tree_model, out GtkTreeIter iter, ref GtkTreeIter child);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_iter_children(IntPtr tree_model, out GtkTreeIter iter, IntPtr parent);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_expand_row(IntPtr tree_view, IntPtr path, bool open_all);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_view_collapse_row(IntPtr tree_view, IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_view_row_expanded(IntPtr tree_view, IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_model_get_path(IntPtr tree_model, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_selection_get_selected(IntPtr selection, out IntPtr model, out GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_store_clear(IntPtr tree_store);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_iter_next(IntPtr tree_model, ref GtkTreeIter iter);

    // Tree data storage
    private sealed class TreeData
    {
        public IntPtr TreeStore { get; set; }
        public IntPtr TreeView { get; set; }
        public bool MultiSelect { get; set; }
        public bool CheckStyle { get; set; }
        public int ColumnCount { get; set; } = 1; // Default: text column
    }

    private sealed class TreeItemData
    {
        public IntPtr TreeHandle { get; set; }
        public GtkTreeIter Iter { get; set; }
    }

    private readonly Dictionary<IntPtr, TreeData> _treeData = new Dictionary<IntPtr, TreeData>();
    private readonly Dictionary<IntPtr, TreeItemData> _treeItemData = new Dictionary<IntPtr, TreeItemData>();

    // Tree operations
    public IntPtr CreateTree(IntPtr parent, int style)
    {
        // Create tree view
        IntPtr treeView = gtk_tree_view_new();
        if (treeView == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK tree view");
        }

        // Create tree store (hierarchical model)
        IntPtr[] columnTypes = new IntPtr[] { new IntPtr((long)g_type_from_name("gchararray")) }; // String column
        IntPtr treeStore = gtk_tree_store_newv(1, columnTypes);

        if (treeStore == IntPtr.Zero)
        {
            gtk_widget_destroy(treeView);
            throw new InvalidOperationException("Failed to create GTK tree store");
        }

        // Set model
        gtk_tree_view_set_model(treeView, treeStore);

        // Create text column
        IntPtr column = gtk_tree_view_column_new();
        IntPtr renderer = gtk_cell_renderer_text_new();
        gtk_tree_view_column_pack_start(column, renderer, true);
        gtk_tree_view_column_add_attribute(column, renderer, "text", 0);
        gtk_tree_view_append_column(treeView, column);

        // Set selection mode
        IntPtr selection = gtk_tree_view_get_selection(treeView);
        if ((style & SWT.MULTI) != 0)
        {
            gtk_tree_selection_set_mode(selection, GtkSelectionMode.Multiple);
        }
        else
        {
            gtk_tree_selection_set_mode(selection, GtkSelectionMode.Single);
        }

        // Enable headers if requested
        if ((style & SWT.HIDE_SELECTION) == 0)
        {
            gtk_tree_view_set_headers_visible(treeView, false);
        }

        // Add to parent using helper (handles GtkWindow -> container routing)
        if (parent != IntPtr.Zero)
        {
            AddChildToParent(parent, treeView);
        }

        // Store tree data
        _treeData[treeView] = new TreeData
        {
            TreeStore = treeStore,
            TreeView = treeView,
            MultiSelect = (style & SWT.MULTI) != 0,
            CheckStyle = (style & SWT.CHECK) != 0
        };

        gtk_widget_show(treeView);

        return treeView;
    }

    public IntPtr[] GetTreeSelection(IntPtr handle)
    {
        if (!_treeData.TryGetValue(handle, out var treeData))
            return Array.Empty<IntPtr>();

        IntPtr selection = gtk_tree_view_get_selection(handle);
        List<IntPtr> selectedItems = new List<IntPtr>();

        if (treeData.MultiSelect)
        {
            // For multi-select, we need to use gtk_tree_selection_get_selected_rows
            // This is simplified - would need full implementation
            return selectedItems.ToArray();
        }
        else
        {
            // Single selection
            if (gtk_tree_selection_get_selected(selection, out IntPtr model, out GtkTreeIter iter))
            {
                // Find the handle for this iter
                foreach (var kvp in _treeItemData)
                {
                    if (kvp.Value.TreeHandle == handle &&
                        kvp.Value.Iter.stamp == iter.stamp &&
                        kvp.Value.Iter.user_data == iter.user_data)
                    {
                        selectedItems.Add(kvp.Key);
                        break;
                    }
                }
            }
        }

        return selectedItems.ToArray();
    }

    public void SetTreeSelection(IntPtr handle, IntPtr[] items)
    {
        if (!_treeData.TryGetValue(handle, out var treeData))
            return;

        IntPtr selection = gtk_tree_view_get_selection(handle);
        gtk_tree_selection_unselect_all(selection);

        foreach (IntPtr itemHandle in items)
        {
            if (_treeItemData.TryGetValue(itemHandle, out var itemData))
            {
                GtkTreeIter iter = itemData.Iter;
                gtk_tree_selection_select_iter(selection, ref iter);
            }
        }
    }

    public void ClearTreeItems(IntPtr handle)
    {
        if (!_treeData.TryGetValue(handle, out var treeData))
            return;

        // Remove all tree item data
        List<IntPtr> itemsToRemove = new List<IntPtr>();
        foreach (var kvp in _treeItemData)
        {
            if (kvp.Value.TreeHandle == handle)
            {
                itemsToRemove.Add(kvp.Key);
            }
        }

        foreach (var item in itemsToRemove)
        {
            _treeItemData.Remove(item);
        }

        // Clear the tree store
        gtk_tree_store_clear(treeData.TreeStore);
    }

    public void ShowTreeItem(IntPtr handle, IntPtr item)
    {
        if (!_treeData.TryGetValue(handle, out var treeData))
            return;

        if (!_treeItemData.TryGetValue(item, out var itemData))
            return;

        GtkTreeIter iter = itemData.Iter;
        IntPtr path = gtk_tree_model_get_path(treeData.TreeStore, ref iter);

        if (path != IntPtr.Zero)
        {
            gtk_tree_view_scroll_to_cell(handle, path, IntPtr.Zero, true, 0.5f, 0.0f);
            gtk_tree_path_free(path);
        }
    }

    // TreeItem operations
    public IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index)
    {
        if (!_treeData.TryGetValue(treeHandle, out var treeData))
            throw new ArgumentException("Invalid tree handle", nameof(treeHandle));

        GtkTreeIter iter;
        IntPtr parentIter = IntPtr.Zero;

        // Get parent iter if specified
        if (parentItemHandle != IntPtr.Zero && _treeItemData.TryGetValue(parentItemHandle, out var parentData))
        {
            // Allocate parent iter
            parentIter = Marshal.AllocHGlobal(Marshal.SizeOf<GtkTreeIter>());
            Marshal.StructureToPtr(parentData.Iter, parentIter, false);
        }

        if (index < 0)
        {
            // Append to end
            gtk_tree_store_append(treeData.TreeStore, out iter, parentIter);
        }
        else
        {
            // Insert at specific position
            gtk_tree_store_insert(treeData.TreeStore, out iter, parentIter, index);
        }

        // Free parent iter memory
        if (parentIter != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(parentIter);
        }

        // Create handle for tree item
        IntPtr itemHandle = Marshal.AllocHGlobal(Marshal.SizeOf<GtkTreeIter>());
        Marshal.StructureToPtr(iter, itemHandle, false);

        // Store item data
        _treeItemData[itemHandle] = new TreeItemData
        {
            TreeHandle = treeHandle,
            Iter = iter
        };

        return itemHandle;
    }

    public void DestroyTreeItem(IntPtr handle)
    {
        if (!_treeItemData.TryGetValue(handle, out var itemData))
            return;

        if (_treeData.TryGetValue(itemData.TreeHandle, out var treeData))
        {
            GtkTreeIter iter = itemData.Iter;
            gtk_tree_store_remove(treeData.TreeStore, ref iter);
        }

        _treeItemData.Remove(handle);
        Marshal.FreeHGlobal(handle);
    }

    public void SetTreeItemText(IntPtr handle, string text)
    {
        if (!_treeItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_treeData.TryGetValue(itemData.TreeHandle, out var treeData))
            return;

        GtkTreeIter iter = itemData.Iter;
        gtk_tree_store_set(treeData.TreeStore, ref iter, 0, text, -1);
    }

    public void SetTreeItemImage(IntPtr handle, IntPtr image)
    {
        // GTK implementation would require pixbuf column type
        // Not implemented in this basic version
    }

    public void SetTreeItemChecked(IntPtr handle, bool checked_)
    {
        // Would require toggle renderer column
        // Not implemented in this basic version
    }

    public bool GetTreeItemChecked(IntPtr handle)
    {
        // Would require toggle renderer column
        // Not implemented in this basic version
        return false;
    }

    public void SetTreeItemExpanded(IntPtr handle, bool expanded)
    {
        if (!_treeItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_treeData.TryGetValue(itemData.TreeHandle, out var treeData))
            return;

        GtkTreeIter iter = itemData.Iter;
        IntPtr path = gtk_tree_model_get_path(treeData.TreeStore, ref iter);

        if (path != IntPtr.Zero)
        {
            if (expanded)
            {
                gtk_tree_view_expand_row(treeData.TreeView, path, false);
            }
            else
            {
                gtk_tree_view_collapse_row(treeData.TreeView, path);
            }
            gtk_tree_path_free(path);
        }
    }

    public bool GetTreeItemExpanded(IntPtr handle)
    {
        if (!_treeItemData.TryGetValue(handle, out var itemData))
            return false;

        if (!_treeData.TryGetValue(itemData.TreeHandle, out var treeData))
            return false;

        GtkTreeIter iter = itemData.Iter;
        IntPtr path = gtk_tree_model_get_path(treeData.TreeStore, ref iter);

        if (path != IntPtr.Zero)
        {
            bool expanded = gtk_tree_view_row_expanded(treeData.TreeView, path);
            gtk_tree_path_free(path);
            return expanded;
        }

        return false;
    }

    public void ClearTreeItemChildren(IntPtr handle)
    {
        if (!_treeItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_treeData.TryGetValue(itemData.TreeHandle, out var treeData))
            return;

        // Get all children and remove them
        List<IntPtr> childrenToRemove = new List<IntPtr>();
        GtkTreeIter childIter;
        GtkTreeIter parentIter = itemData.Iter;

        // Allocate memory for parent iter parameter
        IntPtr parentIterPtr = Marshal.AllocHGlobal(Marshal.SizeOf<GtkTreeIter>());
        Marshal.StructureToPtr(parentIter, parentIterPtr, false);

        if (gtk_tree_model_iter_children(treeData.TreeStore, out childIter, parentIterPtr))
        {
            do
            {
                // Find handle for this child
                foreach (var kvp in _treeItemData)
                {
                    if (kvp.Value.TreeHandle == itemData.TreeHandle &&
                        kvp.Value.Iter.stamp == childIter.stamp &&
                        kvp.Value.Iter.user_data == childIter.user_data)
                    {
                        childrenToRemove.Add(kvp.Key);
                        break;
                    }
                }
            }
            while (gtk_tree_model_iter_next(treeData.TreeStore, ref childIter));
        }

        // Free the allocated memory
        Marshal.FreeHGlobal(parentIterPtr);

        // Remove all children
        foreach (var child in childrenToRemove)
        {
            DestroyTreeItem(child);
        }
    }

    public void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index)
    {
        // This method seems redundant with CreateTreeItem
        // In GTK, items are created and positioned in one operation
        // Leaving as no-op for compatibility
    }
}
