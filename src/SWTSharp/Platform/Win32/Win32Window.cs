using System.Runtime.InteropServices;
using System.ComponentModel;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows (Win32) implementation of a window platform widget.
/// Encapsulates Win32 window (HWND) and provides IPlatformWindow functionality.
/// </summary>
internal class Win32Window : IPlatformWindow
{
    private IntPtr _hwnd;
    private readonly List<IPlatformWidget> _platformChildren = new();
    private bool _disposed;
    private readonly int _style;

    // Event handling
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
#pragma warning disable CS0067 // Event is never used (events will be implemented in future phase)
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    // Win32 Window Styles
    private const uint WS_OVERLAPPED = 0x00000000;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_MINIMIZE = 0x20000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_DISABLED = 0x08000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WS_MAXIMIZE = 0x01000000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_DLGFRAME = 0x00400000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;
    private const uint WS_SYSMENU = 0x00080000;
    private const uint WS_THICKFRAME = 0x00040000;
    private const uint WS_GROUP = 0x00020000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_MINIMIZEBOX = 0x00020000;
    private const uint WS_MAXIMIZEBOX = 0x00010000;
    private const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

    // Win32 Extended Window Styles
    private const uint WS_EX_DLGMODALFRAME = 0x00000001;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;
    private const uint WS_EX_WINDOWEDGE = 0x00000100;
    private const uint WS_EX_CLIENTEDGE = 0x00000200;
    private const uint WS_EX_CONTEXTHELP = 0x00000400;
    private const uint WS_EX_APPWINDOW = 0x00040000;
    private const uint WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE;

    // Window Messages
    private const uint WM_DESTROY = 0x0002;
    private const uint WM_CLOSE = 0x0010;
    private const uint WM_SHOWWINDOW = 0x0018;

    // ShowWindow Commands
    private const int SW_HIDE = 0;
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOW = 5;
    private const int SW_MINIMIZE = 6;
    private const int SW_MAXIMIZE = 3;
    private const int SW_RESTORE = 9;

    public Win32Window(int style, string title)
    {
        _style = style;

        // Create Win32 window
        _hwnd = CreateWin32Window(style, title);

        if (_hwnd == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to create Win32 window");
        }
    }

    private IntPtr CreateWin32Window(int style, string title)
    {
        // Map SWT style to Win32 window styles
        uint windowStyle = GetWindowStyle(style);
        uint windowExStyle = GetWindowExStyle(style);

        // Get module instance
        IntPtr hInstance = GetModuleHandle(null);

        // Create window
        return CreateWindowEx(
            windowExStyle,
            "SWTSharpWindow", // Must match the class registered in Win32Platform
            title,
            windowStyle,
            100, 100, // x, y
            800, 600, // width, height
            IntPtr.Zero, // parent
            IntPtr.Zero, // menu
            hInstance,
            IntPtr.Zero // lpParam
        );
    }

    private static uint GetWindowStyle(int style)
    {
        uint windowStyle = WS_CLIPCHILDREN | WS_CLIPSIBLINGS;

        // Check SWT style bits
        if ((style & SWT.TITLE) != 0 || (style & SWT.CLOSE) != 0)
            windowStyle |= WS_CAPTION;

        if ((style & SWT.MIN) != 0)
            windowStyle |= WS_MINIMIZEBOX;

        if ((style & SWT.MAX) != 0)
            windowStyle |= WS_MAXIMIZEBOX;

        if ((style & SWT.RESIZE) != 0)
            windowStyle |= WS_THICKFRAME;

        if ((style & SWT.BORDER) != 0)
            windowStyle |= WS_BORDER;

        if ((style & SWT.CLOSE) != 0)
            windowStyle |= WS_SYSMENU;

        // Default to overlapped window if no specific style
        if (windowStyle == (WS_CLIPCHILDREN | WS_CLIPSIBLINGS))
            windowStyle |= WS_OVERLAPPEDWINDOW;

        return windowStyle;
    }

    private static uint GetWindowExStyle(int style)
    {
        uint exStyle = WS_EX_APPWINDOW;

        if ((style & SWT.TOOL) != 0)
            exStyle |= WS_EX_TOOLWINDOW;

        if ((style & SWT.ON_TOP) != 0)
            exStyle |= WS_EX_TOPMOST;

        return exStyle;
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height, 0);

        // Fire LayoutRequested event when bounds change
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return default(Rectangle);

        GetWindowRect(_hwnd, out RECT rect);
        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, visible ? SW_SHOW : SW_HIDE);
        UpdateWindow(_hwnd);
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
        // TODO: Implement background color setting
        // This would require handling WM_PAINT or WM_ERASEBKGND messages
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color getting
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // Windows don't have direct foreground color
        // This would affect child controls
    }

    public RGB GetForeground()
    {
        // Windows don't have direct foreground color
        return new RGB(0, 0, 0); // Default black
    }

    public void SetTitle(string title)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SetWindowText(_hwnd, title);
    }

    public string GetTitle()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return "";

        int length = GetWindowTextLength(_hwnd);
        if (length == 0) return "";

        var buffer = new char[length + 1];
        GetWindowText(_hwnd, buffer, buffer.Length);
        return new string(buffer, 0, length);
    }

    public void Open()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, SW_SHOWNORMAL);
        UpdateWindow(_hwnd);
    }

    public void Close()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SendMessage(_hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    public bool IsDisposed => _disposed;

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this window.
    /// Used internally by platform code for parent-child relationships.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        // For Win32, child widgets are created with the window as parent
        // The relationship is established during widget creation
        _platformChildren.Add(child);

        // Fire ChildAdded event
        ChildAdded?.Invoke(this, child);
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null || _disposed) return;

        _platformChildren.Remove(child);

        // Fire ChildRemoved event
        ChildRemoved?.Invoke(this, child);
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return _platformChildren.AsReadOnly();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Remove all children first
            foreach (var child in _platformChildren.ToArray())
            {
                RemoveChild(child);
            }

            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    // Additional window operations for IPlatformWindow
    public void Maximize()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, SW_MAXIMIZE);
    }

    public void Minimize()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, SW_MINIMIZE);
    }

    public void Restore()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, SW_RESTORE);
    }

    // Win32 API P/Invoke declarations
    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
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

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
