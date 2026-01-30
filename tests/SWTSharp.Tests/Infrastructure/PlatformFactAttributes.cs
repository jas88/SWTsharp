using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Fact attribute that only discovers tests on Windows.
/// Tests are NOT discovered (not skipped) on other platforms - they won't appear in results at all.
/// This prevents tests from being discovered that could crash when run on the wrong platform.
/// </summary>
/// <remarks>
/// Uses IXunitTestCaseDiscoverer pattern to return empty enumerable on non-target platforms.
/// This is different from Skip-based attributes which show skipped tests in results.
/// </remarks>
[XunitTestCaseDiscoverer("SWTSharp.Tests.Infrastructure.WindowsFactDiscoverer", "SWTSharp.Tests")]
public sealed class WindowsOnlyFactAttribute : FactAttribute
{
}

/// <summary>
/// Fact attribute that only discovers tests on macOS.
/// Tests are NOT discovered (not skipped) on other platforms - they won't appear in results at all.
/// This prevents tests from being discovered that could crash when run on the wrong platform.
/// </summary>
/// <remarks>
/// Uses IXunitTestCaseDiscoverer pattern to return empty enumerable on non-target platforms.
/// This is different from Skip-based attributes which show skipped tests in results.
/// </remarks>
[XunitTestCaseDiscoverer("SWTSharp.Tests.Infrastructure.MacOSFactDiscoverer", "SWTSharp.Tests")]
public sealed class MacOSOnlyFactAttribute : FactAttribute
{
}

/// <summary>
/// Fact attribute that only discovers tests on Linux.
/// Tests are NOT discovered (not skipped) on other platforms - they won't appear in results at all.
/// This prevents tests from being discovered that could crash when run on the wrong platform.
/// </summary>
/// <remarks>
/// Uses IXunitTestCaseDiscoverer pattern to return empty enumerable on non-target platforms.
/// This is different from Skip-based attributes which show skipped tests in results.
/// </remarks>
[XunitTestCaseDiscoverer("SWTSharp.Tests.Infrastructure.LinuxFactDiscoverer", "SWTSharp.Tests")]
public sealed class LinuxOnlyFactAttribute : FactAttribute
{
}

/// <summary>
/// Discoverer for WindowsOnlyFactAttribute.
/// Returns test cases only on Windows, empty enumerable on other platforms.
/// </summary>
public sealed class WindowsFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public WindowsFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Not on Windows - don't discover this test at all
            return Enumerable.Empty<IXunitTestCase>();
        }

        // On Windows - return the test case normally
        return new[]
        {
            new XunitTestCase(
                _diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod)
        };
    }
}

/// <summary>
/// Discoverer for MacOSOnlyFactAttribute.
/// Returns test cases only on macOS, empty enumerable on other platforms.
/// </summary>
public sealed class MacOSFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public MacOSFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Not on macOS - don't discover this test at all
            return Enumerable.Empty<IXunitTestCase>();
        }

        // On macOS - return the test case normally
        return new[]
        {
            new XunitTestCase(
                _diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod)
        };
    }
}

/// <summary>
/// Discoverer for LinuxOnlyFactAttribute.
/// Returns test cases only on Linux, empty enumerable on other platforms.
/// </summary>
public sealed class LinuxFactDiscoverer : IXunitTestCaseDiscoverer
{
    private readonly IMessageSink _diagnosticMessageSink;

    public LinuxFactDiscoverer(IMessageSink diagnosticMessageSink)
    {
        _diagnosticMessageSink = diagnosticMessageSink;
    }

    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Not on Linux - don't discover this test at all
            return Enumerable.Empty<IXunitTestCase>();
        }

        // On Linux - return the test case normally
        return new[]
        {
            new XunitTestCase(
                _diagnosticMessageSink,
                discoveryOptions.MethodDisplayOrDefault(),
                discoveryOptions.MethodDisplayOptionsOrDefault(),
                testMethod)
        };
    }
}
