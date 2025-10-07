using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS Label implementation - partial class extension
/// </summary>
internal partial class MacOSPlatform
{
    // Label operations
    public IntPtr CreateLabel(IntPtr parent, int style, int alignment, bool wrap)
    {
        // Use NSTextField with non-editable and non-selectable properties for labels
        // For separators, use NSBox with separator style

        if ((style & SWT.SEPARATOR) != 0)
        {
            // Create NSBox for separator
            IntPtr nsBoxClass = objc_getClass("NSBox");
            IntPtr separator = objc_msgSend(objc_msgSend(nsBoxClass, _selAlloc), _selInit);

            // Set box type to separator (NSBoxSeparator = 2)
            IntPtr selSetBoxType = sel_registerName("setBoxType:");
            objc_msgSend(separator, selSetBoxType, (IntPtr)2);

            return separator;
        }

        // Create NSTextField for text label
        IntPtr nsTextFieldClass = objc_getClass("NSTextField");
        IntPtr label = objc_msgSend(objc_msgSend(nsTextFieldClass, _selAlloc), _selInit);

        // Make it non-editable and non-selectable (label behavior)
        IntPtr selSetEditable = sel_registerName("setEditable:");
        IntPtr selSetSelectable = sel_registerName("setSelectable:");
        IntPtr selSetBezeled = sel_registerName("setBezeled:");
        IntPtr selSetDrawsBackground = sel_registerName("setDrawsBackground:");

        objc_msgSend_void(label, selSetEditable, false);
        objc_msgSend_void(label, selSetSelectable, false);
        objc_msgSend_void(label, selSetBezeled, false);
        objc_msgSend_void(label, selSetDrawsBackground, false);

        // Set alignment
        IntPtr selSetAlignment = sel_registerName("setAlignment:");
        int nsAlignment = alignment == SWT.CENTER ? 2 : (alignment == SWT.RIGHT ? 1 : 0);
        objc_msgSend(label, selSetAlignment, (IntPtr)nsAlignment);

        // Set wrapping behavior
        if (wrap)
        {
            IntPtr selSetLineBreakMode = sel_registerName("setLineBreakMode:");
            objc_msgSend(label, selSetLineBreakMode, (IntPtr)0); // NSLineBreakByWordWrapping
        }

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            // Lazy initialize _selAddSubview if not already done
            if (_selAddSubview == IntPtr.Zero)
            {
                _selAddSubview = sel_registerName("addSubview:");
            }

            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, label);
        }

        return label;
    }

    public void SetLabelText(IntPtr handle, string text)
    {
        IntPtr nsString = CreateNSString(text ?? string.Empty);
        IntPtr selSetStringValue = sel_registerName("setStringValue:");
        objc_msgSend(handle, selSetStringValue, nsString);
    }

    public void SetLabelAlignment(IntPtr handle, int alignment)
    {
        IntPtr selSetAlignment = sel_registerName("setAlignment:");
        // NSTextAlignment: 0=left, 1=right, 2=center
        int nsAlignment = alignment == SWT.CENTER ? 2 : (alignment == SWT.RIGHT ? 1 : 0);
        objc_msgSend(handle, selSetAlignment, (IntPtr)nsAlignment);
    }
}
