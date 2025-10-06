using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - List widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    private IntPtr _nsTableViewClass;
    private IntPtr _nsTableColumnClass;
    private IntPtr _nsIndexSetClass;
    private IntPtr _selReloadData;
    private IntPtr _selNumberOfRows;
    private IntPtr _selSelectRowIndexes;
    private IntPtr _selSelectedRowIndexes;
    private IntPtr _selScrollRowToVisible;
    private IntPtr _selAddTableColumn;
    private IntPtr _selSetDataSource;
    private IntPtr _selSetDelegate;
    private IntPtr _selIndexSetWithIndex;
    private IntPtr _selIndexSetWithIndexesInRange;
    private IntPtr _selFirstIndex;
    private IntPtr _selIndexGreaterThanIndex;
    private IntPtr _selCount_indexSet;
    private IntPtr _selSetAllowsMultipleSelection;
    private IntPtr _selSetHeaderView;
    private IntPtr _selInitWithIdentifier;
    private IntPtr _selRowAtPoint;
    private IntPtr _selVisibleRect;

    // NSTableColumn
    private IntPtr _selSetWidth;
    private IntPtr _selSetMinWidth;
    private IntPtr _selSetMaxWidth;

    // NSScrollView for table
    private IntPtr _selSetHasVerticalScroller;
    private IntPtr _selSetDocumentView;
    private IntPtr _selSetAutohidesScrollers;

    // Store list items per list control
    private readonly Dictionary<IntPtr, List<string>> _listItems = new();

    private void InitializeListSelectors()
    {
        if (_nsTableViewClass == IntPtr.Zero)
        {
            _nsTableViewClass = objc_getClass("NSTableView");
            _nsTableColumnClass = objc_getClass("NSTableColumn");
            _nsIndexSetClass = objc_getClass("NSIndexSet");

            if (_nsScrollViewClass == IntPtr.Zero)
            {
                _nsScrollViewClass = objc_getClass("NSScrollView");
            }

            _selReloadData = sel_registerName("reloadData");
            _selNumberOfRows = sel_registerName("numberOfRows");
            _selSelectRowIndexes = sel_registerName("selectRowIndexes:byExtendingSelection:");
            _selSelectedRowIndexes = sel_registerName("selectedRowIndexes");
            _selScrollRowToVisible = sel_registerName("scrollRowToVisible:");
            _selAddTableColumn = sel_registerName("addTableColumn:");
            _selSetDataSource = sel_registerName("setDataSource:");
            _selSetDelegate = sel_registerName("setDelegate:");
            _selIndexSetWithIndex = sel_registerName("indexSetWithIndex:");
            _selIndexSetWithIndexesInRange = sel_registerName("indexSetWithIndexesInRange:");
            _selFirstIndex = sel_registerName("firstIndex");
            _selIndexGreaterThanIndex = sel_registerName("indexGreaterThanIndex:");
            _selCount_indexSet = sel_registerName("count");
            _selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
            _selSetHeaderView = sel_registerName("setHeaderView:");
            _selInitWithIdentifier = sel_registerName("initWithIdentifier:");
            _selRowAtPoint = sel_registerName("rowAtPoint:");
            _selVisibleRect = sel_registerName("visibleRect");
            _selSetWidth = sel_registerName("setWidth:");
            _selSetMinWidth = sel_registerName("setMinWidth:");
            _selSetMaxWidth = sel_registerName("setMaxWidth:");
            _selSetHasVerticalScroller = sel_registerName("setHasVerticalScroller:");
            _selSetDocumentView = sel_registerName("setDocumentView:");
            _selSetAutohidesScrollers = sel_registerName("setAutohidesScrollers:");
        }
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_ulong(IntPtr receiver, IntPtr selector, nuint arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nuint objc_msgSend_ret_ulong(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nuint objc_msgSend_ret_ulong_arg(IntPtr receiver, IntPtr selector, nuint arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double_arg(IntPtr receiver, IntPtr selector, double arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nint objc_msgSend_nint(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [StructLayout(LayoutKind.Sequential)]
    private struct NSPoint
    {
        public double x;
        public double y;
    }

    public IntPtr CreateList(IntPtr parentHandle, int style)
    {
        InitializeListSelectors();

        // Create NSScrollView to contain the table
        IntPtr scrollView = objc_msgSend(_nsScrollViewClass, _selAlloc);
        scrollView = objc_msgSend(scrollView, _selInit);

        // Create NSTableView
        IntPtr tableView = objc_msgSend(_nsTableViewClass, _selAlloc);
        tableView = objc_msgSend(tableView, _selInit);

        // Create a single column for the list
        IntPtr column = objc_msgSend(_nsTableColumnClass, _selAlloc);
        IntPtr columnIdentifier = CreateNSString("ListColumn");
        column = objc_msgSend(column, _selInitWithIdentifier, columnIdentifier);

        // Set column width
        objc_msgSend_double_arg(column, _selSetWidth, 200.0);
        objc_msgSend_double_arg(column, _selSetMinWidth, 50.0);
        objc_msgSend_double_arg(column, _selSetMaxWidth, 10000.0);

        // Add column to table
        objc_msgSend(tableView, _selAddTableColumn, column);

        // Hide header
        objc_msgSend(tableView, _selSetHeaderView, IntPtr.Zero);

        // Set selection mode
        bool multiSelect = (style & SWT.MULTI) != 0;
        objc_msgSend_void(tableView, _selSetAllowsMultipleSelection, multiSelect);

        // Configure scroll view
        objc_msgSend_void(scrollView, _selSetHasVerticalScroller, true);
        objc_msgSend_void(scrollView, _selSetAutohidesScrollers, true);
        objc_msgSend(scrollView, _selSetDocumentView, tableView);

        // Set default frame
        var frame = new CGRect(0, 0, 200, 150);
        objc_msgSend_rect(scrollView, _selSetFrame, frame);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parentHandle, selContentView);
            objc_msgSend(contentView, _selAddSubview, scrollView);
        }

        // Initialize empty items list for this control
        _listItems[scrollView] = new List<string>();

        // Note: In a full implementation, we would set up a data source delegate
        // For now, we're using a simplified approach with the items dictionary

        return scrollView;
    }

    public void AddListItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero || item == null)
            return;

        InitializeListSelectors();

        if (!_listItems.TryGetValue(handle, out var items))
        {
            items = new List<string>();
            _listItems[handle] = items;
        }

        // Add item to our list
        if (index < 0 || index >= items.Count)
        {
            items.Add(item);
        }
        else
        {
            items.Insert(index, item);
        }

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView != IntPtr.Zero)
        {
            // Reload table data
            objc_msgSend_void(tableView, _selReloadData);
        }
    }

    public void RemoveListItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeListSelectors();

        if (_listItems.TryGetValue(handle, out var items))
        {
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);

                // Get the table view from scroll view
                IntPtr tableView = objc_msgSend(handle, _selDocumentView);
                if (tableView != IntPtr.Zero)
                {
                    // Reload table data
                    objc_msgSend_void(tableView, _selReloadData);
                }
            }
        }
    }

    public void ClearListItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeListSelectors();

        if (_listItems.TryGetValue(handle, out var items))
        {
            items.Clear();

            // Get the table view from scroll view
            IntPtr tableView = objc_msgSend(handle, _selDocumentView);
            if (tableView != IntPtr.Zero)
            {
                // Reload table data
                objc_msgSend_void(tableView, _selReloadData);
            }
        }
    }

    public void SetListSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero || indices == null)
            return;

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return;

        if (indices.Length == 0)
        {
            // Deselect all
            IntPtr emptyIndexSet = objc_msgSend(_nsIndexSetClass, _selAlloc);
            emptyIndexSet = objc_msgSend(emptyIndexSet, _selInit);
            objc_msgSend(tableView, _selSelectRowIndexes, emptyIndexSet, IntPtr.Zero);
        }
        else if (indices.Length == 1)
        {
            // Single selection
            IntPtr indexSet = objc_msgSend_ulong(_nsIndexSetClass, _selIndexSetWithIndex, (nuint)indices[0]);
            objc_msgSend(tableView, _selSelectRowIndexes, indexSet, IntPtr.Zero);
        }
        else
        {
            // Multiple selection - need to build index set
            IntPtr mutableIndexSet = objc_getClass("NSMutableIndexSet");
            IntPtr indexSet = objc_msgSend(mutableIndexSet, _selAlloc);
            indexSet = objc_msgSend(indexSet, _selInit);

            IntPtr selAddIndex = sel_registerName("addIndex:");
            foreach (int index in indices)
            {
                objc_msgSend_ulong(indexSet, selAddIndex, (nuint)index);
            }

            objc_msgSend(tableView, _selSelectRowIndexes, indexSet, IntPtr.Zero);
        }
    }

    public int[] GetListSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return Array.Empty<int>();

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return Array.Empty<int>();

        // Get selected row indexes
        IntPtr indexSet = objc_msgSend(tableView, _selSelectedRowIndexes);
        if (indexSet == IntPtr.Zero)
            return Array.Empty<int>();

        // Get count of selected items
        nuint count = objc_msgSend_ret_ulong(indexSet, _selCount_indexSet);
        if (count == 0)
            return Array.Empty<int>();

        // Extract indices
        var indices = new List<int>();
        nuint currentIndex = objc_msgSend_ret_ulong(indexSet, _selFirstIndex);

        const ulong NSNotFound = ulong.MaxValue;
        while (currentIndex != (nuint)NSNotFound && indices.Count < (int)count)
        {
            indices.Add((int)currentIndex);
            currentIndex = objc_msgSend_ret_ulong_arg(indexSet, _selIndexGreaterThanIndex, currentIndex);
        }

        return indices.ToArray();
    }

    public int GetListTopIndex(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return 0;

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return 0;

        // Get visible rect
        CGRect visibleRect;
        objc_msgSend_stret(out visibleRect, tableView, _selVisibleRect);

        // Get row at top of visible rect
        NSPoint topPoint = new NSPoint { x = visibleRect.x, y = visibleRect.y };

        IntPtr selRowAtPoint = sel_registerName("rowAtPoint:");
        long row;
        unsafe
        {
            row = (long)objc_msgSend(tableView, selRowAtPoint, (IntPtr)(&topPoint));
        }

        return row >= 0 ? (int)row : 0;
    }

    public void SetListTopIndex(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || index < 0)
            return;

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return;

        // Scroll to make row visible at top
        objc_msgSend_ulong(tableView, _selScrollRowToVisible, (nuint)index);
    }

    // Combo operations - implemented in MacOSPlatform_Combo.cs
}
