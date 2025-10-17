using System;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformTabItem using NSTabViewItem.
/// Represents a single tab page within an NSTabView.
/// </summary>
internal class MacOSTabItem : IPlatformTabItem
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private readonly MacOSTabFolder _tabFolder;
    private readonly IntPtr _nsTabViewItemHandle;
    private readonly IntPtr _nsTabViewHandle;
    private bool _disposed;
    private string _text = string.Empty;

    // Event handling
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public MacOSTabItem(MacOSTabFolder tabFolder, IntPtr nsTabViewHandle, int style, int index)
    {
        _tabFolder = tabFolder ?? throw new ArgumentNullException(nameof(tabFolder));
        _nsTabViewHandle = nsTabViewHandle;

        // Create NSTabViewItem
        _nsTabViewItemHandle = CreateNSTabViewItem();

        if (_nsTabViewItemHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSTabViewItem");
        }

        // Add tab view item to tab view
        IntPtr addSelector = sel_registerName("addTabViewItem:");
        objc_msgSend(_nsTabViewHandle, addSelector, _nsTabViewItemHandle);
    }

    internal IntPtr GetNativeHandle()
    {
        return _nsTabViewItemHandle;
    }

    public void SetText(string text)
    {
        if (_disposed || _nsTabViewItemHandle == IntPtr.Zero) return;

        _text = text ?? string.Empty;

        // Create NSString from text
        IntPtr nsString = CreateNSString(_text);

        // Call setLabel: on the tab view item
        IntPtr selector = sel_registerName("setLabel:");
        objc_msgSend(_nsTabViewItemHandle, selector, nsString);

        // Release NSString
        IntPtr releaseSelector = sel_registerName("release");
        objc_msgSend(nsString, releaseSelector);
    }

    public string GetText()
    {
        if (_disposed || _nsTabViewItemHandle == IntPtr.Zero) return string.Empty;

        // Call label method
        IntPtr selector = sel_registerName("label");
        IntPtr nsString = objc_msgSend(_nsTabViewItemHandle, selector);

        return NSStringToString(nsString);
    }

    public void SetControl(IPlatformWidget? control)
    {
        if (_disposed || _nsTabViewItemHandle == IntPtr.Zero) return;

        IntPtr controlHandle = IntPtr.Zero;
        if (control is MacOSWidget macOSControl)
        {
            controlHandle = macOSControl.GetNativeHandle();
        }

        if (controlHandle != IntPtr.Zero)
        {
            // Call setView: on the tab view item
            IntPtr selector = sel_registerName("setView:");
            objc_msgSend(_nsTabViewItemHandle, selector, controlHandle);
        }
    }

    public void SetToolTipText(string toolTip)
    {
        if (_disposed || _nsTabViewItemHandle == IntPtr.Zero) return;

        // Create NSString from tooltip
        IntPtr nsString = CreateNSString(toolTip ?? string.Empty);

        // Call setToolTip: on the tab view item
        IntPtr selector = sel_registerName("setToolTip:");
        objc_msgSend(_nsTabViewItemHandle, selector, nsString);

        // Release NSString
        IntPtr releaseSelector = sel_registerName("release");
        objc_msgSend(nsString, releaseSelector);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Remove from tab view and release
        if (_nsTabViewItemHandle != IntPtr.Zero && _nsTabViewHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeTabViewItem:");
            objc_msgSend(_nsTabViewHandle, removeSelector, _nsTabViewItemHandle);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsTabViewItemHandle, releaseSelector);
        }
    }

    #region Private Helper Methods

    private IntPtr CreateNSTabViewItem()
    {
        // Get NSTabViewItem class
        IntPtr nsTabViewItemClass = objc_getClass("NSTabViewItem");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr tabViewItem = objc_msgSend(nsTabViewItemClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        return objc_msgSend(tabViewItem, initSelector);
    }

    private IntPtr CreateNSString(string str)
    {
        if (string.IsNullOrEmpty(str)) str = "";

        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr nsString = objc_msgSend(nsStringClass, allocSelector);

        IntPtr initSelector = sel_registerName("initWithUTF8String:");
        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(str);
        nsString = objc_msgSend(nsString, initSelector, utf8Ptr);
        Marshal.FreeHGlobal(utf8Ptr);

        return nsString;
    }

    private string NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero) return string.Empty;

        IntPtr selector = sel_registerName("UTF8String");
        IntPtr utf8Ptr = objc_msgSend(nsString, selector);
        return Marshal.PtrToStringAnsi(utf8Ptr) ?? string.Empty;
    }

    #endregion

    #region ObjC P/Invoke

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_getClass(string className);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    #endregion
}