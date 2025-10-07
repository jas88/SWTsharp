using SWTSharp.Events;

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
            if (Handle != IntPtr.Zero)
            {
                SWTSharp.Platform.PlatformFactory.Instance.SetTextLimit(Handle, _textLimit);
            }
            // Only truncate if limit > 0 (0 means unlimited)
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.Substring(0, _textLimit);
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
            return _readOnly;
        }
        set
        {
            CheckWidget();
            if (_readOnly != value)
            {
                _readOnly = value;
                if (Handle != IntPtr.Zero)
                {
                    SWTSharp.Platform.PlatformFactory.Instance.SetTextReadOnly(Handle, _readOnly);
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
        IntPtr parentHandle = Parent?.Handle ?? IntPtr.Zero;

        Handle = SWTSharp.Platform.PlatformFactory.Instance.CreateText(parentHandle, Style);

        if (Handle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create native text control");

        if (!string.IsNullOrEmpty(_text))
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetTextContent(Handle, _text);
        }

        if (_readOnly)
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
            TextContent = GetText() + text;
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
        if (Handle != IntPtr.Zero)
        {
            _text = SWTSharp.Platform.PlatformFactory.Instance.GetTextContent(Handle);
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

        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetTextSelection(Handle, start, end);
        }
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
        string currentText = GetText();
        if (start >= 0 && end > start && end <= currentText.Length)
        {
            return currentText.Substring(start, end - start);
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
        // Platform will handle clipboard operations
        // This is a placeholder for when clipboard support is implemented
    }

    /// <summary>
    /// Cuts the selected text to the clipboard.
    /// </summary>
    public void Cut()
    {
        CheckWidget();
        if (_readOnly)
            return;

        // Platform will handle clipboard operations
        // This is a placeholder for when clipboard support is implemented
    }

    /// <summary>
    /// Pastes text from the clipboard at the current cursor position.
    /// </summary>
    public void Paste()
    {
        CheckWidget();
        if (_readOnly)
            return;

        // Platform will handle clipboard operations
        // This is a placeholder for when clipboard support is implemented
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
        if (Handle == IntPtr.Zero)
            return;

        SWTSharp.Platform.PlatformFactory.Instance.SetTextContent(Handle, _text);
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
