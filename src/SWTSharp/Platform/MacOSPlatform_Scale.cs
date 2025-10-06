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

    public IntPtr CreateScale(IntPtr parent, int style)
    {
        InitializeScaleSelectors();

        // Create NSSlider (Scale is implemented as NSSlider with tick marks)
        IntPtr scale = objc_msgSend(_nsSliderClass, _selAlloc);
        scale = objc_msgSend(scale, _selInit);

        // Set slider type based on style
        bool isVertical = (style & SWT.VERTICAL) != 0;
        objc_msgSend(scale, _selSetSliderType, new IntPtr(NSLinearSlider));

        // Set default frame (orientation handled by width/height ratio)
        var frame = isVertical ? new CGRect(0, 0, 20, 200) : new CGRect(0, 0, 200, 20);
        objc_msgSend_rect(scale, _selSetFrame, frame);

        // Set default values
        objc_msgSend_double(scale, _selSetMinValue, 0.0);
        objc_msgSend_double(scale, _selSetMaxValue, 100.0);
        objc_msgSend_double(scale, _selSetDoubleValue, 0.0);

        // Configure tick marks for Scale
        objc_msgSend(scale, _selSetNumberOfTickMarks, new IntPtr(11)); // 0, 10, 20, ..., 100
        objc_msgSend_void(scale, _selSetAllowsTickMarkValuesOnly, false);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, scale);
        }

        return scale;
    }

    public void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeScaleSelectors();

        // Set min and max values
        objc_msgSend_double(handle, _selSetMinValue, (double)minimum);
        objc_msgSend_double(handle, _selSetMaxValue, (double)maximum);

        // Set current value
        objc_msgSend_double(handle, _selSetDoubleValue, (double)selection);

        // Calculate number of tick marks based on increment
        if (increment > 0 && maximum > minimum)
        {
            int numTicks = ((maximum - minimum) / increment) + 1;
            objc_msgSend(handle, _selSetNumberOfTickMarks, new IntPtr(Math.Max(2, numTicks)));
        }
    }

    public void ConnectScaleChanged(IntPtr handle, Action<int> callback)
    {
        if (handle == IntPtr.Zero)
            return;

        _scaleCallbacks[handle] = callback;

        // Note: In a full implementation, you would set up target-action pattern here
        // This requires creating a custom Objective-C class to handle callbacks
        // For now, this stores the callback for future implementation
    }
}
