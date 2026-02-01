using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformMenu using NSMenu.
/// Supports BAR, DROP_DOWN, and POP_UP menu styles.
/// </summary>
internal class MacOSMenu : MacOSWidget, IPlatformMenu
{
    private IntPtr _nsMenu;
    private readonly int _style;
    private readonly List<MacOSMenuItem> _items = new();
    private bool _disposed;
    private bool _visible;
    private int _locationX;
    private int _locationY;
    private IntPtr _attachedWindow;

    // Objective-C runtime imports
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    [DllImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1, long arg2);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1, CGPoint arg2, IntPtr arg3);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_long(IntPtr receiver, IntPtr selector);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double x;
        public double y;

        public CGPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public MacOSMenu(int style)
    {
        _style = style;
        _nsMenu = CreateNSMenu();

        if (_nsMenu == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create NSMenu");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsMenu;
    }

    private IntPtr CreateNSMenu()
    {
        // [[NSMenu alloc] init]
        IntPtr menuClass = objc_getClass("NSMenu");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInit = sel_registerName("init");

        IntPtr menu = objc_msgSend(menuClass, selAlloc);
        menu = objc_msgSend(menu, selInit);

        // Disable auto-enable items for explicit control
        IntPtr selSetAutoenablesItems = sel_registerName("setAutoenablesItems:");
        objc_msgSend_void(menu, selSetAutoenablesItems, IntPtr.Zero); // NO = 0

        return menu;
    }

    public bool IsMenuBar => (_style & SWT.BAR) != 0;

    public bool IsPopupMenu => (_style & SWT.POP_UP) != 0;

    public void SetVisible(bool visible)
    {
        if (_disposed) return;

        _visible = visible;

        if (IsPopupMenu && visible)
        {
            ShowPopup(_locationX, _locationY);
        }
    }

    public bool GetVisible()
    {
        return _visible;
    }

    public void SetLocation(int x, int y)
    {
        _locationX = x;
        _locationY = y;
    }

    public void ShowPopup(int x, int y)
    {
        if (_disposed || _nsMenu == IntPtr.Zero) return;

        // [menu popUpMenuPositioningItem:nil atLocation:location inView:nil]
        IntPtr selPopUp = sel_registerName("popUpMenuPositioningItem:atLocation:inView:");
        var location = new CGPoint(x, y);
        objc_msgSend_void(_nsMenu, selPopUp, IntPtr.Zero, location, IntPtr.Zero);

        _visible = true;
    }

    public void AttachToWindow(IPlatformWindow? window)
    {
        if (_disposed || _nsMenu == IntPtr.Zero) return;

        if (!IsMenuBar)
        {
            // Only menu bars can be attached to windows
            return;
        }

        if (window == null)
        {
            _attachedWindow = IntPtr.Zero;
            return;
        }

        // Get window handle
        IntPtr windowHandle = IntPtr.Zero;
        if (window is MacOSWindow macOSWindow)
        {
            windowHandle = macOSWindow.GetWindowHandle();
        }

        if (windowHandle == IntPtr.Zero) return;

        _attachedWindow = windowHandle;

        // On macOS, the menu bar is application-wide, not per-window
        // Set this menu as the main menu for the application
        IntPtr nsAppClass = objc_getClass("NSApplication");
        IntPtr selSharedApplication = sel_registerName("sharedApplication");
        IntPtr nsApp = objc_msgSend(nsAppClass, selSharedApplication);

        IntPtr selSetMainMenu = sel_registerName("setMainMenu:");
        objc_msgSend_void(nsApp, selSetMainMenu, _nsMenu);
    }

    public IPlatformMenuItem CreateMenuItem(int style, int index)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSMenu));

        var menuItem = new MacOSMenuItem(this, style);

        // Add to NSMenu
        if (index < 0 || index >= _items.Count)
        {
            // Append to end
            IntPtr selAddItem = sel_registerName("addItem:");
            objc_msgSend_void(_nsMenu, selAddItem, menuItem.GetNativeHandle());
            _items.Add(menuItem);
        }
        else
        {
            // Insert at specific index
            IntPtr selInsertItem = sel_registerName("insertItem:atIndex:");
            objc_msgSend_void(_nsMenu, selInsertItem, menuItem.GetNativeHandle(), index);
            _items.Insert(index, menuItem);
        }

        return menuItem;
    }

    public void RemoveItem(IPlatformMenuItem item)
    {
        if (_disposed) return;

        if (item is MacOSMenuItem macOSItem)
        {
            IntPtr selRemoveItem = sel_registerName("removeItem:");
            objc_msgSend_void(_nsMenu, selRemoveItem, macOSItem.GetNativeHandle());
            _items.Remove(macOSItem);
        }
    }

    /// <summary>
    /// Gets the number of items in this menu.
    /// </summary>
    public int ItemCount
    {
        get
        {
            if (_disposed || _nsMenu == IntPtr.Zero) return 0;

            IntPtr selNumberOfItems = sel_registerName("numberOfItems");
            return (int)objc_msgSend_long(_nsMenu, selNumberOfItems);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            // Dispose all items first
            foreach (var item in _items.ToArray())
            {
                try
                {
                    item.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing menu item: {ex.Message}");
                }
            }
            _items.Clear();

            // Release NSMenu
            if (_nsMenu != IntPtr.Zero)
            {
                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(_nsMenu, selRelease);
                _nsMenu = IntPtr.Zero;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing menu: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }
}
