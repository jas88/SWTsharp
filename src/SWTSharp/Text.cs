namespace SWTSharp;

/// <summary>
/// Represents an editable text control.
/// Can be single-line or multi-line, and can be read-only.
/// </summary>
public class Text : Control
{
    private string _text = string.Empty;
    private int _textLimit = int.MaxValue;

    /// <summary>
    /// Gets or sets the text content.
    /// </summary>
    public string TextContent
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
            if (_text.Length > _textLimit)
            {
                _text = _text.Substring(0, _textLimit);
            }
            UpdateText();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of characters that can be entered.
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
            if (value < 0)
            {
                throw new ArgumentException("Text limit cannot be negative");
            }
            _textLimit = value;
            if (_text.Length > _textLimit)
            {
                _text = _text.Substring(0, _textLimit);
                UpdateText();
            }
        }
    }

    /// <summary>
    /// Occurs when the text is modified.
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    /// Creates a new text control.
    /// </summary>
    public Text(Control parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    private void CreateWidget()
    {
        IntPtr parentHandle = Parent?.Handle ?? IntPtr.Zero;

        Handle = SWTSharp.Platform.PlatformFactory.Instance.CreateText(parentHandle, Style);

        if (Handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create native text control");

        if (!string.IsNullOrEmpty(_text))
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetTextContent(Handle, _text);
        }

        if ((Style & SWT.READ_ONLY) != 0)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetTextReadOnly(Handle, true);
        }

        if (_textLimit != int.MaxValue)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetTextLimit(Handle, _textLimit);
        }
    }

    /// <summary>
    /// Appends text to the end of the current text.
    /// </summary>
    public void Append(string text)
    {
        CheckWidget();
        if (text != null)
        {
            TextContent = _text + text;
        }
    }

    /// <summary>
    /// Clears the text.
    /// </summary>
    public void ClearText()
    {
        CheckWidget();
        TextContent = string.Empty;
    }

    /// <summary>
    /// Gets the current text from the platform control.
    /// </summary>
    public string GetText()
    {
        CheckWidget();
        if (Handle != IntPtr.Zero)
        {
            _text = SWTSharp.Platform.PlatformFactory.Instance.GetTextContent(Handle);
        }
        return _text;
    }

    /// <summary>
    /// Sets the text selection.
    /// </summary>
    public void SetSelection(int start, int end)
    {
        CheckWidget();
        if (start < 0 || end < 0 || start > end)
            throw new ArgumentException("Invalid selection range");

        SWTSharp.Platform.PlatformFactory.Instance.SetTextSelection(Handle, start, end);
    }

    /// <summary>
    /// Gets the current selection range.
    /// </summary>
    public (int Start, int End) GetSelection()
    {
        CheckWidget();
        if (Handle != IntPtr.Zero)
        {
            return SWTSharp.Platform.PlatformFactory.Instance.GetTextSelection(Handle);
        }
        return (0, 0);
    }

    /// <summary>
    /// Gets the selected text.
    /// </summary>
    public string GetSelectionText()
    {
        CheckWidget();
        var (start, end) = GetSelection();
        if (start >= 0 && end > start && end <= _text.Length)
        {
            return _text.Substring(start, end - start);
        }
        return string.Empty;
    }

    /// <summary>
    /// Raises the TextChanged event.
    /// </summary>
    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    private void UpdateText()
    {
        if (Handle == IntPtr.Zero)
            return;

        SWTSharp.Platform.PlatformFactory.Instance.SetTextContent(Handle, _text);
        OnTextChanged(EventArgs.Empty);
    }
}
