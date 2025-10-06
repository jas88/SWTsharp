using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Spinner widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Spinner widget constants
    private const int UDM_SETRANGE = 0x0465; // WM_USER + 101
    private const int UDM_SETPOS = 0x0467; // WM_USER + 103
    private const int UDM_SETBUDDY = 0x0469; // WM_USER + 105
    private const int UDS_ALIGNRIGHT = 0x0004;
    private const int UDS_SETBUDDYINT = 0x0002;
    private const int UDS_ARROWKEYS = 0x0020;
    private const int UDS_WRAP = 0x0001;
    private const int ES_NUMBER = 0x2000;
    private const int ES_READONLY = 0x0800;
    private const int EM_SETLIMITTEXT = 0x00C5;

    private class SpinnerData
    {
        public IntPtr EditHandle;
        public IntPtr UpDownHandle;
        public int Digits;
    }

    private Dictionary<IntPtr, SpinnerData> _spinners = new Dictionary<IntPtr, SpinnerData>();
    private Dictionary<IntPtr, Action<int>> _spinnerCallbacks = new Dictionary<IntPtr, Action<int>>();
    private Dictionary<IntPtr, Action> _spinnerModifiedCallbacks = new Dictionary<IntPtr, Action>();

    public IntPtr CreateSpinner(IntPtr parent, int style)
    {
        // Create edit control for text entry
        uint editStyle = WS_CHILD | WS_VISIBLE | WS_BORDER | ES_NUMBER;

        if ((style & SWT.READ_ONLY) != 0)
            editStyle |= ES_READONLY;

        IntPtr editHandle = CreateWindowEx(
            0,
            "EDIT",
            "0",
            editStyle,
            0, 0, 80, 25,
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (editHandle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Create up-down control
        uint upDownStyle = WS_CHILD | WS_VISIBLE | UDS_ALIGNRIGHT | UDS_SETBUDDYINT | UDS_ARROWKEYS;

        if ((style & SWT.WRAP) != 0)
            upDownStyle |= UDS_WRAP;

        IntPtr upDownHandle = CreateWindowEx(
            0,
            "msctls_updown32",
            "",
            upDownStyle,
            0, 0, 0, 0,
            parent,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero
        );

        if (upDownHandle == IntPtr.Zero)
        {
            DestroyWindow(editHandle);
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        // Set edit as buddy
        SendMessage(upDownHandle, UDM_SETBUDDY, editHandle, IntPtr.Zero);

        // Store spinner data
        _spinners[upDownHandle] = new SpinnerData
        {
            EditHandle = editHandle,
            UpDownHandle = upDownHandle,
            Digits = 0
        };

        return upDownHandle;
    }

    public void SetSpinnerValues(IntPtr handle, int selection, int minimum, int maximum, int digits, int increment, int pageIncrement)
    {
        if (!_spinners.TryGetValue(handle, out var data))
            return;

        // Set range (LOWORD = max, HIWORD = min for up-down control)
        SendMessage(handle, UDM_SETRANGE, IntPtr.Zero, new IntPtr((minimum << 16) | (maximum & 0xFFFF)));

        // Set position
        SendMessage(handle, UDM_SETPOS, IntPtr.Zero, new IntPtr(selection));

        // Store digits for later use
        data.Digits = digits;
    }

    public void SetSpinnerTextLimit(IntPtr handle, int limit)
    {
        if (!_spinners.TryGetValue(handle, out var data))
            return;

        SendMessage(data.EditHandle, EM_SETLIMITTEXT, new IntPtr(limit), IntPtr.Zero);
    }

    public void ConnectSpinnerChanged(IntPtr handle, Action<int> callback)
    {
        _spinnerCallbacks[handle] = callback;
    }

    public void ConnectSpinnerModified(IntPtr handle, Action callback)
    {
        _spinnerModifiedCallbacks[handle] = callback;
    }
}
