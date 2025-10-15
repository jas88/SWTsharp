using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Progress bar states for visual feedback.
/// </summary>
public static class ProgressBarState
{
    public const int NORMAL = 0;
    public const int ERROR = 1;
    public const int PAUSED = 2;
}

/// <summary>
/// A progress indicator showing the completion status of a task.
/// Supports horizontal/vertical orientation, smooth/segmented display, and indeterminate mode.
/// </summary>
public class ProgressBar : Control
{
    private int _minimum;
    private int _maximum = 100;
    private int _selection;
    private int _state = ProgressBarState.NORMAL;

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
                UpdateSelection();
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
                UpdateSelection();
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
    /// Gets or sets the visual state of the progress bar.
    /// </summary>
    public int State
    {
        get
        {
            CheckWidget();
            return _state;
        }
        set
        {
            CheckWidget();
            if (_state != value)
            {
                _state = value;
                UpdateState();
            }
        }
    }

    /// <summary>
    /// Creates a new progress bar with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (HORIZONTAL, VERTICAL, SMOOTH, INDETERMINATE).</param>
    public ProgressBar(Composite parent, int style) : base(parent, style)
    {
        if ((style & (SWT.HORIZONTAL | SWT.VERTICAL)) == 0)
        {
            style |= SWT.HORIZONTAL;
        }
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific progress bar widget.
    /// </summary>
    private void CreateWidget()
    {
        // Create IPlatformProgressBar widget using platform widget interface
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateProgressBarWidget(parentWidget, Style);
    }

    /// <summary>
    /// Sets the minimum value.
    /// </summary>
    public void SetMinimum(int value)
    {
        Minimum = value;
    }

    /// <summary>
    /// Sets the maximum value.
    /// </summary>
    public void SetMaximum(int value)
    {
        Maximum = value;
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
            UpdateSelection();
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
    /// Sets the state of the progress bar (NORMAL, ERROR, PAUSED).
    /// </summary>
    public void SetState(int state)
    {
        State = state;
    }

    /// <summary>
    /// Gets the state of the progress bar.
    /// </summary>
    public int GetState()
    {
        CheckWidget();
        return _state;
    }

    /// <summary>
    /// Updates the selection on the native control.
    /// </summary>
    private void UpdateSelection()
    {
        // Use IPlatformProgressBar interface to update selection
        if (PlatformWidget is IPlatformProgressBar progressBarWidget)
        {
            progressBarWidget.Minimum = _minimum;
            progressBarWidget.Maximum = _maximum;
            progressBarWidget.Value = _selection;
        }
    }

    /// <summary>
    /// Updates the state on the native control.
    /// </summary>
    private void UpdateState()
    {
        // Use IPlatformProgressBar interface to update state
        if (PlatformWidget is IPlatformProgressBar progressBarWidget)
        {
            progressBarWidget.State = _state;
        }
    }

    protected override void ReleaseWidget()
    {
        // TODO: Implement proper widget disposal through platform widget interface
        // Platform widget cleanup is handled by parent disposal
        base.ReleaseWidget();
    }
}
