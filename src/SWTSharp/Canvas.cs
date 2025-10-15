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
    protected override void CreateWidget()
    {
        // TODO: Implement canvas creation through platform widget interface
        // The canvas should be created as a drawable surface with paint capabilities
        // TODO: Create IPlatformCanvas widget here
    }

    /// <summary>
    /// Called by the platform when the canvas needs to be painted.
    /// </summary>
    /// <param name="x">X coordinate of the area to paint</param>
    /// <param name="y">Y coordinate of the area to paint</param>
    /// <param name="width">Width of the area to paint</param>
    /// <param name="height">Height of the area to paint</param>
    /// <param name="gc">Platform-specific graphics context (will be wrapped in GC class)</param>
    private void OnPlatformPaint(int x, int y, int width, int height, object? gc)
    {
        // TODO: Implement platform paint event connection through platform widget interface
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
    public override void Redraw()
    {
        CheckWidget();
        // TODO: Implement canvas redraw through platform widget interface
        // This should trigger a full repaint of the canvas surface
    }

    /// <summary>
    /// Forces a specific area of the canvas to redraw.
    /// </summary>
    /// <param name="x">X coordinate of the area to redraw</param>
    /// <param name="y">Y coordinate of the area to redraw</param>
    /// <param name="width">Width of the area to redraw</param>
    /// <param name="height">Height of the area to redraw</param>
    public void Redraw(int x, int y, int width, int height)
    {
        CheckWidget();
        // TODO: Implement canvas area redraw through platform widget interface
        // This should trigger a repaint of the specified rectangular area
    }

    protected override void UpdateVisible()
    {
        // TODO: Implement visibility update through platform widget interface
        if (PlatformWidget != null)
        {
            // TODO: PlatformWidget.SetVisible(Visible);
        }
    }

    protected override void UpdateEnabled()
    {
        // TODO: Implement enabled state update through platform widget interface
        if (PlatformWidget != null)
        {
            // TODO: PlatformWidget.SetEnabled(Enabled);
        }
    }

    protected override void UpdateBounds()
    {
        // TODO: Implement bounds update through platform widget interface
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
