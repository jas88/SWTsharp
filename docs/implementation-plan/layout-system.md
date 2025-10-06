# SWT Layout Management System - Implementation Research

## Executive Summary

This document provides comprehensive research on Eclipse SWT's layout management system, detailing the architecture, algorithms, and implementation patterns needed to port the layout system to SWTSharp.

## Table of Contents

1. [Layout System Architecture](#layout-system-architecture)
2. [Layout Base Class and Interface](#layout-base-class-and-interface)
3. [Layout Computation Algorithm](#layout-computation-algorithm)
4. [Layout Managers](#layout-managers)
5. [Layout Data Classes](#layout-data-classes)
6. [Composite Layout Integration](#composite-layout-integration)
7. [Optimization and Caching](#optimization-and-caching)
8. [Platform-Specific Considerations](#platform-specific-considerations)

---

## 1. Layout System Architecture

### Overview

SWT's layout system provides automatic positioning and sizing of child controls within container (Composite) widgets. The system uses a **two-pass algorithm**:
1. **computeSize()** - Calculate preferred/minimum sizes
2. **layout()** - Position and size controls based on available space

### Core Components

```
org.eclipse.swt.widgets.Composite
    ↓ has-a
org.eclipse.swt.widgets.Layout (abstract base class)
    ↓ implemented by
├── FillLayout
├── RowLayout
├── GridLayout
├── FormLayout
└── StackLayout

Layout Data (attached to controls):
├── RowData
├── GridData
├── FormData
└── (no data for FillLayout/StackLayout)
```

### Design Principles

1. **Separation of Concerns**: Layout logic separated from widget implementation
2. **Pluggable Architecture**: Composites can use any layout manager
3. **Lazy Evaluation**: Layout computed only when needed
4. **Data-Driven**: Controls carry layout data (hints) for their parent's layout
5. **Recursive**: Layouts can trigger child layouts

---

## 2. Layout Base Class and Interface

### Layout Abstract Class

```java
package org.eclipse.swt.widgets;

public abstract class Layout {

    /**
     * Computes and returns the preferred size of the composite based on
     * the layout, taking into account all child controls and their layout data.
     *
     * @param composite the composite to calculate size for
     * @param wHint width hint (or SWT.DEFAULT)
     * @param hHint height hint (or SWT.DEFAULT)
     * @param flushCache if true, cached layout information should be recalculated
     * @return the preferred size (Point with width and height)
     */
    protected abstract Point computeSize(Composite composite, int wHint,
                                        int hHint, boolean flushCache);

    /**
     * Positions and sizes the children of the composite according to the
     * layout algorithm and layout data attached to children.
     *
     * @param composite the composite to lay out
     * @param flushCache if true, cached layout information should be recalculated
     */
    protected abstract boolean layout(Composite composite, boolean flushCache);

    /**
     * Returns layout-specific data for serialization/debugging.
     * Optional override.
     */
    protected boolean flushCache(Control control) {
        return true; // Default: always recalculate
    }
}
```

### Key Concepts

**wHint / hHint (Width/Height Hints)**
- `SWT.DEFAULT` (-1): Calculate natural/preferred size
- Positive value: Constrain dimension and calculate complementary dimension
- Example: `wHint=300, hHint=DEFAULT` means "calculate height needed for 300px width"

**flushCache Parameter**
- `true`: Discard cached measurements, recalculate everything
- `false`: Reuse cached computations if available (optimization)
- Triggered by: control addition/removal, size changes, layout data changes

---

## 3. Layout Computation Algorithm

### Two-Phase Layout Process

#### Phase 1: Size Computation (Bottom-Up)

```
1. computeSize(composite, wHint, hHint, flushCache)
   ↓
2. For each child control:
   a. Get child's layoutData
   b. Query child.computeSize(childWHint, childHHint) recursively
   c. Apply layout data constraints/hints
   ↓
3. Apply layout algorithm's spacing/margin rules
   ↓
4. Return aggregate size as Point(width, height)
```

**Example (GridLayout computeSize):**

```java
protected Point computeSize(Composite composite, int wHint, int hHint,
                           boolean flushCache) {
    // Get grid structure (rows x columns)
    GridData[][] grid = buildGrid(composite);

    // Calculate column widths
    int[] widths = computeColumnWidths(grid, wHint);

    // Calculate row heights
    int[] heights = computeRowHeights(grid, hHint);

    // Sum with margins and spacing
    int totalWidth = sum(widths) + marginWidth * 2 +
                     horizontalSpacing * (numColumns - 1);
    int totalHeight = sum(heights) + marginHeight * 2 +
                      verticalSpacing * (numRows - 1);

    return new Point(totalWidth, totalHeight);
}
```

#### Phase 2: Layout Positioning (Top-Down)

```
1. layout(composite, flushCache)
   ↓
2. Get composite's client area (available space)
   ↓
3. For each child control:
   a. Calculate position (x, y)
   b. Calculate size (width, height)
   c. Apply layout data alignment/grab/span rules
   d. Call child.setBounds(x, y, width, height)
   ↓
4. Recursively trigger child.layout() if child is Composite
   ↓
5. Return true if layout succeeded
```

**Example (GridLayout layout):**

```java
protected boolean layout(Composite composite, boolean flushCache) {
    Rectangle clientArea = composite.getClientArea();

    // Build grid structure
    GridData[][] grid = buildGrid(composite);

    // Distribute available space
    int[] widths = distributeWidth(clientArea.width, grid);
    int[] heights = distributeHeight(clientArea.height, grid);

    // Position each control
    int y = clientArea.y + marginHeight;
    for (int row = 0; row < numRows; row++) {
        int x = clientArea.x + marginWidth;

        for (int col = 0; col < numColumns; col++) {
            Control child = grid[row][col].control;
            if (child != null) {
                int w = calculateSpannedWidth(col, grid[row][col].horizontalSpan, widths);
                int h = calculateSpannedHeight(row, grid[row][col].verticalSpan, heights);

                // Apply alignment
                int childX = x, childY = y;
                int childW = w, childH = h;
                applyAlignment(grid[row][col], x, y, w, h,
                              ref childX, ref childY, ref childW, ref childH);

                child.setBounds(childX, childY, childW, childH);
            }
            x += widths[col] + horizontalSpacing;
        }
        y += heights[row] + verticalSpacing;
    }

    return true;
}
```

### Control Size Calculation

When a layout calls `control.computeSize(wHint, hHint)`:

```java
// In Control class
public Point computeSize(int wHint, int hHint, boolean changed) {
    if (this instanceof Composite) {
        Composite comp = (Composite)this;
        if (comp.layout != null) {
            // Recursively ask layout to compute size
            return comp.layout.computeSize(comp, wHint, hHint, changed);
        }
    }

    // For non-composite controls, query native widget
    return computeNativeSize(wHint, hHint);
}
```

---

## 4. Layout Managers

### 4.1 FillLayout

**Purpose**: Simplest layout - arranges controls in a single row or column, all sized equally.

**Algorithm**:
```
- Type: HORIZONTAL (default) or VERTICAL
- Spacing: gap between controls
- MarginWidth, MarginHeight: outer margins

ComputeSize:
  IF HORIZONTAL:
    width = sum(child preferred widths) + spacing * (n-1) + margins
    height = max(child preferred heights) + margins
  ELSE (VERTICAL):
    width = max(child preferred widths) + margins
    height = sum(child preferred heights) + spacing * (n-1) + margins

Layout:
  Calculate available space per child: space / numChildren
  Position controls sequentially with equal sizing
```

**Properties**:
```java
public class FillLayout extends Layout {
    public int type = SWT.HORIZONTAL;        // HORIZONTAL or VERTICAL
    public int marginWidth = 0;              // Left/right margin
    public int marginHeight = 0;             // Top/bottom margin
    public int spacing = 0;                  // Gap between controls
}
```

**No Layout Data**: FillLayout does not use layout data on children.

**Example**:
```java
// Horizontal buttons, all equal width
Composite parent = new Composite(shell, SWT.NONE);
FillLayout layout = new FillLayout(SWT.HORIZONTAL);
layout.spacing = 5;
parent.setLayout(layout);

new Button(parent, SWT.PUSH).setText("Button 1");
new Button(parent, SWT.PUSH).setText("Button 2");
new Button(parent, SWT.PUSH).setText("Button 3");
// All buttons get equal width: (clientWidth - 2*spacing) / 3
```

---

### 4.2 RowLayout

**Purpose**: Flows controls like text - wraps to next row/column when space runs out.

**Algorithm**:
```
- Type: HORIZONTAL (wrap rows) or VERTICAL (wrap columns)
- Wrap: enable/disable wrapping
- Pack: if true, controls sized to preferred; if false, all equal size
- Justify: if true, expand last row/column to fill space
- Fill: if true, controls expand perpendicular to flow direction

ComputeSize:
  Simulate layout flow with wrapping
  Track maximum row/column width/height
  Sum all row/column dimensions with spacing and margins

Layout:
  Position controls in flow direction
  Wrap when control exceeds available width/height
  Apply justification and fill rules
```

**Properties**:
```java
public class RowLayout extends Layout {
    public int type = SWT.HORIZONTAL;        // Flow direction
    public int marginWidth = 3;
    public int marginHeight = 3;
    public int marginLeft = 3;               // Individual margins (override marginWidth/Height)
    public int marginTop = 3;
    public int marginRight = 3;
    public int marginBottom = 3;
    public int spacing = 3;                  // Gap between controls

    public boolean wrap = true;              // Allow wrapping
    public boolean pack = true;              // Size to preferred vs equal
    public boolean fill = false;             // Expand perpendicular to flow
    public boolean center = false;           // Center in perpendicular direction
    public boolean justify = false;          // Expand last row/column
}
```

**Layout Data**:
```java
public class RowData {
    public int width = SWT.DEFAULT;          // Preferred width hint
    public int height = SWT.DEFAULT;         // Preferred height hint
    public boolean exclude = false;          // Exclude from layout

    public RowData() {}
    public RowData(int width, int height) {
        this.width = width;
        this.height = height;
    }
    public RowData(Point point) {
        this.width = point.x;
        this.height = point.y;
    }
}
```

**Example**:
```java
Composite parent = new Composite(shell, SWT.NONE);
RowLayout layout = new RowLayout();
layout.wrap = true;
layout.pack = false;
layout.justify = true;
parent.setLayout(layout);

Button b1 = new Button(parent, SWT.PUSH);
b1.setText("Button 1");

Button b2 = new Button(parent, SWT.PUSH);
b2.setText("Button 2");
b2.setLayoutData(new RowData(100, 40)); // Fixed size

Button b3 = new Button(parent, SWT.PUSH);
b3.setText("Button 3");
```

---

### 4.3 GridLayout

**Purpose**: Most powerful and common layout - arranges controls in a flexible grid with spanning, alignment, and grab options.

**Algorithm**:
```
- NumColumns: number of columns in grid
- Rows determined automatically by number of controls and spanning

Grid Structure:
  1. Build 2D array of grid cells
  2. Place each control based on column position and span
  3. Empty cells created automatically

ComputeSize:
  1. Calculate minimum/preferred width for each column
     - Consider controls with horizontalSpan=1
     - Factor in grabExcessHorizontalSpace
  2. Calculate minimum/preferred height for each row
     - Consider controls with verticalSpan=1
     - Factor in grabExcessVerticalSpace
  3. Distribute spanned controls' requirements
  4. Sum with margins and spacing

Layout:
  1. Distribute available width to columns
     - Minimum widths first
     - Remaining space to 'grab' columns proportionally
  2. Distribute available height to rows
     - Minimum heights first
     - Remaining space to 'grab' rows proportionally
  3. Position each control in its grid cell(s)
     - Apply horizontal/vertical alignment
     - Apply indents and hints
```

**Properties**:
```java
public class GridLayout extends Layout {
    public int numColumns = 1;               // Number of columns

    public boolean makeColumnsEqualWidth = false; // Force equal column widths

    public int marginWidth = 5;              // Outer margins (left+right)
    public int marginHeight = 5;             // Outer margins (top+bottom)
    public int marginLeft = 0;               // Override marginWidth
    public int marginTop = 0;                // Override marginHeight
    public int marginRight = 0;
    public int marginBottom = 0;

    public int horizontalSpacing = 5;        // Gap between columns
    public int verticalSpacing = 5;          // Gap between rows
}
```

**Layout Data** (Most Complex):
```java
public class GridData {
    // Alignment constants
    public static final int BEGINNING = 1;   // Align to left/top
    public static final int CENTER = 2;      // Center in cell
    public static final int END = 3;         // Align to right/bottom
    public static final int FILL = 4;        // Expand to fill cell

    // Grid position and span
    public int horizontalSpan = 1;           // Number of columns to span
    public int verticalSpan = 1;             // Number of rows to span

    // Alignment
    public int horizontalAlignment = BEGINNING;
    public int verticalAlignment = CENTER;

    // Grab excess space
    public boolean grabExcessHorizontalSpace = false; // Take extra horizontal space
    public boolean grabExcessVerticalSpace = false;   // Take extra vertical space

    // Size hints
    public int widthHint = SWT.DEFAULT;      // Preferred width
    public int heightHint = SWT.DEFAULT;     // Preferred height
    public int minimumWidth = 0;             // Minimum width constraint
    public int minimumHeight = 0;            // Minimum height constraint

    // Indentation
    public int horizontalIndent = 0;         // Left indent in pixels
    public int verticalIndent = 0;           // Top indent in pixels

    // Exclusion
    public boolean exclude = false;          // Exclude from layout

    // Constructors with common patterns
    public GridData() {}
    public GridData(int style) {
        // Style can combine: SWT.FILL, SWT.GRAB_HORIZONTAL, SWT.GRAB_VERTICAL
        // Example: new GridData(SWT.FILL | SWT.GRAB_HORIZONTAL)
    }
    public GridData(int horizontalAlignment, int verticalAlignment,
                   boolean grabExcessHorizontalSpace,
                   boolean grabExcessVerticalSpace) { ... }
}
```

**Common GridData Patterns**:
```java
// Fill horizontally and grab space
GridData gd1 = new GridData(SWT.FILL, SWT.CENTER, true, false);

// Fill both directions and grab both
GridData gd2 = new GridData(SWT.FILL, SWT.FILL, true, true);

// Span 2 columns
GridData gd3 = new GridData();
gd3.horizontalSpan = 2;

// Fixed size
GridData gd4 = new GridData();
gd4.widthHint = 200;
gd4.heightHint = 100;

// Minimum size with fill
GridData gd5 = new GridData(SWT.FILL, SWT.FILL, true, true);
gd5.minimumWidth = 100;
gd5.minimumHeight = 50;
```

**Grid Building Algorithm**:
```java
GridData[][] buildGrid(Composite composite) {
    Control[] children = composite.getChildren();

    // Count rows needed
    int row = 0, col = 0, maxRow = 0;
    for (Control child : children) {
        GridData data = (GridData)child.getLayoutData();
        if (data == null) data = new GridData(); // Default

        if (data.exclude) continue;

        // Handle spanning
        int colSpan = Math.max(1, data.horizontalSpan);
        int rowSpan = Math.max(1, data.verticalSpan);

        // Wrap to next row if needed
        if (col + colSpan > numColumns) {
            row++;
            col = 0;
        }

        // Place control in grid
        for (int r = row; r < row + rowSpan; r++) {
            for (int c = col; c < col + colSpan; c++) {
                grid[r][c] = new GridCell(child, data);
            }
        }

        maxRow = Math.max(maxRow, row + rowSpan - 1);
        col += colSpan;
    }

    return grid; // 2D array of [maxRow+1][numColumns]
}
```

**Example**:
```java
Composite parent = new Composite(shell, SWT.NONE);
parent.setLayout(new GridLayout(2, false)); // 2 columns, not equal width

Label label = new Label(parent, SWT.NONE);
label.setText("Name:");

Text text = new Text(parent, SWT.BORDER);
text.setLayoutData(new GridData(SWT.FILL, SWT.CENTER, true, false));

Button button = new Button(parent, SWT.PUSH);
button.setText("Submit");
GridData buttonData = new GridData();
buttonData.horizontalSpan = 2; // Span both columns
buttonData.horizontalAlignment = GridData.END;
button.setLayoutData(buttonData);
```

---

### 4.4 FormLayout

**Purpose**: Constraint-based layout using attachments to composite edges or other controls. Most flexible but complex.

**Algorithm**:
```
Each control specifies attachments for all 4 edges (left, top, right, bottom)
Attachments can be:
  - Percentage of composite (FormAttachment(numerator, denominator))
  - Offset from composite edge (FormAttachment(0, offset))
  - Relative to another control (FormAttachment(control, offset))
  - Combination (percentage + offset)

ComputeSize:
  1. Build dependency graph of control relationships
  2. Resolve attachment constraints (may require iteration)
  3. Calculate minimum bounding rectangle

Layout:
  1. Resolve all attachment constraints given client area
  2. Calculate absolute positions for each control
  3. Set control bounds
```

**Properties**:
```java
public class FormLayout extends Layout {
    public int marginWidth = 0;
    public int marginHeight = 0;
    public int marginLeft = 0;
    public int marginTop = 0;
    public int marginRight = 0;
    public int marginBottom = 0;
    public int spacing = 0;                  // Default spacing between controls
}
```

**Layout Data**:
```java
public class FormData {
    public int width = SWT.DEFAULT;          // Fixed width override
    public int height = SWT.DEFAULT;         // Fixed height override

    public FormAttachment left;              // Left edge attachment
    public FormAttachment right;             // Right edge attachment
    public FormAttachment top;               // Top edge attachment
    public FormAttachment bottom;            // Bottom edge attachment

    public FormData() {}
    public FormData(int width, int height) {
        this.width = width;
        this.height = height;
    }
}

public class FormAttachment {
    public Control control;                  // Control to attach to (null = composite)
    public int offset;                       // Pixel offset
    public int alignment = SWT.DEFAULT;      // Alignment (TOP, BOTTOM, LEFT, RIGHT, CENTER)

    // Percentage attachment
    public int numerator;                    // Percentage numerator
    public int denominator = 100;            // Percentage denominator

    // Constructors
    public FormAttachment(int numerator) {
        this(numerator, 100, 0);
    }
    public FormAttachment(int numerator, int offset) {
        this(numerator, 100, offset);
    }
    public FormAttachment(int numerator, int denominator, int offset) {
        this.numerator = numerator;
        this.denominator = denominator;
        this.offset = offset;
    }
    public FormAttachment(Control control) {
        this(control, 0, SWT.DEFAULT);
    }
    public FormAttachment(Control control, int offset) {
        this(control, offset, SWT.DEFAULT);
    }
    public FormAttachment(Control control, int offset, int alignment) {
        this.control = control;
        this.offset = offset;
        this.alignment = alignment;
    }
}
```

**Attachment Resolution**:
```java
int resolveAttachment(FormAttachment attachment, int dimension,
                      Rectangle clientArea, boolean isWidth) {
    if (attachment == null) return 0;

    int position;

    if (attachment.control != null) {
        // Relative to another control
        Rectangle bounds = attachment.control.getBounds();

        switch (attachment.alignment) {
            case SWT.LEFT:
            case SWT.TOP:
                position = isWidth ? bounds.x : bounds.y;
                break;
            case SWT.RIGHT:
            case SWT.BOTTOM:
                position = isWidth ? (bounds.x + bounds.width) : (bounds.y + bounds.height);
                break;
            case SWT.CENTER:
                position = isWidth ? (bounds.x + bounds.width / 2) : (bounds.y + bounds.height / 2);
                break;
            default:
                // Default: attach to opposite edge
                position = isWidth ? (bounds.x + bounds.width) : (bounds.y + bounds.height);
        }
    } else {
        // Percentage of composite
        int base = isWidth ? clientArea.width : clientArea.height;
        position = (base * attachment.numerator / attachment.denominator);
    }

    return position + attachment.offset;
}
```

**Example**:
```java
Composite parent = new Composite(shell, SWT.NONE);
FormLayout layout = new FormLayout();
layout.marginWidth = 10;
layout.marginHeight = 10;
parent.setLayout(layout);

// Button in top-left corner
Button b1 = new Button(parent, SWT.PUSH);
b1.setText("Top Left");
FormData fd1 = new FormData();
fd1.top = new FormAttachment(0, 0);    // 0% from top + 0 offset
fd1.left = new FormAttachment(0, 0);   // 0% from left + 0 offset
b1.setLayoutData(fd1);

// Button in top-right corner
Button b2 = new Button(parent, SWT.PUSH);
b2.setText("Top Right");
FormData fd2 = new FormData();
fd2.top = new FormAttachment(0, 0);
fd2.right = new FormAttachment(100, 0); // 100% from left = right edge
b2.setLayoutData(fd2);

// Text field below b1, spanning to b2
Text text = new Text(parent, SWT.BORDER);
FormData fdText = new FormData();
fdText.top = new FormAttachment(b1, 5);      // 5px below b1
fdText.left = new FormAttachment(0, 0);
fdText.right = new FormAttachment(100, 0);
text.setLayoutData(fdText);

// Centered button at 50% width
Button b3 = new Button(parent, SWT.PUSH);
b3.setText("Center");
FormData fd3 = new FormData();
fd3.top = new FormAttachment(text, 10);
fd3.left = new FormAttachment(50, -40);  // 50% - 40px (half button width)
fd3.width = 80;
b3.setLayoutData(fd3);
```

---

### 4.5 StackLayout

**Purpose**: Shows only one control at a time (like a stack of cards). Used for wizards, tabbed interfaces, etc.

**Algorithm**:
```
- TopControl: reference to currently visible control

ComputeSize:
  Return max(preferred sizes of all children)

Layout:
  Hide all controls except topControl
  Size topControl to fill entire client area
```

**Properties**:
```java
public class StackLayout extends Layout {
    public Control topControl;               // Currently visible control

    public int marginWidth = 0;
    public int marginHeight = 0;
}
```

**No Layout Data**: StackLayout does not use layout data.

**Example**:
```java
Composite parent = new Composite(shell, SWT.NONE);
StackLayout layout = new StackLayout();
parent.setLayout(layout);

Composite page1 = new Composite(parent, SWT.NONE);
// ... add controls to page1 ...

Composite page2 = new Composite(parent, SWT.NONE);
// ... add controls to page2 ...

layout.topControl = page1; // Show page1 initially
parent.layout();

// Switch to page2
layout.topControl = page2;
parent.layout();
```

---

## 5. Layout Data Classes

### Overview

Layout data objects are attached to **child controls** to provide hints/constraints to the **parent's layout manager**.

```java
// Pattern
Control child = new Control(parent, SWT.NONE);
LayoutData data = new LayoutData(...);
child.setLayoutData(data);
```

### Storage and Retrieval

```java
// In Control class
private Object layoutData;

public void setLayoutData(Object layoutData) {
    checkWidget();
    this.layoutData = layoutData;

    // Trigger parent layout recalculation
    if (parent != null && parent.layout != null) {
        parent.layout(true); // flushCache = true
    }
}

public Object getLayoutData() {
    checkWidget();
    return layoutData;
}
```

### Type Safety

Layout managers cast layoutData to expected type:

```java
// In GridLayout.layout()
for (Control child : composite.getChildren()) {
    Object data = child.getLayoutData();
    GridData gridData;

    if (data == null || !(data instanceof GridData)) {
        // Use default GridData
        gridData = new GridData();
    } else {
        gridData = (GridData)data;
    }

    // Use gridData...
}
```

### Summary Table

| Layout        | Layout Data Class | Required? | Key Properties |
|---------------|-------------------|-----------|----------------|
| FillLayout    | (none)           | No        | N/A |
| RowLayout     | RowData          | Optional  | width, height, exclude |
| GridLayout    | GridData         | Optional  | alignment, grab, span, hints, indent |
| FormLayout    | FormData         | Required  | attachments (left, right, top, bottom), width, height |
| StackLayout   | (none)           | No        | N/A |

---

## 6. Composite Layout Integration

### Composite Class Extensions

```java
package org.eclipse.swt.widgets;

public class Composite extends Scrollable {

    private Layout layout;
    private Control[] children = new Control[0];

    /**
     * Sets the layout which manages the size and position of children.
     */
    public void setLayout(Layout layout) {
        checkWidget();
        this.layout = layout;
        // Trigger immediate layout
        layout(true);
    }

    /**
     * Gets the current layout.
     */
    public Layout getLayout() {
        checkWidget();
        return layout;
    }

    /**
     * Forces layout of children. Called automatically on resize,
     * or manually when layout data changes.
     *
     * @param changed if true, flush cached layout data
     */
    public void layout(boolean changed) {
        checkWidget();
        if (layout == null) return;

        layout.layout(this, changed);

        // Recursively layout children if they are composites
        for (Control child : children) {
            if (child instanceof Composite) {
                ((Composite)child).layout(changed);
            }
        }
    }

    /**
     * Forces layout and optionally propagates to all descendants.
     *
     * @param changed if true, flush cached layout data
     * @param all if true, recursively layout all descendant composites
     */
    public void layout(boolean changed, boolean all) {
        checkWidget();
        if (layout == null) return;

        layout.layout(this, changed);

        if (all) {
            for (Control child : children) {
                if (child instanceof Composite) {
                    ((Composite)child).layout(changed, all);
                }
            }
        }
    }

    /**
     * Computes preferred size of composite based on layout.
     */
    @Override
    public Point computeSize(int wHint, int hHint, boolean changed) {
        checkWidget();

        if (layout != null) {
            Point size = layout.computeSize(this, wHint, hHint, changed);
            if (size.x == 0) size.x = DEFAULT_WIDTH;
            if (size.y == 0) size.y = DEFAULT_HEIGHT;
            return size;
        }

        // No layout: return default size
        return new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
    }

    /**
     * Gets the client area (space available for children).
     * Excludes borders, scrollbars, etc.
     */
    public Rectangle getClientArea() {
        checkWidget();
        // Platform-specific implementation
        return getClientAreaNative();
    }

    /**
     * Gets array of child controls.
     */
    public Control[] getChildren() {
        checkWidget();
        return children.clone(); // Defensive copy
    }
}
```

### Automatic Layout Triggers

Layouts are automatically triggered when:

1. **Window Resize**: Shell/Composite resized
   ```java
   shell.addListener(SWT.Resize, event -> {
       shell.layout(false); // Don't flush cache, just reflow
   });
   ```

2. **Child Added/Removed**:
   ```java
   void addChild(Control child) {
       // ... add to children array ...
       if (layout != null) {
           layout(true); // Flush cache, recalculate
       }
   }
   ```

3. **Layout Data Changed**:
   ```java
   control.setLayoutData(newData);
   // Triggers parent.layout(true) internally
   ```

4. **Manual Call**:
   ```java
   composite.layout(true);        // Force layout, flush cache
   composite.layout(true, true);  // Force layout recursively
   ```

### Client Area vs Bounds

```java
// Shell bounds: entire window including title bar, borders
Rectangle shellBounds = shell.getBounds();
// { x=100, y=100, width=500, height=400 }

// Client area: interior space for children
Rectangle clientArea = shell.getClientArea();
// { x=0, y=0, width=496, height=370 } (borders/titlebar excluded)

// Layout works with client area
layout.layout(composite, false);
// Positions children within clientArea coordinates
```

---

## 7. Optimization and Caching

### Layout Caching Strategy

Layouts cache computed sizes to avoid redundant calculations:

```java
public class GridLayout extends Layout {

    // Cache fields
    private int cacheWidth = -1;
    private int cacheHeight = -1;
    private int[] cachedColumnWidths;
    private int[] cachedRowHeights;

    @Override
    protected Point computeSize(Composite composite, int wHint, int hHint,
                                boolean flushCache) {
        // Check cache validity
        if (!flushCache && cacheWidth != -1 && wHint == SWT.DEFAULT && hHint == SWT.DEFAULT) {
            return new Point(cacheWidth, cacheHeight);
        }

        // Compute fresh
        // ... expensive calculations ...

        // Update cache
        if (wHint == SWT.DEFAULT && hHint == SWT.DEFAULT) {
            cacheWidth = calculatedWidth;
            cacheHeight = calculatedHeight;
        }

        return new Point(calculatedWidth, calculatedHeight);
    }

    @Override
    protected boolean layout(Composite composite, boolean flushCache) {
        if (flushCache) {
            // Invalidate cache
            cacheWidth = -1;
            cacheHeight = -1;
            cachedColumnWidths = null;
            cachedRowHeights = null;
        }

        // ... layout logic ...
    }
}
```

### When to Flush Cache

- **flushCache = true**: Control added/removed, layout data changed, explicit size change
- **flushCache = false**: Simple resize (aspect ratio change), redraw

### Control Caching

Controls also cache their computed sizes:

```java
public abstract class Control extends Widget {

    private int cachedWidth = -1;
    private int cachedHeight = -1;

    public Point computeSize(int wHint, int hHint, boolean changed) {
        checkWidget();

        if (!changed && wHint == SWT.DEFAULT && hHint == SWT.DEFAULT
            && cachedWidth != -1) {
            return new Point(cachedWidth, cachedHeight);
        }

        Point size = computeSizeInternal(wHint, hHint);

        if (wHint == SWT.DEFAULT && hHint == SWT.DEFAULT) {
            cachedWidth = size.x;
            cachedHeight = size.y;
        }

        return size;
    }

    protected abstract Point computeSizeInternal(int wHint, int hHint);
}
```

### Performance Optimization Tips

1. **Minimize Layout Calls**: Batch control additions, then call `layout(true)` once
   ```java
   composite.setRedraw(false); // Suspend drawing
   try {
       for (int i = 0; i < 100; i++) {
           new Button(composite, SWT.PUSH);
       }
   } finally {
       composite.setRedraw(true);
       composite.layout(true); // Single layout pass
   }
   ```

2. **Use `layout(false)` When Possible**: Resize events don't need cache flush

3. **Prefer Simple Layouts**: FillLayout < RowLayout < GridLayout < FormLayout (complexity)

4. **Avoid Deeply Nested Composites**: Each level adds layout overhead

5. **Use `exclude` Flag**: Temporarily hide controls without removing
   ```java
   gridData.exclude = true;
   control.setVisible(false);
   parent.layout(true);
   ```

---

## 8. Platform-Specific Considerations

### Native Size Queries

Each platform has different methods to query native control sizes:

#### Windows (Win32)
```csharp
// Use GetSystemMetrics, SendMessage, etc.
[DllImport("user32.dll")]
static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

Size GetPreferredSize(IntPtr handle) {
    const uint BCM_GETIDEALSIZE = 0x1601; // Button
    SIZE size;
    SendMessage(handle, BCM_GETIDEALSIZE, 0, ref size);
    return new Size(size.cx, size.cy);
}
```

#### macOS (Cocoa)
```objc
// Use NSView's intrinsicContentSize or fittingSize
NSSize size = [nsView intrinsicContentSize];
```

#### Linux (GTK)
```c
// Use gtk_widget_get_preferred_size
GtkRequisition minimum, natural;
gtk_widget_get_preferred_size(widget, &minimum, &natural);
```

### DPI Scaling

Modern systems require DPI-aware sizing:

```csharp
// Windows
int ScaleToDPI(int value, int dpi) {
    return (value * dpi) / 96; // 96 DPI = 100%
}

// Get monitor DPI
[DllImport("user32.dll")]
static extern uint GetDpiForWindow(IntPtr hwnd);
```

### Font Metrics

Text controls need font-based sizing:

```csharp
// Calculate characters-based width
int GetTextWidth(string text, Font font) {
    using (Graphics g = Graphics.FromHwnd(handle)) {
        SizeF size = g.MeasureString(text, font);
        return (int)Math.Ceiling(size.Width);
    }
}

// Convert dialog units to pixels (Windows)
int DialogUnitsToPixelsX(int du) {
    // 1 DU = 1/4 average character width
    return (du * avgCharWidth) / 4;
}
```

### Trim (Border/Padding)

Platform controls have inherent padding:

```java
Point computeSize(int wHint, int hHint) {
    Point size = computeNativeSize(wHint, hHint);

    // Add platform trim
    Rectangle trim = computeTrim(0, 0, 0, 0);
    size.x += trim.width;
    size.y += trim.height;

    return size;
}

// Platform-specific trim calculation
Rectangle computeTrim(int x, int y, int width, int height) {
    // Example: Button has 8px horizontal, 4px vertical padding
    return new Rectangle(x - 8, y - 4, width + 16, height + 8);
}
```

### Minimum Sizes

Platforms enforce minimum control sizes:

```csharp
// Windows: Buttons have minimum 75x23 DLU
const int MIN_BUTTON_WIDTH = 75;
const int MIN_BUTTON_HEIGHT = 23;

Size EnsureMinimumSize(Size requested) {
    return new Size(
        Math.Max(requested.Width, MIN_BUTTON_WIDTH),
        Math.Max(requested.Height, MIN_BUTTON_HEIGHT)
    );
}
```

### Platform Layout Differences

| Aspect | Windows | macOS | Linux (GTK) |
|--------|---------|-------|-------------|
| Default spacing | 7 DLU | 8pt | 6px |
| Button height | 23 DLU | 32pt | 28px |
| Margin conventions | Tight | Generous | Medium |
| DPI awareness | High DPI API | Retina scaling | Fractional scaling |

---

## Implementation Roadmap for SWTSharp

### Phase 1: Core Infrastructure
1. Implement `Layout` abstract base class
2. Implement `Composite.Layout` property and methods
3. Add `Control.LayoutData` property
4. Implement `Control.ComputeSize()` with platform queries

### Phase 2: Simple Layouts
1. Implement `FillLayout` (no layout data)
2. Implement `StackLayout` (no layout data)
3. Test basic layout and sizing

### Phase 3: Intermediate Layouts
1. Implement `RowLayout` and `RowData`
2. Test wrapping, packing, and flow behavior

### Phase 4: Advanced Layouts
1. Implement `GridLayout` and `GridData`
   - Grid building algorithm
   - Column/row sizing
   - Spanning and alignment
   - Grab space distribution
2. Extensive testing with complex grids

### Phase 5: Expert Layouts
1. Implement `FormLayout`, `FormData`, and `FormAttachment`
   - Constraint resolution engine
   - Dependency graph handling
   - Circular dependency detection
2. Test complex constraint scenarios

### Phase 6: Optimization
1. Implement layout caching
2. Add DPI scaling support
3. Platform-specific size optimizations
4. Performance profiling and tuning

---

## Key Takeaways

1. **Two-Phase Algorithm**: Always compute size first, then position
2. **Bottom-Up Sizing**: Children compute sizes, parents aggregate
3. **Top-Down Positioning**: Parents allocate space, children fill
4. **Layout Data Pattern**: Children carry hints for parent's layout
5. **Caching Critical**: Avoid redundant calculations with smart caching
6. **Platform Abstraction**: Native size queries must be wrapped consistently
7. **Recursive Layouts**: Composites can contain composites infinitely
8. **Client Area**: Always work in client area coordinates, not window bounds

---

## References

- Eclipse SWT Documentation: https://www.eclipse.org/swt/
- SWT Source Code: https://github.com/eclipse-platform/eclipse.platform.swt
- Layout Tutorial: https://www.eclipse.org/articles/Understanding-Layouts/Understanding-Layouts.htm
- GridLayout Article: https://www.eclipse.org/articles/Article-Understanding-Layouts/Understanding-Layouts.htm
- FormLayout Guide: https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/guide/forms_controls_form_layout.htm

---

**Document Version**: 1.0
**Last Updated**: 2025-10-05
**Author**: Research Agent (SWTSharp Implementation)
