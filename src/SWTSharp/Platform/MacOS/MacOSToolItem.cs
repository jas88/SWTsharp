using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformToolItem using real NSToolbarItem.
/// NO pseudo-handles - uses actual NSToolbarItem native object.
/// </summary>
internal class MacOSToolItem : MacOSWidget, IPlatformToolItem
{
    private readonly IntPtr _nsToolbarItem;
    private readonly string _identifier;
    private bool _disposed;

    // Event handling
#pragma warning disable CS0067 // Events not yet implemented - will be added when event handling is implemented
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

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
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    public MacOSToolItem(IntPtr toolbar, string identifier)
    {
        if (toolbar == IntPtr.Zero)
            throw new ArgumentException("Toolbar handle cannot be zero", nameof(toolbar));

        if (string.IsNullOrEmpty(identifier))
            throw new ArgumentException("Identifier cannot be null or empty", nameof(identifier));

        _identifier = identifier;
        _nsToolbarItem = CreateNSToolbarItem(identifier);

        if (_nsToolbarItem == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create NSToolbarItem");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsToolbarItem;
    }

    private IntPtr CreateNSToolbarItem(string identifier)
    {
        // NSToolbarItem* item = [[NSToolbarItem alloc] initWithItemIdentifier:identifier];
        IntPtr nsIdentifier = CreateNSString(identifier);

        IntPtr itemClass = objc_getClass("NSToolbarItem");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr item = objc_msgSend(itemClass, selAlloc);

        IntPtr selInit = sel_registerName("initWithItemIdentifier:");
        item = objc_msgSend(item, selInit, nsIdentifier);

        // Release identifier NSString
        IntPtr selRelease = sel_registerName("release");
        objc_msgSend_void(nsIdentifier, selRelease);

        return item;
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

    public void SetText(string text)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolItem));

        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Text cannot be null or empty", nameof(text));

        try
        {
            IntPtr nsLabel = CreateNSString(text);
            IntPtr selSetLabel = sel_registerName("setLabel:");
            objc_msgSend_void(_nsToolbarItem, selSetLabel, nsLabel);

            // Release NSString
            IntPtr selRelease = sel_registerName("release");
            objc_msgSend_void(nsLabel, selRelease);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set toolbar item text: {ex.Message}");
            throw;
        }
    }

    public string GetText()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolItem));

        try
        {
            IntPtr selLabel = sel_registerName("label");
            IntPtr nsLabel = objc_msgSend(_nsToolbarItem, selLabel);

            if (nsLabel == IntPtr.Zero)
                return string.Empty;

            IntPtr selUTF8String = sel_registerName("UTF8String");
            IntPtr utf8Ptr = objc_msgSend(nsLabel, selUTF8String);

            if (utf8Ptr == IntPtr.Zero)
                return string.Empty;

            return Marshal.PtrToStringAnsi(utf8Ptr) ?? string.Empty;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get toolbar item text: {ex.Message}");
            return string.Empty;
        }
    }

    public void SetImage(IPlatformImage? image)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolItem));

        try
        {
            if (image == null)
            {
                IntPtr selSetImage = sel_registerName("setImage:");
                objc_msgSend_void(_nsToolbarItem, selSetImage, IntPtr.Zero);
                return;
            }

            // Get NSImage handle from platform image
            IntPtr nsImage = IntPtr.Zero;
            if (image is MacOSImage macOSImage)
            {
                nsImage = macOSImage.GetNativeHandle();
            }

            if (nsImage != IntPtr.Zero)
            {
                IntPtr selSetImage = sel_registerName("setImage:");
                objc_msgSend_void(_nsToolbarItem, selSetImage, nsImage);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set toolbar item image: {ex.Message}");
        }
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSToolItem));

        try
        {
            IntPtr selSetEnabled = sel_registerName("setEnabled:");
            objc_msgSend_void(_nsToolbarItem, selSetEnabled, enabled);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to set toolbar item enabled state: {ex.Message}");
        }
    }

    public bool GetEnabled()
    {
        if (_disposed)
            return false;

        try
        {
            IntPtr selIsEnabled = sel_registerName("isEnabled");
            return objc_msgSend_bool(_nsToolbarItem, selIsEnabled);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to get toolbar item enabled state: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            // Release NSToolbarItem
            if (_nsToolbarItem != IntPtr.Zero)
            {
                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(_nsToolbarItem, selRelease);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing toolbar item: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }
}
