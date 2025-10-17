using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - StyledText widget methods.
/// Uses NSTextView for rich text editing.
/// </summary>
internal partial class MacOSPlatform
{
    private class MacOSStyledText : IPlatformStyledText
    {
        private readonly MacOSPlatform _platform;
        private readonly IntPtr _handle;
        private readonly IntPtr _scrollView;
        private string _text = string.Empty;
        private bool _editable = true;
        private bool _disposed;

        public event EventHandler<string>? TextChanged;
        public event EventHandler<int>? SelectionChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public MacOSStyledText(MacOSPlatform platform, IntPtr handle, IntPtr scrollView)
        {
            _platform = platform;
            _handle = handle;
            _scrollView = scrollView;
        }

        public void SetText(string text)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            _text = text ?? string.Empty;
            IntPtr nsString = CreateNSString(_text);
            objc_msgSend(_handle, sel_registerName("setString:"), nsString);
            objc_msgSend(nsString, sel_registerName("release"));
        }

        public string GetText()
        {
            if (_disposed || _handle == IntPtr.Zero) return _text;
            IntPtr nsString = objc_msgSend(_handle, sel_registerName("string"));
            return GetNSString(nsString);
        }

        public void SetEditable(bool editable)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            _editable = editable;
            objc_msgSend_void(_handle, sel_registerName("setEditable:"), editable);
        }

        public void Insert(string text)
        {
            if (_disposed || _handle == IntPtr.Zero || text == null) return;
            IntPtr nsString = CreateNSString(text);
            objc_msgSend(_handle, sel_registerName("insertText:"), nsString);
            objc_msgSend(nsString, sel_registerName("release"));
        }

        public void ReplaceTextRange(int start, int length, string text)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            IntPtr nsString = CreateNSString(text ?? string.Empty);
            // NSTextView doesn't have direct replaceCharactersInRange - would need NSTextStorage
            objc_msgSend(nsString, sel_registerName("release"));
        }

        public void SetSelection(int start, int end)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            // Create NSRange and set selected range
            // For now, simplified - proper implementation would use NSRange struct
        }

        public (int Start, int End) GetSelection()
        {
            if (_disposed || _handle == IntPtr.Zero) return (0, 0);
            // NSRange range = objc_msgSend_stret(_handle, sel_selectedRange);
            return (0, 0); // Simplified
        }

        public string GetSelectionText()
        {
            var (start, end) = GetSelection();
            if (start == end) return string.Empty;
            string text = GetText();
            if (start >= text.Length) return string.Empty;
            int len = Math.Min(end - start, text.Length - start);
            return text.Substring(start, len);
        }

        public void SetCaretOffset(int offset)
        {
            SetSelection(offset, offset);
        }

        public int GetCaretOffset()
        {
            var (start, _) = GetSelection();
            return start;
        }

        public void SetStyleRange(StyleRange range)
        {
            if (_disposed || _handle == IntPtr.Zero || range == null) return;
            // Simplified - would need NSTextStorage for full implementation
        }

        public string GetLine(int lineIndex)
        {
            string text = GetText();
            var lines = text.Split('\n');
            return lineIndex >= 0 && lineIndex < lines.Length ? lines[lineIndex] : string.Empty;
        }

        public int GetLineCount()
        {
            string text = GetText();
            return text.Split('\n').Length;
        }

        public void Copy()
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            objc_msgSend(_handle, sel_registerName("copy:"), _handle);
        }

        public void Cut()
        {
            if (_disposed || _handle == IntPtr.Zero || !_editable) return;
            objc_msgSend(_handle, sel_registerName("cut:"), _handle);
        }

        public void Paste()
        {
            if (_disposed || _handle == IntPtr.Zero || !_editable) return;
            objc_msgSend(_handle, sel_registerName("paste:"), _handle);
        }

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _scrollView == IntPtr.Zero) return;
            CGRect frame = new CGRect(x, y, width, height);
            objc_msgSend_rect(_scrollView, sel_registerName("setFrame:"), frame);
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _scrollView == IntPtr.Zero) return default;
            CGRect frame;
            objc_msgSend_stret(out frame, _scrollView, sel_registerName("frame"));
            return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _scrollView == IntPtr.Zero) return;
            objc_msgSend_void(_scrollView, sel_registerName("setHidden:"), !visible);
        }

        public bool GetVisible()
        {
            if (_disposed || _scrollView == IntPtr.Zero) return false;
            return !objc_msgSend_bool(_scrollView, sel_registerName("isHidden"), IntPtr.Zero);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            objc_msgSend_void(_handle, sel_registerName("setEditable:"), enabled && _editable);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return objc_msgSend_bool(_handle, sel_registerName("isEditable"), IntPtr.Zero);
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
            return new RGB(0, 0, 0); // Default black
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_scrollView != IntPtr.Zero)
                {
                    objc_msgSend(_scrollView, sel_registerName("release"));
                }
                if (_handle != IntPtr.Zero)
                {
                    objc_msgSend(_handle, sel_registerName("release"));
                }
                _disposed = true;
            }
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

        private string GetNSString(IntPtr nsString)
        {
            if (nsString == IntPtr.Zero) return string.Empty;
            IntPtr utf8Ptr = objc_msgSend(nsString, sel_registerName("UTF8String"));
            if (utf8Ptr == IntPtr.Zero) return string.Empty;
#if NETSTANDARD2_0
            return MarshalUTF8String(utf8Ptr);
#else
            return Marshal.PtrToStringUTF8(utf8Ptr) ?? string.Empty;
#endif
        }

#if NETSTANDARD2_0
        private static string MarshalUTF8String(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return string.Empty;
            int length = 0;
            while (Marshal.ReadByte(ptr, length) != 0) length++;
            if (length == 0) return string.Empty;
            byte[] buffer = new byte[length];
            Marshal.Copy(ptr, buffer, 0, length);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }
#endif

        private IntPtr CreateNSColor(RGB color)
        {
            IntPtr nsColorClass = objc_getClass("NSColor");
            return objc_msgSend_color(nsColorClass, sel_registerName("colorWithRed:green:blue:alpha:"),
                color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);
        }

        internal void OnTextChanged()
        {
            _text = GetText();
            TextChanged?.Invoke(this, _text);
        }

        internal void OnSelectionChanged()
        {
            SelectionChanged?.Invoke(this, 0);
        }
    }

    public IPlatformStyledText CreateStyledTextWidget(IPlatformWidget? parent, int style)
    {
        // Create NSScrollView to contain the NSTextView
        IntPtr scrollView = objc_msgSend(objc_msgSend(objc_getClass("NSScrollView"), sel_registerName("alloc")), sel_registerName("init"));

        // Configure scroll view
        objc_msgSend_void(scrollView, sel_registerName("setHasVerticalScroller:"), (style & SWT.V_SCROLL) != 0);
        objc_msgSend_void(scrollView, sel_registerName("setHasHorizontalScroller:"), (style & SWT.H_SCROLL) != 0);
        objc_msgSend_void(scrollView, sel_registerName("setAutohidesScrollers:"), true);

        // Create NSTextView
        IntPtr textView = objc_msgSend(objc_msgSend(objc_getClass("NSTextView"), sel_registerName("alloc")), sel_registerName("init"));

        // Configure text view
        objc_msgSend_void(textView, sel_registerName("setEditable:"), (style & SWT.READ_ONLY) == 0);
        objc_msgSend_void(textView, sel_registerName("setRichText:"), true);

        // Add text view to scroll view
        objc_msgSend(scrollView, sel_registerName("setDocumentView:"), textView);

        var styledTextWidget = new MacOSStyledText(this, textView, scrollView);
        _styledTextWidgets[textView] = styledTextWidget;

        if (parent != null)
        {
            IntPtr parentHandle = IntPtr.Zero;
            if (parent is MacOS.MacOSWidget macOSWidget)
            {
                parentHandle = macOSWidget.GetNativeHandle();
            }
            if (parentHandle != IntPtr.Zero)
            {
                objc_msgSend(parentHandle, sel_registerName("addSubview:"), scrollView);
            }
        }

        return styledTextWidget;
    }

    private Dictionary<IntPtr, MacOSStyledText> _styledTextWidgets = new Dictionary<IntPtr, MacOSStyledText>();
}
