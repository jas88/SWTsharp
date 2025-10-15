using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Spinner widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Spinner data structure
    private sealed class SpinnerData
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

  // REMOVED METHODS (moved to ISpinnerWidget interface):
    // - CreateSpinner(IntPtr parent, int style)
    // - SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    // - SetSpinnerTextLimit(IntPtr handle, int limit)
    // - ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    // - ConnectSpinnerModified(IntPtr handle, Action callback)
    // These methods are now implemented via the ISpinnerWidget interface using proper handles
}
