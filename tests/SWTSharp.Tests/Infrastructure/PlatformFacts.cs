using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Fact attribute that only runs on Windows.
/// Skips on Linux and macOS.
/// </summary>
public sealed class WindowsFactAttribute : FactAttribute
{
    public WindowsFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = $"Windows-only test. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Fact attribute that only runs on Linux.
/// Skips on Windows and macOS.
/// </summary>
public sealed class LinuxFactAttribute : FactAttribute
{
    public LinuxFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Skip = $"Linux-only test. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Fact attribute that only runs on macOS.
/// Skips on Windows and Linux.
/// </summary>
public sealed class MacOSFactAttribute : FactAttribute
{
    public MacOSFactAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Skip = $"macOS-only test. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Theory attribute that only runs on Windows.
/// Skips on Linux and macOS.
/// </summary>
public sealed class WindowsTheoryAttribute : TheoryAttribute
{
    public WindowsTheoryAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Skip = $"Windows-only test. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Theory attribute that only runs on Linux.
/// Skips on Windows and macOS.
/// </summary>
public sealed class LinuxTheoryAttribute : TheoryAttribute
{
    public LinuxTheoryAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Skip = $"Linux-only test. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Theory attribute that only runs on macOS.
/// Skips on Windows and Linux.
/// </summary>
public sealed class MacOSTheoryAttribute : TheoryAttribute
{
    public MacOSTheoryAttribute()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Skip = $"macOS-only test. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Fact attribute that skips on specified platforms.
/// </summary>
public sealed class FactSkipPlatformAttribute : FactAttribute
{
    public FactSkipPlatformAttribute(params string[] platforms)
    {
        foreach (var platform in platforms)
        {
            var shouldSkip = platform.ToLowerInvariant() switch
            {
                "windows" => RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                "linux" => RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                "macos" or "osx" => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                _ => false
            };

            if (shouldSkip)
            {
                Skip = $"Test skipped on {platform}. Current platform: {PlatformTestHelper.CurrentPlatform}";
                break;
            }
        }
    }
}

/// <summary>
/// Theory attribute that skips on specified platforms.
/// </summary>
public sealed class TheorySkipPlatformAttribute : TheoryAttribute
{
    public TheorySkipPlatformAttribute(params string[] platforms)
    {
        foreach (var platform in platforms)
        {
            var shouldSkip = platform.ToLowerInvariant() switch
            {
                "windows" => RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                "linux" => RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                "macos" or "osx" => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                _ => false
            };

            if (shouldSkip)
            {
                Skip = $"Test skipped on {platform}. Current platform: {PlatformTestHelper.CurrentPlatform}";
                break;
            }
        }
    }
}

/// <summary>
/// Fact attribute that only runs on specified platforms.
/// Skips on all other platforms.
/// </summary>
public sealed class FactOnlyPlatformAttribute : FactAttribute
{
    public FactOnlyPlatformAttribute(params string[] platforms)
    {
        bool shouldRun = false;
        foreach (var platform in platforms)
        {
            shouldRun |= platform.ToLowerInvariant() switch
            {
                "windows" => RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                "linux" => RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                "macos" or "osx" => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                _ => false
            };
        }

        if (!shouldRun)
        {
            Skip = $"Test only runs on: {string.Join(", ", platforms)}. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}

/// <summary>
/// Theory attribute that only runs on specified platforms.
/// Skips on all other platforms.
/// </summary>
public sealed class TheoryOnlyPlatformAttribute : TheoryAttribute
{
    public TheoryOnlyPlatformAttribute(params string[] platforms)
    {
        bool shouldRun = false;
        foreach (var platform in platforms)
        {
            shouldRun |= platform.ToLowerInvariant() switch
            {
                "windows" => RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                "linux" => RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                "macos" or "osx" => RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                _ => false
            };
        }

        if (!shouldRun)
        {
            Skip = $"Test only runs on: {string.Join(", ", platforms)}. Current platform: {PlatformTestHelper.CurrentPlatform}";
        }
    }
}
