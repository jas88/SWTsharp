using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Windows implementation of a label platform widget using STATIC control.
/// </summary>
#if NET7_0_OR_GREATER
internal partial class Win32Label : IPlatformTextWidget
#else
internal class Win32Label : IPlatformTextWidget
#endif
{
    private IntPtr _hwnd;
    private bool _disposed;
    private string _text = string.Empty;

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

    public Win32Label(IntPtr parentHandle, int style)
    {
        _hwnd = CreateStaticControl(parentHandle, style);
    }

    public void SetText(string text)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;

        _text = text ?? string.Empty;
        SetWindowText(_hwnd, _text);
        TextChanged?.Invoke(this, _text);
    }

    public string GetText()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return _text;

        int length = GetWindowTextLength(_hwnd);
        if (length == 0) return string.Empty;

        var buffer = new char[length + 1];
        GetWindowText(_hwnd, buffer, buffer.Length);
        return new string(buffer, 0, length);
    }

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;
        SetWindowPos(_hwnd, IntPtr.Zero, x, y, width, height, SWP_NOZORDER | SWP_NOACTIVATE);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _hwnd == IntPtr.Zero) return default;

        GetWindowRect(_hwnd, out RECT rect);
        return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _hwnd == IntPtr.Zero) return;
        ShowWindow(_hwnd, visible ? SW_SHOW : SW_HIDE);
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
        // Background color handled by parent window's WM_CTLCOLORSTATIC
    }

    public RGB GetBackground()
    {
        return new RGB(240, 240, 240); // Default button face color
    }

    public void SetForeground(RGB color)
    {
        // Text color handled by parent window's WM_CTLCOLORSTATIC
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    public IntPtr GetNativeHandle()
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

    private IntPtr CreateStaticControl(IntPtr parentHandle, int style)
    {
        uint dwStyle = WS_CHILD | WS_VISIBLE | SS_LEFT;

        // Handle alignment
        if ((style & SWT.CENTER) != 0)
            dwStyle = (dwStyle & ~SS_LEFT) | SS_CENTER;
        else if ((style & SWT.RIGHT) != 0)
            dwStyle = (dwStyle & ~SS_LEFT) | SS_RIGHT;

        // Handle separator
        if ((style & SWT.SEPARATOR) != 0)
        {
            if ((style & SWT.VERTICAL) != 0)
                dwStyle |= SS_ETCHEDVERT;
            else
                dwStyle |= SS_ETCHEDHORZ;
        }

        return CreateWindowEx(
            0,
            "STATIC",
            "",
            dwStyle,
            0, 0, 100, 20,
            parentHandle,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero
        );
    }

    // Win32 Constants
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint SS_LEFT = 0x00000000;
    private const uint SS_CENTER = 0x00000001;
    private const uint SS_RIGHT = 0x00000002;
    private const uint SS_ETCHEDHORZ = 0x00000010;
    private const uint SS_ETCHEDVERT = 0x00000011;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    // Win32 API
#if NET7_0_OR_GREATER
    [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
#else
    [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
#endif

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

    [DllImport("user32.dll", EntryPoint = "GetWindowTextLengthW", SetLastError = true)]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnableWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bEnable);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowEnabled(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
