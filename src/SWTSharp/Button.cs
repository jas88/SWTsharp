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
            return _text;
        }
        set
        {
            CheckWidget();
            _text = value ?? string.Empty;
            UpdateText();
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
                UpdateSelection();
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
        // Get parent handle
        IntPtr parentHandle = IntPtr.Zero;
        if (Parent != null)
        {
            parentHandle = Parent.Handle;
        }

        // Create platform-specific button
        Handle = SWTSharp.Platform.PlatformFactory.Instance.CreateButton(parentHandle, style, _text);

        // Connect click event handler
        SWTSharp.Platform.PlatformFactory.Instance.ConnectButtonClick(Handle, () => OnClick(EventArgs.Empty));

        // Apply initial selection state if this is a checkable button
        if ((style & (SWT.CHECK | SWT.RADIO | SWT.TOGGLE)) != 0)
        {
            UpdateSelection();
        }
    }

    /// <summary>
    /// Raises the Click event.
    /// </summary>
    protected virtual void OnClick(EventArgs e)
    {
        // For CHECK, RADIO, and TOGGLE buttons, update internal selection state
        if ((Style & (SWT.CHECK | SWT.RADIO | SWT.TOGGLE)) != 0)
        {
            _selection = SWTSharp.Platform.PlatformFactory.Instance.GetButtonSelection(Handle);
        }

        Click?.Invoke(this, e);
    }

    private void UpdateText()
    {
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetButtonText(Handle, _text);
        }
    }

    private void UpdateSelection()
    {
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetButtonSelection(Handle, _selection);
        }
    }

    protected override void UpdateVisible()
    {
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetControlVisible(Handle, Visible);
        }
    }

    protected override void UpdateEnabled()
    {
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetControlEnabled(Handle, Enabled);
        }
    }

    protected override void UpdateBounds()
    {
        if (Handle != IntPtr.Zero)
        {
            var (x, y, width, height) = GetBounds();
            SWTSharp.Platform.PlatformFactory.Instance.SetControlBounds(Handle, x, y, width, height);
        }
    }

    protected override void ReleaseWidget()
    {
        // Platform handles cleanup via parent destruction
        base.ReleaseWidget();
    }
}
