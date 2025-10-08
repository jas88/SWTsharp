using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Skip tests on macOS CI that require UI thread operations.
/// macOS requires NSWindow creation on Thread 1, which is not available
/// in VSTest library mode used by CI.
/// </summary>
public class FactSkipOnMacOSCI : FactAttribute
{
    public FactSkipOnMacOSCI()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
        {
            Skip = "macOS UI tests require Thread 1 (not available in CI VSTest). Use custom adapter for full macOS testing.";
        }
    }
}

/// <summary>
/// Skip tests on macOS CI that require UI thread operations.
/// macOS requires NSWindow creation on Thread 1, which is not available
/// in VSTest library mode used by CI.
/// </summary>
public class TheorySkipOnMacOSCI : TheoryAttribute
{
    public TheorySkipOnMacOSCI()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
        {
            Skip = "macOS UI tests require Thread 1 (not available in CI VSTest). Use custom adapter for full macOS testing.";
        }
    }
}
