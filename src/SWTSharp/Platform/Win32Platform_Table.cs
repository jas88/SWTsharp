namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Table widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Table operations
    public IntPtr CreateTable(IntPtr parent, int style)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void SetTableHeaderVisible(IntPtr handle, bool visible)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void SetTableLinesVisible(IntPtr handle, bool visible)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void SetTableSelection(IntPtr handle, int[] indices)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void ClearTableItems(IntPtr handle)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    public void ShowTableItem(IntPtr handle, int index)
    {
        throw new NotImplementedException("Table not yet implemented on Win32 platform");
    }

    // TableColumn operations
    public IntPtr CreateTableColumn(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void DestroyTableColumn(IntPtr handle)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnText(IntPtr handle, string text)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnWidth(IntPtr handle, int width)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnAlignment(IntPtr handle, int alignment)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnResizable(IntPtr handle, bool resizable)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnMoveable(IntPtr handle, bool moveable)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public void SetTableColumnToolTipText(IntPtr handle, string? toolTip)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    public int PackTableColumn(IntPtr handle)
    {
        throw new NotImplementedException("TableColumn not yet implemented on Win32 platform");
    }

    // TableItem operations
    public IntPtr CreateTableItem(IntPtr parent, int style, int index)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void DestroyTableItem(IntPtr handle)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemText(IntPtr handle, int column, string text)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemImage(IntPtr handle, int column, IntPtr image)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemChecked(IntPtr handle, bool checked_)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemBackground(IntPtr handle, object? color)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemForeground(IntPtr handle, object? color)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }

    public void SetTableItemFont(IntPtr handle, object? font)
    {
        throw new NotImplementedException("TableItem not yet implemented on Win32 platform");
    }
}
