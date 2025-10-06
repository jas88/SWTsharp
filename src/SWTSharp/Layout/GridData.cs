namespace SWTSharp.Layout;

/// <summary>
/// Layout data for controls in a GridLayout.
/// Provides detailed control over positioning, sizing, alignment, and spanning.
/// </summary>
public class GridData
{
    // Alignment constants
    /// <summary>
    /// Align to the beginning (left for horizontal, top for vertical).
    /// </summary>
    public const int BEGINNING = 1;

    /// <summary>
    /// Center in the available space.
    /// </summary>
    public const int CENTER = 2;

    /// <summary>
    /// Align to the end (right for horizontal, bottom for vertical).
    /// </summary>
    public const int END = 3;

    /// <summary>
    /// Fill the entire available space.
    /// </summary>
    public const int FILL = 4;

    /// <summary>
    /// Number of columns this control should span.
    /// </summary>
    public int HorizontalSpan { get; set; } = 1;

    /// <summary>
    /// Number of rows this control should span.
    /// </summary>
    public int VerticalSpan { get; set; } = 1;

    /// <summary>
    /// Horizontal alignment within the cell.
    /// </summary>
    public int HorizontalAlignment { get; set; } = BEGINNING;

    /// <summary>
    /// Vertical alignment within the cell.
    /// </summary>
    public int VerticalAlignment { get; set; } = CENTER;

    /// <summary>
    /// Whether to grab excess horizontal space.
    /// </summary>
    public bool GrabExcessHorizontalSpace { get; set; } = false;

    /// <summary>
    /// Whether to grab excess vertical space.
    /// </summary>
    public bool GrabExcessVerticalSpace { get; set; } = false;

    /// <summary>
    /// Preferred width hint in pixels, or SWT.DEFAULT.
    /// </summary>
    public int WidthHint { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Preferred height hint in pixels, or SWT.DEFAULT.
    /// </summary>
    public int HeightHint { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Minimum width in pixels.
    /// </summary>
    public int MinimumWidth { get; set; } = 0;

    /// <summary>
    /// Minimum height in pixels.
    /// </summary>
    public int MinimumHeight { get; set; } = 0;

    /// <summary>
    /// Left indentation in pixels.
    /// </summary>
    public int HorizontalIndent { get; set; } = 0;

    /// <summary>
    /// Top indentation in pixels.
    /// </summary>
    public int VerticalIndent { get; set; } = 0;

    /// <summary>
    /// Whether to exclude this control from the layout.
    /// </summary>
    public bool Exclude { get; set; } = false;

    /// <summary>
    /// Creates a new GridData with default values.
    /// </summary>
    public GridData()
    {
    }

    /// <summary>
    /// Creates a new GridData with the specified style.
    /// Style can combine SWT.FILL, SWT.HORIZONTAL, SWT.VERTICAL, etc.
    /// </summary>
    public GridData(int style)
    {
        // Parse style bits
        if ((style & SWT.HORIZONTAL) != 0)
        {
            HorizontalAlignment = FILL;
            GrabExcessHorizontalSpace = true;
        }

        if ((style & SWT.VERTICAL) != 0)
        {
            VerticalAlignment = FILL;
            GrabExcessVerticalSpace = true;
        }

        // Note: In real SWT, there are specific constants like SWT.FILL_HORIZONTAL
        // For now, we use the basic constants
    }

    /// <summary>
    /// Creates a new GridData with the specified alignment and grab settings.
    /// </summary>
    public GridData(int horizontalAlignment, int verticalAlignment,
                   bool grabExcessHorizontalSpace, bool grabExcessVerticalSpace)
    {
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        GrabExcessHorizontalSpace = grabExcessHorizontalSpace;
        GrabExcessVerticalSpace = grabExcessVerticalSpace;
    }

    /// <summary>
    /// Creates a new GridData with the specified alignment, grab, and span settings.
    /// </summary>
    public GridData(int horizontalAlignment, int verticalAlignment,
                   bool grabExcessHorizontalSpace, bool grabExcessVerticalSpace,
                   int horizontalSpan, int verticalSpan)
    {
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        GrabExcessHorizontalSpace = grabExcessHorizontalSpace;
        GrabExcessVerticalSpace = grabExcessVerticalSpace;
        HorizontalSpan = horizontalSpan;
        VerticalSpan = verticalSpan;
    }

    /// <summary>
    /// Returns a string representation of this GridData.
    /// </summary>
    public override string ToString()
    {
        return $"GridData {{HSpan={HorizontalSpan}, VSpan={VerticalSpan}, " +
               $"HAlign={HorizontalAlignment}, VAlign={VerticalAlignment}, " +
               $"GrabH={GrabExcessHorizontalSpace}, GrabV={GrabExcessVerticalSpace}}}";
    }
}
