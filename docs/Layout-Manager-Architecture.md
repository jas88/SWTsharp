# SWTSharp Layout Manager Architecture Design

**Document Version:** 1.0
**Date:** October 6, 2025
**Status:** Architecture Design & Analysis

## Executive Summary

This document provides a comprehensive architectural design for SWTSharp's layout manager system, based on Java SWT's proven design patterns. The analysis reveals that **FillLayout, RowLayout, and GridLayout are already implemented**, leaving FormLayout and StackLayout as the primary implementation targets.

### Current Implementation Status

✅ **Completed (3/5 layouts)**
- `Layout` - Abstract base class
- `FillLayout` - Simple fill/wrap layout
- `RowLayout` - Horizontal/vertical rows with wrapping
- `GridLayout` - Advanced grid-based layout
- `GridData` - Grid layout constraint data
- `RowData` - Row layout constraint data

🔨 **Pending (2/5 layouts)**
- `FormLayout` - Constraint-based layout with attachments
- `StackLayout` - Card-style single-control display

---

## 1. Layout Manager Overview

### 1.1 Java SWT Layout Hierarchy

Java SWT provides five primary layout managers, listed from simplest to most complex:

1. **FillLayout** - Forces all children to be equal size in a single row/column
2. **RowLayout** - Flows children with wrapping, margins, and spacing
3. **GridLayout** - Flexible grid with spanning, alignment, and space grabbing
4. **FormLayout** - Constraint-based layout using attachments (relative positioning)
5. **StackLayout** - Shows only one child at a time (card-style switching)

### 1.2 Core Design Patterns

All SWT layouts follow a consistent architecture:

```
Abstract Layout Class
    ├── ComputeSize(composite, wHint, hHint, flushCache) → Point
    │   └── Calculates preferred size based on children
    │
    └── DoLayout(composite, flushCache) → bool
        └── Positions and sizes children based on algorithm

Layout Data Pattern
    └── Control.LayoutData property stores layout-specific constraints
        ├── FillLayout: No layout data needed
        ├── RowLayout: RowData (width, height hints)
        ├── GridLayout: GridData (span, alignment, grab)
        ├── FormLayout: FormData (FormAttachments for each edge)
        └── StackLayout: No layout data needed
```

---

## 2. Current Implementation Analysis

### 2.1 Existing Infrastructure

**File:** `/src/SWTSharp/Layout/Layout.cs`

The abstract base class is well-designed with three key methods:

```csharp
public abstract class Layout
{
    // Calculate preferred size
    protected internal abstract Point ComputeSize(
        Composite composite, int wHint, int hHint, bool flushCache);

    // Perform layout
    protected internal abstract bool DoLayout(
        Composite composite, bool flushCache);

    // Optional cache management
    protected internal virtual bool FlushCache(Control? control);
}
```

**Strengths:**
- Clean separation of size computation and layout execution
- Cache support via `flushCache` parameter
- Protected internal visibility prevents external misuse
- Virtual `FlushCache` for custom cache strategies

**Potential Issues:**
- No built-in minimum/maximum size constraints
- No support for layout animation/transitions
- Cache invalidation is manual (could be event-driven)

### 2.2 Composite Integration

**Files:**
- `/src/SWTSharp/Layout/Composite.cs` (Layout namespace)
- `/src/SWTSharp/Composite.cs` (Main class)

**Duplicate Composite Classes Detected:**

There are TWO different Composite implementations:

1. **Layout.Composite** - Simpler, focuses on layout integration
2. **SWTSharp.Composite** - More complete with LayoutManager, TabList, etc.

**Critical Issue:** This duplication creates confusion and potential bugs.

**Recommendation:** Merge into single implementation in `/src/SWTSharp/Composite.cs`

### 2.3 Implemented Layouts Analysis

#### FillLayout ✅
**Implementation Quality:** Excellent
**File:** `/src/SWTSharp/Layout/FillLayout.cs`

**Algorithm:**
1. Find maximum preferred size among children
2. Distribute equal space to all visible children
3. Support horizontal/vertical orientation

**Edge Cases Handled:**
- No visible children → return margins only
- Division by zero → uses `visibleCount`
- Spacing between children

**Performance:** O(n) where n = number of children

#### RowLayout ✅
**Implementation Quality:** Very Good
**File:** `/src/SWTSharp/Layout/RowLayout.cs`

**Algorithm:**
1. Calculate preferred sizes with wrapping logic
2. Group children into rows/columns
3. Apply alignment (fill, center, pack)
4. Support justify to expand last row

**Features:**
- Wrapping with configurable width/height
- Asymmetric margins (left, right, top, bottom)
- Fill perpendicular direction
- Center alignment
- Justify option

**Edge Cases Handled:**
- Wrap boundary conditions
- Empty rows/columns
- Excluded controls
- Variable child sizes

**Performance:** O(n) for layout, O(n) for size computation

#### GridLayout ✅
**Implementation Quality:** Excellent
**File:** `/src/SWTSharp/Layout/GridLayout.cs`

**Algorithm:**
1. Build 2D grid from children (handles spanning)
2. Calculate minimum column widths and row heights
3. Distribute excess space to grabbing columns/rows
4. Apply alignment and positioning

**Features:**
- Multi-cell spanning (horizontal and vertical)
- Space grabbing (proportional distribution)
- Alignment (BEGINNING, CENTER, END, FILL)
- Equal column width option
- Size hints and minimums
- Indentation support
- Comprehensive caching

**Edge Cases Handled:**
- Complex spanning scenarios
- Sparse grids (empty cells)
- Overflow protection (span clamping)
- Circular dependency prevention (via BuildGrid algorithm)

**Performance:**
- Grid building: O(n)
- Size computation: O(rows × cols)
- Layout: O(rows × cols)

**Caching Strategy:**
```csharp
private int _cacheWidth = -1;
private int _cacheHeight = -1;
private int[]? _cachedColumnWidths;
private int[]? _cachedRowHeights;
```

**Strengths:**
- Sophisticated spanning algorithm
- Efficient space distribution
- Proper minimum size handling
- Cache invalidation on flush

---

## 3. Missing Layouts - Design Specification

### 3.1 FormLayout (Constraint-Based)

#### 3.1.1 Overview

FormLayout is the most powerful and complex SWT layout manager. It uses **mathematical constraints** to position controls relative to:
- Parent composite edges
- Other controls
- Percentage positions
- Absolute offsets

**Mathematical Model:**
```
position = (numerator / denominator × dimension) + offset
```

Where:
- `numerator/denominator` = percentage (e.g., 50/100 = 50%)
- `dimension` = parent width or height
- `offset` = pixel adjustment

#### 3.1.2 Required Classes

**FormLayout.cs**
```csharp
namespace SWTSharp.Layout;

public class FormLayout : Layout
{
    // Margins
    public int MarginWidth { get; set; } = 0;
    public int MarginHeight { get; set; } = 0;
    public int MarginLeft { get; set; } = 0;
    public int MarginTop { get; set; } = 0;
    public int MarginRight { get; set; } = 0;
    public int MarginBottom { get; set; } = 0;

    // Spacing between attached controls
    public int Spacing { get; set; } = 0;

    protected internal override Point ComputeSize(
        Composite composite, int wHint, int hHint, bool flushCache);

    protected internal override bool DoLayout(
        Composite composite, bool flushCache);
}
```

**FormData.cs**
```csharp
namespace SWTSharp.Layout;

public class FormData
{
    // Attachments for each edge
    public FormAttachment? Left { get; set; }
    public FormAttachment? Top { get; set; }
    public FormAttachment? Right { get; set; }
    public FormAttachment? Bottom { get; set; }

    // Size hints
    public int Width { get; set; } = SWT.DEFAULT;
    public int Height { get; set; } = SWT.DEFAULT;

    // Exclude from layout
    public bool Exclude { get; set; } = false;

    // Constructors for common patterns
    public FormData() { }
    public FormData(int width, int height) { }
    public FormData(FormAttachment left, FormAttachment top,
                   FormAttachment right, FormAttachment bottom) { }
}
```

**FormAttachment.cs**
```csharp
namespace SWTSharp.Layout;

public class FormAttachment
{
    // The equation: y = ax + b
    // where: a = numerator/denominator, b = offset

    public int Numerator { get; set; }      // Percentage numerator
    public int Denominator { get; set; }    // Percentage denominator
    public int Offset { get; set; }         // Pixel offset

    public Control? Control { get; set; }   // Attach to control edge
    public int Alignment { get; set; }      // Which edge of control

    // Attach to percentage of parent
    public FormAttachment(int numerator, int denominator, int offset = 0);

    // Attach to control edge
    public FormAttachment(Control control, int offset = 0, int alignment = SWT.DEFAULT);

    // Compute attachment position
    internal int ComputePosition(
        Control control, int compositeSize, bool isVertical);
}
```

#### 3.1.3 Implementation Algorithm

**Phase 1: Dependency Graph Construction**

```csharp
private Dictionary<Control, List<Control>> BuildDependencyGraph(
    Control[] children, Dictionary<Control, FormData> layoutData)
{
    var dependencies = new Dictionary<Control, List<Control>>();

    foreach (var child in children)
    {
        var data = layoutData[child];
        dependencies[child] = new List<Control>();

        // Track dependencies from FormAttachments
        if (data.Left?.Control != null)
            dependencies[child].Add(data.Left.Control);
        if (data.Top?.Control != null)
            dependencies[child].Add(data.Top.Control);
        if (data.Right?.Control != null)
            dependencies[child].Add(data.Right.Control);
        if (data.Bottom?.Control != null)
            dependencies[child].Add(data.Bottom.Control);
    }

    return dependencies;
}
```

**Phase 2: Circular Dependency Detection**

```csharp
private void DetectCircularDependencies(
    Dictionary<Control, List<Control>> dependencies)
{
    foreach (var control in dependencies.Keys)
    {
        var visited = new HashSet<Control>();
        if (HasCycle(control, dependencies, visited))
        {
            throw new SWTException(
                "Circular dependency detected in FormLayout. " +
                "Control attachments must form a directed acyclic graph (DAG).");
        }
    }
}

private bool HasCycle(Control control,
    Dictionary<Control, List<Control>> dependencies,
    HashSet<Control> visited)
{
    if (visited.Contains(control))
        return true;

    visited.Add(control);

    foreach (var dep in dependencies[control])
    {
        if (HasCycle(dep, dependencies, visited))
            return true;
    }

    visited.Remove(control); // Backtrack
    return false;
}
```

**Phase 3: Topological Sort for Layout Order**

```csharp
private List<Control> TopologicalSort(
    Control[] children,
    Dictionary<Control, List<Control>> dependencies)
{
    var sorted = new List<Control>();
    var visited = new HashSet<Control>();

    void Visit(Control control)
    {
        if (visited.Contains(control)) return;
        visited.Add(control);

        foreach (var dep in dependencies[control])
        {
            Visit(dep);
        }

        sorted.Add(control);
    }

    foreach (var child in children)
    {
        Visit(child);
    }

    sorted.Reverse(); // Reverse to get correct order
    return sorted;
}
```

**Phase 4: Position Calculation**

```csharp
protected internal override bool DoLayout(Composite composite, bool flushCache)
{
    var children = composite.GetChildren();
    var clientArea = composite.GetClientArea();

    // Build dependency graph
    var layoutData = children.ToDictionary(c => c, c => GetFormData(c));
    var dependencies = BuildDependencyGraph(children, layoutData);

    // Detect circular dependencies
    DetectCircularDependencies(dependencies);

    // Topological sort for correct processing order
    var sortedChildren = TopologicalSort(children, dependencies);

    // Calculate positions for each control
    foreach (var child in sortedChildren)
    {
        var data = layoutData[child];
        if (data.Exclude || !child.Visible) continue;

        int left = ComputeLeftPosition(child, data, clientArea);
        int top = ComputeTopPosition(child, data, clientArea);
        int right = ComputeRightPosition(child, data, clientArea);
        int bottom = ComputeBottomPosition(child, data, clientArea);

        int x = left;
        int y = top;
        int width = right - left;
        int height = bottom - top;

        // Apply size hints
        if (data.Width != SWT.DEFAULT)
            width = data.Width;
        if (data.Height != SWT.DEFAULT)
            height = data.Height;

        child.SetBounds(x, y, width, height);
    }

    return true;
}
```

#### 3.1.4 Edge Cases and Challenges

**Circular Dependencies:**
```csharp
// BAD: button1.left → button2.right, button2.left → button1.right
var data1 = new FormData();
data1.Left = new FormAttachment(button2, 0, SWT.RIGHT);
var data2 = new FormData();
data2.Left = new FormAttachment(button1, 0, SWT.RIGHT);
```

**Solution:** Detect using depth-first search before layout

**Under-Constrained Controls:**
```csharp
// Missing constraints - what should width/height be?
var data = new FormData();
data.Left = new FormAttachment(0, 10); // Only left edge defined
```

**Solution:** Use default size or throw exception

**Over-Constrained Controls:**
```csharp
// All four edges defined - might conflict with size hints
var data = new FormData();
data.Left = new FormAttachment(0, 10);
data.Right = new FormAttachment(100, -10);
data.Top = new FormAttachment(0, 10);
data.Bottom = new FormAttachment(100, -10);
data.Width = 500; // Conflicts with left/right!
```

**Solution:** Attachments take precedence over size hints

**Performance Considerations:**
- Dependency graph building: O(n)
- Circular detection: O(n × d) where d = max dependency depth
- Topological sort: O(n + e) where e = edges
- Layout calculation: O(n)

**Overall:** O(n × d) - acceptable for typical UI hierarchies

#### 3.1.5 Testing Challenges

**Unit Tests Required:**
1. Simple percentage attachments
2. Control-to-control attachments
3. Circular dependency detection
4. Offset calculations
5. Size hint conflicts
6. Margin and spacing
7. Excluded controls
8. Dynamic resizing
9. Complex nested attachments

**Visual Regression Tests:**
- Difficult to automate (requires pixel-perfect comparison)
- Recommend manual testing with standard layouts

### 3.2 StackLayout (Card-Style)

#### 3.2.1 Overview

StackLayout is the simplest unimplemented layout. It displays only ONE child control at a time, hiding all others. Controls are stacked on top of each other and all have the same size as the client area.

**Key Concept:**
```csharp
layout.TopControl = button1; // Shows button1, hides all others
parent.Layout();             // Trigger layout update
```

#### 3.2.2 Required Classes

**StackLayout.cs**
```csharp
namespace SWTSharp.Custom;  // Note: SWT puts this in custom package

public class StackLayout : Layout
{
    /// <summary>
    /// The control to display on top of the stack.
    /// All other controls are hidden.
    /// </summary>
    public Control? TopControl { get; set; }

    /// <summary>
    /// Left and right margins.
    /// </summary>
    public int MarginWidth { get; set; } = 0;

    /// <summary>
    /// Top and bottom margins.
    /// </summary>
    public int MarginHeight { get; set; } = 0;

    protected internal override Point ComputeSize(
        Composite composite, int wHint, int hHint, bool flushCache)
    {
        // Return size of topControl or maximum of all children
        int maxWidth = 0;
        int maxHeight = 0;

        var children = composite.GetChildren();
        Control? sizeControl = TopControl ?? children.FirstOrDefault();

        if (sizeControl != null)
        {
            var size = GetControlSize(sizeControl);
            maxWidth = size.X;
            maxHeight = size.Y;
        }

        return new Point(
            maxWidth + 2 * MarginWidth,
            maxHeight + 2 * MarginHeight
        );
    }

    protected internal override bool DoLayout(
        Composite composite, bool flushCache)
    {
        var children = composite.GetChildren();
        var clientArea = composite.GetClientArea();

        int x = clientArea.X + MarginWidth;
        int y = clientArea.Y + MarginHeight;
        int width = clientArea.Width - 2 * MarginWidth;
        int height = clientArea.Height - 2 * MarginHeight;

        foreach (var child in children)
        {
            // Size all children the same (even if hidden)
            child.SetBounds(x, y, width, height);

            // Show only topControl
            child.Visible = (child == TopControl);
        }

        return true;
    }
}
```

#### 3.2.3 Implementation Notes

**Simplicity:** StackLayout is trivial compared to FormLayout

**Key Behaviors:**
1. All children have identical size
2. Only topControl is visible
3. Switching is instant (no animation)
4. Parent.Layout() must be called after changing topControl

**Use Cases:**
- Wizard pages
- Tab-like interfaces (without tab bar)
- View switching in composite panels
- Multi-page forms

**Performance:** O(n) where n = number of children (linear iteration)

#### 3.2.4 Testing Challenges

**Unit Tests Required:**
1. TopControl visibility
2. All controls same size
3. Switching between controls
4. Null topControl handling
5. Margin application
6. Size computation

**Edge Cases:**
- TopControl = null → all children hidden
- TopControl not in children → all children hidden
- Rapid switching
- Disposed topControl

---

## 4. Architecture Integration

### 4.1 Namespace Organization

**Current Structure:**
```
SWTSharp/
├── Layout/
│   ├── Layout.cs           (Abstract base)
│   ├── Composite.cs        (⚠️ Duplicate - should remove)
│   ├── FillLayout.cs       ✅
│   ├── RowLayout.cs        ✅
│   ├── RowData.cs          ✅
│   ├── GridLayout.cs       ✅
│   ├── GridData.cs         ✅
│   ├── Point.cs
│   └── Rectangle.cs
├── Composite.cs            (Main implementation)
└── Control.cs
```

**Proposed Structure:**
```
SWTSharp/
├── Layout/
│   ├── Layout.cs           (Abstract base)
│   ├── FillLayout.cs       ✅
│   ├── RowLayout.cs        ✅
│   ├── RowData.cs          ✅
│   ├── GridLayout.cs       ✅
│   ├── GridData.cs         ✅
│   ├── FormLayout.cs       🔨 NEW
│   ├── FormData.cs         🔨 NEW
│   ├── FormAttachment.cs   🔨 NEW
│   ├── Point.cs
│   └── Rectangle.cs
├── Custom/                 🔨 NEW namespace
│   └── StackLayout.cs      🔨 NEW
├── Composite.cs
└── Control.cs
```

**Rationale:**
- FormLayout is standard layout → belongs in `Layout` namespace
- StackLayout is utility → belongs in `Custom` namespace (matching Java SWT)
- Remove duplicate `Layout.Composite`

### 4.2 Composite Class Consolidation

**Critical Issue:** Two Composite implementations exist

**Resolution Plan:**

1. **Primary Implementation:** `/src/SWTSharp/Composite.cs`
   - More feature-complete
   - Has LayoutManager abstraction
   - Includes TabList, BackgroundMode, etc.

2. **Legacy Implementation:** `/src/SWTSharp/Layout/Composite.cs`
   - Simpler, layout-focused
   - Should be removed after migration

**Migration Steps:**

```csharp
// Ensure SWTSharp.Composite has all features:

public class Composite : Control
{
    private Layout? _layout;  // ADD: Support Layout directly

    // MERGE: Support both Layout and LayoutManager
    public Layout? Layout
    {
        get => _layout;
        set
        {
            _layout = value;
            RequestLayout();
        }
    }

    // KEEP: Existing LayoutManager for extensibility
    public LayoutManager? LayoutManager { get; set; }

    // ENHANCE: DoLayout should use Layout if set
    public void DoLayout(bool changed)
    {
        if (_layout != null)
        {
            _layout.DoLayout(this, changed);
        }
        else if (LayoutManager != null)
        {
            LayoutManager.PerformLayout(this, changed);
        }
    }

    // ENHANCE: ComputeSize should use Layout if set
    public Point ComputeSize(int wHint, int hHint, bool changed)
    {
        if (_layout != null)
        {
            return _layout.ComputeSize(this, wHint, hHint, changed);
        }
        else if (LayoutManager != null)
        {
            var size = LayoutManager.ComputeSize(this, wHint, hHint);
            return new Point(size.Width, size.Height);
        }
        return new Point(64, 64);
    }
}
```

### 4.3 Control.SetLayoutData Integration

**Current Implementation:** `/src/SWTSharp/Control.cs`

```csharp
public void SetLayoutData(object? layoutData)
{
    CheckWidget();
    _layoutData = layoutData;

    // Trigger parent layout recalculation
    if (_parent is SWTSharp.Layout.Composite composite && composite.Layout != null)
    {
        composite.DoLayout(true); // flushCache = true
    }
}
```

**Issue:** References `SWTSharp.Layout.Composite` which should be removed

**Fix:**
```csharp
public void SetLayoutData(object? layoutData)
{
    CheckWidget();
    _layoutData = layoutData;

    // Trigger parent layout recalculation
    if (_parent is Composite composite && composite.Layout != null)
    {
        composite.DoLayout(true);
    }
}
```

### 4.4 Resize Event Wiring

**Current Gap:** Layout is not automatically triggered on resize

**Required Enhancement:**

```csharp
public class Composite : Control
{
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        // Auto-layout on resize
        if (Layout != null && !IsLayoutDeferred)
        {
            DoLayout(false); // Don't flush cache on resize
        }
    }
}
```

**Platform Integration:**

Each platform must trigger resize events:

```csharp
// Platform-specific implementations
interface IPlatform
{
    void SetResizeCallback(IntPtr handle, Action callback);
}
```

---

## 5. Implementation Roadmap

### Phase 1: Cleanup and Consolidation (2-3 days)

**Tasks:**
1. ✅ Audit existing layouts (FillLayout, RowLayout, GridLayout)
2. 🔨 Merge Composite implementations
3. 🔨 Fix Control.SetLayoutData parent reference
4. 🔨 Add resize event wiring
5. 🔨 Create comprehensive unit tests for existing layouts
6. 🔨 Document layout data patterns

**Deliverables:**
- Single Composite class
- Resize-triggered layout
- 80%+ test coverage for existing layouts

### Phase 2: StackLayout Implementation (2-3 days)

**Tasks:**
1. 🔨 Create `Custom` namespace
2. 🔨 Implement StackLayout class
3. 🔨 Add unit tests
4. 🔨 Create sample application demonstrating switching
5. 🔨 Document usage patterns

**Deliverables:**
- Working StackLayout
- Sample wizard application
- API documentation

**Risk Level:** LOW (simple implementation)

### Phase 3: FormLayout Foundation (5-7 days)

**Tasks:**
1. 🔨 Implement FormAttachment class
2. 🔨 Implement FormData class
3. 🔨 Create dependency graph algorithms
4. 🔨 Implement circular dependency detection
5. 🔨 Add topological sort
6. 🔨 Comprehensive unit tests

**Deliverables:**
- FormAttachment with position computation
- FormData with constraint validation
- Dependency analysis algorithms
- 90%+ test coverage for algorithms

**Risk Level:** MEDIUM (complex algorithm, needs validation)

### Phase 4: FormLayout Implementation (7-10 days)

**Tasks:**
1. 🔨 Implement FormLayout.ComputeSize
2. 🔨 Implement FormLayout.DoLayout
3. 🔨 Handle edge cases (under/over-constrained)
4. 🔨 Performance optimization
5. 🔨 Integration tests with complex scenarios
6. 🔨 Create sample applications

**Deliverables:**
- Complete FormLayout implementation
- Performance benchmarks
- Sample form designer application
- Migration guide from manual positioning

**Risk Level:** HIGH (complex constraint solving, many edge cases)

### Phase 5: Documentation and Examples (3-5 days)

**Tasks:**
1. 🔨 API documentation for all layouts
2. 🔨 Migration guide from SetBounds to layouts
3. 🔨 Best practices guide
4. 🔨 Layout selection decision tree
5. 🔨 Sample gallery application
6. 🔨 Performance tuning guide

**Deliverables:**
- Complete API docs
- Developer guide
- Sample application suite

---

## 6. Risk Analysis and Mitigation

### 6.1 Circular Dependency Resolution (FormLayout)

**Risk Level:** HIGH
**Impact:** Could cause infinite loops or incorrect layouts

**Mitigation Strategies:**
1. **Defensive Programming:**
   ```csharp
   if (HasCycle(control, dependencies, visited))
   {
       throw new SWTException(
           "Circular dependency in FormLayout. " +
           $"Control {control} creates a cycle.");
   }
   ```

2. **Developer Tools:**
   - Layout visualizer to show dependency graph
   - Warning for potential circular dependencies
   - Design-time validation in designer tools

3. **Testing:**
   - Fuzz testing with random attachments
   - Known circular patterns database
   - Automated detection in CI/CD

### 6.2 Performance Impact

**Risk Level:** MEDIUM
**Impact:** Layout computation could become bottleneck with deep hierarchies

**Current Performance Characteristics:**

| Layout | ComputeSize | DoLayout | Caching | Notes |
|--------|------------|----------|---------|-------|
| FillLayout | O(n) | O(n) | None | Very fast |
| RowLayout | O(n) | O(n) | None | Fast, wrapping adds constant factor |
| GridLayout | O(r×c) | O(r×c) | Yes | Cached column/row sizes |
| FormLayout | O(n×d) | O(n) | TBD | d = dependency depth |
| StackLayout | O(n) | O(n) | None | Trivial |

**Mitigation Strategies:**

1. **Caching:**
   ```csharp
   protected internal override bool FlushCache(Control? control)
   {
       if (control == null)
       {
           // Flush all
           _dependencyCache = null;
           _sizeCache.Clear();
       }
       else
       {
           // Flush only affected controls
           _sizeCache.Remove(control);
       }
       return true;
   }
   ```

2. **Incremental Layout:**
   - Only re-layout changed controls
   - Track dirty regions
   - Skip unchanged subtrees

3. **Performance Monitoring:**
   - Log slow layouts (> 16ms threshold)
   - Track layout frequency
   - Identify layout thrashing

### 6.3 Platform-Specific Resize Behavior

**Risk Level:** MEDIUM
**Impact:** Different platforms may resize differently

**Platform Differences:**

| Platform | Resize Behavior | Event Frequency | Batching |
|----------|----------------|-----------------|----------|
| Windows | Continuous during drag | High | Native |
| macOS | Smooth animation | Very high | Native |
| Linux | Varies by WM | Variable | Depends on WM |

**Mitigation Strategies:**

1. **Debouncing:**
   ```csharp
   private Timer? _resizeTimer;

   protected override void OnNativeResize()
   {
       _resizeTimer?.Stop();
       _resizeTimer = new Timer(16); // 16ms debounce
       _resizeTimer.Elapsed += (s, e) =>
       {
           DoLayout(false);
           _resizeTimer?.Stop();
       };
       _resizeTimer.Start();
   }
   ```

2. **Deferred Layout:**
   ```csharp
   public bool IsLayoutDeferred { get; set; }

   // During resize drag:
   composite.IsLayoutDeferred = true;

   // On resize complete:
   composite.IsLayoutDeferred = false;
   composite.DoLayout(true);
   ```

3. **Platform Abstraction:**
   ```csharp
   interface IPlatform
   {
       ResizeStrategy GetResizeStrategy();
   }

   enum ResizeStrategy
   {
       Immediate,      // Layout on every resize
       Debounced,      // Layout after delay
       OnComplete      // Layout when resize ends
   }
   ```

### 6.4 Breaking Changes to Existing API

**Risk Level:** LOW
**Impact:** Migration effort for existing code

**Potential Breaking Changes:**

1. **Composite Consolidation:**
   ```csharp
   // Before: Layout.Composite
   using SWTSharp.Layout;
   var composite = new Composite(parent, SWT.NONE);

   // After: SWTSharp.Composite
   using SWTSharp;
   var composite = new Composite(parent, SWT.NONE);
   ```

   **Mitigation:** Type alias for backward compatibility
   ```csharp
   namespace SWTSharp.Layout
   {
       [Obsolete("Use SWTSharp.Composite instead")]
       public class Composite : SWTSharp.Composite
       {
           public Composite(Control parent, int style)
               : base(parent, style) { }
       }
   }
   ```

2. **DoLayout Signature:**
   - Current implementation is consistent
   - No changes needed

3. **LayoutData Types:**
   - Existing RowData, GridData are stable
   - New FormData is additive

**Migration Path:**
- Provide obsolete attributes with migration hints
- Document breaking changes in CHANGELOG
- Offer automated migration tool (optional)

### 6.5 Testing Visual Layouts

**Risk Level:** MEDIUM
**Impact:** Difficult to verify correct visual appearance

**Challenges:**

1. **Pixel-Perfect Comparison:**
   - Platform rendering differences
   - Font rendering variations
   - Anti-aliasing differences

2. **Dynamic Content:**
   - Window resizing
   - Content changes
   - DPI scaling

**Mitigation Strategies:**

1. **Unit Tests for Algorithms:**
   ```csharp
   [Fact]
   public void GridLayout_DistributesSpaceEqually_WhenAllGrab()
   {
       var layout = new GridLayout(3, false);
       var composite = CreateTestComposite(300, 100);

       // All children grab horizontal space
       foreach (var child in composite.GetChildren())
       {
           var data = new GridData();
           data.GrabExcessHorizontalSpace = true;
           child.SetLayoutData(data);
       }

       layout.DoLayout(composite, true);

       // Verify equal distribution
       var children = composite.GetChildren();
       Assert.Equal(100, children[0].GetBounds().Width);
       Assert.Equal(100, children[1].GetBounds().Width);
       Assert.Equal(100, children[2].GetBounds().Width);
   }
   ```

2. **Snapshot Testing:**
   - Capture control bounds after layout
   - Compare against known-good snapshots
   - Detect regressions automatically

3. **Manual Testing Suite:**
   - Gallery application with standard layouts
   - Side-by-side comparison with Java SWT
   - Platform-specific test cases

4. **Accessibility Testing:**
   - Verify minimum sizes
   - Check contrast ratios
   - Validate tab order

---

## 7. Performance Considerations

### 7.1 Layout Computation Frequency

**Problem:** Layouts can be triggered frequently during:
- Window resizing
- Content updates
- Visibility changes
- Child additions/removals

**Optimization Strategies:**

1. **Smart Caching:**
   ```csharp
   private sealed class LayoutCache
   {
       public int Width { get; set; }
       public int Height { get; set; }
       public int[] ColumnWidths { get; set; }
       public int[] RowHeights { get; set; }
       public long Timestamp { get; set; }

       public bool IsValid(int width, int height, long maxAge)
       {
           return Width == width &&
                  Height == height &&
                  (DateTime.Now.Ticks - Timestamp) < maxAge;
       }
   }
   ```

2. **Dirty Tracking:**
   ```csharp
   public class Composite : Control
   {
       private bool _layoutDirty = true;

       public void InvalidateLayout()
       {
           _layoutDirty = true;
           if (Parent is Composite parent)
               parent.InvalidateLayout();
       }

       public void DoLayout(bool changed)
       {
           if (!_layoutDirty && !changed)
               return; // Skip if clean

           Layout?.DoLayout(this, changed);
           _layoutDirty = false;
       }
   }
   ```

3. **Incremental Updates:**
   ```csharp
   public void UpdateChild(Control child)
   {
       // Only re-layout affected area
       if (Layout is GridLayout gridLayout)
       {
           gridLayout.LayoutCell(this, child);
       }
       else
       {
           DoLayout(true); // Fallback to full layout
       }
   }
   ```

### 7.2 Memory Overhead

**Current State:**

| Layout | Memory per Instance | Cache Size | Notes |
|--------|-------------------|------------|-------|
| FillLayout | ~64 bytes | None | Minimal |
| RowLayout | ~128 bytes | None | Some lists |
| GridLayout | ~256 bytes | O(r×c) ints | Large cache |
| FormLayout | ~512 bytes | O(n×d) graph | Complex |
| StackLayout | ~64 bytes | None | Minimal |

**Optimization Strategies:**

1. **Weak References for Caches:**
   ```csharp
   private WeakReference<LayoutCache>? _cacheRef;

   protected LayoutCache? GetCache()
   {
       if (_cacheRef?.TryGetTarget(out var cache) == true)
           return cache;
       return null;
   }
   ```

2. **Pooling for Temporary Objects:**
   ```csharp
   private static readonly ObjectPool<List<Control>> _listPool =
       new ObjectPool<List<Control>>(() => new List<Control>());

   protected List<Control> GetWorkingList()
   {
       var list = _listPool.Get();
       list.Clear();
       return list;
   }

   protected void ReleaseWorkingList(List<Control> list)
   {
       _listPool.Return(list);
   }
   ```

3. **Lazy Initialization:**
   ```csharp
   public class FormLayout : Layout
   {
       private Dictionary<Control, FormData>? _dataCache;

       private Dictionary<Control, FormData> DataCache =>
           _dataCache ??= new Dictionary<Control, FormData>();
   }
   ```

### 7.3 Minimum Size Computation

**Challenge:** ComputeSize can be expensive for nested composites

**Strategy: Recursive Caching**

```csharp
private sealed class SizeCache
{
    private readonly Dictionary<(int wHint, int hHint), Point> _cache;
    private long _invalidateTime;

    public Point? GetCachedSize(int wHint, int hHint, long currentTime)
    {
        if (currentTime - _invalidateTime > TimeSpan.FromSeconds(1).Ticks)
        {
            _cache.Clear();
            return null;
        }

        if (_cache.TryGetValue((wHint, hHint), out var size))
            return size;

        return null;
    }

    public void CacheSize(int wHint, int hHint, Point size)
    {
        _cache[(wHint, hHint)] = size;
    }

    public void Invalidate()
    {
        _invalidateTime = DateTime.Now.Ticks;
    }
}
```

---

## 8. Developer Experience Enhancements

### 8.1 Layout Selection Decision Tree

**Documentation Aid:**

```
Need automatic layout?
├─ YES → Continue
└─ NO → Use manual SetBounds()

Children same size?
├─ YES → Use FillLayout
│   ├─ Horizontal → FillLayout(SWT.HORIZONTAL)
│   └─ Vertical → FillLayout(SWT.VERTICAL)
└─ NO → Continue

Need wrapping?
├─ YES → Use RowLayout
│   ├─ Horizontal flow → RowLayout(SWT.HORIZONTAL)
│   └─ Vertical flow → RowLayout(SWT.VERTICAL)
└─ NO → Continue

Need grid structure?
├─ YES → Use GridLayout
│   ├─ Simple grid → GridLayout(numColumns)
│   ├─ Spanning → GridLayout + GridData with span
│   └─ Equal columns → GridLayout(numColumns, true)
└─ NO → Continue

Need relative positioning?
├─ YES → Use FormLayout
│   ├─ Percentage → FormAttachment(numerator, denominator)
│   ├─ Relative to control → FormAttachment(control, offset)
│   └─ Complex constraints → FormData with multiple attachments
└─ NO → Continue

Show one control at a time?
├─ YES → Use StackLayout
│   └─ Switch with topControl property
└─ NO → Custom Layout or manual positioning
```

### 8.2 Common Layout Patterns

**Pattern Library:**

```csharp
namespace SWTSharp.Layout.Patterns;

public static class CommonLayouts
{
    /// <summary>
    /// Creates a typical dialog layout with content and button bar.
    /// </summary>
    public static void ApplyDialogLayout(Composite dialog,
        Composite contentArea, Composite buttonBar)
    {
        var layout = new GridLayout(1, false);
        layout.MarginWidth = 10;
        layout.MarginHeight = 10;
        layout.VerticalSpacing = 10;
        dialog.Layout = layout;

        // Content area grabs vertical space
        var contentData = new GridData(GridData.FILL, GridData.FILL,
                                      true, true);
        contentArea.SetLayoutData(contentData);

        // Button bar at bottom
        var buttonData = new GridData(GridData.END, GridData.CENTER,
                                     false, false);
        buttonBar.SetLayoutData(buttonData);
    }

    /// <summary>
    /// Creates a form layout with labels and inputs.
    /// </summary>
    public static void ApplyFormLayout(Composite form,
        params (Label label, Control input)[] fields)
    {
        var layout = new GridLayout(2, false);
        layout.MarginWidth = 10;
        layout.MarginHeight = 10;
        form.Layout = layout;

        foreach (var (label, input) in fields)
        {
            // Label: right-aligned
            var labelData = new GridData(GridData.END, GridData.CENTER,
                                        false, false);
            label.SetLayoutData(labelData);

            // Input: fills horizontally
            var inputData = new GridData(GridData.FILL, GridData.CENTER,
                                        true, false);
            input.SetLayoutData(inputData);
        }
    }

    /// <summary>
    /// Creates a master-detail layout.
    /// </summary>
    public static void ApplyMasterDetailLayout(Composite container,
        Control master, Control detail)
    {
        var layout = new FormLayout();
        container.Layout = layout;

        // Master on left, 30% width
        var masterData = new FormData();
        masterData.Left = new FormAttachment(0, 0);
        masterData.Top = new FormAttachment(0, 0);
        masterData.Bottom = new FormAttachment(100, 0);
        masterData.Right = new FormAttachment(30, 0);
        master.SetLayoutData(masterData);

        // Detail on right, remaining space
        var detailData = new FormData();
        detailData.Left = new FormAttachment(master, 5);
        detailData.Top = new FormAttachment(0, 0);
        detailData.Bottom = new FormAttachment(100, 0);
        detailData.Right = new FormAttachment(100, 0);
        detail.SetLayoutData(detailData);
    }
}
```

### 8.3 Debugging Tools

**Layout Inspector:**

```csharp
namespace SWTSharp.Layout.Diagnostics;

public static class LayoutDebugger
{
    public static void PrintLayoutTree(Composite root, int indent = 0)
    {
        var prefix = new string(' ', indent * 2);
        Console.WriteLine($"{prefix}{root.GetType().Name}");
        Console.WriteLine($"{prefix}  Layout: {root.Layout?.GetType().Name ?? "None"}");
        Console.WriteLine($"{prefix}  Bounds: {root.GetBounds()}");

        foreach (var child in root.GetChildren())
        {
            Console.WriteLine($"{prefix}  ├─ {child.GetType().Name}");
            Console.WriteLine($"{prefix}  │  LayoutData: {child.GetLayoutData()?.GetType().Name ?? "None"}");
            Console.WriteLine($"{prefix}  │  Bounds: {child.GetBounds()}");
            Console.WriteLine($"{prefix}  │  Visible: {child.Visible}");

            if (child is Composite composite)
            {
                PrintLayoutTree(composite, indent + 1);
            }
        }
    }

    public static void ValidateLayout(Composite composite)
    {
        if (composite.Layout is GridLayout gridLayout)
        {
            ValidateGridLayout(composite, gridLayout);
        }
        else if (composite.Layout is FormLayout formLayout)
        {
            ValidateFormLayout(composite, formLayout);
        }
    }

    private static void ValidateGridLayout(Composite composite, GridLayout layout)
    {
        var children = composite.GetChildren();
        int expectedCells = layout.NumColumns;

        foreach (var child in children)
        {
            if (child.GetLayoutData() is GridData data)
            {
                if (data.HorizontalSpan > layout.NumColumns)
                {
                    Console.WriteLine($"WARNING: {child} spans more columns than available");
                }

                if (data.HorizontalAlignment == GridData.FILL &&
                    !data.GrabExcessHorizontalSpace)
                {
                    Console.WriteLine($"INFO: {child} uses FILL without grabExcessSpace");
                }
            }
        }
    }

    private static void ValidateFormLayout(Composite composite, FormLayout layout)
    {
        var children = composite.GetChildren();

        foreach (var child in children)
        {
            if (child.GetLayoutData() is FormData data)
            {
                int constraintCount = 0;
                if (data.Left != null) constraintCount++;
                if (data.Right != null) constraintCount++;
                if (data.Top != null) constraintCount++;
                if (data.Bottom != null) constraintCount++;

                if (constraintCount == 0)
                {
                    Console.WriteLine($"WARNING: {child} has no constraints");
                }

                if (constraintCount < 2)
                {
                    Console.WriteLine($"INFO: {child} is under-constrained");
                }
            }
        }
    }
}
```

---

## 9. Conclusion

### 9.1 Summary

SWTSharp has a **solid foundation** with three layouts already implemented:
- ✅ FillLayout (simple, well-implemented)
- ✅ RowLayout (flexible, handles wrapping)
- ✅ GridLayout (powerful, sophisticated caching)

**Remaining Work:**
- 🔨 StackLayout (2-3 days, LOW risk)
- 🔨 FormLayout (12-17 days, HIGH complexity)
- 🔨 Infrastructure cleanup (2-3 days)

**Total Estimated Effort:** 16-23 days for complete implementation

### 9.2 Key Recommendations

1. **Immediate Actions:**
   - Consolidate Composite classes
   - Add resize event wiring
   - Create comprehensive tests for existing layouts

2. **Implementation Priority:**
   - Phase 1: Cleanup (critical for stability)
   - Phase 2: StackLayout (quick win, low risk)
   - Phase 3-4: FormLayout (high value, allocate sufficient time)
   - Phase 5: Documentation (essential for adoption)

3. **Risk Management:**
   - Start with FormAttachment and dependency algorithms
   - Extensive unit testing before integration
   - Create sample applications for validation
   - Plan for visual regression testing

4. **Performance:**
   - Profile layout computation in realistic scenarios
   - Implement caching consistently
   - Add debouncing for resize events
   - Monitor layout frequency

### 9.3 Success Criteria

**Functional:**
- All 5 layouts implemented and tested
- No circular dependency crashes
- Correct size computation
- Proper space distribution
- Platform-consistent behavior

**Performance:**
- Layout computation < 16ms (60 FPS threshold)
- Memory overhead < 1KB per composite
- No layout thrashing
- Efficient caching

**Developer Experience:**
- Clear API documentation
- Migration guide from manual positioning
- Sample application gallery
- Debugging tools

**Quality:**
- 90%+ test coverage
- Zero known layout bugs
- Performance benchmarks
- Platform compatibility verified

---

## 10. References

### 10.1 Java SWT Documentation

- Eclipse SWT API Reference: https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/
- Understanding Layouts in SWT: https://www.eclipse.org/articles/Article-Understanding-Layouts/
- FormLayout API: https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/layout/FormLayout.html
- StackLayout API: https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/custom/StackLayout.html

### 10.2 SWTSharp Files Referenced

- `/src/SWTSharp/Layout/Layout.cs`
- `/src/SWTSharp/Layout/FillLayout.cs`
- `/src/SWTSharp/Layout/RowLayout.cs`
- `/src/SWTSharp/Layout/GridLayout.cs`
- `/src/SWTSharp/Layout/Composite.cs`
- `/src/SWTSharp/Composite.cs`
- `/src/SWTSharp/Control.cs`

### 10.3 Design Patterns

- **Strategy Pattern:** Layout algorithms encapsulated in Layout classes
- **Data Transfer Object:** LayoutData classes carry constraints
- **Template Method:** Base Layout class defines algorithm structure
- **Observer Pattern:** Resize events trigger layout recalculation
- **Flyweight Pattern:** Shared layout instances across composites

---

**Document Prepared By:** System Architecture Designer
**Review Status:** Ready for Technical Review
**Next Steps:** Stakeholder approval, begin Phase 1 implementation
