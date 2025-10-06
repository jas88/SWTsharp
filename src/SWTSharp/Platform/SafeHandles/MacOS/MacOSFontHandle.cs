using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.MacOS;

/// <summary>
/// Represents a macOS font handle (NSFont*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native macOS NSFont pointer. NSFont objects are typically cached
/// by the system and don't require manual release, but this handle provides consistent API.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class MacOSFontHandle : SafeFontHandle
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    private static readonly IntPtr _selRelease = sel_registerName("release");

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSFontHandle"/> class.
    /// </summary>
    private MacOSFontHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSFontHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing NSFont* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private MacOSFontHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the macOS font handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        // NSFont objects are typically autoreleased, but we release explicitly if we own it
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
    /// Creates a new macOS font handle.
    /// </summary>
    /// <param name="fontName">The name of the font.</param>
    /// <param name="fontSize">The size of the font in points.</param>
    /// <param name="fontStyle">The SWT style flags for the font.</param>
    /// <returns>A new MacOSFontHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when font creation fails.
    /// </exception>
    internal static MacOSFontHandle Create(string fontName, int fontSize, int fontStyle)
    {
        // This is a placeholder - actual implementation would require
        // proper Objective-C interop to create an NSFont
        throw new NotImplementedException("MacOS font creation via SafeHandle not yet implemented.");
    }

    /// <summary>
    /// Wraps an existing macOS font handle.
    /// </summary>
    /// <param name="existingHandle">The existing NSFont* pointer.</param>
    /// <param name="ownsHandle">true if this instance should release the font; otherwise, false.</param>
    /// <returns>A new MacOSFontHandle instance wrapping the existing handle.</returns>
    public static MacOSFontHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new MacOSFontHandle(existingHandle, ownsHandle);
    }
}
