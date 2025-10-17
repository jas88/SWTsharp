using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of Tree widget using TreeView control (SysTreeView32).
/// TreeView control displays hierarchical tree data with support for single/multi selection and checkboxes.
/// </summary>
internal class Win32Tree : IPlatformComposite
{
    private IntPtr _hwnd;
    private readonly int _style;
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // TreeView styles
    private const uint TVS_HASBUTTONS = 0x0001;
    private const uint TVS_HASLINES = 0x0002;
    private const uint TVS_LINESATROOT = 0x0004;
    private const uint TVS_SHOWSELALWAYS = 0x0020;
    private const uint TVS_CHECKBOXES = 0x0100;
    private const uint TVS_FULLROWSELECT = 0x1000;

    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;

    private const string WC_TREEVIEW = "SysTreeView32";

    public Win32Tree(IntPtr parentHandle, int style)
    {
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Tree] Creating tree. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Build TreeView style
        uint treeStyle = TVS_HASBUTTONS | TVS_HASLINES | TVS_LINESATROOT | TVS_SHOWSELALWAYS;

        // Add checkbox style if CHECK is set
        if ((style & SWT.CHECK) != 0)
        {
            treeStyle |= TVS_CHECKBOXES;
        }

        // Add full row select for better usability
        treeStyle |= TVS_FULLROWSELECT;

        // Build window style
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_VSCROLL | WS_HSCROLL;

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        _hwnd = CreateWindowEx(
            0,                              // Extended style
            WC_TREEVIEW,                    // Window class
            "",                             // Window title
            windowStyle | treeStyle,        // Window style
            0, 0,                           // X, Y position
            100, 100,                       // Width, Height
            parentHandle,                   // Parent window
            IntPtr.Zero,                    // Menu
            GetModuleHandle(null),          // Instance
            IntPtr.Zero                     // Additional data
        );

        if (_hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 tree control. Error code: {error}");
        }

        if (enableLogging)
            Console.WriteLine($"[Win32Tree] Tree created successfully. HWND: 0x{_hwnd:X}");
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this tree.
    /// Used internally by platform code for parent-child relationships.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public void AddChild(IPlatformWidget child)
    {
        // Tree items are data, not child widgets - no-op
    }

    public void RemoveChild(IPlatformWidget child)
    {
        // Tree items are data, not child widgets - no-op
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return Array.Empty<IPlatformWidget>();
    }

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
        // TODO: Implement background color via TVM_SETBKCOLOR
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color via TVM_SETTEXTCOLOR
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    #region Win32 P/Invoke

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
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
