using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Scale widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Scale widget selectors and data
    private IntPtr _selSetNumberOfTickMarks;
    private IntPtr _selSetAllowsTickMarkValuesOnly;
    private readonly Dictionary<IntPtr, Action<int>> _scaleCallbacks = new Dictionary<IntPtr, Action<int>>();

    // REMOVED METHODS (moved to IScaleWidget interface):
    // - CreateScale(IntPtr parent, int style)
    // - SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    // - ConnectScaleChanged(IntPtr handle, Action<int> callback)
    // These methods are now implemented via the IScaleWidget interface using proper handles

    private void InitializeScaleSelectors()
    {
        // Scale uses NSSlider, so initialize slider selectors first
        InitializeSliderSelectors();

        if (_selSetNumberOfTickMarks == IntPtr.Zero)
        {
            _selSetNumberOfTickMarks = sel_registerName("setNumberOfTickMarks:");
            _selSetAllowsTickMarkValuesOnly = sel_registerName("setAllowsTickMarkValuesOnly:");
        }
    }
}
