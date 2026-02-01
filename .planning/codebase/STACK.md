# Technology Stack

**Analysis Date:** 2026-01-29

## Languages

**Primary:**
- C# 12.0 (latest) - Full codebase implementation (core library, tests, samples)

**Secondary:**
- PowerShell / Bash - Build and development scripts (`.github/workflows/ci.yml`)

## Runtime

**Environment:**
- .NET 9.0 (SDK version 9.0.100, configured in `global.json`)
- Targets: netstandard2.0, net8.0, net9.0 (multi-targeting support)

**Package Manager:**
- NuGet - Default .NET package manager
- Lockfile: Not applicable (NuGet uses .csproj package references)

## Frameworks

**Core:**
- .NET SDK (Microsoft.NET.Sdk) - Base project type for all projects

**Testing:**
- xUnit 2.9.3 - Primary test framework (`tests/SWTSharp.Tests/SWTSharp.Tests.csproj`)
- NSubstitute 5.3.0 - Mocking framework for unit tests
- Microsoft.NET.Test.Sdk 18.0.0 - Test infrastructure
- xunit.runner.visualstudio 2.8.2 - Visual Studio test explorer integration
- xunit.extensibility.execution 2.9.3 - xUnit execution engine
- xunit.runner.utility 2.9.3 - Test runner utilities
- coverlet.collector 6.0.4 - Code coverage collection

**Build/Dev:**
- MSBuild (integrated with .NET SDK) - Build orchestration
- GitHub Actions (v4-v6) - CI/CD pipeline orchestration
- CodeQL (github/codeql-action) - Static security analysis

## Key Dependencies

**Critical:**
- Microsoft.Web.WebView2 1.0.3595.46 - Browser widget implementation on Windows/.NET 8+ (conditional: only for net8.0 and net9.0)
  - Enables HTML content rendering via native WebView2 control
  - Conditional dependency: not included for netstandard2.0

**Infrastructure:**
- System libraries (implicit) - Threading, IO, Collections, Reflection, Net.Http, Diagnostics
- Platform-specific P/Invoke - Direct OS API calls for Windows (Win32), macOS (Cocoa/AppKit), Linux (GTK3)

## Configuration

**Environment:**
- `global.json` - Pins .NET SDK to 9.0.100 with rollForward set to latestMinor
- Build configuration: Debug, Release
- Platform targets: Any CPU, x64, x86

**Build:**
- `SWTSharp.csproj` (`src/SWTSharp/SWTSharp.csproj`) - Main library
  - Enable unsafe blocks for P/Invoke
  - Treat warnings as errors
  - Enable code analysis (Recommended mode)
  - Generate XML documentation
  - Trim and AOT compatible (with IL2075 suppression for platform reflection)

- `SWTSharp.Tests.csproj` (`tests/SWTSharp.Tests/SWTSharp.Tests.csproj`) - Test project
  - Output type: Exe (custom test runner with Thread 1 support for macOS)
  - Application manifest for Windows Common Controls v6
  - Custom test adapter integration via post-build copy targets

- `SWTSharp.Sample.csproj` (`samples/SWTSharp.Sample/SWTSharp.Sample.csproj`) - Sample application

- `SWTSharp.TestAdapter.csproj` (`tests/SWTSharp.TestAdapter/SWTSharp.TestAdapter.csproj`) - Custom VSTest adapter
  - Targets netstandard2.0

- `SWTSharp.TestHost.csproj` (`tests/SWTSharp.TestHost/SWTSharp.TestHost.csproj`) - Custom test runner host
  - Targets net9.0

## Platform Requirements

**Development:**
- .NET 9.0 SDK (specified in global.json)
- C# 12.0 or later language features
- Platform SDK for Windows, macOS, or Linux development
- For CI: GitHub Actions runners (ubuntu-latest, windows-latest, macos-latest)
  - macOS runner requires CFRunLoop support on Thread 1 for GUI tests
  - Linux runner requires Xvfb, GTK3, WebKitGTK (4.0 and 4.1) for GUI tests

**Production:**
- .NET Standard 2.0: .NET Framework 4.6.1+, .NET Core 2.0+, Unity, Xamarin
- .NET 8.0 LTS or later
- Platform-specific UI frameworks:
  - Windows: Win32 API, WebView2 (for Browser widget on .NET 8+)
  - macOS: Cocoa/AppKit frameworks
  - Linux: GTK 3.0+, WebKitGTK

## Code Quality & Analysis

**Enabled Features:**
- TreatWarningsAsErrors: true
- EnforceCodeStyleInBuild: true
- EnableNETAnalyzers: true (AnalysisLevel: latest, AnalysisMode: Recommended)
- GenerateDocumentationFile: true
- EnableTrimAnalyzer: true
- EnableSingleFileAnalyzer: true
- IsAotCompatible: true (for net8.0 and net9.0)

**Suppressed Rules:** CA1852, CA1859, CA1707, CA2101, CA1510, CA1805, CA1822, CA1419, CA1806, CA1716, CA1720, CA1310, CA1854, CA1513, CA2208, CA1861, CA1305, CA1838, CA1711, CA1708, CA1866, CA1416, IL2075

**Documentation:** XML comments required (NoWarn excludes CS1591 for missing comments during implementation)

---

*Stack analysis: 2026-01-29*
