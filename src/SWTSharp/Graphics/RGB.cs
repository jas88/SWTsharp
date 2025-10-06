namespace SWTSharp.Graphics;

/// <summary>
/// Device-independent RGB color representation.
/// This struct represents a color using red, green, and blue values.
/// </summary>
public struct RGB : IEquatable<RGB>
{
    /// <summary>
    /// Gets or sets the red component (0-255).
    /// </summary>
    public int Red { get; set; }

    /// <summary>
    /// Gets or sets the green component (0-255).
    /// </summary>
    public int Green { get; set; }

    /// <summary>
    /// Gets or sets the blue component (0-255).
    /// </summary>
    public int Blue { get; set; }

    /// <summary>
    /// Initializes a new instance of the RGB struct.
    /// </summary>
    /// <param name="red">Red component (0-255)</param>
    /// <param name="green">Green component (0-255)</param>
    /// <param name="blue">Blue component (0-255)</param>
    public RGB(int red, int green, int blue)
    {
        Red = Clamp(red);
        Green = Clamp(green);
        Blue = Clamp(blue);
    }

    /// <summary>
    /// Clamps a color component value to the valid range (0-255).
    /// </summary>
    private static int Clamp(int value)
    {
        if (value < 0) return 0;
        if (value > 255) return 255;
        return value;
    }

    /// <summary>
    /// Determines whether this RGB equals another RGB.
    /// </summary>
    public bool Equals(RGB other)
    {
        return Red == other.Red && Green == other.Green && Blue == other.Blue;
    }

    /// <summary>
    /// Determines whether this RGB equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is RGB rgb && Equals(rgb);
    }

    /// <summary>
    /// Returns the hash code for this RGB.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Red;
            hash = hash * 31 + Green;
            hash = hash * 31 + Blue;
            return hash;
        }
    }

    /// <summary>
    /// Returns a string representation of this RGB.
    /// </summary>
    public override string ToString()
    {
        return $"RGB {{red={Red}, green={Green}, blue={Blue}}}";
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(RGB left, RGB right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(RGB left, RGB right)
    {
        return !left.Equals(right);
    }
}
