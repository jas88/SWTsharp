using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.MacOS;

/// <summary>
/// Represents a macOS graphics context handle (CGContextRef) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native macOS Core Graphics context (CGContextRef) and ensures it is properly
/// released when no longer needed using CGContextRelease.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class MacOSGraphicsHandle : SafeGraphicsHandle
{
    private const string CoreGraphicsFramework = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextRelease(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern IntPtr CGContextRetain(IntPtr context);

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSGraphicsHandle"/> class.
    /// </summary>
    private MacOSGraphicsHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSGraphicsHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing CGContextRef pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private MacOSGraphicsHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the macOS graphics context handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        try
        {
            CGContextRelease(handle);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new macOS graphics context for the specified window.
    /// </summary>
    /// <param name="windowHandle">The window handle to get a graphics context for.</param>
    /// <returns>A new MacOSGraphicsHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when graphics context creation fails.
    /// </exception>
    internal static MacOSGraphicsHandle Create(IntPtr windowHandle)
    {
        // This is a placeholder - actual implementation would require
        // proper Objective-C/Core Graphics interop
        throw new NotImplementedException("MacOS graphics context creation via SafeHandle not yet implemented.");
    }

    /// <summary>
    /// Wraps an existing macOS graphics context handle.
    /// </summary>
    /// <param name="existingHandle">The existing CGContextRef pointer.</param>
    /// <param name="ownsHandle">true if this instance should release the context; otherwise, false.</param>
    /// <returns>A new MacOSGraphicsHandle instance wrapping the existing handle.</returns>
    public static MacOSGraphicsHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        var handle = new MacOSGraphicsHandle(existingHandle, ownsHandle);
        if (ownsHandle && existingHandle != IntPtr.Zero)
        {
            // Retain the context since we own it
            CGContextRetain(existingHandle);
        }
        return handle;
    }
}
