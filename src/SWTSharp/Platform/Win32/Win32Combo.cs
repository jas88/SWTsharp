using System.Runtime.InteropServices;
using System.Text;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of IPlatformCombo using ComboBox control.
/// Supports drop-down, drop-down list, and simple combo box styles.
/// </summary>
internal class Win32Combo : IPlatformCombo
{
    private IntPtr _hwnd;
    private readonly int _style;
    private readonly List<string> _items = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255); // White default
    private RGB _foreground = new RGB(0, 0, 0);       // Black default
    private int _selectionIndex = -1;

    // Events required by IPlatformSelectionEvents
    public event EventHandler<int>? SelectionChanged;
    #pragma warning disable CS0067
    public event EventHandler<int>? ItemDoubleClick;

    // Events required by IPlatformEventHandling
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32Combo(IntPtr parentHandle, int style)
    {
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Combo] Creating combo. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Determine combo box style
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_TABSTOP | WS_VSCROLL;
        uint comboStyle = 0;

        // Determine combo type based on SWT style
        bool isSimple = (style & SWT.SIMPLE) != 0;
        bool isReadOnly = (style & SWT.READ_ONLY) != 0;

        if (isSimple)
        {
            comboStyle = CBS_SIMPLE;
        }
        else if (isReadOnly)
        {
            comboStyle = CBS_DROPDOWNLIST;
        }
        else
        {
            comboStyle = CBS_DROPDOWN;
        }

        windowStyle |= comboStyle;

        // Add border if requested
        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        _hwnd = CreateWindowEx(
            0,                              // Extended style
            "COMBOBOX",                     // System ComboBox class
            "",                             // Window title
            windowStyle,                    // Window style
            0, 0,                           // X, Y position
            100, 100,                       // Width, Height (initial size)
            parentHandle,                   // Parent window
            IntPtr.Zero,                    // Menu
            GetModuleHandle(null),          // Instance
            IntPtr.Zero                     // Additional data
        );

        if (_hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 combo box window. Error code: {error}");
        }

        if (enableLogging)
            Console.WriteLine($"[Win32Combo] Combo created successfully. HWND: 0x{_hwnd:X}");
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this combo box.
    /// Used internally by platform code for parent-child relationships.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    #region IPlatformCombo Implementation

    public void AddItem(string item)
    {
        if (_hwnd == IntPtr.Zero || string.IsNullOrEmpty(item)) return;

        _items.Add(item);
        SendMessage(_hwnd, CB_ADDSTRING, IntPtr.Zero, item);
    }

    public void ClearItems()
    {
        if (_hwnd == IntPtr.Zero) return;

        _items.Clear();
        _selectionIndex = -1;
        SendMessage(_hwnd, CB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);
    }

    public int GetItemCount()
    {
        if (_hwnd == IntPtr.Zero) return 0;

        int count = (int)SendMessage(_hwnd, CB_GETCOUNT, IntPtr.Zero, IntPtr.Zero);
        return count;
    }

    public string GetItemAt(int index)
    {
        if (_hwnd == IntPtr.Zero || index < 0 || index >= _items.Count)
            return string.Empty;

        return _items[index];
    }

    public int SelectionIndex
    {
        get
        {
            if (_hwnd == IntPtr.Zero) return -1;

            int index = (int)SendMessage(_hwnd, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
            return index == CB_ERR ? -1 : index;
        }
        set
        {
            if (_hwnd == IntPtr.Zero) return;

            if (value < -1 || value >= _items.Count)
                return;

            var oldIndex = _selectionIndex;
            _selectionIndex = value;

            if (value >= 0)
            {
                SendMessage(_hwnd, CB_SETCURSEL, new IntPtr(value), IntPtr.Zero);
            }
            else
            {
                SendMessage(_hwnd, CB_SETCURSEL, new IntPtr(-1), IntPtr.Zero);
            }

            // Fire SelectionChanged event if index actually changed
            if (oldIndex != _selectionIndex)
            {
                SelectionChanged?.Invoke(this, _selectionIndex);
            }
        }
    }

    public string Text
    {
        get
        {
            if (_hwnd == IntPtr.Zero) return string.Empty;

            int length = (int)SendMessage(_hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
            if (length == 0) return string.Empty;

            var sb = new StringBuilder(length + 1);
            SendMessage(_hwnd, WM_GETTEXT, new IntPtr(sb.Capacity), sb);
            return sb.ToString();
        }
        set
        {
            if (_hwnd == IntPtr.Zero) return;

            SendMessage(_hwnd, WM_SETTEXT, IntPtr.Zero, value ?? string.Empty);

            // Try to find matching item and update selection index
            string textValue = value ?? string.Empty;
            int index = _items.IndexOf(textValue);
            if (index >= 0 && index != _selectionIndex)
            {
                _selectionIndex = index;
                SelectionChanged?.Invoke(this, _selectionIndex);
            }
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_hwnd == IntPtr.Zero) return;

        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height,
            SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public Rectangle GetBounds()
    {
        if (_hwnd == IntPtr.Zero) return default;

        RECT rect;
        GetWindowRect(_hwnd, out rect);

        // Convert screen coordinates to parent client coordinates
        var parent = GetParent(_hwnd);
        if (parent != IntPtr.Zero)
        {
            var topLeft = new POINT { x = rect.Left, y = rect.Top };
            ScreenToClient(parent, ref topLeft);

            return new Rectangle(topLeft.x, topLeft.y, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public void SetVisible(bool visible)
    {
        if (_hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, visible ? SW_SHOW : SW_HIDE);
    }

    public bool GetVisible()
    {
        if (_hwnd == IntPtr.Zero) return false;
        return IsWindowVisible(_hwnd);
    }

    public void SetEnabled(bool enabled)
    {
        if (_hwnd == IntPtr.Zero) return;

        EnableWindow(_hwnd, enabled);
    }

    public bool GetEnabled()
    {
        if (_hwnd == IntPtr.Zero) return false;
        return IsWindowEnabled(_hwnd);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // Note: Background color for combo boxes is more complex on Win32
        // Would require subclassing or owner-draw style
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // Note: Foreground color for combo boxes is more complex on Win32
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Clear items
        _items.Clear();

        // Destroy the window
        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    #endregion

    #region Win32 P/Invoke

    // SWT style constants
    private const int SWT_SIMPLE = 1 << 2;
    private const int SWT_READ_ONLY = 1 << 3;
    private const int SWT_BORDER = 1 << 11;

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_BORDER = 0x00800000;

    // ComboBox Styles
    private const uint CBS_SIMPLE = 0x0001;
    private const uint CBS_DROPDOWN = 0x0002;
    private const uint CBS_DROPDOWNLIST = 0x0003;

    // ComboBox Messages
    private const uint CB_ADDSTRING = 0x0143;
    private const uint CB_DELETESTRING = 0x0144;
    private const uint CB_RESETCONTENT = 0x014B;
    private const uint CB_GETCOUNT = 0x0146;
    private const uint CB_GETCURSEL = 0x0147;
    private const uint CB_SETCURSEL = 0x014E;
    private const uint CB_GETLBTEXT = 0x0148;
    private const uint CB_GETLBTEXTLEN = 0x0149;

    // ComboBox Return Values
    private const int CB_ERR = -1;

    // Window Messages
    private const uint WM_SETTEXT = 0x000C;
    private const uint WM_GETTEXT = 0x000D;
    private const uint WM_GETTEXTLENGTH = 0x000E;

    // SetWindowPos flags
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    // ShowWindow commands
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

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
        public int x;
        public int y;
    }

    #endregion
}
