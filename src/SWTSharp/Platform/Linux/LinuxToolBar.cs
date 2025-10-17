using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of IPlatformToolBar using GtkToolbar widget.
/// </summary>
internal class LinuxToolBar : IPlatformToolBar
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _gtkToolbarHandle;
    private readonly int _style;
    private bool _disposed;
    private readonly List<IntPtr> _items = new();

    private enum GtkToolbarStyle
    {
        Icons = 0,
        Text = 1,
        Both = 2,
        BothHoriz = 3
    }

    private enum GtkOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    public LinuxToolBar(IntPtr parentHandle, int style)
    {
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Linux] Creating toolbar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create GtkToolbar
        _gtkToolbarHandle = gtk_toolbar_new();
        if (_gtkToolbarHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK Toolbar");
        }

        // Set orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            gtk_orientable_set_orientation(_gtkToolbarHandle, GtkOrientation.Vertical);
        }
        else
        {
            gtk_orientable_set_orientation(_gtkToolbarHandle, GtkOrientation.Horizontal);
        }

        // Set toolbar style - FLAT means icons only, otherwise icons and text
        if ((style & SWT.FLAT) != 0)
        {
            gtk_toolbar_set_style(_gtkToolbarHandle, GtkToolbarStyle.Icons);
        }
        else
        {
            gtk_toolbar_set_style(_gtkToolbarHandle, GtkToolbarStyle.Both);
        }

        // Show the toolbar widget
        gtk_widget_show(_gtkToolbarHandle);

        if (enableLogging)
            Console.WriteLine($"[Linux] Toolbar widget created successfully. GTK Handle: 0x{_gtkToolbarHandle:X}");
    }

    /// <summary>
    /// Gets the native GTK handle (GtkToolbar*) for this toolbar.
    /// Used internally by platform code.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _gtkToolbarHandle;
    }

    public void AddItem(string text, IPlatformImage? image)
    {
        if (_gtkToolbarHandle == IntPtr.Zero) return;

        // TODO: Convert IPlatformImage to GtkWidget when image support is added
        // For now, create a tool button with text only
        IntPtr toolButton = gtk_tool_button_new(IntPtr.Zero, text ?? string.Empty);

        if (toolButton == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK ToolButton");
        }

        // Insert at end (-1 position)
        gtk_toolbar_insert(_gtkToolbarHandle, toolButton, -1);

        // Track the item
        _items.Add(toolButton);

        // Show the tool button
        gtk_widget_show(toolButton);
    }

    public void RemoveItem(int index)
    {
        if (_gtkToolbarHandle == IntPtr.Zero) return;
        if (index < 0 || index >= _items.Count) return;

        IntPtr toolButton = _items[index];

        // Remove from toolbar (this will destroy the widget)
        gtk_widget_destroy(toolButton);

        // Remove from tracking list
        _items.RemoveAt(index);
    }

    public void AttachToWindow(IPlatformWindow window)
    {
        // On Linux, toolbar is typically added to a container, not attached to window
        // This is a no-op for GTK as the toolbar is already part of the widget hierarchy
    }

    public int GetItemCount()
    {
        return _items.Count;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Clear items (widgets will be destroyed automatically when toolbar is destroyed)
        _items.Clear();

        // Destroy the toolbar widget
        if (_gtkToolbarHandle != IntPtr.Zero)
        {
            gtk_widget_destroy(_gtkToolbarHandle);
            _gtkToolbarHandle = IntPtr.Zero;
        }
    }

    #region GTK P/Invoke

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_toolbar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toolbar_insert(IntPtr toolbar, IntPtr item, int pos);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toolbar_set_style(IntPtr toolbar, GtkToolbarStyle style);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_tool_button_new(IntPtr icon_widget, string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_orientable_set_orientation(IntPtr orientable, GtkOrientation orientation);

    #endregion
}
