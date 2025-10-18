using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SWTSharp.Graphics;

namespace SWTSharp.Platform.MacOS;

/// <summary>
/// macOS implementation of browser widget using WKWebView.
/// </summary>
internal class MacOSBrowser : MacOSWidget, IPlatformBrowser
{
    private IntPtr _webView;
    private bool _disposed;
    private string _currentUrl = string.Empty;
    private string _currentTitle = string.Empty;
    private bool _isLoading;

    // Events
#pragma warning disable CS0067 // Event is never used
    public event EventHandler<BrowserNavigatingEventArgs>? Navigating;
    public event EventHandler<BrowserNavigatedEventArgs>? Navigated;
    public event EventHandler<BrowserNavigationErrorEventArgs>? NavigationError;
    public event EventHandler<BrowserDocumentCompleteEventArgs>? DocumentComplete;
    public event EventHandler<BrowserTitleChangedEventArgs>? TitleChanged;
    public event EventHandler<BrowserProgressEventArgs>? ProgressChanged;
    public event EventHandler<BrowserStatusTextChangedEventArgs>? StatusTextChanged;
    public event EventHandler<BrowserNewWindowEventArgs>? NewWindow;
    public event EventHandler<BrowserProcessTerminatedEventArgs>? ProcessTerminated;
#pragma warning restore CS0067

    public MacOSBrowser(IntPtr parentHandle, int style)
    {
        _webView = CreateWKWebView(style);

        if (parentHandle != IntPtr.Zero)
        {
            objc_msgSend(parentHandle, sel_registerName("addSubview:"), _webView);
        }
    }

    public bool Navigate(string url)
    {
        if (_disposed || _webView == IntPtr.Zero || string.IsNullOrEmpty(url))
            return false;

        try
        {
            // Create NSURL from string
            IntPtr nsString = CreateNSString(url);
            IntPtr nsUrlClass = objc_getClass("NSURL");
            IntPtr nsUrl = objc_msgSend(nsUrlClass, sel_registerName("URLWithString:"), nsString);
            ReleaseNSString(nsString);

            if (nsUrl == IntPtr.Zero)
                return false;

            // Create NSURLRequest
            IntPtr requestClass = objc_getClass("NSURLRequest");
            IntPtr request = objc_msgSend(objc_msgSend(requestClass, sel_registerName("alloc")),
                sel_registerName("initWithURL:"), nsUrl);

            if (request == IntPtr.Zero)
                return false;

            // Load request
            objc_msgSend(_webView, sel_registerName("loadRequest:"), request);

            _currentUrl = url;
            _isLoading = true;

            // Release request
            objc_msgSend(request, sel_registerName("release"));

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SetText(string html, string? baseUrl = null)
    {
        if (_disposed || _webView == IntPtr.Zero || string.IsNullOrEmpty(html))
            return false;

        try
        {
            IntPtr htmlString = CreateNSString(html);
            IntPtr baseNSUrl = IntPtr.Zero;

            if (!string.IsNullOrEmpty(baseUrl))
            {
                IntPtr baseUrlString = CreateNSString(baseUrl!);
                IntPtr nsUrlClass = objc_getClass("NSURL");
                baseNSUrl = objc_msgSend(nsUrlClass, sel_registerName("URLWithString:"), baseUrlString);
                ReleaseNSString(baseUrlString);
            }

            objc_msgSend_2args(_webView, sel_registerName("loadHTMLString:baseURL:"), htmlString, baseNSUrl);
            ReleaseNSString(htmlString);

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
            return string.Empty;

        try
        {
            IntPtr url = objc_msgSend(_webView, sel_registerName("URL"));
            if (url == IntPtr.Zero)
                return string.Empty;

            IntPtr absoluteString = objc_msgSend(url, sel_registerName("absoluteString"));
            return NSStringToString(absoluteString);
        }
        catch
        {
            return string.Empty;
        }
    }

    public string GetTitle()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return string.Empty;

        try
        {
            IntPtr title = objc_msgSend(_webView, sel_registerName("title"));
            return NSStringToString(title);
        }
        catch
        {
            return string.Empty;
        }
    }

    public bool GoBack()
    {
        if (_disposed || _webView == IntPtr.Zero || !CanGoBack)
            return false;

        objc_msgSend(_webView, sel_registerName("goBack"));
        return true;
    }

    public bool GoForward()
    {
        if (_disposed || _webView == IntPtr.Zero || !CanGoForward)
            return false;

        objc_msgSend(_webView, sel_registerName("goForward"));
        return true;
    }

    public void Refresh()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        objc_msgSend(_webView, sel_registerName("reload"));
    }

    public void Stop()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        objc_msgSend(_webView, sel_registerName("stopLoading"));
        _isLoading = false;
    }

    public bool CanGoBack
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

            return objc_msgSend_bool(_webView, sel_registerName("canGoBack"));
        }
    }

    public bool CanGoForward
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

            return objc_msgSend_bool(_webView, sel_registerName("canGoForward"));
        }
    }

    public bool IsLoading => _isLoading;

    public async Task<string?> ExecuteScriptAsync(string script)
    {
        if (_disposed || _webView == IntPtr.Zero || string.IsNullOrEmpty(script))
            return null;

        var tcs = new TaskCompletionSource<string?>();

        try
        {
            IntPtr scriptString = CreateNSString(script);

            // WKWebView evaluateJavaScript:completionHandler: requires a block
            // For simplicity, we'll use synchronous execution for now
            // Full async implementation would require block creation

            ReleaseNSString(scriptString);

            // Return empty result for now - full implementation requires Objective-C blocks
            tcs.SetResult(null);
        }
        catch (Exception ex)
        {
            tcs.SetException(ex);
        }

        return await tcs.Task;
    }

    public string? ExecuteScript(string script)
    {
        // Synchronous wrapper - blocks on async operation
        return ExecuteScriptAsync(script).GetAwaiter().GetResult();
    }

    public bool JavaScriptEnabled
    {
        get
        {
            if (_disposed || _webView == IntPtr.Zero)
                return false;

            // Get WKPreferences from configuration
            IntPtr configuration = objc_msgSend(_webView, sel_registerName("configuration"));
            if (configuration == IntPtr.Zero)
                return false;

            IntPtr preferences = objc_msgSend(configuration, sel_registerName("preferences"));
            if (preferences == IntPtr.Zero)
                return false;

            return objc_msgSend_bool(preferences, sel_registerName("javaScriptEnabled"));
        }
        set
        {
            if (_disposed || _webView == IntPtr.Zero)
                return;

            IntPtr configuration = objc_msgSend(_webView, sel_registerName("configuration"));
            if (configuration == IntPtr.Zero)
                return;

            IntPtr preferences = objc_msgSend(configuration, sel_registerName("preferences"));
            if (preferences == IntPtr.Zero)
                return;

            objc_msgSend(preferences, sel_registerName("setJavaScriptEnabled:"), value);
        }
    }

    public void SetUserAgent(string userAgent)
    {
        if (_disposed || _webView == IntPtr.Zero || string.IsNullOrEmpty(userAgent))
            return;

        IntPtr userAgentString = CreateNSString(userAgent);
        objc_msgSend(_webView, sel_registerName("setCustomUserAgent:"), userAgentString);
        ReleaseNSString(userAgentString);
    }

    public string GetUserAgent()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return string.Empty;

        IntPtr userAgent = objc_msgSend(_webView, sel_registerName("customUserAgent"));
        return NSStringToString(userAgent);
    }

    public void ClearCookies()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        // WKWebView cookie clearing requires async API
        // For now, this is a stub implementation
        // Full implementation would use WKWebsiteDataStore.removeDataOfTypes
    }

    public void ClearCache()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        // WKWebView cache clearing requires async API
        // For now, this is a stub implementation
        // Full implementation would use WKWebsiteDataStore.removeDataOfTypes
    }

    public Task InitializeAsync()
    {
        // WKWebView doesn't require async initialization like WebView2
        return Task.CompletedTask;
    }

    public bool IsInitialized => true;

    public void SetBounds(int x, int y, int width, int height)
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        NSRect frame = new NSRect
        {
            origin = new NSPoint { x = x, y = y },
            size = new NSSize { width = width, height = height }
        };

        objc_msgSend_rect(_webView, sel_registerName("setFrame:"), frame);
    }

    public Rectangle GetBounds()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return default;

        NSRect frame = objc_msgSend_stret_rect(_webView, sel_registerName("frame"));

        return new Rectangle(
            (int)frame.origin.x,
            (int)frame.origin.y,
            (int)frame.size.width,
            (int)frame.size.height
        );
    }

    public void SetVisible(bool visible)
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        objc_msgSend(_webView, sel_registerName("setHidden:"), !visible);
    }

    public bool GetVisible()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return false;

        return !objc_msgSend_bool(_webView, sel_registerName("isHidden"));
    }

    public void SetEnabled(bool enabled)
    {
        // WKWebView doesn't have a direct enabled property
        // Interaction is controlled through user interaction settings
        if (_disposed || _webView == IntPtr.Zero)
            return;

        // Set userInteractionEnabled (available on NSView)
        objc_msgSend(_webView, sel_registerName("setAllowsBackForwardNavigationGestures:"), enabled);
    }

    public bool GetEnabled()
    {
        if (_disposed || _webView == IntPtr.Zero)
            return false;

        return objc_msgSend_bool(_webView, sel_registerName("allowsBackForwardNavigationGestures"));
    }

    public void SetBackground(RGB color)
    {
        if (_disposed || _webView == IntPtr.Zero)
            return;

        // WKWebView background is controlled through page content
        // We can set the NSView layer background color
        IntPtr layer = objc_msgSend(_webView, sel_registerName("layer"));
        if (layer != IntPtr.Zero)
        {
            IntPtr nsColor = CreateNSColor(color);
            IntPtr cgColor = objc_msgSend(nsColor, sel_registerName("CGColor"));
            objc_msgSend(layer, sel_registerName("setBackgroundColor:"), cgColor);
            ReleaseNSColor(nsColor);
        }
    }

    public RGB GetBackground()
    {
        return new RGB(255, 255, 255); // Default white
    }

    public void SetForeground(RGB color)
    {
        // WKWebView foreground color is controlled through CSS in content
        // Not directly applicable to the view itself
    }

    public RGB GetForeground()
    {
        return new RGB(0, 0, 0); // Default black
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_webView != IntPtr.Zero)
            {
                objc_msgSend(_webView, sel_registerName("release"));
                _webView = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    public override IntPtr GetNativeHandle()
    {
        return _webView;
    }

    private IntPtr CreateWKWebView(int style)
    {
        // Create WKWebViewConfiguration
        IntPtr configClass = objc_getClass("WKWebViewConfiguration");
        IntPtr config = objc_msgSend(objc_msgSend(configClass, sel_registerName("alloc")),
            sel_registerName("init"));

        // Create WKWebView with configuration
        IntPtr webViewClass = objc_getClass("WKWebView");
        IntPtr webView = objc_msgSend(webViewClass, sel_registerName("alloc"));

        // Initialize with frame and configuration
        NSRect frame = new NSRect
        {
            origin = new NSPoint { x = 0, y = 0 },
            size = new NSSize { width = 100, height = 100 }
        };

        webView = objc_msgSend_initWithFrame(webView, sel_registerName("initWithFrame:configuration:"),
            frame, config);

        // Release configuration (webView retains it)
        objc_msgSend(config, sel_registerName("release"));

        return webView;
    }

    private IntPtr CreateNSString(string text)
    {
        IntPtr nsStringClass = objc_getClass("NSString");
        IntPtr nsString = objc_msgSend(nsStringClass, sel_registerName("alloc"));

        byte[] utf8Bytes = System.Text.Encoding.UTF8.GetBytes(text + "\0");

        unsafe
        {
            fixed (byte* ptr = utf8Bytes)
            {
                nsString = objc_msgSend(nsString, sel_registerName("initWithUTF8String:"), (IntPtr)ptr);
            }
        }

        return nsString;
    }

    private void ReleaseNSString(IntPtr nsString)
    {
        if (nsString != IntPtr.Zero)
        {
            objc_msgSend(nsString, sel_registerName("release"));
        }
    }

    private string NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero)
            return string.Empty;

        IntPtr utf8Ptr = objc_msgSend(nsString, sel_registerName("UTF8String"));
        if (utf8Ptr == IntPtr.Zero)
            return string.Empty;

#if NETSTANDARD2_0
        return MarshalUTF8String(utf8Ptr);
#else
        return Marshal.PtrToStringUTF8(utf8Ptr) ?? string.Empty;
#endif
    }

    private IntPtr CreateNSColor(RGB color)
    {
        IntPtr nsColorClass = objc_getClass("NSColor");
        return objc_msgSend_color(nsColorClass, sel_registerName("colorWithRed:green:blue:alpha:"),
            color.Red / 255.0, color.Green / 255.0, color.Blue / 255.0, 1.0);
    }

    private void ReleaseNSColor(IntPtr nsColor)
    {
        // NSColor from factory methods are autoreleased, no need to release
    }

#if NETSTANDARD2_0
    private static string MarshalUTF8String(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return string.Empty;

        int length = 0;
        while (Marshal.ReadByte(ptr, length) != 0)
            length++;

        if (length == 0)
            return string.Empty;

        byte[] buffer = new byte[length];
        Marshal.Copy(ptr, buffer, 0, length);
        return System.Text.Encoding.UTF8.GetString(buffer);
    }
#endif

    // P/Invoke declarations
    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string className);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, bool arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_2args(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_rect(IntPtr receiver, IntPtr selector, NSRect arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend_stret")]
    private static extern NSRect objc_msgSend_stret_rect(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_color(IntPtr receiver, IntPtr selector,
        double red, double green, double blue, double alpha);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_initWithFrame(IntPtr receiver, IntPtr selector,
        NSRect frame, IntPtr config);

    [StructLayout(LayoutKind.Sequential)]
    private struct NSRect
    {
        public NSPoint origin;
        public NSSize size;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSPoint
    {
        public double x;
        public double y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NSSize
    {
        public double width;
        public double height;
    }
}
