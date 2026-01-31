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
    /// Canvas currently uses the standard composite widget from the base class.
    /// Future versions may add IPlatformCanvas interface with double-buffering and native paint events.
    /// </remarks>
    protected override void CreateWidget()
    {
        // Canvas uses the standard composite widget
        // Double-buffering and paint events are handled at the SWTSharp layer
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
    /// Platform paint events are connected when IPlatformCanvas interface is implemented.
    /// Currently, paint events are triggered via Redraw() calls.
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
    /// Triggers a paint event with the full canvas bounds.
    /// Future versions may optimize via platform-native invalidation.
    /// </remarks>
    public override void Redraw()
    {
        CheckWidget();
        // Trigger paint event for the full canvas bounds
        var bounds = GetBounds();
        OnPlatformPaint(0, 0, bounds.Width, bounds.Height, null);
    }

    /// <summary>
    /// Forces a specific area of the canvas to redraw.
    /// </summary>
    /// <param name="x">X coordinate of the area to redraw</param>
    /// <param name="y">Y coordinate of the area to redraw</param>
    /// <param name="width">Width of the area to redraw</param>
    /// <param name="height">Height of the area to redraw</param>
    /// <remarks>
    /// Triggers a paint event for the specified rectangular area.
    /// Future versions may optimize via platform-native invalidation.
    /// </remarks>
    public void Redraw(int x, int y, int width, int height)
    {
        CheckWidget();
        // Trigger paint event for the specified area
        OnPlatformPaint(x, y, width, height, null);
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
