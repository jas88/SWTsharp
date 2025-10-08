using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Singleton cache for Objective-C runtime class pointers.
/// Initialized once on UI thread at startup for thread-safety and reliability.
/// </summary>
internal sealed class ObjCRuntime
{
    private static readonly Lazy<ObjCRuntime> _instance = new(() => new ObjCRuntime());
    public static ObjCRuntime Instance => _instance.Value;

    private const string AppKit = "/System/Library/Frameworks/AppKit.framework/AppKit";

    [DllImport(AppKit)]
    private static extern void NSApplicationLoad();

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    // Common selectors
    public IntPtr SelAlloc { get; private set; }
    public IntPtr SelInit { get; private set; }
    public IntPtr SelRelease { get; private set; }
    public IntPtr SelRetain { get; private set; }
    public IntPtr SelAutorelease { get; private set; }

    // Application classes
    public IntPtr NSApplication { get; private set; }
    public IntPtr NSWindow { get; private set; }
    public IntPtr NSView { get; private set; }
    public IntPtr NSMenu { get; private set; }
    public IntPtr NSMenuItem { get; private set; }

    // Widget classes
    public IntPtr NSTextField { get; private set; }
    public IntPtr NSSecureTextField { get; private set; }
    public IntPtr NSTextView { get; private set; }
    public IntPtr NSButton { get; private set; }
    public IntPtr NSComboBox { get; private set; }
    public IntPtr NSPopUpButton { get; private set; }
    public IntPtr NSProgressIndicator { get; private set; }
    public IntPtr NSBox { get; private set; }

    // Container classes
    public IntPtr NSScrollView { get; private set; }
    public IntPtr NSTableView { get; private set; }
    public IntPtr NSTableColumn { get; private set; }
    public IntPtr NSTabView { get; private set; }
    public IntPtr NSTabViewItem { get; private set; }
    public IntPtr NSToolbar { get; private set; }
    public IntPtr NSToolbarItem { get; private set; }

    // Data classes
    public IntPtr NSIndexSet { get; private set; }
    public IntPtr NSMutableIndexSet { get; private set; }
    public IntPtr NSTreeStore { get; private set; }

    private ObjCRuntime()
    {
        // CRITICAL: Load AppKit framework first
        // This makes all Cocoa/AppKit classes available via objc_getClass
        NSApplicationLoad();

        // Initialize all selectors first
        SelAlloc = sel_registerName("alloc");
        SelInit = sel_registerName("init");
        SelRelease = sel_registerName("release");
        SelRetain = sel_registerName("retain");
        SelAutorelease = sel_registerName("autorelease");

        // Initialize all class pointers (now that AppKit is loaded)
        NSApplication = objc_getClass("NSApplication");
        NSWindow = objc_getClass("NSWindow");
        NSView = objc_getClass("NSView");
        NSMenu = objc_getClass("NSMenu");
        NSMenuItem = objc_getClass("NSMenuItem");

        NSTextField = objc_getClass("NSTextField");
        NSSecureTextField = objc_getClass("NSSecureTextField");
        NSTextView = objc_getClass("NSTextView");
        NSButton = objc_getClass("NSButton");
        NSComboBox = objc_getClass("NSComboBox");
        NSPopUpButton = objc_getClass("NSPopUpButton");
        NSProgressIndicator = objc_getClass("NSProgressIndicator");
        NSBox = objc_getClass("NSBox");

        NSScrollView = objc_getClass("NSScrollView");
        NSTableView = objc_getClass("NSTableView");
        NSTableColumn = objc_getClass("NSTableColumn");
        NSTabView = objc_getClass("NSTabView");
        NSTabViewItem = objc_getClass("NSTabViewItem");
        NSToolbar = objc_getClass("NSToolbar");
        NSToolbarItem = objc_getClass("NSToolbarItem");

        NSIndexSet = objc_getClass("NSIndexSet");
        NSMutableIndexSet = objc_getClass("NSMutableIndexSet");
        NSTreeStore = objc_getClass("NSTreeStore");
    }

    /// <summary>
    /// Ensures the singleton is initialized. Call this early on the UI thread.
    /// </summary>
    public static void EnsureInitialized()
    {
        _ = Instance;
    }
}
