using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Table widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Common Controls library
    private const string ComCtl32 = "comctl32.dll";

    // ListView class name
    private const string WC_LISTVIEW = "SysListView32";
    private const string WC_HEADER = "SysHeader32";

    // ListView styles
    private const uint LVS_ICON = 0x0000;
    private const uint LVS_REPORT = 0x0001;
    private const uint LVS_SMALLICON = 0x0002;
    private const uint LVS_LIST = 0x0003;
    private const uint LVS_TYPEMASK = 0x0003;
    private const uint LVS_SINGLESEL = 0x0004;
    private const uint LVS_SHOWSELALWAYS = 0x0008;
    private const uint LVS_SORTASCENDING = 0x0010;
    private const uint LVS_SORTDESCENDING = 0x0020;
    private const uint LVS_SHAREIMAGELISTS = 0x0040;
    private const uint LVS_NOLABELWRAP = 0x0080;
    private const uint LVS_AUTOARRANGE = 0x0100;
    private const uint LVS_EDITLABELS = 0x0200;
    private const uint LVS_OWNERDATA = 0x1000;
    private const uint LVS_NOSCROLL = 0x2000;
    private const uint LVS_ALIGNTOP = 0x0000;
    private const uint LVS_ALIGNLEFT = 0x0800;
    private const uint LVS_NOCOLUMNHEADER = 0x4000;
    private const uint LVS_NOSORTHEADER = 0x8000;

    // ListView extended styles
    private const uint LVS_EX_GRIDLINES = 0x00000001;
    private const uint LVS_EX_SUBITEMIMAGES = 0x00000002;
    private const uint LVS_EX_CHECKBOXES = 0x00000004;
    private const uint LVS_EX_TRACKSELECT = 0x00000008;
    private const uint LVS_EX_HEADERDRAGDROP = 0x00000010;
    private const uint LVS_EX_FULLROWSELECT = 0x00000020;
    private const uint LVS_EX_ONECLICKACTIVATE = 0x00000040;
    private const uint LVS_EX_TWOCLICKACTIVATE = 0x00000080;
    private const uint LVS_EX_FLATSB = 0x00000100;
    private const uint LVS_EX_REGIONAL = 0x00000200;
    private const uint LVS_EX_INFOTIP = 0x00000400;
    private const uint LVS_EX_UNDERLINEHOT = 0x00000800;
    private const uint LVS_EX_UNDERLINECOLD = 0x00001000;
    private const uint LVS_EX_MULTIWORKAREAS = 0x00002000;

    // ListView messages
    private const uint LVM_FIRST = 0x1000;
    private const uint LVM_GETBKCOLOR = LVM_FIRST + 0;
    private const uint LVM_SETBKCOLOR = LVM_FIRST + 1;
    private const uint LVM_GETIMAGELIST = LVM_FIRST + 2;
    private const uint LVM_SETIMAGELIST = LVM_FIRST + 3;
    private const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
    private const uint LVM_GETITEM = LVM_FIRST + 5;
    private const uint LVM_SETITEM = LVM_FIRST + 6;
    private const uint LVM_INSERTITEM = LVM_FIRST + 7;
    private const uint LVM_DELETEITEM = LVM_FIRST + 8;
    private const uint LVM_DELETEALLITEMS = LVM_FIRST + 9;
    private const uint LVM_GETCALLBACKMASK = LVM_FIRST + 10;
    private const uint LVM_SETCALLBACKMASK = LVM_FIRST + 11;
    private const uint LVM_GETNEXTITEM = LVM_FIRST + 12;
    private const uint LVM_FINDITEM = LVM_FIRST + 13;
    private const uint LVM_GETITEMRECT = LVM_FIRST + 14;
    private const uint LVM_SETITEMPOSITION = LVM_FIRST + 15;
    private const uint LVM_GETITEMPOSITION = LVM_FIRST + 16;
    private const uint LVM_GETSTRINGWIDTH = LVM_FIRST + 17;
    private const uint LVM_HITTEST = LVM_FIRST + 18;
    private const uint LVM_ENSUREVISIBLE = LVM_FIRST + 19;
    private const uint LVM_SCROLL = LVM_FIRST + 20;
    private const uint LVM_REDRAWITEMS = LVM_FIRST + 21;
    private const uint LVM_ARRANGE = LVM_FIRST + 22;
    private const uint LVM_EDITLABEL = LVM_FIRST + 23;
    private const uint LVM_GETEDITCONTROL = LVM_FIRST + 24;
    private const uint LVM_GETCOLUMN = LVM_FIRST + 25;
    private const uint LVM_SETCOLUMN = LVM_FIRST + 26;
    private const uint LVM_INSERTCOLUMN = LVM_FIRST + 27;
    private const uint LVM_DELETECOLUMN = LVM_FIRST + 28;
    private const uint LVM_GETCOLUMNWIDTH = LVM_FIRST + 29;
    private const uint LVM_SETCOLUMNWIDTH = LVM_FIRST + 30;
    private const uint LVM_GETHEADER = LVM_FIRST + 31;
    private const uint LVM_CREATEDRAGIMAGE = LVM_FIRST + 33;
    private const uint LVM_GETVIEWRECT = LVM_FIRST + 34;
    private const uint LVM_GETTEXTCOLOR = LVM_FIRST + 35;
    private const uint LVM_SETTEXTCOLOR = LVM_FIRST + 36;
    private const uint LVM_GETTEXTBKCOLOR = LVM_FIRST + 37;
    private const uint LVM_SETTEXTBKCOLOR = LVM_FIRST + 38;
    private const uint LVM_GETTOPINDEX = LVM_FIRST + 39;
    private const uint LVM_GETCOUNTPERPAGE = LVM_FIRST + 40;
    private const uint LVM_GETORIGIN = LVM_FIRST + 41;
    private const uint LVM_UPDATE = LVM_FIRST + 42;
    private const uint LVM_SETITEMSTATE = LVM_FIRST + 43;
    private const uint LVM_GETITEMSTATE = LVM_FIRST + 44;
    private const uint LVM_GETITEMTEXT = LVM_FIRST + 45;
    private const uint LVM_SETITEMTEXT = LVM_FIRST + 46;
    private const uint LVM_SETITEMCOUNT = LVM_FIRST + 47;
    private const uint LVM_SORTITEMS = LVM_FIRST + 48;
    private const uint LVM_SETITEMPOSITION32 = LVM_FIRST + 49;
    private const uint LVM_GETSELECTEDCOUNT = LVM_FIRST + 50;
    private const uint LVM_GETITEMSPACING = LVM_FIRST + 51;
    private const uint LVM_GETISEARCHSTRING = LVM_FIRST + 52;
    private const uint LVM_SETICONSPACING = LVM_FIRST + 53;
    private const uint LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
    private const uint LVM_GETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 55;
    private const uint LVM_GETSUBITEMRECT = LVM_FIRST + 56;
    private const uint LVM_SUBITEMHITTEST = LVM_FIRST + 57;
    private const uint LVM_SETCOLUMNORDERARRAY = LVM_FIRST + 58;
    private const uint LVM_GETCOLUMNORDERARRAY = LVM_FIRST + 59;
    private const uint LVM_SETHOTITEM = LVM_FIRST + 60;
    private const uint LVM_GETHOTITEM = LVM_FIRST + 61;
    private const uint LVM_SETHOTCURSOR = LVM_FIRST + 62;
    private const uint LVM_GETHOTCURSOR = LVM_FIRST + 63;
    private const uint LVM_APPROXIMATEVIEWRECT = LVM_FIRST + 64;
    private const uint LVM_SETWORKAREAS = LVM_FIRST + 65;

    // LVCOLUMN mask flags
    private const uint LVCF_FMT = 0x0001;
    private const uint LVCF_WIDTH = 0x0002;
    private const uint LVCF_TEXT = 0x0004;
    private const uint LVCF_SUBITEM = 0x0008;
    private const uint LVCF_IMAGE = 0x0010;
    private const uint LVCF_ORDER = 0x0020;

    // LVCOLUMN format flags
    private const uint LVCFMT_LEFT = 0x0000;
    private const uint LVCFMT_RIGHT = 0x0001;
    private const uint LVCFMT_CENTER = 0x0002;
    private const uint LVCFMT_JUSTIFYMASK = 0x0003;
    private const uint LVCFMT_IMAGE = 0x0800;
    private const uint LVCFMT_BITMAP_ON_RIGHT = 0x1000;
    private const uint LVCFMT_COL_HAS_IMAGES = 0x8000;

    // LVITEM mask flags
    private const uint LVIF_TEXT = 0x0001;
    private const uint LVIF_IMAGE = 0x0002;
    private const uint LVIF_PARAM = 0x0004;
    private const uint LVIF_STATE = 0x0008;
    private const uint LVIF_INDENT = 0x0010;
    private const uint LVIF_NORECOMPUTE = 0x0800;

    // LVITEM state flags
    private const uint LVIS_FOCUSED = 0x0001;
    private const uint LVIS_SELECTED = 0x0002;
    private const uint LVIS_CUT = 0x0004;
    private const uint LVIS_DROPHILITED = 0x0008;
    private const uint LVIS_ACTIVATING = 0x0020;
    private const uint LVIS_OVERLAYMASK = 0x0F00;
    private const uint LVIS_STATEIMAGEMASK = 0xF000;

    // State image mask helpers (for checkboxes)
    private const uint INDEXTOSTATEIMAGEMASK_UNCHECKED = 0x1000;
    private const uint INDEXTOSTATEIMAGEMASK_CHECKED = 0x2000;

    // Column width values
    private const int LVSCW_AUTOSIZE = -1;
    private const int LVSCW_AUTOSIZE_USEHEADER = -2;

    // LVCOLUMN structure
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LVCOLUMN
    {
        public uint mask;
        public uint fmt;
        public int cx;
        public string? pszText;
        public int cchTextMax;
        public int iSubItem;
        public int iImage;
        public int iOrder;
    }

    // LVITEM structure
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LVITEM
    {
        public uint mask;
        public int iItem;
        public int iSubItem;
        public uint state;
        public uint stateMask;
        public string? pszText;
        public int cchTextMax;
        public int iImage;
        public IntPtr lParam;
        public int iIndent;
    }

    // InitCommonControlsEx structure
    [StructLayout(LayoutKind.Sequential)]
    private struct INITCOMMONCONTROLSEX
    {
        public int dwSize;
        public int dwICC;
    }

    // Common control classes
    private const int ICC_LISTVIEW_CLASSES = 0x00000001;
    private const int ICC_TREEVIEW_CLASSES = 0x00000002;
    private const int ICC_BAR_CLASSES = 0x00000004;
    private const int ICC_TAB_CLASSES = 0x00000008;

#if NET8_0_OR_GREATER
    [LibraryImport(ComCtl32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool InitCommonControlsEx(ref INITCOMMONCONTROLSEX picce);
#else
    [DllImport(ComCtl32)]
    private static extern bool InitCommonControlsEx(ref INITCOMMONCONTROLSEX picce);
#endif

    // SendMessage overloads for ListView
    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref LVCOLUMN lParam);

    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref LVITEM lParam);

    // Data structures to track table data
    private sealed class TableData
    {
        public IntPtr ListView { get; set; }
        public List<IntPtr> Columns { get; set; } = new();
        public Dictionary<int, TableItemData> Items { get; set; } = new();
        public bool HeaderVisible { get; set; } = true;
        public bool LinesVisible { get; set; } = false;
        public int NextItemId { get; set; } = 1;
    }

    private sealed class TableItemData
    {
        public IntPtr TableHandle { get; set; }
        public int ItemIndex { get; set; }
        public Dictionary<int, string> ColumnText { get; set; } = new();
    }

    private sealed class TableColumnData
    {
        public IntPtr TableHandle { get; set; }
        public int ColumnIndex { get; set; }
        public bool Resizable { get; set; } = true;
        public bool Moveable { get; set; } = false;
    }

    private readonly Dictionary<IntPtr, TableData> _tableData = new();
    private readonly Dictionary<IntPtr, TableItemData> _tableItemData = new();
    private readonly Dictionary<IntPtr, TableColumnData> _tableColumnData = new();
    private static bool _commonControlsInitialized = false;

    private void EnsureCommonControlsInitialized()
    {
        if (!_commonControlsInitialized)
        {
            var icex = new INITCOMMONCONTROLSEX
            {
                dwSize = Marshal.SizeOf<INITCOMMONCONTROLSEX>(),
                dwICC = ICC_LISTVIEW_CLASSES | ICC_TREEVIEW_CLASSES | ICC_BAR_CLASSES | ICC_TAB_CLASSES
            };
            InitCommonControlsEx(ref icex);
            _commonControlsInitialized = true;
        }
    }

    // Table operations
    public IntPtr CreateTable(IntPtr parent, int style)
    {
        EnsureCommonControlsInitialized();

        uint windowStyle = WS_CHILD | WS_VISIBLE | LVS_REPORT | WS_BORDER;

        // Handle multi-selection
        if ((style & SWT.MULTI) == 0)
        {
            windowStyle |= LVS_SINGLESEL;
        }

        // Always show selection
        windowStyle |= LVS_SHOWSELALWAYS;

        var handle = CreateWindowEx(
            0,
            WC_LISTVIEW,
            string.Empty,
            windowStyle,
            0, 0, 100, 100,
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create ListView. Error: {error}");
        }

        // Set extended styles
        uint exStyle = LVS_EX_FULLROWSELECT;

        // Add checkboxes if needed
        if ((style & SWT.CHECK) != 0)
        {
            exStyle |= LVS_EX_CHECKBOXES;
        }

        SendMessage(handle, LVM_SETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, new IntPtr(exStyle));

        // Store table data
        var tableData = new TableData
        {
            ListView = handle,
            HeaderVisible = true
        };
        _tableData[handle] = tableData;

        return handle;
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            return;

        uint style = GetWindowLong(handle, -16); // GWL_STYLE
        if (visible)
        {
            style &= ~LVS_NOCOLUMNHEADER;
        }
        else
        {
            style |= LVS_NOCOLUMNHEADER;
        }
        SetWindowLong(handle, -16, style); // GWL_STYLE

        tableData.HeaderVisible = visible;

        // Force redraw
        SendMessage(handle, LVM_UPDATE, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            return;

        uint exStyle = (uint)SendMessage(handle, LVM_GETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (visible)
        {
            exStyle |= LVS_EX_GRIDLINES;
        }
        else
        {
            exStyle &= ~LVS_EX_GRIDLINES;
        }

        SendMessage(handle, LVM_SETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, new IntPtr(exStyle));
        tableData.LinesVisible = visible;
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            return;

        // First, deselect all items
        LVITEM lvi = new LVITEM
        {
            stateMask = LVIS_SELECTED
        };
        SendMessage(handle, LVM_SETITEMSTATE, new IntPtr(-1), ref lvi);

        // Then select specified indices
        if (indices != null && indices.Length > 0)
        {
            foreach (int index in indices)
            {
                lvi = new LVITEM
                {
                    state = LVIS_SELECTED,
                    stateMask = LVIS_SELECTED
                };
                SendMessage(handle, LVM_SETITEMSTATE, new IntPtr(index), ref lvi);
            }
        }
    }

    public void ClearTableItems(IntPtr handle)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            return;

        SendMessage(handle, LVM_DELETEALLITEMS, IntPtr.Zero, IntPtr.Zero);

        // Clear item tracking
        var itemsToRemove = _tableItemData.Where(kvp => kvp.Value.TableHandle == handle).Select(kvp => kvp.Key).ToList();
        foreach (var itemHandle in itemsToRemove)
        {
            _tableItemData.Remove(itemHandle);
        }

        tableData.Items.Clear();
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            return;

        SendMessage(handle, LVM_ENSUREVISIBLE, new IntPtr(index), IntPtr.Zero);
    }

    // TableColumn operations
    public IntPtr CreateTableColumn(IntPtr parent, int style, int index)
    {
        if (!_tableData.TryGetValue(parent, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(parent));

        // Determine alignment
        uint fmt = LVCFMT_LEFT;
        if ((style & SWT.CENTER) != 0)
            fmt = LVCFMT_CENTER;
        else if ((style & SWT.RIGHT) != 0)
            fmt = LVCFMT_RIGHT;

        LVCOLUMN column = new LVCOLUMN
        {
            mask = LVCF_FMT | LVCF_WIDTH | LVCF_TEXT | LVCF_SUBITEM,
            fmt = fmt,
            cx = 100, // Default width
            pszText = string.Empty,
            cchTextMax = 0,
            iSubItem = index >= 0 ? index : tableData.Columns.Count
        };

        int columnIndex = (int)SendMessage(tableData.ListView, LVM_INSERTCOLUMN,
            new IntPtr(index >= 0 ? index : tableData.Columns.Count), ref column).ToInt32();

        if (columnIndex == -1)
        {
            throw new InvalidOperationException("Failed to insert column");
        }

        // Create a pseudo-handle for the column (encode table handle + column index)
        IntPtr columnHandle = new IntPtr(parent.ToInt64() + columnIndex + 1);
        tableData.Columns.Add(columnHandle);

        // Store column data
        _tableColumnData[columnHandle] = new TableColumnData
        {
            TableHandle = parent,
            ColumnIndex = columnIndex,
            Resizable = true,
            Moveable = false
        };

        return columnHandle;
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return;

        if (_tableData.TryGetValue(columnData.TableHandle, out var tableData))
        {
            SendMessage(tableData.ListView, LVM_DELETECOLUMN, new IntPtr(columnData.ColumnIndex), IntPtr.Zero);
            tableData.Columns.Remove(handle);
        }

        _tableColumnData.Remove(handle);
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return;

        if (!_tableData.TryGetValue(columnData.TableHandle, out var tableData))
            return;

        LVCOLUMN column = new LVCOLUMN
        {
            mask = LVCF_TEXT,
            pszText = text,
            cchTextMax = text.Length
        };

        SendMessage(tableData.ListView, LVM_SETCOLUMN, new IntPtr(columnData.ColumnIndex), ref column);
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return;

        if (!_tableData.TryGetValue(columnData.TableHandle, out var tableData))
            return;

        SendMessage(tableData.ListView, LVM_SETCOLUMNWIDTH, new IntPtr(columnData.ColumnIndex), new IntPtr(width));
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return;

        if (!_tableData.TryGetValue(columnData.TableHandle, out var tableData))
            return;

        uint fmt = LVCFMT_LEFT;
        if ((alignment & SWT.CENTER) != 0)
            fmt = LVCFMT_CENTER;
        else if ((alignment & SWT.RIGHT) != 0)
            fmt = LVCFMT_RIGHT;

        LVCOLUMN column = new LVCOLUMN
        {
            mask = LVCF_FMT,
            fmt = fmt
        };

        SendMessage(tableData.ListView, LVM_SETCOLUMN, new IntPtr(columnData.ColumnIndex), ref column);
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return;

        columnData.Resizable = resizable;
        // Note: Win32 ListView columns are always resizable by default
        // Preventing resize would require custom handling of WM_NOTIFY messages
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return;

        if (!_tableData.TryGetValue(columnData.TableHandle, out var tableData))
            return;

        columnData.Moveable = moveable;

        // Enable/disable header drag-drop
        uint exStyle = (uint)SendMessage(tableData.ListView, LVM_GETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (moveable)
        {
            exStyle |= LVS_EX_HEADERDRAGDROP;
        }
        else
        {
            exStyle &= ~LVS_EX_HEADERDRAGDROP;
        }

        SendMessage(tableData.ListView, LVM_SETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, new IntPtr(exStyle));
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTip)
    {
        // Win32 ListView column tooltips require header control manipulation
        // Not implemented in this basic version
    }

    public int PackTableColumn(IntPtr handle)
    {
        if (!_tableColumnData.TryGetValue(handle, out var columnData))
            return 0;

        if (!_tableData.TryGetValue(columnData.TableHandle, out var tableData))
            return 0;

        // Auto-size to content
        SendMessage(tableData.ListView, LVM_SETCOLUMNWIDTH, new IntPtr(columnData.ColumnIndex), new IntPtr(LVSCW_AUTOSIZE));

        // Get the new width
        int width = SendMessage(tableData.ListView, LVM_GETCOLUMNWIDTH, new IntPtr(columnData.ColumnIndex), IntPtr.Zero).ToInt32();
        return width;
    }

    // TableItem operations
    public IntPtr CreateTableItem(IntPtr parent, int style, int index)
    {
        if (!_tableData.TryGetValue(parent, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(parent));

        int itemIndex = index;
        if (itemIndex < 0)
        {
            // Append at end
            itemIndex = SendMessage(tableData.ListView, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }

        LVITEM item = new LVITEM
        {
            mask = LVIF_TEXT | LVIF_PARAM,
            iItem = itemIndex,
            iSubItem = 0,
            pszText = string.Empty,
            lParam = new IntPtr(tableData.NextItemId)
        };

        int actualIndex = SendMessage(tableData.ListView, LVM_INSERTITEM, IntPtr.Zero, ref item).ToInt32();

        if (actualIndex == -1)
        {
            throw new InvalidOperationException("Failed to insert item");
        }

        // Create pseudo-handle for item
        IntPtr itemHandle = new IntPtr(tableData.NextItemId++);

        var itemData = new TableItemData
        {
            TableHandle = parent,
            ItemIndex = actualIndex
        };

        tableData.Items[actualIndex] = itemData;
        _tableItemData[itemHandle] = itemData;

        return itemHandle;
    }

    public void DestroyTableItem(IntPtr handle)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableHandle, out var tableData))
            return;

        SendMessage(tableData.ListView, LVM_DELETEITEM, new IntPtr(itemData.ItemIndex), IntPtr.Zero);

        tableData.Items.Remove(itemData.ItemIndex);
        _tableItemData.Remove(handle);
    }

    public void SetTableItemText(IntPtr handle, int column, string text)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableHandle, out var tableData))
            return;

        LVITEM item = new LVITEM
        {
            mask = LVIF_TEXT,
            iItem = itemData.ItemIndex,
            iSubItem = column,
            pszText = text,
            cchTextMax = text.Length
        };

        SendMessage(tableData.ListView, LVM_SETITEM, IntPtr.Zero, ref item);

        // Store text in item data
        itemData.ColumnText[column] = text;
    }

    public void SetTableItemImage(IntPtr handle, int column, IntPtr image)
    {
        // Image list support would be required
        // Not implemented in this basic version
    }

    public void SetTableItemChecked(IntPtr handle, bool checked_)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableHandle, out var tableData))
            return;

        LVITEM item = new LVITEM
        {
            mask = LVIF_STATE,
            iItem = itemData.ItemIndex,
            iSubItem = 0,
            state = checked_ ? INDEXTOSTATEIMAGEMASK_CHECKED : INDEXTOSTATEIMAGEMASK_UNCHECKED,
            stateMask = LVIS_STATEIMAGEMASK
        };

        SendMessage(tableData.ListView, LVM_SETITEMSTATE, new IntPtr(itemData.ItemIndex), ref item);
    }

    public bool GetTableItemChecked(IntPtr handle)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            return false;

        if (!_tableData.TryGetValue(itemData.TableHandle, out var tableData))
            return false;

        uint state = (uint)SendMessage(tableData.ListView, LVM_GETITEMSTATE,
            new IntPtr(itemData.ItemIndex), new IntPtr(LVIS_STATEIMAGEMASK)).ToInt32();

        // Check if state image is 2 (checked) - index 1 is unchecked, 2 is checked
        return (state & LVIS_STATEIMAGEMASK) == INDEXTOSTATEIMAGEMASK_CHECKED;
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        // Custom drawing would be required via NM_CUSTOMDRAW
        // Not implemented in this basic version
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        // Custom drawing would be required via NM_CUSTOMDRAW
        // Not implemented in this basic version
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        // Custom drawing would be required via NM_CUSTOMDRAW
        // Not implemented in this basic version
    }
}
