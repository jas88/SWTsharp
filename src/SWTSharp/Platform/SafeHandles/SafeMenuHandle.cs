using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles;

/// <summary>
/// Base class for platform-specific menu handles (HMENU, NSMenu, GtkMenu).
/// Provides safe handle management with automatic resource cleanup for menu resources.
/// </summary>
/// <remarks>
/// Menu handles represent native menu objects used for application menus and context menus.
/// This class ensures proper cleanup and thread-safe handle management.
/// Supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public abstract class SafeMenuHandle : SafeHandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeMenuHandle"/> class.
    /// </summary>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release (not recommended).
    /// </param>
    protected SafeMenuHandle(bool ownsHandle)
        : base(IntPtr.Zero, ownsHandle)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SafeMenuHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An IntPtr object that represents the existing handle.</param>
    /// <param name="ownsHandle">
    /// true to reliably release the handle during finalization;
    /// false to prevent reliable release.
    /// </param>
    protected SafeMenuHandle(IntPtr existingHandle, bool ownsHandle)
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
    /// Executes the code required to free the menu handle.
    /// </summary>
    /// <returns>
    /// true if the handle is released successfully; otherwise, in the event of a catastrophic failure, false.
    /// </returns>
    /// <remarks>
    /// This method is called by the SafeHandle infrastructure and must be implemented
    /// by derived classes to provide platform-specific cleanup of menu resources.
    /// This method runs in a constrained execution region (CER) to guarantee execution.
    /// </remarks>
    protected abstract override bool ReleaseHandle();

    /// <summary>
    /// Creates a platform-specific menu handle.
    /// </summary>
    /// <param name="style">The menu style flags (BAR, DROP_DOWN, or POP_UP).</param>
    /// <returns>A new SafeMenuHandle instance for the created menu.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Thrown when the current platform is not supported.
    /// </exception>
    public static SafeMenuHandle CreatePlatformMenu(int style)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Win32.Win32MenuHandle.Create(style);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOS.MacOSMenuHandle.Create(style);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Linux.LinuxMenuHandle.Create(style);
        }
        else
        {
            throw new PlatformNotSupportedException("Current platform is not supported for menu creation.");
        }
    }
}
