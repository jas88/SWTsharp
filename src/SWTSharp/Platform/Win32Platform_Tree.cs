using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Tree widget methods.
/// </summary>
internal partial class Win32Platform
{
    // TreeView constants
    private const uint TVS_HASBUTTONS = 0x0001;
    private const uint TVS_HASLINES = 0x0002;
    private const uint TVS_LINESATROOT = 0x0004;
    private const uint TVS_CHECKBOXES = 0x0100;
    private const uint TVS_SHOWSELALWAYS = 0x0020;
    private const uint TVS_FULLROWSELECT = 0x1000;

    private const uint TVM_INSERTITEM = 0x1100 + 50;
    private const uint TVM_DELETEITEM = 0x1100 + 1;
    private const uint TVM_EXPAND = 0x1100 + 2;
    private const uint TVM_GETITEM = 0x1100 + 62;
    private const uint TVM_SETITEM = 0x1100 + 63;
    private const uint TVM_GETNEXTITEM = 0x1100 + 10;
    private const uint TVM_SELECTITEM = 0x1100 + 11;
    private const uint TVM_ENSUREVISIBLE = 0x1100 + 20;

    private const uint TVIF_TEXT = 0x0001;
    private const uint TVIF_IMAGE = 0x0002;
    private const uint TVIF_PARAM = 0x0004;
    private const uint TVIF_STATE = 0x0008;
    private const uint TVIF_HANDLE = 0x0010;
    private const uint TVIF_CHILDREN = 0x0040;

    private const uint TVIS_EXPANDED = 0x0020;
    private const uint TVIS_SELECTED = 0x0002;
    private const uint TVIS_STATEIMAGEMASK = 0xF000;

    private const uint TVGN_ROOT = 0x0000;
    private const uint TVGN_NEXT = 0x0001;
    private const uint TVGN_PREVIOUS = 0x0002;
    private const uint TVGN_PARENT = 0x0003;
    private const uint TVGN_CHILD = 0x0004;
    private const uint TVGN_FIRSTVISIBLE = 0x0005;
    private const uint TVGN_NEXTVISIBLE = 0x0006;
    private const uint TVGN_PREVIOUSVISIBLE = 0x0007;
    private const uint TVGN_CARET = 0x0009;

    private const uint TVE_COLLAPSE = 0x0001;
    private const uint TVE_EXPAND = 0x0002;
    private const uint TVE_TOGGLE = 0x0003;

    private static readonly IntPtr TVI_ROOT = new IntPtr(-0x10000);
    private static readonly IntPtr TVI_FIRST = new IntPtr(-0x0FFFF);
    private static readonly IntPtr TVI_LAST = new IntPtr(-0x0FFFE);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TVITEM
    {
        public uint mask;
        public IntPtr hItem;
        public uint state;
        public uint stateMask;
        public IntPtr pszText;
        public int cchTextMax;
        public int iImage;
        public int iSelectedImage;
        public int cChildren;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TVINSERTSTRUCT
    {
        public IntPtr hParent;
        public IntPtr hInsertAfter;
        public TVITEM item;
    }

    // TreeView SendMessage overloads
    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TVINSERTSTRUCT lParam);

    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref TVITEM lParam);

    private class TreeItemData
    {
        public IntPtr TreeHandle;
        public IntPtr ItemHandle;
        public IntPtr ParentItemHandle;
        public string Text = string.Empty;
        public IntPtr ImageHandle;
        public bool Checked;
    }

    private readonly Dictionary<IntPtr, TreeItemData> _treeItems = new();
    private readonly Dictionary<IntPtr, List<IntPtr>> _treeItemChildren = new();

    // Tree operations
    public IntPtr CreateTree(IntPtr parent, int style)
    {
        uint treeStyle = WS_CHILD | WS_VISIBLE | TVS_HASBUTTONS | TVS_HASLINES | TVS_LINESATROOT | TVS_SHOWSELALWAYS;

        // Add checkboxes if CHECK style is set
        if ((style & 0x20) != 0) // SWT.CHECK
        {
            treeStyle |= TVS_CHECKBOXES;
        }

        IntPtr handle = CreateWindowEx(
            WS_EX_CLIENTEDGE,
            "SysTreeView32",
            "",
            treeStyle,
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

        return handle;
    }

    public IntPtr[] GetTreeSelection(IntPtr handle)
    {
        IntPtr selected = SendMessage(handle, TVM_GETNEXTITEM, new IntPtr(TVGN_CARET), IntPtr.Zero);

        if (selected == IntPtr.Zero)
        {
            return Array.Empty<IntPtr>();
        }

        return new[] { selected };
    }

    public void SetTreeSelection(IntPtr handle, IntPtr[] items)
    {
        if (items == null || items.Length == 0)
        {
            SendMessage(handle, TVM_SELECTITEM, new IntPtr(TVGN_CARET), IntPtr.Zero);
            return;
        }

        SendMessage(handle, TVM_SELECTITEM, new IntPtr(TVGN_CARET), items[0]);
    }

    public void ClearTreeItems(IntPtr handle)
    {
        // Get all root items and delete them
        IntPtr rootItem = SendMessage(handle, TVM_GETNEXTITEM, new IntPtr(TVGN_ROOT), IntPtr.Zero);

        while (rootItem != IntPtr.Zero)
        {
            IntPtr nextRoot = SendMessage(handle, TVM_GETNEXTITEM, new IntPtr(TVGN_NEXT), rootItem);
            SendMessage(handle, TVM_DELETEITEM, IntPtr.Zero, rootItem);

            // Clean up our tracking data
            if (_treeItems.ContainsKey(rootItem))
            {
                RemoveTreeItemAndChildren(rootItem);
            }

            rootItem = nextRoot;
        }
    }

    private void RemoveTreeItemAndChildren(IntPtr itemHandle)
    {
        // Remove children first
        if (_treeItemChildren.TryGetValue(itemHandle, out var children))
        {
            foreach (var child in children.ToArray())
            {
                RemoveTreeItemAndChildren(child);
            }
            _treeItemChildren.Remove(itemHandle);
        }

        // Remove the item itself
        _treeItems.Remove(itemHandle);
    }

    public void ShowTreeItem(IntPtr handle, IntPtr item)
    {
        SendMessage(handle, TVM_ENSUREVISIBLE, IntPtr.Zero, item);
    }

    // TreeItem operations
    public IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index)
    {
        var insertStruct = new TVINSERTSTRUCT
        {
            hParent = parentItemHandle == IntPtr.Zero ? TVI_ROOT : parentItemHandle,
            hInsertAfter = index == -1 ? TVI_LAST : TVI_FIRST,
            item = new TVITEM
            {
                mask = TVIF_TEXT | TVIF_PARAM,
                pszText = Marshal.StringToHGlobalUni(""),
                cchTextMax = 0
            }
        };

        IntPtr itemHandle = SendMessage(treeHandle, TVM_INSERTITEM, IntPtr.Zero, ref insertStruct);

        if (itemHandle == IntPtr.Zero)
        {
            Marshal.FreeHGlobal(insertStruct.item.pszText);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        Marshal.FreeHGlobal(insertStruct.item.pszText);

        // Track the tree item
        var itemData = new TreeItemData
        {
            TreeHandle = treeHandle,
            ItemHandle = itemHandle,
            ParentItemHandle = parentItemHandle
        };

        _treeItems[itemHandle] = itemData;

        // Track parent-child relationship
        if (!_treeItemChildren.ContainsKey(parentItemHandle))
        {
            _treeItemChildren[parentItemHandle] = new List<IntPtr>();
        }
        _treeItemChildren[parentItemHandle].Add(itemHandle);

        return itemHandle;
    }

    public void DestroyTreeItem(IntPtr handle)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        SendMessage(itemData.TreeHandle, TVM_DELETEITEM, IntPtr.Zero, handle);
        RemoveTreeItemAndChildren(handle);
    }

    public void SetTreeItemText(IntPtr handle, string text)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Text = text ?? string.Empty;

        IntPtr textPtr = Marshal.StringToHGlobalUni(itemData.Text);

        try
        {
            var item = new TVITEM
            {
                mask = TVIF_TEXT | TVIF_HANDLE,
                hItem = handle,
                pszText = textPtr
            };

            SendMessage(itemData.TreeHandle, TVM_SETITEM, IntPtr.Zero, ref item);
        }
        finally
        {
            Marshal.FreeHGlobal(textPtr);
        }
    }

    public void SetTreeItemImage(IntPtr handle, IntPtr image)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.ImageHandle = image;

        var item = new TVITEM
        {
            mask = TVIF_IMAGE | TVIF_HANDLE,
            hItem = handle,
            iImage = image.ToInt32(),
            iSelectedImage = image.ToInt32()
        };

        SendMessage(itemData.TreeHandle, TVM_SETITEM, IntPtr.Zero, ref item);
    }

    public void SetTreeItemChecked(IntPtr handle, bool checked_)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        itemData.Checked = checked_;

        // Checkbox state is stored in state image index (1-based)
        // Unchecked = 1, Checked = 2
        uint checkState = checked_ ? 2u : 1u;

        var item = new TVITEM
        {
            mask = TVIF_STATE | TVIF_HANDLE,
            hItem = handle,
            state = checkState << 12, // INDEXTOSTATEIMAGEMASK macro
            stateMask = TVIS_STATEIMAGEMASK
        };

        SendMessage(itemData.TreeHandle, TVM_SETITEM, IntPtr.Zero, ref item);
    }

    public bool GetTreeItemChecked(IntPtr handle)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return false;
        }

        return itemData.Checked;
    }

    public void SetTreeItemExpanded(IntPtr handle, bool expanded)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        uint action = expanded ? TVE_EXPAND : TVE_COLLAPSE;
        SendMessage(itemData.TreeHandle, TVM_EXPAND, new IntPtr(action), handle);
    }

    public bool GetTreeItemExpanded(IntPtr handle)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return false;
        }

        var item = new TVITEM
        {
            mask = TVIF_STATE | TVIF_HANDLE,
            hItem = handle,
            stateMask = TVIS_EXPANDED
        };

        SendMessage(itemData.TreeHandle, TVM_GETITEM, IntPtr.Zero, ref item);

        return (item.state & TVIS_EXPANDED) != 0;
    }

    public void ClearTreeItemChildren(IntPtr handle)
    {
        if (!_treeItems.TryGetValue(handle, out var itemData))
        {
            return;
        }

        if (_treeItemChildren.TryGetValue(handle, out var children))
        {
            foreach (var child in children.ToArray())
            {
                DestroyTreeItem(child);
            }
        }
    }

    public void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index)
    {
        // This method is for adding an existing item to a tree
        // In Win32, items are created directly in the tree, so we just update tracking
        if (_treeItems.TryGetValue(itemHandle, out var itemData))
        {
            itemData.ParentItemHandle = parentItemHandle;

            // Update parent-child tracking
            if (!_treeItemChildren.ContainsKey(parentItemHandle))
            {
                _treeItemChildren[parentItemHandle] = new List<IntPtr>();
            }

            if (!_treeItemChildren[parentItemHandle].Contains(itemHandle))
            {
                _treeItemChildren[parentItemHandle].Add(itemHandle);
            }
        }
    }
}
