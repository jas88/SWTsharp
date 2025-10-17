namespace SWTSharp.Platform;

/// <summary>
/// macOS platform factory methods for Tracker widget.
/// </summary>
internal partial class MacOSPlatform
{
    /// <summary>
    /// Creates a macOS (Cocoa) tracker widget.
    /// </summary>
    public IPlatformTracker CreateTracker(IPlatformWidget? parent, int style)
    {
        IntPtr parentView = IntPtr.Zero;

        if (parent is MacOS.MacOSWindow macWindow)
        {
            parentView = macWindow.GetNativeHandle();
        }
        else if (parent is MacOS.MacOSComposite macComposite)
        {
            parentView = macComposite.GetNativeHandle();
        }

        return new MacOS.MacOSTracker(parentView, style);
    }
}
