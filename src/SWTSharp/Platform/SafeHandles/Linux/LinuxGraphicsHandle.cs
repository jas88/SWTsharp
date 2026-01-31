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

    private const string GdkLib = "libgdk-3.so.0";

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gdk_cairo_create(IntPtr window);

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gdk_window_get_effective_toplevel(IntPtr window);

    [DllImport(GdkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_window(IntPtr widget);

    /// <summary>
    /// Creates a new Linux/Cairo graphics context for the specified window.
    /// </summary>
    /// <param name="windowHandle">The GTK widget or GDK window handle to get a graphics context for.</param>
    /// <returns>A new LinuxGraphicsHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when graphics context creation fails. This typically happens when:
    /// - The window handle is not realized (not yet mapped to a GDK window)
    /// - The handle is not a valid GTK widget or GDK window
    /// - GTK/GDK libraries are not properly initialized
    /// </exception>
    /// <remarks>
    /// The windowHandle can be either a GtkWidget pointer or a GdkWindow pointer.
    /// If it's a GtkWidget, its underlying GdkWindow will be used.
    /// For drawing during expose/draw events, the cairo context is typically
    /// provided directly - use FromHandle() in those cases.
    /// </remarks>
    internal static LinuxGraphicsHandle Create(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Window handle cannot be null.", nameof(windowHandle));
        }

        // Try to get GDK window from widget
        IntPtr gdkWindow = gtk_widget_get_window(windowHandle);

        // If that fails, assume windowHandle is already a GdkWindow
        if (gdkWindow == IntPtr.Zero)
        {
            gdkWindow = windowHandle;
        }

        // Create cairo context for the GDK window
        IntPtr cairoContext = gdk_cairo_create(gdkWindow);

        if (cairoContext == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "Failed to create Cairo graphics context. " +
                "Ensure the widget is realized and has a valid GDK window. " +
                "For drawing during expose events, use FromHandle() with the provided cairo_t.");
        }

        var handle = new LinuxGraphicsHandle(cairoContext, true);
        return handle;
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
