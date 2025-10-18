namespace SWTSharp.Graphics;

/// <summary>
/// Represents a point or size with Width and Height coordinates.
/// </summary>
public record struct Point(int Width, int Height)
{
    /// <summary>
    /// Gets or sets the X coordinate (same as Width).
    /// </summary>
    public int X
    {
        readonly get => Width;
        set => Width = value;
    }

    /// <summary>
    /// Gets or sets the Y coordinate (same as Height).
    /// </summary>
    public int Y
    {
        readonly get => Height;
        set => Height = value;
    }
}
