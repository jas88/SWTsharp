using SWTSharp.Graphics;
using SWTSharp.Platform;
using SWTSharp.Platform.MacOS;

namespace SWTSharp;

/// <summary>
/// Represents a row in a Table widget.
/// Each item can have text and images for each column, as well as custom colors and fonts.
/// </summary>
public class TableItem : Widget
{
    private readonly Table _parent;
    private string[] _texts;
    private Image?[] _images;
    private bool _checked;
    private Color? _background;
    private Color? _foreground;
    private Font? _font;
    private IPlatformTableItem? _platformTableItem;

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
    /// Gets or sets the text for the first column.
    /// </summary>
    public string Text
    {
        get
        {
            CheckWidget();
            return GetText(0);
        }
        set
        {
            CheckWidget();
            SetText(0, value);
        }
    }

    /// <summary>
    /// Gets or sets the image for the first column.
    /// </summary>
    public Image? Image
    {
        get
        {
            CheckWidget();
            return GetImage(0);
        }
        set
        {
            CheckWidget();
            SetImage(0, value);
        }
    }

    /// <summary>
    /// Gets or sets the checked state (only for tables with SWT.CHECK style).
    /// </summary>
    public bool Checked
    {
        get
        {
            CheckWidget();
            return _checked;
        }
        set
        {
            CheckWidget();
            if (_checked != value)
            {
                _checked = value;
                // Checked state handled by platform widget through IPlatformTableItem interface
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color for this item.
    /// </summary>
    public Color? Background
    {
        get
        {
            CheckWidget();
            return _background;
        }
        set
        {
            CheckWidget();
            if (_background != value)
            {
                _background = value;
                // Background color handled by platform widget through IPlatformTableItem interface
            }
        }
    }

    /// <summary>
    /// Gets or sets the foreground (text) color for this item.
    /// </summary>
    public Color? Foreground
    {
        get
        {
            CheckWidget();
            return _foreground;
        }
        set
        {
            CheckWidget();
            if (_foreground != value)
            {
                _foreground = value;
                // Foreground color handled by platform widget through IPlatformTableItem interface
            }
        }
    }

    /// <summary>
    /// Gets or sets the font for this item.
    /// </summary>
    public Font? Font
    {
        get
        {
            CheckWidget();
            return _font;
        }
        set
        {
            CheckWidget();
            if (_font != value)
            {
                _font = value;
                // Font handled by platform widget through IPlatformTableItem interface
            }
        }
    }

    /// <summary>
    /// Creates a new table item at the end of the table.
    /// </summary>
    /// <param name="parent">Parent table</param>
    /// <param name="style">Style flags</param>
    public TableItem(Table parent, int style) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        _texts = new string[Math.Max(1, parent.ColumnCount)];
        _images = new Image[Math.Max(1, parent.ColumnCount)];

        // Initialize text array
        for (int i = 0; i < _texts.Length; i++)
        {
            _texts[i] = string.Empty;
        }

        CreateWidget();
        _parent.AddItem(this);
    }

    /// <summary>
    /// Creates a new table item at the specified index.
    /// </summary>
    /// <param name="parent">Parent table</param>
    /// <param name="style">Style flags</param>
    /// <param name="index">Index at which to insert the item</param>
    public TableItem(Table parent, int style, int index) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        _texts = new string[Math.Max(1, parent.ColumnCount)];
        _images = new Image[Math.Max(1, parent.ColumnCount)];

        // Initialize text array
        for (int i = 0; i < _texts.Length; i++)
        {
            _texts[i] = string.Empty;
        }

        CreateWidget(index);
        _parent.AddItem(this, index);
    }

    /// <summary>
    /// Gets the text for the specified column.
    /// </summary>
    /// <param name="column">Column index</param>
    /// <returns>The text</returns>
    public string GetText(int column)
    {
        CheckWidget();
        if (column < 0 || column >= _texts.Length)
        {
            return string.Empty;
        }
        return _texts[column] ?? string.Empty;
    }

    /// <summary>
    /// Sets the text for the specified column.
    /// </summary>
    /// <param name="column">Column index</param>
    /// <param name="text">Text to set</param>
    public void SetText(int column, string text)
    {
        CheckWidget();
        if (column < 0)
        {
            return;
        }

        // Expand arrays if necessary
        if (column >= _texts.Length)
        {
            Array.Resize(ref _texts, column + 1);
            Array.Resize(ref _images, column + 1);
        }

        _texts[column] = text ?? string.Empty;

        // Use platform widget
        if (_platformTableItem != null)
        {
            _platformTableItem.SetText(column, _texts[column]);
        }
    }

    /// <summary>
    /// Sets the text for all columns.
    /// </summary>
    /// <param name="texts">Array of text values</param>
    public void SetText(string[] texts)
    {
        CheckWidget();
        if (texts == null)
        {
            throw new ArgumentNullException(nameof(texts));
        }

        for (int i = 0; i < texts.Length; i++)
        {
            SetText(i, texts[i]);
        }
    }

    /// <summary>
    /// Gets the image for the specified column.
    /// </summary>
    /// <param name="column">Column index</param>
    /// <returns>The image, or null</returns>
    public Image? GetImage(int column)
    {
        CheckWidget();
        if (column < 0 || column >= _images.Length)
        {
            return null;
        }
        return _images[column];
    }

    /// <summary>
    /// Sets the image for the specified column.
    /// </summary>
    /// <param name="column">Column index</param>
    /// <param name="image">Image to set</param>
    public void SetImage(int column, Image? image)
    {
        CheckWidget();
        if (column < 0)
        {
            return;
        }

        // Expand arrays if necessary
        if (column >= _images.Length)
        {
            Array.Resize(ref _texts, column + 1);
            Array.Resize(ref _images, column + 1);
        }

        _images[column] = image;

        // Image handling through platform widget interface
        if (_platformTableItem != null)
        {
            if (image != null)
            {
                var imageAdapter = new MacOSImage(image);
                _platformTableItem.SetImage(column, imageAdapter);
            }
            else
            {
                _platformTableItem.SetImage(column, null);
            }
        }
    }

    /// <summary>
    /// Sets the image for all columns.
    /// </summary>
    /// <param name="images">Array of images</param>
    public void SetImage(Image?[] images)
    {
        CheckWidget();
        if (images == null)
        {
            throw new ArgumentNullException(nameof(images));
        }

        for (int i = 0; i < images.Length; i++)
        {
            SetImage(i, images[i]);
        }
    }

    /// <summary>
    /// Sets the checked state.
    /// </summary>
    /// <param name="checked">True if checked</param>
    public void SetChecked(bool @checked)
    {
        Checked = @checked;
    }

    /// <summary>
    /// Sets the background color.
    /// </summary>
    /// <param name="color">Background color</param>
    public void SetBackground(Color? color)
    {
        Background = color;
    }

    /// <summary>
    /// Sets the foreground color.
    /// </summary>
    /// <param name="color">Foreground color</param>
    public void SetForeground(Color? color)
    {
        Foreground = color;
    }

    /// <summary>
    /// Sets the font.
    /// </summary>
    /// <param name="font">Font</param>
    public void SetFont(Font? font)
    {
        Font = font;
    }

    private void CreateWidget(int index = -1)
    {
        // Create platform table item if platform factory supports it
        if (Platform.PlatformFactory.Instance is MacOSPlatform macOSPlatform)
        {
            // TODO: Implement proper platform table item creation without pseudo-handles
            // TODO: Create IPlatformTableItem widget here through platform widget interface

            // Create platform adapter (temporary workaround)
            _platformTableItem = new MacOSTableItem(macOSPlatform, IntPtr.Zero);
        }
        else
        {
            // Fallback for other platforms - TODO: Implement direct platform widget creation
            throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Direct platform widget creation not implemented for this platform");
        }

        // Set initial text for all columns
        for (int i = 0; i < _texts.Length; i++)
        {
            if (!string.IsNullOrEmpty(_texts[i]))
            {
                // Use platform widget
                if (_platformTableItem != null)
                {
                    _platformTableItem.SetText(i, _texts[i]);
                }
            }
        }
    }

    protected override void ReleaseWidget()
    {
        if (_parent != null)
        {
            _parent.RemoveItem(this);
        }

        // Dispose platform widget
        if (_platformTableItem != null)
        {
            _platformTableItem.Dispose();
            _platformTableItem = null;
        }

        _background = null;
        _foreground = null;
        _font = null;

        base.ReleaseWidget();
    }
}
