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

    // REMOVED METHOD (moved to IListWidget interface):
    // - CreateList(IntPtr parentHandle, int style)
    // This method is now implemented via the IListWidget interface using proper handles

    // REMOVED METHODS (moved to IListWidget interface):
    // - AddListItem(IntPtr handle, string item, int index)
    // - RemoveListItem(IntPtr handle, int index)
    // - ClearListItems(IntPtr handle)
    // - SetListSelection(IntPtr handle, int[] indices)
    // - GetListSelection(IntPtr handle)
    // - GetListTopIndex(IntPtr handle)
    // - SetListTopIndex(IntPtr handle, int index)
    // These methods are now implemented via the IListWidget interface using proper handles

    // Combo operations - implemented in MacOSPlatform_Combo.cs
}
