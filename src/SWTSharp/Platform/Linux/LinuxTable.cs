using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of a table widget using GtkTreeView with GtkListStore.
/// GtkTreeView displays tabular data with columns and rows.
/// </summary>
internal class LinuxTable : LinuxWidget, IPlatformTable
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GLib = "libglib-2.0.so.0";

    private IntPtr _gtkTreeView;
    private IntPtr _gtkListStore;
    private IntPtr _gtkScrolledWindow;
    private readonly List<IntPtr> _columns = new();
    private readonly List<string[]> _rowData = new(); // Cache of row data
    private int _columnCount = 0;
    private bool _headerVisible = true;
    private bool _linesVisible = true;
    private bool _disposed;

    // Events
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    #pragma warning restore CS0067

    public LinuxTable(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxTable] Creating table. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create scrolled window container
        _gtkScrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        gtk_scrolled_window_set_policy(_gtkScrolledWindow,
            GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);

        // Create tree view (initially with no columns)
        _gtkTreeView = gtk_tree_view_new();

        // Create an empty list store (will be recreated when columns are added)
        _gtkListStore = IntPtr.Zero;

        // Add tree view to scrolled window
        gtk_container_add(_gtkScrolledWindow, _gtkTreeView);

        // Add to parent
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkScrolledWindow);
        }

        // Set grid lines visibility
        gtk_tree_view_set_grid_lines(_gtkTreeView,
            _linesVisible ? GTK_TREE_VIEW_GRID_LINES_BOTH : GTK_TREE_VIEW_GRID_LINES_NONE);

        // Set header visibility
        gtk_tree_view_set_headers_visible(_gtkTreeView, _headerVisible);

        // Handle selection style
        IntPtr selection = gtk_tree_view_get_selection(_gtkTreeView);
        if ((style & SWT.MULTI) != 0)
        {
            gtk_tree_selection_set_mode(selection, GTK_SELECTION_MULTIPLE);
        }
        else
        {
            gtk_tree_selection_set_mode(selection, GTK_SELECTION_SINGLE);
        }

        // Show widgets
        gtk_widget_show(_gtkTreeView);
        gtk_widget_show(_gtkScrolledWindow);

        if (enableLogging)
            Console.WriteLine($"[LinuxTable] Table created successfully. Handle: 0x{_gtkTreeView:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _gtkScrolledWindow; // Return scrolled window for parent-child relationships
    }

    public int AddColumn(string text, int width, int alignment)
    {
        // Create column with text cell renderer
        IntPtr column = gtk_tree_view_column_new();
        IntPtr renderer = gtk_cell_renderer_text_new();

        gtk_tree_view_column_pack_start(column, renderer, true);
        gtk_tree_view_column_add_attribute(column, renderer, "text", _columnCount);

        // Set column title
        gtk_tree_view_column_set_title(column, text ?? "");

        // Set column width
        if (width > 0)
        {
            gtk_tree_view_column_set_fixed_width(column, width);
            gtk_tree_view_column_set_sizing(column, GTK_TREE_VIEW_COLUMN_FIXED);
        }

        // Set alignment
        float xalign = alignment == SWT.CENTER ? 0.5f : (alignment == SWT.RIGHT ? 1.0f : 0.0f);
        g_object_set_float(renderer, "xalign", xalign);

        // Add column to tree view
        gtk_tree_view_append_column(_gtkTreeView, column);
        _columns.Add(column);

        int columnIndex = _columnCount++;

        // Recreate list store with new column count
        RecreateListStore();

        return columnIndex;
    }

    public void RemoveColumn(int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count) return;

        IntPtr column = _columns[columnIndex];
        gtk_tree_view_remove_column(_gtkTreeView, column);
        _columns.RemoveAt(columnIndex);
        _columnCount--;

        // Recreate list store
        RecreateListStore();
    }

    public void SetColumnText(int columnIndex, string text)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count) return;
        gtk_tree_view_column_set_title(_columns[columnIndex], text ?? "");
    }

    public void SetColumnWidth(int columnIndex, int width)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count) return;
        if (width > 0)
        {
            gtk_tree_view_column_set_fixed_width(_columns[columnIndex], width);
            gtk_tree_view_column_set_sizing(_columns[columnIndex], GTK_TREE_VIEW_COLUMN_FIXED);
        }
    }

    public void SetColumnAlignment(int columnIndex, int alignment)
    {
        if (columnIndex < 0 || columnIndex >= _columns.Count) return;

        float xalign = alignment == SWT.CENTER ? 0.5f : (alignment == SWT.RIGHT ? 1.0f : 0.0f);

        // Get the renderer from the column
        IntPtr renderers = gtk_cell_layout_get_cells(_columns[columnIndex]);
        if (renderers != IntPtr.Zero)
        {
            IntPtr renderer = g_list_nth_data(renderers, 0);
            if (renderer != IntPtr.Zero)
            {
                g_object_set_float(renderer, "xalign", xalign);
            }
            g_list_free(renderers);
        }
    }

    public int AddItem()
    {
        return AddItem(_rowData.Count);
    }

    public int AddItem(int index)
    {
        if (_gtkListStore == IntPtr.Zero) return -1;

        // Create empty row data
        string[] rowData = new string[Math.Max(1, _columnCount)];
        for (int i = 0; i < rowData.Length; i++)
        {
            rowData[i] = "";
        }

        // Insert into cache
        if (index < 0 || index > _rowData.Count)
        {
            index = _rowData.Count;
        }
        _rowData.Insert(index, rowData);

        // Add to list store
        GtkTreeIter iter;
        gtk_list_store_insert(_gtkListStore, out iter, index);

        // Set empty values for all columns
        for (int col = 0; col < _columnCount; col++)
        {
            gtk_list_store_set_string(_gtkListStore, ref iter, col, "");
        }

        return index;
    }

    public void RemoveItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _rowData.Count || _gtkListStore == IntPtr.Zero) return;

        _rowData.RemoveAt(itemIndex);

        // Remove from list store
        GtkTreeIter iter;
        if (GetIterFromIndex(itemIndex, out iter))
        {
            gtk_list_store_remove(_gtkListStore, ref iter);
        }
    }

    public void RemoveAllItems()
    {
        _rowData.Clear();
        if (_gtkListStore != IntPtr.Zero)
        {
            gtk_list_store_clear(_gtkListStore);
        }
    }

    public void SetItemText(int itemIndex, int columnIndex, string text)
    {
        if (itemIndex < 0 || itemIndex >= _rowData.Count ||
            columnIndex < 0 || columnIndex >= _columnCount ||
            _gtkListStore == IntPtr.Zero) return;

        // Update cache
        var row = _rowData[itemIndex];
        if (row.Length <= columnIndex)
        {
            Array.Resize(ref row, columnIndex + 1);
            _rowData[itemIndex] = row;
        }
        row[columnIndex] = text ?? "";
        _rowData[itemIndex] = row;

        // Update list store
        GtkTreeIter iter;
        if (GetIterFromIndex(itemIndex, out iter))
        {
            gtk_list_store_set_string(_gtkListStore, ref iter, columnIndex, text ?? "");
        }
    }

    public string GetItemText(int itemIndex, int columnIndex)
    {
        if (itemIndex < 0 || itemIndex >= _rowData.Count ||
            columnIndex < 0 || columnIndex >= _columnCount) return "";

        if (_rowData[itemIndex].Length <= columnIndex) return "";
        return _rowData[itemIndex][columnIndex] ?? "";
    }

    public void SetItemImage(int itemIndex, int columnIndex, IPlatformImage? image)
    {
        // TODO: Implement image support for table cells
        // This requires using GdkPixbuf and changing the cell renderer
    }

    public void SetHeaderVisible(bool visible)
    {
        _headerVisible = visible;
        if (_gtkTreeView != IntPtr.Zero)
        {
            gtk_tree_view_set_headers_visible(_gtkTreeView, visible);
        }
    }

    public bool GetHeaderVisible()
    {
        return _headerVisible;
    }

    public void SetLinesVisible(bool visible)
    {
        _linesVisible = visible;
        if (_gtkTreeView != IntPtr.Zero)
        {
            gtk_tree_view_set_grid_lines(_gtkTreeView,
                visible ? GTK_TREE_VIEW_GRID_LINES_BOTH : GTK_TREE_VIEW_GRID_LINES_NONE);
        }
    }

    public bool GetLinesVisible()
    {
        return _linesVisible;
    }

    public void SetSelection(int[] indices)
    {
        if (_gtkTreeView == IntPtr.Zero) return;

        IntPtr selection = gtk_tree_view_get_selection(_gtkTreeView);
        gtk_tree_selection_unselect_all(selection);

        foreach (int index in indices)
        {
            GtkTreeIter iter;
            if (GetIterFromIndex(index, out iter))
            {
                gtk_tree_selection_select_iter(selection, ref iter);
            }
        }
    }

    public int[] GetSelection()
    {
        if (_gtkTreeView == IntPtr.Zero) return Array.Empty<int>();

        IntPtr selection = gtk_tree_view_get_selection(_gtkTreeView);
        IntPtr selectedRows = gtk_tree_selection_get_selected_rows(selection, out IntPtr model);

        var indices = new List<int>();

        IntPtr current = selectedRows;
        while (current != IntPtr.Zero)
        {
            IntPtr path = g_list_nth_data(current, 0);
            if (path != IntPtr.Zero)
            {
                IntPtr indices_ptr = gtk_tree_path_get_indices(path);
                if (indices_ptr != IntPtr.Zero)
                {
                    int index = Marshal.ReadInt32(indices_ptr);
                    indices.Add(index);
                }
            }
            current = g_list_next(current);
        }

        if (selectedRows != IntPtr.Zero)
        {
            g_list_free_full(selectedRows, (IntPtr ptr) => gtk_tree_path_free(ptr));
        }

        return indices.ToArray();
    }

    public int GetItemCount()
    {
        return _rowData.Count;
    }

    public int GetColumnCount()
    {
        return _columnCount;
    }

    public void AddChild(IPlatformWidget child) { /* Tables don't have child widgets */ }
    public void RemoveChild(IPlatformWidget child) { /* Tables don't have child widgets */ }
    public IReadOnlyList<IPlatformWidget> GetChildren() => Array.Empty<IPlatformWidget>();

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_gtkScrolledWindow == IntPtr.Zero) return;
        gtk_widget_set_size_request(_gtkScrolledWindow, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_gtkScrolledWindow == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_gtkScrolledWindow, out allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_gtkScrolledWindow == IntPtr.Zero) return;
        if (visible)
            gtk_widget_show(_gtkScrolledWindow);
        else
            gtk_widget_hide(_gtkScrolledWindow);
    }

    public bool GetVisible()
    {
        if (_gtkScrolledWindow == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_gtkScrolledWindow);
    }

    public void SetEnabled(bool enabled)
    {
        if (_gtkTreeView == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_gtkTreeView, enabled);
    }

    public bool GetEnabled()
    {
        if (_gtkTreeView == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_gtkTreeView);
    }

    public void SetBackground(RGB color) { /* TODO: Implement via CSS */ }
    public RGB GetBackground() => new RGB(255, 255, 255);
    public void SetForeground(RGB color) { /* TODO: Implement via CSS */ }
    public RGB GetForeground() => new RGB(0, 0, 0);

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _columns.Clear();
        _rowData.Clear();

        if (_gtkScrolledWindow != IntPtr.Zero)
        {
            gtk_widget_destroy(_gtkScrolledWindow);
            _gtkScrolledWindow = IntPtr.Zero;
        }

        _gtkTreeView = IntPtr.Zero;
        _gtkListStore = IntPtr.Zero;
    }

    private void RecreateListStore()
    {
        // Store existing data
        var oldData = new List<string[]>(_rowData);

        // Create new list store with correct number of columns
        IntPtr[] types = new IntPtr[Math.Max(1, _columnCount)];
        IntPtr gtype_string = g_type_from_name("gchararray");
        for (int i = 0; i < types.Length; i++)
        {
            types[i] = gtype_string;
        }

        IntPtr newStore = gtk_list_store_newv(types.Length, types);

        // Restore data
        _rowData.Clear();
        if (_gtkListStore != IntPtr.Zero)
        {
            g_object_unref(_gtkListStore);
        }
        _gtkListStore = newStore;
        gtk_tree_view_set_model(_gtkTreeView, _gtkListStore);

        // Re-add all rows
        foreach (var row in oldData)
        {
            int index = AddItem();
            for (int col = 0; col < Math.Min(row.Length, _columnCount); col++)
            {
                SetItemText(index, col, row[col]);
            }
        }
    }

    private bool GetIterFromIndex(int index, out GtkTreeIter iter)
    {
        iter = default;
        if (_gtkListStore == IntPtr.Zero || index < 0) return false;

        IntPtr path = gtk_tree_path_new_from_indices(index, -1);
        bool result = gtk_tree_model_get_iter(_gtkListStore, out iter, path);
        gtk_tree_path_free(path);
        return result;
    }

    #region GTK3 P/Invoke

    private const int GTK_POLICY_AUTOMATIC = 1;
    private const int GTK_SELECTION_SINGLE = 1;
    private const int GTK_SELECTION_MULTIPLE = 3;
    private const int GTK_TREE_VIEW_COLUMN_FIXED = 1;
    private const int GTK_TREE_VIEW_GRID_LINES_NONE = 0;
    private const int GTK_TREE_VIEW_GRID_LINES_BOTH = 3;

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x, y, width, height;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkTreeIter
    {
        public int stamp;
        public IntPtr user_data;
        public IntPtr user_data2;
        public IntPtr user_data3;
    }

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolled_window, int hscrollbar_policy, int vscrollbar_policy);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_model(IntPtr tree_view, IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_selection(IntPtr tree_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_set_mode(IntPtr selection, int mode);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_headers_visible(IntPtr tree_view, bool visible);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_grid_lines(IntPtr tree_view, int grid_lines);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_column_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_text_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_pack_start(IntPtr tree_column, IntPtr cell, bool expand);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void gtk_tree_view_column_add_attribute(IntPtr tree_column, IntPtr cell_renderer, string attribute, int column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void gtk_tree_view_column_set_title(IntPtr tree_column, string title);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_fixed_width(IntPtr tree_column, int fixed_width);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_sizing(IntPtr tree_column, int type);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_append_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_remove_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_store_newv(int n_columns, IntPtr[] types);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_insert(IntPtr list_store, out GtkTreeIter iter, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void gtk_list_store_set(IntPtr list_store, ref GtkTreeIter iter, int column, string value, int terminator = -1);

    private static void gtk_list_store_set_string(IntPtr list_store, ref GtkTreeIter iter, int column, string value)
    {
        gtk_list_store_set(list_store, ref iter, column, value, -1);
    }

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_list_store_remove(IntPtr list_store, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_clear(IntPtr list_store);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_get_iter(IntPtr tree_model, out GtkTreeIter iter, IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_path_new_from_indices(int first_index, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_path_free(IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_path_get_indices(IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_unselect_all(IntPtr selection);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_select_iter(IntPtr selection, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_selection_get_selected_rows(IntPtr selection, out IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_layout_get_cells(IntPtr cell_layout);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern IntPtr g_type_from_name(string name);

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void g_object_set(IntPtr @object, string property_name, float value, IntPtr terminator);

    private static void g_object_set_float(IntPtr @object, string property_name, float value)
    {
        g_object_set(@object, property_name, value, IntPtr.Zero);
    }

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(IntPtr @object);

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_list_nth_data(IntPtr list, uint n);

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_list_next(IntPtr list);

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_list_free(IntPtr list);

    [DllImport(GLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_list_free_full(IntPtr list, Action<IntPtr> free_func);

    #endregion
}
