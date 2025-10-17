namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - ExpandBar widget factory.
/// </summary>
internal partial class Win32Platform
{
    /// <summary>
    /// Creates an ExpandBar widget (accordion control).
    /// </summary>
    public IPlatformExpandBar CreateExpandBarWidget(IPlatformWidget? parent, int style)
    {
        // Extract parent handle, handling null case
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Platform] Creating ExpandBar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var expandBar = new Win32.Win32ExpandBar(this, parentHandle, style);
        return expandBar;
    }
}
