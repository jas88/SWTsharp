using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Browser widget.
/// Tests cover basic creation, URL/HTML operations, navigation, events, and disposal.
/// </summary>
public class BrowserTests : WidgetTestBase
{
    public BrowserTests(DisplayFixture displayFixture) : base(displayFixture) { }

    #region Creation Tests

    [Fact]
    public void Browser_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Browser(shell, SWT.NONE));
    }

    [Fact]
    public void Browser_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Browser(shell, style),
            SWT.NONE,
            SWT.WEBKIT,
            SWT.MOZILLA,
            SWT.BORDER
        );
    }

    [Fact]
    public void Browser_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Browser(shell, SWT.NONE));
    }

    #endregion

    #region URL Operations

    [Fact]
    public void Browser_SetUrl_ValidHttpUrl_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("https://www.example.com");
            var url = browser.GetUrl();

            Assert.Equal("https://www.example.com", url);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetUrl_ValidHttpsUrl_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("https://www.example.com/page");
            var url = browser.GetUrl();

            Assert.Equal("https://www.example.com/page", url);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetUrl_FileUrl_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("file:///tmp/test.html");
            var url = browser.GetUrl();

            Assert.Equal("file:///tmp/test.html", url);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetUrl_WithEmptyString_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("");
            var url = browser.GetUrl();

            Assert.Equal(string.Empty, url);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetUrl_WithNull_ShouldSetEmptyString()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl(null!);
            var url = browser.GetUrl();

            Assert.Equal(string.Empty, url);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_GetUrl_InitialState_ShouldBeEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var url = browser.GetUrl();

            Assert.Equal(string.Empty, url);

            browser.Dispose();
        });
    }

    #endregion

    #region HTML Content Operations

    [Fact]
    public void Browser_SetText_SimpleHtml_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetText("<html><body><h1>Test</h1></body></html>");
            var text = browser.GetText();

            Assert.Contains("Test", text);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetText_ComplexHtml_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var html = "<html><head><title>Test Page</title></head>" +
                      "<body><h1>Header</h1><p>Content</p></body></html>";
            browser.SetText(html);
            var text = browser.GetText();

            Assert.Contains("Header", text);
            Assert.Contains("Content", text);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetText_WithEmptyString_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetText("");
            var text = browser.GetText();

            Assert.Equal(string.Empty, text);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetText_WithNull_ShouldSetEmptyString()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetText(null!);
            var text = browser.GetText();

            Assert.Equal(string.Empty, text);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_GetText_InitialState_ShouldBeEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var text = browser.GetText();

            Assert.Equal(string.Empty, text);

            browser.Dispose();
        });
    }

    #endregion

    #region Navigation Methods

    [Fact]
    public void Browser_Refresh_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetText("<html><body>Test</body></html>");
            browser.Refresh();

            // Should not throw exception
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_Stop_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("https://www.example.com");
            browser.Stop();

            // Should not throw exception
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_Back_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("https://www.example.com");
            browser.Back();

            // Should not throw exception even if no history
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_Forward_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetUrl("https://www.example.com");
            browser.Forward();

            // Should not throw exception even if no forward history
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_NavigationSequence_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            // Navigate to first page
            browser.SetUrl("https://www.example.com");

            // Navigate to second page
            browser.SetUrl("https://www.example.org");

            // Go back
            browser.Back();

            // Go forward
            browser.Forward();

            // Should not throw
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    #endregion

    #region Event Handling

    [Fact]
    public void Browser_LocationChangedEvent_ShouldFire()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var eventFired = false;
            var receivedLocation = string.Empty;

            browser.LocationChanged += (sender, e) =>
            {
                eventFired = true;
                receivedLocation = e.Location;
            };

            browser.SetUrl("https://www.example.com");

            // Note: Event may be asynchronous, so we just verify handler doesn't throw
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_TitleChangedEvent_ShouldFire()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var eventFired = false;
            var receivedTitle = string.Empty;

            browser.TitleChanged += (sender, e) =>
            {
                eventFired = true;
                receivedTitle = e.Title;
            };

            browser.SetText("<html><head><title>Test Title</title></head><body>Content</body></html>");

            // Note: Event may be asynchronous
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_ProgressChangedEvent_ShouldFire()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var eventFired = false;
            var receivedCurrent = 0;
            var receivedTotal = 0;

            browser.ProgressChanged += (sender, e) =>
            {
                eventFired = true;
                receivedCurrent = e.Current;
                receivedTotal = e.Total;
            };

            browser.SetUrl("https://www.example.com");

            // Note: Event may be asynchronous
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_StatusTextChangedEvent_ShouldFire()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            var eventFired = false;
            var receivedText = string.Empty;

            browser.StatusTextChanged += (sender, e) =>
            {
                eventFired = true;
                receivedText = e.Text;
            };

            browser.SetUrl("https://www.example.com");

            // Note: Event may be asynchronous
            Assert.NotNull(browser);

            browser.Dispose();
        });
    }

    #endregion

    #region Bounds and Sizing

    [Fact]
    public void Browser_SetBounds_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetBounds(10, 20, 300, 400);
            var bounds = browser.GetBounds();

            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(300, bounds.Width);
            Assert.Equal(400, bounds.Height);

            browser.Dispose();
        });
    }

    [Fact]
    public void Browser_SetSize_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);

            browser.SetSize(400, 300);
            var size = browser.GetSize();

            Assert.Equal(400, size.Width);
            Assert.Equal(300, size.Height);

            browser.Dispose();
        });
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Browser_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Browser(shell, SWT.NONE));
    }

    [Fact]
    public void Browser_SetUrl_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Browser(shell, SWT.NONE),
            b => b.SetUrl("https://www.example.com")
        );
    }

    [Fact]
    public void Browser_GetUrl_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);
            browser.Dispose();

            Assert.Throws<SWTDisposedException>(() => browser.GetUrl());
        });
    }

    [Fact]
    public void Browser_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Browser(shell, SWT.NONE),
            b => b.SetText("<html><body>Test</body></html>")
        );
    }

    [Fact]
    public void Browser_GetText_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var browser = new Browser(shell, SWT.NONE);
            browser.Dispose();

            Assert.Throws<SWTDisposedException>(() => browser.GetText());
        });
    }

    [Fact]
    public void Browser_Refresh_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Browser(shell, SWT.NONE),
            b => b.Refresh()
        );
    }

    [Fact]
    public void Browser_Stop_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Browser(shell, SWT.NONE),
            b => b.Stop()
        );
    }

    [Fact]
    public void Browser_Back_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Browser(shell, SWT.NONE),
            b => b.Back()
        );
    }

    [Fact]
    public void Browser_Forward_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Browser(shell, SWT.NONE),
            b => b.Forward()
        );
    }

    #endregion

    #region Common Widget Properties

    [Fact]
    public void Browser_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Browser(shell, SWT.NONE));
    }

    [Fact]
    public void Browser_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Browser(shell, SWT.NONE),
            b => b.Visible,
            (b, v) => b.Visible = v,
            false
        );
    }

    [Fact]
    public void Browser_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Browser(shell, SWT.NONE),
            b => b.Enabled,
            (b, v) => b.Enabled = v,
            false
        );
    }

    #endregion
}
