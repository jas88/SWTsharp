namespace SWTSharp.Graphics;

/// <summary>
/// Abstract base class for graphics resources.
/// Resources are platform-specific objects that must be explicitly disposed.
/// </summary>
public abstract class Resource : IDisposable
{
    private bool disposed;

    /// <summary>
    /// Gets the device this resource was created on.
    /// </summary>
    public Device Device { get; }

    /// <summary>
    /// Gets whether this resource has been disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    /// <summary>
    /// Gets the platform-specific handle for this resource.
    /// </summary>
    internal IntPtr Handle { get; set; }

    /// <summary>
    /// Initializes a new instance of the Resource class.
    /// </summary>
    /// <param name="device">The device this resource is created on</param>
    protected Resource(Device device)
    {
        Device = device ?? throw new ArgumentNullException(nameof(device));
        Device.TrackResource(this);
    }

    /// <summary>
    /// Checks if the resource has been disposed and throws an exception if it has.
    /// </summary>
    protected void CheckDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Releases the platform-specific handle.
    /// </summary>
    protected abstract void ReleaseHandle();

    /// <summary>
    /// Disposes the resource.
    /// </summary>
    public void Dispose()
    {
        if (disposed) return;

        Dispose(true);
        System.GC.SuppressFinalize(this);
        disposed = true;
    }

    /// <summary>
    /// Disposes the resource and optionally disposes managed resources.
    /// </summary>
    /// <param name="disposing">True to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Handle != IntPtr.Zero)
        {
            ReleaseHandle();
            Handle = IntPtr.Zero;
        }

        if (disposing)
        {
            Device.UntrackResource(this);
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~Resource()
    {
        Dispose(false);
    }
}
