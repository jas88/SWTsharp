using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Group widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Group widget constants
    private const int BS_GROUPBOX = 0x00000007;

    public IntPtr CreateGroup(IntPtr parent, int style, string text)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE | BS_GROUPBOX;

        IntPtr handle = CreateWindowEx(
            0,
            "BUTTON",
            text,
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

    public void SetGroupText(IntPtr handle, string text)
    {
        IntPtr textPtr = Marshal.StringToHGlobalUni(text);
        try
        {
            SendMessage(handle, WM_SETTEXT, IntPtr.Zero, textPtr);
        }
        finally
        {
            Marshal.FreeHGlobal(textPtr);
        }
    }
}
