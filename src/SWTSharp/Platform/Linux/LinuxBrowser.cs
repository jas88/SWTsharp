using System;
using System.Runtime.InteropServices;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.Linux;

/// <summary>
/// Linux implementation of browser widget using WebKitGTK.
/// Supports both WebKit 4.0 (Ubuntu 22.04) and WebKit 4.1 (Ubuntu 24.04).
///
/// Follows Eclipse SWT's WebKit disposal pattern:
///   - One WebKitWebView per Browser instance (created via webkit_web_view_new)
///   - WebKitWebView directly added to parent container (no GtkScrolledWindow)
///   - First webview instance gets a permanent extra g_object_ref (Bug 522733 workaround)
///   - Disposal: g_object_ref → gtk_container_remove → deferred g_object_unref via g_idle_add
///   - Hardware acceleration disabled via WebKit settings
/// </summary>
internal class LinuxBrowser : LinuxWidget, IPlatformBrowser
{
    private bool _disposed;
    private IntPtr _webView;
    private string _currentUrl = string.Empty;
    private string _currentTitle = string.Empty;
    private bool _isLoading;
    private Rectangle _requestedBounds;

    // Track whether the very first webview has been created (Bug 522733 workaround)
    private static bool s_firstInstanceCreated;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LoadChangedCallback(IntPtr webView, int loadEvent, IntPtr userData);

    // prevent GC collection of the delegate
    private readonly LoadChangedCallback _loadChangedCallback;
    private ulong _loadChangedHandlerId;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool IdleCallback(IntPtr userData);

    // prevent GC collection during deferred unref
    private static readonly List<GCHandle> s_idleCallbackHandles = new();

#if NET5_0_OR_GREATER
    private static IntPtr _webkitLibrary;
    private static bool _webkitInitialized;
    private static bool _webkitAvailable;
    private static string? _loadedLibraryName;

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
    private delegate IntPtr WebkitWebViewGetSettingsDelegate(IntPtr web_view);

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
    private static WebkitWebViewGetSettingsDelegate? _webkit_web_view_get_settings;
#endif

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
                    // Settings are optional — not fatal if missing
                    TryLoadDelegate(_webkitLibrary, "webkit_web_view_get_settings", out _webkit_web_view_get_settings);

                    _webkitAvailable = true;
                    _loadedLibraryName = libName;
                    return;
                }
            }
        }
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
        _loadChangedCallback = OnLoadChanged;

#if NET5_0_OR_GREATER
        EnsureWebKitInitialized();

        if (!_webkitAvailable || _webkit_web_view_new == null)
            throw new InvalidOperationException(
                "WebKitGTK is not available. Install libwebkit2gtk-4.1-0 or libwebkit2gtk-4.0-37.");

        _webView = _webkit_web_view_new();
#else
        _webView = webkit_web_view_new();
#endif

        if (_webView == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create WebKitWebView.");

        // Bug 522733 workaround: permanently ref the first WebKitWebView instance.
        // WebKitGTK >= 2.18 auto-destroys web process on last webview unref;
        // keeping the first one alive prevents a crash on process shutdown.
        if (!s_firstInstanceCreated)
        {
            s_firstInstanceCreated = true;
            g_object_ref(_webView);
        }

        // Disable hardware acceleration (WebKit bug 239429 workaround, matches Java SWT)
        DisableHardwareAcceleration();

        // Connect load-changed signal
        _loadChangedHandlerId = g_signal_connect_data(
            _webView,
            "load-changed",
            Marshal.GetFunctionPointerForDelegate(_loadChangedCallback),
            IntPtr.Zero,
            IntPtr.Zero,
            0);

        // Add directly to parent container — no GtkScrolledWindow needed
        if (parentHandle != IntPtr.Zero)
        {
            gtk_container_add(parentHandle, _webView);
        }

        gtk_widget_show(_webView);

        // Pump GTK events so WebKitGTK can complete async initialization
        // (D-Bus setup, web process spawn). Without this, subsequent
        // webkit_web_view_new() calls may block waiting for events that
        // never get dispatched.
        PumpGtkEvents();
    }

    /// <summary>
    /// Drains pending GTK events so WebKitGTK's internal async operations
    /// (D-Bus, web process IPC) can complete without blocking.
    /// </summary>
    private static void PumpGtkEvents()
    {
        while (gtk_events_pending())
        {
            gtk_main_iteration_do(false);
        }
    }

    private void DisableHardwareAcceleration()
    {
        if (_webView == IntPtr.Zero) return;

#if NET5_0_OR_GREATER
        // Disable WebGL via GObject property on the WebKit settings object.
        // This matches Java SWT's approach of disabling hardware acceleration.
        IntPtr settings = _webkit_web_view_get_settings?.Invoke(_webView) ?? IntPtr.Zero;
        if (settings != IntPtr.Zero)
        {
            g_object_set_bool(settings, "enable-webgl", false, IntPtr.Zero);
        }
#endif
    }

    public override IntPtr GetNativeHandle()
    {
        return _webView;
    }

    private enum WebKitLoadEvent
    {
        Started = 0,
        Redirected = 1,
        Committed = 2,
        Finished = 3
    }

    private void OnLoadChanged(IntPtr webView, int loadEvent, IntPtr userData)
    {
        if (_disposed) return;
        HandleLoadChanged((WebKitLoadEvent)loadEvent);
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
        if (_disposed || _webView == IntPtr.Zero) return;
        _requestedBounds = new Rectangle(x, y, width, height);
        gtk_widget_set_size_request(_webView, width, height);
    }

    public Rectangle GetBounds()
    {
        if (_disposed) return default;
        return _requestedBounds;
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _webView == IntPtr.Zero) return;
        if (visible) gtk_widget_show(_webView);
        else gtk_widget_hide(_webView);
    }

    public bool GetVisible()
    {
        if (_disposed || _webView == IntPtr.Zero) return false;
        return gtk_widget_get_visible(_webView);
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

    /// <summary>
    /// Disposes using Eclipse SWT's WebKitGTK >= 2.18 pattern:
    ///   1. Disconnect signal handlers
    ///   2. g_object_ref to prevent premature destruction
    ///   3. gtk_container_remove to unparent (parent destruction won't touch us)
    ///   4. Deferred g_object_unref via g_idle_add (avoids deadlock inside WebKit callbacks)
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_webView == IntPtr.Zero) return;

        // Disconnect signal handler first
        if (_loadChangedHandlerId != 0)
        {
            g_signal_handler_disconnect(_webView, _loadChangedHandlerId);
            _loadChangedHandlerId = 0;
        }

        // Stop any in-progress loading
#if NET5_0_OR_GREATER
        _webkit_web_view_stop_loading?.Invoke(_webView);
#else
        webkit_web_view_stop_loading(_webView);
#endif

        // Eclipse SWT pattern: ref → unparent → deferred unref
        // The extra ref prevents GTK from destroying the widget when we remove it from
        // its parent. We then schedule g_object_unref via g_idle_add so it happens
        // after all pending GTK/WebKit callbacks have completed, avoiding deadlocks.
        IntPtr webViewToUnref = _webView;
        g_object_ref(webViewToUnref);

        IntPtr parent = gtk_widget_get_parent(webViewToUnref);
        if (parent != IntPtr.Zero)
        {
            gtk_container_remove(parent, webViewToUnref);
        }

        // Schedule deferred unref — prevent delegate from being GC'd
        GCHandle gcHandle = default;
        IdleCallback idleCallback = (_) =>
        {
            g_object_unref(webViewToUnref);
            lock (s_idleCallbackHandles) { s_idleCallbackHandles.Remove(gcHandle); }
            gcHandle.Free();
            return false; // G_SOURCE_REMOVE — run once
        };
        gcHandle = GCHandle.Alloc(idleCallback);
        lock (s_idleCallbackHandles) { s_idleCallbackHandles.Add(gcHandle); }

        g_idle_add(Marshal.GetFunctionPointerForDelegate(idleCallback), IntPtr.Zero);

        _webView = IntPtr.Zero;

        // Pump events so the deferred g_object_unref runs now, before the
        // caller destroys the parent window (which would otherwise cascade
        // into the still-parented webview).
        PumpGtkEvents();
    }

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
    private static extern void gtk_container_add(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void gtk_container_remove(IntPtr container, IntPtr widget);

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr gtk_widget_get_parent(IntPtr widget);

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
    private static extern bool gtk_events_pending();

    [DllImport("libgtk-3.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool gtk_main_iteration_do(bool blocking);

    // GLib

    [DllImport("libglib-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint g_idle_add(IntPtr function, IntPtr data);

    // GObject

    [DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr g_object_ref(IntPtr @object);

    [DllImport("libgobject-2.0.so.0", CallingConvention = CallingConvention.Cdecl)]
    private static extern void g_object_unref(IntPtr @object);

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

    [DllImport("libgobject-2.0.so.0", EntryPoint = "g_object_set", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern void g_object_set_bool(IntPtr @object, string first_property_name, bool value, IntPtr sentinel);

}
