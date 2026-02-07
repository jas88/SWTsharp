using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux implementation of browser widget using WebKitGTK.
/// Supports both WebKit 4.0 (Ubuntu 22.04) and WebKit 4.1 (Ubuntu 24.04).
///
/// Uses WEBKIT_PROCESS_MODEL_MULTIPLE_SECONDARY_PROCESSES with a hidden
/// "parent" WebKitWebView created at init time. All real browser instances
/// are created via webkit_web_view_new_with_related_view(parent), sharing
/// the same web process as the parent view.
/// </summary>
internal class LinuxBrowser : IPlatformBrowser
{
    private IntPtr _webView;
    private IntPtr _scrolledWindow;
    private ulong _loadChangedHandlerId;
    private bool _disposed;
    private string _currentUrl = string.Empty;
    private string _currentTitle = string.Empty;
    private bool _isLoading;
    private Rectangle _requestedBounds;

    // Hidden parent webview — created once, never shown, lives for the process.
    // All real webviews are related views of this parent.
    private static IntPtr s_parentWebView;
    private static bool s_initialized;

    // Maps webview handles to instances for signal callback routing
    private static readonly ConcurrentDictionary<IntPtr, LinuxBrowser> _browserInstances = new();

    // GSignal callback delegate for load-changed
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LoadChangedCallback(IntPtr webView, int loadEvent, IntPtr userData);

    private static readonly LoadChangedCallback _loadChangedCallback = OnLoadChanged;

#if NET5_0_OR_GREATER
    private static IntPtr _webkitLibrary;
    private static bool _webkitInitialized;
    private static bool _webkitAvailable;
    private static string? _loadedLibraryName;

    // Delegate types for WebKit functions
    private delegate IntPtr WebkitWebViewNewDelegate();
    private delegate IntPtr WebkitWebViewNewWithRelatedViewDelegate(IntPtr web_view);
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
    private delegate IntPtr WebkitWebContextGetDefaultDelegate();
    private delegate void WebkitWebContextSetProcessModelDelegate(IntPtr context, int model);

    private static WebkitWebViewNewDelegate? _webkit_web_view_new;
    private static WebkitWebViewNewWithRelatedViewDelegate? _webkit_web_view_new_with_related_view;
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
    private static WebkitWebContextGetDefaultDelegate? _webkit_web_context_get_default;
    private static WebkitWebContextSetProcessModelDelegate? _webkit_web_context_set_process_model;
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

#pragma warning disable CS0067
    public event EventHandler<int>? Click;
    public event EventHandler<int>? FocusGained;
    public event EventHandler<int>? FocusLost;
    public event EventHandler<PlatformKeyEventArgs>? KeyDown;
    public event EventHandler<PlatformKeyEventArgs>? KeyUp;
#pragma warning restore CS0067

#if NET5_0_OR_GREATER
    private static void EnsureWebKitInitialized()
    {
        if (_webkitInitialized)
            return;

        _webkitInitialized = true;
        _webkitAvailable = false;

        string[] webkitLibraries = new[]
        {
            "libwebkit2gtk-4.1.so.0",
            "libwebkit2gtk-4.0.so.37",
        };

        foreach (var libName in webkitLibraries)
        {
            if (NativeLibrary.TryLoad(libName, out _webkitLibrary))
            {
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

                    // Load related-view constructor (optional — falls back to webkit_web_view_new)
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_new_with_related_view",
                        out _webkit_web_view_new_with_related_view);

                    // Use multiple secondary processes so related views actually share a process
                    TryLoadDelegate(_webkitLibrary, "webkit_web_context_get_default", out _webkit_web_context_get_default);
                    TryLoadDelegate(_webkitLibrary, "webkit_web_context_set_process_model", out _webkit_web_context_set_process_model);
                    if (_webkit_web_context_get_default != null && _webkit_web_context_set_process_model != null)
                    {
                        var ctx = _webkit_web_context_get_default();
                        if (ctx != IntPtr.Zero)
                        {
                            // WEBKIT_PROCESS_MODEL_MULTIPLE_SECONDARY_PROCESSES = 0
                            _webkit_web_context_set_process_model(ctx, 0);
                        }
                    }

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

    /// <summary>
    /// Checks whether a pointer is a valid GtkWidget using the GTK type system.
    /// Equivalent to GTK_IS_WIDGET(ptr).
    /// </summary>
    private static bool IsValidGtkWidget(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return false;
        try
        {
            IntPtr widgetType = gtk_widget_get_type();
            return g_type_check_instance_is_a(ptr, widgetType);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates the hidden parent webview. Called once; all real views are related to this.
    /// </summary>
    private static void EnsureParentWebView()
    {
        if (s_initialized) return;
        s_initialized = true;

#if NET5_0_OR_GREATER
        EnsureWebKitInitialized();

        if (!_webkitAvailable || _webkit_web_view_new == null)
            throw new InvalidOperationException(
                "WebKitGTK is not available. Install libwebkit2gtk-4.1-0 or libwebkit2gtk-4.0-37.");

        s_parentWebView = _webkit_web_view_new();
#else
        s_parentWebView = webkit_web_view_new();
#endif

        bool parentIsWidget = IsValidGtkWidget(s_parentWebView);
        Console.Error.WriteLine($"[LinuxBrowser] Parent webview: 0x{s_parentWebView:X}, IsValidGtkWidget={parentIsWidget}");

        if (s_parentWebView == IntPtr.Zero || !parentIsWidget)
        {
            s_parentWebView = IntPtr.Zero;
            throw new InvalidOperationException(
                "webkit_web_view_new() returned a non-GtkWidget handle. WebKitGTK may require a running display.");
        }

        // Hold an extra ref — this view must never be destroyed.
        g_object_ref(s_parentWebView);
    }

    /// <summary>
    /// Creates a new WebKitWebView related to the hidden parent view.
    /// Returns IntPtr.Zero if the parent is unavailable.
    /// </summary>
    private static IntPtr CreateRelatedWebView()
    {
        if (s_parentWebView == IntPtr.Zero)
            return IntPtr.Zero;

#if NET5_0_OR_GREATER
        if (_webkit_web_view_new_with_related_view != null)
            return _webkit_web_view_new_with_related_view(s_parentWebView);

        // Fallback if related-view API unavailable
        return _webkit_web_view_new!();
#else
        return webkit_web_view_new_with_related_view(s_parentWebView);
#endif
    }

    public LinuxBrowser(IntPtr parentHandle, int style)
    {
        EnsureParentWebView();

        _webView = CreateRelatedWebView();
        bool relatedIsWidget = IsValidGtkWidget(_webView);
        Console.Error.WriteLine($"[LinuxBrowser] Related webview: 0x{_webView:X}, IsValidGtkWidget={relatedIsWidget}");

        if (_webView == IntPtr.Zero || !relatedIsWidget)
        {
            _webView = IntPtr.Zero;
            throw new InvalidOperationException(
                "webkit_web_view_new_with_related_view() returned a non-GtkWidget handle.");
        }

        // Create scrolled window container
        _scrolledWindow = gtk_scrolled_window_new(IntPtr.Zero, IntPtr.Zero);
        if (_scrolledWindow == IntPtr.Zero)
        {
            gtk_widget_destroy(_webView);
            _webView = IntPtr.Zero;
            throw new InvalidOperationException("Failed to create GTK ScrolledWindow for browser");
        }
        gtk_scrolled_window_set_policy(_scrolledWindow, 1, 1); // GTK_POLICY_AUTOMATIC

        gtk_container_add(_scrolledWindow, _webView);

        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _scrolledWindow);
        }

        gtk_widget_show(_webView);
        gtk_widget_show(_scrolledWindow);

        _browserInstances[_webView] = this;

        _loadChangedHandlerId = g_signal_connect_data(
            _webView,
            "load-changed",
            Marshal.GetFunctionPointerForDelegate(_loadChangedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0);
    }

    private enum WebKitLoadEvent
    {
        Started = 0,
        Redirected = 1,
        Committed = 2,
        Finished = 3
    }

    private static void OnLoadChanged(IntPtr webView, int loadEvent, IntPtr userData)
    {
        if (!_browserInstances.TryGetValue(webView, out var browser) || browser._disposed)
            return;

        browser.HandleLoadChanged((WebKitLoadEvent)loadEvent);
    }

    private void HandleLoadChanged(WebKitLoadEvent loadEvent)
    {
        switch (loadEvent)
        {
            case WebKitLoadEvent.Started:
                _isLoading = true;
                Navigating?.Invoke(this, new BrowserNavigatingEventArgs { Url = _currentUrl });
                break;

            case WebKitLoadEvent.Committed:
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
        }

        return _currentTitle;
    }

    public bool GoBack()
    {
        if (_disposed || _webView == IntPtr.Zero || !CanGoBack)
            return false;

#if NET5_0_OR_GREATER
        if (_webkit_web_view_go_back == null) return false;
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
        if (_webkit_web_view_go_forward == null) return false;
        _webkit_web_view_go_forward(_webView);
#else
        webkit_web_view_go_forward(_webView);
#endif
        return true;
    }

    public void Refresh()
    {
        if (_disposed || _webView == IntPtr.Zero) return;

#if NET5_0_OR_GREATER
        _webkit_web_view_reload?.Invoke(_webView);
#else
        webkit_web_view_reload(_webView);
#endif
    }

    public void Stop()
    {
        if (_disposed || _webView == IntPtr.Zero) return;

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
            if (_disposed || _webView == IntPtr.Zero) return false;
#if NET5_0_OR_GREATER
            if (!_webkitAvailable || _webkit_web_view_can_go_back == null) return false;
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
            if (_disposed || _webView == IntPtr.Zero) return false;
#if NET5_0_OR_GREATER
            if (!_webkitAvailable || _webkit_web_view_can_go_forward == null) return false;
            return _webkit_web_view_can_go_forward(_webView);
#else
            return webkit_web_view_can_go_forward(_webView);
#endif
        }
    }

    public bool IsLoading => _isLoading;

    public string ExecuteScript(string script) => string.Empty;

    public async Task<string?> ExecuteScriptAsync(string script)
    {
        await Task.CompletedTask;
        return null;
    }

    public bool JavaScriptEnabled { get; set; }
    public void SetUserAgent(string userAgent) { }
    public string GetUserAgent() => string.Empty;
    public void ClearCookies() { }
    public void ClearCache() { }
    public Task InitializeAsync() => Task.CompletedTask;
    public bool IsInitialized => true;

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return;
        _requestedBounds = new Rectangle(x, y, width, height);
        gtk_widget_set_size_request(_scrolledWindow, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return default;
        return _requestedBounds;
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return;
        if (visible) gtk_widget_show(_scrolledWindow);
        else gtk_widget_hide(_scrolledWindow);
    }

    public bool GetVisible()
    {
        if (_disposed || _scrolledWindow == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_scrolledWindow);
    }

    public void SetEnabled(bool enabled)
    {
        if (_disposed || _webView == IntPtr.Zero) return;
        gtk_widget_set_sensitive(_webView, enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _webView == IntPtr.Zero) return false;
        return gtk_widget_get_sensitive(_webView);
    }

    public void SetBackground(RGB color) { }
    public RGB GetBackground() => new RGB(255, 255, 255);
    public void SetForeground(RGB color) { }
    public RGB GetForeground() => new RGB(0, 0, 0);

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;

            if (_webView != IntPtr.Zero && _loadChangedHandlerId > 0)
            {
                g_signal_handler_disconnect(_webView, _loadChangedHandlerId);
                _loadChangedHandlerId = 0;
            }

            if (_webView != IntPtr.Zero)
            {
                _browserInstances.TryRemove(_webView, out _);
            }

            if (_webView != IntPtr.Zero)
            {
#if NET5_0_OR_GREATER
                _webkit_web_view_stop_loading?.Invoke(_webView);
#else
                webkit_web_view_stop_loading(_webView);
#endif
            }

            // Null the handles. Do NOT call gtk_widget_destroy or gtk_container_remove.
            // The parent GtkWindow's destruction recursively cleans up all descendants.
            _scrolledWindow = IntPtr.Zero;
            _webView = IntPtr.Zero;
        }
    }

    // UTF-8 string marshalling helpers

    private static string PtrToStringUTF8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return string.Empty;
#if NETSTANDARD2_0
        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0) length++;
        if (length == 0) return string.Empty;
        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
#else
        return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
#endif
    }

    private static IntPtr MarshalStringToUTF8(string str)
    {
        if (string.IsNullOrEmpty(str)) return IntPtr.Zero;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str + '\0');
        IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, ptr, bytes.Length);
        return ptr;
    }

#if !NET5_0_OR_GREATER
    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr webkit_web_view_new();

    [DllImport("libwebkit2gtk-4.0.so.37", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr webkit_web_view_new_with_related_view(IntPtr web_view);

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

    // GTK P/Invoke

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
    private static extern void gtk_widget_destroy(IntPtr widget);

    // GObject

    [DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_object_ref(IntPtr @object);

    [DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool g_type_check_instance_is_a(IntPtr instance, IntPtr iface_type);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_type();

    [DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_signal_handler_disconnect(IntPtr instance, ulong handler_id);

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
