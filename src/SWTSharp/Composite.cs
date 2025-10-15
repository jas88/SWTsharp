using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Base class for all container widgets that can have child controls.
/// Composites provide layout management and child widget organization.
/// </summary>
public class Composite : Control
{
    private readonly List<Control> _children = new();
    private LayoutManager? _layout;
    private Control[]? _tabList;
    private bool _layoutDeferred;
    private int _backgroundMode = SWT.INHERIT_NONE;

    /// <summary>
    /// Gets the list of child controls in drawing order.
    /// </summary>
    public Control[] Children
    {
        get
        {
            CheckWidget();
            lock (_children)
            {
                return _children.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets or sets the layout manager for this composite.
    /// The layout manager controls automatic positioning and sizing of children.
    /// </summary>
    public LayoutManager? LayoutManager
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
        }
    }

    /// <summary>
    /// Gets or sets the keyboard navigation order for child controls.
    /// If null, uses default tab order based on child creation order.
    /// </summary>
    public Control[]? TabList
    {
        get
        {
            CheckWidget();
            return _tabList;
        }
        set
        {
            CheckWidget();
            _tabList = value;
        }
    }

    /// <summary>
    /// Gets or sets whether layout is currently deferred.
    /// When true, layout operations are suspended until set to false.
    /// </summary>
    public bool IsLayoutDeferred
    {
        get
        {
            CheckWidget();
            return _layoutDeferred;
        }
        set
        {
            CheckWidget();
            _layoutDeferred = value;
        }
    }

    /// <summary>
    /// Gets or sets the background drawing mode for children.
    /// Controls how child widgets inherit background colors/images.
    /// </summary>
    public int BackgroundMode
    {
        get
        {
            CheckWidget();
            return _backgroundMode;
        }
        set
        {
            CheckWidget();
            _backgroundMode = value;
        }
    }

    /// <summary>
    /// Creates a new composite with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite (cannot be null).</param>
    /// <param name="style">The widget style bits.</param>
    public Composite(Composite parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    /// <summary>
    /// Protected constructor for top-level composites (like Shell) that don't have a parent.
    /// </summary>
    /// <param name="style">The widget style bits.</param>
    protected Composite(int style) : base(null!, style)
    {
        // Top-level composites call CreateWidget() in their own constructors
    }

    /// <summary>
    /// Creates the platform-specific container widget.
    /// </summary>
    protected virtual void CreateWidget()
    {
        // Use platform widget
        PlatformWidget = Platform.PlatformFactory.Instance.CreateCompositeWidget(
            Parent?.PlatformWidget,
            Style
        );
    }

    /// <summary>
    /// Adds a child control to this composite.
    /// This method is called internally when a control is created with this composite as parent.
    /// </summary>
    /// <param name="control">The control to add.</param>
    internal void AddChild(Control control)
    {
        if (control == null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        lock (_children)
        {
            if (!_children.Contains(control))
            {
                _children.Add(control);

                // NEW: Add child to platform widget as well
                if (PlatformWidget is IPlatformComposite composite && control.PlatformWidget != null)
                {
                    composite.AddChild(control.PlatformWidget);
                }

                OnChildAdded(control);
            }
        }
    }

    /// <summary>
    /// Removes a child control from this composite.
    /// This method is called internally when a control is disposed.
    /// </summary>
    /// <param name="control">The control to remove.</param>
    internal void RemoveChild(Control control)
    {
        if (control == null)
        {
            return;
        }

        lock (_children)
        {
            if (_children.Remove(control))
            {
                // NEW: Remove child from platform widget as well
                if (PlatformWidget is IPlatformComposite composite && control.PlatformWidget != null)
                {
                    composite.RemoveChild(control.PlatformWidget);
                }

                OnChildRemoved(control);
            }
        }
    }

    /// <summary>
    /// Called when a child control is added to this composite.
    /// Override to perform custom actions when children are added.
    /// </summary>
    /// <param name="control">The control that was added.</param>
    protected virtual void OnChildAdded(Control control)
    {
        // Request layout when a child is added
        RequestLayout();
    }

    /// <summary>
    /// Called when a child control is removed from this composite.
    /// Override to perform custom actions when children are removed.
    /// </summary>
    /// <param name="control">The control that was removed.</param>
    protected virtual void OnChildRemoved(Control control)
    {
        // Request layout when a child is removed
        RequestLayout();
    }

    /// <summary>
    /// Triggers layout computation for child controls.
    /// Uses the assigned layout manager to position and size children.
    /// </summary>
    public void Layout()
    {
        Layout(false);
    }

    /// <summary>
    /// Triggers layout computation with optional cache control.
    /// </summary>
    /// <param name="changed">
    /// If true, flushes the layout cache and forces recomputation.
    /// If false, may use cached layout information.
    /// </param>
    public void Layout(bool changed)
    {
        Layout(changed, false);
    }

    /// <summary>
    /// Triggers layout computation with full control over caching and cascading.
    /// </summary>
    /// <param name="changed">If true, flushes the layout cache.</param>
    /// <param name="all">If true, cascades layout to all descendant composites.</param>
    public void Layout(bool changed, bool all)
    {
        CheckWidget();

        if (_layoutDeferred)
        {
            return;
        }

        if (_layout != null)
        {
            _layout.PerformLayout(this, changed);
        }

        if (all)
        {
            lock (_children)
            {
                foreach (var child in _children)
                {
                    if (child is Composite composite)
                    {
                        composite.Layout(changed, all);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Triggers layout for specific child controls.
    /// </summary>
    /// <param name="changed">Array of controls that have changed and need relayout.</param>
    public void Layout(Control[] changed)
    {
        Layout(changed, SWT.NONE);
    }

    /// <summary>
    /// Triggers layout with advanced control over which children are relaid out.
    /// </summary>
    /// <param name="changed">Array of controls that have changed.</param>
    /// <param name="flags">Layout flags controlling behavior (SWT.CHANGED, SWT.ALL, etc.).</param>
    public void Layout(Control[] changed, int flags)
    {
        CheckWidget();

        if (_layoutDeferred)
        {
            return;
        }

        bool flushCache = (flags & SWT.CHANGED) != 0;
        bool cascadeAll = (flags & SWT.ALL) != 0;

        Layout(flushCache, cascadeAll);
    }

    /// <summary>
    /// Requests that this composite be laid out by its parent's layout manager.
    /// This method should be called when a child's size or position needs to change.
    /// </summary>
    protected void RequestLayout()
    {
        if (Parent is Composite parent)
        {
            parent.Layout(false);
        }
    }

    /// <summary>
    /// Computes the preferred size of this composite based on its children and layout.
    /// </summary>
    /// <param name="wHint">Width hint (-1 for no hint).</param>
    /// <param name="hHint">Height hint (-1 for no hint).</param>
    /// <returns>The preferred size as (width, height).</returns>
    public virtual (int Width, int Height) ComputeSize(int wHint, int hHint)
    {
        CheckWidget();

        if (_layout != null)
        {
            return _layout.ComputeSize(this, wHint, hHint);
        }

        // Default: compute based on children
        int width = 0;
        int height = 0;

        lock (_children)
        {
            foreach (var child in _children)
            {
                if (child.Visible)
                {
                    var bounds = child.GetBounds();
                    width = Math.Max(width, bounds.X + bounds.Width);
                    height = Math.Max(height, bounds.Y + bounds.Height);
                }
            }
        }

        return (width, height);
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

        // Clear layout
        _layout = null;
        _tabList = null;

        // Destroy platform composite widget
        // TODO: Implement proper disposal through platform widget interface
        // Platform widget cleanup is handled by parent disposal

        base.ReleaseWidget();
    }
}

/// <summary>
/// Abstract base class for layout managers.
/// Layout managers control the automatic positioning and sizing of child controls in a composite.
/// </summary>
public abstract class LayoutManager
{
    /// <summary>
    /// Computes the preferred size of the composite based on its children.
    /// </summary>
    /// <param name="composite">The composite to compute size for.</param>
    /// <param name="wHint">Width hint (-1 for no hint).</param>
    /// <param name="hHint">Height hint (-1 for no hint).</param>
    /// <returns>The preferred size as (width, height).</returns>
    public abstract (int Width, int Height) ComputeSize(Composite composite, int wHint, int hHint);

    /// <summary>
    /// Positions and sizes the child controls of the composite.
    /// </summary>
    /// <param name="composite">The composite to lay out.</param>
    /// <param name="changed">If true, layout cache should be flushed.</param>
    public abstract void PerformLayout(Composite composite, bool changed);
}
