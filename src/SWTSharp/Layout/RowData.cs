namespace SWTSharp.Layout;

/// <summary>
/// Layout data for controls in a RowLayout.
/// Each control can specify width and height hints.
/// </summary>
public class RowData
{
    /// <summary>
    /// Preferred width of the control in pixels, or SWT.DEFAULT.
    /// </summary>
    public int Width { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Preferred height of the control in pixels, or SWT.DEFAULT.
    /// </summary>
    public int Height { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Whether to exclude this control from the layout.
    /// </summary>
    public bool Exclude { get; set; } = false;

    /// <summary>
    /// Creates a new RowData with default values.
    /// </summary>
    public RowData()
    {
    }

    /// <summary>
    /// Creates a new RowData with the specified width and height.
    /// </summary>
    public RowData(int width, int height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates a new RowData from a Point.
    /// </summary>
    public RowData(Point point)
    {
        Width = point.X;
        Height = point.Y;
    }

    /// <summary>
    /// Returns a string representation of this RowData.
    /// </summary>
    public override string ToString()
    {
        return $"RowData {{Width={Width}, Height={Height}, Exclude={Exclude}}}";
    }
}
