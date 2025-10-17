using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a scale platform widget.
/// Encapsulates Win32 trackbar control configured for scale display (with tick marks).
/// Similar to Win32Slider but with different visual styling.
/// </summary>
internal partial class Win32Scale : IPlatformScale
{
    private IntPtr _hwnd;
    private bool _disposed;
    private int _minimum;
    private int _maximum = 100;
    private int _value;
    private int _increment = 1;
    private bool _showTicks = true;

    // Static mapping of scale handles to instances for event routing
    private static readonly ConcurrentDictionary<IntPtr, Win32Scale> _scaleInstances = new();

    // Win32 Trackbar Messages
    private const int TBM_GETPOS = 0x0400;          // WM_USER
    private const int TBM_GETRANGEMIN = 0x0401;     // WM_USER + 1
    private const int TBM_GETRANGEMAX = 0x0402;     // WM_USER + 2
    private const int TBM_SETTIC = 0x0404;          // WM_USER + 4
    private const int TBM_SETPOS = 0x0405;          // WM_USER + 5
    private const int TBM_SETRANGE = 0x0406;        // WM_USER + 6
    private const int TBM_SETRANGEMIN = 0x0407;     // WM_USER + 7
    private const int TBM_SETRANGEMAX = 0x0408;     // WM_USER + 8
    private const int TBM_CLEARTICS = 0x0409;       // WM_USER + 9
    private const int TBM_SETSEL = 0x040A;          // WM_USER + 10
    private const int TBM_SETSELSTART = 0x040B;     // WM_USER + 11
    private const int TBM_SETSELEND = 0x040C;       // WM_USER + 12
    private const int TBM_SETTICFREQ = 0x0414;      // WM_USER + 20
    private const int TBM_SETPAGESIZE = 0x0415;     // WM_USER + 21
    private const int TBM_SETLINESIZE = 0x0417;     // WM_USER + 23

    // Trackbar Styles
    private const uint TBS_AUTOTICKS = 0x0001;      // Show tick marks
    private const uint TBS_VERT = 0x0002;           // Vertical orientation
    private const uint TBS_HORZ = 0x0000;           // Horizontal orientation (default)
    private const uint TBS_TOP = 0x0004;            // Ticks on top/left
    private const uint TBS_BOTTOM = 0x0000;         // Ticks on bottom/right (default)
    private const uint TBS_LEFT = 0x0004;           // Same as TBS_TOP for vertical
    private const uint TBS_RIGHT = 0x0000;          // Same as TBS_BOTTOM for vertical
    private const uint TBS_BOTH = 0x0008;           // Ticks on both sides
    private const uint TBS_NOTICKS = 0x0010;        // No tick marks
    private const uint TBS_ENABLESELRANGE = 0x0020; // Enable selection range

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_BORDER = 0x00800000;

    // Event handling
    public event EventHandler<int>? ValueChanged;

    // Suppress unused event warnings - these are part of the interface contract
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32Scale(IntPtr parentHandle, int style)
    {
        _hwnd = CreateScaleControl(parentHandle, style);
        if (_hwnd == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create Win32 scale. Error code: {error}");
        }

        // Register this instance for event routing
        _scaleInstances[_hwnd] = this;

        // Initialize with default values
        UpdateNativeControl();
    }

    private IntPtr CreateScaleControl(IntPtr parentHandle, int style)
    {
        uint trackbarStyle = TBS_AUTOTICKS; // Default: show tick marks
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_TABSTOP;

        // Handle orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            trackbarStyle |= TBS_VERT;
        }

        // Handle border
        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        // Get module handle
        var hInstance = GetModuleHandle(null);

        // Create the trackbar control
        var hwnd = CreateWindowEx(
            0,
            "msctls_trackbar32",    // Standard Windows trackbar class
            "",
            windowStyle | trackbarStyle,
            0, 0, 100, 30,
            parentHandle,
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero
        );

        return hwnd;
    }

    public int Value
    {
        get => GetValue();
        set => SetValue(value);
    }

    public int Minimum
    {
        get => GetMinimum();
        set => SetMinimum(value);
    }

    public int Maximum
    {
        get => GetMaximum();
        set => SetMaximum(value);
    }

    public int Increment
    {
        get => _increment;
        set
        {
            if (value > 0)
            {
                _increment = value;
                UpdateNativeControl();
            }
        }
    }

    public bool ShowTicks
    {
        get => _showTicks;
        set
        {
            if (_showTicks != value)
            {
                _showTicks = value;
                UpdateTickDisplay();
            }
        }
    }

    private void SetValue(int value)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // Clamp value to valid range
        value = Math.Max(_minimum, Math.Min(_maximum, value));

        if (_value != value)
        {
            _value = value;
            SendMessage(_hwnd, TBM_SETPOS, new IntPtr(1), new IntPtr(value));
        }
    }

    private int GetValue()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return _value;

        var result = SendMessage(_hwnd, TBM_GETPOS, IntPtr.Zero, IntPtr.Zero);
        _value = result.ToInt32();
        return _value;
    }

    private void SetMinimum(int minimum)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        if (_minimum != minimum)
        {
            _minimum = minimum;
            SendMessage(_hwnd, TBM_SETRANGEMIN, new IntPtr(1), new IntPtr(minimum));

            // Ensure value is still valid
            if (_value < _minimum)
            {
                SetValue(_minimum);
            }
        }
    }

    private int GetMinimum()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return _minimum;

        var result = SendMessage(_hwnd, TBM_GETRANGEMIN, IntPtr.Zero, IntPtr.Zero);
        _minimum = result.ToInt32();
        return _minimum;
    }

    private void SetMaximum(int maximum)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        if (_maximum != maximum)
        {
            _maximum = maximum;
            SendMessage(_hwnd, TBM_SETRANGEMAX, new IntPtr(1), new IntPtr(maximum));

            // Ensure value is still valid
            if (_value > _maximum)
            {
                SetValue(_maximum);
            }
        }
    }

    private int GetMaximum()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return _maximum;

        var result = SendMessage(_hwnd, TBM_GETRANGEMAX, IntPtr.Zero, IntPtr.Zero);
        _maximum = result.ToInt32();
        return _maximum;
    }

    private void UpdateNativeControl()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // Set range
        SendMessage(_hwnd, TBM_SETRANGE, new IntPtr(1), new IntPtr((_maximum << 16) | (_minimum & 0xFFFF)));

        // Set line size (increment)
        if (_increment > 0)
        {
            SendMessage(_hwnd, TBM_SETLINESIZE, IntPtr.Zero, new IntPtr(_increment));
        }

        // Set position
        SendMessage(_hwnd, TBM_SETPOS, new IntPtr(1), new IntPtr(_value));

        // Update tick display
        UpdateTickDisplay();
    }

    private void UpdateTickDisplay()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        if (_showTicks)
        {
            // Clear existing tick marks
            SendMessage(_hwnd, TBM_CLEARTICS, new IntPtr(1), IntPtr.Zero);

            // Set tick frequency based on increment
            if (_increment > 0)
            {
                SendMessage(_hwnd, TBM_SETTICFREQ, new IntPtr(_increment), IntPtr.Zero);
            }
        }
        else
        {
            // Clear all tick marks
            SendMessage(_hwnd, TBM_CLEARTICS, new IntPtr(1), IntPtr.Zero);
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

        // TODO: Implement background color via WM_CTLCOLORSTATIC or custom drawing
        // Trackbar controls have limited color customization without owner draw
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color retrieval
        return new RGB(240, 240, 240); // Default control background color
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // TODO: Implement foreground color via custom drawing
        // Trackbar controls have limited color customization
    }

    public RGB GetForeground()
    {
        // TODO: Implement foreground color retrieval
        return new RGB(0, 0, 0); // Default black
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this scale.
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
                _scaleInstances.TryRemove(_hwnd, out _);

                // Destroy the window
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Routes scroll messages to the appropriate scale instance.
    /// This should be called from the parent window's message handler.
    /// </summary>
    internal static void RouteScroll(IntPtr hwnd, int scrollCode)
    {
        if (_scaleInstances.TryGetValue(hwnd, out var scale))
        {
            scale.HandleScroll(scrollCode);
        }
    }

    private void HandleScroll(int scrollCode)
    {
        if (_disposed) return;

        // Update value from control
        var newValue = GetValue();

        if (newValue != _value)
        {
            _value = newValue;
            ValueChanged?.Invoke(this, _value);
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
}
