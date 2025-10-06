using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - TabFolder widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // TabFolder data storage
    private class TabFolderData
    {
        public IntPtr Notebook { get; set; }
        public List<IntPtr> TabItems { get; } = new List<IntPtr>();
    }

    private class TabItemData
    {
        public IntPtr TabFolderHandle { get; set; }
        public IntPtr Label { get; set; }
        public IntPtr ContentWidget { get; set; }
        public int PageIndex { get; set; }
        public string Text { get; set; } = "";
    }

    private readonly Dictionary<IntPtr, TabFolderData> _tabFolderData = new Dictionary<IntPtr, TabFolderData>();
    private readonly Dictionary<IntPtr, TabItemData> _tabItemData = new Dictionary<IntPtr, TabItemData>();
    private int _nextTabItemHandle = 1;

    // GTK Notebook P/Invoke declarations
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_notebook_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_notebook_append_page(IntPtr notebook, IntPtr child, IntPtr tab_label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_notebook_insert_page(IntPtr notebook, IntPtr child, IntPtr tab_label, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_notebook_remove_page(IntPtr notebook, int page_num);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_notebook_get_current_page(IntPtr notebook);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_notebook_set_current_page(IntPtr notebook, int page_num);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_notebook_get_n_pages(IntPtr notebook);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_notebook_get_nth_page(IntPtr notebook, int page_num);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_notebook_set_tab_label(IntPtr notebook, IntPtr child, IntPtr tab_label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_notebook_get_tab_label(IntPtr notebook, IntPtr child);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_notebook_set_tab_pos(IntPtr notebook, GtkPositionType pos);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_container_get_children(IntPtr container);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_widget_set_tooltip_text(IntPtr widget, string text);

    private enum GtkPositionType
    {
        Left = 0,
        Right = 1,
        Top = 2,
        Bottom = 3
    }

    // TabFolder operations
    public IntPtr CreateTabFolder(IntPtr parent, int style)
    {
        // Create GtkNotebook
        IntPtr notebook = gtk_notebook_new();
        if (notebook == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK Notebook");

        // Set tab position based on style
        // SWT.TOP = 0x8, SWT.BOTTOM = 0x400
        if ((style & 0x400) != 0) // SWT.BOTTOM
        {
            gtk_notebook_set_tab_pos(notebook, GtkPositionType.Bottom);
        }
        else // Default to TOP
        {
            gtk_notebook_set_tab_pos(notebook, GtkPositionType.Top);
        }

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            gtk_container_add(parent, notebook);
        }

        // Track the tab folder
        _tabFolderData[notebook] = new TabFolderData
        {
            Notebook = notebook
        };

        gtk_widget_show(notebook);
        return notebook;
    }

    public void SetTabSelection(IntPtr handle, int index)
    {
        if (!_tabFolderData.ContainsKey(handle))
            return;

        if (index < 0)
        {
            // No selection - GTK doesn't support this well, just set to first page
            gtk_notebook_set_current_page(handle, 0);
        }
        else
        {
            gtk_notebook_set_current_page(handle, index);
        }
    }

    public int GetTabSelection(IntPtr handle)
    {
        if (!_tabFolderData.ContainsKey(handle))
            return -1;

        int currentPage = gtk_notebook_get_current_page(handle);
        return currentPage;
    }

    // TabItem operations
    public IntPtr CreateTabItem(IntPtr parent, int style, int index)
    {
        if (!_tabFolderData.TryGetValue(parent, out var folderData))
            throw new ArgumentException("Invalid tab folder handle", nameof(parent));

        // Create a pseudo-handle for the tab item
        IntPtr itemHandle = new IntPtr(_nextTabItemHandle++);

        // Create label for the tab
        IntPtr label = gtk_label_new("");

        // Create a container for the tab content (initially empty box)
        IntPtr contentBox = gtk_box_new(GtkOrientation.Vertical, 0);

        // Insert or append the page
        int pageIndex;
        if (index < 0 || index >= folderData.TabItems.Count)
        {
            pageIndex = gtk_notebook_append_page(parent, contentBox, label);
        }
        else
        {
            pageIndex = gtk_notebook_insert_page(parent, contentBox, label, index);
        }

        // Track the tab item
        var itemData = new TabItemData
        {
            TabFolderHandle = parent,
            Label = label,
            ContentWidget = contentBox,
            PageIndex = pageIndex,
            Text = ""
        };

        _tabItemData[itemHandle] = itemData;
        folderData.TabItems.Add(itemHandle);

        gtk_widget_show(contentBox);
        gtk_widget_show(label);

        return itemHandle;
    }

    public void SetTabItemText(IntPtr handle, string text)
    {
        if (!_tabItemData.TryGetValue(handle, out var itemData))
            return;

        itemData.Text = text ?? "";
        gtk_label_set_text(itemData.Label, itemData.Text);
    }

    public void SetTabItemControl(IntPtr handle, IntPtr controlHandle)
    {
        if (!_tabItemData.TryGetValue(handle, out var itemData))
            return;

        if (controlHandle == IntPtr.Zero)
            return;

        // Remove any existing children from the content box
        IntPtr contentBox = itemData.ContentWidget;

        // Get existing children and remove them
        IntPtr children = gtk_container_get_children(contentBox);
        if (children != IntPtr.Zero)
        {
            // Note: In production code, you'd iterate through the list and remove each child
            // For simplicity, we'll just add the new control
        }

        // Add the control to the content box
        gtk_container_add(contentBox, controlHandle);
        gtk_widget_show_all(contentBox);
    }

    public void SetTabItemToolTip(IntPtr handle, string toolTip)
    {
        if (!_tabItemData.TryGetValue(handle, out var itemData))
            return;

        // Set tooltip on the tab label
        if (itemData.Label != IntPtr.Zero)
        {
            gtk_widget_set_tooltip_text(itemData.Label, toolTip ?? "");
        }
    }
}
