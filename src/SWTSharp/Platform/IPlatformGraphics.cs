namespace SWTSharp.Platform;

/// <summary>
/// Interface for platform-specific graphics operations.
/// </summary>
internal interface IPlatformGraphics : IPlatform
{
    // Color operations
    IntPtr CreateColor(int red, int green, int blue);
    void DestroyColor(IntPtr handle);

    // Font operations
    IntPtr CreateFont(string name, int height, int style);
    void DestroyFont(IntPtr handle);

    // Image operations
    IntPtr CreateImage(int width, int height);
    (IntPtr Handle, int Width, int Height) LoadImage(string filename);
    void DestroyImage(IntPtr handle);

    // Graphics Context operations
    IntPtr CreateGraphicsContext(IntPtr drawable);
    IntPtr CreateGraphicsContextForImage(IntPtr imageHandle);
    void DestroyGraphicsContext(IntPtr gcHandle);

    // GC state operations
    void SetGCForeground(IntPtr gcHandle, IntPtr colorHandle);
    void SetGCBackground(IntPtr gcHandle, IntPtr colorHandle);
    void SetGCFont(IntPtr gcHandle, IntPtr fontHandle);
    void SetGCLineWidth(IntPtr gcHandle, int width);
    void SetGCLineStyle(IntPtr gcHandle, int style);
    void SetGCAlpha(IntPtr gcHandle, int alpha);
    void SetGCClipping(IntPtr gcHandle, int x, int y, int width, int height);
    void ResetGCClipping(IntPtr gcHandle);

    // Drawing operations
    void DrawLine(IntPtr gcHandle, int x1, int y1, int x2, int y2);
    void DrawRectangle(IntPtr gcHandle, int x, int y, int width, int height);
    void FillRectangle(IntPtr gcHandle, int x, int y, int width, int height);
    void DrawOval(IntPtr gcHandle, int x, int y, int width, int height);
    void FillOval(IntPtr gcHandle, int x, int y, int width, int height);
    void DrawPolygon(IntPtr gcHandle, int[] pointArray);
    void FillPolygon(IntPtr gcHandle, int[] pointArray);
    void DrawPolyline(IntPtr gcHandle, int[] pointArray);
    void DrawArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle);
    void FillArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle);
    void DrawRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight);
    void FillRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight);

    // Text operations
    void DrawText(IntPtr gcHandle, string text, int x, int y, bool isTransparent);
    (int Width, int Height) GetTextExtent(IntPtr gcHandle, string text);
    int GetCharWidth(IntPtr gcHandle, char ch);

    // Image operations
    void DrawImage(IntPtr gcHandle, IntPtr imageHandle, int x, int y);
    void DrawImageScaled(IntPtr gcHandle, IntPtr imageHandle,
        int srcX, int srcY, int srcWidth, int srcHeight,
        int destX, int destY, int destWidth, int destHeight);

    // Advanced operations
    void CopyArea(IntPtr gcHandle, int srcX, int srcY, int width, int height, int destX, int destY);
}
