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
                // For other IPlatformImage implementations, we'd need conversion logic
                // TODO: Add conversion logic for other image types
            }
        }

        // Use the existing platform implementation
        ((MacOSPlatform)SWTSharp.Platform.PlatformFactory.Instance).SetTableItemImage(_pseudoHandle, column, imageHandle);
    }

    public void SetBackground(RGB color)
    {
        if (_disposed) return;

        // IPlatformTableItem doesn't have background support in existing implementation
        // This would need to be added to MacOSPlatform_Table.cs
        // TODO: Add background color support
    }

    public RGB GetBackground()
    {
        if (_disposed) return new RGB(255, 255, 255); // Default white

        // IPlatformTableItem doesn't have background support in existing implementation
        // This would need to be added to MacOSPlatform_Table.cs
        // TODO: Add background color support
        return new RGB(255, 255, 255); // Default white
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