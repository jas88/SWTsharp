namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Table widget factory methods.
/// </summary>
internal partial class MacOSPlatform
{
    public IPlatformTable CreateTableWidget(IPlatformWidget? parent, int style)
    {
        // Extract parent handle, handling null case
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is MacOS.MacOSWidget macWidget)
        {
            parentHandle = macWidget.GetNativeHandle();
        }

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSPlatform] Creating Table widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var table = new MacOS.MacOSTable(parentHandle, style);

        if (enableLogging)
            Console.WriteLine("[MacOSPlatform] Table widget created successfully");

        return table;
    }
}
