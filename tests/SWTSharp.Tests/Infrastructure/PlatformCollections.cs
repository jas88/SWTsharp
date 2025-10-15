using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Collection for Windows-specific tests.
/// Tests in this collection run serially to avoid threading issues.
/// </summary>
[CollectionDefinition("Windows Tests", DisableParallelization = true)]
public class WindowsTestCollection : ICollectionFixture<DisplayFixture>
{
}

/// <summary>
/// Collection for Linux-specific tests.
/// Tests in this collection run serially to avoid threading issues.
/// </summary>
[CollectionDefinition("Linux Tests", DisableParallelization = true)]
public class LinuxTestCollection : ICollectionFixture<DisplayFixture>
{
}

/// <summary>
/// Collection for macOS-specific tests.
/// Tests in this collection run serially to avoid threading issues with Thread 1 requirements.
/// </summary>
[CollectionDefinition("macOS Tests", DisableParallelization = true)]
public class MacOSTestCollection : ICollectionFixture<DisplayFixture>
{
}

/// <summary>
/// Collection for cross-platform tests that work on all platforms.
/// Tests in this collection run serially due to Display singleton requirements.
/// </summary>
[CollectionDefinition("Cross-Platform Tests", DisableParallelization = true)]
public class CrossPlatformTestCollection : ICollectionFixture<DisplayFixture>
{
}
