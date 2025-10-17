using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of tracker widget.
/// Uses WindowFromDC with XOR drawing for rubber-band rectangle feedback.
/// </summary>
internal class Win32Tracker : IPlatformTracker
{
    private Rectangle[] _rectangles = Array.Empty<Rectangle>();
    private bool _stippled;
    private int _cursorType;
    private readonly IntPtr _parentHwnd;
    private readonly int _style;
    private bool _disposed;
    private bool _tracking;
    private IntPtr _hdc;
    private IntPtr _oldBrush;
    private IntPtr _oldPen;

    public event EventHandler<TrackerEventArgs>? MouseMove;
    public event EventHandler<TrackerEventArgs>? Resize;

    // Win32 API constants
    private const int R2_NOTXORPEN = 10; // XOR drawing mode
    private const int PS_SOLID = 0;
    private const int PS_DOT = 2;
    private const int NULL_BRUSH = 5;

    // Windows messages
    private const uint WM_MOUSEMOVE = 0x0200;
    private const uint WM_LBUTTONUP = 0x0202;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint VK_ESCAPE = 0x1B;

    public Win32Tracker(IntPtr parentHwnd, int style)
    {
        _parentHwnd = parentHwnd == IntPtr.Zero ? GetDesktopWindow() : parentHwnd;
        _style = style;
    }

    public bool Open()
    {
        if (_tracking || _rectangles.Length == 0)
            return false;

        _tracking = true;

        // Get DC for drawing
        _hdc = GetDC(_parentHwnd);
        if (_hdc == IntPtr.Zero)
            return false;

        try
        {
            // Set up XOR drawing mode
            SetROP2(_hdc, R2_NOTXORPEN);

            // Create pen for drawing
            IntPtr pen = CreatePen(_stippled ? PS_DOT : PS_SOLID, 2, RGB(0, 0, 0));
            _oldPen = SelectObject(_hdc, pen);

            // Use null brush for hollow rectangles
            _oldBrush = SelectObject(_hdc, GetStockObject(NULL_BRUSH));

            // Draw initial rectangles
            DrawRectangles();

            // Capture mouse
            SetCapture(_parentHwnd);

            // Track mouse events
            bool completed = TrackMouse();

            // Erase rectangles (XOR them again)
            DrawRectangles();

            // Release mouse capture
            ReleaseCapture();

            // Clean up
            SelectObject(_hdc, _oldBrush);
            SelectObject(_hdc, _oldPen);
            DeleteObject(pen);

            return completed;
        }
        finally
        {
            ReleaseDC(_parentHwnd, _hdc);
            _hdc = IntPtr.Zero;
            _tracking = false;
        }
    }

    public void Close()
    {
        _tracking = false;
    }

    public void SetRectangles(Rectangle[] rectangles)
    {
        _rectangles = rectangles ?? Array.Empty<Rectangle>();
    }

    public Rectangle[] GetRectangles()
    {
        return _rectangles;
    }

    public void SetStippled(bool stippled)
    {
        _stippled = stippled;
    }

    public bool GetStippled()
    {
        return _stippled;
    }

    public void SetCursor(int cursorType)
    {
        _cursorType = cursorType;
    }

    private void DrawRectangles()
    {
        foreach (var rect in _rectangles)
        {
            Rectangle(_hdc, rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        }
    }

    private bool TrackMouse()
    {
        POINT startPoint = new POINT();
        GetCursorPos(ref startPoint);
        ScreenToClient(_parentHwnd, ref startPoint);

        Rectangle[] originalRects = (Rectangle[])_rectangles.Clone();

        while (_tracking)
        {
            // Process messages
            MSG msg = new MSG();
            if (PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE))
            {
                if (msg.message == WM_LBUTTONUP)
                {
                    return true; // Completed successfully
                }
                else if (msg.message == WM_KEYDOWN && msg.wParam.ToInt32() == VK_ESCAPE)
                {
                    // Restore original rectangles
                    DrawRectangles(); // Erase current
                    _rectangles = originalRects;
                    DrawRectangles(); // Draw original
                    return false; // Cancelled
                }
                else if (msg.message == WM_MOUSEMOVE)
                {
                    POINT currentPoint = new POINT();
                    GetCursorPos(ref currentPoint);
                    ScreenToClient(_parentHwnd, ref currentPoint);

                    int dx = currentPoint.X - startPoint.X;
                    int dy = currentPoint.Y - startPoint.Y;

                    // Erase old rectangles
                    DrawRectangles();

                    // Update rectangles
                    for (int i = 0; i < _rectangles.Length; i++)
                    {
                        if ((_style & SWT.RESIZE) != 0)
                        {
                            // Resize mode
                            _rectangles[i] = new Rectangle(
                                originalRects[i].X,
                                originalRects[i].Y,
                                originalRects[i].Width + dx,
                                originalRects[i].Height + dy
                            );

                            var args = new TrackerEventArgs
                            {
                                X = currentPoint.X,
                                Y = currentPoint.Y,
                                Bounds = _rectangles[i]
                            };
                            Resize?.Invoke(this, args);
                        }
                        else
                        {
                            // Move mode
                            _rectangles[i] = new Rectangle(
                                originalRects[i].X + dx,
                                originalRects[i].Y + dy,
                                originalRects[i].Width,
                                originalRects[i].Height
                            );
                        }
                    }

                    // Draw new rectangles
                    DrawRectangles();

                    var moveArgs = new TrackerEventArgs
                    {
                        X = currentPoint.X,
                        Y = currentPoint.Y,
                        Bounds = _rectangles.Length > 0 ? _rectangles[0] : new Rectangle()
                    };
                    MouseMove?.Invoke(this, moveArgs);
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
            else
            {
                // Wait for next message
                WaitMessage();
            }
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Close();
        System.GC.SuppressFinalize(this);
    }

    // Win32 API declarations
    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern int SetROP2(IntPtr hdc, int fnDrawMode);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern IntPtr GetStockObject(int fnObject);

    [DllImport("gdi32.dll")]
    private static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);

    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    [DllImport("user32.dll")]
    private static extern bool TranslateMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern IntPtr DispatchMessage(ref MSG lpMsg);

    [DllImport("user32.dll")]
    private static extern bool WaitMessage();

    private const uint PM_REMOVE = 0x0001;

    private static int RGB(int r, int g, int b)
    {
        return r | (g << 8) | (b << 16);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

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
}
