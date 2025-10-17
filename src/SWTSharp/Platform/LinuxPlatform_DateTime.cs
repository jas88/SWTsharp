namespace SWTSharp.Platform;

/// <summary>
/// Linux GTK platform implementation - DateTime widget factory method.
/// </summary>
internal partial class LinuxPlatform
{
    public IPlatformDateTime CreateDateTimeWidget(IPlatformWidget? parent, int style)
    {
        // Get parent widget handle
        IntPtr parentWidget = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentWidget = linuxWidget.GetNativeHandle();
        }

        var dateTime = new Linux.LinuxDateTime(parentWidget, style);

        // Add to parent container if present
        if (parent is IPlatformComposite composite)
        {
            composite.AddChild(dateTime);
        }

        return dateTime;
    }
}
