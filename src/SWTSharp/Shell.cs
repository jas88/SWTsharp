namespace SWTSharp;

/// <summary>
/// Represents a top-level window (shell) in the UI.
/// Shells are containers for other widgets and represent windows on the screen.
/// </summary>
public class Shell : Composite
{
    private string _text = string.Empty;
    private bool _visible;
    private bool _active;

    /// <summary>
    /// Gets or sets the shell's title text.
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
    /// Gets or sets whether the shell is visible.
    /// </summary>
    public new bool Visible
    {
        get
        {
            CheckWidget();
            return _visible;
        }
        set
        {
            CheckWidget();
            if (_visible != value)
            {
                _visible = value;
                UpdateVisibility();
            }
        }
    }

    /// <summary>
    /// Gets whether this shell is the active shell.
    /// </summary>
    public bool IsActive
    {
        get
        {
            CheckWidget();
            return _active;
        }
    }

    /// <summary>
    /// Creates a new shell using the default display.
    /// </summary>
    public Shell() : this(Display.Default, SWT.SHELL_TRIM)
    {
    }

    /// <summary>
    /// Creates a new shell with the specified style.
    /// </summary>
    public Shell(int style) : this(Display.Default, style)
    {
    }

    /// <summary>
    /// Creates a new shell on the specified display.
    /// </summary>
    public Shell(Display display) : this(display, SWT.SHELL_TRIM)
    {
    }

    /// <summary>
    /// Creates a new shell on the specified display with the specified style.
    /// </summary>
    public Shell(Display display, int style) : base(CheckStyle(style))
    {
        if (display == null)
        {
            throw new ArgumentNullException(nameof(display));
        }

        SetDisplay(display);
        display.AddShell(this);
        CreateWidget();
    }

    /// <summary>
    /// Creates a shell as a child of another shell (dialog).
    /// </summary>
    public Shell(Shell parent) : this(parent, SWT.DIALOG_TRIM)
    {
    }

    /// <summary>
    /// Creates a shell as a child of another shell with the specified style.
    /// </summary>
    public Shell(Shell parent, int style) : base(parent, CheckStyle(style))
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        parent.Display?.AddShell(this);
        CreateWidget();
    }

    private static int CheckStyle(int style)
    {
        // Ensure at least one style bit is set
        if (style == 0)
        {
            return SWT.SHELL_TRIM;
        }
        return style;
    }

    protected override void CreateWidget()
    {
        // Create platform-specific window handle
        Handle = SWTSharp.Platform.PlatformFactory.Instance.CreateWindow(Style, _text);
    }

    /// <summary>
    /// Opens the shell and makes it visible.
    /// </summary>
    public void Open()
    {
        CheckWidget();
        Visible = true;
        _active = true;
    }

    /// <summary>
    /// Closes the shell.
    /// </summary>
    public void Close()
    {
        CheckWidget();
        Visible = false;
        _active = false;
    }

    /// <summary>
    /// Sets the shell's size.
    /// </summary>
    public new void SetSize(int width, int height)
    {
        CheckWidget();
        SWTSharp.Platform.PlatformFactory.Instance.SetWindowSize(Handle, width, height);
    }

    /// <summary>
    /// Sets the shell's location.
    /// </summary>
    public new void SetLocation(int x, int y)
    {
        CheckWidget();
        SWTSharp.Platform.PlatformFactory.Instance.SetWindowLocation(Handle, x, y);
    }

    /// <summary>
    /// Centers the shell on the screen.
    /// </summary>
    public void Center()
    {
        CheckWidget();
        // TODO: Get screen dimensions and calculate center position
        // For now, just use a reasonable default position
        SetLocation(100, 100);
    }

    private void UpdateText()
    {
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetWindowText(Handle, _text);
        }
    }

    private void UpdateVisibility()
    {
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.SetWindowVisible(Handle, _visible);
        }
    }

    protected override void ReleaseWidget()
    {
        // Destroy platform window
        if (Handle != IntPtr.Zero)
        {
            SWTSharp.Platform.PlatformFactory.Instance.DestroyWindow(Handle);
        }

        // Unregister from display
        Display?.RemoveShell(this);

        // Base class (Composite) will handle child disposal
        base.ReleaseWidget();
    }
}

/// <summary>
/// Common shell style combinations.
/// </summary>
public static class ShellStyles
{
    /// <summary>
    /// Shell with title, close, min, max, and resize.
    /// </summary>
    public static int SHELL_TRIM => SWT.CLOSE | SWT.TITLE | SWT.MIN | SWT.MAX | SWT.RESIZE;

    /// <summary>
    /// Shell with title and close button (dialog style).
    /// </summary>
    public static int DIALOG_TRIM => SWT.CLOSE | SWT.TITLE | SWT.BORDER;
}

