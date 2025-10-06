using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace SWTSharp.Platform.SafeHandles;

/// <summary>
/// Base class for platform-specific window handles (HWND, NSWindow, GtkWindow).
/// Provides safe handle management with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This abstract class derives from SafeHandle and must be implemented by
/// platform-specific subclasses to provide proper handle release semantics.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public abstract class SafeWindowHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeWindowHandle"/> class.
    /// </summary>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release (not recommended).
    /// </param>
    protected SafeWindowHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeWindowHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An IntPtr object that represents the existing handle.</param>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release.
    /// </param>
    protected SafeWindowHandle(IntPtr existingHandle, bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
        SetHandle(existingHandle);
    }

    /// <summary>
    /// Gets a value indicating whether the handle value is invalid.
    /// </summary>
    public override bool IsInvalid
    {
        get { return handle == IntPtr.Zero; }
    }

    /// <summary>
    /// Executes the code required to free the handle.
    /// </summary>
    /// <returns>
    /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
    /// </returns>
    /// <remarks>
    /// This method is called by the SafeHandle infrastructure and must be implemented
    /// by derived classes to provide platform-specific cleanup.
    /// This method runs in a constrained execution region (CER) to guarantee execution.
    /// </remarks>
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
    protected abstract override bool ReleaseHandle();

    /// <summary>
    /// Creates a platform-specific window handle.
    /// </summary>
    /// <param name="style">The window style flags.</param>
    /// <param name="title">The window title.</param>
    /// <returns>A new SafeWindowHandle instance for the created window.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    public static SafeWindowHandle CreatePlatformWindow(int style, string title)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Win32.Win32WindowHandle.Create(style, title);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOS.MacOSWindowHandle.Create(style, title);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Linux.LinuxWindowHandle.Create(style, title);
        }
        else
        {
            throw new PlatformNotSupportedException("Current platform is not supported for window creation.");
        }
    }
}
