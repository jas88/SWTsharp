---
phase: 02-core-widget-completion
discussed: 2026-01-30
status: ready_for_planning
---

# Phase 2: Core Widget Completion - Context

## Phase Goal

All existing widgets pass completion criteria (zero TODOs, all platforms, disposal verified)

## Success Criteria

1. Zero TODO comments in all widget implementation files
2. All dialogs return non-null results when user completes action
3. SafeHandle.ReleaseHandle() implemented for all platform handles
4. All 30+ compiler warnings resolved (no suppressions)
5. Missing Linux widgets (Slider, Spinner) fully implemented

## Current State

| Metric | Value | Source |
|--------|-------|--------|
| TODO comments | 199 | `grep -r "TODO" src/` |
| Suppressed warning categories | 10+ | SWTSharp.csproj NoWarn |
| SafeHandle classes | 15 | All have ReleaseHandle() |
| Dialog classes | 4 | FileDialog, ColorDialog, FontDialog, MessageBox |
| Linux Slider/Spinner | Partial | GTK P/Invoke exists, needs completion |

## Decisions

### 1. TODO Prioritization Strategy

**Decision:** Widget-by-widget (Option B)

Complete one widget fully before moving to the next. This aligns with the plan structure:
- Plan 01: Core widgets (Button, Label, Text, Shell, Composite)
- Plan 02: Container widgets (TabFolder, ToolBar, Menu, Group)
- Plan 03: Dialogs
- Plan 04: Linux-specific widgets
- Plan 05: Complex widgets (Table, Tree, Combo, List)
- Plan 06: Graphics and SafeHandle

**Rationale:** Delivers verifiable progress; each widget becomes production-ready before moving on.

### 2. Warning Suppression Approach

**Decision:** Remove all suppressions, fix everything (Option A)

Remove all NoWarn entries from .csproj and resolve every warning:
- CS1591: Add missing XML documentation
- CS0618: Address obsolete API usage
- CS0067: Remove or implement unused events
- CS0169: Remove or use unused fields
- CA1707: Rename identifiers (even P/Invoke wrappers use internal names)
- CA2101: Add explicit marshaling attributes
- All other CA* warnings: Fix properly

**Rationale:** Clean, warning-free codebase is the goal. P/Invoke wrappers can use private methods with proper names and public-facing methods with conventional names.

### 3. Dialog Return Values

**Decision:** Match Java SWT exactly (Option A)

- FileDialog.open() returns `null` when cancelled
- ColorDialog.open() returns `null` when cancelled
- FontDialog.open() returns `null` when cancelled
- MessageBox.open() returns int (SWT.OK, SWT.CANCEL, etc.)

**Rationale:** API compatibility with Java SWT is the core project value. Callers expect null checks.

### 4. Linux Widget Completeness

**Decision:** Feature parity with Windows/macOS (Option A)

Linux Slider and Spinner must support:
- All SWT style flags (HORIZONTAL, VERTICAL, BORDER, etc.)
- All events (Selection, Modify, etc.)
- Proper disposal and handle cleanup
- Matching behavior to Windows/macOS implementations

**Rationale:** Cross-platform consistency is expected. "Write once, run anywhere" requires identical behavior.

### 5. SafeHandle Validation

**Decision:** Both unit and integration tests (Option C)

- **Unit tests:** Test each SafeHandle subclass in isolation (create, release, double-dispose safety)
- **Integration tests:** Test through widget lifecycle (create widget → use → dispose → verify handle released)

**Rationale:** Resource leaks are critical bugs. Belt-and-suspenders testing catches both handle-level and widget-level issues.

## Dependencies

- Phase 1 (Test Infrastructure Foundation) — Complete
- EventSyncHelpers available for async test synchronization
- GUITestBase available for widget test lifecycle
- Platform test attributes available for platform-specific tests

## Constraints

- Must maintain API compatibility with Java SWT 4.x
- Must work on all 3 platforms (Windows, macOS, Linux)
- Must pass CI on all platforms
- No new dependencies; use existing P/Invoke patterns

## Open Questions

None — all gray areas resolved.

---
*Discussed: 2026-01-30*
