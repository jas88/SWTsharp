using System.Runtime.InteropServices;
using System.Drawing;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of a label platform widget.
/// Encapsulates NSTextField and provides IPlatformTextWidget functionality.
/// </summary>
internal class MacOSLabel : MacOSWidget, IPlatformTextWidget
{
    private IntPtr _nsLabelHandle;
    private bool _disposed;

    // Event handling
    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? TextCommitted;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    public MacOSLabel(IntPtr parentHandle, int style, int alignment, bool wrap)
    {
        // Create NSTextField using objc_msgSend
        _nsLabelHandle = CreateNSTextField(parentHandle, style, alignment, wrap);
    }

    public void SetText(string text)
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsLabelHandle, setStringValue:, NSString stringWithString:text)
        var strClass = objc_getClass("NSString");
        var selector = sel_registerName("stringWithString:");
        var textPtr = Marshal.StringToHGlobalAuto(text);
        try
        {
            var nsString = objc_msgSend(strClass, selector, textPtr);
            var setStringValueSelector = sel_registerName("setStringValue:");
            objc_msgSend(_nsLabelHandle, setStringValueSelector, nsString);

            // Fire TextChanged event
            TextChanged?.Invoke(this, text ?? string.Empty);
        }
        finally
        {
            Marshal.FreeHGlobal(textPtr);
        }
    }

    public string GetText()
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return "";

        // objc_msgSend(_nsLabelHandle, stringValue)
        var selector = sel_registerName("stringValue");
        var nsString = objc_msgSend(_nsLabelHandle, selector);
        return NSStringToString(nsString);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsLabelHandle, setFrame:, NSMakeRect(x, y, width, height))
        var rectClass = objc_getClass("NSValue");
        var selector = sel_registerName("valueWithRect:");
        var rect = new NSRect { x = x, y = y, width = width, height = height };
        var rectValue = objc_msgSend(rectClass, selector, rect);

        var setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend(_nsLabelHandle, setFrameSelector, rectValue);
    }

    public SWTSharp.Graphics.Rectangle GetBounds()
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return default(SWTSharp.Graphics.Rectangle);

        // objc_msgSend(_nsLabelHandle, frame)
        var selector = sel_registerName("frame");
        var frameValue = objc_msgSend(_nsLabelHandle, selector);

        // Extract NSRect from NSValue
        var rectSelector = sel_registerName("rectValue");
        var rect = Marshal.PtrToStructure<NSRect>(objc_msgSend(frameValue, rectSelector));

        return new SWTSharp.Graphics.Rectangle((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsLabelHandle, setHidden:, !visible)
        var selector = sel_registerName("setHidden:");
        objc_msgSend(_nsLabelHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return false;

        // objc_msgSend(_nsLabelHandle, isHidden)
        var selector = sel_registerName("isHidden");
        var result = objc_msgSend(_nsLabelHandle, selector);
        return result != IntPtr.Zero;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return;

        // objc_msgSend(_nsLabelHandle, setEnabled:, enabled)
        var selector = sel_registerName("setEnabled:");
        objc_msgSend(_nsLabelHandle, selector, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return false;

        // objc_msgSend(_nsLabelHandle, isEnabled)
        var selector = sel_registerName("isEnabled");
        var result = objc_msgSend(_nsLabelHandle, selector);
        return result != IntPtr.Zero;
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return;

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
        if (_disposed || _nsLabelHandle == IntPtr.Zero) return;

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
            if (_nsLabelHandle != IntPtr.Zero)
            {
                // objc_msgSend(_nsLabelHandle, release)
                var selector = sel_registerName("release");
                objc_msgSend(_nsLabelHandle, selector);
                _nsLabelHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsLabelHandle;
    }

    private IntPtr CreateNSTextField(IntPtr parentHandle, int style, int alignment, bool wrap)
    {
        // Implementation should create NSTextField with proper alignment and wrapping
        var textFieldClass = objc_getClass("NSTextField");
        var allocSelector = sel_registerName("alloc");
        var initSelector = sel_registerName("init");

        var textField = objc_msgSend(textFieldClass, allocSelector);
        var initializedField = objc_msgSend(textField, initSelector);

        // Configure as label (non-editable)
        var setEditableSelector = sel_registerName("setEditable:");
        objc_msgSend(initializedField, setEditableSelector, false);

        var setBorderedSelector = sel_registerName("setBordered:");
        objc_msgSend(initializedField, setBorderedSelector, false);

        var setSelectableSelector = sel_registerName("setSelectable:");
        objc_msgSend(initializedField, setSelectableSelector, false);

        return initializedField;
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
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, NSRect arg);

    private static string NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero) return "";

        var selector = sel_registerName("UTF8String");
        var utf8Ptr = objc_msgSend(nsString, selector);
        return Marshal.PtrToStringAuto(utf8Ptr) ?? "";
    }

    // Event handling methods
    private void OnTextChanged()
    {
        if (_disposed) return;
        var text = GetText();
        TextChanged?.Invoke(this, text);
    }

    private void OnTextCommitted()
    {
        if (_disposed) return;
        var text = GetText();
        TextCommitted?.Invoke(this, text);
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
}