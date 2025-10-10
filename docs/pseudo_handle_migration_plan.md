# Pseudo-Handle Migration Plan: Moving Toward Java SWT Architecture

## Executive Summary

**Current State:** SWTSharp uses pseudo-handles (fabricated IntPtr values like 0x40000000 + id) for widgets that don't map 1:1 to native handles.

**Java SWT Approach:** All handles are real native pointers. Java SWT stores actual NSToolbar*, NSToolbarItem*, etc. pointers as long values.

**API Exposure:** Handle IS publicly exposed as `public override IntPtr Handle` in Control class, but with `protected set`.

**Recommendation:** **Gradual migration** - We can keep Handle exposed but ensure it ALWAYS points to a real native object, using wrapper objects when needed.

---

## Current Pseudo-Handle Usage Analysis

### Locations Using Pseudo-Handles

1. **MacOSPlatform_TabFolder.cs**
   - Line 188: TabItem - `0x20000000 + id`
   - Line 363: ToolBar - `0x40000000 + id`  ⚠️ **CAUSES CRASH**
   - Line 421: ToolItem - `0x30000000 + id`

2. **MacOSPlatform_Table.cs**
   - Line 448: TableItem - unique ID scheme

3. **MacOSPlatform_Tree.cs**
   - Line 243: TreeItem - pseudo-handle

4. **Win32Platform_ToolBar.cs**
   - Line 218: ToolItem - `_nextToolItemId++`
   - Stored in TBBUTTON.dwData field (works because Win32 API expects this)

5. **Win32Platform_Table.cs**
   - Line 449: TableColumn - encoded handle
   - Line 617: TableItem - pseudo-handle

6. **Win32Platform_TabFolder.cs**
   - Line 173: TabItem - encoded handle

7. **LinuxPlatform_TabFolder.cs**
   - Line 144: TabItem - pseudo-handle

8. **LinuxPlatform_Table.cs**
   - Line 436: TableItem - pseudo-handle using row index

9. **LinuxPlatform_ToolBar.cs**
   - Line 126: ToolItem - pseudo-handle

### Why Pseudo-Handles Were Used

1. **Native API limitations**: Some native widgets don't provide direct access to sub-items
   - Win32 ToolBar items are identified by command ID, not HWND
   - NSToolbar items are objects but managed by NSToolbar, not standalone views

2. **Mapping complexity**: One-to-many relationships
   - One NSToolbar has many NSToolbarItems
   - One GtkNotebook has many tab items without individual handles

3. **Consistency**: Trying to provide uniform IntPtr API across platforms

---

## Problems with Pseudo-Handles

### Critical Issues

1. **Segmentation Faults** ⚠️⚠️⚠️
   - Calling objc_msgSend/SendMessage on fake pointers crashes
   - **Current**: ToolBar_Visible_ShouldGetAndSet crash on macOS

2. **Platform Method Confusion**
   - SetControlVisible assumes all handles are native widgets
   - No way to distinguish pseudo from real handles
   - Methods like SetEnabled, SetBounds fail on pseudo-handles

3. **Type Safety Lost**
   - IntPtr contains no type information
   - Can't tell NSToolbar from NSToolbarItem from NSView

4. **Memory Management Issues**
   - Pseudo-handles bypass native memory management
   - Can't use native ref-counting or GC integration

### Lesser Issues

1. **Debugging difficulty**: Pseudo-handles don't appear in native debuggers
2. **Interop limitations**: Can't pass pseudo-handles to native APIs
3. **Platform inconsistency**: Different schemes on each platform

---

## Java SWT Reference Architecture

### How Java SWT Handles This

```java
// Java SWT (simplified)
public class ToolBar extends Composite {
    long nsToolbar;  // Real NSToolbar* pointer

    public ToolBar(Composite parent, int style) {
        nsToolbar = OS.NSToolbar_alloc();
        // ... initialization
    }

    void setVisible(boolean visible) {
        if (nsToolbar != 0) {
            OS.NSToolbar_setVisible(nsToolbar, visible);
        }
    }
}

public class ToolItem extends Item {
    long nsToolbarItem;  // Real NSToolbarItem* pointer
    ToolBar parent;

    public ToolItem(ToolBar parent, int style) {
        nsToolbarItem = OS.NSToolbarItem_alloc();
        parent.addItem(nsToolbarItem);
    }
}
```

**Key Points:**
- Every widget stores REAL native pointer(s)
- Platform-specific behavior in OS bridge layer
- Type-safe: ToolBar knows it has NSToolbar, ToolItem knows it has NSToolbarItem

---

## Proposed Migration Strategy

### Phase 1: Immediate Fix (ToolBar Crash) 🔥

**Goal:** Stop the segfault in ToolBar_Visible_ShouldGetAndSet

**Approach:** Detect pseudo-handles in SetControlVisible and route to specialized methods

```csharp
// MacOSPlatform.cs
public void SetControlVisible(IntPtr handle, bool visible)
{
    // Detect pseudo-handles by high bits
    long handleValue = handle.ToInt64();

    if ((handleValue & 0x40000000) != 0)
    {
        // ToolBar pseudo-handle
        SetToolBarVisible(handle, visible);
        return;
    }

    if ((handleValue & 0x30000000) != 0)
    {
        // ToolItem pseudo-handle
        SetToolItemVisible(handle, visible);
        return;
    }

    if ((handleValue & 0x20000000) != 0)
    {
        // TabItem pseudo-handle
        SetTabItemVisible(handle, visible);
        return;
    }

    // Real handle - use standard API
    objc_msgSend_void(handle, _selSetHidden, !visible);
}

private void SetToolBarVisible(IntPtr pseudoHandle, bool visible)
{
    if (_toolBarData.TryGetValue(pseudoHandle, out var data))
    {
        // NSToolbar uses setVisible:, not setHidden:
        objc_msgSend_void(data.Toolbar, _selSetVisible, visible);
    }
}
```

**Pros:**
- Fixes crash immediately
- Minimal code changes
- Can deploy today

**Cons:**
- Still using pseudo-handles
- Magic number checks are fragile
- Doesn't solve root cause

---

### Phase 2: Handle Abstraction Layer (Medium Term)

**Goal:** Encapsulate handle management, hide pseudo-handles from platform methods

**Approach:** Introduce internal handle types and lookups

```csharp
// New internal class
internal enum HandleType
{
    RealNativeHandle,
    ToolBarHandle,
    ToolItemHandle,
    TabItemHandle,
    TableItemHandle,
    TreeItemHandle
}

internal struct ManagedHandle
{
    public HandleType Type;
    public IntPtr Value;  // Real pointer or pseudo ID

    public bool IsReal => Type == HandleType.RealNativeHandle;
    public bool IsPseudo => !IsReal;
}

internal class HandleRegistry
{
    private Dictionary<IntPtr, ManagedHandle> _handles = new();
    private Dictionary<IntPtr, object> _nativeObjects = new();

    public IntPtr Register(HandleType type, IntPtr realPointer, object nativeData)
    {
        IntPtr publicHandle = realPointer;

        _handles[publicHandle] = new ManagedHandle
        {
            Type = type,
            Value = realPointer
        };
        _nativeObjects[publicHandle] = nativeData;

        return publicHandle;
    }

    public bool TryGetNativeData<T>(IntPtr handle, out T data)
    {
        if (_nativeObjects.TryGetValue(handle, out var obj) && obj is T typed)
        {
            data = typed;
            return true;
        }
        data = default!;
        return false;
    }
}

// Updated MacOSPlatform
public void SetControlVisible(IntPtr handle, bool visible)
{
    if (_handleRegistry.TryGetNativeData<ToolBarData>(handle, out var toolbarData))
    {
        objc_msgSend_void(toolbarData.Toolbar, _selSetVisible, visible);
        return;
    }

    if (_handleRegistry.TryGetNativeData<ToolItemData>(handle, out var itemData))
    {
        // ToolItems don't have visibility - they're in toolbar or not
        return;
    }

    // Assume real native handle
    objc_msgSend_void(handle, _selSetHidden, !visible);
}
```

**Pros:**
- Type-safe handle management
- Platform methods can query handle metadata
- No magic numbers
- Easier to debug

**Cons:**
- More complex infrastructure
- Still not using real pointers for public API
- Requires larger refactoring

---

### Phase 3: Real Handles Only (Long Term - Java SWT Parity)

**Goal:** Every Handle property contains a real native object pointer

**Approach:** Use wrapper NSViews/NSWindows for non-view objects

```csharp
// For NSToolbar (which isn't an NSView)
public IntPtr CreateToolBar(IntPtr parent, int style)
{
    InitializeToolBarSelectors();

    // Create NSToolbar
    IntPtr toolbar = objc_msgSend(_nsToolbarClass, _selAlloc);
    IntPtr identifier = CreateNSString($"Toolbar{_nextToolItemId++}");
    toolbar = objc_msgSend(toolbar, _selInitWithIdentifier, identifier);

    // Option A: Create a wrapper NSView to hold the toolbar
    IntPtr containerView = objc_msgSend(_nsViewClass, _selAlloc);
    containerView = objc_msgSend(containerView, _selInit);

    // Attach toolbar to a window (will be set later when added to parent)
    _toolBarData[containerView] = new ToolBarData
    {
        Toolbar = toolbar,
        ContainerView = containerView,
        Window = IntPtr.Zero
    };

    // Return the REAL NSView pointer
    return containerView;

    // Option B: For ToolBar specifically, return the NSToolbar pointer directly
    // and have SetControlVisible check if it's an NSToolbar instance
}

public void SetControlVisible(IntPtr handle, bool visible)
{
    // Check what kind of object this is using Objective-C runtime
    bool isToolbar = objc_msgSend_bool(handle, _selIsKindOfClass, _nsToolbarClass);

    if (isToolbar)
    {
        objc_msgSend_void(handle, _selSetVisible, visible);
        return;
    }

    // Check if it's a view
    bool isView = objc_msgSend_bool(handle, _selIsKindOfClass, _nsViewClass);

    if (isView)
    {
        objc_msgSend_void(handle, _selSetHidden, !visible);
        return;
    }

    // Unknown type - error
    throw new ArgumentException($"Handle {handle:X} is not a supported widget type");
}
```

**Pros:**
- ✅ True Java SWT parity
- ✅ No pseudo-handles anywhere
- ✅ All handles work with native APIs
- ✅ Can use Objective-C runtime introspection
- ✅ Better memory management (retain/release cycles)

**Cons:**
- ❌ Major refactoring across all platforms
- ❌ Wrapper views add overhead
- ❌ Breaking changes if Handle semantics change
- ❌ Each platform needs different wrapper strategy

---

## Specific Widget Strategies

### NSToolbar (macOS)

**Problem:** NSToolbar is not an NSView, can't call setHidden:

**Solution Options:**

1. **Wrapper View** (Recommended)
   - Create NSView container
   - Store NSToolbar in _toolBarData dictionary keyed by NSView handle
   - Return NSView handle as public Handle

2. **Direct NSToolbar Pointer**
   - Return NSToolbar pointer directly
   - Update platform methods to detect NSToolbar via isKindOfClass:
   - Use NSToolbar-specific APIs (setVisible:)

3. **Synthetic NSControl Subclass** (Advanced)
   - Create custom NSControl subclass that wraps NSToolbar
   - Override visibility methods
   - Most Java SWT-like but requires Objective-C bridging

### NSToolbarItem (macOS)

**Problem:** NSToolbarItem is not an NSView either

**Solution:**
- ToolItems don't really have visibility - they're either in the toolbar or removed
- Could create wrapper NSButton for each item
- Or keep tracking in _toolItemData but use real NSToolbarItem pointers

### Win32 ToolItems

**Problem:** Toolbar items identified by command ID, no HWND

**Solution:**
- Actually already working! TBBUTTON.dwData stores pseudo-handle
- Win32's SendMessage(TB_HIDEBUTTON) works with button index
- Keep current approach for Win32

### GTK ToolItems

**Problem:** GtkToolItem IS a GtkWidget, should have real handle

**Solution:**
- GtkToolItem* is already a real pointer
- Stop using pseudo-handles on Linux
- Return gtk_toolbar_item directly

---

## API Compatibility Considerations

### Current Public API

```csharp
public class ToolBar : Composite
{
    public override IntPtr Handle { get; protected set; }
    // ... rest of API
}
```

**Handle is public** - Applications COULD be using it, though unlikely in typical SWT usage.

### Java SWT API

```java
public class ToolBar extends Composite {
    // NO public handle accessor!
    // Internal: long handle (package-private)
}
```

### Recommendation

**Option A: Keep Handle Public (Safer)**
- Maintain compatibility
- Ensure Handle always contains valid native pointer
- Document that Handle type varies by widget type

**Option B: Deprecate Public Handle (Better)**
- Mark Handle as [Obsolete("Use internal platform handles")]
- Move toward package-private like Java SWT
- Provides cleaner abstraction boundary

**Option C: Typed Handles (Best Long-Term)**
```csharp
public abstract class Widget
{
    internal abstract IntPtr InternalHandle { get; }

    // Provide typed access where needed
    public T GetPlatformHandle<T>() where T : struct
    {
        return Platform.GetTypedHandle<T>(InternalHandle);
    }
}
```

---

## Migration Phases Timeline

### Immediate (This PR)
- ✅ Fix ToolBar crash with pseudo-handle detection in SetControlVisible
- ✅ Add helper methods: SetToolBarVisible, SetToolItemVisible
- ✅ Document the pseudo-handle pattern

### Short Term (Next 2 PRs)
- Implement HandleRegistry for type-safe lookups
- Refactor platform methods to use HandleRegistry
- Remove magic number checks (0x40000000, etc.)

### Medium Term (Next 5 PRs)
- macOS: Replace ToolBar/ToolItem pseudo-handles with wrapper views or direct NSToolbar pointers
- Linux: Audit GTK pseudo-handles, replace with real GtkWidget* where possible
- Win32: Keep TBBUTTON approach but document why it's different

### Long Term (6+ months)
- Move toward Java SWT handle visibility model
- Consider deprecating public Handle property
- Full parity with Java SWT architecture

---

## Testing Strategy

### For Each Phase

1. **Unit Tests**
   - All existing widget tests must pass
   - Add specific tests for handle validity
   - Test platform method dispatch (SetVisible, SetEnabled, etc.)

2. **Integration Tests**
   - Test complex hierarchies (ToolBar in Shell in Display)
   - Test disposal and cleanup
   - Test rapid create/destroy cycles

3. **Platform-Specific Tests**
   - Verify native object lifecycle
   - Check memory leaks with Instruments/Valgrind
   - Test edge cases (null parents, disposed widgets)

---

## Risk Assessment

### High Risk ⚠️⚠️⚠️
- Breaking existing applications that access Handle directly
- Memory leaks if wrapper objects not properly managed
- Platform-specific crashes if native pointers mishandled

### Medium Risk ⚠️⚠️
- Performance overhead from HandleRegistry lookups
- Increased complexity in platform layer
- Harder to debug multi-layered abstraction

### Low Risk ⚠️
- Small API surface changes
- Well-tested with existing test suite
- Gradual migration minimizes blast radius

---

## Recommendation

**Execute Phase 1 immediately** to fix the crash, then:

1. **Phase 2 in next sprint**: Implement HandleRegistry
2. **Phase 3 over 6 months**: Migrate to real handles platform-by-platform
3. **API decision by v1.0**: Decide on Handle visibility (public vs internal)

This approach balances **immediate stability** with **long-term architectural health**.
