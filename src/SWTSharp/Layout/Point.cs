namespace SWTSharp.Layout;

/// <summary>
/// Represents a point with x and y coordinates.
/// Used throughout the layout system for size calculations.
/// </summary>
public struct Point
{
    /// <summary>
    /// The x coordinate or width.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// The y coordinate or height.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Creates a new Point with the specified coordinates.
    /// </summary>
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Returns a string representation of this point.
    /// </summary>
    public override string ToString()
    {
        return $"Point {{X={X}, Y={Y}}}";
    }

    /// <summary>
    /// Determines whether two points are equal.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is Point other)
        {
            return X == other.X && Y == other.Y;
        }
        return false;
    }

    /// <summary>
    /// Returns the hash code for this point.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + X;
            hash = hash * 31 + Y;
            return hash;
        }
    }

    /// <summary>
    /// Equality operator for points.
    /// </summary>
    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for points.
    /// </summary>
    public static bool operator !=(Point left, Point right)
    {
        return !left.Equals(right);
    }
}
