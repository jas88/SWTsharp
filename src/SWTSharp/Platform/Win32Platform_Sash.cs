using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Sash widget methods.
/// Custom window with resize cursor for draggable divider.
/// </summary>
internal partial class Win32Platform
{
    // Cursor constants
    private const int IDC_SIZEWE = 32644; // Horizontal resize cursor
    private const int IDC_SIZENS = 32645; // Vertical resize cursor

#if NET8_0_OR_GREATER
    [LibraryImport(User32, EntryPoint = "LoadCursorW", SetLastError = true)]
    private static partial IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
#else
    [DllImport(User32, EntryPoint = "LoadCursorW", SetLastError = true)]
    private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    private static partial IntPtr SetCursor(IntPtr hCursor);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern IntPtr SetCursor(IntPtr hCursor);
#endif

    private const int WM_SETCURSOR = 0x0020;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_MOUSEMOVE = 0x0200;

    private class Win32Sash : IPlatformSash
    {
        private readonly IntPtr _handle;
        private int _position;
        private bool _isDragging;
        private int _dragStartPos;
        private readonly bool _isVertical;
        private readonly IntPtr _cursor;
        private bool _disposed;

        public event EventHandler<int>? PositionChanged;
        public event EventHandler<int>? Click;
        public event EventHandler<int>? FocusGained;
        public event EventHandler<int>? FocusLost;
        public event EventHandler<PlatformKeyEventArgs>? KeyDown;
        public event EventHandler<PlatformKeyEventArgs>? KeyUp;

        public Win32Sash(IntPtr handle, int style)
        {
            _handle = handle;
            _isVertical = (style & SWT.VERTICAL) != 0;
            _cursor = LoadCursor(IntPtr.Zero, _isVertical ? IDC_SIZENS : IDC_SIZEWE);
        }

        public void SetPosition(int position)
        {
            _position = position;
        }

        public int GetPosition()
        {
            return _position;
        }

        internal void OnMouseDown(int x, int y)
        {
            _isDragging = true;
            _dragStartPos = _isVertical ? y : x;
        }

        internal void OnMouseUp(int x, int y)
        {
            _isDragging = false;
        }

        internal void OnMouseMove(int x, int y)
        {
            if (_isDragging)
            {
                int newPos = _isVertical ? y : x;
                int delta = newPos - _dragStartPos;
                _position += delta;
                _dragStartPos = newPos;
                PositionChanged?.Invoke(this, _position);
            }
        }

        internal void OnSetCursor()
        {
            SetCursor(_cursor);
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
            // Sash background controlled by system
        }

        public RGB GetBackground()
        {
            return new RGB(192, 192, 192); // Default gray
        }

        public void SetForeground(RGB color)
        {
            // Not applicable for sash
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

    public IPlatformSash CreateSashWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        bool isVertical = (style & SWT.VERTICAL) != 0;
        uint windowStyle = WS_CHILD | WS_VISIBLE;

        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        // Create as a simple static control that we'll handle events for
        IntPtr handle = CreateWindowEx(
            0,
            "STATIC",
            string.Empty,
            windowStyle | SS_NOTIFY,
            0, 0,
            isVertical ? 100 : 5, // Default size: wide for vertical, tall for horizontal
            isVertical ? 5 : 100,
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create sash control. Error: {error}");
        }

        var sashWidget = new Win32Sash(handle, style);
        _sashWidgets[handle] = sashWidget;
        return sashWidget;
    }

    private Dictionary<IntPtr, Win32Sash> _sashWidgets = new Dictionary<IntPtr, Win32Sash>();
}
