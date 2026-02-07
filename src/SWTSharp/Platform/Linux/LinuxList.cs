using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of IPlatformList using GtkListBox wrapped in GtkScrolledWindow.
/// </summary>
internal class LinuxList : LinuxWidget, IPlatformList
{
    private const string GtkLib = "libgtk-3.so.0";
    private const string GObjectLib = "libgobject-2.0.so.0";
    private const string GLibLib = "libglib-2.0.so.0";

    private readonly IntPtr _scrolledWindow;
    private readonly IntPtr _listBox;
    private readonly int _style;
    private readonly List<string> _items = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);

    // Static mapping for callback routing
    private static readonly ConcurrentDictionary<IntPtr, LinuxList> _listInstances = new();

    // Callback delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RowSelectedFunc(IntPtr listBox, IntPtr row, IntPtr data);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RowActivatedFunc(IntPtr listBox, IntPtr row, IntPtr data);

    private readonly RowSelectedFunc _rowSelectedCallback;
    private readonly RowActivatedFunc _rowActivatedCallback;

    // Events from IPlatformSelectionEvents
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;

    // Events from IPlatformEventHandling
#pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxList(IntPtr parentHandle, int style)
    {
        _style = style;
        _rowSelectedCallback = OnRowSelectedCallback;
        _rowActivatedCallback = OnRowActivatedCallback;

        // Create GtkListBox
        _listBox = gtk_list_box_new();
        if (_listBox == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK list box");
        }

        // Set selection mode based on style
        int selectionMode = (style & SWT.MULTI) != 0
            ? GTK_SELECTION_MULTIPLE
            : GTK_SELECTION_SINGLE;
        gtk_list_box_set_selection_mode(_listBox, selectionMode);

        // Create scrolled window to contain the list box
        _scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        if (_scrolledWindow == IntPtr.Zero)
        {
            gtk_widget_destroy(_listBox);
            throw new InvalidOperationException("Failed to create GTK scrolled window");
        }

        // Set scroll policy: automatic for both horizontal and vertical
        const int GTK_POLICY_AUTOMATIC = 1;
        gtk_scrolled_window_set_policy(_scrolledWindow, GTK_POLICY_AUTOMATIC, GTK_POLICY_AUTOMATIC);

        // Add list box to scrolled window
        gtk_container_add(_scrolledWindow, _listBox);
        gtk_widget_show(_listBox);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _scrolledWindow);
        }

        gtk_widget_show(_scrolledWindow);

        // Setup event handlers
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        if (_disposed || _listBox == IntPtr.Zero) return;

        // Register this list instance for callback routing
        _listInstances[_listBox] = this;

        // Connect row-selected signal for SelectionChanged event
        g_signal_connect_data(
            _listBox,
            "row-selected",
            Marshal.GetFunctionPointerForDelegate(_rowSelectedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );

        // Connect row-activated signal for ItemDoubleClick event (triggered by double-click or Enter key)
        g_signal_connect_data(
            _listBox,
            "row-activated",
            Marshal.GetFunctionPointerForDelegate(_rowActivatedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0
        );
    }

    private void OnRowSelectedCallback(IntPtr listBox, IntPtr row, IntPtr data)
    {
        if (_listInstances.TryGetValue(listBox, out var list) && list == this && !_disposed)
        {
            int index = row != IntPtr.Zero ? gtk_list_box_row_get_index(row) : -1;
            SelectionChanged?.Invoke(this, index);
        }
    }

    private void OnRowActivatedCallback(IntPtr listBox, IntPtr row, IntPtr data)
    {
        if (_listInstances.TryGetValue(listBox, out var list) && list == this && !_disposed && row != IntPtr.Zero)
        {
            int index = gtk_list_box_row_get_index(row);
            ItemDoubleClick?.Invoke(this, index);
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _scrolledWindow;
    }

    #region IPlatformList Implementation

    public void AddItem(string item)
    {
        if (_disposed || _listBox == IntPtr.Zero) return;

        _items.Add(item ?? string.Empty);

        // Create a label widget for the item
        IntPtr label = gtk_label_new(item ?? string.Empty);
        if (label == IntPtr.Zero)
        {
            _items.RemoveAt(_items.Count - 1);
            throw new InvalidOperationException("Failed to create label for list item");
        }

        gtk_widget_show(label);

        // Insert at the end (-1 means append)
        gtk_list_box_insert(_listBox, label, -1);
    }

    public void ClearItems()
    {
        if (_disposed || _listBox == IntPtr.Zero) return;

        // Remove all rows in reverse order
        for (int i = _items.Count - 1; i >= 0; i--)
        {
            IntPtr row = gtk_list_box_get_row_at_index(_listBox, i);
            if (row != IntPtr.Zero)
            {
                gtk_container_remove(_listBox, row);
            }
        }

        _items.Clear();
    }

    public int GetItemCount()
    {
        return _items.Count;
    }

    public string GetItemAt(int index)
    {
        if (index < 0 || index >= _items.Count)
            return string.Empty;

        return _items[index];
    }

    public int[] SelectionIndices
    {
        get
        {
            if (_disposed || _listBox == IntPtr.Zero)
                return Array.Empty<int>();

            // Get selected rows
            IntPtr selectedRowsList = gtk_list_box_get_selected_rows(_listBox);
            if (selectedRowsList == IntPtr.Zero)
                return Array.Empty<int>();

            try
            {
                uint length = g_list_length(selectedRowsList);
                if (length == 0)
                    return Array.Empty<int>();

                int[] indices = new int[length];
                for (uint i = 0; i < length; i++)
                {
                    IntPtr rowPtr = g_list_nth_data(selectedRowsList, i);
                    if (rowPtr != IntPtr.Zero)
                    {
                        indices[i] = gtk_list_box_row_get_index(rowPtr);
                    }
                }

                return indices;
            }
            finally
            {
                g_list_free(selectedRowsList);
            }
        }
        set
        {
            if (_disposed || _listBox == IntPtr.Zero) return;

            // Clear current selection
            gtk_list_box_unselect_all(_listBox);

            // Select specified indices
            if (value != null)
            {
                foreach (int index in value)
                {
                    if (index >= 0 && index < _items.Count)
                    {
                        IntPtr row = gtk_list_box_get_row_at_index(_listBox, index);
                        if (row != IntPtr.Zero)
                        {
                            gtk_list_box_select_row(_listBox, row);
                        }
                    }
                }
            }
        }
    }

    public int SelectionIndex
    {
        get
        {
            if (_disposed || _listBox == IntPtr.Zero)
                return -1;

            IntPtr selectedRow = gtk_list_box_get_selected_row(_listBox);
            if (selectedRow == IntPtr.Zero)
                return -1;

            return gtk_list_box_row_get_index(selectedRow);
        }
        set
        {
            if (_disposed || _listBox == IntPtr.Zero) return;

            // Clear current selection
            gtk_list_box_unselect_all(_listBox);

            // Select the specified index
            if (value >= 0 && value < _items.Count)
            {
                IntPtr row = gtk_list_box_get_row_at_index(_listBox, value);
                if (row != IntPtr.Zero)
                {
                    gtk_list_box_select_row(_listBox, row);
                }
            }
        }
    }

    public int GetTopIndex()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero || _listBox == IntPtr.Zero)
            return 0;

        // Get vertical adjustment to determine scroll position
        IntPtr vadj = gtk_scrolled_window_get_vadjustment(_scrolledWindow);
        if (vadj == IntPtr.Zero) return 0;

        double scrollPos = gtk_adjustment_get_value(vadj);

        // Find which row is at the top by checking each row's allocation
        for (int i = 0; i < _items.Count; i++)
        {
            IntPtr row = gtk_list_box_get_row_at_index(_listBox, i);
            if (row != IntPtr.Zero)
            {
                GtkAllocation allocation;
                gtk_widget_get_allocation(row, out allocation);
                // If the row's top is at or below scroll position, this is likely the top visible row
                if (allocation.y >= scrollPos)
                    return i;
            }
        }

        return 0;
    }

    public void SetTopIndex(int index)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero || _listBox == IntPtr.Zero) return;
        if (index < 0 || index >= _items.Count) return;

        // Get the row at the index
        IntPtr row = gtk_list_box_get_row_at_index(_listBox, index);
        if (row == IntPtr.Zero) return;

        // Get the row's allocation to find its Y position
        GtkAllocation allocation;
        gtk_widget_get_allocation(row, out allocation);

        // Scroll to the row's Y position
        IntPtr vadj = gtk_scrolled_window_get_vadjustment(_scrolledWindow);
        if (vadj != IntPtr.Zero)
        {
            gtk_adjustment_set_value(vadj, allocation.y);
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return;

        gtk_widget_set_size_request(_scrolledWindow, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_scrolledWindow, out allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return;

        if (visible)
            gtk_widget_show(_scrolledWindow);
        else
            gtk_widget_hide(_scrolledWindow);
    }

    public bool GetVisible()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return false;

        return gtk_widget_get_visible(_scrolledWindow);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return;

        gtk_widget_set_sensitive(_scrolledWindow, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return false;

        return gtk_widget_get_sensitive(_scrolledWindow);
    }

    public void SetBackground(RGB color)
    {
        _background = color;
        // Note: GTK3 theming makes background color complex
        // Would need CSS provider for proper implementation
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
        // Note: GTK3 theming makes foreground color complex
    }

    public RGB GetForeground()
    {
        return _foreground;
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        DetachFromParent();

        // Remove from instance mapping
        if (_listBox != IntPtr.Zero)
        {
            _listInstances.TryRemove(_listBox, out _);
        }

        // Clear items
        _items.Clear();

        // Destroy the widget (destroying the scrolled window also destroys the list box)
        if (_scrolledWindow != IntPtr.Zero)
        {
            gtk_widget_destroy(_scrolledWindow);
        }
    }

    #endregion

    #region GTK P/Invoke

    private const int GTK_SELECTION_SINGLE = 1;
    private const int GTK_SELECTION_MULTIPLE = 3;

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x, y, width, height;
    }

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_insert(IntPtr box, IntPtr child, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_select_row(IntPtr box, IntPtr row);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_unselect_row(IntPtr box, IntPtr row);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_unselect_all(IntPtr box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_get_row_at_index(IntPtr box, int index);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_list_box_row_get_index(IntPtr row);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_get_selected_row(IntPtr box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_list_box_get_selected_rows(IntPtr box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_list_box_set_selection_mode(IntPtr box, int mode);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolled_window, int hscrollbar_policy, int vscrollbar_policy);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_get_vadjustment(IntPtr scrolled_window);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern double gtk_adjustment_get_value(IntPtr adjustment);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_adjustment_set_value(IntPtr adjustment, double value);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_label_new(string str);

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

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint g_list_length(IntPtr list);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_list_nth_data(IntPtr list, uint n);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_list_free(IntPtr list);

    [DllImport(GObjectLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern ulong g_signal_connect_data(
        IntPtr instance,
        string detailed_signal,
        IntPtr c_handler,
        IntPtr data,
        IntPtr destroy_data,
        int connect_flags);

    #endregion
}
