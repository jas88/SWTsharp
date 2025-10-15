using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// A selectable user interface object that represents a range of positive, numeric values.
/// Sliders are typically used to allow users to select a value from a continuous range.
/// </summary>
public class Slider : Control
{
    private int _minimum;
    private int _maximum = 100;
    private int _selection;
    private int _increment = 1;
    private int _pageIncrement = 10;
    private int _thumb = 10;

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
    /// Gets or sets the increment value for arrow buttons.
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
    /// Gets or sets the page increment value for clicking in the trough.
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
    /// Gets or sets the size of the thumb (slider handle).
    /// </summary>
    public int Thumb
    {
        get
        {
            CheckWidget();
            return _thumb;
        }
        set
        {
            CheckWidget();
            if (_thumb != value && value > 0)
            {
                _thumb = value;
                UpdateValues();
            }
        }
    }

    /// <summary>
    /// Creates a new slider with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (HORIZONTAL, VERTICAL).</param>
    public Slider(Composite parent, int style) : base(parent, style)
    {
        if ((style & (SWT.HORIZONTAL | SWT.VERTICAL)) == 0)
        {
            style |= SWT.HORIZONTAL;
        }
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific slider widget.
    /// </summary>
    private void CreateWidget()
    {
        // Create IPlatformSlider widget using platform widget interface
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateSliderWidget(parentWidget, Style);

        // Set initial slider values on platform widget
        UpdateValues();

        // Connect event handlers for slider value changes
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
    /// Sets all slider values at once.
    /// </summary>
    public void SetValues(int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    {
        CheckWidget();
        _minimum = minimum;
        _maximum = maximum;
        _thumb = thumb;
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
        // Use IPlatformSlider interface to update slider values
        if (PlatformWidget is IPlatformSlider sliderWidget)
        {
            sliderWidget.Minimum = _minimum;
            sliderWidget.Maximum = _maximum;
            sliderWidget.Value = _selection;
            sliderWidget.Increment = _increment;
            sliderWidget.PageIncrement = _pageIncrement;
            // Note: Thumb property not available in IPlatformSlider interface yet
            // TODO: Add Thumb property to IPlatformSlider interface when needed
        }
    }

    /// <summary>
    /// Connects platform-specific event handlers.
    /// </summary>
    private void ConnectEventHandlers()
    {
        // Connect slider value changed event handler to platform widget
        if (PlatformWidget is IPlatformSlider sliderWidget)
        {
            sliderWidget.ValueChanged += OnPlatformValueChanged;
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
                Index = newValue // Store the new value in the Index field
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

        // Handle arrow keys for slider navigation
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
        // TODO: Add Command key detection on macOS
        return stateMask;
    }

    protected override void ReleaseWidget()
    {
        // Unsubscribe from platform widget events to prevent memory leaks
        if (PlatformWidget is IPlatformSlider sliderWidget)
        {
            sliderWidget.ValueChanged -= OnPlatformValueChanged;
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
