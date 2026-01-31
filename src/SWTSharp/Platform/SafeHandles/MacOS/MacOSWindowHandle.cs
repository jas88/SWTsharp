using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.MacOS;

/// <summary>
/// Represents a macOS window handle (NSWindow*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native macOS NSWindow pointer and ensures it is properly
/// released when no longer needed. NSWindow objects are reference-counted via Objective-C.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class MacOSWindowHandle : SafeWindowHandle
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    private static readonly IntPtr _selClose = sel_registerName("close");
    private static readonly IntPtr _selRelease = sel_registerName("release");

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSWindowHandle"/> class.
    /// </summary>
    private MacOSWindowHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MacOSWindowHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing NSWindow* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private MacOSWindowHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the macOS window handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        // Close the window and release the reference
        try
        {
            objc_msgSend_void(handle, _selClose);
            objc_msgSend_void(handle, _selRelease);
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
    private static extern IntPtr objc_msgSend_initWithContentRect(
        IntPtr receiver, IntPtr selector,
        double x, double y, double width, double height,
        nuint styleMask, nuint backing, bool defer);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_setTitle(IntPtr receiver, IntPtr selector, IntPtr title);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_stringWithUTF8String(IntPtr receiver, IntPtr selector, string str);

    private static readonly IntPtr _selAlloc = sel_registerName("alloc");
    private static readonly IntPtr _selInitWithContentRect = sel_registerName("initWithContentRect:styleMask:backing:defer:");
    private static readonly IntPtr _selSetTitle = sel_registerName("setTitle:");
    private static readonly IntPtr _selStringWithUTF8String = sel_registerName("stringWithUTF8String:");
    private static readonly IntPtr _clsNSWindow = objc_getClass("NSWindow");
    private static readonly IntPtr _clsNSString = objc_getClass("NSString");

    // NSWindow style mask constants
    private const nuint NSWindowStyleMaskTitled = 1 << 0;
    private const nuint NSWindowStyleMaskClosable = 1 << 1;
    private const nuint NSWindowStyleMaskMiniaturizable = 1 << 2;
    private const nuint NSWindowStyleMaskResizable = 1 << 3;

    // NSBackingStoreType
    private const nuint NSBackingStoreBuffered = 2;

    /// <summary>
    /// Creates a new macOS window handle.
    /// </summary>
    /// <param name="style">The SWT style flags for the window.</param>
    /// <param name="title">The window title.</param>
    /// <returns>A new MacOSWindowHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when window creation fails.
    /// </exception>
    internal static MacOSWindowHandle Create(int style, string title)
    {
        // Convert SWT style to NSWindow style mask
        nuint styleMask = NSWindowStyleMaskTitled | NSWindowStyleMaskClosable | NSWindowStyleMaskMiniaturizable;

        if ((style & SWT.RESIZE) != 0 || (style & SWT.MAX) != 0)
        {
            styleMask |= NSWindowStyleMaskResizable;
        }

        // Allocate and initialize NSWindow
        IntPtr allocated = objc_msgSend_alloc(_clsNSWindow, _selAlloc);
        if (allocated == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to allocate NSWindow.");
        }

        // Default window size and position
        IntPtr window = objc_msgSend_initWithContentRect(
            allocated, _selInitWithContentRect,
            100, 100, 400, 300,  // x, y, width, height
            styleMask, NSBackingStoreBuffered, false);

        if (window == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to initialize NSWindow.");
        }

        // Set window title if provided
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsTitle = objc_msgSend_stringWithUTF8String(_clsNSString, _selStringWithUTF8String, title);
            if (nsTitle != IntPtr.Zero)
            {
                objc_msgSend_setTitle(window, _selSetTitle, nsTitle);
            }
        }

        // initWithContentRect: returns a retained object, so we own it
        return new MacOSWindowHandle(window, true);
    }

    /// <summary>
    /// Wraps an existing macOS window handle.
    /// </summary>
    /// <param name="existingHandle">The existing NSWindow* pointer.</param>
    /// <param name="ownsHandle">true if this instance should close/release the window; otherwise, false.</param>
    /// <returns>A new MacOSWindowHandle instance wrapping the existing handle.</returns>
    public static MacOSWindowHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new MacOSWindowHandle(existingHandle, ownsHandle);
    }
}
