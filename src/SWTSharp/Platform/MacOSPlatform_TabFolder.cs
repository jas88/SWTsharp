using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - TabFolder widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    private readonly Dictionary<IntPtr, TabFolderData> _tabFolderData = new();
    private readonly Dictionary<IntPtr, TabItemData> _tabItemData = new();
    private readonly Dictionary<IntPtr, ToolBarData> _toolBarData = new();
    private readonly Dictionary<IntPtr, ToolItemData> _toolItemData = new();

    private IntPtr _nsTabViewClass;
    private IntPtr _nsTabViewItemClass;
    private IntPtr _selAddTabViewItem;
    private IntPtr _selRemoveTabViewItem;
    private IntPtr _selSelectTabViewItemAtIndex;
    private IntPtr _selIndexOfTabViewItem;
    private IntPtr _selNumberOfTabViewItems;
    private IntPtr _selTabViewItemAtIndex;
    private IntPtr _selSelectedTabViewItem;
    private IntPtr _selSetLabel;
    private IntPtr _selSetView;

    // ToolBar selectors
    private IntPtr _nsToolbarClass;
    private IntPtr _nsToolbarItemClass;
    private IntPtr _selSetToolbar;
    private IntPtr _selSetAllowsUserCustomization;
    private IntPtr _selSetAutosavesConfiguration;
    private IntPtr _selSetDisplayMode;
    private IntPtr _selInsertItemWithItemIdentifier;
    private IntPtr _selToolbarRemoveItemAtIndex; // Use different name to avoid conflict with Combo
    private IntPtr _selSetImage;
    // _selSetAction, _selSetTarget, _selSetEnabled already defined in MacOSPlatform.cs
    private IntPtr _selSetMinSize;
    private IntPtr _selSetMaxSize;
    private IntPtr _selSetMenu;
    private IntPtr _selShowsMenu;

    private sealed class TabFolderData
    {
        public IntPtr TabView { get; set; }
        public List<IntPtr> TabItems { get; set; } = new();
    }

    private sealed class TabItemData
    {
        public IntPtr TabFolderHandle { get; set; }
        public IntPtr TabViewItem { get; set; }
        public IntPtr ContentView { get; set; }
        public int Index { get; set; }
    }

    private sealed class ToolBarData
    {
        public IntPtr Toolbar { get; set; }
        public IntPtr Window { get; set; }
        public List<IntPtr> Items { get; set; } = new();
    }

    private sealed class ToolItemData
    {
        public IntPtr ToolbarItem { get; set; }
        public IntPtr ToolbarHandle { get; set; }
        public string Identifier { get; set; } = "";
        public int Style { get; set; }
    }

    private int _nextTabItemId = 0;
    private int _nextToolItemId = 0;

    private void InitializeTabFolderSelectors()
    {
        // Ensure list selectors are initialized (we reuse _selInitWithIdentifier from there)
        InitializeListSelectors();

        if (_nsTabViewClass == IntPtr.Zero)
        {
            _nsTabViewClass = objc_getClass("NSTabView");
            _nsTabViewItemClass = objc_getClass("NSTabViewItem");

            _selAddTabViewItem = sel_registerName("addTabViewItem:");
            _selRemoveTabViewItem = sel_registerName("removeTabViewItem:");
            _selSelectTabViewItemAtIndex = sel_registerName("selectTabViewItemAtIndex:");
            _selIndexOfTabViewItem = sel_registerName("indexOfTabViewItem:");
            _selNumberOfTabViewItems = sel_registerName("numberOfTabViewItems");
            _selTabViewItemAtIndex = sel_registerName("tabViewItemAtIndex:");
            _selSelectedTabViewItem = sel_registerName("selectedTabViewItem");
            _selSetLabel = sel_registerName("setLabel:");
            _selSetView = sel_registerName("setView:");
        }
    }

    public IntPtr CreateTabFolder(IntPtr parent, int style)
    {
        if (_selAlloc == IntPtr.Zero)
        {
            Initialize();
        }
        InitializeTabFolderSelectors();

        // Create NSTabView
        IntPtr tabView = objc_msgSend(_nsTabViewClass, _selAlloc);
        tabView = objc_msgSend(tabView, _selInit);

        // Set default frame
        IntPtr selSetFrame = sel_registerName("setFrame:");
        CGRect frame = new CGRect(0, 0, 300, 200);
        objc_msgSend_rect(tabView, selSetFrame, frame);

        // Add to parent if provided
        AddChildToParent(parent, tabView);

        // Store tab folder data
        _tabFolderData[tabView] = new TabFolderData
        {
            TabView = tabView
        };

        return tabView;
    }

    public void SetTabSelection(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || !_tabFolderData.TryGetValue(handle, out var data))
            return;

        InitializeTabFolderSelectors();

        if (index >= 0 && index < data.TabItems.Count)
        {
            objc_msgSend(data.TabView, _selSelectTabViewItemAtIndex, new IntPtr(index));
        }
    }

    public int GetTabSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_tabFolderData.TryGetValue(handle, out var data))
            return -1;

        InitializeTabFolderSelectors();

        // Get index of selected tab
        IntPtr selectedItem = objc_msgSend(data.TabView, _selSelectedTabViewItem);
        if (selectedItem == IntPtr.Zero)
            return -1;

        // Find the index of this item
        nint index = objc_msgSend_nint(data.TabView, _selIndexOfTabViewItem, selectedItem);
        return (int)index;
    }

    // TabItem operations
    public IntPtr CreateTabItem(IntPtr tabFolderHandle, int style, int index)
    {
        if (tabFolderHandle == IntPtr.Zero || !_tabFolderData.TryGetValue(tabFolderHandle, out var folderData))
            return IntPtr.Zero;

        InitializeTabFolderSelectors();

        // Create NSTabViewItem with unique identifier
        IntPtr tabViewItem = objc_msgSend(_nsTabViewItemClass, _selAlloc);
        IntPtr identifier = CreateNSString($"TabItem{_nextTabItemId++}");
        tabViewItem = objc_msgSend(tabViewItem, _selInitWithIdentifier, identifier);

        // Set default label
        IntPtr defaultLabel = CreateNSString($"Tab {folderData.TabItems.Count}");
        objc_msgSend(tabViewItem, _selSetLabel, defaultLabel);

        // Create a container view for the tab content
        if (_nsViewClass == IntPtr.Zero)
        {
            _nsViewClass = objc_getClass("NSView");
        }
        IntPtr contentView = objc_msgSend(_nsViewClass, _selAlloc);
        contentView = objc_msgSend(contentView, _selInit);

        // Set content view to tab item
        objc_msgSend(tabViewItem, _selSetView, contentView);

        // Add tab item to tab view
        objc_msgSend(folderData.TabView, _selAddTabViewItem, tabViewItem);

        // Create pseudo-handle for tab item (use high bits to differentiate from other handles)
        IntPtr tabItemHandle = new IntPtr(0x30000000 + _nextTabItemId - 1);

        // Store tab item data
        _tabItemData[tabItemHandle] = new TabItemData
        {
            TabFolderHandle = tabFolderHandle,
            TabViewItem = tabViewItem,
            ContentView = contentView,
            Index = index >= 0 ? index : folderData.TabItems.Count
        };

        // Add to folder's tab items list
        if (index >= 0 && index < folderData.TabItems.Count)
        {
            folderData.TabItems.Insert(index, tabItemHandle);
        }
        else
        {
            folderData.TabItems.Add(tabItemHandle);
        }

        return tabItemHandle;
    }

    public void SetTabItemText(IntPtr handle, string text)
    {
        if (!_tabItemData.TryGetValue(handle, out var data))
            return;

        InitializeTabFolderSelectors();

        IntPtr label = CreateNSString(text ?? "");
        objc_msgSend(data.TabViewItem, _selSetLabel, label);
    }

    public void SetTabItemControl(IntPtr handle, IntPtr controlHandle)
    {
        if (!_tabItemData.TryGetValue(handle, out var data))
            return;

        if (controlHandle == IntPtr.Zero)
            return;

        InitializeTabFolderSelectors();

        // Remove the control from its current parent
        IntPtr selSuperview = sel_registerName("superview");
        IntPtr currentParent = objc_msgSend(controlHandle, selSuperview);
        if (currentParent != IntPtr.Zero)
        {
            IntPtr selRemoveFromSuperview = sel_registerName("removeFromSuperview");
            objc_msgSend(controlHandle, selRemoveFromSuperview);
        }

        // Add control to the tab's content view
            // Lazy initialize _selAddSubview if not already done
            if (_selAddSubview == IntPtr.Zero)
            {
                _selAddSubview = sel_registerName("addSubview:");
            }

        objc_msgSend(data.ContentView, _selAddSubview, controlHandle);

        // Make the control fill the content view
        IntPtr selFrame = sel_registerName("frame");
        objc_msgSend_stret(out CGRect parentFrame, data.ContentView, selFrame);

        IntPtr selSetFrame = sel_registerName("setFrame:");
        CGRect controlFrame = new CGRect(0, 0, parentFrame.width, parentFrame.height);
        objc_msgSend_rect(controlHandle, selSetFrame, controlFrame);
    }

    public void SetTabItemToolTip(IntPtr handle, string toolTip)
    {
        if (!_tabItemData.TryGetValue(handle, out var data))
            return;

        InitializeTabFolderSelectors();

        IntPtr selSetToolTip = sel_registerName("setToolTip:");
        IntPtr toolTipString = CreateNSString(toolTip ?? "");
        objc_msgSend(data.TabViewItem, selSetToolTip, toolTipString);
    }

    private void InitializeToolBarSelectors()
    {
        if (_nsToolbarClass == IntPtr.Zero)
        {
            _nsToolbarClass = objc_getClass("NSToolbar");
            _nsToolbarItemClass = objc_getClass("NSToolbarItem");

            _selSetToolbar = sel_registerName("setToolbar:");
            _selSetAllowsUserCustomization = sel_registerName("setAllowsUserCustomization:");
            _selSetAutosavesConfiguration = sel_registerName("setAutosavesConfiguration:");
            _selSetDisplayMode = sel_registerName("setDisplayMode:");
            _selInsertItemWithItemIdentifier = sel_registerName("insertItemWithItemIdentifier:atIndex:");
            _selToolbarRemoveItemAtIndex = sel_registerName("removeItemAtIndex:");
            _selSetImage = sel_registerName("setImage:");
            _selSetMinSize = sel_registerName("setMinSize:");
            _selSetMaxSize = sel_registerName("setMaxSize:");
            _selSetMenu = sel_registerName("setMenu:");
            _selShowsMenu = sel_registerName("setShowsMenu:");
        }

        // Ensure button selectors are initialized (they're in MacOSPlatform.cs)
        if (_selSetAction == IntPtr.Zero)
        {
            _selSetAction = sel_registerName("setAction:");
            _selSetTarget = sel_registerName("setTarget:");
            _selSetEnabled = sel_registerName("setEnabled:");
        }
    }

    // ToolBar operations
    public IntPtr CreateToolBar(IntPtr parent, int style)
    {
        InitializeToolBarSelectors();

        // Create NSToolbar with unique identifier
        IntPtr toolbar = objc_msgSend(_nsToolbarClass, _selAlloc);
        IntPtr identifier = CreateNSString($"Toolbar{_nextToolItemId++}");
        toolbar = objc_msgSend(toolbar, _selInitWithIdentifier, identifier);

        // Configure toolbar based on style
        if ((style & SWT.FLAT) == 0)
        {
            // Allow user customization for non-flat toolbars
            objc_msgSend(toolbar, _selSetAllowsUserCustomization, new IntPtr(1));
            objc_msgSend(toolbar, _selSetAutosavesConfiguration, new IntPtr(1));
        }

        // Set display mode based on style
        // NSToolbarDisplayModeDefault = 0, NSToolbarDisplayModeIconAndLabel = 1,
        // NSToolbarDisplayModeIconOnly = 2, NSToolbarDisplayModeLabelOnly = 3
        int displayMode = (style & SWT.FLAT) != 0 ? 2 : 1; // Icon only for flat, icon+label otherwise
        objc_msgSend(toolbar, _selSetDisplayMode, new IntPtr(displayMode));

        // Create pseudo-handle for toolbar (use high bits to differentiate)
        IntPtr toolbarHandle = new IntPtr(0x40000000 + _nextToolItemId - 1);

        // Store toolbar data
        _toolBarData[toolbarHandle] = new ToolBarData
        {
            Toolbar = toolbar,
            Window = IntPtr.Zero // Will be set when attached to a window
        };

        return toolbarHandle;
    }

    // ToolItem operations
    public IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index)
    {
        if (!_toolBarData.TryGetValue(toolBarHandle, out var toolbarData))
            return IntPtr.Zero;

        InitializeToolBarSelectors();

        IntPtr toolbarItem;
        string itemIdentifier;

        // Handle separator items specially
        if ((style & SWT.SEPARATOR) != 0)
        {
            // Use built-in separator identifiers
            if ((style & SWT.VERTICAL) != 0)
            {
                itemIdentifier = "NSToolbarSpaceItem";
            }
            else
            {
                itemIdentifier = "NSToolbarFlexibleSpaceItem";
            }
        }
        else
        {
            // Create unique identifier for regular items
            itemIdentifier = $"ToolItem{_nextToolItemId++}";
        }

        // Create NSToolbarItem
        toolbarItem = objc_msgSend(_nsToolbarItemClass, _selAlloc);
        IntPtr identifier = CreateNSString(itemIdentifier);
        toolbarItem = objc_msgSend(toolbarItem, _selInitWithIdentifier, identifier);

        // Set default properties
        if ((style & SWT.SEPARATOR) == 0)
        {
            // Set default label
            IntPtr defaultLabel = CreateNSString($"Item {id}");
            objc_msgSend(toolbarItem, _selSetLabel, defaultLabel);

            // Enable by default
            objc_msgSend(toolbarItem, _selSetEnabled, new IntPtr(1));
        }

        // Create pseudo-handle for tool item
        IntPtr toolItemHandle = new IntPtr(0x50000000 + _nextToolItemId - 1);

        // Store tool item data
        _toolItemData[toolItemHandle] = new ToolItemData
        {
            ToolbarItem = toolbarItem,
            ToolbarHandle = toolBarHandle,
            Identifier = itemIdentifier,
            Style = style
        };

        // Add to toolbar's items list
        if (index >= 0 && index < toolbarData.Items.Count)
        {
            toolbarData.Items.Insert(index, toolItemHandle);
        }
        else
        {
            toolbarData.Items.Add(toolItemHandle);
        }

        return toolItemHandle;
    }

    public void DestroyToolItem(IntPtr handle)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        if (!_toolBarData.TryGetValue(itemData.ToolbarHandle, out var toolbarData))
            return;

        InitializeToolBarSelectors();

        // Find index of item
        int index = toolbarData.Items.IndexOf(handle);
        if (index >= 0)
        {
            // Remove from toolbar
            objc_msgSend(toolbarData.Toolbar, _selToolbarRemoveItemAtIndex, new IntPtr(index));

            // Remove from items list
            toolbarData.Items.RemoveAt(index);
        }

        // Clean up data
        _toolItemData.Remove(handle);
    }

    public void SetToolItemText(IntPtr handle, string text)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        InitializeToolBarSelectors();

        IntPtr label = CreateNSString(text ?? "");
        objc_msgSend(itemData.ToolbarItem, _selSetLabel, label);
    }

    public void SetToolItemImage(IntPtr handle, IntPtr image)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        if (image == IntPtr.Zero)
            return;

        InitializeToolBarSelectors();

        objc_msgSend(itemData.ToolbarItem, _selSetImage, image);
    }

    public void SetToolItemToolTip(IntPtr handle, string toolTip)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        InitializeToolBarSelectors();

        IntPtr selSetToolTip = sel_registerName("setToolTip:");
        IntPtr toolTipString = CreateNSString(toolTip ?? "");
        objc_msgSend(itemData.ToolbarItem, selSetToolTip, toolTipString);
    }

    public void SetToolItemSelection(IntPtr handle, bool selected)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Selection only applies to CHECK and RADIO items
        if ((itemData.Style & (SWT.CHECK | SWT.RADIO)) == 0)
            return;

        InitializeToolBarSelectors();

        // For CHECK/RADIO items, we can use the view property to show selected state
        // In macOS, toolbar items don't have a built-in "selected" state like buttons,
        // so this would typically be handled by the item's view or image
        // For now, we'll use setEnabled to simulate the state
        // A more complete implementation would create a custom NSButton view
        IntPtr selState = sel_registerName("setState:");
        int state = selected ? 1 : 0; // NSOnState = 1, NSOffState = 0
        objc_msgSend(itemData.ToolbarItem, selState, new IntPtr(state));
    }

    public void SetToolItemEnabled(IntPtr handle, bool enabled)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        InitializeToolBarSelectors();

        objc_msgSend(itemData.ToolbarItem, _selSetEnabled, new IntPtr(enabled ? 1 : 0));
    }

    public void SetToolItemWidth(IntPtr handle, int width)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        if (width <= 0)
            return;

        InitializeToolBarSelectors();

        // Set both min and max size to enforce width
        IntPtr selMakeSize = sel_registerName("makeSize::");
        CGSize size = new CGSize { width = width, height = 32 }; // Default height of 32

        // NSSize is passed as two CGFloat arguments
        objc_msgSend_size(itemData.ToolbarItem, _selSetMinSize, size);
        objc_msgSend_size(itemData.ToolbarItem, _selSetMaxSize, size);
    }

    public void SetToolItemControl(IntPtr handle, IntPtr control)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        if (control == IntPtr.Zero)
            return;

        InitializeToolBarSelectors();

        // Set the view for the toolbar item
        IntPtr selSetView = sel_registerName("setView:");
        objc_msgSend(itemData.ToolbarItem, selSetView, control);
    }

    // Tree operations - using NSOutlineView for hierarchical display
}
