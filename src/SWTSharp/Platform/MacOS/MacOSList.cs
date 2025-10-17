using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformList using NSTableView in list mode.
/// Provides complete list widget functionality with native macOS list controls.
/// </summary>
internal class MacOSList : MacOSWidget, IPlatformList
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsScrollViewHandle;
    private IntPtr _nsTableViewHandle;
    private readonly List<string> _items = new();
    private readonly List<int> _selectedIndices = new();
    private bool _disposed;

    // Event handling
    public event EventHandler<int>? SelectionChanged;
    #pragma warning disable CS0067
    public event EventHandler<int>? ItemDoubleClick;
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;

    public MacOSList(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSList] Creating list. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSTableView wrapped in NSScrollView
        CreateNSTableView(parentHandle, style);

        if (_nsTableViewHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSTableView for list");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSList] List created successfully. Handle: 0x{_nsTableViewHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsScrollViewHandle; // Return scroll view as main handle
    }

    #region IPlatformList Implementation

    public void AddItem(string item)
    {
        if (_disposed || string.IsNullOrEmpty(item)) return;

        _items.Add(item);
        ReloadData();
    }

    public void ClearItems()
    {
        if (_disposed) return;

        _items.Clear();
        _selectedIndices.Clear();
        ReloadData();
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
            if (_disposed || _nsTableViewHandle == IntPtr.Zero) return Array.Empty<int>();

            // Get selected row indexes from NSTableView
            IntPtr selector = sel_registerName("selectedRowIndexes");
            IntPtr indexSet = objc_msgSend(_nsTableViewHandle, selector);

            // Convert NSIndexSet to array
            return ConvertIndexSetToArray(indexSet);
        }
        set
        {
            if (_disposed || _nsTableViewHandle == IntPtr.Zero || value == null) return;

            var oldIndices = SelectionIndices;

            // Create NSIndexSet from array
            IntPtr indexSet = ConvertArrayToIndexSet(value);

            // Select rows
            IntPtr selector = sel_registerName("selectRowIndexes:byExtendingSelection:");
            objc_msgSend_indexSet(_nsTableViewHandle, selector, indexSet, false);

            // Fire SelectionChanged event if selection actually changed
            if (!oldIndices.SequenceEqual(value))
            {
                var selectedIndex = value.Length > 0 ? value[0] : -1;
                SelectionChanged?.Invoke(this, selectedIndex);
            }

            // Release index set
            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(indexSet, releaseSelector);
        }
    }

    public int SelectionIndex
    {
        get
        {
            if (_disposed || _nsTableViewHandle == IntPtr.Zero) return -1;

            IntPtr selector = sel_registerName("selectedRow");
            IntPtr result = objc_msgSend(_nsTableViewHandle, selector);
            return (int)result.ToInt64();
        }
        set
        {
            if (_disposed || _nsTableViewHandle == IntPtr.Zero) return;
            if (value < -1 || value >= _items.Count) return;

            int oldIndex = SelectionIndex;

            if (value >= 0)
            {
                IntPtr selector = sel_registerName("selectRowIndexes:byExtendingSelection:");
                IntPtr indexSet = ConvertArrayToIndexSet(new[] { value });
                objc_msgSend_indexSet(_nsTableViewHandle, selector, indexSet, false);

                IntPtr releaseSelector = sel_registerName("release");
                objc_msgSend(indexSet, releaseSelector);
            }
            else
            {
                // Deselect all
                IntPtr selector = sel_registerName("deselectAll:");
                objc_msgSend(_nsTableViewHandle, selector, IntPtr.Zero);
            }

            // Fire SelectionChanged event if selection actually changed
            if (oldIndex != value)
            {
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsScrollViewHandle == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsScrollViewHandle, selector, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsScrollViewHandle == IntPtr.Zero) return default;

        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsScrollViewHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsScrollViewHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsScrollViewHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsScrollViewHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsScrollViewHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsTableViewHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setEnabled:");
        objc_msgSend_void(_nsTableViewHandle, selector, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsTableViewHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isEnabled");
        return objc_msgSend_bool(_nsTableViewHandle, selector);
    }

    public void SetBackground(RGB color)
    {
        // Background color for NSTableView requires layer support
        // TODO: Implement via wantsLayer and backgroundColor
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // Foreground color would apply to text
        // TODO: Implement via cell text color
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Release NSTableView and NSScrollView
        if (_nsTableViewHandle != IntPtr.Zero)
        {
            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsTableViewHandle, releaseSelector);
            _nsTableViewHandle = IntPtr.Zero;
        }

        if (_nsScrollViewHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsScrollViewHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsScrollViewHandle, releaseSelector);
            _nsScrollViewHandle = IntPtr.Zero;
        }
    }

    #endregion

    #region Private Helper Methods

    private void CreateNSTableView(IntPtr parentHandle, int style)
    {
        // Create NSScrollView
        IntPtr scrollViewClass = objc_getClass("NSScrollView");
        IntPtr allocSelector = sel_registerName("alloc");
        _nsScrollViewHandle = objc_msgSend(scrollViewClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        _nsScrollViewHandle = objc_msgSend(_nsScrollViewHandle, initSelector);

        // Set scroll view frame
        var frame = new CGRect(0, 0, 200, 150);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsScrollViewHandle, setFrameSelector, frame);

        // Create NSTableView
        IntPtr tableViewClass = objc_getClass("NSTableView");
        _nsTableViewHandle = objc_msgSend(tableViewClass, allocSelector);
        _nsTableViewHandle = objc_msgSend(_nsTableViewHandle, initSelector);

        // Add single column to table view
        AddColumn();

        // Set table view as document view of scroll view
        IntPtr setDocumentViewSelector = sel_registerName("setDocumentView:");
        objc_msgSend(_nsScrollViewHandle, setDocumentViewSelector, _nsTableViewHandle);

        // Configure scroll view
        IntPtr setHasVerticalScrollerSelector = sel_registerName("setHasVerticalScroller:");
        objc_msgSend_void(_nsScrollViewHandle, setHasVerticalScrollerSelector, true);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, _nsScrollViewHandle);
        }
    }

    private void AddColumn()
    {
        // Create NSTableColumn
        IntPtr columnClass = objc_getClass("NSTableColumn");
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr column = objc_msgSend(columnClass, allocSelector);

        // Initialize with identifier
        IntPtr identifier = CreateNSString("Column");
        IntPtr initSelector = sel_registerName("initWithIdentifier:");
        column = objc_msgSend(column, initSelector, identifier);

        // Add column to table view
        IntPtr addColumnSelector = sel_registerName("addTableColumn:");
        objc_msgSend(_nsTableViewHandle, addColumnSelector, column);

        // Release identifier
        IntPtr releaseSelector = sel_registerName("release");
        objc_msgSend(identifier, releaseSelector);
    }

    private void ReloadData()
    {
        if (_disposed || _nsTableViewHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("reloadData");
        objc_msgSend(_nsTableViewHandle, selector);
    }

    private IntPtr CreateNSString(string str)
    {
        if (string.IsNullOrEmpty(str)) str = "";

        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr nsString = objc_msgSend(nsStringClass, allocSelector);

        IntPtr initSelector = sel_registerName("initWithUTF8String:");
        IntPtr utf8Ptr = Marshal.StringToHGlobalAnsi(str);
        nsString = objc_msgSend(nsString, initSelector, utf8Ptr);
        Marshal.FreeHGlobal(utf8Ptr);

        return nsString;
    }

    private int[] ConvertIndexSetToArray(IntPtr indexSet)
    {
        if (indexSet == IntPtr.Zero) return Array.Empty<int>();

        // Get count
        IntPtr countSelector = sel_registerName("count");
        IntPtr countResult = objc_msgSend(indexSet, countSelector);
        int count = (int)countResult.ToInt64();

        if (count == 0) return Array.Empty<int>();

        var indices = new List<int>();

        // Iterate through index set
        IntPtr firstIndexSelector = sel_registerName("firstIndex");
        int index = (int)objc_msgSend(indexSet, firstIndexSelector).ToInt64();

        while (index != -1 && indices.Count < count)
        {
            indices.Add(index);
            IntPtr indexGreaterThanSelector = sel_registerName("indexGreaterThanIndex:");
            index = (int)objc_msgSend(indexSet, indexGreaterThanSelector, index).ToInt64();
        }

        return indices.ToArray();
    }

    private IntPtr ConvertArrayToIndexSet(int[] indices)
    {
        // Create mutable index set
        IntPtr indexSetClass = objc_getClass("NSMutableIndexSet");
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr indexSet = objc_msgSend(indexSetClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        indexSet = objc_msgSend(indexSet, initSelector);

        // Add each index
        IntPtr addIndexSelector = sel_registerName("addIndex:");
        foreach (int index in indices)
        {
            if (index >= 0 && index < _items.Count)
            {
                objc_msgSend(indexSet, addIndexSelector, index);
            }
        }

        return indexSet;
    }

    #endregion

    #region ObjC P/Invoke

    [StructLayout(LayoutKind.Sequential)]
    private struct CGRect
    {
        public double x;
        public double y;
        public double width;
        public double height;

        public CGRect(double x, double y, double width, double height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_getClass(string className);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport(ObjCLibrary)]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport(ObjCLibrary)]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_indexSet(IntPtr receiver, IntPtr selector, IntPtr indexSet, bool extending);

    #endregion
}
