using SWTSharp.Platform;

namespace SWTSharp.Graphics;

/// <summary>
/// Abstract base class for devices (Display, Printer, etc.).
/// A Device represents a graphics output device and manages device-specific resources.
/// </summary>
public abstract class Device : IDisposable
{
    private bool disposed;
    private readonly List<Resource> resources = new();

    /// <summary>
    /// Gets the platform-specific implementation.
    /// </summary>
    internal IPlatform Platform { get; }

    /// <summary>
    /// Gets whether this device has been disposed.
    /// </summary>
    public bool IsDisposed => disposed;

    /// <summary>
    /// Initializes a new instance of the Device class.
    /// </summary>
    /// <param name="platform">Platform-specific implementation</param>
    protected Device(IPlatform platform)
    {
        Platform = platform ?? throw new ArgumentNullException(nameof(platform));
    }

    /// <summary>
    /// Tracks a resource created on this device.
    /// </summary>
    internal void TrackResource(Resource resource)
    {
        if (resource == null) throw new ArgumentNullException(nameof(resource));
        lock (resources)
        {
            resources.Add(resource);
        }
    }

    /// <summary>
    /// Untracks a resource that has been disposed.
    /// </summary>
    internal void UntrackResource(Resource resource)
    {
        if (resource == null) return;
        lock (resources)
        {
            resources.Remove(resource);
        }
    }

    /// <summary>
    /// Checks if the device has been disposed and throws an exception if it has.
    /// </summary>
    protected void CheckDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// Disposes all resources created on this device.
    /// </summary>
    protected void DisposeResources()
    {
        Resource[] resourcesToDispose;
        lock (resources)
        {
            resourcesToDispose = resources.ToArray();
            resources.Clear();
        }

        foreach (var resource in resourcesToDispose)
        {
            if (!resource.IsDisposed)
            {
                resource.Dispose();
            }
        }
    }

    /// <summary>
    /// Disposes the device and all its resources.
    /// </summary>
    public void Dispose()
    {
        if (disposed) return;

        Dispose(true);
        System.GC.SuppressFinalize(this);
        disposed = true;
    }

    /// <summary>
    /// Disposes the device and optionally disposes managed resources.
    /// </summary>
    /// <param name="disposing">True to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeResources();
        }
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~Device()
    {
        Dispose(false);
    }
}
