using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Spinner widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Spinner data structure
    private class SpinnerData
    {
        public IntPtr TextField { get; set; }
        public IntPtr Stepper { get; set; }
        public int Digits { get; set; }
    }

    // Spinner widget selectors and data
    private readonly Dictionary<IntPtr, SpinnerData> _spinners = new Dictionary<IntPtr, SpinnerData>();
    private readonly Dictionary<IntPtr, Action<int>> _spinnerCallbacks = new Dictionary<IntPtr, Action<int>>();
    private readonly Dictionary<IntPtr, Action> _spinnerModifiedCallbacks = new Dictionary<IntPtr, Action>();

    private IntPtr _nsStepperClass;
    private IntPtr _selSetMinimum;
    private IntPtr _selSetMaximum;
    private IntPtr _selSetIncrement;
    private IntPtr _selSetValueWraps;
    private IntPtr _selSetIntegerValue;
    private IntPtr _selIntegerValue;

    // objc_msgSend variants for Spinner
    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool arg1);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_int(IntPtr receiver, IntPtr selector);

    [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_int(IntPtr receiver, IntPtr selector, long arg1);

    private void InitializeSpinnerSelectors()
    {
        if (_nsStepperClass == IntPtr.Zero)
        {
            _nsStepperClass = objc_getClass("NSStepper");
            _selSetMinimum = sel_registerName("setMinValue:");
            _selSetMaximum = sel_registerName("setMaxValue:");
            _selSetIncrement = sel_registerName("setIncrement:");
            _selSetValueWraps = sel_registerName("setValueWraps:");
            _selSetIntegerValue = sel_registerName("setIntegerValue:");
            _selIntegerValue = sel_registerName("integerValue");
        }

        // Ensure text field selectors are initialized
        InitializeTextSelectors();
    }

    public IntPtr CreateSpinner(IntPtr parent, int style)
    {
        InitializeSpinnerSelectors();

        // Create a container view to hold both text field and stepper
        IntPtr containerView = objc_msgSend(_nsViewClass ?? objc_getClass("NSView"), _selAlloc);
        containerView = objc_msgSend(containerView, _selInit);

        // Create NSTextField for displaying/editing value
        IntPtr textField = objc_msgSend(_nsTextFieldClass, _selAlloc);
        textField = objc_msgSend(textField, _selInit);

        // Create NSStepper for increment/decrement buttons
        IntPtr stepper = objc_msgSend(_nsStepperClass, _selAlloc);
        stepper = objc_msgSend(stepper, _selInit);

        // Set default frame for container
        var containerFrame = new CGRect(0, 0, 120, 24);
        objc_msgSend_rect(containerView, _selSetFrame, containerFrame);

        // Position text field (left side)
        var textFrame = new CGRect(0, 0, 80, 24);
        objc_msgSend_rect(textField, _selSetFrame, textFrame);

        // Position stepper (right side, next to text field)
        var stepperFrame = new CGRect(82, 0, 19, 24);
        objc_msgSend_rect(stepper, _selSetFrame, stepperFrame);

        // Set default stepper values
        objc_msgSend_double(stepper, _selSetMinimum, 0.0);
        objc_msgSend_double(stepper, _selSetMaximum, 100.0);
        objc_msgSend_double(stepper, _selSetIncrement, 1.0);
        objc_msgSend_bool(stepper, _selSetValueWraps, false);
        objc_msgSend_int(stepper, _selSetIntegerValue, 0);

        // Set default text field value
        IntPtr selSetStringValue = sel_registerName("setStringValue:");
        IntPtr nsZero = CreateNSString("0");
        objc_msgSend(textField, selSetStringValue, nsZero);

        // Add text field and stepper to container
        objc_msgSend(containerView, _selAddSubview, textField);
        objc_msgSend(containerView, _selAddSubview, stepper);

        // Add container to parent if provided
        if (parent != IntPtr.Zero)
        {
            IntPtr selContentView = sel_registerName("contentView");
            IntPtr contentView = objc_msgSend(parent, selContentView);
            objc_msgSend(contentView, _selAddSubview, containerView);
        }

        // Store spinner data
        _spinners[containerView] = new SpinnerData
        {
            TextField = textField,
            Stepper = stepper,
            Digits = 0
        };

        return containerView;
    }

    public void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        if (handle == IntPtr.Zero || !_spinners.ContainsKey(handle))
            return;

        InitializeSpinnerSelectors();

        var data = _spinners[handle];

        // Update spinner data
        data.Digits = digits;

        // Set stepper values
        objc_msgSend_double(data.Stepper, _selSetMinimum, (double)minimum);
        objc_msgSend_double(data.Stepper, _selSetMaximum, (double)maximum);
        objc_msgSend_double(data.Stepper, _selSetIncrement, (double)increment);
        objc_msgSend_int(data.Stepper, _selSetIntegerValue, selection);

        // Update text field with formatted value
        string formattedValue = digits > 0
            ? (selection / Math.Pow(10, digits)).ToString($"F{digits}")
            : selection.ToString();

        IntPtr selSetStringValue = sel_registerName("setStringValue:");
        IntPtr nsValue = CreateNSString(formattedValue);
        objc_msgSend(data.TextField, selSetStringValue, nsValue);
    }

    public void SetSpinnerTextLimit(IntPtr handle, int limit)
    {
        if (handle == IntPtr.Zero || !_spinners.ContainsKey(handle))
            return;

        // NSTextField doesn't have a direct text limit property
        // This would need to be implemented via a formatter or delegate
        // For now, this is a no-op on macOS
    }

    public void ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    {
        if (handle == IntPtr.Zero)
            return;

        _spinnerCallbacks[handle] = callback;

        // Note: In a full implementation, you would set up target-action pattern here
        // This requires creating a custom Objective-C class to handle callbacks
        // For now, this stores the callback for future implementation
    }

    public void ConnectSpinnerModified(IntPtr handle, Action callback)
    {
        if (handle == IntPtr.Zero)
            return;

        _spinnerModifiedCallbacks[handle] = callback;

        // Note: In a full implementation, you would set up target-action pattern here
        // This requires creating a custom Objective-C class to handle callbacks
        // For now, this stores the callback for future implementation
    }
}
