namespace SWTSharp.Layout;

/// <summary>
/// A Composite is a control that is capable of containing other controls (children).
/// It serves as the base class for all container widgets.
/// </summary>
public class Composite : Control
{
    private Layout? _layout;
    private readonly List<Control> _children = new();
    private const int DEFAULT_WIDTH = 64;
    private const int DEFAULT_HEIGHT = 64;

    /// <summary>
    /// Gets or sets the layout which manages the size and position of children.
    /// </summary>
    public Layout? Layout
    {
        get
        {
            CheckWidget();
            return _layout;
        }
        set
        {
            CheckWidget();
            _layout = value;
            // Trigger immediate layout
            DoLayout(true);
        }
    }

    /// <summary>
    /// Creates a new Composite.
    /// </summary>
    /// <param name="parent">The parent control.</param>
    /// <param name="style">The style bits.</param>
    public Composite(Control? parent, int style) : base(parent, style)
    {
    }

    /// <summary>
    /// Gets the client area (space available for children).
    /// Excludes borders, scrollbars, etc.
    /// </summary>
    /// <returns>The client area rectangle.</returns>
    public Rectangle GetClientArea()
    {
        CheckWidget();
        var bounds = GetBounds();
        // For now, return the full bounds as client area
        // Platform-specific implementations can override this
        return new Rectangle(0, 0, bounds.Width, bounds.Height);
    }

    /// <summary>
    /// Gets an array of child controls.
    /// </summary>
    /// <returns>Array of child controls.</returns>
    public Control[] GetChildren()
    {
        CheckWidget();
        lock (_children)
        {
            return _children.ToArray(); // Defensive copy
        }
    }

    /// <summary>
    /// Adds a child control to this composite.
    /// </summary>
    internal void AddChild(Control child)
    {
        lock (_children)
        {
            if (!_children.Contains(child))
            {
                _children.Add(child);

                // Trigger layout recalculation
                if (_layout != null)
                {
                    DoLayout(true); // Flush cache
                }
            }
        }
    }

    /// <summary>
    /// Removes a child control from this composite.
    /// </summary>
    internal void RemoveChild(Control child)
    {
        lock (_children)
        {
            if (_children.Remove(child))
            {
                // Trigger layout recalculation
                if (_layout != null)
                {
                    DoLayout(true); // Flush cache
                }
            }
        }
    }

    /// <summary>
    /// Forces layout of children. Called automatically on resize,
    /// or manually when layout data changes.
    /// </summary>
    /// <param name="changed">If true, flush cached layout data.</param>
    public void DoLayout(bool changed)
    {
        DoLayout(changed, false);
    }

    /// <summary>
    /// Forces layout and optionally propagates to all descendants.
    /// </summary>
    /// <param name="changed">If true, flush cached layout data.</param>
    /// <param name="all">If true, recursively layout all descendant composites.</param>
    public void DoLayout(bool changed, bool all)
    {
        CheckWidget();

        if (_layout != null)
        {
            _layout.DoLayout(this, changed);

            if (all)
            {
                // Recursively layout children
                foreach (var child in GetChildren())
                {
                    if (child is Composite composite)
                    {
                        composite.DoLayout(changed, all);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Computes the preferred size of the composite based on layout.
    /// </summary>
    /// <param name="wHint">Width hint or SWT.DEFAULT.</param>
    /// <param name="hHint">Height hint or SWT.DEFAULT.</param>
    /// <param name="changed">If true, flush cached size information.</param>
    /// <returns>The preferred size.</returns>
    public Point ComputeSize(int wHint, int hHint, bool changed)
    {
        CheckWidget();

        if (_layout != null)
        {
            Point size = _layout.ComputeSize(this, wHint, hHint, changed);
            if (size.X == 0) size.X = DEFAULT_WIDTH;
            if (size.Y == 0) size.Y = DEFAULT_HEIGHT;
            return size;
        }

        // No layout: return default size
        return new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
    }

    /// <summary>
    /// Computes the preferred size with default hints.
    /// </summary>
    public Point ComputeSize()
    {
        return ComputeSize(SWT.DEFAULT, SWT.DEFAULT, true);
    }

    protected override void ReleaseWidget()
    {
        // Dispose all children
        lock (_children)
        {
            foreach (var child in _children.ToArray())
            {
                if (child is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _children.Clear();
        }

        _layout = null;
        base.ReleaseWidget();
    }
}
