namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Table widget factory methods.
/// </summary>
internal partial class Win32Platform
{
    public IPlatformTable CreateTableWidget(IPlatformWidget? parent, int style)
    {
        // Extract parent handle, handling null case
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Win32Platform] Creating Table widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var table = new Win32.Win32Table(parentHandle, style);

        if (enableLogging)
            Console.WriteLine("[Win32Platform] Table widget created successfully");

        return table;
    }
}
