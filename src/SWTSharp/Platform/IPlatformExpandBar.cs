namespace SWTSharp.Platform;

/// <summary>
/// Platform expand bar (accordion container widget).
/// </summary>
public interface IPlatformExpandBar : IPlatformComposite, IPlatformExpandEvents
{
    /// <summary>
    /// Gets the number of expand items in the bar.
    /// </summary>
    int GetItemCount();

    /// <summary>
    /// Gets the expand item at the specified index.
    /// </summary>
    IPlatformExpandItem GetItem(int index);

    /// <summary>
    /// Sets the spacing between expand items.
    /// </summary>
    void SetSpacing(int spacing);

    /// <summary>
    /// Gets the spacing between expand items.
    /// </summary>
    int GetSpacing();

    /// <summary>
    /// Creates a new expand item within this bar.
    /// </summary>
    IPlatformExpandItem CreateExpandItem(int style, int index);
}

/// <summary>
/// Platform expand item (expandable section, not a widget).
/// </summary>
public interface IPlatformExpandItem : IDisposable, IPlatformEventHandling
{
    /// <summary>
    /// Sets the text displayed on the item header.
    /// </summary>
    void SetText(string text);

    /// <summary>
    /// Gets the text displayed on the item header.
    /// </summary>
    string GetText();

    /// <summary>
    /// Sets whether the item is expanded.
    /// </summary>
    void SetExpanded(bool expanded);

    /// <summary>
    /// Gets whether the item is expanded.
    /// </summary>
    bool GetExpanded();

    /// <summary>
    /// Sets the height of the content area when expanded.
    /// </summary>
    void SetHeight(int height);

    /// <summary>
    /// Gets the height of the content area when expanded.
    /// </summary>
    int GetHeight();

    /// <summary>
    /// Sets the control displayed when the item is expanded.
    /// </summary>
    void SetControl(IPlatformWidget? control);
}

/// <summary>
/// Event handling interface for expand bar widgets.
/// </summary>
public interface IPlatformExpandEvents
{
    /// <summary>
    /// Occurs when an item is expanded.
    /// </summary>
    event EventHandler<int>? ItemExpanded;

    /// <summary>
    /// Occurs when an item is collapsed.
    /// </summary>
    event EventHandler<int>? ItemCollapsed;
}
