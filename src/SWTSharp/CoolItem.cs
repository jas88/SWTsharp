using SWTSharp.Graphics;
using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// Represents an individual band (item) in a CoolBar.
/// Each CoolItem can contain a control and has a size, preferred size, and minimum size.
/// </summary>
public class CoolItem : Widget
{
    private CoolBar? _parent;
    private Control? _control;
    private int _preferredWidth = 32;
    private int _preferredHeight = 32;
    private int _minimumWidth = 0;
    private int _minimumHeight = 0;
    private IPlatformCoolItem? _platformCoolItem;

    /// <summary>
    /// Gets the parent coolbar of this cool item.
    /// </summary>
    public CoolBar? Parent
    {
        get
        {
            CheckWidget();
            return _parent;
        }
    }

    /// <summary>
    /// Gets or sets the control for this cool item.
    /// </summary>
    public Control? Control
    {
        get
        {
            CheckWidget();
            return _control;
        }
        set
        {
            CheckWidget();
            if (_control != value)
            {
                _control = value;
                UpdateControl();
            }
        }
    }

    /// <summary>
    /// Gets the bounds of this cool item.
    /// </summary>
    public Rectangle Bounds
    {
        get
        {
            CheckWidget();
            if (_platformCoolItem != null)
            {
                return _platformCoolItem.GetBounds();
            }
            return new Rectangle(0, 0, 0, 0);
        }
    }

    /// <summary>
    /// Gets the preferred width of this cool item.
    /// </summary>
    public int PreferredWidth
    {
        get
        {
            CheckWidget();
            return _preferredWidth;
        }
    }

    /// <summary>
    /// Gets the preferred height of this cool item.
    /// </summary>
    public int PreferredHeight
    {
        get
        {
            CheckWidget();
            return _preferredHeight;
        }
    }

    /// <summary>
    /// Gets the minimum width of this cool item.
    /// </summary>
    public int MinimumWidth
    {
        get
        {
            CheckWidget();
            return _minimumWidth;
        }
    }

    /// <summary>
    /// Gets the minimum height of this cool item.
    /// </summary>
    public int MinimumHeight
    {
        get
        {
            CheckWidget();
            return _minimumHeight;
        }
    }

    /// <summary>
    /// Creates a cool item, appending it to the coolbar.
    /// </summary>
    /// <param name="parent">The parent coolbar</param>
    /// <param name="style">The cool item style (DROP_DOWN is supported)</param>
    public CoolItem(CoolBar parent, int style) : this(parent, style, -1)
    {
    }

    /// <summary>
    /// Creates a cool item at the specified index.
    /// </summary>
    /// <param name="parent">The parent coolbar</param>
    /// <param name="style">The cool item style (DROP_DOWN is supported)</param>
    /// <param name="index">The index at which to insert the item, or -1 to append</param>
    public CoolItem(CoolBar parent, int style, int index) : base(parent, style)
    {
        if (parent == null)
        {
            throw new ArgumentNullException(nameof(parent));
        }

        _parent = parent;
        CreateWidget(index);
        _parent.AddItem(this, index);
    }

    private void CreateWidget(int index)
    {
        if (_parent == null || _parent.PlatformCoolBar == null)
        {
            return;
        }

        // Create platform cool item through the coolbar
        _platformCoolItem = _parent.PlatformCoolBar.CreateItem(index, Style);

        if (_platformCoolItem != null)
        {
            // Set initial sizes
            _platformCoolItem.SetPreferredSize(_preferredWidth, _preferredHeight);
            _platformCoolItem.SetMinimumSize(_minimumWidth, _minimumHeight);
        }
    }

    /// <summary>
    /// Returns whether this cool item has DROP_DOWN style.
    /// </summary>
    public bool IsDropDown => (Style & SWT.DROP_DOWN) != 0;

    /// <summary>
    /// Sets the control for this cool item.
    /// </summary>
    public void SetControl(Control? control)
    {
        Control = control;
    }

    /// <summary>
    /// Sets the preferred size for this cool item.
    /// </summary>
    public void SetPreferredSize(int width, int height)
    {
        CheckWidget();
        _preferredWidth = width;
        _preferredHeight = height;
        _platformCoolItem?.SetPreferredSize(_preferredWidth, _preferredHeight);
    }

    /// <summary>
    /// Sets the minimum size for this cool item.
    /// </summary>
    public void SetMinimumSize(int width, int height)
    {
        CheckWidget();
        _minimumWidth = width;
        _minimumHeight = height;
        _platformCoolItem?.SetMinimumSize(_minimumWidth, _minimumHeight);
    }

    /// <summary>
    /// Computes the preferred size of this cool item.
    /// </summary>
    /// <param name="wHint">Width hint or SWT.DEFAULT</param>
    /// <param name="hHint">Height hint or SWT.DEFAULT</param>
    public void ComputeSize(int wHint, int hHint, out int width, out int height)
    {
        CheckWidget();

        if (_control != null)
        {
            var bounds = _control.GetBounds();
            width = (wHint != SWT.DEFAULT) ? wHint : bounds.Width;
            height = (hHint != SWT.DEFAULT) ? hHint : bounds.Height;
        }
        else
        {
            width = _preferredWidth;
            height = _preferredHeight;
        }
    }

    private void UpdateControl()
    {
        if (_platformCoolItem != null && _control != null)
        {
            _platformCoolItem.SetControl(_control.PlatformWidget);
        }
    }

    protected override void ReleaseWidget()
    {
        // Remove from parent coolbar
        _parent?.RemoveItem(this);

        // Release control reference (don't dispose it, it belongs to the user)
        _control = null;

        // Dispose platform cool item
        _platformCoolItem?.Dispose();
        _platformCoolItem = null;

        _parent = null;

        base.ReleaseWidget();
    }
}
