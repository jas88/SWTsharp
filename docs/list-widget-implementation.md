# List Widget Implementation for macOS

## Overview
This document describes the implementation of the List widget for the macOS platform in SWTSharp. The List widget provides a selectable list of string items with support for single and multiple selection modes.

## Implementation Details

### File Modified
- `/Users/jas88/Developer/swtsharp/src/SWTSharp/Platform/MacOSPlatform.cs`

### Native Components Used
The implementation uses the following macOS/Cocoa components:

1. **NSScrollView** - Container that provides scrolling functionality
2. **NSTableView** - The actual list display component (configured with a single column)
3. **NSTableColumn** - Single column to display list items
4. **NSIndexSet** - Manages selected row indices
5. **NSMutableIndexSet** - For building multiple selections

### Architecture

#### Data Storage
- List items are stored in a `Dictionary<IntPtr, List<string>> _listItems`
- The key is the scroll view handle (the widget handle returned from CreateList)
- The value is a List<string> containing the actual list items
- This allows multiple List widgets to be managed independently

#### Widget Structure
```
NSScrollView (returned as handle)
  └─ NSTableView (document view)
       └─ NSTableColumn (single column, header hidden)
```

### Methods Implemented

#### 1. CreateList(IntPtr parentHandle, int style)
Creates a new List widget with the specified style.

**Process:**
1. Initialize list selectors (lazy initialization)
2. Create NSScrollView
3. Create NSTableView
4. Create single NSTableColumn with identifier "ListColumn"
5. Configure column width (200px default, min 50px, max 10000px)
6. Add column to table view
7. Hide header (setHeaderView: nil)
8. Configure selection mode based on SWT.SINGLE or SWT.MULTI style
9. Configure scroll view with vertical scrolling
10. Add table view as document view
11. Add to parent window if provided
12. Initialize empty items list in dictionary

**Returns:** Handle to NSScrollView

#### 2. AddListItem(IntPtr handle, string item, int index)
Adds an item to the list at the specified index.

**Process:**
1. Get or create items list from dictionary
2. Insert item at index (or append if index is out of range)
3. Get NSTableView from scroll view
4. Call reloadData to refresh display

**Note:** In a full implementation, this would use NSTableView data source delegates for better performance with large lists.

#### 3. RemoveListItem(IntPtr handle, int index)
Removes the item at the specified index.

**Process:**
1. Get items list from dictionary
2. Remove item at index
3. Get NSTableView from scroll view
4. Call reloadData to refresh display

#### 4. ClearListItems(IntPtr handle)
Removes all items from the list.

**Process:**
1. Get items list from dictionary
2. Clear the list
3. Get NSTableView from scroll view
4. Call reloadData to refresh display

#### 5. SetListSelection(IntPtr handle, int[] indices)
Sets the selected items by their indices.

**Process:**
1. Get NSTableView from scroll view
2. Handle three cases:
   - **Empty array**: Create empty NSIndexSet and deselect all
   - **Single index**: Use `indexSetWithIndex:` for efficiency
   - **Multiple indices**: Create NSMutableIndexSet and add each index
3. Call `selectRowIndexes:byExtendingSelection:` with byExtendingSelection=false

**Selection Modes:**
- SWT.SINGLE: Only allows single selection (enforced by NSTableView)
- SWT.MULTI: Allows multiple selections

#### 6. GetListSelection(IntPtr handle)
Gets the currently selected indices.

**Process:**
1. Get NSTableView from scroll view
2. Get selectedRowIndexes (returns NSIndexSet)
3. Get count of selected items
4. Iterate through index set:
   - Start with firstIndex
   - Use indexGreaterThanIndex: to iterate
   - Continue until NSNotFound (nuint.MaxValue)
5. Return array of indices

**Returns:** int[] array of selected indices (empty array if none selected)

#### 7. GetListTopIndex(IntPtr handle)
Gets the index of the first visible item (top of viewport).

**Process:**
1. Get NSTableView from scroll view
2. Get visibleRect of the table view
3. Create NSPoint at top-left of visible rect
4. Call `rowAtPoint:` to get the row index
5. Return the row index (or 0 if negative)

**Returns:** Index of top visible row

#### 8. SetListTopIndex(IntPtr handle, int index)
Scrolls the list to make the specified index the top visible item.

**Process:**
1. Get NSTableView from scroll view
2. Call `scrollRowToVisible:` with the specified index

**Note:** This scrolls the row into view but doesn't guarantee it's at the exact top.

### Objective-C Runtime Integration

The implementation uses P/Invoke to call Objective-C runtime functions:

#### New DllImport Signatures
```csharp
[DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
private static extern IntPtr objc_msgSend_ulong(IntPtr receiver, IntPtr selector, nuint arg1);

[DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
private static extern nuint objc_msgSend_ret_ulong(IntPtr receiver, IntPtr selector);

[DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
private static extern void objc_msgSend_double_arg(IntPtr receiver, IntPtr selector, double arg1);
```

#### New Structure
```csharp
[StructLayout(LayoutKind.Sequential)]
private struct NSPoint
{
    public double x;
    public double y;
}
```

### Selectors Registered

The following Objective-C selectors are registered and cached:

- `reloadData` - Refresh table view display
- `selectRowIndexes:byExtendingSelection:` - Set selection
- `selectedRowIndexes` - Get current selection
- `scrollRowToVisible:` - Scroll to row
- `addTableColumn:` - Add column to table
- `setAllowsMultipleSelection:` - Configure selection mode
- `setHeaderView:` - Hide/show header
- `initWithIdentifier:` - Initialize table column
- `rowAtPoint:` - Get row at coordinate
- `visibleRect` - Get visible area
- `setWidth:` - Set column width
- `setMinWidth:` - Set minimum column width
- `setMaxWidth:` - Set maximum column width
- `setHasVerticalScroller:` - Enable vertical scrolling
- `setDocumentView:` - Set scroll view content
- `setAutohidesScrollers:` - Auto-hide scrollbars
- `indexSetWithIndex:` - Create index set with single index
- `firstIndex` - Get first index in set
- `indexGreaterThanIndex:` - Get next index in set
- `count` - Get count of items
- `addIndex:` - Add index to mutable index set

### Memory Management

#### Resource Cleanup
Two cleanup methods are provided:

```csharp
public void ClearListItems()
{
    _listItems.Clear();
}

public void RemoveListItemsForControl(IntPtr handle)
{
    _listItems.Remove(handle);
}
```

These should be called when:
- ClearListItems: When disposing the entire platform
- RemoveListItemsForControl: When a specific List widget is disposed

## Limitations and Future Enhancements

### Current Limitations

1. **No Data Source Delegate**: The implementation uses reloadData for all updates, which is inefficient for large lists. A proper implementation would use NSTableViewDataSource protocol.

2. **No Selection Change Events**: User-initiated selection changes don't trigger callbacks to the C# layer. This requires implementing NSTableViewDelegate.

3. **No Custom Cell Rendering**: All items are displayed as plain text. SWT List doesn't support custom rendering, so this is acceptable.

4. **Simplified Scrolling**: SetListTopIndex scrolls the row into view but doesn't guarantee exact positioning at the top.

### Recommended Enhancements

1. **Implement NSTableViewDataSource**: Create a custom data source class that bridges to the C# items dictionary.

2. **Implement NSTableViewDelegate**: Handle selection changes and notify the C# layer through callbacks.

3. **Optimize Refresh**: Use `insertRowsAtIndexes:` and `removeRowsAtIndexes:` instead of reloadData for better performance.

4. **Better Error Handling**: Add validation for nil pointers and invalid indices.

## Testing

A test file is provided at `/Users/jas88/Developer/swtsharp/tests/ListTest.cs` that demonstrates:

- Creating List widgets with SINGLE and MULTI selection modes
- Adding items
- Setting selections
- Getting selections
- Setting top index
- Displaying the widgets in a window

To run the test (macOS only):
```bash
cd /Users/jas88/Developer/swtsharp/tests
dotnet run --project ListTest.cs
```

## Compatibility

- **Platform**: macOS only (uses Cocoa/AppKit)
- **Frameworks**: .NET Standard 2.0, .NET 8.0, .NET 9.0
- **Native APIs**: Objective-C runtime via P/Invoke
- **SWT Styles Supported**:
  - SWT.SINGLE - Single selection mode
  - SWT.MULTI - Multiple selection mode
  - SWT.BORDER - Visual border (automatic with NSScrollView)

## References

- Apple NSTableView Documentation: https://developer.apple.com/documentation/appkit/nstableview
- Apple NSScrollView Documentation: https://developer.apple.com/documentation/appkit/nsscrollview
- Apple NSIndexSet Documentation: https://developer.apple.com/documentation/foundation/nsindexset
- SWT List Documentation: https://www.eclipse.org/swt/widgets/
