using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation.
/// </summary>
internal partial class Win32Platform : IPlatform
{
    // New platform widget methods (return objects, not handles!)

    public IPlatformWindow CreateWindowWidget(int style, string title)
    {
        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating window widget. Style: 0x{style:X}, Title: '{title}'");

        var window = new SWTSharp.Platform.Win32.Win32Window(style, title);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Window widget created successfully");

        return window;
    }

    public IPlatformWidget CreateButtonWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating button widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var button = new SWTSharp.Platform.Win32.Win32Button(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Button widget created successfully");

        return button;
    }

    public IPlatformWidget CreateLabelWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        return new SWTSharp.Platform.Win32.Win32Label(parentHandle, style);
    }

    public IPlatformTextInput CreateTextWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        return new SWTSharp.Platform.Win32.Win32Text(parentHandle, style);
    }

    public IPlatformComposite CreateCompositeWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating composite widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var composite = new SWTSharp.Platform.Win32.Win32Composite(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Composite widget created successfully");

        return composite;
    }

    public IPlatformToolBar CreateToolBarWidget(IPlatformWindow parent, int style)
    {
        // TODO: Implement Win32ToolBar in Phase 3 (special case)
        throw new NotImplementedException("CreateToolBarWidget will be implemented in Phase 3");
    }

    // Advanced widget factory methods for Phase 5.3
    public IPlatformCombo CreateComboWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating combo widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var combo = new SWTSharp.Platform.Win32.Win32Combo(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Combo widget created successfully");

        return combo;
    }

    public IPlatformList CreateListWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating list widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var list = new SWTSharp.Platform.Win32.Win32List(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] List widget created successfully");

        return list;
    }

    public IPlatformProgressBar CreateProgressBarWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating progressbar widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var progressBar = new SWTSharp.Platform.Win32.Win32ProgressBar(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] ProgressBar widget created successfully");

        return progressBar;
    }

    public IPlatformSlider CreateSliderWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating slider widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var slider = new SWTSharp.Platform.Win32.Win32Slider(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Slider widget created successfully");

        return slider;
    }

    public IPlatformScale CreateScaleWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating scale widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var scale = new SWTSharp.Platform.Win32.Win32Scale(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Scale widget created successfully");

        return scale;
    }

    public IPlatformSpinner CreateSpinnerWidget(IPlatformWidget? parent, int style)
    {
        // Get parent handle - use desktop if no parent
        IntPtr parentHandle = IntPtr.Zero;
        if (parent != null)
        {
            parentHandle = ExtractNativeHandle(parent);
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating spinner widget. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        var spinner = new SWTSharp.Platform.Win32.Win32Spinner(parentHandle, style);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Spinner widget created successfully");

        return spinner;
    }
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";

    // Window Styles
    private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;

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

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
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
    [LibraryImport(User32, EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
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
    [DllImport(User32, EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, SetLastError = true)]
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
    [LibraryImport(User32, EntryPoint = "SetWindowTextW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowText(IntPtr hWnd, string lpString);
#else
    [DllImport(User32, EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode, SetLastError = true)]
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
    [LibraryImport(Kernel32, EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
    private static partial IntPtr GetModuleHandle(string? lpModuleName);
#else
    [DllImport(Kernel32, EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
#endif

    // Note: RegisterClass uses struct marshalling which is not supported by LibraryImport in .NET 8
    // Keep using DllImport for this function
    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

    [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "DefWindowProcW")]
    private static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
#else
    [DllImport(User32, EntryPoint = "DefWindowProcW", CharSet = CharSet.Unicode)]
    private static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
#endif

    // GDI32 functions
    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private IntPtr _hInstance;
    private const string WindowClassName = "SWTSharpWindow";
    private const string CanvasClassName = "SWTSharpCanvas";
    private WndProcDelegate? _wndProcDelegate; // SEC-001: Store delegate to prevent GC
    private WndProcDelegate? _canvasWndProcDelegate; // SEC-001: Store canvas delegate to prevent GC
    private bool _windowClassRegistered;
    private bool _canvasClassRegistered;
    private static readonly bool _enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

    /// <summary>
    /// Initializes the Win32 platform, registering window classes.
    /// </summary>
    public void Initialize()
    {
        if (_enableLogging)
            Console.WriteLine("[Win32] Initializing Win32Platform");

        _hInstance = GetModuleHandle(null);
        if (_hInstance == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to get module handle");
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Module handle: 0x{_hInstance:X}");

        RegisterWindowClass();
        RegisterCanvasClass();

        if (_enableLogging)
            Console.WriteLine("[Win32] Initialization complete");
    }

    /// <summary>
    /// Registers the main SWTSharpWindow class for creating windows.
    /// Handles ERROR_CLASS_ALREADY_EXISTS (1406) by ignoring if class is already registered.
    /// </summary>
    private void RegisterWindowClass()
    {
        if (_windowClassRegistered)
        {
            if (_enableLogging)
                Console.WriteLine($"[Win32] Window class '{WindowClassName}' already registered");
            return;
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Registering window class: {WindowClassName}");

        // SEC-001: Store delegate reference before marshalling to prevent GC
        _wndProcDelegate = WndProc;

        var wndClass = new WNDCLASS
        {
            style = CS_HREDRAW | CS_VREDRAW,
            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate),
            hInstance = _hInstance,
            lpszClassName = WindowClassName,
            hCursor = IntPtr.Zero,
            hbrBackground = new IntPtr(6) // COLOR_WINDOW + 1
        };

        ushort atom = RegisterClass(ref wndClass);
        if (atom == 0)
        {
            int error = Marshal.GetLastWin32Error();
            const int ERROR_CLASS_ALREADY_EXISTS = 1406;

            if (_enableLogging)
                Console.WriteLine($"[Win32] Window class registration returned atom 0. Error: {error}");

            // If class already exists, that's fine - we can use the existing registration
            if (error == ERROR_CLASS_ALREADY_EXISTS)
            {
                if (_enableLogging)
                    Console.WriteLine($"[Win32] Window class already exists (expected)");
                _windowClassRegistered = true;
                return;
            }

            // Any other error is a real problem
            throw new Win32Exception(error, $"Failed to register window class '{WindowClassName}'");
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Window class registered successfully. Atom: 0x{atom:X}");

        _windowClassRegistered = true;
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
        if (_canvasClassRegistered)
        {
            if (_enableLogging)
                Console.WriteLine($"[Win32] Canvas class '{CanvasClassName}' already registered");
            return;
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Registering canvas class: {CanvasClassName}");

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

        ushort atom = RegisterClass(ref wndClass);
        if (atom == 0)
        {
            int error = Marshal.GetLastWin32Error();
            const int ERROR_CLASS_ALREADY_EXISTS = 1406;

            if (_enableLogging)
                Console.WriteLine($"[Win32] Canvas class registration returned atom 0. Error: {error}");

            // If class already exists, that's fine - we can use the existing registration
            if (error == ERROR_CLASS_ALREADY_EXISTS)
            {
                if (_enableLogging)
                    Console.WriteLine($"[Win32] Canvas class already exists (expected)");
                _canvasClassRegistered = true;
                return;
            }

            // Any other error is a real problem
            throw new Win32Exception(error, $"Failed to register canvas class '{CanvasClassName}'");
        }

        if (_enableLogging)
            Console.WriteLine($"[Win32] Canvas class registered successfully. Atom: 0x{atom:X}");

        _canvasClassRegistered = true;
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

    // SetWindowPos flags - used internally by platform implementations
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;

    // Internal helper methods (not part of IPlatform interface)

    /// <summary>
    /// Extracts the native HWND from a platform widget.
    /// Tries GetNativeHandle() method first, then falls back to reflection for _hwnd field.
    /// </summary>
    /// <param name="widget">The platform widget to extract the handle from</param>
    /// <returns>The native HWND, or IntPtr.Zero if extraction fails</returns>
    private static IntPtr ExtractNativeHandle(IPlatformWidget widget)
    {
        if (widget == null)
            return IntPtr.Zero;

        var widgetType = widget.GetType();

        // Primary method: Try GetNativeHandle() method (all Win32 widgets now have this as internal)
        var getNativeHandleMethod = widgetType.GetMethod("GetNativeHandle",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (getNativeHandleMethod != null && getNativeHandleMethod.ReturnType == typeof(IntPtr))
        {
            var result = getNativeHandleMethod.Invoke(widget, null);
            if (result is IntPtr hwnd && hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
        }

        // Fallback: Use reflection to access the private _hwnd field
        // This provides compatibility with widgets that might not have GetNativeHandle() yet
        var hwndField = widgetType.GetField("_hwnd",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (hwndField != null && hwndField.FieldType == typeof(IntPtr))
        {
            var handle = hwndField.GetValue(widget);
            if (handle is IntPtr hwnd && hwnd != IntPtr.Zero)
            {
                return hwnd;
            }
        }

        // If all else fails, return zero (will create with desktop as parent)
        return IntPtr.Zero;
    }

    internal void SetControlEnabled(IntPtr handle, bool enabled)
    {
        EnableWindow(handle, enabled);
    }

    internal void SetControlVisible(IntPtr handle, bool visible)
    {
        ShowWindow(handle, visible ? SW_SHOW : SW_HIDE);
    }

    internal void SetControlBounds(IntPtr handle, int x, int y, int width, int height)
    {
        MoveWindow(handle, x, y, width, height, true);
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

    public void ExecuteOnMainThread(Action action)
    {
        // On Windows, there's no separate "main thread" requirement like macOS
        // Just execute directly
        action();
    }

    // CreateComposite method - REMOVED: Now handled by CreateCompositeWidget() which returns IPlatformComposite
    // public IntPtr CreateComposite(IntPtr parent, int style) - Removed in platform widget migration

    // Button-specific constants - REMOVED: Now handled by platform widget interfaces
    // Removed: BS_PUSHBUTTON, BS_DEFPUSHBUTTON, BS_CHECKBOX, BS_AUTOCHECKBOX, BS_RADIOBUTTON, etc.
    // Removed: BM_SETCHECK, BM_GETCHECK, BM_CLICK, BN_CLICKED, BST_UNCHECKED, BST_CHECKED

    // Control messages
    private const uint WM_COMMAND = 0x0111;
    private const uint WM_SETTEXT = 0x000C;
    private const uint WM_ENABLE = 0x000A;

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "SendMessageW")]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
#else
    [DllImport(User32, EntryPoint = "SendMessageW")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "SendMessageW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);
#else
    [DllImport(User32, EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
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
    // Button callback dictionary - REMOVED: No longer needed with platform widget interfaces
    // private readonly Dictionary<IntPtr, Action> _buttonCallbacks - Removed in platform widget migration
    // ClearButtonCallbacks() method also removed - event handling now in widget classes

    // Canvas data structure - implementations in Win32Platform_Canvas.cs
    // (CanvasData class and _canvasData dictionary defined in Win32Platform_Canvas.cs)

    // Button widget methods - REMOVED: Now handled by CreateButtonWidget() which returns IPlatformWidget
    // public IntPtr CreateButton(IntPtr parent, int style, string text) - Removed in platform widget migration
    // public void SetButtonText(IntPtr handle, string text) - Removed in platform widget migration
    // public void SetButtonSelection(IntPtr handle, bool selected) - Removed in platform widget migration
    // public bool GetButtonSelection(IntPtr handle) - Removed in platform widget migration

    // ConnectButtonClick method - REMOVED: Now handled by IPlatformWidget event system
    // public void ConnectButtonClick(IntPtr handle, Action callback) - Removed in platform widget migration

    // Menu operations - REMOVED: All menu-related P/Invoke declarations and constants removed
    // These are now handled by platform widget interfaces (IPlatformMenu, IPlatformMenuItem)
    // Removed: CreateMenu, CreatePopupMenu, DestroyMenu, SetMenu, AppendMenu, InsertMenu
    // Removed: CheckMenuItem, EnableMenuItem, TrackPopupMenu and all menu constants

    // Label operations - implemented in Win32Platform_Label.cs

    // Essential constants still needed by other platform widget partial classes
    // Border styles (still needed by List, Tree, Combo widgets)
    private const uint WS_EX_CLIENTEDGE = 0x00000200;

    // Window messages still needed by remaining widget implementations
    private const uint WM_GETTEXT = 0x000D;
    private const uint WM_GETTEXTLENGTH = 0x000E;

    // Edit control (Text) constants - REMOVED: Now handled by platform widget interfaces
    // Removed: All ES_ constants (edit control styles)
    // Removed: All EM_ constants (edit control messages)
    // Note: Some constants kept above for use by other widget implementations

    // Text widget methods - REMOVED: Now handled by platform widget interfaces
    // public IntPtr CreateText(IntPtr parent, int style) - Removed in platform widget migration
    // public void SetTextContent(IntPtr handle, string text) - Removed in platform widget migration
    // public string GetTextContent(IntPtr handle) - Removed in platform widget migration
    // public void SetTextSelection(IntPtr handle, int start, int end) - Removed in platform widget migration
    // public (int Start, int End) GetTextSelection(IntPtr handle) - Removed in platform widget migration
    // public void SetTextLimit(IntPtr handle, int limit) - Removed in platform widget migration
    // public void SetTextReadOnly(IntPtr handle, bool readOnly) - Removed in platform widget migration

    // Additional SendMessage overloads still needed by remaining widget implementations
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

    // Note: Some text-specific SendMessage overloads removed as those methods are now handled by platform widget interfaces

    // ========================================================================
    // Widget implementations in separate partial class files:
    // - List widget: Win32Platform_List.cs
    // - Group widget: Win32Platform_Group.cs
    // - Canvas widget: Win32Platform_Canvas.cs (additional methods)
    // - TabFolder widget: Win32Platform_TabFolder.cs
    // - Tree widget: Win32Platform_Tree.cs
    // - Table widget: Win32Platform_Table.cs
    // - Slider widget: Win32Platform_Slider.cs
    // - Scale widget: Win32Platform_Scale.cs
    // - Spinner widget: Win32Platform_Spinner.cs
    // - Dialog methods: Win32Platform_Dialogs.cs
    // - Combo widget: Win32Platform_Combo.cs
    // - Label widget: Win32Platform_Label.cs
    // ========================================================================

    // ProgressBar operations - REMOVED: Now handled by platform widget interfaces
    // All ProgressBar-related constants and methods have been removed:
    // - Progress bar styles: PBS_HORIZONTAL, PBS_VERTICAL, PBS_SMOOTH, PBS_MARQUEE
    // - Progress bar messages: PBM_SETRANGE, PBM_SETPOS, PBM_DELTAPOS, etc.
    // - Progress bar states: PBST_NORMAL, PBST_ERROR, PBST_PAUSED
    // - Methods: CreateProgressBar, SetProgressBarRange, SetProgressBarSelection, SetProgressBarState
}
