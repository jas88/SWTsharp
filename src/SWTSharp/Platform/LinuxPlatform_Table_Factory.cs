namespace SWTSharp.Platform;

/// <summary>
/// Linux platform implementation - Table widget factory methods.
/// </summary>
internal partial class LinuxPlatform
{
    public IPlatformTable CreateTableWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[LinuxPlatform] Creating Table widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var table = new Linux.LinuxTable(parentHandle, style);

        if (enableLogging)
            Console.WriteLine("[LinuxPlatform] Table widget created successfully");

        return table;
    }
}
