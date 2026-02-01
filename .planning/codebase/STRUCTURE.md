# Codebase Structure

**Analysis Date:** 2026-01-29

## Directory Layout

```
swtsharp/
├── src/SWTSharp/                    # Main library code
│   ├── *.cs                         # Core widget classes (Shell, Button, Label, etc.)
│   ├── Control.cs                   # Base for interactive controls
│   ├── Composite.cs                 # Container base class
│   ├── Widget.cs                    # Base for all UI components
│   ├── Display.cs                   # Platform display connection & event loop
│   ├── SWT.cs                       # Constants (style flags, event types)
│   ├── Dialogs/                     # Modal dialogs (FileDialog, ColorDialog, etc.)
│   ├── Events/                      # Event system (Event, Listener interfaces, adapters)
│   ├── Graphics/                    # Graphics abstractions (GC, Color, Font, Image, Device)
│   ├── Layout/                      # Layout managers (GridLayout, FormLayout, etc.)
│   └── Platform/                    # Platform abstraction & implementations
│       ├── I*.cs                    # Interfaces (IPlatform, IPlatformWidget, etc.)
│       ├── PlatformFactory.cs       # Runtime OS detection and factory
│       ├── ObjCRuntime.cs           # Objective-C interop helpers (macOS)
│       ├── SafeHandles/             # Managed wrappers for native handles
│       │   ├── *Handle.cs           # Base SafeHandle classes
│       │   ├── Win32/               # Windows implementations
│       │   ├── MacOS/               # macOS implementations
│       │   └── Linux/               # Linux implementations
│       ├── Win32/                   # Windows implementation (Win32 API)
│       │   ├── Win32Platform.cs     # Main Win32 platform class
│       │   ├── Win32*.cs            # Widget implementations (Win32Button, Win32Text, etc.)
│       │   └── Win32Platform_*.cs   # Partial classes for widget factory methods
│       ├── MacOS/                   # macOS implementation (Cocoa/AppKit)
│       │   ├── MacOSPlatform.cs     # Main macOS platform class
│       │   ├── MacOS*.cs            # Widget implementations (MacOSButton, MacOSText, etc.)
│       │   ├── MacOSPlatform_*.cs   # Partial classes for widget factory methods
│       │   └── MacOSThreading.cs    # GCD (Grand Central Dispatch) threading
│       └── Linux/                   # Linux implementation (GTK)
│           ├── LinuxPlatform.cs     # Main Linux platform class
│           ├── Linux*.cs            # Widget implementations (LinuxButton, LinuxText, etc.)
│           └── LinuxPlatform_*.cs   # Partial classes for widget factory methods
├── tests/
│   ├── SWTSharp.Tests/              # Main test suite
│   │   ├── WidgetTests.cs           # Core widget tests
│   │   ├── Platform/                # Platform-specific tests
│   │   │   ├── WindowsButtonTests.cs
│   │   │   ├── WindowsLabelTests.cs
│   │   │   ├── WindowsTextTests.cs
│   │   │   ├── LinuxButtonTests.cs
│   │   │   ├── LinuxLabelTests.cs
│   │   │   ├── LinuxTextTests.cs
│   │   │   ├── MacOSButtonTests.cs
│   │   │   ├── MacOSLabelTests.cs
│   │   │   ├── MacOSListTests.cs
│   │   │   ├── MacOSComboTests.cs
│   │   │   ├── MacOSCompositeTests.cs
│   │   │   ├── MacOSTextTests.cs
│   │   │   ├── GCDThreadingTests.cs
│   │   │   └── PlatformImportTests.cs
│   │   └── MacOSRunnerTests.cs
│   ├── SWTSharp.TestAdapter/        # Custom xUnit test adapter for SWT threading
│   │   ├── SWTSharpTestDiscoverer.cs
│   │   ├── SWTSharpTestExecutor.cs
│   │   └── ThreadLogger.cs
│   └── SWTSharp.TestHost/           # Test host for custom test execution
├── samples/SWTSharp.Sample/         # Example applications
│   └── Program.cs                   # Sample application entry point
├── .github/workflows/               # CI/CD pipeline
│   └── ci.yml                       # Build, test, package workflow
├── docs/                            # Repository documentation
├── scripts/                         # Utility scripts
├── CLAUDE.md                        # Claude Code instructions
├── README.md                        # Project overview
└── SWTSharp.sln                     # Solution file

```

## Directory Purposes

**`src/SWTSharp/`:**
- Purpose: Core SWT.NET library implementation
- Contains: All public API classes, platform abstraction, graphics system
- Key files: Widget.cs (base class), Display.cs (event loop), SWT.cs (constants)

**`src/SWTSharp/Dialogs/`:**
- Purpose: Modal dialog implementations
- Contains: FileDialog, ColorDialog, FontDialog, MessageBox, DirectoryDialog
- Pattern: Dialog base class with platform-specific implementations in Platform layer

**`src/SWTSharp/Events/`:**
- Purpose: Event system infrastructure
- Contains: Event class (event data), Listener interfaces (typed listeners), adapters
- Key classes: Event, ControlListener, SelectionListener, KeyListener, MouseListener
- Pattern: Base Listener interface + typed listeners with specific event args classes

**`src/SWTSharp/Graphics/`:**
- Purpose: Graphics and resource abstractions
- Contains: GC (graphics context), Color, Font, Image, Device, Point, Rectangle, RGB
- Pattern: Platform-independent public API with platform implementations in Platform/*/Graphics

**`src/SWTSharp/Layout/`:**
- Purpose: Layout manager system
- Contains: Layout base class, GridLayout, FormLayout, FillLayout, RowLayout, StackLayout
- Data classes: GridData, FormData, RowData, FormAttachment for child layout configuration

**`src/SWTSharp/Platform/`:**
- Purpose: Platform abstraction layer
- Contains: Interfaces defining what platforms must implement
- Key interfaces: IPlatform (factory), IPlatformWidget (common widget), IPlatformWindow, IPlatformTextWidget
- Factory: PlatformFactory detects OS and instantiates correct platform at runtime

**`src/SWTSharp/Platform/SafeHandles/`:**
- Purpose: Managed wrappers for native OS handles
- Pattern: Inherit from SafeHandle, override ReleaseHandle() to cleanup
- Base classes: SafeWindowHandle, SafeFontHandle, SafeGraphicsHandle, SafeImageHandle, SafeMenuHandle
- Subclasses: Win32FontHandle, Win32GraphicsHandle, MacOSFontHandle, MacOSGraphicsHandle, etc.

**`src/SWTSharp/Platform/Win32/`:**
- Purpose: Windows implementation using Win32 API
- Contains: Win32Button, Win32Label, Win32Text, Win32Window, etc. (one class per widget type)
- P/Invoke: Windows API constants and extern declarations embedded in each widget class
- Pattern: Partial classes for factory methods in Win32Platform_*.cs files
- Threading: Uses Windows message loop and PostMessage for cross-thread execution
- Key class: Win32Platform.cs - implements IPlatform, instantiates Win32 widgets

**`src/SWTSharp/Platform/MacOS/`:**
- Purpose: macOS implementation using Cocoa/AppKit APIs
- Contains: MacOSButton, MacOSLabel, MacOSText, MacOSWindow, etc.
- Interop: Objective-C runtime via P/Invoke (see ObjCRuntime.cs)
- Threading: Uses Grand Central Dispatch (GCD) dispatch_async to main queue
- Key class: MacOSPlatform.cs + MacOSThreading.cs for GCD handling
- Partial classes: MacOSPlatform_*.cs for widget factory methods

**`src/SWTSharp/Platform/Linux/`:**
- Purpose: Linux implementation using GTK
- Contains: LinuxButton, LinuxLabel, LinuxText, LinuxWindow, etc.
- Interop: GTK 3+ via P/Invoke
- Pattern: Partial classes in LinuxPlatform_*.cs for widget factory methods
- Key class: LinuxPlatform.cs

**`tests/SWTSharp.Tests/`:**
- Purpose: Unit and integration tests
- Location: Co-located with implementation in Platform subdirectories
- Pattern: Test classes follow implementation (e.g., WindowsButtonTests tests Win32Button)
- Custom runner: Uses SWTSharp.TestAdapter for xUnit thread affinity on macOS
- Entry point: Program.cs (custom test runner executable)

**`tests/SWTSharp.TestAdapter/`:**
- Purpose: Custom xUnit test adapter for SWT threading requirements
- Problem: macOS Cocoa requires tests to run on Thread 1
- Solution: Custom ITestFrameworkExecutor that creates Display on correct thread
- Files: SWTSharpTestDiscoverer, SWTSharpTestExecutor, ThreadLogger

**`tests/SWTSharp.TestHost/`:**
- Purpose: Test support library for test execution helpers
- Contains: Utilities used by both TestAdapter and individual tests
- Usage: Referenced by test projects via ProjectReference

**`samples/SWTSharp.Sample/`:**
- Purpose: Example application demonstrating SWT.NET usage
- Contains: Program.cs with runnable sample UI
- Usage: `dotnet run --project samples/SWTSharp.Sample/`

## Key File Locations

**Entry Points:**
- `src/SWTSharp/Display.cs`: Main entry point (Display.Default singleton, event loop)
- `src/SWTSharp/Platform/PlatformFactory.cs`: Platform detection and instantiation
- `samples/SWTSharp.Sample/Program.cs`: Example application
- `tests/SWTSharp.Tests/Program.cs`: Custom test runner

**Configuration:**
- `SWTSharp.sln`: Solution file with project organization
- `src/SWTSharp/SWTSharp.csproj`: Library project settings, target frameworks, NuGet metadata
- `tests/SWTSharp.Tests/SWTSharp.Tests.csproj`: Test project with custom runner
- `global.json`: .NET SDK version

**Core Logic:**
- `src/SWTSharp/Widget.cs`: Base class for all UI components (disposal, events, threading)
- `src/SWTSharp/Control.cs`: Base class for interactive controls (bounds, visibility, enabled)
- `src/SWTSharp/Composite.cs`: Container base (children, layout, tab order)
- `src/SWTSharp/Shell.cs`: Top-level window
- `src/SWTSharp/SWT.cs`: Constants for style flags, event types, colors

**Testing:**
- `tests/SWTSharp.Tests/WidgetTests.cs`: Core widget lifecycle tests
- `tests/SWTSharp.Tests/Platform/`: Platform-specific implementation tests

## Naming Conventions

**Files:**
- Widget classes: `{WidgetName}.cs` (Button.cs, Label.cs, Shell.cs)
- Platform implementations: `{Platform}{WidgetName}.cs` (Win32Button.cs, MacOSButton.cs, LinuxButton.cs)
- Partial factory classes: `{Platform}Platform_{Category}.cs` (Win32Platform_Dialogs.cs, MacOSPlatform_Combo.cs)
- Interfaces: `I{InterfaceName}.cs` (IPlatform.cs, IPlatformWidget.cs)
- Tests: `{Platform}{Category}Tests.cs` (WindowsButtonTests.cs, MacOSTextTests.cs)
- Events: `{EventType}.cs` and `{EventType}Listener.cs` (SelectionEvent.cs, SelectionListener.cs)
- Layout data: `{LayoutType}Data.cs` (GridData.cs, FormData.cs)

**Directories:**
- Platform folders: Named by OS (Win32, MacOS, Linux)
- Test folders: Named by category (Platform/) or feature
- Public API: Root level (Control.cs, Widget.cs, Display.cs)
- Internal abstractions: Nested under parent namespace (Platform/IPlatform.cs)

**Namespaces:**
- Root: `SWTSharp` (public API)
- Subnamespaces: `SWTSharp.Dialogs`, `SWTSharp.Events`, `SWTSharp.Graphics`, `SWTSharp.Layout`
- Platform internal: `SWTSharp.Platform` (IPlatform interfaces)
- Platform implementations: `SWTSharp.Platform.Win32`, `SWTSharp.Platform.MacOS`, `SWTSharp.Platform.Linux`

## Where to Add New Code

**New Widget:**
1. Create public class in `src/SWTSharp/{WidgetName}.cs` inheriting from Control or Composite
2. Add IPlatform{WidgetName} interface in `src/SWTSharp/Platform/IPlatform.cs`
3. Implement `CreateXxxWidget()` method partial class: `src/SWTSharp/Platform/{Platform}Platform_{Category}.cs`
4. Create platform implementation: `src/SWTSharp/Platform/{Platform}/{Platform}{WidgetName}.cs`
5. Add tests: `tests/SWTSharp.Tests/Platform/{Platform}{WidgetName}Tests.cs`
6. Register in PlatformFactory: `src/SWTSharp/Platform/PlatformFactory.cs`

**New Event Type:**
1. Create event args class: `src/SWTSharp/Events/{EventType}Event.cs`
2. Create listener interface: `src/SWTSharp/Events/{EventType}Listener.cs`
3. (Optional) Create adapter: `src/SWTSharp/Events/{EventType}Adapter.cs`
4. Add event type constant to `src/SWTSharp/SWT.cs`

**New Layout Manager:**
1. Create class inheriting from Layout: `src/SWTSharp/Layout/{LayoutName}.cs`
2. Create layout data class: `src/SWTSharp/Layout/{LayoutName}Data.cs` (if needed)
3. Implement ComputeSize() and DoLayout() methods
4. Add tests: `tests/SWTSharp.Tests/{LayoutName}Tests.cs`

**New Dialog:**
1. Create class inheriting from Dialog: `src/SWTSharp/Dialogs/{DialogName}.cs`
2. Add IPlatformDialog{Name} interface if custom layout needed
3. Implement platform-specific classes: `src/SWTSharp/Platform/{Platform}/{Platform}{DialogName}.cs`
4. Add result struct if returning complex data

**Utilities/Helpers:**
- Shared across platforms: `src/SWTSharp/{UtilityName}.cs`
- Platform-specific helpers: `src/SWTSharp/Platform/{Platform}/{PlatformName}Helpers.cs`

**Graphics:**
- Platform-independent abstractions: `src/SWTSharp/Graphics/{ResourceType}.cs`
- Platform-specific implementations: `src/SWTSharp/Platform/{Platform}Graphics.cs` or `{Platform}{ResourceType}.cs`

## Special Directories

**`obj/` and `bin/`:**
- Purpose: Build output (compiled assemblies, intermediates)
- Generated: Yes (automatically by dotnet build)
- Committed: No (in .gitignore)

**`.planning/codebase/`:**
- Purpose: GSD codebase analysis documents
- Generated: By mapping agents
- Committed: Yes
- Files: ARCHITECTURE.md, STRUCTURE.md, CONVENTIONS.md, TESTING.md, STACK.md, INTEGRATIONS.md, CONCERNS.md

**`.github/workflows/`:**
- Purpose: GitHub Actions CI/CD pipeline
- Files: ci.yml (build, test, package)
- Committed: Yes

**`scripts/`:**
- Purpose: Development utility scripts
- Committed: Yes
- Usage: `./scripts/install-hooks.sh` for Git hooks

**`docs/`:**
- Purpose: Repository documentation
- Committed: Yes
- Contents: Architecture docs, API docs, tutorials
