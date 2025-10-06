using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Text widget.
/// </summary>
public class TextTests : WidgetTestBase
{
    [Fact]
    public void Text_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Text(shell, SWT.SINGLE));
    }

    [Fact]
    public void Text_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Text(shell, style),
            SWT.SINGLE,
            SWT.MULTI,
            SWT.PASSWORD,
            SWT.READ_ONLY,
            SWT.WRAP
        );
    }

    [Fact]
    public void Text_TextContent_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Text(shell, SWT.SINGLE),
            t => t.TextContent,
            (t, v) => t.TextContent = v,
            "Test input"
        );
    }

    [Fact]
    public void Text_TextContent_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => new Text(shell, SWT.SINGLE),
            t => t.TextContent,
            (t, v) => t.TextContent = v,
            string.Empty
        );
    }

    [Fact]
    public void Text_TextContent_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var text = new Text(shell, SWT.SINGLE);

        text.TextContent = null!;

        Assert.Equal(string.Empty, text.TextContent);

        text.Dispose();
    }

    [Fact]
    public void Text_Append_ShouldAppendText()
    {
        using var shell = CreateTestShell();
        var text = new Text(shell, SWT.MULTI);

        text.TextContent = "Hello";
        text.Append(" World");

        Assert.Equal("Hello World", text.TextContent);

        text.Dispose();
    }

    [Fact]
    public void Text_Append_MultipleAppends_ShouldConcatenate()
    {
        using var shell = CreateTestShell();
        var text = new Text(shell, SWT.MULTI);

        text.TextContent = "A";
        text.Append("B");
        text.Append("C");

        Assert.Equal("ABC", text.TextContent);

        text.Dispose();
    }

    [Fact]
    public void Text_TextLimit_ShouldLimitText()
    {
        using var shell = CreateTestShell();
        var text = new Text(shell, SWT.SINGLE);

        text.TextLimit = 5;
        text.TextContent = "1234567890";

        Assert.Equal("12345", text.TextContent);

        text.Dispose();
    }

    [Fact]
    public void Text_TextLimit_ZeroShouldRemoveLimit()
    {
        using var shell = CreateTestShell();
        var text = new Text(shell, SWT.SINGLE);

        text.TextLimit = 0;
        text.TextContent = "This is a long text without limit";

        Assert.Equal("This is a long text without limit", text.TextContent);

        text.Dispose();
    }

    [Fact]
    public void Text_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Text(shell, SWT.SINGLE));
    }

    [Fact]
    public void Text_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Text(shell, SWT.SINGLE));
    }

    [Fact]
    public void Text_SetTextContent_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Text(shell, SWT.SINGLE),
            t => t.TextContent = "Test"
        );
    }

    [Fact]
    public void Text_Append_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var text = new Text(shell, SWT.MULTI);
        text.Dispose();

        Assert.Throws<SWTDisposedException>(() => text.Append("Test"));
    }

    [Fact]
    public void Text_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Text(shell, SWT.SINGLE));
    }

    [Fact]
    public void Text_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Text(shell, SWT.SINGLE),
            t => t.Visible,
            (t, v) => t.Visible = v,
            false
        );
    }

    [Fact]
    public void Text_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Text(shell, SWT.SINGLE),
            t => t.Enabled,
            (t, v) => t.Enabled = v,
            false
        );
    }
}
