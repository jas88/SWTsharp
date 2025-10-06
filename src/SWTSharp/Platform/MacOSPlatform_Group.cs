using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Group widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Group widget selectors and constants
    private IntPtr _nsBoxClass;
    private IntPtr _selSetTitlePosition;
    private IntPtr _selSetBorderType;

    // NSBox title positions
    private const int NSNoTitle = 0;
    private const int NSAboveTop = 1;
    private const int NSAtTop = 2;
    private const int NSBelowTop = 3;

    // NSBox border types
    private const int NSNoBorder = 0;
    private const int NSLineBorder = 1;
    private const int NSBezelBorder = 2;
    private const int NSGrooveBorder = 3;

    private void InitializeGroupSelectors()
    {
        if (_nsBoxClass == IntPtr.Zero)
        {
            _nsBoxClass = objc_getClass("NSBox");
            _selSetTitlePosition = sel_registerName("setTitlePosition:");
            _selSetBorderType = sel_registerName("setBorderType:");
        }
    }

    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        InitializeGroupSelectors();

        // Create NSBox
        IntPtr box = objc_msgSend(_nsBoxClass, _selAlloc);
        box = objc_msgSend(box, _selInit);

        // Set title
        if (!string.IsNullOrEmpty(text))
        {
            IntPtr nsTitle = CreateNSString(text);
            objc_msgSend(box, _selSetTitle, nsTitle);
            objc_msgSend(box, _selSetTitlePosition, new IntPtr(NSAtTop));
        }
        else
        {
            objc_msgSend(box, _selSetTitlePosition, new IntPtr(NSNoTitle));
        }

        // Set border type
        objc_msgSend(box, _selSetBorderType, new IntPtr(NSLineBorder));

        // Set default frame
        var frame = new CGRect(0, 0, 200, 100);
        objc_msgSend_rect(box, _selSetFrame, frame);

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, box);
        }

        return box;
    }

    public void SetGroupText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeGroupSelectors();

        if (!string.IsNullOrEmpty(text))
        {
            IntPtr nsTitle = CreateNSString(text);
            objc_msgSend(handle, _selSetTitle, nsTitle);
            objc_msgSend(handle, _selSetTitlePosition, new IntPtr(NSAtTop));
        }
        else
        {
            objc_msgSend(handle, _selSetTitlePosition, new IntPtr(NSNoTitle));
        }
    }
}
