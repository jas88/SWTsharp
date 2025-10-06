using SWTSharp.Events;

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
                if (Handle != IntPtr.Zero)
                {
                    Platform.PlatformFactory.Instance.SetSpinnerTextLimit(Handle, _textLimit);
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
        var parentHandle = Parent?.Handle ?? IntPtr.Zero;
        Handle = Platform.PlatformFactory.Instance.CreateSpinner(parentHandle, Style);
        Platform.PlatformFactory.Instance.SetSpinnerValues(Handle, _selection, _minimum, _maximum, _digits, _increment, _pageIncrement);
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
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetSpinnerValues(Handle, _selection, _minimum, _maximum, _digits, _increment, _pageIncrement);
        }
    }

    /// <summary>
    /// Connects platform-specific event handlers.
    /// </summary>
    private void ConnectEventHandlers()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.ConnectSpinnerChanged(Handle, OnNativeSelectionChanged);
            Platform.PlatformFactory.Instance.ConnectSpinnerModified(Handle, OnNativeModified);
        }
    }

    /// <summary>
    /// Called when the native spinner value changes.
    /// </summary>
    private void OnNativeSelectionChanged(int newValue)
    {
        if (_selection != newValue)
        {
            _selection = newValue;
            var evt = new Event
            {
                Widget = this,
                Detail = SWT.NONE
            };
            NotifyListeners(SWT.Selection, evt);
        }
    }

    /// <summary>
    /// Called when the text field is modified.
    /// </summary>
    private void OnNativeModified()
    {
        var evt = new Event
        {
            Widget = this
        };
        NotifyListeners(SWT.Modify, evt);
    }

    protected override void ReleaseWidget()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.DestroyWindow(Handle);
        }
        base.ReleaseWidget();
    }
}
