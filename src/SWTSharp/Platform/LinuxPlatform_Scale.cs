using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Scale widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK Scale imports (additional beyond Slider imports)
    [DllImport(GtkLib)]
    private static extern void gtk_scale_set_digits(IntPtr scale, int digits);

    // Scale data storage
    private Dictionary<IntPtr, Action<int>> _scaleCallbacks = new Dictionary<IntPtr, Action<int>>();

    // Scale operations
    public IntPtr CreateScale(IntPtr parent, int style)
    {
        GtkOrientation orientation = (style & SWT.VERTICAL) != 0
            ? GtkOrientation.Vertical
            : GtkOrientation.Horizontal;

        // Create GtkScale with value display
        IntPtr scale = gtk_scale_new_with_range(orientation, 0, 100, 1);

        // Show value for scale
        gtk_scale_set_draw_value(scale, true);
        gtk_scale_set_digits(scale, 0); // No decimal places

        gtk_widget_show(scale);

        return scale;
    }

    public void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        gtk_range_set_range(handle, minimum, maximum);
        gtk_range_set_value(handle, selection);

        if (increment > 0 || pageIncrement > 0)
        {
            double step = increment > 0 ? increment : 1;
            double page = pageIncrement > 0 ? pageIncrement : 10;
            gtk_range_set_increments(handle, step, page);
        }
    }

    public void ConnectScaleChanged(IntPtr handle, Action<int> callback)
    {
        _scaleCallbacks[handle] = callback;
    }
}
