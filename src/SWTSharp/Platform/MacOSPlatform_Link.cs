using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Link widget methods.
/// Uses NSTextField with attributed string for hyperlink display.
/// </summary>
internal partial class MacOSPlatform
{
    private class MacOSLink : IPlatformLink
    {
        private readonly MacOSPlatform _platform;
        private readonly IntPtr _handle;
        private string _text = string.Empty;
        private bool _disposed;

        public event EventHandler<string>? LinkClicked;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public MacOSLink(MacOSPlatform platform, IntPtr handle)
        {
            _platform = platform;
            _handle = handle;
        }

        public void SetText(string text)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            _text = text ?? string.Empty;
            // Convert to attributed string with link
            IntPtr attrString = CreateAttributedString(_text);
            objc_msgSend(_handle, sel_registerName("setAttributedStringValue:"), attrString);
            objc_msgSend(attrString, sel_registerName("release"));
        }

        public string GetText()
        {
            return _text;
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            CGRect frame = new CGRect(x, y, width, height);
            objc_msgSend_rect(_handle, sel_registerName("setFrame:"), frame);
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _handle == IntPtr.Zero) return default;
            CGRect frame;
            objc_msgSend_stret(out frame, _handle, sel_registerName("frame"));
            return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            objc_msgSend_void(_handle, sel_registerName("setHidden:"), !visible);
        }

        public bool GetVisible()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return !objc_msgSend_bool(_handle, sel_registerName("isHidden"), IntPtr.Zero);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            objc_msgSend_void(_handle, sel_registerName("setEnabled:"), enabled);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return objc_msgSend_bool(_handle, sel_registerName("isEnabled"), IntPtr.Zero);
        }

        public void SetBackground(RGB color)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            IntPtr nsColor = CreateNSColor(color);
            objc_msgSend(_handle, sel_registerName("setBackgroundColor:"), nsColor);
        }

        public RGB GetBackground()
        {
            return new RGB(255, 255, 255); // Default white
        }

        public void SetForeground(RGB color)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            IntPtr nsColor = CreateNSColor(color);
            objc_msgSend(_handle, sel_registerName("setTextColor:"), nsColor);
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 255); // Default link blue
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle != IntPtr.Zero)
                {
                    objc_msgSend(_handle, sel_registerName("release"));
                }
                _disposed = true;
            }
        }

        private IntPtr CreateAttributedString(string text)
        {
            // Create NSAttributedString with link attributes
            IntPtr nsString = CreateNSString(text);
            IntPtr attrString = objc_msgSend(objc_msgSend(objc_getClass("NSMutableAttributedString"), sel_registerName("alloc")), sel_registerName("initWithString:"), nsString);
            objc_msgSend(nsString, sel_registerName("release"));

            // Add link attribute if text contains <a> tags
            // Simplified implementation - just make the whole text a link
            IntPtr linkClass = objc_getClass("NSURL");
            IntPtr url = objc_msgSend(linkClass, sel_registerName("URLWithString:"), CreateNSString("#"));
            IntPtr linkAttrName = CreateNSString("NSLink");
            objc_msgSend(attrString, sel_registerName("addAttribute:value:range:"), linkAttrName, url);

            return attrString;
        }

        private IntPtr CreateNSString(string text)
        {
            IntPtr nsStringClass = objc_getClass("NSString");
            IntPtr nsString = objc_msgSend(nsStringClass, sel_registerName("alloc"));
            byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text + "\0");
            unsafe
            {
                fixed (byte* ptr = utf8Bytes)
                {
                    nsString = objc_msgSend(nsString, sel_registerName("initWithUTF8String:"), (IntPtr)ptr);
                }
            }
            return nsString;
        }

        private IntPtr CreateNSColor(RGB color)
        {
            IntPtr nsColorClass = objc_getClass("NSColor");
            return objc_msgSend_color(nsColorClass, sel_registerName("colorWithRed:green:blue:alpha:"),
                color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);
        }

        internal void OnLinkClicked(string linkId)
        {
            LinkClicked?.Invoke(this, linkId);
        }
    }

    public IPlatformLink CreateLinkWidget(IPlatformWidget? parent, int style)
    {
        IntPtr nsTextField = objc_msgSend(objc_msgSend(objc_getClass("NSTextField"), sel_registerName("alloc")), sel_registerName("init"));

        // Make it non-editable and non-selectable, but allow links
        objc_msgSend_void(nsTextField, sel_registerName("setEditable:"), false);
        objc_msgSend_void(nsTextField, sel_registerName("setSelectable:"), true);
        objc_msgSend_void(nsTextField, sel_registerName("setBezeled:"), false);
        objc_msgSend_void(nsTextField, sel_registerName("setDrawsBackground:"), false);
        objc_msgSend_void(nsTextField, sel_registerName("setAllowsEditingTextAttributes:"), true);

        var linkWidget = new MacOSLink(this, nsTextField);
        _linkWidgets[nsTextField] = linkWidget;

        if (parent != null)
        {
            IntPtr parentHandle = MacOSPlatformHelpers.GetParentHandle(parent);
            if (parentHandle != IntPtr.Zero)
            {
                objc_msgSend(parentHandle, sel_registerName("addSubview:"), nsTextField);
            }
        }

        return linkWidget;
    }

    private Dictionary<IntPtr, MacOSLink> _linkWidgets = new Dictionary<IntPtr, MacOSLink>();
}
