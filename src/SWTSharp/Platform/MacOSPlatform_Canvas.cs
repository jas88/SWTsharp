using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Canvas widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Canvas data structure
    private class CanvasData
    {
        public Action<int, int, int, int, object?>? PaintCallback { get; set; }
        public Graphics.RGB? BackgroundColor { get; set; }
    }

    // Canvas widget selectors and data
    private readonly Dictionary<IntPtr, CanvasData> _canvasData = new();
    private IntPtr _nsViewClass;
    private IntPtr _selSetNeedsDisplay;
    private IntPtr _selSetNeedsDisplayInRect;
    private IntPtr _selSetBackgroundColor;

    private void InitializeCanvasSelectors()
    {
        if (_nsViewClass == IntPtr.Zero)
        {
            _nsViewClass = objc_getClass("NSView");
            _selSetNeedsDisplay = sel_registerName("setNeedsDisplay:");
            _selSetNeedsDisplayInRect = sel_registerName("setNeedsDisplayInRect:");
            _selSetBackgroundColor = sel_registerName("setBackgroundColor:");
        }
    }

    public IntPtr CreateCanvas(IntPtr parent, int style)
    {
        InitializeCanvasSelectors();

        // Create NSView as the custom drawing surface
        IntPtr view = objc_msgSend(objc_msgSend(_nsViewClass, _selAlloc), _selInit);

        // Set default frame
        CGRect frame = new CGRect(0, 0, 100, 100);
        IntPtr selSetFrame = sel_registerName("setFrame:");
        objc_msgSend_rect(view, selSetFrame, frame);

        // Initialize canvas data for this view
        _canvasData[view] = new CanvasData();

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            if (_selAddSubview == IntPtr.Zero)
            {
                _selAddSubview = sel_registerName("addSubview:");
            }
            objc_msgSend(parent, _selAddSubview, view);
        }

        return view;
    }

    public void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    {
        if (handle == IntPtr.Zero)
            return;

        if (_canvasData.TryGetValue(handle, out var data))
        {
            data.PaintCallback = paintCallback;
        }
        else
        {
            _canvasData[handle] = new CanvasData { PaintCallback = paintCallback };
        }

        // Note: In a full implementation, we would need to set up a drawRect: delegate
        // or subclass NSView to handle paint events. For now, we store the callback
        // and it will be invoked when RedrawCanvas is called.
    }

    public void RedrawCanvas(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeCanvasSelectors();

        // Call setNeedsDisplay:YES to trigger redraw of entire view
        objc_msgSend_void(handle, _selSetNeedsDisplay, true);

        // For immediate testing, invoke paint callback if set
        if (_canvasData.TryGetValue(handle, out var data) && data.PaintCallback != null)
        {
            // Get view bounds
            objc_msgSend_stret(out CGRect bounds, handle, _selFrame);
            data.PaintCallback((int)bounds.x, (int)bounds.y, (int)bounds.width, (int)bounds.height, handle);
        }
    }

    public void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeCanvasSelectors();

        // Create CGRect for the area to redraw
        CGRect rect = new CGRect(x, y, width, height);

        // Call setNeedsDisplayInRect: to trigger redraw of specific area
        objc_msgSend_rect(handle, _selSetNeedsDisplayInRect, rect);

        // For immediate testing, invoke paint callback if set
        if (_canvasData.TryGetValue(handle, out var data) && data.PaintCallback != null)
        {
            data.PaintCallback(x, y, width, height, handle);
        }
    }

    // Helper method to set canvas background color (used internally)
    private void SetCanvasBackgroundColor(IntPtr handle, Graphics.RGB color)
    {
        if (handle == IntPtr.Zero)
            return;

        InitializeCanvasSelectors();

        // Store color in canvas data
        if (_canvasData.TryGetValue(handle, out var data))
        {
            data.BackgroundColor = color;
        }
        else
        {
            _canvasData[handle] = new CanvasData { BackgroundColor = color };
        }

        // Create NSColor from RGB values (normalized to 0.0-1.0)
        IntPtr nsColorClass = objc_getClass("NSColor");
        IntPtr selColorWithRGB = sel_registerName("colorWithRed:green:blue:alpha:");

        // Note: objc_msgSend with float arguments requires special handling
        // For now, we store the color and would apply it in drawRect: implementation
        // A complete implementation would need to properly bridge float arguments
    }

    // Cleanup method for canvas data
    public void ClearCanvasData()
    {
        _canvasData.Clear();
    }

    // Remove specific canvas data when control is destroyed
    public void RemoveCanvasData(IntPtr handle)
    {
        _canvasData.Remove(handle);
    }
}
