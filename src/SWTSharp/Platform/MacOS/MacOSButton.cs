using System.Runtime.InteropServices;
using System.Drawing;
using SWTSharp.Graphics;
using System.Collections.Concurrent;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// Base class for all macOS platform widgets.
/// Provides common functionality and native handle access.
/// </summary>
internal abstract class MacOSWidget
{
    /// <summary>
    /// Gets the native handle for this widget.
    /// </summary>
    public abstract IntPtr GetNativeHandle();
}

/// <summary>
/// macOS implementation of a button platform widget.
/// Encapsulates NSButton and provides IPlatformTextWidget functionality.
/// </summary>
internal class MacOSButton : MacOSWidget, IPlatformTextWidget
{
    private IntPtr _nsButtonHandle;
    private bool _disposed;

    // Static mapping of button handles to instances for callback routing
    private static readonly ConcurrentDictionary<IntPtr, MacOSButton> _buttonInstances = new();

    // Static mapping of target instances to button instances for callback routing
    private static readonly ConcurrentDictionary<IntPtr, MacOSButton> _targetToButtonMap = new();

    // Static callback delegate that Objective-C can invoke
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ButtonActionCallback(IntPtr self, IntPtr selector, IntPtr sender);
    private static readonly ButtonActionCallback _staticClickCallback = OnButtonClickedStatic;

    // Event handling
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? TextCommitted;

    public MacOSButton(IntPtr parentHandle, int style)
    {
        // Create NSButton using objc_msgSend
        _nsButtonHandle = CreateNSButton(parentHandle, style);

        // Setup event handling
        SetupEventHandlers();
    }

    public void SetText(string text)
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsButtonHandle, setTitle:, NSString stringWithString:text)
        IntPtr textPtr = IntPtr.Zero;
        IntPtr nsString = IntPtr.Zero;

        try
        {
            var strClass = objc_getClass("NSString");
            var selector = sel_registerName("stringWithString:");
            textPtr = Marshal.StringToHGlobalAuto(text);
            nsString = objc_msgSend(strClass, selector, textPtr);

            var setTitleSelector = sel_registerName("setTitle:");
            objc_msgSend(_nsButtonHandle, setTitleSelector, nsString);

            // Release the NSString to prevent memory leak
            var releaseSelector = sel_registerName("release");
            objc_msgSend(nsString, releaseSelector);

            // Fire TextChanged event
            TextChanged?.Invoke(this, text);
        }
        finally
        {
            // Always free the unmanaged memory
            if (textPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(textPtr);
            }
        }
    }

    public string GetText()
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return "";

        // objc_msgSend(_nsButtonHandle, title)
        var selector = sel_registerName("title");
        var nsString = objc_msgSend(_nsButtonHandle, selector);
        return NSStringToString(nsString);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsButtonHandle, setFrame:, NSMakeRect(x, y, width, height))
        var rectClass = objc_getClass("NSValue");
        var selector = sel_registerName("valueWithRect:");
        var rect = new NSRect { x = x, y = y, width = width, height = height };
        var rectValue = objc_msgSend(rectClass, selector, rect);

        var setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend(_nsButtonHandle, setFrameSelector, rectValue);
    }

    public SWTSharp.Graphics.Rectangle GetBounds()
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return default(SWTSharp.Graphics.Rectangle);

        // objc_msgSend(_nsButtonHandle, frame)
        var selector = sel_registerName("frame");
        var frameValue = objc_msgSend(_nsButtonHandle, selector);

        // Extract NSRect from NSValue
        var rectSelector = sel_registerName("rectValue");
        var rect = Marshal.PtrToStructure<NSRect>(objc_msgSend(frameValue, rectSelector));

        return new SWTSharp.Graphics.Rectangle((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsButtonHandle, setHidden:, !visible)
        var selector = sel_registerName("setHidden:");
        objc_msgSend(_nsButtonHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return false;

        // objc_msgSend(_nsButtonHandle, isHidden)
        var selector = sel_registerName("isHidden");
        var result = objc_msgSend(_nsButtonHandle, selector);
        return result == IntPtr.Zero; // Returns true when NOT hidden (i.e., visible)
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsButtonHandle, setEnabled:, enabled)
        var selector = sel_registerName("setEnabled:");
        objc_msgSend(_nsButtonHandle, selector, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return false;

        // objc_msgSend(_nsButtonHandle, isEnabled)
        var selector = sel_registerName("isEnabled");
        var result = objc_msgSend(_nsButtonHandle, selector);
        return result != IntPtr.Zero;
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // TODO: Implement background color setting
        // This would require NSColor handling
    }

    public RGB GetBackground()
    {
        // TODO: Implement background color getting
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // TODO: Implement foreground color setting
        // This would require NSColor handling
    }

    public RGB GetForeground()
    {
        // TODO: Implement foreground color getting
        return new RGB(0, 0, 0); // Default black
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_nsButtonHandle != IntPtr.Zero)
            {
                // Remove from instance mappings
                _buttonInstances.TryRemove(_nsButtonHandle, out _);

                // objc_msgSend(_nsButtonHandle, release)
                var selector = sel_registerName("release");
                objc_msgSend(_nsButtonHandle, selector);
                _nsButtonHandle = IntPtr.Zero;
            }

            // Release target instance
            if (_targetInstance != IntPtr.Zero)
            {
                _targetToButtonMap.TryRemove(_targetInstance, out _);

                var selector = sel_registerName("release");
                objc_msgSend(_targetInstance, selector);
                _targetInstance = IntPtr.Zero;
            }

            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsButtonHandle;
    }

    private IntPtr CreateNSButton(IntPtr parentHandle, int style)
    {
        // Implementation moved from MacOSPlatform.CreateButton
        // This should be a basic NSButton creation
        var buttonClass = objc_getClass("NSButton");
        var allocSelector = sel_registerName("alloc");
        var initSelector = sel_registerName("init");

        var button = objc_msgSend(buttonClass, allocSelector);
        return objc_msgSend(button, initSelector);
    }

    // Native method declarations
    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg);

    // P/Invoke declarations for Objective-C runtime class creation
    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_allocateClassPair(IntPtr superclass, string name, int extraBytes);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern bool class_addMethod(IntPtr cls, IntPtr selector, IntPtr implementation, string types);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern void objc_registerClassPair(IntPtr cls);

    // Static fields for the runtime-created target class
    private static IntPtr _targetClass = IntPtr.Zero;
    private static bool _classRegistered = false;
    private static readonly object _classLock = new object();

    // Store target instances to prevent GC
    private IntPtr _targetInstance = IntPtr.Zero;

    private static string NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero) return "";

        var selector = sel_registerName("UTF8String");
        var utf8Ptr = objc_msgSend(nsString, selector);
        return Marshal.PtrToStringAuto(utf8Ptr) ?? "";
    }

    /// <summary>
    /// Sets up native event handlers for the NSButton.
    ///
    /// IMPLEMENTATION STATUS: PARTIAL - Infrastructure in place, but callback mechanism incomplete.
    ///
    /// This method establishes the target/action pattern required for NSButton click events.
    /// The infrastructure (instance mapping, callback delegate) is ready, but the Objective-C
    /// runtime plumbing to actually receive callbacks is not yet implemented.
    ///
    /// WHAT WORKS:
    /// - Instance mapping: Button handles are registered in _buttonInstances dictionary
    /// - Callback infrastructure: ButtonActionCallback delegate and OnButtonClickedStatic are defined
    /// - Target/Action setup: Calls setTarget: and setAction: on the button
    ///
    /// WHAT'S MISSING:
    /// To make this fully functional, we need Objective-C runtime class creation:
    ///
    /// 1. Create custom NSObject subclass at runtime:
    ///    - Call objc_allocateClassPair(objc_getClass("NSObject"), "SWTButtonTarget", 0)
    ///    - This creates a new Objective-C class that can respond to selectors
    ///
    /// 2. Add callback method to the class:
    ///    - Convert _staticClickCallback delegate to function pointer
    ///    - Call class_addMethod(targetClass, actionSelector, functionPtr, "v@:@")
    ///    - Type encoding "v@:@" means: void return, self (@), selector (:), sender (@)
    ///
    /// 3. Register and instantiate:
    ///    - Call objc_registerClassPair(targetClass) to finalize the class
    ///    - Create instance: objc_msgSend(targetClass, sel_registerName("alloc"))
    ///    - Initialize: objc_msgSend(instance, sel_registerName("init"))
    ///
    /// 4. Use the target object:
    ///    - Call setTarget: with the target object (not the button itself)
    ///    - Call setAction: with "buttonClicked:" selector
    ///
    /// 5. Add required P/Invoke declarations:
    ///    [DllImport("/usr/lib/libobjc.A.dylib")]
    ///    private static extern IntPtr objc_allocateClassPair(IntPtr superclass, string name, int extraBytes);
    ///
    ///    [DllImport("/usr/lib/libobjc.A.dylib")]
    ///    private static extern bool class_addMethod(IntPtr cls, IntPtr selector, IntPtr implementation, string types);
    ///
    ///    [DllImport("/usr/lib/libobjc.A.dylib")]
    ///    private static extern void objc_registerClassPair(IntPtr cls);
    ///
    /// ALTERNATIVE SIMPLER APPROACHES:
    /// - Use NSNotificationCenter to observe button clicks (more complex, but avoids runtime class creation)
    /// - Create an Objective-C wrapper library in .m file and P/Invoke to that
    /// - Use existing event notification patterns if available in NSButton
    ///
    /// For reference, see similar implementations in:
    /// - Xamarin.Mac bindings
    /// - MonoMac event handling
    /// - Other Objective-C interop libraries
    /// </summary>
    private void SetupEventHandlers()
    {
        if (_disposed || _nsButtonHandle == IntPtr.Zero) return;

        // Register this button instance for callback routing
        _buttonInstances[_nsButtonHandle] = this;

        // Step 1: Create runtime class once per application lifetime
        lock (_classLock)
        {
            if (!_classRegistered)
            {
                // Create "SWTButtonTarget" class from NSObject
                var nsObjectClass = objc_getClass("NSObject");
                _targetClass = objc_allocateClassPair(nsObjectClass, "SWTButtonTarget", 0);

                if (_targetClass == IntPtr.Zero)
                {
                    throw new InvalidOperationException("Failed to allocate Objective-C class pair for SWTButtonTarget");
                }

                // Step 2: Add "buttonClicked:" method to the class
                var actionSelector = sel_registerName("buttonClicked:");
                var callbackPtr = Marshal.GetFunctionPointerForDelegate(_staticClickCallback);

                // Type encoding: "v@:@" means void return (@=id self, :=SEL selector, @=id sender)
                bool methodAdded = class_addMethod(_targetClass, actionSelector, callbackPtr, "v@:@");

                if (!methodAdded)
                {
                    throw new InvalidOperationException("Failed to add buttonClicked: method to SWTButtonTarget class");
                }

                // Step 3: Register the class with Objective-C runtime
                objc_registerClassPair(_targetClass);
                _classRegistered = true;
            }
        }

        // Step 4: Create target instance for this button
        var allocSelector = sel_registerName("alloc");
        var initSelector = sel_registerName("init");
        _targetInstance = objc_msgSend(_targetClass, allocSelector);
        _targetInstance = objc_msgSend(_targetInstance, initSelector);

        if (_targetInstance == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create SWTButtonTarget instance");
        }

        // Register target-to-button mapping for callback routing
        _targetToButtonMap[_targetInstance] = this;

        // Step 5: Set target and action on the NSButton
        var actionSelector2 = sel_registerName("buttonClicked:");
        var setTargetSelector = sel_registerName("setTarget:");
        var setActionSelector = sel_registerName("setAction:");

        // Set the target to our custom target object (not the button itself)
        objc_msgSend(_nsButtonHandle, setTargetSelector, _targetInstance);
        objc_msgSend(_nsButtonHandle, setActionSelector, actionSelector2);
    }

    /// <summary>
    /// Static callback method that Objective-C runtime can invoke.
    /// Routes the callback to the appropriate MacOSButton instance.
    /// </summary>
    /// <param name="self">The target instance (SWTButtonTarget) that received the action</param>
    /// <param name="selector">The selector that was invoked (buttonClicked:)</param>
    /// <param name="sender">The NSButton that sent the action</param>
    private static void OnButtonClickedStatic(IntPtr self, IntPtr selector, IntPtr sender)
    {
        // Route via target instance (self parameter is our SWTButtonTarget object)
        if (_targetToButtonMap.TryGetValue(self, out var button))
        {
            button.OnClick();
        }
    }

    private void OnClick()
    {
        if (_disposed) return;
        Click?.Invoke(this, 0);
    }

    private void OnFocusGained()
    {
        if (_disposed) return;
        FocusGained?.Invoke(this, 0);
    }

    private void OnFocusLost()
    {
        if (_disposed) return;
        FocusLost?.Invoke(this, 0);
    }

    private void OnKeyDown(PlatformKeyEventArgs args)
    {
        if (_disposed) return;
        KeyDown?.Invoke(this, args);
    }

    private void OnKeyUp(PlatformKeyEventArgs args)
    {
        if (_disposed) return;
        KeyUp?.Invoke(this, args);
    }

    private void OnTextCommitted(string text)
    {
        if (_disposed) return;
        TextCommitted?.Invoke(this, text);
    }
}

/// <summary>
/// Native NSRect structure for macOS.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct NSRect
{
    public double x;
    public double y;
    public double width;
    public double height;
}