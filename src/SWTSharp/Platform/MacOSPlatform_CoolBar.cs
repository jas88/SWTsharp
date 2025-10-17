using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - CoolBar widget factory.
/// </summary>
internal partial class MacOSPlatform
{
    /// <summary>
    /// Creates a CoolBar widget (rearrangeable toolbar container).
    /// </summary>
    public IPlatformCoolBar CreateCoolBarWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = MacOSPlatformHelpers.GetParentHandle(parent);
        return new MacOSCoolBar(parentHandle, style);
    }
}
