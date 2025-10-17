using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - StyledText widget methods.
/// Uses RICHEDIT50W control for rich text editing.
/// </summary>
internal partial class Win32Platform
{
    // RichEdit control constants
    private const uint ES_MULTILINE = 0x0004;
    private const uint ES_WANTRETURN = 0x1000;
    private const uint ES_AUTOVSCROLL = 0x0040;
    private const uint ES_AUTOHSCROLL = 0x0080;

    // RichEdit messages
    private const int EM_SETSEL = 0x00B1;
    private const int EM_REPLACESEL = 0x00C2;
    private const int EM_GETSEL = 0x00B0;
    private const int EM_GETLINECOUNT = 0x00BA;
    private const int EM_GETLINE = 0x00C4;
    private const int EM_LINEFROMCHAR = 0x00C9;
    private const int EM_SETREADONLY = 0x00CF;
    private const int EM_SETBKGNDCOLOR = 0x0443;
    private const int EM_SETCHARFORMAT = 0x0444;
    private const int EM_GETCHARFORMAT = 0x043A;

    private const uint SCF_SELECTION = 0x0001;
    private const uint CFM_BOLD = 0x00000001;
    private const uint CFM_ITALIC = 0x00000002;
    private const uint CFM_COLOR = 0x40000000;
    private const uint CFE_BOLD = 0x00000001;
    private const uint CFE_ITALIC = 0x00000002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CHARFORMAT2
    {
        public uint cbSize;
        public uint dwMask;
        public uint dwEffects;
        public int yHeight;
        public int yOffset;
        public uint crTextColor;
        public byte bCharSet;
        public byte bPitchAndFamily;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szFaceName;
        public ushort wWeight;
        public ushort sSpacing;
        public uint crBackColor;
        public uint lcid;
        public uint dwReserved;
        public short sStyle;
        public ushort wKerning;
        public byte bUnderlineType;
        public byte bAnimation;
        public byte bRevAuthor;
        public byte bReserved1;
    }

    private class Win32StyledText : IPlatformStyledText
    {
        private readonly IntPtr _handle;
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

        public Win32StyledText(IntPtr handle)
        {
            _handle = handle;
        }

        public void SetText(string text)
        {
            _text = text ?? string.Empty;
            Win32Platform.SendMessage(_handle, WM_SETTEXT, IntPtr.Zero, _text);
        }

        public string GetText()
        {
            int length = (int)Win32Platform.SendMessage(_handle, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
            if (length == 0) return string.Empty;

            var buffer = new char[length + 1];
            // Note: This needs a different SendMessage overload - for now we'll use GetWindowText
            int copied = GetWindowTextLength(_handle);
            if (copied == 0) return string.Empty;

            var sb = new System.Text.StringBuilder(copied + 1);
            GetWindowText(_handle, sb, sb.Capacity);
            return sb.ToString();
        }

        public void SetEditable(bool editable)
        {
            _editable = editable;
            Win32Platform.SendMessage(_handle, EM_SETREADONLY, new IntPtr(editable ? 0 : 1), IntPtr.Zero);
        }

        public void Insert(string text)
        {
            if (text == null) return;
            Win32Platform.SendMessage(_handle, EM_REPLACESEL, new IntPtr(1), text);
        }

        public void ReplaceTextRange(int start, int length, string text)
        {
            SetSelection(start, start + length);
            Insert(text ?? string.Empty);
        }

        public void SetSelection(int start, int end)
        {
            Win32Platform.SendMessage(_handle, EM_SETSEL, new IntPtr(start), new IntPtr(end));
        }

        public (int Start, int End) GetSelection()
        {
            int start = 0, end = 0;
            Win32Platform.SendMessage(_handle, EM_GETSEL, ref start, ref end);
            return (start, end);
        }

        public string GetSelectionText()
        {
            var (start, end) = GetSelection();
            if (start == end) return string.Empty;

            string text = GetText();
            if (start >= 0 && end <= text.Length && end > start)
            {
                return text.Substring(start, end - start);
            }
            return string.Empty;
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
            if (range == null) return;

            // Select the range
            SetSelection(range.Start, range.Start + range.Length);

            // Create character format
            var cf = new CHARFORMAT2
            {
                cbSize = (uint)Marshal.SizeOf<CHARFORMAT2>()
            };

            if (range.Foreground != null)
            {
                cf.dwMask |= CFM_COLOR;
                cf.crTextColor = (uint)((range.Foreground.Value.Blue << 16) |
                                       (range.Foreground.Value.Green << 8) |
                                       range.Foreground.Value.Red);
            }

            if ((range.FontStyle & SWT.BOLD) != 0)
            {
                cf.dwMask |= CFM_BOLD;
                cf.dwEffects |= CFE_BOLD;
            }

            if ((range.FontStyle & SWT.ITALIC) != 0)
            {
                cf.dwMask |= CFM_ITALIC;
                cf.dwEffects |= CFE_ITALIC;
            }

            Win32Platform.SendMessage(_handle, EM_SETCHARFORMAT, new IntPtr(SCF_SELECTION), ref cf);
        }

        public string GetLine(int lineIndex)
        {
            int lineCount = GetLineCount();
            if (lineIndex < 0 || lineIndex >= lineCount) return string.Empty;

            var buffer = new char[1024];
            buffer[0] = (char)buffer.Length; // First word is buffer size

            int length = (int)Win32Platform.SendMessage(_handle, EM_GETLINE, new IntPtr(lineIndex), buffer);
            return new string(buffer, 0, length);
        }

        public int GetLineCount()
        {
            return (int)Win32Platform.SendMessage(_handle, EM_GETLINECOUNT, IntPtr.Zero, IntPtr.Zero);
        }

        public void Copy()
        {
            Win32Platform.SendMessage(_handle, WM_COPY, IntPtr.Zero, IntPtr.Zero);
        }

        public void Cut()
        {
            if (!_editable) return;
            Win32Platform.SendMessage(_handle, WM_CUT, IntPtr.Zero, IntPtr.Zero);
        }

        public void Paste()
        {
            if (!_editable) return;
            Win32Platform.SendMessage(_handle, WM_PASTE, IntPtr.Zero, IntPtr.Zero);
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

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            Win32Platform.SetWindowPos(_handle, IntPtr.Zero, x, y, width, height, 0x0004 | 0x0010);
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _handle == IntPtr.Zero) return default;
            RECT rect;
            Win32Platform.GetWindowRect(_handle, out rect);
            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            Win32Platform.ShowWindow(_handle, visible ? 5 : 0);
        }

        public bool GetVisible()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return Win32Platform.IsWindowVisible(_handle);
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            Win32Platform.EnableWindow(_handle, enabled);
        }

        public bool GetEnabled()
        {
            if (_disposed || _handle == IntPtr.Zero) return false;
            return Win32Platform.IsWindowEnabled(_handle);
        }

        public void SetBackground(RGB color)
        {
            // Edit control background typically controlled by system
        }

        public RGB GetBackground()
        {
            return new RGB(255, 255, 255); // Default white
        }

        public void SetForeground(RGB color)
        {
            // Edit control text color typically controlled by system
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 0); // Default black
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_handle != IntPtr.Zero)
                {
                    Win32Platform.DestroyWindow(_handle);
                }
                _disposed = true;
            }
        }
    }

    public IPlatformStyledText CreateStyledTextWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        uint windowStyle = WS_CHILD | WS_VISIBLE | ES_MULTILINE | ES_AUTOVSCROLL;

        if ((style & SWT.WRAP) != 0)
        {
            // Text wrapping is default for multiline, no horizontal scroll
        }
        else
        {
            windowStyle |= ES_AUTOHSCROLL;
        }

        if ((style & SWT.H_SCROLL) != 0)
        {
            windowStyle |= WS_HSCROLL;
        }

        if ((style & SWT.V_SCROLL) != 0)
        {
            windowStyle |= WS_VSCROLL;
        }

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        // Load RichEdit library if not already loaded
        LoadRichEditLibrary();

        IntPtr handle = CreateWindowEx(
            0,
            "RICHEDIT50W",
            string.Empty,
            windowStyle,
            0, 0, 100, 100,
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create styled text control. Error: {error}");
        }

        var styledTextWidget = new Win32StyledText(handle);
        _styledTextWidgets[handle] = styledTextWidget;

        if ((style & SWT.READ_ONLY) != 0)
        {
            styledTextWidget.SetEditable(false);
        }

        return styledTextWidget;
    }

    private Dictionary<IntPtr, Win32StyledText> _styledTextWidgets = new Dictionary<IntPtr, Win32StyledText>();
    private static bool _richEditLoaded = false;

    private void LoadRichEditLibrary()
    {
        if (!_richEditLoaded)
        {
            IntPtr hLib = LoadLibrary("Msftedit.dll");
            if (hLib == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to load RichEdit library (Msftedit.dll)");
            }
            _richEditLoaded = true;
        }
    }

#if NET8_0_OR_GREATER
    [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr LoadLibrary(string lpFileName);
#else
    [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);
#endif

    // StringBuilder not supported by LibraryImport - use DllImport for all targets
    [DllImport(User32, EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
    private static partial int GetWindowTextLength(IntPtr hWnd);
#else
    [DllImport(User32, EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);
#endif

    // CHARFORMAT2 struct not supported by LibraryImport - use DllImport for all targets
    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, ref int wParam, ref int lParam);

    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, char[] lParam);

    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref CHARFORMAT2 lParam);
}
