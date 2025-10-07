namespace SWTSharp.Platform;

/// <summary>
/// Interface for platform-specific implementations.
/// </summary>
public partial interface IPlatform
{
    /// <summary>
    /// Initializes the platform.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Creates a native window handle.
    /// </summary>
    IntPtr CreateWindow(int style, string title);

    /// <summary>
    /// Destroys a native window handle.
    /// </summary>
    void DestroyWindow(IntPtr handle);

    /// <summary>
    /// Shows or hides a window.
    /// </summary>
    void SetWindowVisible(IntPtr handle, bool visible);

    /// <summary>
    /// Sets the window title.
    /// </summary>
    void SetWindowText(IntPtr handle, string text);

    /// <summary>
    /// Sets the window size.
    /// </summary>
    void SetWindowSize(IntPtr handle, int width, int height);

    /// <summary>
    /// Sets the window location.
    /// </summary>
    void SetWindowLocation(IntPtr handle, int x, int y);

    /// <summary>
    /// Processes a single event from the event queue.
    /// </summary>
    bool ProcessEvent();

    /// <summary>
    /// Waits for the next event.
    /// </summary>
    void WaitForEvent();

    /// <summary>
    /// Wakes up the event loop.
    /// </summary>
    void WakeEventLoop();

    // Container operations

    /// <summary>
    /// Creates a native composite/container control.
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Composite style bits</param>
    IntPtr CreateComposite(IntPtr parent, int style);

    // Menu operations

    /// <summary>
    /// Creates a native menu handle.
    /// </summary>
    /// <param name="style">Menu style (BAR, DROP_DOWN, or POP_UP)</param>
    IntPtr CreateMenu(int style);

    /// <summary>
    /// Destroys a native menu handle.
    /// </summary>
    void DestroyMenu(IntPtr handle);

    /// <summary>
    /// Sets the menu bar for a shell.
    /// </summary>
    void SetShellMenuBar(IntPtr shellHandle, IntPtr menuHandle);

    /// <summary>
    /// Shows or hides a menu.
    /// </summary>
    void SetMenuVisible(IntPtr handle, bool visible);

    /// <summary>
    /// Shows a popup menu at the specified screen coordinates.
    /// </summary>
    void ShowPopupMenu(IntPtr menuHandle, int x, int y);

    // Menu item operations

    /// <summary>
    /// Creates a native menu item handle.
    /// </summary>
    /// <param name="menuHandle">Parent menu handle</param>
    /// <param name="style">Menu item style (PUSH, CHECK, RADIO, CASCADE, SEPARATOR)</param>
    /// <param name="id">Unique ID for the menu item</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    IntPtr CreateMenuItem(IntPtr menuHandle, int style, int id, int index);

    /// <summary>
    /// Destroys a native menu item handle.
    /// </summary>
    void DestroyMenuItem(IntPtr handle);

    /// <summary>
    /// Sets the text of a menu item.
    /// </summary>
    void SetMenuItemText(IntPtr handle, string text);

    /// <summary>
    /// Sets the selection state of a CHECK or RADIO menu item.
    /// </summary>
    void SetMenuItemSelection(IntPtr handle, bool selected);

    /// <summary>
    /// Sets the enabled state of a menu item.
    /// </summary>
    void SetMenuItemEnabled(IntPtr handle, bool enabled);

    /// <summary>
    /// Sets the submenu for a CASCADE menu item.
    /// </summary>
    void SetMenuItemSubmenu(IntPtr itemHandle, IntPtr submenuHandle);

    // List control operations

    /// <summary>
    /// Creates a native list control.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="style">List style (SINGLE or MULTI)</param>
    IntPtr CreateList(IntPtr parentHandle, int style);

    /// <summary>
    /// Adds an item to a list control.
    /// </summary>
    void AddListItem(IntPtr handle, string item, int index);

    /// <summary>
    /// Removes an item from a list control.
    /// </summary>
    void RemoveListItem(IntPtr handle, int index);

    /// <summary>
    /// Clears all items from a list control.
    /// </summary>
    void ClearListItems(IntPtr handle);

    /// <summary>
    /// Sets the selection in a list control.
    /// </summary>
    void SetListSelection(IntPtr handle, int[] indices);

    /// <summary>
    /// Gets the current selection from a list control.
    /// </summary>
    int[] GetListSelection(IntPtr handle);

    /// <summary>
    /// Gets the top index (first visible item) in a list control.
    /// </summary>
    int GetListTopIndex(IntPtr handle);

    /// <summary>
    /// Sets the top index (first visible item) in a list control.
    /// </summary>
    void SetListTopIndex(IntPtr handle, int index);

    // Combo control operations

    /// <summary>
    /// Creates a native combo control.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="style">Combo style (DROP_DOWN, READ_ONLY, SIMPLE)</param>
    IntPtr CreateCombo(IntPtr parentHandle, int style);

    /// <summary>
    /// Sets the text in a combo control.
    /// </summary>
    void SetComboText(IntPtr handle, string text);

    /// <summary>
    /// Gets the text from a combo control.
    /// </summary>
    string GetComboText(IntPtr handle);

    /// <summary>
    /// Adds an item to a combo control.
    /// </summary>
    void AddComboItem(IntPtr handle, string item, int index);

    /// <summary>
    /// Removes an item from a combo control.
    /// </summary>
    void RemoveComboItem(IntPtr handle, int index);

    /// <summary>
    /// Clears all items from a combo control.
    /// </summary>
    void ClearComboItems(IntPtr handle);

    /// <summary>
    /// Sets the selection in a combo control.
    /// </summary>
    void SetComboSelection(IntPtr handle, int index);

    /// <summary>
    /// Gets the current selection index from a combo control.
    /// </summary>
    int GetComboSelection(IntPtr handle);

    /// <summary>
    /// Sets the text limit for a combo control.
    /// </summary>
    void SetComboTextLimit(IntPtr handle, int limit);

    /// <summary>
    /// Sets the number of visible items in the drop-down.
    /// </summary>
    void SetComboVisibleItemCount(IntPtr handle, int count);

    /// <summary>
    /// Sets the text selection range in a combo control.
    /// </summary>
    void SetComboTextSelection(IntPtr handle, int start, int end);

    /// <summary>
    /// Gets the text selection range from a combo control.
    /// </summary>
    (int Start, int End) GetComboTextSelection(IntPtr handle);

    /// <summary>
    /// Copies selected text to clipboard.
    /// </summary>
    void ComboTextCopy(IntPtr handle);

    /// <summary>
    /// Cuts selected text to clipboard.
    /// </summary>
    void ComboTextCut(IntPtr handle);

    /// <summary>
    /// Pastes text from clipboard.
    /// </summary>
    void ComboTextPaste(IntPtr handle);

    // Button control operations

    /// <summary>
    /// Creates a native button control.
    /// </summary>
    IntPtr CreateButton(IntPtr parent, int style, string text);

    /// <summary>
    /// Sets the button's text.
    /// </summary>
    void SetButtonText(IntPtr handle, string text);

    /// <summary>
    /// Sets the button's selection state (for CHECK, RADIO, and TOGGLE buttons).
    /// </summary>
    void SetButtonSelection(IntPtr handle, bool selected);

    /// <summary>
    /// Gets the button's selection state.
    /// </summary>
    bool GetButtonSelection(IntPtr handle);

    /// <summary>
    /// Sets the control's enabled state.
    /// </summary>
    void SetControlEnabled(IntPtr handle, bool enabled);

    /// <summary>
    /// Sets the control's visibility.
    /// </summary>
    void SetControlVisible(IntPtr handle, bool visible);

    /// <summary>
    /// Sets the control's bounds.
    /// </summary>
    void SetControlBounds(IntPtr handle, int x, int y, int width, int height);

    /// <summary>
    /// Connects a button click event handler.
    /// </summary>
    void ConnectButtonClick(IntPtr handle, Action callback);

    // Label operations
    /// <summary>
    /// Creates a native label control.
    /// </summary>
    IntPtr CreateLabel(IntPtr parent, int style, int alignment, bool wrap);

    /// <summary>
    /// Sets the label's text content.
    /// </summary>
    void SetLabelText(IntPtr handle, string text);

    /// <summary>
    /// Sets the label's text alignment.
    /// </summary>
    void SetLabelAlignment(IntPtr handle, int alignment);

    // Text control operations
    /// <summary>
    /// Creates a native text control.
    /// </summary>
    IntPtr CreateText(IntPtr parent, int style);

    /// <summary>
    /// Sets the text control's content.
    /// </summary>
    void SetTextContent(IntPtr handle, string text);

    /// <summary>
    /// Gets the text control's content.
    /// </summary>
    string GetTextContent(IntPtr handle);

    /// <summary>
    /// Sets the text selection range.
    /// </summary>
    void SetTextSelection(IntPtr handle, int start, int end);

    /// <summary>
    /// Gets the text selection range.
    /// </summary>
    (int Start, int End) GetTextSelection(IntPtr handle);

    /// <summary>
    /// Sets the text limit.
    /// </summary>
    void SetTextLimit(IntPtr handle, int limit);

    /// <summary>
    /// Sets the text read-only state.
    /// </summary>
    void SetTextReadOnly(IntPtr handle, bool readOnly);

    // Group control operations

    /// <summary>
    /// Creates a native group box control.
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Group style (SHADOW_IN, SHADOW_OUT, SHADOW_ETCHED_IN, SHADOW_ETCHED_OUT, SHADOW_NONE)</param>
    /// <param name="text">Group title text</param>
    /// <returns>Handle to the created group box</returns>
    IntPtr CreateGroup(IntPtr parent, int style, string text);

    /// <summary>
    /// Sets the group box's title text.
    /// </summary>
    /// <param name="handle">Group box handle</param>
    /// <param name="text">New title text</param>
    void SetGroupText(IntPtr handle, string text);

    // Canvas control operations

    /// <summary>
    /// Creates a native canvas (drawable surface) control.
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Canvas style bits</param>
    /// <returns>Handle to the created canvas</returns>
    IntPtr CreateCanvas(IntPtr parent, int style);

    /// <summary>
    /// Connects a paint event handler to a canvas.
    /// </summary>
    /// <param name="handle">Canvas handle</param>
    /// <param name="callback">Callback receiving (x, y, width, height, gc)</param>
    void ConnectCanvasPaint(IntPtr handle, Action<int, int, int, int, object?> callback);

    /// <summary>
    /// Forces the entire canvas to redraw.
    /// </summary>
    /// <param name="handle">Canvas handle</param>
    void RedrawCanvas(IntPtr handle);

    /// <summary>
    /// Forces a specific area of the canvas to redraw.
    /// </summary>
    /// <param name="handle">Canvas handle</param>
    /// <param name="x">X coordinate of area to redraw</param>
    /// <param name="y">Y coordinate of area to redraw</param>
    /// <param name="width">Width of area to redraw</param>
    /// <param name="height">Height of area to redraw</param>
    void RedrawCanvasArea(IntPtr handle, int x, int y, int width, int height);

    // ToolBar operations

    /// <summary>
    /// Creates a native toolbar control.
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Toolbar style (FLAT, WRAP, RIGHT, HORIZONTAL, VERTICAL, SHADOW_OUT)</param>
    /// <returns>Handle to the created toolbar</returns>
    IntPtr CreateToolBar(IntPtr parent, int style);

    // ToolItem operations

    /// <summary>
    /// Creates a native toolbar item handle.
    /// </summary>
    /// <param name="toolBarHandle">Parent toolbar handle</param>
    /// <param name="style">Tool item style (PUSH, CHECK, RADIO, DROP_DOWN, SEPARATOR)</param>
    /// <param name="id">Unique ID for the tool item</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    /// <returns>Handle to the created tool item</returns>
    IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index);

    /// <summary>
    /// Destroys a native tool item handle.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    void DestroyToolItem(IntPtr handle);

    /// <summary>
    /// Sets the text of a tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="text">Text to display</param>
    void SetToolItemText(IntPtr handle, string text);

    /// <summary>
    /// Sets the image of a tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="imageHandle">Image handle (or IntPtr.Zero to clear)</param>
    void SetToolItemImage(IntPtr handle, IntPtr imageHandle);

    /// <summary>
    /// Sets the tooltip text of a tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="text">Tooltip text</param>
    void SetToolItemToolTip(IntPtr handle, string text);

    /// <summary>
    /// Sets the selection state of a CHECK or RADIO tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="selected">Whether the item is selected</param>
    void SetToolItemSelection(IntPtr handle, bool selected);

    /// <summary>
    /// Sets the enabled state of a tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="enabled">Whether the item is enabled</param>
    void SetToolItemEnabled(IntPtr handle, bool enabled);

    /// <summary>
    /// Sets the width of a SEPARATOR tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="width">Width in pixels</param>
    void SetToolItemWidth(IntPtr handle, int width);

    /// <summary>
    /// Sets the control for a custom tool item.
    /// </summary>
    /// <param name="handle">Tool item handle</param>
    /// <param name="controlHandle">Control handle to embed</param>
    void SetToolItemControl(IntPtr handle, IntPtr controlHandle);

    // TabFolder control operations

    /// <summary>
    /// Creates a native tab folder control.
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Tab folder style (SWT.TOP, SWT.BOTTOM for tab position)</param>
    /// <returns>Handle to the created tab folder</returns>
    IntPtr CreateTabFolder(IntPtr parent, int style);

    /// <summary>
    /// Sets the selected tab by index.
    /// </summary>
    /// <param name="handle">Tab folder handle</param>
    /// <param name="index">Index of tab to select, or -1 for no selection</param>
    void SetTabSelection(IntPtr handle, int index);

    /// <summary>
    /// Gets the currently selected tab index.
    /// </summary>
    /// <param name="handle">Tab folder handle</param>
    /// <returns>Index of selected tab, or -1 if no tab is selected</returns>
    int GetTabSelection(IntPtr handle);

    // TabItem operations

    /// <summary>
    /// Creates a native tab item within a tab folder.
    /// </summary>
    /// <param name="parentHandle">Parent tab folder handle</param>
    /// <param name="style">Tab item style</param>
    /// <param name="index">Index at which to insert the tab, or -1 to append</param>
    /// <returns>Handle to the created tab item</returns>
    IntPtr CreateTabItem(IntPtr parentHandle, int style, int index);

    /// <summary>
    /// Sets the text displayed on a tab item.
    /// </summary>
    /// <param name="handle">Tab item handle</param>
    /// <param name="text">Text to display on the tab</param>
    void SetTabItemText(IntPtr handle, string text);

    /// <summary>
    /// Sets the control displayed when a tab is selected.
    /// </summary>
    /// <param name="handle">Tab item handle</param>
    /// <param name="controlHandle">Handle to the control to display, or IntPtr.Zero for no control</param>
    void SetTabItemControl(IntPtr handle, IntPtr controlHandle);

    /// <summary>
    /// Sets the tooltip text for a tab item.
    /// </summary>
    /// <param name="handle">Tab item handle</param>
    /// <param name="text">Tooltip text</param>
    void SetTabItemToolTip(IntPtr handle, string text);

    // Tree control operations

    /// <summary>
    /// Creates a native tree control.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="style">Tree style (SINGLE, MULTI, CHECK)</param>
    /// <returns>Handle to the created tree control</returns>
    IntPtr CreateTree(IntPtr parentHandle, int style);

    /// <summary>
    /// Gets the current selection from a tree control.
    /// </summary>
    /// <param name="handle">Tree control handle</param>
    /// <returns>Array of selected item handles</returns>
    IntPtr[] GetTreeSelection(IntPtr handle);

    /// <summary>
    /// Sets the selection in a tree control.
    /// </summary>
    /// <param name="handle">Tree control handle</param>
    /// <param name="itemHandles">Array of item handles to select</param>
    void SetTreeSelection(IntPtr handle, IntPtr[] itemHandles);

    /// <summary>
    /// Clears all items from a tree control.
    /// </summary>
    /// <param name="handle">Tree control handle</param>
    void ClearTreeItems(IntPtr handle);

    /// <summary>
    /// Shows the specified tree item, scrolling if necessary.
    /// </summary>
    /// <param name="treeHandle">Tree control handle</param>
    /// <param name="itemHandle">Item handle to show</param>
    void ShowTreeItem(IntPtr treeHandle, IntPtr itemHandle);

    // TreeItem operations

    /// <summary>
    /// Creates a native tree item.
    /// </summary>
    /// <param name="treeHandle">Parent tree handle</param>
    /// <param name="parentItemHandle">Parent item handle (IntPtr.Zero for root items)</param>
    /// <param name="style">Item style flags</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    /// <returns>Handle to the created tree item</returns>
    IntPtr CreateTreeItem(IntPtr treeHandle, IntPtr parentItemHandle, int style, int index);

    /// <summary>
    /// Destroys a native tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    void DestroyTreeItem(IntPtr handle);

    /// <summary>
    /// Sets the text of a tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    /// <param name="text">Text to display</param>
    void SetTreeItemText(IntPtr handle, string text);

    /// <summary>
    /// Sets the image of a tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    /// <param name="imageHandle">Image handle (or IntPtr.Zero to clear)</param>
    void SetTreeItemImage(IntPtr handle, IntPtr imageHandle);

    /// <summary>
    /// Sets the checked state of a tree item (only valid if tree has CHECK style).
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    /// <param name="checked">Whether the item is checked</param>
    void SetTreeItemChecked(IntPtr handle, bool @checked);

    /// <summary>
    /// Gets the checked state of a tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    /// <returns>Whether the item is checked</returns>
    bool GetTreeItemChecked(IntPtr handle);

    /// <summary>
    /// Sets the expanded state of a tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    /// <param name="expanded">Whether the item is expanded</param>
    void SetTreeItemExpanded(IntPtr handle, bool expanded);

    /// <summary>
    /// Gets the expanded state of a tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    /// <returns>Whether the item is expanded</returns>
    bool GetTreeItemExpanded(IntPtr handle);

    /// <summary>
    /// Clears all child items from a tree item.
    /// </summary>
    /// <param name="handle">Tree item handle</param>
    void ClearTreeItemChildren(IntPtr handle);

    /// <summary>
    /// Adds a tree item to the tree structure.
    /// </summary>
    /// <param name="treeHandle">Tree control handle</param>
    /// <param name="itemHandle">Item handle to add</param>
    /// <param name="parentItemHandle">Parent item handle (IntPtr.Zero for root)</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    void AddTreeItem(IntPtr treeHandle, IntPtr itemHandle, IntPtr parentItemHandle, int index);

    // Table control operations

    /// <summary>
    /// Creates a native table control.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="style">Table style (SINGLE, MULTI, CHECK, FULL_SELECTION, HIDE_SELECTION)</param>
    IntPtr CreateTable(IntPtr parentHandle, int style);

    /// <summary>
    /// Sets whether the table header is visible.
    /// </summary>
    void SetTableHeaderVisible(IntPtr handle, bool visible);

    /// <summary>
    /// Sets whether grid lines are visible.
    /// </summary>
    void SetTableLinesVisible(IntPtr handle, bool visible);

    /// <summary>
    /// Sets the table selection.
    /// </summary>
    void SetTableSelection(IntPtr handle, int[] indices);

    /// <summary>
    /// Clears all items from the table.
    /// </summary>
    void ClearTableItems(IntPtr handle);

    /// <summary>
    /// Scrolls to show the specified item.
    /// </summary>
    void ShowTableItem(IntPtr handle, int index);

    // TableColumn operations

    /// <summary>
    /// Creates a native table column.
    /// </summary>
    /// <param name="tableHandle">Parent table handle</param>
    /// <param name="style">Column style (LEFT, RIGHT, CENTER)</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    IntPtr CreateTableColumn(IntPtr tableHandle, int style, int index);

    /// <summary>
    /// Destroys a table column.
    /// </summary>
    void DestroyTableColumn(IntPtr handle);

    /// <summary>
    /// Sets the column header text.
    /// </summary>
    void SetTableColumnText(IntPtr handle, string text);

    /// <summary>
    /// Sets the column width.
    /// </summary>
    void SetTableColumnWidth(IntPtr handle, int width);

    /// <summary>
    /// Sets the column alignment.
    /// </summary>
    void SetTableColumnAlignment(IntPtr handle, int alignment);

    /// <summary>
    /// Sets whether the column is resizable.
    /// </summary>
    void SetTableColumnResizable(IntPtr handle, bool resizable);

    /// <summary>
    /// Sets whether the column is moveable.
    /// </summary>
    void SetTableColumnMoveable(IntPtr handle, bool moveable);

    /// <summary>
    /// Sets the column tooltip text.
    /// </summary>
    void SetTableColumnToolTipText(IntPtr handle, string? toolTipText);

    /// <summary>
    /// Automatically sizes the column to fit content.
    /// </summary>
    /// <returns>The new width</returns>
    int PackTableColumn(IntPtr handle);

    // TableItem operations

    /// <summary>
    /// Creates a native table item.
    /// </summary>
    /// <param name="tableHandle">Parent table handle</param>
    /// <param name="style">Item style</param>
    /// <param name="index">Index at which to insert, or -1 to append</param>
    IntPtr CreateTableItem(IntPtr tableHandle, int style, int index);

    /// <summary>
    /// Destroys a table item.
    /// </summary>
    void DestroyTableItem(IntPtr handle);

    /// <summary>
    /// Sets the text for a specific column.
    /// </summary>
    void SetTableItemText(IntPtr handle, int column, string text);

    /// <summary>
    /// Sets the image for a specific column.
    /// </summary>
    void SetTableItemImage(IntPtr handle, int column, IntPtr imageHandle);

    /// <summary>
    /// Sets the checked state (for CHECK style tables).
    /// </summary>
    void SetTableItemChecked(IntPtr handle, bool isChecked);

    /// <summary>
    /// Sets the background color for an item.
    /// </summary>
    void SetTableItemBackground(IntPtr handle, object? color);

    /// <summary>
    /// Sets the foreground color for an item.
    /// </summary>
    void SetTableItemForeground(IntPtr handle, object? color);

    /// <summary>
    /// Sets the font for an item.
    /// </summary>
    void SetTableItemFont(IntPtr handle, object? font);

    // Dialog operations

    /// <summary>
    /// Shows a message box dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="message">Message text</param>
    /// <param name="title">Dialog title</param>
    /// <param name="style">Style bits (icons and buttons)</param>
    /// <returns>Button clicked (SWT.OK, SWT.CANCEL, SWT.YES, SWT.NO, etc.)</returns>
    int ShowMessageBox(IntPtr parentHandle, string message, string title, int style);

    /// <summary>
    /// Shows a file open/save dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="filterPath">Initial directory</param>
    /// <param name="fileName">Initial file name</param>
    /// <param name="filterNames">Filter names (e.g., "Text Files")</param>
    /// <param name="filterExtensions">Filter extensions (e.g., "*.txt")</param>
    /// <param name="style">Style bits (SWT.OPEN, SWT.SAVE, SWT.MULTI)</param>
    /// <param name="overwrite">Prompt on overwrite for SAVE dialogs</param>
    /// <returns>Dialog result with selected files and filter path</returns>
    FileDialogResult ShowFileDialog(
        IntPtr parentHandle,
        string title,
        string filterPath,
        string fileName,
        string[] filterNames,
        string[] filterExtensions,
        int style,
        bool overwrite);

    /// <summary>
    /// Shows a directory selection dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="message">Message text</param>
    /// <param name="filterPath">Initial directory</param>
    /// <returns>Selected directory path, or null if cancelled</returns>
    string? ShowDirectoryDialog(
        IntPtr parentHandle,
        string title,
        string message,
        string filterPath);

    /// <summary>
    /// Shows a color picker dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="initialColor">Initial color</param>
    /// <param name="customColors">Custom colors palette</param>
    /// <returns>Selected color, or null if cancelled</returns>
    Graphics.RGB? ShowColorDialog(
        IntPtr parentHandle,
        string title,
        Graphics.RGB initialColor,
        Graphics.RGB[]? customColors);

    /// <summary>
    /// Shows a font selection dialog.
    /// </summary>
    /// <param name="parentHandle">Parent window handle</param>
    /// <param name="title">Dialog title</param>
    /// <param name="initialFont">Initial font data</param>
    /// <param name="initialColor">Initial font color</param>
    /// <returns>Dialog result with selected font and color</returns>
    FontDialogResult ShowFontDialog(
        IntPtr parentHandle,
        string title,
        Graphics.FontData? initialFont,
        Graphics.RGB? initialColor);
}

/// <summary>
/// Result structure for file dialog.
/// </summary>
public struct FileDialogResult
{
    /// <summary>
    /// Selected file paths (can be multiple for MULTI style).
    /// </summary>
    public string[]? SelectedFiles { get; set; }

    /// <summary>
    /// Selected filter path (directory).
    /// </summary>
    public string? FilterPath { get; set; }

    /// <summary>
    /// Selected filter index (0-based).
    /// </summary>
    public int FilterIndex { get; set; }
}

/// <summary>
/// Result structure for font dialog.
/// </summary>
public struct FontDialogResult
{
    /// <summary>
    /// Selected font data.
    /// </summary>
    public Graphics.FontData? FontData { get; set; }

    /// <summary>
    /// Selected font color.
    /// </summary>
    public Graphics.RGB? Color { get; set; }
}

public partial interface IPlatform
{
    // ProgressBar control operations

    /// <summary>
    /// Creates a native progress bar control.
    /// Platform: Win32 msctls_progress32, macOS NSProgressIndicator, GTK GtkProgressBar
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Progress bar style (HORIZONTAL, VERTICAL, SMOOTH, INDETERMINATE)</param>
    /// <returns>Handle to the created progress bar</returns>
    IntPtr CreateProgressBar(IntPtr parent, int style);

    /// <summary>
    /// Sets the progress bar's range (minimum and maximum values).
    /// </summary>
    /// <param name="handle">Progress bar handle</param>
    /// <param name="minimum">Minimum value</param>
    /// <param name="maximum">Maximum value</param>
    void SetProgressBarRange(IntPtr handle, int minimum, int maximum);

    /// <summary>
    /// Sets the progress bar's current selection (value).
    /// </summary>
    /// <param name="handle">Progress bar handle</param>
    /// <param name="selection">Current value</param>
    void SetProgressBarSelection(IntPtr handle, int selection);

    /// <summary>
    /// Sets the progress bar's state (NORMAL, ERROR, PAUSED).
    /// </summary>
    /// <param name="handle">Progress bar handle</param>
    /// <param name="state">State value</param>
    void SetProgressBarState(IntPtr handle, int state);

    // Slider control operations

    /// <summary>
    /// Creates a native slider control.
    /// Platform: Win32 SCROLLBAR class, macOS NSSlider, GTK GtkScale
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Slider style (HORIZONTAL, VERTICAL)</param>
    /// <returns>Handle to the created slider</returns>
    IntPtr CreateSlider(IntPtr parent, int style);

    /// <summary>
    /// Sets all slider values at once.
    /// </summary>
    /// <param name="handle">Slider handle</param>
    /// <param name="selection">Current value</param>
    /// <param name="minimum">Minimum value</param>
    /// <param name="maximum">Maximum value</param>
    /// <param name="thumb">Thumb (handle) size</param>
    /// <param name="increment">Increment for arrow buttons</param>
    /// <param name="pageIncrement">Increment for clicking in trough</param>
    void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement);

    /// <summary>
    /// Connects a slider value changed event handler.
    /// </summary>
    /// <param name="handle">Slider handle</param>
    /// <param name="callback">Callback receiving new value</param>
    void ConnectSliderChanged(IntPtr handle, Action<int> callback);

    // Scale control operations

    /// <summary>
    /// Creates a native scale control.
    /// Platform: Win32 msctls_trackbar32, macOS NSSlider, GTK GtkScale
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Scale style (HORIZONTAL, VERTICAL)</param>
    /// <returns>Handle to the created scale</returns>
    IntPtr CreateScale(IntPtr parent, int style);

    /// <summary>
    /// Sets all scale values at once.
    /// </summary>
    /// <param name="handle">Scale handle</param>
    /// <param name="selection">Current value</param>
    /// <param name="minimum">Minimum value</param>
    /// <param name="maximum">Maximum value</param>
    /// <param name="increment">Increment for arrow buttons</param>
    /// <param name="pageIncrement">Increment for clicking in trough</param>
    void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement);

    /// <summary>
    /// Connects a scale value changed event handler.
    /// </summary>
    /// <param name="handle">Scale handle</param>
    /// <param name="callback">Callback receiving new value</param>
    void ConnectScaleChanged(IntPtr handle, Action<int> callback);

    // Spinner control operations

    /// <summary>
    /// Creates a native spinner control.
    /// Platform: Win32 UPDOWN_CLASS, macOS NSStepper, GTK GtkSpinButton
    /// </summary>
    /// <param name="parent">Parent window handle</param>
    /// <param name="style">Spinner style (READ_ONLY, WRAP)</param>
    /// <returns>Handle to the created spinner</returns>
    IntPtr CreateSpinner(IntPtr parent, int style);

    /// <summary>
    /// Sets all spinner values at once.
    /// </summary>
    /// <param name="handle">Spinner handle</param>
    /// <param name="selection">Current value</param>
    /// <param name="minimum">Minimum value</param>
    /// <param name="maximum">Maximum value</param>
    /// <param name="digits">Number of decimal places</param>
    /// <param name="increment">Increment for up/down buttons</param>
    /// <param name="pageIncrement">Page increment</param>
    void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement);

    /// <summary>
    /// Sets the maximum number of characters for the spinner's text field.
    /// </summary>
    /// <param name="handle">Spinner handle</param>
    /// <param name="limit">Maximum number of characters</param>
    void SetSpinnerTextLimit(IntPtr handle, int limit);

    /// <summary>
    /// Connects a spinner value changed event handler.
    /// </summary>
    /// <param name="handle">Spinner handle</param>
    /// <param name="callback">Callback receiving new value</param>
    void ConnectSpinnerChanged(IntPtr handle, Action<int> callback);

    /// <summary>
    /// Connects a spinner text modified event handler.
    /// </summary>
    /// <param name="handle">Spinner handle</param>
    /// <param name="callback">Callback invoked when text is modified</param>
    void ConnectSpinnerModified(IntPtr handle, Action callback);
}
