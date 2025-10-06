using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - ProgressBar widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Helper method CreateIndexSetForAllColumns is in main MacOSPlatform.cs

    // ProgressBar operations
    // Shared selectors (_nsProgressIndicatorClass, _selSetMinValue, _selSetMaxValue, _selSetDoubleValue) are in main MacOSPlatform.cs
    private IntPtr _selSetIndeterminate;
    private IntPtr _selStartAnimation;
    private IntPtr _selStopAnimation;
    private IntPtr _selSetStyle;

    // NSProgressIndicator styles
    private const int NSProgressIndicatorBarStyle = 0;
    private const int NSProgressIndicatorSpinningStyle = 1;

    private void InitializeProgressBarSelectors()
    {
        // Note: _nsProgressIndicatorClass, _selSetMinValue, _selSetMaxValue, _selSetDoubleValue are initialized in main MacOSPlatform.Initialize()
        if (_selSetIndeterminate == IntPtr.Zero)
        {
            _selSetIndeterminate = sel_registerName("setIndeterminate:");
            _selStartAnimation = sel_registerName("startAnimation:");
            _selStopAnimation = sel_registerName("stopAnimation:");
            _selSetStyle = sel_registerName("setStyle:");
        }
    }

    // objc_msgSend_double is declared in main MacOSPlatform.cs

    public IntPtr CreateProgressBar(IntPtr parent, int style)
    {
        InitializeProgressBarSelectors();

        // Create NSProgressIndicator
        IntPtr progressBar = objc_msgSend(_nsProgressIndicatorClass, _selAlloc);
        progressBar = objc_msgSend(progressBar, _selInit);

        // Set style - bar style for determinate, spinning for indeterminate
        if ((style & SWT.INDETERMINATE) != 0)
        {
            objc_msgSend(progressBar, _selSetStyle, new IntPtr(NSProgressIndicatorSpinningStyle));
            objc_msgSend_void(progressBar, _selSetIndeterminate, true);
            // Start animation for indeterminate
            objc_msgSend(progressBar, _selStartAnimation, IntPtr.Zero);
        }
        else
        {
            objc_msgSend(progressBar, _selSetStyle, new IntPtr(NSProgressIndicatorBarStyle));
            objc_msgSend_void(progressBar, _selSetIndeterminate, false);
        }

        // Set default frame
        var frame = new CGRect(0, 0, 200, 20);
        objc_msgSend_rect(progressBar, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, progressBar);
        }

        return progressBar;
    }

    public void SetProgressBarRange(IntPtr handle, int minimum, int maximum)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeProgressBarSelectors();

        // Set min and max values
        objc_msgSend_double(handle, _selSetMinValue, (double)minimum);
        objc_msgSend_double(handle, _selSetMaxValue, (double)maximum);
    }

    public void SetProgressBarSelection(IntPtr handle, int value)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeProgressBarSelectors();

        // Set current value
        objc_msgSend_double(handle, _selSetDoubleValue, (double)value);
    }

    public void SetProgressBarState(IntPtr handle, int state)
    {
        if (handle == IntPtr.Zero)
            return;

        // macOS NSProgressIndicator doesn't have built-in state support (normal/error/paused)
        // State colors would need to be implemented via custom drawing or color changes
        // For now, this is a no-op on macOS
    }

    // Slider operations
}
