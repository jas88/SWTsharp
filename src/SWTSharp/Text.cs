using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents an editable text control.
/// Can be single-line or multi-line, and can be read-only.
/// </summary>
public class Text : Control
{
    private string _text = string.Empty;
    private int _textLimit = int.MaxValue;
    private bool _readOnly;
    private char _echoChar;

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
            // Only truncate if limit > 0 (0 means unlimited)
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.SliceToString(0, _textLimit);
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
            // Delegate to platform widget if available
            if (PlatformWidget is IPlatformTextInput textInput)
            {
                textInput.SetTextLimit(value);
            }
            // Only truncate if limit > 0 (0 means unlimited)
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.SliceToString(0, _textLimit);
                UpdateText();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the text control is read-only.
    /// </summary>
    public bool ReadOnly
    {
        get
        {
            CheckWidget();
            // Use platform widget if available
            if (PlatformWidget is IPlatformTextInput textInput)
            {
                return textInput.GetReadOnly();
            }
            return _readOnly;
        }
        set
        {
            CheckWidget();
            if (_readOnly != value)
            {
                _readOnly = value;
                // Delegate to platform widget if available
                if (PlatformWidget is IPlatformTextInput textInput)
                {
                    textInput.SetReadOnly(value);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the echo character for password fields.
    /// Set to '\0' to disable echoing (normal text display).
    /// </summary>
    public char EchoChar
    {
        get
        {
            CheckWidget();
            return _echoChar;
        }
        set
        {
            CheckWidget();
            _echoChar = value;
            // Echo char is typically set through style bits in SWT
            // This property provides runtime control if needed
        }
    }

    /// <summary>
    /// Occurs when the text is modified.
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    /// Occurs before text is modified (verification event).
    /// </summary>
    public event EventHandler<VerifyEventArgs>? Verify;

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a new text control.
    /// </summary>
    public Text(Control parent, int style) : base(parent, style)
    {
        _readOnly = (style & SWT.READ_ONLY) != 0;
        _echoChar = (style & SWT.PASSWORD) != 0 ? '*' : '\0';
        CreateWidget();
    }

    private void CreateWidget()
    {
        // Use platform widget - must complete before subscribing to events
        var widget = SWTSharp.Platform.PlatformFactory.Instance.CreateTextWidget(
            Parent?.PlatformWidget,
            Style
        );

        // Only assign after successful creation
        PlatformWidget = widget;

        // Set initial properties via platform widget interface
        if (PlatformWidget is IPlatformTextInput textInput)
        {
            textInput.SetText(_text);
            textInput.SetReadOnly(_readOnly);
            if (_textLimit != int.MaxValue)
            {
                textInput.SetTextLimit(_textLimit);
            }
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
            string currentText = GetText();
            _text = currentText + text;
            // Only truncate if limit > 0 (0 means unlimited)
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.SliceToString(0, _textLimit);
            }
            UpdateText();
        }
    }

    /// <summary>
    /// Inserts text at the current cursor position.
    /// </summary>
    public void Insert(string text)
    {
        CheckWidget();
        if (text == null)
            return;

        var (start, end) = GetSelection();
        string currentText = GetText();

        // Remove selected text if any
        if (start != end)
        {
            currentText = currentText.Remove(start, end - start);
        }

        // Insert new text at cursor position
        currentText = currentText.Insert(start, text);
        TextContent = currentText;

        // Set cursor after inserted text
        int newPos = start + text.Length;
        SetSelection(newPos, newPos);
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
        // Use platform widget if available
        if (PlatformWidget is IPlatformTextInput textInput)
        {
            _text = textInput.GetText();
        }
        return _text;
    }

    /// <summary>
    /// Sets the text content.
    /// </summary>
    public void SetText(string text)
    {
        CheckWidget();
        TextContent = text;
    }

    /// <summary>
    /// Sets the text selection.
    /// </summary>
    public void SetSelection(int start, int end)
    {
        CheckWidget();
        if (start < 0 || end < 0 || start > end)
            throw new ArgumentException("Invalid selection range");

        // Delegate to platform widget if available
        if (PlatformWidget is IPlatformTextInput textInput)
        {
            textInput.SetSelection(start, end);
        }
    }

    /// <summary>
    /// Gets the current selection range.
    /// </summary>
    public (int Start, int End) GetSelection()
    {
        CheckWidget();
        // Use platform widget if available
        if (PlatformWidget is IPlatformTextInput textInput)
        {
            return textInput.GetSelection();
        }
        // Fallback: no selection (cursor at start)
        return (0, 0);
    }

    /// <summary>
    /// Gets the selected text.
    /// </summary>
    public string GetSelectionText()
    {
        CheckWidget();
        var (start, end) = GetSelection();
        string currentText = GetText();
        if (start >= 0 && end > start && end <= currentText.Length)
        {
            return currentText.SliceToString(start, end - start);
        }
        return string.Empty;
    }

    /// <summary>
    /// Selects all text in the control.
    /// </summary>
    public void SelectAll()
    {
        CheckWidget();
        string text = GetText();
        SetSelection(0, text.Length);
    }

    /// <summary>
    /// Clears the current selection (sets cursor position without selection).
    /// </summary>
    public void ClearSelection()
    {
        CheckWidget();
        var (start, _) = GetSelection();
        SetSelection(start, start);
    }

    /// <summary>
    /// Copies the selected text to the clipboard.
    /// </summary>
    public void Copy()
    {
        CheckWidget();
        // Delegate to platform widget if available
        if (PlatformWidget is IPlatformClipboard clipboard)
        {
            clipboard.Copy();
        }
        // Note: Copy is allowed even for read-only controls
    }

    /// <summary>
    /// Cuts the selected text to the clipboard.
    /// </summary>
    public void Cut()
    {
        CheckWidget();
        if (_readOnly)
            return;

        // Delegate to platform widget if available
        if (PlatformWidget is IPlatformClipboard clipboard)
        {
            clipboard.Cut();
        }
    }

    /// <summary>
    /// Pastes text from the clipboard at the current cursor position.
    /// </summary>
    public void Paste()
    {
        CheckWidget();
        if (_readOnly)
            return;

        // Delegate to platform widget if available
        if (PlatformWidget is IPlatformClipboard clipboard)
        {
            clipboard.Paste();
        }
    }

    /// <summary>
    /// Raises the TextChanged event.
    /// </summary>
    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the Verify event before text changes.
    /// </summary>
    protected virtual void OnVerify(VerifyEventArgs e)
    {
        Verify?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }

    private void UpdateText()
    {
        // Delegate to platform widget if available
        if (PlatformWidget is IPlatformTextInput textInput)
        {
            textInput.SetText(_text);
        }
        OnTextChanged(EventArgs.Empty);
    }

    protected override void ReleaseWidget()
    {
        TextChanged = null;
        Verify = null;
        SelectionChanged = null;
        base.ReleaseWidget();
    }
}

/// <summary>
/// Event arguments for text verification events.
/// </summary>
public class VerifyEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the text to be inserted.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start position of the change.
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the end position of the change.
    /// </summary>
    public int End { get; set; }

    /// <summary>
    /// Gets or sets whether the change should be allowed.
    /// Set to false to prevent the change.
    /// </summary>
    public bool Doit { get; set; } = true;
}
