namespace SWTSharp.Layout;

/// <summary>
/// GridLayout is the most powerful and commonly used layout manager.
/// It arranges controls in a flexible grid with support for spanning, alignment, and space grabbing.
/// </summary>
public class GridLayout : Layout
{
    /// <summary>
    /// Number of columns in the grid.
    /// </summary>
    public int NumColumns { get; set; } = 1;

    /// <summary>
    /// Whether to force all columns to be equal width.
    /// </summary>
    public bool MakeColumnsEqualWidth { get; set; } = false;

    /// <summary>
    /// Left and right margins (if marginLeft/Right not set).
    /// </summary>
    public int MarginWidth { get; set; } = 5;

    /// <summary>
    /// Top and bottom margins (if marginTop/Bottom not set).
    /// </summary>
    public int MarginHeight { get; set; } = 5;

    /// <summary>
    /// Left margin (overrides marginWidth).
    /// </summary>
    public int MarginLeft { get; set; } = 0;

    /// <summary>
    /// Top margin (overrides marginHeight).
    /// </summary>
    public int MarginTop { get; set; } = 0;

    /// <summary>
    /// Right margin (overrides marginWidth).
    /// </summary>
    public int MarginRight { get; set; } = 0;

    /// <summary>
    /// Bottom margin (overrides marginHeight).
    /// </summary>
    public int MarginBottom { get; set; } = 0;

    /// <summary>
    /// Horizontal spacing between columns.
    /// </summary>
    public int HorizontalSpacing { get; set; } = 5;

    /// <summary>
    /// Vertical spacing between rows.
    /// </summary>
    public int VerticalSpacing { get; set; } = 5;

    // Cache fields
    private int _cacheWidth = -1;
    private int _cacheHeight = -1;
    private int[]? _cachedColumnWidths;
    private int[]? _cachedRowHeights;

    /// <summary>
    /// Creates a new GridLayout with one column.
    /// </summary>
    public GridLayout()
    {
    }

    /// <summary>
    /// Creates a new GridLayout with the specified number of columns.
    /// </summary>
    public GridLayout(int numColumns)
    {
        NumColumns = numColumns;
    }

    /// <summary>
    /// Creates a new GridLayout with the specified number of columns and equal width setting.
    /// </summary>
    public GridLayout(int numColumns, bool makeColumnsEqualWidth)
    {
        NumColumns = numColumns;
        MakeColumnsEqualWidth = makeColumnsEqualWidth;
    }

    protected internal override Point ComputeSize(Composite composite, int wHint, int hHint, bool flushCache)
    {
        // Check cache validity
        if (!flushCache && _cacheWidth != -1 && wHint == SWT.DEFAULT && hHint == SWT.DEFAULT)
        {
            return new Point(_cacheWidth, _cacheHeight);
        }

        // Calculate margins once
        int leftMargin = MarginLeft != 0 ? MarginLeft : MarginWidth;
        int rightMargin = MarginRight != 0 ? MarginRight : MarginWidth;
        int topMargin = MarginTop != 0 ? MarginTop : MarginHeight;
        int bottomMargin = MarginBottom != 0 ? MarginBottom : MarginHeight;

        var grid = BuildGrid(composite);
        if (grid.Length == 0)
        {
            return new Point(leftMargin + rightMargin, topMargin + bottomMargin);
        }

        int numRows = grid.Length;
        int numCols = NumColumns;

        // Calculate column widths
        int[] columnWidths = ComputeColumnWidths(grid, wHint);

        // Calculate row heights
        int[] rowHeights = ComputeRowHeights(grid, hHint);

        // Sum with margins and spacing
        int totalWidth = columnWidths.Sum() + HorizontalSpacing * (numCols - 1) + leftMargin + rightMargin;
        int totalHeight = rowHeights.Sum() + VerticalSpacing * (numRows - 1) + topMargin + bottomMargin;

        // Update cache
        if (wHint == SWT.DEFAULT && hHint == SWT.DEFAULT)
        {
            _cacheWidth = totalWidth;
            _cacheHeight = totalHeight;
            _cachedColumnWidths = columnWidths;
            _cachedRowHeights = rowHeights;
        }

        return new Point(totalWidth, totalHeight);
    }

    protected internal override bool DoLayout(Composite composite, bool flushCache)
    {
        if (flushCache)
        {
            _cacheWidth = -1;
            _cacheHeight = -1;
            _cachedColumnWidths = null;
            _cachedRowHeights = null;
        }

        var grid = BuildGrid(composite);
        if (grid.Length == 0) return true;

        var clientArea = composite.GetClientArea();
        int numRows = grid.Length;
        int numCols = NumColumns;

        int leftMargin = MarginLeft != 0 ? MarginLeft : MarginWidth;
        int rightMargin = MarginRight != 0 ? MarginRight : MarginWidth;
        int topMargin = MarginTop != 0 ? MarginTop : MarginHeight;
        int bottomMargin = MarginBottom != 0 ? MarginBottom : MarginHeight;

        int availableWidth = clientArea.Width - leftMargin - rightMargin;
        int availableHeight = clientArea.Height - topMargin - bottomMargin;

        // Distribute available space to columns and rows
        int[] columnWidths = DistributeWidth(availableWidth, grid);
        int[] rowHeights = DistributeHeight(availableHeight, grid);

        // Position each control
        int y = clientArea.Y + topMargin;

        for (int row = 0; row < numRows; row++)
        {
            int x = clientArea.X + leftMargin;

            for (int col = 0; col < numCols; col++)
            {
                var cell = grid[row][col];
                if (cell.Control != null && !cell.Processed)
                {
                    var data = cell.Data;

                    // Calculate spanned width and height
                    int cellWidth = CalculateSpannedWidth(col, data.HorizontalSpan, columnWidths);
                    int cellHeight = CalculateSpannedHeight(row, data.VerticalSpan, rowHeights);

                    // Apply alignment and positioning
                    int childX = x + data.HorizontalIndent;
                    int childY = y + data.VerticalIndent;
                    int childWidth = cellWidth - data.HorizontalIndent;
                    int childHeight = cellHeight - data.VerticalIndent;

                    ApplyAlignment(data, ref childX, ref childY, ref childWidth, ref childHeight,
                                 x, y, cellWidth, cellHeight, cell.Control);

                    cell.Control.SetBounds(childX, childY, childWidth, childHeight);

                    // Mark spanning cells as processed
                    MarkSpanProcessed(grid, row, col, data.HorizontalSpan, data.VerticalSpan);
                }

                x += columnWidths[col] + HorizontalSpacing;
            }

            y += rowHeights[row] + VerticalSpacing;
        }

        return true;
    }

    private GridCell[][] BuildGrid(Composite composite)
    {
        var children = composite.GetChildren();
        if (children.Length == 0) return Array.Empty<GridCell[]>();

        // Count rows needed
        int row = 0, col = 0;
        var tempGrid = new List<List<GridCell>>();

        foreach (var child in children)
        {
            if (!child.Visible) continue;

            var data = GetGridData(child);
            if (data.Exclude) continue;

            // Ensure we have enough rows
            while (tempGrid.Count <= row)
            {
                var newRow = new List<GridCell>();
                for (int i = 0; i < NumColumns; i++)
                {
                    newRow.Add(new GridCell());
                }
                tempGrid.Add(newRow);
            }

            // Find first available cell in current row
            while (col < NumColumns && tempGrid[row][col].Control != null)
            {
                col++;
            }

            // Wrap to next row if needed
            if (col >= NumColumns)
            {
                row++;
                col = 0;

                // Ensure we have the new row
                while (tempGrid.Count <= row)
                {
                    var newRow = new List<GridCell>();
                    for (int i = 0; i < NumColumns; i++)
                    {
                        newRow.Add(new GridCell());
                    }
                    tempGrid.Add(newRow);
                }
            }

            // Place control in grid with spanning
            int colSpan = Math.Max(1, Math.Min(data.HorizontalSpan, NumColumns - col));
            int rowSpan = Math.Max(1, data.VerticalSpan);

            // Ensure we have enough rows for spanning
            while (tempGrid.Count < row + rowSpan)
            {
                var newRow = new List<GridCell>();
                for (int i = 0; i < NumColumns; i++)
                {
                    newRow.Add(new GridCell());
                }
                tempGrid.Add(newRow);
            }

            // Fill spanned cells
            for (int r = row; r < row + rowSpan && r < tempGrid.Count; r++)
            {
                for (int c = col; c < col + colSpan && c < NumColumns; c++)
                {
                    tempGrid[r][c] = new GridCell
                    {
                        Control = child,
                        Data = data,
                        Processed = false
                    };
                }
            }

            col += colSpan;
        }

        // Convert to 2D array
        return tempGrid.Select(rowList => rowList.ToArray()).ToArray();
    }

    private int[] ComputeColumnWidths(GridCell[][] grid, int wHint)
    {
        int[] widths = new int[NumColumns];

        // Calculate minimum width for each column
        for (int col = 0; col < NumColumns; col++)
        {
            int maxWidth = 0;

            for (int row = 0; row < grid.Length; row++)
            {
                var cell = grid[row][col];
                if (cell.Control != null && cell.Data.HorizontalSpan == 1)
                {
                    var size = GetControlSize(cell.Control, cell.Data);
                    maxWidth = Math.Max(maxWidth, size.X + cell.Data.HorizontalIndent);
                }
            }

            widths[col] = maxWidth;
        }

        if (MakeColumnsEqualWidth)
        {
            int maxWidth = widths.Max();
            for (int i = 0; i < widths.Length; i++)
            {
                widths[i] = maxWidth;
            }
        }

        return widths;
    }

    private int[] ComputeRowHeights(GridCell[][] grid, int hHint)
    {
        int[] heights = new int[grid.Length];

        // Calculate minimum height for each row
        for (int row = 0; row < grid.Length; row++)
        {
            int maxHeight = 0;

            for (int col = 0; col < NumColumns; col++)
            {
                var cell = grid[row][col];
                if (cell.Control != null && cell.Data.VerticalSpan == 1)
                {
                    var size = GetControlSize(cell.Control, cell.Data);
                    maxHeight = Math.Max(maxHeight, size.Y + cell.Data.VerticalIndent);
                }
            }

            heights[row] = maxHeight;
        }

        return heights;
    }

    private int[] DistributeWidth(int availableWidth, GridCell[][] grid)
    {
        int[] widths = _cachedColumnWidths ?? ComputeColumnWidths(grid, SWT.DEFAULT);
        int currentWidth = widths.Sum() + HorizontalSpacing * (NumColumns - 1);
        int extraWidth = availableWidth - currentWidth;

        if (extraWidth > 0)
        {
            // Distribute to grabbing columns
            var grabColumns = new bool[NumColumns];
            int grabCount = 0;

            for (int col = 0; col < NumColumns; col++)
            {
                for (int row = 0; row < grid.Length; row++)
                {
                    var cell = grid[row][col];
                    if (cell.Control != null && cell.Data.GrabExcessHorizontalSpace)
                    {
                        grabColumns[col] = true;
                        grabCount++;
                        break;
                    }
                }
            }

            if (grabCount > 0)
            {
                int perColumn = extraWidth / grabCount;
                for (int col = 0; col < NumColumns; col++)
                {
                    if (grabColumns[col])
                    {
                        widths[col] += perColumn;
                    }
                }
            }
        }

        return widths;
    }

    private int[] DistributeHeight(int availableHeight, GridCell[][] grid)
    {
        int[] heights = _cachedRowHeights ?? ComputeRowHeights(grid, SWT.DEFAULT);
        int currentHeight = heights.Sum() + VerticalSpacing * (grid.Length - 1);
        int extraHeight = availableHeight - currentHeight;

        if (extraHeight > 0)
        {
            // Distribute to grabbing rows
            var grabRows = new bool[grid.Length];
            int grabCount = 0;

            for (int row = 0; row < grid.Length; row++)
            {
                for (int col = 0; col < NumColumns; col++)
                {
                    var cell = grid[row][col];
                    if (cell.Control != null && cell.Data.GrabExcessVerticalSpace)
                    {
                        grabRows[row] = true;
                        grabCount++;
                        break;
                    }
                }
            }

            if (grabCount > 0)
            {
                int perRow = extraHeight / grabCount;
                for (int row = 0; row < grid.Length; row++)
                {
                    if (grabRows[row])
                    {
                        heights[row] += perRow;
                    }
                }
            }
        }

        return heights;
    }

    private int CalculateSpannedWidth(int col, int span, int[] columnWidths)
    {
        int width = 0;
        for (int i = col; i < col + span && i < columnWidths.Length; i++)
        {
            width += columnWidths[i];
            if (i > col) width += HorizontalSpacing;
        }
        return width;
    }

    private int CalculateSpannedHeight(int row, int span, int[] rowHeights)
    {
        int height = 0;
        for (int i = row; i < row + span && i < rowHeights.Length; i++)
        {
            height += rowHeights[i];
            if (i > row) height += VerticalSpacing;
        }
        return height;
    }

    private void ApplyAlignment(GridData data, ref int x, ref int y, ref int width, ref int height,
                                int cellX, int cellY, int cellWidth, int cellHeight, Control control)
    {
        var controlSize = GetControlSize(control, data);

        // Horizontal alignment
        switch (data.HorizontalAlignment)
        {
            case GridData.BEGINNING:
                width = Math.Min(controlSize.X, cellWidth - data.HorizontalIndent);
                break;
            case GridData.CENTER:
                width = Math.Min(controlSize.X, cellWidth - data.HorizontalIndent);
                x = cellX + (cellWidth - width) / 2;
                break;
            case GridData.END:
                width = Math.Min(controlSize.X, cellWidth - data.HorizontalIndent);
                x = cellX + cellWidth - width;
                break;
            case GridData.FILL:
                width = cellWidth - data.HorizontalIndent;
                break;
        }

        // Vertical alignment
        switch (data.VerticalAlignment)
        {
            case GridData.BEGINNING:
                height = Math.Min(controlSize.Y, cellHeight - data.VerticalIndent);
                break;
            case GridData.CENTER:
                height = Math.Min(controlSize.Y, cellHeight - data.VerticalIndent);
                y = cellY + (cellHeight - height) / 2;
                break;
            case GridData.END:
                height = Math.Min(controlSize.Y, cellHeight - data.VerticalIndent);
                y = cellY + cellHeight - height;
                break;
            case GridData.FILL:
                height = cellHeight - data.VerticalIndent;
                break;
        }

        // Apply size hints and minimums
        if (data.WidthHint != SWT.DEFAULT)
        {
            width = data.WidthHint;
        }
        if (data.HeightHint != SWT.DEFAULT)
        {
            height = data.HeightHint;
        }

        width = Math.Max(width, data.MinimumWidth);
        height = Math.Max(height, data.MinimumHeight);
    }

    private void MarkSpanProcessed(GridCell[][] grid, int startRow, int startCol, int colSpan, int rowSpan)
    {
        for (int r = startRow; r < startRow + rowSpan && r < grid.Length; r++)
        {
            for (int c = startCol; c < startCol + colSpan && c < NumColumns; c++)
            {
                grid[r][c].Processed = true;
            }
        }
    }

    private GridData GetGridData(Control control)
    {
        var data = control.GetLayoutData();
        return data as GridData ?? new GridData();
    }

    private Point GetControlSize(Control control, GridData data)
    {
        int width = data.WidthHint != SWT.DEFAULT ? data.WidthHint : 64;
        int height = data.HeightHint != SWT.DEFAULT ? data.HeightHint : 24;

        if (control is Composite composite)
        {
            var size = composite.ComputeSize(data.WidthHint, data.HeightHint, true);
            width = size.X;
            height = size.Y;
        }

        width = Math.Max(width, data.MinimumWidth);
        height = Math.Max(height, data.MinimumHeight);

        return new Point(width, height);
    }

    private class GridCell
    {
        public Control? Control { get; set; }
        public GridData Data { get; set; } = new GridData();
        public bool Processed { get; set; }
    }
}
