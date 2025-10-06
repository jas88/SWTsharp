using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation.
/// </summary>
internal partial class Win32Platform : IPlatform
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";

    // Window Styles
    private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CHILD = 0x40000000;

    // Class Styles
    private const uint CS_HREDRAW = 0x0002;
    private const uint CS_VREDRAW = 0x0001;
    private const uint CS_OWNDC = 0x0020;

    // Window Messages
    private const uint WM_QUIT = 0x0012;
    private const uint WM_DESTROY = 0x0002;
    private const uint WM_PAINT = 0x000F;
    private const uint WM_ERASEBKGND = 0x0014;

    // ShowWindow Commands
    private const int SW_SHOW = 5;
    private const int SW_HIDE = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct MSG
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public string lpszMenuName;
        public string lpszClassName;
    }

#if NET8_0_OR_GREATER
    [LibraryImport(User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y,
        int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);
#else
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x, int y,
        int nWidth, int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyWindow(IntPtr hWnd);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowText(IntPtr hWnd, string lpString);
#else
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TranslateMessage(ref MSG lpMsg);
#else
    [DllImport(User32)]
    private static extern bool TranslateMessage(ref MSG lpMsg);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr DispatchMessage(ref MSG lpMsg);
#else
    [DllImport(User32)]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial void PostQuitMessage(int nExitCode);
#else
    [DllImport(User32)]
    private static extern void PostQuitMessage(int nExitCode);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Kernel32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr GetModuleHandle(string? lpModuleName);
#else
    [DllImport(Kernel32, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
#endif

    // Note: RegisterClass uses struct marshalling which is not supported by LibraryImport in .NET 8
    // Keep using DllImport for this function
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
#else
    [DllImport(User32)]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
#endif

    private IntPtr _hInstance;
    private const string WindowClassName = "SWTSharpWindow";
    private const string CanvasClassName = "SWTSharpCanvas";
    private WndProcDelegate? _wndProcDelegate; // SEC-001: Store delegate to prevent GC
    private WndProcDelegate? _canvasWndProcDelegate; // SEC-001: Store canvas delegate to prevent GC

    public void Initialize()
    {
        _hInstance = GetModuleHandle(null);
        RegisterWindowClass();
        RegisterCanvasClass();
    }

    private void RegisterWindowClass()
    {
        // SEC-001: Store delegate reference before marshalling to prevent GC
        _wndProcDelegate = WndProc;

        var wndClass = new WNDCLASS
        {
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = _hInstance,
            lpszClassName = WindowClassName,
            hCursor = IntPtr.Zero,
            hbrBackground = new IntPtr(6) // COLOR_WINDOW + 1
        };

        RegisterClass(ref wndClass);
    }

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_DESTROY)
        {
            PostQuitMessage(0);
            return IntPtr.Zero;
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    private void RegisterCanvasClass()
    {
        // SEC-001: Store delegate reference before marshalling to prevent GC
        _canvasWndProcDelegate = CanvasWndProc;

        var wndClass = new WNDCLASS
        {
            style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_canvasWndProcDelegate),
            hInstance = _hInstance,
            lpszClassName = CanvasClassName,
            hCursor = IntPtr.Zero,
            hbrBackground = IntPtr.Zero // Don't use default background, we'll handle it
        };

        RegisterClass(ref wndClass);
    }

    private IntPtr CanvasWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_PAINT:
                if (_canvasData.TryGetValue(hWnd, out var canvasData) && canvasData.PaintCallback != null)
                {
                    BeginPaint(hWnd, out PAINTSTRUCT ps);
                    try
                    {
                        // Call the paint callback with the paint rectangle and HDC
                        int x = ps.rcPaint.Left;
                        int y = ps.rcPaint.Top;
                        int width = ps.rcPaint.Right - ps.rcPaint.Left;
                        int height = ps.rcPaint.Bottom - ps.rcPaint.Top;
                        canvasData.PaintCallback(x, y, width, height, ps.hdc);
                    }
                    finally
                    {
                        EndPaint(hWnd, ref ps);
                    }
                    return IntPtr.Zero;
                }
                break;

            case WM_ERASEBKGND:
                if (_canvasData.TryGetValue(hWnd, out var data) && data.BackgroundBrush != IntPtr.Zero)
                {
                    // Fill with background color
                    // Note: For now, we return 1 to indicate we handled erase
                    // A full implementation would use FillRect with the background brush
                    return new IntPtr(1);
                }
                break;

            case WM_DESTROY:
                // Cleanup canvas data when window is destroyed
                if (_canvasData.TryGetValue(hWnd, out var destroyData))
                {
                    if (destroyData.BackgroundBrush != IntPtr.Zero)
                    {
                        DeleteObject(destroyData.BackgroundBrush);
                    }
                    _canvasData.Remove(hWnd);
                }
                break;
        }

        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public IntPtr CreateWindow(int style, string title)
    {
        uint dwStyle = WS_OVERLAPPEDWINDOW;

        var handle = CreateWindowEx(
            0,
            WindowClassName,
            title,
            dwStyle,
            100, 100,  // x, y
            800, 600,  // width, height
            IntPtr.Zero,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        return handle;
    }

    void IPlatform.DestroyWindow(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            DestroyWindow(handle);
        }
    }

    public void SetWindowVisible(IntPtr handle, bool visible)
    {
        ShowWindow(handle, visible ? SW_SHOW : SW_HIDE);
    }

    void IPlatform.SetWindowText(IntPtr handle, string text)
    {
        SetWindowText(handle, text);
    }

    // SetWindowPos flags - defined at class level for reuse
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;

    public void SetWindowSize(IntPtr handle, int width, int height)
    {
        SetWindowPos(handle, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_NOZORDER);
    }

    public void SetWindowLocation(IntPtr handle, int x, int y)
    {
        SetWindowPos(handle, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
    }

    public bool ProcessEvent()
    {
        const uint PM_REMOVE = 0x0001;
        if (PeekMessage(out MSG msg, IntPtr.Zero, 0, 0, PM_REMOVE))
        {
            if (msg.message == WM_QUIT)
            {
                return false;
            }

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
            return true;
        }
        return false;
    }

    public void WaitForEvent()
    {
        GetMessage(out MSG msg, IntPtr.Zero, 0, 0);
        TranslateMessage(ref msg);
        DispatchMessage(ref msg);
    }

    public void WakeEventLoop()
    {
        // Post a null message to wake up GetMessage
        PostQuitMessage(0);
    }

    public IntPtr CreateComposite(int style)
    {
        // Create a child window that acts as a container
        // Use WS_CHILD style for composites
        uint windowStyle = WS_CHILD | WS_VISIBLE;

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        IntPtr hInstance = GetModuleHandle(null);
        IntPtr hwnd = CreateWindowEx(
            0,                      // Extended style
            "STATIC",               // Using STATIC class as container
            string.Empty,           // No title
            windowStyle,
            0, 0,                   // Position
            100, 100,               // Default size
            IntPtr.Zero,            // No parent yet
            IntPtr.Zero,            // No menu
            hInstance,
            IntPtr.Zero);

        return hwnd;
    }

    // Button-specific constants
    private const uint BS_PUSHBUTTON = 0x00000000;
    private const uint BS_DEFPUSHBUTTON = 0x00000001;
    private const uint BS_CHECKBOX = 0x00000002;
    private const uint BS_AUTOCHECKBOX = 0x00000003;
    private const uint BS_RADIOBUTTON = 0x00000004;
    private const uint BS_AUTORADIOBUTTON = 0x00000009;
    private const uint BS_3STATE = 0x00000005;
    private const uint BS_AUTO3STATE = 0x00000006;

    // Button messages
    private const uint BM_SETCHECK = 0x00F1;
    private const uint BM_GETCHECK = 0x00F0;
    private const uint BM_CLICK = 0x00F5;

    // Button notification codes
    private const uint BN_CLICKED = 0;

    // Button check states
    private const int BST_UNCHECKED = 0x0000;
    private const int BST_CHECKED = 0x0001;

    // Control messages
    private const uint WM_COMMAND = 0x0111;
    private const uint WM_SETTEXT = 0x000C;
    private const uint WM_ENABLE = 0x000A;

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
#else
    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
#else
    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableWindow(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool enable);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool EnableWindow(IntPtr hWnd, bool enable);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, [MarshalAs(UnmanagedType.Bool)] bool repaint);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);
#endif

    // Canvas/Paint related structures and functions
    [StructLayout(LayoutKind.Sequential)]
    private struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public RECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] rgbReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // Note: BeginPaint/EndPaint use DllImport (not LibraryImport) due to struct marshalling
    [DllImport(User32)]
    private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

    [DllImport(User32)]
    private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    // InvalidateRect with RECT parameter - IntPtr version already in Win32Platform_Label.cs
    [DllImport(User32, SetLastError = true)]
    private static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

    // CreateSolidBrush - using Gdi32 constant from Win32PlatformGraphics.cs
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateSolidBrush(uint color);

    private readonly Dictionary<IntPtr, Action> _buttonCallbacks = new Dictionary<IntPtr, Action>();

    // Canvas data structure
    private class CanvasData
    {
        public Action<int, int, int, int, object?>? PaintCallback { get; set; }
        public Graphics.RGB BackgroundColor { get; set; }
        public IntPtr BackgroundBrush { get; set; }
    }

    private readonly Dictionary<IntPtr, CanvasData> _canvasData = new Dictionary<IntPtr, CanvasData>();

    // LEAK-002: Cleanup method for button callbacks
    public void ClearButtonCallbacks()
    {
        _buttonCallbacks.Clear();
    }

    // LEAK-002: Remove specific button callback when control is destroyed
    public void RemoveButtonCallback(IntPtr handle)
    {
        _buttonCallbacks.Remove(handle);
    }

    public IntPtr CreateButton(IntPtr parent, int style, string text)
    {
        uint buttonStyle = WS_CHILD | WS_VISIBLE;

        // Determine button type from SWT style
        if ((style & SWT.CHECK) != 0)
        {
            buttonStyle |= BS_AUTOCHECKBOX;
        }
        else if ((style & SWT.RADIO) != 0)
        {
            buttonStyle |= BS_AUTORADIOBUTTON;
        }
        else if ((style & SWT.TOGGLE) != 0)
        {
            buttonStyle |= BS_AUTOCHECKBOX; // Toggle behaves like checkbox
        }
        else if ((style & SWT.ARROW) != 0)
        {
            buttonStyle |= BS_PUSHBUTTON; // Arrow buttons need custom drawing
        }
        else // Default to PUSH
        {
            buttonStyle |= BS_PUSHBUTTON;
        }

        var handle = CreateWindowEx(
            0,
            "BUTTON",
            text,
            buttonStyle,
            0, 0, 100, 30, // Default size
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        return handle;
    }

    public void SetButtonText(IntPtr handle, string text)
    {
        SendMessage(handle, WM_SETTEXT, IntPtr.Zero, text);
    }

    public void SetButtonSelection(IntPtr handle, bool selected)
    {
        SendMessage(handle, BM_SETCHECK, new IntPtr(selected ? BST_CHECKED : BST_UNCHECKED), IntPtr.Zero);
    }

    public bool GetButtonSelection(IntPtr handle)
    {
        IntPtr result = SendMessage(handle, BM_GETCHECK, IntPtr.Zero, IntPtr.Zero);
        return result.ToInt32() == BST_CHECKED;
    }

    public void SetControlEnabled(IntPtr handle, bool enabled)
    {
        EnableWindow(handle, enabled);
    }

    public void SetControlVisible(IntPtr handle, bool visible)
    {
        ShowWindow(handle, visible ? SW_SHOW : SW_HIDE);
    }

    public void SetControlBounds(IntPtr handle, int x, int y, int width, int height)
    {
        MoveWindow(handle, x, y, width, height, true);
    }

    public void ConnectButtonClick(IntPtr handle, Action callback)
    {
        // Store callback for later invocation
        _buttonCallbacks[handle] = callback;

        // In Win32, button clicks come through WM_COMMAND messages
        // This would need proper subclassing or a window procedure hook
        // For now, we store the callback - full implementation would require
        // hooking into the parent window's message loop
    }

    // Menu operations
#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    private static partial IntPtr CreateMenu();
#else
    [DllImport(User32, SetLastError = true)]
    private static extern IntPtr CreateMenu();
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    private static partial IntPtr CreatePopupMenu();
#else
    [DllImport(User32, SetLastError = true)]
    private static extern IntPtr CreatePopupMenu();
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DestroyMenu(IntPtr hMenu);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool DestroyMenu(IntPtr hMenu);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetMenu(IntPtr hWnd, IntPtr hMenu);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool SetMenu(IntPtr hWnd, IntPtr hMenu);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AppendMenu(IntPtr hMenu, uint uFlags, UIntPtr uIDNewItem, string lpNewItem);
#else
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool AppendMenu(IntPtr hMenu, uint uFlags, UIntPtr uIDNewItem, string lpNewItem);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool InsertMenu(IntPtr hMenu, uint uPosition, uint uFlags, UIntPtr uIDNewItem, string lpNewItem);
#else
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool InsertMenu(IntPtr hMenu, uint uPosition, uint uFlags, UIntPtr uIDNewItem, string lpNewItem);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CheckMenuItem(IntPtr hMenu, uint uIDCheckItem, uint uCheck);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool CheckMenuItem(IntPtr hMenu, uint uIDCheckItem, uint uCheck);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);
#endif

    // Menu flags
    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint MF_POPUP = 0x00000010;
    private const uint MF_CHECKED = 0x00000008;
    private const uint MF_UNCHECKED = 0x00000000;
    private const uint MF_BYCOMMAND = 0x00000000;
    private const uint MF_BYPOSITION = 0x00000400;
    private const uint MF_ENABLED = 0x00000000;
    private const uint MF_GRAYED = 0x00000001;
    private const uint MF_DISABLED = 0x00000002;

    // TrackPopupMenu flags
    private const uint TPM_LEFTALIGN = 0x0000;
    private const uint TPM_RETURNCMD = 0x0100;

    IntPtr IPlatform.CreateMenu(int style)
    {
        if ((style & SWT.BAR) != 0)
        {
            return CreateMenu();
        }
        else
        {
            return CreatePopupMenu();
        }
    }

    void IPlatform.DestroyMenu(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            DestroyMenu(handle);
        }
    }

    void IPlatform.SetShellMenuBar(IntPtr shellHandle, IntPtr menuHandle)
    {
        SetMenu(shellHandle, menuHandle);
    }

    void IPlatform.SetMenuVisible(IntPtr handle, bool visible)
    {
        // Menus don't have visibility state on Win32
        // Visibility is controlled by showing/hiding the window or popup
    }

    void IPlatform.ShowPopupMenu(IntPtr menuHandle, int x, int y)
    {
        // For popup menus, we need a window handle to associate with
        // This is a simplified version - in production you'd track the active window
        TrackPopupMenu(menuHandle, TPM_LEFTALIGN, x, y, 0, IntPtr.Zero, IntPtr.Zero);
    }

    IntPtr IPlatform.CreateMenuItem(IntPtr menuHandle, int style, int id, int index)
    {
        uint flags = MF_STRING;

        if ((style & SWT.SEPARATOR) != 0)
        {
            flags = MF_SEPARATOR;
        }
        else if ((style & SWT.CASCADE) != 0)
        {
            flags = MF_POPUP;
        }

        if (index >= 0)
        {
            flags |= MF_BYPOSITION;
            InsertMenu(menuHandle, (uint)index, flags, (UIntPtr)id, string.Empty);
        }
        else
        {
            AppendMenu(menuHandle, flags, (UIntPtr)id, string.Empty);
        }

        // Return the menu item ID as the handle (Win32 uses IDs to identify menu items)
        return (IntPtr)id;
    }

    void IPlatform.DestroyMenuItem(IntPtr handle)
    {
        // Menu items are destroyed when their parent menu is destroyed
        // No explicit cleanup needed
    }

    void IPlatform.SetMenuItemText(IntPtr handle, string text)
    {
        // On Win32, we need the menu handle and item ID
        // This is a limitation of our current design - we'd need to track parent menus
        // For now, this is a stub
    }

    void IPlatform.SetMenuItemSelection(IntPtr handle, bool selected)
    {
        // Would need parent menu handle
        // CheckMenuItem(parentMenu, (uint)(int)handle, selected ? MF_CHECKED : MF_UNCHECKED);
    }

    void IPlatform.SetMenuItemEnabled(IntPtr handle, bool enabled)
    {
        // Would need parent menu handle
        // EnableMenuItem(parentMenu, (uint)(int)handle, enabled ? MF_ENABLED : MF_GRAYED);
    }

    void IPlatform.SetMenuItemSubmenu(IntPtr itemHandle, IntPtr submenuHandle)
    {
        // Cascade items are created with the submenu handle as the ID
        // This is handled in CreateMenuItem
    }

    // Label operations - implemented in Win32Platform_Label.cs

    // Edit control (Text) constants
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
    private const uint ES_NUMBER = 0x2000;

    // Edit control messages
    private const uint EM_GETSEL = 0x00B0;
    private const uint EM_SETSEL = 0x00B1;
    private const uint EM_GETRECT = 0x00B2;
    private const uint EM_SETRECT = 0x00B3;
    private const uint EM_SCROLL = 0x00B5;
    private const uint EM_LINESCROLL = 0x00B6;
    private const uint EM_SCROLLCARET = 0x00B7;
    private const uint EM_GETMODIFY = 0x00B8;
    private const uint EM_SETMODIFY = 0x00B9;
    private const uint EM_GETLINECOUNT = 0x00BA;
    private const uint EM_LINEINDEX = 0x00BB;
    private const uint EM_SETHANDLE = 0x00BC;
    private const uint EM_GETHANDLE = 0x00BD;
    private const uint EM_GETTHUMB = 0x00BE;
    private const uint EM_LINELENGTH = 0x00C1;
    private const uint EM_REPLACESEL = 0x00C2;
    private const uint EM_GETLINE = 0x00C4;
    private const uint EM_LIMITTEXT = 0x00C5;
    private const uint EM_CANUNDO = 0x00C6;
    private const uint EM_UNDO = 0x00C7;
    private const uint EM_FMTLINES = 0x00C8;
    private const uint EM_LINEFROMCHAR = 0x00C9;
    private const uint EM_SETTABSTOPS = 0x00CB;
    private const uint EM_SETPASSWORDCHAR = 0x00CC;
    private const uint EM_EMPTYUNDOBUFFER = 0x00CD;
    private const uint EM_GETFIRSTVISIBLELINE = 0x00CE;
    private const uint EM_SETREADONLY = 0x00CF;
    private const uint EM_SETWORDBREAKPROC = 0x00D0;
    private const uint EM_GETWORDBREAKPROC = 0x00D1;
    private const uint EM_GETPASSWORDCHAR = 0x00D2;
    private const uint EM_SETMARGINS = 0x00D3;
    private const uint EM_GETMARGINS = 0x00D4;

    // Window messages for text
    private const uint WM_GETTEXT = 0x000D;
    private const uint WM_GETTEXTLENGTH = 0x000E;

    // Border styles
    private const uint WS_EX_CLIENTEDGE = 0x00000200;

    // Text control operations
    public IntPtr CreateText(IntPtr parent, int style)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE;
        uint exStyle = 0;

        // Multi-line vs single-line
        if ((style & SWT.MULTI) != 0)
        {
            windowStyle |= ES_MULTILINE | ES_AUTOVSCROLL | ES_WANTRETURN;

            if ((style & SWT.WRAP) != 0)
            {
                // Word wrapping - don't add horizontal scroll
            }
            else
            {
                windowStyle |= ES_AUTOHSCROLL;
            }
        }
        else // SINGLE line is default
        {
            windowStyle |= ES_AUTOHSCROLL;
        }

        // Password field
        if ((style & SWT.PASSWORD) != 0)
        {
            windowStyle |= ES_PASSWORD;
        }

        // Read-only
        if ((style & SWT.READ_ONLY) != 0)
        {
            windowStyle |= ES_READONLY;
        }

        // Border
        if ((style & SWT.BORDER) != 0)
        {
            exStyle |= WS_EX_CLIENTEDGE;
        }

        // Search style (Windows Vista+ has native search box)
        // For now, treat as regular text with border
        if ((style & SWT.SEARCH) != 0)
        {
            exStyle |= WS_EX_CLIENTEDGE;
        }

        var handle = CreateWindowEx(
            exStyle,
            "EDIT",           // Windows Edit control
            string.Empty,
            windowStyle,
            0, 0, 100, 25,    // Default size
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        return handle;
    }

    public void SetTextContent(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, WM_SETTEXT, IntPtr.Zero, text ?? string.Empty);
    }

    public string GetTextContent(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return string.Empty;

        // Get the length of the text
        int length = SendMessage(handle, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
        if (length == 0)
            return string.Empty;

        // Allocate buffer and retrieve text
        var buffer = new System.Text.StringBuilder(length + 1);
        SendMessage(handle, WM_GETTEXT, new IntPtr(buffer.Capacity), buffer);

        return buffer.ToString();
    }

    public void SetTextSelection(IntPtr handle, int start, int end)
    {
        if (handle == IntPtr.Zero)
            return;

        // EM_SETSEL wParam=start, lParam=end
        SendMessage(handle, EM_SETSEL, new IntPtr(start), new IntPtr(end));

        // Scroll to make selection visible
        SendMessage(handle, EM_SCROLLCARET, IntPtr.Zero, IntPtr.Zero);
    }

    public (int Start, int End) GetTextSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return (0, 0);

        int start = 0;
        int end = 0;

        // EM_GETSEL retrieves selection range
        SendMessage(handle, EM_GETSEL, ref start, ref end);

        return (start, end);
    }

    public void SetTextLimit(IntPtr handle, int limit)
    {
        if (handle == IntPtr.Zero)
            return;

        // EM_LIMITTEXT sets the maximum number of characters
        // 0 means maximum (64KB for single-line, ~4GB for multi-line on modern Windows)
        SendMessage(handle, EM_LIMITTEXT, new IntPtr(limit), IntPtr.Zero);
    }

    public void SetTextReadOnly(IntPtr handle, bool readOnly)
    {
        if (handle == IntPtr.Zero)
            return;

        // EM_SETREADONLY wParam=1 for read-only, 0 for editable
        SendMessage(handle, EM_SETREADONLY, new IntPtr(readOnly ? 1 : 0), IntPtr.Zero);
    }

    // Additional SendMessage overloads for text operations
    // Note: StringBuilder is not supported by LibraryImport - keep using DllImport
    [DllImport(User32, CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, System.Text.StringBuilder lParam);

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, ref int wParam, ref int lParam);
#else
    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, ref int wParam, ref int lParam);
#endif

    // List control constants
    private const uint LBS_NOTIFY = 0x0001;
    private const uint LBS_SORT = 0x0002;
    private const uint LBS_MULTIPLESEL = 0x0008;
    private const uint LBS_EXTENDEDSEL = 0x0800;
    private const uint LBS_NOINTEGRALHEIGHT = 0x0100;
    private const uint LBS_WANTKEYBOARDINPUT = 0x0400;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;

    // List box messages
    private const uint LB_ADDSTRING = 0x0180;
    private const uint LB_INSERTSTRING = 0x0181;
    private const uint LB_DELETESTRING = 0x0182;
    private const uint LB_RESETCONTENT = 0x0184;
    private const uint LB_SETCURSEL = 0x0186;
    private const uint LB_GETCURSEL = 0x0188;
    private const uint LB_GETSELCOUNT = 0x0190;
    private const uint LB_GETSELITEMS = 0x0191;
    private const uint LB_SETTOPINDEX = 0x0197;
    private const uint LB_GETTOPINDEX = 0x018E;
    private const uint LB_GETCOUNT = 0x018B;
    private const uint LB_SELITEMRANGE = 0x019B;

    // List control operations
    public IntPtr CreateList(IntPtr parentHandle, int style)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_VSCROLL | LBS_NOTIFY | LBS_NOINTEGRALHEIGHT;

        // Determine selection mode
        if ((style & SWT.MULTI) != 0)
        {
            windowStyle |= LBS_EXTENDEDSEL;
        }

        // Add border if requested
        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        var handle = CreateWindowEx(
            WS_EX_CLIENTEDGE,
            "LISTBOX",
            string.Empty,
            windowStyle,
            0, 0, 150, 200,  // Default size
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create list control. Error: {error}");
        }

        return handle;
    }

    public void AddListItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        if (index < 0)
        {
            // Append to end
            SendMessage(handle, LB_ADDSTRING, IntPtr.Zero, item);
        }
        else
        {
            // Insert at specific position
            SendMessage(handle, LB_INSERTSTRING, new IntPtr(index), item);
        }
    }

    public void RemoveListItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, LB_DELETESTRING, new IntPtr(index), IntPtr.Zero);
    }

    public void ClearListItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, LB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetListSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero)
            return;

        // First, clear all selections by setting -1
        SendMessage(handle, LB_SETCURSEL, new IntPtr(-1), IntPtr.Zero);

        if (indices == null || indices.Length == 0)
            return;

        // For single selection, just set the first index
        int count = SendMessage(handle, LB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (indices.Length == 1)
        {
            if (indices[0] >= 0 && indices[0] < count)
            {
                SendMessage(handle, LB_SETCURSEL, new IntPtr(indices[0]), IntPtr.Zero);
            }
        }
        else
        {
            // For multiple selections, use LB_SELITEMRANGE
            foreach (int index in indices)
            {
                if (index >= 0 && index < count)
                {
                    SendMessage(handle, LB_SELITEMRANGE, new IntPtr(1), new IntPtr((index << 16) | index));
                }
            }
        }
    }

    public int[] GetListSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return Array.Empty<int>();

        // Get selection count
        int selCount = SendMessage(handle, LB_GETSELCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (selCount <= 0)
            return Array.Empty<int>();

        if (selCount == -1)
        {
            // Single selection mode
            int index = SendMessage(handle, LB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
            if (index >= 0)
                return new int[] { index };
            return Array.Empty<int>();
        }

        // Multiple selection mode
        int[] buffer = new int[selCount];
        unsafe
        {
            fixed (int* pBuffer = buffer)
            {
                SendMessage(handle, LB_GETSELITEMS, new IntPtr(selCount), new IntPtr(pBuffer));
            }
        }

        return buffer;
    }

    public int GetListTopIndex(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return 0;

        return SendMessage(handle, LB_GETTOPINDEX, IntPtr.Zero, IntPtr.Zero).ToInt32();
    }

    public void SetListTopIndex(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, LB_SETTOPINDEX, new IntPtr(index), IntPtr.Zero);
    }

    // Combo operations - implemented in Win32Platform_Combo.cs

    // Group operations
    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        throw new NotImplementedException("Group not yet implemented on Win32 platform");
    }

    public void SetGroupText(IntPtr handle, string text)
    {
        throw new NotImplementedException("Group not yet implemented on Win32 platform");
    }

    // Canvas operations
    public IntPtr CreateCanvas(IntPtr parent, int style)
    {
        if (parent == IntPtr.Zero)
            throw new ArgumentException("Canvas requires a parent window", nameof(parent));

        uint windowStyle = WS_CHILD | WS_VISIBLE;

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        IntPtr hwnd = CreateWindowEx(
            0,                          // Extended style
            CanvasClassName,            // Use custom canvas class
            string.Empty,               // No title
            windowStyle,
            0, 0,                       // Position (will be set by layout)
            100, 100,                   // Default size
            parent,                     // Parent window
            IntPtr.Zero,                // No menu
            _hInstance,
            IntPtr.Zero);

        if (hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error, $"Failed to create canvas window. Error: {error}");
        }

        // Initialize canvas data with default white background
        var canvasData = new CanvasData
        {
            BackgroundColor = new Graphics.RGB(255, 255, 255),
            BackgroundBrush = CreateSolidBrush(0x00FFFFFF) // White in BGR format
        };
        _canvasData[hwnd] = canvasData;

        return hwnd;
    }

    public void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    {
        if (handle == IntPtr.Zero)
            return;

        if (_canvasData.TryGetValue(handle, out var canvasData))
        {
            canvasData.PaintCallback = paintCallback;
        }
        else
        {
            // If canvas data doesn't exist, create it
            var newData = new CanvasData
            {
                PaintCallback = paintCallback,
                BackgroundColor = new Graphics.RGB(255, 255, 255),
                BackgroundBrush = CreateSolidBrush(0x00FFFFFF)
            };
            _canvasData[handle] = newData;
        }
    }

    public void RedrawCanvas(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        // Invalidate the entire client area
        InvalidateRect(handle, IntPtr.Zero, true);
    }

    public void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    {
        if (handle == IntPtr.Zero)
            return;

        // Create a RECT structure for the specific area
        var rect = new RECT
        {
            Left = x,
            Top = y,
            Right = x + width,
            Bottom = y + height
        };

        InvalidateRect(handle, ref rect, true);
    }

    // TabFolder operations
    public IntPtr CreateTabFolder(IntPtr parent, int style)
    {
        throw new NotImplementedException("TabFolder not yet implemented on Win32 platform");
    }

    public void SetTabSelection(IntPtr handle, int index)
    {
        throw new NotImplementedException("TabFolder not yet implemented on Win32 platform");
    }

    public int GetTabSelection(IntPtr handle)
    {
        throw new NotImplementedException("TabFolder not yet implemented on Win32 platform");
    }

    // TabItem operations
    public IntPtr CreateTabItem(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TabItem not yet implemented on Win32 platform");
    }

    public void SetTabItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TabItem not yet implemented on Win32 platform");
    }

    public void SetTabItemControl(IntPtr handle, IntPtr control)
    {
        throw new NotImplementedException("TabItem not yet implemented on Win32 platform");
    }

    public void SetTabItemToolTip(IntPtr handle, string toolTip)
    {
        throw new NotImplementedException("TabItem not yet implemented on Win32 platform");
    }

    // ToolBar operations
    public IntPtr CreateToolBar(int style)
    {
        throw new NotImplementedException("ToolBar not yet implemented on Win32 platform");
    }

    // ToolItem operations
    public IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void DestroyToolItem(IntPtr handle)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemImage(IntPtr handle, IntPtr image)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemToolTip(IntPtr handle, string toolTip)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemSelection(IntPtr handle, bool selected)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemEnabled(IntPtr handle, bool enabled)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemWidth(IntPtr handle, int width)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    public void SetToolItemControl(IntPtr handle, IntPtr control)
    {
        throw new NotImplementedException("ToolItem not yet implemented on Win32 platform");
    }

    // Tree operations
    public IntPtr CreateTree(IntPtr parent, int style)
    {
        throw new NotImplementedException("Tree not yet implemented on Win32 platform");
    }

    public IntPtr[] GetTreeSelection(IntPtr handle)
    {
        throw new NotImplementedException("Tree not yet implemented on Win32 platform");
    }

    public void SetTreeSelection(IntPtr handle, IntPtr[] items)
    {
        throw new NotImplementedException("Tree not yet implemented on Win32 platform");
    }

    public void ClearTreeItems(IntPtr handle)
    {
        throw new NotImplementedException("Tree not yet implemented on Win32 platform");
    }

    public void ShowTreeItem(IntPtr handle, IntPtr item)
    {
        throw new NotImplementedException("Tree not yet implemented on Win32 platform");
    }

    // TreeItem operations
    public IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void DestroyTreeItem(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void SetTreeItemText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void SetTreeItemImage(IntPtr handle, IntPtr image)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void SetTreeItemChecked(IntPtr handle, bool checked_)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public bool GetTreeItemChecked(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void SetTreeItemExpanded(IntPtr handle, bool expanded)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public bool GetTreeItemExpanded(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void ClearTreeItemChildren(IntPtr handle)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    public void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index)
    {
        throw new NotImplementedException("TreeItem not yet implemented on Win32 platform");
    }

    // Table operations
    public IntPtr CreateTable(IntPtr parent, int style)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void ClearTableItems(IntPtr handle)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    // TableColumn operations
    public IntPtr CreateTableColumn(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTip)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public int PackTableColumn(IntPtr handle)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    // TableItem operations
    public IntPtr CreateTableItem(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void DestroyTableItem(IntPtr handle)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemText(IntPtr handle, int column, string text)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemImage(IntPtr handle, int column, IntPtr image)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemChecked(IntPtr handle, bool checked_)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    // ProgressBar operations

    // Progress bar styles
    private const uint PBS_HORIZONTAL = 0x01;
    private const uint PBS_VERTICAL = 0x04;
    private const uint PBS_SMOOTH = 0x01;
    private const uint PBS_MARQUEE = 0x08;

    // Progress bar messages
    private const uint PBM_SETRANGE = 0x0401;
    private const uint PBM_SETPOS = 0x0402;
    private const uint PBM_DELTAPOS = 0x0403;
    private const uint PBM_SETSTEP = 0x0404;
    private const uint PBM_STEPIT = 0x0405;
    private const uint PBM_SETRANGE32 = 0x0406;
    private const uint PBM_GETRANGE = 0x0407;
    private const uint PBM_GETPOS = 0x0408;
    private const uint PBM_SETBARCOLOR = 0x0409;
    private const uint PBM_SETBKCOLOR = 0x2001;
    private const uint PBM_SETMARQUEE = 0x040A;  // For indeterminate mode
    private const uint PBM_SETSTATE = 0x0410;     // Vista+ only
    private const uint PBM_GETSTATE = 0x0411;     // Vista+ only

    // Progress bar states (Vista+ only)
    private const int PBST_NORMAL = 0x0001;
    private const int PBST_ERROR = 0x0002;
    private const int PBST_PAUSED = 0x0003;

    public IntPtr CreateProgressBar(IntPtr parent, int style)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE;

        // Determine orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            windowStyle |= PBS_VERTICAL;
        }
        else
        {
            windowStyle |= PBS_HORIZONTAL;
        }

        // Smooth style
        if ((style & SWT.SMOOTH) != 0)
        {
            windowStyle |= PBS_SMOOTH;
        }

        // Indeterminate style (marquee)
        if ((style & SWT.INDETERMINATE) != 0)
        {
            windowStyle |= PBS_MARQUEE;
        }

        var handle = CreateWindowEx(
            0,
            "msctls_progress32",  // Progress bar class
            string.Empty,
            windowStyle,
            0, 0, 200, 20,  // Default size
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create progress bar. Error: {error}");
        }

        // Enable marquee animation for indeterminate style
        if ((style & SWT.INDETERMINATE) != 0)
        {
            SendMessage(handle, PBM_SETMARQUEE, new IntPtr(1), new IntPtr(30)); // 30ms update interval
        }

        return handle;
    }

    public void SetProgressBarRange(IntPtr handle, int minimum, int maximum)
    {
        if (handle == IntPtr.Zero)
            return;

        // Use PBM_SETRANGE32 for full 32-bit range
        SendMessage(handle, PBM_SETRANGE32, new IntPtr(minimum), new IntPtr(maximum));
    }

    public void SetProgressBarSelection(IntPtr handle, int value)
    {
        if (handle == IntPtr.Zero)
            return;

        // Set the current position
        SendMessage(handle, PBM_SETPOS, new IntPtr(value), IntPtr.Zero);
    }

    public void SetProgressBarState(IntPtr handle, int state)
    {
        if (handle == IntPtr.Zero)
            return;

        // Map SWT state to Win32 state
        int pbState = state switch
        {
            ProgressBarState.NORMAL => PBST_NORMAL,
            ProgressBarState.ERROR => PBST_ERROR,
            ProgressBarState.PAUSED => PBST_PAUSED,
            _ => PBST_NORMAL
        };

        // PBM_SETSTATE is only available on Windows Vista and later
        // On older systems, this message will be ignored
        SendMessage(handle, PBM_SETSTATE, new IntPtr(pbState), IntPtr.Zero);
    }

    // Slider operations
    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        throw new NotImplementedException("Slider not yet implemented on Win32 platform");
    }

    public void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Slider not yet implemented on Win32 platform");
    }

    public void ConnectSliderChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Slider not yet implemented on Win32 platform");
    }

    // Scale operations
    public IntPtr CreateScale(IntPtr parent, int style)
    {
        throw new NotImplementedException("Scale not yet implemented on Win32 platform");
    }

    public void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Scale not yet implemented on Win32 platform");
    }

    public void ConnectScaleChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Scale not yet implemented on Win32 platform");
    }

    // Spinner operations
    public IntPtr CreateSpinner(IntPtr parent, int style)
    {
        throw new NotImplementedException("Spinner not yet implemented on Win32 platform");
    }

    public void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        throw new NotImplementedException("Spinner not yet implemented on Win32 platform");
    }

    public void SetSpinnerTextLimit(IntPtr handle, int limit)
    {
        throw new NotImplementedException("Spinner not yet implemented on Win32 platform");
    }

    public void ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    {
        throw new NotImplementedException("Spinner not yet implemented on Win32 platform");
    }

    public void ConnectSpinnerModified(IntPtr handle, Action callback)
    {
        throw new NotImplementedException("Spinner not yet implemented on Win32 platform");
    }

    // Dialog operations

    // File Dialog structures and imports
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    // OFN flags
    private const int OFN_READONLY = 0x00000001;
    private const int OFN_OVERWRITEPROMPT = 0x00000002;
    private const int OFN_HIDEREADONLY = 0x00000004;
    private const int OFN_NOCHANGEDIR = 0x00000008;
    private const int OFN_SHOWHELP = 0x00000010;
    private const int OFN_ENABLEHOOK = 0x00000020;
    private const int OFN_NOVALIDATE = 0x00000100;
    private const int OFN_ALLOWMULTISELECT = 0x00000200;
    private const int OFN_EXTENSIONDIFFERENT = 0x00000400;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_CREATEPROMPT = 0x00002000;
    private const int OFN_SHAREAWARE = 0x00004000;
    private const int OFN_NOREADONLYRETURN = 0x00008000;
    private const int OFN_NOTESTFILECREATE = 0x00010000;
    private const int OFN_NONETWORKBUTTON = 0x00020000;
    private const int OFN_EXPLORER = 0x00080000;
    private const int OFN_NODEREFERENCELINKS = 0x00100000;
    private const int OFN_LONGNAMES = 0x00200000;
    private const int OFN_ENABLEINCLUDENOTIFY = 0x00400000;
    private const int OFN_ENABLESIZING = 0x00800000;
    private const int OFN_DONTADDTORECENT = 0x02000000;
    private const int OFN_FORCESHOWHIDDEN = 0x10000000;

    private const string Comdlg32 = "comdlg32.dll";

    // LibraryImport doesn't support complex struct marshalling, use DllImport
    [DllImport(Comdlg32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetOpenFileName(ref OPENFILENAME ofn);

    [DllImport(Comdlg32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetSaveFileName(ref OPENFILENAME ofn);

    public int ShowMessageBox(IntPtr parent, string message, string title, int style)
    {
        throw new NotImplementedException("MessageBox not yet implemented on Win32 platform");
    }

    public FileDialogResult ShowFileDialog(IntPtr parentHandle, string title, string filterPath, string fileName, string[] filterNames, string[] filterExtensions, int style, bool overwrite)
    {
        const int MAX_PATH = 260;
        const int MULTI_SELECT_BUFFER = 65536; // 64KB for multiple file selection

        // Determine buffer size based on MULTI style
        bool isMultiSelect = (style & SWT.MULTI) != 0;
        int bufferSize = isMultiSelect ? MULTI_SELECT_BUFFER : MAX_PATH;

        // Allocate file name buffer
        IntPtr fileBuffer = Marshal.AllocHGlobal(bufferSize * 2); // Unicode = 2 bytes per char
        try
        {
            // Initialize buffer with initial file name
            if (!string.IsNullOrEmpty(fileName))
            {
                byte[] bytes = System.Text.Encoding.Unicode.GetBytes(fileName + "\0");
                Marshal.Copy(bytes, 0, fileBuffer, Math.Min(bytes.Length, bufferSize * 2));
            }
            else
            {
                Marshal.WriteInt16(fileBuffer, 0, 0); // Null terminator
            }

            // Build filter string: "Text Files\0*.txt\0All Files\0*.*\0\0"
            string filter = BuildFilterString(filterNames, filterExtensions);

            var ofn = new OPENFILENAME
            {
                lStructSize = Marshal.SizeOf<OPENFILENAME>(),
                hwndOwner = parentHandle,
                hInstance = _hInstance,
                lpstrFilter = filter,
                lpstrCustomFilter = null!,
                nMaxCustFilter = 0,
                nFilterIndex = 1, // 1-based index
                lpstrFile = fileBuffer,
                nMaxFile = bufferSize,
                lpstrFileTitle = IntPtr.Zero,
                nMaxFileTitle = 0,
                lpstrInitialDir = filterPath,
                lpstrTitle = title,
                Flags = OFN_EXPLORER | OFN_ENABLESIZING | OFN_NOCHANGEDIR,
                nFileOffset = 0,
                nFileExtension = 0,
                lpstrDefExt = null!,
                lCustData = IntPtr.Zero,
                lpfnHook = IntPtr.Zero,
                lpTemplateName = null!,
                pvReserved = IntPtr.Zero,
                dwReserved = 0,
                FlagsEx = 0
            };

            // Set flags based on style
            if ((style & SWT.SAVE) == 0) // OPEN dialog
            {
                ofn.Flags |= OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST;
            }

            if ((style & SWT.SAVE) != 0 && overwrite)
            {
                ofn.Flags |= OFN_OVERWRITEPROMPT;
            }

            if (isMultiSelect)
            {
                ofn.Flags |= OFN_ALLOWMULTISELECT;
            }

            // Show dialog
            bool result;
            if ((style & SWT.SAVE) != 0)
            {
                result = GetSaveFileName(ref ofn);
            }
            else
            {
                result = GetOpenFileName(ref ofn);
            }

            if (!result)
            {
                return new FileDialogResult
                {
                    SelectedFiles = null,
                    FilterPath = null,
                    FilterIndex = 0
                };
            }

            // Parse result
            string[] selectedFiles;
            string? resultFilterPath = null;

            if (isMultiSelect)
            {
                // Parse multi-select format: "directory\0file1\0file2\0\0"
                selectedFiles = ParseMultiSelectFiles(fileBuffer, bufferSize);

                // Extract directory from first file (or single item)
                if (selectedFiles.Length > 0)
                {
                    resultFilterPath = Path.GetDirectoryName(selectedFiles[0]);
                }
            }
            else
            {
                // Single file selection
                string selectedFile = Marshal.PtrToStringUni(fileBuffer) ?? string.Empty;
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    selectedFiles = new[] { selectedFile };
                    resultFilterPath = Path.GetDirectoryName(selectedFile);
                }
                else
                {
                    selectedFiles = Array.Empty<string>();
                }
            }

            return new FileDialogResult
            {
                SelectedFiles = selectedFiles,
                FilterPath = resultFilterPath,
                FilterIndex = ofn.nFilterIndex - 1 // Convert from 1-based to 0-based
            };
        }
        finally
        {
            Marshal.FreeHGlobal(fileBuffer);
        }
    }

    private string BuildFilterString(string[] filterNames, string[] filterExtensions)
    {
        if (filterNames == null || filterExtensions == null || filterNames.Length == 0)
        {
            // Default filter
            return "All Files\0*.*\0\0";
        }

        var builder = new System.Text.StringBuilder();
        int count = Math.Min(filterNames.Length, filterExtensions.Length);

        for (int i = 0; i < count; i++)
        {
            builder.Append(filterNames[i]);
            builder.Append('\0');
            builder.Append(filterExtensions[i]);
            builder.Append('\0');
        }

        builder.Append('\0'); // Double null terminator
        return builder.ToString();
    }

    private string[] ParseMultiSelectFiles(IntPtr buffer, int bufferSize)
    {
        var files = new List<string>();

        // Read directory path first
        string directory = string.Empty;
        int charIndex = 0;

        while (charIndex < bufferSize)
        {
            char c = (char)Marshal.ReadInt16(buffer, charIndex * 2);
            if (c == '\0')
                break;
            directory += c;
            charIndex++;
        }

        if (string.IsNullOrEmpty(directory))
        {
            return Array.Empty<string>();
        }

        // Check if there are multiple files
        charIndex++; // Skip first null terminator
        int nextCharIndex = charIndex;

        // Peek at next character
        if (nextCharIndex < bufferSize)
        {
            char nextChar = (char)Marshal.ReadInt16(buffer, nextCharIndex * 2);
            if (nextChar == '\0')
            {
                // Only one file selected (directory is actually the full file path)
                return new[] { directory };
            }
        }

        // Multiple files - parse each filename
        while (charIndex < bufferSize)
        {
            string filename = string.Empty;
            while (charIndex < bufferSize)
            {
                char c = (char)Marshal.ReadInt16(buffer, charIndex * 2);
                charIndex++;

                if (c == '\0')
                    break;

                filename += c;
            }

            if (string.IsNullOrEmpty(filename))
                break;

            // Combine directory and filename
            files.Add(Path.Combine(directory, filename));
        }

        return files.Count > 0 ? files.ToArray() : new[] { directory };
    }

    public string? ShowDirectoryDialog(IntPtr parentHandle, string title, string message, string filterPath)
    {
        throw new NotImplementedException("DirectoryDialog not yet implemented on Win32 platform");
    }

    public Graphics.RGB? ShowColorDialog(IntPtr parentHandle, string title, Graphics.RGB initialColor, Graphics.RGB[]? customColors)
    {
        throw new NotImplementedException("ColorDialog not yet implemented on Win32 platform");
    }

    public FontDialogResult ShowFontDialog(IntPtr parentHandle, string title, Graphics.FontData? initialFont, Graphics.RGB? initialColor)
    {
        throw new NotImplementedException("FontDialog not yet implemented on Win32 platform");
    }
}
