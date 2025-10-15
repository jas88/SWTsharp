using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of an editable text widget using NSTextField.
/// </summary>
internal class MacOSText : MacOSWidget, IPlatformTextInput
{
    private IntPtr _textField;
    private bool _disposed;
    private string _text = string.Empty;
    private bool _readOnly;
    private int _textLimit = int.MaxValue;

    // Event handling
    public event EventHandler<string>? TextChanged;
#pragma warning disable CS0067 // Event is never used
    public event EventHandler<string>? TextCommitted;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public MacOSText(IntPtr parentHandle, int style)
    {
        _textField = CreateNSTextField(style);
        _readOnly = (style & SWT.READ_ONLY) != 0;

        // Configure as editable text field
        var setEditableSel = sel_registerName("setEditable:");
        objc_msgSend(_textField, setEditableSel, !_readOnly);

        var setBorderedSel = sel_registerName("setBordered:");
        objc_msgSend(_textField, setBorderedSel, true);

        var setSelectableSel = sel_registerName("setSelectable:");
        objc_msgSend(_textField, setSelectableSel, true);

        var setDrawsBackgroundSel = sel_registerName("setDrawsBackground:");
        objc_msgSend(_textField, setDrawsBackgroundSel, true);

        // Handle password style
        if ((style & SWT.PASSWORD) != 0)
        {
            // Use NSSecureTextField for password fields (would need to recreate as different class)
            // For now, just mark it - proper implementation would use NSSecureTextField
        }

        if (parentHandle != IntPtr.Zero)
        {
            var addSubviewSel = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSel, _textField);
        }
    }

    public void SetText(string text)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        _text = text ?? string.Empty;
        IntPtr nsString = CreateNSString(_text);

        var setStringValueSel = sel_registerName("setStringValue:");
        objc_msgSend(_textField, setStringValueSel, nsString);

        ReleaseNSString(nsString);
        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed || _textField == IntPtr.Zero) return _text;

        var stringValueSel = sel_registerName("stringValue");
        IntPtr nsString = objc_msgSend(_textField, stringValueSel);

        if (nsString == IntPtr.Zero)
            return string.Empty;

        var utf8StringSel = sel_registerName("UTF8String");
        IntPtr utf8Ptr = objc_msgSend(nsString, utf8StringSel);

        if (utf8Ptr == IntPtr.Zero)
            return string.Empty;

#if NETSTANDARD2_0
        return MarshalUTF8String(utf8Ptr);
#else
        return Marshal.PtrToStringUTF8(utf8Ptr) ?? string.Empty;
#endif
    }

    public void SetTextLimit(int limit)
    {
        if (_disposed || _textField == IntPtr.Zero) return;
        _textLimit = limit;
        // NSTextField doesn't have built-in text limit - would need delegate to enforce
    }

    public void SetReadOnly(bool readOnly)
    {
        if (_disposed || _textField == IntPtr.Zero) return;
        _readOnly = readOnly;

        var setEditableSel = sel_registerName("setEditable:");
        objc_msgSend(_textField, setEditableSel, !readOnly);
    }

    public bool GetReadOnly()
    {
        return _readOnly;
    }

    public void SetSelection(int start, int end)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        // Get the field editor (NSText) for the text field
        var windowSel = sel_registerName("window");
        IntPtr window = objc_msgSend(_textField, windowSel);

        if (window == IntPtr.Zero) return;

        var fieldEditorSel = sel_registerName("fieldEditor:forObject:");
        IntPtr fieldEditor = objc_msgSend_fieldEditor(window, fieldEditorSel, true, _textField);

        if (fieldEditor == IntPtr.Zero) return;

        // Create NSRange
        NSRange range = new NSRange { location = (nuint)start, length = (nuint)(end - start) };

        var setSelectedRangeSel = sel_registerName("setSelectedRange:");
        objc_msgSend_range(_textField, setSelectedRangeSel, range);
    }

    public (int Start, int End) GetSelection()
    {
        if (_disposed || _textField == IntPtr.Zero) return (0, 0);

        // Get the field editor
        var windowSel = sel_registerName("window");
        IntPtr window = objc_msgSend(_textField, windowSel);

        if (window == IntPtr.Zero) return (0, 0);

        var fieldEditorSel = sel_registerName("fieldEditor:forObject:");
        IntPtr fieldEditor = objc_msgSend_fieldEditor(window, fieldEditorSel, false, _textField);

        if (fieldEditor == IntPtr.Zero) return (0, 0);

        var selectedRangeSel = sel_registerName("selectedRange");
        NSRange range = objc_msgSend_stret(fieldEditor, selectedRangeSel);

        return ((int)range.location, (int)(range.location + range.length));
    }

    public void Insert(string text)
    {
        if (_disposed || _textField == IntPtr.Zero || _readOnly) return;

        var (start, end) = GetSelection();
        string currentText = GetText();

        // Remove selected text if any
        if (start != end && start < currentText.Length)
        {
            currentText = currentText.Remove(start, Math.Min(end - start, currentText.Length - start));
        }

        // Insert new text
        if (start <= currentText.Length)
        {
            currentText = currentText.Insert(start, text ?? string.Empty);
            SetText(currentText);

            // Set cursor after inserted text
            int newPos = start + (text?.Length ?? 0);
            SetSelection(newPos, newPos);
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        var setFrameSel = sel_registerName("setFrame:");
        NSRect frame = new NSRect
        {
            origin = new NSPoint { x = x, y = y },
            size = new NSSize { width = width, height = height }
        };
        objc_msgSend_rect(_textField, setFrameSel, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _textField == IntPtr.Zero) return default;

        var frameSel = sel_registerName("frame");
        NSRect frame = objc_msgSend_stret_rect(_textField, frameSel);

        return new Rectangle(
            (int)frame.origin.x,
            (int)frame.origin.y,
            (int)frame.size.width,
            (int)frame.size.height
        );
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        var setHiddenSel = sel_registerName("setHidden:");
        objc_msgSend(_textField, setHiddenSel, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _textField == IntPtr.Zero) return false;

        var isHiddenSel = sel_registerName("isHidden");
        return !objc_msgSend_bool(_textField, isHiddenSel);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        var setEnabledSel = sel_registerName("setEnabled:");
        objc_msgSend(_textField, setEnabledSel, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _textField == IntPtr.Zero) return false;

        var isEnabledSel = sel_registerName("isEnabled");
        return objc_msgSend_bool(_textField, isEnabledSel);
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        IntPtr nsColor = CreateNSColor(color);

        var setBackgroundColorSel = sel_registerName("setBackgroundColor:");
        objc_msgSend(_textField, setBackgroundColorSel, nsColor);

        ReleaseNSColor(nsColor);
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        if (_disposed || _textField == IntPtr.Zero) return;

        IntPtr nsColor = CreateNSColor(color);

        var setTextColorSel = sel_registerName("setTextColor:");
        objc_msgSend(_textField, setTextColorSel, nsColor);

        ReleaseNSColor(nsColor);
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_textField != IntPtr.Zero)
            {
                var releaseSel = sel_registerName("release");
                objc_msgSend(_textField, releaseSel);
                _textField = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _textField;
    }

    private IntPtr CreateNSTextField(int style)
    {
        // Allocate NSTextField
        IntPtr nsTextFieldClass = objc_getClass("NSTextField");
        var allocSel = sel_registerName("alloc");
        IntPtr textField = objc_msgSend(nsTextFieldClass, allocSel);

        // Initialize with zero frame
        var initSel = sel_registerName("init");
        textField = objc_msgSend(textField, initSel);

        return textField;
    }

    private IntPtr CreateNSString(string text)
    {
        IntPtr nsStringClass = objc_getClass("NSString");
        var allocSel = sel_registerName("alloc");
        IntPtr nsString = objc_msgSend(nsStringClass, allocSel);

        var initSel = sel_registerName("initWithUTF8String:");
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text + "\0");

        unsafe
        {
            fixed (byte* ptr = utf8Bytes)
            {
                nsString = objc_msgSend(nsString, initSel, (IntPtr)ptr);
            }
        }

        return nsString;
    }

    private void ReleaseNSString(IntPtr nsString)
    {
        if (nsString != IntPtr.Zero)
        {
            var releaseSel = sel_registerName("release");
            objc_msgSend(nsString, releaseSel);
        }
    }

    private IntPtr CreateNSColor(RGB color)
    {
        IntPtr nsColorClass = objc_getClass("NSColor");
        var colorSel = sel_registerName("colorWithRed:green:blue:alpha:");

        return objc_msgSend_color(nsColorClass, colorSel,
            color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);
    }

    private void ReleaseNSColor(IntPtr nsColor)
    {
        // NSColor from factory methods are autoreleased, no need to release
    }

#if NETSTANDARD2_0
    private static string MarshalUTF8String(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0)
            length++;

        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
    }
#endif

    // P/Invoke declarations
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
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend_fieldEditor(IntPtr receiver, IntPtr selector, bool arg1, IntPtr arg2);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, NSRect arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_range(IntPtr receiver, IntPtr selector, NSRange arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern NSRange objc_msgSend_stret(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern NSRect objc_msgSend_stret_rect(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_color(IntPtr receiver, IntPtr selector,
        double red, double green, double blue, double alpha);

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRange
    {
        public nuint location;
        public nuint length;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRect
    {
        public NSPoint origin;
        public NSSize size;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSPoint
    {
        public double x;
        public double y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSSize
    {
        public double width;
        public double height;
    }
}
