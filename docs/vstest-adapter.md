# SWTSharp Custom VSTest Adapter

## Overview

The SWTSharp VSTest Adapter is a custom test adapter that solves the macOS Thread 1 requirement by running tests in a separate process with full control over the main thread.

## The Problem It Solves

**macOS NSWindow Thread Requirement**: macOS requires NSWindow and UI operations to occur on the process's first thread (Thread 1). VSTest runs tests on arbitrary threads, causing crashes:

```
*** Terminating app due to uncaught exception 'NSInternalInconsistencyException',
reason: 'NSWindow should only be instantiated on the main thread!'
```

**Solution**: Custom VSTest adapter that:
1. Discovers tests using VSTest's standard discovery mechanism
2. Launches a separate test host process for execution
3. Test host ensures Thread 1 is available for UI operations
4. Reports results back to VSTest for integration with CI/CD

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         VSTest Platform                         │
│                      (dotnet test / vstest.console)             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐          ┌──────────────────┐           │
│  │ Test Discovery   │          │  Test Execution  │           │
│  │                  │          │                  │           │
│  │  1. Scan DLLs    │          │  4. Run Tests    │           │
│  │  2. Find Tests   │          │  5. Report       │           │
│  └────────┬─────────┘          └────────┬─────────┘           │
│           │                              │                      │
│           ↓                              ↓                      │
├───────────────────────────────────────────────────────────────┤
│              SWTSharp.TestAdapter (This Package)              │
├───────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────┐          ┌──────────────────┐           │
│  │  Discoverer      │          │    Executor      │           │
│  │                  │          │                  │           │
│  │ ITestDiscoverer  │          │  ITestExecutor   │           │
│  │                  │          │                  │           │
│  │  - Use xUnit     │          │  - Launch Host   │           │
│  │  - Find Tests    │          │  - Parse Results │           │
│  │  - Return List   │          │  - Report Back   │           │
│  └──────────────────┘          └────────┬─────────┘           │
│                                          │                      │
│                                          │ Process.Start()      │
│                                          ↓                      │
├───────────────────────────────────────────────────────────────┤
│              SWTSharp.TestHost (Separate Process)             │
├───────────────────────────────────────────────────────────────┤
│                                                                 │
│  Thread 1 (Main)              Thread N (Tests)                │
│  ┌──────────────┐            ┌──────────────┐                │
│  │   Program    │            │   xUnit      │                │
│  │    .Main()   │            │   Runner     │                │
│  └──────┬───────┘            └──────┬───────┘                │
│         │                           │                         │
│         ↓                           ↓                         │
│  ┌──────────────┐            ┌──────────────┐                │
│  │MainThread    │<───Invoke──│   Test       │                │
│  │Dispatcher    │            │   Methods    │                │
│  │ RunLoop()    │            │              │                │
│  └──────┬───────┘            └──────────────┘                │
│         │                                                      │
│         ↓                                                      │
│  ┌──────────────┐                                            │
│  │   Display    │  ✅ Works!                                 │
│  │   NSWindow   │     (Thread 1)                             │
│  └──────────────┘                                            │
│                                                                 │
│  Output: [RESULT] TestName: Passed|Failed duration message    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Components

### 1. SWTSharp.TestAdapter (Library)

**Package**: NuGet package that VSTest auto-discovers

**Key Classes**:

- **SWTSharpTestDiscoverer** (`ITestDiscoverer`)
  - Discovers xUnit tests in test assemblies
  - Uses xUnit's native discovery mechanism
  - Returns `TestCase` objects to VSTest
  - Decorated with `[FileExtension]` and `[DefaultExecutorUri]`

- **SWTSharpTestExecutor** (`ITestExecutor`)
  - Launches test host process on macOS
  - Runs tests in-process on Windows/Linux
  - Parses test results from host output
  - Reports results back to VSTest via `IFrameworkHandle`

**Installation**:
```xml
<PackageReference Include="SWTSharp.TestAdapter" Version="1.0.0" />
```

### 2. SWTSharp.TestHost (Executable)

**Purpose**: Separate process that runs tests with Thread 1 control

**Execution Flow** (macOS):
1. `Program.Main()` starts on Thread 1 (process entry point)
2. Initializes `MainThreadDispatcher` on Thread 1
3. Spawns xUnit runner on background thread
4. Thread 1 runs dispatch loop (pumps UI events)
5. Tests use `MainThreadDispatcher.Invoke()` for UI operations
6. Results written to stdout in parseable format

**Output Format**:
```
[INFO] SWTSharp TestHost: Loading test assembly...
[START] Test.Method.Name
[RESULT] Test.Method.Name: Passed 00:00:00.123
[RESULT] Test.Method.Name: Failed 00:00:00.456 Exception message
[INFO] SWTSharp TestHost: Tests completed
```

## VSTest Protocol Details

### Discovery Phase

**VSTest** → **Adapter**:
```csharp
void DiscoverTests(
    IEnumerable<string> sources,        // DLL paths
    IDiscoveryContext discoveryContext, // Settings
    IMessageLogger logger,              // Logging
    ITestCaseDiscoverySink discoverySink // Result sink
);
```

**Adapter** → **VSTest**:
```csharp
discoverySink.SendTestCase(new TestCase
{
    DisplayName = "MyTest",
    ExecutorUri = new Uri("executor://SWTSharpTestExecutor"),
    Source = "path/to/test.dll",
    FullyQualifiedName = "Namespace.Class.Method"
});
```

### Execution Phase

**VSTest** → **Adapter**:
```csharp
void RunTests(
    IEnumerable<TestCase> tests,
    IRunContext runContext,
    IFrameworkHandle frameworkHandle
);
```

**Adapter** → **VSTest**:
```csharp
frameworkHandle.RecordStart(testCase);
frameworkHandle.RecordResult(new TestResult(testCase)
{
    Outcome = TestOutcome.Passed,
    Duration = TimeSpan.FromSeconds(0.123)
});
frameworkHandle.RecordEnd(testCase, TestOutcome.Passed);
```

## Implementation Details

### Adapter Discovery

VSTest discovers adapters automatically via:

1. **NuGet Package Structure**:
   ```
   build/netstandard2.0/
       SWTSharp.TestAdapter.dll
       SWTSharp.TestHost.dll
   ```

2. **Assembly Naming Convention**:
   - Must end with `.TestAdapter.dll`
   - Example: `SWTSharp.TestAdapter.dll` ✅
   - Example: `SWTSharp.Tests.dll` ❌

3. **Attribute Decoration**:
   ```csharp
   [FileExtension(".dll")]
   [DefaultExecutorUri("executor://SWTSharpTestExecutor")]
   public class SWTSharpTestDiscoverer : ITestDiscoverer
   ```

### Test Host Communication

**Process Launch**:
```csharp
var startInfo = new ProcessStartInfo
{
    FileName = "dotnet",
    Arguments = $"SWTSharp.TestHost.dll {testAssembly} {testFilter}",
    RedirectStandardOutput = true,
    RedirectStandardError = true
};
```

**Result Parsing**:
```csharp
process.OutputDataReceived += (sender, e) =>
{
    if (e.Data?.StartsWith("[RESULT]") == true)
    {
        var result = ParseTestResult(e.Data);
        frameworkHandle.RecordResult(result);
    }
};
```

## Benefits

### ✅ Full VSTest Integration

- Works with `dotnet test`
- Visual Studio Test Explorer support
- CI/CD pipeline integration
- Code coverage collection (via Coverlet)
- Parallel test execution (across assemblies)

### ✅ macOS Thread 1 Support

- Test host controls process entry point
- Thread 1 available for UI operations
- No NSWindow crashes
- Full Display/Shell/Widget testing

### ✅ Cross-Platform

- macOS: Separate process with Thread 1
- Windows/Linux: In-process execution (optional)
- Same test code, different execution strategies

### ✅ Standard Test Experience

- No special test attributes required
- Standard xUnit `[Fact]` and `[Theory]`
- Works with existing `DisplayFixture`
- No code changes to test classes

## Limitations

### ⚠️ Performance

Separate process execution has overhead:
- Process startup time (~100-200ms per assembly)
- Inter-process communication
- No test parallelization within assembly

**Mitigation**: Tests run in parallel across assemblies, so multiple test DLLs still benefit.

### ⚠️ Debugging

Debugging test host requires:
1. Set `VSTEST_HOST_DEBUG=1` environment variable
2. Attach debugger to test host process
3. Or debug test host directly: `dotnet run --project SWTSharp.TestHost`

### ⚠️ Test Host Complexity

Test host must:
- Parse command-line arguments
- Load test assemblies dynamically
- Marshal results to stdout
- Handle MainThreadDispatcher lifecycle

## Usage

### Installation

**In Test Project**:
```xml
<ItemGroup>
  <PackageReference Include="SWTSharp.TestAdapter" Version="1.0.0" />
  <PackageReference Include="xunit.v3" Version="3.1.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.0" />
</ItemGroup>
```

### Running Tests

**Command Line**:
```bash
dotnet test
```

**With Coverage**:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Specific Tests**:
```bash
dotnet test --filter "FullyQualifiedName~Button"
```

**Visual Studio**:
- Tests appear in Test Explorer
- Run/debug as normal
- Adapter launches automatically

### CI/CD

**No changes required** - works with existing workflows:

```yaml
- name: Run tests with coverage
  run: dotnet test --configuration Release --collect:"XPlat Code Coverage"

- name: Upload coverage to Codecov
  uses: codecov/codecov-action@v5
  with:
    directory: ./tests/SWTSharp.Tests/TestResults
```

## Development

### Building the Adapter

```bash
dotnet build tests/SWTSharp.TestAdapter
```

### Packaging

```bash
dotnet pack tests/SWTSharp.TestAdapter -c Release
```

Output: `SWTSharp.TestAdapter.1.0.0.nupkg`

### Testing the Adapter

**Local Testing**:
```bash
# Install locally
dotnet nuget push SWTSharp.TestAdapter.1.0.0.nupkg -s ~/.nuget/packages

# Reference in test project
dotnet add package SWTSharp.TestAdapter --version 1.0.0

# Run tests
dotnet test --logger:"console;verbosity=detailed"
```

**Debug Logging**:
```bash
# Enable VSTest diagnostics
export VSTEST_DIAG=1
dotnet test
```

## Comparison: Adapter vs TestRunner

| Feature | Custom Adapter | TestRunner Mode |
|---------|----------------|-----------------|
| **CI/CD Integration** | ✅ Full | ❌ None |
| **Code Coverage** | ✅ Yes | ❌ No |
| **Test Explorer** | ✅ Yes | ❌ No |
| **macOS UI Support** | ✅ Yes | ✅ Yes |
| **Performance** | ⚠️ Process overhead | ✅ Direct |
| **Debugging** | ⚠️ Requires attach | ✅ Direct |
| **Distribution** | ✅ NuGet package | ❌ Manual |
| **Setup Required** | ✅ Automatic | ⚠️ Manual script |

**Recommendation**: Use custom adapter for all scenarios. It provides the best of both worlds:
- VSTest integration (CI/CD, coverage, Test Explorer)
- macOS Thread 1 support (no crashes)
- Cross-platform compatibility
- Standard testing experience

## Future Enhancements

### 1. In-Process Fallback

For non-UI tests, detect and run in-process for performance:
```csharp
if (!RequiresUIThread(testCase))
{
    RunTestInProcess(testCase);
}
```

### 2. Test Host Pooling

Reuse test host processes to reduce overhead:
```csharp
var hostPool = new TestHostPool(maxHosts: 4);
var host = await hostPool.AcquireAsync();
```

### 3. Live Test Discovery

Support VSTest's live test discovery for faster feedback:
```csharp
public class SWTSharpTestDiscoverer : ITestDiscoverer2
{
    // Continuous discovery as tests are modified
}
```

### 4. Source-Based Discovery

Read source files directly for faster discovery:
```csharp
[FileExtension(".cs")]
public class SourceBasedDiscoverer : ITestDiscoverer
{
    // Parse C# files for [Fact] attributes
}
```

## References

- [VSTest Platform Documentation](https://github.com/microsoft/vstest-docs)
- [VSTest Adapter RFC](https://github.com/microsoft/vstest-docs/blob/main/RFCs/0004-Adapter-Extensibility.md)
- [xUnit Runner Utility](https://www.nuget.org/packages/xunit.runner.utility)
- [Microsoft.TestPlatform.ObjectModel](https://www.nuget.org/packages/Microsoft.TestPlatform.ObjectModel)

## Summary

The SWTSharp Custom VSTest Adapter provides a **production-ready solution** for running UI tests on macOS within the VSTest ecosystem. By launching tests in a separate process with Thread 1 control, it solves the fundamental incompatibility between VSTest's threading model and macOS's UI requirements **without sacrificing any VSTest features**.

This is the **recommended approach** for SWTSharp testing going forward, superseding both the dual-mode testing approach and the TestRunner script.
