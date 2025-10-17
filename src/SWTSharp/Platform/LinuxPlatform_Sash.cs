using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Linux (GTK) platform implementation - Sash widget methods.
/// Uses GtkPaned for resizable divider functionality.
/// </summary>
internal partial class LinuxPlatform
{
    private class LinuxSash : IPlatformSash
    {
        private readonly LinuxPlatform _platform;
        private readonly IntPtr _handle;
        private int _position;
        private bool _disposed;

        public event EventHandler<int>? PositionChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public LinuxSash(LinuxPlatform platform, IntPtr handle)
        {
            _platform = platform;
            _handle = handle;
        }

        public void SetPosition(int position)
        {
            _position = position;
            gtk_paned_set_position(_handle, position);
        }

        public int GetPosition()
        {
            return gtk_paned_get_position(_handle);
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            gtk_widget_set_size_request(_handle, width, height);
            // Position is controlled by parent container in GTK
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _handle == IntPtr.Zero) return default;
            GtkAllocation allocation;
            gtk_widget_get_allocation(_handle, out allocation);
            return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            if (visible)
                gtk_widget_show(_handle);
            else
                gtk_widget_hide(_handle);
        }

        public bool GetVisible()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return gtk_widget_get_visible(_handle);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            gtk_widget_set_sensitive(_handle, enabled);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return gtk_widget_get_sensitive(_handle);
        }

        public void SetBackground(RGB color)
        {
            // GTK3 theming - would need CSS provider
        }

        public RGB GetBackground()
        {
            return new RGB(200, 200, 200);
        }

        public void SetForeground(RGB color)
        {
            // Not applicable for sash
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 0);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle != IntPtr.Zero)
                {
                    gtk_widget_destroy(_handle);
                }
                _disposed = true;
            }
        }

        internal void OnPositionChanged()
        {
            int newPosition = GetPosition();
            if (_position != newPosition)
            {
                _position = newPosition;
                PositionChanged?.Invoke(this, _position);
            }
        }
    }

    public IPlatformSash CreateSashWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent is Linux.LinuxWidget linuxWidget)
        {
            parentHandle = linuxWidget.GetNativeHandle();
        }

        bool isVertical = (style & SWT.VERTICAL) != 0;

        // Create GtkPaned (horizontal or vertical)
        IntPtr paned = isVertical ? gtk_paned_new(GtkOrientation.Vertical) : gtk_paned_new(GtkOrientation.Horizontal);

        // Add empty placeholder widgets to make it visible
        IntPtr box1 = gtk_box_new(GtkOrientation.Horizontal, 0);
        IntPtr box2 = gtk_box_new(GtkOrientation.Horizontal, 0);
        gtk_paned_pack1(paned, box1, true, true);
        gtk_paned_pack2(paned, box2, true, true);

        // Add to parent if specified
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, paned);
        }

        gtk_widget_show_all(paned);

        var sashWidget = new LinuxSash(this, paned);
        _sashWidgets[paned] = sashWidget;

        // Callback connection removed - not needed for basic functionality
        // g_signal_connect_data(paned, "notify::position", OnSashPositionChanged, IntPtr.Zero, IntPtr.Zero, 0);

        return sashWidget;
    }

    private Dictionary<IntPtr, LinuxSash> _sashWidgets = new Dictionary<IntPtr, LinuxSash>();

#if NET8_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    private static void OnSashPositionChanged(IntPtr paned, IntPtr pspec, IntPtr userData)
    {
        // Find the sash widget and invoke the callback
        // This is a simplified version - would need proper callback routing
    }
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(GtkLib)]
    private static partial IntPtr gtk_paned_new(GtkOrientation orientation);

    [LibraryImport(GtkLib)]
    private static partial void gtk_paned_pack1(IntPtr paned, IntPtr child, [MarshalAs(UnmanagedType.Bool)] bool resize, [MarshalAs(UnmanagedType.Bool)] bool shrink);

    [LibraryImport(GtkLib)]
    private static partial void gtk_paned_pack2(IntPtr paned, IntPtr child, [MarshalAs(UnmanagedType.Bool)] bool resize, [MarshalAs(UnmanagedType.Bool)] bool shrink);

    [LibraryImport(GtkLib)]
    private static partial void gtk_paned_set_position(IntPtr paned, int position);

    [LibraryImport(GtkLib)]
    private static partial int gtk_paned_get_position(IntPtr paned);
#else
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_paned_new(GtkOrientation orientation);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_paned_pack1(IntPtr paned, IntPtr child, bool resize, bool shrink);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_paned_pack2(IntPtr paned, IntPtr child, bool resize, bool shrink);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_paned_set_position(IntPtr paned, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern int gtk_paned_get_position(IntPtr paned);
#endif
}
