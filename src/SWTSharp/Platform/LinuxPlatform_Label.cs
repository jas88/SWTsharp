using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK Label implementation - partial class extension
/// </summary>
internal partial class LinuxPlatform
{
    // GTK Label and Separator functions
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_label_new(string str);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_label_set_text(IntPtr label, string str);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_label_set_xalign(IntPtr label, float xalign);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_label_set_yalign(IntPtr label, float yalign);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_label_set_line_wrap(IntPtr label, bool wrap);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_separator_new(GtkOrientation orientation);

    // Note: GtkOrientation enum is defined in main LinuxPlatform.cs

    // Label operations
    public IntPtr CreateLabel(IntPtr parent, int style, int alignment, bool wrap)
    {
        IntPtr widget;

        // Check if this is a separator
        if ((style & SWT.SEPARATOR) != 0)
        {
            // Determine orientation
            GtkOrientation orientation = (style & SWT.VERTICAL) != 0
                ? GtkOrientation.Vertical
                : GtkOrientation.Horizontal;

            widget = gtk_separator_new(orientation);
        }
        else
        {
            // Create text label
            widget = gtk_label_new(string.Empty);

            // Set alignment
            float xalign = 0.0f; // Left
            if (alignment == SWT.CENTER)
            {
                xalign = 0.5f;
            }
            else if (alignment == SWT.RIGHT)
            {
                xalign = 1.0f;
            }
            gtk_label_set_xalign(widget, xalign);

            // Set wrapping
            gtk_label_set_line_wrap(widget, wrap);
        }

        if (widget == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK label");
        }

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            gtk_container_add(parent, widget);
        }

        gtk_widget_show(widget);

        return widget;
    }

    public void SetLabelText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid label handle", nameof(handle));
        }

        gtk_label_set_text(handle, text ?? string.Empty);
    }

    public void SetLabelAlignment(IntPtr handle, int alignment)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid label handle", nameof(handle));
        }

        // Set horizontal alignment
        float xalign = 0.0f; // Left
        if (alignment == SWT.CENTER)
        {
            xalign = 0.5f;
        }
        else if (alignment == SWT.RIGHT)
        {
            xalign = 1.0f;
        }

        gtk_label_set_xalign(handle, xalign);
    }
}
