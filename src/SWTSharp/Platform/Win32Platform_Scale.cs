using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Scale widget methods.
/// </summary>
internal partial class Win32Platform
{
    private Dictionary<IntPtr, Action<int>> _scaleCallbacks = new Dictionary<IntPtr, Action<int>>();

    public IntPtr CreateScale(IntPtr parent, int style)
    {
        // Scale is similar to Slider but without thumb size control
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

    public void SetScaleValues(IntPtr handle, int selection, int minimum, int maximum, int increment, int pageIncrement)
    {
        // Set range
        SendMessage(handle, TBM_SETRANGE, new IntPtr(1), new IntPtr((maximum << 16) | (minimum & 0xFFFF)));

        // Set position
        SendMessage(handle, TBM_SETPOS, new IntPtr(1), new IntPtr(selection));

        // Set line size
        if (increment > 0)
            SendMessage(handle, TBM_SETLINESIZE, IntPtr.Zero, new IntPtr(increment));

        // Set page size
        if (pageIncrement > 0)
            SendMessage(handle, TBM_SETPAGESIZE, IntPtr.Zero, new IntPtr(pageIncrement));
    }

    public void ConnectScaleChanged(IntPtr handle, Action<int> callback)
    {
        _scaleCallbacks[handle] = callback;
    }
}
