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
        // TODO: Implement macOS TabFolder using existing MacOSPlatform_TabFolder.cs code
        // For now, throw NotImplementedException to satisfy interface
        // This will be implemented when macOS platform work continues
        throw new NotImplementedException("CreateTabFolderWidget will be implemented for macOS in Phase 5.8");
    }
}
