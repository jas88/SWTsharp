using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Label widget.
/// </summary>
public class LabelTests : WidgetTestBase
{
    [Fact]
    public void Label_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Label(shell, SWT.NONE));
    }

    [Fact]
    public void Label_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Label(shell, style),
            SWT.NONE,
            SWT.SEPARATOR,
            SWT.HORIZONTAL,
            SWT.VERTICAL,
            SWT.WRAP
        );
    }

    [Fact]
    public void Label_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Label(shell, SWT.NONE),
            l => l.Text,
            (l, v) => l.Text = v,
            "Hello World"
        );
    }

    [Fact]
    public void Label_Text_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => new Label(shell, SWT.NONE),
            l => l.Text,
            (l, v) => l.Text = v,
            string.Empty
        );
    }

    [Fact]
    public void Label_Text_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var label = new Label(shell, SWT.NONE);

        label.Text = null!;

        Assert.Equal(string.Empty, label.Text);

        label.Dispose();
    }

    [Fact]
    public void Label_Text_MultipleUpdates_ShouldWork()
    {
        using var shell = CreateTestShell();
        var label = new Label(shell, SWT.NONE);

        label.Text = "First";
        Assert.Equal("First", label.Text);

        label.Text = "Second";
        Assert.Equal("Second", label.Text);

        label.Text = "Third";
        Assert.Equal("Third", label.Text);

        label.Dispose();
    }

    [Fact]
    public void Label_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Label(shell, SWT.NONE));
    }

    [Fact]
    public void Label_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Label(shell, SWT.NONE));
    }

    [Fact]
    public void Label_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Label(shell, SWT.NONE),
            l => l.Text = "Test"
        );
    }

    [Fact]
    public void Label_GetText_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var label = new Label(shell, SWT.NONE);
        label.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = label.Text);
    }

    [Fact]
    public void Label_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Label(shell, SWT.NONE));
    }

    [Fact]
    public void Label_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Label(shell, SWT.NONE),
            l => l.Visible,
            (l, v) => l.Visible = v,
            false
        );
    }

    [Fact]
    public void Label_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Label(shell, SWT.NONE),
            l => l.Enabled,
            (l, v) => l.Enabled = v,
            false
        );
    }

    [Fact]
    public void Label_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var label = new Label(shell, SWT.NONE);

        Assert.Same(shell.Display, label.Display);

        label.Dispose();
    }

    [Fact]
    public void Label_InitialText_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var label = new Label(shell, SWT.NONE);

        Assert.Equal(string.Empty, label.Text);

        label.Dispose();
    }
}
