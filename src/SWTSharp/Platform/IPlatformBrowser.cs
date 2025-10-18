namespace SWTSharp.Platform;

/// <summary>
/// Platform-specific browser widget interface.
/// Provides web content rendering and navigation capabilities using platform-native browser controls.
/// </summary>
public interface IPlatformBrowser : IPlatformWidget, IPlatformBrowserEvents
{
    // Core Navigation

    /// <summary>
    /// Navigates to the specified URL.
    /// </summary>
    /// <param name="url">URL to navigate to</param>
    /// <returns>True if navigation started successfully</returns>
    bool Navigate(string url);

    /// <summary>
    /// Sets the HTML content directly.
    /// </summary>
    /// <param name="html">HTML content to display</param>
    /// <param name="baseUrl">Base URL for resolving relative URLs (optional)</param>
    /// <returns>True if content was set successfully</returns>
    bool SetText(string html, string? baseUrl = null);

    /// <summary>
    /// Gets the current URL.
    /// </summary>
    string GetUrl();

    /// <summary>
    /// Gets the page title.
    /// </summary>
    string GetTitle();

    // Navigation Actions

    /// <summary>
    /// Navigates back in history.
    /// </summary>
    /// <returns>True if navigation occurred</returns>
    bool GoBack();

    /// <summary>
    /// Navigates forward in history.
    /// </summary>
    /// <returns>True if navigation occurred</returns>
    bool GoForward();

    /// <summary>
    /// Reloads the current page.
    /// </summary>
    void Refresh();

    /// <summary>
    /// Stops the current navigation/loading.
    /// </summary>
    void Stop();

    // Navigation State

    /// <summary>
    /// Gets whether back navigation is available.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets whether forward navigation is available.
    /// </summary>
    bool CanGoForward { get; }

    /// <summary>
    /// Gets whether the page is currently loading.
    /// </summary>
    bool IsLoading { get; }

    // JavaScript Execution

    /// <summary>
    /// Executes JavaScript code asynchronously.
    /// </summary>
    /// <param name="script">JavaScript code to execute</param>
    /// <returns>Task that completes with the script result</returns>
    Task<string?> ExecuteScriptAsync(string script);

    /// <summary>
    /// Executes JavaScript and returns result synchronously (blocking).
    /// Use only when async is not possible.
    /// </summary>
    /// <param name="script">JavaScript code to execute</param>
    /// <returns>Script result as string</returns>
    string? ExecuteScript(string script);

    // Configuration

    /// <summary>
    /// Enables or disables JavaScript execution.
    /// </summary>
    bool JavaScriptEnabled { get; set; }

    /// <summary>
    /// Sets the user agent string.
    /// </summary>
    void SetUserAgent(string userAgent);

    /// <summary>
    /// Gets the user agent string.
    /// </summary>
    string GetUserAgent();

    // Cookie Management

    /// <summary>
    /// Clears all cookies.
    /// </summary>
    void ClearCookies();

    /// <summary>
    /// Clears browser cache.
    /// </summary>
    void ClearCache();

    // Async Initialization Support

    /// <summary>
    /// Asynchronously initializes the browser control.
    /// Must be called before any other operations on platforms requiring async init.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Gets whether the browser is initialized and ready for use.
    /// </summary>
    bool IsInitialized { get; }
}

/// <summary>
/// Event handling for browser navigation and loading.
/// </summary>
public interface IPlatformBrowserEvents
{
    /// <summary>
    /// Occurs when navigation is about to start.
    /// </summary>
    event EventHandler<BrowserNavigatingEventArgs>? Navigating;

    /// <summary>
    /// Occurs when navigation has completed successfully.
    /// </summary>
    event EventHandler<BrowserNavigatedEventArgs>? Navigated;

    /// <summary>
    /// Occurs when navigation fails.
    /// </summary>
    event EventHandler<BrowserNavigationErrorEventArgs>? NavigationError;

    /// <summary>
    /// Occurs when document loading is complete.
    /// </summary>
    event EventHandler<BrowserDocumentCompleteEventArgs>? DocumentComplete;

    /// <summary>
    /// Occurs when the page title changes.
    /// </summary>
    event EventHandler<BrowserTitleChangedEventArgs>? TitleChanged;

    /// <summary>
    /// Occurs periodically during page load to report progress.
    /// </summary>
    event EventHandler<BrowserProgressEventArgs>? ProgressChanged;

    /// <summary>
    /// Occurs when the browser status text changes.
    /// </summary>
    event EventHandler<BrowserStatusTextChangedEventArgs>? StatusTextChanged;

    /// <summary>
    /// Occurs when a new window is requested (popup).
    /// </summary>
    event EventHandler<BrowserNewWindowEventArgs>? NewWindow;

    /// <summary>
    /// Occurs when the browser process crashes or terminates.
    /// </summary>
    event EventHandler<BrowserProcessTerminatedEventArgs>? ProcessTerminated;
}

/// <summary>
/// Event arguments for navigation events.
/// </summary>
public class BrowserNavigatingEventArgs : EventArgs
{
    public string Url { get; set; } = string.Empty;
    public bool Cancel { get; set; }
    public bool IsRedirect { get; set; }
    public bool IsUserInitiated { get; set; }
}

public class BrowserNavigatedEventArgs : EventArgs
{
    public string Url { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}

public class BrowserNavigationErrorEventArgs : EventArgs
{
    public string Url { get; set; } = string.Empty;
    public int ErrorCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class BrowserDocumentCompleteEventArgs : EventArgs
{
    public string Url { get; set; } = string.Empty;
}

public class BrowserTitleChangedEventArgs : EventArgs
{
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// Event arguments for progress events.
/// </summary>
public class BrowserProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets the completion percentage (0-100).
    /// </summary>
    public int PercentComplete { get; set; }

    /// <summary>
    /// Gets the number of bytes received.
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// Gets the total number of bytes.
    /// </summary>
    public long TotalBytes { get; set; }
}

/// <summary>
/// Event arguments for status text changed events.
/// </summary>
public class BrowserStatusTextChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the new status text.
    /// </summary>
    public string StatusText { get; set; } = string.Empty;
}

/// <summary>
/// Event arguments for new window events.
/// </summary>
public class BrowserNewWindowEventArgs : EventArgs
{
    /// <summary>
    /// Gets the URL for the new window.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to cancel the new window creation.
    /// </summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Event arguments for process terminated events.
/// </summary>
public class BrowserProcessTerminatedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the process exit code.
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Gets the reason for termination.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
