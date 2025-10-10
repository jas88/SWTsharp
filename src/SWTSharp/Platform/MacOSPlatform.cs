using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation using Cocoa/AppKit via Objective-C runtime.
/// </summary>
internal partial class MacOSPlatform : IPlatform
{
    /// <summary>
    /// Optional custom main thread executor for testing scenarios.
    /// When set, ExecuteOnMainThread will use this instead of GCD.
    /// This allows TestHost to route execution through MainThreadDispatcher.
    /// </summary>
    internal static Action<Action>? CustomMainThreadExecutor { get; set; }

    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
    private const string AppKitFramework = "/System/Library/Frameworks/AppKit.framework/AppKit";
    private const string FoundationFramework = "/System/Library/Frameworks/Foundation.framework/Foundation";
    private const string LibSystem = "libSystem.dylib"; // Let macOS dynamic linker resolve the path

    // Objective-C runtime functions
#if NET7_0_OR_GREATER
    [LibraryImport(ObjCLibrary, EntryPoint = "objc_getClass")]
    private static partial IntPtr objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport(ObjCLibrary, EntryPoint = "sel_registerName")]
    private static partial IntPtr sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_void(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.Bool)] bool arg1);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool objc_msgSend_bool(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial long objc_msgSend_long(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial double objc_msgSend_double(IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_double(IntPtr receiver, IntPtr selector, double arg1);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static partial void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_rect_bool(IntPtr receiver, IntPtr selector, CGRect rect, [MarshalAs(UnmanagedType.Bool)] bool display);

    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static partial void objc_msgSend_size(IntPtr receiver, IntPtr selector, CGSize size);

#else
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
    private static extern void objc_msgSend_rect_bool(IntPtr receiver, IntPtr selector, CGRect rect, bool display);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_size(IntPtr receiver, IntPtr selector, CGSize size);

#endif


    // Objective-C exception handling
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void NSUncaughtExceptionHandlerDelegate(IntPtr exception);

#if NET7_0_OR_GREATER
    [LibraryImport(FoundationFramework)]
    private static partial void NSSetUncaughtExceptionHandler(NSUncaughtExceptionHandlerDelegate handler);
#else
    [DllImport(FoundationFramework)]
    private static extern void NSSetUncaughtExceptionHandler(NSUncaughtExceptionHandlerDelegate handler);
#endif

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
    private IntPtr _selSetFrame;         // NSView setFrame: (single parameter)
    private IntPtr _selSetFrameDisplay;  // NSWindow setFrame:display: (two parameters)
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
    private IntPtr _nsControlClass;
    private IntPtr _selName;
    private IntPtr _selReason;
    private IntPtr _selUTF8String;

    // Window style masks
    private const ulong NSWindowStyleMaskTitled = 1 << 0;
    private const ulong NSWindowStyleMaskClosable = 1 << 1;
    private const ulong NSWindowStyleMaskMiniaturizable = 1 << 2;
    private const ulong NSWindowStyleMaskResizable = 1 << 3;
    private const ulong NSWindowStyleMaskUtilityWindow = 1 << 4;  // 0x10 - not supported, filtered out
    private const ulong NSWindowStyleMaskDocModalWindow = 1 << 6;
    private const ulong NSWindowStyleMaskNonactivatingPanel = 1 << 7;

    // Event masks
    private const ulong NSEventMaskAny = ulong.MaxValue;

    public MacOSPlatform()
    {
        // Ensure ObjC runtime is initialized on UI thread
        // ObjCRuntime now calls NSApplicationLoad() before class lookups
        ObjCRuntime.EnsureInitialized();
        Initialize();
    }

    // Selector validation tracking
    private readonly Dictionary<string, IntPtr> _registeredSelectors = new Dictionary<string, IntPtr>();
    private bool _initialized = false;
    private readonly object _initLock = new object();

    /// <summary>
    /// Register a selector with validation and duplicate detection.
    /// Throws InvalidOperationException if selector registration fails or if duplicate detected.
    /// </summary>
    private IntPtr RegisterSelector(string selectorName)
    {
        if (string.IsNullOrEmpty(selectorName))
            throw new ArgumentException("Selector name cannot be null or empty", nameof(selectorName));

        // Check for duplicate registration
        if (_registeredSelectors.TryGetValue(selectorName, out IntPtr existing))
        {
            throw new InvalidOperationException(
                $"Selector '{selectorName}' has already been registered with pointer {existing:X}. " +
                "This indicates a duplicate selector initialization attempt.");
        }

        // Register the selector
        IntPtr selector = sel_registerName(selectorName);

        // Validate registration succeeded
        if (selector == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                $"Failed to register selector '{selectorName}'. " +
                "The Objective-C runtime returned a null selector pointer.");
        }

        // Track registered selector
        _registeredSelectors[selectorName] = selector;

        return selector;
    }

    public void Initialize()
    {
        // Make Initialize() idempotent - can be called multiple times safely
        if (_initialized)
            return;

        lock (_initLock)
        {
            if (_initialized)
                return;

            // Initialize selectors with validation
            _selAlloc = RegisterSelector("alloc");
        _selInit = RegisterSelector("init");
        _selInitWithContentRect = RegisterSelector("initWithContentRect:styleMask:backing:defer:");
        _selSetTitle = RegisterSelector("setTitle:");
        _selMakeKeyAndOrderFront = RegisterSelector("makeKeyAndOrderFront:");
        _selOrderOut = RegisterSelector("orderOut:");
        _selClose = RegisterSelector("close");
        _selSetFrame = RegisterSelector("setFrame:");           // NSView method
        _selSetFrameDisplay = RegisterSelector("setFrame:display:");  // NSWindow method
        _selFrame = RegisterSelector("frame");
        _selSharedApplication = RegisterSelector("sharedApplication");
        _selRun = RegisterSelector("run");
        _selStop = RegisterSelector("stop:");
        _selNextEventMatchingMask = RegisterSelector("nextEventMatchingMask:untilDate:inMode:dequeue:");
        _selSendEvent = RegisterSelector("sendEvent:");
        _selUpdateWindows = RegisterSelector("updateWindows");

        // Initialize shared widget selectors
        _selSetMinValue = RegisterSelector("setMinValue:");
        _selSetMaxValue = RegisterSelector("setMaxValue:");
        _selSetDoubleValue = RegisterSelector("setDoubleValue:");
        _selIsKindOfClass = RegisterSelector("isKindOfClass:");
        _selContentView = RegisterSelector("contentView");
        _selRespondsToSelector = RegisterSelector("respondsToSelector:");
        _selClass = RegisterSelector("class");
        _selAddSubview = RegisterSelector("addSubview:");

        // Get classes
        _nsApplicationClass = objc_getClass("NSApplication");
        _nsWindowClass = objc_getClass("NSWindow");
        _nsStringClass = objc_getClass("NSString");
        _nsProgressIndicatorClass = objc_getClass("NSProgressIndicator");
        _nsControlClass = objc_getClass("NSControl");

        // Get or create shared application
        _nsApplication = objc_msgSend(_nsApplicationClass, _selSharedApplication);

        // Set up exception handler to catch ObjC exceptions and convert to managed exceptions
        _selName = sel_registerName("name");
        _selReason = sel_registerName("reason");
        _selUTF8String = sel_registerName("UTF8String");
        NSSetUncaughtExceptionHandler(HandleUncaughtException);

        // Initialize common widget selectors upfront to avoid lazy initialization issues
        // This ensures critical selectors are registered early and any missing selectors fail fast
        // Note: Some selectors (ToolBar, Tree, etc.) are still lazily initialized to avoid
        // loading unused Cocoa classes during startup
        InitializeButtonSelectors();
        InitializeMenuSelectors();
        InitializeTextSelectors();

            _initialized = true;
        }
    }

    private static void HandleUncaughtException(IntPtr exception)
    {
        try
        {
            var platform = PlatformFactory.Instance as MacOSPlatform;
            if (platform == null) return;

            // Get exception name
            IntPtr namePtr = objc_msgSend(exception, platform._selName);
            IntPtr nameUTF8 = objc_msgSend(namePtr, platform._selUTF8String);
            string name = Marshal.PtrToStringAnsi(nameUTF8) ?? "Unknown";

            // Get exception reason
            IntPtr reasonPtr = objc_msgSend(exception, platform._selReason);
            IntPtr reasonUTF8 = objc_msgSend(reasonPtr, platform._selUTF8String);
            string reason = Marshal.PtrToStringAnsi(reasonUTF8) ?? "Unknown reason";

            // Log to stderr for test detection
            Console.Error.WriteLine($"Objective-C Exception: {name}");
            Console.Error.WriteLine($"Reason: {reason}");

            // Throw managed exception to fail the test
            throw new InvalidOperationException($"Objective-C Exception: {name} - {reason}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to handle ObjC exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper to add a child view to parent, handling both NSWindow and NSView parents.
    /// NSWindow parents have contentView, NSView parents do not.
    /// </summary>
    private void AddChildToParent(IntPtr parent, IntPtr child)
    {
        if (parent == IntPtr.Zero) return;

        // Get class of parent object
        IntPtr parentClass = objc_msgSend(parent, _selClass);

        // Check if it's NSWindow by comparing class pointers
        IntPtr targetView = parent;
        if (parentClass == _nsWindowClass)
        {
            // Parent is NSWindow, get contentView
            targetView = objc_msgSend(parent, _selContentView);
        }

        // Add child to target view
        objc_msgSend(targetView, _selAddSubview, child);
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
        IntPtr result = IntPtr.Zero;

        // NSWindow MUST be created on the main thread
        ExecuteOnMainThread(() =>
        {
            // Determine window style mask
            ulong styleMask = NSWindowStyleMaskTitled | NSWindowStyleMaskClosable | NSWindowStyleMaskMiniaturizable;

            if ((style & SWT.RESIZE) != 0)
            {
                styleMask |= NSWindowStyleMaskResizable;
            }

            // Filter out unsupported style masks for NSWindow
            // NSWindow does not support NSWindowStyleMaskUtilityWindow (0x10)
            styleMask &= ~NSWindowStyleMaskUtilityWindow;
            styleMask &= ~NSWindowStyleMaskDocModalWindow;
            styleMask &= ~NSWindowStyleMaskNonactivatingPanel;

            // Create window frame
            var frame = new CGRect(100, 100, 800, 600);

            // Allocate and initialize window
            IntPtr window = objc_msgSend(_nsWindowClass, _selAlloc);

            // initWithContentRect:styleMask:backing:defer:
            // This is a complex call - we need to pass multiple arguments
            window = InitWindow(window, frame, styleMask);

            // Set title
            SetWindowText(window, title);

            result = window;
        });

        return result;
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
        if (handle == IntPtr.Zero)
            return;

        // Phase 1 Fix: Detect pseudo-handles that should not be passed to objc_msgSend
        long handleValue = handle.ToInt64();

        if ((handleValue & 0x40000000) != 0)
        {
            // ToolBar pseudo-handle - should use DestroyToolBar instead
            throw new InvalidOperationException("DestroyWindow called on ToolBar pseudo-handle. Use DestroyToolBar instead.");
        }

        if ((handleValue & 0x30000000) != 0)
        {
            // TabItem pseudo-handle - these are cleaned up by their parent TabFolder
            return;
        }

        if ((handleValue & 0x20000000) != 0)
        {
            // Future pseudo-handle type - add handling as needed
            return;
        }

        // Real native handle - check if this is an NSWindow or an NSView
        // NSWindow has the close method, NSView does not
        IntPtr selRespondsToSelector = sel_registerName("respondsToSelector:");
        bool respondsToClose = objc_msgSend_bool(handle, selRespondsToSelector, _selClose);

        if (respondsToClose)
        {
            // It's an NSWindow, use close
            objc_msgSend_void(handle, _selClose);
        }
        else
        {
            // It's an NSView, remove from superview
            IntPtr selRemoveFromSuperview = sel_registerName("removeFromSuperview");
            objc_msgSend_void(handle, selRemoveFromSuperview);
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
#if NET8_0_OR_GREATER
        // Use stack allocation for small strings to avoid heap allocation
        IntPtr sel = sel_registerName("stringWithUTF8String:");
        int maxByteCount = System.Text.Encoding.UTF8.GetMaxByteCount(str.Length) + 1; // +1 for null terminator

        if (maxByteCount <= 256)
        {
            // Stack allocate for small strings
            unsafe
            {
                Span<byte> utf8Bytes = stackalloc byte[maxByteCount];
                int bytesWritten = System.Text.Encoding.UTF8.GetBytes(str, utf8Bytes);
                utf8Bytes[bytesWritten] = 0; // null terminator

                fixed (byte* ptr = utf8Bytes)
                {
                    return objc_msgSend(_nsStringClass, sel, (IntPtr)ptr);
                }
            }
        }
        else
        {
            // Heap allocate for large strings - must use UTF-8, not ANSI
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(str);
            IntPtr utf8 = Marshal.AllocHGlobal(utf8Bytes.Length + 1);
            try
            {
                Marshal.Copy(utf8Bytes, 0, utf8, utf8Bytes.Length);
                Marshal.WriteByte(utf8, utf8Bytes.Length, 0); // null terminator
                return objc_msgSend(_nsStringClass, sel, utf8);
            }
            finally
            {
                Marshal.FreeHGlobal(utf8);
            }
        }
#else
        // .NET Standard 2.0 / older frameworks - must use proper UTF-8 encoding
        IntPtr sel = sel_registerName("stringWithUTF8String:");
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(str);
        IntPtr utf8 = Marshal.AllocHGlobal(utf8Bytes.Length + 1);
        try
        {
            Marshal.Copy(utf8Bytes, 0, utf8, utf8Bytes.Length);
            Marshal.WriteByte(utf8, utf8Bytes.Length, 0); // null terminator
            return objc_msgSend(_nsStringClass, sel, utf8);
        }
        finally
        {
            Marshal.FreeHGlobal(utf8);
        }
#endif
    }

    public void SetWindowSize(IntPtr handle, int width, int height)
    {
        // Get current frame
        objc_msgSend_stret(out CGRect frame, handle, _selFrame);

        // Update size
        frame.width = width;
        frame.height = height;

        // Set new frame (NSWindow uses setFrame:display:)
        objc_msgSend_rect_bool(handle, _selSetFrameDisplay, frame, true);
    }

    public void SetWindowLocation(IntPtr handle, int x, int y)
    {
        // Get current frame
        objc_msgSend_stret(out CGRect frame, handle, _selFrame);

        // Update location (note: Cocoa uses bottom-left origin)
        frame.x = x;
        frame.y = y;

        // Set new frame (NSWindow uses setFrame:display:)
        objc_msgSend_rect_bool(handle, _selSetFrameDisplay, frame, true);
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

    /// <summary>
    /// Executes an action synchronously on the macOS main thread using GCD.
    /// This is required for NSWindow and other AppKit operations that MUST run on the process's first thread.
    /// </summary>
    public void ExecuteOnMainThread(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        // If a custom executor is set (e.g., by TestHost), use it instead of GCD
        if (CustomMainThreadExecutor != null)
        {
            CustomMainThreadExecutor(action);
            return;
        }

        // Without TestHost's CustomMainThreadExecutor, we can't guarantee main thread execution
        // Tests MUST be run via TestHost which provides the main thread dispatcher
        // If running directly without TestHost on macOS, execution will fail
        throw new InvalidOperationException(
            "macOS main thread execution requires TestHost. " +
            "Run tests via: dotnet run --project tests/SWTSharp.TestHost -- tests/SWTSharp.Tests/bin/Debug/netX.0/SWTSharp.Tests.dll");
    }

    public IntPtr CreateComposite(IntPtr parent, int style)
    {
        // Create an NSView instance to act as a container
        IntPtr nsViewClass = objc_getClass("NSView");
        IntPtr view = objc_msgSend(objc_msgSend(nsViewClass, _selAlloc), _selInit);

        // Set a default frame
        CGRect frame = new CGRect(0, 0, 100, 100);
        IntPtr selSetFrame = sel_registerName("setFrame:");
        objc_msgSend_rect(view, selSetFrame, frame);

        // Add to parent if provided
        AddChildToParent(parent, view);

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
    private IntPtr _selIsKindOfClass;
    private IntPtr _selContentView;
    private IntPtr _selRespondsToSelector;
    private IntPtr _selClass;

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
            if (_nsButtonClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSButton class from Objective-C runtime");

            _selButtonWithTitle = RegisterSelector("buttonWithTitle:target:action:");
            _selSetButtonType = RegisterSelector("setButtonType:");
            _selSetState = RegisterSelector("setState:");
            _selState = RegisterSelector("state");
            _selSetTarget = RegisterSelector("setTarget:");
            _selSetAction = RegisterSelector("setAction:");
            _selSetEnabled = RegisterSelector("setEnabled:");
            _selSetHidden = RegisterSelector("setHidden:");
            _selSetFrameOrigin = RegisterSelector("setFrameOrigin:");
            _selSetFrameSize = RegisterSelector("setFrameSize:");
            // Note: _selAddSubview is now initialized in main Initialize() method
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
        AddChildToParent(parent, button);

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
        // Phase 1 Fix: Detect pseudo-handles and route to specialized methods
        long handleValue = handle.ToInt64();

        if ((handleValue & 0x40000000) != 0)
        {
            // ToolBar pseudo-handle - toolbars don't have enabled state
            return;
        }

        if ((handleValue & 0x30000000) != 0)
        {
            // ToolItem pseudo-handle - route to specialized handler
            SetToolItemEnabled(handle, enabled);
            return;
        }

        if ((handleValue & 0x20000000) != 0)
        {
            // TabItem pseudo-handle - tab items don't have independent enabled state
            return;
        }

        // Real native handle - only NSControl and its subclasses have setEnabled:
        // NSView (used by Canvas) does not support this method
        IntPtr selIsKindOfClass = sel_registerName("isKindOfClass:");
        if (objc_msgSend_bool(handle, selIsKindOfClass, _nsControlClass))
        {
            objc_msgSend_void(handle, _selSetEnabled, enabled);
        }
        // For NSView, we could set alpha or userInteractionEnabled, but SWT typically
        // just ignores setEnabled on non-control widgets like Canvas
    }

    public void SetControlVisible(IntPtr handle, bool visible)
    {
        // Phase 1 Fix: Detect pseudo-handles and route to specialized methods
        // Pseudo-handles use high bits to differentiate from real pointers:
        // - 0x40000000 range: ToolBar pseudo-handles
        // - 0x30000000 range: ToolItem pseudo-handles
        // - 0x20000000 range: TabItem pseudo-handles
        long handleValue = handle.ToInt64();

        if ((handleValue & 0x40000000) != 0)
        {
            // ToolBar pseudo-handle - route to specialized handler
            SetToolBarVisible(handle, visible);
            return;
        }

        if ((handleValue & 0x30000000) != 0)
        {
            // ToolItem pseudo-handle - items don't have independent visibility
            // Visibility is controlled by adding/removing from toolbar
            return;
        }

        if ((handleValue & 0x20000000) != 0)
        {
            // TabItem pseudo-handle - route to specialized handler
            SetTabItemVisible(handle, visible);
            return;
        }

        // Real native handle - use standard NSView setHidden: API
        objc_msgSend_void(handle, _selSetHidden, !visible);
    }

    private void SetToolBarVisible(IntPtr pseudoHandle, bool visible)
    {
        if (_toolBarData.TryGetValue(pseudoHandle, out var toolbarData))
        {
            // NSToolbar uses setVisible:, not setHidden:
            objc_msgSend_void(toolbarData.Toolbar, _selSetVisible, visible);
        }
    }

    private void SetTabItemVisible(IntPtr pseudoHandle, bool visible)
    {
        if (_tabItemData.TryGetValue(pseudoHandle, out var itemData))
        {
            // TabItem visibility is controlled through tab selection
            // Individual tabs can't be hidden, only selected/deselected
            // For now, this is a no-op
        }
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
            if (_nsMenuClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSMenu class from Objective-C runtime");

            _nsMenuItemClass = objc_getClass("NSMenuItem");
            if (_nsMenuItemClass == IntPtr.Zero)
                throw new InvalidOperationException("Failed to get NSMenuItem class from Objective-C runtime");

            _selAddItem = RegisterSelector("addItem:");
            _selSetSubmenu = RegisterSelector("setSubmenu:");
            // Reuse _selSetTitle_item as setTitle: already registered
            _selSetTitle_item = _registeredSelectors["setTitle:"];
            // Skip _selSetState as setState: already registered in button selectors
            // Reuse setEnabled: if already registered (from button selectors)
            _selSetEnabled_item = _registeredSelectors.TryGetValue("setEnabled:", out var setEnabled)
                ? setEnabled
                : RegisterSelector("setEnabled:");
            _selSetMainMenu = RegisterSelector("setMainMenu:");
            _selPopUpMenuPositioningItem = RegisterSelector("popUpMenuPositioningItem:atLocation:inView:");
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

    // Text control operations - selectors only (class pointers are in ObjCRuntime)
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
        if (_selStringValue == IntPtr.Zero)
        {
            _selStringValue = RegisterSelector("stringValue");
            _selSetStringValue = RegisterSelector("setStringValue:");
            _selDocumentView = RegisterSelector("documentView");
            _selString = RegisterSelector("string");
            _selSetString = RegisterSelector("setString:");
            _selSetEditable_text = RegisterSelector("setEditable:");
            _selSelectedRange = RegisterSelector("selectedRange");
            _selSetSelectedRange = RegisterSelector("setSelectedRange:");
        }
    }

    public IntPtr CreateText(IntPtr parent, int style)
    {
        var objc = ObjCRuntime.Instance;
        InitializeTextSelectors();

        IntPtr textControl;

        if ((style & SWT.PASSWORD) != 0)
        {
            // Create NSSecureTextField for password fields
            textControl = objc_msgSend(objc.NSSecureTextField, objc.SelAlloc);
            textControl = objc_msgSend(textControl, objc.SelInit);
        }
        else if ((style & SWT.MULTI) != 0)
        {
            // Create NSScrollView with NSTextView for multi-line text
            IntPtr scrollView = objc_msgSend(objc.NSScrollView, objc.SelAlloc);
            scrollView = objc_msgSend(scrollView, objc.SelInit);

            // Create NSTextView
            IntPtr textView = objc_msgSend(objc.NSTextView, objc.SelAlloc);
            textView = objc_msgSend(textView, objc.SelInit);

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
            textControl = objc_msgSend(objc.NSTextField, objc.SelAlloc);
            textControl = objc_msgSend(textControl, objc.SelInit);
        }

        // Set default frame
        var frame = new CGRect(0, 0, 100, 20);
        objc_msgSend_rect(textControl, _selSetFrame, frame);

        // Add to parent if provided
        AddChildToParent(parent, textControl);

        return textControl;
    }

    public void SetTextContent(IntPtr handle, string text)
    {
        InitializeTextSelectors();
        IntPtr nsText = CreateNSString(text ?? string.Empty);

        // Check if it's a scroll view (multi-line) by checking if it responds to documentView
        IntPtr selRespondsToSelector = sel_registerName("respondsToSelector:");
        bool respondsToDocumentView = objc_msgSend_bool(handle, selRespondsToSelector, _selDocumentView);

        if (respondsToDocumentView)
        {
            // It's a scroll view, get documentView and set text on the text view
            IntPtr documentView = objc_msgSend(handle, _selDocumentView);
            if (documentView != IntPtr.Zero)
            {
                objc_msgSend(documentView, _selSetString, nsText);
            }
            else
            {
                // Scroll view exists but no document view yet, use setStringValue as fallback
                objc_msgSend(handle, _selSetStringValue, nsText);
            }
        }
        else
        {
            // Single-line text field or password field
            objc_msgSend(handle, _selSetStringValue, nsText);
        }
    }

    public string GetTextContent(IntPtr handle)
    {
        InitializeTextSelectors();

        // Check if it's a scroll view (multi-line) by checking if it responds to documentView
        IntPtr selRespondsToSelector = sel_registerName("respondsToSelector:");
        bool respondsToDocumentView = objc_msgSend_bool(handle, selRespondsToSelector, _selDocumentView);
        IntPtr nsString;

        if (respondsToDocumentView)
        {
            // It's a scroll view, get text from the document view
            IntPtr documentView = objc_msgSend(handle, _selDocumentView);
            if (documentView != IntPtr.Zero)
            {
                nsString = objc_msgSend(documentView, _selString);
            }
            else
            {
                // Scroll view exists but no document view yet, use stringValue as fallback
                nsString = objc_msgSend(handle, _selStringValue);
            }
        }
        else
        {
            // Single-line text field or password field
            nsString = objc_msgSend(handle, _selStringValue);
        }

        return GetNSStringValue(nsString);
    }

    public void SetTextSelection(IntPtr handle, int start, int end)
    {
        InitializeTextSelectors();

        // Get the text view - check if it responds to documentView first
        IntPtr selRespondsToSelector = sel_registerName("respondsToSelector:");
        bool respondsToDocumentView = objc_msgSend_bool(handle, selRespondsToSelector, _selDocumentView);

        IntPtr textView = handle;
        if (respondsToDocumentView)
        {
            IntPtr documentView = objc_msgSend(handle, _selDocumentView);
            if (documentView != IntPtr.Zero)
            {
                textView = documentView;
            }
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

        // Get the text view - check if it responds to documentView first
        IntPtr selRespondsToSelector = sel_registerName("respondsToSelector:");
        bool respondsToDocumentView = objc_msgSend_bool(handle, selRespondsToSelector, _selDocumentView);

        IntPtr textView = handle;
        if (respondsToDocumentView)
        {
            IntPtr documentView = objc_msgSend(handle, _selDocumentView);
            if (documentView != IntPtr.Zero)
            {
                textView = documentView;
            }
        }

        NSRange range;
        unsafe
        {
            objc_msgSend_stret(out range, textView, _selSelectedRange);
        }

        return ((int)range.location, (int)(range.location + range.length));
    }

#if NET7_0_OR_GREATER
    [LibraryImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static partial void objc_msgSend_stret(out NSRange retval, IntPtr receiver, IntPtr selector);
#else
    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out NSRange retval, IntPtr receiver, IntPtr selector);
#endif

    public void SetTextLimit(IntPtr handle, int limit)
    {
        // NSTextField doesn't have a built-in character limit
        // This would require a delegate implementation
        // For now, we'll leave this as a no-op
    }

    public void SetTextReadOnly(IntPtr handle, bool readOnly)
    {
        InitializeTextSelectors();

        // Get the text view - check if it responds to documentView first
        IntPtr selRespondsToSelector = sel_registerName("respondsToSelector:");
        bool respondsToDocumentView = objc_msgSend_bool(handle, selRespondsToSelector, _selDocumentView);

        IntPtr textView = handle;
        if (respondsToDocumentView)
        {
            IntPtr documentView = objc_msgSend(handle, _selDocumentView);
            if (documentView != IntPtr.Zero)
            {
                textView = documentView;
            }
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
