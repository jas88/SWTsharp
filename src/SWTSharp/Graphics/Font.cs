using SWTSharp.Platform;

namespace SWTSharp.Graphics;

/// <summary>
/// Device-specific font resource.
/// Represents a font that has been allocated on a specific device.
/// Fonts must be explicitly disposed when no longer needed.
/// </summary>
public class Font : Resource
{
    private readonly FontData fontData;

    /// <summary>
    /// Initializes a new instance of the Font class from FontData.
    /// </summary>
    /// <param name="device">The device to create the font on</param>
    /// <param name="fontData">Font description</param>
    public Font(Device device, FontData fontData)
        : base(device)
    {
        this.fontData = fontData ?? throw new ArgumentNullException(nameof(fontData));
        Handle = CreatePlatformFont();
    }

    /// <summary>
    /// Initializes a new instance of the Font class from name, height, and style.
    /// </summary>
    /// <param name="device">The device to create the font on</param>
    /// <param name="name">Font name</param>
    /// <param name="height">Font height in points</param>
    /// <param name="style">Font style (SWT.NORMAL, SWT.BOLD, SWT.ITALIC, or combined)</param>
    public Font(Device device, string name, int height, int style)
        : this(device, new FontData(name, height, style))
    {
    }

    /// <summary>
    /// Creates the platform-specific font handle.
    /// </summary>
    private IntPtr CreatePlatformFont()
    {
        var platformGraphics = Device.Platform as IPlatformGraphics;
        if (platformGraphics == null)
        {
            throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Platform does not support graphics operations");
        }

        return platformGraphics.CreateFont(fontData.Name, fontData.Height, fontData.Style);
    }

    /// <summary>
    /// Gets the font data describing this font.
    /// </summary>
    /// <returns>Font description</returns>
    public FontData GetFontData()
    {
        CheckDisposed();
        return new FontData(fontData.Name, fontData.Height, fontData.Style);
    }

    /// <summary>
    /// Releases the platform-specific font handle.
    /// </summary>
    protected override void ReleaseHandle()
    {
        if (Handle == IntPtr.Zero) return;

        var platformGraphics = Device.Platform as IPlatformGraphics;
        platformGraphics?.DestroyFont(Handle);
    }

    /// <summary>
    /// Determines whether this font equals another font.
    /// </summary>
    public bool Equals(Font? other)
    {
        if (other == null || IsDisposed || other.IsDisposed)
            return false;

        return fontData.Equals(other.fontData);
    }

    /// <summary>
    /// Determines whether this font equals another object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is Font font && Equals(font);
    }

    /// <summary>
    /// Returns the hash code for this font.
    /// </summary>
    public override int GetHashCode()
    {
        return fontData.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of this font.
    /// </summary>
    public override string ToString()
    {
        if (IsDisposed)
            return "Font {disposed}";

        return $"Font {{{fontData}}}";
    }
}
