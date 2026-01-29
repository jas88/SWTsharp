# Codebase Concerns

**Analysis Date:** 2026-01-29

## Tech Debt

**Incomplete Platform Widget Interface Integration:**
- Issue: 200+ TODO comments throughout codebase indicating incomplete platform widget interface implementation across dialogs, containers, and advanced widgets
- Files: `src/SWTSharp/Group.cs`, `src/SWTSharp/Spinner.cs`, `src/SWTSharp/Canvas.cs`, `src/SWTSharp/Combo.cs`, `src/SWTSharp/Text.cs`, `src/SWTSharp/Table.cs`, `src/SWTSharp/Tree.cs`, `src/SWTSharp/ToolBar.cs`, `src/SWTSharp/MenuItem.cs`, and 30+ dialog/platform files
- Impact: Property setters (Text, Visible, Enabled, Bounds, etc.) are not propagating to native platform widgets. State changes occur in C# objects but don't reflect in UI
- Fix approach: Implement platform widget interface calls systematically across all widgets. Priority: Group (IPlatformGroup), advanced widgets (Slider, Spinner), then dialogs (FontDialog, ColorDialog, MessageBox)

**Linux Platform Incomplete (Phase 5.3):**
- Issue: Linux platform is missing Slider and Spinner widget implementations
- Files: `src/SWTSharp/Platform/LinuxPlatform.cs` (lines 165, 183)
- Impact: `CreateSliderWidget()` and `CreateSpinnerWidget()` throw `NotImplementedException` on Linux. Applications using Slider/Spinner on Linux will crash at runtime
- Fix approach: Implement LinuxSlider and LinuxSpinner in `src/SWTSharp/Platform/Linux/` following existing LinuxComposite and LinuxButton patterns

**Unsafe P/Invoke Usage:**
- Issue: 1,557 P/Invoke declarations across platform implementations with minimal validation
- Files: Win32Platform (947 lines), Win32PlatformGraphics (966 lines), MacOSPlatform (1,560 lines), LinuxPlatform (792 lines)
- Impact: No validation of native handles before use. Potential null pointer dereferences or invalid handle exceptions at runtime
- Fix approach: Add precondition checks before every native API call. Document handle lifetime and ownership

**Unimplemented SafeHandle Constructors:**
- Issue: 5 SafeHandle subclasses throw NotImplementedException in Release() overrides
- Files: `src/SWTSharp/Platform/SafeHandles/MacOS/MacOSFontHandle.cs`, `MacOSImageHandle.cs`, `MacOSMenuHandle.cs`, `MacOSGraphicsHandle.cs`, `MacOSWindowHandle.cs`, `src/SWTSharp/Platform/SafeHandles/Linux/LinuxGraphicsHandle.cs`
- Impact: Resource cleanup never happens; platform resources leak when SafeHandle is garbage collected
- Fix approach: Implement ReleaseHandle() for each SafeHandle or document that these are placeholder stubs and actual cleanup is elsewhere

## Memory Leaks

**Event Handler Accumulation (LEAK-001):**
- Issue: Event table cleared in Widget.ReleaseWidget() but only on disposal. Long-lived widgets can accumulate event listeners
- Files: `src/SWTSharp/Widget.cs` (lines 115-135)
- Impact: Event listeners remain in `_eventTable` dictionary even after handlers are no longer needed. Collections growth unbounded
- Fix approach: Implement listener removal API (RemoveListener method) or automatic cleanup when listeners go out of scope. Monitor event table size in tests

**Button Callback Dictionary (LEAK-002):**
- Issue: macOS button callbacks stored in `_buttonCallbacks` dictionary with cleanup methods but no automatic invocation
- Files: `src/SWTSharp/Platform/MacOSPlatform.cs` (lines 964-976)
- Impact: ClearButtonCallbacks() and RemoveButtonCallback() exist but appear never called automatically when buttons are destroyed
- Fix approach: Wire cleanup calls into button widget Dispose() or ReleaseWidget() methods. Verify all platform button widgets call RemoveButtonCallback()

**List Items Dictionary (implicit LEAK-003):**
- Issue: Similar pattern to LEAK-002: `_listItems` dictionary accumulates items without guaranteed cleanup
- Files: `src/SWTSharp/Platform/MacOSPlatform.cs` (lines 978-988)
- Impact: ListItems remain in memory even after List widgets are disposed
- Fix approach: Implement automatic cleanup in List.Dispose() chain similar to button callback cleanup

## Fragile Areas

**Platform-Specific Graphics Context (GC) Management:**
- Files: `src/SWTSharp/Graphics/GC.cs`, `src/SWTSharp/Platform/MacOS/MacOSPlatformGraphics.cs` (665 lines), `src/SWTSharp/Platform/Win32/Win32PlatformGraphics.cs` (966 lines), `src/SWTSharp/Platform/Linux/LinuxPlatformGraphics.cs` (781 lines)
- Why fragile: GC holds native graphics handles (Quartz contexts, GDI+ objects, Cairo contexts) that must be released in exact order. Drawing code has many unsafe blocks
- Safe modification: Any changes to drawing order or color handling must be tested across all platforms. Currently no graphics unit tests
- Test coverage: Graphics package has no unit tests. Only integration tests in WidgetTests

**Threading and Display Thread Validation:**
- Files: `src/SWTSharp/Display.cs` (thread checking), `src/SWTSharp/Widget.cs` (CheckWidget), 36+ files with lock statements
- Why fragile: IsValidThread() check is only a thread identity check; doesn't prevent platform mismatches. Lock granularity varies widely
- Safe modification: Thread-safety changes need comprehensive test coverage on all platforms. Current tests may not catch race conditions
- Test coverage: Threading tests minimal; no stress tests with high contention

**Layout Manager Resolution Across Platform Implementations:**
- Files: `src/SWTSharp/Layout/GridLayout.cs`, `src/SWTSharp/Layout/FormLayout.cs`, 4 platform-specific Composite implementations (MacOSComposite, Win32Composite, LinuxComposite, Win32Group)
- Why fragile: Layout calculation done in abstract Layout classes but final positioning delegated to platform Composite.SetBounds(). Mismatches cause visual corruption
- Safe modification: Layout changes need pixel-perfect testing on all platforms. Mock-based unit tests insufficient
- Test coverage: 0% coverage of layout positioning. Only existence tests (widget creates without crash)

**Color Handling Stubs:**
- Files: 20+ TODO comments for background/foreground color setting (LinuxButton, MacOSButton, MacOSLabel, LinuxGroup, MacOSTable, etc.)
- Why fragile: SetBackground/SetForeground methods are stubs or TODO comments. Code compiles but colors don't apply
- Safe modification: Must implement proper CSS handling for Linux (GtkCssProvider), proper NSColor handling for macOS, proper GDI+ handling for Windows
- Test coverage: No color rendering tests; only color object creation tests

## Test Coverage Gaps

**Platform Widget Creation and Lifecycle:**
- What's not tested: Platform widget creation (CreateButtonWidget, CreateListWidget, etc.), widget disposal chain, callback cleanup
- Files: All `src/SWTSharp/Platform/*/` implementations
- Risk: Memory leaks undetected; handle leaks undetected; platform resources orphaned
- Priority: High - these are integration points between managed and unmanaged code

**Event Handling Across Platforms:**
- What's not tested: Event delivery consistency across Windows/macOS/Linux; event ordering; listener removal during event dispatch
- Files: Event dispatch code in Display, all platform event handlers
- Risk: Platform-specific event ordering bugs; listener cleanup failures; race conditions in async event processing
- Priority: High - events are core to UI responsiveness

**Layout Manager Calculation and Positioning:**
- What's not tested: GridLayout calculation with various child counts/sizes; FormLayout attachment resolution; StackLayout child visibility; interaction between different layouts
- Files: `src/SWTSharp/Layout/` - all layout classes
- Risk: UI rendering incorrect on some platforms; layout calculations diverge between platforms
- Priority: High - affects all interactive applications

**Dialog Implementation Across Platforms:**
- What's not tested: FileDialog, DirectoryDialog, ColorDialog, FontDialog - only tested as stubs (not exercising native dialogs)
- Files: `src/SWTSharp/Dialogs/`, `src/SWTSharp/Platform/*/` dialog implementations
- Risk: Dialogs don't function; platform dialog integration missing
- Priority: Medium - dialogs are critical for file/color selection but not always used

**Graphics Operations (Drawing, Colors, Fonts, Images):**
- What's not tested: GC.DrawLine, DrawRectangle, DrawText with various parameters; Font metrics; Image loading/scaling
- Files: `src/SWTSharp/Graphics/GC.cs`, all platform graphics implementations
- Risk: Drawing broken; text rendering corrupted; images not loaded
- Priority: Medium - affects Canvas and custom drawing applications

**Async Action Execution (Display.AsyncExec/SyncExec):**
- What's not tested: AsyncExec behavior with multiple platforms; SyncExec blocking semantics; queue behavior under high load
- Files: `src/SWTSharp/Display.cs` (AsyncExec, SyncExec methods)
- Risk: Async operations fail; UI thread deadlocks; race conditions
- Priority: Medium - affects responsive UI

## Scaling Limits

**Event Listener Accumulation:**
- Current capacity: Unbounded dictionary in Widget._eventTable
- Limit: Memory exhaustion with 10,000+ listeners on single widget (unlikely but possible in long-running apps)
- Scaling path: Implement WeakReference-based listener storage or automatic listener pruning

**Child Control Collections:**
- Current capacity: List<Control> in Composite has no size limits
- Limit: Performance degradation with 1,000+ children per composite (O(n) layout operations)
- Scaling path: Implement virtual children pattern or spatial indexing for large child counts

**Platform Handle Dictionaries:**
- Current capacity: MacOSPlatform._buttonCallbacks, _listItems dictionaries grow unbounded
- Limit: Memory growth linear with number of widgets created and destroyed
- Scaling path: Implement automatic cleanup; use WeakDictionary pattern; or add explicit lifecycle management

## Known Bugs

**Coverage Collection Disabled (Issue #TBD):**
- Symptoms: Code coverage reports disabled in CI (see README.md line 5)
- Files: `.github/workflows/ci.yml` (likely), README.md comment
- Trigger: xUnit runner on macOS requires special threading; coverage collection incompatible with Thread 1 requirement
- Workaround: Run coverage on Linux/Windows only or use alternative runner

**Compiler Warning Accumulation:**
- Symptoms: 30+ compiler warnings suppressed in SWTSharp.csproj (NoWarn settings lines 33-61 in msbuild.log)
- Files: `src/SWTSharp/SWTSharp.csproj` (NoWarn property)
- Trigger: Suppressed warnings include CA1852, CA1859, CA1707, CA2101, CA1510, CA1805, CA1822, CA1419, CA1806, CA1716, CA1720, CA1310, CA1854, CA1513, CA2208, CA1861, CA1305, CA1838, CA1711, CA1708, CA1866
- Risk: Actual issues hidden by suppression; potential subtle bugs in suppressed areas

**Dialog Methods Return null Without Implementation:**
- Symptoms: Open() methods in dialogs (FileDialog, FontDialog, ColorDialog) return null always
- Files: `src/SWTSharp/Dialogs/FileDialog.cs` (line 107), FontDialog.cs (lines 70-74), ColorDialog.cs (line 59), MessageBox.cs (line 46)
- Trigger: Call any dialog's Open() method
- Workaround: None; dialogs are non-functional stubs

## Security Considerations

**Unsafe Blocks and P/Invoke Security:**
- Risk: Unsafe code blocks (24 found) could dereference invalid pointers, causing crashes or potential memory corruption
- Files: Win32Platform.cs, MacOSPlatform.cs, LinuxPlatform.cs, all platform graphics implementations
- Current mitigation: Runtime validation of IntPtr.Zero checks in some paths but inconsistent
- Recommendations:
  1. Add precondition validation for all P/Invoke calls
  2. Use SafeHandle consistently instead of raw IntPtr
  3. Code review all unsafe blocks for pointer arithmetic correctness

**Unvalidated Native Handle Usage:**
- Risk: Application could pass invalid handle to widget constructors, causing crashes or buffer overruns
- Files: All platform widget constructors (MacOSButton, Win32Button, etc.)
- Current mitigation: None
- Recommendations:
  1. Validate parent handle before use in each platform widget factory
  2. Add try-catch around native API calls that could fail with invalid handles
  3. Log/audit handle lifetime

**P/Invoke Marshaling:**
- Risk: String marshaling, struct marshaling, callback marshaling could expose security issues
- Files: All P/Invoke method declarations (1,557 total)
- Current mitigation: Standard managed/unmanaged boundary protections
- Recommendations:
  1. Document marshaling assumptions for each P/Invoke
  2. Audit string parameter marshaling (null termination, encoding)
  3. Verify all callbacks release pinned handles

## Dependencies at Risk

**Objective-C Runtime Compatibility (macOS):**
- Risk: MacOSPlatform relies on P/Invoke to Objective-C runtime via objc.h. Changes to macOS/Xcode toolchain could break compilation
- Impact: macOS builds fail if objc headers unavailable
- Migration plan: Future: Consider using CoreGraphics/Metal instead of AppKit for widgets; or use a native macOS binding library

**GTK 3 EOL (Linux):**
- Risk: GTK 3 is in maintenance mode; GTK 4 adoption ongoing but different API
- Impact: Linux platform will eventually use deprecated library; may not compile on future Linux distributions
- Migration plan: Plan GTK 4 port; consider wxWidgets or Qt alternative if GTK 3 becomes unavailable

**Windows Win32 Long-term Support:**
- Risk: Win32 API is stable but deprecated in favor of WinUI 3. No active feature development
- Impact: Windows platform will eventually be legacy, though unlikely to break in next 10 years
- Migration plan: Consider WinUI 3 port for new applications; current Win32 implementation will continue to work

## Performance Bottlenecks

**Widget Creation Overhead:**
- Problem: Each widget creation requires platform widget creation + event table initialization + parent hierarchy traversal
- Files: `src/SWTSharp/Control.cs` (constructor), all platform widget factories
- Cause: No pooling or lazy creation; every child causes full initialization
- Improvement path: Implement widget pooling for frequently created/destroyed widgets (ListItem, TableItem); defer event table creation until listeners added

**Layout Calculation on Every Property Change:**
- Problem: Setting Visible, Enabled, or Text on Control calls UpdateVisible/UpdateEnabled/UpdateText which potentially trigger layout
- Files: `src/SWTSharp/Control.cs`, `src/SWTSharp/Composite.cs`
- Cause: No deferred layout batching; each property change recalculates
- Improvement path: Implement layout deferral API (BeginLayout/EndLayout); batch property changes; only recalculate affected subtree

**Synchronous Platform Event Processing:**
- Problem: ReadAndDispatch() blocks on platform event loop; AsyncExec() actions processed during event loop but no priority queue
- Files: `src/SWTSharp/Display.cs` (ReadAndDispatch, ProcessAsyncActions)
- Cause: Single event queue; no async/await support; blocking on native APIs
- Improvement path: Implement event priority levels; consider async/await integration; profile macOS NSRunLoop polling

**Graphics Context Allocation:**
- Problem: Every paint operation may allocate new GC; no GC pooling
- Files: `src/SWTSharp/Graphics/GC.cs`, `src/SWTSharp/Canvas.cs`
- Cause: No resource pooling pattern implemented
- Improvement path: Implement GC object pool; reuse graphics contexts across paint cycles

---

*Concerns audit: 2026-01-29*
