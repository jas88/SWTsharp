using System;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformTabItem that adapts existing NSTabViewItem implementation.
/// This bridges the existing pseudo-handle system to the new platform widget interface.
/// </summary>
internal class MacOSTabItem : IPlatformTabItem
{
    private readonly MacOSPlatform _platform;
    private readonly IntPtr _pseudoHandle; // The pseudo-handle used by existing implementation
    private bool _disposed;

    // Event handling
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    public MacOSTabItem(MacOSPlatform platform, IntPtr pseudoHandle)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _pseudoHandle = pseudoHandle;
    }

    public void SetText(string text)
    {
        if (_disposed) return;

        // Use the existing platform implementation
        _platform.SetTabItemText(_pseudoHandle, text ?? string.Empty);
    }

    public string GetText()
    {
        if (_disposed) return string.Empty;

        // The existing implementation doesn't have a GetText method
        // For now, return empty string - this would need to be added to MacOSPlatform_TabFolder.cs
        return string.Empty;
    }

    public void SetControl(IPlatformWidget? control)
    {
        if (_disposed) return;

        IntPtr controlHandle = IntPtr.Zero;
        if (control != null)
        {
            // If it's a MacOSWidget, get its native handle
            if (control is MacOSWidget macOSControl)
            {
                controlHandle = macOSControl.GetNativeHandle();
            }
            else
            {
                // For other IPlatformWidget implementations, we'd need conversion logic
                // This is a placeholder for future enhancement
                // TODO: Add conversion logic for other widget types
            }
        }

        // Use the existing platform implementation
        _platform.SetTabItemControl(_pseudoHandle, controlHandle);
    }

    public void SetToolTipText(string toolTip)
    {
        if (_disposed) return;

        // Use the existing platform implementation if available
        // For now, this is a no-op as the platform may not have this method yet
        // TODO: Add SetTabItemToolTip to MacOSPlatform_TabFolder.cs
    }

    public IntPtr GetNativeHandle()
    {
        return _pseudoHandle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // Don't destroy the tab item here as it's managed by the tab folder
            // The existing MacOSPlatform.DestroyTabItem is called by the tab folder
            _disposed = true;
        }
    }

    // Event handler methods
    private void OnClick()
    {
        if (_disposed) return;
        Click?.Invoke(this, 0);
    }

    private void OnFocusGained()
    {
        if (_disposed) return;
        FocusGained?.Invoke(this, 0);
    }

    private void OnFocusLost()
    {
        if (_disposed) return;
        FocusLost?.Invoke(this, 0);
    }

    private void OnKeyDown(PlatformKeyEventArgs args)
    {
        if (_disposed) return;
        KeyDown?.Invoke(this, args);
    }

    private void OnKeyUp(PlatformKeyEventArgs args)
    {
        if (_disposed) return;
        KeyUp?.Invoke(this, args);
    }
}