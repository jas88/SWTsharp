using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents a toolbar container widget.
/// A toolbar is a selectable user interface component that displays a row or column of items,
/// where each item is either a button (with text and/or an image) or a separator.
/// </summary>
public class ToolBar : Composite
{
    private readonly List<ToolItem> _items = new();
#pragma warning disable CS0649 // Field is never assigned to - will be assigned when platform widget interface is implemented
    private IPlatformToolBar? _platformToolBar;
#pragma warning restore CS0649

    /// <summary>
    /// Gets an array of all items in the toolbar.
    /// </summary>
    public ToolItem[] Items
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
    /// Gets the number of items in the toolbar.
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
    /// Creates a new toolbar with the specified parent and style.
    /// </summary>
    /// <param name="parent">The parent composite (cannot be null).</param>
    /// <param name="style">The toolbar style bits (FLAT, WRAP, RIGHT, HORIZONTAL, VERTICAL, SHADOW_OUT).</param>
    public ToolBar(Composite parent, int style) : base(parent, CheckStyle(style))
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
        if ((style & (SWT.FLAT | SWT.SHADOW_OUT)) == 0)
        {
            style |= SWT.FLAT;
        }

        return style;
    }

    /// <summary>
    /// Gets the actual platform-specific toolbar handle for use by ToolItem.
    /// TODO: Remove this property when platform widget implementation is available
    /// TODO: Replace with IPlatformToolBar interface access
    /// </summary>
    internal IntPtr ToolBarHandle => IntPtr.Zero; // Return placeholder handle for now

    /// <summary>
    /// Gets the platform toolbar for use by ToolItem.
    /// TODO: Initialize this property when IPlatformToolBar implementation is available
    /// TODO: Create toolbar widget in CreateWidget method using CreateToolBarWidget
    /// </summary>
    internal IPlatformToolBar? PlatformToolBar => _platformToolBar;

    /// <summary>
    /// Creates the platform-specific toolbar widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // TEMPORARY FIX: Until IPlatformToolBar is fully implemented, create as basic Composite
        // This prevents NSWindow state inconsistency and crashes during disposal
        // The ToolBar will function as a container but without native toolbar features
        base.CreateWidget();

        // TODO: Replace base.CreateWidget() with proper toolbar creation:
        // PlatformWidget = Platform.PlatformFactory.Instance.CreateToolBarWidget(
        //     Parent?.PlatformWidget,
        //     Style
        // );
        // TODO: Handle toolbar style bits (FLAT, WRAP, RIGHT, HORIZONTAL, VERTICAL, SHADOW_OUT)
        // TODO: Initialize _platformToolBar from PlatformWidget cast to IPlatformToolBar
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <returns>The item at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When index is out of range.</exception>
    public ToolItem GetItem(int index)
    {
        CheckWidget();
        lock (_items)
        {
            // ROBUST: Validate bounds before array access to prevent NSInternalInconsistencyException
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
    /// Gets the number of items in the toolbar.
    /// </summary>
    /// <returns>The number of items.</returns>
    public int GetItemCount()
    {
        return ItemCount;
    }

    /// <summary>
    /// Returns the zero-based index of the item in the toolbar.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>The index of the item, or -1 if not found.</returns>
    public int IndexOf(ToolItem item)
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
    /// Adds a tool item to the toolbar.
    /// This method is called internally when a ToolItem is created with this toolbar as parent.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="index">The index at which to insert the item, or -1 to append.</param>
    internal void AddItem(ToolItem item, int index)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        lock (_items)
        {
            // ROBUST: Validate insertion index before modifying collection
            if (index < 0 || index > _items.Count)  // Note: > not >= for insertion
            {
                // Invalid index - append to end
                _items.Add(item);
            }
            else
            {
                _items.Insert(index, item);
            }
        }
    }

    /// <summary>
    /// Removes a tool item from the toolbar.
    /// This method is called internally when a ToolItem is disposed.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    internal void RemoveItem(ToolItem item)
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
    /// Returns whether this is a FLAT toolbar.
    /// </summary>
    public bool IsFlat => (Style & SWT.FLAT) != 0;

    /// <summary>
    /// Returns whether this is a WRAP toolbar (items wrap to next line when needed).
    /// </summary>
    public bool IsWrap => (Style & SWT.WRAP) != 0;

    /// <summary>
    /// Returns whether text appears to the RIGHT of images.
    /// </summary>
    public bool IsRight => (Style & SWT.RIGHT) != 0;

    /// <summary>
    /// Returns whether this is a HORIZONTAL toolbar.
    /// </summary>
    public bool IsHorizontal => (Style & SWT.HORIZONTAL) != 0;

    /// <summary>
    /// Returns whether this is a VERTICAL toolbar.
    /// </summary>
    public bool IsVertical => (Style & SWT.VERTICAL) != 0;

    /// <summary>
    /// Returns whether this toolbar has SHADOW_OUT style.
    /// </summary>
    public bool IsShadowOut => (Style & SWT.SHADOW_OUT) != 0;

    protected override void UpdateVisible()
    {
        // Guard against null platform widget during initialization or disposal
        if (PlatformWidget == null)
            return;

        // Use base Composite visibility control until IPlatformToolBar is implemented
        base.UpdateVisible();

        // TODO: Replace with IPlatformToolBar-specific visibility control
        // TODO: _platformToolBar?.SetVisible(Visible);
    }

    protected override void UpdateEnabled()
    {
        // Guard against null platform widget during initialization or disposal
        if (PlatformWidget == null)
            return;

        // Use base Composite enabled control until IPlatformToolBar is implemented
        base.UpdateEnabled();

        // TODO: Replace with IPlatformToolBar-specific enabled control
        // TODO: _platformToolBar?.SetEnabled(Enabled);
        // TODO: May need to propagate to all items
    }

    protected override void ReleaseWidget()
    {
        try
        {
            // ROBUST: Dispose all items first, before platform cleanup
            // This ensures proper cleanup order and prevents access to disposed items
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
                            // Log but don't let one item failure stop others
                            System.Diagnostics.Debug.WriteLine($"Error disposing ToolItem: {ex.Message}");
                        }
                    }
                }
                _items.Clear();
            }

            // Dispose platform toolbar if it exists
            if (_platformToolBar != null)
            {
                try
                {
                    _platformToolBar.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing platform toolbar: {ex.Message}");
                }
                _platformToolBar = null;
            }
        }
        finally
        {
            // ROBUST: Always call base cleanup, even if item disposal fails
            base.ReleaseWidget();
        }
    }
}
