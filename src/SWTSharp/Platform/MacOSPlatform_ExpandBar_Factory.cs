using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - ExpandBar widget factory.
/// </summary>
internal partial class MacOSPlatform
{
    /// <summary>
    /// Creates an ExpandBar widget (accordion control with disclosure triangles).
    /// </summary>
    public IPlatformExpandBar CreateExpandBarWidget(IPlatformWidget? parent, int style)
    {
        // Extract parent handle, handling null case
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is MacOS.MacOSWidget macOSWidget)
        {
            parentHandle = macOSWidget.GetNativeHandle();
        }

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSPlatform] Creating ExpandBar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var expandBar = new MacOS.MacOSExpandBar(parentHandle, style);
        return expandBar;
    }
}
