using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Link widget methods.
/// Uses SysLink control for hyperlink display.
/// Programmatically enables Common Controls v6 via activation context,
/// so consuming applications don't need an app.manifest.
/// </summary>
internal partial class Win32Platform
{
    // SysLink control constants
    private const uint LWS_TRANSPARENT = 0x0001;
    private const uint LWS_IGNORERETURN = 0x0002;
    private const int WM_NOTIFY = 0x004E;
    private const int NM_CLICK = -2;
    private const int NM_RETURN = -4;

    // Common control class flag for SysLink
    private const int ICC_LINK_CLASS = 0x00008000;

    // Activation context flags
    private const uint ACTCTX_FLAG_RESOURCE_NAME_VALID = 0x00000008;
    private const uint ACTCTX_FLAG_SET_PROCESS_DEFAULT = 0x00000010;
    private const uint ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 0x00000004;

    private static bool _linkControlsInitialized;
    private static IntPtr _comctl32ActCtx = IntPtr.Zero;
    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    /// <summary>
    /// Activation context structure for enabling Common Controls v6.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ACTCTX
    {
        public int cbSize;
        public uint dwFlags;
        public string lpSource;
        public ushort wProcessorArchitecture;
        public ushort wLangId;
        public string lpAssemblyDirectory;
        public IntPtr lpResourceName;
        public string lpApplicationName;
        public IntPtr hModule;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateActCtx(ref ACTCTX pActCtx);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ActivateActCtx(IntPtr hActCtx, out IntPtr lpCookie);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeactivateActCtx(uint dwFlags, IntPtr ulCookie);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern void ReleaseActCtx(IntPtr hActCtx);

    /// <summary>
    /// Creates an activation context for Common Controls v6 using shell32.dll's manifest.
    /// Resource #124 in shell32.dll contains the ComCtl32 v6 manifest.
    /// </summary>
    private static IntPtr CreateComctl32ActivationContext()
    {
        // Get the system directory path
        string system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

        var ctx = new ACTCTX
        {
            cbSize = Marshal.SizeOf<ACTCTX>(),
            dwFlags = ACTCTX_FLAG_RESOURCE_NAME_VALID | ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID,
            lpSource = Path.Combine(system32, "shell32.dll"),
            lpAssemblyDirectory = system32,
            lpResourceName = new IntPtr(124) // Resource #124 contains ComCtl32 v6 manifest
        };

        IntPtr hActCtx = CreateActCtx(ref ctx);
        if (hActCtx == INVALID_HANDLE_VALUE)
        {
            int error = Marshal.GetLastWin32Error();
            if (_enableLogging)
                Console.WriteLine($"[Win32] CreateActCtx for ComCtl32 v6 failed. Error: {error}");
            return IntPtr.Zero;
        }

        if (_enableLogging)
            Console.WriteLine("[Win32] Created ComCtl32 v6 activation context successfully");

        return hActCtx;
    }

    private static void EnsureLinkControlsInitialized()
    {
        if (_linkControlsInitialized) return;

        // First, try to create an activation context for ComCtl32 v6
        // This enables SysLink without requiring the consuming app to have a manifest
        if (_comctl32ActCtx == IntPtr.Zero)
        {
            _comctl32ActCtx = CreateComctl32ActivationContext();
        }

        // Activate the context temporarily to initialize common controls
        IntPtr cookie = IntPtr.Zero;
        bool activated = false;
        if (_comctl32ActCtx != IntPtr.Zero)
        {
            activated = ActivateActCtx(_comctl32ActCtx, out cookie);
            if (_enableLogging && activated)
                Console.WriteLine("[Win32] Activated ComCtl32 v6 context for initialization");
        }

        try
        {
            var icc = new INITCOMMONCONTROLSEX
            {
                dwSize = Marshal.SizeOf<INITCOMMONCONTROLSEX>(),
                dwICC = ICC_LINK_CLASS
            };
            bool result = InitCommonControlsEx(ref icc);
            if (!result)
            {
                int error = Marshal.GetLastWin32Error();
                if (_enableLogging)
                    Console.WriteLine($"[Win32] InitCommonControlsEx for ICC_LINK_CLASS failed. Error: {error}");
            }
            else if (_enableLogging)
            {
                Console.WriteLine("[Win32] InitCommonControlsEx for ICC_LINK_CLASS succeeded");
            }
        }
        finally
        {
            if (activated)
            {
                DeactivateActCtx(0, cookie);
            }
        }

        _linkControlsInitialized = true;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WNDCLASSEX
    {
        public int cbSize;
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
        public IntPtr hIconSm;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClassInfoEx(IntPtr hInstance, string lpszClass, ref WNDCLASSEX lpwcx);

    [StructLayout(LayoutKind.Sequential)]
    private struct NMHDR
    {
        public IntPtr hwndFrom;
        public IntPtr idFrom;
        public int code;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NMLINK
    {
        public NMHDR hdr;
        public LITEM item;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct LITEM
    {
        public uint mask;
        public int iLink;
        public uint state;
        public uint stateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string szID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2048)]
        public string szUrl;
    }

    private class Win32Link : IPlatformLink
    {
        private readonly IntPtr _handle;
        private readonly bool _usingSysLink;
        private string _text = string.Empty;
        private bool _disposed;

        public event EventHandler<string>? LinkClicked;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public Win32Link(IntPtr handle, bool usingSysLink = true)
        {
            _handle = handle;
            _usingSysLink = usingSysLink;
        }

        public void SetText(string text)
        {
            if (_disposed || _handle == IntPtr.Zero) return;
            _text = text ?? string.Empty;
            Win32Platform.SendMessage(_handle, WM_SETTEXT, IntPtr.Zero, _text);
        }

        public string GetText()
        {
            return _text;
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
            // SysLink background color is typically controlled by the system
        }

        public RGB GetBackground()
        {
            return new RGB(255, 255, 255);
        }

        public void SetForeground(RGB color)
        {
            // SysLink text color is typically controlled by the system
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
                    Win32Platform.DestroyWindow(_handle);
                }
                _disposed = true;
            }
        }

        internal void OnLinkClicked(string linkId)
        {
            LinkClicked?.Invoke(this, linkId);
        }
    }

    public IPlatformLink CreateLinkWidget(IPlatformWidget? parent, int style)
    {
        EnsureLinkControlsInitialized();

        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;
        LogLinkCreation(parent, parentHandle, style);
        ValidateLinkParent(parentHandle);

        // Try SysLink first, fall back to Static if unavailable
        var sysLinkResult = TryCreateSysLink(parentHandle, style);
        if (sysLinkResult != null)
            return sysLinkResult;

        return CreateStaticLinkFallback(parentHandle, style);
    }

    private void LogLinkCreation(IPlatformWidget? parent, IntPtr parentHandle, int style)
    {
        if (_enableLogging)
            Console.WriteLine($"[Win32] Creating link widget. Parent type: {parent?.GetType().Name ?? "null"}, ParentHandle: 0x{parentHandle:X}, Style: 0x{style:X}");
    }

    private void ValidateLinkParent(IntPtr parentHandle)
    {
        if (parentHandle == IntPtr.Zero)
            throw new InvalidOperationException("Link widget requires a valid parent window");

        if (!IsWindow(parentHandle))
            throw new InvalidOperationException($"Link widget parent handle 0x{parentHandle:X} is not a valid window");
    }

    private Win32Link? TryCreateSysLink(IntPtr parentHandle, int style)
    {
        IntPtr cookie = IntPtr.Zero;
        bool activated = _comctl32ActCtx != IntPtr.Zero && ActivateActCtx(_comctl32ActCtx, out cookie);

        try
        {
            uint windowStyle = WS_CHILD | WS_VISIBLE | LWS_TRANSPARENT;
            if ((style & SWT.BORDER) != 0)
                windowStyle |= 0x00800000; // WS_BORDER

            IntPtr handle = CreateWindowEx(0, "SysLink", string.Empty, windowStyle,
                0, 0, 100, 20, parentHandle, IntPtr.Zero, _hInstance, IntPtr.Zero);

            if (handle == IntPtr.Zero)
            {
                if (_enableLogging)
                    Console.WriteLine($"[Win32] SysLink creation failed with error {Marshal.GetLastWin32Error()}, falling back to Static control");
                return null;
            }

            if (_enableLogging)
                Console.WriteLine("[Win32] Link widget created successfully using SysLink");

            var linkWidget = new Win32Link(handle, usingSysLink: true);
            _linkWidgets[handle] = linkWidget;
            return linkWidget;
        }
        finally
        {
            if (activated)
                DeactivateActCtx(0, cookie);
        }
    }

    private Win32Link CreateStaticLinkFallback(IntPtr parentHandle, int style)
    {
        if (_enableLogging)
            Console.WriteLine("[Win32] Using Static control fallback for Link");

        uint fallbackStyle = WS_CHILD | WS_VISIBLE | SS_NOTIFY;
        if ((style & SWT.BORDER) != 0)
            fallbackStyle |= 0x00800000; // WS_BORDER

        IntPtr handle = CreateWindowEx(0, "Static", string.Empty, fallbackStyle,
            0, 0, 100, 20, parentHandle, IntPtr.Zero, _hInstance, IntPtr.Zero);

        if (handle == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to create link control. Error: {Marshal.GetLastWin32Error()}");

        if (_enableLogging)
            Console.WriteLine("[Win32] Link widget created successfully using Static fallback");

        var fallbackWidget = new Win32Link(handle, usingSysLink: false);
        _linkWidgets[handle] = fallbackWidget;
        return fallbackWidget;
    }

    // Note: SS_NOTIFY is defined in Win32Platform_Label.cs

    private Dictionary<IntPtr, Win32Link> _linkWidgets = new Dictionary<IntPtr, Win32Link>();

    // Static control notification codes (for fallback)
    // WM_COMMAND is defined in Win32Platform.cs
    private const int STN_CLICKED = 0;

    // Called from message loop to handle link notifications (SysLink via WM_NOTIFY)
    internal void HandleLinkNotification(IntPtr handle, IntPtr lParam)
    {
        if (_linkWidgets.TryGetValue(handle, out var linkWidget))
        {
            var nmlink = Marshal.PtrToStructure<NMLINK>(lParam);
            if (nmlink.hdr.code == NM_CLICK || nmlink.hdr.code == NM_RETURN)
            {
                linkWidget.OnLinkClicked(nmlink.item.szID ?? string.Empty);
            }
        }
    }

    // Called from message loop to handle Static control clicks (fallback via WM_COMMAND)
    internal void HandleStaticLinkClick(IntPtr controlHandle)
    {
        if (_linkWidgets.TryGetValue(controlHandle, out var linkWidget))
        {
            // Static controls don't have link IDs, use empty string
            linkWidget.OnLinkClicked(string.Empty);
        }
    }

    /// <summary>
    /// Releases the ComCtl32 v6 activation context.
    /// Called during platform cleanup/disposal.
    /// </summary>
    internal static void CleanupLinkControls()
    {
        if (_comctl32ActCtx != IntPtr.Zero)
        {
            ReleaseActCtx(_comctl32ActCtx);
            _comctl32ActCtx = IntPtr.Zero;
            _linkControlsInitialized = false;
        }
    }
}
