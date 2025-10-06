# SWTSharp Layout Manager Visual Reference

**Document Type:** Visual Diagrams and Reference
**Date:** October 6, 2025

## Layout Class Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                      Layout (Abstract)                       │
├─────────────────────────────────────────────────────────────┤
│  + ComputeSize(composite, wHint, hHint, flushCache) → Point │
│  + DoLayout(composite, flushCache) → bool                   │
│  # FlushCache(control?) → bool                              │
└─────────────────────────────────────────────────────────────┘
                            △
                            │
            ┌───────────────┼───────────────┬────────────┐
            │               │               │            │
┌───────────▼────────┐ ┌────▼────────┐ ┌───▼──────┐ ┌──▼───────┐
│   FillLayout ✅    │ │ RowLayout ✅│ │GridLayout│ │FormLayout│
├────────────────────┤ ├─────────────┤ │    ✅    │ │   🔨    │
│ + Type             │ │ + Type      │ ├──────────┤ ├──────────┤
│ + MarginWidth      │ │ + Wrap      │ │+NumCols  │ │+Margins  │
│ + MarginHeight     │ │ + Pack      │ │+EqualW   │ │+Spacing  │
│ + Spacing          │ │ + Fill      │ │+Margins  │ │          │
└────────────────────┘ │ + Center    │ │+Spacing  │ └──────────┘
                       │ + Justify   │ └──────────┘
                       │ + Margins   │
                       └─────────────┘

        ┌──────────────────────┐
        │  StackLayout 🔨      │
        │  (Custom namespace)  │
        ├──────────────────────┤
        │ + TopControl         │
        │ + MarginWidth        │
        │ + MarginHeight       │
        └──────────────────────┘
```

---

## Layout Data Classes

```
┌─────────────────────────────────────────────────────────────┐
│                   Control.LayoutData                         │
│                    (object property)                         │
└─────────────────────────────────────────────────────────────┘
                            △
                            │
        ┌───────────────────┼───────────────┬────────────┐
        │                   │               │            │
┌───────▼────────┐  ┌───────▼──────┐  ┌────▼──────┐  ┌─▼──────┐
│   (no data)    │  │  RowData ✅  │  │ GridData  │  │FormData│
│  FillLayout    │  ├──────────────┤  │    ✅     │  │  🔨   │
│  uses nothing  │  │ + Width      │  ├───────────┤  ├────────┤
└────────────────┘  │ + Height     │  │+HSpan     │  │+Left   │
                    │ + Exclude    │  │+VSpan     │  │+Right  │
                    └──────────────┘  │+HAlign    │  │+Top    │
                                      │+VAlign    │  │+Bottom │
                                      │+GrabH     │  │+Width  │
                                      │+GrabV     │  │+Height │
                                      │+WidthHint │  │+Exclude│
                                      │+HeightHint│  └────────┘
                                      │+MinWidth  │
                                      │+MinHeight │
                                      │+HIndent   │
                                      │+VIndent   │
                                      │+Exclude   │
                                      └───────────┘
```

---

## FillLayout Visual Examples

### Horizontal Fill
```
┌─────────────────────────────────────────────────────┐
│  Composite (FillLayout - Horizontal)                │
├─────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐  │
│  │   Button    │ │   Button    │ │   Button    │  │
│  │             │ │             │ │             │  │
│  │      1      │ │      2      │ │      3      │  │
│  │             │ │             │ │             │  │
│  └─────────────┘ └─────────────┘ └─────────────┘  │
└─────────────────────────────────────────────────────┘
     Equal Width      Equal Width      Equal Width
```

### Vertical Fill
```
┌──────────────────┐
│  Composite       │
│  (FillLayout)    │
│  Vertical        │
├──────────────────┤
│  ┌────────────┐  │
│  │  Button 1  │  │  Equal
│  └────────────┘  │  Height
│  ┌────────────┐  │
│  │  Button 2  │  │  Equal
│  └────────────┘  │  Height
│  ┌────────────┐  │
│  │  Button 3  │  │  Equal
│  └────────────┘  │  Height
└──────────────────┘
```

---

## RowLayout Visual Examples

### Horizontal with Wrapping
```
┌────────────────────────────────────────────────────────┐
│  Composite (RowLayout - Horizontal, Wrap=true)         │
├────────────────────────────────────────────────────────┤
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐   │
│  │Btn 1 │  │Btn 2 │  │Btn 3 │  │Btn 4 │  │Btn 5 │   │← Row 1
│  └──────┘  └──────┘  └──────┘  └──────┘  └──────┘   │
│                                                        │
│  ┌──────┐  ┌──────┐  ┌──────┐                        │
│  │Btn 6 │  │Btn 7 │  │Btn 8 │                        │← Row 2
│  └──────┘  └──────┘  └──────┘                        │
│                                                        │
│  ┌──────┐                                             │
│  │Btn 9 │                                             │← Row 3
│  └──────┘                                             │
└────────────────────────────────────────────────────────┘
```

### Vertical with Fill
```
┌────────────────────────────┐
│  Composite (RowLayout)     │
│  Vertical, Fill=true       │
├────────────────────────────┤
│  ┌──────────────────────┐  │
│  │      Button 1        │  │← Fills width
│  └──────────────────────┘  │
│  ┌──────────────────────┐  │
│  │      Button 2        │  │← Fills width
│  └──────────────────────┘  │
│  ┌──────────────────────┐  │
│  │      Button 3        │  │← Fills width
│  └──────────────────────┘  │
└────────────────────────────┘
```

---

## GridLayout Visual Examples

### Simple 3-Column Grid
```
┌─────────────────────────────────────────────────────────┐
│  Composite (GridLayout - NumColumns=3)                  │
├─────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │  Btn 1   │  │  Btn 2   │  │  Btn 3   │             │← Row 0
│  │ (0,0)    │  │ (0,1)    │  │ (0,2)    │             │
│  └──────────┘  └──────────┘  └──────────┘             │
│                                                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐             │
│  │  Btn 4   │  │  Btn 5   │  │  Btn 6   │             │← Row 1
│  │ (1,0)    │  │ (1,1)    │  │ (1,2)    │             │
│  └──────────┘  └──────────┘  └──────────┘             │
└─────────────────────────────────────────────────────────┘
```

### Grid with Spanning
```
┌──────────────────────────────────────────────────────────┐
│  Composite (GridLayout - NumColumns=3)                   │
├──────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────┐  ┌──────────┐  │
│  │         Title (HSpan=2)             │  │  Button  │  │← Row 0
│  │         (0,0) → (0,1)               │  │  (0,2)   │  │
│  └─────────────────────────────────────┘  └──────────┘  │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│  │  Field1  │  │  Field2  │  │  Field3  │              │← Row 1
│  │  (1,0)   │  │  (1,1)   │  │  (1,2)   │              │
│  └──────────┘  └──────────┘  └──────────┘              │
└──────────────────────────────────────────────────────────┘
```

### Grid with Space Grabbing
```
┌─────────────────────────────────────────────────────────────┐
│  Composite (GridLayout - NumColumns=2)                      │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────────────────────────────────┐    │
│  │  Label   │  │  Text Field (GrabH=true)             │    │
│  │          │  │  ←─────── Grabs Excess Space ───────→│    │
│  └──────────┘  └──────────────────────────────────────┘    │
│                                                             │
│  ┌──────────┐  ┌──────────────────────────────────────┐    │
│  │  Label   │  │  Text Area (GrabH=true, GrabV=true)  │    │
│  │          │  │                                       │    │
│  │          │  │  ←─────── Grabs Horizontal ─────────→│    │
│  │          │  │                                       │    │
│  │          │  │           ↕ Grabs Vertical ↕         │    │
│  └──────────┘  └──────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

---

## FormLayout Visual Examples

### Percentage-Based Positioning
```
┌──────────────────────────────────────────────────────────┐
│  Composite (FormLayout)                                  │
├──────────────────────────────────────────────────────────┤
│  0%                      50%                      100%   │
│  ┌────────────────────────┐ ┌──────────────────────┐    │
│  │  Left Panel            │ │  Right Panel         │    │
│  │  Left: 0%, Right: 50%  │ │  Left: 50%, Right:100%│   │
│  │                        │ │                      │    │
│  │                        │ │                      │    │
│  └────────────────────────┘ └──────────────────────┘    │
└──────────────────────────────────────────────────────────┘

FormData left = new FormData();
left.Left = new FormAttachment(0, 0);    // 0% from left
left.Right = new FormAttachment(50, 0);  // 50% from left
left.Top = new FormAttachment(0, 0);
left.Bottom = new FormAttachment(100, 0);

FormData right = new FormData();
right.Left = new FormAttachment(50, 0);  // 50% from left
right.Right = new FormAttachment(100, 0); // 100% from left
right.Top = new FormAttachment(0, 0);
right.Bottom = new FormAttachment(100, 0);
```

### Control-Relative Positioning
```
┌──────────────────────────────────────────────────────────┐
│  Composite (FormLayout)                                  │
├──────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────┐   │
│  │  Button 1                                        │   │
│  └──────────────────────────────────────────────────┘   │
│         ↓ 5px spacing                                    │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Button 2 (Top: button1 + 5px)                   │   │
│  └──────────────────────────────────────────────────┘   │
│         ↓ 5px spacing                                    │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Button 3 (Top: button2 + 5px)                   │   │
│  └──────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────┘

FormData data2 = new FormData();
data2.Left = new FormAttachment(0, 10);
data2.Right = new FormAttachment(100, -10);
data2.Top = new FormAttachment(button1, 5);  // 5px below button1
```

### Master-Detail Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Composite (FormLayout)                                     │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────┐ │ ┌───────────────────────────────────┐  │
│  │              │ │ │                                   │  │
│  │   Master     │ │ │         Detail                    │  │
│  │   List       │ │ │         Panel                     │  │
│  │              │ │ │                                   │  │
│  │   30% width  │ │ │         70% width                 │  │
│  │              │ │ │                                   │  │
│  │              │ │ │                                   │  │
│  └──────────────┘ │ └───────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                   ↑ 5px spacing

FormData masterData = new FormData();
masterData.Left = new FormAttachment(0, 0);
masterData.Right = new FormAttachment(30, 0);  // 30%
masterData.Top = new FormAttachment(0, 0);
masterData.Bottom = new FormAttachment(100, 0);

FormData detailData = new FormData();
detailData.Left = new FormAttachment(master, 5);  // 5px from master
detailData.Right = new FormAttachment(100, 0);
detailData.Top = new FormAttachment(0, 0);
detailData.Bottom = new FormAttachment(100, 0);
```

---

## StackLayout Visual Examples

### Card Switching
```
Initial State (topControl = page1):
┌──────────────────────────────────┐
│  Composite (StackLayout)         │
├──────────────────────────────────┤
│  ┌────────────────────────────┐  │
│  │   Page 1 (VISIBLE)         │  │
│  │                            │  │
│  │   [Content of page 1]      │  │
│  │                            │  │
│  └────────────────────────────┘  │
│                                  │
│  (Page 2 - HIDDEN)               │
│  (Page 3 - HIDDEN)               │
└──────────────────────────────────┘

After: layout.TopControl = page2; parent.Layout();
┌──────────────────────────────────┐
│  Composite (StackLayout)         │
├──────────────────────────────────┤
│  ┌────────────────────────────┐  │
│  │   Page 2 (VISIBLE)         │  │
│  │                            │  │
│  │   [Content of page 2]      │  │
│  │                            │  │
│  └────────────────────────────┘  │
│                                  │
│  (Page 1 - HIDDEN)               │
│  (Page 3 - HIDDEN)               │
└──────────────────────────────────┘
```

### Wizard Pattern
```
┌───────────────────────────────────────────────────────┐
│  Wizard Dialog                                        │
├───────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────┐  │
│  │  Content Area (StackLayout)                     │  │
│  │  ┌───────────────────────────────────────────┐  │  │
│  │  │  Wizard Page 1 (topControl)               │  │  │
│  │  │                                           │  │  │
│  │  │  Name: [__________________________]       │  │  │
│  │  │  Email: [_________________________]       │  │  │
│  │  │                                           │  │  │
│  │  └───────────────────────────────────────────┘  │  │
│  └─────────────────────────────────────────────────┘  │
│                                                       │
│  ┌─────────────────────────────────────────────────┐  │
│  │  Button Bar                                     │  │
│  │  [< Back]              [Next >]     [Cancel]    │  │
│  └─────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────┘

"Next" button: layout.TopControl = page2; contentArea.Layout();
```

---

## FormLayout Dependency Graph Example

### Valid DAG (Directed Acyclic Graph)
```
Control Layout Dependencies:

button1: Left → 0%, Top → 0%
            │
            │ (button2 depends on button1)
            ▼
button2: Left → 0%, Top → button1 + 5px
            │
            │ (button3 depends on button2)
            ▼
button3: Left → 0%, Top → button2 + 5px

Dependency Graph:
┌─────────┐
│ button1 │
└────┬────┘
     │
     ▼
┌─────────┐
│ button2 │
└────┬────┘
     │
     ▼
┌─────────┐
│ button3 │
└─────────┘

✅ VALID: Acyclic, can be laid out in order
```

### Invalid Circular Dependency
```
Control Layout Dependencies:

button1: Left → button2.Right + 5px  ─┐
                                      │
                                      │ CIRCULAR!
                                      │
button2: Left → button1.Right + 5px  ─┘

Dependency Graph:
┌─────────┐       ┌─────────┐
│ button1 │ ────→ │ button2 │
└─────────┘       └────┬────┘
     ▲                 │
     │                 │
     └─────────────────┘

❌ INVALID: Circular dependency detected!
Throws: SWTException("Circular dependency in FormLayout")
```

---

## Layout Algorithm Flowcharts

### ComputeSize Algorithm
```
┌─────────────────────────┐
│  ComputeSize(wHint,     │
│  hHint, flushCache)     │
└───────────┬─────────────┘
            │
            ▼
    ┌───────────────┐
    │ Cache valid?  │
    └───────┬───────┘
            │
      Yes ──┼── No
            │       │
            │       ▼
            │   ┌────────────────────┐
            │   │ Get children list  │
            │   └─────────┬──────────┘
            │             │
            │             ▼
            │   ┌─────────────────────┐
            │   │ Calculate preferred │
            │   │ size for each child │
            │   └─────────┬───────────┘
            │             │
            │             ▼
            │   ┌─────────────────────┐
            │   │ Apply layout logic  │
            │   │ (sum, max, grid)    │
            │   └─────────┬───────────┘
            │             │
            │             ▼
            │   ┌─────────────────────┐
            │   │ Add margins/spacing │
            │   └─────────┬───────────┘
            │             │
            │             ▼
            │   ┌─────────────────────┐
            │   │   Cache result      │
            │   └─────────┬───────────┘
            │             │
            └─────────────┘
                          │
                          ▼
                ┌─────────────────┐
                │  Return Point   │
                │  (width, height)│
                └─────────────────┘
```

### DoLayout Algorithm
```
┌─────────────────────────┐
│  DoLayout(composite,    │
│  flushCache)            │
└───────────┬─────────────┘
            │
            ▼
    ┌───────────────┐
    │ Flush cache?  │
    └───────┬───────┘
            │
        Yes ├── No
            │       │
            ▼       │
    ┌──────────────┐│
    │ Clear caches ││
    └──────┬───────┘│
           │        │
           └────────┘
                    │
                    ▼
        ┌────────────────────┐
        │ Get client area    │
        │ (available space)  │
        └─────────┬──────────┘
                  │
                  ▼
        ┌────────────────────┐
        │ Calculate positions│
        │ and sizes for each │
        │ child control      │
        └─────────┬──────────┘
                  │
                  ▼
        ┌────────────────────┐
        │ Apply alignment    │
        │ and constraints    │
        └─────────┬──────────┘
                  │
                  ▼
        ┌────────────────────┐
        │ SetBounds() for    │
        │ each child         │
        └─────────┬──────────┘
                  │
                  ▼
           ┌──────────┐
           │Return true│
           └──────────┘
```

### FormLayout Topological Sort
```
┌─────────────────────────┐
│ Build Dependency Graph  │
└───────────┬─────────────┘
            │
            ▼
    ┌───────────────────┐
    │ Detect Cycles?    │
    └───────┬───────────┘
            │
        Cycle ├── No Cycle
            │           │
            ▼           │
    ┌──────────────┐    │
    │ Throw        │    │
    │ SWTException │    │
    └──────────────┘    │
                        │
                        ▼
            ┌───────────────────────┐
            │ Topological Sort (DFS)│
            └───────────┬───────────┘
                        │
                        ▼
            ┌───────────────────────┐
            │ Get sorted order      │
            └───────────┬───────────┘
                        │
                        ▼
            ┌───────────────────────┐
            │ Layout in sorted order│
            └───────────┬───────────┘
                        │
                        ▼
                  ┌──────────┐
                  │  Done    │
                  └──────────┘
```

---

## Performance Comparison Chart

```
Layout Computation Time (Big-O Notation):

FillLayout:    O(n)        ████
RowLayout:     O(n)        ████
GridLayout:    O(r×c)      ██████
FormLayout:    O(n×d)      ████████
StackLayout:   O(n)        ████

Memory Usage (bytes per instance):

FillLayout:    ~64         ██
RowLayout:     ~128        ████
GridLayout:    ~256+cache  ██████████
FormLayout:    ~512+graph  ████████████████
StackLayout:   ~64         ██

Legend:
n = number of children
r×c = grid rows × columns
d = dependency depth
```

---

## Common Layout Patterns

### Dialog Layout (GridLayout)
```
┌──────────────────────────────────────────────┐
│  Dialog Window                               │
├──────────────────────────────────────────────┤
│  ┌────────────────────────────────────────┐  │
│  │                                        │  │
│  │         Content Area                   │  │ ← GrabV=true
│  │         (Composite)                    │  │
│  │                                        │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  ┌────────────────────────────────────────┐  │
│  │  Button Bar          [OK] [Cancel]     │  │ ← GrabV=false
│  └────────────────────────────────────────┘  │
└──────────────────────────────────────────────┘

GridLayout(1, false)
- Content: GridData(FILL, FILL, true, true)
- Buttons: GridData(END, CENTER, false, false)
```

### Form Layout (GridLayout 2 columns)
```
┌──────────────────────────────────────────────┐
│  Form (GridLayout - NumColumns=2)            │
├──────────────────────────────────────────────┤
│  ┌──────┐  ┌────────────────────────────┐   │
│  │ Name:│  │ [_____________________]    │   │
│  └──────┘  └────────────────────────────┘   │
│                                              │
│  ┌──────┐  ┌────────────────────────────┐   │
│  │Email:│  │ [_____________________]    │   │
│  └──────┘  └────────────────────────────┘   │
│                                              │
│  ┌──────┐  ┌────────────────────────────┐   │
│  │ Age: │  │ [_____________________]    │   │
│  └──────┘  └────────────────────────────┘   │
└──────────────────────────────────────────────┘

Label: GridData(END, CENTER, false, false)
Input: GridData(FILL, CENTER, true, false)
```

---

## Implementation Status Summary

```
┌───────────────────────────────────────────────────────────┐
│  Layout Implementation Status                             │
├───────────────────────────────────────────────────────────┤
│                                                           │
│  ✅ FillLayout        [████████████████████] 100%        │
│  ✅ RowLayout         [████████████████████] 100%        │
│  ✅ GridLayout        [████████████████████] 100%        │
│  🔨 FormLayout        [░░░░░░░░░░░░░░░░░░░░]   0%        │
│  🔨 StackLayout       [░░░░░░░░░░░░░░░░░░░░]   0%        │
│                                                           │
│  Overall Progress:    [████████████░░░░░░░░]  60%        │
│                                                           │
│  ✅ = Implemented and tested                             │
│  🔨 = Pending implementation                             │
└───────────────────────────────────────────────────────────┘

Next Steps:
1. Infrastructure cleanup (Composite consolidation)
2. StackLayout implementation (2-3 days)
3. FormLayout foundation (5-7 days)
4. FormLayout implementation (7-10 days)
5. Documentation and examples (3-5 days)

Total: 19-28 days to 100% completion
```

---

**Document Purpose:** Quick visual reference for layout behaviors and patterns
**Related Documents:**
- `Layout-Manager-Architecture.md` - Full technical design
- `Layout-Manager-Summary.md` - Executive summary
