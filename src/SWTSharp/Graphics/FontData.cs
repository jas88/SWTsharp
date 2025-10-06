namespace SWTSharp.Graphics;

/// <summary>
/// Device-independent font description.
/// This class describes a font using name, height, and style without allocating OS resources.
/// </summary>
public class FontData : IEquatable<FontData>
{
    /// <summary>
    /// Gets or sets the font name (e.g., "Arial", "Helvetica").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the font height in points.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the font style (SWT.NORMAL, SWT.BOLD, SWT.ITALIC, or combined).
    /// </summary>
    public int Style { get; set; }

    /// <summary>
    /// Initializes a new instance of the FontData class with default values.
    /// </summary>
    public FontData()
    {
        Name = "System";
        Height = 12;
        Style = SWT.NORMAL;
    }

    /// <summary>
    /// Initializes a new instance of the FontData class.
    /// </summary>
    /// <param name="name">Font name</param>
    /// <param name="height">Font height in points</param>
    /// <param name="style">Font style</param>
    public FontData(string name, int height, int style)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Height = height > 0 ? height : throw new ArgumentException("Height must be positive", nameof(height));
        Style = style;
    }

    /// <summary>
    /// Determines whether this FontData equals another FontData.
    /// </summary>
    public bool Equals(FontData? other)
    {
        if (other == null) return false;
        return Name == other.Name && Height == other.Height && Style == other.Style;
    }

    /// <summary>
    /// Determines whether this FontData equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is FontData fontData && Equals(fontData);
    }

    /// <summary>
    /// Returns the hash code for this FontData.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (Name?.GetHashCode() ?? 0);
            hash = hash * 31 + Height;
            hash = hash * 31 + Style;
            return hash;
        }
    }

    /// <summary>
    /// Returns a string representation of this FontData.
    /// </summary>
    public override string ToString()
    {
        var styles = new List<string>();
        if ((Style & SWT.BOLD) != 0) styles.Add("bold");
        if ((Style & SWT.ITALIC) != 0) styles.Add("italic");

        var styleStr = styles.Count > 0 ? string.Join(" ", styles) : "normal";
        return $"{Name} {Height}pt {styleStr}";
    }
}
