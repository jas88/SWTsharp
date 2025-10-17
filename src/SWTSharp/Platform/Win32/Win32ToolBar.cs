using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of IPlatformToolBar using Windows ToolbarWindow32 control.
/// </summary>
internal class Win32ToolBar : IPlatformToolBar
{
    private IntPtr _hwnd;
    private readonly int _style;
    private bool _disposed;
    private readonly List<ToolBarItemData> _items = new();

    private class ToolBarItemData
    {
        public string Text { get; set; } = string.Empty;
        public IntPtr ImageHandle { get; set; }
        public int CommandId { get; set; }
    }

    // Win32 constants
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint CCS_NODIVIDER = 0x0040;
    private const uint CCS_NOPARENTALIGN = 0x0008;
    private const uint CCS_VERT = 0x0080;
    private const uint TBSTYLE_FLAT = 0x0800;
    private const uint TBSTYLE_LIST = 0x1000;

    private const byte TBSTYLE_BUTTON = 0x00;
    private const byte TBSTATE_ENABLED = 0x04;

    private const uint WM_USER = 0x0400;
    private const uint TB_BUTTONSTRUCTSIZE = WM_USER + 30;
    private const uint TB_ADDBUTTONS = WM_USER + 20;
    private const uint TB_DELETEBUTTON = WM_USER + 22;
    private const uint TB_BUTTONCOUNT = WM_USER + 24;
    private const uint TB_AUTOSIZE = WM_USER + 33;

    private int _nextCommandId = 1000;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TBBUTTON
    {
        public int iBitmap;
        public int idCommand;
        public byte fsState;
        public byte fsStyle;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] bReserved;
        public IntPtr dwData;
        public IntPtr iString;
    }

    public Win32ToolBar(IntPtr parentHandle, int style)
    {
        _style = style;

        uint toolBarStyle = WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS |
                           CCS_NODIVIDER | CCS_NOPARENTALIGN;

        // Check for vertical orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            toolBarStyle |= CCS_VERT;
        }

        // Add FLAT style
        if ((style & SWT.FLAT) != 0)
        {
            toolBarStyle |= TBSTYLE_FLAT;
        }

        // Add LIST style for text to right of image
        if ((style & SWT.RIGHT) != 0)
        {
            toolBarStyle |= TBSTYLE_LIST;
        }

        _hwnd = CreateWindowEx(
            0,
            "ToolbarWindow32",
            "",
            toolBarStyle,
            0, 0, 100, 24,
            parentHandle,
            IntPtr.Zero,
            GetModuleHandle(null),
            IntPtr.Zero
        );

        if (_hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 toolbar. Error code: {error}");
        }

        // Initialize the toolbar by sending TB_BUTTONSTRUCTSIZE
        SendMessage(_hwnd, TB_BUTTONSTRUCTSIZE, new IntPtr(Marshal.SizeOf<TBBUTTON>()), IntPtr.Zero);
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this toolbar.
    /// Used internally by platform code.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public void AddItem(string text, IPlatformImage? image)
    {
        if (_hwnd == IntPtr.Zero) return;

        int commandId = _nextCommandId++;
        IntPtr imageHandle = IntPtr.Zero;

        // TODO: Convert IPlatformImage to image handle when image support is added
        // For now, use -1 for no image
        int imageBitmap = -1;

        var tbButton = new TBBUTTON
        {
            iBitmap = imageBitmap,
            idCommand = commandId,
            fsState = TBSTATE_ENABLED,
            fsStyle = TBSTYLE_BUTTON,
            bReserved = new byte[6],
            dwData = IntPtr.Zero,
            iString = string.IsNullOrEmpty(text) ? IntPtr.Zero : Marshal.StringToHGlobalUni(text)
        };

        try
        {
            SendMessage(_hwnd, TB_ADDBUTTONS, new IntPtr(1), ref tbButton);

            _items.Add(new ToolBarItemData
            {
                Text = text ?? string.Empty,
                ImageHandle = imageHandle,
                CommandId = commandId
            });

            // Auto-size the toolbar
            SendMessage(_hwnd, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);
        }
        finally
        {
            if (tbButton.iString != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(tbButton.iString);
            }
        }
    }

    public void RemoveItem(int index)
    {
        if (_hwnd == IntPtr.Zero) return;
        if (index < 0 || index >= _items.Count) return;

        SendMessage(_hwnd, TB_DELETEBUTTON, new IntPtr(index), IntPtr.Zero);
        _items.RemoveAt(index);

        // Auto-size the toolbar
        SendMessage(_hwnd, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);
    }

    public void AttachToWindow(IPlatformWindow window)
    {
        // On Windows, toolbar is created with parent window, so this is a no-op
        // The toolbar is already attached via the parent handle in the constructor
    }

    public int GetItemCount()
    {
        if (_hwnd == IntPtr.Zero) return 0;

        int count = (int)SendMessage(_hwnd, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
        return count;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _items.Clear();

        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    #region P/Invoke

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
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TBBUTTON lParam);

    #endregion
}
