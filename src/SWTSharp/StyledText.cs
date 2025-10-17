using SWTSharp.Events;
using SWTSharp.Platform;
using SWTSharp.Graphics;

namespace SWTSharp;

/// <summary>
/// Represents a rich text editor with formatting capabilities.
/// Supports styles, colors, fonts, and advanced text editing.
/// </summary>
public class StyledText : Control
{
    private string _text = string.Empty;
    private int _textLimit = int.MaxValue;
    private bool _editable = true;
    private int _caretOffset;
    private int _topIndex;

    /// <summary>
    /// Gets or sets the text content.
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
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.SliceToString(0, _textLimit);
            }
            UpdateText();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of characters.
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
                throw new ArgumentException("Text limit cannot be negative");
            _textLimit = value;
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.SliceToString(0, _textLimit);
                UpdateText();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the text is editable.
    /// </summary>
    public bool Editable
    {
        get
        {
            CheckWidget();
            return _editable;
        }
        set
        {
            CheckWidget();
            _editable = value;
            if (PlatformWidget is IPlatformStyledText styledTextWidget)
            {
                styledTextWidget.SetEditable(_editable);
            }
        }
    }

    /// <summary>
    /// Gets or sets the caret offset.
    /// </summary>
    public int CaretOffset
    {
        get
        {
            CheckWidget();
            return _caretOffset;
        }
        set
        {
            CheckWidget();
            SetCaretOffset(value);
        }
    }

    /// <summary>
    /// Gets the number of lines in the text.
    /// </summary>
    public int LineCount
    {
        get
        {
            CheckWidget();
            if (PlatformWidget is IPlatformStyledText styledTextWidget)
            {
                return styledTextWidget.GetLineCount();
            }
            return _text.Split('\n').Length;
        }
    }

    /// <summary>
    /// Occurs when the text is modified.
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Creates a new styled text control.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (MULTI, WRAP, READ_ONLY, etc.).</param>
    public StyledText(Composite parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    private void CreateWidget()
    {
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateStyledTextWidget(parentWidget, Style);

        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.SetText(_text);
            styledTextWidget.SetEditable(_editable);
            styledTextWidget.TextChanged += OnPlatformTextChanged;
            styledTextWidget.SelectionChanged += OnPlatformSelectionChanged;
        }

        ConnectEventHandlers();
    }

    /// <summary>
    /// Appends text to the end.
    /// </summary>
    public void Append(string text)
    {
        CheckWidget();
        if (text != null)
        {
            _text += text;
            if (_textLimit > 0 && _text.Length > _textLimit)
            {
                _text = _text.SliceToString(0, _textLimit);
            }
            UpdateText();
        }
    }

    /// <summary>
    /// Inserts text at the current caret position.
    /// </summary>
    public void Insert(string text)
    {
        CheckWidget();
        if (text == null) return;

        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.Insert(text);
        }
    }

    /// <summary>
    /// Replaces text in a range.
    /// </summary>
    public void ReplaceTextRange(int start, int length, string text)
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.ReplaceTextRange(start, length, text);
        }
    }

    /// <summary>
    /// Sets the text selection.
    /// </summary>
    public void SetSelection(int start, int end)
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.SetSelection(start, end);
        }
    }

    /// <summary>
    /// Gets the current selection range.
    /// </summary>
    public (int Start, int End) GetSelection()
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            return styledTextWidget.GetSelection();
        }
        return (0, 0);
    }

    /// <summary>
    /// Gets the selected text.
    /// </summary>
    public string GetSelectionText()
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            return styledTextWidget.GetSelectionText();
        }
        return string.Empty;
    }

    /// <summary>
    /// Selects all text.
    /// </summary>
    public void SelectAll()
    {
        CheckWidget();
        SetSelection(0, _text.Length);
    }

    /// <summary>
    /// Sets the caret offset.
    /// </summary>
    public void SetCaretOffset(int offset)
    {
        CheckWidget();
        _caretOffset = Math.Max(0, Math.Min(_text.Length, offset));
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.SetCaretOffset(_caretOffset);
        }
    }

    /// <summary>
    /// Gets the caret offset.
    /// </summary>
    public int GetCaretOffset()
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            return styledTextWidget.GetCaretOffset();
        }
        return _caretOffset;
    }

    /// <summary>
    /// Sets the text style range with foreground/background colors and font.
    /// </summary>
    public void SetStyleRange(StyleRange range)
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.SetStyleRange(range);
        }
    }

    /// <summary>
    /// Gets the line at the specified index.
    /// </summary>
    public string GetLine(int lineIndex)
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            return styledTextWidget.GetLine(lineIndex);
        }
        var lines = _text.Split('\n');
        return lineIndex >= 0 && lineIndex < lines.Length ? lines[lineIndex] : string.Empty;
    }

    /// <summary>
    /// Copies the selected text to the clipboard.
    /// </summary>
    public void Copy()
    {
        CheckWidget();
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.Copy();
        }
    }

    /// <summary>
    /// Cuts the selected text to the clipboard.
    /// </summary>
    public void Cut()
    {
        CheckWidget();
        if (!_editable) return;
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.Cut();
        }
    }

    /// <summary>
    /// Pastes text from the clipboard.
    /// </summary>
    public void Paste()
    {
        CheckWidget();
        if (!_editable) return;
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.Paste();
        }
    }

    private void UpdateText()
    {
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.SetText(_text);
        }
        OnTextChanged(EventArgs.Empty);
    }

    private void ConnectEventHandlers()
    {
        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained += OnPlatformFocusGained;
            eventHandling.FocusLost += OnPlatformFocusLost;
            eventHandling.KeyDown += OnPlatformKeyDown;
            eventHandling.KeyUp += OnPlatformKeyUp;
        }
    }

    private void OnPlatformTextChanged(object? sender, string newText)
    {
        CheckWidget();
        _text = newText;
        OnTextChanged(EventArgs.Empty);
    }

    private void OnPlatformSelectionChanged(object? sender, int detail)
    {
        CheckWidget();
        OnSelectionChanged(EventArgs.Empty);
    }

    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
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
        if (PlatformWidget is IPlatformStyledText styledTextWidget)
        {
            styledTextWidget.TextChanged -= OnPlatformTextChanged;
            styledTextWidget.SelectionChanged -= OnPlatformSelectionChanged;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        TextChanged = null;
        SelectionChanged = null;
        base.ReleaseWidget();
    }
}

/// <summary>
/// Represents a style range for styled text.
/// </summary>
public class StyleRange
{
    /// <summary>
    /// Gets or sets the start offset.
    /// </summary>
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public RGB? Foreground { get; set; }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public RGB? Background { get; set; }

    /// <summary>
    /// Gets or sets the font style (SWT.NORMAL, SWT.BOLD, SWT.ITALIC).
    /// </summary>
    public int FontStyle { get; set; }
}
