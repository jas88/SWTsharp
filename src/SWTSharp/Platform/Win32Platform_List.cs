using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - List widget methods.
/// </summary>
internal partial class Win32Platform
{
    // List control constants
    private const uint LBS_NOTIFY = 0x0001;
    private const uint LBS_SORT = 0x0002;
    private const uint LBS_MULTIPLESEL = 0x0008;
    private const uint LBS_EXTENDEDSEL = 0x0800;
    private const uint LBS_NOINTEGRALHEIGHT = 0x0100;
    private const uint LBS_WANTKEYBOARDINPUT = 0x0400;
    private const uint WS_VSCROLL = 0x00200000;
    private const uint WS_HSCROLL = 0x00100000;

    // List box messages
    private const uint LB_ADDSTRING = 0x0180;
    private const uint LB_INSERTSTRING = 0x0181;
    private const uint LB_DELETESTRING = 0x0182;
    private const uint LB_RESETCONTENT = 0x0184;
    private const uint LB_SETCURSEL = 0x0186;
    private const uint LB_GETCURSEL = 0x0188;
    private const uint LB_GETSELCOUNT = 0x0190;
    private const uint LB_GETSELITEMS = 0x0191;
    private const uint LB_SETTOPINDEX = 0x0197;
    private const uint LB_GETTOPINDEX = 0x018E;
    private const uint LB_GETCOUNT = 0x018B;
    private const uint LB_SELITEMRANGE = 0x019B;

    // Store list items for tracking (if needed)
    private readonly Dictionary<IntPtr, List<string>> _listItems = new Dictionary<IntPtr, List<string>>();

    // List control operations
    public IntPtr CreateList(IntPtr parentHandle, int style)
    {
        uint windowStyle = WS_CHILD | WS_VISIBLE | WS_VSCROLL | LBS_NOTIFY | LBS_NOINTEGRALHEIGHT;

        // Determine selection mode
        if ((style & SWT.MULTI) != 0)
        {
            windowStyle |= LBS_EXTENDEDSEL;
        }

        // Add border if requested
        if ((style & SWT.BORDER) != 0)
        {
            windowStyle |= 0x00800000; // WS_BORDER
        }

        var handle = CreateWindowEx(
            WS_EX_CLIENTEDGE,
            "LISTBOX",
            string.Empty,
            windowStyle,
            0, 0, 150, 200,  // Default size
            parentHandle,
            IntPtr.Zero,
            _hInstance,
            IntPtr.Zero);

        if (handle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create list control. Error: {error}");
        }

        return handle;
    }

    public void AddListItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        if (index < 0)
        {
            // Append to end
            SendMessage(handle, LB_ADDSTRING, IntPtr.Zero, item);
        }
        else
        {
            // Insert at specific position
            SendMessage(handle, LB_INSERTSTRING, new IntPtr(index), item);
        }
    }

    public void RemoveListItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, LB_DELETESTRING, new IntPtr(index), IntPtr.Zero);
    }

    public void ClearListItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, LB_RESETCONTENT, IntPtr.Zero, IntPtr.Zero);
    }

    public void SetListSelection(IntPtr handle, int[] indices)
    {
        if (handle == IntPtr.Zero)
            return;

        // First, clear all selections by setting -1
        SendMessage(handle, LB_SETCURSEL, new IntPtr(-1), IntPtr.Zero);

        if (indices == null || indices.Length == 0)
            return;

        // For single selection, just set the first index
        int count = SendMessage(handle, LB_GETCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (indices.Length == 1)
        {
            if (indices[0] >= 0 && indices[0] < count)
            {
                SendMessage(handle, LB_SETCURSEL, new IntPtr(indices[0]), IntPtr.Zero);
            }
        }
        else
        {
            // For multiple selections, use LB_SELITEMRANGE
            foreach (int index in indices)
            {
                if (index >= 0 && index < count)
                {
                    SendMessage(handle, LB_SELITEMRANGE, new IntPtr(1), new IntPtr((index << 16) | index));
                }
            }
        }
    }

    public int[] GetListSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return Array.Empty<int>();

        // Get selection count
        int selCount = SendMessage(handle, LB_GETSELCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

        if (selCount <= 0)
            return Array.Empty<int>();

        if (selCount == -1)
        {
            // Single selection mode
            int index = SendMessage(handle, LB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
            if (index >= 0)
                return new int[] { index };
            return Array.Empty<int>();
        }

        // Multiple selection mode
        int[] buffer = new int[selCount];
        unsafe
        {
            fixed (int* pBuffer = buffer)
            {
                SendMessage(handle, LB_GETSELITEMS, new IntPtr(selCount), new IntPtr(pBuffer));
            }
        }

        return buffer;
    }

    public int GetListTopIndex(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            return 0;

        return SendMessage(handle, LB_GETTOPINDEX, IntPtr.Zero, IntPtr.Zero).ToInt32();
    }

    public void SetListTopIndex(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero)
            return;

        SendMessage(handle, LB_SETTOPINDEX, new IntPtr(index), IntPtr.Zero);
    }
}
