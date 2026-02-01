namespace SWTSharp;

/// <summary>
/// Represents a column in a Table widget.
/// Defines the column header, width, alignment, and behavior.
/// </summary>
public class TableColumn : Widget
{
    private readonly Table _parent;
    private string _text = string.Empty;
    private int _width = 100;
    private int _alignment = SWT.LEFT;
    private bool _resizable = true;
    private bool _moveable = false;
    private string? _toolTipText;

    /// <summary>
    /// Gets the parent table.
    /// </summary>
    public Table Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the column header text.
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
            if (_text != value)
            {
                _text = value ?? string.Empty;
                if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
                {
                    int columnIndex = _parent.GetColumnIndex(this);
                    if (columnIndex >= 0)
                    {
                        platformTable.SetColumnText(columnIndex, _text);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the column width in pixels.
    /// </summary>
    public int Width
    {
        get
        {
            CheckWidget();
            return _width;
        }
        set
        {
            CheckWidget();
            if (_width != value && value >= 0)
            {
                _width = value;
                if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
                {
                    int columnIndex = _parent.GetColumnIndex(this);
                    if (columnIndex >= 0)
                    {
                        platformTable.SetColumnWidth(columnIndex, _width);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the column text alignment.
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
                if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
                {
                    int columnIndex = _parent.GetColumnIndex(this);
                    if (columnIndex >= 0)
                    {
                        platformTable.SetColumnAlignment(columnIndex, _alignment);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the column is resizable by the user.
    /// </summary>
    public bool Resizable
    {
        get
        {
            CheckWidget();
            return _resizable;
        }
        set
        {
            CheckWidget();
            if (_resizable != value)
            {
                _resizable = value;
                if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
                {
                    int columnIndex = _parent.GetColumnIndex(this);
                    if (columnIndex >= 0)
                    {
                        platformTable.SetColumnResizable(columnIndex, _resizable);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the column can be moved by the user.
    /// </summary>
    public bool Moveable
    {
        get
        {
            CheckWidget();
            return _moveable;
        }
        set
        {
            CheckWidget();
            if (_moveable != value)
            {
                _moveable = value;
                if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
                {
                    int columnIndex = _parent.GetColumnIndex(this);
                    if (columnIndex >= 0)
                    {
                        platformTable.SetColumnMoveable(columnIndex, _moveable);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the tooltip text for this column.
    /// </summary>
    public string? ToolTipText
    {
        get
        {
            CheckWidget();
            return _toolTipText;
        }
        set
        {
            CheckWidget();
            if (_toolTipText != value)
            {
                _toolTipText = value;
                if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
                {
                    int columnIndex = _parent.GetColumnIndex(this);
                    if (columnIndex >= 0)
                    {
                        platformTable.SetColumnToolTip(columnIndex, _toolTipText);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Creates a new table column at the end of the column list.
    /// </summary>
    /// <param name="parent">Parent table</param>
    /// <param name="style">Style flags (SWT.LEFT, SWT.RIGHT, SWT.CENTER)</param>
    public TableColumn(Table parent, int style) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        _alignment = ExtractAlignment(style);

        CreateWidget();
        _parent.AddColumn(this);
    }

    /// <summary>
    /// Creates a new table column at the specified index.
    /// </summary>
    /// <param name="parent">Parent table</param>
    /// <param name="style">Style flags (SWT.LEFT, SWT.RIGHT, SWT.CENTER)</param>
    /// <param name="index">Index at which to insert the column</param>
    public TableColumn(Table parent, int style, int index) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        _alignment = ExtractAlignment(style);

        CreateWidget(index);
        _parent.AddColumn(this, index);
    }

    /// <summary>
    /// Sets the column width.
    /// </summary>
    /// <param name="width">Width in pixels</param>
    public void SetWidth(int width)
    {
        Width = width;
    }

    /// <summary>
    /// Sets the column header text.
    /// </summary>
    /// <param name="text">Header text</param>
    public void SetText(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Sets the column alignment.
    /// </summary>
    /// <param name="alignment">Alignment (SWT.LEFT, SWT.RIGHT, SWT.CENTER)</param>
    public void SetAlignment(int alignment)
    {
        Alignment = alignment;
    }

    /// <summary>
    /// Sets whether the column is resizable.
    /// </summary>
    /// <param name="resizable">True if resizable</param>
    public void SetResizable(bool resizable)
    {
        Resizable = resizable;
    }

    /// <summary>
    /// Sets whether the column is moveable.
    /// </summary>
    /// <param name="moveable">True if moveable</param>
    public void SetMoveable(bool moveable)
    {
        Moveable = moveable;
    }

    /// <summary>
    /// Sets the tooltip text.
    /// </summary>
    /// <param name="toolTipText">Tooltip text</param>
    public void SetToolTipText(string? toolTipText)
    {
        ToolTipText = toolTipText;
    }

    /// <summary>
    /// Automatically sizes the column to fit its content.
    /// </summary>
    public void Pack()
    {
        CheckWidget();
        if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
        {
            int columnIndex = _parent.GetColumnIndex(this);
            if (columnIndex >= 0)
            {
                _width = platformTable.PackColumn(columnIndex);
            }
        }
    }

    private void CreateWidget(int index = -1)
    {
        // TableColumn is a logical component that operates through the parent Table's platform widget.
        // Column creation in the platform layer is handled via IPlatformTable.AddColumn which is called
        // when the column is added to the table. We store the index for property updates.
        if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
        {
            // Add column to platform layer
            platformTable.AddColumn(_text, _width, _alignment, index);
        }
    }

    private int ExtractAlignment(int style)
    {
        if ((style & SWT.RIGHT) != 0)
        {
            return SWT.RIGHT;
        }
        if ((style & SWT.CENTER) != 0)
        {
            return SWT.CENTER;
        }
        return SWT.LEFT;
    }

    protected override void ReleaseWidget()
    {
        if (_parent != null)
        {
            // Remove column from platform layer before removing from parent's list
            if (_parent.PlatformWidget is Platform.IPlatformTable platformTable)
            {
                int columnIndex = _parent.GetColumnIndex(this);
                if (columnIndex >= 0)
                {
                    platformTable.RemoveColumn(columnIndex);
                }
            }
            _parent.RemoveColumn(this);
        }

        base.ReleaseWidget();
    }
}
