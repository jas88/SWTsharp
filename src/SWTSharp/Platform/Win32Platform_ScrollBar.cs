using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - ScrollBar widget methods.
/// Uses SCROLLBAR window class for standalone scrollbar.
/// </summary>
internal partial class Win32Platform
{
    // ScrollBar style constants
    private const uint SBS_HORZ = 0x0000;
    private const uint SBS_VERT = 0x0001;
    private const int SB_CTL = 2;

    // ScrollBar messages
    private const int SBM_SETRANGE = 0x00E0;
    private const int SBM_SETPOS = 0x00E0 + 1;
    private const int SBM_GETPOS = 0x00E0 + 2;
    private const int SBM_SETRANGEREDRAW = 0x00E0 + 6;
    private const int SBM_SETSCROLLINFO = 0x00E9;
    private const int SBM_GETSCROLLINFO = 0x00EA;

    private const int WM_HSCROLL = 0x0114;
    private const int WM_VSCROLL = 0x0115;

    [StructLayout(LayoutKind.Sequential)]
    private struct SCROLLINFO
    {
        public uint cbSize;
        public uint fMask;
        public int nMin;
        public int nMax;
        public uint nPage;
        public int nPos;
        public int nTrackPos;
    }

    private const uint SIF_RANGE = 0x0001;
    private const uint SIF_PAGE = 0x0002;
    private const uint SIF_POS = 0x0004;
    private const uint SIF_DISABLENOSCROLL = 0x0008;
    private const uint SIF_TRACKPOS = 0x0010;
    private const uint SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

    private class Win32ScrollBar : IPlatformScrollBar
    {
        private readonly IntPtr _handle;
        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private int _increment = 1;
        private int _pageIncrement = 10;
        private int _thumb = 10;
        private bool _disposed;

        public event EventHandler<int>? ValueChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public Win32ScrollBar(IntPtr handle)
        {
            _handle = handle;
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = Math.Max(_minimum, Math.Min(_maximum, value));
                UpdateScrollInfo();
            }
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                UpdateScrollInfo();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                UpdateScrollInfo();
            }
        }

        public int Increment
        {
            get => _increment;
            set => _increment = value;
        }

        public int PageIncrement
        {
            get => _pageIncrement;
            set
            {
                _pageIncrement = value;
                UpdateScrollInfo();
            }
        }

        public int Thumb
        {
            get => _thumb;
            set
            {
                _thumb = value;
                UpdateScrollInfo();
            }
        }

        internal void UpdateScrollInfo()
        {
            var si = new SCROLLINFO
            {
                cbSize = (uint)Marshal.SizeOf<SCROLLINFO>(),
                fMask = SIF_ALL | SIF_DISABLENOSCROLL,
                nMin = _minimum,
                nMax = _maximum,
                nPage = (uint)_thumb,
                nPos = _value,
                nTrackPos = _value
            };

            Win32Platform.SendMessage(_handle, SBM_SETSCROLLINFO, new IntPtr(1), ref si);
        }

        internal void OnScroll(int scrollCode, int position)
        {
            int newValue = _value;

            switch (scrollCode)
            {
                case 0: // SB_LINEUP or SB_LINELEFT
                    newValue -= _increment;
                    break;
                case 1: // SB_LINEDOWN or SB_LINERIGHT
                    newValue += _increment;
                    break;
                case 2: // SB_PAGEUP or SB_PAGELEFT
                    newValue -= _pageIncrement;
                    break;
                case 3: // SB_PAGEDOWN or SB_PAGERIGHT
                    newValue += _pageIncrement;
                    break;
                case 4: // SB_THUMBPOSITION
                case 5: // SB_THUMBTRACK
                    newValue = position;
                    break;
                case 6: // SB_TOP or SB_LEFT
                    newValue = _minimum;
                    break;
                case 7: // SB_BOTTOM or SB_RIGHT
                    newValue = _maximum;
                    break;
            }

            Value = newValue;
            ValueChanged?.Invoke(this, _value);
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
            // ScrollBar background controlled by system
        }

        public RGB GetBackground()
        {
            return new RGB(240, 240, 240); // Default light gray
        }

        public void SetForeground(RGB color)
        {
            // ScrollBar foreground controlled by system
        }

        public RGB GetForeground()
        {
            return new RGB(0, 0, 0);
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

    public IPlatformScrollBar CreateScrollBarWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        bool isVertical = (style & SWT.VERTICAL) != 0;
        uint windowStyle = WS_CHILD | WS_VISIBLE | (isVertical ? SBS_VERT : SBS_HORZ);

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        IntPtr handle = CreateWindowEx(
            0,
            "SCROLLBAR",
            string.Empty,
            windowStyle,
            0, 0,
            isVertical ? 20 : 100,
            isVertical ? 100 : 20,
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create scrollbar control. Error: {error}");
        }

        var scrollBarWidget = new Win32ScrollBar(handle);
        _scrollBarWidgets[handle] = scrollBarWidget;
        scrollBarWidget.UpdateScrollInfo();
        return scrollBarWidget;
    }

    private Dictionary<IntPtr, Win32ScrollBar> _scrollBarWidgets = new Dictionary<IntPtr, Win32ScrollBar>();

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref SCROLLINFO lParam);
#else
    [DllImport(User32)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref SCROLLINFO lParam);
#endif
}
