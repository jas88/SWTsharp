using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a progress bar platform widget.
/// Encapsulates Win32 PROGRESS_CLASS control and provides IPlatformProgressBar functionality.
/// </summary>
internal partial class Win32ProgressBar : IPlatformProgressBar
{
    private IntPtr _hwnd;
    private bool _disposed;
    private int _minimum;
    private int _maximum = 100;
    private int _value;
    private int _state = ProgressBarState.NORMAL;

    // Win32 Progress Bar Styles
    private const uint PBS_SMOOTH = 0x01;
    private const uint PBS_VERTICAL = 0x04;
    private const uint PBS_MARQUEE = 0x08;

    // Progress Bar Messages
    private const uint PBM_SETRANGE = 0x0401;
    private const uint PBM_SETPOS = 0x0402;
    private const uint PBM_DELTAPOS = 0x0403;
    private const uint PBM_SETSTEP = 0x0404;
    private const uint PBM_STEPIT = 0x0405;
    private const uint PBM_SETRANGE32 = 0x0406;
    private const uint PBM_GETRANGE = 0x0407;
    private const uint PBM_GETPOS = 0x0408;
    private const uint PBM_SETBARCOLOR = 0x0409;
    private const uint PBM_SETBKCOLOR = 0x2001;
    private const uint PBM_SETMARQUEE = 0x040A;
    private const uint PBM_GETSTEP = 0x040D;
    private const uint PBM_GETBKCOLOR = 0x040E;
    private const uint PBM_GETBARCOLOR = 0x040F;
    private const uint PBM_SETSTATE = 0x0410;
    private const uint PBM_GETSTATE = 0x0411;

    // Progress Bar States (Vista+)
    private const uint PBST_NORMAL = 0x0001;
    private const uint PBST_ERROR = 0x0002;
    private const uint PBST_PAUSED = 0x0003;

    // Window Styles
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;

    // Event handling (IPlatformValueEvents)
    public event EventHandler<int>? ValueChanged;

    public Win32ProgressBar(IntPtr parentHandle, int style)
    {
        _hwnd = CreateProgressBarControl(parentHandle, style);
        if (_hwnd == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create Win32 progress bar. Error code: {error}");
        }

        // Set default range
        SendMessage(_hwnd, PBM_SETRANGE32, new IntPtr(_minimum), new IntPtr(_maximum));
    }

    private IntPtr CreateProgressBarControl(IntPtr parentHandle, int style)
    {
        uint progressStyle = 0;
        uint windowStyle = WS_CHILD | WS_VISIBLE;

        // Apply SWT styles
        if ((style & SWT.SMOOTH) != 0)
        {
            progressStyle |= PBS_SMOOTH;
        }

        if ((style & SWT.VERTICAL) != 0)
        {
            progressStyle |= PBS_VERTICAL;
        }

        if ((style & SWT.INDETERMINATE) != 0)
        {
            progressStyle |= PBS_MARQUEE;
        }

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        // Get module handle
        var hInstance = GetModuleHandle(null);

        // Create the progress bar window
        var hwnd = CreateWindowEx(
            0,
            "msctls_progress32", // PROGRESS_CLASS
            "",
            windowStyle | progressStyle,
            0, 0, 100, 20,
            parentHandle,
            IntPtr.Zero,
            hInstance,
            IntPtr.Zero
        );

        // Enable marquee mode if indeterminate
        if ((style & SWT.INDETERMINATE) != 0 && hwnd != IntPtr.Zero)
        {
            SendMessage(hwnd, PBM_SETMARQUEE, new IntPtr(1), new IntPtr(30)); // 30ms interval
        }

        return hwnd;
    }

    // IPlatformProgressBar implementation

    public int Value
    {
        get
        {
            if (_disposed || _hwnd == IntPtr.Zero) return _value;
            return SendMessage(_hwnd, PBM_GETPOS, IntPtr.Zero, IntPtr.Zero).ToInt32();
        }
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            value = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != value)
            {
                _value = value;
                SendMessage(_hwnd, PBM_SETPOS, new IntPtr(value), IntPtr.Zero);
                ValueChanged?.Invoke(this, value);
            }
        }
    }

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            if (_minimum != value)
            {
                _minimum = value;
                SendMessage(_hwnd, PBM_SETRANGE32, new IntPtr(_minimum), new IntPtr(_maximum));

                // Clamp current value to new range
                if (_value < _minimum)
                {
                    Value = _minimum;
                }
            }
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            if (_maximum != value)
            {
                _maximum = value;
                SendMessage(_hwnd, PBM_SETRANGE32, new IntPtr(_minimum), new IntPtr(_maximum));

                // Clamp current value to new range
                if (_value > _maximum)
                {
                    Value = _maximum;
                }
            }
        }
    }

    public int State
    {
        get => _state;
        set
        {
            if (_disposed || _hwnd == IntPtr.Zero) return;

            if (_state != value)
            {
                _state = value;
                uint pbState = value switch
                {
                    ProgressBarState.ERROR => PBST_ERROR,
                    ProgressBarState.PAUSED => PBST_PAUSED,
                    _ => PBST_NORMAL
                };
                SendMessage(_hwnd, PBM_SETSTATE, new IntPtr(pbState), IntPtr.Zero);
            }
        }
    }

    // IPlatformWidget implementation

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

        // Set background color (works on Windows Vista+)
        uint colorref = ((uint)color.Red) | (((uint)color.Green) << 8) | (((uint)color.Blue) << 16);
        SendMessage(_hwnd, PBM_SETBKCOLOR, IntPtr.Zero, new IntPtr(colorref));
    }

    public RGB GetBackground()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return new RGB(240, 240, 240);

        // Get background color (works on Windows Vista+)
        var colorref = SendMessage(_hwnd, PBM_GETBKCOLOR, IntPtr.Zero, IntPtr.Zero).ToInt32();
        return new RGB(
            (byte)(colorref & 0xFF),
            (byte)((colorref >> 8) & 0xFF),
            (byte)((colorref >> 16) & 0xFF)
        );
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        // Set bar color (works on Windows Vista+)
        uint colorref = ((uint)color.Red) | (((uint)color.Green) << 8) | (((uint)color.Blue) << 16);
        SendMessage(_hwnd, PBM_SETBARCOLOR, IntPtr.Zero, new IntPtr(colorref));
    }

    public RGB GetForeground()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return new RGB(0, 120, 215);

        // Get bar color (works on Windows Vista+)
        var colorref = SendMessage(_hwnd, PBM_GETBARCOLOR, IntPtr.Zero, IntPtr.Zero).ToInt32();
        return new RGB(
            (byte)(colorref & 0xFF),
            (byte)((colorref >> 8) & 0xFF),
            (byte)((colorref >> 16) & 0xFF)
        );
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this progress bar.
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
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            _disposed = true;
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
