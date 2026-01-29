# External Integrations

**Analysis Date:** 2026-01-29

## APIs & External Services

**Browser Rendering:**
- Microsoft WebView2 - HTML content rendering in Browser widget
  - SDK/Client: Microsoft.Web.WebView2 (1.0.3595.46)
  - Platform: Windows only (.NET 8.0 and net9.0)
  - Requires: Dynamic code generation (not AOT-compatible on Windows)
  - Environment var: None required
  - Implementation: Conditionally included in net8.0/net9.0 builds, unavailable for netstandard2.0

## Data Storage

**Databases:**
- None - SWTSharp is a UI library with no persistent data storage

**File Storage:**
- Local filesystem only - Graphics images and resources loaded from disk
- No cloud storage integration

**Caching:**
- None - In-memory widget and layout management only

## Authentication & Identity

**Auth Provider:**
- None - SWTSharp is a UI framework with no built-in authentication
- Applications using SWTSharp implement their own authentication

## Monitoring & Observability

**Error Tracking:**
- None configured - Error handling delegated to consuming applications

**Logs:**
- Console logging via SWTSHARP_DEBUG environment variable
  - Enabled in CI pipeline for diagnostic output
  - Used for platform detection debugging (`SWTSHARP_DEBUG=1`)
  - Output to `stderr.txt` and stdout during builds

**Code Analysis:**
- CodeQL (github/codeql-action v4) - Static security analysis
  - Language: C#
  - Config: `.github/codeql/codeql-config.yml`
  - Runs on Linux test job in CI pipeline
  - Results uploaded to GitHub Security tab

## CI/CD & Deployment

**Hosting:**
- GitHub - Repository hosting and release distribution

**CI Pipeline:**
- GitHub Actions (`.github/workflows/ci.yml`)
  - Triggers: push to main/develop branches, pull requests, version tags (v*)
  - Platforms: Windows (windows-latest), macOS (macos-latest), Linux (ubuntu-latest)
  - Build Matrix: 3 parallel test jobs per commit
  - Custom test runner environment: Thread 1 dispatch on macOS, Xvfb/GTK on Linux

**Package Distribution:**
- NuGet.org API v3 (`https://api.nuget.org/v3/index.json`)
  - Auto-publish on version tag (v*)
  - Publishes: SWTSharp.nupkg and SWTSharp.snupkg (symbols)
  - API key: `NUGET_API_KEY` secret

**Release Management:**
- GitHub Releases (softprops/action-gh-release v2)
  - Auto-created on version tag
  - Includes: NuGet package, symbols, release notes
  - Triggered by: git tag with v* prefix

## Environment Configuration

**Required env vars:**
- None - No external service credentials needed for library functionality

**Optional env vars:**
- `SWTSHARP_DEBUG`: Set to "1" for diagnostic platform detection logging
- `DISPLAY`: Set to `:99` on Linux CI (Xvfb virtual display)
- `CI`: Set to "true" for deterministic builds
- `CODECOV_TOKEN`: GitHub secret for Codecov coverage uploads (failing job continues)

**Secrets location:**
- GitHub Secrets (`.github/workflows/ci.yml`):
  - `NUGET_API_KEY` - NuGet.org API token for package publishing
  - `CODECOV_TOKEN` - Codecov.io API token for coverage reports (optional, non-blocking)
  - `GITHUB_TOKEN` - Auto-generated token for GitHub Releases

## Webhooks & Callbacks

**Incoming:**
- None - SWTSharp is a library, not a service

**Outgoing:**
- NuGet.org API - dotnet nuget push on release tags
- GitHub API - Create releases via GitHub Actions
- Codecov.io API - Upload coverage reports from CI jobs (non-blocking failures)

## Platform-Specific Integration

**Windows:**
- Win32 API (P/Invoke)
  - Direct native calls for window management, message handling, widget rendering
  - WebView2 for Browser widget (dynamic code generation required)

**macOS:**
- Cocoa/AppKit frameworks (P/Invoke)
  - Native window and control management
  - CFRunLoop dispatch for Thread 1 requirement in GUI tests

**Linux:**
- GTK 3.0+ libraries (P/Invoke)
  - libgtk-3-0, libglib2.0-0, libx11-dev
  - WebKitGTK (libwebkit2gtk-4.1-0 and libwebkit2gtk-4.0-37)
  - Virtual display support (Xvfb :99) for CI headless testing

## Test Infrastructure

**Test Frameworks & Runners:**
- xUnit 2.9.3 - Test framework
- Custom test runner - Executable runner for Thread 1 support on macOS
- Custom VSTest adapter - `SWTSharp.TestAdapter` for IDE integration
- NSubstitute 5.3.0 - Mocking library

**Test Output:**
- Format: TRX (Visual Studio Test Results XML)
- Location: `**/test-results.trx` (uploaded as artifacts in CI)
- Coverage: Collected via coverlet.collector 6.0.4

---

*Integration audit: 2026-01-29*
