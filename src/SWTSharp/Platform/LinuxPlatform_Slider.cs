using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Slider widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK Scale/Range imports for Slider widget
    [DllImport(GtkLib)]
    private static extern IntPtr gtk_scale_new_with_range(GtkOrientation orientation, double min, double max, double step);

    [DllImport(GtkLib)]
    private static extern void gtk_range_set_range(IntPtr range, double min, double max);

    [DllImport(GtkLib)]
    private static extern void gtk_range_set_value(IntPtr range, double value);

    [DllImport(GtkLib)]
    private static extern double gtk_range_get_value(IntPtr range);

    [DllImport(GtkLib)]
    private static extern void gtk_range_set_increments(IntPtr range, double step, double page);

    [DllImport(GtkLib)]
    private static extern void gtk_scale_set_draw_value(IntPtr scale, bool draw_value);

    // Slider data storage
    private Dictionary<IntPtr, Action<int>> _sliderCallbacks = new Dictionary<IntPtr, Action<int>>();

    // Slider operations
    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        GtkOrientation orientation = (style & SWT.VERTICAL) != 0
            ? GtkOrientation.Vertical
            : GtkOrientation.Horizontal;

        // Create GtkScale (used for both sliders and scales in GTK)
        IntPtr scale = gtk_scale_new_with_range(orientation, 0, 100, 1);

        // Don't show value by default for slider
        gtk_scale_set_draw_value(scale, false);

        gtk_widget_show(scale);

        return scale;
    }

    public void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
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

    public void ConnectSliderChanged(IntPtr handle, Action<int> callback)
    {
        _sliderCallbacks[handle] = callback;
        // Would connect "value-changed" signal here
    }
}
