using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Button widget.
/// </summary>
public class ButtonTests : WidgetTestBase
{
    public ButtonTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Button_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Button_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Button(shell, style),
            SWT.PUSH,
            SWT.CHECK,
            SWT.RADIO,
            SWT.TOGGLE,
            SWT.ARROW
        );
    }

    [Fact]
    public void Button_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Button(shell, SWT.PUSH),
            b => b.Text,
            (b, v) => b.Text = v,
            "Click Me"
        );
    }

    [Fact]
    public void Button_Text_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => new Button(shell, SWT.PUSH),
            b => b.Text,
            (b, v) => b.Text = v,
            string.Empty
        );
    }

    [Fact]
    public void Button_Text_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.PUSH);

        button.Text = null!;

        Assert.Equal(string.Empty, button.Text);

        button.Dispose();
    }

    [Fact]
    public void Button_Selection_Check_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.CHECK);

        Assert.False(button.Selection);

        button.Selection = true;
        Assert.True(button.Selection);

        button.Selection = false;
        Assert.False(button.Selection);

        button.Dispose();
    }

    [Fact]
    public void Button_Selection_Radio_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.RADIO);

        // GTK radio buttons may start selected if they're the first in a group
        // Just verify we can set and get the selection state
        button.Selection = true;
        Assert.True(button.Selection);

        button.Dispose();
    }

    [Fact]
    public void Button_Selection_Toggle_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.TOGGLE);

        Assert.False(button.Selection);

        button.Selection = true;
        Assert.True(button.Selection);

        button.Dispose();
    }

    [Fact]
    public void Button_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Button_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Button_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Button(shell, SWT.PUSH),
            b => b.Text = "Test"
        );
    }

    [Fact]
    public void Button_GetText_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.PUSH);
        button.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = button.Text);
    }

    [Fact]
    public void Button_SetSelection_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Button(shell, SWT.CHECK),
            b => b.Selection = true
        );
    }

    [Fact]
    public void Button_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Button_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Button(shell, SWT.PUSH),
            b => b.Visible,
            (b, v) => b.Visible = v,
            false
        );
    }
}
