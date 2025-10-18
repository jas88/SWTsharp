using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a standalone scrollbar control.
/// </summary>
public class ScrollBar : Control
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
    /// Gets or sets the thumb (visible range) size.
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
    /// Gets whether the scrollbar is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            CheckWidget();
            return _maximum > _minimum;
        }
    }

    /// <summary>
    /// Creates a new scrollbar control.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (HORIZONTAL or VERTICAL).</param>
    public ScrollBar(Composite parent, int style) : base(parent, style)
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
        PlatformWidget = Platform.PlatformFactory.Instance.CreateScrollBarWidget(parentWidget, Style);

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
    /// Gets the current size of the scrollbar widget.
    /// </summary>
    /// <returns>The size as a Point (width, height)</returns>
    public Graphics.Point GetSize()
    {
        CheckWidget();
        if (PlatformWidget != null)
        {
            var bounds = PlatformWidget.GetBounds();
            return new Graphics.Point(bounds.Width, bounds.Height);
        }
        return new Graphics.Point(0, 0);
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
    /// Sets all scrollbar values at once.
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

    private void UpdateValues()
    {
        if (PlatformWidget is IPlatformScrollBar scrollBarWidget)
        {
            scrollBarWidget.Minimum = _minimum;
            scrollBarWidget.Maximum = _maximum;
            scrollBarWidget.Value = _selection;
            scrollBarWidget.Increment = _increment;
            scrollBarWidget.PageIncrement = _pageIncrement;
            scrollBarWidget.Thumb = _thumb;
        }
    }

    private void ConnectEventHandlers()
    {
        if (PlatformWidget is IPlatformScrollBar scrollBarWidget)
        {
            scrollBarWidget.ValueChanged += OnPlatformValueChanged;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained += OnPlatformFocusGained;
            eventHandling.FocusLost += OnPlatformFocusLost;
            eventHandling.KeyDown += OnPlatformKeyDown;
            eventHandling.KeyUp += OnPlatformKeyUp;
        }
    }

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
        if (PlatformWidget is IPlatformScrollBar scrollBarWidget)
        {
            scrollBarWidget.ValueChanged -= OnPlatformValueChanged;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        base.ReleaseWidget();
    }
}
