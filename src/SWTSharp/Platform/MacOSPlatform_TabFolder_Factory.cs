using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - TabFolder widget factory.
/// </summary>
internal partial class MacOSPlatform
{
    /// <summary>
    /// Creates a TabFolder widget (tab control).
    /// </summary>
    public IPlatformTabFolder CreateTabFolderWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = MacOSPlatformHelpers.GetParentHandle(parent);

        if (_enableLogging)
            Console.WriteLine($"[macOS] Creating tab folder widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        return new MacOSTabFolder(parentHandle, style);
    }
}
