using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Combo Box widget methods.
/// </summary>
internal partial class Win32Platform
{
    // Combo control constants
    private const uint CBS_SIMPLE = 0x0001;
    private const uint CBS_DROPDOWN = 0x0002;
    private const uint CBS_DROPDOWNLIST = 0x0003;
    private const uint CBS_AUTOHSCROLL = 0x0040;
    private const uint CBS_SORT = 0x0100;
    private const uint CBS_HASSTRINGS = 0x0200;
    private const uint CBS_NOINTEGRALHEIGHT = 0x0400;

    // Combo Box Messages
    private const uint CB_ADDSTRING = 0x0143;
    private const uint CB_DELETESTRING = 0x0144;
    private const uint CB_DIR = 0x0145;
    private const uint CB_GETCOUNT = 0x0146;
    private const uint CB_GETCURSEL = 0x0147;
    private const uint CB_GETLBTEXT = 0x0148;
    private const uint CB_GETLBTEXTLEN = 0x0149;
    private const uint CB_INSERTSTRING = 0x014A;
    private const uint CB_RESETCONTENT = 0x014B;
    private const uint CB_FINDSTRING = 0x014C;
    private const uint CB_SELECTSTRING = 0x014D;
    private const uint CB_SETCURSEL = 0x014E;
    private const uint CB_SHOWDROPDOWN = 0x014F;
    private const uint CB_GETITEMDATA = 0x0150;
    private const uint CB_SETITEMDATA = 0x0151;
    private const uint CB_GETDROPPEDCONTROLRECT = 0x0152;
    private const uint CB_SETITEMHEIGHT = 0x0153;
    private const uint CB_GETITEMHEIGHT = 0x0154;
    private const uint CB_SETEXTENDEDUI = 0x0155;
    private const uint CB_GETEXTENDEDUI = 0x0156;
    private const uint CB_GETDROPPEDSTATE = 0x0157;
    private const uint CB_FINDSTRINGEXACT = 0x0158;
    private const uint CB_SETLOCALE = 0x0159;
    private const uint CB_GETLOCALE = 0x015A;
    private const uint CB_GETTOPINDEX = 0x015B;
    private const uint CB_SETTOPINDEX = 0x015C;
    private const uint CB_GETHORIZONTALEXTENT = 0x015D;
    private const uint CB_SETHORIZONTALEXTENT = 0x015E;
    private const uint CB_GETDROPPEDWIDTH = 0x015F;
    private const uint CB_SETDROPPEDWIDTH = 0x0160;
    private const uint CB_INITSTORAGE = 0x0161;
    private const uint CB_LIMITTEXT = 0x0141;
    private const uint CB_SETEDITSEL = 0x0142;
    private const uint CB_GETEDITSEL = 0x0140;

    // Combo control operations - partial methods implementation
    public IntPtr CreateCombo(IntPtr parentHandle, int style)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE | CBS_HASSTRINGS | CBS_AUTOHSCROLL;
        uint exStyle = 0;

        // Determine combo box style
        if ((style & SWT.READ_ONLY) != 0)
        {
            // Read-only: drop-down list (no text editing)
            windowStyle |= CBS_DROPDOWNLIST;
        }
        else if ((style & SWT.SIMPLE) != 0)
        {
            // Simple: always-visible list
            windowStyle |= CBS_SIMPLE;
        }
        else
        {
            // Default: editable drop-down
            windowStyle |= CBS_DROPDOWN;
        }

        // Border
        if ((style & SWT.BORDER) != 0)
        {
            exStyle |= WS_EX_CLIENTEDGE;
        }

        var handle = CreateWindowEx(
            exStyle,
            "COMBOBOX",       // Windows ComboBox control class
            string.Empty,
            windowStyle,
            0, 0, 120, 200,   // Default size (height includes drop-down)
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create combo control. Error: {error}");
        }

        return handle;
    }

    public void SetComboText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero) return;
        SetWindowText(handle, text ?? string.Empty);
    }

    public string GetComboText(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return string.Empty;

        int length = (int)SendMessage(handle, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
        if (length == 0) return string.Empty;

        var buffer = new char[length + 1];
        unsafe
        {
            fixed (char* pBuffer = buffer)
            {
                SendMessage(handle, WM_GETTEXT, (IntPtr)(length + 1), (IntPtr)pBuffer);
            }
        }
        return new string(buffer, 0, length);
    }

    public void AddComboItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero) return;

        if (index < 0)
        {
            // Append to end
            SendMessage(handle, CB_ADDSTRING, IntPtr.Zero, item ?? string.Empty);
        }
        else
        {
            // Insert at specific index
            SendMessage(handle, CB_INSERTSTRING, (IntPtr)index, item ?? string.Empty);
        }
    }

    public void RemoveComboItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || index < 0) return;
        SendMessage(handle, CB_DELETESTRING, (IntPtr)index, IntPtr.Zero);
    }

    public void ClearComboItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;
        SendMessage(handle, CB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetComboSelection(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero) return;
        SendMessage(handle, CB_SETCURSEL, (IntPtr)index, IntPtr.Zero);
    }

    public int GetComboSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return -1;
        return (int)SendMessage(handle, CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetComboTextLimit(IntPtr handle, int limit)
    {
        if (handle == IntPtr.Zero) return;
        SendMessage(handle, CB_LIMITTEXT, (IntPtr)limit, IntPtr.Zero);
    }

    public void SetComboVisibleItemCount(IntPtr handle, int count)
    {
        if (handle == IntPtr.Zero || count < 1) return;

        // On Win32, we can set the drop-down height
        // Get current item height
        int itemHeight = (int)SendMessage(handle, CB_GETITEMHEIGHT, IntPtr.Zero, IntPtr.Zero);
        if (itemHeight > 0)
        {
            // Set the dropped width to accommodate the items
            int dropHeight = itemHeight * count + 2; // +2 for border
            SendMessage(handle, CB_SETDROPPEDWIDTH, (IntPtr)dropHeight, IntPtr.Zero);
        }
    }

    public void SetComboTextSelection(IntPtr handle, int start, int end)
    {
        if (handle == IntPtr.Zero) return;

        // Pack start and end into lParam: LOWORD=start, HIWORD=end
        int sel = (end << 16) | (start & 0xFFFF);
        SendMessage(handle, CB_SETEDITSEL, IntPtr.Zero, (IntPtr)sel);
    }

    public (int Start, int End) GetComboTextSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return (0, 0);

        uint result = (uint)SendMessage(handle, CB_GETEDITSEL, IntPtr.Zero, IntPtr.Zero);
        int start = (int)(result & 0xFFFF);
        int end = (int)((result >> 16) & 0xFFFF);

        return (start, end);
    }

    public void ComboTextCopy(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // WM_COPY is a standard Windows message for clipboard copy
        const uint WM_COPY = 0x0301;
        SendMessage(handle, WM_COPY, IntPtr.Zero, IntPtr.Zero);
    }

    public void ComboTextCut(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // WM_CUT is a standard Windows message for clipboard cut
        const uint WM_CUT = 0x0300;
        SendMessage(handle, WM_CUT, IntPtr.Zero, IntPtr.Zero);
    }

    public void ComboTextPaste(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // WM_PASTE is a standard Windows message for clipboard paste
        const uint WM_PASTE = 0x0302;
        SendMessage(handle, WM_PASTE, IntPtr.Zero, IntPtr.Zero);
    }
}
