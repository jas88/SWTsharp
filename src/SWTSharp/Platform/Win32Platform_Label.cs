using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Win32 Label implementation - partial class extension
/// </summary>
internal partial class Win32Platform
{
    // Label-specific constants
    private const uint SS_LEFT = 0x00000000;
    private const uint SS_CENTER = 0x00000001;
    private const uint SS_RIGHT = 0x00000002;
    private const uint SS_SIMPLE = 0x0000000B;
    private const uint SS_LEFTNOWORDWRAP = 0x0000000C;
    private const uint SS_NOTIFY = 0x00000100;
    private const uint SS_SUNKEN = 0x00001000;

    // Separator styles
    private const uint SS_ETCHEDHORZ = 0x00000010;
    private const uint SS_ETCHEDVERT = 0x00000011;
    private const uint SS_ETCHEDFRAME = 0x00000012;

    // Additional Win32 API functions needed for Label
    private const int GWL_STYLE = -16;
    private const uint SWP_FRAMECHANGED = 0x0020;

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    private static partial uint GetWindowLong(IntPtr hWnd, int nIndex);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    private static partial uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);
#else
    [DllImport(User32, SetLastError = true)]
    private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
#endif

    // Label operations
    public IntPtr CreateLabel(IntPtr parent, int style, int alignment, bool wrap)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE;

        // Check if this is a separator label
        if ((style & SWT.SEPARATOR) != 0)
        {
            // Determine separator orientation
            if ((style & SWT.HORIZONTAL) != 0)
            {
                windowStyle |= SS_ETCHEDHORZ;
            }
            else if ((style & SWT.VERTICAL) != 0)
            {
                windowStyle |= SS_ETCHEDVERT;
            }
            else
            {
                // Default to horizontal separator
                windowStyle |= SS_ETCHEDHORZ;
            }
        }
        else
        {
            // Regular text label
            // Set alignment
            if (alignment == SWT.CENTER)
            {
                windowStyle |= SS_CENTER;
            }
            else if (alignment == SWT.RIGHT)
            {
                windowStyle |= SS_RIGHT;
            }
            else // SWT.LEFT
            {
                if (wrap)
                {
                    windowStyle |= SS_LEFT;
                }
                else
                {
                    // For single-line labels without wrapping
                    windowStyle |= SS_LEFTNOWORDWRAP;
                }
            }

            // Add SS_NOTIFY to receive mouse events
            windowStyle |= SS_NOTIFY;

            // Apply shadow styles
            if ((style & SWT.SHADOW_IN) != 0)
            {
                windowStyle |= SS_SUNKEN;
            }
            else if ((style & SWT.SHADOW_OUT) != 0)
            {
                // SS_SUNKEN is the closest we have for shadow effects
                windowStyle |= SS_SUNKEN;
            }
        }

        // Add border if requested
        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        var handle = CreateWindowEx(
            0,                      // No extended style
            "STATIC",               // Window class for labels
            string.Empty,           // Initial empty text
            windowStyle,
            0, 0,                   // Position (will be set later)
            100, 20,                // Default size
            parent,
            IntPtr.Zero,            // No menu
            _hInstance,
            IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create label control. Error: {error}");
        }

        return handle;
    }

    public void SetLabelText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid label handle", nameof(handle));
        }

        SendMessage(handle, WM_SETTEXT, IntPtr.Zero, text ?? string.Empty);
    }

    public void SetLabelAlignment(IntPtr handle, int alignment)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid label handle", nameof(handle));
        }

        // Get current window style
        uint currentStyle = GetWindowLong(handle, GWL_STYLE);

        // Remove existing alignment bits
        currentStyle &= ~(SS_LEFT | SS_CENTER | SS_RIGHT | SS_LEFTNOWORDWRAP);

        // Add new alignment
        if (alignment == SWT.CENTER)
        {
            currentStyle |= SS_CENTER;
        }
        else if (alignment == SWT.RIGHT)
        {
            currentStyle |= SS_RIGHT;
        }
        else // SWT.LEFT
        {
            currentStyle |= SS_LEFT;
        }

        // Set the new style
        SetWindowLong(handle, GWL_STYLE, currentStyle);

        // Force redraw to apply new alignment
        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

        // Invalidate to trigger repaint
        InvalidateRect(handle, IntPtr.Zero, true);
    }
}
