using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of a composite container widget.
/// A composite is simply a child window that can contain other widgets.
/// </summary>
internal class Win32Composite : IPlatformComposite
{
    private IntPtr _hwnd;
    private readonly int _style;
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255); // White default
    private RGB _foreground = new RGB(0, 0, 0);       // Black default

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    public Win32Composite(IntPtr parentHandle, int style)
    {
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Composite] Creating composite. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create a child window using the canvas window class
        // Composites are just container windows with WS_CHILD style
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS | WS_CLIPCHILDREN;

        // Add style-specific flags
        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= WS_BORDER;
        }

        _hwnd = CreateWindowEx(
            0,                              // Extended style
            "SWTSharpCanvas",               // Window class (registered in Win32Platform)
            "",                             // Window title
            windowStyle,                    // Window style
            0, 0,                           // X, Y position
            100, 100,                       // Width, Height (initial size, will be set by bounds)
            parentHandle,                   // Parent window
            IntPtr.Zero,                    // Menu
            GetModuleHandle(null),          // Instance
            IntPtr.Zero                     // Additional data
        );

        if (_hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 composite window. Error code: {error}");
        }

        if (enableLogging)
            Console.WriteLine($"[Win32Composite] Composite created successfully. HWND: 0x{_hwnd:X}");
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this composite.
    /// Used internally by platform code for parent-child relationships.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public void AddChild(IPlatformWidget child)
    {
        if (child == null) return;

        lock (_children)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);
            }
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (child == null) return;

        lock (_children)
        {
            _children.Remove(child);
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_hwnd == IntPtr.Zero) return;

        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height,
            SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public void SetVisible(bool visible)
    {
        if (_hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, visible ? SW_SHOW : SW_HIDE);
    }

    public void SetEnabled(bool enabled)
    {
        if (_hwnd == IntPtr.Zero) return;

        EnableWindow(_hwnd, enabled);
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

    public bool GetVisible()
    {
        if (_hwnd == IntPtr.Zero) return false;
        return IsWindowVisible(_hwnd);
    }

    public bool GetEnabled()
    {
        if (_hwnd == IntPtr.Zero) return false;
        return IsWindowEnabled(_hwnd);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via WM_CTLCOLORSTATIC or similar
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

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        lock (_children)
        {
            return _children.ToArray();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Dispose children first
        lock (_children)
        {
            foreach (var child in _children.ToArray())
            {
                if (child is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _children.Clear();
        }

        // Destroy the window
        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    #region Win32 P/Invoke

    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WS_BORDER = 0x00800000;

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
