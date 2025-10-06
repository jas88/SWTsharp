using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Shell widget.
/// </summary>
public class ShellTests : WidgetTestBase
{
    [Fact]
    public void Shell_Create_WithDisplay_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Shell(shell.Display!));
    }

    [Fact]
    public void Shell_Create_WithDefaultConstructor_ShouldSucceed()
    {
        var shell = new Shell();
        Assert.NotNull(shell);
        Assert.NotNull(shell.Display);
        AssertNotDisposed(shell);
        shell.Dispose();
    }

    [Fact]
    public void Shell_Create_WithParentShell_ShouldSucceed()
    {
        using var parentShell = CreateTestShell();
        var childShell = new Shell(parentShell);

        Assert.NotNull(childShell);
        Assert.Same(parentShell.Display, childShell.Display);

        childShell.Dispose();
    }

    [Fact]
    public void Shell_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => shell,
            s => s.Text,
            (s, v) => s.Text = v,
            "Test Window"
        );
    }

    [Fact]
    public void Shell_Text_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => shell,
            s => s.Text,
            (s, v) => s.Text = v,
            string.Empty
        );
    }

    [Fact]
    public void Shell_Text_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        shell.Text = null!;
        Assert.Equal(string.Empty, shell.Text);
        shell.Dispose();
    }

    [Fact]
    public void Shell_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => shell);
    }

    [Fact]
    public void Shell_Dispose_ShouldDisposeChildren()
    {
        using var shell = CreateTestShell();
        var button = new Button(shell, SWT.PUSH);

        shell.Dispose();

        Assert.True(shell.IsDisposed);
        Assert.True(button.IsDisposed);
    }

    [Fact]
    public void Shell_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => shell,
            s => s.Text = "Test"
        );
    }

    [Fact]
    public void Shell_GetText_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        shell.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = shell.Text);
    }

    [Fact]
    public void Shell_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => shell);
    }

    [Fact]
    public void Shell_Display_ShouldReturnSameDisplay()
    {
        using var shell = CreateTestShell();
        var display1 = shell.Display;
        var display2 = shell.Display;

        Assert.Same(display1, display2);

        shell.Dispose();
    }

    [Fact]
    public void Shell_IsDisposed_InitiallyFalse()
    {
        using var shell = CreateTestShell();
        Assert.False(shell.IsDisposed);
        shell.Dispose();
    }

    [Fact]
    public void Shell_MultipleDispose_ShouldNotThrow()
    {
        var shell = CreateTestShell();
        shell.Dispose();
        shell.Dispose(); // Should not throw

        Assert.True(shell.IsDisposed);
    }

    [Fact]
    public void Shell_CheckWidget_WhileValid_ShouldNotThrow()
    {
        using var shell = CreateTestShell();

        // CheckWidget is called internally by property access
        var text = shell.Text; // Should not throw

        Assert.NotNull(text);
        shell.Dispose();
    }
}
