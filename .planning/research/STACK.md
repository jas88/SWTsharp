# Stack Research: SWT Testing

**Research Date:** January 29, 2026
**Project:** SWTSharp - .NET Port of Eclipse SWT 4.x
**Platforms:** Windows (Win32), macOS (Cocoa), Linux (GTK3)
**Target Frameworks:** netstandard2.0, net8.0, net9.0
**CI Target:** GitHub Actions

## Executive Summary

After comprehensive research of testing tools for cross-platform .NET GUI frameworks and Java SWT testing practices, I recommend a **pragmatic, layered testing approach** that balances test coverage, platform compatibility, and CI/CD integration. The current codebase already demonstrates sophisticated testing infrastructure with a custom VSTest adapter for macOS Thread 1 support - this should be retained and enhanced rather than replaced.

## Recommended Testing Stack

### Core Unit Testing Framework

**xUnit 2.9.3** (Current - Maintain) ✅
- **Version:** [2.9.3](https://xunit.net/releases/v2/2.9.3) (Released January 8, 2025)
- **Status:** Latest v2 release; v2 line maintained while v3 development continues
- **Rationale:**
  - Current implementation already uses xUnit 2.9.3 with custom test infrastructure
  - Native support for platform-specific test filtering via `[Fact]` attributes
  - Excellent Visual Studio and VSTest integration
  - Supports custom test adapters (critical for macOS Thread 1 requirement)
  - Multi-target friendly (netstandard2.0, net8.0, net9.0)
- **Migration Path:** Stay on v2 until v3 reaches production maturity
- **References:**
  - [xUnit v2 2.9.3 Release Notes](https://xunit.net/releases/v2/2.9.3)
  - [Unit testing C# with xUnit - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit)
  - [xUnit Getting Started Guide](https://xunit.net/docs/getting-started/v2/getting-started)

**Confidence:** 95% - xUnit 2.9.3 is proven, stable, and already integrated

---

### Custom Test Execution Infrastructure

**Custom VSTest Adapter** (Current - Enhance) ✅
- **Implementation:** `SWTSharp.TestAdapter` + `SWTSharp.TestHost`
- **Rationale:**
  - **Critical macOS Requirement:** UI operations must run on Thread 1 (CFRunLoop)
  - No off-the-shelf solution supports macOS GUI threading requirements
  - Current implementation uses process isolation with proper thread management
  - Allows platform-specific test execution strategies (in-process on Windows/Linux, out-of-process on macOS)
  - Integrates with standard VSTest/dotnet test pipeline
- **Enhancement Recommendations:**
  1. Complete Windows/Linux in-process execution path (currently stubbed)
  2. Add result serialization/deserialization for reliable IPC
  3. Implement proper xUnit test discovery integration
  4. Add diagnostic logging levels (current `SWTSHARP_DEBUG=1` is good start)
- **References:**
  - [VSTest Platform Object Model](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/vstest-v2?view=azure-pipelines)
  - [dotnet vstest command - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-vstest)
  - [VSTest Adapter Extensibility RFC](https://github.com/microsoft/vstest-docs/blob/main/RFCs/0004-Adapter-Extensibility.md)

**Confidence:** 90% - Architecture is sound; implementation needs completion

---

### Mocking Framework

**NSubstitute 5.3.0** (Current - Maintain) ✅
- **Version:** [5.3.0](https://www.nuget.org/packages/nsubstitute/) (Latest as of 2025)
- **Targets:** .NET 6.0, .NET Standard 2.0, .NET Framework 4.6.2+
- **Rationale:**
  - Already integrated in current codebase
  - Clean, intuitive syntax for Arrange-Act-Assert pattern
  - Better for testing interfaces (SWT's widget abstraction layer)
  - Simpler than Moq for straightforward mocking scenarios
  - No SponsorLink controversy (unlike Moq 4.20+)
- **When to Use:** Interface/abstract class mocking, behavioral verification
- **Limitations:** Only works with interfaces or virtual members
- **References:**
  - [NSubstitute Official Documentation](https://nsubstitute.github.io/)
  - [NuGet Package](https://www.nuget.org/packages/nsubstitute/)
  - [Best Mocking Frameworks in .NET: Moq vs. NSubstitute](https://www.jr-it-services.de/best-mocking-frameworks-in-net-moq-vs-nsubstitute/)

**Confidence:** 90% - Proven choice for interface-heavy frameworks like SWT

---

### Code Coverage

**Coverlet 6.0.4** (Current - Maintain) ✅
- **Package:** `coverlet.collector` 6.0.4
- **Version Status:** Latest as of 2025; default for .NET 8+
- **Rationale:**
  - Cross-platform support (Windows, macOS, Linux)
  - Native integration with `dotnet test --collect:"XPlat Code Coverage"`
  - Multiple output formats (Cobertura, OpenCover, LCOV, JSON)
  - Seamless CI/CD integration (GitHub Actions, Azure Pipelines)
  - Low configuration overhead
- **Output Format Recommendation:** Cobertura for Codecov integration
- **CI Integration:**
  ```bash
  dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
  ```
- **Alternatives Considered:**
  - **Microsoft.CodeCoverage**: Microsoft's newer tool, but Coverlet has broader community adoption and better documentation for cross-platform scenarios
- **References:**
  - [Use code coverage for unit testing - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
  - [Coverlet GitHub Repository](https://github.com/coverlet-coverage/coverlet)
  - [How to Set Up Coverlet in CI/CD Pipelines (2025)](https://medium.com/@sancharini.panda/how-to-set-up-and-use-coverlet-coverage-in-ci-cd-pipelines-7852fede8549)

**Confidence:** 95% - Industry standard for .NET cross-platform coverage

---

### Visual/Snapshot Testing

**Verify 28.x** (Recommended - Add) 🆕
- **Version:** Latest 28.x series (supports .NET 9, .NET 8, .NET 6, net462+)
- **Rationale:**
  - **Critical for GUI testing:** Widget appearance, layout rendering, graphics output
  - Simplifies complex object/structure assertions (widget hierarchies)
  - Better than manual bitmap comparison (too brittle)
  - Human-readable diffs for review
  - Supports images, text, JSON, custom serializers
- **Use Cases:**
  - Widget layout verification (bounds, sizing, positioning)
  - Graphics/GC rendering output (canvas, drawing operations)
  - Event handler registration verification
  - Complex widget hierarchy assertions
- **Integration Example:**
  ```csharp
  [Fact]
  public Task VerifyButtonLayout()
  {
      var shell = CreateTestShell();
      var button = new Button(shell, SWT.PUSH);
      button.SetBounds(10, 20, 100, 30);

      return Verify(new {
          Bounds = button.GetBounds(),
          Size = button.GetSize(),
          Location = button.GetLocation()
      });
  }
  ```
- **References:**
  - [Snapshot Testing in .NET with Verify - JetBrains Blog](https://blog.jetbrains.com/dotnet/2024/07/11/snapshot-testing-in-net-with-verify/)
  - [Verify GitHub Repository](https://github.com/VerifyTests/Verify)
  - [Snapshot testing .NET code with Verify (2025)](https://david.gardiner.net.au/2025/07/verify)

**Confidence:** 85% - Excellent fit for GUI testing, proven in other UI frameworks

---

### Performance/Benchmark Testing

**BenchmarkDotNet 0.15.4** (Recommended - Add) 🆕
- **Version:** [0.15.4](https://www.nuget.org/packages/BenchmarkDotNet) (Latest, September 2025)
- **Downloads:** 51.6M+ total downloads
- **Rationale:**
  - **Critical for performance parity with Java SWT**
  - Statistical analysis prevents measurement noise
  - Memory allocation profiling (native interop overhead)
  - Disassembly inspection for P/Invoke optimization
  - Supports multiple runtimes (net8.0, net9.0, .NET Framework)
- **Use Cases:**
  - Widget creation/disposal performance
  - Layout calculation benchmarks
  - Event dispatching throughput
  - Graphics operation benchmarks (GC.DrawLine, GC.FillRectangle)
  - P/Invoke overhead measurement
- **Integration:**
  - Separate project: `tests/SWTSharp.Benchmarks`
  - Run manually or in dedicated CI job (not every commit)
- **References:**
  - [BenchmarkDotNet Official Site](https://benchmarkdotnet.org/)
  - [The Ultimate Guide to .NET Performance Testing with BenchmarkDotNet](https://dev.to/prameshkc/the-ultimate-guide-to-net-performance-testing-with-benchmarkdotnet-3ka2)
  - [Analyze BenchmarkDotNet data in Visual Studio](https://learn.microsoft.com/en-us/visualstudio/profiling/profiling-with-benchmark-dotnet?view=visualstudio)

**Confidence:** 90% - Industry standard for .NET performance testing

---

### Mutation Testing (Optional Enhancement)

**Stryker.NET 4.11.0** (Recommended - Future Phase) 🔮
- **Version:** 4.11.0 (Latest as of 2025)
- **Rationale:**
  - Measures actual test effectiveness (not just coverage percentage)
  - Identifies weak tests that pass despite code bugs
  - Supports MSTest, xUnit, NUnit
  - Parallel test execution for speed
  - 30+ mutation types
- **Priority:** Low (after achieving >80% code coverage)
- **Use Cases:**
  - Validate critical widget lifecycle tests (disposal, event unsubscription)
  - Verify platform abstraction layer tests (Win32/Cocoa/GTK)
  - Ensure error handling paths are tested
- **Cost:** High CPU time (run weekly or on-demand, not every commit)
- **Installation:**
  ```bash
  dotnet tool install -g dotnet-stryker
  cd tests/SWTSharp.Tests
  dotnet stryker
  ```
- **References:**
  - [Mutation testing - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/testing/mutation-testing)
  - [Stryker.NET Official Documentation](https://stryker-mutator.io/docs/stryker-net/introduction/)
  - [Write Perfect Unit Tests with .NET Stryker](https://medium.com/dotnet-pulse/write-perfect-unit-tests-with-net-stryker-mutation-testing-39e0caa1b5b3)

**Confidence:** 75% - Valuable but not critical path; defer until coverage baseline established

---

## Platform-Specific Considerations

### Windows Testing

**Recommended Approach:** In-process testing with manual verification
- **Native UI Access:** Win32 API P/Invoke calls work in-process
- **Test Execution:** Standard xUnit runner works fine
- **Automation Considerations:**
  - **FlaUI** ([GitHub](https://github.com/FlaUI/FlaUI)): Modern .NET library for Windows UI automation
    - Built on Microsoft UI Automation (UIA2/UIA3)
    - Better than WinAppDriver (paused development)
    - Supports WPF, WinForms, UWP, Win32
  - **Recommendation:** Use FlaUI for smoke tests/E2E scenarios, not unit tests
  - **Rationale:** Unit tests should mock platform layer; E2E tests can use FlaUI for validation
- **References:**
  - [FlaUI GitHub](https://github.com/FlaUI/FlaUI)
  - [Ultimate Guide - Alternatives to WinAppDriver (2026)](https://www.testsprite.com/use-cases/en/the-most-accurate-alternatives-to-winappdriver)
  - [Automate Desktop Application with FlaUI](https://blog.nashtechglobal.com/desktop-automation-test-with-flaui/)

**Confidence:** 90% - Windows testing is straightforward; FlaUI available for E2E validation

---

### macOS Testing

**Recommended Approach:** Custom test host with CFRunLoop (Current Implementation)
- **Critical Constraint:** UI operations must run on Thread 1 (CFRunLoop)
- **Current Solution:** `SWTSharp.TestHost` with `GCDDispatcher`
  - Launches via custom VSTest adapter
  - Uses `dispatch_async_f` for thread-safe execution
  - CFRunLoop running on Thread 1
- **Enhancement Recommendations:**
  1. Complete result serialization (currently using stdout parsing)
  2. Add timeout handling for hung tests
  3. Implement test filtering in host process
  4. Add memory profiling hooks (Instruments integration)
- **Automation Considerations:**
  - **Appium Mac2 Driver** ([GitHub](https://github.com/appium/appium-mac2-driver)): Backed by Apple XCTest
    - Supports macOS desktop automation
    - Requires Xcode 15+ and UIAutomation authentication
  - **Recommendation:** Use Appium for E2E tests, not unit tests
  - **Rationale:** Unit tests mock platform layer; E2E tests validate real Cocoa behavior
- **CI Configuration:**
  ```yaml
  - name: Run macOS tests
    timeout-minutes: 5  # Prevent hanging
    env:
      SWTSHARP_DEBUG: 1
    run: dotnet run --project tests/SWTSharp.Tests/SWTSharp.Tests.csproj
  ```
- **References:**
  - [Appium Mac2 Driver GitHub](https://github.com/appium/appium-mac2-driver)
  - [MacOS Applications Test Automation with Appium](https://medium.com/@saurabh_koli/macos-applications-test-automation-with-appium-python-b4d31c7b4534)

**Confidence:** 85% - Current architecture is correct; implementation needs completion

---

### Linux Testing

**Recommended Approach:** Xvfb virtual display + in-process testing
- **Current Implementation:** GitHub Actions uses Xvfb (:99) with GTK 3
- **Display Requirements:**
  - Xvfb provides virtual X11 display
  - GTK 3 requires X11/Wayland connection
  - Tests run headless in CI
- **Dependencies:**
  ```bash
  sudo apt-get install -y xvfb libgtk-3-0 libglib2.0-0 libx11-dev
  sudo Xvfb :99 -screen 0 1024x768x24 &
  export DISPLAY=:99
  ```
- **Automation Considerations:**
  - **Dogtail 1.0.7** ([PyPI](https://pypi.org/project/dogtail/)): Python GUI automation via accessibility layer
    - X11 and Wayland support
    - GTK4 compatibility
  - **LDTP** ([freedesktop.org](https://ldtp.freedesktop.org/)): Linux Desktop Testing Project
    - Works with .NET/GNOME/KDE/Qt applications
    - Uses ATK accessibility layer
  - **Recommendation:** Use Dogtail/LDTP for E2E tests, not unit tests
  - **Rationale:** Unit tests mock platform layer; E2E tests validate real GTK behavior
- **CI Integration:**
  ```yaml
  - name: Setup Xvfb and GTK
    run: |
      sudo apt-get update
      sudo apt-get install -y xvfb libgtk-3-0 libglib2.0-0 libx11-dev
      sudo Xvfb :99 -screen 0 1024x768x24 > /dev/null 2>&1 &
      sleep 3
      echo "DISPLAY=:99" >> $GITHUB_ENV
  ```
- **Alternative:** GitHub Actions marketplace actions
  - [GabrielBB/xvfb-action](https://github.com/marketplace/actions/gabrielbb-xvfb-action)
  - [coactions/setup-xvfb](https://github.com/marketplace/actions/setup-xvfb)
- **References:**
  - [Dogtail PyPI Package](https://pypi.org/project/dogtail/)
  - [LDTP Tutorial](https://github.com/ldtp/ldtp2/blob/master/doc/ldtp-tutorial.rst)
  - [GitHub: GabrielBB/xvfb-action](https://github.com/GabrielBB/xvfb-action)

**Confidence:** 95% - Current CI setup is correct; established Linux headless testing pattern

---

## Java SWT Testing Practices (Reference)

### SWTBot Framework (Java)

**NOT DIRECTLY APPLICABLE** to .NET, but architectural lessons:
- **SWTBot** is the standard testing framework for Eclipse SWT applications
- Uses JUnit for test execution
- Provides high-level widget interaction APIs (click, type, select)
- Supports headless testing via Xvfb on Linux
- **Key Insight:** SWTBot wraps low-level SWT calls with test-friendly APIs
- **Lesson for SWTSharp:** Create helper methods/fixtures that simplify common widget test patterns
- **References:**
  - [SWTBot Tutorial - vogella.com](https://www.vogella.com/tutorials/SWTBot/article.html)
  - [Eclipse SWTBot Project](https://projects.eclipse.org/projects/technology.swtbot)
  - [SWTBot Tests Documentation](https://eclipse.dev/4diac/doc/development/swtBotTestsDocumentation.html)

**Current SWTSharp Equivalent:** `WidgetTestBase` class provides similar abstraction

---

## CI/CD Integration

### GitHub Actions Recommendations

**Current Setup:** Multi-platform testing on Windows, macOS, Linux ✅
- **Windows:** `windows-latest` with direct test execution
- **macOS:** `macos-latest` with 5-minute timeout (prevents hanging)
- **Linux:** `ubuntu-latest` with Xvfb virtual display

**Enhancement Recommendations:**

1. **Coverage Reporting:**
   ```yaml
   - name: Run tests with coverage
     run: dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

   - name: Upload coverage to Codecov
     uses: codecov/codecov-action@v5
     with:
       directory: ./TestResults
       flags: ${{ matrix.os }}
   ```

2. **Test Result Artifacts:**
   ```yaml
   - name: Upload test results
     if: always()
     uses: actions/upload-artifact@v5
     with:
       name: test-results-${{ matrix.os }}
       path: '**/test-results.trx'
       retention-days: 30
   ```

3. **Matrix Testing (Optional):**
   ```yaml
   strategy:
     matrix:
       os: [windows-latest, macos-latest, ubuntu-latest]
       dotnet-version: ['8.0.x', '9.0.x']
   ```

4. **Benchmark CI Job (Weekly):**
   ```yaml
   - name: Run benchmarks
     if: github.event_name == 'schedule'
     run: dotnet run --project tests/SWTSharp.Benchmarks -c Release
   ```

**References:**
- [GitHub Actions for .NET](https://github.com/actions/setup-dotnet)
- Current CI: `.github/workflows/ci.yml`

---

## Alternatives Considered

### ❌ Avalonia UI Testing Approach

**Why NOT Applicable:**
- Avalonia offers [headless testing](https://docs.avaloniaui.net/xpf/advanced/headless-testing) that works without platform windowing systems
- **Problem:** SWTSharp MUST use native platform widgets (Win32, Cocoa, GTK), not a custom rendering engine
- **Lesson:** Headless testing only works for frameworks that own the rendering pipeline
- **References:**
  - [Exploring Headless Testing for XPF and WPF - Avalonia UI](https://avaloniaui.net/blog/exploring-headless-testing-for-xpf-and-wpf-applications)
  - [Avalonia Headless Testing Docs](https://docs.avaloniaui.net/xpf/advanced/headless-testing)

**Confidence:** 100% - Architectural mismatch

---

### ❌ WinAppDriver (Windows)

**Why NOT Recommended:**
- [Development paused](https://testguild.com/automation-tools-desktop/) by Microsoft
- Limited recent updates
- FlaUI is more actively maintained and feature-complete
- **References:**
  - [Ultimate Guide - Alternatives to WinAppDriver (2026)](https://www.testsprite.com/use-cases/en/the-most-accurate-alternatives-to-winappdriver)

**Confidence:** 95% - Clear industry shift to FlaUI

---

### ❌ Moq (Mocking Framework)

**Why NOT Recommended:**
- SponsorLink controversy (v4.20+)
- More complex API than NSubstitute
- NSubstitute already integrated and working
- **References:**
  - [Best Mocking Frameworks in .NET: Moq vs. NSubstitute](https://www.jr-it-services.de/best-mocking-frameworks-in-net-moq-vs-nsubstitute/)
  - [Moq vs NSubstitute - Who is the winner?](https://dev.to/cloudx/moq-vs-nsubstitute-who-is-the-winner-40gi)

**Confidence:** 85% - NSubstitute is sufficient for SWT's interface-heavy architecture

---

### ⚠️ MSTest / NUnit

**Why xUnit is Better for SWTSharp:**
- **Parallel execution:** xUnit runs tests in parallel by default (better for CI speed)
- **Platform filtering:** Clean attribute-based platform skipping (`[WindowsFact]`, `[MacOSFact]`)
- **Modern design:** xUnit v2/v3 architecture aligns with modern .NET practices
- **Ecosystem:** Better integration with VSTest platform for custom adapters
- **References:**
  - [NUnit, xUnit, and MTest: Why are they critical](https://testgrid.io/blog/nunit-vs-xunit-vs-mstest/)

**Confidence:** 90% - xUnit's design philosophy matches SWTSharp's needs

---

## Confidence Levels

| Component | Confidence | Rationale |
|-----------|------------|-----------|
| xUnit 2.9.3 | 95% | Proven, stable, already integrated |
| Custom VSTest Adapter | 90% | Architecture correct; needs implementation completion |
| NSubstitute 5.3.0 | 90% | Good fit for interface mocking |
| Coverlet 6.0.4 | 95% | Industry standard for cross-platform coverage |
| Verify (Snapshot Testing) | 85% | Excellent fit for GUI; needs integration |
| BenchmarkDotNet | 90% | Standard tool; critical for SWT parity |
| Stryker.NET | 75% | Valuable but secondary priority |
| Platform Strategies | 90% | Windows/Linux proven; macOS needs completion |
| CI/CD Setup | 95% | Current implementation is solid foundation |

---

## Implementation Roadmap

### Phase 1: Complete Existing Infrastructure (Weeks 1-2)
1. ✅ Maintain xUnit 2.9.3
2. ⚠️ Complete custom VSTest adapter Windows/Linux execution
3. ⚠️ Fix macOS test host result serialization
4. ✅ Maintain Coverlet coverage collection
5. ⚠️ Document platform-specific test patterns

### Phase 2: Enhanced Testing Capabilities (Weeks 3-4)
1. 🆕 Add Verify for snapshot testing (widget layout, graphics)
2. 🆕 Create benchmark project with BenchmarkDotNet
3. 🆕 Add platform-specific test helpers (WindowsTestHelper, MacOSTestHelper, LinuxTestHelper)
4. 🆕 Implement test data builders for complex widget hierarchies

### Phase 3: CI/CD Enhancements (Week 5)
1. ⚠️ Add coverage reporting to GitHub Actions
2. ⚠️ Configure test result artifact uploads
3. 🆕 Add weekly benchmark CI job
4. 🆕 Create test documentation (testing guide, platform quirks)

### Phase 4: Advanced Testing (Future)
1. 🔮 Evaluate Stryker.NET for mutation testing
2. 🔮 Integrate FlaUI/Appium/Dogtail for E2E tests
3. 🔮 Create visual regression test suite (screenshot comparison)
4. 🔮 Add accessibility testing layer

---

## Testing Principles for SWTSharp

### 1. **Platform Abstraction Testing**
- Test public API surface (SWT API compatibility)
- Mock platform layer for unit tests
- Use platform-specific tests for integration validation

### 2. **Threading Model Validation**
- Every platform has threading constraints (especially macOS)
- Tests must verify thread-safe dispatching
- Use `RunOnUIThread` helpers consistently

### 3. **Memory Management**
- SWT requires explicit disposal
- Test disposal chains (parent disposes children)
- Verify native resource cleanup

### 4. **Event Handling**
- Test event registration/unregistration
- Verify event ordering (SWT event lifecycle)
- Test multi-listener scenarios

### 5. **Layout Testing**
- Verify layout calculations (bounds, sizing)
- Test layout managers (GridLayout, FormLayout, etc.)
- Use Verify for layout snapshot comparisons

---

## Key Lessons from Java SWT Testing

1. **SWTBot Abstraction:** High-level test APIs hide low-level SWT complexity
   - **SWTSharp:** `WidgetTestBase` provides similar abstraction
2. **Headless CI:** Linux testing requires Xvfb virtual display
   - **SWTSharp:** Already implemented in GitHub Actions
3. **Platform Quirks:** Each platform has unique threading/windowing requirements
   - **SWTSharp:** Custom test adapter handles macOS Thread 1 requirement
4. **Lifecycle Testing:** Disposal and resource cleanup are critical
   - **SWTSharp:** `AssertWidgetDisposal` tests verify correct behavior

---

## Version Verification (Quality Gate)

All versions verified against official sources as of January 29, 2026:

- ✅ xUnit 2.9.3 - [Official Release Notes](https://xunit.net/releases/v2/2.9.3)
- ✅ NSubstitute 5.3.0 - [NuGet Package](https://www.nuget.org/packages/nsubstitute/)
- ✅ Coverlet 6.0.4 - [NuGet Package](https://www.nuget.org/packages/coverlet.collector/)
- ✅ Verify 28.x - [GitHub Repository](https://github.com/VerifyTests/Verify)
- ✅ BenchmarkDotNet 0.15.4 - [NuGet Package](https://www.nuget.org/packages/BenchmarkDotNet)
- ✅ Stryker.NET 4.11.0 - [NuGet Package](https://www.nuget.org/packages/stryker/)
- ✅ FlaUI - [GitHub Repository](https://github.com/FlaUI/FlaUI)
- ✅ Appium Mac2 Driver - [GitHub Repository](https://github.com/appium/appium-mac2-driver)
- ✅ Dogtail 1.0.7 - [PyPI Package](https://pypi.org/project/dogtail/)

---

## Conclusion

The recommended testing stack balances pragmatism with completeness:

1. **Keep what works:** xUnit 2.9.3, NSubstitute 5.3.0, Coverlet 6.0.4, custom test adapter
2. **Add critical capabilities:** Verify (snapshot testing), BenchmarkDotNet (performance)
3. **Complete existing infrastructure:** macOS test host, Windows/Linux execution paths
4. **Future enhancements:** Mutation testing, E2E automation tools

This approach achieves full test coverage and API compatibility with Java SWT 4.x while respecting platform constraints (especially macOS Thread 1 requirement) and CI/CD integration requirements.

---

**Document Status:** ✅ Ready for roadmap creation
**Quality Gates Passed:**
- [x] Versions verified against official sources
- [x] Rationale explains WHY, not just WHAT
- [x] Confidence levels assigned to each recommendation
- [x] Platform-specific considerations detailed
- [x] CI/CD integration guidance provided
- [x] Alternatives considered with clear reasoning
