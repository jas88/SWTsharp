using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Linux (GTK) platform implementation - StyledText widget methods.
/// Uses GtkTextView with GtkTextBuffer for rich text editing.
/// </summary>
internal partial class LinuxPlatform
{
    private class LinuxStyledText : IPlatformStyledText
    {
        private readonly LinuxPlatform _platform;
        private readonly IntPtr _handle;
        private IntPtr _buffer;
        private string _text = string.Empty;
        private bool _editable = true;
        private bool _disposed;

        public event EventHandler<string>? TextChanged;
        public event EventHandler<int>? SelectionChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public LinuxStyledText(LinuxPlatform platform, IntPtr handle, IntPtr buffer)
        {
            _platform = platform;
            _handle = handle;
            _buffer = buffer;
        }

        public void SetText(string text)
        {
            _text = text ?? string.Empty;
            gtk_text_buffer_set_text(_buffer, _text, _text.Length);
        }

        public string GetText()
        {
            IntPtr start, end;
            gtk_text_buffer_get_bounds(_buffer, out start, out end);
            IntPtr textPtr = gtk_text_buffer_get_text(_buffer, start, end, false);
            return PtrToStringUTF8(textPtr);
        }

        public void SetEditable(bool editable)
        {
            _editable = editable;
            gtk_text_view_set_editable(_handle, editable);
        }

        public void Insert(string text)
        {
            if (text == null) return;
            gtk_text_buffer_insert_at_cursor(_buffer, text, text.Length);
        }

        public void ReplaceTextRange(int start, int length, string text)
        {
            IntPtr startIter, endIter;
            gtk_text_buffer_get_iter_at_offset(_buffer, out startIter, start);
            gtk_text_buffer_get_iter_at_offset(_buffer, out endIter, start + length);
            gtk_text_buffer_delete(_buffer, ref startIter, ref endIter);
            gtk_text_buffer_insert(_buffer, ref startIter, text ?? string.Empty, -1);
        }

        public void SetSelection(int start, int end)
        {
            IntPtr startIter, endIter;
            gtk_text_buffer_get_iter_at_offset(_buffer, out startIter, start);
            gtk_text_buffer_get_iter_at_offset(_buffer, out endIter, end);
            gtk_text_buffer_select_range(_buffer, ref startIter, ref endIter);
        }

        public (int Start, int End) GetSelection()
        {
            IntPtr startIter, endIter;
            if (gtk_text_buffer_get_selection_bounds(_buffer, out startIter, out endIter))
            {
                int start = gtk_text_iter_get_offset(ref startIter);
                int end = gtk_text_iter_get_offset(ref endIter);
                return (start, end);
            }
            return (0, 0);
        }

        public string GetSelectionText()
        {
            IntPtr startIter, endIter;
            if (gtk_text_buffer_get_selection_bounds(_buffer, out startIter, out endIter))
            {
                IntPtr textPtr = gtk_text_buffer_get_text(_buffer, startIter, endIter, false);
                return PtrToStringUTF8(textPtr);
            }
            return string.Empty;
        }

        public void SetCaretOffset(int offset)
        {
            IntPtr iter;
            gtk_text_buffer_get_iter_at_offset(_buffer, out iter, offset);
            gtk_text_buffer_place_cursor(_buffer, ref iter);
        }

        public int GetCaretOffset()
        {
            IntPtr mark = gtk_text_buffer_get_insert(_buffer);
            IntPtr iter;
            gtk_text_buffer_get_iter_at_mark(_buffer, out iter, mark);
            return gtk_text_iter_get_offset(ref iter);
        }

        public void SetStyleRange(StyleRange range)
        {
            if (range == null) return;

            // Create text tag for styling
            IntPtr tag = gtk_text_tag_new(null);

            // Style setting removed - implement using GtkTextTag API later
            // g_object_set_property not available in our bindings
            // TODO: Implement proper style setting using g_object_set with varargs

            // Get tag table and add tag
            IntPtr tagTable = gtk_text_buffer_get_tag_table(_buffer);
            gtk_text_tag_table_add(tagTable, tag);

            // Apply tag to range
            IntPtr startIter, endIter;
            gtk_text_buffer_get_iter_at_offset(_buffer, out startIter, range.Start);
            gtk_text_buffer_get_iter_at_offset(_buffer, out endIter, range.Start + range.Length);
            gtk_text_buffer_apply_tag(_buffer, tag, ref startIter, ref endIter);
        }

        public string GetLine(int lineIndex)
        {
            IntPtr startIter, endIter;
            gtk_text_buffer_get_iter_at_line(_buffer, out startIter, lineIndex);
            endIter = startIter;
            if (!gtk_text_iter_ends_line(ref endIter))
            {
                gtk_text_iter_forward_to_line_end(ref endIter);
            }
            IntPtr textPtr = gtk_text_buffer_get_text(_buffer, startIter, endIter, false);
            return PtrToStringUTF8(textPtr);
        }

        public int GetLineCount()
        {
            return gtk_text_buffer_get_line_count(_buffer);
        }

        public void Copy()
        {
            IntPtr clipboard = gtk_clipboard_get(GDK_SELECTION_CLIPBOARD);
            gtk_text_buffer_copy_clipboard(_buffer, clipboard);
        }

        public void Cut()
        {
            if (!_editable) return;
            IntPtr clipboard = gtk_clipboard_get(GDK_SELECTION_CLIPBOARD);
            gtk_text_buffer_cut_clipboard(_buffer, clipboard, _editable);
        }

        public void Paste()
        {
            if (!_editable) return;
            IntPtr clipboard = gtk_clipboard_get(GDK_SELECTION_CLIPBOARD);
            gtk_text_buffer_paste_clipboard(_buffer, clipboard, IntPtr.Zero, _editable);
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            gtk_widget_set_size_request(_handle, width, height);
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _handle == IntPtr.Zero) return default;
            GtkAllocation allocation;
            gtk_widget_get_allocation(_handle, out allocation);
            return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            if (visible)
                gtk_widget_show(_handle);
            else
                gtk_widget_hide(_handle);
        }

        public bool GetVisible()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return gtk_widget_get_visible(_handle);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            gtk_widget_set_sensitive(_handle, enabled);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return gtk_widget_get_sensitive(_handle);
        }

        public void SetBackground(RGB color)
        {
            // GTK3 theming - would need CSS provider
        }

        public RGB GetBackground()
        {
            return new RGB(255, 255, 255);
        }

        public void SetForeground(RGB color)
        {
            // GTK3 theming - would need CSS provider
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 0);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle != IntPtr.Zero)
                {
                    gtk_widget_destroy(_handle);
                }
                _disposed = true;
            }
        }

        internal void OnTextChanged()
        {
            _text = GetText();
            TextChanged?.Invoke(this, _text);
        }

        internal void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, 0);
        }
    }

    public IPlatformStyledText CreateStyledTextWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        // Create text buffer
        IntPtr buffer = gtk_text_buffer_new(IntPtr.Zero);

        // Create text view
        IntPtr textView = gtk_text_view_new_with_buffer(buffer);

        // Set properties
        if ((style & SWT.WRAP) != 0)
        {
            gtk_text_view_set_wrap_mode(textView, GtkWrapMode.WORD);
        }

        if ((style & SWT.READ_ONLY) != 0)
        {
            gtk_text_view_set_editable(textView, false);
        }

        // Create scrolled window if scrollbars requested
        IntPtr widget = textView;
        if ((style & (SWT.H_SCROLL | SWT.V_SCROLL)) != 0)
        {
            IntPtr scrolled = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
            gtk_scrolled_window_set_policy(scrolled,
                (int)((style & SWT.H_SCROLL) != 0 ? GtkPolicyType.Automatic : GtkPolicyType.Never),
                (int)((style & SWT.V_SCROLL) != 0 ? GtkPolicyType.Automatic : GtkPolicyType.Never));
            gtk_container_add(scrolled, textView);
            widget = scrolled;
            gtk_widget_show(textView);
        }

        // Add to parent if specified
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, widget);
        }

        gtk_widget_show(widget);

        var styledTextWidget = new LinuxStyledText(this, textView, buffer);
        _styledTextWidgets[textView] = styledTextWidget;

        // Callback connection removed - not needed for basic functionality
        // g_signal_connect_data(buffer, "changed", OnTextBufferChanged, IntPtr.Zero, IntPtr.Zero, 0);

        return styledTextWidget;
    }

    private Dictionary<IntPtr, LinuxStyledText> _styledTextWidgets = new Dictionary<IntPtr, LinuxStyledText>();

#if NET8_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static void OnTextBufferChanged(IntPtr buffer, IntPtr userData)
    {
        // Find the styled text widget and invoke the callback
        // This is a simplified version - would need proper callback routing
    }
#endif

    private enum GtkWrapMode
    {
        NONE = 0,
        CHAR = 1,
        WORD = 2,
        WORD_CHAR = 3
    }

    private enum GtkPolicyType
    {
        Always = 0,
        Automatic = 1,
        Never = 2,
        External = 3
    }

    private const int GDK_SELECTION_CLIPBOARD = 69;

#if NET8_0_OR_GREATER
    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_text_view_new_with_buffer(IntPtr buffer);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_view_set_editable(IntPtr text_view, [MarshalAs(UnmanagedType.Bool)] bool setting);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_view_set_wrap_mode(IntPtr text_view, GtkWrapMode wrap_mode);

    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_text_buffer_new(IntPtr table);

    [LibraryImport(GtkLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_text_buffer_set_text(IntPtr buffer, string text, int len);

    [LibraryImport(GtkLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr gtk_text_buffer_get_text(IntPtr buffer, IntPtr start, IntPtr end, [MarshalAs(UnmanagedType.Bool)] bool include_hidden_chars);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_get_bounds(IntPtr buffer, out IntPtr start, out IntPtr end);

    [LibraryImport(GtkLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_text_buffer_insert_at_cursor(IntPtr buffer, string text, int len);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_get_iter_at_offset(IntPtr buffer, out IntPtr iter, int char_offset);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_get_iter_at_line(IntPtr buffer, out IntPtr iter, int line_number);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_delete(IntPtr buffer, ref IntPtr start, ref IntPtr end);

    [LibraryImport(GtkLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_text_buffer_insert(IntPtr buffer, ref IntPtr iter, string text, int len);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_select_range(IntPtr buffer, ref IntPtr ins, ref IntPtr bound);

    [LibraryImport(GtkLib)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool gtk_text_buffer_get_selection_bounds(IntPtr buffer, out IntPtr start, out IntPtr end);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_place_cursor(IntPtr buffer, ref IntPtr where);

    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_text_buffer_get_insert(IntPtr buffer);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_get_iter_at_mark(IntPtr buffer, out IntPtr iter, IntPtr mark);

    [LibraryImport(GtkLib)]
    private static partial int gtk_text_iter_get_offset(ref IntPtr iter);

    [LibraryImport(GtkLib)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool gtk_text_iter_ends_line(ref IntPtr iter);

    [LibraryImport(GtkLib)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool gtk_text_iter_forward_to_line_end(ref IntPtr iter);

    [LibraryImport(GtkLib)]
    private static partial int gtk_text_buffer_get_line_count(IntPtr buffer);

    [LibraryImport(GtkLib, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr gtk_text_tag_new(string? name);

    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_text_buffer_get_tag_table(IntPtr buffer);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_tag_table_add(IntPtr table, IntPtr tag);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_apply_tag(IntPtr buffer, IntPtr tag, ref IntPtr start, ref IntPtr end);

    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_clipboard_get(int selection);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_copy_clipboard(IntPtr buffer, IntPtr clipboard);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_cut_clipboard(IntPtr buffer, IntPtr clipboard, [MarshalAs(UnmanagedType.Bool)] bool default_editable);

    [LibraryImport(GtkLib)]
    private static partial void gtk_text_buffer_paste_clipboard(IntPtr buffer, IntPtr clipboard, IntPtr override_location, [MarshalAs(UnmanagedType.Bool)] bool default_editable);

#else
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_view_new_with_buffer(IntPtr buffer);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_view_set_editable(IntPtr text_view, bool setting);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_view_set_wrap_mode(IntPtr text_view, GtkWrapMode wrap_mode);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_buffer_new(IntPtr table);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_set_text(IntPtr buffer, string text, int len);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_buffer_get_text(IntPtr buffer, IntPtr start, IntPtr end, bool include_hidden_chars);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_get_bounds(IntPtr buffer, out IntPtr start, out IntPtr end);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_insert_at_cursor(IntPtr buffer, string text, int len);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_get_iter_at_offset(IntPtr buffer, out IntPtr iter, int char_offset);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_get_iter_at_line(IntPtr buffer, out IntPtr iter, int line_number);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_delete(IntPtr buffer, ref IntPtr start, ref IntPtr end);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_insert(IntPtr buffer, ref IntPtr iter, string text, int len);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_select_range(IntPtr buffer, ref IntPtr ins, ref IntPtr bound);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_text_buffer_get_selection_bounds(IntPtr buffer, out IntPtr start, out IntPtr end);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_place_cursor(IntPtr buffer, ref IntPtr where);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_buffer_get_insert(IntPtr buffer);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_get_iter_at_mark(IntPtr buffer, out IntPtr iter, IntPtr mark);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_text_iter_get_offset(ref IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_text_iter_ends_line(ref IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_text_iter_forward_to_line_end(ref IntPtr iter);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_text_buffer_get_line_count(IntPtr buffer);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_tag_new(string? name);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_text_buffer_get_tag_table(IntPtr buffer);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_tag_table_add(IntPtr table, IntPtr tag);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_apply_tag(IntPtr buffer, IntPtr tag, ref IntPtr start, ref IntPtr end);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_clipboard_get(int selection);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_copy_clipboard(IntPtr buffer, IntPtr clipboard);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_cut_clipboard(IntPtr buffer, IntPtr clipboard, bool default_editable);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_text_buffer_paste_clipboard(IntPtr buffer, IntPtr clipboard, IntPtr override_location, bool default_editable);
#endif
}
