using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles;

/// <summary>
/// Base class for platform-specific image/bitmap handles (HBITMAP, NSImage, GdkPixbuf).
/// Provides safe handle management with automatic resource cleanup for image resources.
/// </summary>
/// <remarks>
/// Image handles represent native bitmap or image objects used for graphics operations.
/// This class ensures proper cleanup and thread-safe handle management.
/// Supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public abstract class SafeImageHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeImageHandle"/> class.
    /// </summary>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release (not recommended).
    /// </param>
    protected SafeImageHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeImageHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An IntPtr object that represents the existing handle.</param>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release.
    /// </param>
    protected SafeImageHandle(IntPtr existingHandle, bool ownsHandle)
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
    /// Executes the code required to free the image handle.
    /// </summary>
    /// <returns>
    /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
    /// </returns>
    /// <remarks>
    /// This method is called by the SafeHandle infrastructure and must be implemented
    /// by derived classes to provide platform-specific cleanup of image resources.
    /// This method runs in a constrained execution region (CER) to guarantee execution.
    /// </remarks>
    protected abstract override bool ReleaseHandle();

    /// <summary>
    /// Creates a platform-specific image handle.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <returns>A new SafeImageHandle instance for the created image.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    public static SafeImageHandle CreatePlatformImage(int width, int height)
    {
#if WINDOWS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Win32.Win32ImageHandle.Create(width, height);
        }
#endif
#if MACOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOS.MacOSImageHandle.Create(width, height);
        }
#endif
#if LINUX
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Linux.LinuxImageHandle.Create(width, height);
        }
#endif
        throw new PlatformNotSupportedException("Current platform is not supported for image creation.");
    }
}
