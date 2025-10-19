using System;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

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
#if NET5_0_OR_GREATER
    [RequiresDynamicCode("Browser widget may use WebView2 on Windows which requires dynamic code generation")]
    [RequiresUnreferencedCode("Browser widget may use WebView2 on Windows which uses reflection and COM interop")]
#endif
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
