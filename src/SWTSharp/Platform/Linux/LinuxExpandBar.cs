using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of IPlatformExpandBar using GtkExpander widgets in a GtkBox.
/// Provides native accordion-style expand/collapse functionality.
/// </summary>
internal class LinuxExpandBar : IPlatformExpandBar
{
    private readonly IntPtr _gtkBox;
    private readonly IntPtr _handle;
    private readonly int _style;
    private readonly List<LinuxExpandItem> _items = new();
    private readonly List<IPlatformWidget> _children = new();
    private bool _disposed;
    private RGB _background = new RGB(240, 240, 240);
    private RGB _foreground = new RGB(0, 0, 0);
    private int _spacing = 4;

    #pragma warning disable CS0067
    public event EventHandler<IPlatformWidget>? ChildAdded;
    public event EventHandler<IPlatformWidget>? ChildRemoved;
    public event EventHandler? LayoutRequested;
    public event EventHandler<int>? ItemExpanded;
    public event EventHandler<int>? ItemCollapsed;
    #pragma warning restore CS0067

    public LinuxExpandBar(IntPtr parentHandle, int style)
    {
        _style = style;

        // Create GtkBox for vertical layout
        _gtkBox = gtk_box_new(GtkOrientation.GTK_ORIENTATION_VERTICAL, _spacing);
        _handle = _gtkBox;

        if (_gtkBox == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK Box for ExpandBar");
        }

        // Add scrolling support if requested
        if ((style & SWT.V_SCROLL) != 0)
        {
            var scrolled = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
            gtk_scrolled_window_set_policy(scrolled, GtkPolicyType.GTK_POLICY_NEVER,
                GtkPolicyType.GTK_POLICY_AUTOMATIC);
            gtk_container_add(scrolled, _gtkBox);

            if (parentHandle != IntPtr.Zero)
            {
                gtk_container_add(parentHandle, scrolled);
            }
            gtk_widget_show(scrolled);
        }
        else if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkBox);
        }

        gtk_widget_show(_gtkBox);
    }

    internal IntPtr GetNativeHandle()
    {
        return _handle;
    }

    #region IPlatformExpandBar Implementation

    public int GetItemCount()
    {
        return _items.Count;
    }

    public IPlatformExpandItem GetItem(int index)
    {
        if (index < 0 || index >= _items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _items[index];
    }

    public void SetSpacing(int spacing)
    {
        _spacing = Math.Max(0, spacing);
        if (_gtkBox != IntPtr.Zero)
        {
            gtk_box_set_spacing(_gtkBox, _spacing);
        }
    }

    public int GetSpacing()
    {
        return _spacing;
    }

    public IPlatformExpandItem CreateExpandItem(int style, int index)
    {
        var item = new LinuxExpandItem(this, _gtkBox, style, index);

        if (index >= 0 && index < _items.Count)
        {
            _items.Insert(index, item);
        }
        else
        {
            _items.Add(item);
        }

        return item;
    }

    internal void OnItemExpandedChanged(LinuxExpandItem item, bool expanded)
    {
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            if (expanded)
            {
                ItemExpanded?.Invoke(this, index);
            }
            else
            {
                ItemCollapsed?.Invoke(this, index);
            }
        }
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
            gtk_widget_show(_handle);
        else
            gtk_widget_hide(_handle);
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
    }

    public RGB GetBackground()
    {
        return _background;
    }

    public void SetForeground(RGB color)
    {
        _foreground = color;
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

        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();
        _children.Clear();

        if (_handle != IntPtr.Zero)
        {
            gtk_widget_destroy(_handle);
        }
    }

    #endregion

    #region GTK P/Invoke

    private enum GtkOrientation
    {
        GTK_ORIENTATION_HORIZONTAL,
        GTK_ORIENTATION_VERTICAL
    }

    private enum GtkPolicyType
    {
        GTK_POLICY_ALWAYS,
        GTK_POLICY_AUTOMATIC,
        GTK_POLICY_NEVER
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
    private static extern IntPtr gtk_box_new(GtkOrientation orientation, int spacing);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_box_set_spacing(IntPtr box, int spacing);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolledWindow,
        GtkPolicyType hscrollbarPolicy, GtkPolicyType vscrollbarPolicy);

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

    #endregion
}

/// <summary>
/// Linux/GTK implementation of IPlatformExpandItem using GtkExpander.
/// </summary>
internal class LinuxExpandItem : IPlatformExpandItem
{
    private readonly LinuxExpandBar _expandBar;
    private readonly IntPtr _expander;
    private readonly IntPtr _contentBox;
    private string _text = string.Empty;
    private bool _expanded;
    private int _height = 100;
    private IPlatformWidget? _control;
    private bool _disposed;

    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public LinuxExpandItem(LinuxExpandBar expandBar, IntPtr parentBox, int style, int index)
    {
        _expandBar = expandBar;

        // Create GtkExpander
        _expander = gtk_expander_new("");

        // Create content container
        _contentBox = gtk_box_new(GtkOrientation.GTK_ORIENTATION_VERTICAL, 0);

        // Add content box to expander
        gtk_container_add(_expander, _contentBox);

        // Pack into parent box
        gtk_box_pack_start(parentBox, _expander, false, false, 0);

        // Connect activate signal for expand/collapse events
        g_signal_connect_data(_expander, "activate", OnActivated, IntPtr.Zero, IntPtr.Zero, 0);

        gtk_widget_show(_expander);
        gtk_widget_show(_contentBox);
    }

    private void OnActivated(IntPtr expander, IntPtr userData)
    {
        // Get new expanded state (GTK toggles before signal)
        bool newExpanded = gtk_expander_get_expanded(expander);
        if (_expanded != newExpanded)
        {
            _expanded = newExpanded;
            _expandBar.OnItemExpandedChanged(this, _expanded);
        }
    }

    public void SetText(string text)
    {
        _text = text ?? string.Empty;
        if (_expander != IntPtr.Zero)
        {
            gtk_expander_set_label(_expander, _text);
        }
    }

    public string GetText()
    {
        return _text;
    }

    public void SetExpanded(bool expanded)
    {
        _expanded = expanded;
        if (_expander != IntPtr.Zero)
        {
            gtk_expander_set_expanded(_expander, expanded);
        }
    }

    public bool GetExpanded()
    {
        return _expanded;
    }

    public void SetHeight(int height)
    {
        _height = Math.Max(0, height);
        if (_contentBox != IntPtr.Zero)
        {
            gtk_widget_set_size_request(_contentBox, -1, _height);
        }
    }

    public int GetHeight()
    {
        return _height;
    }

    public void SetControl(IPlatformWidget? control)
    {
        _control = control;

        if (_control != null && _contentBox != IntPtr.Zero)
        {
            IntPtr controlHandle = ExtractGtkHandle(_control);
            if (controlHandle != IntPtr.Zero)
            {
                gtk_container_add(_contentBox, controlHandle);
                gtk_widget_show(controlHandle);
            }
        }
    }

    private static IntPtr ExtractGtkHandle(IPlatformWidget? widget)
    {
        if (widget == null) return IntPtr.Zero;

        var method = widget.GetType().GetMethod("GetNativeHandle",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

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

        // GtkExpander and children are destroyed with parent
    }

    #region GTK P/Invoke

    private enum GtkOrientation
    {
        GTK_ORIENTATION_HORIZONTAL,
        GTK_ORIENTATION_VERTICAL
    }

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_expander_new(string label);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_expander_set_label(IntPtr expander, string label);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_expander_set_expanded(IntPtr expander, bool expanded);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern bool gtk_expander_get_expanded(IntPtr expander);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_box_new(GtkOrientation orientation, int spacing);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_box_pack_start(IntPtr box, IntPtr child, bool expand, bool fill, uint padding);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport("libgobject-2.0.so.0", CharSet = CharSet.Ansi)]
    private static extern ulong g_signal_connect_data(IntPtr instance, string detailedSignal,
        ActivateCallback cHandler, IntPtr data, IntPtr destroyData, int connectFlags);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ActivateCallback(IntPtr expander, IntPtr userData);

    #endregion
}
