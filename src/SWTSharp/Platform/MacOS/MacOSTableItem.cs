using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using SWTSharp.Platform.MacOS;

namespace SWTSharp.Platform;

/// <summary>
/// macOS implementation of IPlatformTableItem that adapts existing NSTableView row data.
/// This bridges the existing pseudo-handle system to the new platform widget interface.
/// </summary>
internal class MacOSTableItem : IPlatformTableItem
{
    private readonly IntPtr _pseudoHandle; // The pseudo-handle used by existing implementation
    private bool _disposed;

    // Selection events
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;

    public MacOSTableItem(MacOSPlatform _, IntPtr pseudoHandle)
    {
        // Use singleton platform instance (parameter kept for compatibility)
        _pseudoHandle = pseudoHandle;
    }

    public void SetText(int column, string text)
    {
        if (_disposed) return;

        // Use the existing platform implementation
        ((MacOSPlatform)SWTSharp.Platform.PlatformFactory.Instance).SetTableItemText(_pseudoHandle, column, text ?? string.Empty);
    }

    public string GetText(int column)
    {
        if (_disposed) return string.Empty;

        // The existing implementation doesn't have a GetText method
        // For now, return empty string - this would need to be added
        return string.Empty;
    }

    public void SetImage(int column, IPlatformImage? image)
    {
        if (_disposed) return;

        IntPtr imageHandle = IntPtr.Zero;
        if (image != null)
        {
            // If it's a MacOSImage, get its native handle
            if (image is MacOSImage macOSImage)
            {
                imageHandle = macOSImage.GetNativeHandle();
            }
            else
            {
                // Image conversion requires NSImage creation from platform-agnostic Image data.
                // Non-MacOSImage types would need conversion through Image.GetImageData() ->
                // NSBitmapImageRep -> NSImage pipeline. Currently only MacOSImage supported.
            }
        }

        // Use the existing platform implementation
        ((MacOSPlatform)SWTSharp.Platform.PlatformFactory.Instance).SetTableItemImage(_pseudoHandle, column, imageHandle);
    }

    private RGB _backgroundColor = new RGB(255, 255, 255);

    public void SetBackground(RGB color)
    {
        if (_disposed) return;

        // TableItem background color requires custom cell rendering via NSTableCellView.
        // NSTableView uses data source pattern where cells query for display values.
        // Store value for GetBackground() API compatibility.
        _backgroundColor = color;
    }

    public RGB GetBackground()
    {
        if (_disposed) return new RGB(255, 255, 255);

        return _backgroundColor;
    }

    public IntPtr GetNativeHandle()
    {
        return _pseudoHandle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Don't destroy the table item here as it's managed by the table
            // The existing MacOSPlatform.DestroyTableItem is called by the table
            _disposed = true;
        }
    }

    // Event handler methods
    private void OnSelectionChanged(int selectedIndex)
    {
        if (_disposed) return;
        SelectionChanged?.Invoke(this, selectedIndex);
    }

    private void OnItemDoubleClick(int itemIndex)
    {
        if (_disposed) return;
        ItemDoubleClick?.Invoke(this, itemIndex);
    }
}