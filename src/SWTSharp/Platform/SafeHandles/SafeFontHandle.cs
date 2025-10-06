using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles;

/// <summary>
/// Base class for platform-specific font handles (HFONT, NSFont, PangoFontDescription).
/// Provides safe handle management with automatic resource cleanup for font resources.
/// </summary>
/// <remarks>
/// Font handles represent native font objects used for text rendering.
/// This class ensures proper cleanup and thread-safe handle management.
/// Supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public abstract class SafeFontHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeFontHandle"/> class.
    /// </summary>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release (not recommended).
    /// </param>
    protected SafeFontHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeFontHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An IntPtr object that represents the existing handle.</param>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release.
    /// </param>
    protected SafeFontHandle(IntPtr existingHandle, bool ownsHandle)
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
    /// Executes the code required to free the font handle.
    /// </summary>
    /// <returns>
    /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
    /// </returns>
    /// <remarks>
    /// This method is called by the SafeHandle infrastructure and must be implemented
    /// by derived classes to provide platform-specific cleanup of font resources.
    /// This method runs in a constrained execution region (CER) to guarantee execution.
    /// </remarks>
    protected abstract override bool ReleaseHandle();

    /// <summary>
    /// Creates a platform-specific font handle.
    /// </summary>
    /// <param name="fontName">The name of the font.</param>
    /// <param name="fontSize">The size of the font in points.</param>
    /// <param name="fontStyle">The style flags for the font (bold, italic, etc.).</param>
    /// <returns>A new SafeFontHandle instance for the created font.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    public static SafeFontHandle CreatePlatformFont(string fontName, int fontSize, int fontStyle)
    {
#if WINDOWS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Win32.Win32FontHandle.Create(fontName, fontSize, fontStyle);
        }
#endif
#if MACOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOS.MacOSFontHandle.Create(fontName, fontSize, fontStyle);
        }
#endif
#if LINUX
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Linux.LinuxFontHandle.Create(fontName, fontSize, fontStyle);
        }
#endif
        throw new PlatformNotSupportedException("Current platform is not supported for font creation.");
    }
}
