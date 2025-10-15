# Testing Strategy for SWTSharp

## Test Categories

SWTSharp tests are categorized using xUnit `[Trait]` attributes to enable selective execution:

### GUI Tests (`Category=GUI`)
- Require windowing environment and display connection
- Include all widget tests (Button, Label, Shell, etc.)
- Must run with custom test runner: `dotnet run --project tests/SWTSharp.Tests`
- **Cannot run in CI headless environments**
- Run locally on developer machines

### Non-GUI Tests (`Category=NonGUI`)
- Can run in headless CI environments
- Include infrastructure tests (GCDThreadingTests, MacOSRunnerTests)
- Don't create widgets or require display connection
- Run in CI using xUnit filter: `dotnet test --filter "Category!=GUI"`

## Running Tests

### Locally (Full Test Suite)
```bash
# macOS - uses custom test runner with Thread 1 dispatch
dotnet run --project tests/SWTSharp.Tests

# Windows/Linux - standard test runner
dotnet test
```

### CI (Non-GUI Tests Only)
```bash
# Filters out GUI tests that require windowing
dotnet test --filter "Category!=GUI"
```

## Why GUI Tests Don't Run in CI

macOS GUI tests have unique requirements that GitHub Actions headless runners cannot provide:

1. **Thread 1 NSApplication Run Loop** - GUI operations must execute on Thread 1 with active `CFRunLoop`
2. **Display Connection** - NSWindow/NSButton require connection to window server
3. **Event Processing** - User interactions need window system event loop

Our custom test runner (`Program.cs`) provides Thread 1 dispatch via `MainThreadDispatcher`, but CI headless environments lack the display connection required for actual GUI operations.

### What Happens Without Display Connection
- Window creation hangs or fails silently
- GUI operations are queued but never processed
- Tests timeout after waiting for operations to complete
- Previous CI runs hung for 75+ minutes without completion

## Platform-Specific Notes

### macOS
- GUI tests: Require custom runner (`dotnet run --project`)
- Non-GUI tests: Can use standard runner (`dotnet test`)
- CI: Non-GUI tests only

### Windows
- All tests: Standard runner (`dotnet test`)
- CI: Full test suite (headless Windows supports GUI test infrastructure)

### Linux
- GUI tests: Require Xvfb virtual display
- Non-GUI tests: Standard runner
- CI: Full test suite with Xvfb configuration

## Test Infrastructure

### DisplayFixture
- Shared xUnit fixture providing `Display` singleton
- Throws exception on macOS if `MainThreadDispatcher` not initialized
- Ensures proper Thread 1 dispatch for GUI operations

### WidgetTestBase
- Base class for all widget tests
- Marked with `[Trait("Category", "GUI")]`
- Provides helper methods: `RunOnUIThread()`, `CreateTestShell()`, etc.

### TestBase
- Base class for all tests (GUI and non-GUI)
- Provides platform detection and common utilities

## Adding New Tests

### For Widget Tests
```csharp
// Automatically inherits Category=GUI trait from WidgetTestBase
public class MyWidgetTests : WidgetTestBase
{
    public MyWidgetTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void MyWidget_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new MyWidget(shell, SWT.NONE));
    }
}
```

### For Non-GUI Tests
```csharp
// Explicitly mark as NonGUI
[Trait("Category", "NonGUI")]
public class MyInfrastructureTests
{
    [Fact]
    public void MyTest_ShouldPass()
    {
        // No DisplayFixture needed
        // No GUI operations
        Assert.True(true);
    }
}
```

## CI Configuration

See `.github/workflows/ci.yml` for platform-specific test execution:

- **Windows**: Full test suite (GUI + Non-GUI)
- **Linux**: Full test suite with Xvfb
- **macOS**: Non-GUI tests only (`--filter "Category!=GUI"`)
