# Testing on macOS - Threading Considerations

## The Problem

macOS/AppKit has a strict requirement: **NSWindow must be created on the process's first thread (Thread 1)**. This creates a conflict with .NET's test runners:

- **VSTest** (used by `dotnet test`) runs tests on arbitrary threads for parallelization
- **xUnit** with `[UIFact]` attribute only creates STA threads (Windows concept), not Thread 1
- Without Thread 1 = NSWindow creation crashes

## The Solution: Dual-Mode Testing

We provide TWO ways to run tests:

### Mode 1: VSTest (CI, Coverage, Cross-Platform)

**Use for:**
- CI/CD pipelines (Windows, Linux)
- Code coverage collection
- Visual Studio Test Explorer
- Automated testing

**Command:**
```bash
dotnet test
```

**How it works:**
- Test project is a **library** (no custom entry point)
- VSTest discovers tests via reflection
- Coverlet collects code coverage
- Tests run on VSTest's threads

**Limitations:**
- ⚠️ **macOS UI tests will crash** (NSWindow not on Thread 1)
- ✅ Works perfectly on Windows/Linux
- ✅ Coverage collection works

### Mode 2: Custom TestRunner (macOS Local Development)

**Use for:**
- Local macOS development
- Running UI tests on macOS
- Debugging macOS-specific issues

**Command:**
```bash
# Using the helper script:
./scripts/test-macos.sh

# Or manually:
dotnet build -c Debug -p:RunTestRunner=true
dotnet run --project tests/SWTSharp.Tests
```

**How it works:**
1. `TestRunner.Main()` starts on Thread 1 (process entry point)
2. Initializes `MainThreadDispatcher` on Thread 1
3. Spawns xUnit on a background thread
4. Thread 1 runs dispatch loop (pumps UI events)
5. Tests use `MainThreadDispatcher.Invoke()` to marshal UI operations to Thread 1

**Limitations:**
- ⚠️ No VSTest integration (no Test Explorer, no `dotnet test` features)
- ⚠️ No automatic coverage collection
- ✅ Full macOS UI support

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     VSTest Mode (CI)                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐                                           │
│  │   VSTest    │                                           │
│  │  Discoverer │───────────────┐                          │
│  └─────────────┘               │                          │
│                                 ↓                          │
│                         ┌──────────────┐                   │
│                         │  Test Class  │                   │
│                         │  (Thread N)  │                   │
│                         └──────────────┘                   │
│                                 │                          │
│                                 ↓ (Crashes on macOS)       │
│                         ┌──────────────┐                   │
│                         │   Display    │                   │
│                         │   NSWindow   │                   │
│                         └──────────────┘                   │
│                                                             │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                 TestRunner Mode (macOS)                     │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Thread 1 (Main)              Thread N (xUnit)             │
│  ┌──────────────┐            ┌──────────────┐             │
│  │ TestRunner   │            │  Test Class  │             │
│  │    .Main()   │            │              │             │
│  └──────┬───────┘            └──────┬───────┘             │
│         │                           │                      │
│         ↓                           ↓                      │
│  ┌──────────────┐            ┌──────────────┐             │
│  │MainThread    │←───Invoke──│  Display     │             │
│  │Dispatcher    │            │  Tests       │             │
│  │ RunLoop()    │            │              │             │
│  └──────┬───────┘            └──────────────┘             │
│         │                                                  │
│         ↓                                                  │
│  ┌──────────────┐                                         │
│  │   Display    │                                         │
│  │   NSWindow   │  ✅ Works! (Thread 1)                   │
│  └──────────────┘                                         │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## CI/CD Strategy

Our `.github/workflows/ci.yml` uses **VSTest mode**:

### Windows/Linux Jobs
```yaml
- name: Run tests with coverage
  run: dotnet test --collect:"XPlat Code Coverage"
```
- ✅ Full test execution
- ✅ Code coverage collection
- ✅ All tests pass

### macOS Job
```yaml
- name: Run tests with coverage
  run: dotnet test --collect:"XPlat Code Coverage"
```
- ⚠️ UI tests crash (known limitation)
- ✅ Non-UI tests pass
- ✅ Partial coverage collection

**Future improvement:** Could skip UI tests on macOS CI using:
```csharp
[FactSkipOnMacOSCI]
public void MyUITest() { }
```

## For Test Authors

### Writing Platform-Agnostic Tests

```csharp
[Collection("Display Tests")]
public class MyWidgetTests
{
    private readonly DisplayFixture _fixture;

    public MyWidgetTests(DisplayFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestWidget()
    {
        // DisplayFixture handles threading:
        // - VSTest Mode: Crashes on macOS (limitation)
        // - TestRunner Mode: Works on all platforms
        var shell = new Shell(_fixture.Display);
        // ... test code
    }
}
```

### Skip Tests on macOS CI

```csharp
public class FactSkipOnMacOSCI : FactAttribute
{
    public FactSkipOnMacOSCI()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) &&
            Environment.GetEnvironmentVariable("CI") == "true")
        {
            Skip = "macOS UI tests crash in CI (Thread 1 requirement)";
        }
    }
}

[FactSkipOnMacOSCI]
public void MyUITest() { }
```

## MainThreadDispatcher API

The `MainThreadDispatcher` provides thread marshaling:

```csharp
// Check if dispatcher is initialized
if (MainThreadDispatcher.IsInitialized)
{
    // Synchronously execute on Thread 1
    MainThreadDispatcher.Invoke(() =>
    {
        var shell = new Shell(display);
        shell.Open();
    });

    // Or with return value
    var result = MainThreadDispatcher.Invoke(() =>
    {
        return shell.GetBounds();
    });
}
```

## Summary

| Feature | VSTest Mode | TestRunner Mode |
|---------|-------------|-----------------|
| CI/CD Integration | ✅ | ❌ |
| Code Coverage | ✅ | ❌ |
| Test Discovery | ✅ | ❌ |
| Windows/Linux UI | ✅ | ✅ |
| macOS UI | ❌ | ✅ |
| Parallel Execution | ✅ | ❌ |
| Visual Studio | ✅ | ❌ |

**Recommendation:**
- Use `dotnet test` for CI and development (VSTest mode)
- Use `./scripts/test-macos.sh` for local macOS UI testing only
- Skip macOS UI tests in CI, or accept they will crash

## References

- [xUnit.net Documentation](https://xunit.net/)
- [VSTest Platform](https://github.com/microsoft/vstest)
- [Apple Threading Programming Guide](https://developer.apple.com/library/archive/documentation/Cocoa/Conceptual/Multithreading/ThreadSafety/ThreadSafety.html)
- [NSWindow Documentation](https://developer.apple.com/documentation/appkit/nswindow)
