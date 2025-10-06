namespace SWTSharp.Layout;

/// <summary>
/// RowLayout is a layout manager that positions controls in rows or columns,
/// wrapping to the next row or column when space runs out.
/// </summary>
public class RowLayout : Layout
{
    /// <summary>
    /// Type specifies whether the layout places controls in rows or columns.
    /// Values are SWT.HORIZONTAL (default) or SWT.VERTICAL.
    /// </summary>
    public int Type { get; set; } = SWT.HORIZONTAL;

    /// <summary>
    /// MarginWidth specifies the left and right margins.
    /// </summary>
    public int MarginWidth { get; set; } = 3;

    /// <summary>
    /// MarginHeight specifies the top and bottom margins.
    /// </summary>
    public int MarginHeight { get; set; } = 3;

    /// <summary>
    /// MarginLeft specifies the left margin (overrides marginWidth).
    /// </summary>
    public int MarginLeft { get; set; } = 3;

    /// <summary>
    /// MarginTop specifies the top margin (overrides marginHeight).
    /// </summary>
    public int MarginTop { get; set; } = 3;

    /// <summary>
    /// MarginRight specifies the right margin (overrides marginWidth).
    /// </summary>
    public int MarginRight { get; set; } = 3;

    /// <summary>
    /// MarginBottom specifies the bottom margin (overrides marginHeight).
    /// </summary>
    public int MarginBottom { get; set; } = 3;

    /// <summary>
    /// Spacing specifies the gap between controls.
    /// </summary>
    public int Spacing { get; set; } = 3;

    /// <summary>
    /// Wrap specifies whether controls should wrap to the next row/column.
    /// </summary>
    public bool Wrap { get; set; } = true;

    /// <summary>
    /// Pack specifies whether controls should be sized to their preferred size (true)
    /// or made equal size (false).
    /// </summary>
    public bool Pack { get; set; } = true;

    /// <summary>
    /// Fill specifies whether controls should expand to fill available space
    /// perpendicular to the flow direction.
    /// </summary>
    public bool Fill { get; set; } = false;

    /// <summary>
    /// Center specifies whether controls should be centered in the perpendicular direction.
    /// </summary>
    public bool Center { get; set; } = false;

    /// <summary>
    /// Justify specifies whether to expand the last row/column to fill available space.
    /// </summary>
    public bool Justify { get; set; } = false;

    /// <summary>
    /// Creates a new RowLayout with horizontal orientation.
    /// </summary>
    public RowLayout()
    {
    }

    /// <summary>
    /// Creates a new RowLayout with the specified orientation.
    /// </summary>
    public RowLayout(int type)
    {
        Type = type;
    }

    protected internal override Point ComputeSize(Composite composite, int wHint, int hHint, bool flushCache)
    {
        var children = composite.GetChildren();
        if (children.Length == 0)
        {
            return new Point(
                MarginLeft + MarginRight,
                MarginTop + MarginBottom
            );
        }

        int maxWidth = 0;
        int maxHeight = 0;

        if (Type == SWT.HORIZONTAL)
        {
            // Calculate width needed for horizontal flow
            int currentRowWidth = 0;
            int currentRowHeight = 0;
            int totalHeight = 0;

            foreach (var child in children)
            {
                if (!IsVisible(child)) continue;

                var data = GetLayoutData(child);
                if (data.Exclude) continue;

                var size = GetChildSize(child, data);

                if (Wrap && currentRowWidth > 0 &&
                    currentRowWidth + Spacing + size.X > wHint - MarginLeft - MarginRight)
                {
                    // Wrap to next row
                    maxWidth = Math.Max(maxWidth, currentRowWidth);
                    totalHeight += currentRowHeight + Spacing;
                    currentRowWidth = size.X;
                    currentRowHeight = size.Y;
                }
                else
                {
                    if (currentRowWidth > 0) currentRowWidth += Spacing;
                    currentRowWidth += size.X;
                    currentRowHeight = Math.Max(currentRowHeight, size.Y);
                }
            }

            maxWidth = Math.Max(maxWidth, currentRowWidth);
            totalHeight += currentRowHeight;

            return new Point(
                maxWidth + MarginLeft + MarginRight,
                totalHeight + MarginTop + MarginBottom
            );
        }
        else
        {
            // Calculate height needed for vertical flow
            int currentColWidth = 0;
            int currentColHeight = 0;
            int totalWidth = 0;

            foreach (var child in children)
            {
                if (!IsVisible(child)) continue;

                var data = GetLayoutData(child);
                if (data.Exclude) continue;

                var size = GetChildSize(child, data);

                if (Wrap && currentColHeight > 0 &&
                    currentColHeight + Spacing + size.Y > hHint - MarginTop - MarginBottom)
                {
                    // Wrap to next column
                    maxHeight = Math.Max(maxHeight, currentColHeight);
                    totalWidth += currentColWidth + Spacing;
                    currentColHeight = size.Y;
                    currentColWidth = size.X;
                }
                else
                {
                    if (currentColHeight > 0) currentColHeight += Spacing;
                    currentColHeight += size.Y;
                    currentColWidth = Math.Max(currentColWidth, size.X);
                }
            }

            maxHeight = Math.Max(maxHeight, currentColHeight);
            totalWidth += currentColWidth;

            return new Point(
                totalWidth + MarginLeft + MarginRight,
                maxHeight + MarginTop + MarginBottom
            );
        }
    }

    protected internal override bool DoLayout(Composite composite, bool flushCache)
    {
        var children = composite.GetChildren();
        var clientArea = composite.GetClientArea();

        if (children.Length == 0) return true;

        int availableWidth = clientArea.Width - MarginLeft - MarginRight;
        int availableHeight = clientArea.Height - MarginTop - MarginBottom;

        if (Type == SWT.HORIZONTAL)
        {
            LayoutHorizontal(children, clientArea, availableWidth, availableHeight);
        }
        else
        {
            LayoutVertical(children, clientArea, availableWidth, availableHeight);
        }

        return true;
    }

    private void LayoutHorizontal(Control[] children, Rectangle clientArea,
                                  int availableWidth, int availableHeight)
    {
        int x = clientArea.X + MarginLeft;
        int y = clientArea.Y + MarginTop;
        int rowHeight = 0;
        int rowWidth = 0;

        var rowControls = new List<(Control control, Point size)>();

        foreach (var child in children)
        {
            if (!IsVisible(child)) continue;

            var data = GetLayoutData(child);
            if (data.Exclude) continue;

            var size = GetChildSize(child, data);

            // Check if we need to wrap
            if (Wrap && rowWidth > 0 && rowWidth + Spacing + size.X > availableWidth)
            {
                // Layout current row
                LayoutRow(rowControls, x - rowWidth, y, rowWidth, rowHeight);

                // Start new row
                y += rowHeight + Spacing;
                rowWidth = 0;
                rowHeight = 0;
                rowControls.Clear();
            }

            if (rowWidth > 0) rowWidth += Spacing;
            rowWidth += size.X;
            rowHeight = Math.Max(rowHeight, size.Y);
            rowControls.Add((child, size));
        }

        // Layout final row
        if (rowControls.Count > 0)
        {
            LayoutRow(rowControls, x, y, rowWidth, rowHeight);
        }
    }

    private void LayoutRow(List<(Control control, Point size)> rowControls,
                          int startX, int startY, int rowWidth, int rowHeight)
    {
        int x = startX;

        foreach (var (control, size) in rowControls)
        {
            int width = size.X;
            int height = Fill ? rowHeight : size.Y;
            int y = startY;

            if (Center && !Fill)
            {
                y += (rowHeight - height) / 2;
            }

            control.SetBounds(x, y, width, height);
            x += width + Spacing;
        }
    }

    private void LayoutVertical(Control[] children, Rectangle clientArea,
                               int availableWidth, int availableHeight)
    {
        int x = clientArea.X + MarginLeft;
        int y = clientArea.Y + MarginTop;
        int colWidth = 0;
        int colHeight = 0;

        var colControls = new List<(Control control, Point size)>();

        foreach (var child in children)
        {
            if (!IsVisible(child)) continue;

            var data = GetLayoutData(child);
            if (data.Exclude) continue;

            var size = GetChildSize(child, data);

            // Check if we need to wrap
            if (Wrap && colHeight > 0 && colHeight + Spacing + size.Y > availableHeight)
            {
                // Layout current column
                LayoutColumn(colControls, x, y - colHeight, colWidth, colHeight);

                // Start new column
                x += colWidth + Spacing;
                colWidth = 0;
                colHeight = 0;
                colControls.Clear();
            }

            if (colHeight > 0) colHeight += Spacing;
            colHeight += size.Y;
            colWidth = Math.Max(colWidth, size.X);
            colControls.Add((child, size));
        }

        // Layout final column
        if (colControls.Count > 0)
        {
            LayoutColumn(colControls, x, clientArea.Y + MarginTop, colWidth, colHeight);
        }
    }

    private void LayoutColumn(List<(Control control, Point size)> colControls,
                             int startX, int startY, int colWidth, int colHeight)
    {
        int y = startY;

        foreach (var (control, size) in colControls)
        {
            int width = Fill ? colWidth : size.X;
            int height = size.Y;
            int x = startX;

            if (Center && !Fill)
            {
                x += (colWidth - width) / 2;
            }

            control.SetBounds(x, y, width, height);
            y += height + Spacing;
        }
    }

    private bool IsVisible(Control control)
    {
        return control.Visible;
    }

    private RowData GetLayoutData(Control control)
    {
        var data = control.GetLayoutData();
        return data as RowData ?? new RowData();
    }

    private Point GetChildSize(Control child, RowData data)
    {
        int width = data.Width != SWT.DEFAULT ? data.Width : 64;
        int height = data.Height != SWT.DEFAULT ? data.Height : 24;

        if (child is Composite composite)
        {
            var size = composite.ComputeSize(data.Width, data.Height, true);
            width = size.X;
            height = size.Y;
        }

        return new Point(width, height);
    }
}
