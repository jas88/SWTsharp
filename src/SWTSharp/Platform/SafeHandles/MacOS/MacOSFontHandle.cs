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

    [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_IntPtr_double(IntPtr receiver, IntPtr selector, IntPtr arg1, double arg2);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_double(IntPtr receiver, IntPtr selector, double arg1);

    private static readonly IntPtr _selFontWithNameSize = sel_registerName("fontWithName:size:");
    private static readonly IntPtr _selSystemFontOfSize = sel_registerName("systemFontOfSize:");
    private static readonly IntPtr _selBoldSystemFontOfSize = sel_registerName("boldSystemFontOfSize:");
    private static readonly IntPtr _selRetain = sel_registerName("retain");
    private static readonly IntPtr _clsNSFont = objc_getClass("NSFont");
    private static readonly IntPtr _clsNSString = objc_getClass("NSString");
    private static readonly IntPtr _selStringWithUTF8String = sel_registerName("stringWithUTF8String:");

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_ret_IntPtr(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_long(IntPtr receiver, IntPtr selector, IntPtr arg1, long arg2);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_string(IntPtr receiver, IntPtr selector, string arg1);

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
        IntPtr font = IntPtr.Zero;

        // If no font name specified, use system font
        if (string.IsNullOrEmpty(fontName))
        {
            if ((fontStyle & SWT.BOLD) != 0)
            {
                font = objc_msgSend_IntPtr_double(_clsNSFont, _selBoldSystemFontOfSize, fontSize);
            }
            else
            {
                font = objc_msgSend_IntPtr_double(_clsNSFont, _selSystemFontOfSize, fontSize);
            }
        }
        else
        {
            // Create NSString for font name
            IntPtr nsName = objc_msgSend_string(_clsNSString, _selStringWithUTF8String, fontName);
            if (nsName != IntPtr.Zero)
            {
                font = objc_msgSend_IntPtr_IntPtr_double(_clsNSFont, _selFontWithNameSize, nsName, fontSize);
            }
        }

        // Apply italic trait if needed
        if (font != IntPtr.Zero && (fontStyle & SWT.ITALIC) != 0)
        {
            IntPtr fontManagerClass = objc_getClass("NSFontManager");
            IntPtr selSharedFontManager = sel_registerName("sharedFontManager");
            IntPtr fontManager = objc_msgSend_ret_IntPtr(fontManagerClass, selSharedFontManager);

            IntPtr selConvertFont = sel_registerName("convertFont:toHaveTrait:");
            IntPtr italicFont = objc_msgSend_IntPtr_long(fontManager, selConvertFont, font, 0x1); // NSItalicFontMask = 0x1
            if (italicFont != IntPtr.Zero)
            {
                font = italicFont;
            }
        }

        if (font == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create macOS font '{fontName ?? "system"}' at size {fontSize}.");
        }

        // Retain the font since we own it
        objc_msgSend_void(font, _selRetain);

        return new MacOSFontHandle(font, true);
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
