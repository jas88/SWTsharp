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
        // All platform implementations compiled into binary
        // Runtime OS detection selects correct implementation

        // Diagnostic logging
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
        {
            Console.WriteLine($"[PlatformFactory] Detecting platform...");
            Console.WriteLine($"[PlatformFactory] OS: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"[PlatformFactory] Architecture: {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"[PlatformFactory] Framework: {RuntimeInformation.FrameworkDescription}");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (enableLogging)
                Console.WriteLine($"[PlatformFactory] Creating Win32Platform");
            return new Win32Platform();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (enableLogging)
                Console.WriteLine($"[PlatformFactory] Creating MacOSPlatform");
            return new MacOSPlatform();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (enableLogging)
                Console.WriteLine($"[PlatformFactory] Creating LinuxPlatform");
            return new LinuxPlatform();
        }

        throw new PlatformNotSupportedException($"Platform not supported: {RuntimeInformation.OSDescription}");
    }
}
