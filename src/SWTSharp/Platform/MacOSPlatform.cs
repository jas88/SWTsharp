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

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

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

    // Cached classes
    private IntPtr _nsApplicationClass;
    private IntPtr _nsWindowClass;
    private IntPtr _nsStringClass;
    private IntPtr _nsApplication;

    // Window style masks
    private const ulong NSWindowStyleMaskTitled = 1 << 0;
    private const ulong NSWindowStyleMaskClosable = 1 << 1;
    private const ulong NSWindowStyleMaskMiniaturizable = 1 << 2;
    private const ulong NSWindowStyleMaskResizable = 1 << 3;

    // Event masks
    private const ulong NSEventMaskAny = ulong.MaxValue;

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

        // Get classes
        _nsApplicationClass = objc_getClass("NSApplication");
        _nsWindowClass = objc_getClass("NSWindow");
        _nsStringClass = objc_getClass("NSString");

        // Get or create shared application
        _nsApplication = objc_msgSend(_nsApplicationClass, _selSharedApplication);
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
            _selAddSubview = sel_registerName("addSubview:");
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
    private IntPtr _nsTableViewClass;
    private IntPtr _nsTableColumnClass;
    private IntPtr _nsIndexSetClass;
    private IntPtr _selReloadData;
    private IntPtr _selNumberOfRows;
    private IntPtr _selSelectRowIndexes;
    private IntPtr _selSelectedRowIndexes;
    private IntPtr _selScrollRowToVisible;
    private IntPtr _selAddTableColumn;
    private IntPtr _selSetDataSource;
    private IntPtr _selSetDelegate;
    private IntPtr _selIndexSetWithIndex;
    private IntPtr _selIndexSetWithIndexesInRange;
    private IntPtr _selFirstIndex;
    private IntPtr _selIndexGreaterThanIndex;
    private IntPtr _selCount_indexSet;
    private IntPtr _selSetAllowsMultipleSelection;
    private IntPtr _selSetHeaderView;
    private IntPtr _selInitWithIdentifier;
    private IntPtr _selRowAtPoint;
    private IntPtr _selVisibleRect;

    // NSTableColumn
    private IntPtr _selSetWidth;
    private IntPtr _selSetMinWidth;
    private IntPtr _selSetMaxWidth;

    // NSScrollView for table
    private IntPtr _selSetHasVerticalScroller;
    private IntPtr _selSetDocumentView;
    private IntPtr _selSetAutohidesScrollers;

    // Store list items per list control
    private readonly Dictionary<IntPtr, List<string>> _listItems = new();

    private void InitializeListSelectors()
    {
        if (_nsTableViewClass == IntPtr.Zero)
        {
            _nsTableViewClass = objc_getClass("NSTableView");
            _nsTableColumnClass = objc_getClass("NSTableColumn");
            _nsIndexSetClass = objc_getClass("NSIndexSet");

            if (_nsScrollViewClass == IntPtr.Zero)
            {
                _nsScrollViewClass = objc_getClass("NSScrollView");
            }

            _selReloadData = sel_registerName("reloadData");
            _selNumberOfRows = sel_registerName("numberOfRows");
            _selSelectRowIndexes = sel_registerName("selectRowIndexes:byExtendingSelection:");
            _selSelectedRowIndexes = sel_registerName("selectedRowIndexes");
            _selScrollRowToVisible = sel_registerName("scrollRowToVisible:");
            _selAddTableColumn = sel_registerName("addTableColumn:");
            _selSetDataSource = sel_registerName("setDataSource:");
            _selSetDelegate = sel_registerName("setDelegate:");
            _selIndexSetWithIndex = sel_registerName("indexSetWithIndex:");
            _selIndexSetWithIndexesInRange = sel_registerName("indexSetWithIndexesInRange:");
            _selFirstIndex = sel_registerName("firstIndex");
            _selIndexGreaterThanIndex = sel_registerName("indexGreaterThanIndex:");
            _selCount_indexSet = sel_registerName("count");
            _selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
            _selSetHeaderView = sel_registerName("setHeaderView:");
            _selInitWithIdentifier = sel_registerName("initWithIdentifier:");
            _selRowAtPoint = sel_registerName("rowAtPoint:");
            _selVisibleRect = sel_registerName("visibleRect");
            _selSetWidth = sel_registerName("setWidth:");
            _selSetMinWidth = sel_registerName("setMinWidth:");
            _selSetMaxWidth = sel_registerName("setMaxWidth:");
            _selSetHasVerticalScroller = sel_registerName("setHasVerticalScroller:");
            _selSetDocumentView = sel_registerName("setDocumentView:");
            _selSetAutohidesScrollers = sel_registerName("setAutohidesScrollers:");
        }
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_ulong(IntPtr receiver, IntPtr selector, nuint arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nuint objc_msgSend_ret_ulong(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern nuint objc_msgSend_ret_ulong_arg(IntPtr receiver, IntPtr selector, nuint arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double_arg(IntPtr receiver, IntPtr selector, double arg1);

    [StructLayout(LayoutKind.Sequential)]
    private struct NSPoint
    {
        public double x;
        public double y;
    }

    public IntPtr CreateList(IntPtr parentHandle, int style)
    {
        InitializeListSelectors();

        // Create NSScrollView to contain the table
        IntPtr scrollView = objc_msgSend(_nsScrollViewClass, _selAlloc);
        scrollView = objc_msgSend(scrollView, _selInit);

        // Create NSTableView
        IntPtr tableView = objc_msgSend(_nsTableViewClass, _selAlloc);
        tableView = objc_msgSend(tableView, _selInit);

        // Create a single column for the list
        IntPtr column = objc_msgSend(_nsTableColumnClass, _selAlloc);
        IntPtr columnIdentifier = CreateNSString("ListColumn");
        column = objc_msgSend(column, _selInitWithIdentifier, columnIdentifier);

        // Set column width
        objc_msgSend_double_arg(column, _selSetWidth, 200.0);
        objc_msgSend_double_arg(column, _selSetMinWidth, 50.0);
        objc_msgSend_double_arg(column, _selSetMaxWidth, 10000.0);

        // Add column to table
        objc_msgSend(tableView, _selAddTableColumn, column);

        // Hide header
        objc_msgSend(tableView, _selSetHeaderView, IntPtr.Zero);

        // Set selection mode
        bool multiSelect = (style & SWT.MULTI) != 0;
        objc_msgSend_void(tableView, _selSetAllowsMultipleSelection, multiSelect);

        // Configure scroll view
        objc_msgSend_void(scrollView, _selSetHasVerticalScroller, true);
        objc_msgSend_void(scrollView, _selSetAutohidesScrollers, true);
        objc_msgSend(scrollView, _selSetDocumentView, tableView);

        // Set default frame
        var frame = new CGRect(0, 0, 200, 150);
        objc_msgSend_rect(scrollView, _selSetFrame, frame);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parentHandle, selContentView);
            objc_msgSend(contentView, _selAddSubview, scrollView);
        }

        // Initialize empty items list for this control
        _listItems[scrollView] = new List<string>();

        // Note: In a full implementation, we would set up a data source delegate
        // For now, we're using a simplified approach with the items dictionary

        return scrollView;
    }

    public void AddListItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero || item == null)
            return;

        InitializeListSelectors();

        if (!_listItems.TryGetValue(handle, out var items))
        {
            items = new List<string>();
            _listItems[handle] = items;
        }

        // Add item to our list
        if (index < 0 || index >= items.Count)
        {
            items.Add(item);
        }
        else
        {
            items.Insert(index, item);
        }

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView != IntPtr.Zero)
        {
            // Reload table data
            objc_msgSend_void(tableView, _selReloadData);
        }
    }

    public void RemoveListItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeListSelectors();

        if (_listItems.TryGetValue(handle, out var items))
        {
            if (index >= 0 && index < items.Count)
            {
                items.RemoveAt(index);

                // Get the table view from scroll view
                IntPtr tableView = objc_msgSend(handle, _selDocumentView);
                if (tableView != IntPtr.Zero)
                {
                    // Reload table data
                    objc_msgSend_void(tableView, _selReloadData);
                }
            }
        }
    }

    public void ClearListItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeListSelectors();

        if (_listItems.TryGetValue(handle, out var items))
        {
            items.Clear();

            // Get the table view from scroll view
            IntPtr tableView = objc_msgSend(handle, _selDocumentView);
            if (tableView != IntPtr.Zero)
            {
                // Reload table data
                objc_msgSend_void(tableView, _selReloadData);
            }
        }
    }

    public void SetListSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero || indices == null)
            return;

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return;

        if (indices.Length == 0)
        {
            // Deselect all
            IntPtr emptyIndexSet = objc_msgSend(_nsIndexSetClass, _selAlloc);
            emptyIndexSet = objc_msgSend(emptyIndexSet, _selInit);
            objc_msgSend(tableView, _selSelectRowIndexes, emptyIndexSet, IntPtr.Zero);
        }
        else if (indices.Length == 1)
        {
            // Single selection
            IntPtr indexSet = objc_msgSend_ulong(_nsIndexSetClass, _selIndexSetWithIndex, (nuint)indices[0]);
            objc_msgSend(tableView, _selSelectRowIndexes, indexSet, IntPtr.Zero);
        }
        else
        {
            // Multiple selection - need to build index set
            IntPtr mutableIndexSet = objc_getClass("NSMutableIndexSet");
            IntPtr indexSet = objc_msgSend(mutableIndexSet, _selAlloc);
            indexSet = objc_msgSend(indexSet, _selInit);

            IntPtr selAddIndex = sel_registerName("addIndex:");
            foreach (int index in indices)
            {
                objc_msgSend_ulong(indexSet, selAddIndex, (nuint)index);
            }

            objc_msgSend(tableView, _selSelectRowIndexes, indexSet, IntPtr.Zero);
        }
    }

    public int[] GetListSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return Array.Empty<int>();

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return Array.Empty<int>();

        // Get selected row indexes
        IntPtr indexSet = objc_msgSend(tableView, _selSelectedRowIndexes);
        if (indexSet == IntPtr.Zero)
            return Array.Empty<int>();

        // Get count of selected items
        nuint count = objc_msgSend_ret_ulong(indexSet, _selCount_indexSet);
        if (count == 0)
            return Array.Empty<int>();

        // Extract indices
        var indices = new List<int>();
        nuint currentIndex = objc_msgSend_ret_ulong(indexSet, _selFirstIndex);

        const ulong NSNotFound = ulong.MaxValue;
        while (currentIndex != (nuint)NSNotFound && indices.Count < (int)count)
        {
            indices.Add((int)currentIndex);
            currentIndex = objc_msgSend_ret_ulong_arg(indexSet, _selIndexGreaterThanIndex, currentIndex);
        }

        return indices.ToArray();
    }

    public int GetListTopIndex(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return 0;

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return 0;

        // Get visible rect
        CGRect visibleRect;
        objc_msgSend_stret(out visibleRect, tableView, _selVisibleRect);

        // Get row at top of visible rect
        NSPoint topPoint = new NSPoint { x = visibleRect.x, y = visibleRect.y };

        IntPtr selRowAtPoint = sel_registerName("rowAtPoint:");
        long row;
        unsafe
        {
            row = (long)objc_msgSend(tableView, selRowAtPoint, (IntPtr)(&topPoint));
        }

        return row >= 0 ? (int)row : 0;
    }

    public void SetListTopIndex(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || index < 0)
            return;

        InitializeListSelectors();

        // Get the table view from scroll view
        IntPtr tableView = objc_msgSend(handle, _selDocumentView);
        if (tableView == IntPtr.Zero)
            return;

        // Scroll to make row visible at top
        objc_msgSend_ulong(tableView, _selScrollRowToVisible, (nuint)index);
    }

    // Combo operations - implemented in MacOSPlatform_Combo.cs

    // Group operations
    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        throw new NotImplementedException("Group not yet implemented on macOS platform");
    }

    public void SetGroupText(IntPtr handle, string text)
    {
        throw new NotImplementedException("Group not yet implemented on macOS platform");
    }

    // Canvas operations

    // Store canvas metadata (paint callbacks and background colors)
    private class CanvasData
    {
        public Action<int, int, int, int, object?>? PaintCallback { get; set; }
        public Graphics.RGB? BackgroundColor { get; set; }
    }

    private readonly Dictionary<IntPtr, CanvasData> _canvasData = new();
    private IntPtr _nsViewClass;
    private IntPtr _selSetNeedsDisplay;
    private IntPtr _selSetNeedsDisplayInRect;
    private IntPtr _selSetBackgroundColor;

    private void InitializeCanvasSelectors()
    {
        if (_nsViewClass == IntPtr.Zero)
        {
            _nsViewClass = objc_getClass("NSView");
            _selSetNeedsDisplay = sel_registerName("setNeedsDisplay:");
            _selSetNeedsDisplayInRect = sel_registerName("setNeedsDisplayInRect:");
            _selSetBackgroundColor = sel_registerName("setBackgroundColor:");
        }
    }

    public IntPtr CreateCanvas(IntPtr parent, int style)
    {
        InitializeCanvasSelectors();

        // Create NSView as the custom drawing surface
        IntPtr view = objc_msgSend(objc_msgSend(_nsViewClass, _selAlloc), _selInit);

        // Set default frame
        CGRect frame = new CGRect(0, 0, 100, 100);
        IntPtr selSetFrame = sel_registerName("setFrame:");
        objc_msgSend_rect(view, selSetFrame, frame);

        // Initialize canvas data for this view
        _canvasData[view] = new CanvasData();

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            if (_selAddSubview == IntPtr.Zero)
            {
                _selAddSubview = sel_registerName("addSubview:");
            }
            objc_msgSend(parent, _selAddSubview, view);
        }

        return view;
    }

    public void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    {
        if (handle == IntPtr.Zero)
            return;

        if (_canvasData.TryGetValue(handle, out var data))
        {
            data.PaintCallback = paintCallback;
        }
        else
        {
            _canvasData[handle] = new CanvasData { PaintCallback = paintCallback };
        }

        // Note: In a full implementation, we would need to set up a drawRect: delegate
        // or subclass NSView to handle paint events. For now, we store the callback
        // and it will be invoked when RedrawCanvas is called.
    }

    public void RedrawCanvas(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeCanvasSelectors();

        // Call setNeedsDisplay:YES to trigger redraw of entire view
        objc_msgSend_void(handle, _selSetNeedsDisplay, true);

        // For immediate testing, invoke paint callback if set
        if (_canvasData.TryGetValue(handle, out var data) && data.PaintCallback != null)
        {
            // Get view bounds
            objc_msgSend_stret(out CGRect bounds, handle, _selFrame);
            data.PaintCallback((int)bounds.x, (int)bounds.y, (int)bounds.width, (int)bounds.height, handle);
        }
    }

    public void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeCanvasSelectors();

        // Create CGRect for the area to redraw
        CGRect rect = new CGRect(x, y, width, height);

        // Call setNeedsDisplayInRect: to trigger redraw of specific area
        objc_msgSend_rect(handle, _selSetNeedsDisplayInRect, rect);

        // For immediate testing, invoke paint callback if set
        if (_canvasData.TryGetValue(handle, out var data) && data.PaintCallback != null)
        {
            data.PaintCallback(x, y, width, height, handle);
        }
    }

    // Helper method to set canvas background color (used internally)
    private void SetCanvasBackgroundColor(IntPtr handle, Graphics.RGB color)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeCanvasSelectors();

        // Store color in canvas data
        if (_canvasData.TryGetValue(handle, out var data))
        {
            data.BackgroundColor = color;
        }
        else
        {
            _canvasData[handle] = new CanvasData { BackgroundColor = color };
        }

        // Create NSColor from RGB values (normalized to 0.0-1.0)
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr selColorWithRGB = sel_registerName("colorWithRed:green:blue:alpha:");

        // Note: objc_msgSend with float arguments requires special handling
        // For now, we store the color and would apply it in drawRect: implementation
        // A complete implementation would need to properly bridge float arguments
    }

    // Cleanup method for canvas data
    public void ClearCanvasData()
    {
        _canvasData.Clear();
    }

    // Remove specific canvas data when control is destroyed
    public void RemoveCanvasData(IntPtr handle)
    {
        _canvasData.Remove(handle);
    }

    // TabFolder operations
    public IntPtr CreateTabFolder(IntPtr parent, int style)
    {
        throw new NotImplementedException("TabFolder not yet implemented on macOS platform");
    }

    public void SetTabSelection(IntPtr handle, int index)
    {
        throw new NotImplementedException("TabFolder not yet implemented on macOS platform");
    }

    public int GetTabSelection(IntPtr handle)
    {
        throw new NotImplementedException("TabFolder not yet implemented on macOS platform");
    }

    // TabItem operations
    public IntPtr CreateTabItem(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TabItem not yet implemented on macOS platform");
    }

    public void SetTabItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TabItem not yet implemented on macOS platform");
    }

    public void SetTabItemControl(IntPtr handle, IntPtr control)
    {
        throw new NotImplementedException("TabItem not yet implemented on macOS platform");
    }

    public void SetTabItemToolTip(IntPtr handle, string toolTip)
    {
        throw new NotImplementedException("TabItem not yet implemented on macOS platform");
    }

    // ToolBar operations
    public IntPtr CreateToolBar(int style)
    {
        throw new NotImplementedException("ToolBar not yet implemented on macOS platform");
    }

    // ToolItem operations
    public IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void DestroyToolItem(IntPtr handle)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemImage(IntPtr handle, IntPtr image)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemToolTip(IntPtr handle, string toolTip)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemSelection(IntPtr handle, bool selected)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemEnabled(IntPtr handle, bool enabled)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemWidth(IntPtr handle, int width)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    public void SetToolItemControl(IntPtr handle, IntPtr control)
    {
        throw new NotImplementedException("ToolItem not yet implemented on macOS platform");
    }

    // Tree operations
    public IntPtr CreateTree(IntPtr parent, int style)
    {
        throw new NotImplementedException("Tree not yet implemented on macOS platform");
    }

    public IntPtr[] GetTreeSelection(IntPtr handle)
    {
        throw new NotImplementedException("Tree not yet implemented on macOS platform");
    }

    public void SetTreeSelection(IntPtr handle, IntPtr[] items)
    {
        throw new NotImplementedException("Tree not yet implemented on macOS platform");
    }

    public void ClearTreeItems(IntPtr handle)
    {
        throw new NotImplementedException("Tree not yet implemented on macOS platform");
    }

    public void ShowTreeItem(IntPtr handle, IntPtr item)
    {
        throw new NotImplementedException("Tree not yet implemented on macOS platform");
    }

    // TreeItem operations
    public IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void DestroyTreeItem(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void SetTreeItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void SetTreeItemImage(IntPtr handle, IntPtr image)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void SetTreeItemChecked(IntPtr handle, bool checked_)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public bool GetTreeItemChecked(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void SetTreeItemExpanded(IntPtr handle, bool expanded)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public bool GetTreeItemExpanded(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void ClearTreeItemChildren(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    public void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index)
    {
        throw new NotImplementedException("TreeItem not yet implemented on macOS platform");
    }

    // Table operations - using NSTableView for multi-column grid display
    private readonly Dictionary<IntPtr, TableData> _tableData = new();
    private IntPtr _selSetGridStyleMask;
    private IntPtr _selRemoveTableColumn;
    private IntPtr _selTableColumns;
    private IntPtr _selObjectAtIndex;

    // NSTableView grid style constants
    private const int NSTableViewGridNone = 0;
    private const int NSTableViewSolidVerticalGridLineMask = 1 << 0;
    private const int NSTableViewSolidHorizontalGridLineMask = 1 << 1;

    private class TableData
    {
        public IntPtr TableView { get; set; }
        public List<IntPtr> Columns { get; set; } = new();
        public List<TableRow> Rows { get; set; } = new();
        public bool HeaderVisible { get; set; } = true;
        public bool LinesVisible { get; set; } = false;
    }

    private class TableRow
    {
        public Dictionary<int, string> CellText { get; set; } = new();
        public Dictionary<int, IntPtr> CellImages { get; set; } = new();
        public bool Checked { get; set; } = false;
    }

    private void InitializeTableSelectors()
    {
        InitializeListSelectors(); // Reuse NSTableView selectors

        if (_selSetGridStyleMask == IntPtr.Zero)
        {
            _selSetGridStyleMask = sel_registerName("setGridStyleMask:");
            _selRemoveTableColumn = sel_registerName("removeTableColumn:");
            _selTableColumns = sel_registerName("tableColumns");
            _selObjectAtIndex = sel_registerName("objectAtIndex:");
        }
    }

    public IntPtr CreateTable(IntPtr parent, int style)
    {
        InitializeTableSelectors();

        // Create NSScrollView to contain the table
        IntPtr scrollView = objc_msgSend(_nsScrollViewClass, _selAlloc);
        scrollView = objc_msgSend(scrollView, _selInit);

        // Create NSTableView
        IntPtr tableView = objc_msgSend(_nsTableViewClass, _selAlloc);
        tableView = objc_msgSend(tableView, _selInit);

        // Set selection mode
        bool multiSelect = (style & SWT.MULTI) != 0;
        objc_msgSend_void(tableView, _selSetAllowsMultipleSelection, multiSelect);

        // Configure grid lines (default off)
        objc_msgSend(tableView, _selSetGridStyleMask, new IntPtr(NSTableViewGridNone));

        // Header visible by default
        // Note: Header visibility is controlled via setHeaderView: (set to nil to hide)

        // Configure scroll view
        objc_msgSend_void(scrollView, _selSetHasVerticalScroller, true);
        objc_msgSend_void(scrollView, _selSetAutohidesScrollers, true);
        objc_msgSend(scrollView, _selSetDocumentView, tableView);

        // Set default frame
        var frame = new CGRect(0, 0, 400, 300);
        objc_msgSend_rect(scrollView, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, scrollView);
        }

        // Initialize table data
        _tableData[scrollView] = new TableData
        {
            TableView = tableView,
            HeaderVisible = true,
            LinesVisible = false
        };

        return scrollView;
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();
        data.HeaderVisible = visible;

        if (visible)
        {
            // Create default header view if needed
            IntPtr selTableHeaderView = sel_registerName("tableHeaderView");
            IntPtr currentHeader = objc_msgSend(data.TableView, selTableHeaderView);

            if (currentHeader == IntPtr.Zero)
            {
                // Create new header view
                IntPtr nsTableHeaderViewClass = objc_getClass("NSTableHeaderView");
                IntPtr headerView = objc_msgSend(nsTableHeaderViewClass, _selAlloc);
                headerView = objc_msgSend(headerView, _selInit);
                objc_msgSend(data.TableView, _selSetHeaderView, headerView);
            }
        }
        else
        {
            // Hide header by setting to nil
            objc_msgSend(data.TableView, _selSetHeaderView, IntPtr.Zero);
        }
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();
        data.LinesVisible = visible;

        int gridStyle = visible
            ? (NSTableViewSolidVerticalGridLineMask | NSTableViewSolidHorizontalGridLineMask)
            : NSTableViewGridNone;

        objc_msgSend(data.TableView, _selSetGridStyleMask, new IntPtr(gridStyle));
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();

        if (indices == null || indices.Length == 0)
        {
            // Clear selection
            IntPtr emptyIndexSet = objc_msgSend(_nsIndexSetClass, _selAlloc);
            emptyIndexSet = objc_msgSend(emptyIndexSet, _selInit);
            objc_msgSend(data.TableView, _selSelectRowIndexes, emptyIndexSet, new IntPtr(0));
            return;
        }

        // Create mutable index set
        IntPtr nsMutableIndexSetClass = objc_getClass("NSMutableIndexSet");
        IntPtr indexSet = objc_msgSend(nsMutableIndexSetClass, _selAlloc);
        indexSet = objc_msgSend(indexSet, _selInit);

        IntPtr selAddIndex = sel_registerName("addIndex:");
        foreach (int index in indices)
        {
            if (index >= 0 && index < data.Rows.Count)
            {
                objc_msgSend_ulong(indexSet, selAddIndex, (nuint)index);
            }
        }

        objc_msgSend(data.TableView, _selSelectRowIndexes, indexSet, new IntPtr(0));
    }

    public void ClearTableItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();
        data.Rows.Clear();
        objc_msgSend_void(data.TableView, _selReloadData);
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || !_tableData.TryGetValue(handle, out var data))
            return;

        InitializeTableSelectors();

        if (index >= 0 && index < data.Rows.Count)
        {
            objc_msgSend_ulong(data.TableView, _selScrollRowToVisible, (nuint)index);
        }
    }

    // TableColumn operations
    private readonly Dictionary<IntPtr, ColumnData> _columnData = new();
    private IntPtr _selSetResizingMask;
    private IntPtr _selHeaderCell;
    private IntPtr _selSetToolTip;
    private IntPtr _selSizeToFit;

    // NSTableColumn resizing masks
    private const int NSTableColumnNoResizing = 0;
    private const int NSTableColumnAutoresizingMask = 1 << 0;
    private const int NSTableColumnUserResizingMask = 1 << 1;

    private class ColumnData
    {
        public IntPtr TableScrollView { get; set; }
        public int ColumnIndex { get; set; }
        public int Alignment { get; set; } = SWT.LEFT;
    }

    private void InitializeColumnSelectors()
    {
        InitializeTableSelectors();
        InitializeTextSelectors(); // Reuse _selSetStringValue from Text control

        if (_selSetResizingMask == IntPtr.Zero)
        {
            _selSetResizingMask = sel_registerName("setResizingMask:");
            _selHeaderCell = sel_registerName("headerCell");
            _selSetToolTip = sel_registerName("setToolTip:");
            _selSizeToFit = sel_registerName("sizeToFit");
        }
    }

    public IntPtr CreateTableColumn(IntPtr tableHandle, int style, int index)
    {
        if (tableHandle == IntPtr.Zero || !_tableData.TryGetValue(tableHandle, out var data))
            return IntPtr.Zero;

        InitializeColumnSelectors();

        // Create NSTableColumn
        IntPtr column = objc_msgSend(_nsTableColumnClass, _selAlloc);
        IntPtr columnIdentifier = CreateNSString($"Column{data.Columns.Count}");
        column = objc_msgSend(column, _selInitWithIdentifier, columnIdentifier);

        // Set default width
        objc_msgSend_double_arg(column, _selSetWidth, 100.0);
        objc_msgSend_double_arg(column, _selSetMinWidth, 20.0);
        objc_msgSend_double_arg(column, _selSetMaxWidth, 10000.0);

        // Set resizable by default
        objc_msgSend(column, _selSetResizingMask,
            new IntPtr(NSTableColumnAutoresizingMask | NSTableColumnUserResizingMask));

        // Add column to table at specified index
        if (index < 0 || index >= data.Columns.Count)
        {
            objc_msgSend(data.TableView, _selAddTableColumn, column);
            data.Columns.Add(column);
        }
        else
        {
            // NSTableView doesn't have insertTableColumn, so we add and then move
            objc_msgSend(data.TableView, _selAddTableColumn, column);
            data.Columns.Insert(index, column);

            // Move column to correct position
            IntPtr selMoveColumn = sel_registerName("moveColumn:toColumn:");
            objc_msgSend(data.TableView, selMoveColumn,
                new IntPtr(data.Columns.Count - 1), new IntPtr(index));
        }

        // Store column data
        _columnData[column] = new ColumnData
        {
            TableScrollView = tableHandle,
            ColumnIndex = index >= 0 ? index : data.Columns.Count - 1,
            Alignment = style & (SWT.LEFT | SWT.RIGHT | SWT.CENTER)
        };

        return column;
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_columnData.TryGetValue(handle, out var colData))
            return;

        if (!_tableData.TryGetValue(colData.TableScrollView, out var data))
            return;

        InitializeColumnSelectors();

        // Remove from table view
        objc_msgSend(data.TableView, _selRemoveTableColumn, handle);

        // Remove from our tracking
        data.Columns.Remove(handle);
        _columnData.Remove(handle);

        // Update column indices for remaining columns
        for (int i = 0; i < data.Columns.Count; i++)
        {
            if (_columnData.TryGetValue(data.Columns[i], out var remainingCol))
            {
                remainingCol.ColumnIndex = i;
            }
        }
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();

        // Get header cell and set its string value
        IntPtr headerCell = objc_msgSend(handle, _selHeaderCell);
        if (headerCell != IntPtr.Zero)
        {
            IntPtr nsText = CreateNSString(text ?? string.Empty);
            objc_msgSend(headerCell, _selSetStringValue, nsText);
        }
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();
        objc_msgSend_double_arg(handle, _selSetWidth, Math.Max(0, width));
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        if (handle == IntPtr.Zero || !_columnData.TryGetValue(handle, out var colData))
            return;

        colData.Alignment = alignment;
        // Note: Column alignment in NSTableView is typically set on the cell,
        // not the column itself. This would require custom cell setup.
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();

        int mask = resizable
            ? (NSTableColumnAutoresizingMask | NSTableColumnUserResizingMask)
            : NSTableColumnNoResizing;

        objc_msgSend(handle, _selSetResizingMask, new IntPtr(mask));
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        if (handle == IntPtr.Zero || !_columnData.TryGetValue(handle, out var colData))
            return;

        if (!_tableData.TryGetValue(colData.TableScrollView, out var data))
            return;

        InitializeColumnSelectors();

        // Set column reordering on the table view
        IntPtr selSetAllowsColumnReordering = sel_registerName("setAllowsColumnReordering:");
        objc_msgSend_void(data.TableView, selSetAllowsColumnReordering, moveable);
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTipText)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeColumnSelectors();

        IntPtr headerCell = objc_msgSend(handle, _selHeaderCell);
        if (headerCell != IntPtr.Zero)
        {
            if (string.IsNullOrEmpty(toolTipText))
            {
                objc_msgSend(headerCell, _selSetToolTip, IntPtr.Zero);
            }
            else
            {
                IntPtr nsToolTip = CreateNSString(toolTipText);
                objc_msgSend(headerCell, _selSetToolTip, nsToolTip);
            }
        }
    }

    public int PackTableColumn(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return 0;

        InitializeColumnSelectors();

        // Auto-size the column to fit content
        objc_msgSend_void(handle, _selSizeToFit);

        // Get the new width
        IntPtr selWidth = sel_registerName("width");
        double width = objc_msgSend_ret_double(handle, selWidth);

        return (int)width;
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_fpret")]
    private static extern double objc_msgSend_ret_double(IntPtr receiver, IntPtr selector);

    // TableItem operations
    // In NSTableView, items are not objects but row indices
    // We track row data in our TableData structure
    private readonly Dictionary<IntPtr, ItemData> _itemData = new();
    private int _nextItemId = 1;

    private class ItemData
    {
        public IntPtr TableScrollView { get; set; }
        public int RowIndex { get; set; }
        public int ItemId { get; set; }
    }

    public IntPtr CreateTableItem(IntPtr tableHandle, int style, int index)
    {
        if (tableHandle == IntPtr.Zero || !_tableData.TryGetValue(tableHandle, out var data))
            return IntPtr.Zero;

        InitializeTableSelectors();

        // Create a new row
        var row = new TableRow();

        // Add row at specified index
        if (index < 0 || index >= data.Rows.Count)
        {
            data.Rows.Add(row);
            index = data.Rows.Count - 1;
        }
        else
        {
            data.Rows.Insert(index, row);
        }

        // Create a pseudo-handle for the item (using a unique ID)
        int itemId = _nextItemId++;
        IntPtr itemHandle = new IntPtr(0x10000000 + itemId); // Use high bit pattern to distinguish from real pointers

        // Store item data
        _itemData[itemHandle] = new ItemData
        {
            TableScrollView = tableHandle,
            RowIndex = index,
            ItemId = itemId
        };

        // Reload table to show new row
        objc_msgSend_void(data.TableView, _selReloadData);

        return itemHandle;
    }

    public void DestroyTableItem(IntPtr handle)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        InitializeTableSelectors();

        // Remove row from data
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            data.Rows.RemoveAt(itemData.RowIndex);

            // Update row indices for all items after this one
            foreach (var kvp in _itemData.ToList())
            {
                if (kvp.Value.TableScrollView == itemData.TableScrollView &&
                    kvp.Value.RowIndex > itemData.RowIndex)
                {
                    kvp.Value.RowIndex--;
                }
            }

            // Reload table
            objc_msgSend_void(data.TableView, _selReloadData);
        }

        // Remove from tracking
        _itemData.Remove(handle);
    }

    public void SetTableItemText(IntPtr handle, int columnIndex, string text)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        // Update cell text in our data structure
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            var row = data.Rows[itemData.RowIndex];
            row.CellText[columnIndex] = text ?? string.Empty;

            // Reload the affected row
            InitializeTableSelectors();
            IntPtr selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
            IntPtr indexSet = objc_msgSend(_nsIndexSetClass, _selIndexSetWithIndex, new IntPtr(itemData.RowIndex));
            IntPtr allColumnsIndexSet = CreateIndexSetForAllColumns(data.Columns.Count);
            objc_msgSend(data.TableView, selReloadDataForRowIndexes, indexSet, allColumnsIndexSet);
        }
    }

    public void SetTableItemImage(IntPtr handle, int columnIndex, IntPtr imageHandle)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        // Update cell image in our data structure
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            var row = data.Rows[itemData.RowIndex];
            if (imageHandle == IntPtr.Zero)
            {
                row.CellImages.Remove(columnIndex);
            }
            else
            {
                row.CellImages[columnIndex] = imageHandle;
            }

            // Reload the affected row
            InitializeTableSelectors();
            IntPtr selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
            IntPtr indexSet = objc_msgSend(_nsIndexSetClass, _selIndexSetWithIndex, new IntPtr(itemData.RowIndex));
            IntPtr allColumnsIndexSet = CreateIndexSetForAllColumns(data.Columns.Count);
            objc_msgSend(data.TableView, selReloadDataForRowIndexes, indexSet, allColumnsIndexSet);
        }
    }

    public void SetTableItemChecked(IntPtr handle, bool isChecked)
    {
        if (handle == IntPtr.Zero || !_itemData.TryGetValue(handle, out var itemData))
            return;

        if (!_tableData.TryGetValue(itemData.TableScrollView, out var data))
            return;

        // Update checked state in our data structure
        if (itemData.RowIndex >= 0 && itemData.RowIndex < data.Rows.Count)
        {
            var row = data.Rows[itemData.RowIndex];
            row.Checked = isChecked;

            // Reload the affected row
            InitializeTableSelectors();
            IntPtr selReloadDataForRowIndexes = sel_registerName("reloadDataForRowIndexes:columnIndexes:");
            IntPtr indexSet = objc_msgSend(_nsIndexSetClass, _selIndexSetWithIndex, new IntPtr(itemData.RowIndex));
            IntPtr allColumnsIndexSet = CreateIndexSetForAllColumns(data.Columns.Count);
            objc_msgSend(data.TableView, selReloadDataForRowIndexes, indexSet, allColumnsIndexSet);
        }
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        // Note: Row background colors in NSTableView are typically handled via
        // NSTableViewDelegate methods (willDisplayCell:forTableColumn:row:)
        // For a full implementation, we would need to set up a custom delegate
        // For now, this is a placeholder that stores the intent
        if (handle == IntPtr.Zero)
            return;

        // In a complete implementation, we would:
        // 1. Store the color in the row data
        // 2. Set up a table view delegate
        // 3. Implement willDisplayCell to apply the background color
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        // Note: Similar to background, foreground colors are typically handled
        // via NSTableViewDelegate methods
        if (handle == IntPtr.Zero)
            return;

        // In a complete implementation, we would:
        // 1. Store the color in the row data
        // 2. Set up a table view delegate
        // 3. Implement willDisplayCell to apply the foreground color
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        // Note: Fonts are also typically handled via NSTableViewDelegate
        if (handle == IntPtr.Zero)
            return;

        // In a complete implementation, we would:
        // 1. Store the font in the row data
        // 2. Set up a table view delegate
        // 3. Implement willDisplayCell to apply the font
    }

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

    // ProgressBar operations

    private IntPtr _nsProgressIndicatorClass;
    private IntPtr _selSetMinValue;
    private IntPtr _selSetMaxValue;
    private IntPtr _selSetDoubleValue;
    private IntPtr _selSetIndeterminate;
    private IntPtr _selStartAnimation;
    private IntPtr _selStopAnimation;
    private IntPtr _selSetStyle;

    // NSProgressIndicator styles
    private const int NSProgressIndicatorBarStyle = 0;
    private const int NSProgressIndicatorSpinningStyle = 1;

    private void InitializeProgressBarSelectors()
    {
        if (_nsProgressIndicatorClass == IntPtr.Zero)
        {
            _nsProgressIndicatorClass = objc_getClass("NSProgressIndicator");
            _selSetMinValue = sel_registerName("setMinValue:");
            _selSetMaxValue = sel_registerName("setMaxValue:");
            _selSetDoubleValue = sel_registerName("setDoubleValue:");
            _selSetIndeterminate = sel_registerName("setIndeterminate:");
            _selStartAnimation = sel_registerName("startAnimation:");
            _selStopAnimation = sel_registerName("stopAnimation:");
            _selSetStyle = sel_registerName("setStyle:");
        }
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_double(IntPtr receiver, IntPtr selector, double arg1);

    public IntPtr CreateProgressBar(IntPtr parent, int style)
    {
        InitializeProgressBarSelectors();

        // Create NSProgressIndicator
        IntPtr progressBar = objc_msgSend(_nsProgressIndicatorClass, _selAlloc);
        progressBar = objc_msgSend(progressBar, _selInit);

        // Set style - bar style for determinate, spinning for indeterminate
        if ((style & SWT.INDETERMINATE) != 0)
        {
            objc_msgSend(progressBar, _selSetStyle, new IntPtr(NSProgressIndicatorSpinningStyle));
            objc_msgSend_void(progressBar, _selSetIndeterminate, true);
            // Start animation for indeterminate
            objc_msgSend(progressBar, _selStartAnimation, IntPtr.Zero);
        }
        else
        {
            objc_msgSend(progressBar, _selSetStyle, new IntPtr(NSProgressIndicatorBarStyle));
            objc_msgSend_void(progressBar, _selSetIndeterminate, false);
        }

        // Set default frame
        var frame = new CGRect(0, 0, 200, 20);
        objc_msgSend_rect(progressBar, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, progressBar);
        }

        return progressBar;
    }

    public void SetProgressBarRange(IntPtr handle, int minimum, int maximum)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeProgressBarSelectors();

        // Set min and max values
        objc_msgSend_double(handle, _selSetMinValue, (double)minimum);
        objc_msgSend_double(handle, _selSetMaxValue, (double)maximum);
    }

    public void SetProgressBarSelection(IntPtr handle, int value)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeProgressBarSelectors();

        // Set current value
        objc_msgSend_double(handle, _selSetDoubleValue, (double)value);
    }

    public void SetProgressBarState(IntPtr handle, int state)
    {
        if (handle == IntPtr.Zero)
            return;

        // macOS NSProgressIndicator doesn't have built-in state support (normal/error/paused)
        // State colors would need to be implemented via custom drawing or color changes
        // For now, this is a no-op on macOS
    }

    // Slider operations
    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        throw new NotImplementedException("Slider not yet implemented on macOS platform");
    }

    public void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Slider not yet implemented on macOS platform");
    }

    public void ConnectSliderChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Slider not yet implemented on macOS platform");
    }

    // Scale operations
    public IntPtr CreateScale(IntPtr parent, int style)
    {
        throw new NotImplementedException("Scale not yet implemented on macOS platform");
    }

    public void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Scale not yet implemented on macOS platform");
    }

    public void ConnectScaleChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Scale not yet implemented on macOS platform");
    }

    // Spinner operations
    public IntPtr CreateSpinner(IntPtr parent, int style)
    {
        throw new NotImplementedException("Spinner not yet implemented on macOS platform");
    }

    public void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Spinner not yet implemented on macOS platform");
    }

    public void SetSpinnerTextLimit(IntPtr handle, int limit)
    {
        throw new NotImplementedException("Spinner not yet implemented on macOS platform");
    }

    public void ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Spinner not yet implemented on macOS platform");
    }

    public void ConnectSpinnerModified(IntPtr handle, Action callback)
    {
        throw new NotImplementedException("Spinner not yet implemented on macOS platform");
    }

    // Dialog operations
    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        throw new NotImplementedException("MessageBox not yet implemented on macOS platform");
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        // Initialize selectors for file dialogs
        IntPtr nsOpenPanelClass = objc_getClass("NSOpenPanel");
        IntPtr nsSavePanelClass = objc_getClass("NSSavePanel");
        IntPtr nsUrlClass = objc_getClass("NSURL");
        IntPtr nsArrayClass = objc_getClass("NSArray");

        IntPtr selOpenPanel = sel_registerName("openPanel");
        IntPtr selSavePanel = sel_registerName("savePanel");
        IntPtr selRunModal = sel_registerName("runModal");
        IntPtr selSetTitle = sel_registerName("setTitle:");
        IntPtr selSetMessage = sel_registerName("setMessage:");
        IntPtr selSetCanChooseFiles = sel_registerName("setCanChooseFiles:");
        IntPtr selSetCanChooseDirectories = sel_registerName("setCanChooseDirectories:");
        IntPtr selSetAllowsMultipleSelection = sel_registerName("setAllowsMultipleSelection:");
        IntPtr selSetCanCreateDirectories = sel_registerName("setCanCreateDirectories:");
        IntPtr selSetDirectoryURL = sel_registerName("setDirectoryURL:");
        IntPtr selSetNameFieldStringValue = sel_registerName("setNameFieldStringValue:");
        IntPtr selSetAllowedFileTypes = sel_registerName("setAllowedFileTypes:");
        IntPtr selURLs = sel_registerName("URLs");
        IntPtr selURL = sel_registerName("URL");
        IntPtr selPath = sel_registerName("path");
        IntPtr selCount = sel_registerName("count");
        IntPtr selObjectAtIndex = sel_registerName("objectAtIndex:");
        IntPtr selFileURLWithPath = sel_registerName("fileURLWithPath:");

        // Modal response codes
        const long NSModalResponseOK = 1;

        bool isSave = (style & SWT.SAVE) != 0;
        bool isMulti = (style & SWT.MULTI) != 0;

        IntPtr panel;
        if (isSave)
        {
            panel = objc_msgSend(nsSavePanelClass, selSavePanel);
        }
        else
        {
            panel = objc_msgSend(nsOpenPanelClass, selOpenPanel);

            // Configure for open panel
            objc_msgSend_void(panel, selSetCanChooseFiles, true);
            objc_msgSend_void(panel, selSetCanChooseDirectories, false);
            objc_msgSend_void(panel, selSetAllowsMultipleSelection, isMulti);
        }

        // Set title
        if (!string.IsNullOrEmpty(title))
        {
            IntPtr nsTitle = CreateNSString(title);
            objc_msgSend(panel, selSetTitle, nsTitle);
        }

        // Set initial directory
        if (!string.IsNullOrEmpty(filterPath) && Directory.Exists(filterPath))
        {
            IntPtr pathString = CreateNSString(filterPath);
            IntPtr directoryURL = objc_msgSend(nsUrlClass, selFileURLWithPath, pathString);
            objc_msgSend(panel, selSetDirectoryURL, directoryURL);
        }

        // Set initial file name
        if (!string.IsNullOrEmpty(fileName))
        {
            IntPtr nsFileName = CreateNSString(fileName);
            objc_msgSend(panel, selSetNameFieldStringValue, nsFileName);
        }

        // Set allowed file types (filters)
        if (filterExtensions != null && filterExtensions.Length > 0)
        {
            IntPtr nsArray = CreateNSArray(ExtractFileExtensions(filterExtensions));
            objc_msgSend(panel, selSetAllowedFileTypes, nsArray);
        }

        // Run modal dialog
        long response = (long)objc_msgSend(panel, selRunModal);

        if (response != NSModalResponseOK)
        {
            return new FileDialogResult
            {
                SelectedFiles = null,
                FilterPath = null,
                FilterIndex = 0
            };
        }

        // Get selected files
        string[] selectedFiles;
        string? resultFilterPath = null;

        if (isSave)
        {
            // Save panel - single file
            IntPtr url = objc_msgSend(panel, selURL);
            IntPtr pathPtr = objc_msgSend(url, selPath);
            string? path = GetNSStringValue(pathPtr);

            if (!string.IsNullOrEmpty(path))
            {
                selectedFiles = new[] { path };
                resultFilterPath = Path.GetDirectoryName(path);
            }
            else
            {
                selectedFiles = Array.Empty<string>();
            }
        }
        else
        {
            // Open panel - possibly multiple files
            IntPtr urls = objc_msgSend(panel, selURLs);
            int count = (int)(long)objc_msgSend(urls, selCount);

            if (count > 0)
            {
                selectedFiles = new string[count];
                for (int i = 0; i < count; i++)
                {
                    IntPtr url = objc_msgSend(urls, selObjectAtIndex, new IntPtr(i));
                    IntPtr pathPtr = objc_msgSend(url, selPath);
                    string? path = GetNSStringValue(pathPtr);
                    selectedFiles[i] = path ?? string.Empty;
                }

                // Get directory from first file
                if (selectedFiles.Length > 0 && !string.IsNullOrEmpty(selectedFiles[0]))
                {
                    resultFilterPath = Path.GetDirectoryName(selectedFiles[0]);
                }
            }
            else
            {
                selectedFiles = Array.Empty<string>();
            }
        }

        return new FileDialogResult
        {
            SelectedFiles = selectedFiles,
            FilterPath = resultFilterPath,
            FilterIndex = 0 // macOS doesn't provide selected filter index
        };
    }

    private string[] ExtractFileExtensions(string[] filterExtensions)
    {
        var extensions = new List<string>();

        foreach (var filter in filterExtensions)
        {
            if (string.IsNullOrEmpty(filter) || filter == "*.*" || filter == "*")
                continue;

            // Parse patterns like "*.txt", "*.jpg;*.png"
            string[] patterns = filter.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pattern in patterns)
            {
                string ext = pattern.Trim();
                if (ext.StartsWith("*."))
                {
                    ext = ext.Substring(2); // Remove "*."
                }
                else if (ext.StartsWith("."))
                {
                    ext = ext.Substring(1); // Remove "."
                }

                if (!string.IsNullOrEmpty(ext))
                {
                    extensions.Add(ext);
                }
            }
        }

        return extensions.ToArray();
    }

    private IntPtr CreateNSArray(string[] items)
    {
        IntPtr nsArrayClass = objc_getClass("NSArray");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInitWithObjects = sel_registerName("initWithObjects:count:");

        // Create array of NSString objects
        IntPtr[] nsStrings = new IntPtr[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            nsStrings[i] = CreateNSString(items[i]);
        }

        // Allocate memory for object array
        IntPtr objectsPtr = Marshal.AllocHGlobal(IntPtr.Size * items.Length);
        try
        {
            Marshal.Copy(nsStrings, 0, objectsPtr, items.Length);

            // Create NSArray
            IntPtr array = objc_msgSend(nsArrayClass, selAlloc);
            return objc_msgSend(array, selInitWithObjects, objectsPtr, new IntPtr(items.Length));
        }
        finally
        {
            Marshal.FreeHGlobal(objectsPtr);
        }
    }

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        throw new NotImplementedException("DirectoryDialog not yet implemented on macOS platform");
    }

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        throw new NotImplementedException("ColorDialog not yet implemented on macOS platform");
    }

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        throw new NotImplementedException("FontDialog not yet implemented on macOS platform");
    }
}
