using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Win32;

/// <summary>
/// Win32 implementation of IPlatformCoolBar using Windows REBARWINDOW32 control.
/// </summary>
internal class Win32CoolBar : IPlatformCoolBar
{
    private IntPtr _hwnd;
    private readonly int _style;
    private bool _disposed;
    private readonly List<Win32CoolItem> _items = new();
    private bool _locked = false;

    // Win32 constants
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CLIPCHILDREN = 0x02000000;
    private const uint WS_CLIPSIBLINGS = 0x04000000;
    private const uint CCS_NODIVIDER = 0x0040;
    private const uint CCS_NOPARENTALIGN = 0x0008;
    private const uint CCS_VERT = 0x0080;
    private const uint RBS_BANDBORDERS = 0x0400;
    private const uint RBS_FIXEDORDER = 0x0800;
    private const uint RBS_VARHEIGHT = 0x0200;

    private const uint WM_USER = 0x0400;
    private const uint RB_INSERTBAND = WM_USER + 1;
    private const uint RB_DELETEBAND = WM_USER + 2;
    private const uint RB_GETBANDCOUNT = WM_USER + 12;
    private const uint RB_SETBANDINFO = WM_USER + 6;
    private const uint RB_GETBANDINFO = WM_USER + 5;

    [StructLayout(LayoutKind.Sequential)]
    internal struct REBARBANDINFO
    {
        public uint cbSize;
        public uint fMask;
        public uint fStyle;
        public uint clrFore;
        public uint clrBack;
        public IntPtr lpText;
        public uint cch;
        public int iImage;
        public IntPtr hwndChild;
        public uint cxMinChild;
        public uint cyMinChild;
        public uint cx;
        public IntPtr hbmBack;
        public uint wID;
        public uint cyChild;
        public uint cyMaxChild;
        public uint cyIntegral;
        public uint cxIdeal;
        public IntPtr lParam;
        public uint cxHeader;
    }

    public Win32CoolBar(IntPtr parentHandle, int style)
    {
        _style = style;

        uint coolBarStyle = WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS |
                           CCS_NODIVIDER | CCS_NOPARENTALIGN | RBS_BANDBORDERS | RBS_VARHEIGHT;

        // Check for vertical orientation
        if ((style & SWT.VERTICAL) != 0)
        {
            coolBarStyle |= CCS_VERT;
        }

        _hwnd = CreateWindowEx(
            0,
            "ReBarWindow32",
            "",
            coolBarStyle,
            0, 0, 100, 24,
            parentHandle,
            IntPtr.Zero,
            GetModuleHandle(null),
            IntPtr.Zero
        );

        if (_hwnd == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(error,
                $"Failed to create Win32 rebar (coolbar). Error code: {error}");
        }
    }

    internal IntPtr GetNativeHandle()
    {
        return _hwnd;
    }

    public IPlatformCoolItem CreateItem(int index, int style)
    {
        if (_hwnd == IntPtr.Zero)
            throw new ObjectDisposedException(nameof(Win32CoolBar));

        var item = new Win32CoolItem(this, index, style);

        if (index < 0 || index >= _items.Count)
        {
            _items.Add(item);
        }
        else
        {
            _items.Insert(index, item);
        }

        return item;
    }

    public void RemoveItem(int index)
    {
        if (_hwnd == IntPtr.Zero) return;
        if (index < 0 || index >= _items.Count) return;

        SendMessage(_hwnd, RB_DELETEBAND, new IntPtr(index), IntPtr.Zero);
        _items[index].Dispose();
        _items.RemoveAt(index);
    }

    public int GetItemCount()
    {
        if (_hwnd == IntPtr.Zero) return 0;
        return (int)SendMessage(_hwnd, RB_GETBANDCOUNT, IntPtr.Zero, IntPtr.Zero);
    }

    public bool GetLocked()
    {
        return _locked;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;
        // On Win32, we can implement this by adding RBS_FIXEDORDER style
        // For simplicity, we'll just track the state for now
    }

    internal void InsertBand(int index, REBARBANDINFO bandInfo)
    {
        SendMessage(_hwnd, RB_INSERTBAND, new IntPtr(index), ref bandInfo);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var item in _items.ToArray())
        {
            item.Dispose();
        }
        _items.Clear();

        if (_hwnd != IntPtr.Zero)
        {
            DestroyWindow(_hwnd);
            _hwnd = IntPtr.Zero;
        }
    }

    #region P/Invoke

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string lpClassName,
        string lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref REBARBANDINFO lParam);

    #endregion

    /// <summary>
    /// Win32 implementation of IPlatformCoolItem.
    /// </summary>
    internal class Win32CoolItem : IPlatformCoolItem
    {
        private readonly Win32CoolBar _parent;
        private readonly int _index;
        private readonly int _style;
        private IntPtr _childWindow;
        private int _preferredWidth = 100;
        private int _preferredHeight = 24;
        private int _minimumWidth = 0;
        private int _minimumHeight = 0;
        private bool _disposed;

        public Win32CoolItem(Win32CoolBar parent, int index, int style)
        {
            _parent = parent;
            _index = index < 0 ? parent.GetItemCount() : index;
            _style = style;

            // Create band
            var bandInfo = new REBARBANDINFO
            {
                cbSize = (uint)Marshal.SizeOf<REBARBANDINFO>(),
                fMask = 0x00000001 | 0x00000100, // RBBIM_STYLE | RBBIM_CHILD
                fStyle = 0x0001, // RBBS_BREAK if needed
                hwndChild = IntPtr.Zero,
                cxMinChild = (uint)_minimumWidth,
                cyMinChild = (uint)_minimumHeight,
                cx = (uint)_preferredWidth
            };

            _parent.InsertBand(_index, bandInfo);
        }

        public void SetControl(IPlatformWidget? control)
        {
            // Extract native handle using reflection
            if (control != null)
            {
                var getNativeHandleMethod = control.GetType().GetMethod("GetNativeHandle",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (getNativeHandleMethod != null)
                {
                    _childWindow = (IntPtr)(getNativeHandleMethod.Invoke(control, null) ?? IntPtr.Zero);
                }
                else
                {
                    _childWindow = IntPtr.Zero;
                }
            }
            else
            {
                _childWindow = IntPtr.Zero;
            }

            // Update band info
            var bandInfo = new REBARBANDINFO
            {
                cbSize = (uint)Marshal.SizeOf<REBARBANDINFO>(),
                fMask = 0x00000100, // RBBIM_CHILD
                hwndChild = _childWindow
            };

            SendMessage(_parent.GetNativeHandle(), 0x0400 + 6, new IntPtr(_index), ref bandInfo);
        }

        public Rectangle GetBounds()
        {
            // Would need to implement RB_GETRECT to get actual bounds
            return new Rectangle(0, 0, _preferredWidth, _preferredHeight);
        }

        public void SetPreferredSize(int width, int height)
        {
            _preferredWidth = width;
            _preferredHeight = height;

            var bandInfo = new REBARBANDINFO
            {
                cbSize = (uint)Marshal.SizeOf<REBARBANDINFO>(),
                fMask = 0x00000010, // RBBIM_SIZE
                cx = (uint)width
            };

            SendMessage(_parent.GetNativeHandle(), 0x0400 + 6, new IntPtr(_index), ref bandInfo);
        }

        public void SetMinimumSize(int width, int height)
        {
            _minimumWidth = width;
            _minimumHeight = height;

            var bandInfo = new REBARBANDINFO
            {
                cbSize = (uint)Marshal.SizeOf<REBARBANDINFO>(),
                fMask = 0x00000020, // RBBIM_CHILDSIZE
                cxMinChild = (uint)width,
                cyMinChild = (uint)height
            };

            SendMessage(_parent.GetNativeHandle(), 0x0400 + 6, new IntPtr(_index), ref bandInfo);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _childWindow = IntPtr.Zero;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, ref REBARBANDINFO lParam);
    }
}
