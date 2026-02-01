using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformMenuItem using NSMenuItem.
/// Supports PUSH, CHECK, RADIO, CASCADE, and SEPARATOR styles.
/// </summary>
internal class MacOSMenuItem : MacOSWidget, IPlatformMenuItem
{
    private IntPtr _nsMenuItem;
    private readonly MacOSMenu _parentMenu;
    private readonly int _style;
    private MacOSMenu? _submenu;
    private bool _disposed;
    private string _text = string.Empty;
    private int _accelerator;
    private bool _selection;

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
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, ulong arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_long(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    // NSMenuItem state constants
    private const long NSControlStateValueOff = 0;
    private const long NSControlStateValueOn = 1;
    private const long NSControlStateValueMixed = -1;

    // Modifier masks for keyboard accelerators
    private const ulong NSEventModifierFlagShift = 1 << 17;
    private const ulong NSEventModifierFlagControl = 1 << 18;
    private const ulong NSEventModifierFlagOption = 1 << 19;
    private const ulong NSEventModifierFlagCommand = 1 << 20;

    // Event handling
#pragma warning disable CS0067 // Events not yet fully wired - callback mechanism needs Objective-C runtime class
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    public event EventHandler? Selected;
#pragma warning restore CS0067

    public MacOSMenuItem(MacOSMenu parentMenu, int style)
    {
        _parentMenu = parentMenu ?? throw new ArgumentNullException(nameof(parentMenu));
        _style = style;

        if (IsSeparator)
        {
            _nsMenuItem = CreateSeparatorItem();
        }
        else
        {
            _nsMenuItem = CreateNSMenuItem();
        }

        if (_nsMenuItem == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create NSMenuItem");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsMenuItem;
    }

    private IntPtr CreateNSMenuItem()
    {
        // [[NSMenuItem alloc] initWithTitle:@"" action:nil keyEquivalent:@""]
        IntPtr menuItemClass = objc_getClass("NSMenuItem");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInitWithTitle = sel_registerName("initWithTitle:action:keyEquivalent:");

        IntPtr item = objc_msgSend(menuItemClass, selAlloc);

        // Create empty NSStrings for title and key equivalent
        IntPtr emptyString = CreateNSString(string.Empty);

        item = objc_msgSend(item, selInitWithTitle, emptyString, IntPtr.Zero, emptyString);

        return item;
    }

    private IntPtr CreateSeparatorItem()
    {
        // [NSMenuItem separatorItem]
        IntPtr menuItemClass = objc_getClass("NSMenuItem");
        IntPtr selSeparatorItem = sel_registerName("separatorItem");

        return objc_msgSend(menuItemClass, selSeparatorItem);
    }

    private IntPtr CreateNSString(string text)
    {
        IntPtr strClass = objc_getClass("NSString");
        IntPtr selector = sel_registerName("stringWithUTF8String:");
        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(text ?? string.Empty);

        try
        {
            return objc_msgSend(strClass, selector, utf8Ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(utf8Ptr);
        }
    }

    private string GetNSStringValue(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero)
            return string.Empty;

        IntPtr selUTF8String = sel_registerName("UTF8String");
        IntPtr utf8Ptr = objc_msgSend(nsString, selUTF8String);

        if (utf8Ptr == IntPtr.Zero)
            return string.Empty;

        return Marshal.PtrToStringAnsi(utf8Ptr) ?? string.Empty;
    }

    public void SetText(string text)
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator) return;

        _text = text ?? string.Empty;

        IntPtr nsTitle = CreateNSString(_text);
        IntPtr selSetTitle = sel_registerName("setTitle:");
        objc_msgSend_void(_nsMenuItem, selSetTitle, nsTitle);
    }

    public string GetText()
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator)
            return string.Empty;

        IntPtr selTitle = sel_registerName("title");
        IntPtr nsTitle = objc_msgSend(_nsMenuItem, selTitle);
        return GetNSStringValue(nsTitle);
    }

    public void SetImage(IPlatformImage? image)
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator) return;

        IntPtr nsImage = IntPtr.Zero;
        if (image is MacOSImage macOSImage)
        {
            nsImage = macOSImage.GetNativeHandle();
        }

        IntPtr selSetImage = sel_registerName("setImage:");
        objc_msgSend_void(_nsMenuItem, selSetImage, nsImage);
    }

    public void SetAccelerator(int accelerator)
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator) return;

        _accelerator = accelerator;

        // SWT accelerator format: modifier flags OR'd with key code
        // Special keys have KEYCODE_BIT (0x1000000) set, ASCII chars are in lower bits
        int keyCode;
        if ((accelerator & SWT.KEYCODE_BIT) != 0)
        {
            // Special key (F1-F12, arrows, etc.) - preserve the full key code
            keyCode = accelerator & (SWT.KEYCODE_BIT | 0xFF);
        }
        else
        {
            // ASCII character - mask off any modifier bits in high bytes
            keyCode = accelerator & 0xFF;
        }

        // Build key equivalent string for macOS
        string keyEquivalent = GetMacOSKeyEquivalent(keyCode);

        // Set key equivalent
        IntPtr nsKeyEquivalent = CreateNSString(keyEquivalent);
        IntPtr selSetKeyEquivalent = sel_registerName("setKeyEquivalent:");
        objc_msgSend_void(_nsMenuItem, selSetKeyEquivalent, nsKeyEquivalent);

        // Build modifier mask from SWT modifier flags
        ulong modifierMask = 0;

        if ((accelerator & SWT.COMMAND) != 0)
            modifierMask |= NSEventModifierFlagCommand;
        if ((accelerator & SWT.SHIFT) != 0)
            modifierMask |= NSEventModifierFlagShift;
        if ((accelerator & SWT.ALT) != 0)
            modifierMask |= NSEventModifierFlagOption;
        if ((accelerator & SWT.CTRL) != 0)
            modifierMask |= NSEventModifierFlagControl;

        // Set modifier mask
        IntPtr selSetKeyEquivalentModifierMask = sel_registerName("setKeyEquivalentModifierMask:");
        objc_msgSend_void(_nsMenuItem, selSetKeyEquivalentModifierMask, modifierMask);
    }

    /// <summary>
    /// Converts an SWT key code to a macOS key equivalent string.
    /// </summary>
    private static string GetMacOSKeyEquivalent(int keyCode)
    {
        // Handle special keys (F1-F12, arrows, etc.)
        if ((keyCode & SWT.KEYCODE_BIT) != 0)
        {
            int specialKey = keyCode & 0xFF;
            return specialKey switch
            {
                1 => "\uF700",  // ARROW_UP - NSUpArrowFunctionKey
                2 => "\uF701",  // ARROW_DOWN - NSDownArrowFunctionKey
                3 => "\uF702",  // ARROW_LEFT - NSLeftArrowFunctionKey
                4 => "\uF703",  // ARROW_RIGHT - NSRightArrowFunctionKey
                5 => "\uF72C",  // PAGE_UP - NSPageUpFunctionKey
                6 => "\uF72D",  // PAGE_DOWN - NSPageDownFunctionKey
                7 => "\uF729",  // HOME - NSHomeFunctionKey
                8 => "\uF72B",  // END - NSEndFunctionKey
                9 => "\uF727",  // INSERT - NSInsertFunctionKey
                10 => "\uF704", // F1 - NSF1FunctionKey
                11 => "\uF705", // F2 - NSF2FunctionKey
                12 => "\uF706", // F3 - NSF3FunctionKey
                13 => "\uF707", // F4 - NSF4FunctionKey
                14 => "\uF708", // F5 - NSF5FunctionKey
                15 => "\uF709", // F6 - NSF6FunctionKey
                16 => "\uF70A", // F7 - NSF7FunctionKey
                17 => "\uF70B", // F8 - NSF8FunctionKey
                18 => "\uF70C", // F9 - NSF9FunctionKey
                19 => "\uF70D", // F10 - NSF10FunctionKey
                20 => "\uF70E", // F11 - NSF11FunctionKey
                21 => "\uF70F", // F12 - NSF12FunctionKey
                _ => string.Empty
            };
        }

        // Handle ASCII characters
        return keyCode switch
        {
            SWT.CR => "\r",
            SWT.BS => "\b",
            SWT.DEL => "\u007F",
            SWT.ESC => "\u001B",
            SWT.TAB => "\t",
            _ when keyCode >= 32 && keyCode < 127 => ((char)keyCode).ToString().ToLowerInvariant(),
            _ => string.Empty
        };
    }

    public int GetAccelerator()
    {
        return _accelerator;
    }

    public void SetSelection(bool selected)
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator) return;

        if (!IsCheck && !IsRadio) return;

        _selection = selected;

        long state = selected ? NSControlStateValueOn : NSControlStateValueOff;
        IntPtr selSetState = sel_registerName("setState:");
        objc_msgSend_void(_nsMenuItem, selSetState, state);
    }

    public bool GetSelection()
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator)
            return false;

        if (!IsCheck && !IsRadio)
            return false;

        IntPtr selState = sel_registerName("state");
        long state = objc_msgSend_long(_nsMenuItem, selState);
        return state == NSControlStateValueOn;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator) return;

        IntPtr selSetEnabled = sel_registerName("setEnabled:");
        objc_msgSend_void(_nsMenuItem, selSetEnabled, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator)
            return false;

        IntPtr selIsEnabled = sel_registerName("isEnabled");
        return objc_msgSend_bool(_nsMenuItem, selIsEnabled);
    }

    public void SetMenu(IPlatformMenu? menu)
    {
        if (_disposed || _nsMenuItem == IntPtr.Zero || IsSeparator) return;

        if (!IsCascade) return;

        _submenu = menu as MacOSMenu;

        IntPtr nsSubmenu = _submenu?.GetNativeHandle() ?? IntPtr.Zero;
        IntPtr selSetSubmenu = sel_registerName("setSubmenu:");
        objc_msgSend_void(_nsMenuItem, selSetSubmenu, nsSubmenu);
    }

    public IPlatformMenu? GetMenu()
    {
        return _submenu;
    }

    public bool IsSeparator => (_style & SWT.SEPARATOR) != 0;

    public bool IsCascade => (_style & SWT.CASCADE) != 0;

    public bool IsCheck => (_style & SWT.CHECK) != 0;

    public bool IsRadio => (_style & SWT.RADIO) != 0;

    public bool IsPush => !IsSeparator && !IsCascade && !IsCheck && !IsRadio;

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            // Dispose submenu if cascade
            if (_submenu != null)
            {
                _submenu.Dispose();
                _submenu = null;
            }

            // Note: Separator items are shared/cached by Cocoa and should NOT be released
            // Regular menu items created with alloc+init need to be released
            if (_nsMenuItem != IntPtr.Zero && !IsSeparator)
            {
                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(_nsMenuItem, selRelease);
            }
            _nsMenuItem = IntPtr.Zero;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing menu item: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }
}
