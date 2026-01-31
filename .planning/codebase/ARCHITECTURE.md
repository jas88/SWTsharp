# Architecture

**Analysis Date:** 2026-01-29

## Pattern Overview

**Overall:** Multi-platform abstraction with platform-specific implementations

**Key Characteristics:**
- Cross-platform GUI toolkit (Java SWT port to .NET)
- Platform abstraction layer via `IPlatform` interface
- Three runtime implementations: Windows (Win32), macOS (Cocoa), Linux (GTK)
- All platforms compiled into single binary; runtime detection selects implementation
- Widget-based hierarchical component model
- Event-driven architecture with typed listeners
- Native look and feel using platform-specific APIs via P/Invoke and Objective-C runtime

## Layers

**Widget Layer (Public API):**
- Purpose: SWT-compatible public API for .NET developers
- Location: `src/SWTSharp/*.cs` (Shell, Button, Label, Text, Composite, etc.)
- Contains: High-level widget classes, event classes, dialogs, graphics abstractions
- Depends on: Platform abstraction interfaces, graphics classes
- Used by: External applications consuming the library

**Platform Abstraction Layer:**
- Purpose: Unifies platform differences through interfaces
- Location: `src/SWTSharp/Platform/I*.cs` (IPlatform, IPlatformWidget, IPlatformWindow, etc.)
- Contains: Interface definitions for platform capabilities
- Depends on: Nothing (pure interfaces)
- Used by: Widget layer, platform implementations

**Platform Implementation Layer:**
- Purpose: Native OS-specific implementations
- Location: `src/SWTSharp/Platform/Win32/`, `src/SWTSharp/Platform/MacOS/`, `src/SWTSharp/Platform/Linux/`
- Contains: Win32Button, MacOSButton, LinuxButton, etc. for each platform
- Depends on: Platform abstraction interfaces, P/Invoke declarations, native API bindings
- Used by: PlatformFactory for runtime instantiation

**Graphics Layer:**
- Purpose: Abstraction for drawing, colors, fonts, images
- Location: `src/SWTSharp/Graphics/*.cs` (GC, Color, Font, Image, Device)
- Contains: Graphics context, resource management, platform-independent drawing API
- Depends on: Platform graphics implementations
- Used by: Widget layer for rendering

**Event System:**
- Purpose: Event propagation and listener management
- Location: `src/SWTSharp/Events/*.cs` (Event, Listener interfaces, typed event args)
- Contains: Base Event class, listener interfaces (ControlListener, SelectionListener, etc.), adapters
- Depends on: Widget classes
- Used by: All widgets for event dispatch

**Layout System:**
- Purpose: Automatic positioning and sizing of child controls
- Location: `src/SWTSharp/Layout/*.cs` (Layout, GridLayout, FormLayout, FillLayout, etc.)
- Contains: Abstract Layout base class, concrete layout implementations, layout data classes
- Depends on: Control classes
- Used by: Composite widgets

**Dialog Layer:**
- Purpose: Modal dialog windows and platform file/color pickers
- Location: `src/SWTSharp/Dialogs/*.cs` (FileDialog, ColorDialog, MessageBox, etc.)
- Contains: Dialog base class, specialized dialogs, result structures
- Depends on: Platform abstraction, shell/window widgets
- Used by: Applications for user interaction

## Data Flow

**Application Startup:**
1. Application creates Display (entry point)
2. Display calls PlatformFactory.Instance
3. PlatformFactory detects OS at runtime via RuntimeInformation
4. PlatformFactory instantiates appropriate platform (Win32Platform, MacOSPlatform, or LinuxPlatform)
5. Display stores platform reference internally

**Widget Creation (e.g., new Button):**
1. Application creates Button with parent Composite and style flags
2. Button constructor calls base Control constructor
3. Control stores parent, style, and Display reference
4. Button calls platform.CreateButtonWidget(parent, style)
5. Platform implementation creates native control (Win32Button, MacOSButton, or LinuxButton)
6. Platform widget stored in Widget._platformWidget (internal property)
7. Platform widget handles all native communication

**Event Handling:**
1. Platform detects native OS event (e.g., WM_COMMAND on Win32, NSButtonCell action on macOS)
2. Platform implementation translates to SWT event type (e.g., SelectionEvent)
3. Platform raises C# event (e.g., Click event)
4. Widget listens to platform events and adds to internal event table
5. Application adds listeners via AddListener or specialized handlers (Click, Selection, etc.)
6. Widget dispatches event to all registered listeners
7. TypedListener bridges untyped Listener interface to typed listeners (ControlListener, SelectionListener)

**Widget Lifecycle:**
1. Application creates widget → platform implementation created
2. Application uses widget → queries/modifies state via properties
3. Platform implementation syncs state to native control
4. Application disposes widget → platform implementation cleans up native resources
5. Native handle released, widget marked as disposed

**State Management:**
- Widget-level state: stored in widget fields (e.g., `_text`, `_visible`, `_enabled`)
- Platform-level state: stored in platform implementation (e.g., HWND on Windows, NSButton pointer on macOS)
- Bidirectional sync: widget properties call platform methods; platform events update widget state
- Example: Shell.Text property calls platform.GetTitle()/SetTitle()

## Key Abstractions

**IPlatform Interface:**
- Purpose: Single access point for all platform services
- Location: `src/SWTSharp/Platform/IPlatform.cs`
- Responsibility: Widget factory methods, event loop control, dialog handling
- Implementation: Win32Platform, MacOSPlatform, LinuxPlatform
- Methods: ProcessEvent(), WaitForEvent(), WakeEventLoop(), CreateButtonWidget(), etc.

**IPlatformWidget Interface:**
- Purpose: Base abstraction for any platform-specific widget
- Location: `src/SWTSharp/Platform/IPlatformWidget.cs`
- Responsibility: Common widget behavior (bounds, visibility, enabled state, parent)
- Specializations: IPlatformWindow, IPlatformTextWidget, IPlatformComposite, etc.

**IPlatformWindow Interface:**
- Purpose: Specialized abstraction for top-level windows (Shell)
- Location: `src/SWTSharp/Platform/IPlatformWidget.cs`
- Methods: GetTitle(), SetTitle(), GetVisible(), SetVisible()

**IPlatformGraphics Interface:**
- Purpose: Platform-specific graphics context implementation
- Implementations: Win32PlatformGraphics, MacOSPlatformGraphics, LinuxPlatformGraphics
- Used by: GC (Graphics Context) class for drawing operations

**SafeHandle Classes:**
- Purpose: Managed wrappers for native handles with automatic resource cleanup
- Location: `src/SWTSharp/Platform/SafeHandles/*.cs`
- Examples: SafeWindowHandle, SafeFontHandle, SafeGraphicsHandle
- Pattern: Inherit from SafeHandle, override ReleaseHandle() to call native cleanup

**Display Class:**
- Purpose: Central connection to platform display system and event loop
- Location: `src/SWTSharp/Display.cs`
- Responsibility: Event queue processing, thread affinity, async execution, shell management
- Singleton pattern: Display.Default is a process-wide singleton
- Thread safety: Lock-protected for multi-threaded access

**Widget Base Class:**
- Purpose: Foundation for all UI components
- Location: `src/SWTSharp/Widget.cs`
- Responsibility: Disposal tracking, event table management, thread validation
- Key method: CheckWidget() - validates widget not disposed and on correct thread

## Entry Points

**Display.Default (static property):**
- Location: `src/SWTSharp/Display.cs`
- Triggers: Application startup
- Responsibilities: Lazily creates thread-local Display, provides singleton pattern

**Widget Constructor Chain:**
- Location: Base Control/Shell classes
- Triggers: `new Shell()`, `new Button(parent)`, etc.
- Responsibilities: Initialize display reference, parent relationship, style storage, platform widget creation

**Display.ReadAndDispatch() / Display.Sleep():**
- Location: `src/SWTSharp/Display.cs`
- Triggers: Event loop (called repeatedly in application main loop)
- Responsibilities: Process pending events or sleep until event available

**PlatformFactory.Instance (static property):**
- Location: `src/SWTSharp/Platform/PlatformFactory.cs`
- Triggers: First widget creation or Display initialization
- Responsibilities: Detect OS at runtime, instantiate correct platform implementation

## Error Handling

**Strategy:** Exception-based with custom SWT exception types

**Patterns:**
- `SWTDisposedException` - thrown when accessing disposed widgets
- `SWTInvalidThreadException` - thrown when accessing widget from wrong thread
- `SWTException` - base class for SWT-specific errors
- Platform exceptions: P/Invoke failures wrapped in managed exceptions
- CheckWidget() method: Guards all public APIs against disposed/wrong-thread access

## Cross-Cutting Concerns

**Logging:**
- Diagnostic logging via SWTSHARP_DEBUG environment variable
- Example: PlatformFactory logs platform detection when SWTSHARP_DEBUG=1

**Validation:**
- Thread validation: All public widget access checked against Display thread
- Disposal checking: CheckWidget() guards against use-after-dispose
- Style validation: Constructor style bits passed to platform implementation

**Authentication:**
- Not applicable (no auth system in GUI library)

**Thread Safety:**
- Display uses locks (Lock in NET9_0, object lock otherwise)
- Platform implementations use ConcurrentDictionary for handle-to-instance mapping (e.g., Win32Button._buttonInstances)
- Composite._children protected by lock
- Widget event table protected by _eventTableLock

**Resource Management:**
- IDisposable pattern on all major classes (Widget, Display, GC, etc.)
- SafeHandle subclasses for native handle cleanup
- Dispose chain: parent disposal propagates to children
- Shell disposal triggers removal from Display shell list
