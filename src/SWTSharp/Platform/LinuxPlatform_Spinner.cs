using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Spinner widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // GTK SpinButton imports for Spinner widget
    [DllImport(GtkLib)]
    private static extern IntPtr gtk_spin_button_new_with_range(double min, double max, double step);

    [DllImport(GtkLib)]
    private static extern void gtk_spin_button_set_value(IntPtr spin_button, double value);

    [DllImport(GtkLib)]
    private static extern double gtk_spin_button_get_value(IntPtr spin_button);

    [DllImport(GtkLib)]
    private static extern void gtk_spin_button_set_digits(IntPtr spin_button, uint digits);

    [DllImport(GtkLib)]
    private static extern void gtk_spin_button_set_wrap(IntPtr spin_button, bool wrap);

    [DllImport(GtkLib)]
    private static extern void gtk_spin_button_set_increments(IntPtr spin_button, double step, double page);

    [DllImport(GtkLib)]
    private static extern IntPtr gtk_spin_button_get_adjustment(IntPtr spin_button);

    [DllImport(GtkLib)]
    private static extern void gtk_adjustment_set_lower(IntPtr adjustment, double lower);

    [DllImport(GtkLib)]
    private static extern void gtk_adjustment_set_upper(IntPtr adjustment, double upper);

    // Spinner data storage
    private Dictionary<IntPtr, Action<int>> _spinnerCallbacks = new Dictionary<IntPtr, Action<int>>();
    private Dictionary<IntPtr, Action> _spinnerModifiedCallbacks = new Dictionary<IntPtr, Action>();

    // Spinner operations
    public IntPtr CreateSpinner(IntPtr parent, int style)
    {
        // Create GtkSpinButton
        IntPtr spinner = gtk_spin_button_new_with_range(0, 100, 1);

        // Set wrap behavior
        bool wrap = (style & SWT.WRAP) != 0;
        gtk_spin_button_set_wrap(spinner, wrap);

        gtk_widget_show(spinner);

        return spinner;
    }

    public void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        gtk_spin_button_set_value(handle, selection);

        // Update range
        IntPtr adjustment = gtk_spin_button_get_adjustment(handle);
        if (adjustment != IntPtr.Zero)
        {
            gtk_adjustment_set_lower(adjustment, minimum);
            gtk_adjustment_set_upper(adjustment, maximum);
        }

        // Set increments
        if (increment > 0 || pageIncrement > 0)
        {
            double step = increment > 0 ? increment : 1;
            double page = pageIncrement > 0 ? pageIncrement : 10;
            gtk_spin_button_set_increments(handle, step, page);
        }

        // Set decimal digits
        gtk_spin_button_set_digits(handle, (uint)Math.Max(0, digits));
    }

    public int GetSpinnerSelection(IntPtr handle)
    {
        double value = gtk_spin_button_get_value(handle);
        return (int)Math.Round(value);
    }

    public void ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    {
        _spinnerCallbacks[handle] = callback;
    }

    public void ConnectSpinnerModified(IntPtr handle, Action callback)
    {
        _spinnerModifiedCallbacks[handle] = callback;
    }

    public void SetSpinnerTextLimit(IntPtr handle, int limit)
    {
        // GTK SpinButton doesn't have a direct text limit API
        // The text is automatically managed based on the numeric range
        // This is a no-op for GTK implementation
    }
}
