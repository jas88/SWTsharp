using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformToolBar using real NSToolbar.
/// NO pseudo-handles - uses actual NSToolbar and NSToolbarItem native objects.
/// </summary>
internal class MacOSToolBar : MacOSWidget, IPlatformToolBar
{
    private readonly IntPtr _nsToolbar;  // Real NSToolbar handle
    private readonly IntPtr _window;      // NSWindow this toolbar is attached to
    private readonly List<MacOSToolItem> _items = new();
    private bool _disposed;

    // Import Objective-C runtime functions
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1, long arg2);

    public MacOSToolBar(IntPtr window, int style)
    {
        if (window == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be zero", nameof(window));

        _window = window;

        // Create real NSToolbar
        _nsToolbar = CreateNSToolbar(style);

        if (_nsToolbar == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create NSToolbar");

        // Attach to window immediately
        AttachToWindow(window);
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsToolbar;
    }

    private IntPtr CreateNSToolbar(int style)
    {
        // NSToolbar* toolbar = [[NSToolbar alloc] initWithIdentifier:@"MainToolbar"];
        IntPtr identifier = CreateNSString("MainToolbar");

        IntPtr toolbarClass = objc_getClass("NSToolbar");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr toolbar = objc_msgSend(toolbarClass, selAlloc);

        IntPtr selInit = sel_registerName("initWithIdentifier:");
        toolbar = objc_msgSend(toolbar, selInit, identifier);

        // Release identifier NSString
        IntPtr selRelease = sel_registerName("release");
        objc_msgSend_void(identifier, selRelease);

        if (toolbar == IntPtr.Zero)
            return IntPtr.Zero;

        // Configure toolbar
        IntPtr selSetAllowsUserCustomization = sel_registerName("setAllowsUserCustomization:");
        objc_msgSend_void(toolbar, selSetAllowsUserCustomization, false);

        IntPtr selSetAutosavesConfiguration = sel_registerName("setAutosavesConfiguration:");
        objc_msgSend_void(toolbar, selSetAutosavesConfiguration, false);

        // Set display mode based on style
        // NSToolbarDisplayModeDefault = 0, NSToolbarDisplayModeIconAndLabel = 1,
        // NSToolbarDisplayModeIconOnly = 2, NSToolbarDisplayModeLabelOnly = 3
        long displayMode = (style & SWT.FLAT) != 0 ? 2 : 1; // Icon only for flat, icon+label otherwise
        IntPtr selSetDisplayMode = sel_registerName("setDisplayMode:");
        objc_msgSend_void(toolbar, selSetDisplayMode, displayMode);

        return toolbar;
    }

    private IntPtr CreateNSString(string text)
    {
        // Create NSString from C# string using UTF8
        IntPtr strClass = objc_getClass("NSString");
        IntPtr selector = sel_registerName("stringWithUTF8String:");
        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(text);

        try
        {
            return objc_msgSend(strClass, selector, utf8Ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(utf8Ptr);
        }
    }

    public void AddItem(string text, IPlatformImage? image)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolBar));

        // ROBUST: Validate parameters
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Item text cannot be null or empty", nameof(text));

        try
        {
            // Create NSToolbarItem with unique identifier
            string identifier = $"item_{_items.Count}_{Guid.NewGuid():N}";
            var toolItem = new MacOSToolItem(_nsToolbar, identifier);
            toolItem.SetText(text);

            if (image != null)
            {
                toolItem.SetImage(image);
            }

            _items.Add(toolItem);

            // Insert into toolbar
            IntPtr sel = sel_registerName("insertItemWithItemIdentifier:atIndex:");
            IntPtr nsIdentifier = CreateNSString(identifier);

            try
            {
                objc_msgSend_void(_nsToolbar, sel, nsIdentifier, (long)(_items.Count - 1));
            }
            finally
            {
                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(nsIdentifier, selRelease);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to add toolbar item: {ex.Message}");
            throw;
        }
    }

    public void RemoveItem(int index)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolBar));

        // ROBUST: Validate bounds BEFORE modifying collection
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                index,
                $"Index must be between 0 and {_items.Count - 1}, but was {index}"
            );
        }

        try
        {
            var item = _items[index];

            // Remove from NSToolbar
            IntPtr sel = sel_registerName("removeItemAtIndex:");
            objc_msgSend_void(_nsToolbar, sel, (long)index);

            // Dispose and remove from list
            item.Dispose();
            _items.RemoveAt(index);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to remove toolbar item at index {index}: {ex.Message}");
            throw;
        }
    }

    private void AttachToWindow(IntPtr window)
    {
        if (_disposed || window == IntPtr.Zero) return;

        // [window setToolbar:toolbar];
        IntPtr sel = sel_registerName("setToolbar:");
        objc_msgSend_void(window, sel, _nsToolbar);
    }

    public void AttachToWindow(IPlatformWindow window)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolBar));

        if (window == null)
            throw new ArgumentNullException(nameof(window));

        if (window is MacOSWindow macOSWindow)
        {
            AttachToWindow(macOSWindow.GetNativeHandle());
        }
    }

    public int GetItemCount()
    {
        if (_disposed) return 0;
        return _items.Count;
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            // ROBUST: Dispose items first
            foreach (var item in _items.ToArray())
            {
                try
                {
                    item.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing toolbar item: {ex.Message}");
                }
            }
            _items.Clear();

            // Detach from window
            if (_window != IntPtr.Zero)
            {
                IntPtr sel = sel_registerName("setToolbar:");
                objc_msgSend_void(_window, sel, IntPtr.Zero);
            }

            // Release NSToolbar
            if (_nsToolbar != IntPtr.Zero)
            {
                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(_nsToolbar, selRelease);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing toolbar: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }
}
