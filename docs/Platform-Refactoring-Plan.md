# SWTSharp Platform Implementation Refactoring Plan

## Executive Summary

This plan outlines the systematic refactoring of the three main platform implementation files into smaller, more maintainable partial class files. The goal is to reduce file sizes from 3,000+ lines to 200-500 lines per file while maintaining all functionality and improving discoverability.

## Current State

### File Sizes
- **Win32Platform.cs**: 3,249 lines (104KB)
- **MacOSPlatform.cs**: 3,846 lines (134KB)
- **LinuxPlatform.cs**: 3,456 lines (119KB)

### Already Extracted (Partial Classes)
Each platform has the following partial classes already created:
- `{Platform}Platform_Combo.cs` (235 lines)
- `{Platform}Platform_Label.cs` (190 lines)

## Widget Analysis

### Widget Line Count Distribution (Win32Platform as reference)

| Widget Type | Lines | Methods | Complexity | Priority |
|-------------|-------|---------|------------|----------|
| **Dialogs** | 686 | 4 types (MessageBox, File, Directory, Color, Font) | High | 1 |
| **TabFolder + TabItem** | 327 | 7 methods | Medium | 2 |
| **Canvas** | 324 | 4 methods + event handlers | Medium | 3 |
| **Tree + TreeItem** | 301 | 15 methods | High | 4 |
| **Spinner** | 191 | 5 methods | Low | 5 |
| **List** | 159 | 8 methods | Medium | 6 |
| **Table + TableColumn + TableItem** | 146 | 23 methods | High | 7 |
| **ProgressBar** | 103 | 4 methods | Low | 8 |
| **Scale** | 71 | 3 methods | Low | 9 |
| **Slider** | 57 | 3 methods | Low | 10 |
| **Group** | 39 | 2 methods | Low | 11 |
| **Button** | ~200 | Already implemented in main file | Medium | N/A |
| **Text** | ~150 | Already implemented in main file | Medium | N/A |
| **Menu + MenuItem** | ~300 | Already implemented in main file | Medium | N/A |
| **ToolBar + ToolItem** | ~250 | Already implemented in main file | Medium | N/A |

## Proposed File Structure

### Phase 1: High-Priority Extractions (Weeks 1-2)

#### 1. Dialog Operations (686 lines → Split into 4 files)

**File: `{Platform}Platform_MessageBox.cs`** (~150 lines)
- Constants for message box styles
- `ShowMessageBox()` implementation
- Platform-specific message box flags and handling

**File: `{Platform}Platform_FileDialog.cs`** (~200 lines)
- File open/save dialog implementation
- `ShowFileDialog()` and result handling
- Filter parsing and multi-file selection

**File: `{Platform}Platform_DirectoryDialog.cs`** (~150 lines)
- Directory selection dialog
- `ShowDirectoryDialog()` implementation
- Path browsing and validation

**File: `{Platform}Platform_ColorFontDialog.cs`** (~186 lines)
- Color picker dialog (~90 lines)
- Font selection dialog (~90 lines)
- `ShowColorDialog()` and `ShowFontDialog()`

#### 2. TabFolder Operations (327 lines → 1 file)

**File: `{Platform}Platform_TabFolder.cs`** (~330 lines)
- TabFolder constants and structures
- `CreateTabFolder()` (50 lines)
- `SetTabSelection()`, `GetTabSelection()` (30 lines)
- `CreateTabItem()` (60 lines)
- `SetTabItemText()`, `SetTabItemControl()`, `SetTabItemToolTip()` (100 lines)
- Tab switching logic and event handling (90 lines)

#### 3. Canvas Operations (324 lines → 1 file)

**File: `{Platform}Platform_Canvas.cs`** (~330 lines)
- Canvas window class registration
- `CreateCanvas()` implementation (100 lines)
- `ConnectCanvasPaint()` event handling (80 lines)
- `RedrawCanvas()` and `RedrawCanvasArea()` (60 lines)
- Paint message handling and GC integration (90 lines)

### Phase 2: Tree and List Controls (Weeks 3-4)

#### 4. Tree Operations (301 lines → Split into 2 files)

**File: `{Platform}Platform_Tree.cs`** (~160 lines)
- Tree control constants
- `CreateTree()` implementation
- `GetTreeSelection()`, `SetTreeSelection()`
- `ClearTreeItems()`, `ShowTreeItem()`
- Tree control message handling

**File: `{Platform}Platform_TreeItem.cs`** (~145 lines)
- `CreateTreeItem()` implementation
- `DestroyTreeItem()`
- `SetTreeItemText()`, `SetTreeItemImage()`
- `SetTreeItemChecked()`, `GetTreeItemChecked()`
- `SetTreeItemExpanded()`, `GetTreeItemExpanded()`
- `ClearTreeItemChildren()`, `AddTreeItem()`

#### 5. List Control (159 lines → 1 file)

**File: `{Platform}Platform_List.cs`** (~160 lines)
- List control constants
- `CreateList()` implementation
- `AddListItem()`, `RemoveListItem()`, `ClearListItems()`
- `SetListSelection()`, `GetListSelection()`
- `GetListTopIndex()`, `SetListTopIndex()`
- Selection handling logic

### Phase 3: Table Controls (Week 5)

#### 6. Table Operations (146 lines → Split into 3 files)

**File: `{Platform}Platform_Table.cs`** (~50 lines)
- Table control constants
- `CreateTable()` implementation
- `SetTableHeaderVisible()`, `SetTableLinesVisible()`
- `SetTableSelection()`, `ClearTableItems()`, `ShowTableItem()`

**File: `{Platform}Platform_TableColumn.cs`** (~50 lines)
- `CreateTableColumn()`, `DestroyTableColumn()`
- `SetTableColumnText()`, `SetTableColumnWidth()`
- `SetTableColumnAlignment()`, `SetTableColumnResizable()`
- `SetTableColumnMoveable()`, `SetTableColumnToolTipText()`
- `PackTableColumn()`

**File: `{Platform}Platform_TableItem.cs`** (~50 lines)
- `CreateTableItem()`, `DestroyTableItem()`
- `SetTableItemText()`, `SetTableItemImage()`
- `SetTableItemChecked()`, `SetTableItemBackground()`
- `SetTableItemForeground()`, `SetTableItemFont()`

### Phase 4: Simple Controls (Week 6)

#### 7. Spinner Control (191 lines → 1 file)

**File: `{Platform}Platform_Spinner.cs`** (~195 lines)
- Spinner control constants
- `CreateSpinner()` implementation
- `SetSpinnerValues()` (all parameters)
- `SetSpinnerTextLimit()`
- `ConnectSpinnerChanged()`, `ConnectSpinnerModified()`
- Up/down button handling

#### 8. ProgressBar Control (103 lines → 1 file)

**File: `{Platform}Platform_ProgressBar.cs`** (~105 lines)
- ProgressBar constants
- `CreateProgressBar()` implementation
- `SetProgressBarRange()`, `SetProgressBarSelection()`
- `SetProgressBarState()` (normal, error, paused)
- Indeterminate mode handling

#### 9. Scale Control (71 lines → 1 file)

**File: `{Platform}Platform_Scale.cs`** (~75 lines)
- Scale control constants
- `CreateScale()` implementation
- `SetScaleValues()` (all parameters)
- `ConnectScaleChanged()` event handling

#### 10. Slider Control (57 lines → 1 file)

**File: `{Platform}Platform_Slider.cs`** (~60 lines)
- Slider control constants
- `CreateSlider()` implementation
- `SetSliderValues()` (all parameters)
- `ConnectSliderChanged()` event handling

#### 11. Group Control (39 lines → 1 file)

**File: `{Platform}Platform_Group.cs`** (~40 lines)
- Group box constants
- `CreateGroup()` implementation
- `SetGroupText()`

### Phase 5: Extract Remaining Widgets (Week 7)

#### 12. Button Control

**File: `{Platform}Platform_Button.cs`** (~200 lines)
- Extract from main file
- Button constants and styles
- `CreateButton()` implementation
- `SetButtonText()`, `SetButtonSelection()`, `GetButtonSelection()`
- `ConnectButtonClick()` event handling

#### 13. Text Control

**File: `{Platform}Platform_Text.cs`** (~150 lines)
- Extract from main file
- Text control constants
- `CreateText()` implementation
- `SetTextContent()`, `GetTextContent()`
- `SetTextSelection()`, `GetTextSelection()`
- `SetTextLimit()`, `SetTextReadOnly()`

#### 14. Menu and MenuItem

**File: `{Platform}Platform_Menu.cs`** (~150 lines)
- Extract from main file
- Menu constants and structures
- `CreateMenu()`, `DestroyMenu()`
- `SetShellMenuBar()`, `SetMenuVisible()`
- `ShowPopupMenu()`

**File: `{Platform}Platform_MenuItem.cs`** (~150 lines)
- Extract from main file
- MenuItem constants
- `CreateMenuItem()`, `DestroyMenuItem()`
- `SetMenuItemText()`, `SetMenuItemSelection()`
- `SetMenuItemEnabled()`, `SetMenuItemSubmenu()`

#### 15. ToolBar and ToolItem

**File: `{Platform}Platform_ToolBar.cs`** (~100 lines)
- Extract from main file
- ToolBar constants
- `CreateToolBar()` implementation
- ToolBar layout and positioning

**File: `{Platform}Platform_ToolItem.cs`** (~150 lines)
- Extract from main file
- ToolItem constants
- `CreateToolItem()`, `DestroyToolItem()`
- `SetToolItemText()`, `SetToolItemImage()`, `SetToolItemToolTip()`
- `SetToolItemSelection()`, `SetToolItemEnabled()`
- `SetToolItemWidth()`, `SetToolItemControl()`

## What Remains in Main Platform File

After all extractions, the main `{Platform}Platform.cs` file should contain (~800-1000 lines):

### Core Infrastructure (300-400 lines)
1. **P/Invoke Declarations** (~200 lines)
   - Common Win32/macOS/GTK API imports
   - Shared structures (MSG, POINT, RECT, etc.)
   - Common constants (WS_*, WM_*, etc.)

2. **Class Setup and Initialization** (~100 lines)
   - Class declaration and namespace
   - Instance fields (_hInstance, delegates, etc.)
   - `Initialize()` method
   - Window class registration
   - WndProc delegate setup

### Core Window Operations (200-300 lines)
3. **Window Management** (~150 lines)
   - `CreateWindow()`, `DestroyWindow()`
   - `SetWindowVisible()`, `SetWindowText()`
   - `SetWindowSize()`, `SetWindowLocation()`
   - Window message handling

4. **Event Loop** (~100 lines)
   - `ProcessEvent()`, `WaitForEvent()`
   - `WakeEventLoop()`
   - Message pump implementation

### Container and Control Management (200-300 lines)
5. **Composite/Container** (~50 lines)
   - `CreateComposite()` implementation

6. **Common Control Operations** (~150 lines)
   - `SetControlEnabled()`, `SetControlVisible()`
   - `SetControlBounds()`
   - Shared helper methods

7. **Graphics Integration** (~50 lines)
   - Graphics context creation
   - Integration with IPlatformGraphics

## Implementation Strategy

### Refactoring Process for Each Widget

1. **Preparation Phase**
   - Create new partial class file
   - Copy namespace and class declaration
   - Add XML documentation header

2. **Extraction Phase**
   - Identify all constants used by the widget
   - Copy all P/Invoke declarations needed
   - Extract all widget-specific methods
   - Ensure dependencies are included

3. **Validation Phase**
   - Compile and verify no errors
   - Run existing unit tests
   - Check for missing dependencies
   - Verify method signatures match interface

4. **Cleanup Phase**
   - Remove extracted code from main file
   - Update documentation
   - Add code comments where needed

### Template for New Partial Files

```csharp
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// {Platform} {Widget} implementation - partial class extension
/// </summary>
internal partial class {Platform}Platform
{
    // Widget-specific constants
    // ...

    // Widget-specific P/Invoke declarations (if needed)
    // ...

    // Widget operations
    // ...
}
```

## Benefits of This Refactoring

### 1. **Improved Maintainability**
- Easier to find and modify widget-specific code
- Reduced cognitive load when working on specific widgets
- Better organization follows Single Responsibility Principle

### 2. **Enhanced Discoverability**
- File names clearly indicate widget type
- Related functionality grouped together
- Easier navigation in IDE

### 3. **Better Testing**
- Can focus unit tests on specific widget files
- Easier to identify coverage gaps
- Simplified debugging

### 4. **Reduced Merge Conflicts**
- Multiple developers can work on different widgets simultaneously
- Smaller files reduce likelihood of conflicts
- Changes isolated to specific widget concerns

### 5. **Documentation Clarity**
- Each file has focused XML documentation
- Platform-specific notes can be more detailed
- Examples can be widget-specific

## Success Metrics

1. **File Size Compliance**: All partial files between 200-500 lines
2. **Zero Functionality Loss**: All existing tests pass
3. **Build Success**: No compilation errors across all platforms
4. **Code Coverage**: Maintain or improve test coverage
5. **Performance**: No regression in platform operation performance

## Timeline

- **Week 1-2**: Phase 1 (Dialogs, TabFolder, Canvas) - 3 widgets
- **Week 3-4**: Phase 2 (Tree, List) - 2 widgets
- **Week 5**: Phase 3 (Table components) - 1 widget family
- **Week 6**: Phase 4 (Simple controls: Spinner, ProgressBar, Scale, Slider, Group) - 5 widgets
- **Week 7**: Phase 5 (Extract remaining: Button, Text, Menu, ToolBar) - 4 widgets
- **Week 8**: Final validation, documentation, cleanup

## Risk Mitigation

### Risks
1. **Breaking existing functionality**: Mitigated by comprehensive testing after each extraction
2. **Missing dependencies**: Mitigated by careful analysis before extraction
3. **Platform inconsistencies**: Mitigated by applying same pattern across all three platforms
4. **Performance regression**: Mitigated by benchmarking before/after

### Rollback Plan
- Use Git feature branches for each widget extraction
- Tag working state before each phase
- Keep main branch stable with working code

## Post-Refactoring Structure

```
SWTSharp/Platform/
├── IPlatform.cs
├── PlatformFactory.cs
│
├── Win32Platform.cs (800-1000 lines) - Core infrastructure
├── Win32Platform_Button.cs (200 lines)
├── Win32Platform_Canvas.cs (330 lines)
├── Win32Platform_Combo.cs (235 lines) ✓ Exists
├── Win32Platform_ColorFontDialog.cs (186 lines)
├── Win32Platform_DirectoryDialog.cs (150 lines)
├── Win32Platform_FileDialog.cs (200 lines)
├── Win32Platform_Group.cs (40 lines)
├── Win32Platform_Label.cs (190 lines) ✓ Exists
├── Win32Platform_List.cs (160 lines)
├── Win32Platform_Menu.cs (150 lines)
├── Win32Platform_MenuItem.cs (150 lines)
├── Win32Platform_MessageBox.cs (150 lines)
├── Win32Platform_ProgressBar.cs (105 lines)
├── Win32Platform_Scale.cs (75 lines)
├── Win32Platform_Slider.cs (60 lines)
├── Win32Platform_Spinner.cs (195 lines)
├── Win32Platform_TabFolder.cs (330 lines)
├── Win32Platform_Table.cs (50 lines)
├── Win32Platform_TableColumn.cs (50 lines)
├── Win32Platform_TableItem.cs (50 lines)
├── Win32Platform_Text.cs (150 lines)
├── Win32Platform_ToolBar.cs (100 lines)
├── Win32Platform_ToolItem.cs (150 lines)
├── Win32Platform_Tree.cs (160 lines)
├── Win32Platform_TreeItem.cs (145 lines)
│
├── MacOSPlatform.cs (800-1000 lines) - Core infrastructure
├── MacOSPlatform_Button.cs
├── MacOSPlatform_Canvas.cs
... (same pattern as Win32)
│
├── LinuxPlatform.cs (800-1000 lines) - Core infrastructure
├── LinuxPlatform_Button.cs
├── LinuxPlatform_Canvas.cs
... (same pattern as Win32)
│
├── Win32/
│   ├── Win32PlatformGraphics.cs
│   └── ... (SafeHandles, etc.)
│
├── MacOS/
│   ├── MacOSPlatformGraphics.cs
│   └── ... (SafeHandles, etc.)
│
└── Linux/
    ├── LinuxPlatformGraphics.cs
    └── ... (SafeHandles, etc.)
```

## Conclusion

This refactoring plan will transform the SWTSharp platform implementation from three monolithic files into a well-organized, maintainable structure with 25+ focused partial class files per platform. Each file will have a clear, single purpose and be small enough to understand quickly, while maintaining all existing functionality and improving code quality.

The systematic approach with phased implementation ensures low risk and maintains project stability throughout the refactoring process.
