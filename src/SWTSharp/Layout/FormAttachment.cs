namespace SWTSharp.Layout;

/// <summary>
/// FormAttachment specifies the attachment of a side of a control to a position
/// in the parent composite. Attachments can be to:
/// - A percentage of the parent (numerator/denominator)
/// - Another control
/// - A combination of both with an offset
/// </summary>
public class FormAttachment
{
    /// <summary>
    /// Numerator for percentage-based attachment.
    /// </summary>
    public int Numerator { get; set; }

    /// <summary>
    /// Denominator for percentage-based attachment (default 100 for percentages).
    /// </summary>
    public int Denominator { get; set; } = 100;

    /// <summary>
    /// Offset in pixels from the computed position.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Control to attach to (if attaching to another control's edge).
    /// </summary>
    public Control? Control { get; set; }

    /// <summary>
    /// Which side of the control to attach to.
    /// Valid values: SWT.TOP, SWT.BOTTOM, SWT.LEFT, SWT.RIGHT, SWT.CENTER, SWT.DEFAULT
    /// </summary>
    public int Alignment { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Creates an attachment to a percentage position with an optional offset.
    /// </summary>
    /// <param name="numerator">Numerator (e.g., 50 for 50%)</param>
    /// <param name="offset">Offset in pixels from the percentage position</param>
    public FormAttachment(int numerator, int offset = 0)
    {
        Numerator = numerator;
        Denominator = 100;
        Offset = offset;
    }

    /// <summary>
    /// Creates an attachment to a percentage position with custom denominator.
    /// </summary>
    /// <param name="numerator">Numerator</param>
    /// <param name="denominator">Denominator</param>
    /// <param name="offset">Offset in pixels</param>
    public FormAttachment(int numerator, int denominator, int offset)
    {
        Numerator = numerator;
        Denominator = denominator;
        Offset = offset;
    }

    /// <summary>
    /// Creates an attachment to another control.
    /// </summary>
    /// <param name="control">Control to attach to</param>
    /// <param name="offset">Offset in pixels from the control</param>
    /// <param name="alignment">Which edge of the control (SWT.TOP, BOTTOM, LEFT, RIGHT, CENTER, DEFAULT)</param>
    public FormAttachment(Control control, int offset = 0, int alignment = SWT.DEFAULT)
    {
        Control = control;
        Offset = offset;
        Alignment = alignment;
    }
}
