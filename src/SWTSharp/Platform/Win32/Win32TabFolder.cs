using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of IPlatformTabFolder using Windows SysTabControl32 control.
/// </summary>
internal class Win32TabFolder : IPlatformTabFolder
{
    private readonly IntPtr _hwnd;
    private readonly int _style;
    private readonly Win32Platform _platform;
    private readonly List<Win32TabItem> _items = new();
    private bool _disposed;
    private RGB _background = new RGB(240, 240, 240);
    private RGB _foreground = new RGB(0, 0, 0);
    private readonly List<IPlatformWidget> _children = new();

    // Events required by IPlatformComposite
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;

    // Events required by IPlatformSelectionEvents
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    #pragma warning restore CS0067

    public Win32TabFolder(Win32Platform platform, IntPtr parentHandle, int style)
    {
        _platform = platform;
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32TabFolder] Creating tab folder. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Use platform's CreateTabFolder method
        _hwnd = _platform.CreateTabFolder(parentHandle, style);

        if (_hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create Win32 TabFolder");
        }

        if (enableLogging)
            Console.WriteLine($"[Win32TabFolder] TabFolder created successfully. HWND: 0x{_hwnd:X}");
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this tab folder.
    /// Used internally by platform code.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    /// <summary>
    /// Helper method to extract native handle from Win32 platform widgets.
    /// </summary>
    private static IntPtr ExtractNativeHandle(IPlatformWidget? widget)
    {
        if (widget == null)
            return IntPtr.Zero;

        // Try casting to known Win32 widget types that have GetNativeHandle()
        if (widget is Win32TabFolder tabFolder)
            return tabFolder.GetNativeHandle();
        if (widget is Win32Combo combo)
            return combo.GetNativeHandle();
        if (widget is Win32TabItem tabItem)
            return tabItem.GetNativeHandle();

        // If we can't extract the handle, return IntPtr.Zero
        // This is a limitation that may need to be addressed when more widget types are added
        return IntPtr.Zero;
    }

    #region IPlatformTabFolder Implementation

    public int GetItemCount()
    {
        return _items.Count;
    }

    public IPlatformTabItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _items[index];
    }

    public int SelectionIndex
    {
        get
        {
            if (_hwnd == IntPtr.Zero) return -1;
            int index = _platform.GetTabSelection(_hwnd);
            return index;
        }
        set
        {
            if (_hwnd == IntPtr.Zero) return;

            int oldIndex = SelectionIndex;
            _platform.SetTabSelection(_hwnd, value);

            if (oldIndex != value)
            {
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    public IPlatformTabItem CreateTabItem(int style, int index)
    {
        // Create the tab item using platform method
        var tabItem = new Win32TabItem(_platform, _hwnd, style, index);
        _items.Insert(index >= 0 && index < _items.Count ? index : _items.Count, tabItem);
        return tabItem;
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
        // Note: Tab control background color is complex on Win32
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

        // Dispose all tab items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        // Clear children list
        _children.Clear();

        // Destroy the window
        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
        }
    }

    #endregion

    #region Win32 P/Invoke

    // SetWindowPos flags
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    // ShowWindow commands
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

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

/// <summary>
/// Win32 implementation of IPlatformTabItem.
/// </summary>
internal class Win32TabItem : IPlatformTabItem
{
    private readonly Win32Platform _platform;
    private readonly IntPtr _tabFolderHandle;
    private readonly IntPtr _tabItemHandle;
    private readonly int _style;
    private bool _disposed;

    // Events required by IPlatformEventHandling
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public Win32TabItem(Win32Platform platform, IntPtr tabFolderHandle, int style, int index)
    {
        _platform = platform;
        _tabFolderHandle = tabFolderHandle;
        _style = style;

        // Create the tab item using platform method
        _tabItemHandle = _platform.CreateTabItem(tabFolderHandle, style, index);

        if (_tabItemHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create Win32 TabItem");
        }
    }

    internal IntPtr GetNativeHandle()
    {
        return _tabItemHandle;
    }

    /// <summary>
    /// Helper method to extract native handle from Win32 platform widgets.
    /// </summary>
    private static IntPtr ExtractNativeHandle(IPlatformWidget? widget)
    {
        if (widget == null)
            return IntPtr.Zero;

        // Try casting to known Win32 widget types that have GetNativeHandle()
        if (widget is Win32TabFolder tabFolder)
            return tabFolder.GetNativeHandle();
        if (widget is Win32Combo combo)
            return combo.GetNativeHandle();
        if (widget is Win32TabItem tabItem)
            return tabItem.GetNativeHandle();

        // If we can't extract the handle, return IntPtr.Zero
        // This is a limitation that may need to be addressed when more widget types are added
        return IntPtr.Zero;
    }

    public void SetText(string text)
    {
        if (_tabItemHandle == IntPtr.Zero) return;
        _platform.SetTabItemText(_tabItemHandle, text);
    }

    public string GetText()
    {
        // Text is managed by the platform layer
        return string.Empty;
    }

    public void SetControl(IPlatformWidget? control)
    {
        if (_tabItemHandle == IntPtr.Zero) return;

        IntPtr controlHandle = ExtractNativeHandle(control);
        _platform.SetTabItemControl(_tabItemHandle, controlHandle);
    }

    public void SetToolTipText(string toolTip)
    {
        if (_tabItemHandle == IntPtr.Zero) return;
        _platform.SetTabItemToolTip(_tabItemHandle, toolTip);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Tab items are automatically cleaned up when tab folder is destroyed
        // No explicit cleanup needed for pseudo-handles
    }
}
