using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Slider widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Slider widget selectors and data
    private IntPtr _nsSliderClass;
    private IntPtr _selSetSliderType;
    private readonly Dictionary<IntPtr, Action<int>> _sliderCallbacks = new Dictionary<IntPtr, Action<int>>();

    // NSSlider types
    private const int NSLinearSlider = 0;
    private const int NSCircularSlider = 1;

    // objc_msgSend_double is declared in main MacOSPlatform.cs

    // REMOVED METHODS (moved to ISliderWidget interface):
    // - CreateSlider(IntPtr parent, int style)
    // - SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    // - ConnectSliderChanged(IntPtr handle, Action<int> callback)
    // These methods are now implemented via the ISliderWidget interface using proper handles

    private void InitializeSliderSelectors()
    {
        if (_nsSliderClass == IntPtr.Zero)
        {
            _nsSliderClass = objc_getClass("NSSlider");
            _selSetSliderType = sel_registerName("setSliderType:");

            // Reuse selectors if already initialized
            if (_selSetMinValue == IntPtr.Zero)
                _selSetMinValue = sel_registerName("setMinValue:");
            if (_selSetMaxValue == IntPtr.Zero)
                _selSetMaxValue = sel_registerName("setMaxValue:");
            if (_selSetDoubleValue == IntPtr.Zero)
                _selSetDoubleValue = sel_registerName("setDoubleValue:");
        }
    }

    // Shared selectors (_selSetMinValue, _selSetMaxValue, _selSetDoubleValue) are in main MacOSPlatform.cs
}
