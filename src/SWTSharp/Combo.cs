namespace SWTSharp;

/// <summary>
/// Represents a combination of a text field and a drop-down list.
/// Supports editable and read-only modes, as well as simple (always visible list) mode.
/// </summary>
public class Combo : Control
{
    private readonly System.Collections.Generic.List<string> _items = new();
    private string _text = string.Empty;
    private int _selectionIndex = -1;
    private bool _readOnly;
    private bool _dropDown;
    private int _textLimit = int.MaxValue;
    private int _visibleItemCount = 5;

    /// <summary>
    /// Gets or sets the text content of the combo.
    /// For read-only combos, this reflects the selected item.
    /// For editable combos, this can be any text.
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
            string newText = value ?? string.Empty;
            if (newText.Length > _textLimit)
            {
                newText = newText.SliceToString(0, _textLimit);
            }
            if (_text != newText)
            {
                _text = newText;
                UpdateText();
                OnTextChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets the number of items in the combo.
    /// </summary>
    public int ItemCount
    {
        get
        {
            CheckWidget();
            return _items.Count;
        }
    }

    /// <summary>
    /// Gets or sets all items in the combo.
    /// </summary>
    public string[] Items
    {
        get
        {
            CheckWidget();
            return _items.ToArray();
        }
        set
        {
            CheckWidget();
            _items.Clear();
            _selectionIndex = -1;
            if (value != null)
            {
                _items.AddRange(value);
                UpdateItems();
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the selected item.
    /// Returns -1 if no item is selected.
    /// </summary>
    public int SelectionIndex
    {
        get
        {
            CheckWidget();
            return _selectionIndex;
        }
        set
        {
            CheckWidget();
            if (value < -1 || value >= _items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Selection index out of range");
            }
            if (_selectionIndex != value)
            {
                _selectionIndex = value;
                if (value >= 0)
                {
                    _text = _items[value];
                }
                UpdateSelection();
                OnSelectionChanged(EventArgs.Empty);
            }
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
                Text = _text.SliceToString(0, _textLimit);
            }
            Platform.PlatformFactory.Instance.SetComboTextLimit(Handle, _textLimit);
        }
    }

    /// <summary>
    /// Gets or sets the number of items that are visible in the drop-down list.
    /// </summary>
    public int VisibleItemCount
    {
        get
        {
            CheckWidget();
            return _visibleItemCount;
        }
        set
        {
            CheckWidget();
            if (value < 1)
            {
                throw new ArgumentException("Visible item count must be at least 1");
            }
            _visibleItemCount = value;
            Platform.PlatformFactory.Instance.SetComboVisibleItemCount(Handle, value);
        }
    }

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Occurs when the text is modified (editable combos only).
    /// </summary>
    public event EventHandler? TextChanged;

    /// <summary>
    /// Creates a new combo control.
    /// </summary>
    /// <param name="parent">Parent control</param>
    /// <param name="style">Style flags (SWT.DROP_DOWN, SWT.READ_ONLY, SWT.SIMPLE)</param>
    public Combo(Control parent, int style) : base(parent, style)
    {
        _readOnly = (style & SWT.READ_ONLY) != 0;
        _dropDown = (style & SWT.DROP_DOWN) != 0 || (style & SWT.SIMPLE) == 0;
        CreateWidget();
    }

    /// <summary>
    /// Adds an item to the end of the combo.
    /// </summary>
    /// <param name="item">Item to add</param>
    public void Add(string item)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        _items.Add(item);
        Platform.PlatformFactory.Instance.AddComboItem(Handle, item, _items.Count - 1);
    }

    /// <summary>
    /// Adds an item at a specific index.
    /// </summary>
    /// <param name="item">Item to add</param>
    /// <param name="index">Index where to insert the item</param>
    public void Add(string item, int index)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        if (index < 0 || index > _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        _items.Insert(index, item);
        Platform.PlatformFactory.Instance.AddComboItem(Handle, item, index);

        // Update selection index if needed
        if (_selectionIndex >= index)
        {
            _selectionIndex++;
        }
    }

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">Index of the item to remove</param>
    public void Remove(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        _items.RemoveAt(index);
        Platform.PlatformFactory.Instance.RemoveComboItem(Handle, index);

        // Update selection index
        if (_selectionIndex == index)
        {
            _selectionIndex = -1;
            _text = string.Empty;
        }
        else if (_selectionIndex > index)
        {
            _selectionIndex--;
        }
    }

    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">Item to remove</param>
    public void Remove(string item)
    {
        CheckWidget();
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            Remove(index);
        }
    }

    /// <summary>
    /// Removes all items from the combo.
    /// </summary>
    public void RemoveAll()
    {
        CheckWidget();
        _items.Clear();
        _selectionIndex = -1;
        _text = string.Empty;
        Platform.PlatformFactory.Instance.ClearComboItems(Handle);
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">Item index</param>
    /// <returns>The item string</returns>
    public string GetItem(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        return _items[index];
    }

    /// <summary>
    /// Gets all items in the combo.
    /// </summary>
    /// <returns>Array of all items</returns>
    public string[] GetItems()
    {
        CheckWidget();
        return _items.ToArray();
    }

    /// <summary>
    /// Selects the item at the specified index.
    /// </summary>
    /// <param name="index">Index to select</param>
    public void Select(int index)
    {
        CheckWidget();
        if (index < 0 || index >= _items.Count)
        {
            return;
        }
        SelectionIndex = index;
    }

    /// <summary>
    /// Deselects all items.
    /// </summary>
    public void DeselectAll()
    {
        CheckWidget();
        _selectionIndex = -1;
        if (_readOnly)
        {
            _text = string.Empty;
        }
        UpdateSelection();
    }

    /// <summary>
    /// Clears the current text selection.
    /// </summary>
    public void ClearSelection()
    {
        CheckWidget();
        Platform.PlatformFactory.Instance.SetComboTextSelection(Handle, 0, 0);
    }

    /// <summary>
    /// Sets the text selection range.
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    public void SetSelection(int start, int end)
    {
        CheckWidget();
        if (start < 0 || end < 0 || start > _text.Length || end > _text.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid selection range");
        }
        Platform.PlatformFactory.Instance.SetComboTextSelection(Handle, start, end);
    }

    /// <summary>
    /// Gets the text selection range.
    /// </summary>
    /// <returns>Tuple of (start, end) positions</returns>
    public (int Start, int End) GetSelection()
    {
        CheckWidget();
        return Platform.PlatformFactory.Instance.GetComboTextSelection(Handle);
    }

    /// <summary>
    /// Searches the combo for an item.
    /// </summary>
    /// <param name="item">Item to find</param>
    /// <param name="start">Start index for search</param>
    /// <returns>Index of the item, or -1 if not found</returns>
    public int IndexOf(string item, int start = 0)
    {
        CheckWidget();
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }
        return _items.IndexOf(item, start);
    }

    /// <summary>
    /// Copies the selected text to the clipboard.
    /// </summary>
    public void Copy()
    {
        CheckWidget();
        Platform.PlatformFactory.Instance.ComboTextCopy(Handle);
    }

    /// <summary>
    /// Cuts the selected text to the clipboard (editable combos only).
    /// </summary>
    public void Cut()
    {
        CheckWidget();
        if (!_readOnly)
        {
            Platform.PlatformFactory.Instance.ComboTextCut(Handle);
        }
    }

    /// <summary>
    /// Pastes text from the clipboard (editable combos only).
    /// </summary>
    public void Paste()
    {
        CheckWidget();
        if (!_readOnly)
        {
            Platform.PlatformFactory.Instance.ComboTextPaste(Handle);
        }
    }

    private void CreateWidget()
    {
        Handle = Platform.PlatformFactory.Instance.CreateCombo(Parent?.Handle ?? IntPtr.Zero, Style);
        if (Handle == IntPtr.Zero)
        {
            throw new SWTException(SWT.ERROR_NO_HANDLES, "Failed to create combo control");
        }
    }

    private void UpdateItems()
    {
        Platform.PlatformFactory.Instance.ClearComboItems(Handle);
        for (int i = 0; i < _items.Count; i++)
        {
            Platform.PlatformFactory.Instance.AddComboItem(Handle, _items[i], i);
        }
    }

    private void UpdateText()
    {
        Platform.PlatformFactory.Instance.SetComboText(Handle, _text);
    }

    private void UpdateSelection()
    {
        Platform.PlatformFactory.Instance.SetComboSelection(Handle, _selectionIndex);
        if (_selectionIndex >= 0 && _selectionIndex < _items.Count)
        {
            _text = _items[_selectionIndex];
            UpdateText();
        }
    }

    /// <summary>
    /// Raises the SelectionChanged event.
    /// </summary>
    protected virtual void OnSelectionChanged(EventArgs e)
    {
        SelectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the TextChanged event.
    /// </summary>
    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    protected override void ReleaseWidget()
    {
        _items.Clear();
        _text = string.Empty;
        _selectionIndex = -1;
        base.ReleaseWidget();
    }
}
