using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformImage that adapts the existing Image class.
/// This serves as a bridge between the new platform widget system and the existing Image class.
/// </summary>
internal class MacOSImage : IPlatformImage
{
    private readonly Image _image;
    private bool _disposed;

    public MacOSImage(Image image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
    }

    public int Width => _image.Width;

    public int Height => _image.Height;

    public IntPtr GetNativeHandle()
    {
        if (_disposed) return IntPtr.Zero;

        // Return the underlying handle from the existing Image class
        return _image.Handle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Don't dispose the underlying Image as it may be shared
            // This adapter just references the existing Image
            _disposed = true;
        }
    }
}