using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a slider platform widget.
/// Encapsulates Win32 TRACKBAR control and provides IPlatformSlider functionality.
/// </summary>
#if NET7_0_OR_GREATER
internal partial class Win32Slider : IPlatformSlider
#else
internal class Win32Slider : IPlatformSlider
#endif
{
    private IntPtr _hwnd;
    private bool _disposed;
    private int _value;
    private int _minimum;
    private int _maximum = 100;
    private int _increment = 1;
    private int _pageIncrement = 10;

    // Win32 Trackbar Constants
    private const string TRACKBAR_CLASS = "msctls_trackbar32";

    // Trackbar Styles
    private const uint TBS_HORZ = 0x0000;
    private const uint TBS_VERT = 0x0002;
    private const uint TBS_AUTOTICKS = 0x0001;
    private const uint TBS_NOTICKS = 0x0010;
    private const uint TBS_BOTH = 0x0008;
    private const uint TBS_ENABLESELRANGE = 0x0020;
    private const uint TBS_NOTHUMB = 0x0080;
    private const uint TBS_TOOLTIPS = 0x0100;

    // Trackbar Messages
    private const uint TBM_GETPOS = 0x0400;           // WM_USER
    private const uint TBM_GETRANGEMIN = 0x0401;      // WM_USER + 1
    private const uint TBM_GETRANGEMAX = 0x0402;      // WM_USER + 2
    private const uint TBM_SETTIC = 0x0404;           // WM_USER + 4
    private const uint TBM_SETPOS = 0x0405;           // WM_USER + 5
    private const uint TBM_SETRANGE = 0x0406;         // WM_USER + 6
    private const uint TBM_SETRANGEMIN = 0x0407;      // WM_USER + 7
    private const uint TBM_SETRANGEMAX = 0x0408;      // WM_USER + 8
    private const uint TBM_CLEARTICS = 0x0409;        // WM_USER + 9
    private const uint TBM_SETSEL = 0x040A;           // WM_USER + 10
    private const uint TBM_SETSELSTART = 0x040B;      // WM_USER + 11
    private const uint TBM_SETSELEND = 0x040C;        // WM_USER + 12
    private const uint TBM_GETPTICS = 0x040E;         // WM_USER + 14
    private const uint TBM_GETTICPOS = 0x040F;        // WM_USER + 15
    private const uint TBM_GETNUMTICS = 0x0410;       // WM_USER + 16
    private const uint TBM_GETSELSTART = 0x0411;      // WM_USER + 17
    private const uint TBM_GETSELEND = 0x0412;        // WM_USER + 18
    private const uint TBM_CLEARSEL = 0x0413;         // WM_USER + 19
    private const uint TBM_SETTICFREQ = 0x0414;       // WM_USER + 20
    private const uint TBM_SETPAGESIZE = 0x0415;      // WM_USER + 21
    private const uint TBM_GETPAGESIZE = 0x0416;      // WM_USER + 22
    private const uint TBM_SETLINESIZE = 0x0417;      // WM_USER + 23
    private const uint TBM_GETLINESIZE = 0x0418;      // WM_USER + 24
    private const uint TBM_GETTHUMBRECT = 0x0419;     // WM_USER + 25
    private const uint TBM_GETCHANNELRECT = 0x041A;   // WM_USER + 26
    private const uint TBM_SETTHUMBLENGTH = 0x041B;   // WM_USER + 27
    private const uint TBM_GETTHUMBLENGTH = 0x041C;   // WM_USER + 28

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_BORDER = 0x00800000;

    // Window Messages
    private const uint WM_HSCROLL = 0x0114;
    private const uint WM_VSCROLL = 0x0115;

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

    // Static mapping of slider handles to instances for WM_HSCROLL/WM_VSCROLL routing
    private static readonly ConcurrentDictionary<IntPtr, Win32Slider> _sliderInstances = new();

    public Win32Slider(IntPtr parentHandle, int style)
    {
        _hwnd = CreateSliderControl(parentHandle, style);
        if (_hwnd == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create Win32 trackbar. Error code: {error}");
        }

        // Register this instance for event routing
        _sliderInstances[_hwnd] = this;

        // Set default range
        SetRange(_minimum, _maximum);
    }

    private IntPtr CreateSliderControl(IntPtr parentHandle, int style)
    {
        uint trackbarStyle = TBS_AUTOTICKS;
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_TABSTOP;

        // Determine orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            trackbarStyle |= TBS_VERT;
        }
        else
        {
            trackbarStyle |= TBS_HORZ;
        }

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        // Get module handle
        var hInstance = GetModuleHandle(null);

        // Create the trackbar window
        var hwnd = CreateWindowEx(
            0,
            TRACKBAR_CLASS,
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
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return _value;

            var pos = SendMessage(_hwnd, TBM_GETPOS, IntPtr.Zero, IntPtr.Zero);
            _value = pos.ToInt32();
            return _value;
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            value = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != value)
            {
                _value = value;
                SendMessage(_hwnd, TBM_SETPOS, new IntPtr(1), new IntPtr(_value)); // 1 = redraw
            }
        }
    }

    public int Minimum
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return _minimum;
            return _minimum;
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            _minimum = value;
            if (_maximum < _minimum) _maximum = _minimum;
            if (_value < _minimum) _value = _minimum;
            SetRange(_minimum, _maximum);
        }
    }

    public int Maximum
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return _maximum;
            return _maximum;
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            _maximum = value;
            if (_minimum > _maximum) _minimum = _maximum;
            if (_value > _maximum) _value = _maximum;
            SetRange(_minimum, _maximum);
        }
    }

    public int Increment
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return _increment;
            return _increment;
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            _increment = Math.Max(1, value);
            SendMessage(_hwnd, TBM_SETLINESIZE, IntPtr.Zero, new IntPtr(_increment));
        }
    }

    public int PageIncrement
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return _pageIncrement;
            return _pageIncrement;
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            _pageIncrement = Math.Max(1, value);
            SendMessage(_hwnd, TBM_SETPAGESIZE, IntPtr.Zero, new IntPtr(_pageIncrement));
        }
    }

    private void SetRange(int min, int max)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // Set range (LOWORD = min, HIWORD = max)
        int range = (max << 16) | (min & 0xFFFF);
        SendMessage(_hwnd, TBM_SETRANGE, new IntPtr(1), new IntPtr(range)); // 1 = redraw
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
        // Trackbar control background color is typically controlled by the system
        // Would need custom drawing to implement
    }

    public RGB GetBackground()
    {
        return new RGB(240, 240, 240); // Default system color
    }

    public void SetForeground(RGB color)
    {
        // Trackbar control foreground color is typically controlled by the system
        // Would need custom drawing to implement
    }

    public RGB GetForeground()
    {
        return new RGB(0, 120, 215); // Default system accent color
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this slider.
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
                _sliderInstances.TryRemove(_hwnd, out _);

                // Destroy the window
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Routes WM_HSCROLL/WM_VSCROLL messages to the appropriate slider instance.
    /// This should be called from the parent window's message handler.
    /// </summary>
    internal static void RouteScroll(IntPtr hwnd)
    {
        if (_sliderInstances.TryGetValue(hwnd, out var slider))
        {
            slider.HandleScroll();
        }
    }

    private void HandleScroll()
    {
        if (_disposed) return;

        // Read the new position from the control
        var newValue = Value; // This will call the getter which reads from the control

        // Fire ValueChanged event if the value actually changed
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
