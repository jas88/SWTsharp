using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformCombo using real NSComboBox native widget.
/// NO pseudo-handles - uses actual Objective-C objects.
/// </summary>
internal class MacOSCombo : MacOSWidget, IPlatformCombo
{
    private IntPtr _nsComboBox;
    private readonly List<string> _items = new();
    private bool _disposed;
    private int _selectionIndex = -1;
    private bool _visible = true;
    private bool _enabled = true;
    private Rectangle _bounds;

    // Event handling
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    // Objective-C runtime imports
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, long arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_long(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    // Architecture-specific struct return handling:
    // - ARM64: objc_msgSend_stret doesn't exist, use objc_msgSend with direct return
    // - x86_64: objc_msgSend_stret required for structs > 16 bytes (CGRect is 32 bytes)
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern CGRect objc_msgSend_cgrect_arm64(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret_x64(out CGRect retval, IntPtr receiver, IntPtr selector);

    private static void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector)
    {
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            retval = objc_msgSend_cgrect_arm64(receiver, selector);
        }
        else
        {
            objc_msgSend_stret_x64(out retval, receiver, selector);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double x, y, width, height;

        public CGRect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    public MacOSCombo(IntPtr parentHandle, int style)
    {
        _nsComboBox = CreateNSComboBox(style);

        if (_nsComboBox == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create NSComboBox");

        // Add to parent view if provided
        if (parentHandle != IntPtr.Zero)
        {
            var selAddSubview = sel_registerName("addSubview:");
            objc_msgSend_void(parentHandle, selAddSubview, _nsComboBox);
        }

        // Set default size
        SetBounds(0, 0, 150, 26);
    }

    private IntPtr CreateNSComboBox(int style)
    {
        // NSComboBox* combo = [[NSComboBox alloc] init];
        var comboClass = objc_getClass("NSComboBox");
        var selAlloc = sel_registerName("alloc");
        var selInit = sel_registerName("init");

        var combo = objc_msgSend(comboClass, selAlloc);
        combo = objc_msgSend(combo, selInit);

        if (combo == IntPtr.Zero)
            return IntPtr.Zero;

        // Configure combo box - respect READ_ONLY style
        var selSetEditable = sel_registerName("setEditable:");
        bool isEditable = (style & SWT.READ_ONLY) == 0;
        objc_msgSend_void(combo, selSetEditable, isEditable);

        var selSetUsesDataSource = sel_registerName("setUsesDataSource:");
        objc_msgSend_void(combo, selSetUsesDataSource, false);

        return combo;
    }

    private IntPtr CreateNSString(string text)
    {
        var strClass = objc_getClass("NSString");
        var selector = sel_registerName("stringWithUTF8String:");
        // Use UTF-8 encoding, not ANSI, for Objective-C NSString
        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes((text ?? string.Empty) + "\0");
        var utf8Ptr = Marshal.AllocHGlobal(utf8Bytes.Length);
        try
        {
            Marshal.Copy(utf8Bytes, 0, utf8Ptr, utf8Bytes.Length);
            return objc_msgSend(strClass, selector, utf8Ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(utf8Ptr);
        }
    }

    private string NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero) return string.Empty;
        var selector = sel_registerName("UTF8String");
        var utf8Ptr = objc_msgSend(nsString, selector);
        return Marshal.PtrToStringAnsi(utf8Ptr) ?? string.Empty;
    }

    public void AddItem(string item)
    {
        if (_disposed || _nsComboBox == IntPtr.Zero || string.IsNullOrEmpty(item)) return;

        _items.Add(item);

        // [combo addItemWithObjectValue:@"item"]
        var selAddItem = sel_registerName("addItemWithObjectValue:");
        var nsItem = CreateNSString(item);
        objc_msgSend_void(_nsComboBox, selAddItem, nsItem);
    }

    public void ClearItems()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        _items.Clear();
        _selectionIndex = -1;

        // [combo removeAllItems]
        var selRemoveAll = sel_registerName("removeAllItems");
        objc_msgSend_void(_nsComboBox, selRemoveAll);
    }

    public int GetItemCount()
    {
        if (_disposed) return 0;
        return _items.Count;
    }

    public string GetItemAt(int index)
    {
        if (_disposed || index < 0 || index >= _items.Count)
            return string.Empty;

        return _items[index];
    }

    public int SelectionIndex
    {
        get
        {
            if (_disposed || _nsComboBox == IntPtr.Zero) return -1;

            // Get actual selection from native control
            var selIndexOfSelectedItem = sel_registerName("indexOfSelectedItem");
            return (int)objc_msgSend_long(_nsComboBox, selIndexOfSelectedItem);
        }
        set
        {
            if (_disposed || _nsComboBox == IntPtr.Zero) return;
            if (value < -1 || value >= _items.Count) return;

            var oldIndex = _selectionIndex;
            _selectionIndex = value;

            if (value >= 0)
            {
                // [combo selectItemAtIndex:value]
                var selSelectItem = sel_registerName("selectItemAtIndex:");
                objc_msgSend_void(_nsComboBox, selSelectItem, (long)value);
            }
            else
            {
                // Deselect
                var selDeselectItem = sel_registerName("deselectItemAtIndex:");
                if (oldIndex >= 0)
                    objc_msgSend_void(_nsComboBox, selDeselectItem, (long)oldIndex);
            }

            if (oldIndex != _selectionIndex)
            {
                SelectionChanged?.Invoke(this, _selectionIndex);
            }
        }
    }

    public string Text
    {
        get
        {
            if (_disposed || _nsComboBox == IntPtr.Zero) return string.Empty;

            // [combo stringValue]
            var selStringValue = sel_registerName("stringValue");
            var nsString = objc_msgSend(_nsComboBox, selStringValue);
            return NSStringToString(nsString);
        }
        set
        {
            if (_disposed || _nsComboBox == IntPtr.Zero) return;

            // Try to find the item in the list
            int index = _items.IndexOf(value ?? string.Empty);
            if (index >= 0)
            {
                SelectionIndex = index;
            }
            else
            {
                // Set text directly for editable combo
                var selSetStringValue = sel_registerName("setStringValue:");
                var nsString = CreateNSString(value ?? string.Empty);
                objc_msgSend_void(_nsComboBox, selSetStringValue, nsString);
            }
        }
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        _bounds = new Rectangle(x, y, width, height);

        var rect = new CGRect(x, y, width, height);
        var selSetFrame = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsComboBox, selSetFrame, rect);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return default;

        var selFrame = sel_registerName("frame");
        objc_msgSend_stret(out CGRect rect, _nsComboBox, selFrame);
        return new Rectangle((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        _visible = visible;
        var selSetHidden = sel_registerName("setHidden:");
        objc_msgSend_void(_nsComboBox, selSetHidden, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return false;

        var selIsHidden = sel_registerName("isHidden");
        return !objc_msgSend_bool(_nsComboBox, selIsHidden);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        _enabled = enabled;
        var selSetEnabled = sel_registerName("setEnabled:");
        objc_msgSend_void(_nsComboBox, selSetEnabled, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return false;

        var selIsEnabled = sel_registerName("isEnabled");
        return objc_msgSend_bool(_nsComboBox, selIsEnabled);
    }

    public void SetBackground(RGB color)
    {
        // NSComboBox background color would require NSColor creation
        // Not implemented for now
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255);
    }

    public void SetForeground(RGB color)
    {
        // NSComboBox foreground color would require NSColor creation
        // Not implemented for now
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0);
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsComboBox;
    }

    public void SetTextLimit(int limit)
    {
        // NSComboBox uses NSCell which has a formatter for text limit
        // For now, this is not implemented - would need NSNumberFormatter or NSTextFieldCell
    }

    public void SetVisibleItemCount(int count)
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        // [combo setNumberOfVisibleItems:count]
        var selSetNumberOfVisibleItems = sel_registerName("setNumberOfVisibleItems:");
        objc_msgSend_void(_nsComboBox, selSetNumberOfVisibleItems, (long)count);
    }

    public void SetTextSelection(int start, int end)
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        // Get the field editor for this control
        // This is complex on macOS - would need window's field editor
        // Simplified: just select all or nothing for now
    }

    public (int Start, int End) GetTextSelection()
    {
        // Getting selection from NSComboBox requires field editor access
        return (0, 0);
    }

    public void Copy()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        // Simulate Cmd+C through responder chain
        // NSComboBox should handle this through its text field
    }

    public void Cut()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        // Simulate Cmd+X through responder chain
    }

    public void Paste()
    {
        if (_disposed || _nsComboBox == IntPtr.Zero) return;

        // Simulate Cmd+V through responder chain
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_nsComboBox != IntPtr.Zero)
        {
            // Remove from superview
            var selRemoveFromSuperview = sel_registerName("removeFromSuperview");
            objc_msgSend_void(_nsComboBox, selRemoveFromSuperview);

            // Release
            var selRelease = sel_registerName("release");
            objc_msgSend_void(_nsComboBox, selRelease);
            _nsComboBox = IntPtr.Zero;
        }

        _disposed = true;
    }
}