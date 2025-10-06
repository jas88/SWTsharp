using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.Linux;

/// <summary>
/// Represents a Linux/GdkPixbuf image handle (GdkPixbuf*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native GdkPixbuf pointer and ensures it is properly
/// unreferenced when no longer needed using g_object_unref.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class LinuxImageHandle : SafeImageHandle
{
    private const string GdkPixbufLib = "libgdk_pixbuf-2.0.so.0";
    private const string GLibLib = "libgobject-2.0.so.0";

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(IntPtr @object);

    [DllImport(GLibLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_object_ref(IntPtr @object);

    [DllImport(GdkPixbufLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gdk_pixbuf_new(int colorspace, bool has_alpha, int bits_per_sample, int width, int height);

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxImageHandle"/> class.
    /// </summary>
    private LinuxImageHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxImageHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing GdkPixbuf* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private LinuxImageHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the Linux/GdkPixbuf image handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        try
        {
            if (handle != IntPtr.Zero)
            {
                g_object_unref(handle);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new Linux/GdkPixbuf image handle.
    /// </summary>
    /// <param name="width">The width of the image in pixels.</param>
    /// <param name="height">The height of the image in pixels.</param>
    /// <returns>A new LinuxImageHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when image creation fails.
    /// </exception>
    internal static LinuxImageHandle Create(int width, int height)
    {
        const int GDK_COLORSPACE_RGB = 0;
        const int BITS_PER_SAMPLE = 8;

        IntPtr pixbuf = gdk_pixbuf_new(GDK_COLORSPACE_RGB, true, BITS_PER_SAMPLE, width, height);

        if (pixbuf == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create GdkPixbuf with dimensions {width}x{height}.");
        }

        return new LinuxImageHandle(pixbuf, true);
    }

    /// <summary>
    /// Wraps an existing Linux/GdkPixbuf image handle.
    /// </summary>
    /// <param name="existingHandle">The existing GdkPixbuf* pointer.</param>
    /// <param name="ownsHandle">true if this instance should unref the image; otherwise, false.</param>
    /// <returns>A new LinuxImageHandle instance wrapping the existing handle.</returns>
    public static LinuxImageHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        var handle = new LinuxImageHandle(existingHandle, ownsHandle);
        if (ownsHandle && existingHandle != IntPtr.Zero)
        {
            // Reference the pixbuf since we own it
            g_object_ref(existingHandle);
        }
        return handle;
    }
}
