namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - DateTime widget factory method.
/// </summary>
internal partial class Win32Platform
{
    public IPlatformDateTime CreateDateTimeWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating DateTime widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var dateTime = new Win32.Win32DateTime(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] DateTime widget created successfully");

        return dateTime;
    }
}
