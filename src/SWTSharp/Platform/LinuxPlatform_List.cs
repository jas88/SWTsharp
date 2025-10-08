using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - List widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK List Box widget imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_insert(IntPtr box, IntPtr child, int position);

    // Note: gtk_list_box_remove was added in GTK 4.0, use gtk_container_remove for GTK3
    // [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    // private static extern void gtk_list_box_remove(IntPtr box, IntPtr child);

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

    // GLib list functions
    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint g_list_length(IntPtr list);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_list_nth_data(IntPtr list, uint n);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_list_free(IntPtr list);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_set_value(IntPtr adjustment, double value);

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

        // Add to parent if provided (use helper to handle GtkWindow containers)
        if (parentHandle != IntPtr.Zero)
        {
            AddChildToParent(parentHandle, scrolledWindow);
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

        // Remove from the list box (use gtk_container_remove for GTK3)
        gtk_container_remove(listBox, row);

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

        // Remove all rows from the list box (use gtk_container_remove for GTK3)
        for (int i = rows.Count - 1; i >= 0; i--)
        {
            IntPtr row = rows[i];
            gtk_container_remove(listBox, row);
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
}
