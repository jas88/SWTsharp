using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux (GTK) implementation of IPlatformImage that adapts the existing Image class.
/// </summary>
internal class LinuxImage : IPlatformImage
{
    private readonly Image _image;
    private bool _disposed;

    public LinuxImage(Image image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
    }

    public int Width => _image.Width;

    public int Height => _image.Height;

    public IntPtr GetNativeHandle()
    {
        if (_disposed) return IntPtr.Zero;
        return _image.Handle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Don't dispose the underlying Image as it may be shared
            _disposed = true;
        }
    }
}
