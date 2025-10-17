namespace SWTSharp.Platform;

/// <summary>
/// Win32 platform factory methods for Tracker widget.
/// </summary>
internal partial class Win32Platform
{
    /// <summary>
    /// Creates a Win32 tracker widget.
    /// </summary>
    public IPlatformTracker CreateTracker(IPlatformWidget? parent, int style)
    {
        IntPtr parentHwnd = IntPtr.Zero;

        if (parent is Win32.Win32Window win32Window)
        {
            parentHwnd = win32Window.GetNativeHandle();
        }
        else if (parent is Win32.Win32Composite win32Composite)
        {
            parentHwnd = win32Composite.GetNativeHandle();
        }

        return new Win32.Win32Tracker(parentHwnd, style);
    }
}
