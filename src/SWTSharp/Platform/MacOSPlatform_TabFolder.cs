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
    private IntPtr _selSetVisible; // For NSToolbar visibility

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

    // REMOVED FIELDS (no longer used after TabFolder method removal):
    // private int _nextTabItemId = 0;
    // This is now handled by the proper widget interface implementations

    // Field still needed for ToolBar operations (not removed in this cleanup)
    private int _nextToolItemId = 0;

    private void InitializeTabFolderSelectors()
    {
        // Ensure list selectors are initialized (we reuse _selInitWithIdentifier from there)
        InitializeListSelectors();

        if (_nsTabViewClass == IntPtr.Zero)
        {
            _nsTabViewClass = objc_getClass("NSTabView");
            if (_nsTabViewClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSTabView class from Objective-C runtime");

            _nsTabViewItemClass = objc_getClass("NSTabViewItem");
            if (_nsTabViewItemClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSTabViewItem class from Objective-C runtime");

            _selAddTabViewItem = RegisterSelector("addTabViewItem:");
            _selRemoveTabViewItem = RegisterSelector("removeTabViewItem:");
            _selSelectTabViewItemAtIndex = RegisterSelector("selectTabViewItemAtIndex:");
            _selIndexOfTabViewItem = RegisterSelector("indexOfTabViewItem:");
            _selNumberOfTabViewItems = RegisterSelector("numberOfTabViewItems");
            _selTabViewItemAtIndex = RegisterSelector("tabViewItemAtIndex:");
            _selSelectedTabViewItem = RegisterSelector("selectedTabViewItem");
            _selSetLabel = RegisterSelector("setLabel:");
            _selSetView = RegisterSelector("setView:");
        }
    }

    // REMOVED METHODS (moved to ITabFolderWidget interface):
    // - CreateTabFolder(IntPtr parent, int style)
    // - SetTabSelection(IntPtr handle, int index)
    // - GetTabSelection(IntPtr handle)
    // - CreateTabItem(IntPtr tabFolderHandle, int style, int index)
    // - SetTabItemText(IntPtr handle, string text)
    // - SetTabItemControl(IntPtr handle, IntPtr controlHandle)
    // - SetTabItemToolTip(IntPtr handle, string toolTip)
    // These methods are now implemented via the ITabFolderWidget interface using proper handles

    private IntPtr _selInitWithItemIdentifier;

    private void InitializeToolBarSelectors()
    {
        if (_nsToolbarClass == IntPtr.Zero)
        {
            _nsToolbarClass = objc_getClass("NSToolbar");
            if (_nsToolbarClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSToolbar class from Objective-C runtime");

            _nsToolbarItemClass = objc_getClass("NSToolbarItem");
            if (_nsToolbarItemClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSToolbarItem class from Objective-C runtime");

            _selSetToolbar = RegisterSelector("setToolbar:");
            _selSetAllowsUserCustomization = RegisterSelector("setAllowsUserCustomization:");
            _selSetAutosavesConfiguration = RegisterSelector("setAutosavesConfiguration:");
            _selSetDisplayMode = RegisterSelector("setDisplayMode:");
            _selInsertItemWithItemIdentifier = RegisterSelector("insertItemWithItemIdentifier:atIndex:");
            _selToolbarRemoveItemAtIndex = RegisterSelector("removeItemAtIndex:");
            _selSetImage = RegisterSelector("setImage:");
            _selSetMinSize = RegisterSelector("setMinSize:");
            _selSetMaxSize = RegisterSelector("setMaxSize:");
            _selSetMenu = RegisterSelector("setMenu:");
            _selShowsMenu = RegisterSelector("setShowsMenu:");
            _selSetVisible = RegisterSelector("setVisible:");
            _selInitWithItemIdentifier = RegisterSelector("initWithItemIdentifier:");  // NSToolbarItem uses initWithItemIdentifier:, not initWithIdentifier:
        }

        // Ensure button selectors are initialized (they're in MacOSPlatform.cs)
        // These selectors are shared - always check if they're already registered
        if (_selSetAction == IntPtr.Zero)
        {
            // These may be registered by InitializeButtonSelectors, so always try to reuse first
            if (_registeredSelectors.TryGetValue("setAction:", out var selAction))
            {
                _selSetAction = selAction;
            }
            else
            {
                _selSetAction = RegisterSelector("setAction:");
            }
        }
        if (_selSetTarget == IntPtr.Zero)
        {
            if (_registeredSelectors.TryGetValue("setTarget:", out var selTarget))
            {
                _selSetTarget = selTarget;
            }
            else
            {
                _selSetTarget = RegisterSelector("setTarget:");
            }
        }
        if (_selSetEnabled == IntPtr.Zero)
        {
            if (_registeredSelectors.TryGetValue("setEnabled:", out var selEnabled))
            {
                _selSetEnabled = selEnabled;
            }
            else
            {
                _selSetEnabled = RegisterSelector("setEnabled:");
            }
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
        toolbarItem = objc_msgSend(toolbarItem, _selInitWithItemIdentifier, identifier);

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

    public void DestroyToolBar(IntPtr handle)
    {
        if (!_toolBarData.TryGetValue(handle, out var toolbarData))
            return;

        InitializeToolBarSelectors();

        // Remove all tool items first
        foreach (var itemHandle in toolbarData.Items.ToArray())
        {
            DestroyToolItem(itemHandle);
        }

        // Remove toolbar from window if attached
        if (toolbarData.Window != IntPtr.Zero)
        {
            objc_msgSend(toolbarData.Window, _selSetToolbar, IntPtr.Zero);
        }

        // Release the NSToolbar
        IntPtr selRelease = sel_registerName("release");
        objc_msgSend(toolbarData.Toolbar, selRelease);

        // Clean up data
        _toolBarData.Remove(handle);
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

    /// <summary>
    /// Attaches a toolbar to a window using the existing NSToolBar implementation.
    /// This method bridges the platform widget interface to the existing pseudo-handle system.
    /// </summary>
    public void AttachToolBarToWindow(IntPtr toolbarHandle, IntPtr windowHandle)
    {
        if (!_toolBarData.TryGetValue(toolbarHandle, out var toolbarData))
            return;

        if (windowHandle == IntPtr.Zero)
            return;

        InitializeToolBarSelectors();

        // Set the toolbar on the window
        objc_msgSend(windowHandle, _selSetToolbar, toolbarData.Toolbar);

        // Update the toolbar data to remember the window
        toolbarData.Window = windowHandle;
    }

    // TabItem bridge methods for IPlatformTabItem implementations
    // These methods bridge the new interface system to the existing NSTabViewItem implementation

    public void SetTabItemText(IntPtr handle, string text)
    {
        if (!_tabItemData.TryGetValue(handle, out var tabItemData))
            return;

        var nsString = CreateNSString(text ?? string.Empty);
        objc_msgSend(tabItemData.TabViewItem, _selSetLabel, nsString);
    }

    public void SetTabItemControl(IntPtr handle, IntPtr controlHandle)
    {
        if (!_tabItemData.TryGetValue(handle, out var tabItemData))
            return;

        objc_msgSend(tabItemData.TabViewItem, _selSetView, controlHandle);
    }

    // CreateTabItem method for IPlatformTabItem implementations
    // This creates a TabItem that can be used independently with the IPlatformTabItem interface
    public IntPtr CreateTabItemForInterface(IntPtr tabFolderHandle, int style, int index)
    {
        // For now, this is a placeholder implementation
        // In a full implementation, this would create an NSTabViewItem and return a pseudo-handle
        // For Phase 5.1, we'll create a simple pseudo-handle that the IPlatformTabItem can use

        IntPtr pseudoHandle = new IntPtr(0x30000000 + _nextTabItemId++);

        // Create a dummy TabItemData entry for now
        _tabItemData[pseudoHandle] = new TabItemData
        {
            TabFolderHandle = tabFolderHandle,
            TabViewItem = IntPtr.Zero // Would be actual NSTabViewItem in full implementation
        };

        return pseudoHandle;
    }

    private static int _nextTabItemId = 1000;

    // Tree operations - using NSOutlineView for hierarchical display
}
