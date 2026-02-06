# Roadmap: SWTSharp

## Overview

SWTSharp will achieve full API compatibility with Java SWT 4.x through a four-phase approach: first establishing reliable test infrastructure across all three platforms (Windows, macOS, Linux), then systematically completing existing widgets to zero TODOs, followed by implementing critical missing packages (DND, Accessibility, StyledText), and finally adding polish features like Printing and performance validation. Each phase delivers verifiable capability before proceeding.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Test Infrastructure Foundation** - Working test infrastructure on all 3 platforms with CI green
- [ ] **Phase 2: Core Widget Completion** - All existing widgets pass completion criteria (zero TODOs, all platforms)
- [ ] **Phase 3: Missing Critical Packages** - Complete implementations for DND, Accessibility, StyledText
- [ ] **Phase 4: Polish and Performance** - Performance parity, Printing, remaining features

## Phase Details

### Phase 1: Test Infrastructure Foundation
**Goal**: Working test infrastructure on all 3 platforms with CI green
**Depends on**: Nothing (first phase)
**Requirements**: TEST-01, TEST-02, TEST-03, TEST-04, TEST-05
**Success Criteria** (what must be TRUE):
  1. All existing tests pass on Windows, macOS, and Linux in CI
  2. macOS tests complete without timeout (Thread 1 requirement solved)
  3. Code coverage collection works on all platforms
  4. Platform-specific test attributes ([WindowsOnlyFact], [LinuxOnlyFact], [MacOSOnlyFact]) filter correctly
  5. Test helpers use event-based synchronization (no flaky polling)
**Plans**: 4 plans

Plans:
- [x] 01-01-PLAN.md — Complete VSTest adapter for all platforms (Wave 1)
- [x] 01-02-PLAN.md — Platform-specific test attributes and CI configuration (Wave 1)
- [x] 01-03-PLAN.md — Coverage collection and synchronization helpers (Wave 2)
- [x] 01-04-PLAN.md — Gap closure: obsolete polling helpers, add DisplayFixture (Wave 1, gap_closure)

### Phase 2: Core Widget Completion
**Goal**: All existing widgets pass completion criteria (zero TODOs, all platforms, disposal verified)
**Depends on**: Phase 1
**Requirements**: WIDGET-01, WIDGET-02, WIDGET-03, WIDGET-04, WIDGET-05, WIDGET-06, WIDGET-07, WIDGET-08, WIDGET-09
**Success Criteria** (what must be TRUE):
  1. Zero TODO comments in all widget implementation files
  2. All dialogs return non-null results when user completes action
  3. SafeHandle.ReleaseHandle() implemented for all platform handles
  4. All 30+ compiler warnings resolved (no suppressions)
  5. Missing Linux widgets (Slider, Spinner) fully implemented
**Plans**: 7 plans

Plans:
- [x] 02-01-PLAN.md — Core widgets (Button, Label, Text, Shell, Composite)
- [x] 02-02-PLAN.md — Container widgets (TabFolder, ToolBar, Menu, Group)
- [x] 02-03-PLAN.md — Dialog completion (FileDialog, ColorDialog, FontDialog, MessageBox)
- [x] 02-04-PLAN.md — Linux-specific widgets (Slider, Spinner)
- [x] 02-05-PLAN.md — Complex widgets (Table, Tree, Combo, List)
- [x] 02-06-PLAN.md — Graphics and SafeHandle completion
- [ ] 02-07-PLAN.md — Gap closure: Platform implementation TODOs (73 color/styling TODOs)

### Phase 3: Missing Critical Packages
**Goal**: Complete implementations for missing table-stakes packages
**Depends on**: Phase 2
**Requirements**: PKG-01, PKG-02, PKG-03
**Success Criteria** (what must be TRUE):
  1. Drag and Drop operations work between widgets and external applications
  2. StyledText widget supports syntax highlighting and text formatting
  3. Accessibility APIs (UIA/NSAccessibility/ATK) expose widget semantics to screen readers
**Plans**: TBD

Plans:
- [ ] 03-01: Drag and Drop package (org.eclipse.swt.dnd equivalent)
- [ ] 03-02: StyledText completion
- [ ] 03-03: Accessibility package (org.eclipse.swt.accessibility equivalent)

### Phase 4: Polish and Performance
**Goal**: Performance parity with Java SWT, advanced features, production readiness
**Depends on**: Phase 3
**Requirements**: PERF-01, PERF-02, PERF-03, PERF-04
**Success Criteria** (what must be TRUE):
  1. Benchmarks show widget creation/disposal within 2x of Java SWT performance
  2. Printing support works on all platforms
  3. Browser widget supports WebView2 (Windows), WebKit (macOS), WebKitGTK (Linux)
  4. Stress tests (10K+ widget cycles) complete without SafeHandle leaks
**Plans**: TBD

Plans:
- [ ] 04-01: Performance benchmarks and optimization
- [ ] 04-02: Printing support
- [ ] 04-03: Browser widget integration
- [ ] 04-04: Stress testing and leak detection

## Requirements Coverage

| ID | Description | Phase |
|----|-------------|-------|
| TEST-01 | macOS Thread 1 custom VSTest adapter | 1 |
| TEST-02 | Platform-specific test attributes | 1 |
| TEST-03 | Code coverage collection on all platforms | 1 |
| TEST-04 | Event-based test synchronization | 1 |
| TEST-05 | CI green on Windows/macOS/Linux | 1 |
| WIDGET-01 | Complete 200+ TODOs in widget implementations | 2 |
| WIDGET-02 | Functional dialogs (FileDialog, ColorDialog, FontDialog, MessageBox) | 2 |
| WIDGET-03 | Missing Linux widgets (Slider, Spinner) | 2 |
| WIDGET-04 | SafeHandle Release() methods for all handles | 2 |
| WIDGET-05 | Full test coverage for public API | 2 |
| WIDGET-06 | Graphics (GC) test coverage | 2 |
| WIDGET-07 | Layout manager test coverage | 2 |
| WIDGET-08 | Resolve 30+ compiler warnings | 2 |
| WIDGET-09 | Core/Container/Complex widget completion | 2 |
| PKG-01 | Drag and Drop package | 3 |
| PKG-02 | StyledText widget completion | 3 |
| PKG-03 | Accessibility package | 3 |
| PERF-01 | Performance benchmarks (BenchmarkDotNet) | 4 |
| PERF-02 | Printing support | 4 |
| PERF-03 | Browser widget (WebView2/WebKit/WebKitGTK) | 4 |
| PERF-04 | Stress tests and leak detection | 4 |

**Coverage:** 20/20 requirements mapped

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Test Infrastructure Foundation | 4/4 | Complete | 2026-01-30 |
| 2. Core Widget Completion | 6/7 | Gap closure pending | - |
| 3. Missing Critical Packages | 0/3 | Not started | - |
| 4. Polish and Performance | 0/4 | Not started | - |

---
*Roadmap version: 1.1*
*Updated: 2026-02-01 - Added gap closure plan 02-07*
