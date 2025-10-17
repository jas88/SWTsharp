using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Link widget methods.
/// Uses SysLink control for hyperlink display.
/// </summary>
internal partial class Win32Platform
{
    // SysLink control constants
    private const uint LWS_TRANSPARENT = 0x0001;
    private const uint LWS_IGNORERETURN = 0x0002;
    private const int WM_NOTIFY = 0x004E;
    private const int NM_CLICK = -2;
    private const int NM_RETURN = -4;

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
        private string _text = string.Empty;
        private bool _disposed;

        public event EventHandler<string>? LinkClicked;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public Win32Link(IntPtr handle)
        {
            _handle = handle;
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
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        uint windowStyle = WS_CHILD | WS_VISIBLE | LWS_TRANSPARENT;

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        IntPtr handle = CreateWindowEx(
            0,
            "SysLink",
            string.Empty,
            windowStyle,
            0, 0, 100, 20,
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create link control. Error: {error}");
        }

        var linkWidget = new Win32Link(handle);
        _linkWidgets[handle] = linkWidget;
        return linkWidget;
    }

    private Dictionary<IntPtr, Win32Link> _linkWidgets = new Dictionary<IntPtr, Win32Link>();

    // Called from message loop to handle link notifications
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
}
