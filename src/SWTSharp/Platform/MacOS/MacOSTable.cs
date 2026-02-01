using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformTable using NSTableView with NSTableColumn.
/// Provides a complete table widget implementation with column and row management.
/// </summary>
internal class MacOSTable : MacOSWidget, IPlatformTable
{
    private IntPtr _scrollView;
    private IntPtr _tableView;
    #pragma warning disable CS0169
    private IntPtr _dataSource;
    private IntPtr _delegate;
    #pragma warning restore CS0169
    private readonly List<IntPtr> _columns = new();
    private readonly List<RowData> _rows = new();
    private readonly HashSet<int> _moveableColumns = new(); // Track per-column moveable state
    private bool _headerVisible = true;
    private bool _linesVisible = false;
    private bool _disposed;
    private readonly bool _multiSelect;

    // Row data structure
    private sealed class RowData
    {
        public Dictionary<int, string> ColumnText { get; set; } = new();
        public Dictionary<int, IntPtr> ColumnImages { get; set; } = new();
    }

    // Objective-C selectors
    private static IntPtr _selScrollView;
    private static IntPtr _selTableView;
    private static IntPtr _selAddTableColumn;
    private static IntPtr _selRemoveTableColumn;
    private static IntPtr _selTableColumns;
    private static IntPtr _selReloadData;
    private static IntPtr _selSetHeaderView;
    private static IntPtr _selHeaderView;
    private static IntPtr _selSetGridStyleMask;
    private static IntPtr _selSetAllowsMultipleSelection;
    private static IntPtr _selSelectRowIndexes;
    private static IntPtr _selSelectedRowIndexes;
    private static IntPtr _selDeselectAll;
    private static IntPtr _selNumberOfRows;
    private static IntPtr _selBeginUpdates;
    private static IntPtr _selEndUpdates;
    private static IntPtr _selInsertRowsAtIndexes;
    private static IntPtr _selRemoveRowsAtIndexes;
    private static IntPtr _selReloadDataForRowIndexes;
    private static IntPtr _selMoveColumn;

    // Objective-C classes
    private static IntPtr _nsScrollViewClass;
    private static IntPtr _nsTableViewClass;
    private static IntPtr _nsTableColumnClass;
    private static IntPtr _nsTableHeaderViewClass;
    private static IntPtr _nsIndexSetClass;

    // NSTableView grid style constants
    private const int NSTableViewGridNone = 0;
    private const int NSTableViewSolidVerticalGridLineMask = 1 << 0;
    private const int NSTableViewSolidHorizontalGridLineMask = 1 << 1;

    // Events
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<int>? KeyUp;
    #pragma warning restore CS0067

    static MacOSTable()
    {
        InitializeSelectors();
    }

    private static void InitializeSelectors()
    {
        _selScrollView = sel_registerName("scrollView");
        _selTableView = sel_registerName("tableView");
        _selAddTableColumn = sel_registerName("addTableColumn:");
        _selRemoveTableColumn = sel_registerName("removeTableColumn:");
        _selTableColumns = sel_registerName("tableColumns");
        _selReloadData = sel_registerName("reloadData");
        _selSetHeaderView = sel_registerName("setHeaderView:");
        _selHeaderView = sel_registerName("headerView");
        _selSetGridStyleMask = sel_registerName("setGridStyleMask:");
        _selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
        _selSelectRowIndexes = sel_registerName("selectRowIndexes:byExtendingSelection:");
        _selSelectedRowIndexes = sel_registerName("selectedRowIndexes");
        _selDeselectAll = sel_registerName("deselectAll:");
        _selNumberOfRows = sel_registerName("numberOfRows");
        _selBeginUpdates = sel_registerName("beginUpdates");
        _selEndUpdates = sel_registerName("endUpdates");
        _selInsertRowsAtIndexes = sel_registerName("insertRowsAtIndexes:withAnimation:");
        _selRemoveRowsAtIndexes = sel_registerName("removeRowsAtIndexes:withAnimation:");
        _selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
        _selMoveColumn = sel_registerName("moveColumn:toColumn:");

        _nsScrollViewClass = objc_getClass("NSScrollView");
        _nsTableViewClass = objc_getClass("NSTableView");
        _nsTableColumnClass = objc_getClass("NSTableColumn");
        _nsTableHeaderViewClass = objc_getClass("NSTableHeaderView");
        _nsIndexSetClass = objc_getClass("NSIndexSet");
    }

    public MacOSTable(IntPtr parentHandle, int style)
    {
        _multiSelect = (style & SWT.MULTI) != 0;

        // Create NSScrollView
        _scrollView = objc_msgSend(_nsScrollViewClass, sel_registerName("alloc"));
        _scrollView = objc_msgSend(_scrollView, sel_registerName("init"));

        // Create NSTableView
        _tableView = objc_msgSend(_nsTableViewClass, sel_registerName("alloc"));
        _tableView = objc_msgSend(_tableView, sel_registerName("init"));

        // Set table properties
        objc_msgSend(_tableView, _selSetAllowsMultipleSelection, _multiSelect);

        // Set scroll view's document view
        IntPtr selSetDocumentView = sel_registerName("setDocumentView:");
        objc_msgSend(_scrollView, selSetDocumentView, _tableView);

        // Enable borders on scroll view
        IntPtr selSetHasVerticalScroller = sel_registerName("setHasVerticalScroller:");
        IntPtr selSetHasHorizontalScroller = sel_registerName("setHasHorizontalScroller:");
        IntPtr selSetBorderType = sel_registerName("setBorderType:");
        objc_msgSend(_scrollView, selSetHasVerticalScroller, true);
        objc_msgSend(_scrollView, selSetHasHorizontalScroller, true);
        objc_msgSend(_scrollView, selSetBorderType, new IntPtr(1)); // NSBezelBorder

        // Setup data source and delegate (simplified - would need proper delegate implementation)
        // For a complete implementation, we'd create custom NSTableViewDataSource and NSTableViewDelegate
        // For now, we'll use reloadData to update the table

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr selAddSubview = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, selAddSubview, _scrollView);
        }

        // Set grid lines to none by default
        objc_msgSend(_tableView, _selSetGridStyleMask, new IntPtr(NSTableViewGridNone));
    }

    public override IntPtr GetNativeHandle()
    {
        return _scrollView;
    }

    // Column Management
    public int AddColumn(string text, int width, int alignment, int index = -1)
    {
        if (_disposed) return -1;

        // Determine insertion index
        int insertIndex = (index < 0 || index > _columns.Count) ? _columns.Count : index;

        // Create NSTableColumn
        IntPtr column = objc_msgSend(_nsTableColumnClass, sel_registerName("alloc"));
        IntPtr identifier = CreateNSString($"Column{_columns.Count}");
        IntPtr selInitWithIdentifier = sel_registerName("initWithIdentifier:");
        column = objc_msgSend(column, selInitWithIdentifier, identifier);

        // Set column properties
        IntPtr selSetWidth = sel_registerName("setWidth:");
        IntPtr selSetMinWidth = sel_registerName("setMinWidth:");
        IntPtr selSetMaxWidth = sel_registerName("setMaxWidth:");
        IntPtr selHeaderCell = sel_registerName("headerCell");
        IntPtr selSetStringValue = sel_registerName("setStringValue:");

        objc_msgSend_double(column, selSetWidth, width > 0 ? (double)width : 100.0);
        objc_msgSend_double(column, selSetMinWidth, 20.0);
        objc_msgSend_double(column, selSetMaxWidth, 10000.0);

        // Set header text
        IntPtr headerCell = objc_msgSend(column, selHeaderCell);
        IntPtr nsText = CreateNSString(text ?? "");
        objc_msgSend(headerCell, selSetStringValue, nsText);

        // Set alignment (on the column's data cell)
        IntPtr selDataCell = sel_registerName("dataCell");
        IntPtr selSetAlignment = sel_registerName("setAlignment:");
        IntPtr dataCell = objc_msgSend(column, selDataCell);

        int nsAlignment = alignment switch
        {
            SWT.CENTER => 2, // NSTextAlignmentCenter
            SWT.RIGHT => 1,  // NSTextAlignmentRight
            _ => 0           // NSTextAlignmentLeft
        };
        objc_msgSend(dataCell, selSetAlignment, new IntPtr(nsAlignment));

        // Add column to table (always appends)
        objc_msgSend(_tableView, _selAddTableColumn, column);

        // Track column in our list
        if (insertIndex >= _columns.Count)
        {
            _columns.Add(column);
        }
        else
        {
            _columns.Insert(insertIndex, column);

            // Move column from end to correct position in NSTableView
            // moveColumn:toColumn: moves the column at fromIndex to toIndex
            int fromIndex = _columns.Count - 1; // Column was appended, so it's at the end
            objc_msgSend(_tableView, _selMoveColumn, new IntPtr(fromIndex), new IntPtr(insertIndex));

            // Shift _moveableColumns indices >= insertIndex up by 1
            var shifted = new HashSet<int>();
            foreach (int idx in _moveableColumns)
            {
                shifted.Add(idx >= insertIndex ? idx + 1 : idx);
            }
            _moveableColumns.Clear();
            foreach (int idx in shifted)
            {
                _moveableColumns.Add(idx);
            }
        }

        // Reload data to show new column
        objc_msgSend(_tableView, _selReloadData);

        return insertIndex;
    }

    public void RemoveColumn(int columnIndex)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        IntPtr column = _columns[columnIndex];
        objc_msgSend(_tableView, _selRemoveTableColumn, column);
        _columns.RemoveAt(columnIndex);

        // Update _moveableColumns: remove this index and shift indices > columnIndex down by 1
        _moveableColumns.Remove(columnIndex);
        var shifted = new HashSet<int>();
        foreach (int idx in _moveableColumns)
        {
            shifted.Add(idx > columnIndex ? idx - 1 : idx);
        }
        _moveableColumns.Clear();
        foreach (int idx in shifted)
        {
            _moveableColumns.Add(idx);
        }

        // Re-evaluate allowsColumnReordering - disable if no columns are moveable
        bool anyMoveable = _moveableColumns.Count > 0;
        IntPtr selSetAllowsColumnReordering = sel_registerName("setAllowsColumnReordering:");
        objc_msgSend_void(_tableView, selSetAllowsColumnReordering, anyMoveable);

        // Update row data
        foreach (var row in _rows)
        {
            var newText = new Dictionary<int, string>();
            var newImages = new Dictionary<int, IntPtr>();

            foreach (var kvp in row.ColumnText)
            {
                if (kvp.Key < columnIndex)
                    newText[kvp.Key] = kvp.Value;
                else if (kvp.Key > columnIndex)
                    newText[kvp.Key - 1] = kvp.Value;
            }

            foreach (var kvp in row.ColumnImages)
            {
                if (kvp.Key < columnIndex)
                    newImages[kvp.Key] = kvp.Value;
                else if (kvp.Key > columnIndex)
                    newImages[kvp.Key - 1] = kvp.Value;
            }

            row.ColumnText = newText;
            row.ColumnImages = newImages;
        }

        objc_msgSend(_tableView, _selReloadData);
    }

    public void SetColumnText(int columnIndex, string text)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        IntPtr column = _columns[columnIndex];
        IntPtr selHeaderCell = sel_registerName("headerCell");
        IntPtr selSetStringValue = sel_registerName("setStringValue:");
        IntPtr headerCell = objc_msgSend(column, selHeaderCell);
        IntPtr nsText = CreateNSString(text ?? "");
        objc_msgSend(headerCell, selSetStringValue, nsText);
    }

    public void SetColumnWidth(int columnIndex, int width)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        IntPtr column = _columns[columnIndex];
        IntPtr selSetWidth = sel_registerName("setWidth:");
        objc_msgSend_double(column, selSetWidth, Math.Max(0, width));
    }

    public void SetColumnAlignment(int columnIndex, int alignment)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        IntPtr column = _columns[columnIndex];
        IntPtr selDataCell = sel_registerName("dataCell");
        IntPtr selSetAlignment = sel_registerName("setAlignment:");
        IntPtr dataCell = objc_msgSend(column, selDataCell);

        int nsAlignment = alignment switch
        {
            SWT.CENTER => 2,
            SWT.RIGHT => 1,
            _ => 0
        };
        objc_msgSend(dataCell, selSetAlignment, new IntPtr(nsAlignment));
        objc_msgSend(_tableView, _selReloadData);
    }

    public void SetColumnResizable(int columnIndex, bool resizable)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        IntPtr column = _columns[columnIndex];
        IntPtr selSetResizingMask = sel_registerName("setResizingMask:");

        // NSTableColumnNoResizing = 0, NSTableColumnAutoresizingMask = 1, NSTableColumnUserResizingMask = 2
        int mask = resizable ? (1 | 2) : 0; // Autoresize + User resize, or none
        objc_msgSend(column, selSetResizingMask, new IntPtr(mask));
    }

    public void SetColumnMoveable(int columnIndex, bool moveable)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        // Track per-column moveable state
        if (moveable)
            _moveableColumns.Add(columnIndex);
        else
            _moveableColumns.Remove(columnIndex);

        // NSTableView only has table-wide allowsColumnReordering.
        // Enable reordering if ANY column is moveable.
        bool anyMoveable = _moveableColumns.Count > 0;
        IntPtr selSetAllowsColumnReordering = sel_registerName("setAllowsColumnReordering:");
        objc_msgSend_void(_tableView, selSetAllowsColumnReordering, anyMoveable);
    }

    public int PackColumn(int columnIndex)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return 0;

        IntPtr column = _columns[columnIndex];

        // Get column width using sizeToFit if available, otherwise estimate
        IntPtr selSizeToFit = sel_registerName("sizeToFit");
        objc_msgSend(column, selSizeToFit);

        // Get the new width
        IntPtr selWidth = sel_registerName("width");
        double width = objc_msgSend_double_ret(column, selWidth);

        return (int)Math.Max(width, 50);
    }

    // Row Management
    public int AddItem()
    {
        return AddItem(_rows.Count);
    }

    public int AddItem(int index)
    {
        if (_disposed) return -1;

        if (index < 0 || index > _rows.Count)
            index = _rows.Count;

        var rowData = new RowData();
        _rows.Insert(index, rowData);

        // Reload table to show new row
        objc_msgSend(_tableView, _selReloadData);

        return index;
    }

    public void RemoveItem(int itemIndex)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count) return;

        _rows.RemoveAt(itemIndex);
        objc_msgSend(_tableView, _selReloadData);
    }

    public void RemoveAllItems()
    {
        if (_disposed) return;

        _rows.Clear();
        objc_msgSend(_tableView, _selReloadData);
    }

    public void SetItemText(int itemIndex, int columnIndex, string text)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count || columnIndex < 0) return;

        _rows[itemIndex].ColumnText[columnIndex] = text ?? "";

        // Reload the specific row
        IntPtr selIndexSetWithIndex = sel_registerName("indexSetWithIndex:");
        IntPtr rowIndexSet = objc_msgSend(_nsIndexSetClass, selIndexSetWithIndex, new IntPtr(itemIndex));
        IntPtr columnIndexSet = CreateIndexSetForAllColumns();
        objc_msgSend(_tableView, _selReloadDataForRowIndexes, rowIndexSet, columnIndexSet);
    }

    public string GetItemText(int itemIndex, int columnIndex)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count || columnIndex < 0)
            return "";

        return _rows[itemIndex].ColumnText.TryGetValue(columnIndex, out var text) ? text : "";
    }

    public void SetItemImage(int itemIndex, int columnIndex, IPlatformImage? image)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count || columnIndex < 0) return;

        if (image is MacOSImage macImage)
        {
            _rows[itemIndex].ColumnImages[columnIndex] = macImage.GetNativeHandle();
        }
        else
        {
            _rows[itemIndex].ColumnImages.Remove(columnIndex);
        }

        // Reload the specific row
        IntPtr selIndexSetWithIndex = sel_registerName("indexSetWithIndex:");
        IntPtr rowIndexSet = objc_msgSend(_nsIndexSetClass, selIndexSetWithIndex, new IntPtr(itemIndex));
        IntPtr columnIndexSet = CreateIndexSetForAllColumns();
        objc_msgSend(_tableView, _selReloadDataForRowIndexes, rowIndexSet, columnIndexSet);
    }

    // Header and Grid Lines
    public void SetHeaderVisible(bool visible)
    {
        if (_disposed) return;

        _headerVisible = visible;

        if (visible)
        {
            // Create and set header view
            IntPtr headerView = objc_msgSend(_nsTableHeaderViewClass, sel_registerName("alloc"));
            headerView = objc_msgSend(headerView, sel_registerName("init"));
            objc_msgSend(_tableView, _selSetHeaderView, headerView);
        }
        else
        {
            // Remove header view
            objc_msgSend(_tableView, _selSetHeaderView, IntPtr.Zero);
        }
    }

    public bool GetHeaderVisible()
    {
        return _headerVisible;
    }

    public void SetLinesVisible(bool visible)
    {
        if (_disposed) return;

        _linesVisible = visible;

        int gridStyle = visible
            ? (NSTableViewSolidVerticalGridLineMask | NSTableViewSolidHorizontalGridLineMask)
            : NSTableViewGridNone;

        objc_msgSend(_tableView, _selSetGridStyleMask, new IntPtr(gridStyle));
    }

    public bool GetLinesVisible()
    {
        return _linesVisible;
    }

    // Selection
    public void SetSelection(int[] indices)
    {
        if (_disposed) return;

        // Deselect all first
        objc_msgSend(_tableView, _selDeselectAll, IntPtr.Zero);

        if (indices != null && indices.Length > 0)
        {
            // Create index set with all selected indices
            IntPtr indexSet = CreateIndexSet(indices);
            IntPtr selSelect = sel_registerName("selectRowIndexes:byExtendingSelection:");
            objc_msgSend(_tableView, selSelect, indexSet, IntPtr.Zero); // Pass IntPtr.Zero for false
        }
    }

    public int[] GetSelection()
    {
        if (_disposed) return Array.Empty<int>();

        IntPtr selectedIndexes = objc_msgSend(_tableView, _selSelectedRowIndexes);
        return IndexSetToArray(selectedIndexes);
    }

    public int GetItemCount()
    {
        if (_disposed) return 0;
        return _rows.Count;
    }

    public int GetColumnCount()
    {
        if (_disposed) return 0;
        return _columns.Count;
    }

    public void SetColumnToolTip(int columnIndex, string? tooltip)
    {
        // NSTableColumn doesn't have direct tooltip support
        // Would require custom header cell implementation
    }

    public void ShowItem(int itemIndex)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count) return;

        // [tableView scrollRowToVisible:itemIndex]
        IntPtr selScrollRowToVisible = sel_registerName("scrollRowToVisible:");
        objc_msgSend(_tableView, selScrollRowToVisible, new IntPtr(itemIndex));
    }

    // IPlatformComposite implementation
    public void AddChild(IPlatformWidget child) { /* Tables don't have child widgets */ }
    public void RemoveChild(IPlatformWidget child) { /* Tables don't have child widgets */ }
    public IReadOnlyList<IPlatformWidget> GetChildren() => Array.Empty<IPlatformWidget>();

    // IPlatformWidget implementation
    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed) return;

        IntPtr selSetFrame = sel_registerName("setFrame:");
        CGRect frame = new CGRect(x, y, width, height);
        objc_msgSend_rect(_scrollView, selSetFrame, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed) return default;

        IntPtr selFrame = sel_registerName("frame");
        CGRect frame = default;
        objc_msgSend_stret(ref frame, _scrollView, selFrame);
        return new Rectangle((int)frame.origin.x, (int)frame.origin.y, (int)frame.size.width, (int)frame.size.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed) return;

        IntPtr selSetHidden = sel_registerName("setHidden:");
        objc_msgSend(_scrollView, selSetHidden, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed) return false;

        IntPtr selIsHidden = sel_registerName("isHidden");
        return !objc_msgSend_bool(_scrollView, selIsHidden);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed) return;

        IntPtr selSetEnabled = sel_registerName("setEnabled:");
        objc_msgSend(_tableView, selSetEnabled, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed) return false;

        IntPtr selIsEnabled = sel_registerName("isEnabled");
        return objc_msgSend_bool(_tableView, selIsEnabled);
    }

    private RGB _backgroundColor = new RGB(255, 255, 255);
    private RGB _foregroundColor = new RGB(0, 0, 0);

    public void SetBackground(RGB color)
    {
        // NSTableView background color can be set via setBackgroundColor: but this
        // may interfere with alternating row colors and selection highlighting.
        // Store value for GetBackground() API compatibility.
        _backgroundColor = color;
    }

    public RGB GetBackground()
    {
        return _backgroundColor;
    }

    public void SetForeground(RGB color)
    {
        // NSTableView text color requires custom cell rendering or attributed strings.
        // Default behavior uses system appearance colors. Store for getter.
        _foregroundColor = color;
    }

    public RGB GetForeground()
    {
        return _foregroundColor;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _columns.Clear();
        _rows.Clear();

        if (_scrollView != IntPtr.Zero)
        {
            IntPtr selRelease = sel_registerName("release");
            objc_msgSend(_scrollView, selRelease);
            _scrollView = IntPtr.Zero;
        }

        _tableView = IntPtr.Zero;
    }

    // Helper methods
    private IntPtr CreateNSString(string str)
    {
        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr selStringWithUTF8String = sel_registerName("stringWithUTF8String:");
        return objc_msgSend(nsStringClass, selStringWithUTF8String, str);
    }

    private IntPtr CreateIndexSet(int[] indices)
    {
        IntPtr indexSet = objc_msgSend(_nsIndexSetClass, sel_registerName("indexSet"));
        IntPtr mutableIndexSet = objc_msgSend(indexSet, sel_registerName("mutableCopy"));

        IntPtr selAddIndex = sel_registerName("addIndex:");
        foreach (int index in indices)
        {
            if (index >= 0 && index < _rows.Count)
            {
                objc_msgSend(mutableIndexSet, selAddIndex, new IntPtr(index));
            }
        }

        return mutableIndexSet;
    }

    private IntPtr CreateIndexSetForAllColumns()
    {
        IntPtr selIndexSetWithIndexesInRange = sel_registerName("indexSetWithIndexesInRange:");
        NSRange range = new NSRange(0, _columns.Count);
        return objc_msgSend(_nsIndexSetClass, selIndexSetWithIndexesInRange, range);
    }

    private int[] IndexSetToArray(IntPtr indexSet)
    {
        IntPtr selCount = sel_registerName("count");
        int count = (int)objc_msgSend(indexSet, selCount);

        if (count == 0) return Array.Empty<int>();

        var indices = new List<int>();
        IntPtr selFirstIndex = sel_registerName("firstIndex");
        IntPtr selIndexGreaterThanIndex = sel_registerName("indexGreaterThanIndex:");

        int index = (int)objc_msgSend(indexSet, selFirstIndex);
        while (index != -1)
        {
            indices.Add(index);
            index = (int)objc_msgSend(indexSet, selIndexGreaterThanIndex, new IntPtr(index));
        }

        return indices.ToArray();
    }

    // Structures for Objective-C interop
    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double x;
        public double y;

        public CGPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double width;
        public double height;

        public CGSize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public CGPoint origin;
        public CGSize size;

        public CGRect(double x, double y, double width, double height)
        {
            origin = new CGPoint(x, y);
            size = new CGSize(width, height);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRange
    {
        public int location;
        public int length;

        public NSRange(int location, int length)
        {
            this.location = location;
            this.length = length;
        }
    }

    // Objective-C runtime imports
    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRange arg1);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

    // For methods that take a double/CGFloat argument (not returning float - that would need fpret on x64)
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double(IntPtr receiver, IntPtr selector, double arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend_fpret")]
    private static extern double objc_msgSend_fpret_ret(IntPtr receiver, IntPtr selector);

    // For setFrame: which takes CGRect as input argument (not returns it)
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect arg);

    // Architecture-specific struct return handling:
    // - ARM64: objc_msgSend_stret doesn't exist, use objc_msgSend with direct return
    // - x86_64: objc_msgSend_stret required for structs > 16 bytes (CGRect is 32 bytes)
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern CGRect objc_msgSend_cgrect_arm64(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret_x64(out CGRect retval, IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern CGRect objc_msgSend_cgrect_arg_arm64(IntPtr receiver, IntPtr selector, CGRect arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret_arg_x64(out CGRect retval, IntPtr receiver, IntPtr selector, CGRect arg1);

    private static void objc_msgSend_stret(ref CGRect retval, IntPtr receiver, IntPtr selector)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            retval = objc_msgSend_cgrect_arm64(receiver, selector);
        }
        else
        {
            objc_msgSend_stret_x64(out retval, receiver, selector);
        }
    }

    private static void objc_msgSend_stret(ref CGRect retval, IntPtr receiver, IntPtr selector, CGRect arg1)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            retval = objc_msgSend_cgrect_arg_arm64(receiver, selector, arg1);
        }
        else
        {
            objc_msgSend_stret_arg_x64(out retval, receiver, selector, arg1);
        }
    }

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    private static extern double objc_msgSend_double_ret(IntPtr receiver, IntPtr selector);
}
