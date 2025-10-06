using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Canvas widget methods.
/// </summary>
internal partial class Win32Platform
{
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

    // InvalidateRect with RECT parameter
    [DllImport(User32, SetLastError = true)]
    private static extern bool InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

    // CreateSolidBrush - using Gdi32
    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateSolidBrush(uint color);

    // Canvas data structure
    private class CanvasData
    {
        public Action<int, int, int, int, object?>? PaintCallback { get; set; }
        public Graphics.RGB BackgroundColor { get; set; }
        public IntPtr BackgroundBrush { get; set; }
    }

    private readonly Dictionary<IntPtr, CanvasData> _canvasData = new Dictionary<IntPtr, CanvasData>();

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
            throw new System.ComponentModel.Win32Exception(error, $"Failed to create canvas window. Error: {error}");
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

        // Invalidate the entire client area (using IntPtr version from Win32Platform_Label.cs)
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

    public void SetCanvasBackground(IntPtr handle, Graphics.RGB color)
    {
        if (handle == IntPtr.Zero)
            return;

        if (_canvasData.TryGetValue(handle, out var canvasData))
        {
            // Delete old brush
            if (canvasData.BackgroundBrush != IntPtr.Zero)
            {
                DeleteObject(canvasData.BackgroundBrush);
            }

            // Create new brush with the new color (BGR format)
            uint colorRef = (uint)(color.Blue << 16 | color.Green << 8 | color.Red);
            canvasData.BackgroundColor = color;
            canvasData.BackgroundBrush = CreateSolidBrush(colorRef);

            // Invalidate to trigger repaint
            RedrawCanvas(handle);
        }
    }
}
