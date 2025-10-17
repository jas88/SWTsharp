using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a list platform widget.
/// Encapsulates Win32 ListBox control and provides IPlatformList functionality.
/// </summary>
internal partial class Win32List : IPlatformList
{
    private IntPtr _hwnd;
    private bool _disposed;
    private readonly bool _multiSelect;
    private readonly List<string> _items = new();

    // Static mapping of list handles to instances for WM_COMMAND routing
    private static readonly ConcurrentDictionary<IntPtr, Win32List> _listInstances = new();

    // Win32 ListBox Styles
    private const uint LBS_NOTIFY = 0x0001;
    private const uint LBS_SORT = 0x0002;
    private const uint LBS_NOREDRAW = 0x0004;
    private const uint LBS_MULTIPLESEL = 0x0008;
    private const uint LBS_OWNERDRAWFIXED = 0x0010;
    private const uint LBS_OWNERDRAWVARIABLE = 0x0020;
    private const uint LBS_HASSTRINGS = 0x0040;
    private const uint LBS_USETABSTOPS = 0x0080;
    private const uint LBS_NOINTEGRALHEIGHT = 0x0100;
    private const uint LBS_MULTICOLUMN = 0x0200;
    private const uint LBS_WANTKEYBOARDINPUT = 0x0400;
    private const uint LBS_EXTENDEDSEL = 0x0800;
    private const uint LBS_DISABLENOSCROLL = 0x1000;
    private const uint LBS_NODATA = 0x2000;
    private const uint LBS_NOSEL = 0x4000;
    private const uint LBS_COMBOBOX = 0x8000;
    private const uint LBS_STANDARD = LBS_NOTIFY | LBS_SORT | WS_VSCROLL | WS_BORDER;

    // ListBox Messages
    private const uint LB_ADDSTRING = 0x0180;
    private const uint LB_INSERTSTRING = 0x0181;
    private const uint LB_DELETESTRING = 0x0182;
    private const uint LB_SELITEMRANGEEX = 0x0183;
    private const uint LB_RESETCONTENT = 0x0184;
    private const uint LB_SETSEL = 0x0185;
    private const uint LB_SETCURSEL = 0x0186;
    private const uint LB_GETSEL = 0x0187;
    private const uint LB_GETCURSEL = 0x0188;
    private const uint LB_GETTEXT = 0x0189;
    private const uint LB_GETTEXTLEN = 0x018A;
    private const uint LB_GETCOUNT = 0x018B;
    private const uint LB_SELECTSTRING = 0x018C;
    private const uint LB_DIR = 0x018D;
    private const uint LB_GETTOPINDEX = 0x018E;
    private const uint LB_FINDSTRING = 0x018F;
    private const uint LB_GETSELCOUNT = 0x0190;
    private const uint LB_GETSELITEMS = 0x0191;
    private const uint LB_SETTABSTOPS = 0x0192;
    private const uint LB_GETHORIZONTALEXTENT = 0x0193;
    private const uint LB_SETHORIZONTALEXTENT = 0x0194;
    private const uint LB_SETCOLUMNWIDTH = 0x0195;
    private const uint LB_ADDFILE = 0x0196;
    private const uint LB_SETTOPINDEX = 0x0197;
    private const uint LB_GETITEMRECT = 0x0198;
    private const uint LB_GETITEMDATA = 0x0199;
    private const uint LB_SETITEMDATA = 0x019A;
    private const uint LB_SELITEMRANGE = 0x019B;
    private const uint LB_SETANCHORINDEX = 0x019C;
    private const uint LB_GETANCHORINDEX = 0x019D;
    private const uint LB_SETCARETINDEX = 0x019E;
    private const uint LB_GETCARETINDEX = 0x019F;
    private const uint LB_SETITEMHEIGHT = 0x01A0;
    private const uint LB_GETITEMHEIGHT = 0x01A1;
    private const uint LB_FINDSTRINGEXACT = 0x01A2;
    private const uint LB_SETLOCALE = 0x01A5;
    private const uint LB_GETLOCALE = 0x01A6;
    private const uint LB_SETCOUNT = 0x01A7;
    private const uint LB_INITSTORAGE = 0x01A8;
    private const uint LB_ITEMFROMPOINT = 0x01A9;

    // ListBox Return Values
    private const int LB_OKAY = 0;
    private const int LB_ERR = -1;
    private const int LB_ERRSPACE = -2;

    // ListBox Notifications
    private const uint LBN_ERRSPACE = unchecked((uint)-2);
    private const uint LBN_SELCHANGE = 1;
    private const uint LBN_DBLCLK = 2;
    private const uint LBN_SELCANCEL = 3;
    private const uint LBN_SETFOCUS = 4;
    private const uint LBN_KILLFOCUS = 5;

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;

    // Extended Window Styles
    private const uint WS_EX_CLIENTEDGE = 0x00000200;

    // Event handling
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;

    // Suppress unused event warnings - these are part of the interface contract
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32List(IntPtr parentHandle, int style)
    {
        _multiSelect = (style & SWT.MULTI) != 0;
        _hwnd = CreateListControl(parentHandle, style);
        if (_hwnd == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create Win32 list. Error code: {error}");
        }

        // Register this instance for event routing
        _listInstances[_hwnd] = this;
    }

    private IntPtr CreateListControl(IntPtr parentHandle, int style)
    {
        uint listStyle = LBS_NOTIFY | LBS_HASSTRINGS | LBS_NOINTEGRALHEIGHT;
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_VSCROLL;
        uint exStyle = 0;

        // Apply multi-selection style
        if (_multiSelect)
        {
            listStyle |= LBS_EXTENDEDSEL;
        }

        // Apply border style
        if ((style & SWT.BORDER) != 0)
        {
            exStyle |= WS_EX_CLIENTEDGE;
        }

        // Apply horizontal scroll
        if ((style & SWT.H_SCROLL) != 0)
        {
            windowStyle |= WS_HSCROLL;
        }

        // Get module handle
        var hInstance = GetModuleHandle(null);

        // Create the listbox window
        var hwnd = CreateWindowEx(
            exStyle,
            "LISTBOX",
            "",
            windowStyle | listStyle,
            0, 0, 100, 100,
            parentHandle,
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero
        );

        return hwnd;
    }

    public void AddItem(string item)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;
        if (item == null) return;

        _items.Add(item);
        SendMessage(_hwnd, LB_ADDSTRING, IntPtr.Zero, item);
    }

    public void ClearItems()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _items.Clear();
        SendMessage(_hwnd, LB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);
    }

    public int GetItemCount()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return 0;

        var count = SendMessage(_hwnd, LB_GETCOUNT, IntPtr.Zero, IntPtr.Zero);
        return count.ToInt32();
    }

    public string GetItemAt(int index)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return string.Empty;
        if (index < 0 || index >= _items.Count) return string.Empty;

        return _items[index];
    }

    public int[] SelectionIndices
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return Array.Empty<int>();

            if (_multiSelect)
            {
                var count = SendMessage(_hwnd, LB_GETSELCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();
                if (count <= 0) return Array.Empty<int>();

                var indices = new int[count];
                SendMessage(_hwnd, LB_GETSELITEMS, new IntPtr(count), indices);
                return indices;
            }
            else
            {
                var index = SendMessage(_hwnd, LB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
                if (index < 0) return Array.Empty<int>();
                return new[] { index };
            }
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;
            if (value == null) return;

            if (_multiSelect)
            {
                // Clear all selections first
                SendMessage(_hwnd, LB_SETSEL, IntPtr.Zero, new IntPtr(-1));

                // Set each selected index
                foreach (var index in value)
                {
                    if (index >= 0 && index < GetItemCount())
                    {
                        SendMessage(_hwnd, LB_SETSEL, new IntPtr(1), new IntPtr(index));
                    }
                }
            }
            else
            {
                // Single selection mode - only use first index
                if (value.Length > 0)
                {
                    var index = value[0];
                    if (index >= 0 && index < GetItemCount())
                    {
                        SendMessage(_hwnd, LB_SETCURSEL, new IntPtr(index), IntPtr.Zero);
                    }
                }
                else
                {
                    // Clear selection
                    SendMessage(_hwnd, LB_SETCURSEL, new IntPtr(-1), IntPtr.Zero);
                }
            }
        }
    }

    public int SelectionIndex
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return -1;

            var index = SendMessage(_hwnd, LB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
            return index;
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            if (value < 0)
            {
                // Clear selection
                if (_multiSelect)
                {
                    SendMessage(_hwnd, LB_SETSEL, IntPtr.Zero, new IntPtr(-1));
                }
                else
                {
                    SendMessage(_hwnd, LB_SETCURSEL, new IntPtr(-1), IntPtr.Zero);
                }
            }
            else if (value < GetItemCount())
            {
                if (_multiSelect)
                {
                    // Clear all and select one
                    SendMessage(_hwnd, LB_SETSEL, IntPtr.Zero, new IntPtr(-1));
                    SendMessage(_hwnd, LB_SETSEL, new IntPtr(1), new IntPtr(value));
                }
                else
                {
                    SendMessage(_hwnd, LB_SETCURSEL, new IntPtr(value), IntPtr.Zero);
                }
            }
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        MoveWindow(_hwnd, x, y, width, height, true);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return default;

        RECT rect;
        GetWindowRect(_hwnd, out rect);

        // Convert screen coordinates to parent client coordinates
        var parent = GetParent(_hwnd);
        if (parent != IntPtr.Zero)
        {
            var topLeft = new POINT { X = rect.Left, Y = rect.Top };
            ScreenToClient(parent, ref topLeft);
            return new Rectangle(topLeft.X, topLeft.Y, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, visible ? SW_SHOW : SW_HIDE);
    }

    public bool GetVisible()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return false;

        return IsWindowVisible(_hwnd);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        EnableWindow(_hwnd, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return false;

        return IsWindowEnabled(_hwnd);
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // TODO: Implement background color via WM_CTLCOLORLISTBOX
        // Would require subclassing or owner draw
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color retrieval
        return new RGB(255, 255, 255); // Default white background
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // TODO: Implement foreground color via owner draw
    }

    public RGB GetForeground()
    {
        // TODO: Implement foreground color retrieval
        return new RGB(0, 0, 0); // Default black text
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this list.
    /// Used internally by platform code for parent-child relationships.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_hwnd != IntPtr.Zero)
            {
                // Remove from instance mapping
                _listInstances.TryRemove(_hwnd, out _);

                // Destroy the window
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            _items.Clear();
            _disposed = true;
        }
    }

    /// <summary>
    /// Routes WM_COMMAND messages to the appropriate list instance.
    /// This should be called from the parent window's message handler.
    /// </summary>
    internal static void RouteCommand(IntPtr hwnd, uint notificationCode)
    {
        if (_listInstances.TryGetValue(hwnd, out var list))
        {
            list.HandleCommand(notificationCode);
        }
    }

    private void HandleCommand(uint notificationCode)
    {
        if (_disposed) return;

        switch (notificationCode)
        {
            case LBN_SELCHANGE:
                var index = SelectionIndex;
                SelectionChanged?.Invoke(this, index);
                break;

            case LBN_DBLCLK:
                var dblClickIndex = SelectionIndex;
                ItemDoubleClick?.Invoke(this, dblClickIndex);
                break;

            case LBN_SETFOCUS:
                FocusGained?.Invoke(this, 0);
                break;

            case LBN_KILLFOCUS:
                FocusLost?.Invoke(this, 0);
                break;
        }
    }

    // Win32 API declarations
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";

    private const int SW_SHOW = 5;
    private const int SW_HIDE = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y,
        int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);
#else
    [DllImport(User32, EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y,
        int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(IntPtr hWnd);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, [MarshalAs(UnmanagedType.Bool)] bool repaint);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool enable);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool EnableWindow(IntPtr hWnd, bool enable);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowEnabled(IntPtr hWnd);
#else
    [DllImport(User32)]
    private static extern bool IsWindowEnabled(IntPtr hWnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);
#else
    [DllImport(User32)]
    private static extern bool IsWindowVisible(IntPtr hWnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr GetParent(IntPtr hWnd);
#else
    [DllImport(User32)]
    private static extern IntPtr GetParent(IntPtr hWnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Kernel32, EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr GetModuleHandle(string? lpModuleName);
#else
    [DllImport(Kernel32, EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
#else
    [DllImport(User32, EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
#else
    [DllImport(User32, EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, int[] lParam);
#else
    [DllImport(User32, EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, int[] lParam);
#endif
}
