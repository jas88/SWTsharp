namespace SWTSharp.Graphics;

/// <summary>
/// Device-independent rectangle representation.
/// Represents a rectangular area with x, y coordinates and width, height dimensions.
/// </summary>
public struct Rectangle : IEquatable<Rectangle>
{
    /// <summary>
    /// Gets or sets the x-coordinate of the rectangle's upper-left corner.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the y-coordinate of the rectangle's upper-left corner.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the rectangle.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rectangle.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the Rectangle struct.
    /// </summary>
    /// <param name="x">X-coordinate</param>
    /// <param name="y">Y-coordinate</param>
    /// <param name="width">Width</param>
    /// <param name="height">Height</param>
    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets a value indicating whether the rectangle is empty (width or height is zero or negative).
    /// </summary>
    public bool IsEmpty => Width <= 0 || Height <= 0;

    /// <summary>
    /// Determines whether this rectangle contains the specified point.
    /// </summary>
    public bool Contains(int x, int y)
    {
        return x >= X && x < X + Width && y >= Y && y < Y + Height;
    }

    /// <summary>
    /// Determines whether this rectangle intersects another rectangle.
    /// </summary>
    public bool Intersects(Rectangle rect)
    {
        return rect.X < X + Width && X < rect.X + rect.Width &&
               rect.Y < Y + Height && Y < rect.Y + rect.Height;
    }

    /// <summary>
    /// Determines whether this rectangle equals another rectangle.
    /// </summary>
    public bool Equals(Rectangle other)
    {
        return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
    }

    /// <summary>
    /// Determines whether this rectangle equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Rectangle rect && Equals(rect);
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
    /// Returns a string representation of this rectangle.
    /// </summary>
    public override string ToString()
    {
        return $"Rectangle {{x={X}, y={Y}, width={Width}, height={Height}}}";
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Rectangle left, Rectangle right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Rectangle left, Rectangle right)
    {
        return !left.Equals(right);
    }
}
