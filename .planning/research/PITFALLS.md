# Pitfalls Research: Cross-Platform GUI Testing

## Executive Summary

This document identifies critical pitfalls specific to cross-platform GUI testing for SWT-to-.NET porting projects. Based on analysis of the SWTSharp codebase, these pitfalls represent real failure modes found in brownfield GUI porting projects with incomplete implementations across Windows (Win32), macOS (Cocoa), and Linux (GTK3).

**Current State**: 200+ TODOs, dialogs returning null, SafeHandle resource leaks, no graphics tests, macOS threading issues.

---

## 1. Threading and UI Tests

### Pitfall: Platform-Specific Thread Affinity Requirements

**Description**: Each GUI platform has different main thread requirements that are incompatible with standard .NET test runners.

**Real Example from SWTSharp**:
- **macOS**: Requires Thread 1 to run `CFRunLoop` for GCD dispatch to work. Standard `dotnet test` runs on any thread, causing `dispatch_sync_f` to deadlock.
- **Windows**: Win32 uses message pumps (`GetMessage`/`DispatchMessage`) but is more forgiving about which thread.
- **Linux**: GTK3 requires `gtk_main()` on main thread but can use `Xvfb` for headless testing.

**Warning Signs**:
- Tests hang indefinitely on macOS but pass on Windows/Linux
- `timeout-minutes: 5` in CI configuration (line 83 of `.github/workflows/ci.yml`)
- Custom test adapter with separate test host process (`SWTSharpTestExecutor.cs`)
- Comments like "Thread 1 MUST be running CFRunLoop" (`GCDThreadingTests.cs:186`)
- Tests with `MainThreadDispatcher.IsInitialized` checks (lines 100-104)

**Prevention Strategy**:
1. **Phase 1 (Foundation)**: Design test infrastructure from day 1:
   - Create platform-specific test hosts (like `SWTSharp.TestHost`)
   - Implement custom VSTest adapter for Thread 1 control on macOS
   - Document threading model in `tests/README.md`

2. **Phase 2 (Core Implementation)**:
   - All GUI tests MUST use `Display.SyncExec()` or `Display.AsyncExec()`
   - Never call native GUI functions directly from test threads
   - Add assertions: `Assert.True(Display.Current != null, "Must run on UI thread")`

3. **CI Configuration**:
   - Use separate test runners per platform (don't force one runner for all)
   - Set conservative timeouts (5-10 minutes) to catch deadlocks early
   - Enable diagnostic logging: `SWTSHARP_DEBUG=1`

**Phase Mapping**:
- **Must address in Phase 1**: Threading infrastructure (blocking issue)
- **Continuously validate in Phases 2-4**: Every widget must work with thread dispatch

---

## 2. Platform-Specific Behavior Differences

### Pitfall: Assuming Identical Widget Behavior Across Platforms

**Description**: SWT widgets have subtle platform-specific differences that cause tests to pass on one platform and fail on others.

**Real Examples**:
- **Dialog return values**: `FileDialog.Open()` returns `null` (stub implementation in `FileDialog.cs:108`)
- **Button styles**: Win32 `BS_PUSHBUTTON` vs macOS `NSButton` types vs GTK `GtkButton`
- **Text measurement**: Font metrics differ between GDI+, CoreText, and Pango
- **Event ordering**: Key events fire in different sequences on each platform
- **Default values**: Window sizes, colors, fonts all platform-dependent

**Warning Signs**:
- Dialogs returning `null` instead of results
- Tests with platform-specific conditionals: `if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))`
- 200+ TODO comments scattered across platform implementations
- Platform-specific test files: `MacOSButtonTests.cs`, `WindowsButtonTests.cs`, `LinuxButtonTests.cs`
- Stub implementations marked with `// TODO: Implement X through platform widget interface`

**Prevention Strategy**:
1. **Phase 1 (Foundation)**:
   - Define abstract test base classes that test widget contracts, not implementations
   - Document expected behavior differences in `docs/PLATFORM_DIFFERENCES.md`
   - Create platform-specific test traits: `[Trait("Platform", "macOS")]`

2. **Phase 2 (Core Implementation)**:
   - Implement behavior parity testing: same input → same output on all platforms
   - For dialogs: Test cancellation, file selection, multi-selection separately
   - For events: Test event data properties, not just that event fired
   - Use behavior-driven tests: "GIVEN open file dialog, WHEN user cancels, THEN return null"

3. **Phase 3 (Polish)**:
   - Create cross-platform test matrix in CI
   - Add visual comparison tests (screenshot diffing with tolerance for platform differences)
   - Document intentional differences (e.g., native file picker UI will always differ)

**Phase Mapping**:
- **Phase 1**: Document known differences (non-blocking)
- **Phase 2**: Fix behavior parity for core widgets (blocking for API stability)
- **Phase 3**: Add visual testing for complex widgets (nice-to-have)

---

## 3. Resource Cleanup and Leaks

### Pitfall: SafeHandle Leaks in Graphics Operations

**Description**: Graphics contexts (HDC, CGContext, cairo_t) are native resources that leak if not properly disposed. Standard .NET finalizers run too late, causing resource exhaustion in long-running tests.

**Real Example from SWTSharp**:
- `SafeGraphicsHandle` base class exists (`SafeGraphicsHandle.cs`)
- Platform implementations: `Win32GraphicsHandle`, `MacOSGraphicsHandle`, `LinuxGraphicsHandle`
- **Warning**: No tests for graphics context cleanup
- **Warning**: Widget disposal tests exist (`TestHelpers.cs:82-93`) but no graphics-specific tests

**Warning Signs**:
- Platform-specific graphics handle classes but no tests for them
- TODOs in graphics implementations (`MacOSPlatformGraphics.cs:2`)
- `IDisposable` implementations without corresponding disposal tests
- CI runs out of memory on long test suites
- Handle count increases over time (detectable via OS tools)

**Prevention Strategy**:
1. **Phase 1 (Foundation)**:
   - Create `GraphicsContextTests` test class
   - Test pattern: Create 1000+ contexts in loop, verify all are released
   - Use `WeakReference` to verify finalization: `Assert.True(weakRef.IsAlive == false)`
   - Add tests for `using` block disposal and exception paths

2. **Phase 2 (Core Implementation)**:
   - Every widget with graphics MUST dispose context in `Dispose()`
   - Use `try/finally` or `using` in all paint handlers
   - Add assertions in destructors: `Debug.Assert(handle == IntPtr.Zero, "Handle leaked")`
   - Enable leak detection in debug builds

3. **Test Infrastructure**:
   - Create `ResourceTracker` helper to monitor handle counts
   - Add test attribute `[ResourceLeak]` that fails if handles increase
   - Run stress tests: 10,000 widget create/dispose cycles

**Code Pattern to Prevent**:
```csharp
// BAD: Context may leak if paint throws exception
void OnPaint() {
    var gc = GetGraphicsContext();
    DoPaint(gc);  // If this throws, gc is not disposed
    gc.Dispose();
}

// GOOD: Context always disposed
void OnPaint() {
    using var gc = GetGraphicsContext();
    DoPaint(gc);
}
```

**Phase Mapping**:
- **Phase 1**: Add graphics handle disposal tests (blocking - prevents accumulation)
- **Phase 2**: Fix all leak paths in widgets (blocking for stability)
- **Phase 3**: Add continuous leak monitoring in CI (nice-to-have)

---

## 4. CI Environment Challenges

### Pitfall: Headless Testing Mismatches Production Environments

**Description**: GUI tests in CI run in virtualized/headless environments (Xvfb, no WindowServer) that behave differently from developer machines with real displays.

**Real Example from SWTSharp CI** (`.github/workflows/ci.yml`):
- **Windows**: Real display (GitHub Actions runners have UI)
- **macOS**: Real WindowServer with loginwindow (line 77 comment)
- **Linux**: Virtual display via Xvfb (lines 126-141), requires GTK3 + WebKitGTK setup

**Warning Signs**:
- Complex CI setup with platform-specific display management
- Tests pass locally but fail in CI (or vice versa)
- Xvfb configuration requires specific screen resolution (1024x768x24)
- WebKitGTK version conflicts between 4.0 and 4.1 (lines 132-136)
- CI needs `DISPLAY=:99` environment variable
- 5-minute timeout on macOS tests to prevent hanging

**Prevention Strategy**:
1. **Phase 1 (Foundation)**:
   - Document CI environment differences in `tests/README.md`
   - Create "smoke test" suite that runs in < 30 seconds
   - Test display detection: `Assert.NotNull(Display.Default)`
   - Separate test traits: `[Trait("Category", "NonGUI")]` vs GUI tests

2. **CI Configuration Best Practices**:
   - **Linux**: Start Xvfb BEFORE building (not just before tests)
   - **Linux**: Install both WebKitGTK versions if multi-targeting (4.0 and 4.1)
   - **Linux**: Use `sleep 3` after Xvfb start to ensure it's ready
   - **macOS**: Use `timeout-minutes` to prevent infinite hangs
   - **All platforms**: Enable debug logging in CI: `SWTSHARP_DEBUG=1`

3. **Test Design**:
   - Categorize tests: `NonGUI` (can run headless) vs `RequiresDisplay`
   - Mock native dialogs for unit tests (test logic, not OS integration)
   - Integration tests should verify native behavior, but keep count small

**Environment Setup Checklist**:
```yaml
# Linux CI setup
- Install Xvfb
- Install GTK3 (libgtk-3-0, libglib2.0-0, libx11-dev)
- Install WebKitGTK 4.1 AND 4.0 (for multi-targeting)
- Start Xvfb on display :99
- Export DISPLAY=:99
- Wait 3 seconds for Xvfb initialization
```

**Phase Mapping**:
- **Phase 1**: CI infrastructure working with basic smoke tests (blocking)
- **Phase 2**: Add GUI tests gradually, ensure they work in CI (blocking per widget)
- **Phase 3**: Add visual comparison tests (requires screenshot capability)

---

## 5. Test Flakiness

### Pitfall: Timing-Dependent Tests Without Proper Synchronization

**Description**: GUI tests that rely on timing (`Thread.Sleep`) instead of proper synchronization primitives are inherently flaky and fail unpredictably in CI.

**Real Examples from SWTSharp**:
- `TestHelpers.WaitFor()` polls every 10ms with timeout (lines 151-163)
- Browser navigation test uses polling: `while (!complete && DateTime.UtcNow < timeout)` (`BrowserTests.cs`)
- Thread safety issues in `MainThreadDispatcher` can cause race conditions

**Warning Signs**:
- Tests that pass 99% of the time but occasionally fail
- CI failures that disappear when re-run
- Tests with `Thread.Sleep()` calls
- Polling loops without proper cancellation
- Time-based assertions: "Operation should complete within 5 seconds"

**Prevention Strategy**:
1. **Phase 1 (Foundation)**:
   - Replace polling with event-based synchronization:
     ```csharp
     // BAD: Polling
     while (!condition() && DateTime.UtcNow < timeout) {
         Thread.Sleep(10);
     }

     // GOOD: Event-based
     var completed = new ManualResetEventSlim();
     widget.OperationComplete += (s, e) => completed.Set();
     widget.StartOperation();
     Assert.True(completed.Wait(timeout));
     ```

2. **Test Infrastructure**:
   - Create `SyncContext` helper for UI thread synchronization
   - Use `TaskCompletionSource` for async operations
   - Avoid bare `Thread.Sleep` - always have timeout + reason

3. **Flakiness Detection**:
   - Run tests 100 times in CI to detect rare failures
   - Use test retry attribute (xUnit: `[Retry(3)]`) only for known-flaky external dependencies
   - Never use retry to hide bugs - investigate every retry

**Phase Mapping**:
- **Phase 1**: Eliminate polling in test infrastructure (blocking)
- **Phase 2**: Fix flaky tests as discovered (continuous)
- **Phase 3**: Add flakiness detection to CI (nice-to-have)

---

## 6. Common SWT Porting Mistakes

### Pitfall: Incomplete Widget Lifecycle Implementation

**Description**: SWT widgets have complex lifecycle (create → configure → layout → paint → dispose) that must be implemented completely for each platform.

**Real Examples from SWTSharp**:
- 200+ TODO comments indicating incomplete implementations
- Dialogs return null (stub implementations)
- Missing event handlers in platform implementations
- No graphics tests (paint/draw operations untested)

**Warning Signs**:
- Widgets "mostly work" but have edge case bugs
- Parent/child relationships not properly maintained
- Events not firing or firing in wrong order
- Memory leaks from incomplete disposal
- Layout calculations incorrect (widgets overlap or disappear)

**Prevention Strategy**:
1. **Define Widget Completion Criteria**:
   ```markdown
   A widget is "complete" when:
   - [ ] All public properties implemented on all 3 platforms
   - [ ] All events fire correctly (with tests)
   - [ ] Disposal releases all native resources (verified by leak test)
   - [ ] Layout calculations match SWT behavior (measured, not visual comparison)
   - [ ] TODO count for this widget is zero
   ```

2. **Phase 2 (Core Implementation)**:
   - Implement widgets in priority order (Shell → Button → Label → Text → ...)
   - Don't start next widget until previous is "complete" by criteria above
   - Add checkbox list in issue/PR to track completion

3. **Architecture Mistakes to Avoid**:
   - ❌ Don't copy-paste platform implementations (maintain DRY principle)
   - ❌ Don't implement Windows first then "port" to macOS/Linux (platform-specific from start)
   - ❌ Don't skip event implementation "for now" (events are core contract)
   - ❌ Don't use `throw new NotImplementedException()` in production code (return sensible defaults)

**Phase Mapping**:
- **Phase 1**: Define completion criteria (blocking for planning)
- **Phase 2**: Apply criteria to each widget (blocking per widget)
- **Phase 4**: Verify all widgets meet criteria (blocking for release)

---

## 7. Prevention Strategies Summary

### Strategy 1: Test-First Development (Phase 1)

**What**: Write platform-agnostic test for expected behavior BEFORE implementing widget.

**Why**: Prevents "works on my platform" syndrome.

**How**:
1. Write test that runs on all 3 platforms
2. Test fails on all 3 platforms (stub implementations)
3. Implement platform-specific code until test passes
4. Test now passes on all 3 platforms

**Example**:
```csharp
// Write this FIRST (Phase 1)
[Fact]
public void Button_Click_FiresEvent()
{
    using var display = new Display();
    using var shell = new Shell(display);
    using var button = new Button(shell, SWT.PUSH);

    bool clicked = false;
    button.Click += (s, e) => clicked = true;

    // Simulate click (platform-agnostic)
    button.NotifyListeners(SWT.Selection, new Event());

    Assert.True(clicked);
}

// Then implement platform-specific code (Phase 2)
```

### Strategy 2: Resource Leak Detection (Phase 1)

**What**: Automated tests that fail if native resources leak.

**How**:
```csharp
[Fact]
public void GraphicsContext_Dispose_ReleasesHandle()
{
    SafeGraphicsHandle? handle = null;
    var weakRef = new WeakReference(handle);

    void CreateAndDispose() {
        using var display = new Display();
        using var shell = new Shell(display);
        handle = SafeGraphicsHandle.CreatePlatformGraphicsContext(shell.Handle);
        // handle goes out of scope, should be collected
    }

    CreateAndDispose();
    GC.Collect();
    GC.WaitForPendingFinalizers();

    Assert.False(weakRef.IsAlive, "Handle leaked - finalizer did not run");
}
```

### Strategy 3: Platform Matrix Testing (Phase 3)

**What**: CI runs same test suite on Windows, macOS, Linux.

**How** (already in `.github/workflows/ci.yml`):
- 3 separate jobs: `test-windows`, `test-macos`, `test-linux`
- Each job runs full test suite
- Upload coverage separately per platform
- Fail build if ANY platform fails

**Extension**: Add visual comparison testing (Phase 3)
- Screenshot each widget on each platform
- Compare to reference images with tolerance
- Flag unexpected differences for manual review

### Strategy 4: Fail-Fast on Missing Implementations (Phase 1)

**What**: Instead of TODO comments, use runtime checks that fail tests.

**Bad Pattern**:
```csharp
public string? Open()
{
    // TODO: Implement file dialog
    return null;  // Silent failure, tests might not catch this
}
```

**Good Pattern**:
```csharp
public string? Open()
{
    CheckWidget();
    var result = Platform.OpenFileDialog(this);
    if (result == null && !IsCancelledByUser)
    {
        throw new InvalidOperationException(
            "Dialog implementation incomplete for this platform. " +
            $"Platform: {Environment.OSVersion.Platform}");
    }
    return result;
}
```

### Strategy 5: Documentation-Driven Development (Phase 1)

**What**: Document expected behavior BEFORE writing code.

**Why**: Prevents "feature creep" and ensures all platforms implement same contract.

**Example**: `docs/widgets/FileDialog.md`
```markdown
# FileDialog Contract

## Methods
### Open() -> string?
- Returns: Full path to selected file, or null if user cancelled
- Throws: InvalidOperationException if parent shell is disposed
- Thread: Must be called from UI thread (Display.SyncExec)

## Platform Differences
- Windows: Uses native GetOpenFileName API
- macOS: Uses NSOpenPanel with modern UI
- Linux: Uses GtkFileChooserDialog

## Test Coverage
- [x] Returns path when file selected
- [x] Returns null when user cancels
- [x] Throws when parent disposed
- [x] Respects FilterExtensions property
- [x] Handles multiple file selection (MULTI style)
```

---

## Phase-Specific Checklist

### Phase 1 (Foundation) - Must Address
- [ ] Threading infrastructure (macOS Thread 1 requirement)
- [ ] Test infrastructure (custom VSTest adapter)
- [ ] CI setup (Xvfb, WindowServer, display management)
- [ ] Resource leak detection tests
- [ ] Widget completion criteria defined
- [ ] Platform difference documentation

### Phase 2 (Core Implementation) - Per Widget
- [ ] Test-first: Write cross-platform test
- [ ] Implement all 3 platforms simultaneously
- [ ] Verify event firing on all platforms
- [ ] Add disposal tests with leak detection
- [ ] Zero TODOs for this widget
- [ ] CI passes on all 3 platforms

### Phase 3 (Polish) - Nice to Have
- [ ] Visual comparison tests
- [ ] Performance benchmarks
- [ ] Flakiness detection (100x test runs)
- [ ] Stress tests (10,000+ widget cycles)
- [ ] Memory profiling in CI

### Phase 4 (Validation) - Before Release
- [ ] All widgets pass completion criteria
- [ ] No SafeHandle leaks detected
- [ ] CI success rate > 99.5% (max 0.5% flakiness)
- [ ] Zero high-priority TODOs
- [ ] All dialogs return non-null results

---

## Conclusion

The most critical pitfalls for SWTSharp are:

1. **macOS threading** (blocks all GUI tests, must fix in Phase 1)
2. **SafeHandle leaks** (causes resource exhaustion, must test in Phase 1)
3. **Incomplete widget implementations** (200+ TODOs, systematically address in Phase 2)
4. **CI environment differences** (must stabilize in Phase 1 before adding GUI tests)

Prevention requires:
- **Up-front investment in test infrastructure** (Phase 1, ~2-3 weeks)
- **Disciplined widget implementation** (Phase 2, complete each widget fully before moving to next)
- **Continuous validation** (CI must pass on all 3 platforms for every commit)

The brownfield nature of this project means addressing these pitfalls will require refactoring existing code, not just adding new features. Budget time for cleanup in addition to new development.
