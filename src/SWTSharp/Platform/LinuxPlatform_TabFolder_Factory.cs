namespace SWTSharp.Platform;

/// <summary>
/// Linux platform implementation - TabFolder widget factory.
/// </summary>
internal partial class LinuxPlatform
{
    /// <summary>
    /// Creates a TabFolder widget (tab control).
    /// </summary>
    public IPlatformTabFolder CreateTabFolderWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        return new Linux.LinuxTabFolder(parentHandle, style);
    }
}
