using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a button platform widget.
/// Encapsulates Win32 button control and provides IPlatformTextWidget functionality.
/// </summary>
internal partial class Win32Button : IPlatformTextWidget
{
    private IntPtr _hwnd;
    private bool _disposed;
    private bool _selection;

    // Static mapping of button handles to instances for WM_COMMAND routing
    private static readonly ConcurrentDictionary<IntPtr, Win32Button> _buttonInstances = new();

    // Win32 Button Styles
    private const uint BS_PUSHBUTTON = 0x00000000;
    private const uint BS_DEFPUSHBUTTON = 0x00000001;
    private const uint BS_CHECKBOX = 0x00000002;
    private const uint BS_AUTOCHECKBOX = 0x00000003;
    private const uint BS_RADIOBUTTON = 0x00000004;
    private const uint BS_3STATE = 0x00000005;
    private const uint BS_AUTO3STATE = 0x00000006;
    private const uint BS_GROUPBOX = 0x00000007;
    private const uint BS_AUTORADIOBUTTON = 0x00000009;
    private const uint BS_PUSHBOX = 0x0000000A;
    private const uint BS_OWNERDRAW = 0x0000000B;
    private const uint BS_TYPEMASK = 0x0000000F;
    private const uint BS_LEFTTEXT = 0x00000020;
    private const uint BS_TEXT = 0x00000000;
    private const uint BS_ICON = 0x00000040;
    private const uint BS_BITMAP = 0x00000080;
    private const uint BS_LEFT = 0x00000100;
    private const uint BS_RIGHT = 0x00000200;
    private const uint BS_CENTER = 0x00000300;
    private const uint BS_TOP = 0x00000400;
    private const uint BS_BOTTOM = 0x00000800;
    private const uint BS_VCENTER = 0x00000C00;
    private const uint BS_PUSHLIKE = 0x00001000;
    private const uint BS_MULTILINE = 0x00002000;
    private const uint BS_NOTIFY = 0x00004000;
    private const uint BS_FLAT = 0x00008000;

    // Button Messages
    private const uint BM_GETCHECK = 0x00F0;
    private const uint BM_SETCHECK = 0x00F1;
    private const uint BM_GETSTATE = 0x00F2;
    private const uint BM_SETSTATE = 0x00F3;
    private const uint BM_SETSTYLE = 0x00F4;
    private const uint BM_CLICK = 0x00F5;
    private const uint BM_GETIMAGE = 0x00F6;
    private const uint BM_SETIMAGE = 0x00F7;

    // Button States
    private const uint BST_UNCHECKED = 0x0000;
    private const uint BST_CHECKED = 0x0001;
    private const uint BST_INDETERMINATE = 0x0002;
    private const uint BST_PUSHED = 0x0004;
    private const uint BST_FOCUS = 0x0008;

    // Button Notifications
    private const uint BN_CLICKED = 0;
    private const uint BN_PAINT = 1;
    private const uint BN_HILITE = 2;
    private const uint BN_UNHILITE = 3;
    private const uint BN_DISABLE = 4;
    private const uint BN_DOUBLECLICKED = 5;
    private const uint BN_PUSHED = BN_HILITE;
    private const uint BN_UNPUSHED = BN_UNHILITE;
    private const uint BN_DBLCLK = BN_DOUBLECLICKED;
    private const uint BN_SETFOCUS = 6;
    private const uint BN_KILLFOCUS = 7;

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_BORDER = 0x00800000;

    // Window Messages
    private const uint WM_SETTEXT = 0x000C;
    private const uint WM_GETTEXT = 0x000D;
    private const uint WM_GETTEXTLENGTH = 0x000E;

    // Event handling
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<string>? TextChanged;

    // Suppress unused event warnings - these are part of the interface contract
    #pragma warning disable CS0067
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    public event EventHandler<string>? TextCommitted;
    #pragma warning restore CS0067

    public Win32Button(IntPtr parentHandle, int style)
    {
        _hwnd = CreateButtonControl(parentHandle, style);
        if (_hwnd == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create Win32 button. Error code: {error}");
        }

        // Register this instance for event routing
        _buttonInstances[_hwnd] = this;
    }

    private IntPtr CreateButtonControl(IntPtr parentHandle, int style)
    {
        uint buttonStyle = GetButtonStyle(style);
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_TABSTOP | BS_NOTIFY;

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        if ((style & SWT.FLAT) != 0)
        {
            buttonStyle |= BS_FLAT;
        }

        // Get module handle
        var hInstance = GetModuleHandle(null);

        // Create the button window
        var hwnd = CreateWindowEx(
            0,
            "BUTTON",
            "",
            windowStyle | buttonStyle,
            0, 0, 100, 30,
            parentHandle,
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero
        );

        return hwnd;
    }

    private uint GetButtonStyle(int swtStyle)
    {
        if ((swtStyle & SWT.PUSH) != 0)
        {
            return BS_PUSHBUTTON;
        }
        else if ((swtStyle & SWT.CHECK) != 0)
        {
            return BS_AUTOCHECKBOX;
        }
        else if ((swtStyle & SWT.RADIO) != 0)
        {
            return BS_AUTORADIOBUTTON;
        }
        else if ((swtStyle & SWT.TOGGLE) != 0)
        {
            return BS_PUSHBUTTON | BS_PUSHLIKE;
        }
        else if ((swtStyle & SWT.ARROW) != 0)
        {
            // Arrow buttons require owner draw
            return BS_OWNERDRAW;
        }

        // Default to push button
        return BS_PUSHBUTTON;
    }

    public void SetText(string text)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SendMessage(_hwnd, WM_SETTEXT, IntPtr.Zero, text ?? string.Empty);
        TextChanged?.Invoke(this, text ?? string.Empty);
    }

    public string GetText()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return string.Empty;

        var length = SendMessage(_hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
        if (length == 0) return string.Empty;

        var buffer = new System.Text.StringBuilder(length + 1);
        SendMessage(_hwnd, WM_GETTEXT, new IntPtr(buffer.Capacity), buffer);
        return buffer.ToString();
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

        // TODO: Implement background color via WM_CTLCOLORBTN
        // Would require subclassing or owner draw
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color retrieval
        return new RGB(240, 240, 240); // Default button face color
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

    public bool GetSelection()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return false;

        var state = SendMessage(_hwnd, BM_GETCHECK, IntPtr.Zero, IntPtr.Zero);
        return state.ToInt32() == (int)BST_CHECKED;
    }

    public void SetSelection(bool selected)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _selection = selected;
        SendMessage(_hwnd, BM_SETCHECK, new IntPtr(selected ? BST_CHECKED : BST_UNCHECKED), IntPtr.Zero);
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this button.
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
                _buttonInstances.TryRemove(_hwnd, out _);

                // Destroy the window
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Routes WM_COMMAND messages to the appropriate button instance.
    /// This should be called from the parent window's message handler.
    /// </summary>
    internal static void RouteCommand(IntPtr hwnd, uint notificationCode)
    {
        if (_buttonInstances.TryGetValue(hwnd, out var button))
        {
            button.HandleCommand(notificationCode);
        }
    }

    private void HandleCommand(uint notificationCode)
    {
        if (_disposed) return;

        switch (notificationCode)
        {
            case BN_CLICKED:
                // Update selection state for checkboxes and radio buttons
                _selection = GetSelection();
                Click?.Invoke(this, 0);
                break;

            case BN_SETFOCUS:
                FocusGained?.Invoke(this, 0);
                break;

            case BN_KILLFOCUS:
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

    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, System.Text.StringBuilder lParam);
}
