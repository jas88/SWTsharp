using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.Linux;

/// <summary>
/// Represents a Linux/Cairo graphics context handle (cairo_t*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native Cairo graphics context pointer (cairo_t*) and ensures it is properly
/// released when no longer needed using cairo_destroy.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class LinuxGraphicsHandle : SafeGraphicsHandle
{
    private const string CairoLib = "libcairo.so.2";

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void cairo_destroy(IntPtr cr);

    [DllImport(CairoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr cairo_reference(IntPtr cr);

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxGraphicsHandle"/> class.
    /// </summary>
    private LinuxGraphicsHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxGraphicsHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing cairo_t* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private LinuxGraphicsHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the Linux/Cairo graphics context handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        try
        {
            if (handle != IntPtr.Zero)
            {
                cairo_destroy(handle);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new Linux/Cairo graphics context for the specified window.
    /// </summary>
    /// <param name="windowHandle">The window handle to get a graphics context for.</param>
    /// <returns>A new LinuxGraphicsHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when graphics context creation fails.
    /// </exception>
    internal static LinuxGraphicsHandle Create(IntPtr windowHandle)
    {
        // This is a placeholder - actual implementation would require
        // proper GTK/Cairo interop to create a cairo context
        throw new NotImplementedException("Linux graphics context creation via SafeHandle not yet implemented.");
    }

    /// <summary>
    /// Wraps an existing Linux/Cairo graphics context handle.
    /// </summary>
    /// <param name="existingHandle">The existing cairo_t* pointer.</param>
    /// <param name="ownsHandle">true if this instance should destroy the context; otherwise, false.</param>
    /// <returns>A new LinuxGraphicsHandle instance wrapping the existing handle.</returns>
    public static LinuxGraphicsHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        var handle = new LinuxGraphicsHandle(existingHandle, ownsHandle);
        if (ownsHandle && existingHandle != IntPtr.Zero)
        {
            // Reference the context since we own it
            cairo_reference(existingHandle);
        }
        return handle;
    }
}
