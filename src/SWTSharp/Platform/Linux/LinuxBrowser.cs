using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux implementation of browser widget using WebKitGTK.
/// </summary>
internal class LinuxBrowser : IPlatformBrowser
{
    private IntPtr _webView; // WebKitWebView
    private IntPtr _scrolledWindow; // GtkScrolledWindow container
    private bool _disposed;
    private string _currentUrl = string.Empty;
    private string _currentTitle = string.Empty;
    private bool _isLoading;

    // Event handling
    public event EventHandler<BrowserNavigatedEventArgs>? Navigated;
    public event EventHandler<BrowserNavigationErrorEventArgs>? NavigationError;
    public event EventHandler<BrowserNavigatingEventArgs>? Navigating;
    public event EventHandler<BrowserDocumentCompleteEventArgs>? DocumentComplete;
    public event EventHandler<BrowserTitleChangedEventArgs>? TitleChanged;
    public event EventHandler<BrowserProgressEventArgs>? ProgressChanged;
    public event EventHandler<BrowserStatusTextChangedEventArgs>? StatusTextChanged;
    public event EventHandler<BrowserNewWindowEventArgs>? NewWindow;
    public event EventHandler<BrowserProcessTerminatedEventArgs>? ProcessTerminated;

#pragma warning disable CS0067 // Event is never used
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

    public LinuxBrowser(IntPtr parentHandle, int style)
    {
        // Create WebKitWebView
        _webView = webkit_web_view_new();

        if (_webView == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create WebKitWebView. WebKitGTK may not be installed.");
        }

        // Create scrolled window container
        _scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        gtk_scrolled_window_set_policy(_scrolledWindow, 1, 1); // GTK_POLICY_AUTOMATIC

        // Add web view to scrolled window
        gtk_container_add(_scrolledWindow, _webView);

        // Add to parent if specified
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _scrolledWindow);
        }

        gtk_widget_show(_webView);
        gtk_widget_show(_scrolledWindow);

        // Note: Event signal connections removed per instructions
        // Events like load-changed, load-failed, etc. would be connected here
        // using g_signal_connect_data if needed in the future
    }

    public bool Navigate(string url)
    {
        if (_disposed || _webView == IntPtr.Zero || string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            // Marshal string to UTF-8 pointer
            IntPtr urlPtr = MarshalStringToUTF8(url);
            webkit_web_view_load_uri(_webView, urlPtr);
            Marshal.FreeHGlobal(urlPtr);

            _currentUrl = url;
            _isLoading = true;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SetText(string html, string? baseUrl = null)
    {
        if (_disposed || _webView == IntPtr.Zero || html == null)
            return false;

        try
        {
            IntPtr htmlPtr = MarshalStringToUTF8(html);
            IntPtr baseUrlPtr = string.IsNullOrEmpty(baseUrl)
                ? IntPtr.Zero
                : MarshalStringToUTF8(baseUrl!);

            webkit_web_view_load_html(_webView, htmlPtr, baseUrlPtr);

            Marshal.FreeHGlobal(htmlPtr);
            if (baseUrlPtr != IntPtr.Zero)
                Marshal.FreeHGlobal(baseUrlPtr);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GetUrl()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return _currentUrl;

        try
        {
            IntPtr urlPtr = webkit_web_view_get_uri(_webView);
            if (urlPtr != IntPtr.Zero)
            {
                _currentUrl = PtrToStringUTF8(urlPtr);
            }
        }
        catch
        {
            // Return cached URL on error
        }

        return _currentUrl;
    }

    public string GetTitle()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return _currentTitle;

        try
        {
            IntPtr titlePtr = webkit_web_view_get_title(_webView);
            if (titlePtr != IntPtr.Zero)
            {
                _currentTitle = PtrToStringUTF8(titlePtr);
            }
        }
        catch
        {
            // Return cached title on error
        }

        return _currentTitle;
    }

    public bool GoBack()
    {
        if (_disposed || _webView == IntPtr.Zero || !CanGoBack)
            return false;

        webkit_web_view_go_back(_webView);
        return true;
    }

    public bool GoForward()
    {
        if (_disposed || _webView == IntPtr.Zero || !CanGoForward)
            return false;

        webkit_web_view_go_forward(_webView);
        return true;
    }

    public void Refresh()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        webkit_web_view_reload(_webView);
    }

    public void Stop()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        webkit_web_view_stop_loading(_webView);
        _isLoading = false;
    }

    public bool CanGoBack
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

            return webkit_web_view_can_go_back(_webView);
        }
    }

    public bool CanGoForward
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

            return webkit_web_view_can_go_forward(_webView);
        }
    }

    public bool IsLoading => _isLoading;

    public string ExecuteScript(string script)
    {
        // Synchronous stub - WebKitGTK JavaScript execution requires callbacks
        return string.Empty;
    }

    public async Task<string?> ExecuteScriptAsync(string script)
    {
        // WebKitGTK JavaScript execution would require async callback setup
        // Return null for now - full implementation requires g_signal_connect callbacks
        await Task.CompletedTask;
        return null;
    }

    public bool JavaScriptEnabled { get; set; }

    public void SetUserAgent(string userAgent)
    {
        // Stub - WebKitGTK user agent setting requires WebKitSettings API
    }

    public string GetUserAgent()
    {
        // Stub - WebKitGTK user agent retrieval requires WebKitSettings API
        return string.Empty;
    }

    public void ClearCookies()
    {
        // Stub - WebKitGTK cookie clearing requires WebKitWebContext API
    }

    public void ClearCache()
    {
        // Stub - WebKitGTK cache clearing requires WebKitWebContext API
    }

    public Task InitializeAsync()
    {
        // WebKitGTK doesn't require async initialization like WebView2
        return Task.CompletedTask;
    }

    public bool IsInitialized => true;

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return;

        gtk_widget_set_size_request(_scrolledWindow, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return default;

        GtkAllocation allocation;
        gtk_widget_get_allocation(_scrolledWindow, out allocation);

        return new Rectangle(allocation.x, allocation.y, allocation.width, allocation.height);
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return;

        if (visible)
            gtk_widget_show(_scrolledWindow);
        else
            gtk_widget_hide(_scrolledWindow);
    }

    public bool GetVisible()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return false;

        return gtk_widget_get_visible(_scrolledWindow);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        gtk_widget_set_sensitive(_webView, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return false;

        return gtk_widget_get_sensitive(_webView);
    }

    public void SetBackground(RGB color)
    {
        // GTK3 background colors are typically set via CSS
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // GTK3 foreground colors are typically set via CSS
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_scrolledWindow != IntPtr.Zero)
            {
                gtk_widget_destroy(_scrolledWindow);
                _scrolledWindow = IntPtr.Zero;
            }

            _webView = IntPtr.Zero;
            _disposed = true;
        }
    }

    // Helper methods for UTF-8 string marshalling

    private static string PtrToStringUTF8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

#if NETSTANDARD2_0
        // Manual UTF-8 decoding for netstandard2.0
        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0)
            length++;

        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
#else
        // Use built-in method for .NET Core 2.1+
        return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
#endif
    }

    private static IntPtr MarshalStringToUTF8(string str)
    {
        if (string.IsNullOrEmpty(str))
            return IntPtr.Zero;

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str + '\0');
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }

    // WebKitGTK P/Invoke declarations

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr webkit_web_view_new();

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern void webkit_web_view_load_uri(IntPtr web_view, IntPtr uri);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern void webkit_web_view_load_html(IntPtr web_view, IntPtr content, IntPtr base_uri);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr webkit_web_view_get_uri(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr webkit_web_view_get_title(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern void webkit_web_view_go_back(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern void webkit_web_view_go_forward(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool webkit_web_view_can_go_back(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool webkit_web_view_can_go_forward(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern void webkit_web_view_reload(IntPtr web_view);

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern void webkit_web_view_stop_loading(IntPtr web_view);

    // GTK P/Invoke declarations

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_scrolled_window_new(IntPtr hadjustment, IntPtr vadjustment);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_scrolled_window_set_policy(IntPtr scrolled_window, int hscrollbar_policy, int vscrollbar_policy);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_show(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_hide(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_visible(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_sensitive(IntPtr widget, bool sensitive);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_widget_get_sensitive(IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_set_size_request(IntPtr widget, int width, int height);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_get_allocation(IntPtr widget, out GtkAllocation allocation);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_widget_destroy(IntPtr widget);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
