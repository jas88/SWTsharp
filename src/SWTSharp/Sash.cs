using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a resizable divider/splitter control.
/// Used to create resizable sections in layouts.
/// </summary>
public class Sash : Control
{
    private int _position;

    /// <summary>
    /// Gets or sets the sash position.
    /// </summary>
    public int Position
    {
        get
        {
            CheckWidget();
            return _position;
        }
        set
        {
            CheckWidget();
            _position = value;
            UpdatePosition();
        }
    }

    /// <summary>
    /// Occurs when the sash is dragged.
    /// </summary>
    public event EventHandler<SashEventArgs>? SashMoved;

    /// <summary>
    /// Creates a new sash control.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (HORIZONTAL or VERTICAL).</param>
    public Sash(Composite parent, int style) : base(parent, style)
    {
        if ((style & (SWT.HORIZONTAL | SWT.VERTICAL)) == 0)
        {
            style |= SWT.HORIZONTAL;
        }
        CreateWidget();
    }

    private void CreateWidget()
    {
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateSashWidget(parentWidget, Style);

        if (PlatformWidget is IPlatformSash sashWidget)
        {
            sashWidget.SetPosition(_position);
            sashWidget.PositionChanged += OnPlatformPositionChanged;
        }

        ConnectEventHandlers();
    }

    /// <summary>
    /// Sets the sash position.
    /// </summary>
    public void SetPosition(int position)
    {
        CheckWidget();
        Position = position;
    }

    /// <summary>
    /// Gets the sash position.
    /// </summary>
    public int GetPosition()
    {
        CheckWidget();
        return _position;
    }

    private void UpdatePosition()
    {
        if (PlatformWidget is IPlatformSash sashWidget)
        {
            sashWidget.SetPosition(_position);
        }
    }

    private void ConnectEventHandlers()
    {
        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained += OnPlatformFocusGained;
            eventHandling.FocusLost += OnPlatformFocusLost;
            eventHandling.KeyDown += OnPlatformKeyDown;
            eventHandling.KeyUp += OnPlatformKeyUp;
        }
    }

    private void OnPlatformPositionChanged(object? sender, int newPosition)
    {
        CheckWidget();

        if (_position != newPosition)
        {
            _position = newPosition;

            var sashEvent = new SashEventArgs
            {
                X = (Style & SWT.HORIZONTAL) != 0 ? 0 : newPosition,
                Y = (Style & SWT.VERTICAL) != 0 ? 0 : newPosition
            };
            SashMoved?.Invoke(this, sashEvent);

            var selectionEvent = new Event
            {
                Detail = SWT.NONE,
                X = sashEvent.X,
                Y = sashEvent.Y,
                Time = Environment.TickCount
            };
            NotifyListeners(SWT.Selection, selectionEvent);
        }
    }

    private void OnPlatformFocusGained(object? sender, int detail)
    {
        CheckWidget();
        var focusEvent = new Event { Detail = detail, Time = Environment.TickCount };
        NotifyListeners(SWT.FocusIn, focusEvent);
    }

    private void OnPlatformFocusLost(object? sender, int detail)
    {
        CheckWidget();
        var focusEvent = new Event { Detail = detail, Time = Environment.TickCount };
        NotifyListeners(SWT.FocusOut, focusEvent);
    }

    private void OnPlatformKeyDown(object? sender, PlatformKeyEventArgs e)
    {
        CheckWidget();
        var keyEvent = new Event
        {
            KeyCode = e.KeyCode,
            Character = e.Character,
            StateMask = GetStateMaskFromPlatformArgs(e),
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.KeyDown, keyEvent);
    }

    private void OnPlatformKeyUp(object? sender, PlatformKeyEventArgs e)
    {
        CheckWidget();
        var keyEvent = new Event
        {
            KeyCode = e.KeyCode,
            Character = e.Character,
            StateMask = GetStateMaskFromPlatformArgs(e),
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.KeyUp, keyEvent);
    }

    private int GetStateMaskFromPlatformArgs(PlatformKeyEventArgs e)
    {
        int stateMask = 0;
        if (e.Shift) stateMask |= SWT.SHIFT;
        if (e.Control) stateMask |= SWT.CTRL;
        if (e.Alt) stateMask |= SWT.ALT;
        return stateMask;
    }

    protected override void ReleaseWidget()
    {
        if (PlatformWidget is IPlatformSash sashWidget)
        {
            sashWidget.PositionChanged -= OnPlatformPositionChanged;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        SashMoved = null;
        base.ReleaseWidget();
    }
}

/// <summary>
/// Event arguments for sash movement events.
/// </summary>
public class SashEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the X position (for vertical sash).
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y position (for horizontal sash).
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets whether the movement should be allowed.
    /// </summary>
    public bool Doit { get; set; } = true;
}
