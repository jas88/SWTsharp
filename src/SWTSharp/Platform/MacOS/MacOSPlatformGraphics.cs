using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS Graphics Platform Implementation using Core Graphics (Quartz 2D) and Core Text.
/// </summary>
internal partial class MacOSPlatform : IPlatformGraphics
{
    private const string CoreGraphicsFramework = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string CoreTextFramework = "/System/Library/Frameworks/CoreText.framework/CoreText";

    // CGContext P/Invoke declarations
    [DllImport(CoreGraphicsFramework)]
    private static extern IntPtr CGContextRetain(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextRelease(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextSetRGBStrokeColor(IntPtr context, double red, double green, double blue, double alpha);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextSetRGBFillColor(IntPtr context, double red, double green, double blue, double alpha);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextSetLineWidth(IntPtr context, double width);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextSetLineDash(IntPtr context, double phase, double[] lengths, nuint count);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextSetAlpha(IntPtr context, double alpha);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextSaveGState(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextRestoreGState(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextBeginPath(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextClosePath(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextMoveToPoint(IntPtr context, double x, double y);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextAddLineToPoint(IntPtr context, double x, double y);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextAddRect(IntPtr context, CGRect rect);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextAddEllipseInRect(IntPtr context, CGRect rect);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextAddArc(IntPtr context, double x, double y, double radius, double startAngle, double endAngle, int clockwise);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextAddCurveToPoint(IntPtr context, double cp1x, double cp1y, double cp2x, double cp2y, double x, double y);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextStrokePath(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextFillPath(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextClipToRect(IntPtr context, CGRect rect);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextClipToRects(IntPtr context, CGRect[] rects, nuint count);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGContextDrawImage(IntPtr context, CGRect rect, IntPtr image);

    [DllImport(CoreGraphicsFramework)]
    private static extern IntPtr CGBitmapContextCreate(IntPtr data, nuint width, nuint height, nuint bitsPerComponent,
        nuint bytesPerRow, IntPtr space, uint bitmapInfo);

    [DllImport(CoreGraphicsFramework)]
    private static extern IntPtr CGColorSpaceCreateDeviceRGB();

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGColorSpaceRelease(IntPtr space);

    [DllImport(CoreGraphicsFramework)]
    private static extern IntPtr CGBitmapContextCreateImage(IntPtr context);

    [DllImport(CoreGraphicsFramework)]
    private static extern void CGImageRelease(IntPtr image);

    // Graphics context state tracking
    private class GraphicsState
    {
        public IntPtr Context { get; set; }
        public IntPtr ForegroundColor { get; set; }
        public IntPtr BackgroundColor { get; set; }
        public IntPtr Font { get; set; }
        public int LineWidth { get; set; } = 1;
        public int LineStyle { get; set; } = SWT.LINE_SOLID;
        public int Alpha { get; set; } = 255;
    }

    private readonly Dictionary<IntPtr, GraphicsState> _graphicsContexts = new Dictionary<IntPtr, GraphicsState>();

    // Color operations
    public IntPtr CreateColor(int red, int green, int blue)
    {
        // Create NSColor object
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr selColorWithRed = sel_registerName("colorWithRed:green:blue:alpha:");

        // NSColor uses 0.0-1.0 range, not 0-255
        double r = red / 255.0;
        double g = green / 255.0;
        double b = blue / 255.0;

        // Create color using objc_msgSend
        return objc_msgSend_color(nsColorClass, selColorWithRed, r, g, b, 1.0);
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_color(IntPtr receiver, IntPtr selector, double r, double g, double b, double a);

    public void DestroyColor(IntPtr handle)
    {
        // NSColor is reference counted and managed by autorelease pool
        // No explicit cleanup needed
    }

    // Font operations
    public IntPtr CreateFont(string name, int height, int style)
    {
        IntPtr nsFontClass = objc_getClass("NSFont");
        IntPtr selFontWithName = sel_registerName("fontWithName:size:");

        IntPtr fontName = CreateNSString(name);
        double fontSize = height > 0 ? height : 12.0;

        IntPtr font = objc_msgSend(nsFontClass, selFontWithName, fontName);

        // Handle bold and italic styles
        if ((style & SWT.BOLD) != 0 || (style & SWT.ITALIC) != 0)
        {
            IntPtr nsFontManagerClass = objc_getClass("NSFontManager");
            IntPtr selSharedFontManager = sel_registerName("sharedFontManager");
            IntPtr fontManager = objc_msgSend(nsFontManagerClass, selSharedFontManager);

            uint traits = 0;
            if ((style & SWT.BOLD) != 0)
                traits |= 2; // NSBoldFontMask
            if ((style & SWT.ITALIC) != 0)
                traits |= 1; // NSItalicFontMask

            IntPtr selConvertFont = sel_registerName("convertFont:toHaveTrait:");
            font = objc_msgSend(fontManager, selConvertFont, font);
        }

        return font;
    }

    public void DestroyFont(IntPtr handle)
    {
        // NSFont is reference counted, no explicit cleanup needed
    }

    // Image operations
    public IntPtr CreateImage(int width, int height)
    {
        IntPtr colorSpace = CGColorSpaceCreateDeviceRGB();
        nuint w = (nuint)width;
        nuint h = (nuint)height;
        nuint bytesPerRow = w * 4; // RGBA
        const uint kCGImageAlphaPremultipliedLast = 1;
        const uint kCGBitmapByteOrder32Big = (4 << 12);

        IntPtr context = CGBitmapContextCreate(IntPtr.Zero, w, h, 8, bytesPerRow, colorSpace,
            kCGImageAlphaPremultipliedLast | kCGBitmapByteOrder32Big);

        CGColorSpaceRelease(colorSpace);

        // Create CGImage from bitmap context
        IntPtr image = CGBitmapContextCreateImage(context);
        CGContextRelease(context);

        return image;
    }

    public (IntPtr Handle, int Width, int Height) LoadImage(string filename)
    {
        // Load image using NSImage
        IntPtr nsImageClass = objc_getClass("NSImage");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInitWithContentsOfFile = sel_registerName("initWithContentsOfFile:");
        IntPtr selSize = sel_registerName("size");

        IntPtr nsFilename = CreateNSString(filename);
        IntPtr nsImage = objc_msgSend(nsImageClass, selAlloc);
        nsImage = objc_msgSend(nsImage, selInitWithContentsOfFile, nsFilename);

        if (nsImage == IntPtr.Zero)
            return (IntPtr.Zero, 0, 0);

        // Get image size
        CGRect size;
        unsafe
        {
            objc_msgSend_stret(out size, nsImage, selSize);
        }

        return (nsImage, (int)size.width, (int)size.height);
    }

    public void DestroyImage(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            CGImageRelease(handle);
        }
    }

    // Graphics Context operations
    public IntPtr CreateGraphicsContext(IntPtr drawable)
    {
        // Get CGContext from NSView
        IntPtr selGraphicsContext = sel_registerName("graphicsContext");
        IntPtr selCGContext = sel_registerName("CGContext");

        IntPtr graphicsContext = objc_msgSend(drawable, selGraphicsContext);
        IntPtr cgContext = objc_msgSend(graphicsContext, selCGContext);

        if (cgContext != IntPtr.Zero)
        {
            cgContext = CGContextRetain(cgContext);
            _graphicsContexts[cgContext] = new GraphicsState { Context = cgContext };
        }

        return cgContext;
    }

    public IntPtr CreateGraphicsContextForImage(IntPtr imageHandle)
    {
        // For images, we need to create a bitmap context
        // This is simplified - proper implementation would extract image dimensions
        IntPtr colorSpace = CGColorSpaceCreateDeviceRGB();
        const uint kCGImageAlphaPremultipliedLast = 1;
        const uint kCGBitmapByteOrder32Big = (4 << 12);

        IntPtr context = CGBitmapContextCreate(IntPtr.Zero, 100, 100, 8, 400, colorSpace,
            kCGImageAlphaPremultipliedLast | kCGBitmapByteOrder32Big);

        CGColorSpaceRelease(colorSpace);

        if (context != IntPtr.Zero)
        {
            _graphicsContexts[context] = new GraphicsState { Context = context };
        }

        return context;
    }

    public void DestroyGraphicsContext(IntPtr gcHandle)
    {
        if (gcHandle != IntPtr.Zero)
        {
            _graphicsContexts.Remove(gcHandle);
            CGContextRelease(gcHandle);
        }
    }

    // GC state operations
    public void SetGCForeground(IntPtr gcHandle, IntPtr colorHandle)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            state.ForegroundColor = colorHandle;
        }
    }

    public void SetGCBackground(IntPtr gcHandle, IntPtr colorHandle)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            state.BackgroundColor = colorHandle;
        }
    }

    public void SetGCFont(IntPtr gcHandle, IntPtr fontHandle)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            state.Font = fontHandle;
        }
    }

    public void SetGCLineWidth(IntPtr gcHandle, int width)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            state.LineWidth = width;
            CGContextSetLineWidth(gcHandle, width);
        }
    }

    public void SetGCLineStyle(IntPtr gcHandle, int style)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            state.LineStyle = style;

            // Convert SWT line style to CGContext line dash pattern
            switch (style)
            {
                case SWT.LINE_SOLID:
                    CGContextSetLineDash(gcHandle, 0, null, 0);
                    break;
                case SWT.LINE_DASH:
                    CGContextSetLineDash(gcHandle, 0, new double[] { 8, 4 }, 2);
                    break;
                case SWT.LINE_DOT:
                    CGContextSetLineDash(gcHandle, 0, new double[] { 2, 2 }, 2);
                    break;
                case SWT.LINE_DASHDOT:
                    CGContextSetLineDash(gcHandle, 0, new double[] { 8, 4, 2, 4 }, 4);
                    break;
                case SWT.LINE_DASHDOTDOT:
                    CGContextSetLineDash(gcHandle, 0, new double[] { 8, 4, 2, 4, 2, 4 }, 6);
                    break;
            }
        }
    }

    public void SetGCAlpha(IntPtr gcHandle, int alpha)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state))
        {
            state.Alpha = alpha;
            CGContextSetAlpha(gcHandle, alpha / 255.0);
        }
    }

    public void SetGCClipping(IntPtr gcHandle, int x, int y, int width, int height)
    {
        CGContextSaveGState(gcHandle);
        var clipRect = new CGRect(x, y, width, height);
        CGContextClipToRect(gcHandle, clipRect);
    }

    public void ResetGCClipping(IntPtr gcHandle)
    {
        CGContextRestoreGState(gcHandle);
    }

    // Helper to apply current foreground color
    private void ApplyStrokeColor(IntPtr gcHandle)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state) && state.ForegroundColor != IntPtr.Zero)
        {
            // Extract RGB from NSColor - simplified, assumes RGB color space
            IntPtr selRedComponent = sel_registerName("redComponent");
            IntPtr selGreenComponent = sel_registerName("greenComponent");
            IntPtr selBlueComponent = sel_registerName("blueComponent");

            double r = GetDoubleValue(state.ForegroundColor, selRedComponent);
            double g = GetDoubleValue(state.ForegroundColor, selGreenComponent);
            double b = GetDoubleValue(state.ForegroundColor, selBlueComponent);

            CGContextSetRGBStrokeColor(gcHandle, r, g, b, state.Alpha / 255.0);
        }
    }

    private void ApplyFillColor(IntPtr gcHandle)
    {
        if (_graphicsContexts.TryGetValue(gcHandle, out var state) && state.BackgroundColor != IntPtr.Zero)
        {
            IntPtr selRedComponent = sel_registerName("redComponent");
            IntPtr selGreenComponent = sel_registerName("greenComponent");
            IntPtr selBlueComponent = sel_registerName("blueComponent");

            double r = GetDoubleValue(state.BackgroundColor, selRedComponent);
            double g = GetDoubleValue(state.BackgroundColor, selGreenComponent);
            double b = GetDoubleValue(state.BackgroundColor, selBlueComponent);

            CGContextSetRGBFillColor(gcHandle, r, g, b, state.Alpha / 255.0);
        }
    }

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_fpret")]
    private static extern double objc_msgSend_fpret(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret_rect(out CGRect retval, IntPtr receiver, IntPtr selector, IntPtr arg);

    private double GetDoubleValue(IntPtr obj, IntPtr selector)
    {
        return objc_msgSend_fpret(obj, selector);
    }

    // Drawing operations
    public void DrawLine(IntPtr gcHandle, int x1, int y1, int x2, int y2)
    {
        ApplyStrokeColor(gcHandle);
        CGContextBeginPath(gcHandle);
        CGContextMoveToPoint(gcHandle, x1, y1);
        CGContextAddLineToPoint(gcHandle, x2, y2);
        CGContextStrokePath(gcHandle);
    }

    public void DrawRectangle(IntPtr gcHandle, int x, int y, int width, int height)
    {
        ApplyStrokeColor(gcHandle);
        var rect = new CGRect(x, y, width, height);
        CGContextBeginPath(gcHandle);
        CGContextAddRect(gcHandle, rect);
        CGContextStrokePath(gcHandle);
    }

    public void FillRectangle(IntPtr gcHandle, int x, int y, int width, int height)
    {
        ApplyFillColor(gcHandle);
        var rect = new CGRect(x, y, width, height);
        CGContextBeginPath(gcHandle);
        CGContextAddRect(gcHandle, rect);
        CGContextFillPath(gcHandle);
    }

    public void DrawOval(IntPtr gcHandle, int x, int y, int width, int height)
    {
        ApplyStrokeColor(gcHandle);
        var rect = new CGRect(x, y, width, height);
        CGContextBeginPath(gcHandle);
        CGContextAddEllipseInRect(gcHandle, rect);
        CGContextStrokePath(gcHandle);
    }

    public void FillOval(IntPtr gcHandle, int x, int y, int width, int height)
    {
        ApplyFillColor(gcHandle);
        var rect = new CGRect(x, y, width, height);
        CGContextBeginPath(gcHandle);
        CGContextAddEllipseInRect(gcHandle, rect);
        CGContextFillPath(gcHandle);
    }

    public void DrawPolygon(IntPtr gcHandle, int[] pointArray)
    {
        if (pointArray == null || pointArray.Length < 2)
            return;

        ApplyStrokeColor(gcHandle);
        CGContextBeginPath(gcHandle);
        CGContextMoveToPoint(gcHandle, pointArray[0], pointArray[1]);

        for (int i = 2; i < pointArray.Length; i += 2)
        {
            CGContextAddLineToPoint(gcHandle, pointArray[i], pointArray[i + 1]);
        }

        CGContextClosePath(gcHandle);
        CGContextStrokePath(gcHandle);
    }

    public void FillPolygon(IntPtr gcHandle, int[] pointArray)
    {
        if (pointArray == null || pointArray.Length < 2)
            return;

        ApplyFillColor(gcHandle);
        CGContextBeginPath(gcHandle);
        CGContextMoveToPoint(gcHandle, pointArray[0], pointArray[1]);

        for (int i = 2; i < pointArray.Length; i += 2)
        {
            CGContextAddLineToPoint(gcHandle, pointArray[i], pointArray[i + 1]);
        }

        CGContextClosePath(gcHandle);
        CGContextFillPath(gcHandle);
    }

    public void DrawPolyline(IntPtr gcHandle, int[] pointArray)
    {
        if (pointArray == null || pointArray.Length < 2)
            return;

        ApplyStrokeColor(gcHandle);
        CGContextBeginPath(gcHandle);
        CGContextMoveToPoint(gcHandle, pointArray[0], pointArray[1]);

        for (int i = 2; i < pointArray.Length; i += 2)
        {
            CGContextAddLineToPoint(gcHandle, pointArray[i], pointArray[i + 1]);
        }

        CGContextStrokePath(gcHandle);
    }

    public void DrawArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        ApplyStrokeColor(gcHandle);

        double centerX = x + width / 2.0;
        double centerY = y + height / 2.0;
        double radius = Math.Min(width, height) / 2.0;

        // Convert angles from degrees to radians
        double startRad = startAngle * Math.PI / 180.0;
        double endRad = (startAngle + arcAngle) * Math.PI / 180.0;

        CGContextBeginPath(gcHandle);
        CGContextAddArc(gcHandle, centerX, centerY, radius, startRad, endRad, 0);
        CGContextStrokePath(gcHandle);
    }

    public void FillArc(IntPtr gcHandle, int x, int y, int width, int height, int startAngle, int arcAngle)
    {
        ApplyFillColor(gcHandle);

        double centerX = x + width / 2.0;
        double centerY = y + height / 2.0;
        double radius = Math.Min(width, height) / 2.0;

        double startRad = startAngle * Math.PI / 180.0;
        double endRad = (startAngle + arcAngle) * Math.PI / 180.0;

        CGContextBeginPath(gcHandle);
        CGContextMoveToPoint(gcHandle, centerX, centerY);
        CGContextAddArc(gcHandle, centerX, centerY, radius, startRad, endRad, 0);
        CGContextClosePath(gcHandle);
        CGContextFillPath(gcHandle);
    }

    public void DrawRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        ApplyStrokeColor(gcHandle);
        DrawRoundRectPath(gcHandle, x, y, width, height, arcWidth, arcHeight);
        CGContextStrokePath(gcHandle);
    }

    public void FillRoundRectangle(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        ApplyFillColor(gcHandle);
        DrawRoundRectPath(gcHandle, x, y, width, height, arcWidth, arcHeight);
        CGContextFillPath(gcHandle);
    }

    private void DrawRoundRectPath(IntPtr gcHandle, int x, int y, int width, int height, int arcWidth, int arcHeight)
    {
        double radius = Math.Min(arcWidth, arcHeight) / 2.0;

        CGContextBeginPath(gcHandle);
        CGContextMoveToPoint(gcHandle, x + radius, y);
        CGContextAddLineToPoint(gcHandle, x + width - radius, y);
        CGContextAddArc(gcHandle, x + width - radius, y + radius, radius, -Math.PI / 2, 0, 0);
        CGContextAddLineToPoint(gcHandle, x + width, y + height - radius);
        CGContextAddArc(gcHandle, x + width - radius, y + height - radius, radius, 0, Math.PI / 2, 0);
        CGContextAddLineToPoint(gcHandle, x + radius, y + height);
        CGContextAddArc(gcHandle, x + radius, y + height - radius, radius, Math.PI / 2, Math.PI, 0);
        CGContextAddLineToPoint(gcHandle, x, y + radius);
        CGContextAddArc(gcHandle, x + radius, y + radius, radius, Math.PI, -Math.PI / 2, 0);
        CGContextClosePath(gcHandle);
    }

    // Text operations
    public void DrawText(IntPtr gcHandle, string text, int x, int y, bool isTransparent)
    {
        if (string.IsNullOrEmpty(text))
            return;

        ApplyFillColor(gcHandle);

        // Use NSString drawing for simplicity
        IntPtr nsText = CreateNSString(text);

        // This is simplified - proper implementation would use Core Text for better control
        // For now, use basic NSString drawing (requires NSGraphicsContext)
    }

    public (int Width, int Height) GetTextExtent(IntPtr gcHandle, string text)
    {
        if (string.IsNullOrEmpty(text))
            return (0, 0);

        // Use NSString to measure text
        IntPtr nsText = CreateNSString(text);
        IntPtr selSizeWithAttributes = sel_registerName("sizeWithAttributes:");

        if (_graphicsContexts.TryGetValue(gcHandle, out var state) && state.Font != IntPtr.Zero)
        {
            // Create attributes dictionary with font
            IntPtr nsDictionaryClass = objc_getClass("NSDictionary");
            IntPtr selDictionaryWithObject = sel_registerName("dictionaryWithObject:forKey:");
            IntPtr nsAttributedStringKey = CreateNSString("NSFont");

            IntPtr attributes = objc_msgSend(nsDictionaryClass, selDictionaryWithObject, state.Font);

            CGRect size;
            objc_msgSend_stret_rect(out size, nsText, selSizeWithAttributes, attributes);

            return ((int)Math.Ceiling(size.width), (int)Math.Ceiling(size.height));
        }

        return (0, 0);
    }

    public int GetCharWidth(IntPtr gcHandle, char ch)
    {
        var (width, _) = GetTextExtent(gcHandle, ch.ToString());
        return width;
    }

    // Image operations
    public void DrawImage(IntPtr gcHandle, IntPtr imageHandle, int x, int y)
    {
        if (imageHandle == IntPtr.Zero)
            return;

        // Get image size first (simplified)
        var rect = new CGRect(x, y, 100, 100); // TODO: Get actual image size
        CGContextDrawImage(gcHandle, rect, imageHandle);
    }

    public void DrawImageScaled(IntPtr gcHandle, IntPtr imageHandle,
        int srcX, int srcY, int srcWidth, int srcHeight,
        int destX, int destY, int destWidth, int destHeight)
    {
        if (imageHandle == IntPtr.Zero)
            return;

        // This is simplified - proper implementation would handle source rectangle
        var destRect = new CGRect(destX, destY, destWidth, destHeight);
        CGContextDrawImage(gcHandle, destRect, imageHandle);
    }

    public void CopyArea(IntPtr gcHandle, int srcX, int srcY, int width, int height, int destX, int destY)
    {
        // This requires creating a temporary image from the source area
        // Simplified implementation
        CGContextSaveGState(gcHandle);
        // TODO: Implement actual copy using CGBitmapContextCreateImage
        CGContextRestoreGState(gcHandle);
    }
}
