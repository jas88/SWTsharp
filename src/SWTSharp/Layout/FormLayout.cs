namespace SWTSharp.Layout;

/// <summary>
/// FormLayout provides constraint-based positioning where each control's edges
/// can be attached to percentages of the parent or to other controls' edges.
/// This is the most flexible layout manager but requires careful constraint definition.
/// </summary>
public class FormLayout : Layout
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

    /// <summary>
    /// MarginLeft specifies the left margin (overrides MarginWidth if set).
    /// </summary>
    public int MarginLeft { get; set; } = 0;

    /// <summary>
    /// MarginTop specifies the top margin (overrides MarginHeight if set).
    /// </summary>
    public int MarginTop { get; set; } = 0;

    /// <summary>
    /// MarginRight specifies the right margin (overrides MarginWidth if set).
    /// </summary>
    public int MarginRight { get; set; } = 0;

    /// <summary>
    /// MarginBottom specifies the bottom margin (overrides MarginHeight if set).
    /// </summary>
    public int MarginBottom { get; set; } = 0;

    /// <summary>
    /// Spacing between controls (used when controls attach to each other).
    /// </summary>
    public int Spacing { get; set; } = 0;

    /// <summary>
    /// Creates a new FormLayout.
    /// </summary>
    public FormLayout()
    {
    }

    protected internal override Point ComputeSize(Composite composite, int wHint, int hHint, bool flushCache)
    {
        var children = composite.GetChildren().Where(c => c.Visible).ToArray();

        if (children.Length == 0)
        {
            int leftMargin = MarginLeft != 0 ? MarginLeft : MarginWidth;
            int rightMargin = MarginRight != 0 ? MarginRight : MarginWidth;
            int topMargin = MarginTop != 0 ? MarginTop : MarginHeight;
            int bottomMargin = MarginBottom != 0 ? MarginBottom : MarginHeight;

            return new Point(
                leftMargin + rightMargin,
                topMargin + bottomMargin
            );
        }

        // Build dependency graph to determine processing order
        var graph = BuildDependencyGraph(children);

        // Check for circular dependencies
        if (HasCircularDependencies(graph, children))
        {
            throw new InvalidOperationException(
                "FormLayout: Circular dependency detected in control attachments. " +
                "Controls cannot have cyclic references (A depends on B, B depends on A)."
            );
        }

        // For size computation, we need to consider all controls
        // and find the maximum extent in each dimension
        int maxWidth = 0;
        int maxHeight = 0;

        foreach (var child in children)
        {
            var data = GetFormData(child);
            var size = GetControlSize(child, data);

            // Estimate position based on attachments
            int left = EstimatePosition(data.Left, wHint, true);
            int top = EstimatePosition(data.Top, hHint, false);
            int width = data.Width != SWT.DEFAULT ? data.Width : size.X;
            int height = data.Height != SWT.DEFAULT ? data.Height : size.Y;

            maxWidth = Math.Max(maxWidth, left + width);
            maxHeight = Math.Max(maxHeight, top + height);
        }

        int leftMargin2 = MarginLeft != 0 ? MarginLeft : MarginWidth;
        int rightMargin2 = MarginRight != 0 ? MarginRight : MarginWidth;
        int topMargin2 = MarginTop != 0 ? MarginTop : MarginHeight;
        int bottomMargin2 = MarginBottom != 0 ? MarginBottom : MarginHeight;

        return new Point(
            maxWidth + leftMargin2 + rightMargin2,
            maxHeight + topMargin2 + bottomMargin2
        );
    }

    protected internal override bool DoLayout(Composite composite, bool flushCache)
    {
        var children = composite.GetChildren().Where(c => c.Visible).ToArray();
        if (children.Length == 0) return true;

        var clientArea = composite.GetClientArea();
        int width = clientArea.Width;
        int height = clientArea.Height;

        int leftMargin = MarginLeft != 0 ? MarginLeft : MarginWidth;
        int rightMargin = MarginRight != 0 ? MarginRight : MarginWidth;
        int topMargin = MarginTop != 0 ? MarginTop : MarginHeight;
        int bottomMargin = MarginBottom != 0 ? MarginBottom : MarginHeight;

        // Build dependency graph
        var graph = BuildDependencyGraph(children);

        // Check for circular dependencies
        if (HasCircularDependencies(graph, children))
        {
            throw new InvalidOperationException(
                "FormLayout: Circular dependency detected in control attachments."
            );
        }

        // Topological sort to determine processing order
        var processingOrder = TopologicalSort(graph, children);

        // Store calculated positions
        var positions = new Dictionary<Control, Rectangle>();

        // Process controls in dependency order
        foreach (var child in processingOrder)
        {
            var data = GetFormData(child);
            var size = GetControlSize(child, data);

            // Calculate position based on attachments
            int x = clientArea.X + leftMargin;
            int y = clientArea.Y + topMargin;
            int w = data.Width != SWT.DEFAULT ? data.Width : size.X;
            int h = data.Height != SWT.DEFAULT ? data.Height : size.Y;

            // Process left attachment
            if (data.Left != null)
            {
                x = CalculateAttachment(data.Left, width, height, positions, true, true) + clientArea.X;
            }

            // Process top attachment
            if (data.Top != null)
            {
                y = CalculateAttachment(data.Top, width, height, positions, false, true) + clientArea.Y;
            }

            // Process right attachment (affects width if left is also set)
            if (data.Right != null)
            {
                int right = CalculateAttachment(data.Right, width, height, positions, true, false) + clientArea.X;

                if (data.Left != null)
                {
                    // Both left and right set: compute width
                    w = right - x;
                }
                else
                {
                    // Only right set: position from right
                    x = right - w;
                }
            }

            // Process bottom attachment (affects height if top is also set)
            if (data.Bottom != null)
            {
                int bottom = CalculateAttachment(data.Bottom, width, height, positions, false, false) + clientArea.Y;

                if (data.Top != null)
                {
                    // Both top and bottom set: compute height
                    h = bottom - y;
                }
                else
                {
                    // Only bottom set: position from bottom
                    y = bottom - h;
                }
            }

            // Ensure non-negative dimensions
            w = Math.Max(0, w);
            h = Math.Max(0, h);

            // Store position for dependency resolution
            positions[child] = new Rectangle(x - clientArea.X, y - clientArea.Y, w, h);

            // Apply to control
            child.SetBounds(x, y, w, h);
        }

        return true;
    }

    private Dictionary<Control, List<Control>> BuildDependencyGraph(Control[] children)
    {
        var graph = new Dictionary<Control, List<Control>>();

        foreach (var child in children)
        {
            graph[child] = new List<Control>();
            var data = GetFormData(child);

            // Add dependencies based on attachments
            if (data.Left?.Control != null && children.Contains(data.Left.Control))
            {
                graph[child].Add(data.Left.Control);
            }
            if (data.Right?.Control != null && children.Contains(data.Right.Control))
            {
                graph[child].Add(data.Right.Control);
            }
            if (data.Top?.Control != null && children.Contains(data.Top.Control))
            {
                graph[child].Add(data.Top.Control);
            }
            if (data.Bottom?.Control != null && children.Contains(data.Bottom.Control))
            {
                graph[child].Add(data.Bottom.Control);
            }
        }

        return graph;
    }

    private bool HasCircularDependencies(Dictionary<Control, List<Control>> graph, Control[] children)
    {
        var visited = new HashSet<Control>();
        var recursionStack = new HashSet<Control>();

        foreach (var node in children)
        {
            if (HasCycle(node, graph, visited, recursionStack))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasCycle(Control node, Dictionary<Control, List<Control>> graph,
                         HashSet<Control> visited, HashSet<Control> recursionStack)
    {
        if (recursionStack.Contains(node))
        {
            return true; // Cycle detected
        }

        if (visited.Contains(node))
        {
            return false; // Already processed
        }

        visited.Add(node);
        recursionStack.Add(node);

        if (graph.TryGetValue(node, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                if (HasCycle(dependency, graph, visited, recursionStack))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(node);
        return false;
    }

    private List<Control> TopologicalSort(Dictionary<Control, List<Control>> graph, Control[] children)
    {
        var sorted = new List<Control>();
        var visited = new HashSet<Control>();

        foreach (var node in children)
        {
            if (!visited.Contains(node))
            {
                TopologicalSortVisit(node, graph, visited, sorted);
            }
        }

        return sorted;
    }

    private void TopologicalSortVisit(Control node, Dictionary<Control, List<Control>> graph,
                                     HashSet<Control> visited, List<Control> sorted)
    {
        visited.Add(node);

        if (graph.TryGetValue(node, out var dependencies))
        {
            foreach (var dependency in dependencies)
            {
                if (!visited.Contains(dependency))
                {
                    TopologicalSortVisit(dependency, graph, visited, sorted);
                }
            }
        }

        sorted.Add(node);
    }

    private int CalculateAttachment(FormAttachment attachment, int parentWidth, int parentHeight,
                                   Dictionary<Control, Rectangle> positions, bool horizontal, bool isStart)
    {
        int position = 0;

        // Percentage-based attachment
        if (attachment.Control == null)
        {
            int size = horizontal ? parentWidth : parentHeight;
            position = (size * attachment.Numerator) / attachment.Denominator;
        }
        // Control-based attachment
        else if (positions.TryGetValue(attachment.Control, out var controlBounds))
        {
            int alignment = attachment.Alignment;

            if (horizontal)
            {
                position = alignment switch
                {
                    SWT.LEFT => controlBounds.X,
                    SWT.RIGHT => controlBounds.X + controlBounds.Width,
                    SWT.CENTER => controlBounds.X + controlBounds.Width / 2,
                    _ => isStart ? controlBounds.X + controlBounds.Width + Spacing : controlBounds.X - Spacing
                };
            }
            else
            {
                position = alignment switch
                {
                    SWT.TOP => controlBounds.Y,
                    SWT.BOTTOM => controlBounds.Y + controlBounds.Height,
                    SWT.CENTER => controlBounds.Y + controlBounds.Height / 2,
                    _ => isStart ? controlBounds.Y + controlBounds.Height + Spacing : controlBounds.Y - Spacing
                };
            }
        }

        return position + attachment.Offset;
    }

    private int EstimatePosition(FormAttachment? attachment, int hint, bool horizontal)
    {
        if (attachment == null) return 0;

        if (attachment.Control == null)
        {
            // Percentage-based
            int size = hint != SWT.DEFAULT ? hint : 100;
            return (size * attachment.Numerator) / attachment.Denominator + attachment.Offset;
        }

        // Control-based: estimate at 0 (will be resolved during layout)
        return attachment.Offset;
    }

    private FormData GetFormData(Control control)
    {
        var data = control.GetLayoutData();
        return data as FormData ?? new FormData();
    }

    private Point GetControlSize(Control control, FormData data)
    {
        int width = data.Width != SWT.DEFAULT ? data.Width : 64;
        int height = data.Height != SWT.DEFAULT ? data.Height : 24;

        if (control is Composite composite && data.Width == SWT.DEFAULT && data.Height == SWT.DEFAULT)
        {
            var size = composite.ComputeSize(SWT.DEFAULT, SWT.DEFAULT, true);
            width = size.X;
            height = size.Y;
        }

        return new Point(width, height);
    }
}
