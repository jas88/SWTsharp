using System;

namespace SWTSharp.Platform;

/// <summary>
/// Linux platform implementation - Browser widget factory method.
/// </summary>
internal partial class LinuxPlatform
{
    /// <summary>
    /// Creates a browser widget using WebKitGTK.
    /// </summary>
    /// <param name="parent">Parent widget (optional)</param>
    /// <param name="style">Widget style flags</param>
    /// <returns>Platform-specific browser widget</returns>
    public IPlatformBrowser CreateBrowserWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        if (_enableLogging)
            Console.WriteLine($"[Linux] Creating browser widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var browser = new Linux.LinuxBrowser(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Linux] Browser widget created successfully using WebKitGTK");

        return browser;
    }
}
