using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.MacOS;

/// <summary>
/// Represents a macOS image handle (NSImage*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native macOS NSImage pointer and ensures it is properly
/// released when no longer needed via Objective-C reference counting.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class MacOSImageHandle : SafeImageHandle
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    private static readonly IntPtr _selRelease = sel_registerName("release");

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSImageHandle"/> class.
    /// </summary>
    private MacOSImageHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSImageHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing NSImage* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private MacOSImageHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the macOS image handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        try
        {
            if (handle != IntPtr.Zero)
            {
                objc_msgSend_void(handle, _selRelease);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new macOS image handle.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <returns>A new MacOSImageHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when image creation fails.
    /// </exception>
    internal static MacOSImageHandle Create(int width, int height)
    {
        // This is a placeholder - actual implementation would require
        // proper Objective-C interop to create an NSImage
        throw new NotImplementedException("MacOS image creation via SafeHandle not yet implemented.");
    }

    /// <summary>
    /// Wraps an existing macOS image handle.
    /// </summary>
    /// <param name="existingHandle">The existing NSImage* pointer.</param>
    /// <param name="ownsHandle">true if this instance should release the image; otherwise, false.</param>
    /// <returns>A new MacOSImageHandle instance wrapping the existing handle.</returns>
    public static MacOSImageHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new MacOSImageHandle(existingHandle, ownsHandle);
    }
}
