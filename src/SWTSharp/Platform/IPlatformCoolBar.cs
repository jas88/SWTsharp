using SWTSharp.Graphics;

namespace SWTSharp.Platform;

/// <summary>
/// Platform-specific interface for CoolBar widget (rearrangeable toolbar container).
/// CoolBars contain CoolItems (bands) that can be resized and rearranged.
/// </summary>
public interface IPlatformCoolBar : IDisposable
{
    /// <summary>
    /// Creates a new cool item at the specified index.
    /// </summary>
    /// <param name="index">The index at which to insert the item, or -1 to append.</param>
    /// <param name="style">The cool item style (DROP_DOWN is supported).</param>
    /// <returns>A platform-specific cool item implementation.</returns>
    IPlatformCoolItem CreateItem(int index, int style);

    /// <summary>
    /// Removes the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    void RemoveItem(int index);

    /// <summary>
    /// Gets the number of items in the coolbar.
    /// </summary>
    /// <returns>The item count.</returns>
    int GetItemCount();

    /// <summary>
    /// Gets whether the coolbar is locked (items cannot be moved).
    /// </summary>
    /// <returns>True if locked, false otherwise.</returns>
    bool GetLocked();

    /// <summary>
    /// Sets whether the coolbar is locked (items cannot be moved).
    /// </summary>
    /// <param name="locked">True to lock, false to unlock.</param>
    void SetLocked(bool locked);
}

/// <summary>
/// Platform-specific interface for CoolItem (individual band in a CoolBar).
/// </summary>
public interface IPlatformCoolItem : IDisposable
{
    /// <summary>
    /// Sets the control contained in this cool item.
    /// </summary>
    /// <param name="control">The platform widget to display in this item.</param>
    void SetControl(IPlatformWidget? control);

    /// <summary>
    /// Gets the bounds of this cool item.
    /// </summary>
    /// <returns>The item bounds (x, y, width, height).</returns>
    Rectangle GetBounds();

    /// <summary>
    /// Sets the preferred size of this cool item.
    /// </summary>
    /// <param name="width">The preferred width.</param>
    /// <param name="height">The preferred height.</param>
    void SetPreferredSize(int width, int height);

    /// <summary>
    /// Sets the minimum size of this cool item.
    /// </summary>
    /// <param name="width">The minimum width.</param>
    /// <param name="height">The minimum height.</param>
    void SetMinimumSize(int width, int height);
}
