namespace SWTSharp.Platform;

/// <summary>
/// Linux platform factory methods for Tracker widget.
/// </summary>
internal partial class LinuxPlatform
{
    /// <summary>
    /// Creates a Linux (GTK) tracker widget.
    /// </summary>
    public IPlatformTracker CreateTracker(IPlatformWidget? parent, int style)
    {
        IntPtr parentWidget = IntPtr.Zero;

        if (parent is Linux.LinuxWindow linuxWindow)
        {
            parentWidget = linuxWindow.GetNativeHandle();
        }
        else if (parent is Linux.LinuxComposite linuxComposite)
        {
            parentWidget = linuxComposite.GetNativeHandle();
        }

        return new Linux.LinuxTracker(parentWidget, style);
    }
}
