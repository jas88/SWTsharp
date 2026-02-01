# Phase 1: Test Infrastructure Foundation - Research

**Researched:** 2026-01-30
**Domain:** Cross-platform .NET GUI testing with xUnit, custom VSTest adapters, macOS Thread 1 requirements
**Confidence:** HIGH

## Summary

Phase 1 establishes reliable test infrastructure for SWTSharp across Windows, macOS, and Linux. The primary technical challenge is macOS's requirement that all GUI operations run on Thread 1 (the main thread), which conflicts with standard test runners executing tests on worker threads.

The existing codebase already has a custom VSTest adapter architecture (`SWTSharp.TestAdapter` + `SWTSharp.TestHost`) that solves this by launching a separate test host process where Thread 1 runs `CFRunLoopRun()` and uses GCD (Grand Central Dispatch) to dispatch GUI operations. This pattern is proven and working in the current codebase.

Microsoft.Testing.Platform investigation shows it's a modern alternative to VSTest but doesn't provide inherent threading control - custom execution logic would still be required. Staying with the VSTest adapter approach is recommended as the infrastructure already exists and works.

**Primary recommendation:** Complete the existing custom VSTest adapter implementation, add platform-specific test attributes using xUnit's `IXunitTestCaseDiscoverer` pattern, configure Coverlet for multi-platform coverage merging, and use `TaskCompletionSource<T>` with `TaskCreationOptions.RunContinuationsAsynchronously` for event-based synchronization.

## Standard Stack

The established libraries/tools for cross-platform .NET GUI testing with custom threading requirements:

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| xUnit.net | 2.9.3 | Test framework | De facto standard for .NET testing; excellent extensibility via `IXunitTestCaseDiscoverer` |
| Microsoft.TestPlatform.ObjectModel | 18.0.1 | VSTest adapter interfaces | Official VSTest extensibility API for custom test adapters |
| xunit.runner.utility | 2.9.3 | xUnit execution engine | Provides `XunitFrontController` for programmatic test discovery/execution |
| Coverlet | 6.0.4 | Code coverage | Cross-platform .NET coverage tool with MSBuild integration and merge support |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| NSubstitute | 5.3.0 | Mocking | Non-GUI unit tests with platform interface mocks |
| xunit.extensibility.execution | 2.9.3 | Custom test cases | Creating platform-specific test attributes |
| Codecov | GitHub Action v5 | Coverage reporting | Uploading merged coverage from CI |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| VSTest adapter | Microsoft.Testing.Platform | M.T.P is newer but requires .NET 10 SDK for native support; doesn't solve threading - would need custom execution anyway |
| Custom test host process | In-process thread control | Cannot control Thread 1 in-process when tests run via `dotnet test`; process isolation is cleaner |
| Coverlet | dotCover, Altcover | Coverlet is free, cross-platform, integrates well with MSBuild; others are commercial or less maintained |

**Installation:**
```bash
# Test adapter project
dotnet add package Microsoft.TestPlatform.ObjectModel --version 18.0.1
dotnet add package xunit.runner.utility --version 2.9.3
dotnet add package xunit.extensibility.execution --version 2.9.3

# Test project
dotnet add package xunit --version 2.9.3
dotnet add package coverlet.collector --version 6.0.4
dotnet add package NSubstitute --version 5.3.0
```

## Architecture Patterns

### Recommended Project Structure
```
tests/
├── SWTSharp.TestAdapter/        # Custom VSTest adapter (netstandard2.0)
│   ├── SWTSharpTestDiscoverer.cs
│   ├── SWTSharpTestExecutor.cs
│   └── SWTSharp.TestAdapter.csproj
├── SWTSharp.TestHost/           # Test host executable (net9.0)
│   ├── Program.cs
│   ├── MainThreadDispatcher.cs
│   └── SWTSharp.TestHost.csproj
├── SWTSharp.Tests.Core/         # Shared mocked tests (netstandard2.0)
│   ├── Infrastructure/TestBase.cs
│   └── SWTSharp.Tests.Core.csproj
├── SWTSharp.Tests.Windows/      # Windows GUI tests (net9.0-windows)
│   └── Platform/WindowsButtonTests.cs
├── SWTSharp.Tests.MacOS/        # macOS GUI tests (net9.0)
│   └── Platform/MacOSButtonTests.cs
└── SWTSharp.Tests.Linux/        # Linux GUI tests (net9.0)
    └── Platform/LinuxButtonTests.cs
```

### Pattern 1: Custom VSTest Adapter for Thread 1 Control

**What:** VSTest adapter that launches a separate test host process where Thread 1 runs CFRunLoop and dispatches GUI operations via GCD

**When to use:** When platform requires specific thread for operations (macOS Cocoa, Windows COM STA)

**Example:**
```csharp
// Source: Existing SWTSharp codebase
[ExtensionUri(ExecutorUri)]
public class SWTSharpTestExecutor : ITestExecutor
{
    public const string ExecutorUri = "executor://SWTSharpTestExecutor";

    public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext,
                        IFrameworkHandle frameworkHandle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            RunTestsInMacOSHost(tests, frameworkHandle);
        }
        else
        {
            RunTestsInDefaultHost(tests, frameworkHandle);
        }
    }

    private void RunTestsInMacOSHost(IEnumerable<TestCase> tests, IFrameworkHandle frameworkHandle)
    {
        var testHostPath = GetTestHostPath();
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{testHostPath}\" \"{testAssembly}\" {testFilter}",
            UseShellExecute = false,
            RedirectStandardOutput = true
        };

        using var process = Process.Start(startInfo);
        // Parse test results from stdout
    }
}
```

**Test Host Pattern:**
```csharp
// Source: Existing SWTSharp.TestHost/Program.cs
public static int Main(string[] args)
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        // Initialize on Thread 1 (main process thread)
        MainThreadDispatcher.Initialize();

        // Hook SWTSharp to route ExecuteOnMainThread through dispatcher
        MacOSPlatform.CustomMainThreadExecutor = MainThreadDispatcher.Invoke;

        // Run tests directly on Thread 1
        return RunTests(testAssembly, testFilter);
    }
    else
    {
        return RunTests(testAssembly, testFilter);
    }
}
```

### Pattern 2: Platform-Specific Test Attributes with xUnit

**What:** Custom xUnit attributes that skip tests on incompatible platforms using `IXunitTestCaseDiscoverer`

**When to use:** When tests should only run on specific platforms

**Example:**
```csharp
// Source: xUnit extensibility patterns
// https://xunit.net/docs/getting-started/v3/migration-extensibility

[AttributeUsage(AttributeTargets.Method)]
[XunitTestCaseDiscoverer("SWTSharp.Tests.MacOSFactDiscoverer", "SWTSharp.Tests")]
public class MacOSFactAttribute : FactAttribute { }

public class MacOSFactDiscoverer : IXunitTestCaseDiscoverer
{
    public IEnumerable<IXunitTestCase> Discover(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new[] { new XunitTestCase(/* ... */) };
        }

        // Skip on non-macOS platforms
        return new[] { new XunitTestCase(/* skipped case */) };
    }
}

// Usage:
[MacOSFact]
public void NSWindow_RequiresThread1()
{
    // Only runs on macOS
}
```

### Pattern 3: TaskCompletionSource for Event-Based Synchronization

**What:** Use `TaskCompletionSource<T>` to convert event-based async operations into awaitable Tasks

**When to use:** Widget event testing where you need to wait for a GUI event to fire

**Example:**
```csharp
// Source: Microsoft Learn - TaskCompletionSource patterns
// https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/implementing-the-task-based-asynchronous-pattern

public static Task<bool> WaitForButtonClick(Button button, TimeSpan timeout)
{
    // CRITICAL: Use RunContinuationsAsynchronously for .NET 4.6.1+
    var tcs = new TaskCompletionSource<bool>(
        TaskCreationOptions.RunContinuationsAsynchronously);

    SelectionListener listener = null;
    var timer = new System.Threading.Timer(_ =>
    {
        button.RemoveSelectionListener(listener);
        tcs.TrySetException(new TimeoutException("Button click timeout"));
    }, null, timeout, Timeout.InfiniteTimeSpan);

    listener = new SelectionListener(() =>
    {
        timer.Dispose();
        button.RemoveSelectionListener(listener);
        tcs.TrySetResult(true);
    });

    button.AddSelectionListener(listener);
    return tcs.Task;
}

// Usage in test:
[Fact]
public async Task Button_Click_FiresEvent()
{
    await RunOnUIThread(async () =>
    {
        var button = new Button(shell, SWT.PUSH);
        button.PerformClick();

        bool clicked = await WaitForButtonClick(button, TimeSpan.FromSeconds(5));
        Assert.True(clicked);
    });
}
```

**Critical detail:** Always use `TaskCreationOptions.RunContinuationsAsynchronously` when creating `TaskCompletionSource` instances on .NET 4.6.1+ to prevent deadlocks. Without this flag, continuations run synchronously on the thread that completes the TCS, which can cause deadlocks.

Source: [The danger of TaskCompletionSource class](https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/)

### Pattern 4: Multi-Platform Coverage Merging with Coverlet

**What:** Collect separate coverage files from each platform job and merge into single report

**When to use:** CI with platform-specific test jobs needing combined coverage

**Example:**
```bash
# Source: Coverlet documentation
# https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/Examples/MSBuild/MergeWith/HowTo.md

# Windows job
dotnet test /p:CollectCoverage=true \
  /p:CoverletOutputFormat=json \
  /p:CoverletOutput=./coverage-windows.json

# macOS job
dotnet test /p:CollectCoverage=true \
  /p:CoverletOutputFormat=json \
  /p:CoverletOutput=./coverage-macos.json

# Linux job (final merge)
dotnet test /p:CollectCoverage=true \
  /p:MergeWith="../coverage-windows.json;../coverage-macos.json" \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage-merged.xml
```

**GitHub Actions pattern:**
```yaml
# Upload coverage artifacts from each platform
- name: Upload coverage artifact
  uses: actions/upload-artifact@v5
  with:
    name: coverage-${{ matrix.os }}
    path: coverage-*.json

# Merge step (separate job after all platforms complete)
merge-coverage:
  needs: [test-windows, test-macos, test-linux]
  runs-on: ubuntu-latest
  steps:
    - uses: actions/download-artifact@v5
      with:
        pattern: coverage-*
        path: ./coverage
    - name: Merge coverage
      run: |
        dotnet test /p:CollectCoverage=true \
          /p:MergeWith="./coverage/**/*.json" \
          /p:CoverletOutputFormat=opencover
    - uses: codecov/codecov-action@v5
      with:
        file: ./coverage-merged.xml
```

### Pattern 5: xUnit Test Collection Parallelization Control

**What:** Control which tests run in parallel vs sequentially using `[Collection]` attributes

**When to use:** GUI tests must run sequentially; non-GUI tests can parallelize

**Example:**
```csharp
// Source: xUnit parallelization documentation
// https://xunit.net/docs/running-tests-in-parallel

// Define a sequential collection for GUI tests
[CollectionDefinition("GUI Tests", DisableParallelization = true)]
public class GUITestCollection { }

// All GUI tests use this collection (run sequentially)
[Collection("GUI Tests")]
public class ButtonTests : GUITestBase
{
    [Fact]
    public void Button_Click() { /* ... */ }
}

[Collection("GUI Tests")]
public class LabelTests : GUITestBase
{
    [Fact]
    public void Label_Text() { /* ... */ }
}

// Non-GUI tests use default parallelization (run in parallel)
public class MockedWidgetTests
{
    [Fact]
    public void Widget_MockedPlatform() { /* runs in parallel */ }
}

// Assembly-level configuration
[assembly: CollectionBehavior(MaxParallelThreads = 4)]
```

### Anti-Patterns to Avoid

- **Polling loops instead of TaskCompletionSource:** Don't use `while (!eventFired) { Thread.Sleep(100); }` - this is flaky, wastes CPU, and has race conditions. Use TCS pattern.

- **Thread.Sleep for synchronization:** Never use fixed delays like `Thread.Sleep(500)` hoping GUI operations complete. Use event-based signaling with timeouts.

- **Running GUI tests in parallel:** macOS/Windows GUI frameworks are inherently single-threaded. Parallel GUI tests will crash or produce flaky results.

- **Forgetting RunContinuationsAsynchronously:** Creating `TaskCompletionSource` without this flag on .NET 4.6.1+ can deadlock if continuations try to acquire locks held by the SetResult caller.

- **Using Microsoft.Testing.Platform expecting automatic threading:** M.T.P doesn't solve threading challenges - you still need custom execution logic. VSTest adapter is the proven approach.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Test discovery/execution | Custom test framework | xUnit + VSTest adapter | xUnit has 10+ years of edge case handling; test discovery is complex (generics, async, theory data) |
| Code coverage | Custom IL instrumentation | Coverlet | Coverage requires deep .NET IL knowledge; Coverlet handles PDB mapping, branch detection, multi-targeting |
| Event → async conversion | Custom event waiters | TaskCompletionSource<T> | TCS is thread-safe, handles cancellation, integrates with async/await; hand-rolled versions have race conditions |
| Platform detection | `#if WINDOWS` everywhere | `RuntimeInformation.IsOSPlatform()` | Compile-time switches break multi-targeting; runtime detection works with RID-specific builds |
| CI test result parsing | Grep test output | VSTest .trx format | Standard XML format works with all CI systems; parsing stdout is fragile |

**Key insight:** Test infrastructure has decades of accumulated wisdom in edge cases. The hard parts aren't "run this method" - they're generic test methods, theory data generation, async test lifecycle, exception handling, timeout detection, hanging test cleanup, and parallel test isolation. xUnit solved these; don't rebuild them.

## Common Pitfalls

### Pitfall 1: macOS Thread 1 Deadlock with dispatch_sync

**What goes wrong:** Using `dispatch_sync_f` to dispatch GUI operations from worker threads deadlocks when Thread 1 isn't running CFRunLoop

**Why it happens:** GCD main queue only processes work when CFRunLoop (or NSRunLoop) is actively running on Thread 1. If Thread 1 is blocked or idle, dispatched work never executes, and `dispatch_sync_f` waits forever.

**How to avoid:**
1. Always run CFRunLoop on Thread 1: `CFRunLoopRun()` in test host Program.Main
2. Use `dispatch_async_f` + manual synchronization (ManualResetEventSlim) instead of `dispatch_sync_f`
3. Detect deadlock with timeout and fail with clear message

**Warning signs:**
- Tests hang indefinitely on macOS but pass on Windows/Linux
- Thread dumps show worker threads blocked in `dispatch_sync_f`
- No error message, just infinite wait

**Source:** Existing `GCDThreadingTests.cs` documents this: "dispatch_sync_f(mainQueue, ...) dispatches work to macOS main queue. Thread 1 MUST be running CFRunLoop for work to be processed."

### Pitfall 2: TaskCompletionSource Deadlock Without RunContinuationsAsynchronously

**What goes wrong:** Deadlock when continuation tries to acquire a lock held by the thread calling `SetResult()`

**Why it happens:** By default, `TaskCompletionSource<T>` runs continuations **synchronously** on the thread that calls `SetResult()`. If that thread holds a lock and the continuation tries to acquire the same lock, deadlock.

**How to avoid:** Always create with `TaskCreationOptions.RunContinuationsAsynchronously` on .NET 4.6.1+:
```csharp
var tcs = new TaskCompletionSource<bool>(
    TaskCreationOptions.RunContinuationsAsynchronously);
```

**Warning signs:**
- Tests hang intermittently under load
- Thread dumps show two threads each waiting for the other
- Adding logging "fixes" the issue (changes timing)

**Source:** [Microsoft DevBlogs: The danger of TaskCompletionSource<T> class](https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/)

### Pitfall 3: VSTest Adapter Discovery Not Finding Tests

**What goes wrong:** Custom VSTest adapter builds but tests aren't discovered; `dotnet test` reports "0 tests found"

**Why it happens:** VSTest auto-discovery requires:
1. Adapter DLL in test assembly's output directory
2. Correct `[FileExtension]` and `[DefaultExecutorUri]` attributes
3. Adapter targets netstandard2.0 (not net9.0)
4. Test assembly name matches discovery filter (e.g., "SWTSharp.Tests")

**How to avoid:**
1. Use MSBuild `<Target Name="CopyTestAdapter" AfterTargets="Build">` to copy adapter DLL
2. Verify attributes on discoverer class match executor URI exactly
3. Target netstandard2.0 for adapter (VSTest host is .NET Framework on older Visual Studio)
4. Check adapter DLL is in bin/Debug/net9.0/ alongside test DLL

**Warning signs:**
- `dotnet test` shows "Test run detected DLL(s) which were built for different framework"
- No tests discovered, but test project compiles
- Tests discovered in Visual Studio Test Explorer but not CLI

**Source:** [Microsoft vstest-docs: Adapter Extensibility RFC](https://github.com/microsoft/vstest-docs/blob/main/RFCs/0004-Adapter-Extensibility.md)

### Pitfall 4: Coverage Not Collected with Custom Test Adapter

**What goes wrong:** Coverlet reports 0% coverage even though tests run and pass

**Why it happens:** Coverlet integrates with VSTest via data collector, but custom test adapters that launch separate processes can break the instrumentation path. Coverlet instruments the test assembly expecting in-process execution.

**How to avoid:**
1. Use `coverlet.collector` package (data collector mode) instead of `coverlet.msbuild`
2. Ensure test host process loads instrumented assemblies (same bin directory)
3. Pass `--collect:"XPlat Code Coverage"` to `dotnet test`
4. Verify instrumented DLLs exist in TestResults directory

**Warning signs:**
- TestResults folder empty or contains only TRX file
- Coverlet logs show "0 modules instrumented"
- Coverage works with standard xUnit but not custom adapter

**Source:** [Coverlet GitHub: collector mode documentation](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/VSTestIntegration.md)

### Pitfall 5: Platform-Specific Tests Run on Wrong Platform in CI

**What goes wrong:** `[MacOSFact]` tests execute on Windows runner and crash with platform-specific P/Invoke errors

**Why it happens:** Custom `IXunitTestCaseDiscoverer` implementation doesn't properly skip tests - it returns a test case with Skip reason instead of returning empty collection

**How to avoid:**
```csharp
// WRONG - still creates a test case
public IEnumerable<IXunitTestCase> Discover(...)
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        return new[] { new XunitTestCase(/* Skip = "macOS only" */) };
    }
}

// CORRECT - no test case created
public IEnumerable<IXunitTestCase> Discover(...)
{
    if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        return Enumerable.Empty<IXunitTestCase>();  // ← Return empty
    }
}
```

**Warning signs:**
- Tests marked skipped instead of not discovered
- Platform-specific exceptions in CI on wrong OS
- Test count varies between local and CI

**Source:** [xUnit GitHub: Custom Fact execution discussion](https://github.com/xunit/xunit/issues/1614)

## Code Examples

Verified patterns from official sources:

### Custom VSTest Discoverer Implementation

```csharp
// Source: Microsoft VSTest Platform documentation
// https://github.com/microsoft/vstest/blob/main/src/Microsoft.TestPlatform.ObjectModel/Adapter/Interfaces/ITestDiscoverer.cs

[FileExtension(".dll")]
[FileExtension(".exe")]
[DefaultExecutorUri(SWTSharpTestExecutor.ExecutorUri)]
public class SWTSharpTestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        foreach (var source in sources)
        {
            // Use xUnit's XunitFrontController for discovery
            using var controller = new XunitFrontController(
                AppDomainSupport.Denied,
                assemblyFileName: source,
                configFileName: null,
                shadowCopy: false);

            var visitor = new TestDiscoveryVisitor();
            controller.Find(
                includeSourceInformation: true,
                messageSink: visitor,
                discoveryOptions: TestFrameworkOptions.ForDiscovery());

            visitor.Finished.WaitOne();

            // Convert xUnit test cases to VSTest test cases
            foreach (var xunitTestCase in visitor.TestCases)
            {
                var vstestCase = new TestCase(
                    fullyQualifiedName: xunitTestCase.DisplayName,
                    executorUri: new Uri(SWTSharpTestExecutor.ExecutorUri),
                    source: source);

                discoverySink.SendTestCase(vstestCase);
            }
        }
    }
}
```

### macOS CFRunLoop Integration

```csharp
// Source: Existing SWTSharp.TestHost/MainThreadDispatcher.cs
// Core Foundation P/Invoke declarations

[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
private static extern IntPtr CFRunLoopGetMain();

[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
private static extern void CFRunLoopRun();

[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
private static extern void CFRunLoopStop(IntPtr rl);

// GCD dispatch
private const string LibSystem = "/usr/lib/libSystem.dylib";

[DllImport(LibSystem, EntryPoint = "dispatch_async_f")]
private static extern void dispatch_async_f(IntPtr queue, IntPtr context, IntPtr work);

// Usage pattern
public static void RunLoop()
{
    if (Thread.CurrentThread.ManagedThreadId != _mainThread?.ManagedThreadId)
        throw new InvalidOperationException("RunLoop must run on same thread as Initialize()");

    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        _mainRunLoop = CFRunLoopGetCurrent();
        CFRunLoopRun();  // Blocks until CFRunLoopStop() called
    }
    else
    {
        // Windows/Linux: custom BlockingCollection dispatch loop
        while (_running)
        {
            if (_workQueue.TryTake(out var action, 100))
                action();
        }
    }
}
```

### Event-Based Synchronization Helper

```csharp
// Source: Microsoft patterns + SWTSharp requirements
public static class EventSyncHelpers
{
    public static Task<T> WaitForEvent<T>(
        Action<Action<T>> subscribe,
        Action<Action<T>> unsubscribe,
        TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Action<T> handler = null;
        var timer = new Timer(_ =>
        {
            unsubscribe(handler);
            tcs.TrySetException(
                new TimeoutException($"Event timeout after {timeout}"));
        }, null, timeout, Timeout.InfiniteTimeSpan);

        handler = (result) =>
        {
            timer.Dispose();
            unsubscribe(handler);
            tcs.TrySetResult(result);
        };

        subscribe(handler);
        return tcs.Task;
    }
}

// Usage:
var clicked = await EventSyncHelpers.WaitForEvent<bool>(
    subscribe: handler => button.SelectionChanged += handler,
    unsubscribe: handler => button.SelectionChanged -= handler,
    timeout: TimeSpan.FromSeconds(5));
```

### Platform-Specific Test Attribute

```csharp
// Source: xUnit v3 extensibility patterns
// https://xunit.net/docs/getting-started/v3/migration-extensibility

[AttributeUsage(AttributeTargets.Method)]
[XunitTestCaseDiscoverer(
    "SWTSharp.Tests.Platform.WindowsFactDiscoverer",
    "SWTSharp.Tests")]
public class WindowsFactAttribute : FactAttribute { }

public class WindowsFactDiscoverer : IXunitTestCaseDiscoverer
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new[] {
                new XunitTestCase(
                    _diagnosticMessageSink,
                    discoveryOptions.MethodDisplayOrDefault(),
                    discoveryOptions.MethodDisplayOptionsOrDefault(),
                    testMethod)
            };
        }

        // Not Windows - don't discover this test
        return Enumerable.Empty<IXunitTestCase>();
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| VSTest-only | Microsoft.Testing.Platform | .NET 9 SDK (2024) | M.T.P is modern but VSTest still viable; M.T.P requires .NET 10 for native support |
| NUnit, MSTest | xUnit | ~2013-2015 | xUnit became de facto for .NET OSS; better extensibility and async support |
| Manual coverage instrumentation | Coverlet | ~2018 | First cross-platform .NET coverage tool; displaced paid tools |
| Event-based patterns (EAP) | Task-based patterns (TAP) | .NET 4.5 (2012) | `async`/`await` made event→Task wrapping standard |
| AppDomains for test isolation | Process isolation | .NET Core (2016) | AppDomains removed; separate processes now standard for isolation |

**Deprecated/outdated:**
- **MSTest/VSTest as default:** While still supported, xUnit is preferred for new projects due to better extensibility
- **dotCover/NCover for coverage:** Commercial tools; Coverlet provides free cross-platform alternative
- **Manual TaskCompletionSource without RunContinuationsAsynchronously:** Causes deadlocks; always use flag on .NET 4.6.1+
- **Matrix strategy for platform tests in single workflow:** Prefer separate named jobs for clarity in status checks

## Open Questions

Things that couldn't be fully resolved:

1. **Microsoft.Testing.Platform threading control**
   - What we know: M.T.P doesn't provide inherent threading control; you'd still need custom execution logic
   - What's unclear: Whether M.T.P's in-process execution model makes Thread 1 control easier or harder
   - Recommendation: Investigate by creating minimal M.T.P test project and attempting CFRunLoop integration; if not cleaner than VSTest adapter, stick with VSTest

2. **Coverage accuracy with separate test host process**
   - What we know: Coverlet uses VSTest data collector which may not instrument separate process correctly
   - What's unclear: Whether coverlet.collector can instrument TestHost.exe or only the test assembly
   - Recommendation: Verify coverage numbers match expected (80%+); if low, investigate in-process adapter mode for Windows/Linux while keeping out-of-process for macOS

3. **xUnit v3 migration timeline**
   - What we know: xUnit v3 has breaking changes in extensibility APIs (namespace moves, interface changes)
   - What's unclear: When v3 becomes stable and recommended for production
   - Recommendation: Stay on xUnit 2.x for now; monitor v3 release schedule for migration after stable release

## Sources

### Primary (HIGH confidence)

- [xUnit.net documentation](https://xunit.net/docs/running-tests-in-parallel) - Test parallelization control
- [Microsoft Learn: TaskCompletionSource patterns](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/implementing-the-task-based-asynchronous-pattern) - Event-based async patterns
- [Microsoft Learn: Microsoft.Testing.Platform vs VSTest](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-vs-vstest) - Platform comparison
- [Coverlet documentation](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/Examples/MSBuild/MergeWith/HowTo.md) - Multi-platform coverage merging
- [Microsoft VSTest Platform RFCs](https://github.com/microsoft/vstest-docs/blob/main/RFCs/0004-Adapter-Extensibility.md) - Custom adapter interfaces
- [xUnit v3 migration guide](https://xunit.net/docs/getting-started/v3/migration-extensibility) - Extensibility changes

### Secondary (MEDIUM confidence)

- [Microsoft DevBlogs: TaskCompletionSource dangers](https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/) - RunContinuationsAsynchronously requirement
- [GitHub: xUnit custom attributes discussion](https://github.com/xunit/xunit/issues/1614) - Platform-specific test patterns
- [GitHub: Coverlet VSTest integration](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/VSTestIntegration.md) - Coverage with custom adapters

### Tertiary (LOW confidence)

- Blog posts from 2020-2024 on xUnit parallelization (verified against official docs)
- Stack Overflow discussions on TaskCompletionSource patterns (verified against Microsoft Learn)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All packages are official Microsoft or widely-adopted OSS with active maintenance
- Architecture patterns: HIGH - VSTest adapter approach is proven in existing codebase; xUnit patterns documented officially
- Don't hand-roll: HIGH - Based on years of ecosystem evolution and known edge cases
- Common pitfalls: HIGH - Documented in existing codebase (GCDThreadingTests.cs) and official Microsoft sources
- Code examples: HIGH - All examples from official sources or existing working codebase

**Research date:** 2026-01-30
**Valid until:** 2026-04-30 (90 days - stable domain)

**Caveats:**
- Microsoft.Testing.Platform is evolving rapidly; investigation needed if it simplifies threading
- xUnit v3 extensibility breaking changes may require updates when stable
- Coverlet coverage accuracy with separate process needs verification
