namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - ExpandBar widget factory.
/// </summary>
internal partial class LinuxPlatform
{
    /// <summary>
    /// Creates an ExpandBar widget (accordion control using GtkExpander).
    /// </summary>
    public IPlatformExpandBar CreateExpandBarWidget(IPlatformWidget? parent, int style)
    {
        // Extract parent handle, handling null case
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxPlatform] Creating ExpandBar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var expandBar = new Linux.LinuxExpandBar(parentHandle, style);
        return expandBar;
    }
}
