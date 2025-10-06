namespace SWTSharp.Layout;

/// <summary>
/// StackLayout displays a single control at a time (card-style layout).
/// The control to display is determined by the TopControl property.
/// All other controls are hidden when the layout is applied.
/// </summary>
public class StackLayout : Layout
{
    /// <summary>
    /// MarginWidth specifies the number of pixels of horizontal margin
    /// that will be placed along the left and right edges of the layout.
    /// </summary>
    public int MarginWidth { get; set; } = 0;

    /// <summary>
    /// MarginHeight specifies the number of pixels of vertical margin
    /// that will be placed along the top and bottom edges of the layout.
    /// </summary>
    public int MarginHeight { get; set; } = 0;

    private Control? _topControl;

    /// <summary>
    /// Gets or sets the control to display. All other controls are hidden.
    /// When set, triggers a layout recalculation.
    /// </summary>
    public Control? TopControl
    {
        get => _topControl;
        set
        {
            if (_topControl != value)
            {
                _topControl = value;
                // Layout will be triggered by the composite when TopControl changes
            }
        }
    }

    /// <summary>
    /// Creates a new StackLayout.
    /// </summary>
    public StackLayout()
    {
    }

    protected internal override Point ComputeSize(Composite composite, int wHint, int hHint, bool flushCache)
    {
        var children = composite.GetChildren();

        if (children.Length == 0 || TopControl == null)
        {
            return new Point(
                2 * MarginWidth,
                2 * MarginHeight
            );
        }

        // Only compute size for the top control
        if (!children.Contains(TopControl))
        {
            // TopControl not a child of this composite
            return new Point(2 * MarginWidth, 2 * MarginHeight);
        }

        var size = GetControlSize(TopControl, wHint, hHint);

        return new Point(
            size.X + 2 * MarginWidth,
            size.Y + 2 * MarginHeight
        );
    }

    protected internal override bool DoLayout(Composite composite, bool flushCache)
    {
        var children = composite.GetChildren();
        if (children.Length == 0) return true;

        var clientArea = composite.GetClientArea();

        int x = clientArea.X + MarginWidth;
        int y = clientArea.Y + MarginHeight;
        int width = clientArea.Width - 2 * MarginWidth;
        int height = clientArea.Height - 2 * MarginHeight;

        // Ensure non-negative dimensions
        width = Math.Max(0, width);
        height = Math.Max(0, height);

        // Hide all controls except TopControl
        foreach (var child in children)
        {
            if (child == TopControl && child.Visible)
            {
                // Position and size the top control to fill the client area
                child.SetBounds(x, y, width, height);
            }
            else
            {
                // Hide other controls by positioning them off-screen
                // (Some platforms don't support visibility toggling, so we move them)
                child.SetBounds(-1, -1, 0, 0);
            }
        }

        return true;
    }

    private Point GetControlSize(Control control, int wHint, int hHint)
    {
        if (control is Composite composite)
        {
            return composite.ComputeSize(wHint, hHint, true);
        }

        // Default size for non-composite controls
        return new Point(64, 24);
    }
}
