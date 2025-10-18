#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
using SWTSharp.Graphics;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace SWTSharp.Platform;

/// <summary>
/// Windows (Win32) platform implementation - Browser widget using WebView2.
/// WebView2 package is cross-platform compatible but runtime only works on Windows.
/// </summary>
internal partial class Win32Platform
{
    private class Win32Browser : IPlatformBrowser
    {
        private readonly IntPtr _parentHandle;
        private WebView2? _webView2;
        private bool _disposed;
        private bool _isInitialized;
        private string _currentUrl = string.Empty;
        private string _currentTitle = string.Empty;
        private bool _isLoading;
        private readonly bool _isWindows;

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

        public Win32Browser(IntPtr parentHandle)
        {
            _parentHandle = parentHandle;
            _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (_isWindows)
            {
                try
                {
                    // Create WebView2 control
                    _webView2 = new WebView2
                    {
                        Dock = System.Windows.Forms.DockStyle.Fill
                    };

                    // Wire up events
                    _webView2.NavigationStarting += OnNavigationStarting;
                    _webView2.NavigationCompleted += OnNavigationCompleted;
                    _webView2.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;

                    // Note: Actual window integration requires more Win32 interop
                    // For now, we create the control but need parent window setup
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Win32Browser] Failed to create WebView2 control: {ex.Message}");
                    _webView2 = null;
                }
            }
        }

        private void OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                Console.WriteLine($"[Win32Browser] WebView2 initialization failed: {e.InitializationException?.Message}");
                _isInitialized = false;
                return;
            }

            if (_webView2?.CoreWebView2 != null)
            {
                // Wire up CoreWebView2 events
                _webView2.CoreWebView2.DocumentTitleChanged += (s, args) =>
                {
                    _currentTitle = _webView2.CoreWebView2.DocumentTitle;
                    TitleChanged?.Invoke(this, new BrowserTitleChangedEventArgs { Title = _currentTitle });
                };

                _webView2.CoreWebView2.ProcessFailed += (s, args) =>
                {
                    ProcessTerminated?.Invoke(this, new BrowserProcessTerminatedEventArgs());
                };

                _isInitialized = true;
            }
        }

        private void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            _isLoading = true;
            var args = new BrowserNavigatingEventArgs
            {
                Url = e.Uri,
                IsUserInitiated = e.IsUserInitiated,
                IsRedirect = e.IsRedirected
            };
            Navigating?.Invoke(this, args);

            if (args.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _isLoading = false;

            if (_webView2?.CoreWebView2 != null)
            {
                _currentUrl = _webView2.CoreWebView2.Source;
                _currentTitle = _webView2.CoreWebView2.DocumentTitle;
            }

            if (e.IsSuccess)
            {
                Navigated?.Invoke(this, new BrowserNavigatedEventArgs { Url = _currentUrl });
                DocumentComplete?.Invoke(this, new BrowserDocumentCompleteEventArgs { Url = _currentUrl });
            }
            else
            {
                var errorMessage = e.WebErrorStatus.ToString();
                NavigationError?.Invoke(this, new BrowserNavigationErrorEventArgs
                {
                    Url = _currentUrl,
                    ErrorMessage = errorMessage
                });
            }
        }

        public async Task InitializeAsync()
        {
            if (!_isWindows || _webView2 == null)
            {
                _isInitialized = false;
                return;
            }

            try
            {
                await _webView2.EnsureCoreWebView2Async(null);
                _isInitialized = _webView2.CoreWebView2 != null;
            }
            catch (WebView2RuntimeNotFoundException)
            {
                Console.WriteLine("[Win32Browser] WebView2 runtime not found. Please install from https://developer.microsoft.com/microsoft-edge/webview2/");
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Win32Browser] WebView2 initialization failed: {ex.Message}");
                _isInitialized = false;
            }
        }

        public bool IsInitialized => _isInitialized;

        public bool Navigate(string url)
        {
            if (!_isInitialized || _webView2?.CoreWebView2 == null || string.IsNullOrEmpty(url))
                return false;

            try
            {
                _webView2.CoreWebView2.Navigate(url);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Win32Browser] Navigation failed: {ex.Message}");
                return false;
            }
        }

        public bool SetText(string html, string? baseUrl = null)
        {
            if (!_isInitialized || _webView2?.CoreWebView2 == null || string.IsNullOrEmpty(html))
                return false;

            try
            {
                _webView2.CoreWebView2.NavigateToString(html);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Win32Browser] SetText failed: {ex.Message}");
                return false;
            }
        }

        public string GetUrl() => _currentUrl;

        public string GetTitle() => _currentTitle;

        public bool IsLoading => _isLoading;

        public async Task<string?> ExecuteScriptAsync(string script)
        {
            if (!_isInitialized || _webView2?.CoreWebView2 == null || string.IsNullOrEmpty(script))
                return null;

            try
            {
                var result = await _webView2.CoreWebView2.ExecuteScriptAsync(script);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Win32Browser] ExecuteScriptAsync failed: {ex.Message}");
                return null;
            }
        }

        public string? ExecuteScript(string script)
        {
            // Synchronous wrapper - blocks on async operation
            return ExecuteScriptAsync(script).GetAwaiter().GetResult();
        }

        public bool JavaScriptEnabled
        {
            get => _webView2?.CoreWebView2?.Settings?.IsScriptEnabled ?? true;
            set
            {
                if (_isInitialized && _webView2?.CoreWebView2?.Settings != null)
                {
                    _webView2.CoreWebView2.Settings.IsScriptEnabled = value;
                }
            }
        }

        public void SetUserAgent(string userAgent)
        {
            if (_isInitialized && _webView2?.CoreWebView2?.Settings != null)
            {
                _webView2.CoreWebView2.Settings.UserAgent = userAgent;
            }
        }

        public string GetUserAgent()
        {
            return _webView2?.CoreWebView2?.Settings?.UserAgent ?? string.Empty;
        }

        public async void ClearCookies()
        {
            if (_isInitialized && _webView2?.CoreWebView2 != null)
            {
                try
                {
                    var cookieManager = _webView2.CoreWebView2.CookieManager;
                    var cookies = await cookieManager.GetCookiesAsync(string.Empty);
                    foreach (var cookie in cookies)
                    {
                        cookieManager.DeleteCookie(cookie);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Win32Browser] ClearCookies failed: {ex.Message}");
                }
            }
        }

        public async void ClearCache()
        {
            if (_isInitialized && _webView2?.CoreWebView2?.Profile != null)
            {
                try
                {
                    await _webView2.CoreWebView2.Profile.ClearBrowsingDataAsync(
                        CoreWebView2BrowsingDataKinds.AllDomStorage |
                        CoreWebView2BrowsingDataKinds.AllSite |
                        CoreWebView2BrowsingDataKinds.DiskCache |
                        CoreWebView2BrowsingDataKinds.CacheStorage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Win32Browser] ClearCache failed: {ex.Message}");
                }
            }
        }

        public bool GoBack()
        {
            if (!_isInitialized || _webView2?.CoreWebView2 == null || !CanGoBack)
                return false;

            try
            {
                _webView2.CoreWebView2.GoBack();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool GoForward()
        {
            if (!_isInitialized || _webView2?.CoreWebView2 == null || !CanGoForward)
                return false;

            try
            {
                _webView2.CoreWebView2.GoForward();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Refresh()
        {
            if (_isInitialized && _webView2?.CoreWebView2 != null)
            {
                _webView2.CoreWebView2.Reload();
            }
        }

        public void Stop()
        {
            if (_isInitialized && _webView2?.CoreWebView2 != null)
            {
                _webView2.CoreWebView2.Stop();
            }
        }

        public bool CanGoBack => _webView2?.CoreWebView2?.CanGoBack ?? false;

        public bool CanGoForward => _webView2?.CoreWebView2?.CanGoForward ?? false;

        public void SetBounds(int x, int y, int width, int height)
        {
            if (_disposed || _webView2 == null)
                return;

            _webView2.Left = x;
            _webView2.Top = y;
            _webView2.Width = width;
            _webView2.Height = height;
        }

        public Rectangle GetBounds()
        {
            if (_disposed || _webView2 == null)
                return default;

            return new Rectangle(_webView2.Left, _webView2.Top, _webView2.Width, _webView2.Height);
        }

        public void SetVisible(bool visible)
        {
            if (_disposed || _webView2 == null)
                return;

            _webView2.Visible = visible;
        }

        public bool GetVisible()
        {
            return _webView2?.Visible ?? false;
        }

        public void SetEnabled(bool enabled)
        {
            if (_disposed || _webView2 == null)
                return;

            _webView2.Enabled = enabled;
        }

        public bool GetEnabled()
        {
            return _webView2?.Enabled ?? false;
        }

        public void SetBackground(RGB color) { }

        public RGB GetBackground() => new RGB(255, 255, 255);

        public void SetForeground(RGB color) { }

        public RGB GetForeground() => new RGB(0, 0, 0);

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_webView2 != null)
                {
                    _webView2.NavigationStarting -= OnNavigationStarting;
                    _webView2.NavigationCompleted -= OnNavigationCompleted;
                    _webView2.CoreWebView2InitializationCompleted -= OnCoreWebView2InitializationCompleted;
                    _webView2.Dispose();
                    _webView2 = null;
                }
                _disposed = true;
            }
        }
    }

    public IPlatformBrowser CreateBrowserWidget(IPlatformWidget? parent, int style)
    {
        IntPtr parentHandle = parent != null ? ExtractNativeHandle(parent) : IntPtr.Zero;

        if (_enableLogging)
        {
            var platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "WebView2" : "stub (non-Windows)";
            Console.WriteLine($"[Win32] Creating browser widget ({platform}). Parent: 0x{parentHandle:X}, Style: 0x{style:X}");
        }

        var browser = new Win32Browser(parentHandle);

        if (_enableLogging)
            Console.WriteLine($"[Win32] Browser widget created successfully");

        return browser;
    }
}
#endif
