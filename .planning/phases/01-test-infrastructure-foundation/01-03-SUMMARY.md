---
phase: 01-test-infrastructure-foundation
plan: 03
subsystem: test-infrastructure
tags: [testing, async, coverage, xunit, tcs]
dependency-graph:
  requires: ["01-01", "01-02"]
  provides: ["EventSyncHelpers", "GUITestBase", "coverage-collection"]
  affects: ["02-*"]
tech-stack:
  added: []
  patterns: ["TaskCompletionSource with RunContinuationsAsynchronously", "IAsyncLifetime for test lifecycle"]
key-files:
  created:
    - tests/SWTSharp.Tests/Infrastructure/EventSyncHelpers.cs
    - tests/SWTSharp.Tests/Infrastructure/GUITestBase.cs
  modified:
    - .github/workflows/ci.yml
decisions:
  - decision: "Use TaskCreationOptions.RunContinuationsAsynchronously for all TCS instances"
    context: "Prevents deadlocks when continuations need locks held by SetResult caller"
    date: 2026-01-30
  - decision: "GUITestBase strict disposal checking throws on undisposed widgets"
    context: "Forces test cleanup, prevents resource leaks"
    date: 2026-01-30
metrics:
  duration: ~4 minutes
  completed: 2026-01-30
---

# Phase 01 Plan 03: Test Sync Helpers and Coverage Summary

**One-liner:** TaskCompletionSource-based event synchronization and Coverlet XPlat coverage across all CI platforms.

## What Was Built

### EventSyncHelpers.cs
Async-friendly event synchronization helpers replacing polling loops:

- `WaitForEvent<T>`: Subscribe to event, complete when fired, with timeout
- `WaitForCondition`: Async condition checking with Timer (no Thread.Sleep polling)
- `WaitForEventCount<T>`: Wait for N event firings, return all args
- `WaitForEventWithTrigger<T>`: Trigger action then wait for event

All methods use `TaskCreationOptions.RunContinuationsAsynchronously` to prevent deadlocks.

### GUITestBase.cs
Base class for GUI tests with comprehensive lifecycle management:

- Display lifecycle: Shared (default) or isolated (opt-in)
- Shell auto-tracking via `CreateShell()` methods
- Strict disposal checking: Tests fail if widgets not disposed
- Event queue verification: Tests fail if queue left dirty
- Failure diagnostics: Captures widget tree to TestResults folder
- Collection definition: `"GUI Tests"` with `DisableParallelization = true`

### CI Coverage Configuration
Updated `.github/workflows/ci.yml`:

- Added `--collect:"XPlat Code Coverage"` to all platform test commands
- Added `--results-directory ./TestResults` for consistent output
- Updated artifact uploads to include `coverage.cobertura.xml`
- Added `merge-coverage` job that:
  - Downloads coverage from all platforms
  - Uploads merged coverage to Codecov
  - Runs conditionally (if any platform succeeded)

## Commits

| Commit | Type | Description |
|--------|------|-------------|
| fa61e67 | feat | Add EventSyncHelpers with TaskCompletionSource pattern |
| 830c247 | feat | Add GUITestBase for GUI test management |
| 9ff0b6c | feat | Configure Coverlet coverage collection in CI |

## Key Decisions

1. **TaskCreationOptions.RunContinuationsAsynchronously mandatory**: Without this flag, deadlocks occur when continuations try to acquire locks held by the SetResult caller. This is documented in RESEARCH.md Pitfall 2.

2. **Strict disposal checking throws exceptions**: Tests that leave widgets undisposed fail with detailed error messages listing all undisposed widgets. This enforces proper cleanup.

3. **Event queue verification drains 100 events max**: If queue still has events after 100 ReadAndDispatch calls, test fails with possible infinite loop warning.

4. **System.DateTime qualification required**: SWTSharp has a custom DateTime class that shadows System.DateTime, requiring fully qualified references.

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

All verification criteria met:
- Build succeeds without errors
- EventSyncHelpers.cs contains RunContinuationsAsynchronously (5 occurrences)
- GUITestBase.cs contains IsDisposed disposal checking (7 occurrences)
- ci.yml contains XPlat Code Coverage (3 occurrences, one per platform)

## Files Changed

### Created
- `tests/SWTSharp.Tests/Infrastructure/EventSyncHelpers.cs` (270 lines)
- `tests/SWTSharp.Tests/Infrastructure/GUITestBase.cs` (438 lines)

### Modified
- `.github/workflows/ci.yml` (+68/-44 lines)

## Next Phase Readiness

Phase 1 complete with all 3 plans executed:
- Plan 01: VSTest adapter with XunitFrontController
- Plan 02: Platform-specific test attributes
- Plan 03: Event sync helpers and coverage (this plan)

Phase 2 (Widget Test Suite) can now begin with:
- TaskCompletionSource-based async synchronization available
- GUITestBase for consistent GUI test lifecycle
- Coverage collection active on all platforms
