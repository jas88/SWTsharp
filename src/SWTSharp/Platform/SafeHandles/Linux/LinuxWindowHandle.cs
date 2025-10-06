using System.Runtime.InteropServices;

namespace SWTSharp.Platform.SafeHandles.Linux;

/// <summary>
/// Represents a Linux/GTK window handle (GtkWindow*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native GTK window pointer (GtkWindow*) and ensures it is properly
/// destroyed when no longer needed using gtk_widget_destroy.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class LinuxWindowHandle : SafeWindowHandle
{
    private const string GtkLib = "libgtk-3.so.0";

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_window_new(int type);

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxWindowHandle"/> class.
    /// </summary>
    private LinuxWindowHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxWindowHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing GtkWindow* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private LinuxWindowHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the Linux/GTK window handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // In a CER, we must not throw exceptions
        try
        {
            if (handle != IntPtr.Zero)
            {
                gtk_widget_destroy(handle);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a new Linux/GTK window handle.
    /// </summary>
    /// <param name="style">The SWT style flags for the window.</param>
    /// <param name="title">The window title.</param>
    /// <returns>A new LinuxWindowHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when window creation fails.
    /// </exception>
    internal static LinuxWindowHandle Create(int style, string title)
    {
        const int GTK_WINDOW_TOPLEVEL = 0;

        IntPtr window = gtk_window_new(GTK_WINDOW_TOPLEVEL);

        if (window == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK window.");
        }

        return new LinuxWindowHandle(window, true);
    }

    /// <summary>
    /// Wraps an existing Linux/GTK window handle.
    /// </summary>
    /// <param name="existingHandle">The existing GtkWindow* pointer.</param>
    /// <param name="ownsHandle">true if this instance should destroy the window; otherwise, false.</param>
    /// <returns>A new LinuxWindowHandle instance wrapping the existing handle.</returns>
    public static LinuxWindowHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new LinuxWindowHandle(existingHandle, ownsHandle);
    }
}
