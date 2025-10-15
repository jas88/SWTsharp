using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// Helper methods for macOS platform implementations.
/// </summary>
internal static class MacOSPlatformHelpers
{
    /// <summary>
    /// Unified parent handle resolution method.
    /// Extracts native handle from platform widget with fallback to Handle property.
    /// </summary>
    /// <param name="parent">Parent platform widget (can be null for top-level windows)</param>
    /// <returns>Native handle or IntPtr.Zero if no parent</returns>
    public static IntPtr GetParentHandle(IPlatformWidget? parent)
    {
        if (parent is MacOSWidget macOSParent)
        {
            return macOSParent.GetNativeHandle();
        }

        // For widgets that don't yet have platform widgets, fall back to Handle
        // This maintains backward compatibility during migration
        return IntPtr.Zero;
    }

    /// <summary>
    /// Converts SWT alignment constants to NSTextAlignment values.
    /// </summary>
    /// <param name="swtAlignment">SWT alignment constant</param>
    /// <returns>NSTextAlignment value</returns>
    public static int ConvertAlignment(int swtAlignment)
    {
        return swtAlignment switch
        {
            SWT.LEFT => 0, // NSTextAlignmentLeft
            SWT.CENTER => 1, // NSTextAlignmentCenter
            SWT.RIGHT => 2, // NSTextAlignmentRight
            _ => 0 // Default to left alignment
        };
    }
}