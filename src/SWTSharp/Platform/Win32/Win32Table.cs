using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a table platform widget using ListView control.
/// Encapsulates Win32 ListView control (SysListView32) and provides IPlatformTable functionality.
/// </summary>
internal partial class Win32Table : IPlatformTable
{
    private IntPtr _hwnd;
    private IntPtr _scrolledWindow;
    private bool _disposed;
    private readonly List<ColumnInfo> _columns = new();
    private readonly List<RowData> _rows = new();
    private bool _headerVisible = true;
    private bool _linesVisible = false;
    private readonly bool _multiSelect;

    // Static mapping of table handles to instances
    private static readonly ConcurrentDictionary<IntPtr, Win32Table> _tableInstances = new();

    // Column and row data structures
    private sealed class ColumnInfo
    {
        public string Text { get; set; } = "";
        public int Width { get; set; } = 100;
        public int Alignment { get; set; } = SWT.LEFT;
    }

    private sealed class RowData
    {
        public Dictionary<int, string> ColumnText { get; set; } = new();
        public Dictionary<int, IPlatformImage?> ColumnImages { get; set; } = new();
    }

    // Win32 constants (from Win32Platform_Table.cs)
    private const string User32 = "user32.dll";
    private const string ComCtl32 = "comctl32.dll";
    private const string WC_LISTVIEW = "SysListView32";

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;

    // ListView Styles
    private const uint LVS_REPORT = 0x0001;
    private const uint LVS_SINGLESEL = 0x0004;
    private const uint LVS_SHOWSELALWAYS = 0x0008;
    private const uint LVS_NOCOLUMNHEADER = 0x4000;

    // ListView Extended Styles
    private const uint LVS_EX_GRIDLINES = 0x00000001;
    private const uint LVS_EX_FULLROWSELECT = 0x00000020;

    // ListView Messages
    private const uint LVM_FIRST = 0x1000;
    private const uint LVM_INSERTCOLUMN = LVM_FIRST + 27;
    private const uint LVM_DELETECOLUMN = LVM_FIRST + 28;
    private const uint LVM_SETCOLUMNWIDTH = LVM_FIRST + 30;
    private const uint LVM_INSERTITEM = LVM_FIRST + 7;
    private const uint LVM_DELETEITEM = LVM_FIRST + 8;
    private const uint LVM_DELETEALLITEMS = LVM_FIRST + 9;
    private const uint LVM_SETITEM = LVM_FIRST + 6;
    private const uint LVM_GETITEMTEXT = LVM_FIRST + 45;
    private const uint LVM_SETITEMSTATE = LVM_FIRST + 43;
    private const uint LVM_GETITEMCOUNT = LVM_FIRST + 4;
    private const uint LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54;
    private const uint LVM_GETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 55;
    private const uint LVM_SETCOLUMN = LVM_FIRST + 26;
    private const uint LVM_GETSELECTEDCOUNT = LVM_FIRST + 50;
    private const uint LVM_GETNEXTITEM = LVM_FIRST + 12;

    // LVCOLUMN flags
    private const uint LVCF_FMT = 0x0001;
    private const uint LVCF_WIDTH = 0x0002;
    private const uint LVCF_TEXT = 0x0004;
    private const uint LVCF_SUBITEM = 0x0008;

    // LVCOLUMN format
    private const uint LVCFMT_LEFT = 0x0000;
    private const uint LVCFMT_RIGHT = 0x0001;
    private const uint LVCFMT_CENTER = 0x0002;

    // LVITEM flags
    private const uint LVIF_TEXT = 0x0001;
    private const uint LVIF_STATE = 0x0008;

    // LVITEM state
    private const uint LVIS_SELECTED = 0x0002;

    // LVNI flags for LVM_GETNEXTITEM
    private const uint LVNI_SELECTED = 0x0002;

    // Structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LVCOLUMN
    {
        public uint mask;
        public uint fmt;
        public int cx;
        public string? pszText;
        public int cchTextMax;
        public int iSubItem;
    }

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
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

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
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32Table(IntPtr parentHandle, int style)
    {
        _multiSelect = (style & SWT.MULTI) != 0;

        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_BORDER | WS_VSCROLL | WS_HSCROLL | LVS_REPORT | LVS_SHOWSELALWAYS;

        if (!_multiSelect)
        {
            windowStyle |= LVS_SINGLESEL;
        }

        _hwnd = CreateWindowEx(
            0,
            WC_LISTVIEW,
            "",
            windowStyle,
            0, 0, 100, 100,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero);

        if (_hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create ListView. Error: {Marshal.GetLastWin32Error()}");
        }

        // Set extended styles
        uint exStyle = LVS_EX_FULLROWSELECT;
        SendMessage(_hwnd, LVM_SETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, new IntPtr(exStyle));

        _scrolledWindow = _hwnd; // For this implementation, same as hwnd
        _tableInstances[_hwnd] = this;
    }

    public IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    // Column Management
    public int AddColumn(string text, int width, int alignment)
    {
        if (_disposed) return -1;

        uint fmt = alignment switch
        {
            SWT.CENTER => LVCFMT_CENTER,
            SWT.RIGHT => LVCFMT_RIGHT,
            _ => LVCFMT_LEFT
        };

        var column = new LVCOLUMN
        {
            mask = LVCF_FMT | LVCF_WIDTH | LVCF_TEXT | LVCF_SUBITEM,
            fmt = fmt,
            cx = width > 0 ? width : 100,
            pszText = text ?? "",
            cchTextMax = (text ?? "").Length,
            iSubItem = _columns.Count
        };

        int result = (int)SendMessage(_hwnd, LVM_INSERTCOLUMN, new IntPtr(_columns.Count), ref column);

        if (result != -1)
        {
            _columns.Add(new ColumnInfo
            {
                Text = text ?? "",
                Width = width > 0 ? width : 100,
                Alignment = alignment
            });
        }

        return result;
    }

    public void RemoveColumn(int columnIndex)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        SendMessage(_hwnd, LVM_DELETECOLUMN, new IntPtr(columnIndex), IntPtr.Zero);
        _columns.RemoveAt(columnIndex);

        // Update column data in rows
        foreach (var row in _rows)
        {
            var newText = new Dictionary<int, string>();
            var newImages = new Dictionary<int, IPlatformImage?>();

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
    }

    public void SetColumnText(int columnIndex, string text)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        _columns[columnIndex].Text = text ?? "";

        var column = new LVCOLUMN
        {
            mask = LVCF_TEXT,
            pszText = text ?? "",
            cchTextMax = (text ?? "").Length
        };

        SendMessage(_hwnd, LVM_SETCOLUMN, new IntPtr(columnIndex), ref column);
    }

    public void SetColumnWidth(int columnIndex, int width)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        _columns[columnIndex].Width = width;
        SendMessage(_hwnd, LVM_SETCOLUMNWIDTH, new IntPtr(columnIndex), new IntPtr(width));
    }

    public void SetColumnAlignment(int columnIndex, int alignment)
    {
        if (_disposed || columnIndex < 0 || columnIndex >= _columns.Count) return;

        _columns[columnIndex].Alignment = alignment;

        uint fmt = alignment switch
        {
            SWT.CENTER => LVCFMT_CENTER,
            SWT.RIGHT => LVCFMT_RIGHT,
            _ => LVCFMT_LEFT
        };

        var column = new LVCOLUMN
        {
            mask = LVCF_FMT,
            fmt = fmt
        };

        SendMessage(_hwnd, LVM_SETCOLUMN, new IntPtr(columnIndex), ref column);
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

        var item = new LVITEM
        {
            mask = LVIF_TEXT,
            iItem = index,
            iSubItem = 0,
            pszText = ""
        };

        int actualIndex = (int)SendMessage(_hwnd, LVM_INSERTITEM, IntPtr.Zero, ref item);

        if (actualIndex != -1)
        {
            var rowData = new RowData();
            if (actualIndex <= _rows.Count)
                _rows.Insert(actualIndex, rowData);
            else
                _rows.Add(rowData);
        }

        return actualIndex;
    }

    public void RemoveItem(int itemIndex)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count) return;

        SendMessage(_hwnd, LVM_DELETEITEM, new IntPtr(itemIndex), IntPtr.Zero);
        _rows.RemoveAt(itemIndex);
    }

    public void RemoveAllItems()
    {
        if (_disposed) return;

        SendMessage(_hwnd, LVM_DELETEALLITEMS, IntPtr.Zero, IntPtr.Zero);
        _rows.Clear();
    }

    public void SetItemText(int itemIndex, int columnIndex, string text)
    {
        if (_disposed || itemIndex < 0 || itemIndex >= _rows.Count || columnIndex < 0) return;

        _rows[itemIndex].ColumnText[columnIndex] = text ?? "";

        var item = new LVITEM
        {
            mask = LVIF_TEXT,
            iItem = itemIndex,
            iSubItem = columnIndex,
            pszText = text ?? "",
            cchTextMax = (text ?? "").Length
        };

        SendMessage(_hwnd, LVM_SETITEM, IntPtr.Zero, ref item);
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

        _rows[itemIndex].ColumnImages[columnIndex] = image;
        // TODO: Implement image list support for ListView
    }

    // Header and Grid Lines
    public void SetHeaderVisible(bool visible)
    {
        if (_disposed) return;

        _headerVisible = visible;
        uint style = GetWindowLong(_hwnd, -16); // GWL_STYLE

        if (visible)
            style &= ~LVS_NOCOLUMNHEADER;
        else
            style |= LVS_NOCOLUMNHEADER;

        SetWindowLong(_hwnd, -16, style);
    }

    public bool GetHeaderVisible()
    {
        return _headerVisible;
    }

    public void SetLinesVisible(bool visible)
    {
        if (_disposed) return;

        _linesVisible = visible;
        uint exStyle = (uint)SendMessage(_hwnd, LVM_GETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (visible)
            exStyle |= LVS_EX_GRIDLINES;
        else
            exStyle &= ~LVS_EX_GRIDLINES;

        SendMessage(_hwnd, LVM_SETEXTENDEDLISTVIEWSTYLE, IntPtr.Zero, new IntPtr(exStyle));
    }

    public bool GetLinesVisible()
    {
        return _linesVisible;
    }

    // Selection
    public void SetSelection(int[] indices)
    {
        if (_disposed) return;

        // Deselect all
        var item = new LVITEM
        {
            stateMask = LVIS_SELECTED
        };
        SendMessage(_hwnd, LVM_SETITEMSTATE, new IntPtr(-1), ref item);

        // Select specified indices
        if (indices != null)
        {
            foreach (int index in indices)
            {
                if (index >= 0 && index < _rows.Count)
                {
                    item = new LVITEM
                    {
                        state = LVIS_SELECTED,
                        stateMask = LVIS_SELECTED
                    };
                    SendMessage(_hwnd, LVM_SETITEMSTATE, new IntPtr(index), ref item);
                }
            }
        }
    }

    public int[] GetSelection()
    {
        if (_disposed) return Array.Empty<int>();

        var selectedIndices = new List<int>();
        int index = -1;

        while (true)
        {
            index = (int)SendMessage(_hwnd, LVM_GETNEXTITEM, new IntPtr(index), new IntPtr(LVNI_SELECTED));
            if (index == -1) break;
            selectedIndices.Add(index);
        }

        return selectedIndices.ToArray();
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

    // IPlatformComposite implementation
    public void AddChild(IPlatformWidget child) { /* Tables don't have child widgets */ }
    public void RemoveChild(IPlatformWidget child) { /* Tables don't have child widgets */ }
    public IReadOnlyList<IPlatformWidget> GetChildren() => Array.Empty<IPlatformWidget>();

    // IPlatformWidget implementation
    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed) return;
        MoveWindow(_hwnd, x, y, width, height, true);
    }

    public Rectangle GetBounds()
    {
        if (_disposed) return default;

        RECT rect;
        GetWindowRect(_hwnd, out rect);
        return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed) return;
        ShowWindow(_hwnd, visible ? 5 : 0); // SW_SHOW : SW_HIDE
    }

    public bool GetVisible()
    {
        if (_disposed) return false;
        return IsWindowVisible(_hwnd);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed) return;
        EnableWindow(_hwnd, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed) return false;
        return IsWindowEnabled(_hwnd);
    }

    public void SetBackground(RGB color)
    {
        // TODO: Implement via custom drawing or subclassing
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255);
    }

    public void SetForeground(RGB color)
    {
        // TODO: Implement via custom drawing
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _tableInstances.TryRemove(_hwnd, out _);

        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }

        _columns.Clear();
        _rows.Clear();
    }

    // Win32 API
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref LVCOLUMN lParam);

    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref LVITEM lParam);

    [DllImport(User32)]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport(User32)]
    private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport(User32)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport(User32)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport(User32)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport(User32)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport(User32)]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport(User32)]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport(User32)]
    private static extern bool DestroyWindow(IntPtr hWnd);
}
