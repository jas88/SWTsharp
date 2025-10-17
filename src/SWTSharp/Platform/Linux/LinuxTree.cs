using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK3 implementation of Tree widget using GtkTreeView with GtkTreeStore.
/// GtkTreeView displays hierarchical tree data with support for single/multi selection and checkboxes.
/// </summary>
internal class LinuxTree : LinuxWidget, IPlatformComposite
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";

    private IntPtr _treeView;
    private IntPtr _treeStore;
    private readonly int _style;
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private readonly bool _multiSelect;
    private readonly bool _check;

    // Events required by IPlatformContainerEvents
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    #pragma warning restore CS0067

    public LinuxTree(IntPtr parentHandle, int style)
    {
        _style = style;
        _multiSelect = (style & SWT.MULTI) != 0;
        _check = (style & SWT.CHECK) != 0;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxTree] Creating tree. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create tree store with 2 columns: text (string) and checked state (boolean)
        // Column 0: G_TYPE_STRING (text), Column 1: G_TYPE_BOOLEAN (checked state)
        IntPtr stringType = g_type_from_name("gchararray");
        IntPtr booleanType = g_type_from_name("gboolean");

        _treeStore = gtk_tree_store_new(2, stringType, booleanType);
        if (_treeStore == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK tree store");
        }

        // Create tree view
        _treeView = gtk_tree_view_new_with_model(_treeStore);
        if (_treeView == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK tree view");
        }

        // Create text column
        IntPtr textRenderer = gtk_cell_renderer_text_new();
        IntPtr textColumn = gtk_tree_view_column_new();
        gtk_tree_view_column_pack_start(textColumn, textRenderer, true);
        gtk_tree_view_column_add_attribute(textColumn, textRenderer, "text", 0);
        gtk_tree_view_append_column(_treeView, textColumn);

        // Add checkbox column if CHECK style is set
        if (_check)
        {
            IntPtr checkRenderer = gtk_cell_renderer_toggle_new();
            IntPtr checkColumn = gtk_tree_view_column_new();
            gtk_tree_view_column_pack_start(checkColumn, checkRenderer, false);
            gtk_tree_view_column_add_attribute(checkColumn, checkRenderer, "active", 1);
            gtk_tree_view_insert_column(_treeView, checkColumn, 0); // Insert at front
        }

        // Configure selection mode
        IntPtr selection = gtk_tree_view_get_selection(_treeView);
        int selectionMode = _multiSelect ? GTK_SELECTION_MULTIPLE : GTK_SELECTION_SINGLE;
        gtk_tree_selection_set_mode(selection, selectionMode);

        // Create scrolled window
        IntPtr scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        gtk_scrolled_window_set_policy(scrolledWindow, GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);
        gtk_container_add(scrolledWindow, _treeView);

        // Add to parent
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, scrolledWindow);
        }

        // Show widgets
        gtk_widget_show(_treeView);
        gtk_widget_show(scrolledWindow);

        if (enableLogging)
            Console.WriteLine($"[LinuxTree] Tree created successfully. TreeView: 0x{_treeView:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _treeView;
    }

    public void AddChild(IPlatformWidget child)
    {
        // Tree items are data, not child widgets - no-op
    }

    public void RemoveChild(IPlatformWidget child)
    {
        // Tree items are data, not child widgets - no-op
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return Array.Empty<IPlatformWidget>();
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _treeView == IntPtr.Zero) return;

        gtk_widget_set_size_request(_treeView, width, height);
        LayoutRequested?.Invoke(this, EventArgs.Empty);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _treeView == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_treeView, out allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _treeView == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_treeView);
        else
            gtk_widget_hide(_treeView);
    }

    public bool GetVisible()
    {
        if (_disposed || _treeView == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_treeView);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _treeView == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_treeView, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _treeView == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_treeView);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // TODO: Implement background color via CSS provider
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // TODO: Implement foreground color via CSS provider
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_treeView != IntPtr.Zero)
        {
            gtk_widget_destroy(_treeView);
            _treeView = IntPtr.Zero;
        }

        if (_treeStore != IntPtr.Zero)
        {
            g_object_unref(_treeStore);
            _treeStore = IntPtr.Zero;
        }
    }

    // GTK3 P/Invoke declarations

    private const int GTK_SELECTION_SINGLE = 1;
    private const int GTK_SELECTION_MULTIPLE = 3;
    private const int GTK_POLICY_AUTOMATIC = 1;

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_new_with_model(IntPtr model);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_store_new(int n_columns, params IntPtr[] types);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_column_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_text_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_cell_renderer_toggle_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_view_column_pack_start(IntPtr column, IntPtr renderer, bool expand);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void gtk_tree_view_column_add_attribute(IntPtr column, IntPtr renderer, string attribute, int column_index);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_append_column(IntPtr tree_view, IntPtr column);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_tree_view_insert_column(IntPtr tree_view, IntPtr column, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_tree_view_get_selection(IntPtr tree_view);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tree_selection_set_mode(IntPtr selection, int type);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolled_window, int hscrollbar_policy, int vscrollbar_policy);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern IntPtr g_type_from_name(string name);

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(IntPtr @object);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
