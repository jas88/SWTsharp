using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Slider widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Slider widget constants
    private const int TBM_GETPOS = 0x0400; // WM_USER
    private const int TBM_SETPOS = 0x0405; // WM_USER + 5
    private const int TBM_SETRANGE = 0x0406; // WM_USER + 6
    private const int TBM_SETPAGESIZE = 0x0415; // WM_USER + 21
    private const int TBM_SETLINESIZE = 0x0417; // WM_USER + 23
    private const int TBM_SETTHUMBLENGTH = 0x041B; // WM_USER + 27
    private const int TBS_AUTOTICKS = 0x0001;
    private const int TBS_VERT = 0x0002;

    private Dictionary<IntPtr, Action<int>> _sliderCallbacks = new Dictionary<IntPtr, Action<int>>();

    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE | TBS_AUTOTICKS;

        if ((style & SWT.VERTICAL) != 0)
            windowStyle |= TBS_VERT;

        IntPtr handle = CreateWindowEx(
            0,
            "msctls_trackbar32",
            "",
            windowStyle,
            0, 0, 100, 100,
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return handle;
    }

    public void SetSliderValues(IntPtr handle, int selection, int minimum, int maximum, int thumb, int increment, int pageIncrement)
    {
        // Set range (LOWORD = min, HIWORD = max)
        SendMessage(handle, TBM_SETRANGE, new IntPtr(1), new IntPtr((maximum << 16) | (minimum & 0xFFFF)));

        // Set position
        SendMessage(handle, TBM_SETPOS, new IntPtr(1), new IntPtr(selection));

        // Set thumb length
        if (thumb > 0)
            SendMessage(handle, TBM_SETTHUMBLENGTH, new IntPtr(thumb), IntPtr.Zero);

        // Set line size (increment)
        if (increment > 0)
            SendMessage(handle, TBM_SETLINESIZE, IntPtr.Zero, new IntPtr(increment));

        // Set page size
        if (pageIncrement > 0)
            SendMessage(handle, TBM_SETPAGESIZE, IntPtr.Zero, new IntPtr(pageIncrement));
    }

    public void ConnectSliderChanged(IntPtr handle, Action<int> callback)
    {
        _sliderCallbacks[handle] = callback;
    }
}
