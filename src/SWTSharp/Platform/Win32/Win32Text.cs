using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of an editable text widget using EDIT control.
/// </summary>
#if NET7_0_OR_GREATER
internal partial class Win32Text : IPlatformTextInput
#else
internal class Win32Text : IPlatformTextInput
#endif
{
    private IntPtr _hwnd;
    private bool _disposed;
    private string _text = string.Empty;
    private bool _readOnly;
    private int _textLimit = 32767; // Default Windows EDIT control limit

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

    // Windows constants
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_TABSTOP = 0x00010000;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;

    private const uint ES_LEFT = 0x0000;
    private const uint ES_CENTER = 0x0001;
    private const uint ES_RIGHT = 0x0002;
    private const uint ES_MULTILINE = 0x0004;
    private const uint ES_UPPERCASE = 0x0008;
    private const uint ES_LOWERCASE = 0x0010;
    private const uint ES_PASSWORD = 0x0020;
    private const uint ES_AUTOVSCROLL = 0x0040;
    private const uint ES_AUTOHSCROLL = 0x0080;
    private const uint ES_NOHIDESEL = 0x0100;
    private const uint ES_READONLY = 0x0800;
    private const uint ES_WANTRETURN = 0x1000;

    private const uint EM_SETSEL = 0x00B1;
    private const uint EM_GETSEL = 0x00B0;
    private const uint EM_LIMITTEXT = 0x00C5;
    private const uint EM_SETREADONLY = 0x00CF;
    private const uint EM_REPLACESEL = 0x00C2;

    private const uint WM_GETTEXT = 0x000D;
    private const uint WM_GETTEXTLENGTH = 0x000E;
    private const uint WM_SETTEXT = 0x000C;

    public Win32Text(IntPtr parentHandle, int style)
    {
        _hwnd = CreateEditControl(parentHandle, style);
        _readOnly = (style & SWT.READ_ONLY) != 0;

        if (_hwnd == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create Win32 EDIT control");
        }
    }

    public void SetText(string text)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _text = text ?? string.Empty;

        // Enforce text limit
        if (_text.Length > _textLimit)
        {
            _text = _text.Substring(0, _textLimit);
        }

        SendMessage(_hwnd, WM_SETTEXT, IntPtr.Zero, _text);
        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return _text;

        int length = (int)SendMessage(_hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
        if (length == 0) return string.Empty;

        // Allocate buffer for text (length + 1 for null terminator)
        char[] buffer = new char[length + 1];

        unsafe
        {
            fixed (char* pBuffer = buffer)
            {
                SendMessage(_hwnd, WM_GETTEXT, new IntPtr(buffer.Length), new IntPtr(pBuffer));
            }
        }

        return new string(buffer, 0, length);
    }

    public void SetTextLimit(int limit)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _textLimit = limit;
        SendMessage(_hwnd, EM_LIMITTEXT, new IntPtr(limit), IntPtr.Zero);
    }

    public void SetReadOnly(bool readOnly)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _readOnly = readOnly;
        SendMessage(_hwnd, EM_SETREADONLY, new IntPtr(readOnly ? 1 : 0), IntPtr.Zero);
    }

    public bool GetReadOnly()
    {
        return _readOnly;
    }

    public void SetSelection(int start, int end)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SendMessage(_hwnd, EM_SETSEL, new IntPtr(start), new IntPtr(end));
    }

    public (int Start, int End) GetSelection()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return (0, 0);

        int start = 0;
        int end = 0;

        unsafe
        {
            SendMessage(_hwnd, EM_GETSEL, new IntPtr(&start), new IntPtr(&end));
        }

        return (start, end);
    }

    public void Insert(string text)
    {
        if (_disposed || _hwnd == IntPtr.Zero || _readOnly) return;

        // EM_REPLACESEL replaces the current selection with the text
        // If no selection, it inserts at the cursor position
        SendMessage(_hwnd, EM_REPLACESEL, new IntPtr(1), text ?? string.Empty); // 1 = can undo

        _text = GetText();
        TextChanged?.Invoke(this, _text);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height,
            0x0004 | 0x0010); // SWP_NOZORDER | SWP_NOACTIVATE
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return default;

        RECT rect = new RECT();
        GetWindowRect(_hwnd, ref rect);

        return new Rectangle(rect.left, rect.top,
            rect.right - rect.left, rect.bottom - rect.top);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        ShowWindow(_hwnd, visible ? 5 : 0); // SW_SHOW = 5, SW_HIDE = 0
    }

    public bool GetVisible()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return false;

        return IsWindowVisible(_hwnd);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        EnableWindow(_hwnd, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return false;

        return IsWindowEnabled(_hwnd);
    }

    public void SetBackground(RGB color)
    {
        // EDIT control background color is typically controlled by the system
        // Would need custom WM_CTLCOLOREDIT handling in parent window
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // EDIT control text color is typically controlled by the system
        // Would need custom WM_CTLCOLOREDIT handling in parent window
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    /// <summary>
    /// Gets the native Win32 handle (HWND) for this text control.
    /// Used internally by platform code for parent-child relationships.
    /// </summary>
    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_hwnd != IntPtr.Zero)
            {
                DestroyWindow(_hwnd);
                _hwnd = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    private IntPtr CreateEditControl(IntPtr parentHandle, int style)
    {
        uint dwStyle = WS_CHILD | WS_VISIBLE | WS_BORDER | WS_TABSTOP | ES_AUTOHSCROLL | ES_LEFT;

        // Handle multi-line
        if ((style & SWT.MULTI) != 0)
        {
            dwStyle |= ES_MULTILINE | ES_AUTOVSCROLL | ES_WANTRETURN;
            dwStyle |= WS_VSCROLL;
        }

        // Handle wrap
        if ((style & SWT.WRAP) != 0)
        {
            dwStyle &= ~ES_AUTOHSCROLL; // Remove horizontal scroll for wrap
            dwStyle &= ~WS_HSCROLL;
        }

        // Handle password
        if ((style & SWT.PASSWORD) != 0)
        {
            dwStyle |= ES_PASSWORD;
        }

        // Handle read-only
        if ((style & SWT.READ_ONLY) != 0)
        {
            dwStyle |= ES_READONLY;
        }

        // Handle alignment
        if ((style & SWT.CENTER) != 0)
        {
            dwStyle = (dwStyle & ~ES_LEFT) | ES_CENTER;
        }
        else if ((style & SWT.RIGHT) != 0)
        {
            dwStyle = (dwStyle & ~ES_LEFT) | ES_RIGHT;
        }

        // Handle vertical scroll
        if ((style & SWT.V_SCROLL) != 0)
        {
            dwStyle |= WS_VSCROLL;
        }

        // Handle horizontal scroll
        if ((style & SWT.H_SCROLL) != 0)
        {
            dwStyle |= WS_HSCROLL;
        }

        return CreateWindowEx(0, "EDIT", "", dwStyle,
            0, 0, 100, 20, parentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
    }

    // P/Invoke declarations with conditional compilation

#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW",
        StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(IntPtr hWnd);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [LibraryImport("user32.dll", EntryPoint = "SendMessageW",
        StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bEnable);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsWindowEnabled(IntPtr hWnd);
#else
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW",
        CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, string lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    [DllImport("user32.dll")]
    private static extern bool IsWindowEnabled(IntPtr hWnd);
#endif

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
