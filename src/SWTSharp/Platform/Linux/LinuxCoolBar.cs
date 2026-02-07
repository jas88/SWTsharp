using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux/GTK implementation of IPlatformCoolBar using GtkBox with handles for rearrangeable bands.
/// </summary>
internal class LinuxCoolBar : IPlatformCoolBar
{
    private const string GtkLib = "libgtk-3.so.0";

    private IntPtr _gtkBoxHandle;
    private readonly int _style;
    private bool _disposed;
    private readonly List<LinuxCoolItem> _items = new();
    private bool _locked = false;

    private enum GtkOrientation
    {
        Horizontal = 0,
        Vertical = 1
    }

    public LinuxCoolBar(IntPtr parentHandle, int style)
    {
        _style = style;

        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[Linux] Creating coolbar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create GtkBox (horizontal or vertical based on style)
        GtkOrientation orientation = (style & SWT.VERTICAL) != 0
            ? GtkOrientation.Vertical
            : GtkOrientation.Horizontal;

        _gtkBoxHandle = gtk_box_new(orientation, 0);
        if (_gtkBoxHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GTK Box for CoolBar");
        }

        // Make it homogeneous to allow rearrangeable bands
        gtk_box_set_homogeneous(_gtkBoxHandle, false);

        gtk_widget_show(_gtkBoxHandle);

        if (enableLogging)
            Console.WriteLine($"[Linux] CoolBar widget created successfully. GTK Handle: 0x{_gtkBoxHandle:X}");
    }

    internal IntPtr GetNativeHandle()
    {
        return _gtkBoxHandle;
    }

    public IPlatformCoolItem CreateItem(int index, int style)
    {
        if (_gtkBoxHandle == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(LinuxCoolBar));

        var item = new LinuxCoolItem(this, index, style);

        if (index < 0 || index >= _items.Count)
        {
            _items.Add(item);
        }
        else
        {
            _items.Insert(index, item);
        }

        return item;
    }

    public void RemoveItem(int index)
    {
        if (_gtkBoxHandle == IntPtr.Zero) return;
        if (index < 0 || index >= _items.Count) return;

        _items[index].Dispose();
        _items.RemoveAt(index);
    }

    public int GetItemCount()
    {
        return _items.Count;
    }

    public bool GetLocked()
    {
        return _locked;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;
        // On GTK, we would need to disable drag-and-drop for items
        // For simplicity, we'll just track the state for now
    }

    internal void PackItem(IntPtr child, int position)
    {
        if (position < 0)
        {
            gtk_box_pack_end(_gtkBoxHandle, child, false, false, 0);
        }
        else
        {
            gtk_box_pack_start(_gtkBoxHandle, child, false, false, 0);
            gtk_box_reorder_child(_gtkBoxHandle, child, position);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Detach from parent container to prevent double-destroy
        if (_gtkBoxHandle != IntPtr.Zero)
        {
            var parent = gtk_widget_get_parent(_gtkBoxHandle);
            if (parent != IntPtr.Zero)
            {
                gtk_container_remove(parent, _gtkBoxHandle);
            }
        }

        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        // Do NOT call gtk_widget_destroy -- parent window destruction handles cleanup.
        _gtkBoxHandle = IntPtr.Zero;
    }

    #region GTK P/Invoke

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_box_new(GtkOrientation orientation, int spacing);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_box_set_homogeneous(IntPtr box, bool homogeneous);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_box_pack_start(IntPtr box, IntPtr child, bool expand, bool fill, uint padding);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_box_pack_end(IntPtr box, IntPtr child, bool expand, bool fill, uint padding);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_box_reorder_child(IntPtr box, IntPtr child, int position);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_parent(IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    #endregion

    /// <summary>
    /// Linux implementation of IPlatformCoolItem.
    /// Uses GtkFrame as container since GtkHandleBox is deprecated in GTK 3.4+.
    /// </summary>
    internal class LinuxCoolItem : IPlatformCoolItem
    {
        private readonly LinuxCoolBar _parent;
        private readonly int _index;
        private readonly int _style;
        private IntPtr _container;  // GtkFrame (replaces deprecated GtkHandleBox)
        private IntPtr _childWidget;
        private int _preferredWidth = 100;
        private int _preferredHeight = 24;
        private int _minimumWidth = 0;
        private int _minimumHeight = 0;
        private bool _disposed;

        public LinuxCoolItem(LinuxCoolBar parent, int index, int style)
        {
            _parent = parent;
            _index = index < 0 ? parent.GetItemCount() : index;
            _style = style;

            // Create GtkFrame as container (GtkHandleBox was deprecated in GTK 3.4)
            _container = gtk_frame_new(IntPtr.Zero);
            if (_container == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create GTK Frame for CoolItem");
            }

            // Remove frame shadow for cleaner look
            gtk_frame_set_shadow_type(_container, 0); // GTK_SHADOW_NONE

            gtk_widget_set_size_request(_container, _preferredWidth, _preferredHeight);
            gtk_widget_show(_container);

            // Pack into parent
            _parent.PackItem(_container, _index);
        }

        public void SetControl(IPlatformWidget? control)
        {
            // Remove old child if any
            if (_childWidget != IntPtr.Zero)
            {
                gtk_container_remove(_container, _childWidget);
                _childWidget = IntPtr.Zero;
            }

            // Extract native GTK handle using reflection
            if (control != null)
            {
                var getNativeHandleMethod = control.GetType().GetMethod("GetNativeHandle",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (getNativeHandleMethod != null)
                {
                    _childWidget = (IntPtr)(getNativeHandleMethod.Invoke(control, null) ?? IntPtr.Zero);
                }
                else
                {
                    _childWidget = IntPtr.Zero;
                }
            }
            else
            {
                _childWidget = IntPtr.Zero;
            }

            if (_childWidget != IntPtr.Zero)
            {
                // Remove widget from any existing parent first (GTK widgets can only have one parent)
                IntPtr currentParent = gtk_widget_get_parent(_childWidget);
                if (currentParent != IntPtr.Zero)
                {
                    gtk_container_remove(currentParent, _childWidget);
                }

                gtk_container_add(_container, _childWidget);
                gtk_widget_show(_childWidget);
            }
        }

        public Rectangle GetBounds()
        {
            // Would need to query GTK allocation to get actual bounds
            return new Rectangle(0, 0, _preferredWidth, _preferredHeight);
        }

        public void SetPreferredSize(int width, int height)
        {
            _preferredWidth = width;
            _preferredHeight = height;

            if (_container != IntPtr.Zero)
            {
                gtk_widget_set_size_request(_container, width, height);
            }
        }

        public void SetMinimumSize(int width, int height)
        {
            _minimumWidth = width;
            _minimumHeight = height;

            // Use size request for minimum size
            if (_container != IntPtr.Zero && width > 0 && height > 0)
            {
                gtk_widget_set_size_request(_container,
                    Math.Max(width, _preferredWidth),
                    Math.Max(height, _preferredHeight));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Do NOT call gtk_widget_destroy -- parent destruction handles cleanup.
            _container = IntPtr.Zero;
            _childWidget = IntPtr.Zero;
        }

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_frame_new(IntPtr label);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_frame_set_shadow_type(IntPtr frame, int type);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_container_add(IntPtr container, IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr gtk_widget_get_parent(IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_widget_show(IntPtr widget);

        [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gtk_widget_destroy(IntPtr widget);
    }
}
