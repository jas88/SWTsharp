using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux implementation of an editable text widget using GtkEntry or GtkTextView.
/// </summary>
internal class LinuxText : IPlatformTextInput
{
    private IntPtr _widget; // GtkEntry or GtkTextView
    private IntPtr _textBuffer; // GtkTextBuffer for multi-line (GtkTextView)
    private IntPtr _scrolledWindow; // GtkScrolledWindow for multi-line
    private bool _disposed;
    private string _text = string.Empty;
    private bool _readOnly;
    private int _textLimit = int.MaxValue;
    private bool _isMultiLine;

    // Event handling
    public event EventHandler<string>? TextChanged;
#pragma warning disable CS0067 // Event is never used
    public event EventHandler<string>? TextCommitted;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    // GTK constants
    private const int GTK_POLICY_AUTOMATIC = 1;
    private const int GTK_POLICY_NEVER = 2;

    public LinuxText(IntPtr parentHandle, int style)
    {
        _isMultiLine = (style & SWT.MULTI) != 0;
        _readOnly = (style & SWT.READ_ONLY) != 0;

        if (_isMultiLine)
        {
            CreateMultiLineText(parentHandle, style);
        }
        else
        {
            CreateSingleLineText(parentHandle, style);
        }
    }

    private void CreateSingleLineText(IntPtr parentHandle, int style)
    {
        // Create GtkEntry
        _widget = gtk_entry_new();

        // Handle password
        if ((style & SWT.PASSWORD) != 0)
        {
            gtk_entry_set_visibility(_widget, false);
            gtk_entry_set_invisible_char(_widget, '*');
        }

        // Handle read-only
        if (_readOnly)
        {
            gtk_editable_set_editable(_widget, false);
        }

        // Handle alignment
        float xalign = 0.0f; // Left
        if ((style & SWT.CENTER) != 0)
            xalign = 0.5f;
        else if ((style & SWT.RIGHT) != 0)
            xalign = 1.0f;

        gtk_entry_set_alignment(_widget, xalign);

        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _widget);
        }

        gtk_widget_show(_widget);
    }

    private void CreateMultiLineText(IntPtr parentHandle, int style)
    {
        // Create GtkTextView
        _widget = gtk_text_view_new();

        // Get the text buffer
        _textBuffer = gtk_text_view_get_buffer(_widget);

        // Handle read-only
        if (_readOnly)
        {
            gtk_text_view_set_editable(_widget, false);
        }

        // Handle wrap
        if ((style & SWT.WRAP) != 0)
        {
            gtk_text_view_set_wrap_mode(_widget, 2); // GTK_WRAP_WORD = 2
        }

        // Create scrolled window for multi-line text
        _scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);

        // Set scroll policies
        int hpolicy = GTK_POLICY_AUTOMATIC;
        int vpolicy = GTK_POLICY_AUTOMATIC;

        if ((style & SWT.H_SCROLL) == 0)
            hpolicy = GTK_POLICY_NEVER;
        if ((style & SWT.V_SCROLL) == 0)
            vpolicy = GTK_POLICY_NEVER;

        gtk_scrolled_window_set_policy(_scrolledWindow, hpolicy, vpolicy);

        // Add text view to scrolled window
        gtk_container_add(_scrolledWindow, _widget);

        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _scrolledWindow);
        }

        gtk_widget_show(_widget);
        gtk_widget_show(_scrolledWindow);
    }

    public void SetText(string text)
    {
        if (_disposed) return;

        _text = text ?? string.Empty;

        // Enforce text limit
        if (_text.Length > _textLimit)
        {
            _text = _text.Substring(0, _textLimit);
        }

        if (_isMultiLine)
        {
            if (_textBuffer != IntPtr.Zero)
            {
                gtk_text_buffer_set_text(_textBuffer, _text, _text.Length);
            }
        }
        else
        {
            if (_widget != IntPtr.Zero)
            {
                gtk_entry_set_text(_widget, _text);
            }
        }

        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed) return _text;

        if (_isMultiLine)
        {
            if (_textBuffer == IntPtr.Zero) return string.Empty;

            IntPtr startIter = Marshal.AllocHGlobal(80); // sizeof(GtkTextIter)
            IntPtr endIter = Marshal.AllocHGlobal(80);

            try
            {
                gtk_text_buffer_get_start_iter(_textBuffer, startIter);
                gtk_text_buffer_get_end_iter(_textBuffer, endIter);

                IntPtr textPtr = gtk_text_buffer_get_text(_textBuffer, startIter, endIter, false);
                string result = PtrToStringUTF8(textPtr);

                g_free(textPtr);

                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(startIter);
                Marshal.FreeHGlobal(endIter);
            }
        }
        else
        {
            if (_widget == IntPtr.Zero) return string.Empty;

            IntPtr textPtr = gtk_entry_get_text(_widget);
            return PtrToStringUTF8(textPtr);
        }
    }

    public void SetTextLimit(int limit)
    {
        if (_disposed) return;

        _textLimit = limit;

        if (!_isMultiLine && _widget != IntPtr.Zero)
        {
            gtk_entry_set_max_length(_widget, limit);
        }
        // Note: GtkTextView doesn't have built-in max length - would need signal handler
    }

    public void SetReadOnly(bool readOnly)
    {
        if (_disposed) return;

        _readOnly = readOnly;

        if (_isMultiLine)
        {
            if (_widget != IntPtr.Zero)
            {
                gtk_text_view_set_editable(_widget, !readOnly);
            }
        }
        else
        {
            if (_widget != IntPtr.Zero)
            {
                gtk_editable_set_editable(_widget, !readOnly);
            }
        }
    }

    public bool GetReadOnly()
    {
        return _readOnly;
    }

    public void SetSelection(int start, int end)
    {
        if (_disposed) return;

        if (_isMultiLine)
        {
            if (_textBuffer == IntPtr.Zero) return;

            IntPtr startIter = Marshal.AllocHGlobal(80);
            IntPtr endIter = Marshal.AllocHGlobal(80);

            try
            {
                gtk_text_buffer_get_iter_at_offset(_textBuffer, startIter, start);
                gtk_text_buffer_get_iter_at_offset(_textBuffer, endIter, end);
                gtk_text_buffer_select_range(_textBuffer, startIter, endIter);
            }
            finally
            {
                Marshal.FreeHGlobal(startIter);
                Marshal.FreeHGlobal(endIter);
            }
        }
        else
        {
            if (_widget != IntPtr.Zero)
            {
                gtk_editable_select_region(_widget, start, end);
            }
        }
    }

    public (int Start, int End) GetSelection()
    {
        if (_disposed) return (0, 0);

        if (_isMultiLine)
        {
            if (_textBuffer == IntPtr.Zero) return (0, 0);

            IntPtr startIter = Marshal.AllocHGlobal(80);
            IntPtr endIter = Marshal.AllocHGlobal(80);

            try
            {
                gtk_text_buffer_get_selection_bounds(_textBuffer, startIter, endIter);

                int start = gtk_text_iter_get_offset(startIter);
                int end = gtk_text_iter_get_offset(endIter);

                return (start, end);
            }
            finally
            {
                Marshal.FreeHGlobal(startIter);
                Marshal.FreeHGlobal(endIter);
            }
        }
        else
        {
            if (_widget == IntPtr.Zero) return (0, 0);

            int start = 0;
            int end = 0;

            unsafe
            {
                gtk_editable_get_selection_bounds(_widget, &start, &end);
            }

            return (start, end);
        }
    }

    public void Insert(string text)
    {
        if (_disposed || _readOnly) return;

        string insertText = text ?? string.Empty;

        if (_isMultiLine)
        {
            if (_textBuffer == IntPtr.Zero) return;

            // Get current insert position
            IntPtr insertMark = gtk_text_buffer_get_insert(_textBuffer);
            IntPtr iter = Marshal.AllocHGlobal(80);

            try
            {
                gtk_text_buffer_get_iter_at_mark(_textBuffer, iter, insertMark);
                gtk_text_buffer_insert(_textBuffer, iter, insertText, insertText.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(iter);
            }
        }
        else
        {
            if (_widget == IntPtr.Zero) return;

            int position = gtk_editable_get_position(_widget);
            gtk_editable_insert_text(_widget, insertText, insertText.Length, ref position);
            gtk_editable_set_position(_widget, position);
        }

        _text = GetText();
        TextChanged?.Invoke(this, _text);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed) return;

        IntPtr targetWidget = _isMultiLine ? _scrolledWindow : _widget;

        if (targetWidget != IntPtr.Zero)
        {
            gtk_widget_set_size_request(targetWidget, width, height);

            // For fixed positioning, would need parent to be GtkFixed
            // Most GTK apps use layout managers instead
        }
    }

    public Rectangle GetBounds()
    {
        if (_disposed) return default;

        IntPtr targetWidget = _isMultiLine ? _scrolledWindow : _widget;

        if (targetWidget == IntPtr.Zero) return default;

        // Get allocated size
        GtkAllocation allocation;
        gtk_widget_get_allocation(targetWidget, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed) return;

        IntPtr targetWidget = _isMultiLine ? _scrolledWindow : _widget;

        if (targetWidget != IntPtr.Zero)
        {
            if (visible)
                gtk_widget_show(targetWidget);
            else
                gtk_widget_hide(targetWidget);
        }
    }

    public bool GetVisible()
    {
        if (_disposed) return false;

        IntPtr targetWidget = _isMultiLine ? _scrolledWindow : _widget;

        if (targetWidget == IntPtr.Zero) return false;

        return gtk_widget_get_visible(targetWidget);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed) return;

        if (_widget != IntPtr.Zero)
        {
            gtk_widget_set_sensitive(_widget, enabled);
        }
    }

    public bool GetEnabled()
    {
        if (_disposed) return false;

        if (_widget == IntPtr.Zero) return false;

        return gtk_widget_get_sensitive(_widget);
    }

    public void SetBackground(RGB color)
    {
        // GTK3 background colors are typically set via CSS
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // GTK3 foreground colors are typically set via CSS
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_scrolledWindow != IntPtr.Zero)
            {
                gtk_widget_destroy(_scrolledWindow);
                _scrolledWindow = IntPtr.Zero;
            }
            else if (_widget != IntPtr.Zero)
            {
                gtk_widget_destroy(_widget);
            }

            _widget = IntPtr.Zero;
            _textBuffer = IntPtr.Zero;
            _disposed = true;
        }
    }

    private static string PtrToStringUTF8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

#if NETSTANDARD2_0
        // Manual UTF-8 decoding for netstandard2.0
        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0)
            length++;

        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
#else
        // Use built-in method for .NET Core 2.1+
        return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
#endif
    }

    // GTK P/Invoke declarations

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_entry_new();

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_entry_set_text(IntPtr entry, string text);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_entry_get_text(IntPtr entry);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_entry_set_visibility(IntPtr entry, bool visible);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_entry_set_invisible_char(IntPtr entry, uint ch);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_entry_set_max_length(IntPtr entry, int max);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_entry_set_alignment(IntPtr entry, float xalign);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_text_view_new();

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_text_view_get_buffer(IntPtr textView);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_view_set_editable(IntPtr textView, bool setting);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_view_set_wrap_mode(IntPtr textView, int wrapMode);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolledWindow, int hscrollbarPolicy, int vscrollbarPolicy);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_text_buffer_set_text(IntPtr buffer, string text, int len);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_text_buffer_get_text(IntPtr buffer, IntPtr start, IntPtr end, bool includeHiddenChars);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_buffer_get_start_iter(IntPtr buffer, IntPtr iter);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_buffer_get_end_iter(IntPtr buffer, IntPtr iter);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_buffer_get_iter_at_offset(IntPtr buffer, IntPtr iter, int charOffset);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_buffer_get_iter_at_mark(IntPtr buffer, IntPtr iter, IntPtr mark);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_text_buffer_select_range(IntPtr buffer, IntPtr ins, IntPtr bound);

    [DllImport("libgtk-3.so.0")]
    private static extern bool gtk_text_buffer_get_selection_bounds(IntPtr buffer, IntPtr start, IntPtr end);

    [DllImport("libgtk-3.so.0")]
    private static extern int gtk_text_iter_get_offset(IntPtr iter);

    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_text_buffer_get_insert(IntPtr buffer);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_text_buffer_insert(IntPtr buffer, IntPtr iter,
        string text, int len);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_editable_set_editable(IntPtr editable, bool isEditable);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_editable_select_region(IntPtr editable, int startPos, int endPos);

    [DllImport("libgtk-3.so.0")]
    private static extern unsafe bool gtk_editable_get_selection_bounds(IntPtr editable, int* startPos, int* endPos);

    [DllImport("libgtk-3.so.0")]
    private static extern int gtk_editable_get_position(IntPtr editable);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_editable_set_position(IntPtr editable, int position);

    [DllImport("libgtk-3.so.0", CharSet = CharSet.Ansi)]
    private static extern void gtk_editable_insert_text(IntPtr editable,
        string newText, int newTextLength, ref int position);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport("libgtk-3.so.0")]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport("libglib-2.0.so.0")]
    private static extern void g_free(IntPtr mem);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
