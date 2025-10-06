using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - TabFolder widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Tab Control Styles
    private const uint TCS_TABS = 0x0000;
    private const uint TCS_BOTTOM = 0x0002;
    private const uint TCS_MULTILINE = 0x0200;
    private const uint TCS_TOOLTIPS = 0x4000;

    // Tab Control Messages
    private const uint TCM_FIRST = 0x1300;
    private const uint TCM_INSERTITEM = TCM_FIRST + 7;
    private const uint TCM_DELETEITEM = TCM_FIRST + 8;
    private const uint TCM_SETITEM = TCM_FIRST + 6;
    private const uint TCM_GETCURSEL = TCM_FIRST + 11;
    private const uint TCM_SETCURSEL = TCM_FIRST + 12;
    private const uint TCM_GETITEMCOUNT = TCM_FIRST + 4;

    // Tab Control Item Flags
    private const uint TCIF_TEXT = 0x0001;
    private const uint TCIF_IMAGE = 0x0002;
    private const uint TCIF_PARAM = 0x0008;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TCITEM
    {
        public uint mask;
        public uint dwState;
        public uint dwStateMask;
        public IntPtr pszText;
        public int cchTextMax;
        public int iImage;
        public IntPtr lParam;
    }

    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TCITEM lParam);

    private class TabItemData
    {
        public IntPtr TabFolderHandle;
        public int TabIndex;
        public string Text = string.Empty;
        public IntPtr ControlHandle;
    }

    private readonly Dictionary<IntPtr, TabItemData> _tabItems = new();
    private readonly Dictionary<IntPtr, List<IntPtr>> _tabFolderItems = new();
    private int _nextTabItemId = 1;

    public IntPtr CreateTabFolder(IntPtr parent, int style)
    {
        uint tabStyle = WS_CHILD | WS_VISIBLE | TCS_TABS | TCS_TOOLTIPS;

        // Check for BOTTOM style (SWT.BOTTOM = 0x400)
        if ((style & 0x400) != 0)
        {
            tabStyle |= TCS_BOTTOM;
        }

        // Add multiline support if needed
        tabStyle |= TCS_MULTILINE;

        IntPtr handle = CreateWindowEx(
            0,
            "SysTabControl32",
            "",
            tabStyle,
            0, 0, 100, 100,
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Initialize tab items list for this tab folder
        _tabFolderItems[handle] = new List<IntPtr>();

        return handle;
    }

    public void SetTabSelection(IntPtr handle, int index)
    {
        if (index < 0)
        {
            // No selection
            return;
        }

        int currentIndex = (int)SendMessage(handle, TCM_GETCURSEL, IntPtr.Zero, IntPtr.Zero);

        // Hide control of previously selected tab
        if (currentIndex >= 0 && _tabFolderItems.TryGetValue(handle, out var items))
        {
            foreach (var itemHandle in items)
            {
                if (_tabItems.TryGetValue(itemHandle, out var itemData) &&
                    itemData.TabIndex == currentIndex &&
                    itemData.ControlHandle != IntPtr.Zero)
                {
                    SetControlVisible(itemData.ControlHandle, false);
                    break;
                }
            }
        }

        // Set new selection
        SendMessage(handle, TCM_SETCURSEL, new IntPtr(index), IntPtr.Zero);

        // Show control of newly selected tab
        if (_tabFolderItems.TryGetValue(handle, out items))
        {
            foreach (var itemHandle in items)
            {
                if (_tabItems.TryGetValue(itemHandle, out var itemData) &&
                    itemData.TabIndex == index &&
                    itemData.ControlHandle != IntPtr.Zero)
                {
                    SetControlVisible(itemData.ControlHandle, true);
                    break;
                }
            }
        }
    }

    public int GetTabSelection(IntPtr handle)
    {
        return (int)SendMessage(handle, TCM_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
    }

    // TabItem operations
    public IntPtr CreateTabItem(IntPtr tabFolderHandle, int style, int index)
    {
        // Get current tab count
        int tabCount = (int)SendMessage(tabFolderHandle, TCM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);

        // Determine insert position
        int insertIndex = index;
        if (index < 0 || index > tabCount)
        {
            insertIndex = tabCount; // Append
        }

        // Create TCITEM structure
        var tcItem = new TCITEM
        {
            mask = TCIF_TEXT,
            pszText = Marshal.StringToHGlobalUni(""),
            cchTextMax = 0
        };

        // Insert the tab
        IntPtr result = SendMessage(tabFolderHandle, TCM_INSERTITEM, new IntPtr(insertIndex), ref tcItem);
        Marshal.FreeHGlobal(tcItem.pszText);

        if (result == new IntPtr(-1))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Create a pseudo-handle for the tab item (encode tab folder handle and index)
        IntPtr tabItemHandle = new IntPtr(_nextTabItemId++);

        // Track the tab item
        var itemData = new TabItemData
        {
            TabFolderHandle = tabFolderHandle,
            TabIndex = insertIndex,
            Text = string.Empty,
            ControlHandle = IntPtr.Zero
        };

        _tabItems[tabItemHandle] = itemData;

        // Add to tab folder's item list
        if (!_tabFolderItems.ContainsKey(tabFolderHandle))
        {
            _tabFolderItems[tabFolderHandle] = new List<IntPtr>();
        }
        _tabFolderItems[tabFolderHandle].Add(tabItemHandle);

        return tabItemHandle;
    }

    public void SetTabItemText(IntPtr handle, string text)
    {
        if (!_tabItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Text = text ?? string.Empty;

        var tcItem = new TCITEM
        {
            mask = TCIF_TEXT,
            pszText = Marshal.StringToHGlobalUni(itemData.Text),
            cchTextMax = itemData.Text.Length
        };

        SendMessage(itemData.TabFolderHandle, TCM_SETITEM, new IntPtr(itemData.TabIndex), ref tcItem);
        Marshal.FreeHGlobal(tcItem.pszText);
    }

    public void SetTabItemControl(IntPtr handle, IntPtr controlHandle)
    {
        if (!_tabItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.ControlHandle = controlHandle;

        // Show or hide the control based on whether this tab is selected
        if (controlHandle != IntPtr.Zero)
        {
            int currentSelection = GetTabSelection(itemData.TabFolderHandle);
            bool isSelected = currentSelection == itemData.TabIndex;
            SetControlVisible(controlHandle, isSelected);
        }
    }

    public void SetTabItemToolTip(IntPtr handle, string toolTip)
    {
        // Tab control tooltips are typically handled automatically by the TCS_TOOLTIPS style
        // For now, this is a no-op as Win32 tab controls manage tooltips internally
        // If custom tooltip support is needed, we would need to handle TTN_GETDISPINFO notifications
    }
}
