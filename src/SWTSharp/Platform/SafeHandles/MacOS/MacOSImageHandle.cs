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

    [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_alloc(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_initWithSize(IntPtr receiver, IntPtr selector, double width, double height);

    private static readonly IntPtr _selAlloc = sel_registerName("alloc");
    private static readonly IntPtr _selInitWithSize = sel_registerName("initWithSize:");
    private static readonly IntPtr _clsNSImage = objc_getClass("NSImage");

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
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentException($"Image dimensions must be positive. Got {width}x{height}.");
        }

        // [[NSImage alloc] initWithSize:NSMakeSize(width, height)]
        IntPtr allocated = objc_msgSend_alloc(_clsNSImage, _selAlloc);
        if (allocated == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to allocate NSImage.");
        }

        IntPtr image = objc_msgSend_initWithSize(allocated, _selInitWithSize, width, height);
        if (image == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create NSImage with dimensions {width}x{height}.");
        }

        // initWithSize: returns a retained object, so we own it
        return new MacOSImageHandle(image, true);
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
