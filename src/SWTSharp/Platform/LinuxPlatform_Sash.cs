namespace SWTSharp.Platform;

/// <summary>
/// Linux (GTK) platform implementation - Sash widget factory methods.
/// </summary>
internal partial class LinuxPlatform
{
    public IPlatformSash CreateSashWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        return new Linux.LinuxSash(parentHandle, style);
    }
}
