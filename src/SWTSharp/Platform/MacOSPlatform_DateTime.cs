using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - DateTime widget factory method.
/// </summary>
internal partial class MacOSPlatform
{
    public IPlatformDateTime CreateDateTimeWidget(IPlatformWidget? parent, int style)
    {
        // Get parent view handle
        IntPtr parentView = IntPtr.Zero;
        if (parent is MacOS.MacOSWidget macOSWidget)
        {
            parentView = macOSWidget.GetNativeHandle();
        }

        var dateTime = new MacOS.MacOSDateTime(parentView, style);

        // Add to parent container if present
        if (parent is IPlatformComposite composite)
        {
            composite.AddChild(dateTime);
        }

        return dateTime;
    }
}
