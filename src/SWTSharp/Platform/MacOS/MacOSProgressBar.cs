using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformProgressBar using NSProgressIndicator.
/// Provides complete progress bar functionality with native macOS progress controls.
/// </summary>
internal class MacOSProgressBar : MacOSWidget, IPlatformProgressBar
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsProgressIndicatorHandle;
    private bool _disposed;
    private int _value = 0;
    private int _minimum = 0;
    private int _maximum = 100;
    private int _state = 0; // Normal state

    // Event handling
    public event EventHandler<int>? ValueChanged;

    public MacOSProgressBar(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSProgressBar] Creating progress bar. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSProgressIndicator
        _nsProgressIndicatorHandle = CreateNSProgressIndicator(parentHandle, style);

        if (_nsProgressIndicatorHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSProgressIndicator");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSProgressBar] Progress bar created successfully. Handle: 0x{_nsProgressIndicatorHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsProgressIndicatorHandle;
    }

    #region IPlatformProgressBar Implementation

    public int Value
    {
        get
        {
            if (_disposed) return 0;
            return _value;
        }
        set
        {
            if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return;

            var oldValue = _value;
            _value = Math.Max(_minimum, Math.Min(_maximum, value));

            // Call setDoubleValue:
            IntPtr selector = sel_registerName("setDoubleValue:");
            objc_msgSend_double(_nsProgressIndicatorHandle, selector, (double)_value);

            if (oldValue != _value)
            {
                ValueChanged?.Invoke(this, _value);
            }
        }
    }

    public int Minimum
    {
        get
        {
            if (_disposed) return 0;
            return _minimum;
        }
        set
        {
            if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return;

            _minimum = value;
            if (_maximum < _minimum) _maximum = _minimum;
            if (_value < _minimum) _value = _minimum;

            // Call setMinValue:
            IntPtr selector = sel_registerName("setMinValue:");
            objc_msgSend_double(_nsProgressIndicatorHandle, selector, (double)_minimum);
        }
    }

    public int Maximum
    {
        get
        {
            if (_disposed) return 100;
            return _maximum;
        }
        set
        {
            if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return;

            _maximum = value;
            if (_minimum > _maximum) _minimum = _maximum;
            if (_value > _maximum) _value = _maximum;

            // Call setMaxValue:
            IntPtr selector = sel_registerName("setMaxValue:");
            objc_msgSend_double(_nsProgressIndicatorHandle, selector, (double)_maximum);
        }
    }

    public int State
    {
        get
        {
            if (_disposed) return 0;
            return _state;
        }
        set
        {
            if (_disposed) return;
            _state = value;
            // macOS doesn't have direct state support like Windows
            // Different states would require different visual treatments
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsProgressIndicatorHandle, selector, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return default;

        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsProgressIndicatorHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsProgressIndicatorHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsProgressIndicatorHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setEnabled:");
        objc_msgSend_void(_nsProgressIndicatorHandle, selector, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsProgressIndicatorHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isEnabled");
        return objc_msgSend_bool(_nsProgressIndicatorHandle, selector);
    }

    public void SetBackground(RGB color)
    {
        // NSProgressIndicator doesn't support custom background colors
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // NSProgressIndicator doesn't support custom foreground colors directly
    }

    public RGB GetForeground()
    {
        return new RGB(0, 120, 215); // Default blue
    }

    #endregion

    #region IDisposable Implementation

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_nsProgressIndicatorHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsProgressIndicatorHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsProgressIndicatorHandle, releaseSelector);

            _nsProgressIndicatorHandle = IntPtr.Zero;
        }
    }

    #endregion

    #region Private Helper Methods

    private IntPtr CreateNSProgressIndicator(IntPtr parentHandle, int style)
    {
        // Get NSProgressIndicator class
        IntPtr progressClass = objc_getClass("NSProgressIndicator");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr progressIndicator = objc_msgSend(progressClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        progressIndicator = objc_msgSend(progressIndicator, initSelector);

        // Set initial frame
        var frame = new CGRect(0, 0, 200, 20);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(progressIndicator, setFrameSelector, frame);

        // Set to determinate style (bar style)
        IntPtr setIndeterminateSelector = sel_registerName("setIndeterminate:");
        objc_msgSend_void(progressIndicator, setIndeterminateSelector, false);

        // Set range
        IntPtr setMinValueSelector = sel_registerName("setMinValue:");
        objc_msgSend_double(progressIndicator, setMinValueSelector, 0.0);

        IntPtr setMaxValueSelector = sel_registerName("setMaxValue:");
        objc_msgSend_double(progressIndicator, setMaxValueSelector, 100.0);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, progressIndicator);
        }

        return progressIndicator;
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
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_double(IntPtr receiver, IntPtr selector, double arg);

    [DllImport(ObjCLibrary)]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary)]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, CGRect rect);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_stret")]
    private static extern void objc_msgSend_stret(out CGRect retval, IntPtr receiver, IntPtr selector);

    #endregion
}
