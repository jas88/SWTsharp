using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a non-selectable text label or separator.
/// Labels can display text with various alignment and wrapping options,
/// or act as horizontal/vertical separators.
/// </summary>
public class Label : Control
{
    private string _text = string.Empty;
    private int _alignment = SWT.LEFT;
    private bool _isSeparator;

    /// <summary>
    /// Gets or sets the label's text.
    /// This property is only applicable for text labels, not separators.
    /// </summary>
    public string Text
    {
        get
        {
            CheckWidget();
            // NEW: Use platform widget
            if (PlatformWidget is IPlatformTextWidget textWidget && !_isSeparator)
            {
                return textWidget.GetText();
            }
            // OLD: Fallback to internal field for backwards compatibility
            return _text;
        }
        set
        {
            CheckWidget();
            if (_isSeparator)
                return; // Separators don't have text

            _text = value ?? string.Empty;
            // Use platform widget
            if (PlatformWidget is IPlatformTextWidget textWidget)
            {
                textWidget.SetText(_text);
            }
        }
    }

    /// <summary>
    /// Gets or sets the text alignment (LEFT, CENTER, or RIGHT).
    /// This property is only applicable for text labels, not separators.
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
            if (_isSeparator)
                return; // Separators don't have alignment

            if (_alignment != value &&
                (value == SWT.LEFT || value == SWT.CENTER || value == SWT.RIGHT))
            {
                _alignment = value;
                // TODO: Implement alignment updates through platform widget interface
            }
        }
    }

    /// <summary>
    /// Gets whether this label is a separator.
    /// </summary>
    public bool IsSeparator
    {
        get
        {
            CheckWidget();
            return _isSeparator;
        }
    }

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="parent">The parent composite control</param>
    /// <param name="style">
    /// Style bits. Can include:
    /// - Alignment: SWT.LEFT (default), SWT.CENTER, SWT.RIGHT
    /// - Text wrapping: SWT.WRAP
    /// - Separator: SWT.SEPARATOR combined with SWT.HORIZONTAL or SWT.VERTICAL
    /// - Shadow: SWT.SHADOW_IN, SWT.SHADOW_OUT, SWT.SHADOW_ETCHED_IN, SWT.SHADOW_ETCHED_OUT
    /// - Border: SWT.BORDER
    /// </param>
    public Label(Control parent, int style) : base(parent, style)
    {
        // Check if this is a separator
        _isSeparator = (style & SWT.SEPARATOR) != 0;

        if (!_isSeparator)
        {
            // Extract alignment from style for text labels
            if ((style & SWT.CENTER) != 0)
                _alignment = SWT.CENTER;
            else if ((style & SWT.RIGHT) != 0)
                _alignment = SWT.RIGHT;
            else
                _alignment = SWT.LEFT;
        }

        CreateWidget();
    }

    private void CreateWidget()
    {
        // Use platform widget
        PlatformWidget = SWTSharp.Platform.PlatformFactory.Instance.CreateLabelWidget(
            Parent?.PlatformWidget,
            Style
        );

        // Set initial text if this is a text label (not a separator)
        if (!_isSeparator && !string.IsNullOrEmpty(_text))
        {
            // Use platform widget
            if (PlatformWidget is IPlatformTextWidget textWidget)
            {
                textWidget.SetText(_text);
            }
        }
    }

    
    /// <summary>
    /// Returns a string representation of this label.
    /// </summary>
    public override string ToString()
    {
        if (_isSeparator)
        {
            string orientation = (Style & SWT.VERTICAL) != 0 ? "VERTICAL" : "HORIZONTAL";
            return $"Label {{Separator, {orientation}}}";
        }
        return $"Label {{Text: \"{_text}\", Alignment: {GetAlignmentName()}}}";
    }

    private string GetAlignmentName()
    {
        if (_alignment == SWT.CENTER) return "CENTER";
        if (_alignment == SWT.RIGHT) return "RIGHT";
        return "LEFT";
    }

    protected override void ReleaseWidget()
    {
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
