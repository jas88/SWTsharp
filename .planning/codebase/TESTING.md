# Testing Patterns

**Analysis Date:** 2026-01-29

## Test Framework

**Runner:**
- xUnit 2.9.3
- Config: Custom test runner via `SWTSharp.TestHost`
- Custom VSTest adapter: `SWTSharp.TestAdapter` (netstandard2.0)

**Assertion Library:**
- xUnit assertions: `Assert.True()`, `Assert.False()`, `Assert.Equal()`, `Assert.NotNull()`, `Assert.Same()`, `Assert.Throws<T>()`
- No external assertion library (FluentAssertions, etc.)

**Run Commands:**
```bash
# Custom test runner (required for macOS)
dotnet run --project tests/SWTSharp.Tests

# Standard dotnet test (Windows/Linux only)
dotnet test

# Specific test class
dotnet test --filter "ClassName"
```

## Test File Organization

**Location:**
- Co-located in `/tests/SWTSharp.Tests/` directory
- Namespace mirrors source structure: `SWTSharp.Tests.Widgets`, `SWTSharp.Tests.Dialogs`, `SWTSharp.Tests.Platform`
- Infrastructure helpers in: `/tests/SWTSharp.Tests/Infrastructure/`

**Naming:**
- Test files: `{Widget}Tests.cs` (example: `ButtonTests.cs`, `TextTests.cs`)
- Helper files: `{Helper}TestHelper.cs` or `{Helper}Base.cs`
- Platform-specific: `{Platform}{Widget}Tests.cs` (example: `WindowsButtonTests.cs`, `MacOSTextTests.cs`)

**Structure:**
```
tests/SWTSharp.Tests/
├── Infrastructure/           # Test support
│   ├── WidgetTestBase.cs     # Base class for widget tests
│   ├── TestBase.cs           # Base class for all tests
│   ├── DisplayCollection.cs  # xUnit collection definition
│   ├── DisplayFixture.cs     # Shared Display fixture
│   ├── PlatformTestHelper.cs # Platform detection utilities
│   ├── FactSkipOnMacOSCI.cs  # Custom test attributes
│   └── ...
├── Widgets/                  # Widget-specific tests
│   ├── ButtonTests.cs
│   ├── TextTests.cs
│   ├── CompositeTests.cs
│   └── ...
├── Dialogs/                  # Dialog tests
│   ├── MessageBoxTests.cs
│   ├── FileDialogTests.cs
│   └── ...
├── Platform/                 # Platform-specific tests
│   ├── WindowsButtonTests.cs
│   ├── MacOSTextTests.cs
│   └── ...
└── Program.cs               # Custom test runner entry point
```

## Test Structure

**Suite Organization:**
```csharp
// From ButtonTests.cs
[Trait("Category", "GUI")]
public class ButtonTests : WidgetTestBase
{
    public ButtonTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Button_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Button(shell, SWT.PUSH));
    }
}
```

**Patterns:**

1. **Class Setup:**
   - Inherit from `WidgetTestBase` (extends `TestBase`)
   - Constructor accepts `DisplayFixture` via xUnit dependency injection
   - Marked with `[Trait("Category", "GUI")]` for filtering

2. **Test Methods:**
   - All public, marked with `[Fact]`
   - Naming: `{WidgetName}_{Feature}_{ExpectedOutcome}()`
   - Example: `Button_Create_ShouldSucceed()`, `Text_Append_AfterDispose_ShouldThrow()`

3. **Test Body:**
   - Wrapped in `RunOnUIThread(() => { })` for cross-platform thread safety
   - Create widget via factory method
   - Assert using test helper methods
   - Explicit disposal: `widget.Dispose()` or `using var shell = CreateTestShell()`

## Mocking

**Framework:** NSubstitute 5.3.0

**Patterns:**
```csharp
// From TestBase.cs
protected IPlatform MockPlatform { get; private set; } = null!;

protected TestBase(DisplayFixture displayFixture)
{
    Display = displayFixture.Display;

    // Create mock platform for testing
    MockPlatform = Substitute.For<IPlatform>();

    // Set up default mock behaviors
    SetupDefaultMockBehaviors();
}

protected virtual void SetupDefaultMockBehaviors()
{
    // Default mock behaviors can be set here
    // Platform-specific test classes can override this
}
```

**What to Mock:**
- Platform implementations: `IPlatform`, `IPlatformGraphics`, `IPlatformWidget`
- External dependencies (if any)
- Not mocked: Display, Shell, actual widget implementations (integration testing approach)

**What NOT to Mock:**
- Widget classes under test
- Display fixture (real instance shared across tests)
- Layout managers
- Event listeners
- Platform abstractions when testing platform-specific code (platform tests use real implementations)

## Fixtures and Factories

**Test Data:**
- Anonymous objects for arbitrary data: `widget.Data = new { Name = "Test", Value = 42 }`
- String constants for widget text tests: `"Click Me"`, `"Test input"`
- Style constants: `SWT.PUSH`, `SWT.CHECK`, `SWT.SINGLE`, `SWT.MULTI`

**Factory Pattern:**
```csharp
// From WidgetTestBase.cs
protected void AssertWidgetCreation<T>(Func<Shell, T> factory) where T : Widget
{
    RunOnUIThread(() =>
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);

        Assert.NotNull(widget);
        AssertNotDisposed(widget);
        Assert.Same(shell.Display, widget.Display);

        widget.Dispose();
        AssertDisposed(widget);
    });
}

// Usage in test
[Fact]
public void Button_Create_ShouldSucceed()
{
    AssertWidgetCreation(shell => new Button(shell, SWT.PUSH));
}
```

**Shared Fixture:**
- `DisplayFixture`: IDisposable, created once per xUnit collection, shared across all tests
- Ensures single Display instance across all tests
- Handles platform-specific initialization (macOS requires custom test runner)
- Location: `/tests/SWTSharp.Tests/Infrastructure/DisplayCollection.cs`

## Coverage

**Requirements:** Not enforced; coverage collector configured but no targets

**View Coverage:**
```bash
# With coverage collection
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Types

**Unit Tests:**
- Scope: Individual widget creation, property get/set, disposal
- Approach: Create widget with various style combinations, verify behavior
- Files: `tests/SWTSharp.Tests/Widgets/*.cs`
- Naming: `{Widget}Tests.cs`
- Example: `ButtonTests.cs` tests Button widget in isolation

**Integration Tests:**
- Scope: Multiple widgets working together, layout calculations, event propagation
- Approach: Create composite hierarchies, verify Display behavior
- Files: `tests/SWTSharp.Tests/Widgets/CompositeTests.cs`, layout tests
- Platform-specific: `tests/SWTSharp.Tests/Platform/*Tests.cs`

**Platform-Specific Tests:**
- Scope: Verify correct platform implementations for each OS
- Approach: Test platform-specific behavior (Win32 API, GObject/GTK, Cocoa)
- Files: `tests/SWTSharp.Tests/Platform/Windows*.cs`, `MacOS*.cs`, `Linux*.cs`
- Example: `WindowsButtonTests.cs` tests Win32-specific Button behavior

**E2E Tests:**
- Framework: Not used in traditional sense; custom test runner (`Program.cs`) serves as minimal E2E
- Approach: Tests run through actual Display/Shell hierarchy with platform initialization
- Key distinction: No separate E2E phase; all tests are integration-level

## Common Patterns

**Async Testing:**
```csharp
// From TestBase.cs
protected void RunOnUIThread(Action action)
{
    Display.SyncExec(action);
}

protected T RunOnUIThread<T>(Func<T> func)
{
    T? result = default;
    Display.SyncExec(() =>
    {
        result = func();
    });
    return result!;
}

// Usage in test
[Fact]
public void Button_Selection_Check_ShouldGetAndSet()
{
    RunOnUIThread(() =>
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.CHECK);

        Assert.False(button.Selection);
        button.Selection = true;
        Assert.True(button.Selection);

        button.Dispose();
    });
}
```

**Error Testing:**
```csharp
// From WidgetTestBase.cs
protected void AssertThrowsAfterDisposal<T>(Func<Shell, T> factory, Action<T> operation) where T : Widget
{
    RunOnUIThread(() =>
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);
        widget.Dispose();

        Assert.Throws<SWTDisposedException>(() => operation(widget));
    });
}

// Usage in test
[Fact]
public void Button_SetText_AfterDispose_ShouldThrow()
{
    AssertThrowsAfterDisposal(
        shell => new Button(shell, SWT.PUSH),
        b => b.Text = "Test"
    );
}
```

**Property Testing:**
```csharp
// From WidgetTestBase.cs
protected void AssertPropertyGetSet<T, TProp>(
    Func<Shell, T> factory,
    Func<T, TProp> getter,
    Action<T, TProp> setter,
    TProp testValue) where T : Widget
{
    RunOnUIThread(() =>
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);

        setter(widget, testValue);
        var actualValue = getter(widget);

        Assert.Equal(testValue, actualValue);

        widget.Dispose();
    });
}

// Usage in test
[Fact]
public void Button_Text_ShouldGetAndSet()
{
    AssertPropertyGetSet(
        shell => new Button(shell, SWT.PUSH),
        b => b.Text,
        (b, v) => b.Text = v,
        "Click Me"
    );
}
```

**Multi-Style Testing:**
```csharp
// From WidgetTestBase.cs
protected void AssertWidgetStyles<T>(Func<Shell, int, T> factory, params int[] styles) where T : Widget
{
    RunOnUIThread(() =>
    {
        using var shell = CreateTestShell();

        foreach (var style in styles)
        {
            var widget = factory(shell, style);
            Assert.NotNull(widget);
            AssertNotDisposed(widget);
            widget.Dispose();
        }
    });
}

// Usage in test
[Fact]
public void Button_Create_WithStyles_ShouldSucceed()
{
    AssertWidgetStyles(
        (shell, style) => new Button(shell, style),
        SWT.PUSH,
        SWT.CHECK,
        SWT.RADIO,
        SWT.TOGGLE,
        SWT.ARROW
    );
}
```

**Disposal Pattern:**
```csharp
// Always use 'using' statement for widgets
using var shell = CreateTestShell();
var button = new Button(shell, SWT.PUSH);
// ... test code ...
button.Dispose();  // Explicit when not using 'using'
```

## Collection Definitions and Traits

**Collection Isolation:**
```csharp
// From DisplayCollection.cs
[CollectionDefinition("Display Tests", DisableParallelization = true)]
public class DisplayCollection : ICollectionFixture<DisplayFixture>
{
}
```
- All tests marked with `[Collection("Display Tests")]` run serially
- Necessary because Display is a singleton tied to a single thread

**Test Traits:**
```csharp
[Trait("Category", "GUI")]
public class ButtonTests : WidgetTestBase
{
}
```
- Used to categorize tests for filtering
- `Category` trait values: "GUI", "Platform", etc.

## macOS Special Requirements

**Custom Test Runner:**
- macOS requires custom test runner: `SWTSharp.Tests.Program.Main()`
- Standard `dotnet test` does NOT work on macOS
- Runner uses `MainThreadDispatcher` to execute tests on Thread 1 (Cocoa requirement)
- Validation in `DisplayFixture` constructor throws if MainThreadDispatcher not initialized

**Invocation:**
```bash
# macOS: must use custom runner
dotnet run --project tests/SWTSharp.Tests

# Windows/Linux: can use standard test runner
dotnet test
```

---

*Testing analysis: 2026-01-29*
