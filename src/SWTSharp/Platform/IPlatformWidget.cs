using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Base interface for all platform-specific widget implementations.
/// Platform widgets encapsulate native handles and provide widget operations.
/// Widget layer never sees IntPtr handles - only platform implementations do.
/// </summary>
public interface IPlatformWidget : IDisposable
{
    /// <summary>
    /// Sets the bounds (position and size) of the widget.
    /// </summary>
    void SetBounds(int x, int y, int width, int height);

    /// <summary>
    /// Gets the bounds of the widget.
    /// </summary>
    Rectangle GetBounds();

    /// <summary>
    /// Sets whether the widget is visible.
    /// </summary>
    void SetVisible(bool visible);

    /// <summary>
    /// Gets whether the widget is visible.
    /// </summary>
    bool GetVisible();

    /// <summary>
    /// Sets whether the widget is enabled.
    /// </summary>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Gets whether the widget is enabled.
    /// </summary>
    bool GetEnabled();

    /// <summary>
    /// Sets the background color of the widget.
    /// </summary>
    void SetBackground(RGB color);

    /// <summary>
    /// Gets the background color of the widget.
    /// </summary>
    RGB GetBackground();

    /// <summary>
    /// Sets the foreground color of the widget.
    /// </summary>
    void SetForeground(RGB color);

    /// <summary>
    /// Gets the foreground color of the widget.
    /// </summary>
    RGB GetForeground();
}

/// <summary>
/// Platform widget that can display text.
/// </summary>
public interface IPlatformTextWidget : IPlatformWidget, IPlatformTextEvents, IPlatformEventHandling
{
    /// <summary>
    /// Sets the text content of the widget.
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// Gets the text content of the widget.
    /// </summary>
    string GetText();
}

/// <summary>
/// Platform widget for editable text input (extends IPlatformTextWidget with editing features).
/// </summary>
public interface IPlatformTextInput : IPlatformTextWidget
{
    /// <summary>
    /// Sets the maximum number of characters that can be entered.
    /// </summary>
    void SetTextLimit(int limit);

    /// <summary>
    /// Sets whether the text is read-only.
    /// </summary>
    void SetReadOnly(bool readOnly);

    /// <summary>
    /// Gets whether the text is read-only.
    /// </summary>
    bool GetReadOnly();

    /// <summary>
    /// Sets the text selection range.
    /// </summary>
    void SetSelection(int start, int end);

    /// <summary>
    /// Gets the current text selection range.
    /// </summary>
    (int Start, int End) GetSelection();

    /// <summary>
    /// Inserts text at the current cursor position.
    /// </summary>
    void Insert(string text);
}

/// <summary>
/// Platform widget that can contain other widgets.
/// </summary>
public interface IPlatformComposite : IPlatformWidget, IPlatformContainerEvents
{
    /// <summary>
    /// Adds a child widget to this composite.
    /// </summary>
    void AddChild(IPlatformWidget child);

    /// <summary>
    /// Removes a child widget from this composite.
    /// </summary>
    void RemoveChild(IPlatformWidget child);

    /// <summary>
    /// Gets all child widgets.
    /// </summary>
    IReadOnlyList<IPlatformWidget> GetChildren();
}

/// <summary>
/// Platform window (top-level shell).
/// </summary>
public interface IPlatformWindow : IPlatformComposite, IPlatformEventHandling
{
    /// <summary>
    /// Sets the window title.
    /// </summary>
    void SetTitle(string title);

    /// <summary>
    /// Gets the window title.
    /// </summary>
    string GetTitle();

    /// <summary>
    /// Opens (shows) the window.
    /// </summary>
    void Open();

    /// <summary>
    /// Closes the window.
    /// </summary>
    void Close();

    /// <summary>
    /// Gets whether the window is disposed.
    /// </summary>
    bool IsDisposed { get; }
}

/// <summary>
/// Platform toolbar (special window decoration, not a standard widget).
/// </summary>
public interface IPlatformToolBar : IDisposable
{
    /// <summary>
    /// Adds an item to the toolbar.
    /// </summary>
    void AddItem(string text, IPlatformImage? image);

    /// <summary>
    /// Removes an item from the toolbar at the specified index.
    /// </summary>
    void RemoveItem(int index);

    /// <summary>
    /// Attaches this toolbar to a window.
    /// </summary>
    void AttachToWindow(IPlatformWindow window);

    /// <summary>
    /// Gets the number of items in the toolbar.
    /// </summary>
    int GetItemCount();
}

/// <summary>
/// Platform table item (data row, not a widget).
/// </summary>
public interface IPlatformTableItem : IDisposable, IPlatformSelectionEvents
{
    /// <summary>
    /// Sets the text for a specific column.
    /// </summary>
    void SetText(int column, string text);

    /// <summary>
    /// Gets the text for a specific column.
    /// </summary>
    string GetText(int column);

    /// <summary>
    /// Sets the image for a specific column.
    /// </summary>
    void SetImage(int column, IPlatformImage? image);

    /// <summary>
    /// Sets the background color for the item.
    /// </summary>
    void SetBackground(RGB color);

    /// <summary>
    /// Gets the background color for the item.
    /// </summary>
    RGB GetBackground();
}

/// <summary>
/// Platform tree item (data node, not a widget).
/// </summary>
public interface IPlatformTreeItem : IDisposable, IPlatformSelectionEvents
{
    /// <summary>
    /// Sets the text of the tree item.
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// Gets the text of the tree item.
    /// </summary>
    string GetText();

    /// <summary>
    /// Sets the image of the tree item.
    /// </summary>
    void SetImage(IPlatformImage? image);

    /// <summary>
    /// Expands or collapses the tree item.
    /// </summary>
    void SetExpanded(bool expanded);

    /// <summary>
    /// Gets whether the tree item is expanded.
    /// </summary>
    bool GetExpanded();

    /// <summary>
    /// Sets whether the tree item is checked (for checkbox trees).
    /// </summary>
    void SetChecked(bool @checked);

    /// <summary>
    /// Gets whether the tree item is checked.
    /// </summary>
    bool GetChecked();
}

/// <summary>
/// Platform image resource.
/// </summary>
public interface IPlatformImage : IDisposable
{
    /// <summary>
    /// Gets the width of the image.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the image.
    /// </summary>
    int Height { get; }
}


/// <summary>
/// Platform tab item (tab page, not a widget).
/// </summary>
public interface IPlatformTabItem : IDisposable, IPlatformEventHandling
{
    /// <summary>
    /// Sets the text displayed on the tab.
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// Gets the text displayed on the tab.
    /// </summary>
    string GetText();

    /// <summary>
    /// Sets the control displayed when the tab is selected.
    /// </summary>
    void SetControl(IPlatformWidget? control);
}

/// <summary>
/// Platform toolbar item (button in toolbar, not a standard widget).
/// </summary>
public interface IPlatformToolItem : IDisposable, IPlatformEventHandling
{
    /// <summary>
    /// Sets the text of the toolbar item.
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// Gets the text of the toolbar item.
    /// </summary>
    string GetText();

    /// <summary>
    /// Sets the image of the toolbar item.
    /// </summary>
    void SetImage(IPlatformImage? image);

    /// <summary>
    /// Sets whether the toolbar item is enabled.
    /// </summary>
    void SetEnabled(bool enabled);

    /// <summary>
    /// Gets whether the toolbar item is enabled.
    /// </summary>
    bool GetEnabled();
}

// Event Handling Interfaces for Phase 5.8+

/// <summary>
/// Basic event handling interface for platform widgets.
/// </summary>
public interface IPlatformEventHandling
{
    /// <summary>
    /// Occurs when the widget is clicked.
    /// </summary>
    event EventHandler<int>? Click;

    /// <summary>
    /// Occurs when the widget gets focus.
    /// </summary>
    event EventHandler<int>? FocusGained;

    /// <summary>
    /// Occurs when the widget loses focus.
    /// </summary>
    event EventHandler<int>? FocusLost;

    /// <summary>
    /// Occurs when a key is pressed.
    /// </summary>
    event EventHandler<PlatformKeyEventArgs>? KeyDown;

    /// <summary>
    /// Occurs when a key is released.
    /// </summary>
    event EventHandler<PlatformKeyEventArgs>? KeyUp;
}

/// <summary>
/// Event arguments for keyboard events.
/// </summary>
public class PlatformKeyEventArgs : EventArgs
{
    /// <summary>
    /// The key code.
    /// </summary>
    public int KeyCode { get; set; }

    /// <summary>
    /// The character code.
    /// </summary>
    public char Character { get; set; }

    /// <summary>
    /// Whether shift key is pressed.
    /// </summary>
    public bool Shift { get; set; }

    /// <summary>
    /// Whether control key is pressed.
    /// </summary>
    public bool Control { get; set; }

    /// <summary>
    /// Whether alt key is pressed.
    /// </summary>
    public bool Alt { get; set; }
}

/// <summary>
/// Event handling interface for value-based widgets (Slider, Scale, Spinner, ProgressBar).
/// </summary>
public interface IPlatformValueEvents
{
    /// <summary>
    /// Occurs when the value changes.
    /// </summary>
    event EventHandler<int>? ValueChanged;
}

/// <summary>
/// Event handling interface for selection-based widgets (Combo, List, Table, Tree).
/// </summary>
public interface IPlatformSelectionEvents
{
    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    event EventHandler<int>? SelectionChanged;

    /// <summary>
    /// Occurs when an item is double-clicked.
    /// </summary>
    event EventHandler<int>? ItemDoubleClick;
}

/// <summary>
/// Event handling interface for text-based widgets (Text, Label).
/// </summary>
public interface IPlatformTextEvents
{
    /// <summary>
    /// Occurs when the text is modified.
    /// </summary>
    event EventHandler<string>? TextChanged;

    /// <summary>
    /// Occurs when the text is committed (Enter key or focus loss).
    /// </summary>
    event EventHandler<string>? TextCommitted;
}

/// <summary>
/// Event handling interface for container widgets (Composite, TabFolder).
/// </summary>
public interface IPlatformContainerEvents
{
    /// <summary>
    /// Occurs when a child widget is added.
    /// </summary>
    event EventHandler<IPlatformWidget>? ChildAdded;

    /// <summary>
    /// Occurs when a child widget is removed.
    /// </summary>
    event EventHandler<IPlatformWidget>? ChildRemoved;

    /// <summary>
    /// Occurs when the layout needs to be updated.
    /// </summary>
    event EventHandler? LayoutRequested;
}

// Advanced Widget Interfaces for Phase 5.5+

/// <summary>
/// Platform combo box (dropdown) widget.
/// </summary>
public interface IPlatformCombo : IPlatformWidget, IPlatformSelectionEvents, IPlatformEventHandling
{
    /// <summary>
    /// Adds an item to the combo box.
    /// </summary>
    void AddItem(string item);

    /// <summary>
    /// Removes all items from the combo box.
    /// </summary>
    void ClearItems();

    /// <summary>
    /// Gets the number of items in the combo box.
    /// </summary>
    int GetItemCount();

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    string GetItemAt(int index);

    /// <summary>
    /// Gets or sets the selected index.
    /// </summary>
    int SelectionIndex { get; set; }

    /// <summary>
    /// Gets or sets the selected text.
    /// </summary>
    string Text { get; set; }
}

/// <summary>
/// Platform list widget.
/// </summary>
public interface IPlatformList : IPlatformWidget, IPlatformSelectionEvents, IPlatformEventHandling
{
    /// <summary>
    /// Adds an item to the list.
    /// </summary>
    void AddItem(string item);

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    void ClearItems();

    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    int GetItemCount();

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    string GetItemAt(int index);

    /// <summary>
    /// Gets or sets the selected indices.
    /// </summary>
    int[] SelectionIndices { get; set; }

    /// <summary>
    /// Gets or sets the selected index (single selection mode).
    /// </summary>
    int SelectionIndex { get; set; }
}

/// <summary>
/// Platform progress bar widget.
/// </summary>
public interface IPlatformProgressBar : IPlatformWidget, IPlatformValueEvents
{
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    int Value { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    int Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    int Maximum { get; set; }

    /// <summary>
    /// Gets or sets the state (normal, error, paused).
    /// </summary>
    int State { get; set; }
}

/// <summary>
/// Platform slider widget.
/// </summary>
public interface IPlatformSlider : IPlatformWidget, IPlatformValueEvents, IPlatformEventHandling
{
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    int Value { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    int Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    int Maximum { get; set; }

    /// <summary>
    /// Gets or sets the increment value.
    /// </summary>
    int Increment { get; set; }

    /// <summary>
    /// Gets or sets the page increment value.
    /// </summary>
    int PageIncrement { get; set; }
}

/// <summary>
/// Platform scale widget.
/// </summary>
public interface IPlatformScale : IPlatformWidget, IPlatformValueEvents, IPlatformEventHandling
{
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    int Value { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    int Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    int Maximum { get; set; }

    /// <summary>
    /// Gets or sets the increment value.
    /// </summary>
    int Increment { get; set; }

    /// <summary>
    /// Gets or sets whether to show tick marks.
    /// </summary>
    bool ShowTicks { get; set; }
}

/// <summary>
/// Platform spinner widget.
/// </summary>
public interface IPlatformSpinner : IPlatformWidget, IPlatformValueEvents, IPlatformEventHandling
{
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    int Value { get; set; }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    int Minimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    int Maximum { get; set; }

    /// <summary>
    /// Gets or sets the increment value.
    /// </summary>
    int Increment { get; set; }

    /// <summary>
    /// Gets or sets the number of digits to display.
    /// </summary>
    int Digits { get; set; }
}

