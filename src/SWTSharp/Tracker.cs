using SWTSharp.Graphics;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Instances of this class provide visual feedback during resize/move operations.
/// Trackers display one or more rectangles that can be dragged and resized.
/// This is typically used for implementing custom drag-and-drop or resize operations.
/// </summary>
public class Tracker : IDisposable
{
    private IPlatformTracker? _platformTracker;
    private Rectangle[] _rectangles = Array.Empty<Rectangle>();
    private bool _stippled;
    private int _cursorType = SWT.NONE;
    private readonly Composite? _parent;
    private readonly int _style;
    private bool _disposed;

    /// <summary>
    /// Occurs when the mouse moves during tracking.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? MouseMove;

    /// <summary>
    /// Occurs when a rectangle is resized.
    /// </summary>
    public event EventHandler<TrackerEventArgs>? Resize;

    /// <summary>
    /// Creates a tracker on the display.
    /// </summary>
    /// <param name="style">Style bits (SWT.LEFT, SWT.RIGHT, SWT.UP, SWT.DOWN, SWT.RESIZE)</param>
    public Tracker(int style) : this(null, style)
    {
    }

    /// <summary>
    /// Creates a tracker on the specified parent composite.
    /// </summary>
    /// <param name="parent">Parent composite (can be null for display tracker)</param>
    /// <param name="style">Style bits (SWT.LEFT, SWT.RIGHT, SWT.UP, SWT.DOWN, SWT.RESIZE)</param>
    public Tracker(Composite? parent, int style)
    {
        _parent = parent;
        _style = style;
    }

    /// <summary>
    /// Gets or sets the rectangles being tracked.
    /// </summary>
    public Rectangle[] Rectangles
    {
        get
        {
            CheckWidget();
            return _rectangles;
        }
        set
        {
            CheckWidget();
            _rectangles = value ?? Array.Empty<Rectangle>();
            _platformTracker?.SetRectangles(_rectangles);
        }
    }

    /// <summary>
    /// Gets or sets whether rectangles should be drawn stippled (dotted).
    /// </summary>
    public bool Stippled
    {
        get
        {
            CheckWidget();
            return _stippled;
        }
        set
        {
            CheckWidget();
            _stippled = value;
            _platformTracker?.SetStippled(value);
        }
    }

    /// <summary>
    /// Gets or sets the cursor type to use during tracking.
    /// </summary>
    public int CursorType
    {
        get
        {
            CheckWidget();
            return _cursorType;
        }
        set
        {
            CheckWidget();
            _cursorType = value;
            _platformTracker?.SetCursor(value);
        }
    }

    /// <summary>
    /// Opens the tracker and starts tracking mouse events.
    /// Returns true if tracking was completed successfully, false if cancelled.
    /// This method blocks until tracking is complete.
    /// </summary>
    /// <returns>True if tracking completed, false if cancelled</returns>
    public bool Open()
    {
        CheckWidget();

        // Create platform tracker
        _platformTracker = PlatformFactory.Instance.CreateTracker(_parent?.PlatformWidget, _style);

        // Wire up events
        _platformTracker.MouseMove += OnPlatformMouseMove;
        _platformTracker.Resize += OnPlatformResize;

        // Set initial state
        _platformTracker.SetRectangles(_rectangles);
        _platformTracker.SetStippled(_stippled);
        _platformTracker.SetCursor(_cursorType);

        // Open tracker (blocking call)
        bool result = _platformTracker.Open();

        // Update rectangles from platform
        _rectangles = _platformTracker.GetRectangles();

        return result;
    }

    /// <summary>
    /// Closes the tracker and stops tracking.
    /// </summary>
    public void Close()
    {
        if (_platformTracker != null)
        {
            _platformTracker.MouseMove -= OnPlatformMouseMove;
            _platformTracker.Resize -= OnPlatformResize;
            _platformTracker.Close();
            _platformTracker.Dispose();
            _platformTracker = null;
        }
    }

    private void OnPlatformMouseMove(object? sender, TrackerEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    private void OnPlatformResize(object? sender, TrackerEventArgs e)
    {
        Resize?.Invoke(this, e);
    }

    private void CheckWidget()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Disposes the tracker and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Close();
        System.GC.SuppressFinalize(this);
    }
}
