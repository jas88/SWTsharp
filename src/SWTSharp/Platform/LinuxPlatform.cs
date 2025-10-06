using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux platform implementation using GTK3 via P/Invoke.
/// </summary>
internal class LinuxPlatform : IPlatform
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GdkLib = "libgdk-3.so.0";
    private const string GLibLib = "libglib-2.0.so.0";

    // GTK initialization and main loop
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_init_check(ref int argc, ref IntPtr argv);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_main();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_main_quit();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_events_pending();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_main_iteration();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_main_iteration_do(bool blocking);

    // Window creation and management
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_window_new(GtkWindowType type);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_window_set_title(IntPtr window, string title);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_set_default_size(IntPtr window, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_resize(IntPtr window, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_window_move(IntPtr window, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show_all(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_window(IntPtr widget);

    // Signal handling
    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern ulong g_signal_connect_data(
        IntPtr instance,
        string detailed_signal,
        IntPtr callback,
        IntPtr data,
        IntPtr destroy_data,
        int connect_flags);

    // GDK event handling
    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gdk_window_process_all_updates();

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gdk_events_pending();

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gdk_event_get();

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gdk_event_put(IntPtr evt);

    // Application
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_application_new(string application_id, int flags);

    // Enums
    private enum GtkWindowType
    {
        Toplevel = 0,
        Popup = 1
    }

    private bool _initialized;

    public void Initialize()
    {
        if (_initialized)
            return;

        int argc = 0;
        IntPtr argv = IntPtr.Zero;

        if (!gtk_init_check(ref argc, ref argv))
        {
            throw new InvalidOperationException("Failed to initialize GTK");
        }

        _initialized = true;
    }

    public IntPtr CreateWindow(int style, string title)
    {
        // Create a top-level window
        IntPtr window = gtk_window_new(GtkWindowType.Toplevel);

        if (window == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK window");
        }

        // Set window properties
        gtk_window_set_title(window, title);
        gtk_window_set_default_size(window, 800, 600);

        // Connect destroy signal to quit application when window is closed
        // For now, we'll skip signal handling to keep it simple
        // In production, you'd want: g_signal_connect_data(window, "destroy", callback, ...)

        return window;
    }

    public void DestroyWindow(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            gtk_widget_destroy(handle);
        }
    }

    public void SetWindowVisible(IntPtr handle, bool visible)
    {
        if (visible)
        {
            gtk_widget_show_all(handle);
        }
        else
        {
            gtk_widget_hide(handle);
        }
    }

    public void SetWindowText(IntPtr handle, string text)
    {
        gtk_window_set_title(handle, text);
    }

    public void SetWindowSize(IntPtr handle, int width, int height)
    {
        gtk_window_resize(handle, width, height);
    }

    public void SetWindowLocation(IntPtr handle, int x, int y)
    {
        gtk_window_move(handle, x, y);
    }

    public bool ProcessEvent()
    {
        // Check if there are pending events
        if (gtk_events_pending())
        {
            // Process one iteration without blocking
            gtk_main_iteration_do(false);
            return true;
        }

        return false;
    }

    public void WaitForEvent()
    {
        // Block until an event is available and process it
        gtk_main_iteration_do(true);
    }

    public void WakeEventLoop()
    {
        // Wake up the main loop by quitting it
        // This will cause gtk_main_iteration_do(true) to return
        gtk_main_quit();
    }

    public IntPtr CreateComposite(int style)
    {
        // Create a GtkFixed container widget (allows absolute positioning)
        // This is similar to SWT's Composite when no layout is set
        IntPtr composite = gtk_fixed_new();

        if (composite != IntPtr.Zero)
        {
            // Show the widget by default
            gtk_widget_show(composite);
        }

        return composite;
    }

    // Button creation and management
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_button_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_button_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_button_set_label(IntPtr button, string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_button_get_label(IntPtr button);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_check_button_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_radio_button_new_with_label(IntPtr group, string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_toggle_button_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toggle_button_set_active(IntPtr toggle_button, bool is_active);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_toggle_button_get_active(IntPtr toggle_button);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_fixed_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_put(IntPtr @fixed, IntPtr widget, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_move(IntPtr @fixed, IntPtr widget, int x, int y);

    private delegate void GtkSignalFunc(IntPtr widget, IntPtr data);

    private readonly Dictionary<IntPtr, Action> _buttonCallbacks = new Dictionary<IntPtr, Action>();
    private readonly Dictionary<IntPtr, IntPtr> _widgetContainers = new Dictionary<IntPtr, IntPtr>();

    public IntPtr CreateButton(IntPtr parent, int style, string text)
    {
        IntPtr button;

        // Create appropriate button type based on SWT style
        if ((style & SWT.CHECK) != 0)
        {
            button = gtk_check_button_new_with_label(text);
        }
        else if ((style & SWT.RADIO) != 0)
        {
            button = gtk_radio_button_new_with_label(IntPtr.Zero, text);
        }
        else if ((style & SWT.TOGGLE) != 0)
        {
            button = gtk_toggle_button_new_with_label(text);
        }
        else // Default to PUSH
        {
            button = gtk_button_new_with_label(text);
        }

        if (button == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK button");
        }

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            // GTK requires a container for absolute positioning
            // We'll use gtk_fixed for simple layout
            gtk_container_add(parent, button);
        }

        gtk_widget_show(button);

        return button;
    }

    public void SetButtonText(IntPtr handle, string text)
    {
        gtk_button_set_label(handle, text);
    }

    public void SetButtonSelection(IntPtr handle, bool selected)
    {
        // Only works for toggle-style buttons
        gtk_toggle_button_set_active(handle, selected);
    }

    public bool GetButtonSelection(IntPtr handle)
    {
        // Only works for toggle-style buttons
        return gtk_toggle_button_get_active(handle);
    }

    public void SetControlEnabled(IntPtr handle, bool enabled)
    {
        gtk_widget_set_sensitive(handle, enabled);
    }

    public void SetControlVisible(IntPtr handle, bool visible)
    {
        if (visible)
        {
            gtk_widget_show(handle);
        }
        else
        {
            gtk_widget_hide(handle);
        }
    }

    public void SetControlBounds(IntPtr handle, int x, int y, int width, int height)
    {
        // Set size
        gtk_widget_set_size_request(handle, width, height);

        // Position would require getting parent container
        // and using gtk_fixed_move if it's a GtkFixed
        // For now, just set the size request
    }

    public void ConnectButtonClick(IntPtr handle, Action callback)
    {
        // Store callback
        _buttonCallbacks[handle] = callback;

        // Create a delegate for the signal handler
        GtkSignalFunc signalHandler = (widget, data) =>
        {
            if (_buttonCallbacks.TryGetValue(widget, out var cb))
            {
                cb?.Invoke();
            }
        };

        // Connect the "clicked" signal
        IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(signalHandler);
        g_signal_connect_data(handle, "clicked", funcPtr, IntPtr.Zero, IntPtr.Zero, 0);

        // Keep delegate alive
        GC.KeepAlive(signalHandler);
    }

    // Menu operations
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_bar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_item_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_menu_item_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_check_menu_item_new_with_label(string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_radio_menu_item_new_with_label(IntPtr group, string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_separator_menu_item_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_menu_shell_append(IntPtr menu_shell, IntPtr child);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_menu_shell_insert(IntPtr menu_shell, IntPtr child, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_menu_item_set_label(IntPtr menu_item, string label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_menu_item_set_submenu(IntPtr menu_item, IntPtr submenu);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_check_menu_item_set_active(IntPtr check_menu_item, bool is_active);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_menu_popup_at_pointer(IntPtr menu, IntPtr trigger_event);

    IntPtr IPlatform.CreateMenu(int style)
    {
        if ((style & SWT.BAR) != 0)
        {
            return gtk_menu_bar_new();
        }
        else
        {
            return gtk_menu_new();
        }
    }

    void IPlatform.DestroyMenu(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            gtk_widget_destroy(handle);
        }
    }

    void IPlatform.SetShellMenuBar(IntPtr shellHandle, IntPtr menuHandle)
    {
        // Create a vertical box to hold menu bar and content
        // This is simplified - in production you'd need proper container management
        gtk_container_add(shellHandle, menuHandle);
        gtk_widget_show(menuHandle);
    }

    void IPlatform.SetMenuVisible(IntPtr handle, bool visible)
    {
        if (visible)
        {
            gtk_widget_show(handle);
        }
        else
        {
            gtk_widget_hide(handle);
        }
    }

    void IPlatform.ShowPopupMenu(IntPtr menuHandle, int x, int y)
    {
        gtk_menu_popup_at_pointer(menuHandle, IntPtr.Zero);
    }

    IntPtr IPlatform.CreateMenuItem(IntPtr menuHandle, int style, int id, int index)
    {
        IntPtr menuItem;

        if ((style & SWT.SEPARATOR) != 0)
        {
            menuItem = gtk_separator_menu_item_new();
        }
        else if ((style & SWT.CHECK) != 0)
        {
            menuItem = gtk_check_menu_item_new_with_label(string.Empty);
        }
        else if ((style & SWT.RADIO) != 0)
        {
            menuItem = gtk_radio_menu_item_new_with_label(IntPtr.Zero, string.Empty);
        }
        else
        {
            menuItem = gtk_menu_item_new_with_label(string.Empty);
        }

        // Add to menu
        if (index >= 0)
        {
            gtk_menu_shell_insert(menuHandle, menuItem, index);
        }
        else
        {
            gtk_menu_shell_append(menuHandle, menuItem);
        }

        gtk_widget_show(menuItem);

        return menuItem;
    }

    void IPlatform.DestroyMenuItem(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            gtk_widget_destroy(handle);
        }
    }

    void IPlatform.SetMenuItemText(IntPtr handle, string text)
    {
        gtk_menu_item_set_label(handle, text);
    }

    void IPlatform.SetMenuItemSelection(IntPtr handle, bool selected)
    {
        gtk_check_menu_item_set_active(handle, selected);
    }

    void IPlatform.SetMenuItemEnabled(IntPtr handle, bool enabled)
    {
        gtk_widget_set_sensitive(handle, enabled);
    }

    void IPlatform.SetMenuItemSubmenu(IntPtr itemHandle, IntPtr submenuHandle)
    {
        gtk_menu_item_set_submenu(itemHandle, submenuHandle);
    }

    // Label operations
    public IntPtr CreateLabel(IntPtr parent, int style, int alignment, bool wrap)
    {
        // TODO: Implement Label creation
        throw new NotImplementedException("Label not yet implemented on Linux platform");
    }

    public void SetLabelText(IntPtr handle, string text)
    {
        // TODO: Implement SetLabelText
        throw new NotImplementedException("Label not yet implemented on Linux platform");
    }

    public void SetLabelAlignment(IntPtr handle, int alignment)
    {
        // TODO: Implement SetLabelAlignment
        throw new NotImplementedException("Label not yet implemented on Linux platform");
    }

    // Text control operations
    public IntPtr CreateText(IntPtr parent, int style)
    {
        // TODO: Implement Text control creation
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    public void SetTextContent(IntPtr handle, string text)
    {
        // TODO: Implement SetTextContent
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    public string GetTextContent(IntPtr handle)
    {
        // TODO: Implement GetTextContent
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    public void SetTextSelection(IntPtr handle, int start, int end)
    {
        // TODO: Implement SetTextSelection
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    public (int Start, int End) GetTextSelection(IntPtr handle)
    {
        // TODO: Implement GetTextSelection
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    public void SetTextLimit(IntPtr handle, int limit)
    {
        // TODO: Implement SetTextLimit
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    public void SetTextReadOnly(IntPtr handle, bool readOnly)
    {
        // TODO: Implement SetTextReadOnly
        throw new NotImplementedException("Text control not yet implemented on Linux platform");
    }

    // List control operations
    public IntPtr CreateList(IntPtr parentHandle, int style)
    {
        // TODO: Implement List control creation
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public void AddListItem(IntPtr handle, string item, int index)
    {
        // TODO: Implement AddListItem
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public void RemoveListItem(IntPtr handle, int index)
    {
        // TODO: Implement RemoveListItem
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public void ClearListItems(IntPtr handle)
    {
        // TODO: Implement ClearListItems
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public void SetListSelection(IntPtr handle, int[] indices)
    {
        // TODO: Implement SetListSelection
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public int[] GetListSelection(IntPtr handle)
    {
        // TODO: Implement GetListSelection
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public int GetListTopIndex(IntPtr handle)
    {
        // TODO: Implement GetListTopIndex
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    public void SetListTopIndex(IntPtr handle, int index)
    {
        // TODO: Implement SetListTopIndex
        throw new NotImplementedException("List control not yet implemented on Linux platform");
    }

    // Combo control operations
    public IntPtr CreateCombo(IntPtr parentHandle, int style)
    {
        // TODO: Implement Combo control creation
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void SetComboText(IntPtr handle, string text)
    {
        // TODO: Implement SetComboText
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public string GetComboText(IntPtr handle)
    {
        // TODO: Implement GetComboText
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void AddComboItem(IntPtr handle, string item, int index)
    {
        // TODO: Implement AddComboItem
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void RemoveComboItem(IntPtr handle, int index)
    {
        // TODO: Implement RemoveComboItem
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void ClearComboItems(IntPtr handle)
    {
        // TODO: Implement ClearComboItems
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void SetComboSelection(IntPtr handle, int index)
    {
        // TODO: Implement SetComboSelection
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public int GetComboSelection(IntPtr handle)
    {
        // TODO: Implement GetComboSelection
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void SetComboTextLimit(IntPtr handle, int limit)
    {
        // TODO: Implement SetComboTextLimit
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void SetComboVisibleItemCount(IntPtr handle, int count)
    {
        // TODO: Implement SetComboVisibleItemCount
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void SetComboTextSelection(IntPtr handle, int start, int end)
    {
        // TODO: Implement SetComboTextSelection
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public (int Start, int End) GetComboTextSelection(IntPtr handle)
    {
        // TODO: Implement GetComboTextSelection
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void ComboTextCopy(IntPtr handle)
    {
        // TODO: Implement ComboTextCopy
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void ComboTextCut(IntPtr handle)
    {
        // TODO: Implement ComboTextCut
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    public void ComboTextPaste(IntPtr handle)
    {
        // TODO: Implement ComboTextPaste
        throw new NotImplementedException("Combo control not yet implemented on Linux platform");
    }

    // Group operations
    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        throw new NotImplementedException("Group not yet implemented on Linux platform");
    }

    public void SetGroupText(IntPtr handle, string text)
    {
        throw new NotImplementedException("Group not yet implemented on Linux platform");
    }

    // Canvas operations
    public IntPtr CreateCanvas(IntPtr parent, int style)
    {
        throw new NotImplementedException("Canvas not yet implemented on Linux platform");
    }

    public void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    {
        throw new NotImplementedException("Canvas not yet implemented on Linux platform");
    }

    public void RedrawCanvas(IntPtr handle)
    {
        throw new NotImplementedException("Canvas not yet implemented on Linux platform");
    }

    public void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    {
        throw new NotImplementedException("Canvas not yet implemented on Linux platform");
    }

    // TabFolder operations
    public IntPtr CreateTabFolder(IntPtr parent, int style)
    {
        throw new NotImplementedException("TabFolder not yet implemented on Linux platform");
    }

    public void SetTabSelection(IntPtr handle, int index)
    {
        throw new NotImplementedException("TabFolder not yet implemented on Linux platform");
    }

    public int GetTabSelection(IntPtr handle)
    {
        throw new NotImplementedException("TabFolder not yet implemented on Linux platform");
    }

    // TabItem operations
    public IntPtr CreateTabItem(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TabItem not yet implemented on Linux platform");
    }

    public void SetTabItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TabItem not yet implemented on Linux platform");
    }

    public void SetTabItemControl(IntPtr handle, IntPtr control)
    {
        throw new NotImplementedException("TabItem not yet implemented on Linux platform");
    }

    public void SetTabItemToolTip(IntPtr handle, string toolTip)
    {
        throw new NotImplementedException("TabItem not yet implemented on Linux platform");
    }

    // ToolBar operations
    public IntPtr CreateToolBar(int style)
    {
        throw new NotImplementedException("ToolBar not yet implemented on Linux platform");
    }

    // ToolItem operations
    public IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void DestroyToolItem(IntPtr handle)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemImage(IntPtr handle, IntPtr image)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemToolTip(IntPtr handle, string toolTip)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemSelection(IntPtr handle, bool selected)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemEnabled(IntPtr handle, bool enabled)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemWidth(IntPtr handle, int width)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    public void SetToolItemControl(IntPtr handle, IntPtr control)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Linux platform");
    }

    // Tree operations
    public IntPtr CreateTree(IntPtr parent, int style)
    {
        throw new NotImplementedException("Tree not yet implemented on Linux platform");
    }

    public IntPtr[] GetTreeSelection(IntPtr handle)
    {
        throw new NotImplementedException("Tree not yet implemented on Linux platform");
    }

    public void SetTreeSelection(IntPtr handle, IntPtr[] items)
    {
        throw new NotImplementedException("Tree not yet implemented on Linux platform");
    }

    public void ClearTreeItems(IntPtr handle)
    {
        throw new NotImplementedException("Tree not yet implemented on Linux platform");
    }

    public void ShowTreeItem(IntPtr handle, IntPtr item)
    {
        throw new NotImplementedException("Tree not yet implemented on Linux platform");
    }

    // TreeItem operations
    public IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void DestroyTreeItem(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void SetTreeItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void SetTreeItemImage(IntPtr handle, IntPtr image)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void SetTreeItemChecked(IntPtr handle, bool checked_)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public bool GetTreeItemChecked(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void SetTreeItemExpanded(IntPtr handle, bool expanded)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public bool GetTreeItemExpanded(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void ClearTreeItemChildren(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    public void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Linux platform");
    }

    // Table operations
    public IntPtr CreateTable(IntPtr parent, int style)
    {
        throw new NotImplementedException("Table not yet implemented on Linux platform");
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        throw new NotImplementedException("Table not yet implemented on Linux platform");
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        throw new NotImplementedException("Table not yet implemented on Linux platform");
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        throw new NotImplementedException("Table not yet implemented on Linux platform");
    }

    public void ClearTableItems(IntPtr handle)
    {
        throw new NotImplementedException("Table not yet implemented on Linux platform");
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        throw new NotImplementedException("Table not yet implemented on Linux platform");
    }

    // TableColumn operations
    public IntPtr CreateTableColumn(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTip)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    public int PackTableColumn(IntPtr handle)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Linux platform");
    }

    // TableItem operations
    public IntPtr CreateTableItem(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void DestroyTableItem(IntPtr handle)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void SetTableItemText(IntPtr handle, int column, string text)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void SetTableItemImage(IntPtr handle, int column, IntPtr image)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void SetTableItemChecked(IntPtr handle, bool checked_)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        throw new NotImplementedException("TableItem not yet implemented on Linux platform");
    }

    // ProgressBar operations
    public IntPtr CreateProgressBar(IntPtr parent, int style)
    {
        throw new NotImplementedException("ProgressBar not yet implemented on Linux platform");
    }

    public void SetProgressBarRange(IntPtr handle, int minimum, int maximum)
    {
        throw new NotImplementedException("ProgressBar not yet implemented on Linux platform");
    }

    public void SetProgressBarSelection(IntPtr handle, int value)
    {
        throw new NotImplementedException("ProgressBar not yet implemented on Linux platform");
    }

    public void SetProgressBarState(IntPtr handle, int state)
    {
        throw new NotImplementedException("ProgressBar not yet implemented on Linux platform");
    }

    // Slider operations
    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        throw new NotImplementedException("Slider not yet implemented on Linux platform");
    }

    public void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Slider not yet implemented on Linux platform");
    }

    public void ConnectSliderChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Slider not yet implemented on Linux platform");
    }

    // Scale operations
    public IntPtr CreateScale(IntPtr parent, int style)
    {
        throw new NotImplementedException("Scale not yet implemented on Linux platform");
    }

    public void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Scale not yet implemented on Linux platform");
    }

    public void ConnectScaleChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Scale not yet implemented on Linux platform");
    }

    // Spinner operations
    public IntPtr CreateSpinner(IntPtr parent, int style)
    {
        throw new NotImplementedException("Spinner not yet implemented on Linux platform");
    }

    public void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Spinner not yet implemented on Linux platform");
    }

    public void SetSpinnerTextLimit(IntPtr handle, int limit)
    {
        throw new NotImplementedException("Spinner not yet implemented on Linux platform");
    }

    public void ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Spinner not yet implemented on Linux platform");
    }

    public void ConnectSpinnerModified(IntPtr handle, Action callback)
    {
        throw new NotImplementedException("Spinner not yet implemented on Linux platform");
    }

    // Dialog operations
    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        throw new NotImplementedException("MessageBox not yet implemented on Linux platform");
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        throw new NotImplementedException("FileDialog not yet implemented on Linux platform");
    }

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        throw new NotImplementedException("DirectoryDialog not yet implemented on Linux platform");
    }

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        throw new NotImplementedException("ColorDialog not yet implemented on Linux platform");
    }

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        throw new NotImplementedException("FontDialog not yet implemented on Linux platform");
    }
}
