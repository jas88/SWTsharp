using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Skip tests on macOS CI that require UI thread operations.
/// macOS requires NSWindow creation on Thread 1, which is not available
/// in VSTest library mode used by CI.
/// Tests are NOT skipped when SWTSHARP_USE_CUSTOM_ADAPTER=1 is set,
/// indicating the custom test adapter is being used.
/// </summary>
public class FactSkipOnMacOSCI : FactAttribute
{
    public FactSkipOnMacOSCI()
    {
        // Only skip if on macOS AND in CI AND NOT using custom adapter
        var useCustomAdapter = Environment.GetEnvironmentVariable("SWTSHARP_USE_CUSTOM_ADAPTER") == "1";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) &&
            !useCustomAdapter)
        {
            Skip = "macOS UI tests require Thread 1 (not available in CI VSTest library mode). Set SWTSHARP_USE_CUSTOM_ADAPTER=1 to use custom adapter.";
        }
    }
}

/// <summary>
/// Skip tests on macOS CI that require UI thread operations.
/// macOS requires NSWindow creation on Thread 1, which is not available
/// in VSTest library mode used by CI.
/// Tests are NOT skipped when SWTSHARP_USE_CUSTOM_ADAPTER=1 is set,
/// indicating the custom test adapter is being used.
/// </summary>
public class TheorySkipOnMacOSCI : TheoryAttribute
{
    public TheorySkipOnMacOSCI()
    {
        // Only skip if on macOS AND in CI AND NOT using custom adapter
        var useCustomAdapter = Environment.GetEnvironmentVariable("SWTSHARP_USE_CUSTOM_ADAPTER") == "1";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) &&
            !useCustomAdapter)
        {
            Skip = "macOS UI tests require Thread 1 (not available in CI VSTest library mode). Set SWTSHARP_USE_CUSTOM_ADAPTER=1 to use custom adapter.";
        }
    }
}
