using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace SWTSharp.Platform.SafeHandles.Linux;

/// <summary>
/// Represents a Linux/GTK menu handle (GtkMenu*) with automatic resource cleanup.
/// </summary>
/// <remarks>
/// This class wraps a native GTK menu pointer (GtkMenu*) and ensures it is properly
/// destroyed when no longer needed using gtk_widget_destroy.
/// Thread-safe and supports .NET Standard 2.0, .NET 8.0, and .NET 9.0.
/// </remarks>
public sealed class LinuxMenuHandle : SafeMenuHandle
{
    private const string GtkLib = "libgtk-3.so.0";

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_bar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxMenuHandle"/> class.
    /// </summary>
    private LinuxMenuHandle()
        : base(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LinuxMenuHandle"/> class with an existing handle.
    /// </summary>
    /// <param name="existingHandle">An existing GtkMenu* pointer.</param>
    /// <param name="ownsHandle">true if this instance owns the handle; otherwise, false.</param>
    private LinuxMenuHandle(IntPtr existingHandle, bool ownsHandle)
        : base(existingHandle, ownsHandle)
    {
    }

    /// <summary>
    /// Executes the code required to free the Linux/GTK menu handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
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
    /// Creates a new Linux/GTK menu handle.
    /// </summary>
    /// <param name="style">The SWT style flags for the menu.</param>
    /// <returns>A new LinuxMenuHandle instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when menu creation fails.
    /// </exception>
    internal static LinuxMenuHandle Create(int style)
    {
        IntPtr menu;

        if ((style & SWT.BAR) != 0)
        {
            menu = gtk_menu_bar_new();
        }
        else
        {
            menu = gtk_menu_new();
        }

        if (menu == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK menu.");
        }

        return new LinuxMenuHandle(menu, true);
    }

    /// <summary>
    /// Wraps an existing Linux/GTK menu handle.
    /// </summary>
    /// <param name="existingHandle">The existing GtkMenu* pointer.</param>
    /// <param name="ownsHandle">true if this instance should destroy the menu; otherwise, false.</param>
    /// <returns>A new LinuxMenuHandle instance wrapping the existing handle.</returns>
    public static LinuxMenuHandle FromHandle(IntPtr existingHandle, bool ownsHandle = false)
    {
        return new LinuxMenuHandle(existingHandle, ownsHandle);
    }
}
