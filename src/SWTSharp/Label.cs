namespace SWTSharp;

/// <summary>
/// Represents a non-selectable text label.
/// </summary>
public class Label : Control
{
    private string _text = string.Empty;
    private int _alignment = SWT.LEFT;

    /// <summary>
    /// Gets or sets the label's text.
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
    /// Gets or sets the text alignment (LEFT, CENTER, or RIGHT).
    /// </summary>
    public int Alignment
    {
        get
        {
            CheckWidget();
            return _alignment;
        }
        set
        {
            CheckWidget();
            if (_alignment != value)
            {
                _alignment = value;
                UpdateAlignment();
            }
        }
    }

    /// <summary>
    /// Creates a new label.
    /// </summary>
    public Label(Control parent, int style) : base(parent, style)
    {
        // Extract alignment from style
        if ((style & SWT.CENTER) != 0)
            _alignment = SWT.CENTER;
        else if ((style & SWT.RIGHT) != 0)
            _alignment = SWT.RIGHT;
        else
            _alignment = SWT.LEFT;

        CreateWidget();
    }

    private void CreateWidget()
    {
        IntPtr parentHandle = Parent?.Handle ?? IntPtr.Zero;
        bool wrap = (Style & SWT.WRAP) != 0;

        Handle = SWTSharp.Platform.PlatformFactory.Instance.CreateLabel(parentHandle, Style, _alignment, wrap);

        if (Handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create native label");

        if (!string.IsNullOrEmpty(_text))
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetLabelText(Handle, _text);
        }
    }

    private void UpdateText()
    {
        if (Handle == IntPtr.Zero)
            return;

        SWTSharp.Platform.PlatformFactory.Instance.SetLabelText(Handle, _text);
    }

    private void UpdateAlignment()
    {
        if (Handle == IntPtr.Zero)
            return;

        SWTSharp.Platform.PlatformFactory.Instance.SetLabelAlignment(Handle, _alignment);
    }
}
