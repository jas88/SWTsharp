namespace SWTSharp.Platform.Linux;

/// <summary>
/// Helper methods for GTK widget operations.
/// </summary>
internal static class GtkHelpers
{
    /// <summary>
    /// Validates that a GTK widget handle is non-null, throwing InvalidOperationException if null.
    /// </summary>
    /// <param name="handle">The GTK widget handle to validate.</param>
    /// <param name="widgetName">Name of the widget for the error message.</param>
    /// <returns>The validated handle.</returns>
    /// <exception cref="InvalidOperationException">Thrown when handle is IntPtr.Zero.</exception>
    public static IntPtr ThrowOnNull(IntPtr handle, string widgetName)
    {
        if (handle == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Failed to create GTK widget: {widgetName}");
        }
        return handle;
    }
}
