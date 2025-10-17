using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformCoolBar using NSStackView for rearrangeable bands.
/// Note: macOS doesn't have a native "rebar" control like Windows, so we use NSStackView.
/// </summary>
internal class MacOSCoolBar : MacOSWidget, IPlatformCoolBar
{
    private readonly IntPtr _nsStackView;
    private readonly List<MacOSCoolItem> _items = new();
    private bool _disposed;
    private bool _locked = false;

    // Import Objective-C runtime functions
    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_getClass")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "sel_registerName")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, long arg1);

    public MacOSCoolBar(IntPtr parent, int style)
    {
        // Create NSStackView (available since macOS 10.9)
        IntPtr stackViewClass = objc_getClass("NSStackView");
        IntPtr selAlloc = sel_registerName("alloc");
        IntPtr selInit = sel_registerName("init");

        _nsStackView = objc_msgSend(stackViewClass, selAlloc);
        _nsStackView = objc_msgSend(_nsStackView, selInit);

        if (_nsStackView == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create NSStackView for CoolBar");

        // Set orientation (0 = horizontal, 1 = vertical)
        IntPtr selSetOrientation = sel_registerName("setOrientation:");
        long orientation = (style & SWT.VERTICAL) != 0 ? 1 : 0;
        objc_msgSend_void(_nsStackView, selSetOrientation, orientation);

        // Set spacing between items
        IntPtr selSetSpacing = sel_registerName("setSpacing:");
        double spacing = 2.0;
        unsafe
        {
            IntPtr spacingPtr = *(IntPtr*)&spacing;
            objc_msgSend(_nsStackView, selSetSpacing, spacingPtr);
        }

        // Set distribution mode (fill equally)
        IntPtr selSetDistribution = sel_registerName("setDistribution:");
        objc_msgSend_void(_nsStackView, selSetDistribution, 0); // NSStackViewDistributionGravityAreas

        // Add to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selAddSubview = sel_registerName("addSubview:");
            objc_msgSend_void(parent, selAddSubview, _nsStackView);
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsStackView;
    }

    public IPlatformCoolItem CreateItem(int index, int style)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSCoolBar));

        var item = new MacOSCoolItem(this, index, style);

        if (index < 0 || index >= _items.Count)
        {
            _items.Add(item);
        }
        else
        {
            _items.Insert(index, item);
        }

        return item;
    }

    public void RemoveItem(int index)
    {
        if (_disposed) return;
        if (index < 0 || index >= _items.Count) return;

        _items[index].Dispose();
        _items.RemoveAt(index);
    }

    public int GetItemCount()
    {
        if (_disposed) return 0;
        return _items.Count;
    }

    public bool GetLocked()
    {
        return _locked;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;
        // On macOS, we would disable drag gestures on the stack view items
        // For simplicity, we'll just track the state for now
    }

    internal void AddArrangedSubview(IntPtr view, int index)
    {
        if (index < 0)
        {
            // Append to end
            IntPtr selAddArrangedSubview = sel_registerName("addArrangedSubview:");
            objc_msgSend_void(_nsStackView, selAddArrangedSubview, view);
        }
        else
        {
            // Insert at index
            IntPtr selInsertArrangedSubview = sel_registerName("insertArrangedSubview:atIndex:");
            objc_msgSend_void(_nsStackView, selInsertArrangedSubview, view);
            objc_msgSend_void(_nsStackView, sel_registerName("atIndex:"), (long)index);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            foreach (var item in _items.ToArray())
            {
                try
                {
                    item.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing coolbar item: {ex.Message}");
                }
            }
            _items.Clear();

            if (_nsStackView != IntPtr.Zero)
            {
                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(_nsStackView, selRelease);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error disposing coolbar: {ex.Message}");
        }
        finally
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// macOS implementation of IPlatformCoolItem.
    /// </summary>
    internal class MacOSCoolItem : IPlatformCoolItem
    {
        private readonly MacOSCoolBar _parent;
        private readonly int _index;
        private readonly int _style;
        private IntPtr _containerView;
        private IntPtr _childView;
        private int _preferredWidth = 100;
        private int _preferredHeight = 24;
        private int _minimumWidth = 0;
        private int _minimumHeight = 0;
        private bool _disposed;

        public MacOSCoolItem(MacOSCoolBar parent, int index, int style)
        {
            _parent = parent;
            _index = index < 0 ? parent.GetItemCount() : index;
            _style = style;

            // Create NSView as container for the cool item
            IntPtr nsViewClass = objc_getClass("NSView");
            IntPtr selAlloc = sel_registerName("alloc");
            IntPtr selInit = sel_registerName("init");

            _containerView = objc_msgSend(objc_msgSend(nsViewClass, selAlloc), selInit);

            if (_containerView == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create NSView for CoolItem");
            }

            // Set initial frame
            IntPtr selSetFrame = sel_registerName("setFrame:");
            SetFrame(_containerView, 0, 0, _preferredWidth, _preferredHeight);

            // Add to parent stack view
            _parent.AddArrangedSubview(_containerView, _index);
        }

        public void SetControl(IPlatformWidget? control)
        {
            // Extract native NSView handle using reflection
            if (control != null)
            {
                var getNativeHandleMethod = control.GetType().GetMethod("GetNativeHandle",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (getNativeHandleMethod != null)
                {
                    _childView = (IntPtr)(getNativeHandleMethod.Invoke(control, null) ?? IntPtr.Zero);
                }
                else
                {
                    _childView = IntPtr.Zero;
                }
            }
            else
            {
                _childView = IntPtr.Zero;
            }

            if (_childView != IntPtr.Zero)
            {
                IntPtr selAddSubview = sel_registerName("addSubview:");
                objc_msgSend_void(_containerView, selAddSubview, _childView);

                // Resize child to fill container
                SetFrame(_childView, 0, 0, _preferredWidth, _preferredHeight);
            }
        }

        public Rectangle GetBounds()
        {
            // Would need to query NSView frame to get actual bounds
            return new Rectangle(0, 0, _preferredWidth, _preferredHeight);
        }

        public void SetPreferredSize(int width, int height)
        {
            _preferredWidth = width;
            _preferredHeight = height;

            if (_containerView != IntPtr.Zero)
            {
                SetFrame(_containerView, 0, 0, width, height);

                if (_childView != IntPtr.Zero)
                {
                    SetFrame(_childView, 0, 0, width, height);
                }
            }
        }

        public void SetMinimumSize(int width, int height)
        {
            _minimumWidth = width;
            _minimumHeight = height;

            // NSView doesn't have direct minimum size constraints without Auto Layout
            // For simplicity, we'll just ensure preferred size is at least minimum
            if (_containerView != IntPtr.Zero)
            {
                int actualWidth = Math.Max(width, _preferredWidth);
                int actualHeight = Math.Max(height, _preferredHeight);
                SetFrame(_containerView, 0, 0, actualWidth, actualHeight);
            }
        }

        private void SetFrame(IntPtr view, double x, double y, double width, double height)
        {
            // Create CGRect and call setFrame:
            IntPtr selSetFrame = sel_registerName("setFrame:");

            unsafe
            {
                double* frame = stackalloc double[4];
                frame[0] = x;
                frame[1] = y;
                frame[2] = width;
                frame[3] = height;

                objc_msgSend(view, selSetFrame, (IntPtr)frame);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_containerView != IntPtr.Zero)
            {
                IntPtr selRemoveFromSuperview = sel_registerName("removeFromSuperview");
                objc_msgSend_void(_containerView, selRemoveFromSuperview);

                IntPtr selRelease = sel_registerName("release");
                objc_msgSend_void(_containerView, selRelease);
                _containerView = IntPtr.Zero;
            }

            _childView = IntPtr.Zero;
        }

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, IntPtr arg1);
    }
}
