using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformScale using NSSlider with tick marks.
/// Provides complete scale functionality with native macOS slider controls and visible tick marks.
/// </summary>
internal class MacOSScale : MacOSWidget, IPlatformScale
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsSliderHandle;
    private bool _disposed;
    private int _value = 50;
    private int _minimum = 0;
    private int _maximum = 100;
    private int _increment = 1;
    private bool _showTicks = true; // Scale shows ticks by default

    // Event handling
    public event EventHandler<int>? ValueChanged;
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public MacOSScale(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSScale] Creating scale. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSSlider with tick marks
        _nsSliderHandle = CreateNSSliderWithTicks(parentHandle, style);

        if (_nsSliderHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSSlider for scale");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSScale] Scale created successfully. Handle: 0x{_nsSliderHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _nsSliderHandle;
    }

    #region IPlatformScale Implementation

    public int Value
    {
        get
        {
            if (_disposed || _nsSliderHandle == IntPtr.Zero) return 0;

            IntPtr selector = sel_registerName("doubleValue");
            double value = objc_msgSend_double_ret(_nsSliderHandle, selector);
            return (int)value;
        }
        set
        {
            if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

            var oldValue = _value;
            _value = Math.Max(_minimum, Math.Min(_maximum, value));

            IntPtr selector = sel_registerName("setDoubleValue:");
            objc_msgSend_double(_nsSliderHandle, selector, (double)_value);

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
            if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

            _minimum = value;
            if (_maximum < _minimum) _maximum = _minimum;
            if (_value < _minimum) _value = _minimum;

            IntPtr selector = sel_registerName("setMinValue:");
            objc_msgSend_double(_nsSliderHandle, selector, (double)_minimum);

            UpdateTickMarks();
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
            if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

            _maximum = value;
            if (_minimum > _maximum) _minimum = _maximum;
            if (_value > _maximum) _value = _maximum;

            IntPtr selector = sel_registerName("setMaxValue:");
            objc_msgSend_double(_nsSliderHandle, selector, (double)_maximum);

            UpdateTickMarks();
        }
    }

    public int Increment
    {
        get
        {
            if (_disposed) return 1;
            return _increment;
        }
        set
        {
            if (_disposed) return;

            _increment = Math.Max(1, value);
            UpdateTickMarks();
        }
    }

    public bool ShowTicks
    {
        get
        {
            if (_disposed) return false;
            return _showTicks;
        }
        set
        {
            if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

            _showTicks = value;

            if (_showTicks)
            {
                UpdateTickMarks();

                // Enable tick marks
                IntPtr setTickMarkPositionSelector = sel_registerName("setTickMarkPosition:");
                objc_msgSend(_nsSliderHandle, setTickMarkPositionSelector, 1); // NSTickMarkPositionBelow
            }
            else
            {
                // Disable tick marks
                IntPtr setNumberOfTickMarksSelector = sel_registerName("setNumberOfTickMarks:");
                objc_msgSend(_nsSliderHandle, setNumberOfTickMarksSelector, 0);
            }
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_nsSliderHandle, selector, frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero) return default;

        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _nsSliderHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_nsSliderHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_nsSliderHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setEnabled:");
        objc_msgSend_void(_nsSliderHandle, selector, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isEnabled");
        return objc_msgSend_bool(_nsSliderHandle, selector);
    }

    public void SetBackground(RGB color)
    {
        // NSSlider doesn't support custom background colors
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // NSSlider doesn't support custom foreground colors directly
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

        if (_nsSliderHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_nsSliderHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsSliderHandle, releaseSelector);

            _nsSliderHandle = IntPtr.Zero;
        }
    }

    #endregion

    #region Private Helper Methods

    private IntPtr CreateNSSliderWithTicks(IntPtr parentHandle, int style)
    {
        // Get NSSlider class
        IntPtr sliderClass = objc_getClass("NSSlider");

        // Allocate and initialize
        IntPtr allocSelector = sel_registerName("alloc");
        IntPtr slider = objc_msgSend(sliderClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        slider = objc_msgSend(slider, initSelector);

        // Set initial frame
        var frame = new CGRect(0, 0, 200, 30); // Slightly taller for tick marks
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(slider, setFrameSelector, frame);

        // Set range
        IntPtr setMinValueSelector = sel_registerName("setMinValue:");
        objc_msgSend_double(slider, setMinValueSelector, 0.0);

        IntPtr setMaxValueSelector = sel_registerName("setMaxValue:");
        objc_msgSend_double(slider, setMaxValueSelector, 100.0);

        // Set initial value
        IntPtr setDoubleValueSelector = sel_registerName("setDoubleValue:");
        objc_msgSend_double(slider, setDoubleValueSelector, 50.0);

        // Configure tick marks
        IntPtr setNumberOfTickMarksSelector = sel_registerName("setNumberOfTickMarks:");
        objc_msgSend(slider, setNumberOfTickMarksSelector, 11); // 0, 10, 20, ..., 100

        IntPtr setTickMarkPositionSelector = sel_registerName("setTickMarkPosition:");
        objc_msgSend(slider, setTickMarkPositionSelector, 1); // NSTickMarkPositionBelow

        IntPtr setAllowsTickMarkValuesOnlySelector = sel_registerName("setAllowsTickMarkValuesOnly:");
        objc_msgSend_void(slider, setAllowsTickMarkValuesOnlySelector, false);

        // Add to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            IntPtr addSubviewSelector = sel_registerName("addSubview:");
            objc_msgSend(parentHandle, addSubviewSelector, slider);
        }

        return slider;
    }

    private void UpdateTickMarks()
    {
        if (_disposed || _nsSliderHandle == IntPtr.Zero || !_showTicks) return;

        // Calculate number of tick marks based on increment
        int range = _maximum - _minimum;
        int tickCount = Math.Max(2, (range / _increment) + 1);

        IntPtr selector = sel_registerName("setNumberOfTickMarks:");
        objc_msgSend(_nsSliderHandle, selector, tickCount);
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
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg);

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

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_fpret")]
    private static extern double objc_msgSend_double_ret(IntPtr receiver, IntPtr selector);

    #endregion
}
