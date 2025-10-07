using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - ToolBar widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Toolbar Styles
    private const uint TBSTYLE_FLAT = 0x0800;
    private const uint TBSTYLE_LIST = 0x1000;
    private const uint TBSTYLE_TRANSPARENT = 0x8000;
    private const uint CCS_NODIVIDER = 0x0040;
    private const uint CCS_NORESIZE = 0x0004;
    private const uint CCS_NOPARENTALIGN = 0x0008;
    private const uint CCS_VERT = 0x0080;

    // Toolbar Button Styles
    private const byte TBSTYLE_BUTTON = 0x00;
    private const byte TBSTYLE_CHECK = 0x02;
    private const byte TBSTYLE_GROUP = 0x04;
    private const byte TBSTYLE_SEP = 0x01;
    private const byte TBSTYLE_DROPDOWN = 0x08;

    // Toolbar Button States
    private const byte TBSTATE_ENABLED = 0x04;
    private const byte TBSTATE_CHECKED = 0x01;
    private const byte TBSTATE_HIDDEN = 0x08;

    // Window Messages
    private const uint WM_USER = 0x0400;

    // Toolbar Messages
    private const uint TB_BUTTONSTRUCTSIZE = WM_USER + 30;
    private const uint TB_ADDBUTTONS = WM_USER + 20;
    private const uint TB_DELETEBUTTON = WM_USER + 22;
    private const uint TB_GETBUTTON = WM_USER + 23;
    private const uint TB_BUTTONCOUNT = WM_USER + 24;
    private const uint TB_SETBUTTONINFO = WM_USER + 66;
    private const uint TB_GETBUTTONINFO = WM_USER + 65;
    private const uint TB_ENABLEBUTTON = WM_USER + 1;
    private const uint TB_CHECKBUTTON = WM_USER + 2;
    private const uint TB_SETBUTTONSIZE = WM_USER + 31;
    private const uint TB_AUTOSIZE = WM_USER + 33;
    private const uint TB_GETITEMRECT = WM_USER + 29;

    // Toolbar Button Info Flags
    private const uint TBIF_IMAGE = 0x00000001;
    private const uint TBIF_TEXT = 0x00000002;
    private const uint TBIF_STATE = 0x00000004;
    private const uint TBIF_STYLE = 0x00000008;
    private const uint TBIF_LPARAM = 0x00000010;
    private const uint TBIF_COMMAND = 0x00000020;
    private const uint TBIF_SIZE = 0x00000040;

    // SWT Style constants (from SWT class)
    private const int SWT_HORIZONTAL = 1 << 8;
    private const int SWT_VERTICAL = 1 << 9;
    private const int SWT_FLAT = 1 << 23;
    private const int SWT_RIGHT = 1 << 17;
    private const int SWT_PUSH = 1 << 3;
    private const int SWT_CHECK = 1 << 5;
    private const int SWT_RADIO = 1 << 4;
    private const int SWT_SEPARATOR = 1 << 1;
    private const int SWT_DROP_DOWN = 1 << 2;

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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TBBUTTONINFO
    {
        public uint cbSize;
        public uint dwMask;
        public int idCommand;
        public int iImage;
        public byte fsState;
        public byte fsStyle;
        public ushort cx;
        public IntPtr lParam;
        public IntPtr pszText;
        public int cchText;
    }

    // RECT struct is defined in Win32Platform_Canvas.cs

    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TBBUTTON lParam);

    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TBBUTTONINFO lParam);

    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref RECT lParam);

    private sealed class ToolBarData
    {
        public IntPtr ToolBar { get; set; }
        public List<IntPtr> ToolItems { get; set; } = new();
        public bool IsVertical { get; set; }
    }

    private sealed class ToolItemData
    {
        public IntPtr ToolBarHandle { get; set; }
        public int CommandId { get; set; }
        public int Style { get; set; }
        public string Text { get; set; } = "";
        public IntPtr ImageHandle { get; set; }
        public string ToolTip { get; set; } = "";
        public int ButtonIndex { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Selected { get; set; }
        public int Width { get; set; }
        public IntPtr ControlHandle { get; set; }
    }

    private readonly Dictionary<IntPtr, ToolBarData> _toolBars = new();
    private readonly Dictionary<IntPtr, ToolItemData> _toolItems = new();
    private readonly Dictionary<int, IntPtr> _toolItemsByCommandId = new();
    private int _nextToolItemId = 1000; // Start at 1000 to avoid conflicts

    public IntPtr CreateToolBar(IntPtr parent, int style)
    {
        uint toolBarStyle = WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS |
                           CCS_NODIVIDER | CCS_NOPARENTALIGN;

        bool isVertical = (style & SWT_VERTICAL) != 0;
        if (isVertical)
        {
            toolBarStyle |= CCS_VERT;
        }

        // Add FLAT style
        if ((style & SWT_FLAT) != 0)
        {
            toolBarStyle |= TBSTYLE_FLAT;
        }

        // Add LIST style for text to right of image
        if ((style & SWT_RIGHT) != 0)
        {
            toolBarStyle |= TBSTYLE_LIST;
        }

        IntPtr handle = CreateWindowEx(
            0,
            "ToolbarWindow32",
            "",
            toolBarStyle,
            0, 0, 100, 24, // Default size
            parent,         // Set parent window
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Initialize the toolbar by sending TB_BUTTONSTRUCTSIZE
        SendMessage(handle, TB_BUTTONSTRUCTSIZE, new IntPtr(Marshal.SizeOf<TBBUTTON>()), IntPtr.Zero);

        // Store toolbar data
        _toolBars[handle] = new ToolBarData
        {
            ToolBar = handle,
            IsVertical = isVertical
        };

        return handle;
    }

    public IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index)
    {
        if (!_toolBars.TryGetValue(toolBarHandle, out var toolBarData))
        {
            throw new ArgumentException("Invalid toolbar handle", nameof(toolBarHandle));
        }

        // Determine button style
        byte buttonStyle = TBSTYLE_BUTTON;
        if ((style & SWT_CHECK) != 0)
        {
            buttonStyle = TBSTYLE_CHECK;
        }
        else if ((style & SWT_RADIO) != 0)
        {
            buttonStyle = (byte)(TBSTYLE_CHECK | TBSTYLE_GROUP);
        }
        else if ((style & SWT_SEPARATOR) != 0)
        {
            buttonStyle = TBSTYLE_SEP;
        }
        else if ((style & SWT_DROP_DOWN) != 0)
        {
            buttonStyle = TBSTYLE_DROPDOWN;
        }

        // Create command ID
        int commandId = id > 0 ? id : _nextToolItemId++;

        // Create pseudo-handle for the tool item
        IntPtr toolItemHandle = new IntPtr(_nextToolItemId++);

        // Determine insert position
        int buttonCount = (int)SendMessage(toolBarHandle, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
        int insertIndex = (index >= 0 && index <= buttonCount) ? index : buttonCount;

        // Create TBBUTTON structure
        var tbButton = new TBBUTTON
        {
            iBitmap = -1, // No image initially
            idCommand = commandId,
            fsState = TBSTATE_ENABLED,
            fsStyle = buttonStyle,
            bReserved = new byte[6],
            dwData = toolItemHandle,
            iString = IntPtr.Zero // No text initially
        };

        // Insert the button
        IntPtr result = SendMessage(toolBarHandle, TB_ADDBUTTONS, new IntPtr(1), ref tbButton);

        if (result == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Store tool item data
        var itemData = new ToolItemData
        {
            ToolBarHandle = toolBarHandle,
            CommandId = commandId,
            Style = style,
            ButtonIndex = insertIndex
        };

        _toolItems[toolItemHandle] = itemData;
        _toolItemsByCommandId[commandId] = toolItemHandle;
        toolBarData.ToolItems.Add(toolItemHandle);

        // Auto-size the toolbar
        SendMessage(toolBarHandle, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);

        return toolItemHandle;
    }

    public void DestroyToolItem(IntPtr handle)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        // Find button index
        int buttonCount = (int)SendMessage(itemData.ToolBarHandle, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);

        for (int i = 0; i < buttonCount; i++)
        {
            var tbButton = new TBBUTTON();
            SendMessage(itemData.ToolBarHandle, TB_GETBUTTON, new IntPtr(i), ref tbButton);

            if (tbButton.idCommand == itemData.CommandId)
            {
                SendMessage(itemData.ToolBarHandle, TB_DELETEBUTTON, new IntPtr(i), IntPtr.Zero);
                break;
            }
        }

        // Remove from tracking dictionaries
        _toolItems.Remove(handle);
        _toolItemsByCommandId.Remove(itemData.CommandId);

        if (_toolBars.TryGetValue(itemData.ToolBarHandle, out var toolBarData))
        {
            toolBarData.ToolItems.Remove(handle);
        }

        // Auto-size the toolbar
        SendMessage(itemData.ToolBarHandle, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetToolItemText(IntPtr handle, string text)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Text = text ?? string.Empty;

        var buttonInfo = new TBBUTTONINFO
        {
            cbSize = (uint)Marshal.SizeOf<TBBUTTONINFO>(),
            dwMask = TBIF_TEXT,
            pszText = Marshal.StringToHGlobalUni(itemData.Text),
            cchText = itemData.Text.Length
        };

        SendMessage(itemData.ToolBarHandle, TB_SETBUTTONINFO,
                   new IntPtr(itemData.CommandId), ref buttonInfo);

        Marshal.FreeHGlobal(buttonInfo.pszText);

        // Auto-size the toolbar
        SendMessage(itemData.ToolBarHandle, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetToolItemImage(IntPtr handle, IntPtr imageHandle)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.ImageHandle = imageHandle;

        // For now, we'll use the image index directly
        // In a full implementation, this would involve image lists
        int imageIndex = imageHandle != IntPtr.Zero ? imageHandle.ToInt32() : -1;

        var buttonInfo = new TBBUTTONINFO
        {
            cbSize = (uint)Marshal.SizeOf<TBBUTTONINFO>(),
            dwMask = TBIF_IMAGE,
            iImage = imageIndex
        };

        SendMessage(itemData.ToolBarHandle, TB_SETBUTTONINFO,
                   new IntPtr(itemData.CommandId), ref buttonInfo);

        // Auto-size the toolbar
        SendMessage(itemData.ToolBarHandle, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetToolItemToolTip(IntPtr handle, string toolTip)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.ToolTip = toolTip ?? string.Empty;

        // Toolbar tooltips are typically handled via TTN_GETDISPINFO notifications
        // For now, store the tooltip text and it can be used in event handlers
        // Full implementation would require handling WM_NOTIFY messages
    }

    public void SetToolItemSelection(IntPtr handle, bool selected)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Selected = selected;

        // Only applies to CHECK and RADIO buttons
        if ((itemData.Style & (SWT_CHECK | SWT_RADIO)) == 0)
        {
            return;
        }

        SendMessage(itemData.ToolBarHandle, TB_CHECKBUTTON,
                   new IntPtr(itemData.CommandId),
                   new IntPtr(selected ? 1 : 0));
    }

    public void SetToolItemEnabled(IntPtr handle, bool enabled)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Enabled = enabled;

        SendMessage(itemData.ToolBarHandle, TB_ENABLEBUTTON,
                   new IntPtr(itemData.CommandId),
                   new IntPtr(enabled ? 1 : 0));
    }

    public void SetToolItemWidth(IntPtr handle, int width)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Width = width;

        // Only applies to SEPARATOR items
        if ((itemData.Style & SWT_SEPARATOR) == 0)
        {
            return;
        }

        var buttonInfo = new TBBUTTONINFO
        {
            cbSize = (uint)Marshal.SizeOf<TBBUTTONINFO>(),
            dwMask = TBIF_SIZE,
            cx = (ushort)width
        };

        SendMessage(itemData.ToolBarHandle, TB_SETBUTTONINFO,
                   new IntPtr(itemData.CommandId), ref buttonInfo);

        // Auto-size the toolbar
        SendMessage(itemData.ToolBarHandle, TB_AUTOSIZE, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetToolItemControl(IntPtr handle, IntPtr controlHandle)
    {
        if (!_toolItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.ControlHandle = controlHandle;

        // Get the button's rectangle
        var rect = new RECT();
        int buttonIndex = GetToolItemButtonIndex(itemData);

        if (buttonIndex >= 0)
        {
            SendMessage(itemData.ToolBarHandle, TB_GETITEMRECT,
                       new IntPtr(buttonIndex), ref rect);

            // Position the control within the button area
            if (controlHandle != IntPtr.Zero)
            {
                SetParent(controlHandle, itemData.ToolBarHandle);
                SetControlBounds(controlHandle,
                               rect.Left, rect.Top,
                               rect.Right - rect.Left,
                               rect.Bottom - rect.Top);
                SetControlVisible(controlHandle, true);
            }
        }
    }

    private int GetToolItemButtonIndex(ToolItemData itemData)
    {
        int buttonCount = (int)SendMessage(itemData.ToolBarHandle, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);

        for (int i = 0; i < buttonCount; i++)
        {
            var tbButton = new TBBUTTON();
            SendMessage(itemData.ToolBarHandle, TB_GETBUTTON, new IntPtr(i), ref tbButton);

            if (tbButton.idCommand == itemData.CommandId)
            {
                return i;
            }
        }

        return -1;
    }

    [DllImport(User32)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
}
