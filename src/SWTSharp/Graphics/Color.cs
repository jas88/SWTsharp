using SWTSharp.Platform;

namespace SWTSharp.Graphics;

/// <summary>
/// Device-specific color resource.
/// Represents a color with red, green, and blue components that has been allocated on a specific device.
/// Colors must be explicitly disposed when no longer needed.
/// </summary>
public class Color : Resource
{
    private readonly int red;
    private readonly int green;
    private readonly int blue;

    /// <summary>
    /// Gets the red component (0-255).
    /// </summary>
    public int Red
    {
        get
        {
            CheckDisposed();
            return red;
        }
    }

    /// <summary>
    /// Gets the green component (0-255).
    /// </summary>
    public int Green
    {
        get
        {
            CheckDisposed();
            return green;
        }
    }

    /// <summary>
    /// Gets the blue component (0-255).
    /// </summary>
    public int Blue
    {
        get
        {
            CheckDisposed();
            return blue;
        }
    }

    /// <summary>
    /// Initializes a new instance of the Color class from RGB components.
    /// </summary>
    /// <param name="device">The device to create the color on</param>
    /// <param name="red">Red component (0-255)</param>
    /// <param name="green">Green component (0-255)</param>
    /// <param name="blue">Blue component (0-255)</param>
    public Color(Device device, int red, int green, int blue)
        : base(device)
    {
        this.red = Clamp(red);
        this.green = Clamp(green);
        this.blue = Clamp(blue);

        Handle = CreatePlatformColor();
    }

    /// <summary>
    /// Initializes a new instance of the Color class from an RGB object.
    /// </summary>
    /// <param name="device">The device to create the color on</param>
    /// <param name="rgb">RGB color values</param>
    public Color(Device device, RGB rgb)
        : this(device, rgb.Red, rgb.Green, rgb.Blue)
    {
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
    /// Creates the platform-specific color handle.
    /// </summary>
    private IntPtr CreatePlatformColor()
    {
        var platformColor = Device.Platform as IPlatformGraphics;
        if (platformColor == null)
        {
            throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Platform does not support graphics operations");
        }

        return platformColor.CreateColor(red, green, blue);
    }

    /// <summary>
    /// Gets the RGB representation of this color.
    /// </summary>
    /// <returns>RGB color values</returns>
    public RGB GetRGB()
    {
        CheckDisposed();
        return new RGB(red, green, blue);
    }

    /// <summary>
    /// Releases the platform-specific color handle.
    /// </summary>
    protected override void ReleaseHandle()
    {
        if (Handle == IntPtr.Zero) return;

        var platformColor = Device.Platform as IPlatformGraphics;
        platformColor?.DestroyColor(Handle);
    }

    /// <summary>
    /// Determines whether this color equals another color.
    /// </summary>
    public bool Equals(Color? other)
    {
        if (other == null || IsDisposed || other.IsDisposed)
            return false;

        return red == other.red && green == other.green && blue == other.blue;
    }

    /// <summary>
    /// Determines whether this color equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Color color && Equals(color);
    }

    /// <summary>
    /// Returns the hash code for this color.
    /// </summary>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + red;
            hash = hash * 31 + green;
            hash = hash * 31 + blue;
            return hash;
        }
    }

    /// <summary>
    /// Returns a string representation of this color.
    /// </summary>
    public override string ToString()
    {
        if (IsDisposed)
            return "Color {disposed}";

        return $"Color {{red={red}, green={green}, blue={blue}}}";
    }
}
