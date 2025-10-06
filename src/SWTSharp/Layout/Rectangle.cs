namespace SWTSharp.Layout;

/// <summary>
/// Represents a rectangle with x, y, width, and height.
/// Used for client area calculations in layouts.
/// </summary>
public struct Rectangle
{
    /// <summary>
    /// The x coordinate of the upper-left corner.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The y coordinate of the upper-left corner.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Creates a new Rectangle with the specified bounds.
    /// </summary>
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Returns a string representation of this rectangle.
    /// </summary>
    public override string ToString()
    {
        return $"Rectangle {{X={X}, Y={Y}, Width={Width}, Height={Height}}}";
    }

    /// <summary>
    /// Determines whether two rectangles are equal.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Rectangle other)
        {
            return X == other.X && Y == other.Y &&
                   Width == other.Width && Height == other.Height;
        }
        return false;
    }

    /// <summary>
    /// Returns the hash code for this rectangle.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            hash = hash * 31 + Width;
            hash = hash * 31 + Height;
            return hash;
        }
    }

    /// <summary>
    /// Equality operator for rectangles.
    /// </summary>
    public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for rectangles.
    /// </summary>
    public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !left.Equals(right);
    }
}
