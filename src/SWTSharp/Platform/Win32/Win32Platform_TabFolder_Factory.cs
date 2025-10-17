namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - TabFolder widget factory.
/// </summary>
internal partial class Win32Platform
{
    /// <summary>
    /// Creates a TabFolder widget (tab control).
    /// </summary>
    public IPlatformTabFolder CreateTabFolderWidget(IPlatformWidget? parent, int style)
    {
        // Extract parent handle, handling null case
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Platform] Creating TabFolder widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var tabFolder = new Win32.Win32TabFolder(this, parentHandle, style);
        return tabFolder;
    }
}
