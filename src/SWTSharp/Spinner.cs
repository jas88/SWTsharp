using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// A selectable user interface object that allows the user to enter a number or select
/// a number from a predefined range using up/down buttons.
/// </summary>
public class Spinner : Composite
{
    private int _minimum;
    private int _maximum = 100;
    private int _selection;
    private int _increment = 1;
    private int _pageIncrement = 10;
    private int _digits;
    private int _textLimit;

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
    /// Gets or sets the increment value for the up/down buttons.
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
    /// Gets or sets the number of decimal places to display.
    /// </summary>
    public int Digits
    {
        get
        {
            CheckWidget();
            return _digits;
        }
        set
        {
            CheckWidget();
            if (_digits != value && value >= 0)
            {
                _digits = value;
                UpdateValues();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of characters that the text field can hold.
    /// A value of 0 removes the limit (unlimited).
    /// </summary>
    public int TextLimit
    {
        get
        {
            CheckWidget();
            return _textLimit;
        }
        set
        {
            CheckWidget();
            if (_textLimit != value && value >= 0)
            {
                _textLimit = value;
                if (PlatformWidget is IPlatformSpinner spinnerWidget)
                {
                    spinnerWidget.TextLimit = _textLimit;
                }
            }
        }
    }

    /// <summary>
    /// Creates a new spinner with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (READ_ONLY, WRAP).</param>
    public Spinner(Composite parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific spinner widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // Create IPlatformSpinner widget using platform widget interface
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateSpinnerWidget(parentWidget, Style);

        // Set initial spinner values and connect event handlers
        UpdateValues();
        ConnectEventHandlers();
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
    /// Sets all spinner values at once.
    /// </summary>
    public void SetValues(int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        CheckWidget();
        _minimum = minimum;
        _maximum = maximum;
        _digits = digits;
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
        // Use IPlatformSpinner interface to update spinner values
        if (PlatformWidget is IPlatformSpinner spinnerWidget)
        {
            spinnerWidget.Minimum = _minimum;
            spinnerWidget.Maximum = _maximum;
            spinnerWidget.Value = _selection;
            spinnerWidget.Increment = _increment;
            spinnerWidget.Digits = _digits;
        }
    }

    /// <summary>
    /// Connects platform-specific event handlers.
    /// </summary>
    private void ConnectEventHandlers()
    {
        // Connect spinner event handlers to platform widget
        if (PlatformWidget is IPlatformSpinner spinnerWidget)
        {
            spinnerWidget.ValueChanged += OnPlatformValueChanged;
            spinnerWidget.TextChanged += OnPlatformTextChanged;
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
    /// Handles platform widget text changed events.
    /// </summary>
    private void OnPlatformTextChanged(object? sender, string newText)
    {
        CheckWidget();

        var modifyEvent = new Event
        {
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.Modify, modifyEvent);
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
        if (PlatformWidget is IPlatformSpinner spinnerWidget)
        {
            spinnerWidget.ValueChanged -= OnPlatformValueChanged;
            spinnerWidget.TextChanged -= OnPlatformTextChanged;
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
