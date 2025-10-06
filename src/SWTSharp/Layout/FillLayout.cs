namespace SWTSharp.Layout;

/// <summary>
/// FillLayout is the simplest layout manager.
/// It lays out controls in a single row or column, forcing them to be the same size.
/// </summary>
public class FillLayout : Layout
{
    /// <summary>
    /// Type specifies how controls will be positioned within the layout.
    /// Possible values are SWT.HORIZONTAL (default) and SWT.VERTICAL.
    /// </summary>
    public int Type { get; set; } = SWT.HORIZONTAL;

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

    /// <summary>
    /// Spacing specifies the number of pixels between the edge of one control
    /// and the edge of its neighboring control.
    /// </summary>
    public int Spacing { get; set; } = 0;

    /// <summary>
    /// Creates a new FillLayout with horizontal orientation.
    /// </summary>
    public FillLayout()
    {
    }

    /// <summary>
    /// Creates a new FillLayout with the specified orientation.
    /// </summary>
    /// <param name="type">SWT.HORIZONTAL or SWT.VERTICAL</param>
    public FillLayout(int type)
    {
        Type = type;
    }

    protected internal override Point ComputeSize(Composite composite, int wHint, int hHint, bool flushCache)
    {
        var children = composite.GetChildren();
        int count = children.Length;

        if (count == 0)
        {
            return new Point(
                2 * MarginWidth,
                2 * MarginHeight
            );
        }

        int maxWidth = 0;
        int maxHeight = 0;

        // Find maximum preferred size among children
        foreach (var child in children)
        {
            if (!child.Visible) continue;

            var size = GetChildSize(child, wHint, hHint);
            maxWidth = Math.Max(maxWidth, size.X);
            maxHeight = Math.Max(maxHeight, size.Y);
        }

        int visibleCount = children.Count(c => c.Visible);
        if (visibleCount == 0)
        {
            return new Point(2 * MarginWidth, 2 * MarginHeight);
        }

        int width, height;

        if (Type == SWT.HORIZONTAL)
        {
            // Horizontal: sum widths, take max height
            width = maxWidth * visibleCount + Spacing * (visibleCount - 1) + 2 * MarginWidth;
            height = maxHeight + 2 * MarginHeight;
        }
        else
        {
            // Vertical: take max width, sum heights
            width = maxWidth + 2 * MarginWidth;
            height = maxHeight * visibleCount + Spacing * (visibleCount - 1) + 2 * MarginHeight;
        }

        return new Point(width, height);
    }

    protected internal override bool DoLayout(Composite composite, bool flushCache)
    {
        var children = composite.GetChildren();
        var clientArea = composite.GetClientArea();

        if (children.Length == 0) return true;

        int visibleCount = children.Count(c => c.Visible);
        if (visibleCount == 0) return true;

        int availableWidth = clientArea.Width - 2 * MarginWidth;
        int availableHeight = clientArea.Height - 2 * MarginHeight;

        if (Type == SWT.HORIZONTAL)
        {
            // Horizontal layout
            int childWidth = (availableWidth - Spacing * (visibleCount - 1)) / visibleCount;
            int childHeight = availableHeight;

            int x = clientArea.X + MarginWidth;
            int y = clientArea.Y + MarginHeight;

            foreach (var child in children)
            {
                if (!child.Visible) continue;

                child.SetBounds(x, y, childWidth, childHeight);
                x += childWidth + Spacing;
            }
        }
        else
        {
            // Vertical layout
            int childWidth = availableWidth;
            int childHeight = (availableHeight - Spacing * (visibleCount - 1)) / visibleCount;

            int x = clientArea.X + MarginWidth;
            int y = clientArea.Y + MarginHeight;

            foreach (var child in children)
            {
                if (!child.Visible) continue;

                child.SetBounds(x, y, childWidth, childHeight);
                y += childHeight + Spacing;
            }
        }

        return true;
    }

    private Point GetChildSize(Control child, int wHint, int hHint)
    {
        // For simple controls, return a default size
        // More sophisticated implementations would query the control's preferred size
        if (child is Composite composite)
        {
            return composite.ComputeSize(wHint, hHint, true);
        }

        // Default size for non-composite controls
        return new Point(64, 24);
    }
}
