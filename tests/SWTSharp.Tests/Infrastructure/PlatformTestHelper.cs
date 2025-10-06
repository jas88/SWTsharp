using System.Runtime.InteropServices;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Helper class for platform-specific test execution and skipping.
/// </summary>
public static class PlatformTestHelper
{
    /// <summary>
    /// Gets the current platform name.
    /// </summary>
    public static string CurrentPlatform
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";

            return "Unknown";
        }
    }

    /// <summary>
    /// Checks if the current platform is Windows.
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Checks if the current platform is macOS.
    /// </summary>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Checks if the current platform is Linux.
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Checks if test should be skipped on non-Windows platforms.
    /// </summary>
    public static bool ShouldSkipIfNotWindows()
    {
        return !IsWindows;
    }

    /// <summary>
    /// Checks if test should be skipped on non-macOS platforms.
    /// </summary>
    public static bool ShouldSkipIfNotMacOS()
    {
        return !IsMacOS;
    }

    /// <summary>
    /// Checks if test should be skipped on non-Linux platforms.
    /// </summary>
    public static bool ShouldSkipIfNotLinux()
    {
        return !IsLinux;
    }

    /// <summary>
    /// Checks if test should be skipped in headless CI environment.
    /// </summary>
    public static bool ShouldSkipIfHeadless()
    {
        var isCI = Environment.GetEnvironmentVariable("CI")?.ToLower() == "true";
        var hasDisplay = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

        return isCI && !hasDisplay && IsLinux;
    }

    /// <summary>
    /// Gets the platform-specific SWT platform constant.
    /// </summary>
    public static string GetSWTPlatform()
    {
        if (IsWindows) return SWT.PLATFORM_WIN32;
        if (IsMacOS) return SWT.PLATFORM_MACOSX;
        if (IsLinux) return SWT.PLATFORM_LINUX;
        throw new PlatformNotSupportedException();
    }
}
