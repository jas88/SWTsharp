using SWTSharp.Platform.Linux;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - CoolBar widget factory.
/// </summary>
internal partial class LinuxPlatform
{
    /// <summary>
    /// Creates a CoolBar widget (rearrangeable toolbar container).
    /// </summary>
    public IPlatformCoolBar CreateCoolBarWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }
        return new LinuxCoolBar(parentHandle, style);
    }
}
