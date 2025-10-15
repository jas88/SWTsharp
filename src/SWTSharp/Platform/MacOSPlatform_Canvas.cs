using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Canvas widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Canvas data structure
    private sealed class CanvasData
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
            // _selContentView and _selIsKindOfClass are now defined in MacOSPlatform.cs
        }

        // Ensure _nsWindowClass is initialized (defined in MacOSPlatform.cs)
        if (_nsWindowClass == IntPtr.Zero)
        {
            _nsWindowClass = objc_getClass("NSWindow");
        }
    }

    // REMOVED METHODS (moved to ICanvasWidget interface):
    // - CreateCanvas(IntPtr parent, int style)
    // - ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> paintCallback)
    // - RedrawCanvas(IntPtr handle)
    // - RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height)
    // These methods are now implemented via the ICanvasWidget interface using proper handles

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
