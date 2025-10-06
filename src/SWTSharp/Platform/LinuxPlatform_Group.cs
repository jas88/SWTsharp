using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Group widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK Frame imports for Group widget
    [DllImport(GtkLib)]
    private static extern IntPtr gtk_frame_new(string label);

    [DllImport(GtkLib)]
    private static extern void gtk_frame_set_label(IntPtr frame, string label);

    // Group operations
    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        // Create GtkFrame with label
        IntPtr frame = gtk_frame_new(text);
        gtk_widget_show(frame);
        return frame;
    }

    public void SetGroupText(IntPtr handle, string text)
    {
        gtk_frame_set_label(handle, text);
    }
}
