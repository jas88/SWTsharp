# Phase 1: Test Infrastructure Foundation - Context

**Gathered:** 2026-01-29
**Status:** Ready for planning

<domain>
## Phase Boundary

Establish reliable test infrastructure that works on Windows, macOS, and Linux with CI green. This phase solves the macOS Thread 1 requirement, sets up platform-specific test projects, configures CI for all three platforms, and provides event-based synchronization helpers. Widget implementation and test coverage for specific widgets are out of scope (Phase 2).

</domain>

<decisions>
## Implementation Decisions

### macOS Threading Approach
- Investigate whether Microsoft.Testing.Platform handles threading differently; if not, use custom VSTest adapter
- Only GUI tests run on Thread 1; non-GUI tests run on arbitrary threads (faster parallelization)
- GUI tests identified by inheritance from GUITestBase (not attributes or detection)
- Timeout deadlocked tests at 30 seconds, kill and fail with clear message (no retry)
- Best-effort diagnostic capture before killing timed-out tests
- macOS requires WindowServer (no headless mode)
- Linux uses Xvfb provided by CI workflow (tests assume display exists)
- Test adapter is internal to SWTSharp only (not a reusable NuGet package)

### Test Base Classes
- GUITestBase manages Display creation/disposal (setup/teardown)
- Display sharing is configurable: default shared for speed, opt-in fresh Display for isolation
- MockedTestBase for non-GUI mocked tests (separate from GUITestBase, no threading requirements)
- Strict disposal checking: tests fail if widgets not disposed (no exceptions, no opt-out)
- Tests fail if event queue left dirty (pending events)
- On failure, capture: screenshot + widget tree + exception

### Test Parallelization
- Mocked tests can run in parallel
- GUI tests run sequentially (inherent single-threaded constraint)
- Minimal logging during execution (pass/fail only); details on failure
- Windows and Linux use platform-native test execution (no special isolation)

### Test Organization
- Separate test projects per platform: SWTSharp.Tests.Windows, .MacOS, .Linux
- Shared project SWTSharp.Tests.Core for cross-platform mocked tests + utilities
- Platform projects only build on their target platform (conditional compilation)
- SWTSharp.TestAdapter remains a separate project for macOS threading solution
- Test file naming: ClassNameTests.cs (e.g., ButtonTests.cs)
- Test method naming: plain descriptive (e.g., ButtonClickDoesNotFireEventWhenDisabled)
- Namespace structure mirrors main library (SWTSharp.Graphics → SWTSharp.Tests.Core.Graphics)
- Test data lives within each test project (Tests/TestData/ folder)
- No test categories/traits for filtering — all tests equal

### CI Configuration
- Single workflow with separate jobs per platform (not matrix, not separate workflows)
- Runners: windows-latest, macos-15 (Sequoia), ubuntu-latest
- All platforms must pass for PR to merge (no platform can be ignored)
- Job timeout: 15 minutes
- Coverage reported to Codecov
- Minimum 80% coverage threshold (fails CI if below)
- Upload diagnostic artifacts (screenshots, widget trees) on failure only
- .NET 9.0 SDK only (not testing other frameworks in CI)
- Cache NuGet packages for faster builds
- Run on PRs + main branch only (not every push)
- No scheduled/nightly runs

### Synchronization Helpers
- Use TaskCompletionSource for async-friendly synchronization
- Event subscription pattern: subscribe to widget event, signal TaskCompletionSource when received
- Default timeout: 5 seconds
- Timeout configurable per-call via parameter (not test-level)

### Claude's Discretion
- Exact implementation of Microsoft.Testing.Platform investigation
- TestAdapter internal architecture
- Diagnostic capture implementation details
- NuGet cache key strategy
- Exact CI step ordering

</decisions>

<specifics>
## Specific Ideas

- "Investigate Microsoft.Testing.Platform threading first — it's newer and might handle things differently"
- Platform projects should conditionally compile so Windows tests don't even build on macOS CI
- Strict disposal checking with no opt-out ensures leak detection from day one

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-test-infrastructure-foundation*
*Context gathered: 2026-01-29*
