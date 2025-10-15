using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformList that adapts the existing NSTableView implementation.
/// This bridges the existing pseudo-handle system to the new platform widget interface.
/// </summary>
internal class MacOSList : MacOSWidget, IPlatformList
{
    private readonly MacOSPlatform _platform;
    private readonly IntPtr _pseudoHandle; // The pseudo-handle used by existing implementation
    private readonly List<string> _items = new();
    private readonly List<int> _selectedIndices = new();
    private bool _disposed;

    // Event handling
    public event EventHandler<int>? SelectionChanged;
    public event EventHandler<int>? ItemDoubleClick;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    public MacOSList(IntPtr parentHandle, int style)
    {
        _platform = new MacOSPlatform(); // Get platform instance for method calls

        // TODO: Implement CreateList in MacOSPlatform
        // _pseudoHandle = _platform.CreateList(parentHandle, style);
        _pseudoHandle = IntPtr.Zero; // Placeholder

        if (_pseudoHandle == IntPtr.Zero)
        {
            // For now, create a dummy handle to avoid null reference exceptions
            _pseudoHandle = new IntPtr(0x61000000); // Pseudo-handle pattern
            // throw new InvalidOperationException("Failed to create NSTableView - CreateList not implemented yet");
        }
    }

    public void AddItem(string item)
    {
        if (_disposed || string.IsNullOrEmpty(item)) return;

        _items.Add(item);
        // TODO: Implement AddListItem in MacOSPlatform
        // _platform.AddListItem(_pseudoHandle, item);
    }

    public void ClearItems()
    {
        if (_disposed) return;

        _items.Clear();
        _selectedIndices.Clear();
        // TODO: Implement ClearListItems in MacOSPlatform
        // _platform.ClearListItems(_pseudoHandle);
    }

    public int GetItemCount()
    {
        if (_disposed) return 0;
        return _items.Count;
    }

    public string GetItemAt(int index)
    {
        if (_disposed || index < 0 || index >= _items.Count)
            return string.Empty;

        return _items[index];
    }

    public int[] SelectionIndices
    {
        get
        {
            if (_disposed) return Array.Empty<int>();
            return _selectedIndices.ToArray();
        }
        set
        {
            if (_disposed || value == null) return;

            var oldIndices = _selectedIndices.ToArray();
            _selectedIndices.Clear();
            _selectedIndices.AddRange(value.Where(i => i >= 0 && i < _items.Count));

            // Update the platform selection
            // TODO: Implement SetListSelection in MacOSPlatform
            // _platform.SetListSelection(_pseudoHandle, _selectedIndices.Count > 0 ? _selectedIndices.ToArray() : Array.Empty<int>());

            // Fire SelectionChanged event if selection actually changed
            if (!oldIndices.SequenceEqual(_selectedIndices))
            {
                var selectedIndex = _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
                SelectionChanged?.Invoke(this, selectedIndex);
            }
        }
    }

    public int SelectionIndex
    {
        get
        {
            if (_disposed) return -1;
            return _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
        }
        set
        {
            if (_disposed || value < -1 || value >= _items.Count) return;

            var oldIndex = _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
            _selectedIndices.Clear();
            if (value >= 0)
            {
                _selectedIndices.Add(value);
            }

            // Update the platform selection
            // TODO: Implement SetListSelection in MacOSPlatform
            // _platform.SetListSelection(_pseudoHandle, _selectedIndices.Count > 0 ? _selectedIndices.ToArray() : Array.Empty<int>());

            // Fire SelectionChanged event if selection actually changed
            if (oldIndex != value)
            {
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    // IPlatformWidget interface implementation
    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed) return;
        _platform.SetControlBounds(_pseudoHandle, x, y, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed) return default(Rectangle);
        // For now, return default bounds - would need to implement GetControlBounds
        return default(Rectangle);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed) return;
        _platform.SetControlVisible(_pseudoHandle, visible);
    }

    public bool GetVisible()
    {
        if (_disposed) return false;
        // For now, return true - would need to implement GetControlVisible
        return true;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed) return;
        _platform.SetControlEnabled(_pseudoHandle, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed) return false;
        // For now, return true - would need to implement GetControlEnabled
        return true;
    }

    public void SetBackground(RGB color)
    {
        // Not implemented for lists
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // Not implemented for lists
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    public override IntPtr GetNativeHandle()
    {
        return _pseudoHandle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            // TODO: Implement DestroyList in MacOSPlatform
            // _platform.DestroyList(_pseudoHandle);
            _disposed = true;
        }
    }

    // Event handling methods
    private void OnSelectionChanged()
    {
        if (_disposed) return;
        var selectedIndex = _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
        SelectionChanged?.Invoke(this, selectedIndex);
    }

    private void OnItemDoubleClick()
    {
        if (_disposed) return;
        var selectedIndex = _selectedIndices.Count > 0 ? _selectedIndices[0] : -1;
        ItemDoubleClick?.Invoke(this, selectedIndex);
    }

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