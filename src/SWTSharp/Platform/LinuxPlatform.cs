using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux platform implementation using GTK3 via P/Invoke.
/// </summary>
internal partial class LinuxPlatform : IPlatform
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
    private readonly Dictionary<GtkSignalFunc, object> _delegateReferences = new Dictionary<GtkSignalFunc, object>();

    // LEAK-002: Cleanup method for button callbacks
    public void ClearButtonCallbacks()
    {
        _buttonCallbacks.Clear();
        _delegateReferences.Clear();
    }

    // LEAK-002: Remove specific button callback when control is destroyed
    public void RemoveButtonCallback(IntPtr handle)
    {
        _buttonCallbacks.Remove(handle);
    }

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

        // LEAK-002: Store delegate reference to prevent GC collection
        _delegateReferences[signalHandler] = handle;

        // Connect the "clicked" signal
        IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(signalHandler);
        g_signal_connect_data(handle, "clicked", funcPtr, IntPtr.Zero, IntPtr.Zero, 0);
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

    // Label operations - implemented in LinuxPlatform_Label.cs

    // GTK Text/Entry widget imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_entry_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_entry_set_text(IntPtr entry, string text);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_entry_get_text(IntPtr entry);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_entry_set_visibility(IntPtr entry, bool visible);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_entry_set_max_length(IntPtr entry, int max);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_editable_set_editable(IntPtr editable, bool is_editable);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_editable_select_region(IntPtr editable, int start_pos, int end_pos);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_editable_get_selection_bounds(IntPtr editable, out int start_pos, out int end_pos);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_view_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_view_get_buffer(IntPtr text_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_text_buffer_set_text(IntPtr buffer, string text, int len);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_buffer_get_text(IntPtr buffer, IntPtr start, IntPtr end, bool include_hidden_chars);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_get_start_iter(IntPtr buffer, IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_get_end_iter(IntPtr buffer, IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_view_set_editable(IntPtr text_view, bool setting);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolled_window, int hscrollbar_policy, int vscrollbar_policy);

    private readonly Dictionary<IntPtr, bool> _textWidgetTypes = new Dictionary<IntPtr, bool>(); // true = TextView, false = Entry

    // Text control operations
    public IntPtr CreateText(IntPtr parent, int style)
    {
        IntPtr widget;
        bool isMultiLine = (style & SWT.MULTI) != 0;

        if (isMultiLine)
        {
            // Create multi-line text view with scrolled window
            IntPtr textView = gtk_text_view_new();
            IntPtr scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);

            // Set scroll policy: automatic for both horizontal and vertical
            gtk_scrolled_window_set_policy(scrolledWindow, 1, 1); // GTK_POLICY_AUTOMATIC = 1

            gtk_container_add(scrolledWindow, textView);
            gtk_widget_show(textView);

            widget = scrolledWindow;
            _textWidgetTypes[widget] = true; // Mark as TextView
            _widgetContainers[textView] = scrolledWindow; // Store relationship
        }
        else
        {
            // Create single-line entry
            widget = gtk_entry_new();
            _textWidgetTypes[widget] = false; // Mark as Entry

            // Handle password style
            if ((style & SWT.PASSWORD) != 0)
            {
                gtk_entry_set_visibility(widget, false);
            }
        }

        // Handle read-only style
        if ((style & SWT.READ_ONLY) != 0)
        {
            SetTextReadOnly(widget, true);
        }

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            gtk_container_add(parent, widget);
        }

        gtk_widget_show(widget);

        return widget;
    }

    public void SetTextContent(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid text handle", nameof(handle));

        if (_textWidgetTypes.TryGetValue(handle, out bool isTextView))
        {
            if (isTextView)
            {
                // Multi-line TextView - get actual TextView widget
                IntPtr textView = IntPtr.Zero;
                foreach (var kvp in _widgetContainers)
                {
                    if (kvp.Value == handle)
                    {
                        textView = kvp.Key;
                        break;
                    }
                }

                if (textView != IntPtr.Zero)
                {
                    IntPtr buffer = gtk_text_view_get_buffer(textView);
                    gtk_text_buffer_set_text(buffer, text ?? string.Empty, -1);
                }
            }
            else
            {
                // Single-line Entry
                gtk_entry_set_text(handle, text ?? string.Empty);
            }
        }
        else
        {
            // Fallback: assume Entry
            gtk_entry_set_text(handle, text ?? string.Empty);
        }
    }

    public string GetTextContent(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid text handle", nameof(handle));

        if (_textWidgetTypes.TryGetValue(handle, out bool isTextView))
        {
            if (isTextView)
            {
                // Multi-line TextView
                IntPtr textView = IntPtr.Zero;
                foreach (var kvp in _widgetContainers)
                {
                    if (kvp.Value == handle)
                    {
                        textView = kvp.Key;
                        break;
                    }
                }

                if (textView != IntPtr.Zero)
                {
                    IntPtr buffer = gtk_text_view_get_buffer(textView);

                    // Allocate GtkTextIter structures (80 bytes each for safety)
                    IntPtr startIter = Marshal.AllocHGlobal(80);
                    IntPtr endIter = Marshal.AllocHGlobal(80);

                    try
                    {
                        gtk_text_buffer_get_start_iter(buffer, startIter);
                        gtk_text_buffer_get_end_iter(buffer, endIter);

                        IntPtr textPtr = gtk_text_buffer_get_text(buffer, startIter, endIter, false);
#if NETSTANDARD2_0
                        string result = Marshal.PtrToStringAnsi(textPtr) ?? string.Empty;
#else
                        string result = Marshal.PtrToStringUTF8(textPtr) ?? string.Empty;
#endif

                        // Free the string returned by GTK
                        g_free(textPtr);

                        return result;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(startIter);
                        Marshal.FreeHGlobal(endIter);
                    }
                }
                return string.Empty;
            }
            else
            {
                // Single-line Entry
                IntPtr textPtr = gtk_entry_get_text(handle);
#if NETSTANDARD2_0
                return Marshal.PtrToStringAnsi(textPtr) ?? string.Empty;
#else
                return Marshal.PtrToStringUTF8(textPtr) ?? string.Empty;
#endif
            }
        }
        else
        {
            // Fallback: assume Entry
            IntPtr textPtr = gtk_entry_get_text(handle);
#if NETSTANDARD2_0
            return Marshal.PtrToStringAnsi(textPtr) ?? string.Empty;
#else
            return Marshal.PtrToStringUTF8(textPtr) ?? string.Empty;
#endif
        }
    }

    public void SetTextSelection(IntPtr handle, int start, int end)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid text handle", nameof(handle));

        if (_textWidgetTypes.TryGetValue(handle, out bool isTextView))
        {
            if (!isTextView)
            {
                // Single-line Entry supports selection
                gtk_editable_select_region(handle, start, end);
            }
            // Note: TextView selection not implemented yet - requires GtkTextBuffer iters
        }
        else
        {
            // Fallback: assume Entry
            gtk_editable_select_region(handle, start, end);
        }
    }

    public (int Start, int End) GetTextSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid text handle", nameof(handle));

        if (_textWidgetTypes.TryGetValue(handle, out bool isTextView))
        {
            if (!isTextView)
            {
                // Single-line Entry supports selection
                if (gtk_editable_get_selection_bounds(handle, out int start, out int end))
                {
                    return (start, end);
                }
                return (0, 0);
            }
            // Note: TextView selection not implemented yet
            return (0, 0);
        }
        else
        {
            // Fallback: assume Entry
            if (gtk_editable_get_selection_bounds(handle, out int start, out int end))
            {
                return (start, end);
            }
            return (0, 0);
        }
    }

    public void SetTextLimit(IntPtr handle, int limit)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid text handle", nameof(handle));

        if (_textWidgetTypes.TryGetValue(handle, out bool isTextView))
        {
            if (!isTextView)
            {
                // Single-line Entry supports max length
                gtk_entry_set_max_length(handle, limit);
            }
            // Note: TextView max length requires custom signal handling
        }
        else
        {
            // Fallback: assume Entry
            gtk_entry_set_max_length(handle, limit);
        }
    }

    public void SetTextReadOnly(IntPtr handle, bool readOnly)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid text handle", nameof(handle));

        if (_textWidgetTypes.TryGetValue(handle, out bool isTextView))
        {
            if (isTextView)
            {
                // Multi-line TextView
                IntPtr textView = IntPtr.Zero;
                foreach (var kvp in _widgetContainers)
                {
                    if (kvp.Value == handle)
                    {
                        textView = kvp.Key;
                        break;
                    }
                }

                if (textView != IntPtr.Zero)
                {
                    gtk_text_view_set_editable(textView, !readOnly);
                }
            }
            else
            {
                // Single-line Entry
                gtk_editable_set_editable(handle, !readOnly);
            }
        }
        else
        {
            // Fallback: assume Entry
            gtk_editable_set_editable(handle, !readOnly);
        }
    }

    // GLib memory functions
    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_free(IntPtr mem);

    // GTK List Box widget imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_insert(IntPtr box, IntPtr child, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_remove(IntPtr box, IntPtr child);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_select_row(IntPtr box, IntPtr row);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_unselect_row(IntPtr box, IntPtr row);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_get_row_at_index(IntPtr box, int index);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_list_box_row_get_index(IntPtr row);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_get_selected_row(IntPtr box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_set_selection_mode(IntPtr box, int mode);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_get_selected_rows(IntPtr box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_unselect_all(IntPtr box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_get_vadjustment(IntPtr scrolled_window);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern double gtk_adjustment_get_value(IntPtr adjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_set_value(IntPtr adjustment, double value);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_list_nth_data(IntPtr list, uint n);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint g_list_length(IntPtr list);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_list_free(IntPtr list);

    // GTK selection mode enum
    private enum GtkSelectionMode
    {
        None = 0,
        Single = 1,
        Browse = 2,
        Multiple = 3
    }

    // Track list box rows for each list widget
    private readonly Dictionary<IntPtr, List<IntPtr>> _listBoxRows = new Dictionary<IntPtr, List<IntPtr>>();
    // Track the list box widget handle for each scrolled window
    private readonly Dictionary<IntPtr, IntPtr> _listBoxWidgets = new Dictionary<IntPtr, IntPtr>();

    // List control operations
    public IntPtr CreateList(IntPtr parentHandle, int style)
    {
        // Create GtkListBox
        IntPtr listBox = gtk_list_box_new();

        if (listBox == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK list box");
        }

        // Set selection mode based on SWT style
        GtkSelectionMode selectionMode = (style & SWT.MULTI) != 0
            ? GtkSelectionMode.Multiple
            : GtkSelectionMode.Single;
        gtk_list_box_set_selection_mode(listBox, (int)selectionMode);

        // Create scrolled window to contain the list box
        IntPtr scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);

        // Set scroll policy: automatic for both horizontal and vertical
        gtk_scrolled_window_set_policy(scrolledWindow, 1, 1); // GTK_POLICY_AUTOMATIC = 1

        gtk_container_add(scrolledWindow, listBox);
        gtk_widget_show(listBox);

        // Track the relationship between scrolled window and list box
        _listBoxWidgets[scrolledWindow] = listBox;

        // Initialize row tracking for this list
        _listBoxRows[scrolledWindow] = new List<IntPtr>();

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, scrolledWindow);
        }

        gtk_widget_show(scrolledWindow);

        return scrolledWindow;
    }

    public void AddListItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        if (!_listBoxRows.TryGetValue(handle, out var rows))
        {
            throw new ArgumentException("List handle not found in tracking dictionary", nameof(handle));
        }

        if (!_listBoxWidgets.TryGetValue(handle, out IntPtr listBox))
        {
            throw new ArgumentException("List box widget not found", nameof(handle));
        }

        // Create a label widget for the item
        IntPtr label = gtk_label_new(item ?? string.Empty);
        if (label == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create label for list item");
        }
        gtk_widget_show(label);

        // Insert at specified index (-1 means append)
        gtk_list_box_insert(listBox, label, index);

        // Track the row handle
        IntPtr row = gtk_list_box_get_row_at_index(listBox, index >= 0 ? index : rows.Count);
        if (index >= 0 && index < rows.Count)
        {
            rows.Insert(index, row);
        }
        else
        {
            rows.Add(row);
        }
    }

    public void RemoveListItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        if (!_listBoxRows.TryGetValue(handle, out var rows))
        {
            throw new ArgumentException("List handle not found in tracking dictionary", nameof(handle));
        }

        if (!_listBoxWidgets.TryGetValue(handle, out IntPtr listBox))
        {
            throw new ArgumentException("List box widget not found", nameof(handle));
        }

        if (index < 0 || index >= rows.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index out of range");
        }

        // Get the row to remove
        IntPtr row = rows[index];

        // Remove from the list box
        gtk_list_box_remove(listBox, row);

        // Remove from tracking list
        rows.RemoveAt(index);
    }

    public void ClearListItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        if (!_listBoxRows.TryGetValue(handle, out var rows))
        {
            throw new ArgumentException("List handle not found in tracking dictionary", nameof(handle));
        }

        if (!_listBoxWidgets.TryGetValue(handle, out IntPtr listBox))
        {
            throw new ArgumentException("List box widget not found", nameof(handle));
        }

        // Remove all rows from the list box
        for (int i = rows.Count - 1; i >= 0; i--)
        {
            IntPtr row = rows[i];
            gtk_list_box_remove(listBox, row);
        }

        rows.Clear();
    }

    public void SetListSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        if (!_listBoxRows.TryGetValue(handle, out var rows))
        {
            throw new ArgumentException("List handle not found in tracking dictionary", nameof(handle));
        }

        if (!_listBoxWidgets.TryGetValue(handle, out IntPtr listBox))
        {
            throw new ArgumentException("List box widget not found", nameof(handle));
        }

        // Clear current selection
        gtk_list_box_unselect_all(listBox);

        // Select specified indices
        if (indices != null)
        {
            foreach (int index in indices)
            {
                if (index >= 0 && index < rows.Count)
                {
                    IntPtr row = rows[index];
                    gtk_list_box_select_row(listBox, row);
                }
            }
        }
    }

    public int[] GetListSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        if (!_listBoxRows.TryGetValue(handle, out var rows))
        {
            throw new ArgumentException("List handle not found in tracking dictionary", nameof(handle));
        }

        if (!_listBoxWidgets.TryGetValue(handle, out IntPtr listBox))
        {
            throw new ArgumentException("List box widget not found", nameof(handle));
        }

        // Get selected rows
        IntPtr selectedRowsList = gtk_list_box_get_selected_rows(listBox);

        if (selectedRowsList == IntPtr.Zero)
        {
            return Array.Empty<int>();
        }

        try
        {
            uint length = g_list_length(selectedRowsList);

            if (length == 0)
            {
                return Array.Empty<int>();
            }

            int[] indices = new int[length];
            for (uint i = 0; i < length; i++)
            {
                IntPtr rowPtr = g_list_nth_data(selectedRowsList, i);
                if (rowPtr != IntPtr.Zero)
                {
                    int index = gtk_list_box_row_get_index(rowPtr);
                    indices[i] = index;
                }
            }

            return indices;
        }
        finally
        {
            g_list_free(selectedRowsList);
        }
    }

    public int GetListTopIndex(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        // Get the vertical adjustment from the scrolled window
        IntPtr adjustment = gtk_scrolled_window_get_vadjustment(handle);

        if (adjustment == IntPtr.Zero)
            return 0;

        // Get the current scroll position
        double value = gtk_adjustment_get_value(adjustment);

        // Convert scroll position to item index
        // This is approximate - actual implementation would need item height
        // For now, assume each item is roughly 30 pixels
        return (int)(value / 30.0);
    }

    public void SetListTopIndex(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid list handle", nameof(handle));

        if (index < 0)
            index = 0;

        // Get the vertical adjustment from the scrolled window
        IntPtr adjustment = gtk_scrolled_window_get_vadjustment(handle);

        if (adjustment == IntPtr.Zero)
            return;

        // Convert index to scroll position
        // This is approximate - actual implementation would need item height
        // For now, assume each item is roughly 30 pixels
        double value = index * 30.0;

        gtk_adjustment_set_value(adjustment, value);
    }

    // Combo operations - implemented in LinuxPlatform_Combo.cs

    // Group operations
    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        throw new NotImplementedException("Group not yet implemented on Linux platform");
    }

    public void SetGroupText(IntPtr handle, string text)
    {
        throw new NotImplementedException("Group not yet implemented on Linux platform");
    }

    // GTK Drawing Area widget imports for Canvas
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_drawing_area_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_queue_draw(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_queue_draw_area(IntPtr widget, int x, int y, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_override_background_color(IntPtr widget, int state, ref GdkRGBA color);

    // GdkRGBA struct for color representation
    [StructLayout(LayoutKind.Sequential)]
    private struct GdkRGBA
    {
        public double Red;
        public double Green;
        public double Blue;
        public double Alpha;

        public GdkRGBA(Graphics.RGB rgb)
        {
            Red = rgb.Red / 255.0;
            Green = rgb.Green / 255.0;
            Blue = rgb.Blue / 255.0;
            Alpha = 1.0;
        }
    }

    // Canvas data storage
    private class CanvasData
    {
        public GdkRGBA BackgroundColor { get; set; }
        public Action<int, int, int, int, object?>? PaintCallback { get; set; }
    }

    // Track canvas widgets and their data
    private readonly Dictionary<IntPtr, CanvasData> _canvasData = new Dictionary<IntPtr, CanvasData>();
    private readonly Dictionary<GtkSignalFunc, object> _canvasDelegateReferences = new Dictionary<GtkSignalFunc, object>();

    // Canvas operations
    public IntPtr CreateCanvas(IntPtr parent, int style)
    {
        // Create GtkDrawingArea widget
        IntPtr canvas = gtk_drawing_area_new();

        if (canvas == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK drawing area");
        }

        // Initialize canvas data with default white background
        var backgroundColor = new GdkRGBA(new Graphics.RGB(255, 255, 255));
        var canvasData = new CanvasData
        {
            BackgroundColor = backgroundColor
        };
        _canvasData[canvas] = canvasData;

        // Set default background color
        gtk_widget_override_background_color(canvas, 0, ref backgroundColor);

        // Connect "draw" signal for paint events
        GtkSignalFunc drawHandler = (widget, data) =>
        {
            if (_canvasData.TryGetValue(widget, out var canvasInfo) && canvasInfo.PaintCallback != null)
            {
                // Get widget dimensions for full redraw
                // For now, pass dummy values - proper implementation would query widget allocation
                canvasInfo.PaintCallback(0, 0, 0, 0, data);
            }
        };

        // Store delegate reference to prevent GC collection
        _canvasDelegateReferences[drawHandler] = canvas;

        // Connect the "draw" signal
        IntPtr funcPtr = Marshal.GetFunctionPointerForDelegate(drawHandler);
        g_signal_connect_data(canvas, "draw", funcPtr, IntPtr.Zero, IntPtr.Zero, 0);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            gtk_container_add(parent, canvas);
        }

        gtk_widget_show(canvas);

        return canvas;
    }

    public void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid canvas handle", nameof(handle));

        if (!_canvasData.TryGetValue(handle, out var canvasData))
        {
            throw new ArgumentException("Canvas handle not found", nameof(handle));
        }

        // Store the paint callback
        canvasData.PaintCallback = paintCallback;
    }

    public void RedrawCanvas(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid canvas handle", nameof(handle));

        // Queue a full redraw of the entire widget
        gtk_widget_queue_draw(handle);
    }

    public void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid canvas handle", nameof(handle));

        // Queue a partial redraw of the specified area
        gtk_widget_queue_draw_area(handle, x, y, width, height);
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

    // GTK TreeView/ListStore P/Invoke declarations for Table widget
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_new_with_model(IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_store_newv(int n_columns, IntPtr[] types);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_model(IntPtr tree_view, IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_model(IntPtr tree_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_column_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_append_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_remove_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_column(IntPtr tree_view, int n);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_view_column_set_title(IntPtr tree_column, string title);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_column_get_title(IntPtr tree_column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_fixed_width(IntPtr tree_column, int fixed_width);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_column_get_width(IntPtr tree_column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_resizable(IntPtr tree_column, bool resizable);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_reorderable(IntPtr tree_column, bool reorderable);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_set_alignment(IntPtr tree_column, float xalign);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_text_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_toggle_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_view_column_pack_start(IntPtr tree_column, IntPtr cell, bool expand);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_view_column_add_attribute(IntPtr tree_column, IntPtr cell_renderer, string attribute, int column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_append(IntPtr list_store, out GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_insert(IntPtr list_store, out GtkTreeIter iter, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_remove(IntPtr list_store, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_clear(IntPtr list_store);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_list_store_set(IntPtr list_store, ref GtkTreeIter iter, int column, string val, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_store_set_value(IntPtr list_store, ref GtkTreeIter iter, int column, ref GValue value);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_get_iter(IntPtr tree_model, out GtkTreeIter iter, IntPtr path);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_tree_model_iter_nth_child(IntPtr tree_model, out GtkTreeIter iter, IntPtr parent, int n);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_model_iter_n_children(IntPtr tree_model, IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tree_model_get(IntPtr tree_model, ref GtkTreeIter iter, int column, out IntPtr data, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_selection(IntPtr tree_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_set_mode(IntPtr selection, GtkSelectionMode type);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_select_iter(IntPtr selection, ref GtkTreeIter iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_unselect_all(IntPtr selection);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_selection_get_selected_rows(IntPtr selection, out IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_headers_visible(IntPtr tree_view, bool headers_visible);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_set_grid_lines(IntPtr tree_view, GtkTreeViewGridLines grid_lines);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_path_new_from_indices(int index, int terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_scroll_to_cell(IntPtr tree_view, IntPtr path, IntPtr column, bool use_align, float row_align, float col_align);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_path_free(IntPtr path);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_type_from_name(string name);

    // GTK TreeIter structure
    [StructLayout(LayoutKind.Sequential)]
    private struct GtkTreeIter
    {
        public int stamp;
        public IntPtr user_data;
        public IntPtr user_data2;
        public IntPtr user_data3;
    }

    // GValue structure for setting values
    [StructLayout(LayoutKind.Sequential)]
    private struct GValue
    {
        public IntPtr g_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public long[] data;
    }

    // GTK TreeView grid lines enum
    private enum GtkTreeViewGridLines
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = 3
    }

    // Table data storage
    private class TableData
    {
        public IntPtr ScrolledWindow { get; set; }
        public IntPtr TreeView { get; set; }
        public IntPtr ListStore { get; set; }
        public List<IntPtr> Columns { get; } = new List<IntPtr>();
        public List<GtkTreeIter> Rows { get; } = new List<GtkTreeIter>();
        public int Style { get; set; }
        public bool CheckStyle { get; set; }
    }

    // Column data storage
    private class TableColumnData
    {
        public IntPtr TableHandle { get; set; }
        public IntPtr ColumnHandle { get; set; }
        public int ColumnIndex { get; set; }
        public int Alignment { get; set; }
    }

    // Track table widgets and their data
    private readonly Dictionary<IntPtr, TableData> _tableData = new Dictionary<IntPtr, TableData>();
    private readonly Dictionary<IntPtr, TableColumnData> _columnData = new Dictionary<IntPtr, TableColumnData>();
    private readonly Dictionary<IntPtr, (IntPtr tableHandle, int rowIndex)> _tableItemData = new Dictionary<IntPtr, (IntPtr, int)>();

    // Table operations
    public IntPtr CreateTable(IntPtr parent, int style)
    {
        // Create scrolled window to contain the tree view
        IntPtr scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        gtk_scrolled_window_set_policy(scrolledWindow, 1, 1); // GTK_POLICY_AUTOMATIC = 1

        // Create list store initially with no columns (will be added dynamically)
        // Start with just one column to avoid issues
        IntPtr[] types = new IntPtr[] { g_type_from_name("gchararray") };
        IntPtr listStore = gtk_list_store_newv(1, types);

        // Create tree view with the model
        IntPtr treeView = gtk_tree_view_new_with_model(listStore);

        // Add tree view to scrolled window
        gtk_container_add(scrolledWindow, treeView);

        // Configure selection mode
        IntPtr selection = gtk_tree_view_get_selection(treeView);
        if ((style & SWT.MULTI) != 0)
        {
            gtk_tree_selection_set_mode(selection, GtkSelectionMode.Multiple);
        }
        else
        {
            gtk_tree_selection_set_mode(selection, GtkSelectionMode.Single);
        }

        // Set headers visible by default
        gtk_tree_view_set_headers_visible(treeView, true);

        // Show both widgets
        gtk_widget_show(treeView);
        gtk_widget_show(scrolledWindow);

        // Store table data
        var tableData = new TableData
        {
            ScrolledWindow = scrolledWindow,
            TreeView = treeView,
            ListStore = listStore,
            Style = style,
            CheckStyle = (style & SWT.CHECK) != 0
        };
        _tableData[scrolledWindow] = tableData;

        return scrolledWindow;
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        gtk_tree_view_set_headers_visible(tableData.TreeView, visible);
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        gtk_tree_view_set_grid_lines(tableData.TreeView, visible ? GtkTreeViewGridLines.Both : GtkTreeViewGridLines.None);
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        IntPtr selection = gtk_tree_view_get_selection(tableData.TreeView);
        gtk_tree_selection_unselect_all(selection);

        if (indices != null)
        {
            foreach (int index in indices)
            {
                if (gtk_tree_model_iter_nth_child(tableData.ListStore, out GtkTreeIter iter, IntPtr.Zero, index))
                {
                    gtk_tree_selection_select_iter(selection, ref iter);
                }
            }
        }
    }

    public void ClearTableItems(IntPtr handle)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        gtk_list_store_clear(tableData.ListStore);
        tableData.Rows.Clear();
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        if (!_tableData.TryGetValue(handle, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(handle));

        IntPtr path = gtk_tree_path_new_from_indices(index, -1);
        gtk_tree_view_scroll_to_cell(tableData.TreeView, path, IntPtr.Zero, false, 0.0f, 0.0f);
        gtk_tree_path_free(path);
    }

    // TableColumn operations
    public IntPtr CreateTableColumn(IntPtr parent, int style, int index)
    {
        if (!_tableData.TryGetValue(parent, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(parent));

        // Create tree view column
        IntPtr column = gtk_tree_view_column_new();

        // Create cell renderer
        IntPtr cellRenderer = gtk_cell_renderer_text_new();

        // Pack renderer into column
        gtk_tree_view_column_pack_start(column, cellRenderer, true);

        // Determine column index in list store
        int columnIndex = tableData.Columns.Count;

        // Add attribute binding (column index in model)
        gtk_tree_view_column_add_attribute(column, cellRenderer, "text", columnIndex);

        // Set alignment
        float alignment = 0.0f; // Left by default
        if ((style & SWT.CENTER) != 0)
            alignment = 0.5f;
        else if ((style & SWT.RIGHT) != 0)
            alignment = 1.0f;

        gtk_tree_view_column_set_alignment(column, alignment);

        // Make column resizable by default
        gtk_tree_view_column_set_resizable(column, true);

        // Add column to tree view
        gtk_tree_view_append_column(tableData.TreeView, column);

        // Store column
        tableData.Columns.Add(column);

        // Store column data
        var columnData = new TableColumnData
        {
            TableHandle = parent,
            ColumnHandle = column,
            ColumnIndex = columnIndex,
            Alignment = style
        };
        _columnData[column] = columnData;

        return column;
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        if (!_columnData.TryGetValue(handle, out var columnData))
            throw new ArgumentException("Invalid column handle", nameof(handle));

        if (_tableData.TryGetValue(columnData.TableHandle, out var tableData))
        {
            gtk_tree_view_remove_column(tableData.TreeView, handle);
            tableData.Columns.Remove(handle);
        }

        _columnData.Remove(handle);
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        gtk_tree_view_column_set_title(handle, text);
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        gtk_tree_view_column_set_fixed_width(handle, width);
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        float align = 0.0f; // Left
        if ((alignment & SWT.CENTER) != 0)
            align = 0.5f;
        else if ((alignment & SWT.RIGHT) != 0)
            align = 1.0f;

        gtk_tree_view_column_set_alignment(handle, align);

        if (_columnData.TryGetValue(handle, out var columnData))
        {
            columnData.Alignment = alignment;
        }
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        gtk_tree_view_column_set_resizable(handle, resizable);
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        gtk_tree_view_column_set_reorderable(handle, moveable);
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTip)
    {
        // GTK TreeView columns don't support tooltips directly
        // This would require custom implementation with signals
    }

    public int PackTableColumn(IntPtr handle)
    {
        // Auto-size column to content
        // GTK does this automatically, just return current width
        return gtk_tree_view_column_get_width(handle);
    }

    // TableItem operations
    public IntPtr CreateTableItem(IntPtr parent, int style, int index)
    {
        if (!_tableData.TryGetValue(parent, out var tableData))
            throw new ArgumentException("Invalid table handle", nameof(parent));

        GtkTreeIter iter;

        if (index < 0 || index >= tableData.Rows.Count)
        {
            // Append to end
            gtk_list_store_append(tableData.ListStore, out iter);
            tableData.Rows.Add(iter);
            index = tableData.Rows.Count - 1;
        }
        else
        {
            // Insert at specific position
            gtk_list_store_insert(tableData.ListStore, out iter, index);
            tableData.Rows.Insert(index, iter);
        }

        // Create a pseudo-handle for the item (using row index)
        IntPtr itemHandle = new IntPtr(tableData.Rows.Count * 1000 + index);
        _tableItemData[itemHandle] = (parent, index);

        return itemHandle;
    }

    public void DestroyTableItem(IntPtr handle)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            throw new ArgumentException("Invalid table item handle", nameof(handle));

        if (_tableData.TryGetValue(itemData.tableHandle, out var tableData))
        {
            if (itemData.rowIndex >= 0 && itemData.rowIndex < tableData.Rows.Count)
            {
                GtkTreeIter iter = tableData.Rows[itemData.rowIndex];
                gtk_list_store_remove(tableData.ListStore, ref iter);
                tableData.Rows.RemoveAt(itemData.rowIndex);
            }
        }

        _tableItemData.Remove(handle);
    }

    public void SetTableItemText(IntPtr handle, int column, string text)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            throw new ArgumentException("Invalid table item handle", nameof(handle));

        if (!_tableData.TryGetValue(itemData.tableHandle, out var tableData))
            return;

        if (itemData.rowIndex >= 0 && itemData.rowIndex < tableData.Rows.Count)
        {
            GtkTreeIter iter = tableData.Rows[itemData.rowIndex];
            gtk_list_store_set(tableData.ListStore, ref iter, column, text, -1);
        }
    }

    public void SetTableItemImage(IntPtr handle, int column, IntPtr image)
    {
        // GTK implementation would require pixbuf column type
        // Not implemented in this basic version
    }

    public void SetTableItemChecked(IntPtr handle, bool checked_)
    {
        if (!_tableItemData.TryGetValue(handle, out var itemData))
            throw new ArgumentException("Invalid table item handle", nameof(handle));

        if (!_tableData.TryGetValue(itemData.tableHandle, out var tableData))
            return;

        if (!tableData.CheckStyle)
            return;

        // Would require toggle renderer in first column
        // Not fully implemented in this basic version
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        // Would require cell data function for custom rendering
        // Not implemented in this basic version
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        // Would require cell data function for custom rendering
        // Not implemented in this basic version
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        // Would require cell data function for custom rendering
        // Not implemented in this basic version
    }

    // ProgressBar operations

    // GTK ProgressBar widget imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_progress_bar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_set_fraction(IntPtr pbar, double fraction);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_pulse(IntPtr pbar);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_orientable_set_orientation(IntPtr orientable, GtkOrientation orientation);

    // GTK orientation enum
    private enum GtkOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    // Store progress bar range for fraction calculation
    private readonly Dictionary<IntPtr, (int min, int max)> _progressBarRanges = new Dictionary<IntPtr, (int, int)>();

    // Store indeterminate state
    private readonly Dictionary<IntPtr, bool> _progressBarIndeterminate = new Dictionary<IntPtr, bool>();

    public IntPtr CreateProgressBar(IntPtr parent, int style)
    {
        // Create GtkProgressBar
        IntPtr progressBar = gtk_progress_bar_new();

        if (progressBar == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK progress bar");
        }

        // Set orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            gtk_orientable_set_orientation(progressBar, GtkOrientation.Vertical);
        }
        else
        {
            gtk_orientable_set_orientation(progressBar, GtkOrientation.Horizontal);
        }

        // Store indeterminate state
        bool isIndeterminate = (style & SWT.INDETERMINATE) != 0;
        _progressBarIndeterminate[progressBar] = isIndeterminate;

        // Initialize range (default 0-100)
        _progressBarRanges[progressBar] = (0, 100);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            gtk_container_add(parent, progressBar);
        }

        gtk_widget_show(progressBar);

        return progressBar;
    }

    public void SetProgressBarRange(IntPtr handle, int minimum, int maximum)
    {
        if (handle == IntPtr.Zero)
            return;

        // Store the range for fraction calculation
        _progressBarRanges[handle] = (minimum, maximum);

        // If not indeterminate, update the fraction with current value
        if (!_progressBarIndeterminate.TryGetValue(handle, out bool isIndeterminate) || !isIndeterminate)
        {
            // Recalculate fraction with new range (assume current value is minimum for now)
            double fraction = 0.0;
            gtk_progress_bar_set_fraction(handle, fraction);
        }
    }

    public void SetProgressBarSelection(IntPtr handle, int value)
    {
        if (handle == IntPtr.Zero)
            return;

        // Check if indeterminate
        if (_progressBarIndeterminate.TryGetValue(handle, out bool isIndeterminate) && isIndeterminate)
        {
            // For indeterminate progress bars, pulse instead of setting fraction
            gtk_progress_bar_pulse(handle);
            return;
        }

        // Get the range
        if (!_progressBarRanges.TryGetValue(handle, out var range))
        {
            range = (0, 100); // Default range
        }

        int minimum = range.min;
        int maximum = range.max;

        // Calculate fraction (0.0 to 1.0)
        double fraction = 0.0;
        if (maximum > minimum)
        {
            fraction = (double)(value - minimum) / (maximum - minimum);
            // Clamp to 0.0 - 1.0
            fraction = Math.Max(0.0, Math.Min(1.0, fraction));
        }

        gtk_progress_bar_set_fraction(handle, fraction);
    }

    public void SetProgressBarState(IntPtr handle, int state)
    {
        if (handle == IntPtr.Zero)
            return;

        // GTK GtkProgressBar doesn't have built-in state support (normal/error/paused)
        // State colors would need to be implemented via CSS styling
        // For now, this is a no-op on Linux
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

    // GTK File Chooser Dialog imports
    private enum GtkFileChooserAction
    {
        Open = 0,
        Save = 1,
        SelectFolder = 2,
        CreateFolder = 3
    }

    private enum GtkResponseType
    {
        None = -1,
        Reject = -2,
        Accept = -3,
        DeleteEvent = -4,
        OK = -5,
        Cancel = -6,
        Close = -7,
        Yes = -8,
        No = -9,
        Apply = -10,
        Help = -11
    }

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_file_chooser_dialog_new(
        string title,
        IntPtr parent,
        GtkFileChooserAction action,
        string first_button_text,
        GtkResponseType first_button_response,
        string second_button_text,
        GtkResponseType second_button_response,
        IntPtr null_terminator);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_dialog_run(IntPtr dialog);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_chooser_set_current_folder(IntPtr chooser, string filename);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_chooser_set_current_name(IntPtr chooser, string name);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_file_chooser_get_filename(IntPtr chooser);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_file_chooser_get_filenames(IntPtr chooser);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_file_chooser_set_select_multiple(IntPtr chooser, bool select_multiple);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_file_chooser_set_do_overwrite_confirmation(IntPtr chooser, bool do_overwrite_confirmation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_file_filter_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_filter_set_name(IntPtr filter, string name);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_file_filter_add_pattern(IntPtr filter, string pattern);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_file_chooser_add_filter(IntPtr chooser, IntPtr filter);

    // GLib GSList functions
    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_slist_nth_data(IntPtr list, uint n);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint g_slist_length(IntPtr list);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_slist_free(IntPtr list);

    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        throw new NotImplementedException("MessageBox not yet implemented on Linux platform");
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        bool isSave = (style & SWT.SAVE) != 0;
        bool isMulti = (style & SWT.MULTI) != 0;

        GtkFileChooserAction action = isSave ? GtkFileChooserAction.Save : GtkFileChooserAction.Open;

        // Create file chooser dialog
        IntPtr dialog = gtk_file_chooser_dialog_new(
            title ?? "Select File",
            parentHandle,
            action,
            "_Cancel",
            GtkResponseType.Cancel,
            isSave ? "_Save" : "_Open",
            GtkResponseType.Accept,
            IntPtr.Zero);

        if (dialog == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK file chooser dialog");
        }

        try
        {
            // Set initial directory
            if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
            {
                gtk_file_chooser_set_current_folder(dialog, filterPath);
            }

            // Set initial file name
            if (!string.IsNullOrEmpty(fileName))
            {
                if (isSave)
                {
                    gtk_file_chooser_set_current_name(dialog, fileName);
                }
            }

            // Set multi-select
            if (isMulti && !isSave)
            {
                gtk_file_chooser_set_select_multiple(dialog, true);
            }

            // Set overwrite confirmation for save dialogs
            if (isSave && overwrite)
            {
                gtk_file_chooser_set_do_overwrite_confirmation(dialog, true);
            }

            // Add file filters
            if (filterNames != null && filterExtensions != null)
            {
                int count = Math.Min(filterNames.Length, filterExtensions.Length);
                for (int i = 0; i < count; i++)
                {
                    IntPtr filter = gtk_file_filter_new();
                    gtk_file_filter_set_name(filter, filterNames[i]);

                    // Parse extension patterns (e.g., "*.txt", "*.jpg;*.png")
                    string[] patterns = filterExtensions[i].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string pattern in patterns)
                    {
                        gtk_file_filter_add_pattern(filter, pattern.Trim());
                    }

                    gtk_file_chooser_add_filter(dialog, filter);
                }
            }

            // Run dialog
            int response = gtk_dialog_run(dialog);

            if (response != (int)GtkResponseType.Accept)
            {
                return new FileDialogResult
                {
                    SelectedFiles = null,
                    FilterPath = null,
                    FilterIndex = 0
                };
            }

            // Get selected files
            string[] selectedFiles;
            string? resultFilterPath = null;

            if (isMulti)
            {
                // Get multiple files
                IntPtr filesList = gtk_file_chooser_get_filenames(dialog);
                uint length = g_slist_length(filesList);

                if (length > 0)
                {
                    selectedFiles = new string[length];
                    for (uint i = 0; i < length; i++)
                    {
                        IntPtr filePtr = g_slist_nth_data(filesList, i);
#if NETSTANDARD2_0
                        string? file = Marshal.PtrToStringAnsi(filePtr);
#else
                        string? file = Marshal.PtrToStringUTF8(filePtr);
#endif
                        selectedFiles[i] = file ?? string.Empty;

                        // Free the individual string
                        g_free(filePtr);
                    }

                    // Free the list
                    g_slist_free(filesList);

                    // Get directory from first file
                    if (selectedFiles.Length > 0 && !string.IsNullOrEmpty(selectedFiles[0]))
                    {
                        resultFilterPath = Path.GetDirectoryName(selectedFiles[0]);
                    }
                }
                else
                {
                    selectedFiles = Array.Empty<string>();
                }
            }
            else
            {
                // Get single file
                IntPtr filePtr = gtk_file_chooser_get_filename(dialog);
                if (filePtr != IntPtr.Zero)
                {
#if NETSTANDARD2_0
                    string? file = Marshal.PtrToStringAnsi(filePtr);
#else
                    string? file = Marshal.PtrToStringUTF8(filePtr);
#endif
                    if (!string.IsNullOrEmpty(file))
                    {
                        selectedFiles = new[] { file };
                        resultFilterPath = Path.GetDirectoryName(file);
                    }
                    else
                    {
                        selectedFiles = Array.Empty<string>();
                    }

                    g_free(filePtr);
                }
                else
                {
                    selectedFiles = Array.Empty<string>();
                }
            }

            return new FileDialogResult
            {
                SelectedFiles = selectedFiles,
                FilterPath = resultFilterPath,
                FilterIndex = 0 // GTK doesn't provide selected filter index easily
            };
        }
        finally
        {
            gtk_widget_destroy(dialog);
        }
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
