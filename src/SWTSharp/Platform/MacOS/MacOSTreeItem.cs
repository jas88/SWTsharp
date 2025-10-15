using System;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformTreeItem that adapts existing NSTreeNode data.
/// This bridges the existing pseudo-handle system to the new platform widget interface.
/// </summary>
internal class MacOSTreeItem : IPlatformTreeItem
{
    private readonly MacOSPlatform _platform;
    private readonly IntPtr _pseudoHandle; // The pseudo-handle used by existing implementation
    private bool _disposed;

    // Selection events
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;

    public MacOSTreeItem(MacOSPlatform platform, IntPtr pseudoHandle)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _pseudoHandle = pseudoHandle;
    }

    public void SetText(string text)
    {
        if (_disposed) return;

        // Use the existing platform implementation
        _platform.SetTreeItemText(_pseudoHandle, text ?? string.Empty);
    }

    public string GetText()
    {
        if (_disposed) return string.Empty;

        // The existing implementation doesn't have a GetText method
        // For now, return empty string - this would need to be added
        return string.Empty;
    }

    public void SetImage(IPlatformImage? image)
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
        _platform.SetTreeItemImage(_pseudoHandle, imageHandle);
    }

    public void SetExpanded(bool expanded)
    {
        if (_disposed) return;

        // Use the existing platform implementation
        _platform.SetTreeItemExpanded(_pseudoHandle, expanded);
    }

    public bool GetExpanded()
    {
        if (_disposed) return false;

        // The existing implementation doesn't have a GetExpanded method
        // For now, return false - this would need to be added
        return false;
    }

    public void SetChecked(bool @checked)
    {
        if (_disposed) return;

        // Use the existing platform implementation
        _platform.SetTreeItemChecked(_pseudoHandle, @checked);
    }

    public bool GetChecked()
    {
        if (_disposed) return false;

        // The existing implementation doesn't have a GetChecked method
        // For now, return false - this would need to be added
        return false;
    }

    public IntPtr GetNativeHandle()
    {
        return _pseudoHandle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Don't destroy the tree item here as it's managed by the tree
            // The existing MacOSPlatform.DestroyTreeItem is called by the tree
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