# Research Summary

**Project:** SWTSharp - .NET port of Eclipse SWT
**Goal:** Full test coverage and API compatibility with Java SWT 4.x
**Platforms:** Windows (Win32), macOS (Cocoa), Linux (GTK3)
**Researched:** 2026-01-29
**Confidence:** HIGH

## Executive Summary

SWTSharp is a cross-platform .NET GUI framework porting Eclipse SWT 4.x with native platform widgets. The project is approximately 60% complete with 89 widget/class files implemented but 200+ TODOs indicating incomplete implementations. The critical challenge is testing across three fundamentally different native platforms (Win32, Cocoa, GTK3), each with unique threading models and resource management requirements.

The recommended approach is a **three-layer testing strategy**: (1) cross-platform unit tests with mocked `IPlatform` interfaces, (2) platform-specific integration tests using custom xUnit attributes (`[WindowsFact]`, `[LinuxFact]`, `[MacOSFact]`), and (3) a custom test runner for macOS Thread 1 requirements. The existing test infrastructure with xUnit 2.9.3, NSubstitute, and Coverlet provides a solid foundation. Add Verify for snapshot testing and BenchmarkDotNet for performance validation.

Key risks include macOS threading (UI must run on Thread 1 or tests deadlock), SafeHandle resource leaks in graphics operations, and the 200+ incomplete widget implementations. These must be addressed systematically: threading infrastructure in Phase 1, widget completion criteria enforced per-widget in Phase 2, with continuous CI validation on all three platforms.

## Key Findings

### Testing Stack

The current stack is well-chosen and should be retained. xUnit 2.9.3 provides platform-specific test filtering, NSubstitute 5.3.0 handles interface mocking for the platform abstraction layer, and Coverlet 6.0.4 delivers cross-platform coverage collection. The custom VSTest adapter for macOS Thread 1 support is architecturally sound but needs implementation completion.

**Core technologies:**
- **xUnit 2.9.3:** Unit testing framework - already integrated, supports custom test adapters critical for macOS
- **NSubstitute 5.3.0:** Mocking framework - clean syntax for testing SWT's interface-heavy architecture
- **Coverlet 6.0.4:** Coverage collection - cross-platform, integrates with GitHub Actions
- **Verify 28.x (add):** Snapshot testing - critical for widget layout, graphics output, complex object assertions
- **BenchmarkDotNet 0.15.4 (add):** Performance testing - essential for validating parity with Java SWT

### API Coverage Gap Analysis

Current implementation is ~60% complete. Critical packages missing entirely: Drag & Drop, Accessibility, Printing.

**Must have (table stakes):**
- Complete existing 173+ TODOs in implemented widgets
- Drag & Drop (org.eclipse.swt.dnd) - critical for Eclipse integration
- Browser events/listeners - modern apps require embedded web content
- StyledText completion - Eclipse text editor depends on it
- Accessibility (org.eclipse.swt.accessibility) - compliance requirement

**Should have (competitive):**
- Async/await Display events - modern .NET patterns
- Fluent API extension methods - improved developer experience
- Nullable reference types - reduces null reference exceptions
- LINQ for widget trees - query widget hierarchies naturally

**Defer (v2+):**
- OpenGL package - use OpenTK instead
- Animation framework - nice-to-have
- SVG support - nice-to-have
- Hot reload support - complex, low priority

### Architecture Recommendations

Test architecture uses a three-layer approach with clear separation. The infrastructure layer provides `TestBase` and `WidgetTestBase` with shared Display fixture and thread synchronization. Widget tests validate public API behavior through mocked platform interfaces. Platform tests use real native APIs for integration validation. The custom test runner handles macOS Thread 1 requirement through process isolation with GCD dispatch.

**Major components:**
1. **Infrastructure Layer** - TestBase, WidgetTestBase, DisplayFixture, PlatformFacts (thread sync, platform detection)
2. **Widget Test Layer** - Cross-platform behavioral tests in `tests/Widgets/` using mocked IPlatform
3. **Platform Test Layer** - Platform-specific integration tests in `tests/Platform/` with real native calls
4. **Test Execution Layer** - Custom VSTest adapter and TestHost for macOS Thread 1 requirement

### Critical Pitfalls to Avoid

1. **macOS Thread 1 Deadlock** - All UI operations MUST run on Thread 1. Standard `dotnet test` runs on arbitrary threads. Use custom test runner with `Display.SyncExec()` for all GUI operations. Already partially implemented in `SWTSharp.TestHost`.

2. **SafeHandle Resource Leaks** - Graphics contexts (HDC, CGContext, cairo_t) leak if not disposed. No graphics disposal tests currently exist. Add stress tests: create/dispose 10,000+ contexts, verify all released.

3. **Platform Behavior Differences** - Dialogs returning null (stub implementations), font metrics differ, event ordering varies. Define widget completion criteria: all platforms must pass identical behavioral tests.

4. **CI Environment Mismatch** - Linux needs Xvfb, macOS needs WindowServer access, WebKitGTK version conflicts. Current CI setup is mostly correct but needs display initialization before tests start.

5. **Test Flakiness from Timing** - `TestHelpers.WaitFor()` uses polling. Replace with event-based synchronization using `ManualResetEventSlim` or `TaskCompletionSource`.

## Implementation Priorities

Based on the research, recommended priority order:

1. **Complete test infrastructure (Phase 1)** - macOS threading blocks all GUI tests; this is a prerequisite for everything else
2. **Finish existing widgets (Phase 2a)** - 173+ TODOs must reach zero; don't start new features while existing ones are incomplete
3. **Implement Drag & Drop (Phase 2b)** - high user value, required for Eclipse RCP applications
4. **Complete Browser integration (Phase 2c)** - modern apps need embedded web content
5. **Add Accessibility (Phase 3)** - compliance requirement for enterprise/government customers
6. **Finish StyledText (Phase 3)** - complex but essential for text editing applications
7. **Add Printing support (Phase 4)** - desktop application requirement

## Implications for Roadmap

Based on research, suggested phase structure:

### Phase 1: Test Infrastructure Foundation
**Rationale:** macOS threading is a blocking issue - no GUI tests can run reliably until Thread 1 requirement is solved
**Delivers:** Working test infrastructure on all 3 platforms, CI green across Windows/macOS/Linux
**Addresses:** Test framework completion, custom VSTest adapter, platform detection
**Avoids:** macOS Thread 1 deadlock, CI environment mismatch

### Phase 2: Core Widget Completion
**Rationale:** 200+ TODOs indicate incomplete implementations; must stabilize before adding features
**Delivers:** All existing widgets pass completion criteria (zero TODOs, all platforms, disposal verified)
**Uses:** xUnit, NSubstitute, Verify for snapshot testing
**Implements:** Widget test layer with full coverage
**Sub-phases:**
- 2a: Core widgets (Button, Label, Text, Combo, List, Shell, Composite)
- 2b: Container widgets (TabFolder, ToolBar, Menu, Group)
- 2c: Complex widgets (Table, Tree, Browser)

### Phase 3: Missing Critical Packages
**Rationale:** DND, Accessibility, StyledText are required for Eclipse compatibility
**Delivers:** Complete implementations for missing table-stakes packages
**Addresses:** Drag & Drop, Accessibility, StyledText, custom widgets
**Avoids:** Incomplete widget lifecycle (test-first development)

### Phase 4: Polish and Performance
**Rationale:** Performance parity with Java SWT, advanced features
**Delivers:** Benchmarks, Printing, remaining dialogs, platform refinements
**Uses:** BenchmarkDotNet for performance validation
**Implements:** Performance benchmarks, stress tests, visual regression tests

### Phase Ordering Rationale

- **Phase 1 before Phase 2:** Cannot validate widget fixes without working test infrastructure
- **Phase 2 before Phase 3:** Don't add new packages while existing implementations have 200+ TODOs
- **Core widgets before complex widgets:** Shell/Composite/Button are dependencies for Table/Tree/Browser
- **DND before Accessibility:** DND is more commonly used; Accessibility can follow same patterns

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 2c (Browser):** Complex WebView2/WebKit/WebKitGTK integration, version-specific APIs
- **Phase 3 (DND):** Platform DND APIs differ significantly; needs careful abstraction design
- **Phase 3 (Accessibility):** Platform AT APIs (UIA, NSAccessibility, ATK) have different models

Phases with standard patterns (skip research-phase):
- **Phase 1:** Test infrastructure patterns well-documented; current codebase has working examples
- **Phase 2a/2b:** Basic widget patterns established; follow existing Button/Label implementation

## Success Criteria

- [ ] All tests pass on Windows, macOS, and Linux in CI
- [ ] Zero TODOs in core widget implementations
- [ ] Code coverage >80% across all platforms
- [ ] No SafeHandle leaks (verified by stress tests)
- [ ] macOS tests complete without 5-minute timeout
- [ ] All dialogs return non-null results when user completes action
- [ ] Widget completion criteria met for all table-stakes widgets

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| macOS Thread 1 deadlock | HIGH - blocks all GUI testing | Complete custom VSTest adapter, use Display.SyncExec everywhere |
| SafeHandle leaks | HIGH - resource exhaustion | Add leak detection tests in Phase 1, stress test with 10K+ cycles |
| Platform behavior differences | MEDIUM - tests pass on one platform, fail on others | Define completion criteria, test-first development, platform matrix CI |
| 200+ incomplete TODOs | MEDIUM - technical debt accumulation | Enforce zero-TODO completion criteria per widget |
| Browser integration complexity | MEDIUM - WebView2/WebKit version conflicts | Research phase before implementation, document version requirements |
| CI flakiness | LOW - wasted developer time | Replace polling with event-based sync, add retry only for external deps |

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | xUnit 2.9.3, NSubstitute, Coverlet all verified against official sources |
| Features | HIGH | Based on official Eclipse SWT 4.x API documentation |
| Architecture | HIGH | Three-layer test approach validated by existing codebase patterns |
| Pitfalls | HIGH | Identified from real issues in SWTSharp codebase (200+ TODOs, CI timeouts) |

**Overall confidence:** HIGH

### Gaps to Address

- **Graphics testing:** No tests for GC/Image/Font operations currently; need to add in Phase 2
- **WebKitGTK versioning:** CI installs both 4.0 and 4.1; need to determine which version to target
- **Wayland support:** Linux testing uses X11/Xvfb; Wayland support status unclear

## Sources

### Primary (HIGH confidence)
- Eclipse SWT 4.x Official Documentation (help.eclipse.org)
- xUnit 2.9.3 Release Notes (xunit.net)
- Microsoft .NET Testing Documentation (learn.microsoft.com)
- SWTSharp codebase analysis (89 widget files, 200+ TODOs)

### Secondary (MEDIUM confidence)
- SWTBot Testing Framework documentation (vogella.com)
- FlaUI Windows Automation documentation (github.com/FlaUI)
- Verify Snapshot Testing documentation (github.com/VerifyTests)

### Tertiary (LOW confidence)
- Appium Mac2 Driver for E2E testing (github.com/appium) - not yet integrated
- Stryker.NET mutation testing (stryker-mutator.io) - future phase consideration

---
*Synthesized: 2026-01-29*
*Ready for roadmap: yes*
