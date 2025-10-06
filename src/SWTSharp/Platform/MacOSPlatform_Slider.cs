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

    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        InitializeSliderSelectors();

        // Create NSSlider
        IntPtr slider = objc_msgSend(_nsSliderClass, _selAlloc);
        slider = objc_msgSend(slider, _selInit);

        // Set slider type based on style
        bool isVertical = (style & SWT.VERTICAL) != 0;
        objc_msgSend(slider, _selSetSliderType, new IntPtr(NSLinearSlider));

        // Set default frame (orientation handled by width/height ratio)
        var frame = isVertical ? new CGRect(0, 0, 20, 200) : new CGRect(0, 0, 200, 20);
        objc_msgSend_rect(slider, _selSetFrame, frame);

        // Set default values
        objc_msgSend_double(slider, _selSetMinValue, 0.0);
        objc_msgSend_double(slider, _selSetMaxValue, 100.0);
        objc_msgSend_double(slider, _selSetDoubleValue, 0.0);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, slider);
        }

        return slider;
    }

    public void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeSliderSelectors();

        // Set min and max values
        objc_msgSend_double(handle, _selSetMinValue, (double)minimum);
        objc_msgSend_double(handle, _selSetMaxValue, (double)maximum);

        // Set current value
        objc_msgSend_double(handle, _selSetDoubleValue, (double)selection);

        // Note: NSSlider doesn't have direct thumb, increment, or pageIncrement properties
        // These would need to be handled through custom behaviors or ignored
    }

    public void ConnectSliderChanged(IntPtr handle, Action<int> callback)
    {
        if (handle == IntPtr.Zero)
            return;

        _sliderCallbacks[handle] = callback;

        // Note: In a full implementation, you would set up target-action pattern here
        // This requires creating a custom Objective-C class to handle callbacks
        // For now, this stores the callback for future implementation
    }

    // Shared selectors (_selSetMinValue, _selSetMaxValue, _selSetDoubleValue) are in main MacOSPlatform.cs
}
