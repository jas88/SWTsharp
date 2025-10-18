using SWTSharp.Platform;

namespace SWTSharp;

/// <summary>
/// A browser widget that displays HTML content using platform-native browser controls.
/// Supports navigation, JavaScript execution, and web content rendering.
/// </summary>
public class Browser : Composite
{
    private IPlatformBrowser? _platformBrowser;
    private string _url = string.Empty;
    private string _text = string.Empty;

    /// <summary>
    /// Gets the current URL.
    /// </summary>
    public string Url
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.GetUrl() ?? _url;
        }
    }

    /// <summary>
    /// Gets the current page title.
    /// </summary>
    public string Title
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.GetTitle() ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets whether the browser can navigate back.
    /// </summary>
    public bool CanGoBack
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.CanGoBack ?? false;
        }
    }

    /// <summary>
    /// Gets whether the browser can navigate forward.
    /// </summary>
    public bool CanGoForward
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.CanGoForward ?? false;
        }
    }

    /// <summary>
    /// Gets whether a page is currently loading.
    /// </summary>
    public bool IsLoading
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.IsLoading ?? false;
        }
    }

    /// <summary>
    /// Gets whether the browser is initialized and ready for use.
    /// </summary>
    public bool IsInitialized
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.IsInitialized ?? false;
        }
    }

    /// <summary>
    /// Gets or sets whether JavaScript execution is enabled.
    /// </summary>
    public bool JavaScriptEnabled
    {
        get
        {
            CheckWidget();
            return _platformBrowser?.JavaScriptEnabled ?? true;
        }
        set
        {
            CheckWidget();
            if (_platformBrowser != null)
            {
                _platformBrowser.JavaScriptEnabled = value;
            }
        }
    }

    /// <summary>
    /// Occurs when navigation is about to start.
    /// </summary>
    public event EventHandler<BrowserNavigatingEventArgs>? Navigating;

    /// <summary>
    /// Occurs when navigation has completed.
    /// </summary>
    public event EventHandler<BrowserNavigatedEventArgs>? Navigated;

    /// <summary>
    /// Occurs when navigation fails.
    /// </summary>
    public event EventHandler<BrowserNavigationErrorEventArgs>? NavigationError;

    /// <summary>
    /// Occurs when the document has finished loading.
    /// </summary>
    public event EventHandler<BrowserDocumentCompleteEventArgs>? DocumentComplete;

    /// <summary>
    /// Occurs when loading progress changes.
    /// </summary>
    public event EventHandler<BrowserProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Occurs when the page title changes.
    /// </summary>
    public event EventHandler<BrowserTitleChangedEventArgs>? TitleChanged;

    /// <summary>
    /// Occurs when the status text changes.
    /// </summary>
    public event EventHandler<BrowserStatusTextChangedEventArgs>? StatusTextChanged;

    /// <summary>
    /// Occurs when a new window is requested.
    /// </summary>
    public event EventHandler<BrowserNewWindowEventArgs>? NewWindow;

    /// <summary>
    /// Occurs when the browser process terminates.
    /// </summary>
    public event EventHandler<BrowserProcessTerminatedEventArgs>? ProcessTerminated;

    /// <summary>
    /// Creates a new browser control.
    /// </summary>
    /// <param name="parent">The parent composite</param>
    /// <param name="style">The widget style bits</param>
    public Browser(Composite parent, int style) : base(parent, style)
    {
        CreateWidget();
    }

    /// <summary>
    /// Creates the platform-specific browser widget.
    /// </summary>
    protected override void CreateWidget()
    {
        // Create platform browser widget
        var widget = Platform.PlatformFactory.Instance.CreateBrowserWidget(
            Parent?.PlatformWidget,
            Style
        );

        PlatformWidget = widget;
        _platformBrowser = widget as IPlatformBrowser;

        // Wire up events
        if (_platformBrowser != null)
        {
            _platformBrowser.Navigating += OnPlatformNavigating;
            _platformBrowser.Navigated += OnPlatformNavigated;
            _platformBrowser.NavigationError += OnPlatformNavigationError;
            _platformBrowser.DocumentComplete += OnPlatformDocumentComplete;
            _platformBrowser.ProgressChanged += OnPlatformProgressChanged;
            _platformBrowser.TitleChanged += OnPlatformTitleChanged;
            _platformBrowser.StatusTextChanged += OnPlatformStatusTextChanged;
            _platformBrowser.NewWindow += OnPlatformNewWindow;
            _platformBrowser.ProcessTerminated += OnPlatformProcessTerminated;
        }
    }

    /// <summary>
    /// Asynchronously initializes the browser control.
    /// Must be called before using the browser on platforms that require async initialization (e.g., Windows WebView2).
    /// </summary>
    public async Task InitializeAsync()
    {
        CheckWidget();
        if (_platformBrowser != null)
        {
            await _platformBrowser.InitializeAsync();
        }
    }

    /// <summary>
    /// Navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to</param>
    /// <returns>True if navigation started successfully</returns>
    public bool Navigate(string url)
    {
        CheckWidget();
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        _url = url;
        return _platformBrowser?.Navigate(url) ?? false;
    }

    /// <summary>
    /// Sets the HTML content directly.
    /// </summary>
    /// <param name="html">The HTML content to display</param>
    /// <param name="baseUrl">Optional base URL for resolving relative URLs</param>
    /// <returns>True if content was set successfully</returns>
    public bool SetText(string html, string? baseUrl = null)
    {
        CheckWidget();
        if (html == null)
        {
            throw new ArgumentNullException(nameof(html));
        }

        _text = html;
        return _platformBrowser?.SetText(html, baseUrl) ?? false;
    }

    /// <summary>
    /// Gets the current URL.
    /// </summary>
    public string GetUrl()
    {
        CheckWidget();
        return Url;
    }

    /// <summary>
    /// Gets the current page title.
    /// </summary>
    public string GetTitle()
    {
        CheckWidget();
        return Title;
    }

    /// <summary>
    /// Navigates back in the browsing history.
    /// </summary>
    /// <returns>True if navigation occurred</returns>
    public bool GoBack()
    {
        CheckWidget();
        return _platformBrowser?.GoBack() ?? false;
    }

    /// <summary>
    /// Navigates forward in the browsing history.
    /// </summary>
    /// <returns>True if navigation occurred</returns>
    public bool GoForward()
    {
        CheckWidget();
        return _platformBrowser?.GoForward() ?? false;
    }

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    public void Refresh()
    {
        CheckWidget();
        _platformBrowser?.Refresh();
    }

    /// <summary>
    /// Stops the current page loading.
    /// </summary>
    public void Stop()
    {
        CheckWidget();
        _platformBrowser?.Stop();
    }

    /// <summary>
    /// Executes JavaScript code asynchronously and returns the result.
    /// </summary>
    /// <param name="script">The JavaScript code to execute</param>
    /// <returns>The result of the script execution</returns>
    public Task<string?> ExecuteScriptAsync(string script)
    {
        CheckWidget();
        if (string.IsNullOrEmpty(script))
        {
            throw new ArgumentException("Script cannot be null or empty", nameof(script));
        }

        return _platformBrowser?.ExecuteScriptAsync(script)
            ?? Task.FromResult<string?>(null);
    }

    /// <summary>
    /// Executes JavaScript code synchronously (blocking).
    /// Note: This may block the UI thread. Prefer ExecuteScriptAsync when possible.
    /// </summary>
    /// <param name="script">The JavaScript code to execute</param>
    /// <returns>The result of the script execution</returns>
    public string? ExecuteScript(string script)
    {
        CheckWidget();
        if (string.IsNullOrEmpty(script))
        {
            throw new ArgumentException("Script cannot be null or empty", nameof(script));
        }

        return _platformBrowser?.ExecuteScript(script);
    }

    /// <summary>
    /// Sets the user agent string.
    /// </summary>
    /// <param name="userAgent">The user agent string</param>
    public void SetUserAgent(string userAgent)
    {
        CheckWidget();
        if (string.IsNullOrEmpty(userAgent))
        {
            throw new ArgumentException("User agent cannot be null or empty", nameof(userAgent));
        }

        _platformBrowser?.SetUserAgent(userAgent);
    }

    /// <summary>
    /// Gets the user agent string.
    /// </summary>
    public string GetUserAgent()
    {
        CheckWidget();
        return _platformBrowser?.GetUserAgent() ?? string.Empty;
    }

    /// <summary>
    /// Clears all cookies.
    /// </summary>
    public void ClearCookies()
    {
        CheckWidget();
        _platformBrowser?.ClearCookies();
    }

    /// <summary>
    /// Clears the browser cache.
    /// </summary>
    public void ClearCache()
    {
        CheckWidget();
        _platformBrowser?.ClearCache();
    }

    // Event handlers that forward platform events to public events

    private void OnPlatformNavigating(object? sender, BrowserNavigatingEventArgs e)
    {
        CheckWidget();
        Navigating?.Invoke(this, e);
    }

    private void OnPlatformNavigated(object? sender, BrowserNavigatedEventArgs e)
    {
        CheckWidget();
        Navigated?.Invoke(this, e);
    }

    private void OnPlatformNavigationError(object? sender, BrowserNavigationErrorEventArgs e)
    {
        CheckWidget();
        NavigationError?.Invoke(this, e);
    }

    private void OnPlatformDocumentComplete(object? sender, BrowserDocumentCompleteEventArgs e)
    {
        CheckWidget();
        DocumentComplete?.Invoke(this, e);
    }

    private void OnPlatformProgressChanged(object? sender, BrowserProgressEventArgs e)
    {
        CheckWidget();
        ProgressChanged?.Invoke(this, e);
    }

    private void OnPlatformTitleChanged(object? sender, BrowserTitleChangedEventArgs e)
    {
        CheckWidget();
        TitleChanged?.Invoke(this, e);
    }

    private void OnPlatformStatusTextChanged(object? sender, BrowserStatusTextChangedEventArgs e)
    {
        CheckWidget();
        StatusTextChanged?.Invoke(this, e);
    }

    private void OnPlatformNewWindow(object? sender, BrowserNewWindowEventArgs e)
    {
        CheckWidget();
        NewWindow?.Invoke(this, e);
    }

    private void OnPlatformProcessTerminated(object? sender, BrowserProcessTerminatedEventArgs e)
    {
        CheckWidget();
        ProcessTerminated?.Invoke(this, e);
    }

    protected override void ReleaseWidget()
    {
        // Unsubscribe from platform events to prevent memory leaks
        if (_platformBrowser != null)
        {
            _platformBrowser.Navigating -= OnPlatformNavigating;
            _platformBrowser.Navigated -= OnPlatformNavigated;
            _platformBrowser.NavigationError -= OnPlatformNavigationError;
            _platformBrowser.DocumentComplete -= OnPlatformDocumentComplete;
            _platformBrowser.ProgressChanged -= OnPlatformProgressChanged;
            _platformBrowser.TitleChanged -= OnPlatformTitleChanged;
            _platformBrowser.StatusTextChanged -= OnPlatformStatusTextChanged;
            _platformBrowser.NewWindow -= OnPlatformNewWindow;
            _platformBrowser.ProcessTerminated -= OnPlatformProcessTerminated;
        }

        // Clear event subscriptions
        Navigating = null;
        Navigated = null;
        NavigationError = null;
        DocumentComplete = null;
        ProgressChanged = null;
        TitleChanged = null;
        StatusTextChanged = null;
        NewWindow = null;
        ProcessTerminated = null;

        _platformBrowser = null;
        base.ReleaseWidget();
    }
}
