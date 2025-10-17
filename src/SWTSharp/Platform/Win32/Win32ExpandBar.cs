using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of IPlatformExpandBar using custom implementation with disclosure buttons.
/// Creates a vertical container with expand/collapse buttons for each section.
/// </summary>
internal class Win32ExpandBar : IPlatformExpandBar
{
    private readonly IntPtr _hwnd;
    private readonly int _style;
    private readonly Win32Platform _platform;
    private readonly List<Win32ExpandItem> _items = new();
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(240, 240, 240);
    private RGB _foreground = new RGB(0, 0, 0);
    private int _spacing = 4;

    // Events required by IPlatformComposite
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    // Events required by IPlatformExpandEvents
    public event EventHandler<int>? ItemExpanded;
    public event EventHandler<int>? ItemCollapsed;

    public Win32ExpandBar(Win32Platform platform, IntPtr parentHandle, int style)
    {
        _platform = platform;
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32ExpandBar] Creating expand bar. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create a container window for the expand bar
        _hwnd = CreateWindowEx(
            0,
            "STATIC",
            "",
            WS_CHILD | WS_VISIBLE | ((style & SWT.BORDER) != 0 ? WS_BORDER : 0) |
            ((style & SWT.V_SCROLL) != 0 ? WS_VSCROLL : 0),
            0, 0, 200, 300,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (_hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create Win32 ExpandBar");
        }

        if (enableLogging)
            Console.WriteLine($"[Win32ExpandBar] ExpandBar created successfully. HWND: 0x{_hwnd:X}");
    }

    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    #region IPlatformExpandBar Implementation

    public int GetItemCount()
    {
        return _items.Count;
    }

    public IPlatformExpandItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _items[index];
    }

    public void SetSpacing(int spacing)
    {
        _spacing = Math.Max(0, spacing);
        LayoutItems();
    }

    public int GetSpacing()
    {
        return _spacing;
    }

    public IPlatformExpandItem CreateExpandItem(int style, int index)
    {
        var item = new Win32ExpandItem(_platform, _hwnd, style, index, this);

        if (index >= 0 && index < _items.Count)
        {
            _items.Insert(index, item);
        }
        else
        {
            _items.Add(item);
        }

        LayoutItems();
        return item;
    }

    #endregion

    #region Layout Management

    private void LayoutItems()
    {
        int y = 0;
        RECT clientRect;
        GetClientRect(_hwnd, out clientRect);
        int width = clientRect.Right - clientRect.Left;

        foreach (var item in _items)
        {
            int itemHeight = item.GetTotalHeight();
            item.SetBounds(0, y, width, itemHeight);
            y += itemHeight + _spacing;
        }
    }

    internal void OnItemExpandedChanged(Win32ExpandItem item, bool expanded)
    {
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            if (expanded)
            {
                ItemExpanded?.Invoke(this, index);
            }
            else
            {
                ItemCollapsed?.Invoke(this, index);
            }

            LayoutItems();
        }
    }

    #endregion

    #region IPlatformComposite Implementation

    public void AddChild(IPlatformWidget child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
            ChildAdded?.Invoke(this, child);
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (_children.Remove(child))
        {
            ChildRemoved?.Invoke(this, child);
        }
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return _children.AsReadOnly();
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_hwnd == IntPtr.Zero) return;
        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
        LayoutItems();
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
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
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

        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();
        _children.Clear();

        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
        }
    }

    #endregion

    #region Win32 P/Invoke

    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    private const int WS_BORDER = 0x00800000;
    private const int WS_VSCROLL = 0x00200000;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

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

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
        IntPtr hInstance, IntPtr lpParam);

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
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    #endregion
}

/// <summary>
/// Win32 implementation of IPlatformExpandItem using button + container.
/// </summary>
internal class Win32ExpandItem : IPlatformExpandItem
{
    private readonly Win32Platform _platform;
    private readonly IntPtr _parentHandle;
    private readonly Win32ExpandBar _expandBar;
    private readonly IntPtr _buttonHandle;
    private readonly IntPtr _containerHandle;
    private string _text = string.Empty;
    private bool _expanded;
    private int _height = 100;
    private IPlatformWidget? _control;
    private bool _disposed;

    private const int HEADER_HEIGHT = 24;

    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32ExpandItem(Win32Platform platform, IntPtr parentHandle, int style, int index, Win32ExpandBar expandBar)
    {
        _platform = platform;
        _parentHandle = parentHandle;
        _expandBar = expandBar;

        // Create button for header
        _buttonHandle = CreateWindowEx(
            0,
            "BUTTON",
            "",
            WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON | BS_LEFT,
            0, 0, 200, HEADER_HEIGHT,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero
        );

        // Create container for content
        _containerHandle = CreateWindowEx(
            0,
            "STATIC",
            "",
            WS_CHILD | WS_CLIPCHILDREN,
            0, HEADER_HEIGHT, 200, _height,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (_buttonHandle == IntPtr.Zero || _containerHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create Win32 ExpandItem");
        }

        UpdateButtonText();
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_buttonHandle != IntPtr.Zero)
        {
            SetWindowPos(_buttonHandle, IntPtr.Zero, x, y, width, HEADER_HEIGHT, SWP_NOZORDER);
        }

        if (_containerHandle != IntPtr.Zero && _expanded)
        {
            SetWindowPos(_containerHandle, IntPtr.Zero, x, y + HEADER_HEIGHT, width, _height, SWP_NOZORDER);
        }
    }

    public int GetTotalHeight()
    {
        return HEADER_HEIGHT + (_expanded ? _height : 0);
    }

    public void SetText(string text)
    {
        _text = text ?? string.Empty;
        UpdateButtonText();
    }

    public string GetText()
    {
        return _text;
    }

    public void SetExpanded(bool expanded)
    {
        if (_expanded != expanded)
        {
            _expanded = expanded;
            ShowWindow(_containerHandle, _expanded ? SW_SHOW : SW_HIDE);
            UpdateButtonText();
            _expandBar.OnItemExpandedChanged(this, _expanded);
        }
    }

    public bool GetExpanded()
    {
        return _expanded;
    }

    public void SetHeight(int height)
    {
        _height = Math.Max(0, height);
        if (_expanded && _containerHandle != IntPtr.Zero)
        {
            RECT rect;
            GetWindowRect(_containerHandle, out rect);
            SetWindowPos(_containerHandle, IntPtr.Zero, 0, 0, rect.Right - rect.Left, _height,
                SWP_NOMOVE | SWP_NOZORDER);
        }
    }

    public int GetHeight()
    {
        return _height;
    }

    public void SetControl(IPlatformWidget? control)
    {
        _control = control;

        if (_control != null && _containerHandle != IntPtr.Zero)
        {
            // Position control inside container
            RECT rect;
            GetClientRect(_containerHandle, out rect);
            _control.SetBounds(0, 0, rect.Right, rect.Bottom);
            _control.SetVisible(_expanded);
        }
    }

    private void UpdateButtonText()
    {
        if (_buttonHandle != IntPtr.Zero)
        {
            string prefix = _expanded ? "▼ " : "► ";
            SetWindowText(_buttonHandle, prefix + _text);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_containerHandle != IntPtr.Zero)
        {
            DestroyWindow(_containerHandle);
        }

        if (_buttonHandle != IntPtr.Zero)
        {
            DestroyWindow(_buttonHandle);
        }
    }

    #region Win32 P/Invoke

    private const int WS_CHILD = 0x40000000;
    private const int WS_VISIBLE = 0x10000000;
    private const int WS_CLIPCHILDREN = 0x02000000;
    private const int BS_PUSHBUTTON = 0x00000000;
    private const int BS_LEFT = 0x00000100;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOMOVE = 0x0002;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr CreateWindowEx(
        int dwExStyle, string lpClassName, string lpWindowName, int dwStyle,
        int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu,
        IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    #endregion
}
