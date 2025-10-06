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
    private const string GObjectLib = "libgobject-2.0.so.0";

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
    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern ulong g_signal_connect_data(
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string detailed_signal,
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

    private enum GtkOrientation
    {
        Horizontal = 0,
        Vertical = 1
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
    private static extern IntPtr gtk_box_new(GtkOrientation orientation, int spacing);

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

    // ========================================================================
    // Widget implementations are in separate partial class files:
    // - LinuxPlatform_List.cs: List widget (GtkListBox)
    // - LinuxPlatform_Group.cs: Group widget (GtkFrame)
    // - LinuxPlatform_Canvas.cs: Canvas widget (GtkDrawingArea)
    // - LinuxPlatform_Slider.cs: Slider widget (GtkScale)
    // - LinuxPlatform_Scale.cs: Scale widget (GtkScale with marks)
    // - LinuxPlatform_Spinner.cs: Spinner widget (GtkSpinButton)
    // - LinuxPlatform_TabFolder.cs: TabFolder widget (GtkNotebook)
    // - LinuxPlatform_Tree.cs: Tree widget (GtkTreeView)
    // - LinuxPlatform_Table.cs: Table widget (GtkTreeView multi-column)
    // - LinuxPlatform_Dialogs.cs: Dialogs (MessageBox, FileDialog, etc.)
    // - LinuxPlatform_Label.cs: Label widget (GtkLabel)
    // - LinuxPlatform_Combo.cs: Combo widget (GtkComboBox)
    // ========================================================================

    // GTK ProgressBar imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_progress_bar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_set_fraction(IntPtr pbar, double fraction);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_set_pulse_step(IntPtr pbar, double fraction);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_progress_bar_pulse(IntPtr pbar);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_orientable_set_orientation(IntPtr orientable, GtkOrientation orientation);

    // ProgressBar data storage
    private readonly Dictionary<IntPtr, (int Min, int Max)> _progressBarRanges = new Dictionary<IntPtr, (int, int)>();

    // ProgressBar operations
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

        // Initialize range
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

        _progressBarRanges[handle] = (minimum, maximum);

        // If we have a current value, update the fraction to reflect new range
        // (GTK uses 0.0 to 1.0 fraction rather than absolute values)
    }

    public void SetProgressBarSelection(IntPtr handle, int value)
    {
        if (handle == IntPtr.Zero)
            return;

        // Get range for this progress bar
        if (_progressBarRanges.TryGetValue(handle, out var range))
        {
            int min = range.Min;
            int max = range.Max;
            int span = max - min;

            if (span > 0)
            {
                double fraction = (double)(value - min) / span;
                fraction = Math.Max(0.0, Math.Min(1.0, fraction)); // Clamp to 0-1
                gtk_progress_bar_set_fraction(handle, fraction);
            }
        }
    }

    public void SetProgressBarState(IntPtr handle, int state)
    {
        if (handle == IntPtr.Zero)
            return;

        // GTK doesn't have built-in error/paused states like Windows
        // We could change colors using CSS, but for now just ignore
        // This matches the behavior on platforms that don't support this feature
    }
}
