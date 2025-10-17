using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a rearrangeable toolbar container widget (CoolBar).
/// A CoolBar is a composite widget that allows the user to lock, unlock,
/// and rearrange bands (CoolItems) containing other widgets.
/// </summary>
public class CoolBar : Composite
{
    private readonly List<CoolItem> _items = new();
#pragma warning disable CS0649 // Field is never assigned to - will be assigned when platform widget interface is implemented
    private IPlatformCoolBar? _platformCoolBar;
#pragma warning restore CS0649

    /// <summary>
    /// Gets an array of all items in the coolbar.
    /// </summary>
    public CoolItem[] Items
    {
        get
        {
            CheckWidget();
            lock (_items)
            {
                return _items.ToArray();
            }
        }
    }

    /// <summary>
    /// Gets the number of items in the coolbar.
    /// </summary>
    public int ItemCount
    {
        get
        {
            CheckWidget();
            lock (_items)
            {
                return _items.Count;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the coolbar is locked (items cannot be moved).
    /// </summary>
    public bool Locked
    {
        get
        {
            CheckWidget();
            return _platformCoolBar?.GetLocked() ?? false;
        }
        set
        {
            CheckWidget();
            _platformCoolBar?.SetLocked(value);
        }
    }

    /// <summary>
    /// Creates a new coolbar with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite (cannot be null).</param>
    /// <param name="style">The coolbar style bits (FLAT, HORIZONTAL, VERTICAL).</param>
    public CoolBar(Composite parent, int style) : base(parent, CheckStyle(style))
    {
        // CreateWidget is already called by base Composite constructor
    }

    private static int CheckStyle(int style)
    {
        // Ensure default orientation is HORIZONTAL if not specified
        if ((style & (SWT.HORIZONTAL | SWT.VERTICAL)) == 0)
        {
            style |= SWT.HORIZONTAL;
        }

        // FLAT is the default if no border style is specified
        if ((style & SWT.FLAT) == 0)
        {
            style |= SWT.FLAT;
        }

        return style;
    }

    /// <summary>
    /// Gets the platform coolbar for use by CoolItem.
    /// </summary>
    internal IPlatformCoolBar? PlatformCoolBar => _platformCoolBar;

    /// <summary>
    /// Creates the platform-specific coolbar widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // Create real coolbar through factory
        _platformCoolBar = Platform.PlatformFactory.Instance.CreateCoolBarWidget(
            Parent?.PlatformWidget,
            Style
        );

        if (_platformCoolBar == null)
        {
            throw new InvalidOperationException(
                "Platform failed to create IPlatformCoolBar implementation"
            );
        }

        // Set PlatformWidget to the coolbar if it also implements IPlatformWidget
        PlatformWidget = _platformCoolBar as IPlatformWidget;
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <returns>The item at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When index is out of range.</exception>
    public CoolItem GetItem(int index)
    {
        CheckWidget();
        lock (_items)
        {
            if (index < 0 || index >= _items.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    $"Index must be between 0 and {_items.Count - 1}, but was {index}"
                );
            }
            return _items[index];
        }
    }

    /// <summary>
    /// Gets the number of items in the coolbar.
    /// </summary>
    /// <returns>The number of items.</returns>
    public int GetItemCount()
    {
        return ItemCount;
    }

    /// <summary>
    /// Returns the zero-based index of the item in the coolbar.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(CoolItem item)
    {
        CheckWidget();
        if (item == null)
        {
            return -1;
        }

        lock (_items)
        {
            return _items.IndexOf(item);
        }
    }

    /// <summary>
    /// Adds a cool item to the coolbar.
    /// This method is called internally when a CoolItem is created with this coolbar as parent.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="index">The index at which to insert the item, or -1 to append.</param>
    internal void AddItem(CoolItem item, int index)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            if (index < 0 || index > _items.Count)
            {
                _items.Add(item);
            }
            else
            {
                _items.Insert(index, item);
            }
        }
    }

    /// <summary>
    /// Removes a cool item from the coolbar.
    /// This method is called internally when a CoolItem is disposed.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    internal void RemoveItem(CoolItem item)
    {
        if (item == null)
        {
            return;
        }

        lock (_items)
        {
            _items.Remove(item);
        }
    }

    /// <summary>
    /// Returns whether this is a FLAT coolbar.
    /// </summary>
    public bool IsFlat => (Style & SWT.FLAT) != 0;

    /// <summary>
    /// Returns whether this is a HORIZONTAL coolbar.
    /// </summary>
    public bool IsHorizontal => (Style & SWT.HORIZONTAL) != 0;

    /// <summary>
    /// Returns whether this is a VERTICAL coolbar.
    /// </summary>
    public bool IsVertical => (Style & SWT.VERTICAL) != 0;

    /// <summary>
    /// Sets whether the coolbar is locked.
    /// </summary>
    /// <param name="locked">true to lock, false to unlock.</param>
    public void SetLocked(bool locked)
    {
        Locked = locked;
    }

    protected override void UpdateVisible()
    {
        if (PlatformWidget == null)
            return;

        base.UpdateVisible();
    }

    protected override void UpdateEnabled()
    {
        if (PlatformWidget == null)
            return;

        base.UpdateEnabled();
    }

    protected override void ReleaseWidget()
    {
        try
        {
            // Dispose all items first, before platform cleanup
            lock (_items)
            {
                foreach (var item in _items.ToArray())
                {
                    if (item is IDisposable disposable)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error disposing CoolItem: {ex.Message}");
                        }
                    }
                }
                _items.Clear();
            }

            // Dispose platform coolbar if it exists
            if (_platformCoolBar != null)
            {
                try
                {
                    _platformCoolBar.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing platform coolbar: {ex.Message}");
                }
                _platformCoolBar = null;
            }
        }
        finally
        {
            base.ReleaseWidget();
        }
    }
}
