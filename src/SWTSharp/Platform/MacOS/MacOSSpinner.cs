using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of IPlatformSpinner using NSStepper combined with NSTextField.
/// Provides complete spinner functionality with native macOS stepper controls.
/// </summary>
internal class MacOSSpinner : MacOSWidget, IPlatformSpinner
{
    private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";

    private IntPtr _nsStepperHandle;
    private IntPtr _nsTextFieldHandle;
    private IntPtr _containerViewHandle;
    private bool _disposed;
    private int _value = 0;
    private int _minimum = 0;
    private int _maximum = 100;
    private int _increment = 1;
    private int _digits = 0;

    // Event handling
    public event EventHandler<int>? ValueChanged;
    #pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
    #pragma warning restore CS0067

    public MacOSSpinner(IntPtr parentHandle, int style)
    {
        bool enableLogging = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG") == "1";

        if (enableLogging)
            Console.WriteLine($"[MacOSSpinner] Creating spinner. Parent: 0x{parentHandle:X}, Style: 0x{style:X}");

        // Create NSStepper with NSTextField
        CreateNSSpinner(parentHandle, style);

        if (_nsStepperHandle == IntPtr.Zero || _nsTextFieldHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create NSStepper and NSTextField for spinner");
        }

        if (enableLogging)
            Console.WriteLine($"[MacOSSpinner] Spinner created successfully. Stepper: 0x{_nsStepperHandle:X}, TextField: 0x{_nsTextFieldHandle:X}");
    }

    public override IntPtr GetNativeHandle()
    {
        return _containerViewHandle; // Return container view as main handle
    }

    #region IPlatformSpinner Implementation

    public int Value
    {
        get
        {
            if (_disposed || _nsStepperHandle == IntPtr.Zero) return 0;

            IntPtr selector = sel_registerName("doubleValue");
            double value = objc_msgSend_double_ret(_nsStepperHandle, selector);
            return (int)value;
        }
        set
        {
            if (_disposed || _nsStepperHandle == IntPtr.Zero) return;

            var oldValue = _value;
            _value = Math.Max(_minimum, Math.Min(_maximum, value));

            // Update stepper
            IntPtr selector = sel_registerName("setDoubleValue:");
            objc_msgSend_double(_nsStepperHandle, selector, (double)_value);

            // Update text field
            UpdateTextField();

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
            if (_disposed || _nsStepperHandle == IntPtr.Zero) return;

            _minimum = value;
            if (_maximum < _minimum) _maximum = _minimum;
            if (_value < _minimum) _value = _minimum;

            IntPtr selector = sel_registerName("setMinValue:");
            objc_msgSend_double(_nsStepperHandle, selector, (double)_minimum);
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
            if (_disposed || _nsStepperHandle == IntPtr.Zero) return;

            _maximum = value;
            if (_minimum > _maximum) _minimum = _maximum;
            if (_value > _maximum) _value = _maximum;

            IntPtr selector = sel_registerName("setMaxValue:");
            objc_msgSend_double(_nsStepperHandle, selector, (double)_maximum);
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
            if (_disposed || _nsStepperHandle == IntPtr.Zero) return;

            _increment = Math.Max(1, value);

            IntPtr selector = sel_registerName("setIncrement:");
            objc_msgSend_double(_nsStepperHandle, selector, (double)_increment);
        }
    }

    public int Digits
    {
        get
        {
            if (_disposed) return 0;
            return _digits;
        }
        set
        {
            if (_disposed) return;

            _digits = Math.Max(0, value);
            UpdateTextField();
        }
    }

    #endregion

    #region IPlatformWidget Implementation

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _containerViewHandle == IntPtr.Zero) return;

        var frame = new CGRect(x, y, width, height);
        IntPtr selector = sel_registerName("setFrame:");
        objc_msgSend_rect(_containerViewHandle, selector, frame);

        // Reposition child controls
        var textFieldFrame = new CGRect(0, 0, width - 25, height);
        objc_msgSend_rect(_nsTextFieldHandle, selector, textFieldFrame);

        var stepperFrame = new CGRect(width - 24, 0, 24, height);
        objc_msgSend_rect(_nsStepperHandle, selector, stepperFrame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _containerViewHandle == IntPtr.Zero) return default;

        IntPtr selector = sel_registerName("frame");
        objc_msgSend_stret(out CGRect frame, _containerViewHandle, selector);

        return new Rectangle((int)frame.x, (int)frame.y, (int)frame.width, (int)frame.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _containerViewHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setHidden:");
        objc_msgSend_void(_containerViewHandle, selector, !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _containerViewHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isHidden");
        bool hidden = objc_msgSend_bool(_containerViewHandle, selector);
        return !hidden;
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _nsStepperHandle == IntPtr.Zero || _nsTextFieldHandle == IntPtr.Zero) return;

        IntPtr selector = sel_registerName("setEnabled:");
        objc_msgSend_void(_nsStepperHandle, selector, enabled);
        objc_msgSend_void(_nsTextFieldHandle, selector, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _nsStepperHandle == IntPtr.Zero) return false;

        IntPtr selector = sel_registerName("isEnabled");
        return objc_msgSend_bool(_nsStepperHandle, selector);
    }

    public void SetBackground(RGB color)
    {
        // NSStepper and NSTextField have limited background color support
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // NSStepper and NSTextField have limited foreground color support
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

        if (_nsTextFieldHandle != IntPtr.Zero)
        {
            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsTextFieldHandle, releaseSelector);
            _nsTextFieldHandle = IntPtr.Zero;
        }

        if (_nsStepperHandle != IntPtr.Zero)
        {
            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_nsStepperHandle, releaseSelector);
            _nsStepperHandle = IntPtr.Zero;
        }

        if (_containerViewHandle != IntPtr.Zero)
        {
            IntPtr removeSelector = sel_registerName("removeFromSuperview");
            objc_msgSend(_containerViewHandle, removeSelector);

            IntPtr releaseSelector = sel_registerName("release");
            objc_msgSend(_containerViewHandle, releaseSelector);
            _containerViewHandle = IntPtr.Zero;
        }
    }

    #endregion

    #region Private Helper Methods

    private void CreateNSSpinner(IntPtr parentHandle, int style)
    {
        // Create container view to hold text field and stepper
        IntPtr viewClass = objc_getClass("NSView");
        IntPtr allocSelector = sel_registerName("alloc");
        _containerViewHandle = objc_msgSend(viewClass, allocSelector);

        IntPtr initSelector = sel_registerName("init");
        _containerViewHandle = objc_msgSend(_containerViewHandle, initSelector);

        // Set container frame
        var containerFrame = new CGRect(0, 0, 120, 24);
        IntPtr setFrameSelector = sel_registerName("setFrame:");
        objc_msgSend_rect(_containerViewHandle, setFrameSelector, containerFrame);

        // Create text field
        IntPtr textFieldClass = objc_getClass("NSTextField");
        _nsTextFieldHandle = objc_msgSend(textFieldClass, allocSelector);
        _nsTextFieldHandle = objc_msgSend(_nsTextFieldHandle, initSelector);

        var textFieldFrame = new CGRect(0, 0, 96, 24);
        objc_msgSend_rect(_nsTextFieldHandle, setFrameSelector, textFieldFrame);

        // Create stepper
        IntPtr stepperClass = objc_getClass("NSStepper");
        _nsStepperHandle = objc_msgSend(stepperClass, allocSelector);
        _nsStepperHandle = objc_msgSend(_nsStepperHandle, initSelector);

        var stepperFrame = new CGRect(96, 0, 24, 24);
        objc_msgSend_rect(_nsStepperHandle, setFrameSelector, stepperFrame);

        // Configure stepper
        IntPtr setMinValueSelector = sel_registerName("setMinValue:");
        objc_msgSend_double(_nsStepperHandle, setMinValueSelector, 0.0);

        IntPtr setMaxValueSelector = sel_registerName("setMaxValue:");
        objc_msgSend_double(_nsStepperHandle, setMaxValueSelector, 100.0);

        IntPtr setIncrementSelector = sel_registerName("setIncrement:");
        objc_msgSend_double(_nsStepperHandle, setIncrementSelector, 1.0);

        IntPtr setDoubleValueSelector = sel_registerName("setDoubleValue:");
        objc_msgSend_double(_nsStepperHandle, setDoubleValueSelector, 0.0);

        // Add text field and stepper to container
        IntPtr addSubviewSelector = sel_registerName("addSubview:");
        objc_msgSend(_containerViewHandle, addSubviewSelector, _nsTextFieldHandle);
        objc_msgSend(_containerViewHandle, addSubviewSelector, _nsStepperHandle);

        // Update text field with initial value
        UpdateTextField();

        // Add container to parent if provided
        if (parentHandle != IntPtr.Zero)
        {
            objc_msgSend(parentHandle, addSubviewSelector, _containerViewHandle);
        }
    }

    private void UpdateTextField()
    {
        if (_disposed || _nsTextFieldHandle == IntPtr.Zero) return;

        // Format value based on digits
        string formattedValue;
        if (_digits > 0)
        {
            double displayValue = _value / Math.Pow(10, _digits);
            formattedValue = displayValue.ToString($"F{_digits}");
        }
        else
        {
            formattedValue = _value.ToString();
        }

        // Create NSString and set on text field
        IntPtr nsString = CreateNSString(formattedValue);
        IntPtr selector = sel_registerName("setStringValue:");
        objc_msgSend(_nsTextFieldHandle, selector, nsString);

        // Release NSString
        IntPtr releaseSelector = sel_registerName("release");
        objc_msgSend(nsString, releaseSelector);
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

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend_fpret")]
    private static extern double objc_msgSend_double_ret(IntPtr receiver, IntPtr selector);

    #endregion
}
