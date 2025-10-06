using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles;

/// <summary>
/// Base class for platform-specific graphics context handles (HDC, CGContext, cairo_t).
/// Provides safe handle management with automatic resource cleanup for graphics operations.
/// </summary>
/// <remarks>
/// Graphics contexts are used for drawing operations and must be properly released
/// to avoid resource leaks. This class ensures thread-safe cleanup.
/// Supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public abstract class SafeGraphicsHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeGraphicsHandle"/> class.
    /// </summary>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release (not recommended).
    /// </param>
    protected SafeGraphicsHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeGraphicsHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An IntPtr object that represents the existing handle.</param>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release.
    /// </param>
    protected SafeGraphicsHandle(IntPtr existingHandle, bool ownsHandle)
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
    /// Executes the code required to free the graphics context handle.
    /// </summary>
    /// <returns>
    /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
    /// </returns>
    /// <remarks>
    /// This method is called by the SafeHandle infrastructure and must be implemented
    /// by derived classes to provide platform-specific cleanup of graphics resources.
    /// This method runs in a constrained execution region (CER) to guarantee execution.
    /// </remarks>
    protected abstract override bool ReleaseHandle();

    /// <summary>
    /// Creates a platform-specific graphics context handle.
    /// </summary>
    /// <param name="windowHandle">The window handle to create a graphics context for.</param>
    /// <returns>A new SafeGraphicsHandle instance for the created graphics context.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    public static SafeGraphicsHandle CreatePlatformGraphicsContext(IntPtr windowHandle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Win32.Win32GraphicsHandle.Create(windowHandle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOS.MacOSGraphicsHandle.Create(windowHandle);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Linux.LinuxGraphicsHandle.Create(windowHandle);
        }
        else
        {
            throw new PlatformNotSupportedException("Current platform is not supported for graphics context creation.");
        }
    }
}
