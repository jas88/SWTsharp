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
        // TODO: Implement Linux TabFolder using GTK GtkNotebook
        // For now, throw NotImplementedException to satisfy interface
        // This will be implemented when Linux platform work continues
        throw new NotImplementedException("CreateTabFolderWidget will be implemented for Linux in Phase 5.8");
    }
}
