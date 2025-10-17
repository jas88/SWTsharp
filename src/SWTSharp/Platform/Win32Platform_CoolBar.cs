using SWTSharp.Platform.Win32;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - CoolBar widget factory.
/// </summary>
internal partial class Win32Platform
{
    /// <summary>
    /// Creates a CoolBar widget (rearrangeable toolbar container).
    /// </summary>
    public IPlatformCoolBar CreateCoolBarWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;
        return new Win32CoolBar(parentHandle, style);
    }
}
