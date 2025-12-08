namespace SWTSharp.Platform;

/// <summary>
/// Linux (GTK) platform implementation - ScrollBar widget factory methods.
/// </summary>
internal partial class LinuxPlatform
{
    public IPlatformScrollBar CreateScrollBarWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        return new Linux.LinuxScrollBar(parentHandle, style);
    }
}
