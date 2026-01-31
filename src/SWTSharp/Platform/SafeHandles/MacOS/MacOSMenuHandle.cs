using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.MacOS;

/// <summary>
/// Represents a macOS menu handle (NSMenu*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native macOS NSMenu pointer and ensures it is properly
/// released when no longer needed via Objective-C reference counting.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class MacOSMenuHandle : SafeMenuHandle
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    private static readonly IntPtr _selRelease = sel_registerName("release");

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSMenuHandle"/> class.
    /// </summary>
    private MacOSMenuHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSMenuHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing NSMenu* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private MacOSMenuHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the macOS menu handle.
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
    private static extern IntPtr objc_msgSend_init(IntPtr receiver, IntPtr selector);

    private static readonly IntPtr _selAlloc = sel_registerName("alloc");
    private static readonly IntPtr _selInit = sel_registerName("init");
    private static readonly IntPtr _clsNSMenu = objc_getClass("NSMenu");

    /// <summary>
    /// Creates a new macOS menu handle.
    /// </summary>
    /// <param name="style">The SWT style flags for the menu.</param>
    /// <returns>A new MacOSMenuHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when menu creation fails.
    /// </exception>
    internal static MacOSMenuHandle Create(int style)
    {
        // [[NSMenu alloc] init]
        IntPtr allocated = objc_msgSend_alloc(_clsNSMenu, _selAlloc);
        if (allocated == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to allocate NSMenu.");
        }

        IntPtr menu = objc_msgSend_init(allocated, _selInit);
        if (menu == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to initialize NSMenu.");
        }

        // init returns a retained object, so we own it
        return new MacOSMenuHandle(menu, true);
    }

    /// <summary>
    /// Wraps an existing macOS menu handle.
    /// </summary>
    /// <param name="existingHandle">The existing NSMenu* pointer.</param>
    /// <param name="ownsHandle">true if this instance should release the menu; otherwise, false.</param>
    /// <returns>A new MacOSMenuHandle instance wrapping the existing handle.</returns>
    public static MacOSMenuHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new MacOSMenuHandle(existingHandle, ownsHandle);
    }
}
