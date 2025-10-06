# Event System Research Report for SWTSharp

**Research Agent Report**
**Date:** 2025-10-06
**Context:** Complete event handling system implementation across all platforms

---

## Executive Summary

SWTSharp has established foundational event infrastructure with basic listener pattern support but requires comprehensive event handling implementation across all widget types and platforms. This research analyzes Java SWT's event model, current SWTSharp implementation, platform-specific considerations, and provides actionable implementation phases.

**Current State:** ~905 lines of event code covering 8 event types (basic foundation)
**Target State:** Complete event system with 20+ event types across 30+ widget types

---

## 1. Java SWT Event Model Analysis

### 1.1 Core Architecture

Java SWT uses a **typed listener pattern** with the following structure:

```java
// Base untyped listener (low-level)
public interface Listener {
    void handleEvent(Event event);
}

// Typed listeners (high-level, developer-friendly)
public interface SelectionListener {
    void widgetSelected(SelectionEvent e);
    void widgetDefaultSelected(SelectionEvent e);
}

// TypedListener bridges untyped and typed
class TypedListener implements Listener {
    EventListener eventListener;
    void handleEvent(Event e) {
        // Dispatch to typed methods based on event type
    }
}
```

**Key Design Principles:**
1. **Two-tier system:** Untyped (flexible) + Typed (convenient)
2. **Event class hierarchy:** Base Event class → Specialized events (MouseEvent, KeyEvent, etc.)
3. **Widget-centric:** Widgets manage their own listener lists
4. **Thread-safe dispatch:** Events fire on UI thread only
5. **Exception isolation:** Listener exceptions don't break event chain

### 1.2 Event Types Taxonomy

#### Core Input Events
- **MouseEvent**: Down, Up, DoubleClick, Move, Enter, Exit, Hover, Wheel
- **KeyEvent**: KeyDown, KeyUp
- **FocusEvent**: FocusIn, FocusOut
- **TraverseEvent**: Tab navigation handling

#### Widget Lifecycle Events
- **ControlEvent**: Move, Resize
- **DisposeEvent**: Widget disposal
- **PaintEvent**: Redraw notifications
- **ShowEvent/HideEvent**: Visibility changes

#### Widget-Specific Events
- **SelectionEvent**: Button click, list selection, menu item selection, etc.
- **ModifyEvent**: Text field changes
- **VerifyEvent**: Pre-validation of text input
- **ExpandEvent/CollapseEvent**: Tree item expansion
- **MenuEvent**: Menu show/hide, menu detection
- **ShellEvent**: Window activation, deactivation, close
- **ArmEvent**: Menu item highlighting
- **HelpEvent**: F1 key / help button

#### Advanced Events
- **DragEvent**: Drag and drop operations
- **DropEvent**: Drop target operations
- **MeasureEvent/EraseEvent/PaintItemEvent**: Owner-draw support
- **SegmentEvent**: BiDi text segment information
- **GestureEvent/TouchEvent**: Multi-touch and gestures

---

## 2. Current SWTSharp Implementation Analysis

### 2.1 Existing Event Infrastructure

**Files Analyzed:**
- `/src/SWTSharp/Widget.cs` (470 lines) - Core event management
- `/src/SWTSharp/Events/Event.cs` (112 lines) - Base event class
- `/src/SWTSharp/Events/TypedListener.cs` (154 lines) - Event routing
- `/src/SWTSharp/Events/*.cs` (19 event-related files)

**Current Capabilities:**

✅ **Implemented:**
- Base event storage (`Dictionary<int, List<IListener>>`)
- TypedListener routing for 10 event types
- Basic listener registration/unregistration
- Exception handling in event dispatch (SEC-005 compliant)
- Memory leak prevention (LEAK-001 pattern)
- .NET 8+ optimization with ArrayPool

✅ **Event Types Partially Implemented:**
1. Selection (Selection, DefaultSelection)
2. Mouse (MouseDown, MouseUp, MouseDoubleClick)
3. MouseMove
4. MouseTrack (Enter, Exit, Hover)
5. Key (KeyDown, KeyUp)
6. Focus (FocusIn, FocusOut)
7. Control (Move, Resize)
8. Dispose

**Event Constants in SWT.cs (Lines 158-194):**
```csharp
public const int KeyDown = 1;
public const int KeyUp = 2;
public const int MouseDown = 3;
public const int MouseUp = 4;
public const int MouseMove = 5;
public const int MouseEnter = 6;
public const int MouseExit = 7;
public const int MouseDoubleClick = 8;
public const int MouseHover = 9;
public const int Paint = 10;
public const int Move = 11;
public const int Dispose = 13;
public const int Selection = 14;
public const int DefaultSelection = 15;
public const int FocusIn = 16;
public const int FocusOut = 17;
// ... (18 more event types defined but not implemented)
```

### 2.2 Current Event Dispatch Pattern

**Widget.NotifyListeners Implementation:**
```csharp
public void NotifyListeners(int eventType, Event @event)
{
    if (_eventTable != null && _eventTable.TryGetValue(eventType, out var listeners))
    {
        @event.Type = eventType;
        @event.Widget = this;
        @event.Display ??= _display;

        // Copy listeners to avoid modification during iteration
        var listenersCopy = ArrayPool<IListener>.Shared.Rent(count);
        try {
            for (int i = 0; i < count; i++) {
                try {
                    listenersCopy[i].HandleEvent(@event);
                } catch (Exception ex) {
                    // Log error, don't break chain
                    NotifyApplicationError(ex, @event);
                }
            }
        } finally {
            ArrayPool<IListener>.Shared.Return(listenersCopy, clearArray: true);
        }
    }
}
```

**Strengths:**
- Thread-safe copy for iteration
- Exception isolation
- Memory efficient (.NET 8+)
- Proper cleanup on widget disposal

**Limitations:**
- No event bubbling/propagation
- No event priority system
- No async event support
- No event filtering/interception

### 2.3 Current Platform Event Connection

**Example: Button Click (Button.cs:87)**
```csharp
SWTSharp.Platform.PlatformFactory.Instance.ConnectButtonClick(
    Handle,
    () => OnClick(EventArgs.Empty)
);
```

**Analysis:**
- Simple delegate-based approach
- No generic event connection mechanism
- Each widget type has custom connection logic
- No support for multiple platform events → single SWT event

---

## 3. Platform Event Model Analysis

### 3.1 Windows (Win32) Event System

**Architecture:** Message-based event system

**Key Mechanisms:**
1. **Window Procedure (WndProc):** Central message dispatcher
2. **Message Queue:** GetMessage/PeekMessage/DispatchMessage loop
3. **Message Types:** WM_* constants (WM_COMMAND, WM_PAINT, WM_LBUTTONDOWN, etc.)
4. **Subclassing:** SetWindowLongPtr to intercept messages

**Common Event Messages:**
```cpp
// Mouse events
WM_LBUTTONDOWN    = 0x0201
WM_LBUTTONUP      = 0x0202
WM_MOUSEMOVE      = 0x0200
WM_MOUSEWHEEL     = 0x020A

// Keyboard events
WM_KEYDOWN        = 0x0100
WM_KEYUP          = 0x0101
WM_CHAR           = 0x0102

// Window events
WM_SIZE           = 0x0005
WM_MOVE           = 0x0003
WM_PAINT          = 0x000F
WM_CLOSE          = 0x0010
WM_DESTROY        = 0x0002

// Control events
WM_COMMAND        = 0x0111  // Button clicks, menu selections
WM_NOTIFY         = 0x004E  // Complex control notifications
```

**Threading Model:**
- Each window has an owning thread
- Messages processed on creating thread
- Cross-thread messaging via SendMessage/PostMessage

**Current Implementation Status:**
- Basic message pump exists in Win32Platform.cs
- Simple CreateWindow/DestroyWindow
- ConnectButtonClick uses callback storage
- **MISSING:** WndProc subclassing for event routing

**Implementation Challenges:**
1. **Subclassing complexity:** Need to preserve original WndProc
2. **LPARAM/WPARAM parsing:** Extract event data from message parameters
3. **Memory management:** Marshal.GetFunctionPointerForDelegate lifetime
4. **Notification codes:** Different controls use different notification mechanisms

### 3.2 macOS (Cocoa/AppKit) Event System

**Architecture:** Target-Action + Delegate + Notification patterns

**Key Mechanisms:**
1. **Target-Action:** Button → target → action selector
2. **Delegates:** NSWindowDelegate, NSTextFieldDelegate, etc.
3. **Notification Center:** NSNotificationCenter for broadcast events
4. **Responder Chain:** Event bubbling through view hierarchy

**Event Handling Patterns:**
```objc
// Target-Action (for simple controls)
[button setTarget:self];
[button setAction:@selector(buttonClicked:)];

// Delegate (for complex widgets)
[textField setDelegate:self];
// Implement NSTextFieldDelegate methods

// Notifications (for broadcast events)
[[NSNotificationCenter defaultCenter]
    addObserver:self
    selector:@selector(windowDidResize:)
    name:NSWindowDidResizeNotification
    object:window];

// Responder Chain (for keyboard/mouse)
- (void)mouseDown:(NSEvent *)event {
    // Handle mouse down
    [super mouseDown:event]; // Pass to next responder
}
```

**Current Implementation Status:**
- Objective-C runtime bridging in place
- Basic NSWindow/NSApplication setup
- **MISSING:** Target-Action registration
- **MISSING:** Delegate protocol implementations
- **MISSING:** Notification observer setup

**Implementation Challenges:**
1. **Objective-C runtime complexity:** Manual selector registration, message sending
2. **Callback marshaling:** C# delegates → Objective-C blocks/selectors
3. **Memory management:** Objective-C reference counting (retain/release)
4. **Delegate protocols:** Cannot implement Objective-C protocols directly in C#
5. **Block callbacks:** Complex trampolines required for NSNotification blocks

### 3.3 Linux (GTK) Event System

**Architecture:** Signal/slot mechanism (GObject signals)

**Key Mechanisms:**
1. **Signal connections:** g_signal_connect()
2. **Event handlers:** Callback functions with specific signatures
3. **Event masks:** Enable/disable event types
4. **Event propagation:** TRUE/FALSE return controls bubble

**Common GTK Signals:**
```c
// Mouse events
"button-press-event"
"button-release-event"
"motion-notify-event"
"scroll-event"

// Keyboard events
"key-press-event"
"key-release-event"

// Widget events
"focus-in-event"
"focus-out-event"
"configure-event"  // Resize/move
"destroy"

// Complex widget signals
"clicked"           // GtkButton
"toggled"           // GtkCheckButton
"changed"           // GtkEntry, GtkComboBox
"row-activated"     // GtkTreeView
```

**Signal Connection Pattern:**
```c
// Connect signal
gulong handler_id = g_signal_connect(
    widget,
    "button-press-event",
    G_CALLBACK(on_button_press),
    user_data
);

// Callback signature
gboolean on_button_press(GtkWidget *widget, GdkEventButton *event, gpointer data) {
    // Handle event
    return FALSE; // FALSE = propagate, TRUE = consume
}
```

**Current Implementation Status:**
- GTK initialization in LinuxPlatform.cs
- Basic widget creation
- **MISSING:** g_signal_connect implementation
- **MISSING:** Event mask configuration
- **MISSING:** GCallback trampolines

**Implementation Challenges:**
1. **GCallback marshaling:** C# delegates → C function pointers
2. **Event struct parsing:** GdkEventButton, GdkEventKey, etc.
3. **Signal lifecycle:** Disconnect on widget disposal
4. **Thread safety:** GTK not thread-safe, requires gdk_threads_enter/leave
5. **Event masks:** Must enable events explicitly (gtk_widget_add_events)

---

## 4. Missing Event Types Requiring Implementation

### 4.1 High Priority (Core Functionality)

| Event Type | Use Cases | Complexity | Platform Considerations |
|------------|-----------|------------|------------------------|
| **MouseWheel** | Scrolling, zoom | Medium | Win32: WM_MOUSEWHEEL, macOS: scrollWheel:, GTK: scroll-event |
| **Modify** | Text field changes | Medium | Win32: EN_CHANGE, macOS: textDidChange:, GTK: changed |
| **Verify** | Input validation | High | Pre-event, requires cancellation support |
| **Paint** | Custom drawing | High | Win32: WM_PAINT, macOS: drawRect:, GTK: draw |
| **Activate/Deactivate** | Window focus | Medium | Shell-specific events |
| **Show/Hide** | Visibility tracking | Low | Widget visibility changes |
| **Iconify/Deiconify** | Window minimize | Low | Shell-specific events |

### 4.2 Medium Priority (Enhanced Functionality)

| Event Type | Use Cases | Complexity | Dependencies |
|------------|-----------|------------|--------------|
| **Expand/Collapse** | Tree navigation | Medium | Tree widget |
| **Arm** | Menu highlighting | Low | Menu system |
| **MenuDetect** | Context menu trigger | Medium | Right-click detection |
| **Help** | F1 key handling | Low | Keyboard events |
| **Traverse** | Tab navigation | Medium | Focus system |
| **DragDetect** | Drag initiation | Medium | Mouse tracking |
| **Settings** | System theme changes | Low | Platform notifications |

### 4.3 Low Priority (Advanced Features)

| Event Type | Use Cases | Complexity | Notes |
|------------|-----------|------------|-------|
| **MeasureItem/EraseItem/PaintItem** | Owner-draw | Very High | Custom rendering |
| **Gesture** | Multi-touch | High | Platform-specific |
| **Touch** | Touch screen | High | Windows 8+, iOS |
| **Segment** | BiDi text | Medium | Internationalization |

---

## 5. Event System Architecture Design

### 5.1 Proposed Event Class Hierarchy

```
Event (base)
├── TypedEvent (abstract base for all typed events)
│   ├── MouseEvent
│   │   └── MouseWheelEvent (extends with delta/scrollAmount)
│   ├── KeyEvent
│   ├── FocusEvent
│   ├── SelectionEvent
│   ├── ModifyEvent
│   ├── VerifyEvent (cancellable)
│   ├── ControlEvent
│   │   ├── MoveEvent
│   │   └── ResizeEvent
│   ├── PaintEvent (includes GC, clip region)
│   ├── ShellEvent
│   │   ├── ActivateEvent
│   │   ├── DeactivateEvent
│   │   ├── CloseEvent
│   │   └── IconifyEvent
│   ├── TreeEvent
│   │   ├── ExpandEvent
│   │   └── CollapseEvent
│   ├── MenuEvent
│   │   ├── ArmEvent
│   │   └── MenuDetectEvent
│   ├── TraverseEvent
│   ├── DragEvent
│   ├── DropEvent
│   ├── HelpEvent
│   └── SettingsEvent
```

### 5.2 Enhanced Listener Interfaces

**Proposal: Add adapter base classes for all listeners**

```csharp
// Existing pattern
public interface IMouseListener {
    void MouseDown(MouseEvent e);
    void MouseUp(MouseEvent e);
    void MouseDoubleClick(MouseEvent e);
}

// Add adapter for convenience
public abstract class MouseAdapter : IMouseListener {
    public virtual void MouseDown(MouseEvent e) { }
    public virtual void MouseUp(MouseEvent e) { }
    public virtual void MouseDoubleClick(MouseEvent e) { }
}

// New listeners needed
public interface IMouseWheelListener {
    void MouseScrolled(MouseWheelEvent e);
}

public interface IModifyListener {
    void ModifyText(ModifyEvent e);
}

public interface IVerifyListener {
    void VerifyText(VerifyEvent e); // Can set e.Doit = false
}

public interface IPaintListener {
    void PaintControl(PaintEvent e);
}

public interface IShellListener {
    void ShellActivated(ShellEvent e);
    void ShellDeactivated(ShellEvent e);
    void ShellClosed(ShellEvent e);
    void ShellIconified(ShellEvent e);
    void ShellDeiconified(ShellEvent e);
}

public interface ITreeListener {
    void TreeExpanded(TreeEvent e);
    void TreeCollapsed(TreeEvent e);
}

public interface IMenuListener {
    void MenuShown(MenuEvent e);
    void MenuHidden(MenuEvent e);
}

public interface IArmListener {
    void WidgetArmed(ArmEvent e);
}

public interface ITraverseListener {
    void KeyTraversed(TraverseEvent e);
}

public interface IDragDetectListener {
    void DragDetected(DragEvent e);
}

public interface IHelpListener {
    void HelpRequested(HelpEvent e);
}
```

### 5.3 Platform Event Connection Architecture

**Proposed: Generic Event Hook System**

```csharp
public interface IPlatform {
    // Generic event connection
    void ConnectEvent(IntPtr handle, int eventType, EventHook hook);
    void DisconnectEvent(IntPtr handle, int eventType, EventHook hook);
}

public class EventHook {
    public int EventType { get; set; }
    public Action<Event> Handler { get; set; }
    public IntPtr PlatformData { get; set; } // Platform-specific token
}

// Platform-specific implementation
public partial class Win32Platform : IPlatform {
    // WndProc subclassing
    private Dictionary<IntPtr, WndProcHook> _wndProcHooks;

    public void ConnectEvent(IntPtr handle, int eventType, EventHook hook) {
        if (!_wndProcHooks.TryGetValue(handle, out var wndProc)) {
            wndProc = SubclassWindow(handle);
            _wndProcHooks[handle] = wndProc;
        }
        wndProc.AddEventHook(eventType, hook);
    }
}

public partial class MacOSPlatform : IPlatform {
    // Target-Action + Delegate registration
    private Dictionary<IntPtr, MacEventBridge> _eventBridges;

    public void ConnectEvent(IntPtr handle, int eventType, EventHook hook) {
        var bridge = GetOrCreateBridge(handle);
        bridge.RegisterEvent(eventType, hook);
    }
}

public partial class LinuxPlatform : IPlatform {
    // GSignal connection
    private Dictionary<IntPtr, List<ulong>> _signalHandlers;

    public void ConnectEvent(IntPtr handle, int eventType, EventHook hook) {
        var signalName = MapEventTypeToSignal(eventType);
        var handlerId = g_signal_connect(handle, signalName,
            Marshal.GetFunctionPointerForDelegate(CreateCallback(hook)));

        if (!_signalHandlers.TryGetValue(handle, out var handlers)) {
            handlers = new List<ulong>();
            _signalHandlers[handle] = handlers;
        }
        handlers.Add(handlerId);
    }
}
```

### 5.4 Event Dispatch Enhancements

**Proposed Features:**

1. **Event Bubbling (Optional)**
```csharp
public void NotifyListeners(int eventType, Event @event, bool bubbleToParent = false)
{
    // Current notification logic...

    if (bubbleToParent && !@event.Consumed && this is Control control) {
        control.Parent?.NotifyListeners(eventType, @event, bubbleToParent);
    }
}

// Add to Event class
public bool Consumed { get; set; }
```

2. **Event Filtering/Interception**
```csharp
public interface IEventFilter {
    bool FilterEvent(Event e); // Return true to consume
}

public class Display {
    private List<IEventFilter> _eventFilters = new();

    public void AddEventFilter(IEventFilter filter) {
        _eventFilters.Add(filter);
    }
}
```

3. **Async Event Support (Low Priority)**
```csharp
public interface IAsyncListener {
    Task HandleEventAsync(Event e);
}

// Fire-and-forget async dispatch
public async void NotifyListenersAsync(int eventType, Event @event) {
    var tasks = listeners.Select(l => l.HandleEventAsync(@event));
    await Task.WhenAll(tasks);
}
```

---

## 6. Implementation Phases and Complexity Assessment

### Phase 1: Core Platform Event Connection (3-4 weeks, HIGH complexity)

**Goal:** Establish generic event hook system for all three platforms

**Tasks:**
1. **Win32 WndProc Subclassing**
   - Implement SetWindowLongPtr-based subclassing
   - Create WndProcHook class to manage event routing
   - Handle LPARAM/WPARAM parameter extraction
   - Ensure proper cleanup on widget disposal
   - **Complexity:** HIGH (pointer management, Win32 API intricacies)
   - **Risk:** Memory leaks, crashes if WndProc cleanup fails

2. **macOS Target-Action Bridge**
   - Create Objective-C proxy class (C# → ObjC trampoline)
   - Implement block-based callbacks for notifications
   - Handle delegate protocol implementations via categories
   - Manage retain/release cycles
   - **Complexity:** VERY HIGH (ObjC runtime, memory management)
   - **Risk:** Crashes from improper retain/release, selector name collisions

3. **GTK Signal Connection**
   - Implement g_signal_connect wrapper
   - Create GCallback trampolines (C# delegate → C function pointer)
   - Parse GdkEvent structures for event data
   - Handle signal disconnection on widget disposal
   - **Complexity:** HIGH (GObject type system, callback lifetime)
   - **Risk:** Memory leaks, threading issues (GTK not thread-safe)

**Deliverables:**
- `PlatformEventHook` base class
- Platform-specific implementations (Win32EventHook, MacOSEventHook, LinuxEventHook)
- Unit tests for event connection/disconnection
- Memory leak tests

**Blockers to Address:**
- **Win32:** Marshal.GetFunctionPointerForDelegate lifetime management
- **macOS:** Objective-C block creation from C#
- **GTK:** Cross-thread invocation (gdk_threads_enter/leave)

### Phase 2: Mouse and Keyboard Events (2 weeks, MEDIUM complexity)

**Goal:** Complete mouse and keyboard event support across all widgets

**Tasks:**
1. **Implement Missing Mouse Events**
   - MouseWheel (Win32: WM_MOUSEWHEEL, macOS: scrollWheel:, GTK: scroll-event)
   - Mouse tracking improvements (hover, enter/exit consistency)
   - Mouse button state tracking (handle multiple buttons)

2. **Enhance Keyboard Events**
   - Key modifiers (Shift, Ctrl, Alt, Command)
   - Dead key handling (international keyboards)
   - IME support (East Asian input methods)

3. **Create Event Classes**
   - `MouseWheelEvent` (delta, scrollAmount, orientation)
   - Enhanced `KeyEvent` (modifiers, key location, IME state)

**Deliverables:**
- Complete mouse event support
- Complete keyboard event support
- Cross-platform key code mapping
- Interactive test application

**Complexity Factors:**
- Key code translation across platforms
- IME event handling (low priority, can defer)
- Touch event vs. mouse event differentiation (Windows 8+)

### Phase 3: Widget Lifecycle and Control Events (1.5 weeks, LOW-MEDIUM complexity)

**Goal:** Implement paint, resize, move, visibility events

**Tasks:**
1. **Paint Events**
   - Win32: WM_PAINT message handling
   - macOS: drawRect: override
   - GTK: draw signal
   - Create `PaintEvent` with GC context

2. **Control Events**
   - Win32: WM_SIZE, WM_MOVE
   - macOS: viewDidResize, viewDidMove
   - GTK: configure-event
   - Enhanced `ControlEvent` with old/new bounds

3. **Visibility Events**
   - Win32: WM_SHOWWINDOW
   - macOS: viewWillAppear/viewDidDisappear
   - GTK: show/hide signals

**Deliverables:**
- `PaintEvent`, `MoveEvent`, `ResizeEvent` classes
- Canvas widget with paint support
- Layout system integration (resize triggers layout)

### Phase 4: Widget-Specific Events (2-3 weeks, MEDIUM complexity)

**Goal:** Implement events for complex widgets (Tree, Table, Menu, Shell)

**Tasks:**
1. **Shell Events**
   - Activate/Deactivate (window focus)
   - Iconify/Deiconify (minimize/restore)
   - Close event (cancellable)
   - Win32: WM_ACTIVATE, WM_SIZE (SIZE_MINIMIZED), WM_CLOSE
   - macOS: NSWindowDelegate methods
   - GTK: focus-in/out-event, window-state-event

2. **Tree Events**
   - Expand/Collapse
   - Win32: TVN_ITEMEXPANDING/TVN_ITEMEXPANDED
   - macOS: NSOutlineViewDelegate methods
   - GTK: row-expanded/row-collapsed

3. **Menu Events**
   - Arm (menu item highlighted)
   - MenuDetect (context menu trigger)
   - Win32: WM_MENUSELECT, WM_CONTEXTMENU
   - macOS: NSMenuDelegate methods
   - GTK: select/deselect signals

4. **Text Widget Events**
   - Modify (text changed)
   - Verify (pre-change validation, cancellable)
   - Win32: EN_CHANGE, EN_UPDATE
   - macOS: textDidChange:, textShouldChange:
   - GTK: changed, insert-text/delete-text

**Deliverables:**
- `ShellEvent`, `TreeEvent`, `MenuEvent`, `ModifyEvent`, `VerifyEvent`
- ShellListener, TreeListener, MenuListener, ModifyListener, VerifyListener
- Adapter classes for all new listeners

### Phase 5: Advanced Events and Polish (2 weeks, LOW-MEDIUM complexity)

**Goal:** Implement remaining events and optimize performance

**Tasks:**
1. **Navigation Events**
   - Traverse (tab key navigation)
   - DragDetect (drag gesture start)
   - Help (F1 key)

2. **System Events**
   - Settings (theme changes, DPI changes)
   - Win32: WM_SETTINGCHANGE, WM_DPICHANGED
   - macOS: NSNotificationCenter system notifications
   - GTK: style-updated signal

3. **Performance Optimization**
   - Event coalescing (multiple mouse moves → single event)
   - Lazy event hook creation (only connect when listeners exist)
   - Batch event dispatch for high-frequency events

4. **Testing and Documentation**
   - Comprehensive event test suite
   - Event system documentation
   - Migration guide from .NET events to SWT events

**Deliverables:**
- Complete event system
- Performance benchmarks
- Full API documentation
- Sample applications demonstrating all event types

---

## 7. Key Blockers and Mitigation Strategies

### 7.1 Threading Model Complications

**Problem:** Each platform has different threading requirements
- **Win32:** Message pump on creating thread, SendMessage for cross-thread
- **macOS:** All UI on main thread, performSelectorOnMainThread for cross-thread
- **GTK:** Not thread-safe, requires gdk_threads_enter/leave

**Mitigation:**
1. Enforce Display thread checking (already in place)
2. Provide `Display.AsyncExec()` and `Display.SyncExec()` for cross-thread invocation
3. Document threading requirements clearly
4. Add runtime thread validation (throw on wrong thread)

**Implementation:**
```csharp
public class Display {
    public void AsyncExec(Action action) {
        if (IsValidThread()) {
            action();
        } else {
            PostToUIThread(action);
        }
    }

    public void SyncExec(Action action) {
        if (IsValidThread()) {
            action();
        } else {
            SendToUIThread(action);
        }
    }
}
```

### 7.2 Platform Event Model Differences

**Problem:** Platform event granularity doesn't match 1:1 with SWT events
- Win32: WM_COMMAND for all button clicks, need to parse notification code
- macOS: Separate selectors for each event type
- GTK: Different signals for same logical event

**Mitigation:**
1. Create platform-specific event mapping tables
2. Implement event normalization layer
3. Use EventHook abstraction to hide platform differences
4. Provide platform-specific event constants for advanced users

**Mapping Table Example:**
```csharp
private static readonly Dictionary<int, PlatformEventMapping> EventMappings = new() {
    [SWT.MouseDown] = new PlatformEventMapping {
        Win32 = (WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN),
        MacOS = ("mouseDown:", "rightMouseDown:", "otherMouseDown:"),
        GTK = ("button-press-event",)
    },
    // ... more mappings
};
```

### 7.3 Callback Marshaling Complexity

**Problem:** C# delegates → native callbacks requires careful lifetime management

**Win32 Issues:**
- `Marshal.GetFunctionPointerForDelegate()` requires keeping delegate alive
- GC can collect delegate while native code still uses it
- Need to store delegate references in instance fields

**macOS Issues:**
- Objective-C blocks are complex to create from C#
- Need to create Objective-C trampoline objects
- Retain/release cycles must be managed manually

**GTK Issues:**
- GCallback trampolines need to match exact signature
- GObject signal marshaling requires proper argument parsing
- Cross-thread signals require main loop invocation

**Mitigation:**
1. **Delegate Storage:** Keep strong references to all callback delegates
```csharp
private readonly List<Delegate> _callbackRefs = new();

private IntPtr CreateCallback(Action<Event> handler) {
    var callback = new NativeCallback(handler);
    _callbackRefs.Add(callback); // Prevent GC
    return Marshal.GetFunctionPointerForDelegate(callback);
}
```

2. **Weak Reference Cleanup:** Remove callbacks on widget disposal
3. **Platform-Specific Trampoline Classes:**
   - `Win32WndProcTrampoline`
   - `MacOSActionTrampoline`
   - `GTKCallbackTrampoline`

### 7.4 Memory Management for Event Handlers

**Problem:** Event handler leaks can accumulate over widget lifetime

**Current Protection (LEAK-001):**
```csharp
protected virtual void ReleaseWidget() {
    // Clear event handlers to prevent memory leaks
    if (_eventTable != null) {
        foreach (var eventType in _eventTable.Keys.ToList()) {
            if (_eventTable.TryGetValue(eventType, out var listeners)) {
                listeners.Clear();
            }
        }
        _eventTable.Clear();
        _eventTable = null;
    }
}
```

**Additional Mitigations:**
1. **Weak Event Pattern (optional, for long-lived listeners):**
```csharp
public class WeakEventListener : IListener {
    private WeakReference<IListener> _weakRef;

    public void HandleEvent(Event e) {
        if (_weakRef.TryGetTarget(out var listener)) {
            listener.HandleEvent(e);
        } else {
            // Self-unregister
        }
    }
}
```

2. **Event Handler Lifetime Tracking:**
```csharp
#if DEBUG
private static int _totalListeners = 0;
public void AddListener(int eventType, IListener listener) {
    // ... existing code ...
    Interlocked.Increment(ref _totalListeners);
    Debug.WriteLine($"Total listeners: {_totalListeners}");
}
#endif
```

3. **Automatic Cleanup on Parent Disposal:** Already implemented via Composite.ReleaseWidget()

### 7.5 Performance Overhead of Event Dispatch

**Problem:** High-frequency events (MouseMove, Paint) can cause performance issues

**Current Optimization:**
- ArrayPool for listener copying (.NET 8+)
- Exception isolation prevents cascade failures

**Additional Mitigations:**

1. **Event Coalescing:**
```csharp
private Event? _coalescedMouseMove;
private Timer? _coalesceTimer;

public void QueueMouseMove(Event e) {
    _coalescedMouseMove = e; // Replace previous
    _coalesceTimer?.Start();  // Fire after 16ms (60fps)
}
```

2. **Lazy Event Hook Creation:**
```csharp
public void AddListener(int eventType, IListener listener) {
    // ... add to _eventTable ...

    if (listeners.Count == 1) {
        // First listener: connect platform event
        ConnectPlatformEvent(eventType);
    }
}

public void RemoveListener(int eventType, IListener listener) {
    // ... remove from _eventTable ...

    if (listeners.Count == 0) {
        // Last listener: disconnect platform event
        DisconnectPlatformEvent(eventType);
    }
}
```

3. **Event Priority System (future):**
```csharp
public void AddListener(int eventType, IListener listener, int priority = 0) {
    // Higher priority listeners fire first
    listeners.Insert(GetInsertIndex(priority), listener);
}
```

---

## 8. Event Types Priority Matrix

| Priority | Event Type | Widgets Affected | Estimated Effort | Dependencies |
|----------|-----------|------------------|------------------|--------------|
| **P0** | MouseWheel | All Controls | 3 days | Phase 1 (platform hooks) |
| **P0** | Paint | Canvas, custom widgets | 5 days | Graphics system |
| **P0** | Modify | Text, Combo | 2 days | Phase 1 |
| **P0** | Verify | Text | 3 days | Modify event |
| **P1** | Activate/Deactivate | Shell | 2 days | Phase 1 |
| **P1** | Iconify/Deiconify | Shell | 1 day | Shell events |
| **P1** | Close | Shell | 1 day | Shell events |
| **P1** | Show/Hide | All Controls | 1 day | Phase 1 |
| **P1** | Expand/Collapse | Tree | 2 days | Tree widget |
| **P2** | Arm | Menu, MenuItem | 1 day | Menu system |
| **P2** | MenuDetect | All Controls | 2 days | Mouse events |
| **P2** | Traverse | All Controls | 3 days | Focus system |
| **P2** | DragDetect | All Controls | 2 days | Mouse tracking |
| **P2** | Help | All Controls | 1 day | Key events |
| **P3** | Settings | Display | 2 days | Platform notifications |
| **P3** | MeasureItem/EraseItem/PaintItem | List, Table, Tree | 5 days | Owner-draw support |
| **P3** | Gesture | All Controls | 5 days | Platform-specific |
| **P3** | Touch | All Controls | 5 days | Windows 8+, iOS |
| **P3** | Segment | Text widgets | 3 days | BiDi support |

**Total Estimated Effort:** 10-12 weeks (with 1 developer)

---

## 9. Testing Strategy

### 9.1 Unit Tests

**Event Infrastructure Tests:**
```csharp
[TestClass]
public class EventSystemTests {
    [TestMethod]
    public void TestEventDispatchOrder() { }

    [TestMethod]
    public void TestEventListenerAddRemove() { }

    [TestMethod]
    public void TestEventExceptionIsolation() { }

    [TestMethod]
    public void TestEventMemoryLeaks() { }

    [TestMethod]
    public void TestEventCoalescing() { }
}
```

**Platform-Specific Tests:**
```csharp
[TestClass]
public class Win32EventTests {
    [TestMethod]
    public void TestWndProcSubclassing() { }

    [TestMethod]
    public void TestMessageParsing() { }
}

[TestClass]
public class MacOSEventTests {
    [TestMethod]
    public void TestTargetActionBridge() { }

    [TestMethod]
    public void TestDelegateRetainCycle() { }
}

[TestClass]
public class GTKEventTests {
    [TestMethod]
    public void TestGSignalConnection() { }

    [TestMethod]
    public void TestGCallbackTrampoline() { }
}
```

### 9.2 Integration Tests

**Event Flow Tests:**
- User clicks button → Selection event fires
- User types in text field → Modify events fire
- User resizes window → Resize events fire on all children
- User presses F1 → Help event fires

**Cross-Widget Tests:**
- Tab key navigation triggers Traverse events
- Context menu detection (right-click → MenuDetect)
- Drag detection (mouse down + move → DragDetect)

### 9.3 Manual Testing

**Interactive Test Application:**
```csharp
public class EventTestShell : Shell {
    private ListBox _eventLog;

    public EventTestShell() {
        // Create widgets with all event types
        // Log all events to _eventLog for inspection
    }
}
```

**Test Checklist:**
- [ ] Mouse events (all buttons, wheel, hover)
- [ ] Keyboard events (all keys, modifiers, IME)
- [ ] Focus events (tab, click, programmatic)
- [ ] Window events (minimize, restore, close)
- [ ] Resize events (drag, maximize, programmatic)
- [ ] Paint events (partial, full, invalidate)
- [ ] Menu events (show, arm, select)
- [ ] Tree events (expand, collapse, selection)
- [ ] Text events (type, modify, verify)

---

## 10. Documentation Requirements

### 10.1 API Documentation

**For Each Event Type:**
- Event class documentation (fields, properties)
- Listener interface documentation
- Adapter class documentation
- Usage examples
- Platform-specific notes

**Example:**
```csharp
/// <summary>
/// Instances of this class are sent whenever a mouse button is pressed or released.
/// </summary>
/// <remarks>
/// <para>
/// This event is sent for all mouse buttons (left, right, middle, and extended buttons 4-5).
/// The <see cref="Button"/> field indicates which button was pressed or released.
/// </para>
/// <para>
/// <b>Platform Notes:</b>
/// <list type="bullet">
/// <item><description>Windows: Derived from WM_LBUTTONDOWN, WM_RBUTTONDOWN, etc.</description></item>
/// <item><description>macOS: Derived from mouseDown:, rightMouseDown:, otherMouseDown:</description></item>
/// <item><description>Linux: Derived from button-press-event signal</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// button.AddMouseListener(new MouseAdapter() {
///     public override void MouseDown(MouseEvent e) {
///         if (e.Button == 1) {
///             Console.WriteLine("Left button pressed");
///         }
///     }
/// });
/// </code>
/// </example>
public class MouseEvent : TypedEvent { ... }
```

### 10.2 Developer Guide

**Topics to Cover:**
1. Event System Overview
2. Adding Event Listeners (typed vs. untyped)
3. Event Adapter Pattern
4. Event Lifecycle and Disposal
5. Threading Considerations
6. Performance Best Practices
7. Platform-Specific Behavior
8. Migrating from .NET Events
9. Custom Event Types (advanced)

### 10.3 Migration Guide

**For users coming from WinForms/WPF:**
```markdown
## Event System Comparison

| .NET Events | SWT Events |
|------------|-----------|
| `button.Click += Handler;` | `button.AddSelectionListener(listener);` |
| `EventHandler<MouseEventArgs>` | `IMouseListener` or `MouseAdapter` |
| `e.Handled = true;` | `e.Doit = false;` (for cancellable events) |
| No cleanup required | Must remove listeners before disposal (optional but recommended) |
```

---

## 11. Summary and Recommendations

### 11.1 Current State Assessment

**Strengths:**
- ✅ Solid foundation with TypedListener pattern
- ✅ Memory-safe event dispatch with exception isolation
- ✅ .NET 8+ optimizations (ArrayPool)
- ✅ Basic event types (Selection, Mouse, Key, Focus) partially implemented

**Weaknesses:**
- ❌ No platform event connection mechanism (critical blocker)
- ❌ Missing 70% of event types (Paint, Modify, Shell events, etc.)
- ❌ No event hook cleanup on widget disposal
- ❌ Platform implementations incomplete (Win32, macOS, GTK)

### 11.2 Recommended Implementation Order

**Phase 1 (Critical Path):**
1. Win32 WndProc subclassing
2. macOS Target-Action bridge
3. GTK g_signal_connect wrapper
4. Generic EventHook abstraction

**Phase 2 (High Value):**
1. MouseWheel event
2. Paint event (unlocks custom drawing)
3. Modify/Verify events (unlocks text validation)
4. Shell events (window lifecycle)

**Phase 3 (Core Completeness):**
1. Tree events
2. Menu events
3. Remaining widget-specific events

**Phase 4 (Polish):**
1. Advanced events (Gesture, Touch)
2. Performance optimization
3. Comprehensive testing
4. Documentation

### 11.3 Risk Mitigation

**Highest Risks:**
1. **macOS Objective-C complexity:** Mitigate with incremental development, extensive testing
2. **Memory leaks from callbacks:** Mitigate with careful lifecycle management, automated leak tests
3. **Threading issues:** Mitigate with strict thread checking, clear documentation
4. **Performance degradation:** Mitigate with lazy hook creation, event coalescing

**Contingency Plans:**
- If macOS proves too complex: Consider using MonoMac/Xamarin.Mac abstractions
- If GTK threading issues arise: Document limitations, provide workarounds
- If performance issues occur: Implement event batching, throttling

### 11.4 Success Criteria

**Phase 1 Complete:**
- [ ] All three platforms can fire events from native controls
- [ ] Event hooks properly cleaned up on widget disposal
- [ ] No memory leaks in event system
- [ ] Thread-safe event dispatch

**Full Implementation Complete:**
- [ ] All 20+ event types implemented
- [ ] All widgets support appropriate events
- [ ] 90%+ test coverage
- [ ] Performance benchmarks meet targets (< 1ms overhead per event)
- [ ] Complete API documentation
- [ ] Zero known memory leaks

---

## 12. References and Resources

### 12.1 Java SWT Documentation
- Eclipse SWT Javadoc: https://help.eclipse.org/latest/index.jsp?topic=%2Forg.eclipse.platform.doc.isv%2Freference%2Fapi%2Forg%2Feclipse%2Fswt%2Fpackage-summary.html
- SWT Event Handling Guide: https://www.eclipse.org/articles/Article-SWT-Events/SWT-Events.html
- SWT Developer Guide: https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/guide/swt.htm

### 12.2 Platform Event Documentation
- **Win32:** https://docs.microsoft.com/en-us/windows/win32/winmsg/window-messages
- **macOS Cocoa:** https://developer.apple.com/documentation/appkit/nsresponder
- **GTK:** https://docs.gtk.org/gtk4/signal.Widget.html

### 12.3 Current SWTSharp Codebase
- Widget.cs: /src/SWTSharp/Widget.cs
- Events: /src/SWTSharp/Events/*.cs
- Platforms: /src/SWTSharp/Platform/*.cs

---

## Appendix A: Platform Event Mapping Table

| SWT Event | Win32 Message | macOS Selector | GTK Signal |
|-----------|--------------|----------------|------------|
| MouseDown | WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN | mouseDown:, rightMouseDown:, otherMouseDown: | button-press-event |
| MouseUp | WM_LBUTTONUP, WM_RBUTTONUP, WM_MBUTTONUP | mouseUp:, rightMouseUp:, otherMouseUp: | button-release-event |
| MouseMove | WM_MOUSEMOVE | mouseMoved: | motion-notify-event |
| MouseWheel | WM_MOUSEWHEEL | scrollWheel: | scroll-event |
| MouseEnter | WM_MOUSELEAVE (TrackMouseEvent) | mouseEntered: | enter-notify-event |
| MouseExit | WM_MOUSELEAVE | mouseExited: | leave-notify-event |
| KeyDown | WM_KEYDOWN | keyDown: | key-press-event |
| KeyUp | WM_KEYUP | keyUp: | key-release-event |
| FocusIn | WM_SETFOCUS | becomeFirstResponder | focus-in-event |
| FocusOut | WM_KILLFOCUS | resignFirstResponder | focus-out-event |
| Paint | WM_PAINT | drawRect: | draw |
| Resize | WM_SIZE | viewDidResize | configure-event |
| Move | WM_MOVE | viewDidMove | configure-event |
| Show | WM_SHOWWINDOW | viewWillAppear | show |
| Hide | WM_SHOWWINDOW | viewDidDisappear | hide |
| Activate | WM_ACTIVATE | windowDidBecomeKey | focus-in-event |
| Deactivate | WM_ACTIVATE | windowDidResignKey | focus-out-event |
| Close | WM_CLOSE | windowShouldClose: | delete-event |
| Selection | WM_COMMAND (BN_CLICKED, etc.) | action selector | clicked, toggled, changed |
| Modify | WM_COMMAND (EN_CHANGE) | textDidChange: | changed |

---

## Appendix B: Existing Event Code Inventory

**Event Classes (9 files, ~200 lines):**
- Event.cs (112 lines)
- MouseEvent.cs (51 lines)
- KeyEvent.cs (48 lines)
- SelectionEvent.cs (55 lines)
- ControlEvent.cs (21 lines)
- FocusEvent.cs (21 lines)
- DisposeEvent.cs (14 lines)
- SWTEventArgs.cs (56 lines - legacy, may be removed)

**Listener Interfaces (11 files, ~150 lines):**
- IListener.cs (21 lines)
- IMouseListener.cs (52 lines)
- IMouseMoveListener.cs (13 lines)
- IMouseTrackListener.cs (42 lines)
- IKeyListener.cs (32 lines)
- ISelectionListener.cs (21 lines)
- IFocusListener.cs (30 lines)
- IControlListener.cs (34 lines)
- IDisposeListener.cs (13 lines)

**Adapter Classes (2 files, ~60 lines):**
- MouseAdapter.cs (implied, not yet seen)
- SelectionAdapter.cs (34 lines)

**Infrastructure (2 files, ~550 lines):**
- TypedListener.cs (154 lines)
- Widget.cs event methods (470 lines total, ~200 lines event-related)

**Total:** ~905 lines of event system code

**Missing:** ~15 event types, ~20 listener interfaces, platform event connection infrastructure

---

**End of Research Report**
