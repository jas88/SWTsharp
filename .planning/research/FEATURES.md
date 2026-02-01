# Features Research: Eclipse SWT 4.x API

**Research Date:** 2026-01-29
**Project:** SWTSharp - .NET port of Eclipse SWT 4.x
**Goal:** API compatibility with Java SWT 4.x for brownfield porting project

**Current Status:**
- 89 widget/class files implemented (excluding platform implementations)
- 173+ TODO comments across 51 files
- Partial implementations for most core widgets

---

## Table Stakes (Core Widgets)

### Priority 1: Essential UI Controls (MUST HAVE)
**Complexity: Medium | Dependencies: Display, Shell, Control base**

All widgets from [org.eclipse.swt.widgets](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/widgets/package-summary.html):

- ✅ **Button** - Selectable user interface object (PUSH, CHECK, RADIO, TOGGLE, ARROW)
- ✅ **Label** - Non-selectable text or image display
- ✅ **Text** - Editable text field (SINGLE, MULTI, READ_ONLY, PASSWORD, SEARCH, WRAP)
- ✅ **Combo** - Drop-down list with optional editable text field
- ✅ **List** - Scrollable list of selectable strings (SINGLE, MULTI selection)
- ✅ **Canvas** - Surface for custom drawing
- ⚠️ **Composite** - Container for other controls (TODOs present)
- ⚠️ **Group** - Labeled border around child controls (13 TODOs)
- ✅ **Shell** - Top-level window or dialog
- ⚠️ **ProgressBar** - Visual progress indicator (HORIZONTAL, VERTICAL, SMOOTH, INDETERMINATE)
- ⚠️ **Slider** - Numeric value selector with thumb (2 TODOs)
- ⚠️ **Scale** - Numeric value selector with tick marks (3 TODOs)
- ⚠️ **Spinner** - Numeric value with increment/decrement buttons (3 TODOs)
- ✅ **Link** - Hyperlink text with embedded URLs

**Status:** ~80% complete. Core functionality exists but TODOs indicate missing methods/features.

### Priority 2: Hierarchical Data Controls (MUST HAVE)
**Complexity: High | Dependencies: Control, Item classes, platform-specific rendering**

- ⚠️ **Table** - Multi-column data grid (1 TODO)
  - ⚠️ **TableColumn** - Column definition (12 TODOs)
  - ⚠️ **TableItem** - Individual row (2 TODOs)
  - Features: sorting, virtual mode, check boxes, full selection

- ⚠️ **Tree** - Hierarchical data display (4 TODOs)
  - **TreeColumn** - Column for tree with details
  - ⚠️ **TreeItem** - Individual node (5 TODOs)
  - Features: check boxes, virtual mode, expand/collapse

**Status:** ~70% complete. Basic functionality works but advanced features (virtual mode, custom drawing) need completion.

### Priority 3: Container & Layout Widgets (MUST HAVE)
**Complexity: Medium | Dependencies: Control base**

- ⚠️ **TabFolder** - Tabbed interface container (2 TODOs)
  - ⚠️ **TabItem** - Individual tab (2 TODOs)

- ⚠️ **CoolBar** - Toolbar with movable/resizable bands
  - **CoolItem** - Individual band in CoolBar

- ⚠️ **ToolBar** - Standard button toolbar (9 TODOs)
  - ⚠️ **ToolItem** - Individual toolbar button (4 TODOs)

- ⚠️ **Menu** - Menu bar, popup, or drop-down (5 TODOs)
  - ⚠️ **MenuItem** - Individual menu entry (7 TODOs)

- ⚠️ **ExpandBar** - Collapsible sections container
  - **ExpandItem** - Individual collapsible section

- **Sash** - Movable divider for resizing areas
- **ScrollBar** - Scrollbar control (usually embedded in Scrollable)

**Status:** ~75% complete. Basic functionality implemented, advanced features pending.

### Priority 4: System & Special Widgets (MUST HAVE)
**Complexity: High | Dependencies: OS integration**

- ✅ **Display** - Event loop and system resources manager (CORE - mostly complete)
- **Widget** - Abstract base class for all widgets
- **Control** - Abstract base for all UI controls
- **Scrollable** - Base for scrollable controls
- **Item** - Abstract base for items (TableItem, TreeItem, etc.)
- **Decorations** - Window decorations base class
- **Caret** - Text cursor/insertion point
- **IME** - Input Method Editor support
- **Tracker** - Visual rectangle tracking for resize/move
- **Monitor** - Display screen information
- **TaskBar** - System taskbar integration (Windows 7+)
- **TaskItem** - Individual taskbar item
- **Tray** - System tray integration
- **TrayItem** - System tray icon

**Status:** Core classes ~80% complete. System integration features need platform-specific work.

### Priority 5: Date/Time & Browser (MUST HAVE)
**Complexity: High | Dependencies: Platform-specific controls**

- **DateTime** - Date and time picker (DATE, TIME, CALENDAR, SHORT, MEDIUM, LONG)
- ✅ **Browser** - Embedded web browser (MOZILLA, WEBKIT)
  - Platform: WebView2 (Windows), WebKit (macOS), WebKitGTK (Linux)
  - Dependencies: [org.eclipse.swt.browser](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/browser/package-summary.html) events and listeners

**Status:** DateTime ~50% complete. Browser basic structure exists but needs full listener/event implementation.

---

## Table Stakes (Graphics)

### Graphics Core (MUST HAVE)
**Complexity: High | Dependencies: Platform graphics APIs**

From [org.eclipse.swt.graphics](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/package-summary.html):

- ✅ **Color** - RGB color with alpha
- ✅ **Font** - Text font with style
- ✅ **FontData** - Font descriptor
- **FontMetrics** - Font measurement data
- ✅ **GC** (Graphics Context) - Drawing surface (TODOs present)
- ✅ **Image** - Bitmap/icon image
- **ImageData** - Raw image pixel data
- **ImageLoader** - Multi-format image I/O
- **PaletteData** - Color palette for indexed images
- **Cursor** - Mouse cursor shapes
- ✅ **RGB** - RGB color value
- **RGBA** - RGBA color with alpha
- **Point** - X/Y coordinate
- **Rectangle** - Position and size
- **Region** - Geometric shape for clipping
- **Path** - Vector drawing path
- **Pattern** - Fill pattern (gradient, texture)
- **Transform** - 2D affine transformation matrix
- **TextLayout** - Advanced text rendering with styles
- **LineAttributes** - Line drawing attributes (width, cap, join, dash)
- **TextStyle** - Text styling (font, color, underline, strikeout)
- **GlyphMetrics** - Individual character metrics
- **Device** - Abstract graphics device
- **DeviceData** - Device configuration

**Drawing Operations Required:**
- Lines, rectangles, ovals, arcs, polygons, polylines
- Text rendering with anti-aliasing
- Image drawing (scaling, transparency)
- Clipping regions
- Transformations (translate, rotate, scale)
- Alpha blending

**Status:** ~60% complete. Basic drawing works but advanced features (transforms, patterns, TextLayout) need work.

---

## Table Stakes (Layouts)

### Layout Managers (MUST HAVE)
**Complexity: Medium | Dependencies: Composite, Control sizing**

From [org.eclipse.swt.layout](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/layout/package-summary.html):

- ✅ **FillLayout** - Simple fill/wrap layout (HORIZONTAL, VERTICAL)
- ✅ **RowLayout** - Flow layout with wrapping
  - ✅ **RowData** - Per-control sizing hints

- ✅ **GridLayout** - Grid-based layout (most common)
  - ✅ **GridData** - Cell positioning and spanning

- ✅ **FormLayout** - Constraint-based positioning
  - ✅ **FormData** - Edge attachment rules
  - ✅ **FormAttachment** - Attachment to edges/controls

- ✅ **StackLayout** - Show one child at a time
- **BorderLayout** - 5-region layout (NORTH, SOUTH, EAST, WEST, CENTER)
  - **BorderData** - Region specification

**Status:** ~90% complete. Core layouts work well. BorderLayout not yet implemented.

---

## Table Stakes (Events)

### Event System (MUST HAVE)
**Complexity: Medium | Dependencies: Widget base, typed event dispatch**

From [org.eclipse.swt.events](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/events/package-summary.html):

**Mouse Events:**
- ✅ **MouseEvent** - Mouse button and position
- ✅ **MouseListener** - Mouse click events
- ✅ **MouseMoveListener** - Mouse movement
- ✅ **MouseTrackListener** - Mouse enter/exit/hover
- **MouseWheelListener** - Mouse wheel scrolling

**Keyboard Events:**
- ✅ **KeyEvent** - Key press/release with modifiers
- ✅ **KeyListener** - Key events

**Focus Events:**
- ✅ **FocusEvent** - Focus gain/loss
- ✅ **FocusListener** - Focus change notifications

**Selection Events:**
- ✅ **SelectionEvent** - Widget selection (button, list, menu)
- ✅ **SelectionListener** - Selection notifications
- ✅ **SelectionAdapter** - Default implementations

**Control Events:**
- ✅ **ControlEvent** - Resize, move
- ✅ **ControlListener** - Control change notifications

**Widget Events:**
- ✅ **DisposeEvent** - Widget disposal
- ✅ **DisposeListener** - Cleanup notification
- **PaintEvent** - Repaint request
- **PaintListener** - Custom drawing
- **ModifyEvent** - Text modification
- **ModifyListener** - Text change notification
- **VerifyEvent** - Input validation
- **VerifyListener** - Pre-modification validation

**Shell Events:**
- **ShellEvent** - Window state changes
- **ShellListener** - Window events
- **ShellAdapter** - Default implementations

**Tree Events:**
- **TreeEvent** - Tree expand/collapse
- **TreeListener** - Tree change notifications
- **TreeAdapter** - Default implementations

**Other Events:**
- **ArmEvent** - Menu item armed
- **DragDetectEvent** - Drag gesture detected
- **ExpandEvent** - ExpandBar expand/collapse
- **GestureEvent** - Touch gestures
- **HelpEvent** - F1 help request
- **MenuEvent** - Menu show/hide
- **MenuDetectEvent** - Context menu request
- **SegmentEvent** - BiDi text segmentation
- **TouchEvent** - Touch input
- **TraverseEvent** - Tab/arrow key navigation

**Core Infrastructure:**
- ✅ **TypedListener** - Event adapter wrapper
- ✅ **Listener** - Untyped event handler
- ✅ **Event** - Generic event object

**Status:** ~70% complete. Common events implemented. Advanced events (gestures, touch, BiDi) not yet done.

---

## Table Stakes (Dialogs)

### Standard Dialogs (MUST HAVE)
**Complexity: Low-Medium | Dependencies: Shell, platform native dialogs**

- ⚠️ **Dialog** - Base class for dialogs
- ⚠️ **MessageBox** - Alert/confirmation dialog (1 TODO)
  - Buttons: OK, CANCEL, YES, NO, RETRY, ABORT, IGNORE
  - Icons: ERROR, INFORMATION, QUESTION, WARNING, WORKING

- ⚠️ **FileDialog** - File open/save picker (1 TODO)
  - Modes: OPEN, SAVE, MULTI (multiple selection)

- ⚠️ **DirectoryDialog** - Folder picker (1 TODO)
- ⚠️ **ColorDialog** - Color picker (1 TODO)
- ⚠️ **FontDialog** - Font selector (5 TODOs)
- **PrintDialog** - Printer selection
  - See [org.eclipse.swt.printing](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/printing/package-summary.html)

**Status:** ~60% complete. Basic dialogs work but need refinement. PrintDialog not implemented.

---

## Table Stakes (Custom Widgets)

### Advanced Custom Controls (MUST HAVE)
**Complexity: High | Dependencies: Pure Java implementations, complex rendering**

From [org.eclipse.swt.custom](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/custom/package-summary.html):

**Priority 1: Text Editing**
- ⚠️ **StyledText** - Rich text editor with syntax highlighting (1 TODO in Linux impl)
  - Features: Styles, colors, fonts, word wrap, line numbers
  - Events: LineStyleListener, LineBackgroundListener, CaretListener
  - **StyleRange** - Text formatting range
  - **Bullet** - List bullets/numbering
  - **ST** - StyledText constants

**Priority 2: Enhanced Tabs**
- **CTabFolder** - Enhanced tab control with close buttons
  - **CTabItem** - Individual tab
  - **CTabFolderEvent** - Tab events
  - **CTabFolder2Adapter** - Event adapter
  - **CTabFolderRenderer** - Custom rendering

**Priority 3: Layout Helpers**
- **SashForm** - Resizable panes with sash
- **ViewForm** - Eclipse view-style container
- **CBanner** - Curved banner with left/center/right areas
- **ScrolledComposite** - Scrolling container for large content

**Priority 4: Editors**
- **ControlEditor** - Embed editor in control
- **TableEditor** - In-place table cell editing
- **TreeEditor** - In-place tree node editing
- **TableCursor** - Spreadsheet-style cell selection
- **TreeCursor** - Tree cell selection

**Priority 5: Other Custom Widgets**
- **CLabel** - Label with gradient background and image positioning
- **CCombo** - Custom-drawn combo box
- **PopupList** - Popup selection list
- **BusyIndicator** - Show busy cursor during operation

**Events:**
- **BidiSegmentEvent** - BiDi text segmentation
- **CaretEvent** - Text caret movement
- **ExtendedModifyEvent** - Detailed text modification
- **LineBackgroundEvent** - Line background painting
- **LineStyleEvent** - Line styling
- **MovementEvent** - Text movement
- **PaintObjectEvent** - Custom object painting in text
- **TextChangedEvent** - Text content changed
- **TextChangingEvent** - Text about to change

**Status:** ~40% complete. StyledText structure exists but many features incomplete. Other custom widgets not started.

---

## Table Stakes (Browser Integration)

### Web Browser Component (MUST HAVE)
**Complexity: Very High | Dependencies: Platform web engines**

From [org.eclipse.swt.browser](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/browser/package-summary.html):

- ✅ **Browser** - Embedded web browser widget
  - Style: MOZILLA, WEBKIT (platform-specific)
  - Platform: WebView2 (Win), WebKit (macOS), WebKitGTK (Linux)

**Navigation & Content:**
- `setUrl(String)` - Navigate to URL
- `setText(String)` - Set HTML content
- `setHtml(String)` - Set HTML with base URL
- `execute(String)` - Execute JavaScript
- `evaluate(String)` - Evaluate JS and return result
- `forward()`, `back()`, `refresh()`, `stop()`

**Listeners:**
- **LocationListener** - URL navigation events
  - `changing()` - Before navigation (can cancel)
  - `changed()` - After navigation
  - **LocationAdapter** - Default implementations

- **ProgressListener** - Page load progress
  - **ProgressAdapter**

- **StatusTextListener** - Status bar text

- **TitleListener** - Page title changes

- **VisibilityWindowListener** - Window visibility
  - **VisibilityWindowAdapter**

- **OpenWindowListener** - New window requests

- **CloseWindowListener** - Window close requests

- **AuthenticationListener** - HTTP authentication

**Functions:**
- **BrowserFunction** - Expose Java/C# methods to JavaScript

**Status:** ~30% complete. Basic structure exists. Needs full listener/event implementation and platform integration.

---

## Table Stakes (Drag & Drop)

### DND Support (MUST HAVE)
**Complexity: High | Dependencies: Platform DND APIs, clipboard integration**

From [org.eclipse.swt.dnd](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/dnd/package-summary.html):

**Core Classes:**
- **DragSource** - Drag operation initiator
  - Operations: MOVE, COPY, LINK
  - **DragSourceEvent** - Drag events
  - **DragSourceListener** - Drag callbacks
  - **DragSourceAdapter** - Default implementations

- **DropTarget** - Drop operation receiver
  - **DropTargetEvent** - Drop events
  - **DropTargetListener** - Drop callbacks
  - **DropTargetAdapter** - Default implementations

**Transfer Types:**
- **Transfer** - Base class for data conversion
- **TextTransfer** - Plain text
- **RTFTransfer** - Rich text format
- **HTMLTransfer** - HTML content
- **URLTransfer** - URLs
- **FileTransfer** - File paths
- **ImageTransfer** - Images
- **ByteArrayTransfer** - Raw bytes

**Utilities:**
- **DND** - Constants and utilities
- **Clipboard** - System clipboard access
- **TransferData** - Platform-specific data format

**Status:** Not implemented. Critical for Eclipse integration and general usability.

---

## Table Stakes (Accessibility)

### Accessibility Support (MUST HAVE for compliance)
**Complexity: High | Dependencies: Platform accessibility APIs**

From [org.eclipse.swt.accessibility](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/accessibility/package-summary.html):

**Core:**
- **Accessible** - Bridge to assistive technology
  - `addAccessibleListener()` - Add AT support
  - `addAccessibleControlListener()` - Control info
  - `addAccessibleTextListener()` - Text info
  - `addAccessibleActionListener()` - Action info
  - `addAccessibleEditableTextListener()` - Editable text
  - `addAccessibleHyperlinkListener()` - Hyperlinks
  - `addAccessibleTableListener()` - Table data
  - `addAccessibleValueListener()` - Value controls

**Listeners & Events:**
- **AccessibleListener** - Basic info (name, description, help)
- **AccessibleControlListener** - Control properties
- **AccessibleTextListener** - Text content
- **AccessibleActionListener** - Available actions
- **AccessibleEditableTextListener** - Text editing
- **AccessibleHyperlinkListener** - Link info
- **AccessibleTableListener** - Table structure
- **AccessibleValueListener** - Numeric values

**Adapters:**
- **AccessibleAdapter** - Default implementations
- **AccessibleControlAdapter**
- **AccessibleTextAdapter**
- **AccessibleActionAdapter**
- **AccessibleEditableTextAdapter**
- **AccessibleHyperlinkAdapter**
- **AccessibleTableAdapter**
- **AccessibleValueAdapter**

**Constants:**
- **ACC** - Accessibility constants

**Status:** Not implemented. Required for government/enterprise compliance.

---

## Table Stakes (Printing)

### Print Support (MUST HAVE)
**Complexity: Medium | Dependencies: Platform print APIs**

From [org.eclipse.swt.printing](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/printing/package-summary.html):

- **Printer** - Print device
  - `getPrinterList()` - Available printers
  - `startJob()`, `endJob()` - Print job control
  - `startPage()`, `endPage()` - Page rendering
  - Extends Device, so GC can draw to it

- **PrinterData** - Printer configuration
  - Paper size, orientation (PORTRAIT, LANDSCAPE)
  - Copies, collation
  - Page range
  - Duplex mode

- **PrintDialog** - Printer selection dialog

**Status:** Not implemented. Important for desktop applications.

---

## Differentiators

### 1. Enhanced .NET Integration
**Complexity: Medium | Priority: High**

- **WPF/WinForms Interop** - Host SWT in WPF/WinForms and vice versa
  - `ElementHost` for WPF embedding
  - `WindowsFormsHost` for WinForms
  - Proper focus/input handling across boundaries

- **XAML-style Data Binding** - Optional binding layer
  - `INotifyPropertyChanged` support
  - Binding expressions for widget properties
  - Two-way binding for input controls

- **Async/Await Display Events** - Modern async patterns
  - `Display.RunAsync()` for UI thread marshalling
  - Task-based event handlers
  - `CancellationToken` support

### 2. Modern C# Features
**Complexity: Low | Priority: High**

- **Extension Methods** - Fluent API
  ```csharp
  new Button(shell, SWT.PUSH)
      .WithText("Click Me")
      .WithSize(100, 30)
      .OnSelection(e => Console.WriteLine("Clicked"));
  ```

- **LINQ for Widgets** - Query widget trees
  ```csharp
  shell.Descendants<Button>()
      .Where(b => b.Text.Contains("OK"))
      .ForEach(b => b.Enabled = false);
  ```

- **Pattern Matching** - Modern C# syntax
  - Switch expressions for event handling
  - Type patterns for widget casting

- **Nullable Reference Types** - Null safety
  - Annotate all public APIs
  - Reduce null reference exceptions

### 3. Performance Enhancements
**Complexity: Medium-High | Priority: Medium**

- **Native AOT Support** - Fast startup, small size
  - Compatible with .NET 8+ Native AOT
  - Trim-friendly design
  - No reflection in hot paths

- **Span<T> for Graphics** - Zero-copy rendering
  - Use `Span<byte>` for image data
  - `Memory<T>` for buffers

- **Modern Threading** - Better concurrency
  - `Channel<T>` for event queuing
  - `ValueTask` for hot paths
  - Pooled objects for frequent allocations

### 4. Developer Experience
**Complexity: Low-Medium | Priority: High**

- **IntelliSense-Friendly** - Better IDE support
  - XML documentation for all public APIs
  - Code snippets for common patterns
  - Roslyn analyzers for best practices

- **Debug Visualizers** - Better debugging
  - Custom visualizers for Image, Color, Rectangle
  - Widget tree visualizer
  - Event trace visualizer

- **Source Generators** - Compile-time code gen
  - Generate boilerplate for custom widgets
  - Automatic dispose pattern implementation

- **Hot Reload Support** - .NET 6+ hot reload
  - Reload layout changes without restart
  - Safe property updates

### 5. Cross-Platform Enhancements
**Complexity: High | Priority: Medium**

- **Platform Detection** - Automatic platform selection
  - No need to reference platform-specific assemblies
  - Runtime platform detection

- **Unified Packaging** - Single NuGet package
  - Platform-specific natives embedded
  - RID-specific assets

- **Linux Wayland Support** - Modern Linux
  - Support both X11 and Wayland
  - Runtime detection and fallback

### 6. Additional Features Not in Java SWT
**Complexity: Varies | Priority: Low-Medium**

- **Vector Graphics** - SVG support
  - Native SVG rendering in Image
  - SVG icons scale without blur

- **Animation Framework** - Smooth transitions
  - Property animations (size, position, opacity)
  - Easing functions
  - Timeline-based animations

- **Modern Input** - Touch and pen
  - Multi-touch gestures (pinch, rotate)
  - Pressure-sensitive pen input
  - Better high-DPI support

- **Dark Mode** - Automatic theme switching
  - Respond to OS theme changes
  - Per-window theme override

---

## Anti-features (Do Not Port)

### 1. Deprecated Java APIs
**Do not implement deprecated SWT classes:**

- **TypedListener** (deprecated) - Internal implementation detail
- Old event model methods - Use typed listeners only
- Platform-specific internal classes in `org.eclipse.swt.internal.*`

### 2. Java-Specific Workarounds
**These are Java limitations, not needed in C#:**

- **SWT Thread Checks** - Too strict
  - Java SWT throws on non-UI thread access
  - C# can use `BeginInvoke()` pattern instead
  - Warning/debug-only checks, not exceptions

- **Dispose Pattern Complexity** - C# has `IDisposable`
  - Don't require manual dispose() for every widget
  - Use finalizers + SafeHandles for native resources
  - Optional aggressive disposal for resource-constrained scenarios

### 3. Legacy Features
**Modern alternatives exist:**

- **AWT/Swing Interop** - Not relevant to .NET
- **SWT on Java Applets** - Applets are dead
- **Eclipse-Specific Integration** - Focus on general desktop apps
  - Don't require Eclipse Plugin infrastructure
  - Don't tie to Eclipse workspace APIs

### 4. OpenGL via SWT
**Use specialized libraries instead:**

- **org.eclipse.swt.opengl** package - Anti-feature
  - [SWT OpenGL](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/opengl/package-summary.html) is minimal wrapper
  - For .NET, use OpenTK, Veldrid, or Silk.NET
  - SWT Canvas can host these libraries
  - Don't replicate GLCanvas - it's redundant

### 5. Platform-Specific Quirks
**Don't replicate Java's platform issues:**

- **GTK2 vs GTK3 switches** - .NET can use modern GTK4
- **Carbon API support** - macOS Carbon is dead, use Cocoa only
- **Windows XP workarounds** - .NET 6+ doesn't support XP anyway

### 6. Over-Abstraction
**Keep it simple:**

- **Multiple inheritance workarounds** - C# has interfaces
- **Anonymous inner classes** - C# has lambdas
- **Verbose event registration** - Use C# events and delegates

---

## Dependencies

### Inter-Package Dependencies (SWT Internal)

```
Widget (base)
  ↓
Control → Scrollable → Composite → Shell
  ↓          ↓
Item     Canvas
  ↓
Menu
```

**Critical Path:**
1. **Display** → Foundation for event loop, must work first
2. **Widget** → Base class for all widgets
3. **Control** → Base for UI controls
4. **Composite** → Container for layouts
5. **Shell** → Top-level window
6. **Layouts** → Positioning system
7. **Events** → User interaction
8. **Graphics** → Drawing and images

### External Dependencies

**Platform Native Libraries:**
- **Windows:** Win32 API (user32, gdi32, comctl32, WebView2)
- **macOS:** Cocoa, WebKit
- **Linux:** GTK3/GTK4, Cairo, WebKitGTK

**Optional .NET Dependencies:**
- **System.Drawing.Common** - For Color/Font interop (Windows only)
- **SkiaSharp** - Alternative 2D graphics backend
- **WebView2 SDK** - For Browser on Windows

### Feature Dependencies

| Feature | Depends On | Complexity |
|---------|-----------|-----------|
| Button | Control, Events | Low |
| Table | Scrollable, Item, Graphics | High |
| StyledText | Canvas, TextLayout, Events | Very High |
| Browser | WebView host, JS bridge | Very High |
| DND | Clipboard, Transfer | High |
| Printing | Printer APIs, GC | Medium |
| Accessibility | Platform AT APIs | High |

### Implementation Order (Recommended)

**Phase 1: Foundation (Weeks 1-4)**
1. Display, Widget, Control basics
2. Shell, Composite
3. Basic events (Mouse, Key, Selection)
4. Simple widgets (Button, Label, Text)

**Phase 2: Layout & Graphics (Weeks 5-8)**
5. Layout managers (FillLayout, GridLayout)
6. Graphics (GC, Color, Font, Image basics)
7. Canvas and custom drawing

**Phase 3: Complex Widgets (Weeks 9-16)**
8. List, Combo
9. Table, Tree (basic)
10. Menu, ToolBar
11. TabFolder, Group

**Phase 4: Advanced Features (Weeks 17-24)**
12. Table/Tree (virtual, custom draw)
13. Browser integration
14. Custom widgets (StyledText, CTabFolder)
15. Drag & Drop

**Phase 5: Polish & Compliance (Weeks 25+)**
16. Accessibility
17. Printing
18. Remaining dialogs
19. Performance optimization
20. Platform-specific refinements

---

## Sources

This research is based on official Eclipse SWT documentation:

- [SWT Documentation](https://eclipse.dev/eclipse/swt/docs.html)
- [org.eclipse.swt Package](https://help.eclipse.org/latest/rtopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/package-summary.html)
- [org.eclipse.swt.widgets Package](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/widgets/package-summary.html)
- [org.eclipse.swt.graphics Package](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/graphics/package-summary.html)
- [org.eclipse.swt.layout Package](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/layout/package-summary.html)
- [org.eclipse.swt.events Package](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/events/package-summary.html)
- [org.eclipse.swt.custom Package](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/custom/package-summary.html)
- [org.eclipse.swt.browser Package](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/browser/package-summary.html)
- [org.eclipse.swt.dnd Package](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/dnd/package-summary.html)
- [org.eclipse.swt.accessibility Package](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/accessibility/package-summary.html)
- [org.eclipse.swt.printing Package](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/printing/package-summary.html)
- [org.eclipse.swt.opengl Package](https://help.eclipse.org/latest/nftopic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/opengl/package-summary.html)
- [org.eclipse.swt.program Package](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/program/package-summary.html)

---

## Summary

**Total Table Stakes:** ~150 classes across 10 packages
**Currently Implemented:** ~89 widget/class files (~60%)
**TODOs to Address:** 173+ across 51 files

**Critical Gaps:**
1. Drag & Drop (complete package missing)
2. Accessibility (complete package missing)
3. Printing (complete package missing)
4. Custom widgets (StyledText incomplete, others missing)
5. Browser events/listeners (structure exists, events incomplete)
6. Advanced graphics (TextLayout, Transform, Path incomplete)

**Recommended Priority:**
1. Complete TODOs in existing widgets (173 items)
2. Implement Drag & Drop (high user value)
3. Finish Browser integration (modern apps need this)
4. Add Accessibility (compliance requirement)
5. Complete StyledText (Eclipse depends on it)
6. Add Printing support
7. Implement remaining custom widgets

**Differentiators to Add:**
1. Async/await Display events (high value, low effort)
2. Fluent API extension methods (high value, low effort)
3. Nullable reference types (medium value, medium effort)
4. Better .NET integration (high value, high effort)

**Do NOT Port:**
- OpenGL package (use OpenTK instead)
- Java-specific workarounds
- Deprecated APIs
- Eclipse-specific internals
