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

    // REMOVED METHODS (moved to IProgressBarWidget interface):
    // - CreateProgressBar(IntPtr parent, int style)
    // - SetProgressBarRange(IntPtr handle, int minimum, int maximum)
    // - SetProgressBarSelection(IntPtr handle, int value)
    // - SetProgressBarState(IntPtr handle, int state)
    // These methods are now implemented via the IProgressBarWidget interface using proper handles

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

    // Slider operations
}
