using SWTSharp.Platform.Linux;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - Group widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    public IPlatformComposite CreateGroupWidget(IPlatformWidget? parent, int style, string text)
    {
        // Get parent handle
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxComposite composite)
        {
            parentHandle = composite.GetNativeHandle();
        }
        else if (parent is Linux.LinuxWindow window)
        {
            parentHandle = window.GetNativeHandle();
        }
        else if (parent is Linux.LinuxGroup group)
        {
            // Use the internal container handle for nested groups
            parentHandle = group.GetContainerHandle();
        }
        else if (parent is Linux.LinuxTabFolder tabFolder)
        {
            parentHandle = tabFolder.GetNativeHandle();
        }

        return new Linux.LinuxGroup(parentHandle, style, text);
    }
}
