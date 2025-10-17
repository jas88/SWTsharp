using SWTSharp.Platform.Win32;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Group widget methods.
/// </summary>
internal partial class Win32Platform
{
    public IPlatformComposite CreateGroupWidget(IPlatformWidget? parent, int style, string text)
    {
        // Get parent handle
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Win32.Win32Composite composite)
        {
            parentHandle = composite.GetNativeHandle();
        }
        else if (parent is Win32.Win32Window window)
        {
            parentHandle = window.GetNativeHandle();
        }
        else if (parent is Win32.Win32Group group)
        {
            parentHandle = group.GetNativeHandle();
        }
        else if (parent is Win32.Win32TabFolder tabFolder)
        {
            parentHandle = tabFolder.GetNativeHandle();
        }

        return new Win32.Win32Group(parentHandle, style, text);
    }
}
