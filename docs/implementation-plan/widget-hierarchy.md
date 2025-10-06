# Eclipse SWT Widget Hierarchy and API Structure

## Executive Summary

This document provides a comprehensive analysis of the Eclipse SWT (Standard Widget Toolkit) widget hierarchy and API structure based on research of the official Eclipse SWT source code and documentation. This analysis will guide the implementation of SWT# (SWT for C#/.NET).

**Research Date:** October 5, 2025
**Source Repository:** https://github.com/eclipse-platform/eclipse.platform.swt
**Primary Package:** org.eclipse.swt.widgets

## 1. Complete Class Hierarchy

### 1.1 Base Widget Hierarchy Tree

```
Widget (abstract base class)
├── Caret
├── Control (abstract)
│   ├── Button
│   ├── Label
│   ├── Link
│   ├── ProgressBar
│   ├── Sash
│   ├── Scale
│   ├── Slider
│   └── Scrollable (abstract)
│       ├── Composite
│       │   ├── Canvas
│       │   │   └── Decorations
│       │   │       └── Shell
│       │   ├── Group
│       │   ├── TabFolder
│       │   ├── ToolBar
│       │   └── (custom composites)
│       ├── Combo
│       ├── CoolBar
│       ├── DateTime
│       ├── ExpandBar
│       ├── List
│       ├── Spinner
│       ├── Table
│       ├── Text
│       └── Tree
├── IME
├── Item (abstract)
│   ├── CoolItem
│   ├── ExpandItem
│   ├── MenuItem
│   ├── TabItem
│   ├── TableColumn
│   ├── TableItem
│   ├── TaskItem
│   ├── ToolItem
│   ├── TrayItem
│   ├── TreeColumn
│   └── TreeItem
├── Menu
├── ScrollBar
├── TaskBar
├── ToolTip
├── Tracker
└── Tray
```

### 1.2 Key Inheritance Relationships

**Widget** → Root of all UI objects
- Manages lifecycle, events, resource management
- Abstract base for all SWT components

**Control** (extends Widget) → Interactive UI elements
- Represents visible, interactive widgets
- Handles layout, sizing, colors, fonts, events
- Parent-child relationships

**Scrollable** (extends Control) → Controls with scrollbars
- Base for widgets that can have scrollbars
- Includes most container and data widgets

**Composite** (extends Scrollable) → Container widgets
- Can contain other controls as children
- Manages layout and child widget organization
- Base class for custom compound widgets

**Canvas** (extends Composite) → Custom drawing surface
- Used for custom widgets that draw themselves
- Extends Composite (implementation artifact)
- Base class for custom basic widgets

**Item** (extends Widget) → Non-control UI elements
- Represents items within containers (menu items, tree items, etc.)
- Not independently visible controls

## 2. Core Base Classes API

### 2.1 Widget Class

**Purpose:** Abstract base class for all SWT user interface objects

**Key Methods:**

#### Resource Management
```java
void dispose()                    // Releases OS resources for widget and descendants
boolean isDisposed()              // Checks if widget has been disposed
void checkWidget()                // Validates widget access and thread safety
void addDisposeListener(DisposeListener listener)
```

#### Event Handling
```java
void addListener(int eventType, Listener listener)      // Register event listener
void removeListener(int eventType, Listener listener)   // Remove event listener
void notifyListeners(int eventType, Event event)        // Trigger event notifications
boolean isListening(int eventType)                      // Check if listeners exist
```

#### Properties and Data
```java
void setData(Object data)              // Set unnamed widget-level data
Object getData()                       // Get widget-level data
void setData(String key, Object value) // Set named property
Object getData(String key)             // Get named property
int getStyle()                         // Get widget style information
Display getDisplay()                   // Get associated display
```

#### Lifecycle
```java
void checkSubclass()                   // Control widget subclassing
```

**Important Notes:**
- All widgets must be created and accessed from the UI thread
- Subclassing is strongly discouraged outside SWT implementation
- Resources must be explicitly disposed

### 2.2 Control Class

**Purpose:** Abstract base for all windowed user interface elements

**Key Methods:**

#### Layout and Sizing
```java
Point computeSize(int wHint, int hHint)           // Returns preferred size
Point computeSize(int wHint, int hHint, boolean changed)
void pack()                                        // Resize to preferred size
void pack(boolean changed)
void requestLayout()                               // Request repositioning by layouts
void setBounds(int x, int y, int width, int height) // Set size and location
void setBounds(Rectangle rect)
void setSize(int width, int height)
void setSize(Point size)
void setLocation(int x, int y)
void setLocation(Point location)
Rectangle getBounds()
Point getSize()
Point getLocation()
```

#### Visibility and State
```java
void setVisible(boolean visible)       // Mark control as visible/invisible
boolean getVisible()                   // Check visibility flag
boolean isVisible()                    // Recursively check visibility
void setEnabled(boolean enabled)       // Enable/disable control
boolean getEnabled()                   // Check enabled flag
boolean isEnabled()                    // Recursively check enabled state
```

#### Parent-Child Relationships
```java
Composite getParent()                  // Get control's parent
Shell getShell()                       // Get top-level shell
void setParent(Composite parent)       // Change control's parent
void moveAbove(Control control)        // Change drawing order
void moveBelow(Control control)        // Change drawing order
```

#### Colors and Appearance
```java
void setBackground(Color color)        // Set background color
Color getBackground()
void setForeground(Color color)        // Set foreground/text color
Color getForeground()
void setFont(Font font)                // Set text rendering font
Font getFont()
void setBackgroundImage(Image image)   // Set background image
Image getBackgroundImage()
void setCursor(Cursor cursor)          // Set mouse cursor
Cursor getCursor()
```

#### Redrawing and Updates
```java
void redraw()                          // Mark control for redrawing
void redraw(int x, int y, int width, int height, boolean all)
void update()                          // Force paint requests to process
boolean print(GC gc)                   // Print control and children
```

#### Event Listeners (Typed)
```java
void addControlListener(ControlListener listener)    // Move/resize events
void addMouseListener(MouseListener listener)        // Mouse button events
void addMouseMoveListener(MouseMoveListener listener)
void addMouseTrackListener(MouseTrackListener listener)
void addMouseWheelListener(MouseWheelListener listener)
void addKeyListener(KeyListener listener)            // Keyboard events
void addPaintListener(PaintListener listener)        // Paint events
void addFocusListener(FocusListener listener)        // Focus events
void addDragDetectListener(DragDetectListener listener)
void addMenuDetectListener(MenuDetectListener listener)
void addTouchListener(TouchListener listener)
void addGestureListener(GestureListener listener)
```

#### Focus Management
```java
boolean setFocus()                     // Attempt to set input focus
boolean isFocusControl()               // Check if control has focus
boolean forceFocus()                   // Force focus to this control
```

#### Menu Support
```java
void setMenu(Menu menu)                // Set popup menu
Menu getMenu()
```

**Important Notes:**
- Controls must have a Composite parent
- All UI operations must occur on the UI thread
- Layout is managed by parent's Layout object

### 2.3 Composite Class

**Purpose:** Control that can contain other controls (container widget)

**Extends:** Scrollable → Control → Widget

**Key Methods:**

#### Child Management
```java
Control[] getChildren()                // Returns child controls in draw order
Control[] getTabList()                 // Gets tabbing order for controls
void setTabList(Control[] tabList)     // Sets specific tabbing order
```

#### Layout Handling
```java
void setLayout(Layout layout)          // Sets the layout manager
Layout getLayout()                     // Retrieves current layout
void layout()                          // Triggers layout for children
void layout(boolean changed)           // Layout with optional cache flushing
void layout(boolean changed, boolean all)  // Comprehensive layout with cascade
void layout(Control[] changed)         // Layouts specific widgets
void layout(Control[] changed, int flags)  // Advanced layout with flags
boolean isLayoutDeferred()             // Checks if layout is deferred
void setLayoutDeferred(boolean defer)  // Temporarily suspends layout
```

#### Background Drawing
```java
int getBackgroundMode()                // Returns background drawing mode
void setBackgroundMode(int mode)       // Sets background drawing mode
```

**Layout Deferred Flags:**
```java
SWT.NONE           // Normal layout
SWT.CHANGED        // Flush layout cache
SWT.ALL            // Cascade to all descendants
```

**Background Modes:**
```java
SWT.INHERIT_NONE     // Children don't inherit background
SWT.INHERIT_DEFAULT  // Default inheritance
SWT.INHERIT_FORCE    // Force background inheritance
```

**Important Notes:**
- Composite is the base class for all container widgets
- Custom compound widgets should extend Composite
- Layout managers control automatic positioning of children
- Changes to children typically require calling layout()

## 3. Major Widget Classes

### 3.1 Selection Widgets

#### List Widget

**Purpose:** Scrollable widget displaying a list of selectable text items

**Style Bits:**
- `SWT.SINGLE` - Single selection
- `SWT.MULTI` - Multiple selection

**Key Methods:**

```java
// Item Management
void add(String string)                   // Add item to end
void add(String string, int index)        // Add item at index
void remove(int index)                    // Remove item at index
void remove(String string)                // Remove first occurrence
void removeAll()                          // Clear all items
void setItems(String... items)            // Replace all items
void setItem(int index, String string)    // Update item text

// Query Methods
int getItemCount()                        // Total number of items
String[] getItems()                       // All items as array
String getItem(int index)                 // Item at index
int indexOf(String string)                // Find index of item
int indexOf(String string, int start)     // Find index starting from position

// Selection Methods
void select(int index)                    // Select item at index
void select(int start, int end)           // Select range
void selectAll()                          // Select all (multi-select only)
void deselect(int index)                  // Deselect item
void deselect(int start, int end)         // Deselect range
void deselectAll()                        // Deselect all
void setSelection(int index)              // Set selection to item
void setSelection(int[] indices)          // Set selection to items
void setSelection(String[] items)         // Set selection by text
String[] getSelection()                   // Get selected items
int[] getSelectionIndices()               // Get selected indices
int getSelectionIndex()                   // Get first selection index
int getSelectionCount()                   // Number of selected items

// Scrolling
int getTopIndex()                         // Get top visible item index
void setTopIndex(int index)               // Scroll to make item top
void showSelection()                      // Scroll to show selection

// Events
void addSelectionListener(SelectionListener listener)
```

#### Combo Widget

**Purpose:** Combination of text field and dropdown list

**Style Bits:**
- `SWT.DROP_DOWN` - Dropdown with text entry
- `SWT.READ_ONLY` - Dropdown without text entry
- `SWT.SIMPLE` - Always visible list

**Key Methods:**

```java
// Item Management
void add(String string)                   // Add item to end
void add(String string, int index)        // Add item at index
void remove(int index)                    // Remove item
void remove(int start, int end)           // Remove range
void removeAll()                          // Clear all items
void setItems(String... items)            // Replace all items
String[] getItems()                       // Get all items
int getItemCount()                        // Total items
int indexOf(String string)                // Find item index
int indexOf(String string, int start)

// Text Input
void setText(String string)               // Set text field content
String getText()                          // Get text field content
void setTextLimit(int limit)              // Set max text length
int getTextLimit()                        // Get text limit
void clearSelection()                     // Clear text selection

// Selection
void select(int index)                    // Select item
void deselect(int index)                  // Deselect item
void deselectAll()                        // Clear selection
int getSelectionIndex()                   // Get selected index
Point getSelection()                      // Get text selection range

// Dropdown Control
void setListVisible(boolean visible)      // Show/hide dropdown
boolean getListVisible()                  // Check dropdown visibility
void setVisibleItemCount(int count)       // Set visible dropdown items
int getVisibleItemCount()

// Events
void addSelectionListener(SelectionListener listener)
void addModifyListener(ModifyListener listener)
void addVerifyListener(VerifyListener listener)
```

#### Table Widget

**Purpose:** Widget displaying tabular data with rows and columns

**Style Bits:**
- `SWT.SINGLE` / `SWT.MULTI` - Selection mode
- `SWT.CHECK` - Checkboxes for items
- `SWT.FULL_SELECTION` - Full row selection
- `SWT.VIRTUAL` - Lazy loading of items

**Key Methods:**

```java
// Column Management
int getColumnCount()                      // Number of columns
TableColumn[] getColumns()                // All columns
TableColumn getColumn(int index)          // Column at index
int[] getColumnOrder()                    // Display order
void setColumnOrder(int[] order)          // Set display order

// Item Management
int getItemCount()                        // Number of items
TableItem[] getItems()                    // All items
TableItem getItem(int index)              // Item at index
TableItem getItem(Point point)            // Item at coordinates
void removeAll()                          // Remove all items
void setItemCount(int count)              // Set item count (virtual)

// Selection
TableItem[] getSelection()                // Selected items
int getSelectionCount()                   // Number selected
int[] getSelectionIndices()               // Selected indices
int getSelectionIndex()                   // First selection
void select(int index)                    // Select item
void select(int[] indices)                // Select items
void selectAll()                          // Select all
void deselectAll()                        // Clear selection
void setSelection(TableItem item)         // Set selection
void setSelection(TableItem[] items)      // Set selection

// Appearance
void setHeaderVisible(boolean show)       // Show/hide headers
boolean getHeaderVisible()
void setLinesVisible(boolean show)        // Show/hide grid lines
boolean getLinesVisible()
void setSortColumn(TableColumn column)    // Set sort column
TableColumn getSortColumn()
void setSortDirection(int direction)      // SWT.UP, SWT.DOWN, SWT.NONE
int getSortDirection()

// Scrolling
int getTopIndex()                         // Top visible item
void setTopIndex(int index)               // Scroll to item
void showSelection()                      // Scroll to selection

// Events
void addSelectionListener(SelectionListener listener)
```

#### Tree Widget

**Purpose:** Widget displaying hierarchical data

**Style Bits:**
- `SWT.SINGLE` / `SWT.MULTI` - Selection mode
- `SWT.CHECK` - Checkboxes for items
- `SWT.VIRTUAL` - Lazy loading

**Key Methods:**

```java
// Item Management
int getItemCount()                        // Root-level items count
TreeItem[] getItems()                     // Root-level items
TreeItem getItem(int index)               // Root item at index
TreeItem getItem(Point point)             // Item at coordinates
void removeAll()                          // Remove all items
void setItemCount(int count)              // Set root item count (virtual)

// Selection
TreeItem[] getSelection()                 // Selected items
int getSelectionCount()                   // Number selected
void setSelection(TreeItem item)          // Set selection
void setSelection(TreeItem[] items)       // Set selection
void selectAll()                          // Select all
void deselectAll()                        // Clear selection

// Expansion
void addTreeListener(TreeListener listener) // Expand/collapse events
void showItem(TreeItem item)              // Scroll to item

// Appearance
void setHeaderVisible(boolean show)       // Show/hide column headers
boolean getHeaderVisible()
void setLinesVisible(boolean show)        // Show/hide tree lines
boolean getLinesVisible()

// Scrolling
TreeItem getTopItem()                     // Top visible item
void setTopItem(TreeItem item)            // Scroll to make item top

// Events
void addSelectionListener(SelectionListener listener)
```

### 3.2 Container Widgets

#### Shell

**Purpose:** Top-level window (extends Decorations → Canvas → Composite)

**Style Bits:**
- `SWT.SHELL_TRIM` - Standard window decoration
- `SWT.DIALOG_TRIM` - Dialog decoration
- `SWT.NO_TRIM` - No decoration
- `SWT.CLOSE` / `SWT.MIN` / `SWT.MAX` - Individual decorations
- `SWT.RESIZE` - Resizable border
- `SWT.TITLE` - Title bar
- `SWT.APPLICATION_MODAL` / `SWT.SYSTEM_MODAL` - Modality

**Key Methods:**

```java
void open()                               // Make shell visible
void close()                              // Close shell
void setText(String string)               // Set title bar text
String getText()
void setImage(Image image)                // Set title bar icon
void setImages(Image[] images)            // Set icon at multiple sizes
void setActive()                          // Bring to front
void setMinimized(boolean minimized)      // Minimize/restore
boolean getMinimized()
void setMaximized(boolean maximized)      // Maximize/restore
boolean getMaximized()
void setFullScreen(boolean fullScreen)    // Enter/exit fullscreen
boolean getFullScreen()
void forceActive()                        // Force to front

// Events
void addShellListener(ShellListener listener)
```

#### Group

**Purpose:** Composite with labeled border (etched frame)

**Key Methods:**

```java
void setText(String string)               // Set group label
String getText()
```

#### TabFolder

**Purpose:** Composite organizing children in tabbed pages

**Key Methods:**

```java
int getItemCount()                        // Number of tabs
TabItem[] getItems()                      // All tab items
TabItem getItem(int index)                // Tab at index
int[] getSelectionIndex()                 // Selected tab index
TabItem getSelection()                    // Selected tab item
void setSelection(int index)              // Select tab by index
void setSelection(TabItem item)           // Select tab

// Events
void addSelectionListener(SelectionListener listener)
```

#### ToolBar

**Purpose:** Composite containing tool items (buttons, separators)

**Style Bits:**
- `SWT.FLAT` - Flat appearance
- `SWT.HORIZONTAL` / `SWT.VERTICAL` - Orientation
- `SWT.WRAP` - Wrap items to multiple rows

**Key Methods:**

```java
int getItemCount()                        // Number of items
ToolItem[] getItems()                     // All tool items
ToolItem getItem(int index)               // Item at index
ToolItem getItem(Point point)             // Item at coordinates

// Events
void addSelectionListener(SelectionListener listener)
```

### 3.3 Menu System

#### Menu

**Purpose:** Container for menu items (menu bar, popup, dropdown)

**Style Bits:**
- `SWT.BAR` - Menu bar
- `SWT.DROP_DOWN` - Dropdown menu
- `SWT.POP_UP` - Popup/context menu
- `SWT.NO_RADIO_GROUP` - Disable radio grouping

**Constructors:**
```java
Menu(Control parent)                      // Popup menu on control
Menu(Decorations parent, int style)       // Menu bar on window
Menu(Menu parentMenu)                     // Submenu on menu
Menu(MenuItem parentItem)                 // Dropdown on menu item
```

**Key Methods:**

```java
// Item Management
MenuItem getItem(int index)               // Get menu item
int getItemCount()                        // Number of items
MenuItem[] getItems()                     // All items
int indexOf(MenuItem item)                // Find item index

// Visibility
void setVisible(boolean visible)          // Show/hide menu
boolean getVisible()
boolean isVisible()                       // Check if visible
void setEnabled(boolean enabled)          // Enable/disable
boolean getEnabled()
boolean isEnabled()

// Position (for popup menus)
void setLocation(int x, int y)            // Position popup menu
void setLocation(Point location)

// Configuration
void setDefaultItem(MenuItem item)        // Set default item
MenuItem getDefaultItem()
void setOrientation(int orientation)      // Text direction

// Events
void addMenuListener(MenuListener listener)
void addHelpListener(HelpListener listener)
```

#### MenuItem

**Purpose:** Selectable item within a menu

**Style Bits:**
- `SWT.PUSH` - Standard menu item
- `SWT.CHECK` - Checkable menu item
- `SWT.RADIO` - Radio button menu item
- `SWT.SEPARATOR` - Visual separator
- `SWT.CASCADE` - Submenu indicator

**Key Methods:**

```java
void setText(String string)               // Set menu text
String getText()
void setImage(Image image)                // Set icon
Image getImage()
void setEnabled(boolean enabled)          // Enable/disable
boolean getEnabled()
void setSelection(boolean selected)       // Check/uncheck (CHECK/RADIO)
boolean getSelection()
void setMenu(Menu menu)                   // Set submenu (CASCADE)
Menu getMenu()
void setAccelerator(int accelerator)      // Set keyboard shortcut
int getAccelerator()

// Events
void addSelectionListener(SelectionListener listener)
void addArmListener(ArmListener listener)
void addHelpListener(HelpListener listener)
```

### 3.4 Simple Controls

#### Button

**Style Bits:**
- `SWT.PUSH` - Push button
- `SWT.CHECK` - Checkbox
- `SWT.RADIO` - Radio button
- `SWT.TOGGLE` - Toggle button
- `SWT.ARROW` - Arrow button
- `SWT.FLAT` - Flat appearance

**Key Methods:**

```java
void setText(String string)
String getText()
void setImage(Image image)
Image getImage()
void setSelection(boolean selected)       // For CHECK, RADIO, TOGGLE
boolean getSelection()
void setAlignment(int alignment)          // SWT.LEFT, CENTER, RIGHT
int getAlignment()

// Events
void addSelectionListener(SelectionListener listener)
```

#### Label

**Style Bits:**
- `SWT.SEPARATOR` - Visual separator line
- `SWT.HORIZONTAL` / `SWT.VERTICAL` - Separator orientation
- `SWT.SHADOW_IN` / `SWT.SHADOW_OUT` / `SWT.SHADOW_NONE` - Separator style

**Key Methods:**

```java
void setText(String string)
String getText()
void setImage(Image image)
Image getImage()
void setAlignment(int alignment)          // SWT.LEFT, CENTER, RIGHT
int getAlignment()
```

#### Text

**Style Bits:**
- `SWT.SINGLE` / `SWT.MULTI` - Single or multi-line
- `SWT.READ_ONLY` - Non-editable
- `SWT.WRAP` - Word wrap (multi-line)
- `SWT.PASSWORD` - Password field
- `SWT.SEARCH` - Search field styling

**Key Methods:**

```java
void setText(String string)
String getText()
void append(String string)
void insert(String string)
void setTextLimit(int limit)
int getTextLimit()
void setEditable(boolean editable)
boolean getEditable()
Point getSelection()                      // Selection range
void setSelection(int start, int end)
void selectAll()
void clearSelection()
String getSelectionText()
int getCaretPosition()
void setCaretPosition(int position)

// Events
void addModifyListener(ModifyListener listener)
void addVerifyListener(VerifyListener listener)
void addSelectionListener(SelectionListener listener)
```

#### ProgressBar

**Style Bits:**
- `SWT.HORIZONTAL` / `SWT.VERTICAL` - Orientation
- `SWT.SMOOTH` - Smooth progress
- `SWT.INDETERMINATE` - Indeterminate/busy indicator

**Key Methods:**

```java
void setMinimum(int value)
int getMinimum()
void setMaximum(int value)
int getMaximum()
void setSelection(int value)              // Current progress
int getSelection()
int getState()                            // SWT.NORMAL, ERROR, PAUSED
void setState(int state)
```

### 3.5 Other Important Widgets

#### Canvas

**Purpose:** Composite for custom drawing

**Key Methods:**

```java
void addPaintListener(PaintListener listener)
Caret getCaret()
void setCaret(Caret caret)
void scroll(int destX, int destY, int x, int y, int width, int height, boolean all)
```

#### Spinner

**Purpose:** Text field with up/down buttons for numeric input

**Key Methods:**

```java
void setMinimum(int value)
int getMinimum()
void setMaximum(int value)
int getMaximum()
void setSelection(int value)
int getSelection()
void setIncrement(int value)
int getIncrement()
void setPageIncrement(int value)
int getPageIncrement()
void setDigits(int value)                 // Decimal places
int getDigits()

// Events
void addSelectionListener(SelectionListener listener)
void addModifyListener(ModifyListener listener)
```

#### Scale

**Purpose:** Slider control for selecting numeric value

**Style Bits:**
- `SWT.HORIZONTAL` / `SWT.VERTICAL` - Orientation

**Key Methods:**

```java
void setMinimum(int value)
int getMinimum()
void setMaximum(int value)
int getMaximum()
void setSelection(int value)
int getSelection()
void setIncrement(int value)
int getIncrement()
void setPageIncrement(int value)
int getPageIncrement()

// Events
void addSelectionListener(SelectionListener listener)
```

#### Slider

**Purpose:** Scrollbar-like control (similar to Scale but different appearance)

**Key Methods:** (same as Scale)

## 4. Event Handling Pattern

### 4.1 Event System Architecture

SWT provides two listener APIs:

1. **Untyped Listener API** - Low-level, generic event handling
2. **Typed Listener API** - High-level, type-safe event handling

#### Untyped Event System

**Core Interfaces:**
```java
// Generic listener interface
interface Listener {
    void handleEvent(Event event);
}

// Generic event object
class Event {
    Widget widget;           // Widget that generated event
    Display display;         // Display
    int type;               // Event type (SWT.Selection, SWT.Paint, etc.)
    int detail;             // Additional detail
    Object data;            // Custom data
    int x, y;               // Mouse coordinates
    int width, height;      // Size for paint/resize events
    int button;             // Mouse button
    char character;         // Keyboard character
    int keyCode;            // Keyboard key code
    int stateMask;          // Modifier keys (SHIFT, CTRL, ALT)
    // ... many other fields
}
```

**Usage:**
```java
widget.addListener(SWT.Selection, event -> {
    System.out.println("Selected: " + event.widget);
});

widget.removeListener(SWT.Selection, listener);
widget.notifyListeners(SWT.Selection, event);
```

**Common Event Types:**
```java
SWT.Selection          // User selection (button click, list selection, etc.)
SWT.DefaultSelection   // Default selection (double-click, Enter key)
SWT.Paint              // Widget needs repainting
SWT.Resize             // Widget resized
SWT.Move               // Widget moved
SWT.KeyDown / KeyUp    // Keyboard input
SWT.MouseDown / MouseUp / MouseMove / MouseDoubleClick
SWT.FocusIn / FocusOut // Focus changes
SWT.Show / Hide        // Visibility changes
SWT.Dispose            // Widget disposed
```

#### Typed Event System

**Advantages:**
- Type-safe, specific listener interfaces
- Better IDE support (autocomplete, refactoring)
- More intuitive API

**Common Typed Listeners:**

```java
// Selection events
interface SelectionListener extends EventListener {
    void widgetSelected(SelectionEvent e);
    void widgetDefaultSelected(SelectionEvent e);
}

// Mouse events
interface MouseListener extends EventListener {
    void mouseDown(MouseEvent e);
    void mouseUp(MouseEvent e);
    void mouseDoubleClick(MouseEvent e);
}

interface MouseMoveListener extends EventListener {
    void mouseMove(MouseEvent e);
}

// Keyboard events
interface KeyListener extends EventListener {
    void keyPressed(KeyEvent e);
    void keyReleased(KeyEvent e);
}

// Paint events
interface PaintListener extends EventListener {
    void paintControl(PaintEvent e);
}

// Focus events
interface FocusListener extends EventListener {
    void focusGained(FocusEvent e);
    void focusLost(FocusEvent e);
}

// Control events (resize, move)
interface ControlListener extends EventListener {
    void controlMoved(ControlEvent e);
    void controlResized(ControlEvent e);
}

// Modify events (text changes)
interface ModifyListener extends EventListener {
    void modifyText(ModifyEvent e);
}

// Verify events (validate input before change)
interface VerifyListener extends EventListener {
    void verifyText(VerifyEvent e);  // e.doit = false to reject
}

// Tree events
interface TreeListener extends EventListener {
    void treeExpanded(TreeEvent e);
    void treeCollapsed(TreeEvent e);
}

// Menu events
interface MenuListener extends EventListener {
    void menuHidden(MenuEvent e);
    void menuShown(MenuEvent e);
}

// Shell events
interface ShellListener extends EventListener {
    void shellActivated(ShellEvent e);
    void shellClosed(ShellEvent e);
    void shellDeactivated(ShellEvent e);
    void shellDeiconified(ShellEvent e);
    void shellIconified(ShellEvent e);
}
```

**Usage:**
```java
button.addSelectionListener(new SelectionListener() {
    public void widgetSelected(SelectionEvent e) {
        System.out.println("Button clicked");
    }
    public void widgetDefaultSelected(SelectionEvent e) {
        // Default action (rarely used for buttons)
    }
});

// Or with lambda (Java 8+):
button.addSelectionListener(SelectionListener.widgetSelectedAdapter(e -> {
    System.out.println("Button clicked");
}));
```

### 4.2 TypedListener Implementation

**Internal Adapter Pattern:**

SWT uses `TypedListener` internally to bridge typed and untyped listeners:

```java
// Internal SWT implementation (DO NOT use in application code)
class TypedListener implements Listener {
    private EventListener eventListener;

    public void handleEvent(Event e) {
        switch (e.type) {
            case SWT.Selection:
                SelectionEvent se = new SelectionEvent(e);
                ((SelectionListener)eventListener).widgetSelected(se);
                break;
            // ... other event types
        }
    }
}
```

**Modern API (SWT 4.0+):**
```java
// For custom widget implementations
widget.addTypedListener(listener, SWT.Selection, SWT.DefaultSelection);
widget.removeTypedListener(SWT.Selection, listener);
widget.getTypedListeners(SWT.Selection, SelectionListener.class);
```

**Note:** TypedListener itself is deprecated for direct use and will become internal-only.

### 4.3 Event Propagation

**Key Concepts:**

1. **Event Bubbling:** Some events bubble up the widget hierarchy
2. **Event.doit Flag:** Set to `false` to cancel default behavior
3. **Verify Events:** Occur before modification, allow cancellation

**Example:**
```java
text.addVerifyListener(e -> {
    // Only allow digits
    if (!e.text.matches("\\d*")) {
        e.doit = false;  // Reject input
    }
});
```

### 4.4 Adapter Classes

SWT provides adapter classes with empty implementations for convenience:

```java
// Instead of implementing all methods:
widget.addMouseListener(new MouseAdapter() {
    @Override
    public void mouseDown(MouseEvent e) {
        // Only override what you need
    }
});
```

**Common Adapters:**
- `SelectionAdapter`
- `MouseAdapter`
- `MouseTrackAdapter`
- `MouseMoveListener` (single method, no adapter needed)
- `KeyAdapter`
- `FocusAdapter`
- `ControlAdapter`
- `TreeAdapter`
- `MenuAdapter`
- `ShellAdapter`

## 5. Implementation Priority

### 5.1 Phase 1: Core Foundation (Highest Priority)

**Essential Base Classes:**
1. **Widget** - Absolute foundation for everything
2. **Control** - Base for all visual widgets
3. **Composite** - Container functionality
4. **Canvas** - Custom drawing support

**Rationale:** These four classes form the core hierarchy that all other widgets depend on.

### 5.2 Phase 2: Basic Container and Shell (High Priority)

**Essential for Minimal Applications:**
1. **Shell** - Top-level windows (extends Decorations → Canvas → Composite)
2. **Display** - Event loop and resource management
3. **Layout** - Basic layout support
   - GridLayout
   - FillLayout
   - RowLayout

**Rationale:** Needed to create and display any application window.

### 5.3 Phase 3: Basic Controls (High Priority)

**Most Common Widgets:**
1. **Button** - Essential for user interaction
2. **Label** - Text and image display
3. **Text** - Text input
4. **Group** - Visual grouping

**Rationale:** These widgets appear in virtually every application.

### 5.4 Phase 4: Selection Widgets (Medium-High Priority)

**Data Selection and Display:**
1. **List** - Simple list selection
2. **Combo** - Dropdown selection with text input
3. **Tree** - Hierarchical data
4. **Table** - Tabular data

**Supporting Classes:**
- TreeItem
- TableItem
- TableColumn
- TreeColumn

**Rationale:** Critical for data-driven applications.

### 5.5 Phase 5: Menu System (Medium Priority)

**Application Navigation:**
1. **Menu** - Menu container
2. **MenuItem** - Menu items

**Rationale:** Required for professional applications, but not needed for proof-of-concept.

### 5.6 Phase 6: Advanced Containers (Medium Priority)

**Enhanced Organization:**
1. **TabFolder** / **TabItem** - Tabbed interfaces
2. **ToolBar** / **ToolItem** - Toolbar support
3. **CoolBar** / **CoolItem** - Rearrangeable toolbars
4. **ExpandBar** / **ExpandItem** - Collapsible sections

**Rationale:** Common in modern applications but can be deferred.

### 5.7 Phase 7: Additional Controls (Lower Priority)

**Specialized Widgets:**
1. **ProgressBar** - Progress indication
2. **Spinner** - Numeric input
3. **Scale** / **Slider** - Value selection
4. **DateTime** - Date/time selection
5. **Link** - Hyperlinks

**Rationale:** Useful but not essential for basic functionality.

### 5.8 Phase 8: Advanced Features (Lowest Priority)

**Optional Enhancements:**
1. **Sash** - Resizable dividers
2. **Tracker** - Rubber banding
3. **Caret** - Custom text cursors
4. **ToolTip** - Custom tooltips
5. **Tray** / **TrayItem** - System tray
6. **TaskBar** / **TaskItem** - Taskbar integration
7. **IME** - Input method support

**Rationale:** Platform-specific or rarely used features.

## 6. Key Implementation Notes

### 6.1 Style Bits

SWT uses integer constants for widget styles, combined with bitwise OR:

```java
// Common style bits
SWT.NONE           // No special style
SWT.BORDER         // Add border
SWT.FLAT           // Flat appearance
SWT.SHADOW_IN      // Inset shadow
SWT.SHADOW_OUT     // Outset shadow
SWT.SHADOW_NONE    // No shadow

// Selection modes
SWT.SINGLE         // Single selection
SWT.MULTI          // Multiple selection

// Orientation
SWT.HORIZONTAL     // Horizontal layout
SWT.VERTICAL       // Vertical layout

// Alignment
SWT.LEFT           // Left-aligned
SWT.CENTER         // Center-aligned
SWT.RIGHT          // Right-aligned
SWT.TOP            // Top-aligned
SWT.BOTTOM         // Bottom-aligned

// Text styles
SWT.WRAP           // Word wrap
SWT.READ_ONLY      // Non-editable
SWT.PASSWORD       // Password field

// Window styles
SWT.CLOSE          // Close button
SWT.MIN            // Minimize button
SWT.MAX            // Maximize button
SWT.RESIZE         // Resizable border
SWT.TITLE          // Title bar
SWT.SHELL_TRIM     // Standard window (CLOSE|TITLE|MIN|MAX|RESIZE)
SWT.DIALOG_TRIM    // Dialog window (CLOSE|TITLE|BORDER)

// Modality
SWT.APPLICATION_MODAL  // Block parent window
SWT.PRIMARY_MODAL      // Block all windows
SWT.SYSTEM_MODAL       // Block entire system (use sparingly)
```

**Usage:**
```java
Text text = new Text(parent, SWT.BORDER | SWT.MULTI | SWT.WRAP);
```

### 6.2 Resource Management

**Critical Rules:**
1. **Explicit Disposal:** SWT resources (Colors, Fonts, Images, etc.) must be explicitly disposed
2. **Widget Disposal:** Disposing a widget disposes all its children
3. **Resource Lifetime:** Don't dispose system resources (returned by `getSystemColor()`, etc.)
4. **Display Disposal:** Disposes all resources created on that display

**Pattern:**
```java
Color color = new Color(display, 255, 0, 0);
try {
    // Use color
} finally {
    color.dispose();
}

// Or with widget listeners:
widget.addDisposeListener(e -> {
    color.dispose();
});
```

### 6.3 Threading Model

**Strict Rules:**
1. **UI Thread Only:** All widget access must occur on the UI thread
2. **Display.asyncExec():** Execute code on UI thread from background thread
3. **Display.syncExec():** Execute and wait for completion

**Pattern:**
```java
Display.getDefault().asyncExec(() -> {
    label.setText("Updated from background thread");
});
```

### 6.4 Layout System

**Layout Managers:**
- Automatically position and size child widgets
- Triggered by `composite.layout()`
- Common layouts: GridLayout, FillLayout, RowLayout, FormLayout

**Layout Data:**
- Each widget in a layout can have layout data
- Example: GridData for GridLayout

```java
Composite composite = new Composite(parent, SWT.NONE);
composite.setLayout(new GridLayout(2, false));

Label label = new Label(composite, SWT.NONE);
label.setText("Name:");

Text text = new Text(composite, SWT.BORDER);
text.setLayoutData(new GridData(SWT.FILL, SWT.CENTER, true, false));
```

### 6.5 Native Widget Behavior

**Key Principle:** SWT uses native widgets when possible

**Implications for C# Implementation:**
- On Windows: Use Win32 controls
- On macOS: Use Cocoa/AppKit controls
- On Linux: Use GTK controls
- Behavior may vary slightly by platform
- Test on all target platforms

## 7. Critical API Patterns

### 7.1 Parent-Required Pattern

**All widgets require a parent (except Display and Shell):**

```java
Button button = new Button(parent, SWT.PUSH);  // parent must be Composite
```

### 7.2 Immutable Style Pattern

**Style bits cannot be changed after construction:**

```java
// Correct:
Text text = new Text(parent, SWT.BORDER | SWT.MULTI);

// Wrong - style cannot be changed:
text.setStyle(SWT.READ_ONLY);  // NO SUCH METHOD
```

### 7.3 Dispose Pattern

**Explicit resource cleanup:**

```java
widget.addDisposeListener(e -> {
    // Clean up resources
    image.dispose();
    font.dispose();
});

// Dispose hierarchy:
shell.dispose();  // Disposes all children automatically
```

### 7.4 Bounds vs. Size + Location

**Two ways to set position/size:**

```java
// Method 1: Combined
control.setBounds(10, 20, 100, 50);  // x, y, width, height

// Method 2: Separate
control.setLocation(10, 20);
control.setSize(100, 50);
```

### 7.5 Check-Get-Set Pattern

**Validation before operations:**

```java
if (!widget.isDisposed()) {
    widget.setText("New text");
}
```

## 8. C# Implementation Recommendations

### 8.1 Naming Conventions

**C# Convention Mapping:**
```csharp
// Java: getChildren()
// C#:   Children property
public Control[] Children { get; }

// Java: setText(String text)
// C#:   Text property
public string Text { get; set; }

// Java: addSelectionListener(SelectionListener listener)
// C#:   SelectionChanged event
public event EventHandler<SelectionEventArgs> SelectionChanged;

// Java: dispose()
// C#:   Implement IDisposable
public void Dispose() { }
```

### 8.2 Event Handling

**Use C# events instead of listeners:**

```csharp
// Java pattern:
button.addSelectionListener(new SelectionListener() {
    public void widgetSelected(SelectionEvent e) { }
});

// C# pattern:
button.SelectionChanged += (sender, e) => {
    // Handle event
};
```

### 8.3 Resource Management

**Implement IDisposable pattern:**

```csharp
public abstract class Widget : IDisposable
{
    protected bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            // Dispose unmanaged resources
            disposed = true;
        }
    }

    ~Widget()
    {
        Dispose(false);
    }
}
```

### 8.4 Style Flags

**Use C# enum flags:**

```csharp
[Flags]
public enum SWT
{
    NONE = 0,
    BORDER = 1 << 0,
    PUSH = 1 << 1,
    CHECK = 1 << 2,
    RADIO = 1 << 3,
    // ...
}

// Usage:
var button = new Button(parent, SWT.BORDER | SWT.PUSH);
```

### 8.5 Collections

**Use C# collections:**

```csharp
// Java: Control[] getChildren()
// C#:   ReadOnlyCollection or array
public IReadOnlyList<Control> Children { get; }
```

## 9. Testing Strategy

### 9.1 Unit Testing Priorities

**Phase 1 Tests:**
1. Widget lifecycle (construction, disposal)
2. Parent-child relationships
3. Property get/set operations
4. Event registration/firing

**Phase 2 Tests:**
1. Layout calculations
2. Visibility and enabled state propagation
3. Focus management
4. Coordinate transformations

**Phase 3 Tests:**
1. Widget-specific functionality
2. Selection management
3. Item management
4. Menu behavior

### 9.2 Integration Testing

**Focus Areas:**
1. Platform-specific behavior
2. Event propagation
3. Layout with multiple widgets
4. Resource cleanup

### 9.3 Visual Testing

**Verification:**
1. Widget appearance matches native platform
2. Layout behaves correctly
3. Event handling works as expected
4. Performance is acceptable

## 10. Documentation References

### 10.1 Official SWT Resources

- **Main Repository:** https://github.com/eclipse-platform/eclipse.platform.swt
- **API Documentation:** https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/widgets/package-summary.html
- **Widget Hierarchy:** https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/reference/api/org/eclipse/swt/widgets/package-tree.html
- **SWT Homepage:** https://eclipse.dev/eclipse/swt/

### 10.2 Tutorials and Examples

- **Vogella SWT Tutorial:** https://www.vogella.com/tutorials/SWT/article.html
- **Writing Your Own Widget:** https://www.eclipse.org/articles/Article-Writing Your Own Widget/Writing Your Own Widget.htm
- **Official Widget Screenshots:** https://eclipse.dev/eclipse/swt/widgets/

### 10.3 Key Packages

- `org.eclipse.swt.widgets` - Core widget classes
- `org.eclipse.swt.events` - Event classes and listeners
- `org.eclipse.swt.layout` - Layout managers
- `org.eclipse.swt.graphics` - Graphics resources (Color, Font, Image, etc.)
- `org.eclipse.swt` - SWT constants and utilities

## 11. Summary and Next Steps

### 11.1 Key Findings

1. **Clean Hierarchy:** Widget → Control → Composite forms the core structure
2. **Dual Event System:** Untyped (generic) and typed (specific) listeners coexist
3. **Native Focus:** SWT prioritizes platform-native widgets
4. **Resource Management:** Explicit disposal required for all SWT resources
5. **Thread Safety:** Strict UI thread requirements
6. **Immutable Styles:** Style bits set at construction time

### 11.2 Implementation Roadmap

**Immediate Priority (Phase 1-2):**
- Widget, Control, Composite, Canvas base classes
- Shell and Display for windowing
- Basic layout support

**Short Term (Phase 3-4):**
- Button, Label, Text, Group
- List, Combo, Tree, Table

**Medium Term (Phase 5-6):**
- Menu system
- TabFolder, ToolBar

**Long Term (Phase 7-8):**
- Specialized controls
- Platform-specific features

### 11.3 Critical Success Factors

1. **Accurate Native Bindings:** Proper P/Invoke for Win32/Cocoa/GTK
2. **Memory Management:** Correct dispose pattern implementation
3. **Event System:** Reliable event dispatching
4. **Layout System:** Functional automatic positioning
5. **Testing:** Comprehensive test coverage

### 11.4 Recommended First Implementation

**Minimal Viable Product:**
```csharp
// Goal: Display a window with a button that responds to clicks

Widget (abstract base)
  └─ Control (abstract)
      └─ Composite
          ├─ Canvas
          │   └─ Decorations
          │       └─ Shell
          └─ Button

Display (event loop)
Layout (basic positioning)
Event system (minimal)
```

This will prove the core architecture and platform integration before expanding to the full widget set.

---

**Document Version:** 1.0
**Research Completed:** October 5, 2025
**Next Review:** After Phase 1 implementation
