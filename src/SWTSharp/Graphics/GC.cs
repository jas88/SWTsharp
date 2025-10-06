using SWTSharp.Platform;

namespace SWTSharp.Graphics;

/// <summary>
/// Graphics Context (GC) class for drawing operations.
/// Provides a device-independent interface for 2D graphics rendering.
/// </summary>
public class GC : Resource
{
    private Color? foreground;
    private Color? background;
    private Font? font;
    private int lineWidth = 1;
    private int lineStyle = SWT.LINE_SOLID;
    private int alpha = 255;
    private Rectangle? clipping;
    private readonly IntPtr drawable;

    /// <summary>
    /// Gets or sets the foreground color used for drawing operations.
    /// </summary>
    public Color? Foreground
    {
        get
        {
            CheckDisposed();
            return foreground;
        }
        set
        {
            CheckDisposed();
            if (value != null && value.Device != Device)
                throw new ArgumentException("Color must be created on the same device as the GC");

            foreground = value;
            if (Handle != IntPtr.Zero && value != null)
            {
                PlatformGraphics.SetGCForeground(Handle, value.Handle);
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color used for drawing operations.
    /// </summary>
    public Color? Background
    {
        get
        {
            CheckDisposed();
            return background;
        }
        set
        {
            CheckDisposed();
            if (value != null && value.Device != Device)
                throw new ArgumentException("Color must be created on the same device as the GC");

            background = value;
            if (Handle != IntPtr.Zero && value != null)
            {
                PlatformGraphics.SetGCBackground(Handle, value.Handle);
            }
        }
    }

    /// <summary>
    /// Gets or sets the font used for text drawing operations.
    /// </summary>
    public Font? Font
    {
        get
        {
            CheckDisposed();
            return font;
        }
        set
        {
            CheckDisposed();
            if (value != null && value.Device != Device)
                throw new ArgumentException("Font must be created on the same device as the GC");

            font = value;
            if (Handle != IntPtr.Zero && value != null)
            {
                PlatformGraphics.SetGCFont(Handle, value.Handle);
            }
        }
    }

    /// <summary>
    /// Gets or sets the line width for drawing operations.
    /// </summary>
    public int LineWidth
    {
        get
        {
            CheckDisposed();
            return lineWidth;
        }
        set
        {
            CheckDisposed();
            if (value < 0)
                throw new ArgumentException("Line width cannot be negative", nameof(value));

            lineWidth = value;
            if (Handle != IntPtr.Zero)
            {
                PlatformGraphics.SetGCLineWidth(Handle, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the line style for drawing operations.
    /// </summary>
    public int LineStyle
    {
        get
        {
            CheckDisposed();
            return lineStyle;
        }
        set
        {
            CheckDisposed();
            lineStyle = value;
            if (Handle != IntPtr.Zero)
            {
                PlatformGraphics.SetGCLineStyle(Handle, value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the alpha (transparency) value for drawing operations.
    /// Valid range: 0 (transparent) to 255 (opaque).
    /// </summary>
    public int Alpha
    {
        get
        {
            CheckDisposed();
            return alpha;
        }
        set
        {
            CheckDisposed();
            if (value < 0 || value > 255)
                throw new ArgumentException("Alpha must be between 0 and 255", nameof(value));

            alpha = value;
            if (Handle != IntPtr.Zero)
            {
                PlatformGraphics.SetGCAlpha(Handle, value);
            }
        }
    }

    /// <summary>
    /// Gets the clipping region.
    /// </summary>
    public Rectangle? Clipping
    {
        get
        {
            CheckDisposed();
            return clipping;
        }
    }

    private IPlatformGraphics PlatformGraphics
    {
        get
        {
            var platform = Device.Platform as IPlatformGraphics;
            if (platform == null)
                throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Platform does not support graphics operations");
            return platform;
        }
    }

    /// <summary>
    /// Initializes a new instance of the GC class for a drawable object.
    /// </summary>
    /// <param name="device">The device to create the GC on</param>
    /// <param name="drawable">Platform-specific drawable handle (HWND, window, etc.)</param>
    public GC(Device device, IntPtr drawable)
        : base(device)
    {
        if (drawable == IntPtr.Zero)
            throw new ArgumentException("Drawable handle cannot be zero", nameof(drawable));

        this.drawable = drawable;
        Handle = PlatformGraphics.CreateGraphicsContext(drawable);

        // Initialize default graphics state
        InitializeDefaults();
    }

    /// <summary>
    /// Initializes a new instance of the GC class for an image.
    /// </summary>
    /// <param name="image">The image to draw on</param>
    public GC(Image image)
        : base(image.Device)
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (image.IsDisposed)
            throw new ObjectDisposedException(nameof(image));

        this.drawable = image.Handle;
        Handle = PlatformGraphics.CreateGraphicsContextForImage(image.Handle);

        // Initialize default graphics state
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        // Set default colors if available
        try
        {
            // Attempt to create default colors (black foreground, white background)
            foreground = new Color(Device, 0, 0, 0);
            background = new Color(Device, 255, 255, 255);

            if (Handle != IntPtr.Zero)
            {
                PlatformGraphics.SetGCForeground(Handle, foreground.Handle);
                PlatformGraphics.SetGCBackground(Handle, background.Handle);
            }
        }
        catch
        {
            // If color creation fails, continue with null colors
        }
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    public void DrawLine(int x1, int y1, int x2, int y2)
    {
        CheckDisposed();
        PlatformGraphics.DrawLine(Handle, x1, y1, x2, y2);
    }

    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    public void DrawRectangle(int x, int y, int width, int height)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.DrawRectangle(Handle, x, y, width, height);
    }

    /// <summary>
    /// Draws the outline of a rectangle.
    /// </summary>
    public void DrawRectangle(Rectangle rect)
    {
        DrawRectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Fills a rectangle with the current background color.
    /// </summary>
    public void FillRectangle(int x, int y, int width, int height)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.FillRectangle(Handle, x, y, width, height);
    }

    /// <summary>
    /// Fills a rectangle with the current background color.
    /// </summary>
    public void FillRectangle(Rectangle rect)
    {
        FillRectangle(rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Draws the outline of an oval within the specified bounding rectangle.
    /// </summary>
    public void DrawOval(int x, int y, int width, int height)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.DrawOval(Handle, x, y, width, height);
    }

    /// <summary>
    /// Fills an oval within the specified bounding rectangle.
    /// </summary>
    public void FillOval(int x, int y, int width, int height)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.FillOval(Handle, x, y, width, height);
    }

    /// <summary>
    /// Draws the outline of a polygon defined by an array of points.
    /// </summary>
    public void DrawPolygon(int[] pointArray)
    {
        CheckDisposed();
        if (pointArray == null)
            throw new ArgumentNullException(nameof(pointArray));
        if (pointArray.Length < 6 || pointArray.Length % 2 != 0)
            throw new ArgumentException("Point array must contain at least 3 points (6 values) and have even length");

        PlatformGraphics.DrawPolygon(Handle, pointArray);
    }

    /// <summary>
    /// Fills a polygon defined by an array of points.
    /// </summary>
    public void FillPolygon(int[] pointArray)
    {
        CheckDisposed();
        if (pointArray == null)
            throw new ArgumentNullException(nameof(pointArray));
        if (pointArray.Length < 6 || pointArray.Length % 2 != 0)
            throw new ArgumentException("Point array must contain at least 3 points (6 values) and have even length");

        PlatformGraphics.FillPolygon(Handle, pointArray);
    }

    /// <summary>
    /// Draws a polyline (connected line segments) through the specified points.
    /// </summary>
    public void DrawPolyline(int[] pointArray)
    {
        CheckDisposed();
        if (pointArray == null)
            throw new ArgumentNullException(nameof(pointArray));
        if (pointArray.Length < 4 || pointArray.Length % 2 != 0)
            throw new ArgumentException("Point array must contain at least 2 points (4 values) and have even length");

        PlatformGraphics.DrawPolyline(Handle, pointArray);
    }

    /// <summary>
    /// Draws an arc within the specified bounding rectangle.
    /// </summary>
    /// <param name="x">X coordinate of bounding rectangle</param>
    /// <param name="y">Y coordinate of bounding rectangle</param>
    /// <param name="width">Width of bounding rectangle</param>
    /// <param name="height">Height of bounding rectangle</param>
    /// <param name="startAngle">Starting angle in degrees (0 is at 3 o'clock)</param>
    /// <param name="arcAngle">Arc angle in degrees (positive is counter-clockwise)</param>
    public void DrawArc(int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.DrawArc(Handle, x, y, width, height, startAngle, arcAngle);
    }

    /// <summary>
    /// Fills an arc within the specified bounding rectangle.
    /// </summary>
    public void FillArc(int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.FillArc(Handle, x, y, width, height, startAngle, arcAngle);
    }

    /// <summary>
    /// Draws a rounded rectangle with the specified arc width and height.
    /// </summary>
    public void DrawRoundRectangle(int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");
        if (arcWidth < 0 || arcHeight < 0)
            throw new ArgumentException("Arc width and height must be non-negative");

        PlatformGraphics.DrawRoundRectangle(Handle, x, y, width, height, arcWidth, arcHeight);
    }

    /// <summary>
    /// Fills a rounded rectangle with the specified arc width and height.
    /// </summary>
    public void FillRoundRectangle(int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");
        if (arcWidth < 0 || arcHeight < 0)
            throw new ArgumentException("Arc width and height must be non-negative");

        PlatformGraphics.FillRoundRectangle(Handle, x, y, width, height, arcWidth, arcHeight);
    }

    /// <summary>
    /// Draws text at the specified location with transparency control.
    /// </summary>
    /// <param name="text">The text to draw</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="isTransparent">If true, background is not filled</param>
    public void DrawText(string text, int x, int y, bool isTransparent)
    {
        CheckDisposed();
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        PlatformGraphics.DrawText(Handle, text, x, y, isTransparent);
    }

    /// <summary>
    /// Draws text at the specified location (opaque background).
    /// </summary>
    public void DrawText(string text, int x, int y)
    {
        DrawText(text, x, y, false);
    }

    /// <summary>
    /// Draws text at the specified location with specified flags.
    /// </summary>
    /// <param name="text">The text to draw</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="flags">Drawing flags (SWT.DRAW_DELIMITER, SWT.DRAW_TAB, SWT.DRAW_MNEMONIC, SWT.DRAW_TRANSPARENT)</param>
    public void DrawText(string text, int x, int y, int flags)
    {
        CheckDisposed();
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        bool isTransparent = (flags & SWT.DRAW_TRANSPARENT) != 0;
        PlatformGraphics.DrawText(Handle, text, x, y, isTransparent);
    }

    /// <summary>
    /// Draws a string at the specified location (always transparent background).
    /// </summary>
    public void DrawString(string text, int x, int y)
    {
        DrawText(text, x, y, true);
    }

    /// <summary>
    /// Draws a string at the specified location with transparency control.
    /// </summary>
    public void DrawString(string text, int x, int y, bool isTransparent)
    {
        DrawText(text, x, y, isTransparent);
    }

    /// <summary>
    /// Draws an image at the specified location.
    /// </summary>
    public void DrawImage(Image image, int x, int y)
    {
        CheckDisposed();
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (image.IsDisposed)
            throw new ObjectDisposedException(nameof(image));
        PlatformGraphics.DrawImage(Handle, image.Handle, x, y);
    }

    /// <summary>
    /// Draws a portion of an image at the specified location with scaling.
    /// </summary>
    /// <param name="image">The image to draw</param>
    /// <param name="srcX">Source X coordinate within the image</param>
    /// <param name="srcY">Source Y coordinate within the image</param>
    /// <param name="srcWidth">Source width within the image</param>
    /// <param name="srcHeight">Source height within the image</param>
    /// <param name="destX">Destination X coordinate</param>
    /// <param name="destY">Destination Y coordinate</param>
    /// <param name="destWidth">Destination width (for scaling)</param>
    /// <param name="destHeight">Destination height (for scaling)</param>
    public void DrawImage(Image image, int srcX, int srcY, int srcWidth, int srcHeight,
                          int destX, int destY, int destWidth, int destHeight)
    {
        CheckDisposed();
        if (image == null)
            throw new ArgumentNullException(nameof(image));

        if (image.IsDisposed)
            throw new ObjectDisposedException(nameof(image));

        if (srcWidth < 0 || srcHeight < 0 || destWidth < 0 || destHeight < 0)
            throw new ArgumentException("Dimensions must be non-negative");

        PlatformGraphics.DrawImageScaled(Handle, image.Handle,
            srcX, srcY, srcWidth, srcHeight,
            destX, destY, destWidth, destHeight);
    }

    /// <summary>
    /// Sets the clipping region to the specified rectangle.
    /// </summary>
    public void SetClipping(int x, int y, int width, int height)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        clipping = new Rectangle(x, y, width, height);
        PlatformGraphics.SetGCClipping(Handle, x, y, width, height);
    }

    /// <summary>
    /// Sets the clipping region to the specified rectangle.
    /// </summary>
    public void SetClipping(Rectangle rect)
    {
        SetClipping(rect.X, rect.Y, rect.Width, rect.Height);
    }

    /// <summary>
    /// Clears the clipping region.
    /// </summary>
    public void SetClipping()
    {
        CheckDisposed();
        clipping = null;
        PlatformGraphics.ResetGCClipping(Handle);
    }

    /// <summary>
    /// Copies a rectangular area of the drawable.
    /// </summary>
    public void CopyArea(int srcX, int srcY, int width, int height, int destX, int destY)
    {
        CheckDisposed();
        if (width < 0 || height < 0)
            throw new ArgumentException("Width and height must be non-negative");

        PlatformGraphics.CopyArea(Handle, srcX, srcY, width, height, destX, destY);
    }

    /// <summary>
    /// Gets the advance width of the specified character in the current font.
    /// </summary>
    public int GetCharWidth(char ch)
    {
        CheckDisposed();
        return PlatformGraphics.GetCharWidth(Handle, ch);
    }

    /// <summary>
    /// Computes the extent (width and height) of the specified string in the current font.
    /// </summary>
    public Layout.Point StringExtent(string text)
    {
        CheckDisposed();
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        var (width, height) = PlatformGraphics.GetTextExtent(Handle, text);
        return new Layout.Point(width, height);
    }

    /// <summary>
    /// Computes the extent (width and height) of the specified text in the current font.
    /// </summary>
    public Layout.Point TextExtent(string text)
    {
        return StringExtent(text);
    }

    /// <summary>
    /// Releases the platform-specific graphics context handle.
    /// </summary>
    protected override void ReleaseHandle()
    {
        if (Handle == IntPtr.Zero) return;

        // Dispose default colors if we created them
        foreground?.Dispose();
        background?.Dispose();

        PlatformGraphics.DestroyGraphicsContext(Handle);
    }

    /// <summary>
    /// Returns a string representation of this GC.
    /// </summary>
    public override string ToString()
    {
        if (IsDisposed)
            return "GC {disposed}";

        return $"GC {{handle={Handle}, drawable={drawable}}}";
    }
}
