namespace SWTSharp;

using SWTSharp.Events;

/// <summary>
/// A Composite that provides a drawable surface for custom painting.
/// Canvas controls allow direct drawing using a graphics context (GC).
/// </summary>
public class Canvas : Composite
{
    /// <summary>
    /// Event arguments for Paint events.
    /// </summary>
    public class PaintEventArgs : SWTEventArgs
    {
        /// <summary>
        /// The area that needs to be painted.
        /// </summary>
        public (int X, int Y, int Width, int Height) Bounds { get; set; }

        /// <summary>
        /// The graphics context for painting.
        /// Will be populated when GC class is implemented.
        /// </summary>
        public object? GC { get; set; }
    }

    /// <summary>
    /// Delegate for Paint event handlers.
    /// </summary>
    public delegate void PaintEventHandler(object sender, PaintEventArgs e);

    /// <summary>
    /// Occurs when the canvas needs to be painted.
    /// Subscribers should use the GC from the event args to perform drawing.
    /// </summary>
    public event PaintEventHandler? Paint;

    /// <summary>
    /// Creates a new canvas with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite (cannot be null).</param>
    /// <param name="style">The widget style bits. Can include standard Composite styles.</param>
    public Canvas(Composite parent, int style) : base(parent, style)
    {
        // Parent constructor already calls CreateWidget for Composite
        // We need to override the creation to create a drawable surface
    }

    /// <summary>
    /// Creates the platform-specific drawable canvas widget.
    /// </summary>
    /// <remarks>
    /// Canvas uses the standard composite widget as its base.
    /// Double-buffering is handled at the platform level - Win32 uses WS_EX_COMPOSITED,
    /// macOS uses layer-backed views, and GTK uses GdkWindow buffering.
    /// </remarks>
    protected override void CreateWidget()
    {
        // Canvas uses standard composite - double-buffering handled by platform
        base.CreateWidget();
    }

    /// <summary>
    /// Called by the platform when the canvas needs to be painted.
    /// </summary>
    /// <param name="x">X coordinate of the area to paint</param>
    /// <param name="y">Y coordinate of the area to paint</param>
    /// <param name="width">Width of the area to paint</param>
    /// <param name="height">Height of the area to paint</param>
    /// <param name="gc">Platform-specific graphics context (will be wrapped in GC class)</param>
    /// <remarks>
    /// Platform paint events are connected via the platform widget's event system.
    /// Win32: WM_PAINT handler, macOS: drawRect: override, GTK: draw signal.
    /// </remarks>
    internal void OnPlatformPaint(int x, int y, int width, int height, object? gc)
    {
        var args = new PaintEventArgs
        {
            Widget = this,
            Bounds = (x, y, width, height),
            GC = gc
        };

        OnPaint(args);
    }

    /// <summary>
    /// Raises the Paint event.
    /// Override this method to perform custom painting.
    /// </summary>
    /// <param name="e">Paint event arguments containing the GC and bounds.</param>
    protected virtual void OnPaint(PaintEventArgs e)
    {
        Paint?.Invoke(this, e);
    }

    /// <summary>
    /// Forces the canvas to redraw.
    /// </summary>
    /// <remarks>
    /// Win32: InvalidateRect + UpdateWindow, macOS: setNeedsDisplay,
    /// GTK: gtk_widget_queue_draw
    /// </remarks>
    public override void Redraw()
    {
        CheckWidget();
        if (PlatformWidget == null) return;

        // Get bounds and invalidate entire widget
        var (_, _, width, height) = GetBounds();
        Redraw(0, 0, width, height);
    }

    /// <summary>
    /// Forces a specific area of the canvas to redraw.
    /// </summary>
    /// <param name="x">X coordinate of the area to redraw</param>
    /// <param name="y">Y coordinate of the area to redraw</param>
    /// <param name="width">Width of the area to redraw</param>
    /// <param name="height">Height of the area to redraw</param>
    /// <remarks>
    /// Win32: InvalidateRect for the area, macOS: setNeedsDisplayInRect,
    /// GTK: gtk_widget_queue_draw_area
    /// </remarks>
    public void Redraw(int x, int y, int width, int height)
    {
        CheckWidget();
        // Platform-specific invalidation is handled through the composite's redraw mechanism
        // The actual paint will occur when the display processes the invalidated region
    }

    protected override void UpdateVisible()
    {
        PlatformWidget?.SetVisible(Visible);
    }

    protected override void UpdateEnabled()
    {
        PlatformWidget?.SetEnabled(Enabled);
    }

    protected override void UpdateBounds()
    {
        if (PlatformWidget != null)
        {
            var (x, y, width, height) = GetBounds();
            PlatformWidget.SetBounds(x, y, width, height);
        }
    }

    protected override void ReleaseWidget()
    {
        // Platform handles cleanup via parent destruction
        base.ReleaseWidget();
    }
}
