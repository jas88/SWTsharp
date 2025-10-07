using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Table widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    private readonly Dictionary<IntPtr, TableData> _tableData = new();
    private IntPtr _selSetGridStyleMask;
    private IntPtr _selRemoveTableColumn;
    private IntPtr _selTableColumns;
    private IntPtr _selObjectAtIndex;

    // NSTableView grid style constants
    private const int NSTableViewGridNone = 0;
    private const int NSTableViewSolidVerticalGridLineMask = 1 << 0;
    private const int NSTableViewSolidHorizontalGridLineMask = 1 << 1;

    private sealed class TableData
    {
        public IntPtr TableView { get; set; }
        public List<IntPtr> Columns { get; set; } = new();
        public List<TableRow> Rows { get; set; } = new();
        public bool HeaderVisible { get; set; } = true;
        public bool LinesVisible { get; set; } = false;
    }

    private sealed class TableRow
    {
        public Dictionary<int, string> CellText { get; set; } = new();
        public Dictionary<int, IntPtr> CellImages { get; set; } = new();
        public bool Checked { get; set; } = false;
    }

    private void InitializeTableSelectors()
    {
        InitializeListSelectors(); // Reuse NSTableView selectors

        if (_selSetGridStyleMask == IntPtr.Zero)
        {
            _selSetGridStyleMask = sel_registerName("setGridStyleMask:");
            _selRemoveTableColumn = sel_registerName("removeTableColumn:");
            _selTableColumns = sel_registerName("tableColumns");
            _selObjectAtIndex = sel_registerName("objectAtIndex:");
        }
    }

    public IntPtr CreateTable(IntPtr parent, int style)
    {
        InitializeTableSelectors();

        // Create NSScrollView to contain the table
        IntPtr scrollView = objc_msgSend(_nsScrollViewClass, _selAlloc);
        scrollView = objc_msgSend(scrollView, _selInit);

        // Create NSTableView
        IntPtr tableView = objc_msgSend(_nsTableViewClass, _selAlloc);
        tableView = objc_msgSend(tableView, _selInit);

        // Set selection mode
        bool multiSelect = (style & SWT.MULTI) != 0;
        objc_msgSend_void(tableView, _selSetAllowsMultipleSelection, multiSelect);

        // Configure grid lines (default off)
        objc_msgSend(tableView, _selSetGridStyleMask, new IntPtr(NSTableViewGridNone));

        // Header visible by default
        // Note: Header visibility is controlled via setHeaderView: (set to nil to hide)

        // Configure scroll view
        objc_msgSend_void(scrollView, _selSetHasVerticalScroller, true);
        objc_msgSend_void(scrollView, _selSetAutohidesScrollers, true);
        objc_msgSend(scrollView, _selSetDocumentView, tableView);

        // Set default frame
        var frame = new CGRect(0, 0, 400, 300);
        objc_msgSend_rect(scrollView, _selSetFrame, frame);

        // Add to parent if provided
        AddChildToParent(parent, scrollView);

        // Initialize table data
        _tableData[scrollView] = new TableData
        {
            TableView = tableView,
            HeaderVisible = true,
            LinesVisible = false
        };

        return scrollView;
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();
        data.HeaderVisible = visible;

        if (visible)
        {
            // Create default header view if needed
            IntPtr selTableHeaderView = sel_registerName("tableHeaderView");
            IntPtr currentHeader = objc_msgSend(data.TableView, selTableHeaderView);

            if (currentHeader == IntPtr.Zero)
            {
                // Create new header view
                IntPtr nsTableHeaderViewClass = objc_getClass("NSTableHeaderView");
                IntPtr headerView = objc_msgSend(nsTableHeaderViewClass, _selAlloc);
                headerView = objc_msgSend(headerView, _selInit);
                objc_msgSend(data.TableView, _selSetHeaderView, headerView);
            }
        }
        else
        {
            // Hide header by setting to nil
            objc_msgSend(data.TableView, _selSetHeaderView, IntPtr.Zero);
        }
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();
        data.LinesVisible = visible;

        int gridStyle = visible
            ? (NSTableViewSolidVerticalGridLineMask | NSTableViewSolidHorizontalGridLineMask)
            : NSTableViewGridNone;

        objc_msgSend(data.TableView, _selSetGridStyleMask, new IntPtr(gridStyle));
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();

        if (indices == null || indices.Length == 0)
        {
            // Clear selection
            IntPtr emptyIndexSet = objc_msgSend(_nsIndexSetClass, _selAlloc);
            emptyIndexSet = objc_msgSend(emptyIndexSet, _selInit);
            objc_msgSend(data.TableView, _selSelectRowIndexes, emptyIndexSet, new IntPtr(0));
            return;
        }

        // Create mutable index set
        IntPtr nsMutableIndexSetClass = objc_getClass("NSMutableIndexSet");
        IntPtr indexSet = objc_msgSend(nsMutableIndexSetClass, _selAlloc);
        indexSet = objc_msgSend(indexSet, _selInit);

        IntPtr selAddIndex = sel_registerName("addIndex:");
        foreach (int index in indices)
        {
            if (index >= 0 && index < data.Rows.Count)
            {
                objc_msgSend_ulong(indexSet, selAddIndex, (nuint)index);
            }
        }

        objc_msgSend(data.TableView, _selSelectRowIndexes, indexSet, new IntPtr(0));
    }

    public void ClearTableItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();
        data.Rows.Clear();
        objc_msgSend_void(data.TableView, _selReloadData);
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();

        if (index >= 0 && index < data.Rows.Count)
        {
            objc_msgSend_ulong(data.TableView, _selScrollRowToVisible, (nuint)index);
        }
    }

    // TableColumn operations
    private readonly Dictionary<IntPtr, ColumnData> _columnData = new();
    private IntPtr _selSetResizingMask;
    private IntPtr _selHeaderCell;
    private IntPtr _selSetToolTip;
    private IntPtr _selSizeToFit;

    // NSTableColumn resizing masks
    private const int NSTableColumnNoResizing = 0;
    private const int NSTableColumnAutoresizingMask = 1 << 0;
    private const int NSTableColumnUserResizingMask = 1 << 1;

    private sealed class ColumnData
    {
        public IntPtr TableScrollView { get; set; }
        public int ColumnIndex { get; set; }
        public int Alignment { get; set; } = SWT.LEFT;
    }

    private void InitializeColumnSelectors()
    {
        InitializeTableSelectors();
        InitializeTextSelectors(); // Reuse _selSetStringValue from Text control

        if (_selSetResizingMask == IntPtr.Zero)
        {
            _selSetResizingMask = sel_registerName("setResizingMask:");
            _selHeaderCell = sel_registerName("headerCell");
            _selSetToolTip = sel_registerName("setToolTip:");
            _selSizeToFit = sel_registerName("sizeToFit");
        }
    }

    public IntPtr CreateTableColumn(IntPtr tableHandle, int style, int index)
    {
        if (tableHandle == IntPtr.Zero || !_tableData.TryGetValue(tableHandle, out var data))
            return IntPtr.Zero;

        InitializeColumnSelectors();

        // Create NSTableColumn
        IntPtr column = objc_msgSend(_nsTableColumnClass, _selAlloc);
        IntPtr columnIdentifier = CreateNSString($"Column{data.Columns.Count}");
        column = objc_msgSend(column, _selInitWithIdentifier, columnIdentifier);

        // Set default width
        objc_msgSend_double_arg(column, _selSetWidth, 100.0);
        objc_msgSend_double_arg(column, _selSetMinWidth, 20.0);
        objc_msgSend_double_arg(column, _selSetMaxWidth, 10000.0);

        // Set resizable by default
        objc_msgSend(column, _selSetResizingMask,
            new IntPtr(NSTableColumnAutoresizingMask | NSTableColumnUserResizingMask));

        // Add column to table at specified index
        if (index < 0 || index >= data.Columns.Count)
        {
            objc_msgSend(data.TableView, _selAddTableColumn, column);
            data.Columns.Add(column);
        }
        else
        {
            // NSTableView doesn't have insertTableColumn, so we add and then move
            objc_msgSend(data.TableView, _selAddTableColumn, column);
            data.Columns.Insert(index, column);

            // Move column to correct position
            IntPtr selMoveColumn = sel_registerName("moveColumn:toColumn:");
            objc_msgSend(data.TableView, selMoveColumn,
                new IntPtr(data.Columns.Count - 1), new IntPtr(index));
        }

        // Store column data
        _columnData[column] = new ColumnData
        {
            TableScrollView = tableHandle,
            ColumnIndex = index >= 0 ? index : data.Columns.Count - 1,
            Alignment = style & (SWT.LEFT | SWT.RIGHT | SWT.CENTER)
        };

        return column;
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_columnData.TryGetValue(handle, out var colData))
            return;

        if (!_tableData.TryGetValue(colData.TableScrollView, out var data))
            return;

        InitializeColumnSelectors();

        // Remove from table view
        objc_msgSend(data.TableView, _selRemoveTableColumn, handle);

        // Remove from our tracking
        data.Columns.Remove(handle);
        _columnData.Remove(handle);

        // Update column indices for remaining columns
        for (int i = 0; i < data.Columns.Count; i++)
        {
            if (_columnData.TryGetValue(data.Columns[i], out var remainingCol))
            {
                remainingCol.ColumnIndex = i;
            }
        }
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();

        // Get header cell and set its string value
        IntPtr headerCell = objc_msgSend(handle, _selHeaderCell);
        if (headerCell != IntPtr.Zero)
        {
            IntPtr nsText = CreateNSString(text ?? string.Empty);
            objc_msgSend(headerCell, _selSetStringValue, nsText);
        }
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();
        objc_msgSend_double_arg(handle, _selSetWidth, Math.Max(0, width));
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        if (handle == IntPtr.Zero || !_columnData.TryGetValue(handle, out var colData))
            return;

        colData.Alignment = alignment;
        // Note: Column alignment in NSTableView is typically set on the cell,
        // not the column itself. This would require custom cell setup.
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();

        int mask = resizable
            ? (NSTableColumnAutoresizingMask | NSTableColumnUserResizingMask)
            : NSTableColumnNoResizing;

        objc_msgSend(handle, _selSetResizingMask, new IntPtr(mask));
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        if (handle == IntPtr.Zero || !_columnData.TryGetValue(handle, out var colData))
            return;

        if (!_tableData.TryGetValue(colData.TableScrollView, out var data))
            return;

        InitializeColumnSelectors();

        // Set column reordering on the table view
        IntPtr selSetAllowsColumnReordering = sel_registerName("setAllowsColumnReordering:");
        objc_msgSend_void(data.TableView, selSetAllowsColumnReordering, moveable);
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTipText)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();

        IntPtr headerCell = objc_msgSend(handle, _selHeaderCell);
        if (headerCell != IntPtr.Zero)
        {
            if (string.IsNullOrEmpty(toolTipText))
            {
                objc_msgSend(headerCell, _selSetToolTip, IntPtr.Zero);
            }
            else
            {
                IntPtr nsToolTip = CreateNSString(toolTipText!);
                objc_msgSend(headerCell, _selSetToolTip, nsToolTip);
            }
        }
    }

    public int PackTableColumn(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return 0;

        InitializeColumnSelectors();

        // Auto-size the column to fit content
        objc_msgSend_void(handle, _selSizeToFit);

        // Get the new width
        IntPtr selWidth = sel_registerName("width");
        double width = objc_msgSend_ret_double(handle, selWidth);

        return (int)width;
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_fpret")]
    private static extern double objc_msgSend_ret_double(IntPtr receiver, IntPtr selector);

    // TableItem operations
    // In NSTableView, items are not objects but row indices
    // We track row data in our TableData structure
    private readonly Dictionary<IntPtr, ItemData> _itemData = new();
    private int _nextItemId = 1;

    private sealed class ItemData
    {
        public IntPtr TableScrollView { get; set; }
        public int RowIndex { get; set; }
        public int ItemId { get; set; }
    }

    public IntPtr CreateTableItem(IntPtr tableHandle, int style, int index)
    {
        if (tableHandle == IntPtr.Zero || !_tableData.TryGetValue(tableHandle, out var data))
            return IntPtr.Zero;

        InitializeTableSelectors();

        // Create a new row
        var row = new TableRow();

        // Add row at specified index
        if (index < 0 || index >= data.Rows.Count)
        {
            data.Rows.Add(row);
            index = data.Rows.Count - 1;
        }
        else
        {
            data.Rows.Insert(index, row);
        }

        // Create a pseudo-handle for the item (using a unique ID)
        int itemId = _nextItemId++;
        IntPtr itemHandle = new IntPtr(0x10000000 + itemId); // Use high bit pattern to distinguish from real pointers

        // Store item data
        _itemData[itemHandle] = new ItemData
        {
            TableScrollView = tableHandle,
            RowIndex = index,
            ItemId = itemId
        };

        // Reload table to show new row
        objc_msgSend_void(data.TableView, _selReloadData);

        return itemHandle;
    }

    public void DestroyTableItem(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        InitializeTableSelectors();

        // Remove row from data
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            data.Rows.RemoveAt(itemData.RowIndex);

            // Update row indices for all items after this one
            foreach (var kvp in _itemData.ToList())
            {
                if (kvp.Value.TableScrollView == itemData.TableScrollView &&
                    kvp.Value.RowIndex > itemData.RowIndex)
                {
                    kvp.Value.RowIndex--;
                }
            }

            // Reload table
            objc_msgSend_void(data.TableView, _selReloadData);
        }

        // Remove from tracking
        _itemData.Remove(handle);
    }

    public void SetTableItemText(IntPtr handle, int columnIndex, string text)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        // Update cell text in our data structure
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            var row = data.Rows[itemData.RowIndex];
            row.CellText[columnIndex] = text ?? string.Empty;

            // Reload the affected row
            InitializeTableSelectors();
            IntPtr selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
            IntPtr indexSet = objc_msgSend(_nsIndexSetClass, _selIndexSetWithIndex, new IntPtr(itemData.RowIndex));
            IntPtr allColumnsIndexSet = CreateIndexSetForAllColumns(data.Columns.Count);
            objc_msgSend(data.TableView, selReloadDataForRowIndexes, indexSet, allColumnsIndexSet);
        }
    }

    public void SetTableItemImage(IntPtr handle, int columnIndex, IntPtr imageHandle)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        // Update cell image in our data structure
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            var row = data.Rows[itemData.RowIndex];
            if (imageHandle == IntPtr.Zero)
            {
                row.CellImages.Remove(columnIndex);
            }
            else
            {
                row.CellImages[columnIndex] = imageHandle;
            }

            // Reload the affected row
            InitializeTableSelectors();
            IntPtr selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
            IntPtr indexSet = objc_msgSend(_nsIndexSetClass, _selIndexSetWithIndex, new IntPtr(itemData.RowIndex));
            IntPtr allColumnsIndexSet = CreateIndexSetForAllColumns(data.Columns.Count);
            objc_msgSend(data.TableView, selReloadDataForRowIndexes, indexSet, allColumnsIndexSet);
        }
    }

    public void SetTableItemChecked(IntPtr handle, bool isChecked)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        // Update checked state in our data structure
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            var row = data.Rows[itemData.RowIndex];
            row.Checked = isChecked;

            // Reload the affected row
            InitializeTableSelectors();
            IntPtr selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
            IntPtr indexSet = objc_msgSend(_nsIndexSetClass, _selIndexSetWithIndex, new IntPtr(itemData.RowIndex));
            IntPtr allColumnsIndexSet = CreateIndexSetForAllColumns(data.Columns.Count);
            objc_msgSend(data.TableView, selReloadDataForRowIndexes, indexSet, allColumnsIndexSet);
        }
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        // Note: Row background colors in NSTableView are typically handled via
        // NSTableViewDelegate methods (willDisplayCell:forTableColumn:row:)
        // For a full implementation, we would need to set up a custom delegate
        // For now, this is a placeholder that stores the intent
        if (handle == IntPtr.Zero)
            return;

        // In a complete implementation, we would:
        // 1. Store the color in the row data
        // 2. Set up a table view delegate
        // 3. Implement willDisplayCell to apply the background color
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        // Note: Similar to background, foreground colors are typically handled
        // via NSTableViewDelegate methods
        if (handle == IntPtr.Zero)
            return;

        // In a complete implementation, we would:
        // 1. Store the color in the row data
        // 2. Set up a table view delegate
        // 3. Implement willDisplayCell to apply the foreground color
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        // Note: Fonts are also typically handled via NSTableViewDelegate
        if (handle == IntPtr.Zero)
            return;

        // In a complete implementation, we would:
        // 1. Store the font in the row data
        // 2. Set up a table view delegate
        // 3. Implement willDisplayCell to apply the font
    }

    // Helper method CreateIndexSetForAllColumns and _nsProgressIndicatorClass are in main MacOSPlatform.cs
    // ProgressBar operations moved to MacOSPlatform_ProgressBar.cs
}
