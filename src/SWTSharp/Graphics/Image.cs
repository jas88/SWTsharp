using SWTSharp.Platform;

namespace SWTSharp.Graphics;

/// <summary>
/// Device-specific image resource.
/// Represents a bitmap image that has been allocated on a specific device.
/// Images must be explicitly disposed when no longer needed.
/// </summary>
public class Image : Resource
{
    private readonly int width;
    private readonly int height;

    /// <summary>
    /// Gets the bounds of the image.
    /// </summary>
    public Rectangle Bounds
    {
        get
        {
            CheckDisposed();
            return new Rectangle(0, 0, width, height);
        }
    }

    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    public int Width
    {
        get
        {
            CheckDisposed();
            return width;
        }
    }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    public int Height
    {
        get
        {
            CheckDisposed();
            return height;
        }
    }

    /// <summary>
    /// Initializes a new instance of the Image class with specified dimensions.
    /// </summary>
    /// <param name="device">The device to create the image on</param>
    /// <param name="width">Image width in pixels</param>
    /// <param name="height">Image height in pixels</param>
    public Image(Device device, int width, int height)
        : base(device)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be positive", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be positive", nameof(height));

        this.width = width;
        this.height = height;

        Handle = CreatePlatformImage(width, height);
    }

    /// <summary>
    /// Initializes a new instance of the Image class from a file.
    /// </summary>
    /// <param name="device">The device to create the image on</param>
    /// <param name="filename">Path to the image file</param>
    public Image(Device device, string filename)
        : base(device)
    {
        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException("Filename cannot be null or empty", nameof(filename));

        if (!File.Exists(filename))
            throw new FileNotFoundException("Image file not found", filename);

        var result = CreatePlatformImageFromFile(filename);
        Handle = result.Handle;
        this.width = result.Width;
        this.height = result.Height;
    }

    /// <summary>
    /// Creates a blank platform-specific image.
    /// </summary>
    private IntPtr CreatePlatformImage(int width, int height)
    {
        var platformGraphics = Device.Platform as IPlatformGraphics;
        if (platformGraphics == null)
        {
            throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Platform does not support graphics operations");
        }

        return platformGraphics.CreateImage(width, height);
    }

    /// <summary>
    /// Creates a platform-specific image from a file.
    /// </summary>
    private (IntPtr Handle, int Width, int Height) CreatePlatformImageFromFile(string filename)
    {
        var platformGraphics = Device.Platform as IPlatformGraphics;
        if (platformGraphics == null)
        {
            throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Platform does not support graphics operations");
        }

        return platformGraphics.LoadImage(filename);
    }

    /// <summary>
    /// Releases the platform-specific image handle.
    /// </summary>
    protected override void ReleaseHandle()
    {
        if (Handle == IntPtr.Zero) return;

        var platformGraphics = Device.Platform as IPlatformGraphics;
        platformGraphics?.DestroyImage(Handle);
    }

    /// <summary>
    /// Returns a string representation of this image.
    /// </summary>
    public override string ToString()
    {
        if (IsDisposed)
            return "Image {disposed}";

        return $"Image {{width={width}, height={height}}}";
    }
}
