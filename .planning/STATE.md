# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2025-01-29)

**Core value:** API compatibility with Java SWT 4.x
**Current focus:** Phase 2 Complete - Ready for Phase 3 (Layout & Packaging)

## Current Position

Phase: 2 of 4 (Core Widget Completion) - COMPLETE
Plan: 6 of 6 in Phase 2
Status: Phase 2 complete, ready for Phase 3
Last activity: 2026-01-31 - Completed 02-06-PLAN.md (Graphics/SafeHandle)

Progress: [████████░░] 83% (10/12 total plans)

## Performance Metrics

**Velocity:**
- Total plans completed: 10
- Average duration: ~6.5 minutes
- Total execution time: ~65 minutes

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01 | 4 | ~15m | ~4m |
| 02 | 6 | ~50m | ~8m |

**Recent Trend:**
- Last 5 plans: 02-02, 02-03, 02-04, 02-05, 02-06
- Trend: Consistent execution, complex widgets take longer

*Updated after each plan completion*

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [Roadmap]: Four-phase approach derived from research (test infra -> widgets -> packages -> polish)
- [Roadmap]: Phase 1 prioritizes macOS Thread 1 issue as blocking all GUI tests
- [01-01]: Use XunitFrontController with AppDomainSupport.Denied for in-process test execution on Windows/Linux
- [01-01]: 30-second per-test timeout with [TIMEOUT] message format for deadlock detection
- [01-02]: Use IXunitTestCaseDiscoverer pattern to return empty enumerable on wrong platform (tests not discovered, not skipped)
- [01-03]: TaskCreationOptions.RunContinuationsAsynchronously mandatory for all TCS to prevent deadlocks
- [01-03]: GUITestBase strict disposal checking throws on undisposed widgets
- [01-04]: Mark legacy polling methods [Obsolete] rather than remove for backward compatibility
- [01-04]: Use IAsyncLifetime for xUnit fixtures needing async setup/teardown
- [02-05]: TableColumn delegates to IPlatformTable methods rather than separate IPlatformTableColumn
- [02-05]: TreeItem uses data source pattern - stores state locally, platform queries when rendering
- [02-05]: Combo text limit enforced in managed layer, clipboard operations native
- [02-06]: SafeHandle Create() throws InvalidOperationException with guidance to use FromHandle()
- [02-06]: Canvas.Redraw() triggers paint events synchronously via OnPlatformPaint()
- [02-06]: MSB3277 suppressed for WebView2 WindowsBase version conflict (benign on non-Windows)

### Pending Todos

None.

### Blockers/Concerns

- macOS Thread 1 requirement addressed with TestHost (Phase 1)
- Pre-existing Table/TreeItem objc_msgSend_fpret entry point issue on ARM macOS
- Canvas tests exist but require custom test runner on macOS

## Session Continuity

Last session: 2026-01-31
Stopped at: Phase 2 complete (all 6 plans executed)
Resume file: None
Next: Phase 3 - Layout Managers & Packaging
