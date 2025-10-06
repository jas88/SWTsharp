using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Win32 Graphics Platform Implementation - GDI Operations
/// </summary>
internal partial class Win32Platform : IPlatformGraphics
{
    private const string Gdi32 = "gdi32.dll";
    private const string Msimg32 = "msimg32.dll";

    // GDI Object Types
    private const int OBJ_PEN = 1;
    private const int OBJ_BRUSH = 2;
    private const int OBJ_FONT = 6;

    // Stock Objects
    private const int NULL_BRUSH = 5;
    private const int NULL_PEN = 8;
    private const int DC_BRUSH = 18;
    private const int DC_PEN = 19;

    // Pen Styles
    private const int PS_SOLID = 0;
    private const int PS_DASH = 1;
    private const int PS_DOT = 2;
    private const int PS_DASHDOT = 3;
    private const int PS_DASHDOTDOT = 4;

    // Background modes
    private const int TRANSPARENT = 1;
    private const int OPAQUE = 2;

    // Ternary raster operations
    private const int SRCCOPY = 0x00CC0020;
    private const int SRCPAINT = 0x00EE0086;
    private const int SRCAND = 0x008800C6;

    // Alpha blend operations
    private const byte AC_SRC_OVER = 0x00;
    private const byte AC_SRC_ALPHA = 0x01;

    [StructLayout(LayoutKind.Sequential)]
    private struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE
    {
        public int cx;
        public int cy;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct TEXTMETRIC
    {
        public int tmHeight;
        public int tmAscent;
        public int tmDescent;
        public int tmInternalLeading;
        public int tmExternalLeading;
        public int tmAveCharWidth;
        public int tmMaxCharWidth;
        public int tmWeight;
        public int tmOverhang;
        public int tmDigitizedAspectX;
        public int tmDigitizedAspectY;
        public char tmFirstChar;
        public char tmLastChar;
        public char tmDefaultChar;
        public char tmBreakChar;
        public byte tmItalic;
        public byte tmUnderlined;
        public byte tmStruckOut;
        public byte tmPitchAndFamily;
        public byte tmCharSet;
    }

    // GDI function imports
#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial IntPtr GetDC(IntPtr hWnd);
#else
    [DllImport(User32)]
    private static extern IntPtr GetDC(IntPtr hWnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32)]
    private static partial int ReleaseDC(IntPtr hWnd, IntPtr hDC);
#else
    [DllImport(User32)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial IntPtr CreateCompatibleDC(IntPtr hDC);
#else
    [DllImport(Gdi32)]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(IntPtr hDC);
#else
    [DllImport(Gdi32)]
    private static extern bool DeleteDC(IntPtr hDC);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
#else
    [DllImport(Gdi32)]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);
#else
    [DllImport(Gdi32)]
    private static extern IntPtr CreatePen(int fnPenStyle, int nWidth, int crColor);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial IntPtr CreateSolidBrush(int crColor);
#else
    [DllImport(Gdi32)]
    private static extern IntPtr CreateSolidBrush(int crColor);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial int SetBkMode(IntPtr hDC, int iBkMode);
#else
    [DllImport(Gdi32)]
    private static extern int SetBkMode(IntPtr hDC, int iBkMode);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial int SetTextColor(IntPtr hDC, int crColor);
#else
    [DllImport(Gdi32)]
    private static extern int SetTextColor(IntPtr hDC, int crColor);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial int SetBkColor(IntPtr hDC, int crColor);
#else
    [DllImport(Gdi32)]
    private static extern int SetBkColor(IntPtr hDC, int crColor);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool MoveToEx(IntPtr hDC, int x, int y, IntPtr lpPoint);
#else
    [DllImport(Gdi32)]
    private static extern bool MoveToEx(IntPtr hDC, int x, int y, IntPtr lpPoint);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool LineTo(IntPtr hDC, int x, int y);
#else
    [DllImport(Gdi32)]
    private static extern bool LineTo(IntPtr hDC, int x, int y);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Rectangle(IntPtr hDC, int left, int top, int right, int bottom);
#else
    [DllImport(Gdi32)]
    private static extern bool Rectangle(IntPtr hDC, int left, int top, int right, int bottom);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Ellipse(IntPtr hDC, int left, int top, int right, int bottom);
#else
    [DllImport(Gdi32)]
    private static extern bool Ellipse(IntPtr hDC, int left, int top, int right, int bottom);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Polygon(IntPtr hDC, POINT[] lpPoints, int nCount);
#else
    [DllImport(Gdi32)]
    private static extern bool Polygon(IntPtr hDC, POINT[] lpPoints, int nCount);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Polyline(IntPtr hDC, POINT[] lpPoints, int nCount);
#else
    [DllImport(Gdi32)]
    private static extern bool Polyline(IntPtr hDC, POINT[] lpPoints, int nCount);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool RoundRect(IntPtr hDC, int left, int top, int right, int bottom, int width, int height);
#else
    [DllImport(Gdi32)]
    private static extern bool RoundRect(IntPtr hDC, int left, int top, int right, int bottom, int width, int height);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Arc(IntPtr hDC, int left, int top, int right, int bottom, int xStart, int yStart, int xEnd, int yEnd);
#else
    [DllImport(Gdi32)]
    private static extern bool Arc(IntPtr hDC, int left, int top, int right, int bottom, int xStart, int yStart, int xEnd, int yEnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Pie(IntPtr hDC, int left, int top, int right, int bottom, int xStart, int yStart, int xEnd, int yEnd);
#else
    [DllImport(Gdi32)]
    private static extern bool Pie(IntPtr hDC, int left, int top, int right, int bottom, int xStart, int yStart, int xEnd, int yEnd);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool TextOut(IntPtr hDC, int x, int y, string lpString, int c);
#else
    [DllImport(Gdi32, CharSet = CharSet.Unicode)]
    private static extern bool TextOut(IntPtr hDC, int x, int y, string lpString, int c);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetTextExtentPoint32(IntPtr hDC, string lpString, int c, out SIZE psizl);
#else
    [DllImport(Gdi32, CharSet = CharSet.Unicode)]
    private static extern bool GetTextExtentPoint32(IntPtr hDC, string lpString, int c, out SIZE psizl);
#endif

    // Note: GetTextMetrics uses complex struct, use DllImport for all targets
    [DllImport(Gdi32, CharSet = CharSet.Unicode)]
    private static extern bool GetTextMetrics(IntPtr hDC, out TEXTMETRIC lptm);

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);
#else
    [DllImport(Gdi32)]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, int rop);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, int rop);
#else
    [DllImport(Gdi32)]
    private static extern bool StretchBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, int rop);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Msimg32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AlphaBlend(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, BLENDFUNCTION blendFunction);
#else
    [DllImport(Msimg32)]
    private static extern bool AlphaBlend(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
        IntPtr hdcSrc, int xSrc, int ySrc, int wSrc, int hSrc, BLENDFUNCTION blendFunction);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial IntPtr GetStockObject(int i);
#else
    [DllImport(Gdi32)]
    private static extern IntPtr GetStockObject(int i);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);
#else
    [DllImport(Gdi32)]
    private static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Gdi32)]
    private static partial int SelectClipRgn(IntPtr hDC, IntPtr hRgn);
#else
    [DllImport(Gdi32)]
    private static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn);
#endif

    // GC Handle to window mapping
    private readonly Dictionary<IntPtr, IntPtr> _gcToWindow = new();
    private readonly Dictionary<IntPtr, GCState> _gcStates = new();

    private sealed class GCState
    {
        public IntPtr Pen = IntPtr.Zero;
        public IntPtr Brush = IntPtr.Zero;
        public IntPtr Font = IntPtr.Zero;
        public int ForegroundColor = 0x000000; // Black
        public int BackgroundColor = 0xFFFFFF; // White
        public int LineWidth = 1;
        public int LineStyle = SWT.LINE_SOLID;
        public int Alpha = 255;
        public IntPtr ClipRegion = IntPtr.Zero;
    }

    // IPlatformGraphics implementation
    public IntPtr CreateColor(int red, int green, int blue)
    {
        // On Windows, colors are represented as COLORREF (0x00BBGGRR)
        int colorRef = (blue << 16) | (green << 8) | red;
        return new IntPtr(colorRef);
    }

    public void DestroyColor(IntPtr handle)
    {
        // Colors on Windows don't need explicit cleanup
    }

    public IntPtr CreateFont(string name, int height, int style)
    {
        // TODO: Implement font creation using CreateFont Win32 API
        // For now, return default system font
        return GetStockObject(13); // DEFAULT_GUI_FONT
    }

    public void DestroyFont(IntPtr handle)
    {
        // TODO: Implement font destruction
        // Don't delete stock objects
    }

    public IntPtr CreateImage(int width, int height)
    {
        // TODO: Implement image creation using CreateDIBSection
        throw new NotImplementedException("Image creation not yet implemented on Win32 platform");
    }

    public (IntPtr Handle, int Width, int Height) LoadImage(string filename)
    {
        // TODO: Implement image loading
        throw new NotImplementedException("Image loading not yet implemented on Win32 platform");
    }

    public void DestroyImage(IntPtr handle)
    {
        // TODO: Implement image destruction
    }

    public IntPtr CreateGraphicsContext(IntPtr drawable)
    {
        IntPtr hdc = GetDC(drawable);
        if (hdc == IntPtr.Zero)
            throw new SWTException(SWT.ERROR_NO_HANDLES, "Failed to create graphics context");

        _gcToWindow[hdc] = drawable;
        _gcStates[hdc] = new GCState();

        return hdc;
    }

    public IntPtr CreateGraphicsContextForImage(IntPtr imageHandle)
    {
        IntPtr hdc = CreateCompatibleDC(IntPtr.Zero);
        if (hdc == IntPtr.Zero)
            throw new SWTException(SWT.ERROR_NO_HANDLES, "Failed to create graphics context for image");

        SelectObject(hdc, imageHandle);
        _gcStates[hdc] = new GCState();

        return hdc;
    }

    public void DestroyGraphicsContext(IntPtr gcHandle)
    {
        if (gcHandle == IntPtr.Zero) return;

        // Clean up GC state resources
        if (_gcStates.TryGetValue(gcHandle, out var state))
        {
            if (state.Pen != IntPtr.Zero)
                DeleteObject(state.Pen);
            if (state.Brush != IntPtr.Zero)
                DeleteObject(state.Brush);
            if (state.ClipRegion != IntPtr.Zero)
                DeleteObject(state.ClipRegion);

            _gcStates.Remove(gcHandle);
        }

        // Release or delete DC
        if (_gcToWindow.TryGetValue(gcHandle, out var hwnd))
        {
            ReleaseDC(hwnd, gcHandle);
            _gcToWindow.Remove(gcHandle);
        }
        else
        {
            DeleteDC(gcHandle);
        }
    }

    public void SetGCForeground(IntPtr gcHandle, IntPtr colorHandle)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        state.ForegroundColor = colorHandle.ToInt32();
        UpdatePen(gcHandle, state);
        SetTextColor(gcHandle, state.ForegroundColor);
    }

    public void SetGCBackground(IntPtr gcHandle, IntPtr colorHandle)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        state.BackgroundColor = colorHandle.ToInt32();
        UpdateBrush(gcHandle, state);
        SetBkColor(gcHandle, state.BackgroundColor);
    }

    public void SetGCFont(IntPtr gcHandle, IntPtr fontHandle)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        state.Font = fontHandle;
        SelectObject(gcHandle, fontHandle);
    }

    public void SetGCLineWidth(IntPtr gcHandle, int width)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        state.LineWidth = width;
        UpdatePen(gcHandle, state);
    }

    public void SetGCLineStyle(IntPtr gcHandle, int style)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        state.LineStyle = style;
        UpdatePen(gcHandle, state);
    }

    public void SetGCAlpha(IntPtr gcHandle, int alpha)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        state.Alpha = alpha;
    }

    public void SetGCClipping(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        // Remove old clipping region
        if (state.ClipRegion != IntPtr.Zero)
        {
            DeleteObject(state.ClipRegion);
        }

        // Create new clipping region
        state.ClipRegion = CreateRectRgn(x, y, x + width, y + height);
        SelectClipRgn(gcHandle, state.ClipRegion);
    }

    public void ResetGCClipping(IntPtr gcHandle)
    {
        if (!_gcStates.TryGetValue(gcHandle, out var state))
            return;

        if (state.ClipRegion != IntPtr.Zero)
        {
            DeleteObject(state.ClipRegion);
            state.ClipRegion = IntPtr.Zero;
        }

        SelectClipRgn(gcHandle, IntPtr.Zero);
    }

    private void UpdatePen(IntPtr hdc, GCState state)
    {
        // Delete old pen
        if (state.Pen != IntPtr.Zero)
            DeleteObject(state.Pen);

        // Map SWT line style to Win32 pen style
        int penStyle = state.LineStyle switch
        {
            SWT.LINE_SOLID => PS_SOLID,
            SWT.LINE_DASH => PS_DASH,
            SWT.LINE_DOT => PS_DOT,
            SWT.LINE_DASHDOT => PS_DASHDOT,
            SWT.LINE_DASHDOTDOT => PS_DASHDOTDOT,
            _ => PS_SOLID
        };

        // Create new pen
        state.Pen = CreatePen(penStyle, state.LineWidth, state.ForegroundColor);
        SelectObject(hdc, state.Pen);
    }

    private void UpdateBrush(IntPtr hdc, GCState state)
    {
        // Delete old brush
        if (state.Brush != IntPtr.Zero)
            DeleteObject(state.Brush);

        // Create new brush
        state.Brush = CreateSolidBrush(state.BackgroundColor);
        SelectObject(hdc, state.Brush);
    }

    // Drawing operations
    public void DrawLine(IntPtr gcHandle, int x1, int y1, int x2, int y2)
    {
        MoveToEx(gcHandle, x1, y1, IntPtr.Zero);
        LineTo(gcHandle, x2, y2);
    }

    public void DrawRectangle(IntPtr gcHandle, int x, int y, int width, int height)
    {
        // Save current brush and use null brush for outline only
        IntPtr oldBrush = SelectObject(gcHandle, GetStockObject(NULL_BRUSH));
        Rectangle(gcHandle, x, y, x + width, y + height);
        SelectObject(gcHandle, oldBrush);
    }

    public void FillRectangle(IntPtr gcHandle, int x, int y, int width, int height)
    {
        // Ensure brush is set
        if (_gcStates.TryGetValue(gcHandle, out var state))
        {
            UpdateBrush(gcHandle, state);
        }

        // Save current pen and use null pen for fill only
        IntPtr oldPen = SelectObject(gcHandle, GetStockObject(NULL_PEN));
        Rectangle(gcHandle, x, y, x + width, y + height);
        SelectObject(gcHandle, oldPen);
    }

    public void DrawOval(IntPtr gcHandle, int x, int y, int width, int height)
    {
        IntPtr oldBrush = SelectObject(gcHandle, GetStockObject(NULL_BRUSH));
        Ellipse(gcHandle, x, y, x + width, y + height);
        SelectObject(gcHandle, oldBrush);
    }

    public void FillOval(IntPtr gcHandle, int x, int y, int width, int height)
    {
        if (_gcStates.TryGetValue(gcHandle, out var state))
        {
            UpdateBrush(gcHandle, state);
        }

        IntPtr oldPen = SelectObject(gcHandle, GetStockObject(NULL_PEN));
        Ellipse(gcHandle, x, y, x + width, y + height);
        SelectObject(gcHandle, oldPen);
    }

    public void DrawPolygon(IntPtr gcHandle, int[] pointArray)
    {
        POINT[] points = ConvertToPointArray(pointArray);
        IntPtr oldBrush = SelectObject(gcHandle, GetStockObject(NULL_BRUSH));
        Polygon(gcHandle, points, points.Length);
        SelectObject(gcHandle, oldBrush);
    }

    public void FillPolygon(IntPtr gcHandle, int[] pointArray)
    {
        POINT[] points = ConvertToPointArray(pointArray);
        if (_gcStates.TryGetValue(gcHandle, out var state))
        {
            UpdateBrush(gcHandle, state);
        }

        IntPtr oldPen = SelectObject(gcHandle, GetStockObject(NULL_PEN));
        Polygon(gcHandle, points, points.Length);
        SelectObject(gcHandle, oldPen);
    }

    public void DrawPolyline(IntPtr gcHandle, int[] pointArray)
    {
        POINT[] points = ConvertToPointArray(pointArray);
        Polyline(gcHandle, points, points.Length);
    }

    public void DrawArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        (int xStart, int yStart, int xEnd, int yEnd) = CalculateArcPoints(x, y, width, height, startAngle, arcAngle);
        IntPtr oldBrush = SelectObject(gcHandle, GetStockObject(NULL_BRUSH));
        Arc(gcHandle, x, y, x + width, y + height, xStart, yStart, xEnd, yEnd);
        SelectObject(gcHandle, oldBrush);
    }

    public void FillArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        (int xStart, int yStart, int xEnd, int yEnd) = CalculateArcPoints(x, y, width, height, startAngle, arcAngle);

        if (_gcStates.TryGetValue(gcHandle, out var state))
        {
            UpdateBrush(gcHandle, state);
        }

        IntPtr oldPen = SelectObject(gcHandle, GetStockObject(NULL_PEN));
        Pie(gcHandle, x, y, x + width, y + height, xStart, yStart, xEnd, yEnd);
        SelectObject(gcHandle, oldPen);
    }

    public void DrawRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        IntPtr oldBrush = SelectObject(gcHandle, GetStockObject(NULL_BRUSH));
        RoundRect(gcHandle, x, y, x + width, y + height, arcWidth, arcHeight);
        SelectObject(gcHandle, oldBrush);
    }

    public void FillRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        if (_gcStates.TryGetValue(gcHandle, out var state))
        {
            UpdateBrush(gcHandle, state);
        }

        IntPtr oldPen = SelectObject(gcHandle, GetStockObject(NULL_PEN));
        RoundRect(gcHandle, x, y, x + width, y + height, arcWidth, arcHeight);
        SelectObject(gcHandle, oldPen);
    }

    public void DrawText(IntPtr gcHandle, string text, int x, int y, bool isTransparent)
    {
        SetBkMode(gcHandle, isTransparent ? TRANSPARENT : OPAQUE);
        TextOut(gcHandle, x, y, text, text.Length);
    }

    public (int Width, int Height) GetTextExtent(IntPtr gcHandle, string text)
    {
        SIZE size;
        GetTextExtentPoint32(gcHandle, text, text.Length, out size);
        return (size.cx, size.cy);
    }

    public int GetCharWidth(IntPtr gcHandle, char ch)
    {
        SIZE size;
        GetTextExtentPoint32(gcHandle, ch.ToString(), 1, out size);
        return size.cx;
    }

    public void DrawImage(IntPtr gcHandle, IntPtr imageHandle, int x, int y)
    {
        // TODO: Implement image drawing
        // This requires creating a compatible DC and using BitBlt
        throw new NotImplementedException("Image drawing not yet implemented on Win32 platform");
    }

    public void DrawImageScaled(IntPtr gcHandle, IntPtr imageHandle,
        int srcX, int srcY, int srcWidth, int srcHeight,
        int destX, int destY, int destWidth, int destHeight)
    {
        // TODO: Implement scaled image drawing using StretchBlt
        throw new NotImplementedException("Scaled image drawing not yet implemented on Win32 platform");
    }

    public void CopyArea(IntPtr gcHandle, int srcX, int srcY, int width, int height, int destX, int destY)
    {
        BitBlt(gcHandle, destX, destY, width, height, gcHandle, srcX, srcY, SRCCOPY);
    }

    // Helper methods
    private POINT[] ConvertToPointArray(int[] pointArray)
    {
        int count = pointArray.Length / 2;
        POINT[] points = new POINT[count];

        for (int i = 0; i < count; i++)
        {
            points[i] = new POINT
            {
                X = pointArray[i * 2],
                Y = pointArray[i * 2 + 1]
            };
        }

        return points;
    }

    private (int xStart, int yStart, int xEnd, int yEnd) CalculateArcPoints(
        int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        // Convert angles to radians (SWT uses degrees, 0 is at 3 o'clock, positive is counter-clockwise)
        double startRad = startAngle * Math.PI / 180.0;
        double endRad = (startAngle + arcAngle) * Math.PI / 180.0;

        // Calculate center and radii
        int centerX = x + width / 2;
        int centerY = y + height / 2;
        int radiusX = width / 2;
        int radiusY = height / 2;

        // Calculate start and end points
        int xStart = centerX + (int)(radiusX * Math.Cos(startRad));
        int yStart = centerY - (int)(radiusY * Math.Sin(startRad));
        int xEnd = centerX + (int)(radiusX * Math.Cos(endRad));
        int yEnd = centerY - (int)(radiusY * Math.Sin(endRad));

        return (xStart, yStart, xEnd, yEnd);
    }
}
