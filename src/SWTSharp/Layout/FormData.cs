namespace SWTSharp.Layout;

/// <summary>
/// FormData specifies the attachments for a control in a FormLayout.
/// Each side (left, right, top, bottom) can be attached independently using FormAttachment.
/// </summary>
public class FormData
{
    /// <summary>
    /// Width hint in pixels. If specified, overrides width calculated from attachments.
    /// </summary>
    public int Width { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Height hint in pixels. If specified, overrides height calculated from attachments.
    /// </summary>
    public int Height { get; set; } = SWT.DEFAULT;

    /// <summary>
    /// Attachment for the left side of the control.
    /// </summary>
    public FormAttachment? Left { get; set; }

    /// <summary>
    /// Attachment for the right side of the control.
    /// </summary>
    public FormAttachment? Right { get; set; }

    /// <summary>
    /// Attachment for the top side of the control.
    /// </summary>
    public FormAttachment? Top { get; set; }

    /// <summary>
    /// Attachment for the bottom side of the control.
    /// </summary>
    public FormAttachment? Bottom { get; set; }

    /// <summary>
    /// Creates FormData with no attachments.
    /// </summary>
    public FormData()
    {
    }

    /// <summary>
    /// Creates FormData with specific width and height.
    /// </summary>
    /// <param name="width">Width hint</param>
    /// <param name="height">Height hint</param>
    public FormData(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
