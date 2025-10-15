# SWTSharp Test Suite

This directory contains the comprehensive test suite for SWTSharp, a cross-platform .NET wrapper for the Standard Widget Toolkit (SWT).

## Table of Contents

- [Running Tests](#running-tests)
- [Platform-Specific Testing](#platform-specific-testing)
- [Test Infrastructure](#test-infrastructure)
- [Writing Tests](#writing-tests)
- [CI/CD](#cicd)
- [Troubleshooting](#troubleshooting)

## Running Tests

### Quick Start

**Windows:**
```bash
dotnet run --project tests/SWTSharp.Tests
```

**Linux:**
```bash
# Ensure X11 is available
export DISPLAY=:0
dotnet run --project tests/SWTSharp.Tests
```

**macOS:**
```bash
# MUST use the custom test runner (not 'dotnet test')
dotnet run --project tests/SWTSharp.Tests
```

### Why Not `dotnet test`?

On **macOS**, SWTSharp requires UI operations to run on **Thread 1** (the main thread), which is a requirement of the macOS Cocoa framework. The standard `dotnet test` command runs tests in a library mode that doesn't give us control over Thread 1.

**Solution:** We provide a custom test runner (`Program.cs`) that:
1. Initializes a dispatcher on Thread 1
2. Runs xUnit tests on a background thread
3. Routes all UI operations to Thread 1

On **Windows** and **Linux**, you can use either approach, but the custom runner is recommended for consistency.

## Platform-Specific Testing

### Test Attributes

SWTSharp provides platform-specific test attributes to control test execution:

```csharp
// Windows-only tests
[WindowsFact]
public void TestWindowsFeature() { }

[WindowsTheory]
[InlineData(1, 2)]
public void TestWindowsTheory(int a, int b) { }

// Linux-only tests
[LinuxFact]
public void TestLinuxFeature() { }

// macOS-only tests
[MacOSFact]
public void TestMacOSFeature() { }

// Skip on specific platforms
[FactSkipPlatform("macOS", "Linux")]
public void TestWindowsOnlyFeature() { }

// Run only on specific platforms
[FactOnlyPlatform("Windows", "Linux")]
public void TestNonMacOSFeature() { }
```

### Test Collections

Tests are organized into platform-specific collections:

```csharp
// Windows-specific tests
[Collection("Windows Tests")]
public class MyWindowsTests : TestBase
{
    public MyWindowsTests(DisplayFixture fixture) : base(fixture) { }
}

// Cross-platform tests (default)
[Collection("Display Tests")]
public class MyCrossPlatformTests : TestBase
{
    public MyCrossPlatformTests(DisplayFixture fixture) : base(fixture) { }
}
```

Available collections:
- `"Display Tests"` - Default, cross-platform tests
- `"Windows Tests"` - Windows-specific tests
- `"Linux Tests"` - Linux-specific tests
- `"macOS Tests"` - macOS-specific tests
- `"Cross-Platform Tests"` - Explicitly cross-platform

## Test Infrastructure

### Base Classes

**TestBase:**
The base class for all widget tests, providing:
- Shared `Display` instance
- `RunOnUIThread()` helpers
- `CreateTestShell()` helper
- Automatic cleanup

```csharp
public class MyWidgetTests : TestBase
{
    public MyWidgetTests(DisplayFixture fixture) : base(fixture) { }

    [Fact]
    public void TestWidget()
    {
        var shell = CreateTestShell();
        RunOnUIThread(() =>
        {
            // Your test code here
        });
    }
}
```

**DisplayFixture:**
Shared fixture that creates a single Display instance for all tests. On macOS, ensures Display is created on Thread 1.

### Test Helpers

**TestHelpers** provides utilities for common operations:

```csharp
// Create widgets
var shell = TestHelpers.CreateTestShell(Display);
var button = TestHelpers.CreateTestButton(shell, "Click Me");
var label = TestHelpers.CreateTestLabel(shell, "Hello");

// Assert disposal
TestHelpers.AssertDisposed(widget);
TestHelpers.AssertNotDisposed(widget);

// Event testing
TestHelpers.AssertEventFired(
    h => button.Click += h,
    h => button.Click -= h,
    () => button.NotifyListeners(SWT.Selection, new Event()),
    args => Assert.NotNull(args)
);

// Wait for conditions
TestHelpers.AssertCondition(
    () => widget.IsDisposed,
    "Widget should be disposed within timeout"
);

// Measure performance
var elapsed = TestHelpers.MeasureUITime(Display, () =>
{
    // Code to measure
});
```

## Writing Tests

### Example: Basic Widget Test

```csharp
[Collection("Display Tests")]
public class ButtonTests : TestBase
{
    public ButtonTests(DisplayFixture fixture) : base(fixture) { }

    [Fact]
    public void Button_Creation_ShouldSucceed()
    {
        var shell = CreateTestShell();
        Button? button = null;

        RunOnUIThread(() =>
        {
            button = new Button(shell, SWT.PUSH);
            button.Text = "Test Button";
        });

        Assert.NotNull(button);
        Assert.False(button.IsDisposed);
        Assert.Equal("Test Button", button.Text);
    }
}
```

### Example: Platform-Specific Test

```csharp
[Collection("macOS Tests")]
public class MacOSSpecificTests : TestBase
{
    public MacOSSpecificTests(DisplayFixture fixture) : base(fixture) { }

    [MacOSFact]
    public void MacOS_Specific_Feature_ShouldWork()
    {
        // This test only runs on macOS
        var shell = CreateTestShell();
        RunOnUIThread(() =>
        {
            // macOS-specific testing
        });
    }

    [FactSkipPlatform("macOS")]
    public void Feature_Not_Available_On_MacOS()
    {
        // This test runs on Windows and Linux only
    }
}
```

### Example: Event Testing

```csharp
[Fact]
public void Button_Click_ShouldFireEvent()
{
    var shell = CreateTestShell();
    var button = TestHelpers.CreateTestButton(shell, "Click Me");

    bool eventFired = false;
    EventHandler<SelectionEventArgs> handler = (s, e) => eventFired = true;

    RunOnUIThread(() =>
    {
        button.Click += handler;
        button.NotifyListeners(SWT.Selection, new Event());
        button.Click -= handler;
    });

    Assert.True(eventFired);
}
```

## CI/CD

### GitHub Actions Workflow

Tests run automatically on:
- **Windows** (windows-latest)
- **Linux** (ubuntu-latest with Xvfb)
- **macOS** (macos-latest with custom runner)

See `.github/workflows/ci.yml` for the complete workflow.

### Platform Matrix

```yaml
jobs:
  test-windows:
    runs-on: windows-latest
    steps:
      - run: dotnet run --project tests/SWTSharp.Tests

  test-linux:
    runs-on: ubuntu-latest
    steps:
      - name: Setup Xvfb
        run: |
          sudo Xvfb :99 -screen 0 1024x768x24 &
          echo "DISPLAY=:99" >> $GITHUB_ENV
      - run: dotnet run --project tests/SWTSharp.Tests

  test-macos:
    runs-on: macos-latest
    steps:
      - run: dotnet run --project tests/SWTSharp.Tests
```

### Coverage Reports

Test coverage is collected using Coverlet and uploaded to Codecov:
- **Windows**: `flags: windows`
- **Linux**: `flags: linux`
- **macOS**: `flags: macos`

## Troubleshooting

### macOS: "Tests must run through custom test runner"

**Problem:** You used `dotnet test` on macOS.

**Solution:** Use the custom runner:
```bash
dotnet run --project tests/SWTSharp.Tests
```

### Linux: "Cannot open display"

**Problem:** No X11 server is running.

**Solution:** Start Xvfb:
```bash
Xvfb :99 -screen 0 1024x768x24 &
export DISPLAY=:99
dotnet run --project tests/SWTSharp.Tests
```

### Windows: Tests hang

**Problem:** UI thread deadlock.

**Solution:** Ensure all widget operations are wrapped in `RunOnUIThread()`:
```csharp
RunOnUIThread(() =>
{
    // All widget operations here
});
```

### Tests fail with "Widget is disposed"

**Problem:** Widget was disposed before assertions.

**Solution:** Keep references to widgets and dispose them explicitly:
```csharp
var shell = CreateTestShell();
try
{
    // Your test code
}
finally
{
    if (!shell.IsDisposed)
    {
        shell.Dispose();
    }
}
```

### Platform-specific test runs on wrong platform

**Problem:** Used wrong attribute or collection.

**Solution:** Use platform-specific attributes:
```csharp
[MacOSFact] // Only runs on macOS
[WindowsFact] // Only runs on Windows
[LinuxFact] // Only runs on Linux
```

## Best Practices

1. **Always use TestBase** - Inherit from `TestBase` for automatic Display setup and cleanup
2. **Wrap UI operations** - Use `RunOnUIThread()` for all widget operations
3. **Use helper methods** - Leverage `TestHelpers` for common operations
4. **Platform-specific attributes** - Use `[WindowsFact]`, `[LinuxFact]`, `[MacOSFact]` for platform-specific tests
5. **Clean up resources** - Dispose widgets explicitly or use `using` statements
6. **Test disposal** - Always verify widgets are disposed properly
7. **Event testing** - Use `TestHelpers.AssertEventFired()` for event verification
8. **Timeout assertions** - Use `TestHelpers.AssertCondition()` for async operations

## Contributing

When adding new tests:
1. Choose the appropriate base class (`TestBase`)
2. Use the correct collection attribute
3. Add platform-specific attributes if needed
4. Include XML documentation
5. Follow existing patterns for consistency
6. Run tests on all platforms before submitting PR

## Resources

- [xUnit Documentation](https://xunit.net/)
- [SWTSharp API Documentation](../README.md)
- [CI/CD Workflow](.github/workflows/ci.yml)
- [Test Infrastructure](Infrastructure/)
