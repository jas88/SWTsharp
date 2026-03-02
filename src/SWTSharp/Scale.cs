using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// A user interface object that represents a range of numeric values.
/// Similar to Slider but with a different visual representation (typically with tick marks).
/// </summary>
public class Scale : Control
{
    private int _minimum;
    private int _maximum = 100;
    private int _selection;
    private int _increment = 1;
    private int _pageIncrement = 10;

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    public int Minimum
    {
        get
        {
            CheckWidget();
            return _minimum;
        }
        set
        {
            CheckWidget();
            if (_minimum != value)
            {
                _minimum = value;
                if (_selection < _minimum) _selection = _minimum;
                UpdateValues();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    public int Maximum
    {
        get
        {
            CheckWidget();
            return _maximum;
        }
        set
        {
            CheckWidget();
            if (_maximum != value)
            {
                _maximum = value;
                if (_selection > _maximum) _selection = _maximum;
                UpdateValues();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current selection (value).
    /// </summary>
    public int Selection
    {
        get
        {
            CheckWidget();
            return _selection;
        }
        set
        {
            CheckWidget();
            SetSelection(value);
        }
    }

    /// <summary>
    /// Gets or sets the increment value.
    /// </summary>
    public int Increment
    {
        get
        {
            CheckWidget();
            return _increment;
        }
        set
        {
            CheckWidget();
            if (_increment != value && value > 0)
            {
                _increment = value;
                UpdateValues();
            }
        }
    }

    /// <summary>
    /// Gets or sets the page increment value.
    /// </summary>
    public int PageIncrement
    {
        get
        {
            CheckWidget();
            return _pageIncrement;
        }
        set
        {
            CheckWidget();
            if (_pageIncrement != value && value > 0)
            {
                _pageIncrement = value;
                UpdateValues();
            }
        }
    }

    /// <summary>
    /// Creates a new scale with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (HORIZONTAL, VERTICAL).</param>
    public Scale(Composite parent, int style) : base(parent, style)
    {
        if ((style & (SWT.HORIZONTAL | SWT.VERTICAL)) == 0)
        {
            style |= SWT.HORIZONTAL;
        }
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific scale widget.
    /// </summary>
    private void CreateWidget()
    {
        // Create IPlatformScale widget using platform widget interface
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateScaleWidget(parentWidget, Style);

        // Initialize scale values and connect event handlers
        UpdateValues();
        ConnectEventHandlers();
    }

    /// <summary>
    /// Sets the current selection value.
    /// </summary>
    public void SetSelection(int value)
    {
        CheckWidget();
        value = Math.Max(_minimum, Math.Min(_maximum, value));
        if (_selection != value)
        {
            _selection = value;
            UpdateValues();
        }
    }

    /// <summary>
    /// Gets the current selection value.
    /// </summary>
    public int GetSelection()
    {
        CheckWidget();
        return _selection;
    }

    /// <summary>
    /// Sets all scale values at once.
    /// </summary>
    public void SetValues(int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        CheckWidget();
        _minimum = minimum;
        _maximum = maximum;
        _increment = increment;
        _pageIncrement = pageIncrement;
        _selection = Math.Max(_minimum, Math.Min(_maximum, selection));
        UpdateValues();
    }

    /// <summary>
    /// Updates all values on the native control.
    /// </summary>
    private void UpdateValues()
    {
        // Use IPlatformScale interface to update scale values
        if (PlatformWidget is IPlatformScale scaleWidget)
        {
            scaleWidget.Minimum = _minimum;
            scaleWidget.Maximum = _maximum;
            scaleWidget.Value = _selection;
            scaleWidget.Increment = _increment;
            scaleWidget.PageIncrement = _pageIncrement;
            scaleWidget.ShowTicks = true;
        }
    }

    /// <summary>
    /// Connects platform-specific event handlers.
    /// </summary>
    private void ConnectEventHandlers()
    {
        // Connect scale change event handler to platform widget
        if (PlatformWidget is IPlatformScale scaleWidget)
        {
            scaleWidget.ValueChanged += OnPlatformValueChanged;
        }

        // Connect standard widget events
        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained += OnPlatformFocusGained;
            eventHandling.FocusLost += OnPlatformFocusLost;
            eventHandling.KeyDown += OnPlatformKeyDown;
            eventHandling.KeyUp += OnPlatformKeyUp;
        }
    }

    /// <summary>
    /// Handles platform widget value changed events.
    /// </summary>
    private void OnPlatformValueChanged(object? sender, int newValue)
    {
        CheckWidget();

        if (_selection != newValue)
        {
            _selection = newValue;

            var selectionEvent = new Event
            {
                Detail = SWT.NONE,
                Time = Environment.TickCount,
                Index = newValue
            };
            NotifyListeners(SWT.Selection, selectionEvent);
        }
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

        // Handle arrow keys for scale navigation
        if (e.KeyCode == SWT.ARROW_LEFT || e.KeyCode == SWT.ARROW_DOWN)
        {
            SetSelection(_selection - _increment);
        }
        else if (e.KeyCode == SWT.ARROW_RIGHT || e.KeyCode == SWT.ARROW_UP)
        {
            SetSelection(_selection + _increment);
        }
        else if (e.KeyCode == SWT.PAGE_UP)
        {
            SetSelection(_selection + _pageIncrement);
        }
        else if (e.KeyCode == SWT.PAGE_DOWN)
        {
            SetSelection(_selection - _pageIncrement);
        }
        else if (e.KeyCode == SWT.HOME)
        {
            SetSelection(_minimum);
        }
        else if (e.KeyCode == SWT.END)
        {
            SetSelection(_maximum);
        }
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
    /// Converts platform key event arguments to SWT state mask.
    /// </summary>
    private int GetStateMaskFromPlatformArgs(PlatformKeyEventArgs e)
    {
        int stateMask = 0;
        if (e.Shift) stateMask |= SWT.SHIFT;
        if (e.Control) stateMask |= SWT.CTRL;
        if (e.Alt) stateMask |= SWT.ALT;
        if (e.Command) stateMask |= SWT.COMMAND;
        return stateMask;
    }

    protected override void ReleaseWidget()
    {
        // Unsubscribe from platform widget events to prevent memory leaks
        if (PlatformWidget is IPlatformScale scaleWidget)
        {
            scaleWidget.ValueChanged -= OnPlatformValueChanged;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        // Platform widget cleanup is handled by parent disposal
        base.ReleaseWidget();
    }
}
