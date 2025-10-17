using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux platform implementation using GTK3 via P/Invoke.
/// </summary>
internal partial class LinuxPlatform : IPlatform
{
    // New platform widget methods (return objects, not handles!)

    public IPlatformWindow CreateWindowWidget(int style, string title)
    {
        return new Linux.LinuxWindow(style, title);
    }

    public IPlatformWidget CreateButtonWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        if (_enableLogging)
            Console.WriteLine($"[Linux] Creating button widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var button = new Linux.LinuxButton(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Linux] Button widget created successfully");

        return button;
    }

    public IPlatformWidget CreateLabelWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        return new Linux.LinuxLabel(parentHandle, style);
    }

    public IPlatformTextInput CreateTextWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        return new Linux.LinuxText(parentHandle, style);
    }

    public IPlatformComposite CreateCompositeWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxComposite in Phase 2
        throw new NotImplementedException("CreateCompositeWidget will be implemented in Phase 2");
    }

    public IPlatformToolBar CreateToolBarWidget(IPlatformWindow parent, int style)
    {
        // Extract parent window handle
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWindow linuxWindow)
        {
            parentHandle = linuxWindow.GetNativeHandle();
        }

        if (_enableLogging)
            Console.WriteLine($"[Linux] Creating toolbar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var toolbar = new SWTSharp.Platform.Linux.LinuxToolBar(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Linux] Toolbar widget created successfully");

        return toolbar;
    }

    // Advanced widget factory methods for Phase 5.3
    public IPlatformCombo CreateComboWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxCombo in Phase 5.3
        throw new NotImplementedException("CreateComboWidget will be implemented in Phase 5.3");
    }

    public IPlatformList CreateListWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxList in Phase 5.3
        throw new NotImplementedException("CreateListWidget will be implemented in Phase 5.3");
    }

    public IPlatformProgressBar CreateProgressBarWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxProgressBar in Phase 5.3
        throw new NotImplementedException("CreateProgressBarWidget will be implemented in Phase 5.3");
    }

    public IPlatformSlider CreateSliderWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxSlider in Phase 5.3
        throw new NotImplementedException("CreateSliderWidget will be implemented in Phase 5.3");
    }

    public IPlatformScale CreateScaleWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxScale in Phase 5.3
        throw new NotImplementedException("CreateScaleWidget will be implemented in Phase 5.3");
    }

    public IPlatformSpinner CreateSpinnerWidget(IPlatformWidget? parent, int style)
    {
        // TODO: Implement LinuxSpinner in Phase 5.3
        throw new NotImplementedException("CreateSpinnerWidget will be implemented in Phase 5.3");
    }
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

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern bool g_type_check_instance_is_a(IntPtr instance, IntPtr gtype);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_type();

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
    private static readonly bool _enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

    // Window container mapping - GtkWindow -> container widget
    // This is needed for window management but will be removed when windows migrate to platform widget interfaces
    private readonly Dictionary<IntPtr, IntPtr> _widgetContainers = new Dictionary<IntPtr, IntPtr>();

    public void Initialize()
    {
        if (_initialized)
        {
            if (_enableLogging)
                Console.WriteLine("[Linux] GTK already initialized");
            return;
        }

        if (_enableLogging)
            Console.WriteLine("[Linux] Initializing GTK");

        int argc = 0;
        IntPtr argv = IntPtr.Zero;

        if (!gtk_init_check(ref argc, ref argv))
        {
            if (_enableLogging)
                Console.WriteLine("[Linux] GTK initialization FAILED");
            throw new InvalidOperationException("Failed to initialize GTK");
        }

        _initialized = true;

        if (_enableLogging)
            Console.WriteLine("[Linux] GTK initialized successfully");
    }

    public IntPtr CreateWindow(int style, string title)
    {
        if (_enableLogging)
            Console.WriteLine($"[Linux] Creating window. Style: 0x{style:X}, Title: '{title}'");

        // Create a top-level window
        IntPtr window = gtk_window_new(GtkWindowType.Toplevel);

        if (window == IntPtr.Zero)
        {
            if (_enableLogging)
                Console.WriteLine("[Linux] Window creation FAILED - gtk_window_new returned NULL");
            throw new InvalidOperationException("Failed to create GTK window");
        }

        if (_enableLogging)
            Console.WriteLine($"[Linux] GtkWindow created: 0x{window:X}");

        // Set window properties
        gtk_window_set_title(window, title);
        gtk_window_set_default_size(window, 800, 600);

        // CRITICAL: GtkWindow is a GtkBin and can only contain ONE child widget.
        // Create a GtkFixed container to hold multiple widgets, like NSWindow's contentView.
        IntPtr container = gtk_fixed_new();
        if (container != IntPtr.Zero)
        {
            if (_enableLogging)
                Console.WriteLine($"[Linux] GtkFixed container created: 0x{container:X}");

            gtk_container_add(window, container);
            gtk_widget_show(container);

            // Map window -> container for child widget additions
            _widgetContainers[window] = container;

            if (_enableLogging)
                Console.WriteLine($"[Linux] Container added to window");
        }
        else
        {
            if (_enableLogging)
                Console.WriteLine("[Linux] WARNING: Container creation failed");
        }

        // Connect destroy signal to quit application when window is closed
        // For now, we'll skip signal handling to keep it simple
        // In production, you'd want: g_signal_connect_data(window, "destroy", callback, ...)

        return window;
    }

    /// <summary>
    /// Safely destroy a GTK widget with proper type validation.
    /// </summary>
    private void SafeDestroyWidget(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Validate that this is actually a GtkWidget before destroying
        IntPtr widgetType = gtk_widget_get_type();
        if (widgetType != IntPtr.Zero && g_type_check_instance_is_a(handle, widgetType))
        {
            gtk_widget_destroy(handle);
        }
    }

    /// <summary>
    /// Helper to add a child widget to parent, handling GtkWindow -> container mapping.
    /// GtkWindow can only contain one child, so we use the container we created.
    /// </summary>
    private void AddChildToParent(IntPtr parent, IntPtr child)
    {
        if (parent == IntPtr.Zero) return;

        // Check if parent is a window with a container
        if (_widgetContainers.TryGetValue(parent, out IntPtr container))
        {
            // Parent is a GtkWindow, add to its container
            gtk_container_add(container, child);
        }
        else
        {
            // Parent is a regular container widget
            gtk_container_add(parent, child);
        }
    }

    public void DestroyWindow(IntPtr handle)
    {
        SafeDestroyWidget(handle);
        _widgetContainers.Remove(handle);
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
        // On Linux/GTK, we don't use gtk_main() so gtk_main_quit() is invalid
        // The gtk_main_iteration_do(true) in WaitForEvent() will naturally wake up
        // when events are posted via AsyncExec -> Wake() -> ProcessEvent() cycle
        // No explicit wake mechanism needed - GTK handles this internally
    }

    public void ExecuteOnMainThread(Action action)
    {
        // On Linux/GTK, there's no separate "main thread" requirement like macOS
        // GTK is generally flexible with threading, so just execute directly
        action();
    }

    // CreateComposite - REMOVED: Now handled by CreateCompositeWidget() method
    // This method was removed as part of the platform widget interface migration
    // Use CreateCompositeWidget() instead which returns IPlatformComposite objects

    // Button creation and management - REMOVED: Now handled by platform widget interfaces
    // The following DllImport declarations were removed as part of the platform widget interface migration:
    // - gtk_button_new_with_label
    // - gtk_button_new
    // - gtk_button_set_label
    // - gtk_button_get_label
    // - gtk_check_button_new_with_label
    // - gtk_radio_button_new_with_label
    // - gtk_toggle_button_new_with_label
    // - gtk_toggle_button_set_active
    // - gtk_toggle_button_get_active
    // These are now used in the LinuxButton widget class

    // Text/Entry DllImport declarations - NEEDED by other partial class files
    // These are still used by LinuxPlatform_Combo.cs and other widget files
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

    // Scrolled window DllImport declarations - NEEDED by other partial class files
    // These are still used by LinuxPlatform_List.cs, LinuxPlatform_Table.cs, etc.
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolled_window, int hscrollbar_policy, int vscrollbar_policy);

    // Orientation DllImport declarations - NEEDED by other partial class files
    // These are still used by LinuxPlatform_ToolBar.cs, LinuxPlatform_ProgressBar.cs, etc.
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_orientable_set_orientation(IntPtr orientable, GtkOrientation orientation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_box_new(GtkOrientation orientation, int spacing);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_fixed_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_put(IntPtr @fixed, IntPtr widget, int x, int y);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_fixed_move(IntPtr @fixed, IntPtr widget, int x, int y);

    // GtkSignalFunc delegate - REMOVED from main class but still needed by other partial classes
    // Note: This delegate is still used by other partial class files (LinuxPlatform_Canvas.cs, etc.)
    // It will be fully removed when all widget classes migrate to the new platform widget interfaces
    private delegate void GtkSignalFunc(IntPtr widget, IntPtr data);

    // Button callback management - REMOVED: Now handled by platform widget interfaces
    // The following members were removed as part of the platform widget interface migration:
    // - _buttonCallbacks dictionary
    // - _delegateReferences dictionary
    // - ClearButtonCallbacks() method
    // - RemoveButtonCallback() method
    // Button event handling is now managed by individual widget classes

      // CreateButton - REMOVED: Now handled by CreateButtonWidget() method
    // SetButtonText - REMOVED: Now handled by IPlatformTextWidget interface
    // SetButtonSelection - REMOVED: Now handled by IPlatformTextWidget interface
    // GetButtonSelection - REMOVED: Now handled by IPlatformTextWidget interface
    // These methods were removed as part of the platform widget interface migration
    // Use CreateButtonWidget() instead which returns IPlatformWidget objects

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

    // ConnectButtonClick - REMOVED: Now handled by IPlatformTextWidget interface
    // This method was removed as part of the platform widget interface migration
    // Button events are now handled through the platform widget interface event system

    // Menu operations - REMOVED: Now handled by platform widget interfaces
    // The following DllImport declarations were removed as part of the platform widget interface migration:
    // - gtk_menu_bar_new
    // - gtk_menu_new
    // - gtk_menu_item_new
    // - gtk_menu_item_new_with_label
    // - gtk_check_menu_item_new_with_label
    // - gtk_radio_menu_item_new_with_label
    // - gtk_separator_menu_item_new
    // - gtk_menu_shell_append
    // - gtk_menu_shell_insert
    // - gtk_menu_item_set_label
    // - gtk_menu_item_set_submenu
    // - gtk_check_menu_item_set_active
    // - gtk_menu_popup_at_pointer
    // Menu creation and management is now handled by individual menu widget classes

    // Label operations - implemented in LinuxPlatform_Label.cs

    // Text/Entry control operations - REMOVED: Now handled by platform widget interfaces
    // The following methods were removed as part of the platform widget interface migration:
    // - CreateText (now handled by CreateTextWidget method)
    // - SetTextContent, GetTextContent (now handled by IPlatformTextWidget interface)
    // - SetTextSelection, GetTextSelection (now handled by IPlatformTextWidget interface)
    // - SetTextLimit, SetTextReadOnly (now handled by IPlatformTextWidget interface)
    // The following members were also removed:
    // - _textWidgetTypes dictionary
    // - _textViewWidgets dictionary
    // Text widgets are now created through platform-specific widget classes


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

    // ProgressBar control operations - REMOVED: Now handled by platform widget interfaces
    // The following methods were removed as part of the platform widget interface migration:
    // - CreateProgressBar (now handled by CreateProgressBarWidget method)
    // - SetProgressBarRange, SetProgressBarSelection (now handled by IPlatformProgressBar interface)
    // - SetProgressBarState (now handled by IPlatformProgressBar interface)
    // The following DllImport declarations were also removed (moved to individual widget classes):
    // - gtk_progress_bar_new, gtk_progress_bar_set_fraction, gtk_progress_bar_set_pulse_step
    // - gtk_progress_bar_pulse
    // The following member was also removed:
    // - _progressBarRanges dictionary
    // ProgressBar widgets are now created through platform-specific widget classes

#if NETSTANDARD2_0
    /// <summary>
    /// Helper method to convert UTF-8 pointer to string for .NET Standard 2.0.
    /// Marshal.PtrToStringUTF8 was added in .NET Core 2.1+
    /// </summary>
    private static string GetUtf8String(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

        // Find null terminator
        int length = 0;
        unsafe
        {
            byte* p = (byte*)ptr;
            while (p[length] != 0)
                length++;
        }

        if (length == 0)
            return string.Empty;

        // Copy to managed array and decode
        byte[] bytes = new byte[length];
        Marshal.Copy(ptr, bytes, 0, length);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
#endif
}
