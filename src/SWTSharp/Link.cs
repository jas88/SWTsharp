using SWTSharp.Events;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a hyperlink text control with click events.
/// Displays text with HTML-like markup: &lt;a&gt;clickable text&lt;/a&gt;
/// </summary>
public class Link : Control
{
    private string _text = string.Empty;

    /// <summary>
    /// Gets or sets the link text with HTML-like markup.
    /// Use &lt;a&gt;text&lt;/a&gt; or &lt;a href="id"&gt;text&lt;/a&gt; for clickable links.
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
    /// Occurs when a link is clicked.
    /// </summary>
    public event EventHandler<LinkEventArgs>? LinkSelected;

    /// <summary>
    /// Creates a new link control.
    /// </summary>
    /// <param name="parent">The parent composite.</param>
    /// <param name="style">Style bits (BORDER).</param>
    public Link(Composite parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    private void CreateWidget()
    {
        var parentWidget = Parent?.PlatformWidget;
        PlatformWidget = Platform.PlatformFactory.Instance.CreateLinkWidget(parentWidget, Style);

        if (PlatformWidget is IPlatformLink linkWidget)
        {
            linkWidget.SetText(_text);
            linkWidget.LinkClicked += OnPlatformLinkClicked;
        }

        ConnectEventHandlers();
    }

    /// <summary>
    /// Sets the link text.
    /// </summary>
    public void SetText(string text)
    {
        CheckWidget();
        Text = text;
    }

    /// <summary>
    /// Gets the link text.
    /// </summary>
    public string GetText()
    {
        CheckWidget();
        return _text;
    }

    /// <summary>
    /// Gets or sets the bounds of the link control.
    /// </summary>
    public Graphics.Rectangle Bounds
    {
        get
        {
            CheckWidget();
            var bounds = GetBounds();
            return new Graphics.Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }
        set
        {
            CheckWidget();
            SetBounds(value.X, value.Y, value.Width, value.Height);
        }
    }

    /// <summary>
    /// Gets or sets the size of the link control.
    /// </summary>
    public Graphics.Point Size
    {
        get
        {
            CheckWidget();
            var bounds = GetBounds();
            return new Graphics.Point(bounds.Width, bounds.Height);
        }
        set
        {
            CheckWidget();
            SetSize(value.Width, value.Height);
        }
    }

    private void UpdateText()
    {
        if (PlatformWidget is IPlatformLink linkWidget)
        {
            linkWidget.SetText(_text);
        }
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

    private void OnPlatformLinkClicked(object? sender, string linkId)
    {
        CheckWidget();

        var linkEvent = new LinkEventArgs
        {
            Text = linkId
        };
        LinkSelected?.Invoke(this, linkEvent);

        var selectionEvent = new Event
        {
            Detail = SWT.NONE,
            Text = linkId,
            Time = Environment.TickCount
        };
        NotifyListeners(SWT.Selection, selectionEvent);
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
        if (PlatformWidget is IPlatformLink linkWidget)
        {
            linkWidget.LinkClicked -= OnPlatformLinkClicked;
        }

        if (PlatformWidget is IPlatformEventHandling eventHandling)
        {
            eventHandling.FocusGained -= OnPlatformFocusGained;
            eventHandling.FocusLost -= OnPlatformFocusLost;
            eventHandling.KeyDown -= OnPlatformKeyDown;
            eventHandling.KeyUp -= OnPlatformKeyUp;
        }

        LinkSelected = null;
        base.ReleaseWidget();
    }
}

/// <summary>
/// Event arguments for link selection events.
/// </summary>
public class LinkEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the link text or identifier.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
