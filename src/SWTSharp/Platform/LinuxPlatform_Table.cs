using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Table widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK TreeView/ListStore P/Invoke declarations for Table widget
    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern nuint g_type_from_name(string name);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_new_with_model(IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_store_newv(int n_columns, IntPtr[] types);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_model(IntPtr tree_view, IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_model(IntPtr tree_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_column_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_append_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_remove_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_column(IntPtr tree_view, int n);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_view_column_set_title(IntPtr tree_column, string title);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_column_get_title(IntPtr tree_column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_fixed_width(IntPtr tree_column, int fixed_width);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_column_get_width(IntPtr tree_column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_resizable(IntPtr tree_column, bool resizable);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_reorderable(IntPtr tree_column, bool reorderable);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_alignment(IntPtr tree_column, float xalign);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_text_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_toggle_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_view_column_pack_start(IntPtr tree_column, IntPtr cell, bool expand);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_view_column_add_attribute(IntPtr tree_column, IntPtr cell_renderer, string attribute, int column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_append(IntPtr list_store, out GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_insert(IntPtr list_store, out GtkTreeIter iter, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_remove(IntPtr list_store, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_clear(IntPtr list_store);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_list_store_set(IntPtr list_store, ref GtkTreeIter iter, int column, string val, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_set_value(IntPtr list_store, ref GtkTreeIter iter, int column, ref GValue value);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_get_iter(IntPtr tree_model, out GtkTreeIter iter, IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_iter_nth_child(IntPtr tree_model, out GtkTreeIter iter, IntPtr parent, int n);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_model_iter_n_children(IntPtr tree_model, IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_model_get(IntPtr tree_model, ref GtkTreeIter iter, int column, out IntPtr data, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_selection(IntPtr tree_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_set_mode(IntPtr selection, GtkSelectionMode type);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_select_iter(IntPtr selection, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_unselect_all(IntPtr selection);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_selection_get_selected_rows(IntPtr selection, out IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_headers_visible(IntPtr tree_view, bool headers_visible);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_grid_lines(IntPtr tree_view, GtkTreeViewGridLines grid_lines);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_path_new_from_indices(int index, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_scroll_to_cell(IntPtr tree_view, IntPtr path, IntPtr column, bool use_align, float row_align, float col_align);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_path_free(IntPtr path);

    // GTK TreeIter structure
    [StructLayout(LayoutKind.Sequential)]
    private struct GtkTreeIter
    {
        public int stamp;
        public IntPtr user_data;
        public IntPtr user_data2;
        public IntPtr user_data3;
    }

    // GValue structure for setting values
    [StructLayout(LayoutKind.Sequential)]
    private struct GValue
    {
        public IntPtr g_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public long[] data;
    }

    // GTK TreeView grid lines enum
    private enum GtkTreeViewGridLines
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = 3
    }

    // Table data storage
    private sealed class TableData
    {
        public IntPtr ScrolledWindow { get; set; }
        public IntPtr TreeView { get; set; }
        public IntPtr ListStore { get; set; }
        public List<IntPtr> Columns { get; } = new List<IntPtr>();
        public List<GtkTreeIter> Rows { get; } = new List<GtkTreeIter>();
        public int Style { get; set; }
        public bool CheckStyle { get; set; }
    }

    // Column data storage
    private sealed class TableColumnData
    {
        public IntPtr TableHandle { get; set; }
        public IntPtr ColumnHandle { get; set; }
        public int ColumnIndex { get; set; }
        public int Alignment { get; set; }
    }

    // Track table widgets and their data
    private readonly Dictionary<IntPtr, TableData> _tableData = new Dictionary<IntPtr, TableData>();
    private readonly Dictionary<IntPtr, TableColumnData> _columnData = new Dictionary<IntPtr, TableColumnData>();
    private readonly Dictionary<IntPtr, (IntPtr tableHandle, int rowIndex)> _tableItemData = new Dictionary<IntPtr, (IntPtr, int)>();

    // Table operations
    public IntPtr CreateTable(IntPtr parent, int style)
    {
        // Create scrolled window to contain the tree view
        IntPtr scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        gtk_scrolled_window_set_policy(scrolledWindow, 1, 1); // GTK_POLICY_AUTOMATIC = 1

        // Create list store initially with no columns (will be added dynamically)
        // Start with just one column to avoid issues
        IntPtr[] types = new IntPtr[] { new IntPtr((long)g_type_from_name("gchararray")) };
        IntPtr listStore = gtk_list_store_newv(1, types);

        // Create tree view with the model
        IntPtr treeView = gtk_tree_view_new_with_model(listStore);

        // Add tree view to scrolled window
        gtk_container_add(scrolledWindow, treeView);

        // Configure selection mode
        IntPtr selection = gtk_tree_view_get_selection(treeView);
        if ((style & SWT.MULTI) != 0)
        {
            gtk_tree_selection_set_mode(selection, GtkSelectionMode.Multiple);
        }
        else
        {
            gtk_tree_selection_set_mode(selection, GtkSelectionMode.Single);
        }

        // Set headers visible by default
        gtk_tree_view_set_headers_visible(treeView, true);

        // Show both widgets
        gtk_widget_show(treeView);
        gtk_widget_show(scrolledWindow);

        // Store table data
        var tableData = new TableData
        {
            ScrolledWindow = scrolledWindow,
            TreeView = treeView,
            ListStore = listStore,
            Style = style,
            CheckStyle = (style & SWT.CHECK) != 0
        };
        _tableData[scrolledWindow] = tableData;

        return scrolledWindow;
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        gtk_tree_view_set_headers_visible(tableData.TreeView, visible);
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        gtk_tree_view_set_grid_lines(tableData.TreeView, visible ? GtkTreeViewGridLines.Both : GtkTreeViewGridLines.None);
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        IntPtr selection = gtk_tree_view_get_selection(tableData.TreeView);
        gtk_tree_selection_unselect_all(selection);

        if (indices != null)
        {
            foreach (int index in indices)
            {
                if (gtk_tree_model_iter_nth_child(tableData.ListStore, out GtkTreeIter iter, IntPtr.Zero, index))
                {
                    gtk_tree_selection_select_iter(selection, ref iter);
                }
            }
        }
    }

    public void ClearTableItems(IntPtr handle)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        gtk_list_store_clear(tableData.ListStore);
        tableData.Rows.Clear();
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        IntPtr path = gtk_tree_path_new_from_indices(index, -1);
        gtk_tree_view_scroll_to_cell(tableData.TreeView, path, IntPtr.Zero, false, 0.0f, 0.0f);
        gtk_tree_path_free(path);
    }

    // TableColumn operations
    public IntPtr CreateTableColumn(IntPtr parent, int style, int index)
    {
        if (!_tableData.TryGetValue(parent, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(parent));

        // Create tree view column
        IntPtr column = gtk_tree_view_column_new();

        // Create cell renderer
        IntPtr cellRenderer = gtk_cell_renderer_text_new();

        // Pack renderer into column
        gtk_tree_view_column_pack_start(column, cellRenderer, true);

        // Determine column index in list store
        int columnIndex = tableData.Columns.Count;

        // Add attribute binding (column index in model)
        gtk_tree_view_column_add_attribute(column, cellRenderer, "text", columnIndex);

        // Set alignment
        float alignment = 0.0f; // Left by default
        if ((style & SWT.CENTER) != 0)
            alignment = 0.5f;
        else if ((style & SWT.RIGHT) != 0)
            alignment = 1.0f;

        gtk_tree_view_column_set_alignment(column, alignment);

        // Make column resizable by default
        gtk_tree_view_column_set_resizable(column, true);

        // Add column to tree view
        gtk_tree_view_append_column(tableData.TreeView, column);

        // Store column
        tableData.Columns.Add(column);

        // Store column data
        var columnData = new TableColumnData
        {
            TableHandle = parent,
            ColumnHandle = column,
            ColumnIndex = columnIndex,
            Alignment = style
        };
        _columnData[column] = columnData;

        return column;
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        if (!_columnData.TryGetValue(handle, out var columnData))
            throw new ArgumentException("Invalid column handle", nameof(handle));

        if (_tableData.TryGetValue(columnData.TableHandle, out var tableData))
        {
            gtk_tree_view_remove_column(tableData.TreeView, handle);
            tableData.Columns.Remove(handle);
        }

        _columnData.Remove(handle);
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        gtk_tree_view_column_set_title(handle, text);
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        gtk_tree_view_column_set_fixed_width(handle, width);
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        float align = 0.0f; // Left
        if ((alignment & SWT.CENTER) != 0)
            align = 0.5f;
        else if ((alignment & SWT.RIGHT) != 0)
            align = 1.0f;

        gtk_tree_view_column_set_alignment(handle, align);

        if (_columnData.TryGetValue(handle, out var columnData))
        {
            columnData.Alignment = alignment;
        }
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        gtk_tree_view_column_set_resizable(handle, resizable);
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        gtk_tree_view_column_set_reorderable(handle, moveable);
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTip)
    {
        // GTK TreeView columns don't support tooltips directly
        // This would require custom implementation with signals
    }

    public int PackTableColumn(IntPtr handle)
    {
        // Auto-size column to content
        // GTK does this automatically, just return current width
        return gtk_tree_view_column_get_width(handle);
    }

    // TableItem operations
    public IntPtr CreateTableItem(IntPtr parent, int style, int index)
    {
        if (!_tableData.TryGetValue(parent, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(parent));

        GtkTreeIter iter;

        if (index < 0 || index >= tableData.Rows.Count)
        {
            // Append to end
            gtk_list_store_append(tableData.ListStore, out iter);
            tableData.Rows.Add(iter);
            index = tableData.Rows.Count - 1;
        }
        else
        {
            // Insert at specific position
            gtk_list_store_insert(tableData.ListStore, out iter, index);
            tableData.Rows.Insert(index, iter);
        }

        // Create a pseudo-handle for the item (using row index)
        IntPtr itemHandle = new IntPtr(tableData.Rows.Count * 1000 + index);
        _tableItemData[itemHandle] = (parent, index);

        return itemHandle;
    }

    public void DestroyTableItem(IntPtr handle)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            throw new ArgumentException("Invalid table item handle", nameof(handle));

        if (_tableData.TryGetValue(itemData.tableHandle, out var tableData))
        {
            if (itemData.rowIndex >= 0 && itemData.rowIndex < tableData.Rows.Count)
            {
                GtkTreeIter iter = tableData.Rows[itemData.rowIndex];
                gtk_list_store_remove(tableData.ListStore, ref iter);
                tableData.Rows.RemoveAt(itemData.rowIndex);
            }
        }

        _tableItemData.Remove(handle);
    }

    public void SetTableItemText(IntPtr handle, int column, string text)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            throw new ArgumentException("Invalid table item handle", nameof(handle));

        if (!_tableData.TryGetValue(itemData.tableHandle, out var tableData))
            return;

        if (itemData.rowIndex >= 0 && itemData.rowIndex < tableData.Rows.Count)
        {
            GtkTreeIter iter = tableData.Rows[itemData.rowIndex];
            gtk_list_store_set(tableData.ListStore, ref iter, column, text, -1);
        }
    }

    public void SetTableItemImage(IntPtr handle, int column, IntPtr image)
    {
        // GTK implementation would require pixbuf column type
        // Not implemented in this basic version
    }

    public void SetTableItemChecked(IntPtr handle, bool checked_)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            throw new ArgumentException("Invalid table item handle", nameof(handle));

        if (!_tableData.TryGetValue(itemData.tableHandle, out var tableData))
            return;

        if (!tableData.CheckStyle)
            return;

        // Would require toggle renderer in first column
        // Not fully implemented in this basic version
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        // Would require cell data function for custom rendering
        // Not implemented in this basic version
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        // Would require cell data function for custom rendering
        // Not implemented in this basic version
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        // Would require cell data function for custom rendering
        // Not implemented in this basic version
    }
}
