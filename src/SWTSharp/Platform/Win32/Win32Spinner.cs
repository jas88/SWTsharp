using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of a spinner (up-down) widget.
/// A spinner consists of a buddy edit control paired with an up-down control.
/// </summary>
internal class Win32Spinner : IPlatformSpinner
{
    private IntPtr _upDownHandle;
    private IntPtr _editHandle;
    private readonly int _style;
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255); // White default
    private RGB _foreground = new RGB(0, 0, 0);       // Black default
    private int _minimum;
    private int _maximum = 100;
    private int _value;
    private int _increment = 1;
    private int _digits;

    // Events required by IPlatformValueEvents and IPlatformEventHandling
    #pragma warning disable CS0067
    public event EventHandler<int>? ValueChanged;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32Spinner(IntPtr parentHandle, int style)
    {
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Spinner] Creating spinner. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create edit control for text entry
        uint editStyle = WS_CHILD | WS_VISIBLE | WS_BORDER | ES_NUMBER;

        if ((style & SWT.READ_ONLY) != 0)
            editStyle |= ES_READONLY;

        _editHandle = CreateWindowEx(
            0,
            "EDIT",
            "0",
            editStyle,
            0, 0, 80, 25,
            parentHandle,
            IntPtr.Zero,
            GetModuleHandle(null),
            IntPtr.Zero
        );

        if (_editHandle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 edit control for spinner. Error code: {error}");
        }

        // Create up-down control
        uint upDownStyle = WS_CHILD | WS_VISIBLE | UDS_ALIGNRIGHT | UDS_SETBUDDYINT | UDS_ARROWKEYS;

        if ((style & SWT.WRAP) != 0)
            upDownStyle |= UDS_WRAP;

        _upDownHandle = CreateWindowEx(
            0,
            UPDOWN_CLASS,
            "",
            upDownStyle,
            0, 0, 0, 0,
            parentHandle,
            IntPtr.Zero,
            GetModuleHandle(null),
            IntPtr.Zero
        );

        if (_upDownHandle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            DestroyWindow(_editHandle);
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 up-down control. Error code: {error}");
        }

        // Set edit as buddy control
        SendMessage(_upDownHandle, UDM_SETBUDDY, _editHandle, IntPtr.Zero);

        // Set initial range
        SendMessage(_upDownHandle, UDM_SETRANGE32, new IntPtr(_minimum), new IntPtr(_maximum));

        // Set initial position
        SendMessage(_upDownHandle, UDM_SETPOS32, IntPtr.Zero, new IntPtr(_value));

        if (enableLogging)
            Console.WriteLine($"[Win32Spinner] Spinner created. UpDown: 0x{_upDownHandle:X}, Edit: 0x{_editHandle:X}");
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for the up-down control.
    /// Used internally by platform code.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _upDownHandle;
    }

    public int Value
    {
        get
        {
            if (_upDownHandle == IntPtr.Zero) return _value;

            int pos = SendMessage(_upDownHandle, UDM_GETPOS32, IntPtr.Zero, IntPtr.Zero).ToInt32();
            _value = pos;
            return _value;
        }
        set
        {
            if (_upDownHandle == IntPtr.Zero) return;

            // Clamp value to min/max range
            value = Math.Max(_minimum, Math.Min(_maximum, value));
            _value = value;

            SendMessage(_upDownHandle, UDM_SETPOS32, IntPtr.Zero, new IntPtr(value));
        }
    }

    public int Minimum
    {
        get => _minimum;
        set
        {
            _minimum = value;
            if (_upDownHandle != IntPtr.Zero)
            {
                SendMessage(_upDownHandle, UDM_SETRANGE32, new IntPtr(_minimum), new IntPtr(_maximum));
            }
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            _maximum = value;
            if (_upDownHandle != IntPtr.Zero)
            {
                SendMessage(_upDownHandle, UDM_SETRANGE32, new IntPtr(_minimum), new IntPtr(_maximum));
            }
        }
    }

    public int Increment
    {
        get => _increment;
        set
        {
            _increment = value > 0 ? value : 1;
            // Note: Win32 up-down controls always increment by 1
            // Larger increments would need custom handling
        }
    }

    public int Digits
    {
        get => _digits;
        set
        {
            _digits = value >= 0 ? value : 0;
            // Note: Decimal places require custom formatting in the edit control
            // For now, we store the value but don't implement decimal display
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_editHandle == IntPtr.Zero) return;

        // Position the edit control - the up-down control will auto-position itself
        SetWindowPos(_editHandle, IntPtr.Zero, x, y, width, height,
            SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public Rectangle GetBounds()
    {
        if (_editHandle == IntPtr.Zero) return default;

        RECT rect;
        GetWindowRect(_editHandle, out rect);

        // Convert screen coordinates to parent client coordinates
        var parent = GetParent(_editHandle);
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
        if (_editHandle == IntPtr.Zero || _upDownHandle == IntPtr.Zero) return;

        int cmd = visible ? SW_SHOW : SW_HIDE;
        ShowWindow(_editHandle, cmd);
        ShowWindow(_upDownHandle, cmd);
    }

    public bool GetVisible()
    {
        if (_editHandle == IntPtr.Zero) return false;
        return IsWindowVisible(_editHandle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_editHandle == IntPtr.Zero || _upDownHandle == IntPtr.Zero) return;

        EnableWindow(_editHandle, enabled);
        EnableWindow(_upDownHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_editHandle == IntPtr.Zero) return false;
        return IsWindowEnabled(_editHandle);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via WM_CTLCOLOREDIT
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Destroy both controls
        if (_upDownHandle != IntPtr.Zero)
        {
            DestroyWindow(_upDownHandle);
            _upDownHandle = IntPtr.Zero;
        }

        if (_editHandle != IntPtr.Zero)
        {
            DestroyWindow(_editHandle);
            _editHandle = IntPtr.Zero;
        }
    }

    #region Win32 P/Invoke

    private const string UPDOWN_CLASS = "msctls_updown32";

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;

    // Edit Control Styles
    private const uint ES_NUMBER = 0x2000;
    private const uint ES_READONLY = 0x0800;

    // Up-Down Control Styles
    private const uint UDS_ALIGNRIGHT = 0x0004;
    private const uint UDS_SETBUDDYINT = 0x0002;
    private const uint UDS_ARROWKEYS = 0x0020;
    private const uint UDS_WRAP = 0x0001;

    // Up-Down Control Messages
    private const int UDM_SETRANGE32 = 0x046F; // WM_USER + 111
    private const int UDM_GETRANGE32 = 0x0470; // WM_USER + 112
    private const int UDM_SETPOS32 = 0x0471;   // WM_USER + 113
    private const int UDM_GETPOS32 = 0x0472;   // WM_USER + 114
    private const int UDM_SETBUDDY = 0x0469;   // WM_USER + 105

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
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

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
