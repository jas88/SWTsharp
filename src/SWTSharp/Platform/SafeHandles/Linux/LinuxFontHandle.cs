using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.Linux;

/// <summary>
/// Represents a Linux/Pango font handle (PangoFontDescription*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native Pango font description pointer and ensures it is properly
/// freed when no longer needed using pango_font_description_free.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class LinuxFontHandle : SafeFontHandle
{
    private const string PangoLib = "libpango-1.0.so.0";

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void pango_font_description_free(IntPtr desc);

    [DllImport(PangoLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr pango_font_description_from_string(string str);

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxFontHandle"/> class.
    /// </summary>
    private LinuxFontHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxFontHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing PangoFontDescription* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private LinuxFontHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the Linux/Pango font handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        try
        {
            if (handle != IntPtr.Zero)
            {
                pango_font_description_free(handle);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new Linux/Pango font handle.
    /// </summary>
    /// <param name="fontName">The name of the font.</param>
    /// <param name="fontSize">The size of the font in points.</param>
    /// <param name="fontStyle">The SWT style flags for the font.</param>
    /// <returns>A new LinuxFontHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when font creation fails.
    /// </exception>
    internal static LinuxFontHandle Create(string fontName, int fontSize, int fontStyle)
    {
        // Build Pango font description string
        string styleStr = string.Empty;
        if ((fontStyle & SWT.BOLD) != 0)
        {
            styleStr += " Bold";
        }
        if ((fontStyle & SWT.ITALIC) != 0)
        {
            styleStr += " Italic";
        }

        string fontDesc = $"{fontName}{styleStr} {fontSize}";

        IntPtr desc = pango_font_description_from_string(fontDesc);

        if (desc == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create Pango font description for '{fontDesc}'.");
        }

        return new LinuxFontHandle(desc, true);
    }

    /// <summary>
    /// Wraps an existing Linux/Pango font handle.
    /// </summary>
    /// <param name="existingHandle">The existing PangoFontDescription* pointer.</param>
    /// <param name="ownsHandle">true if this instance should free the font; otherwise, false.</param>
    /// <returns>A new LinuxFontHandle instance wrapping the existing handle.</returns>
    public static LinuxFontHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new LinuxFontHandle(existingHandle, ownsHandle);
    }
}
