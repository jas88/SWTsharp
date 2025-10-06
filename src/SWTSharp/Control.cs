namespace SWTSharp;

/// <summary>
/// Base class for all user interface controls.
/// Controls are widgets that can have a parent and can receive user input.
/// </summary>
public abstract class Control : Widget
{
    private bool _visible = true;
    private bool _enabled = true;
    private int _x, _y, _width, _height;
    private Control? _parent;
    private IntPtr _handle;
    private object? _layoutData;

    /// <summary>
    /// Gets the parent control.
    /// </summary>
    public Control? Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the platform-specific window handle.
    /// </summary>
    public override IntPtr Handle
    {
        get => _handle;
        protected set => _handle = value;
    }

    /// <summary>
    /// Gets or sets whether the control is visible.
    /// </summary>
    public bool Visible
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
                UpdateVisible();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the control is enabled.
    /// </summary>
    public bool Enabled
    {
        get
        {
            CheckWidget();
            return _enabled;
        }
        set
        {
            CheckWidget();
            if (_enabled != value)
            {
                _enabled = value;
                UpdateEnabled();
            }
        }
    }

    /// <summary>
    /// Creates a new control.
    /// </summary>
    protected Control(Control? parent, int style) : base(parent, style)
    {
        _parent = parent;
        if (parent is Composite composite)
        {
            composite.AddChild(this);
        }
    }

    /// <summary>
    /// Sets the control's bounds (location and size).
    /// </summary>
    public void SetBounds(int x, int y, int width, int height)
    {
        CheckWidget();
        _x = x;
        _y = y;
        _width = width;
        _height = height;
        UpdateBounds();
    }

    /// <summary>
    /// Sets the control's size.
    /// </summary>
    public void SetSize(int width, int height)
    {
        CheckWidget();
        _width = width;
        _height = height;
        UpdateBounds();
    }

    /// <summary>
    /// Sets the control's location.
    /// </summary>
    public void SetLocation(int x, int y)
    {
        CheckWidget();
        _x = x;
        _y = y;
        UpdateBounds();
    }

    /// <summary>
    /// Gets the control's bounds.
    /// </summary>
    public (int X, int Y, int Width, int Height) GetBounds()
    {
        CheckWidget();
        return (_x, _y, _width, _height);
    }

    /// <summary>
    /// Gets or sets the layout data associated with this control.
    /// The layout data provides hints to the parent's layout manager.
    /// </summary>
    public object? GetLayoutData()
    {
        CheckWidget();
        return _layoutData;
    }

    /// <summary>
    /// Sets the layout data for this control.
    /// </summary>
    public void SetLayoutData(object? layoutData)
    {
        CheckWidget();
        _layoutData = layoutData;

        // Trigger parent layout recalculation
        if (_parent is SWTSharp.Layout.Composite composite && composite.Layout != null)
        {
            composite.DoLayout(true); // flushCache = true
        }
    }

    /// <summary>
    /// Forces the control to redraw.
    /// </summary>
    public virtual void Redraw()
    {
        CheckWidget();
        // Platform-specific redraw
    }

    /// <summary>
    /// Updates the control's visibility.
    /// </summary>
    protected virtual void UpdateVisible()
    {
        // Platform-specific visibility update
    }

    /// <summary>
    /// Updates the control's enabled state.
    /// </summary>
    protected virtual void UpdateEnabled()
    {
        // Platform-specific enabled update
    }

    /// <summary>
    /// Updates the control's bounds.
    /// </summary>
    protected virtual void UpdateBounds()
    {
        // Platform-specific bounds update
    }

    protected override void ReleaseWidget()
    {
        if (_parent is Composite composite)
        {
            composite.RemoveChild(this);
        }
        _parent = null;
        base.ReleaseWidget();
    }
}
