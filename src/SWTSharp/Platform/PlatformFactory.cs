using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Factory for creating platform-specific implementations.
/// </summary>
internal static class PlatformFactory
{
    private static IPlatform? _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets the platform-specific implementation.
    /// </summary>
    public static IPlatform Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = CreatePlatform();
                    }
                }
            }
            return _instance;
        }
    }

    private static IPlatform CreatePlatform()
    {
        #if WINDOWS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new Win32Platform();
        }
        #endif

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOSPlatform();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new LinuxPlatform();
        }

        throw new PlatformNotSupportedException($"Platform not supported: {RuntimeInformation.OSDescription}");
    }
}
