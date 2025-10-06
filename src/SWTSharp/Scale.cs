using SWTSharp.Events;

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
        var parentHandle = Parent?.Handle ?? IntPtr.Zero;
        Handle = Platform.PlatformFactory.Instance.CreateScale(parentHandle, Style);
        Platform.PlatformFactory.Instance.SetScaleValues(Handle, _selection, _minimum, _maximum, _increment, _pageIncrement);
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
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.SetScaleValues(Handle, _selection, _minimum, _maximum, _increment, _pageIncrement);
        }
    }

    /// <summary>
    /// Connects platform-specific event handlers.
    /// </summary>
    private void ConnectEventHandlers()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.ConnectScaleChanged(Handle, OnNativeSelectionChanged);
        }
    }

    /// <summary>
    /// Called when the native scale value changes.
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

    protected override void ReleaseWidget()
    {
        if (Handle != IntPtr.Zero)
        {
            Platform.PlatformFactory.Instance.DestroyWindow(Handle);
        }
        base.ReleaseWidget();
    }
}
