using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux implementation of browser widget using WebKitGTK.
/// Supports both WebKit 4.0 (Ubuntu 22.04) and WebKit 4.1 (Ubuntu 24.04).
/// </summary>
internal class LinuxBrowser : IPlatformBrowser
{
    private IntPtr _webView; // WebKitWebView
    private IntPtr _scrolledWindow; // GtkScrolledWindow container
    private bool _disposed;
    private string _currentUrl = string.Empty;
    private string _currentTitle = string.Empty;
    private bool _isLoading;
    private Rectangle _requestedBounds;

    // Static mapping of webview handles to instances for callback routing
    private static readonly ConcurrentDictionary<IntPtr, LinuxBrowser> _browserInstances = new();

    // GSignal callback delegate for load-changed
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LoadChangedCallback(IntPtr webView, int loadEvent, IntPtr userData);

    // Keep delegate alive to prevent GC
    private static readonly LoadChangedCallback _loadChangedCallback = OnLoadChanged;

#if NET5_0_OR_GREATER
    // WebKit library handle for dynamic loading (.NET 5+)
    private static IntPtr _webkitLibrary;
    private static bool _webkitInitialized;
    private static bool _webkitAvailable;
    private static string? _loadedLibraryName;

    // Delegate types for WebKit functions
    private delegate IntPtr WebkitWebViewNewDelegate();
    private delegate void WebkitWebViewLoadUriDelegate(IntPtr web_view, IntPtr uri);
    private delegate void WebkitWebViewLoadHtmlDelegate(IntPtr web_view, IntPtr content, IntPtr base_uri);
    private delegate IntPtr WebkitWebViewGetUriDelegate(IntPtr web_view);
    private delegate IntPtr WebkitWebViewGetTitleDelegate(IntPtr web_view);
    private delegate void WebkitWebViewGoBackDelegate(IntPtr web_view);
    private delegate void WebkitWebViewGoForwardDelegate(IntPtr web_view);
    private delegate bool WebkitWebViewCanGoBackDelegate(IntPtr web_view);
    private delegate bool WebkitWebViewCanGoForwardDelegate(IntPtr web_view);
    private delegate void WebkitWebViewReloadDelegate(IntPtr web_view);
    private delegate void WebkitWebViewStopLoadingDelegate(IntPtr web_view);

    // Function delegates for WebKit (loaded dynamically)
    private static WebkitWebViewNewDelegate? _webkit_web_view_new;
    private static WebkitWebViewLoadUriDelegate? _webkit_web_view_load_uri;
    private static WebkitWebViewLoadHtmlDelegate? _webkit_web_view_load_html;
    private static WebkitWebViewGetUriDelegate? _webkit_web_view_get_uri;
    private static WebkitWebViewGetTitleDelegate? _webkit_web_view_get_title;
    private static WebkitWebViewGoBackDelegate? _webkit_web_view_go_back;
    private static WebkitWebViewGoForwardDelegate? _webkit_web_view_go_forward;
    private static WebkitWebViewCanGoBackDelegate? _webkit_web_view_can_go_back;
    private static WebkitWebViewCanGoForwardDelegate? _webkit_web_view_can_go_forward;
    private static WebkitWebViewReloadDelegate? _webkit_web_view_reload;
    private static WebkitWebViewStopLoadingDelegate? _webkit_web_view_stop_loading;
#endif

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

#if NET5_0_OR_GREATER
    /// <summary>
    /// Initializes the WebKit library by trying WebKit 4.1 first (Ubuntu 24.04), then 4.0.
    /// </summary>
    private static void EnsureWebKitInitialized()
    {
        if (_webkitInitialized)
            return;

        _webkitInitialized = true;
        _webkitAvailable = false;

        // Try WebKit 4.1 first (Ubuntu 24.04+), then fall back to 4.0
        string[] webkitLibraries = new[]
        {
            "libwebkit2gtk-4.1.so.0",  // Ubuntu 24.04+
            "libwebkit2gtk-4.0.so.37", // Ubuntu 22.04 and older
        };

        foreach (var libName in webkitLibraries)
        {
            if (NativeLibrary.TryLoad(libName, out _webkitLibrary))
            {
                // Load all function pointers
                if (TryLoadDelegate(_webkitLibrary, "webkit_web_view_new", out _webkit_web_view_new) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_load_uri", out _webkit_web_view_load_uri) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_load_html", out _webkit_web_view_load_html) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_get_uri", out _webkit_web_view_get_uri) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_get_title", out _webkit_web_view_get_title) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_go_back", out _webkit_web_view_go_back) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_go_forward", out _webkit_web_view_go_forward) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_can_go_back", out _webkit_web_view_can_go_back) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_can_go_forward", out _webkit_web_view_can_go_forward) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_reload", out _webkit_web_view_reload) &&
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_stop_loading", out _webkit_web_view_stop_loading))
                {
                    _webkitAvailable = true;
                    _loadedLibraryName = libName;
                    Console.WriteLine($"[LinuxBrowser] Successfully loaded {libName}");
                    return;
                }
            }
        }

        Console.WriteLine("[LinuxBrowser] WebKitGTK not available - browser widget will not function");
    }

    private static bool TryLoadDelegate<T>(IntPtr library, string name, out T? del) where T : Delegate
    {
        del = null;
        if (!NativeLibrary.TryGetExport(library, name, out var ptr))
            return false;
        del = Marshal.GetDelegateForFunctionPointer<T>(ptr);
        return del != null;
    }
#endif

    public LinuxBrowser(IntPtr parentHandle, int style)
    {
#if NET5_0_OR_GREATER
        EnsureWebKitInitialized();

        if (!_webkitAvailable || _webkit_web_view_new == null)
        {
            throw new InvalidOperationException("WebKitGTK is not available. Please install libwebkit2gtk-4.1-0 or libwebkit2gtk-4.0-37.");
        }

        // Create WebKitWebView using dynamic delegate
        _webView = _webkit_web_view_new();
#else
        // netstandard2.0: Use static P/Invoke (assumes WebKit 4.0)
        _webView = webkit_web_view_new();
#endif

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

        // Register instance for callback routing
        _browserInstances[_webView] = this;

        // Connect to load-changed signal for navigation events
        g_signal_connect_data(
            _webView,
            "load-changed",
            Marshal.GetFunctionPointerForDelegate(_loadChangedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0);
    }

    /// <summary>
    /// WebKit load event enumeration.
    /// </summary>
    private enum WebKitLoadEvent
    {
        Started = 0,
        Redirected = 1,
        Committed = 2,
        Finished = 3
    }

    /// <summary>
    /// Static callback for WebKit load-changed signal.
    /// </summary>
    private static void OnLoadChanged(IntPtr webView, int loadEvent, IntPtr userData)
    {
        if (!_browserInstances.TryGetValue(webView, out var browser) || browser._disposed)
            return;

        browser.HandleLoadChanged((WebKitLoadEvent)loadEvent);
    }

    /// <summary>
    /// Instance method to handle load state changes.
    /// </summary>
    private void HandleLoadChanged(WebKitLoadEvent loadEvent)
    {
        switch (loadEvent)
        {
            case WebKitLoadEvent.Started:
                _isLoading = true;
                Navigating?.Invoke(this, new BrowserNavigatingEventArgs { Url = _currentUrl });
                break;

            case WebKitLoadEvent.Committed:
                // Navigation committed - update URL from WebKit
                UpdateCurrentUrlFromWebKit();
                Navigated?.Invoke(this, new BrowserNavigatedEventArgs { Url = _currentUrl });
                break;

            case WebKitLoadEvent.Finished:
                _isLoading = false;
                UpdateCurrentUrlFromWebKit();
                DocumentComplete?.Invoke(this, new BrowserDocumentCompleteEventArgs { Url = _currentUrl });
                break;
        }
    }

    /// <summary>
    /// Updates _currentUrl from WebKit's actual URI.
    /// </summary>
    private void UpdateCurrentUrlFromWebKit()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

#if NET5_0_OR_GREATER
        if (!_webkitAvailable || _webkit_web_view_get_uri == null)
            return;

        IntPtr urlPtr = _webkit_web_view_get_uri(_webView);
#else
        IntPtr urlPtr = webkit_web_view_get_uri(_webView);
#endif
        if (urlPtr != IntPtr.Zero)
        {
            _currentUrl = PtrToStringUTF8(urlPtr);
        }
    }

    public bool Navigate(string url)
    {
        if (_disposed || _webView == IntPtr.Zero || string.IsNullOrWhiteSpace(url))
            return false;

#if NET5_0_OR_GREATER
        if (!_webkitAvailable || _webkit_web_view_load_uri == null)
            return false;
#endif

        try
        {
            // Marshal string to UTF-8 pointer
            IntPtr urlPtr = MarshalStringToUTF8(url);
#if NET5_0_OR_GREATER
            _webkit_web_view_load_uri(_webView, urlPtr);
#else
            webkit_web_view_load_uri(_webView, urlPtr);
#endif
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
        if (_disposed || _webView == IntPtr.Zero)
            return false;

        // WebKit requires non-null content - treat null/empty as blank page
        html = string.IsNullOrEmpty(html) ? "<html><body></body></html>" : html;

#if NET5_0_OR_GREATER
        if (!_webkitAvailable || _webkit_web_view_load_html == null)
            return false;
#endif

        try
        {
            IntPtr htmlPtr = MarshalStringToUTF8(html);
            IntPtr baseUrlPtr = string.IsNullOrEmpty(baseUrl)
                ? IntPtr.Zero
                : MarshalStringToUTF8(baseUrl!);

#if NET5_0_OR_GREATER
            _webkit_web_view_load_html(_webView, htmlPtr, baseUrlPtr);
#else
            webkit_web_view_load_html(_webView, htmlPtr, baseUrlPtr);
#endif

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
        // Return cached URL - this matches the URL that was set via Navigate()
        // rather than the browser's potentially normalized URL (which may have trailing slash)
        return _currentUrl;
    }

    public string GetTitle()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return _currentTitle;

#if NET5_0_OR_GREATER
        if (!_webkitAvailable || _webkit_web_view_get_title == null)
            return _currentTitle;
#endif

        try
        {
#if NET5_0_OR_GREATER
            IntPtr titlePtr = _webkit_web_view_get_title(_webView);
#else
            IntPtr titlePtr = webkit_web_view_get_title(_webView);
#endif
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

#if NET5_0_OR_GREATER
        if (_webkit_web_view_go_back == null)
            return false;
        _webkit_web_view_go_back(_webView);
#else
        webkit_web_view_go_back(_webView);
#endif
        return true;
    }

    public bool GoForward()
    {
        if (_disposed || _webView == IntPtr.Zero || !CanGoForward)
            return false;

#if NET5_0_OR_GREATER
        if (_webkit_web_view_go_forward == null)
            return false;
        _webkit_web_view_go_forward(_webView);
#else
        webkit_web_view_go_forward(_webView);
#endif
        return true;
    }

    public void Refresh()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

#if NET5_0_OR_GREATER
        _webkit_web_view_reload?.Invoke(_webView);
#else
        webkit_web_view_reload(_webView);
#endif
    }

    public void Stop()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

#if NET5_0_OR_GREATER
        _webkit_web_view_stop_loading?.Invoke(_webView);
#else
        webkit_web_view_stop_loading(_webView);
#endif
        _isLoading = false;
    }

    public bool CanGoBack
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

#if NET5_0_OR_GREATER
            if (!_webkitAvailable || _webkit_web_view_can_go_back == null)
                return false;
            return _webkit_web_view_can_go_back(_webView);
#else
            return webkit_web_view_can_go_back(_webView);
#endif
        }
    }

    public bool CanGoForward
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

#if NET5_0_OR_GREATER
            if (!_webkitAvailable || _webkit_web_view_can_go_forward == null)
                return false;
            return _webkit_web_view_can_go_forward(_webView);
#else
            return webkit_web_view_can_go_forward(_webView);
#endif
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

        _requestedBounds = new Rectangle(x, y, width, height);
        gtk_widget_set_size_request(_scrolledWindow, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero)
            return default;

        // Return requested bounds since GTK allocation may not reflect size request
        // until widget is realized and laid out
        return _requestedBounds;
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
            // Remove from instance mapping
            if (_webView != IntPtr.Zero)
            {
                _browserInstances.TryRemove(_webView, out _);
            }

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

#if !NET5_0_OR_GREATER
    // WebKitGTK P/Invoke declarations for netstandard2.0 (only WebKit 4.0)
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
#endif

    // GTK P/Invoke declarations (used by all target frameworks)

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

    // GObject signal connection
    [DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern ulong g_signal_connect_data(
        IntPtr instance,
        string detailed_signal,
        IntPtr c_handler,
        IntPtr data,
        IntPtr destroy_data,
        int connect_flags);

    [StructLayout(LayoutKind.Sequential)]
    private struct GtkAllocation
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }
}
