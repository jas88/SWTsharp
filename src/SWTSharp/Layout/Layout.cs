namespace SWTSharp.Layout;

/// <summary>
/// Abstract base class for all layout managers.
/// Layouts control the positioning and sizing of child controls within a Composite.
/// </summary>
public abstract class Layout
{
    /// <summary>
    /// Computes and returns the preferred size of the composite based on
    /// the layout, taking into account all child controls and their layout data.
    /// </summary>
    /// <param name="composite">The composite to calculate size for.</param>
    /// <param name="wHint">Width hint in pixels, or SWT.DEFAULT (-1) for natural width.</param>
    /// <param name="hHint">Height hint in pixels, or SWT.DEFAULT (-1) for natural height.</param>
    /// <param name="flushCache">If true, cached layout information should be recalculated.</param>
    /// <returns>The preferred size as a Point with width (X) and height (Y).</returns>
    protected internal abstract Point ComputeSize(Composite composite, int wHint, int hHint, bool flushCache);

    /// <summary>
    /// Positions and sizes the children of the composite according to the
    /// layout algorithm and layout data attached to children.
    /// </summary>
    /// <param name="composite">The composite to lay out.</param>
    /// <param name="flushCache">If true, cached layout information should be recalculated.</param>
    /// <returns>True if the layout was successful.</returns>
    protected internal abstract bool DoLayout(Composite composite, bool flushCache);

    /// <summary>
    /// Flushes cached layout information for the specified control.
    /// Override this method to provide custom cache management.
    /// </summary>
    /// <param name="control">The control whose cache should be flushed.</param>
    /// <returns>True if cache was flushed, false otherwise.</returns>
    protected internal virtual bool FlushCache(Control? control)
    {
        return true; // Default: always recalculate
    }
}
