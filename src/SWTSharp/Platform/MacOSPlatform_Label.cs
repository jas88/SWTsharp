using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS Label implementation - partial class extension
/// </summary>
internal partial class MacOSPlatform
{
    // REMOVED METHODS (moved to ILabelWidget interface):
    // - CreateLabel(IntPtr parent, int style, int alignment, bool wrap)
    // - SetLabelText(IntPtr handle, string text)
    // - SetLabelAlignment(IntPtr handle, int alignment)
    // These methods are now implemented via the ILabelWidget interface using proper handles
}
