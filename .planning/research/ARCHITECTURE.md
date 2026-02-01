# Architecture Research: Testing Patterns

## Executive Summary

This document defines the test architecture for SWTSharp, a cross-platform .NET GUI framework with platform abstraction. The testing strategy balances cross-platform tests using mocked platform interfaces with platform-specific integration tests that validate real native behavior.

**Key Finding:** SWTSharp uses a three-layer testing approach:
1. **Cross-platform unit tests** with mocked `IPlatform` interfaces
2. **Platform-specific integration tests** using custom xUnit attributes (`[WindowsFact]`, `[LinuxFact]`, `[MacOSFact]`)
3. **Custom test runner** for macOS Thread 1 requirements

## Test Project Structure

### Physical Organization

```
tests/
├── SWTSharp.Tests/              # Main test assembly
│   ├── Infrastructure/          # Test base classes and helpers
│   │   ├── TestBase.cs         # Base class with Display fixture
│   │   ├── WidgetTestBase.cs   # Widget-specific test patterns
│   │   ├── PlatformTestHelper.cs
│   │   ├── PlatformFacts.cs    # Custom xUnit attributes
│   │   └── TestHelpers.cs      # Static helper methods
│   ├── Widgets/                 # Cross-platform widget tests
│   │   ├── ButtonTests.cs      # Tests Button via public API
│   │   ├── TextTests.cs
│   │   ├── CompositeTests.cs
│   │   └── [Widget]Tests.cs
│   ├── Platform/                # Platform-specific tests
│   │   ├── WindowsButtonTests.cs
│   │   ├── MacOSButtonTests.cs
│   │   ├── LinuxButtonTests.cs
│   │   └── [Platform][Widget]Tests.cs
│   ├── Dialogs/                 # Dialog tests
│   ├── Examples/                # Test pattern examples
│   │   └── PlatformTestExamples.cs
│   └── Program.cs              # Custom test runner entry point
├── SWTSharp.TestHost/           # Test host infrastructure
└── SWTSharp.TestAdapter/        # Custom xUnit test adapter
```

### Logical Organization

**Component Boundaries:**
- **Infrastructure Layer:** Provides test base classes, fixtures, and platform detection
- **Widget Test Layer:** Cross-platform behavioral tests
- **Platform Test Layer:** Platform-specific implementation tests
- **Test Helpers:** Static utilities for common test operations

**Build Order:**
1. `SWTSharp` (library under test)
2. `SWTSharp.TestHost` (test infrastructure)
3. `SWTSharp.TestAdapter` (xUnit adapter)
4. `SWTSharp.Tests` (test assembly)

## Unit Test Patterns (Mocked Platform)

### Pattern 1: TestBase with Mock Platform

Cross-platform tests inherit from `TestBase` which provides:
- Shared `Display` instance via `DisplayFixture`
- Mock `IPlatform` via NSubstitute
- UI thread synchronization helpers
- Automatic cleanup

**Data Flow:**
```
Test Method
    ↓
RunOnUIThread(action)
    ↓
Display.SyncExec(action)
    ↓
Widget operations
    ↓
Mock IPlatform (NSubstitute)
```

**Example:**
```csharp
[Collection("Display Tests")]
public class ButtonTests : WidgetTestBase
{
    public ButtonTests(DisplayFixture fixture) : base(fixture) { }

    [Fact]
    public void Button_Create_ShouldSucceed()
    {
        // Uses helper method that wraps RunOnUIThread
        AssertWidgetCreation(shell => new Button(shell, SWT.PUSH));
    }

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
}
```

### Pattern 2: WidgetTestBase Helper Methods

`WidgetTestBase` provides common test patterns:
- `AssertWidgetCreation<T>()` - Verify creation and disposal
- `AssertControlParent<T>()` - Verify parent relationship
- `AssertPropertyGetSet<T, TProp>()` - Test property behavior
- `AssertWidgetStyles<T>()` - Test multiple style combinations
- `AssertThrowsAfterDisposal<T>()` - Verify disposed state exceptions

**Component Boundaries:**
- **TestBase:** Display management, thread synchronization
- **WidgetTestBase:** Widget-specific assertion patterns
- **Widget Test Classes:** Specific widget behavior

### Pattern 3: Mock Platform Setup

Custom mock behaviors per test class:

```csharp
protected override void SetupDefaultMockBehaviors()
{
    // Set up platform-specific mock responses
    MockPlatform.CreateButtonWidget(Arg.Any<IPlatformWidget?>(), Arg.Any<int>())
        .Returns(callInfo => Substitute.For<IPlatformTextWidget>());
}
```

## Integration Test Patterns (Real Platform)

### Pattern 4: Platform-Specific Tests with Custom Attributes

Platform-specific tests use custom xUnit attributes that skip based on `RuntimeInformation.IsOSPlatform()`:

**Available Attributes:**
- `[WindowsFact]` / `[WindowsTheory]` - Windows only
- `[LinuxFact]` / `[LinuxTheory]` - Linux only
- `[MacOSFact]` / `[MacOSTheory]` - macOS only
- `[FactSkipPlatform("Windows", "Linux")]` - Skip on specified platforms
- `[FactOnlyPlatform("Windows", "Linux")]` - Run only on specified platforms

**Data Flow:**
```
Test Discovery (xUnit)
    ↓
Attribute Skip Check (Platform Detection)
    ↓
Test Execution (if not skipped)
    ↓
Real IPlatform Implementation
    ↓
Native Win32/Cocoa/GTK3 APIs
```

**Example:**
```csharp
[Collection("Cross-Platform Tests")]
public class WindowsButtonTests : TestBase
{
    [WindowsFact]
    public void Button_PlatformWidget_ShouldBeWin32Button()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            // Verify platform widget is created
            Assert.NotNull(button.PlatformWidget);
            Assert.True(button.PlatformWidget is IPlatformTextWidget);
        });
    }
}
```

### Pattern 5: Platform-Specific Event Testing

macOS tests validate Objective-C runtime integration:

```csharp
[MacOSFact]
public void MacOSButton_Click_Event_ShouldFire()
{
    RunOnUIThread(() =>
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.PUSH);

        int clickCount = 0;
        button.Click += (sender, e) => clickCount++;

        // Get native NSButton handle via reflection
        var buttonHandle = GetNSButtonHandle(button);

        // Trigger action via objc_msgSend
        TriggerNSButtonClick(buttonHandle);

        Assert.Equal(1, clickCount);
    });
}
```

### Pattern 6: Collections for Platform Isolation

xUnit collections ensure platform-specific tests don't interfere:

```csharp
[Collection("Windows Tests")]
public class WindowsSpecificTests : TestBase { }

[Collection("Linux Tests")]
public class LinuxSpecificTests : TestBase { }

[Collection("macOS Tests")]
public class MacOSSpecificTests : TestBase { }

[Collection("Cross-Platform Tests")]  // Default
public class GeneralTests : TestBase { }
```

## Platform-Specific Test Considerations

### Windows (Win32)

**Characteristics:**
- Standard Win32 message loop
- No Thread 1 requirement
- COM interop for some widgets (Browser uses WebView2)

**Test Approach:**
- Direct Win32 API calls for verification
- Standard xUnit execution model
- Can use `dotnet test` or custom runner

**Key Tests:**
- Win32 handle creation
- Window message handling
- Common Controls v6 integration

### macOS (Cocoa)

**Characteristics:**
- **MUST run on Thread 1** (macOS requirement)
- Objective-C runtime interop
- Target/Action event pattern
- Grand Central Dispatch (GCD) for threading

**Test Approach:**
- **Custom test runner required** (`dotnet run`, not `dotnet test`)
- Display created on Thread 1 via `DisplayFixture`
- Tests run on background thread, UI ops dispatched to Thread 1
- Reflection to access native NSView handles for testing

**Key Tests:**
- ObjC runtime class creation
- Target/Action event routing
- GCD main queue execution
- Memory management (ARC integration)

**Critical Pattern:**
```csharp
// Custom runner in Program.cs
static void Main()
{
    // Ensure Thread 1 is available for macOS
    var dispatcher = new MacOSTestDispatcher();
    dispatcher.RunTests();
}
```

### Linux (GTK3)

**Characteristics:**
- GTK+ 3.x event loop
- GObject signal system
- Requires X11 display (or Wayland)

**Test Approach:**
- Xvfb (virtual framebuffer) in CI
- Standard xUnit execution with `DISPLAY` env var
- Can use `dotnet test` or custom runner

**Key Tests:**
- GObject signal connections
- GTK widget hierarchy
- X11 display availability

**CI Setup:**
```yaml
- name: Setup Xvfb
  run: |
    sudo Xvfb :99 -screen 0 1024x768x24 &
    echo "DISPLAY=:99" >> $GITHUB_ENV
```

## Test Data Flow

### Cross-Platform Test Flow

```
┌─────────────────────────────────────────────────────────────┐
│ Test Method (ButtonTests.cs)                                │
├─────────────────────────────────────────────────────────────┤
│ RunOnUIThread(() => {                                       │
│     var button = new Button(shell, SWT.PUSH);              │
│     button.Text = "Test";                                   │
│     Assert.Equal("Test", button.Text);                      │
│ })                                                           │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ Display.SyncExec(action)                                    │
├─────────────────────────────────────────────────────────────┤
│ - Queues action on UI thread                                │
│ - Blocks until complete                                     │
│ - Returns result                                            │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ Button Widget Layer                                         │
├─────────────────────────────────────────────────────────────┤
│ - Validates parameters                                      │
│ - Calls PlatformWidget.SetText("Test")                     │
│ - Returns PlatformWidget.GetText()                         │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ Mock IPlatformTextWidget (NSubstitute)                     │
├─────────────────────────────────────────────────────────────┤
│ - Records calls                                             │
│ - Returns configured values                                 │
│ - No native API calls                                       │
└─────────────────────────────────────────────────────────────┘
```

### Platform-Specific Test Flow

```
┌─────────────────────────────────────────────────────────────┐
│ Test Method ([WindowsFact] WindowsButtonTests.cs)          │
├─────────────────────────────────────────────────────────────┤
│ if (!IsWindows) Skip("Windows-only test");                 │
│ RunOnUIThread(() => {                                       │
│     var button = new Button(shell, SWT.PUSH);              │
│     Assert.NotNull(button.PlatformWidget);                  │
│ })                                                           │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ Display.SyncExec(action)                                    │
│ (Same as cross-platform)                                    │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ Button Widget Layer                                         │
│ (Same as cross-platform)                                    │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ REAL Win32Platform Implementation                           │
├─────────────────────────────────────────────────────────────┤
│ - CreateButtonWidget() calls Win32 API                      │
│ - Creates native HWND                                       │
│ - Returns Win32Button : IPlatformTextWidget                 │
└──────────────────┬──────────────────────────────────────────┘
                   ↓
┌─────────────────────────────────────────────────────────────┐
│ Native Win32 API                                            │
├─────────────────────────────────────────────────────────────┤
│ - CreateWindowEx()                                          │
│ - SendMessage(WM_SETTEXT)                                   │
│ - SendMessage(WM_GETTEXT)                                   │
└─────────────────────────────────────────────────────────────┘
```

### Thread Affinity (macOS)

```
┌─────────────────────────────────────────────────────────────┐
│ Thread 1 (Main)                                             │
├─────────────────────────────────────────────────────────────┤
│ - Display created here (DisplayFixture)                     │
│ - RunLoop active                                            │
│ - GCD main queue processes actions                          │
└──────────────────┬──────────────────────────────────────────┘
                   ↑ dispatch_sync
┌──────────────────┴──────────────────────────────────────────┐
│ Background Thread (xUnit Test Execution)                    │
├─────────────────────────────────────────────────────────────┤
│ - Tests run here                                            │
│ - RunOnUIThread() dispatches to Thread 1                    │
│ - Blocks waiting for result                                 │
└─────────────────────────────────────────────────────────────┘
```

## Suggested Test Organization

### Test File Naming Convention

```
Format: [Platform?][Component][Tests|Spec].cs

Examples:
✓ ButtonTests.cs              # Cross-platform button tests
✓ WindowsButtonTests.cs       # Windows-specific button tests
✓ MacOSButtonTests.cs         # macOS-specific button tests
✓ LinuxButtonTests.cs         # Linux-specific button tests
✓ CompositeTests.cs           # Cross-platform composite tests
✓ ShellTests.cs               # Cross-platform shell tests
✓ FileDialogTests.cs          # Cross-platform dialog tests
```

### Test Class Organization

**Cross-Platform Tests:**
```csharp
[Collection("Display Tests")]
public class ButtonTests : WidgetTestBase
{
    // Group by feature area
    #region Creation Tests
    #endregion

    #region Property Tests
    #endregion

    #region Event Tests
    #endregion

    #region Disposal Tests
    #endregion
}
```

**Platform-Specific Tests:**
```csharp
[Collection("Cross-Platform Tests")]
public class WindowsButtonTests : TestBase
{
    #region Creation Tests
    [WindowsFact]
    public void Button_Create_WithPushStyle_ShouldSucceed() { }
    #endregion

    #region Platform-Specific Tests
    [WindowsFact]
    public void Button_PlatformWidget_ShouldBeWin32Button() { }
    #endregion

    #region Integration Tests
    [WindowsFact]
    public void Button_MultipleButtons_ShouldWorkIndependently() { }
    #endregion
}
```

### Test Categories by Scope

**Unit Tests (Mocked):**
- Widget creation and disposal
- Property get/set behavior
- Event subscription/unsubscription
- Argument validation
- State management

**Integration Tests (Real Platform):**
- Platform widget creation
- Native handle management
- Event routing through platform layer
- Multi-widget interactions
- Memory leak detection
- Performance benchmarks

**CI/CD Tests:**
- All unit tests (fast, no platform dependencies)
- Platform-specific integration tests on respective platforms
- Headless mode tests (Linux with Xvfb)
- Coverage collection per platform

### Test Fixture Strategy

**DisplayFixture (Shared):**
- Creates single Display per test collection
- Thread-safe initialization
- Automatic cleanup on collection disposal
- Handles macOS Thread 1 requirement

**Per-Test Isolation:**
- Each test creates its own Shell
- Widgets disposed in test teardown
- No shared state between tests
- TestBase.Dispose() cleans up shells

### Helper Organization

**Static Helpers (TestHelpers.cs):**
```csharp
// Widget creation
CreateTestShell(Display, configure)
CreateTestButton(parent, text, style)
CreateTestLabel(parent, text, style)

// Assertions
AssertDisposed(widget)
AssertNotDisposed(widget)
AssertCondition(predicate, timeout, message)

// Event testing
AssertEventFired<T>(subscribe, unsubscribe, trigger, validate)
AssertEventNotFired<T>(subscribe, unsubscribe, trigger)

// Performance
MeasureTime(action)
MeasureUITime(display, action)
```

**Instance Helpers (TestBase/WidgetTestBase):**
```csharp
// TestBase
RunOnUIThread(action)
RunOnUIThread<T>(func)
CreateTestShell()

// WidgetTestBase
AssertWidgetCreation<T>(factory)
AssertControlParent<T>(factory)
AssertPropertyGetSet<T, TProp>(factory, getter, setter, value)
AssertWidgetStyles<T>(factory, styles)
```

## Build Order Implications

### Dependency Graph

```
SWTSharp (library)
    ↓
SWTSharp.TestHost (infrastructure)
    ↓
SWTSharp.TestAdapter (xUnit adapter)
    ↓
SWTSharp.Tests (test assembly)
```

### Build Considerations

1. **SWTSharp.TestHost** must build before tests
   - Contains DisplayFixture
   - Platform detection logic
   - Thread synchronization primitives

2. **SWTSharp.TestAdapter** integrates with VSTest
   - Copied to test output directory
   - Auto-discovered by VSTest
   - Handles custom test execution

3. **SWTSharp.Tests** requires all dependencies
   - References TestHost for fixtures
   - Includes TestAdapter DLLs in output
   - Can run as executable (macOS) or library (Windows/Linux)

### CI/CD Build Matrix

```yaml
Strategy:
  matrix:
    os: [windows-latest, ubuntu-latest, macos-latest]

Steps:
  - build: dotnet build
  - test-windows: dotnet run --project tests/SWTSharp.Tests
  - test-linux: dotnet run --project tests/SWTSharp.Tests (with Xvfb)
  - test-macos: dotnet run --project tests/SWTSharp.Tests (custom runner)
```

## Quality Gates

### Components Clearly Defined

✓ **Infrastructure Layer:** TestBase, WidgetTestBase, Fixtures, Helpers
✓ **Widget Test Layer:** Cross-platform behavioral tests in Widgets/
✓ **Platform Test Layer:** Platform-specific tests in Platform/
✓ **Test Runner:** Custom Program.cs for macOS Thread 1 support

**Boundaries:**
- Infrastructure provides shared services (Display, mocking, threading)
- Widget tests validate public API behavior
- Platform tests validate native integration
- No cross-layer dependencies (platform tests don't call widget test helpers)

### Data Flow Direction Explicit

✓ **Cross-Platform Tests:** Test → Widget → Mock Platform
✓ **Integration Tests:** Test → Widget → Real Platform → Native API
✓ **Thread Flow (macOS):** Background Thread → GCD → Thread 1 → Native
✓ **Event Flow:** Native → Platform → Widget → Test Handler

**Constraints:**
- UI operations always go through `RunOnUIThread()`
- Platform abstractions never expose native handles to widget layer
- Tests never directly call native APIs (go through platform layer)

### Build Order Implications Noted

✓ **Dependency Order:** SWTSharp → TestHost → TestAdapter → Tests
✓ **Output Configuration:** TestAdapter DLLs copied to Tests output
✓ **Execution Model:** Executable on macOS, library elsewhere
✓ **CI Integration:** Different test execution per platform

**Critical Build Steps:**
1. Build SWTSharp library
2. Build TestHost infrastructure
3. Build TestAdapter (netstandard2.0 for VSTest)
4. Build Tests with OutputType=Exe for macOS
5. Copy TestAdapter to Tests output directory
6. Copy TestHost to Tests output directory

## Recommendations

### For New Widget Tests

1. Start with cross-platform tests in `Widgets/[Widget]Tests.cs`
2. Use `WidgetTestBase` helper methods
3. Test only public API behavior
4. Add platform-specific tests in `Platform/[Platform][Widget]Tests.cs` as needed
5. Focus integration tests on platform-specific features (event routing, native handles)

### For Event Testing

1. Use `TestHelpers.AssertEventFired()` for cross-platform tests
2. For platform-specific event testing, use native triggering (like `objc_msgSend` on macOS)
3. Always wrap in `RunOnUIThread()`
4. Verify event cleanup on disposal

### For CI/CD

1. Run unit tests on all platforms (fast feedback)
2. Run integration tests only on target platform
3. Use Xvfb for Linux CI
4. Use custom runner for macOS CI
5. Collect platform-specific coverage reports

### For Memory Management

1. Always dispose widgets in tests (use `using` or explicit `Dispose()`)
2. Use `TestBase.Dispose()` for automatic cleanup
3. Test disposal behavior explicitly
4. Verify no leaks with platform tools (Instruments on macOS, etc.)

## Conclusion

SWTSharp's test architecture effectively balances cross-platform testing with platform-specific validation. The three-layer approach (mocked unit tests, platform-specific integration tests, custom runner) provides:

- Fast feedback from unit tests
- Platform validation from integration tests
- Proper threading on macOS
- Clear separation of concerns

The architecture is ready for expansion with new widgets and platforms while maintaining test quality and isolation.
