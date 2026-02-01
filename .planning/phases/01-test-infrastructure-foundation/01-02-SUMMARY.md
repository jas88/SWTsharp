---
phase: 01-test-infrastructure-foundation
plan: 02
subsystem: test-infrastructure
tags: [xunit, test-discovery, ci, github-actions, platform-attributes]

dependency_graph:
  requires: [01-01]
  provides:
    - platform-specific-test-attributes
    - ci-dotnet-test-workflow
  affects: [01-03, 02-*]

tech_stack:
  added: []
  patterns:
    - IXunitTestCaseDiscoverer for platform filtering
    - Discovery-time test exclusion

key_files:
  created:
    - tests/SWTSharp.Tests/Infrastructure/PlatformFactAttributes.cs
  modified:
    - .github/workflows/ci.yml

decisions:
  - id: platform-discovery-pattern
    choice: Use IXunitTestCaseDiscoverer to return empty enumerable on wrong platform
    rationale: Tests are not discovered (not skipped) preventing them from appearing in results and avoiding crashes on wrong platform

metrics:
  duration: ~2 minutes
  completed: 2025-01-30
---

# Phase 1 Plan 2: Platform Test Attributes and CI dotnet test

Platform-specific IXunitTestCaseDiscoverer attributes for filtering tests at discovery time, plus CI workflow update to use dotnet test.

## What Was Built

### Task 1: Platform-Specific Test Discovery Attributes

Created `PlatformFactAttributes.cs` with three fact attributes and their discoverers:

1. **WindowsOnlyFactAttribute** + WindowsFactDiscoverer
2. **MacOSOnlyFactAttribute** + MacOSFactDiscoverer
3. **LinuxOnlyFactAttribute** + LinuxFactDiscoverer

Each discoverer:
- Implements `IXunitTestCaseDiscoverer`
- Checks `RuntimeInformation.IsOSPlatform()` at discovery time
- Returns `Enumerable.Empty<IXunitTestCase>()` on wrong platform (test not discovered)
- Returns actual `XunitTestCase` on correct platform

**Key difference from existing PlatformFacts.cs**: The existing attributes use Skip property (tests appear as "skipped" in results). The new attributes use discoverers to not discover tests at all on wrong platforms (tests don't appear in results).

### Task 2: CI Workflow Updates

Updated `.github/workflows/ci.yml`:

1. **Test command**: Changed from `dotnet run --project` to `dotnet test` for proper VSTest discovery
2. **Job timeouts**: Added 15-minute timeout to all three platform jobs
3. **macOS runner**: Changed from `macos-latest` to `macos-15` (Sequoia)
4. **NuGet caching**: Added `actions/cache@v4` for `~/.nuget/packages`
5. **Push trigger**: Simplified to main branch only (develop still in pull_request)
6. **Comments**: Updated macOS comments to reference platform discoverer attributes

## Technical Details

### Attribute Design

```csharp
// Attribute points to discoverer
[XunitTestCaseDiscoverer("SWTSharp.Tests.Infrastructure.WindowsFactDiscoverer", "SWTSharp.Tests")]
public sealed class WindowsOnlyFactAttribute : FactAttribute { }

// Discoverer checks platform and returns appropriate test cases
public sealed class WindowsFactDiscoverer : IXunitTestCaseDiscoverer
{
    public IEnumerable<IXunitTestCase> Discover(...)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Enumerable.Empty<IXunitTestCase>();  // Not discovered
        return new[] { new XunitTestCase(...) };        // Discovered
    }
}
```

### CI Workflow Changes

| Setting | Before | After |
|---------|--------|-------|
| Test command | `dotnet run --project` | `dotnet test --no-build --logger trx` |
| Job timeout | none | 15 minutes |
| macOS runner | macos-latest | macos-15 |
| NuGet cache | none | actions/cache@v4 |
| Push branches | main, develop | main only |

## Commits

| Hash | Type | Message |
|------|------|---------|
| 6e64ca7 | feat | add platform-specific test discovery attributes |
| 5d1f147 | chore | update CI workflow to use dotnet test |

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

- Build succeeds (0 errors, warnings are pre-existing WindowsBase conflicts)
- YAML validation passes
- Test list command works with dotnet test
- All three CI jobs have 15-minute timeouts
- All three CI jobs use dotnet test command
- macOS job uses macos-15 runner
- NuGet caching configured for all jobs

## Files Created/Modified

### Created
- `tests/SWTSharp.Tests/Infrastructure/PlatformFactAttributes.cs` (159 lines)

### Modified
- `.github/workflows/ci.yml` (+35 lines, -9 lines)

## Next Phase Readiness

**Ready to proceed**: Phase 1 Plan 3 (Custom Test Runner)

**Blockers**: None

**Note**: The pre-existing custom adapter has a GUID parsing error that will need to be addressed separately. This does not block standard xUnit test discovery which works correctly.
