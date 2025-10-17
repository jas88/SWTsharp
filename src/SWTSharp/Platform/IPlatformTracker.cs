using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Platform-specific interface for tracker widget.
/// Trackers provide visual feedback during resize/move operations using rubber-band rectangles.
/// </summary>
public interface IPlatformTracker : IDisposable
{
    /// <summary>
    /// Opens the tracker and starts tracking mouse events.
    /// Returns true if tracking was completed successfully, false if cancelled.
    /// </summary>
    /// <returns>True if tracking completed, false if cancelled</returns>
    bool Open();

    /// <summary>
    /// Closes the tracker and stops tracking.
    /// </summary>
    void Close();

    /// <summary>
    /// Sets the rectangles to be drawn during tracking.
    /// </summary>
    /// <param name="rectangles">Array of rectangles to track</param>
    void SetRectangles(Rectangle[] rectangles);

    /// <summary>
    /// Gets the current tracking rectangles.
    /// </summary>
    /// <returns>Array of current rectangles</returns>
    Rectangle[] GetRectangles();

    /// <summary>
    /// Sets whether rectangles should be drawn stippled (dotted).
    /// </summary>
    /// <param name="stippled">True for stippled drawing, false for solid</param>
    void SetStippled(bool stippled);

    /// <summary>
    /// Gets whether rectangles are drawn stippled.
    /// </summary>
    bool GetStippled();

    /// <summary>
    /// Sets the cursor to use during tracking.
    /// </summary>
    void SetCursor(int cursorType);

    /// <summary>
    /// Occurs when the mouse moves during tracking.
    /// </summary>
    event EventHandler<TrackerEventArgs>? MouseMove;

    /// <summary>
    /// Occurs when a rectangle is resized.
    /// </summary>
    event EventHandler<TrackerEventArgs>? Resize;
}

/// <summary>
/// Event arguments for tracker events.
/// </summary>
public class TrackerEventArgs : EventArgs
{
    /// <summary>
    /// The current mouse X coordinate.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The current mouse Y coordinate.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The current tracking rectangle.
    /// </summary>
    public Rectangle Bounds { get; set; }

    /// <summary>
    /// Whether the tracking should be cancelled.
    /// </summary>
    public bool Cancel { get; set; }
}
