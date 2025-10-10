using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Linux/GTK platform implementation - ToolBar widget methods.
/// </summary>
internal partial class LinuxPlatform
{
    // ToolBar data storage
    private sealed class ToolBarData
    {
        public IntPtr Toolbar { get; set; }
        public List<IntPtr> Items { get; set; } = new();
    }

    private sealed class ToolItemData
    {
        public IntPtr ToolItem { get; set; }
        public IntPtr ToolBarHandle { get; set; }
        public int Style { get; set; }
        public IntPtr IconWidget { get; set; }
        public IntPtr Label { get; set; }
    }

    private readonly Dictionary<IntPtr, ToolBarData> _toolBarData = new();
    private readonly Dictionary<IntPtr, ToolItemData> _toolItemData = new();
    private int _nextToolItemHandle = 1;

    // GTK Toolbar P/Invoke declarations
    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_toolbar_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toolbar_insert(IntPtr toolbar, IntPtr item, int pos);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toolbar_set_style(IntPtr toolbar, GtkToolbarStyle style);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern IntPtr gtk_tool_button_new(IntPtr icon_widget, string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_toggle_tool_button_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_separator_tool_item_new();

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_menu_tool_button_new(IntPtr icon_widget, string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tool_button_set_label(IntPtr button, string? label);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tool_button_set_icon_widget(IntPtr button, IntPtr icon_widget);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
    private static extern void gtk_tool_item_set_tooltip_text(IntPtr tool_item, string? text);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_toggle_tool_button_set_active(IntPtr button, bool is_active);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_toggle_tool_button_get_active(IntPtr button);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_separator_tool_item_set_draw(IntPtr item, bool draw);

    [DllImport(GtkLib, CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_tool_item_set_homogeneous(IntPtr tool_item, bool homogeneous);

    private enum GtkToolbarStyle
    {
        Icons = 0,
        Text = 1,
        Both = 2,
        BothHoriz = 3
    }

    // ToolBar operations
    public IntPtr CreateToolBar(IntPtr parent, int style)
    {
        // Create GtkToolbar
        IntPtr toolbar = gtk_toolbar_new();
        if (toolbar == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK Toolbar");

        // Set orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            gtk_orientable_set_orientation(toolbar, GtkOrientation.Vertical);
        }
        else
        {
            gtk_orientable_set_orientation(toolbar, GtkOrientation.Horizontal);
        }

        // Set toolbar style - default to icons and text
        // FLAT style typically means icons only
        if ((style & SWT.FLAT) != 0)
        {
            gtk_toolbar_set_style(toolbar, GtkToolbarStyle.Icons);
        }
        else
        {
            gtk_toolbar_set_style(toolbar, GtkToolbarStyle.Both);
        }

        // Track the toolbar
        _toolBarData[toolbar] = new ToolBarData
        {
            Toolbar = toolbar
        };

        gtk_widget_show(toolbar);
        return toolbar;
    }

    // ToolItem operations
    public IntPtr CreateToolItem(IntPtr toolBarHandle, int style, int id, int index)
    {
        if (!_toolBarData.TryGetValue(toolBarHandle, out var barData))
            throw new ArgumentException("Invalid toolbar handle", nameof(toolBarHandle));

        // Create a pseudo-handle for the tool item
        IntPtr itemHandle = new IntPtr(_nextToolItemHandle++);

        IntPtr toolItem;

        // Create appropriate tool item type based on style
        if ((style & SWT.SEPARATOR) != 0)
        {
            toolItem = gtk_separator_tool_item_new();
            // Make separator visible by default
            gtk_separator_tool_item_set_draw(toolItem, true);
        }
        else if ((style & SWT.DROP_DOWN) != 0)
        {
            // Menu tool button for drop-down items
            toolItem = gtk_menu_tool_button_new(IntPtr.Zero, null);
        }
        else if ((style & SWT.CHECK) != 0 || (style & SWT.RADIO) != 0)
        {
            // Toggle tool button for CHECK and RADIO
            toolItem = gtk_toggle_tool_button_new();
        }
        else // Default to PUSH
        {
            toolItem = gtk_tool_button_new(IntPtr.Zero, null);
        }

        if (toolItem == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create GTK ToolItem");

        // Insert or append the item
        if (index < 0 || index >= barData.Items.Count)
        {
            gtk_toolbar_insert(toolBarHandle, toolItem, -1);
        }
        else
        {
            gtk_toolbar_insert(toolBarHandle, toolItem, index);
        }

        // Track the tool item
        var itemData = new ToolItemData
        {
            ToolItem = toolItem,
            ToolBarHandle = toolBarHandle,
            Style = style,
            IconWidget = IntPtr.Zero,
            Label = IntPtr.Zero
        };

        _toolItemData[itemHandle] = itemData;
        barData.Items.Add(itemHandle);

        gtk_widget_show(toolItem);

        return itemHandle;
    }

    public void DestroyToolBar(IntPtr handle)
    {
        if (!_toolBarData.TryGetValue(handle, out var barData))
            return;

        // Destroy all tool items first
        foreach (var itemHandle in barData.Items.ToArray())
        {
            DestroyToolItem(itemHandle);
        }

        // Destroy the GTK widget
        if (barData.Toolbar != IntPtr.Zero)
        {
            gtk_widget_destroy(barData.Toolbar);
        }

        // Remove from tracking
        _toolBarData.Remove(handle);
    }

    public void DestroyToolItem(IntPtr handle)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Remove from toolbar's item list
        if (_toolBarData.TryGetValue(itemData.ToolBarHandle, out var barData))
        {
            barData.Items.Remove(handle);
        }

        // Destroy the GTK widget
        if (itemData.ToolItem != IntPtr.Zero)
        {
            gtk_widget_destroy(itemData.ToolItem);
        }

        // Remove from tracking
        _toolItemData.Remove(handle);
    }

    public void SetToolItemText(IntPtr handle, string text)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Don't set text on separators
        if ((itemData.Style & SWT.SEPARATOR) != 0)
            return;

        gtk_tool_button_set_label(itemData.ToolItem, text ?? "");
    }

    public void SetToolItemImage(IntPtr handle, IntPtr imageHandle)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Don't set image on separators
        if ((itemData.Style & SWT.SEPARATOR) != 0)
            return;

        // For now, we'll use imageHandle directly as the icon widget
        // In a full implementation, this would convert an Image to a GtkWidget
        if (imageHandle != IntPtr.Zero)
        {
            gtk_tool_button_set_icon_widget(itemData.ToolItem, imageHandle);
            itemData.IconWidget = imageHandle;
        }
        else
        {
            gtk_tool_button_set_icon_widget(itemData.ToolItem, IntPtr.Zero);
            itemData.IconWidget = IntPtr.Zero;
        }
    }

    public void SetToolItemToolTip(IntPtr handle, string text)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        gtk_tool_item_set_tooltip_text(itemData.ToolItem, text ?? "");
    }

    public void SetToolItemSelection(IntPtr handle, bool selected)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Only works for CHECK and RADIO items
        if ((itemData.Style & (SWT.CHECK | SWT.RADIO)) != 0)
        {
            gtk_toggle_tool_button_set_active(itemData.ToolItem, selected);
        }
    }

    public void SetToolItemEnabled(IntPtr handle, bool enabled)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        gtk_widget_set_sensitive(itemData.ToolItem, enabled);
    }

    public void SetToolItemWidth(IntPtr handle, int width)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Only applicable to separators
        if ((itemData.Style & SWT.SEPARATOR) != 0)
        {
            gtk_widget_set_size_request(itemData.ToolItem, width, -1);
        }
    }

    public void SetToolItemControl(IntPtr handle, IntPtr controlHandle)
    {
        if (!_toolItemData.TryGetValue(handle, out var itemData))
            return;

        // Custom controls in toolbars require creating a custom GtkToolItem
        // For now, this is a simplified implementation
        // A full implementation would use gtk_tool_item_new() and add the control as a child
        if (controlHandle != IntPtr.Zero)
        {
            // Add control to the tool item
            // Note: This requires the tool item to be a container
            gtk_container_add(itemData.ToolItem, controlHandle);
            gtk_widget_show(controlHandle);
        }
    }
}
