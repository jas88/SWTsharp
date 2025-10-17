using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of IPlatformTabFolder using GtkNotebook.
/// </summary>
internal class LinuxTabFolder : IPlatformTabFolder
{
    private readonly IntPtr _gtkNotebook;
    private readonly IntPtr _handle;
    private readonly int _style;
    private readonly List<LinuxTabItem> _items = new();
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(240, 240, 240);
    private RGB _foreground = new RGB(0, 0, 0);

    // Events required by IPlatformComposite
    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;

    // Events required by IPlatformSelectionEvents
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    #pragma warning restore CS0067

    public LinuxTabFolder(IntPtr parentHandle, int style)
    {
        _style = style;

        // Create GtkNotebook for tab control
        _gtkNotebook = gtk_notebook_new();
        _handle = _gtkNotebook;

        if (_gtkNotebook == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK Notebook");
        }

        // Configure notebook properties
        gtk_notebook_set_scrollable(_gtkNotebook, true);
        gtk_notebook_set_show_tabs(_gtkNotebook, true);
        gtk_notebook_set_show_border(_gtkNotebook, (style & SWT.BORDER) != 0);

        // Set tab position (default to top)
        if ((style & SWT.BOTTOM) != 0)
        {
            gtk_notebook_set_tab_pos(_gtkNotebook, GtkPositionType.GTK_POS_BOTTOM);
        }
        else
        {
            gtk_notebook_set_tab_pos(_gtkNotebook, GtkPositionType.GTK_POS_TOP);
        }

        // Add to parent container if provided
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkNotebook);
        }

        // Connect switch-page signal for selection changes
        g_signal_connect_data(_gtkNotebook, "switch-page", OnSwitchPage, IntPtr.Zero, IntPtr.Zero, 0);

        // Show the widget
        gtk_widget_show(_gtkNotebook);
    }

    private void OnSwitchPage(IntPtr notebook, IntPtr page, uint pageNum, IntPtr userData)
    {
        SelectionChanged?.Invoke(this, (int)pageNum);
    }

    /// <summary>
    /// Gets the native GTK handle for this tab folder.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _handle;
    }

    #region IPlatformTabFolder Implementation

    public int GetItemCount()
    {
        return _items.Count;
    }

    public IPlatformTabItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _items[index];
    }

    public int SelectionIndex
    {
        get
        {
            if (_gtkNotebook == IntPtr.Zero) return -1;
            return gtk_notebook_get_current_page(_gtkNotebook);
        }
        set
        {
            if (_gtkNotebook == IntPtr.Zero) return;

            int oldIndex = SelectionIndex;
            gtk_notebook_set_current_page(_gtkNotebook, value);

            if (oldIndex != value)
            {
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    public IPlatformTabItem CreateTabItem(int style, int index)
    {
        // Create tab label and content container
        var tabItem = new LinuxTabItem(this, _gtkNotebook, style, index);

        if (index >= 0 && index < _items.Count)
        {
            _items.Insert(index, tabItem);
        }
        else
        {
            _items.Add(tabItem);
        }

        return tabItem;
    }

    #endregion

    #region IPlatformComposite Implementation

    public void AddChild(IPlatformWidget child)
    {
        if (!_children.Contains(child))
        {
            _children.Add(child);
            ChildAdded?.Invoke(this, child);
        }
    }

    public void RemoveChild(IPlatformWidget child)
    {
        if (_children.Remove(child))
        {
            ChildRemoved?.Invoke(this, child);
        }
    }

    public IReadOnlyList<IPlatformWidget> GetChildren()
    {
        return _children.AsReadOnly();
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_handle == IntPtr.Zero) return;

        var allocation = new GtkAllocation
        {
            x = x,
            y = y,
            width = width,
            height = height
        };
        gtk_widget_size_allocate(_handle, ref allocation);
    }

    public Rectangle GetBounds()
    {
        if (_handle == IntPtr.Zero) return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_handle, out allocation);
        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_handle == IntPtr.Zero) return;

        if (visible)
        {
            gtk_widget_show(_handle);
        }
        else
        {
            gtk_widget_hide(_handle);
        }
    }

    public bool GetVisible()
    {
        if (_handle == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_handle);
    }

    public void SetEnabled(bool enabled)
    {
        if (_handle == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_handle, enabled);
    }

    public bool GetEnabled()
    {
        if (_handle == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_handle);
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

        // Dispose all tab items
        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        // Clear children list
        _children.Clear();

        // Destroy the widget
        if (_handle != IntPtr.Zero)
        {
            gtk_widget_destroy(_handle);
        }
    }

    #endregion

    #region GTK P/Invoke

    private enum GtkPositionType
    {
        GTK_POS_LEFT,
        GTK_POS_RIGHT,
        GTK_POS_TOP,
        GTK_POS_BOTTOM
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_notebook_new();

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_scrollable(IntPtr notebook, bool scrollable);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_show_tabs(IntPtr notebook, bool showTabs);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_show_border(IntPtr notebook, bool showBorder);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_tab_pos(IntPtr notebook, GtkPositionType pos);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern int gtk_notebook_get_current_page(IntPtr notebook);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_current_page(IntPtr notebook, int pageNum);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern int gtk_notebook_append_page(IntPtr notebook, IntPtr child, IntPtr tabLabel);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern int gtk_notebook_insert_page(IntPtr notebook, IntPtr child, IntPtr tabLabel, int position);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_tab_label_text(IntPtr notebook, IntPtr child, string tabText);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_notebook_get_tab_label(IntPtr notebook, IntPtr child);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_size_allocate(IntPtr widget, ref GtkAllocation allocation);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport("libgobject-2.0.so.0", CharSet = CharSet.Ansi)]
    private static extern ulong g_signal_connect_data(IntPtr instance, string detailedSignal, SwitchPageCallback cHandler, IntPtr data, IntPtr destroyData, int connectFlags);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SwitchPageCallback(IntPtr notebook, IntPtr page, uint pageNum, IntPtr userData);

    #endregion
}

/// <summary>
/// Linux/GTK implementation of IPlatformTabItem.
/// </summary>
internal class LinuxTabItem : IPlatformTabItem
{
    private readonly LinuxTabFolder _parent;
    private readonly IntPtr _notebook;
    private readonly IntPtr _contentContainer;
    private readonly IntPtr _tabLabel;
    private readonly int _style;
    private string _text = string.Empty;
    private bool _disposed;

    // Events required by IPlatformEventHandling
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public LinuxTabItem(LinuxTabFolder parent, IntPtr notebook, int style, int index)
    {
        _parent = parent;
        _notebook = notebook;
        _style = style;

        // Create a GtkBox as content container for the tab page
        _contentContainer = gtk_box_new(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0);

        // Create a GtkLabel for the tab label
        _tabLabel = gtk_label_new("");

        // Insert or append the page to the notebook
        int pageIndex;
        if (index >= 0)
        {
            pageIndex = gtk_notebook_insert_page(_notebook, _contentContainer, _tabLabel, index);
        }
        else
        {
            pageIndex = gtk_notebook_append_page(_notebook, _contentContainer, _tabLabel);
        }

        if (pageIndex < 0)
        {
            throw new InvalidOperationException("Failed to create GTK tab item");
        }

        // Show the widgets
        gtk_widget_show(_contentContainer);
        gtk_widget_show(_tabLabel);
    }

    /// <summary>
    /// Gets the native content container handle for this tab item.
    /// </summary>
    internal IntPtr GetContentContainer()
    {
        return _contentContainer;
    }

    public void SetText(string text)
    {
        _text = text ?? string.Empty;

        if (_notebook != IntPtr.Zero && _contentContainer != IntPtr.Zero)
        {
            gtk_notebook_set_tab_label_text(_notebook, _contentContainer, _text);
        }
    }

    public string GetText()
    {
        return _text;
    }

    public void SetControl(IPlatformWidget? control)
    {
        if (_contentContainer == IntPtr.Zero) return;

        // Extract native GTK handle from platform widget
        IntPtr controlHandle = ExtractGtkHandle(control);

        if (controlHandle != IntPtr.Zero)
        {
            // Add the control to the content container
            gtk_container_add(_contentContainer, controlHandle);
            gtk_widget_show(controlHandle);
        }
    }

    public void SetToolTipText(string toolTip)
    {
        if (_tabLabel != IntPtr.Zero)
        {
            gtk_widget_set_tooltip_text(_tabLabel, toolTip ?? string.Empty);
        }
    }

    /// <summary>
    /// Extracts GTK widget handle from platform widget.
    /// </summary>
    private static IntPtr ExtractGtkHandle(IPlatformWidget? widget)
    {
        if (widget == null) return IntPtr.Zero;

        // Try casting to known Linux widget types
        if (widget is LinuxTabFolder tabFolder)
            return tabFolder.GetNativeHandle();
        if (widget is LinuxTabItem tabItem)
            return tabItem.GetContentContainer();
        if (widget is LinuxComposite composite)
            return composite.GetNativeHandle();

        // Fallback: try reflection to find GetNativeHandle method
        var method = widget.GetType().GetMethod("GetNativeHandle",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

        if (method != null)
        {
            var result = method.Invoke(widget, null);
            if (result is IntPtr handle)
                return handle;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Tab items are cleaned up when notebook is destroyed
        // Labels and containers are owned by the notebook
    }

    #region GTK P/Invoke

    private enum GtkOrientation
    {
        GTK_ORIENTATION_HORIZONTAL,
        GTK_ORIENTATION_VERTICAL
    }

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_box_new(GtkOrientation orientation, int spacing);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_label_new(string str);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern int gtk_notebook_append_page(IntPtr notebook, IntPtr child, IntPtr tabLabel);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern int gtk_notebook_insert_page(IntPtr notebook, IntPtr child, IntPtr tabLabel, int position);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_notebook_set_tab_label_text(IntPtr notebook, IntPtr child, string tabText);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_set_tooltip_text(IntPtr widget, string text);

    #endregion
}
