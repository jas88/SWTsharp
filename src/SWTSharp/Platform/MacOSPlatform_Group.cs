using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Group widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    public IPlatformComposite CreateGroupWidget(IPlatformWidget? parent, int style, string text)
    {
        // Get parent handle
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is MacOS.MacOSComposite composite)
        {
            parentHandle = composite.GetNativeHandle();
        }
        else if (parent is MacOS.MacOSWindow window)
        {
            parentHandle = window.GetNativeHandle();
        }
        else if (parent is MacOS.MacOSGroup group)
        {
            // Use the content view handle for nested groups
            parentHandle = group.GetContentViewHandle();
        }
        else if (parent is MacOS.MacOSTabFolder tabFolder)
        {
            parentHandle = tabFolder.GetNativeHandle();
        }

        return new MacOS.MacOSGroup(parentHandle, style, text);
    }
}
