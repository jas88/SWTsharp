# SWTSharp

## What This Is

SWTSharp is a .NET port of Eclipse SWT (Standard Widget Toolkit), providing a cross-platform GUI toolkit with native look and feel on Windows (Win32), macOS (Cocoa/AppKit), and Linux (GTK3). It targets API compatibility with Java SWT 4.x while leveraging modern C# features (.NET 9.0, source generators, Native AOT support).

## Core Value

**API compatibility with Java SWT 4.x**: Developers familiar with Java SWT should be able to use SWTSharp with minimal learning curve, and SWT applications should port with straightforward code translation.

## Requirements

### Validated

- ✓ Platform abstraction via IPlatform/IPlatformWidget interfaces — existing
- ✓ Widget hierarchy (Widget → Control → Composite/Shell) — existing
- ✓ Event system with typed listeners — existing
- ✓ Layout managers (GridLayout, FormLayout, FillLayout, StackLayout, RowLayout) — existing
- ✓ Core widgets (Button, Label, Text, Composite, Shell) — existing
- ✓ Multi-targeting (netstandard2.0, net8.0, net9.0) — existing
- ✓ Platform detection and runtime selection — existing
- ✓ Basic test infrastructure with xUnit — existing

### Active

- [ ] Complete platform widget interface integration (200+ TODO items)
- [ ] Implement functional dialogs (FileDialog, ColorDialog, FontDialog, MessageBox)
- [ ] Implement missing Linux widgets (Slider, Spinner)
- [ ] Implement SafeHandle Release() methods for all platform handles
- [ ] Full test coverage for public API
- [ ] Graphics (GC) test coverage
- [ ] Layout manager test coverage
- [ ] Platform integration tests running on all 3 platforms in CI
- [ ] Resolve all compiler warnings (30+ currently suppressed)
- [ ] Enable code coverage in CI

### Out of Scope

- WebView2/Browser widget — Complex dependency, defer to future milestone
- Custom widget painting beyond GC basics — Focus on SWT API compatibility first
- Accessibility (ARIA, screen readers) — Important but defer to dedicated milestone
- Internationalization/RTL support — Defer to future milestone
- Performance optimization — Get correctness first, optimize later

## Context

**Current State:**
- Codebase has substantial structure but many incomplete implementations
- 200+ TODO comments indicating unfinished platform widget integration
- Dialogs exist as stubs but return null (non-functional)
- Linux platform missing Slider and Spinner implementations
- SafeHandle subclasses have unimplemented Release() methods
- Test coverage gaps: no graphics tests, no layout tests, minimal platform tests
- 30+ compiler warnings suppressed in .csproj
- Code coverage disabled in CI due to macOS threading requirements

**Java SWT Reference:**
- Targeting Eclipse SWT 4.x (current version as of 2025)
- API documentation at https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/package-summary.html

**Platform APIs:**
- Windows: Win32 API via P/Invoke
- macOS: Cocoa/AppKit via Objective-C runtime P/Invoke
- Linux: GTK3 via P/Invoke

## Constraints

- **Tech stack**: .NET 9.0 SDK, multi-targeting netstandard2.0/net8.0/net9.0
- **CI**: GitHub Actions, must pass on Windows/macOS/Linux runners
- **Compatibility**: API should match Java SWT 4.x method signatures where possible
- **Dependencies**: Minimize external dependencies; platform-specific code via P/Invoke

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Port SWT rather than wrap native toolkit | SWT API is well-known, enables Java SWT app porting | — Pending |
| Multi-targeting rather than single TFM | Support both legacy and modern .NET | — Pending |
| P/Invoke rather than native library | Avoid native compilation complexity | — Pending |
| xUnit + NSubstitute for testing | Standard .NET testing stack | — Pending |

---
*Last updated: 2026-01-29 after initialization*
