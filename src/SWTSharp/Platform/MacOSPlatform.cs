using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation using Cocoa/AppKit via Objective-C runtime.
/// </summary>
internal partial class MacOSPlatform : IPlatform
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";
    private const string FoundationFramework = "/System/Library/Frameworks/Foundation.framework/Foundation";

    // Objective-C runtime functions
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
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_long(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern double objc_msgSend_double(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double(IntPtr receiver, IntPtr selector, double arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_size(IntPtr receiver, IntPtr selector, CGSize size);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public CGRect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGSize
    {
        public double width;
        public double height;

        public CGSize(double width, double height)
        {
            this.width = width;
            this.height = height;
        }
    }

    // Cached selectors
    private IntPtr _selAlloc;
    private IntPtr _selInit;
    private IntPtr _selInitWithContentRect;
    private IntPtr _selSetTitle;
    private IntPtr _selMakeKeyAndOrderFront;
    private IntPtr _selOrderOut;
    private IntPtr _selClose;
    private IntPtr _selSetFrame;
    private IntPtr _selFrame;
    private IntPtr _selSharedApplication;
    private IntPtr _selRun;
    private IntPtr _selStop;
    private IntPtr _selNextEventMatchingMask;
    private IntPtr _selSendEvent;
    private IntPtr _selUpdateWindows;

    // Shared selectors (used by multiple widgets: ProgressBar, Slider, Scale)
    private IntPtr _selSetMinValue;
    private IntPtr _selSetMaxValue;
    private IntPtr _selSetDoubleValue;

    // Cached classes
    private IntPtr _nsApplicationClass;
    private IntPtr _nsWindowClass;
    private IntPtr _nsStringClass;
    private IntPtr _nsApplication;
    private IntPtr _nsProgressIndicatorClass;

    // Window style masks
    private const ulong NSWindowStyleMaskTitled = 1 << 0;
    private const ulong NSWindowStyleMaskClosable = 1 << 1;
    private const ulong NSWindowStyleMaskMiniaturizable = 1 << 2;
    private const ulong NSWindowStyleMaskResizable = 1 << 3;

    // Event masks
    private const ulong NSEventMaskAny = ulong.MaxValue;

    public MacOSPlatform()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Initialize selectors
        _selAlloc = sel_registerName("alloc");
        _selInit = sel_registerName("init");
        _selInitWithContentRect = sel_registerName("initWithContentRect:styleMask:backing:defer:");
        _selSetTitle = sel_registerName("setTitle:");
        _selMakeKeyAndOrderFront = sel_registerName("makeKeyAndOrderFront:");
        _selOrderOut = sel_registerName("orderOut:");
        _selClose = sel_registerName("close");
        _selSetFrame = sel_registerName("setFrame:display:");
        _selFrame = sel_registerName("frame");
        _selSharedApplication = sel_registerName("sharedApplication");
        _selRun = sel_registerName("run");
        _selStop = sel_registerName("stop:");
        _selNextEventMatchingMask = sel_registerName("nextEventMatchingMask:untilDate:inMode:dequeue:");
        _selSendEvent = sel_registerName("sendEvent:");
        _selUpdateWindows = sel_registerName("updateWindows");

        // Initialize shared widget selectors (used by many widgets)
        _selSetMinValue = sel_registerName("setMinValue:");
        _selSetMaxValue = sel_registerName("setMaxValue:");
        _selSetDoubleValue = sel_registerName("setDoubleValue:");
        _selAddSubview = sel_registerName("addSubview:");

        // Get classes
        _nsApplicationClass = objc_getClass("NSApplication");
        _nsWindowClass = objc_getClass("NSWindow");
        _nsStringClass = objc_getClass("NSString");
        _nsProgressIndicatorClass = objc_getClass("NSProgressIndicator");

        // Get or create shared application
        _nsApplication = objc_msgSend(_nsApplicationClass, _selSharedApplication);
    }

    // Helper method for creating index sets (used by Table and other widgets)
    private IntPtr CreateIndexSetForAllColumns(int columnCount)
    {
        IntPtr nsMutableIndexSetClass = objc_getClass("NSMutableIndexSet");
        IntPtr indexSet = objc_msgSend(nsMutableIndexSetClass, _selAlloc);
        indexSet = objc_msgSend(indexSet, _selInit);

        IntPtr selAddIndex = sel_registerName("addIndex:");
        for (int i = 0; i < columnCount; i++)
        {
            objc_msgSend_ulong(indexSet, selAddIndex, (nuint)i);
        }

        return indexSet;
    }

    public IntPtr CreateWindow(int style, string title)
    {
        // Determine window style mask
        ulong styleMask = NSWindowStyleMaskTitled | NSWindowStyleMaskClosable | NSWindowStyleMaskMiniaturizable;

        if ((style & SWT.RESIZE) != 0)
        {
            styleMask |= NSWindowStyleMaskResizable;
        }

        // Create window frame
        var frame = new CGRect(100, 100, 800, 600);

        // Allocate and initialize window
        IntPtr window = objc_msgSend(_nsWindowClass, _selAlloc);

        // initWithContentRect:styleMask:backing:defer:
        // This is a complex call - we need to pass multiple arguments
        window = InitWindow(window, frame, styleMask);

        // Set title
        SetWindowText(window, title);

        return window;
    }

    private IntPtr InitWindow(IntPtr window, CGRect frame, ulong styleMask)
    {
        // For complex Objective-C calls, we use objc_msgSend with proper signatures
        // backing: NSBackingStoreBuffered = 2, defer: NO = 0
        const int NSBackingStoreBuffered = 2;

        // This is a simplified version - in production you'd want proper marshaling
        IntPtr sel = sel_registerName("initWithContentRect:styleMask:backing:defer:");

        // Use unsafe code for complex marshaling
        unsafe
        {
            // Create argument buffer
            var args = stackalloc IntPtr[4];
            args[0] = *((IntPtr*)&frame); // contentRect
            args[1] = (IntPtr)styleMask;   // styleMask
            args[2] = (IntPtr)NSBackingStoreBuffered; // backing
            args[3] = IntPtr.Zero;         // defer (NO)

            // Call via objc_msgSend - simplified version
            // In reality, this needs more careful handling
            return objc_msgSend(window, sel, (IntPtr)(&frame), (IntPtr)styleMask);
        }
    }

    public void DestroyWindow(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            objc_msgSend_void(handle, _selClose);
        }
    }

    public void SetWindowVisible(IntPtr handle, bool visible)
    {
        if (visible)
        {
            objc_msgSend(handle, _selMakeKeyAndOrderFront, IntPtr.Zero);
        }
        else
        {
            objc_msgSend(handle, _selOrderOut, IntPtr.Zero);
        }
    }

    public void SetWindowText(IntPtr handle, string text)
    {
        // Create NSString
        IntPtr nsString = CreateNSString(text);
        objc_msgSend(handle, _selSetTitle, nsString);
    }

    // SEC-002: Already has proper try-finally for memory management
    private IntPtr CreateNSString(string str)
    {
        IntPtr sel = sel_registerName("stringWithUTF8String:");
        IntPtr utf8 = Marshal.StringToHGlobalAnsi(str);
        try
        {
            return objc_msgSend(_nsStringClass, sel, utf8);
        }
        finally
        {
            Marshal.FreeHGlobal(utf8);
        }
    }

    public void SetWindowSize(IntPtr handle, int width, int height)
    {
        // Get current frame
        objc_msgSend_stret(out CGRect frame, handle, _selFrame);

        // Update size
        frame.width = width;
        frame.height = height;

        // Set new frame
        objc_msgSend_rect(handle, _selSetFrame, frame);
    }

    public void SetWindowLocation(IntPtr handle, int x, int y)
    {
        // Get current frame
        objc_msgSend_stret(out CGRect frame, handle, _selFrame);

        // Update location (note: Cocoa uses bottom-left origin)
        frame.x = x;
        frame.y = y;

        // Set new frame
        objc_msgSend_rect(handle, _selSetFrame, frame);
    }

    public bool ProcessEvent()
    {
        // Create date for immediate timeout
        IntPtr nsDateClass = objc_getClass("NSDate");
        IntPtr selDistantPast = sel_registerName("distantPast");
        IntPtr distantPast = objc_msgSend(nsDateClass, selDistantPast);

        // Get default run loop mode
        IntPtr nsDefaultRunLoopMode = CreateNSString("kCFRunLoopDefaultMode");

        // Poll for event
        IntPtr evt = NextEvent(NSEventMaskAny, distantPast, nsDefaultRunLoopMode, true);

        if (evt != IntPtr.Zero)
        {
            objc_msgSend(_nsApplication, _selSendEvent, evt);
            objc_msgSend_void(_nsApplication, _selUpdateWindows);
            return true;
        }

        return false;
    }

    private IntPtr NextEvent(ulong mask, IntPtr untilDate, IntPtr mode, bool dequeue)
    {
        // nextEventMatchingMask:untilDate:inMode:dequeue:
        // This is complex - simplified version
        unsafe
        {
            var args = stackalloc IntPtr[4];
            args[0] = (IntPtr)mask;
            args[1] = untilDate;
            args[2] = mode;
            args[3] = dequeue ? (IntPtr)1 : IntPtr.Zero;

            return objc_msgSend(_nsApplication, _selNextEventMatchingMask, args[0], args[1]);
        }
    }

    public void WaitForEvent()
    {
        // Block until event available
        IntPtr nsDateClass = objc_getClass("NSDate");
        IntPtr selDistantFuture = sel_registerName("distantFuture");
        IntPtr distantFuture = objc_msgSend(nsDateClass, selDistantFuture);

        IntPtr nsDefaultRunLoopMode = CreateNSString("kCFRunLoopDefaultMode");

        IntPtr evt = NextEvent(NSEventMaskAny, distantFuture, nsDefaultRunLoopMode, true);

        if (evt != IntPtr.Zero)
        {
            objc_msgSend(_nsApplication, _selSendEvent, evt);
            objc_msgSend_void(_nsApplication, _selUpdateWindows);
        }
    }

    public void WakeEventLoop()
    {
        // Stop the event loop
        objc_msgSend(_nsApplication, _selStop, _nsApplication);
    }

    public IntPtr CreateComposite(int style)
    {
        // Create an NSView instance to act as a container
        IntPtr nsViewClass = objc_getClass("NSView");
        IntPtr view = objc_msgSend(objc_msgSend(nsViewClass, _selAlloc), _selInit);

        // Set a default frame
        CGRect frame = new CGRect(0, 0, 100, 100);
        IntPtr selSetFrame = sel_registerName("setFrame:");
        objc_msgSend_rect(view, selSetFrame, frame);

        return view;
    }

    // Button-specific selectors and classes
    private IntPtr _nsButtonClass;
    private IntPtr _selButtonWithTitle;
    private IntPtr _selSetButtonType;
    private IntPtr _selSetState;
    private IntPtr _selState;
    private IntPtr _selSetTarget;
    private IntPtr _selSetAction;
    private IntPtr _selSetEnabled;
    private IntPtr _selSetHidden;
    private IntPtr _selSetFrameOrigin;
    private IntPtr _selSetFrameSize;
    private IntPtr _selAddSubview;

    // NSButton types
    private const int NSButtonTypeMomentaryLight = 0;
    private const int NSButtonTypePushOnPushOff = 1;
    private const int NSButtonTypeToggle = 2;
    private const int NSButtonTypeSwitch = 3;
    private const int NSButtonTypeRadio = 4;
    private const int NSButtonTypeMomentaryChange = 5;
    private const int NSButtonTypeOnOff = 6;

    // NSControl states
    private const int NSControlStateValueOff = 0;
    private const int NSControlStateValueOn = 1;

    private readonly Dictionary<IntPtr, Action> _buttonCallbacks = new Dictionary<IntPtr, Action>();

    // LEAK-002: Cleanup method for button callbacks
    public void ClearButtonCallbacks()
    {
        _buttonCallbacks.Clear();
    }

    // LEAK-002: Remove specific button callback when control is destroyed
    public void RemoveButtonCallback(IntPtr handle)
    {
        _buttonCallbacks.Remove(handle);
    }

    // Cleanup method for list items
    public void ClearListItems()
    {
        _listItems.Clear();
    }

    // Remove specific list items when control is destroyed
    public void RemoveListItemsForControl(IntPtr handle)
    {
        _listItems.Remove(handle);
    }

    private void InitializeButtonSelectors()
    {
        if (_nsButtonClass == IntPtr.Zero)
        {
            _nsButtonClass = objc_getClass("NSButton");
            _selButtonWithTitle = sel_registerName("buttonWithTitle:target:action:");
            _selSetButtonType = sel_registerName("setButtonType:");
            _selSetState = sel_registerName("setState:");
            _selState = sel_registerName("state");
            _selSetTarget = sel_registerName("setTarget:");
            _selSetAction = sel_registerName("setAction:");
            _selSetEnabled = sel_registerName("setEnabled:");
            _selSetHidden = sel_registerName("setHidden:");
            _selSetFrameOrigin = sel_registerName("setFrameOrigin:");
            _selSetFrameSize = sel_registerName("setFrameSize:");
            // Note: _selAddSubview is now initialized in Initialize() method
        }
    }

    public IntPtr CreateButton(IntPtr parent, int style, string text)
    {
        InitializeButtonSelectors();

        // Create NSButton
        IntPtr button = objc_msgSend(_nsButtonClass, _selAlloc);
        button = objc_msgSend(button, _selInit);

        // Set button type based on SWT style
        int buttonType;
        if ((style & SWT.CHECK) != 0)
        {
            buttonType = NSButtonTypeSwitch;
        }
        else if ((style & SWT.RADIO) != 0)
        {
            buttonType = NSButtonTypeRadio;
        }
        else if ((style & SWT.TOGGLE) != 0)
        {
            buttonType = NSButtonTypePushOnPushOff;
        }
        else // Default to PUSH
        {
            buttonType = NSButtonTypeMomentaryLight;
        }

        // Set button type
        objc_msgSend(button, _selSetButtonType, new IntPtr(buttonType));

        // Set title
        IntPtr nsTitle = CreateNSString(text);
        objc_msgSend(button, _selSetTitle, nsTitle);

        // Set default frame
        var frame = new CGRect(0, 0, 100, 30);
        objc_msgSend_rect(button, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            // Get content view of window
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, button);
        }

        return button;
    }

    public void SetButtonText(IntPtr handle, string text)
    {
        IntPtr nsText = CreateNSString(text);
        objc_msgSend(handle, _selSetTitle, nsText);
    }

    public void SetButtonSelection(IntPtr handle, bool selected)
    {
        int state = selected ? NSControlStateValueOn : NSControlStateValueOff;
        objc_msgSend(handle, _selSetState, new IntPtr(state));
    }

    public bool GetButtonSelection(IntPtr handle)
    {
        IntPtr state = objc_msgSend(handle, _selState);
        return state.ToInt32() == NSControlStateValueOn;
    }

    public void SetControlEnabled(IntPtr handle, bool enabled)
    {
        objc_msgSend_void(handle, _selSetEnabled, enabled);
    }

    public void SetControlVisible(IntPtr handle, bool visible)
    {
        objc_msgSend_void(handle, _selSetHidden, !visible);
    }

    public void SetControlBounds(IntPtr handle, int x, int y, int width, int height)
    {
        var frame = new CGRect(x, y, width, height);
        objc_msgSend_rect(handle, _selSetFrame, frame);
    }

    public void ConnectButtonClick(IntPtr handle, Action callback)
    {
        // Store callback
        _buttonCallbacks[handle] = callback;

        // In a full implementation, we would:
        // 1. Create an Objective-C target object
        // 2. Set up a selector that calls our C# callback
        // 3. Use setTarget:action: to connect it
        // For now, just store the callback
    }

    // Menu operations
    private IntPtr _nsMenuClass;
    private IntPtr _nsMenuItemClass;
    private IntPtr _selAddItem;
    private IntPtr _selSetSubmenu;
    private IntPtr _selSetTitle_item;
    // _selSetState is already defined in button section
    private IntPtr _selSetEnabled_item;
    private IntPtr _selSetMainMenu;
    private IntPtr _selPopUpMenuPositioningItem;

    private void InitializeMenuSelectors()
    {
        if (_nsMenuClass == IntPtr.Zero)
        {
            _nsMenuClass = objc_getClass("NSMenu");
            _nsMenuItemClass = objc_getClass("NSMenuItem");
            _selAddItem = sel_registerName("addItem:");
            _selSetSubmenu = sel_registerName("setSubmenu:");
            _selSetTitle_item = sel_registerName("setTitle:");
            _selSetState = sel_registerName("setState:");
            _selSetEnabled_item = sel_registerName("setEnabled:");
            _selSetMainMenu = sel_registerName("setMainMenu:");
            _selPopUpMenuPositioningItem = sel_registerName("popUpMenuPositioningItem:atLocation:inView:");
        }
    }

    IntPtr IPlatform.CreateMenu(int style)
    {
        InitializeMenuSelectors();

        // Allocate and initialize menu
        IntPtr menu = objc_msgSend(_nsMenuClass, _selAlloc);
        menu = objc_msgSend(menu, _selInit);

        return menu;
    }

    void IPlatform.DestroyMenu(IntPtr handle)
    {
        // NSMenu is reference counted, will be freed automatically
    }

    void IPlatform.SetShellMenuBar(IntPtr shellHandle, IntPtr menuHandle)
    {
        objc_msgSend(_nsApplication, _selSetMainMenu, menuHandle);
    }

    void IPlatform.SetMenuVisible(IntPtr handle, bool visible)
    {
        // Menu visibility is controlled by the application
    }

    void IPlatform.ShowPopupMenu(IntPtr menuHandle, int x, int y)
    {
        // Show popup menu at location
        // This requires an NSEvent and NSView - simplified version
        // objc_msgSend with proper parameters would be needed
    }

    IntPtr IPlatform.CreateMenuItem(IntPtr menuHandle, int style, int id, int index)
    {
        IntPtr menuItem;

        if ((style & SWT.SEPARATOR) != 0)
        {
            IntPtr selSeparatorItem = sel_registerName("separatorItem");
            menuItem = objc_msgSend(_nsMenuItemClass, selSeparatorItem);
        }
        else
        {
            // Create regular menu item
            IntPtr selInitWithTitle = sel_registerName("initWithTitle:action:keyEquivalent:");
            menuItem = objc_msgSend(_nsMenuItemClass, _selAlloc);

            IntPtr emptyString = CreateNSString(string.Empty);
            IntPtr selNull = IntPtr.Zero;

            // initWithTitle:action:keyEquivalent:
            menuItem = objc_msgSend(menuItem, selInitWithTitle, emptyString);
        }

        // Add to parent menu
        objc_msgSend(menuHandle, _selAddItem, menuItem);

        return menuItem;
    }

    void IPlatform.DestroyMenuItem(IntPtr handle)
    {
        // NSMenuItem is reference counted, will be freed automatically
    }

    void IPlatform.SetMenuItemText(IntPtr handle, string text)
    {
        IntPtr nsString = CreateNSString(text);
        objc_msgSend(handle, _selSetTitle_item, nsString);
    }

    void IPlatform.SetMenuItemSelection(IntPtr handle, bool selected)
    {
        // NSMenuItem state: 0=off, 1=on
        objc_msgSend(handle, _selSetState, (IntPtr)(selected ? 1 : 0));
    }

    void IPlatform.SetMenuItemEnabled(IntPtr handle, bool enabled)
    {
        objc_msgSend_void(handle, _selSetEnabled_item, enabled);
    }

    void IPlatform.SetMenuItemSubmenu(IntPtr itemHandle, IntPtr submenuHandle)
    {
        objc_msgSend(itemHandle, _selSetSubmenu, submenuHandle);
    }

    // Label operations - implemented in MacOSPlatform_Label.cs

    // Text control operations - selectors
    private IntPtr _nsTextFieldClass;
    private IntPtr _nsTextViewClass;
    private IntPtr _nsSecureTextFieldClass;
    private IntPtr _nsScrollViewClass;
    private IntPtr _selStringValue;
    private IntPtr _selSetStringValue;
    private IntPtr _selDocumentView;
    private IntPtr _selString;
    private IntPtr _selSetString;
    private IntPtr _selSetEditable_text;
    private IntPtr _selSelectedRange;
    private IntPtr _selSetSelectedRange;

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRange
    {
        public nuint location;
        public nuint length;

        public NSRange(int start, int length)
        {
            this.location = (nuint)start;
            this.length = (nuint)length;
        }
    }

    private void InitializeTextSelectors()
    {
        if (_nsTextFieldClass == IntPtr.Zero)
        {
            _nsTextFieldClass = objc_getClass("NSTextField");
            _nsTextViewClass = objc_getClass("NSTextView");
            _nsSecureTextFieldClass = objc_getClass("NSSecureTextField");
            _nsScrollViewClass = objc_getClass("NSScrollView");
            _selStringValue = sel_registerName("stringValue");
            _selSetStringValue = sel_registerName("setStringValue:");
            _selDocumentView = sel_registerName("documentView");
            _selString = sel_registerName("string");
            _selSetString = sel_registerName("setString:");
            _selSetEditable_text = sel_registerName("setEditable:");
            _selSelectedRange = sel_registerName("selectedRange");
            _selSetSelectedRange = sel_registerName("setSelectedRange:");
        }
    }

    public IntPtr CreateText(IntPtr parent, int style)
    {
        InitializeTextSelectors();

        IntPtr textControl;

        if ((style & SWT.PASSWORD) != 0)
        {
            // Create NSSecureTextField for password fields
            textControl = objc_msgSend(_nsSecureTextFieldClass, _selAlloc);
            textControl = objc_msgSend(textControl, _selInit);
        }
        else if ((style & SWT.MULTI) != 0)
        {
            // Create NSScrollView with NSTextView for multi-line text
            IntPtr scrollView = objc_msgSend(_nsScrollViewClass, _selAlloc);
            scrollView = objc_msgSend(scrollView, _selInit);

            // Create NSTextView
            IntPtr textView = objc_msgSend(_nsTextViewClass, _selAlloc);
            textView = objc_msgSend(textView, _selInit);

            // Set up scroll view
            IntPtr selSetDocumentView = sel_registerName("setDocumentView:");
            IntPtr selSetHasVerticalScroller = sel_registerName("setHasVerticalScroller:");
            IntPtr selSetHasHorizontalScroller = sel_registerName("setHasHorizontalScroller:");

            objc_msgSend(scrollView, selSetDocumentView, textView);
            objc_msgSend_void(scrollView, selSetHasVerticalScroller, true);

            if ((style & SWT.WRAP) == 0)
            {
                objc_msgSend_void(scrollView, selSetHasHorizontalScroller, true);
            }

            textControl = scrollView;
        }
        else
        {
            // Create NSTextField for single-line text
            textControl = objc_msgSend(_nsTextFieldClass, _selAlloc);
            textControl = objc_msgSend(textControl, _selInit);
        }

        // Set default frame
        var frame = new CGRect(0, 0, 100, 20);
        objc_msgSend_rect(textControl, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, textControl);
        }

        return textControl;
    }

    public void SetTextContent(IntPtr handle, string text)
    {
        InitializeTextSelectors();
        IntPtr nsText = CreateNSString(text ?? string.Empty);

        // Check if it's a scroll view (multi-line)
        IntPtr documentView = objc_msgSend(handle, _selDocumentView);
        if (documentView != IntPtr.Zero)
        {
            // It's a scroll view, set text on the text view
            objc_msgSend(documentView, _selSetString, nsText);
        }
        else
        {
            // Single-line or password field
            objc_msgSend(handle, _selSetStringValue, nsText);
        }
    }

    public string GetTextContent(IntPtr handle)
    {
        InitializeTextSelectors();

        // Check if it's a scroll view (multi-line)
        IntPtr documentView = objc_msgSend(handle, _selDocumentView);
        IntPtr nsString;

        if (documentView != IntPtr.Zero)
        {
            // It's a scroll view, get text from the text view
            nsString = objc_msgSend(documentView, _selString);
        }
        else
        {
            // Single-line or password field
            nsString = objc_msgSend(handle, _selStringValue);
        }

        return GetNSStringValue(nsString);
    }

    public void SetTextSelection(IntPtr handle, int start, int end)
    {
        InitializeTextSelectors();

        // Get the text view
        IntPtr textView = objc_msgSend(handle, _selDocumentView);
        if (textView == IntPtr.Zero)
        {
            textView = handle; // Single-line field
        }

        var range = new NSRange(start, end - start);
        unsafe
        {
            IntPtr rangePtr = (IntPtr)(&range);
            objc_msgSend(textView, _selSetSelectedRange, rangePtr);
        }
    }

    public (int Start, int End) GetTextSelection(IntPtr handle)
    {
        InitializeTextSelectors();

        // Get the text view
        IntPtr textView = objc_msgSend(handle, _selDocumentView);
        if (textView == IntPtr.Zero)
        {
            textView = handle; // Single-line field
        }

        NSRange range;
        unsafe
        {
            objc_msgSend_stret(out range, textView, _selSelectedRange);
        }

        return ((int)range.location, (int)(range.location + range.length));
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out NSRange retval, IntPtr receiver, IntPtr selector);

    public void SetTextLimit(IntPtr handle, int limit)
    {
        // NSTextField doesn't have a built-in character limit
        // This would require a delegate implementation
        // For now, we'll leave this as a no-op
    }

    public void SetTextReadOnly(IntPtr handle, bool readOnly)
    {
        InitializeTextSelectors();

        // Get the text view
        IntPtr textView = objc_msgSend(handle, _selDocumentView);
        if (textView == IntPtr.Zero)
        {
            textView = handle; // Single-line field
        }

        objc_msgSend_void(textView, _selSetEditable_text, !readOnly);
    }

    // Helper method to get string value from NSString
    private string GetNSStringValue(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero)
            return string.Empty;

        IntPtr selUTF8String = sel_registerName("UTF8String");
        IntPtr utf8 = objc_msgSend(nsString, selUTF8String);

        if (utf8 == IntPtr.Zero)
            return string.Empty;

        return Marshal.PtrToStringAnsi(utf8) ?? string.Empty;
    }

    // List control operations - using NSTableView

    // Widget implementations are in separate partial class files:
    // - MacOSPlatform_Label.cs: Label widget
    // - MacOSPlatform_Combo.cs: Combo/ComboBox widget
    // - MacOSPlatform_List.cs: List widget (NSTableView single column)
    // - MacOSPlatform_Group.cs: Group widget (NSBox)
    // - MacOSPlatform_Canvas.cs: Canvas widget (NSView)
    // - MacOSPlatform_ProgressBar.cs: ProgressBar widget (NSProgressIndicator)
    // - MacOSPlatform_TabFolder.cs: TabFolder and ToolBar widgets (NSTabView, ToolBar stubs)
    // - MacOSPlatform_Tree.cs: Tree widget (NSOutlineView)
    // - MacOSPlatform_Table.cs: Table widgets (NSTableView multi-column)
    // - MacOSPlatform_Slider.cs: Slider widget (NSSlider)
    // - MacOSPlatform_Scale.cs: Scale widget (NSSlider with ticks)
    // - MacOSPlatform_Spinner.cs: Spinner widget (NSStepper + NSTextField)
    // - MacOSPlatform_Dialogs.cs: All dialogs (NSAlert, NSOpenPanel, NSColorPanel, NSFontPanel)
}
