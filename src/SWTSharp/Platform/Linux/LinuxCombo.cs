using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of IPlatformCombo using GtkComboBoxText.
/// </summary>
internal class LinuxCombo : LinuxWidget, IPlatformCombo
{
    private readonly IntPtr _gtkComboBox;
    private readonly IntPtr _handle;
    private readonly int _style;
    private readonly List<string> _items = new();
    private bool _disposed;
    private RGB _background = new RGB(255, 255, 255);
    private RGB _foreground = new RGB(0, 0, 0);
    private int _selectionIndex = -1;

    // Events required by IPlatformSelectionEvents
    public event EventHandler<int>? SelectionChanged;
    #pragma warning disable CS0067
    public event EventHandler<int>? ItemDoubleClick;

    // Events required by IPlatformEventHandling
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public LinuxCombo(IntPtr parentHandle, int style)
    {
        _style = style;

        // Create GtkComboBoxText (simple text-based combo)
        if ((style & SWT.READ_ONLY) != 0)
        {
            // Read-only combo box (no entry field)
            _gtkComboBox = gtk_combo_box_text_new();
        }
        else
        {
            // Editable combo box (with entry field)
            _gtkComboBox = gtk_combo_box_text_new_with_entry();
        }

        _handle = _gtkComboBox;

        if (_gtkComboBox == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK ComboBoxText");
        }

        // Add to parent container if provided
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _gtkComboBox);
        }

        // Connect changed signal for selection changes
        g_signal_connect_data(_gtkComboBox, "changed", OnChangedCallback, IntPtr.Zero, IntPtr.Zero, 0);

        // Show the widget
        gtk_widget_show(_gtkComboBox);
    }

    private void OnChangedCallback(IntPtr comboBox, IntPtr userData)
    {
        int newIndex = gtk_combo_box_get_active(_gtkComboBox);
        if (newIndex != _selectionIndex)
        {
            _selectionIndex = newIndex;
            SelectionChanged?.Invoke(this, _selectionIndex);
        }
    }

    /// <summary>
    /// Gets the native GTK handle for this combo box.
    /// </summary>
    public override IntPtr GetNativeHandle()
    {
        return _handle;
    }

    #region IPlatformCombo Implementation

    public void AddItem(string item)
    {
        if (_gtkComboBox == IntPtr.Zero || string.IsNullOrEmpty(item)) return;

        _items.Add(item);
        gtk_combo_box_text_append_text(_gtkComboBox, item);
    }

    public void ClearItems()
    {
        if (_gtkComboBox == IntPtr.Zero) return;

        _items.Clear();
        _selectionIndex = -1;
        gtk_combo_box_text_remove_all(_gtkComboBox);
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

    public int SelectionIndex
    {
        get
        {
            if (_gtkComboBox == IntPtr.Zero) return -1;
            return gtk_combo_box_get_active(_gtkComboBox);
        }
        set
        {
            if (_gtkComboBox == IntPtr.Zero) return;

            int oldIndex = _selectionIndex;
            _selectionIndex = value;

            if (value >= 0 && value < _items.Count)
            {
                gtk_combo_box_set_active(_gtkComboBox, value);
            }
            else
            {
                gtk_combo_box_set_active(_gtkComboBox, -1);
            }

            // Fire SelectionChanged event if index actually changed
            if (oldIndex != _selectionIndex)
            {
                SelectionChanged?.Invoke(this, _selectionIndex);
            }
        }
    }

    public string Text
    {
        get
        {
            if (_gtkComboBox == IntPtr.Zero) return string.Empty;

            // Try to get active text first (works for both editable and read-only)
            IntPtr textPtr = gtk_combo_box_text_get_active_text(_gtkComboBox);
            if (textPtr != IntPtr.Zero)
            {
#if NETSTANDARD2_0
                string result = GetUtf8String(textPtr);
#else
                string result = Marshal.PtrToStringUTF8(textPtr) ?? string.Empty;
#endif
                g_free(textPtr);
                return result;
            }

            // For editable combos, get text from entry
            IntPtr entry = gtk_bin_get_child(_gtkComboBox);
            if (entry != IntPtr.Zero)
            {
                IntPtr entryTextPtr = gtk_entry_get_text(entry);
#if NETSTANDARD2_0
                return GetUtf8String(entryTextPtr);
#else
                return Marshal.PtrToStringUTF8(entryTextPtr) ?? string.Empty;
#endif
            }

            return string.Empty;
        }
        set
        {
            if (_gtkComboBox == IntPtr.Zero) return;

            // Try to find the item in the list
            string textValue = value ?? string.Empty;
            int index = _items.IndexOf(textValue);
            if (index >= 0)
            {
                SelectionIndex = index;
            }
            else
            {
                // For editable combos, set the text directly
                IntPtr entry = gtk_bin_get_child(_gtkComboBox);
                if (entry != IntPtr.Zero)
                {
                    gtk_entry_set_text(entry, textValue);
                }
            }
        }
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

        // Clear items
        _items.Clear();

        // Destroy the widget
        if (_handle != IntPtr.Zero)
        {
            gtk_widget_destroy(_handle);
        }
    }

    #endregion

    #region GTK P/Invoke

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_combo_box_text_new();

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_combo_box_text_new_with_entry();

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_combo_box_text_append_text(IntPtr combo_box, string text);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_combo_box_text_insert_text(IntPtr combo_box, int position, string text);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_combo_box_text_remove(IntPtr combo_box, int position);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_combo_box_text_remove_all(IntPtr combo_box);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_combo_box_text_get_active_text(IntPtr combo_box);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern int gtk_combo_box_get_active(IntPtr combo_box);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_combo_box_set_active(IntPtr combo_box, int index);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_bin_get_child(IntPtr bin);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_entry_set_text(IntPtr entry, string text);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern IntPtr gtk_entry_get_text(IntPtr entry);

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
    private static extern ulong g_signal_connect_data(IntPtr instance, string detailedSignal, ChangedCallback cHandler, IntPtr data, IntPtr destroyData, int connectFlags);

    [DllImport("libglib-2.0.so.0", CharSet = CharSet.Ansi)]
    private static extern void g_free(IntPtr mem);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ChangedCallback(IntPtr comboBox, IntPtr userData);

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

    #endregion
}
