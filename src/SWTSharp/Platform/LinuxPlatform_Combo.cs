using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Combo Box widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK ComboBox imports
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_combo_box_text_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_combo_box_text_new_with_entry();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_combo_box_text_append_text(IntPtr combo_box, string text);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_combo_box_text_insert_text(IntPtr combo_box, int position, string text);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_combo_box_text_remove(IntPtr combo_box, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_combo_box_text_remove_all(IntPtr combo_box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_combo_box_text_get_active_text(IntPtr combo_box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_combo_box_get_active(IntPtr combo_box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_combo_box_set_active(IntPtr combo_box, int index);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_bin_get_child(IntPtr bin);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_combo_box_popup(IntPtr combo_box);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_combo_box_popdown(IntPtr combo_box);

    // Combo control operations
    public IntPtr CreateCombo(IntPtr parentHandle, int style)
    {
        IntPtr combo;

        if ((style & SWT.READ_ONLY) != 0)
        {
            // Create read-only combo box (no entry field)
            combo = gtk_combo_box_text_new();
        }
        else
        {
            // Create editable combo box (with entry field)
            combo = gtk_combo_box_text_new_with_entry();
        }

        if (combo == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK combo box");
        }

        // Add to parent if provided (use helper to handle GtkWindow containers)
        if (parentHandle != IntPtr.Zero)
        {
            AddChildToParent(parentHandle, combo);
        }

        gtk_widget_show(combo);

        return combo;
    }

    public void SetComboText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero) return;

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            gtk_entry_set_text(entry, text ?? string.Empty);
        }
    }

    public string GetComboText(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return string.Empty;

        // Try to get active text first (works for both editable and read-only)
        IntPtr textPtr = gtk_combo_box_text_get_active_text(handle);
        if (textPtr != IntPtr.Zero)
        {
#if NETSTANDARD2_0
            string result = Marshal.PtrToStringAnsi(textPtr) ?? string.Empty;
#else
            string result = Marshal.PtrToStringUTF8(textPtr) ?? string.Empty;
#endif
            g_free(textPtr);
            return result;
        }

        // For editable combos, get text from entry
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            IntPtr entryTextPtr = gtk_entry_get_text(entry);
#if NETSTANDARD2_0
            return Marshal.PtrToStringAnsi(entryTextPtr) ?? string.Empty;
#else
            return Marshal.PtrToStringUTF8(entryTextPtr) ?? string.Empty;
#endif
        }

        return string.Empty;
    }

    public void AddComboItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero) return;

        if (index < 0)
        {
            // Append to end
            gtk_combo_box_text_append_text(handle, item ?? string.Empty);
        }
        else
        {
            // Insert at specific position
            gtk_combo_box_text_insert_text(handle, index, item ?? string.Empty);
        }
    }

    public void RemoveComboItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || index < 0) return;
        gtk_combo_box_text_remove(handle, index);
    }

    public void ClearComboItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;
        gtk_combo_box_text_remove_all(handle);
    }

    public void SetComboSelection(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero) return;
        gtk_combo_box_set_active(handle, index);
    }

    public int GetComboSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return -1;
        return gtk_combo_box_get_active(handle);
    }

    public void SetComboTextLimit(IntPtr handle, int limit)
    {
        if (handle == IntPtr.Zero) return;

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            gtk_entry_set_max_length(entry, limit);
        }
    }

    public void SetComboVisibleItemCount(IntPtr handle, int count)
    {
        if (handle == IntPtr.Zero || count < 1) return;

        // GtkComboBox doesn't have a direct way to set visible item count
        // This would require custom popup sizing
        // For now, this is a no-op
    }

    public void SetComboTextSelection(IntPtr handle, int start, int end)
    {
        if (handle == IntPtr.Zero) return;

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            gtk_editable_select_region(entry, start, end);
        }
    }

    public (int Start, int End) GetComboTextSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return (0, 0);

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            if (gtk_editable_get_selection_bounds(entry, out int start, out int end))
            {
                return (start, end);
            }
        }

        return (0, 0);
    }

    public void ComboTextCopy(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            // Copy selection to clipboard
            g_signal_emit_by_name(entry, "copy-clipboard");
        }
    }

    public void ComboTextCut(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            // Cut selection to clipboard
            g_signal_emit_by_name(entry, "cut-clipboard");
        }
    }

    public void ComboTextPaste(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Get the entry child widget (for editable combos)
        IntPtr entry = gtk_bin_get_child(handle);
        if (entry != IntPtr.Zero)
        {
            // Paste from clipboard
            g_signal_emit_by_name(entry, "paste-clipboard");
        }
    }

    // Helper function for emitting signals
    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void g_signal_emit_by_name(IntPtr instance, string detailed_signal);
}
