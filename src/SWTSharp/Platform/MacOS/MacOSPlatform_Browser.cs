using System;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS platform implementation - Browser widget factory methods.
/// </summary>
internal partial class MacOSPlatform
{
    /// <summary>
    /// Creates a platform-specific browser widget using WKWebView.
    /// </summary>
    /// <param name="parent">Parent widget (optional)</param>
    /// <param name="style">Widget style flags</param>
    /// <returns>Platform browser implementation</returns>
    public IPlatformBrowser CreateBrowserWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;

        if (parent is MacOSWidget macOSWidget)
        {
            parentHandle = macOSWidget.GetNativeHandle();
        }

        var browser = new MacOSBrowser(parentHandle, style);

        // Note: Widget dictionary for event routing not yet implemented in MacOSPlatform
        // This can be added when MacOSPlatform implements a centralized event routing system

        return browser;
    }
}
