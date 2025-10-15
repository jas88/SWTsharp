using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Group widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Group widget selectors and constants
    private IntPtr _nsBoxClass;
    private IntPtr _selSetTitlePosition;
    private IntPtr _selSetBorderType;

    // NSBox title positions
    private const int NSNoTitle = 0;
    private const int NSAboveTop = 1;
    private const int NSAtTop = 2;
    private const int NSBelowTop = 3;

    // NSBox border types
    private const int NSNoBorder = 0;
    private const int NSLineBorder = 1;
    private const int NSBezelBorder = 2;
    private const int NSGrooveBorder = 3;

    // REMOVED METHODS (moved to IGroupWidget interface):
    // - CreateGroup(IntPtr parent, int style, string text)
    // - SetGroupText(IntPtr handle, string text)
    // These methods are now implemented via the IGroupWidget interface using proper handles

    private void InitializeGroupSelectors()
    {
        if (_nsBoxClass == IntPtr.Zero)
        {
            _nsBoxClass = objc_getClass("NSBox");
            _selSetTitlePosition = sel_registerName("setTitlePosition:");
            _selSetBorderType = sel_registerName("setBorderType:");
        }
    }
}
