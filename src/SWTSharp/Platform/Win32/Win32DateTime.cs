using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of DateTime widget using DATETIMEPICK_CLASS control.
/// </summary>
#if NET7_0_OR_GREATER
internal partial class Win32DateTime : IPlatformDateTime
#else
internal class Win32DateTime : IPlatformDateTime
#endif
{
    private IntPtr _hwnd;
    private bool _disposed;
    private int _year, _month, _day;
    private int _hours, _minutes, _seconds;

    // Event handling
    public event EventHandler? SelectionChanged;
#pragma warning disable CS0067 // Event is never used
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    // Windows constants
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_TABSTOP = 0x00010000;

    // DateTime Picker Styles
    private const uint DTS_UPDOWN = 0x0001;            // Use up-down control
    private const uint DTS_SHOWNONE = 0x0002;          // Show "none" checkbox
    private const uint DTS_SHORTDATEFORMAT = 0x0000;   // Short date format
    private const uint DTS_LONGDATEFORMAT = 0x0004;    // Long date format
    private const uint DTS_TIMEFORMAT = 0x0009;        // Time format
    private const uint DTS_SHORTDATECENTURYFORMAT = 0x000C; // Short date with century

    // DateTime Picker Messages
    private const uint DTM_GETSYSTEMTIME = 0x1001;
    private const uint DTM_SETSYSTEMTIME = 0x1002;
    private const uint DTM_GETRANGE = 0x1003;
    private const uint DTM_SETRANGE = 0x1004;

    // DateTime Picker Notifications
    private const uint DTN_DATETIMECHANGE = unchecked((uint)-759);

    // GDT (Get DateTime) flags
    private const int GDT_VALID = 0;
    private const int GDT_NONE = 1;

    public Win32DateTime(IntPtr parentHandle, int style)
    {
        _hwnd = CreateDateTimeControl(parentHandle, style);

        if (_hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create Win32 DateTime Picker control");
        }

        // Initialize with current date/time
        var now = System.DateTime.Now;
        _year = now.Year;
        _month = now.Month - 1; // SWT uses 0-based months
        _day = now.Day;
        _hours = now.Hour;
        _minutes = now.Minute;
        _seconds = now.Second;

        SetDate(_year, _month, _day);
        SetTime(_hours, _minutes, _seconds);
    }

    public void SetDate(int year, int month, int day)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _year = year;
        _month = month;
        _day = day;

        SYSTEMTIME st = new SYSTEMTIME
        {
            wYear = (ushort)year,
            wMonth = (ushort)(month + 1), // Win32 uses 1-based months
            wDay = (ushort)day,
            wHour = (ushort)_hours,
            wMinute = (ushort)_minutes,
            wSecond = (ushort)_seconds
        };

        SendMessage(_hwnd, DTM_SETSYSTEMTIME, new IntPtr(GDT_VALID), ref st);
    }

    public (int Year, int Month, int Day) GetDate()
    {
        if (_disposed || _hwnd == IntPtr.Zero)
            return (_year, _month, _day);

        SYSTEMTIME st = new SYSTEMTIME();
        int result = (int)SendMessage(_hwnd, DTM_GETSYSTEMTIME, IntPtr.Zero, ref st);

        if (result == GDT_VALID)
        {
            _year = st.wYear;
            _month = st.wMonth - 1; // Convert to 0-based
            _day = st.wDay;
        }

        return (_year, _month, _day);
    }

    public void SetTime(int hours, int minutes, int seconds)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _hours = hours;
        _minutes = minutes;
        _seconds = seconds;

        SYSTEMTIME st = new SYSTEMTIME
        {
            wYear = (ushort)_year,
            wMonth = (ushort)(_month + 1),
            wDay = (ushort)_day,
            wHour = (ushort)hours,
            wMinute = (ushort)minutes,
            wSecond = (ushort)seconds
        };

        SendMessage(_hwnd, DTM_SETSYSTEMTIME, new IntPtr(GDT_VALID), ref st);
    }

    public (int Hours, int Minutes, int Seconds) GetTime()
    {
        if (_disposed || _hwnd == IntPtr.Zero)
            return (_hours, _minutes, _seconds);

        SYSTEMTIME st = new SYSTEMTIME();
        int result = (int)SendMessage(_hwnd, DTM_GETSYSTEMTIME, IntPtr.Zero, ref st);

        if (result == GDT_VALID)
        {
            _hours = st.wHour;
            _minutes = st.wMinute;
            _seconds = st.wSecond;
        }

        return (_hours, _minutes, _seconds);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height,
            0x0004 | 0x0010); // SWP_NOZORDER | SWP_NOACTIVATE
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return default;

        RECT rect = new RECT();
        GetWindowRect(_hwnd, ref rect);

        return new Rectangle(rect.left, rect.top,
            rect.right - rect.left, rect.bottom - rect.top);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, visible ? 5 : 0); // SW_SHOW = 5, SW_HIDE = 0
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
        // DateTime Picker background is typically controlled by the system
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // DateTime Picker foreground is typically controlled by the system
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this DateTime control.
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

    private IntPtr CreateDateTimeControl(IntPtr parentHandle, int style)
    {
        uint dwStyle = WS_CHILD | WS_VISIBLE | WS_BORDER | WS_TABSTOP;

        // Determine format based on SWT style
        if ((style & SWT.TIME) != 0)
        {
            dwStyle |= DTS_TIMEFORMAT;
        }
        else if ((style & SWT.CALENDAR) != 0)
        {
            // For calendar, we use the MonthCalendar control instead
            return CreateWindowEx(0, "MonthCalendar", "", dwStyle,
                0, 0, 200, 150, parentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
        else // SWT.DATE or default
        {
            if ((style & SWT.LONG) != 0)
            {
                dwStyle |= DTS_LONGDATEFORMAT;
            }
            else if ((style & SWT.MEDIUM) != 0)
            {
                dwStyle |= DTS_SHORTDATECENTURYFORMAT;
            }
            else // SHORT or default
            {
                dwStyle |= DTS_SHORTDATEFORMAT;
            }
        }

        return CreateWindowEx(0, "SysDateTimePick32", "", dwStyle,
            0, 0, 200, 24, parentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    // P/Invoke declarations

#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW",
        StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref SYSTEMTIME lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bEnable);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowEnabled(IntPtr hWnd);
#else
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW",
        CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, ref SYSTEMTIME lParam);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);
#endif

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }
}
