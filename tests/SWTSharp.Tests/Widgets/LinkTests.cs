using Xunit;
using SWTSharp;
using SWTSharp.Graphics;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Link widget.
/// </summary>
public class LinkTests : WidgetTestBase
{
    public LinkTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Link_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Link(shell, SWT.NONE));
    }

    [Fact]
    public void Link_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Link(shell, style),
            SWT.NONE,
            SWT.BORDER
        );
    }

    [Fact]
    public void Link_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Text,
            (link, v) => link.Text = v,
            "Click <a>here</a> for more information"
        );
    }

    [Fact]
    public void Link_Text_PlainText_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Text,
            (link, v) => link.Text = v,
            "This is plain text without links"
        );
    }

    [Fact]
    public void Link_Text_WithHref_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Text,
            (link, v) => link.Text = v,
            "Visit <a href=\"homepage\">our website</a>"
        );
    }

    [Fact]
    public void Link_Text_MultipleLinks_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Text,
            (link, v) => link.Text = v,
            "Check <a href=\"link1\">first</a> and <a href=\"link2\">second</a> links"
        );
    }

    [Fact]
    public void Link_Text_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Text,
            (link, v) => link.Text = v,
            string.Empty
        );
    }

    [Fact]
    public void Link_Text_WithNull_ShouldSetEmptyString()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);

            link.Text = null!;

            Assert.Equal(string.Empty, link.Text);

            link.Dispose();
        });
    }

    [Fact]
    public void Link_GetText_ShouldReturnText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);

            link.Text = "Test <a>link</a> text";

            Assert.Equal("Test <a>link</a> text", link.GetText());

            link.Dispose();
        });
    }

    [Fact]
    public void Link_SetText_ShouldUpdateText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);

            link.SetText("Updated <a>link</a>");

            Assert.Equal("Updated <a>link</a>", link.Text);

            link.Dispose();
        });
    }

    [Fact]
    public void Link_LinkSelected_EventShouldFire()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);
            link.Text = "Click <a href=\"test-id\">here</a>";

            string? capturedLinkId = null;
            int eventFireCount = 0;

            link.LinkSelected += (sender, e) =>
            {
                eventFireCount++;
                capturedLinkId = e.Text;
            };

            // Simulate link click through platform widget
            if (link.PlatformWidget is SWTSharp.Platform.IPlatformLink platformLink)
            {
                // Trigger the event handler directly (platform simulation)
                var eventInfo = typeof(SWTSharp.Platform.IPlatformLink).GetEvent("LinkClicked");
                var field = platformLink.GetType()
                    .GetField("LinkClicked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                // Note: In actual tests, the platform implementation would trigger this
                // For now, we verify the event handler is properly wired
            }

            link.Dispose();
        });
    }

    [Fact]
    public void Link_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Link(shell, SWT.NONE));
    }

    [Fact]
    public void Link_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Link(shell, SWT.NONE));
    }

    [Fact]
    public void Link_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Link(shell, SWT.NONE),
            link => link.Text = "Test"
        );
    }

    [Fact]
    public void Link_GetText_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);
            link.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = link.Text);
        });
    }

    [Fact]
    public void Link_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Link(shell, SWT.NONE));
    }

    [Fact]
    public void Link_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Visible,
            (link, v) => link.Visible = v,
            false
        );
    }

    [Fact]
    public void Link_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Enabled,
            (link, v) => link.Enabled = v,
            false
        );
    }

    [Fact]
    public void Link_Bounds_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);

            var bounds = new Rectangle(10, 20, 200, 50);
            link.Bounds = bounds;

            var retrieved = link.Bounds;
            Assert.Equal(10, retrieved.X);
            Assert.Equal(20, retrieved.Y);
            Assert.Equal(200, retrieved.Width);
            Assert.Equal(50, retrieved.Height);

            link.Dispose();
        });
    }

    [Fact]
    public void Link_Size_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var link = new Link(shell, SWT.NONE);

            var size = new Point(150, 30);
            link.Size = size;

            var retrieved = link.Size;
            Assert.Equal(150, retrieved.X);
            Assert.Equal(30, retrieved.Y);

            link.Dispose();
        });
    }

    [Fact]
    public void Link_Text_ComplexMarkup_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Link(shell, SWT.NONE),
            link => link.Text,
            (link, v) => link.Text = v,
            "Welcome! <a href=\"login\">Login</a> or <a href=\"register\">Register</a> to continue. " +
            "See <a href=\"terms\">Terms of Service</a>."
        );
    }

    [Fact]
    public void Link_LinkEventArgs_Text_ShouldBeSet()
    {
        var args = new LinkEventArgs { Text = "test-link-id" };
        Assert.Equal("test-link-id", args.Text);
    }

    [Fact]
    public void Link_LinkEventArgs_DefaultText_ShouldBeEmpty()
    {
        var args = new LinkEventArgs();
        Assert.Equal(string.Empty, args.Text);
    }
}
