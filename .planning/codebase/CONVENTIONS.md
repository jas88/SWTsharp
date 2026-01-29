# Coding Conventions

**Analysis Date:** 2026-01-29

## Naming Patterns

**Files:**
- Pascal case: `Color.cs`, `GridLayout.cs`, `ButtonTests.cs`
- Namespace-based organization: `SWTSharp.Graphics`, `SWTSharp.Layout`, `SWTSharp.Events`
- Interface files use `I` prefix: `ISelectionListener.cs`, `IPlatform.cs`
- Platform-specific files use naming patterns: `Win32Platform_Label.cs`, `LinuxPlatform_Combo.cs`, `MacOSWindowHandle.cs`

**Functions:**
- Pascal case for public methods: `WidgetSelected()`, `WidgetDefaultSelected()`, `CreateTestShell()`, `RunOnUIThread()`
- Camel case for private methods: `CheckDisposed()`, `ReleaseHandle()`
- Property accessors: `get { }` and `set { }` inline or auto properties with `{ get; set; }`
- Test methods follow naming convention: `[Fact] public void Widget_Feature_ShouldExpectation()`
  - Pattern: `{WidgetName}_{Feature}_{ExpectedBehavior}()`
  - Example: `Button_Create_ShouldSucceed()`, `Text_Append_AfterDispose_ShouldThrow()`

**Variables:**
- Camel case: `red`, `green`, `blue`, `shell`, `widget`, `testValue`
- Private fields use underscore prefix: `_disposed`, `_uiThread`, `_cacheWidth`, `_cachedColumnWidths`
- Loop variables: `i`, `shell`

**Types:**
- Public classes: `Button`, `Text`, `Color`, `GridLayout`
- Abstract base classes: `Resource`, `Layout`, `Widget`
- Interfaces: `ISelectionListener`, `IPlatform`, `IDisposable`
- Exceptions: `SWTException`, `SWTDisposedException`, `SWTInvalidThreadException`
- Collections: `List<T>`, `Dictionary<TKey, TValue>` (Microsoft.Collections)

## Code Style

**Formatting:**
- Target: .NET 9.0 (netstandard2.0, net8.0, net9.0 for library)
- ImplicitUsings: enabled
- Nullable: enabled (strict null checking)
- LangVersion: latest
- Indentation: 4 spaces (inferred from code)
- Brace style: Allman (opening braces on new line)
- Line length: No explicit limit observed, but code stays readable

**Linting:**
- TreatWarningsAsErrors: true
- EnforceCodeStyleInBuild: true
- EnableNETAnalyzers: true
- AnalysisLevel: latest
- AnalysisMode: Recommended
- GenerateDocumentationFile: true
- Specific analyzer suppressions documented in `.csproj` files:
  - CA1852: Sealed class suppressions for partial platform classes
  - CA1859: Concrete types vs interfaces (for cross-platform design)
  - CA1707, CA2101, CA1510, CA1805, CA1822, CA1419, CA1806, CA1716, CA1720: P/Invoke and cross-platform concerns
  - CA1310, CA1854, CA1513: String comparison and LINQ patterns
  - CA1416: Platform compatibility checks

## Import Organization

**Order:**
1. System namespaces (System, System.Collections, System.Runtime.InteropServices, etc.)
2. SWTSharp core namespaces (using SWTSharp)
3. SWTSharp nested namespaces (using SWTSharp.Graphics, using SWTSharp.Platform)
4. Test infrastructure (using Xunit, using NSubstitute)

**Examples:**
```csharp
// From Color.cs
using SWTSharp.Platform;

namespace SWTSharp.Graphics;
```

```csharp
// From ButtonTests.cs
using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;
```

**Path Aliases:**
- Not explicitly used; imports are relative to namespace declarations
- Global usings via `.csproj` ItemGroup: `<Using Include="Xunit" />` in test projects

## Error Handling

**Patterns:**
- Custom exceptions inherit from `SWTException`: `SWTDisposedException`, `SWTInvalidThreadException`
- Disposed state checks: `CheckDisposed()` method throws `ObjectDisposedException` if resource is disposed
- Thread safety violations throw: `SWTInvalidThreadException`
- Platform incompatibility throws: `SWTException(SWT.ERROR_NOT_IMPLEMENTED, "message")`
- Argument validation: `ArgumentNullException(nameof(parameter))` for null checks
- Platform-specific exceptions: `PlatformNotSupportedException()` in helper methods

**Example from `Resource.cs`:**
```csharp
protected void CheckDisposed()
{
    if (disposed)
    {
        throw new ObjectDisposedException(GetType().Name);
    }
}
```

**Example from `Color.cs`:**
```csharp
private IntPtr CreatePlatformColor()
{
    var platformColor = Device.Platform as IPlatformGraphics;
    if (platformColor == null)
    {
        throw new SWTException(SWT.ERROR_NOT_IMPLEMENTED, "Platform does not support graphics operations");
    }
    return platformColor.CreateColor(red, green, blue);
}
```

## Logging

**Framework:** Console output only (Console.WriteLine, Console.Error.WriteLine)

**Patterns:**
- Test infrastructure logging: `Console.WriteLine()` for diagnostic output
- Module initializer logging: Thread ID tracking to temp file at `Path.GetTempPath()`
- No structured logging framework (Serilog, NLog, etc.) in use
- Logging used primarily for test diagnostics and debugging platform initialization

**Example from `ModuleInitializer.cs`:**
```csharp
var logPath = Path.Combine(Path.GetTempPath(), "test-thread-log.txt");
File.AppendAllText(logPath, msg + "\n");
```

## Comments

**When to Comment:**
- XML documentation for all public types and public members
- Inline comments for complex platform-specific logic
- Explaining "why" rather than "what" (code should be self-explanatory)
- Platform quirks and workarounds
- TODOs for incomplete implementations

**JSDoc/TSDoc:**
- Uses XML documentation comments (C# style):
  - `/// <summary>` for brief description
  - `/// <param name="x">` for parameters
  - `/// <returns>` for return values
  - `/// <remarks>` for detailed explanation

**Example:**
```csharp
/// <summary>
/// Gets the red component (0-255).
/// </summary>
public int Red
{
    get
    {
        CheckDisposed();
        return red;
    }
}
```

## Function Design

**Size:**
- Generally under 50 lines; complex layout calculations may be longer
- Test helper methods: 10-30 lines
- Widget property implementations: 5-20 lines

**Parameters:**
- Prefer specific types over `object` (generics used extensively)
- Generic type parameters for flexibility: `T`, `TProp` in test helpers
- Factory pattern with lambdas: `Func<Shell, T>` for widget creation
- Callbacks and actions: `Action`, `Func<T>` for async operations

**Return Values:**
- Explicit return statements; no implicit returns
- Nullable reference types used properly: `T?` for optional returns
- Result unwrapping with `!` operator when non-null guarantee exists

## Module Design

**Exports:**
- Public classes, interfaces, and exceptions exported directly from namespace
- File-scoped namespaces: `namespace SWTSharp.Graphics;` (C# 11+)
- All public API members have XML documentation
- Internal/private members lack documentation by suppression rule (CS1591)

**Barrel Files:**
- Not used; each widget/component has its own file
- Namespaces provide logical grouping (SWTSharp.Graphics, SWTSharp.Layout, SWTSharp.Events)

**Disposal Pattern:**
- IDisposable implementation with `Dispose(bool)` protected method
- Finalizer for resource cleanup: `~Resource()` calls `Dispose(false)`
- `GC.SuppressFinalize(this)` after successful disposal
- CheckDisposed() guards in getters/setters

---

*Convention analysis: 2026-01-29*
