using SWTSharp.Platform;
using SWTSharp.Events;

namespace SWTSharp;

/// <summary>
/// Represents a button control.
/// Buttons can be push buttons, check buttons, radio buttons, or toggle buttons.
/// </summary>
public class Button : Control
{
    private string _text = string.Empty;
    private bool _selection;

    /// <summary>
    /// Gets or sets the button's text.
    /// </summary>
    public string Text
    {
        get
        {
            CheckWidget();
            // NEW: Use platform widget
            if (PlatformWidget is IPlatformTextWidget textWidget)
            {
                return textWidget.GetText();
            }
            // OLD: Fallback to internal field for backwards compatibility
            return _text;
        }
        set
        {
            CheckWidget();
            _text = value ?? string.Empty;
            // Use platform widget
            if (PlatformWidget is IPlatformTextWidget textWidget)
            {
                textWidget.SetText(_text);
            }
        }
    }

    /// <summary>
    /// Gets or sets the selection state (for CHECK, RADIO, and TOGGLE buttons).
    /// </summary>
    public bool Selection
    {
        get
        {
            CheckWidget();
            return _selection;
        }
        set
        {
            CheckWidget();
            if (_selection != value)
            {
                _selection = value;
                // TODO: Implement selection updates through platform widget interface
            }
        }
    }

    /// <summary>
    /// Occurs when the button is clicked.
    /// </summary>
    public event EventHandler? Click;

    /// <summary>
    /// Creates a new button.
    /// </summary>
    public Button(Control parent, int style) : base(parent, style)
    {
        if ((style & SWT.PUSH) == 0 && (style & SWT.CHECK) == 0 &&
            (style & SWT.RADIO) == 0 && (style & SWT.TOGGLE) == 0 &&
            (style & SWT.ARROW) == 0)
        {
            // Default to PUSH style
            CreateWidget(style | SWT.PUSH);
        }
        else
        {
            CreateWidget(style);
        }
    }

    private void CreateWidget(int style)
    {
        // Use platform widget - must complete before subscribing to events
        var widget = SWTSharp.Platform.PlatformFactory.Instance.CreateButtonWidget(
            Parent?.PlatformWidget,
            style
        );

        // Only assign and subscribe to events after successful creation
        PlatformWidget = widget;

        // Subscribe to platform widget events
        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.Click += OnPlatformClick;
            eventHandling.FocusGained += OnPlatformFocusGained;
            eventHandling.FocusLost += OnPlatformFocusLost;
            eventHandling.KeyDown += OnPlatformKeyDown;
            eventHandling.KeyUp += OnPlatformKeyUp;
        }
    }

    /// <summary>
    /// Handles platform widget click events and converts them to SWT events.
    /// </summary>
    private void OnPlatformClick(object? sender, int button)
    {
        CheckWidget();

        // Create SWT MouseDown event
        var mouseDownEvent = new Event
        {
            Button = button,
            Time = Environment.TickCount,
            StateMask = GetCurrentStateMask()
        };
        NotifyListeners(SWT.MouseDown, mouseDownEvent);

        // Create SWT MouseUp event
        var mouseUpEvent = new Event
        {
            Button = button,
            Time = Environment.TickCount,
            StateMask = GetCurrentStateMask()
        };
        NotifyListeners(SWT.MouseUp, mouseUpEvent);

        // Update selection state for CHECK, RADIO, and TOGGLE buttons
        if ((Style & SWT.CHECK) != 0 || (Style & SWT.TOGGLE) != 0)
        {
            _selection = !_selection;
        }
        else if ((Style & SWT.RADIO) != 0)
        {
            _selection = true;
            // TODO: Unselect other radio buttons in the same group
        }

        // Raise the legacy Click event for backwards compatibility
        Click?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Handles platform widget focus gained events.
    /// </summary>
    private void OnPlatformFocusGained(object? sender, int detail)
    {
        CheckWidget();

        var focusEvent = new Event
        {
            Detail = detail,
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.FocusIn, focusEvent);
    }

    /// <summary>
    /// Handles platform widget focus lost events.
    /// </summary>
    private void OnPlatformFocusLost(object? sender, int detail)
    {
        CheckWidget();

        var focusEvent = new Event
        {
            Detail = detail,
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.FocusOut, focusEvent);
    }

    /// <summary>
    /// Handles platform widget key down events.
    /// </summary>
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

    /// <summary>
    /// Handles platform widget key up events.
    /// </summary>
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

    /// <summary>
    /// Gets the current keyboard and mouse state mask.
    /// </summary>
    private int GetCurrentStateMask()
    {
        int stateMask = 0;
        // TODO: Implement platform-specific state detection
        // For now, return 0 as placeholder
        return stateMask;
    }

    /// <summary>
    /// Converts platform key event arguments to SWT state mask.
    /// </summary>
    private int GetStateMaskFromPlatformArgs(PlatformKeyEventArgs e)
    {
        int stateMask = 0;
        if (e.Shift) stateMask |= SWT.SHIFT;
        if (e.Control) stateMask |= SWT.CTRL;
        if (e.Alt) stateMask |= SWT.ALT;
        // TODO: Add Command key detection on macOS
        return stateMask;
    }

    /// <summary>
    /// Raises the Click event.
    /// </summary>
    protected virtual void OnClick(EventArgs e)
    {
        Click?.Invoke(this, e);
    }


    protected override void ReleaseWidget()
    {
        // Unsubscribe from platform widget events to prevent memory leaks
        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.Click -= OnPlatformClick;
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        // Clear the Click event subscription to prevent memory leaks
        Click = null;

        // NEW: Dispose platform widget
        if (PlatformWidget != null)
        {
            PlatformWidget.Dispose();
            PlatformWidget = null;
        }

        // Platform handles cleanup via parent destruction
        base.ReleaseWidget();
    }
}
