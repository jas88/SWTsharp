using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Control base class using Button as concrete implementation.
/// </summary>
public class ControlTests : WidgetTestBase
{
    [Fact]
    public void Control_Visible_DefaultTrue()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);

        Assert.True(control.Visible);

        control.Dispose();
    }

    [Fact]
    public void Control_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Button(shell, SWT.PUSH),
            c => c.Visible,
            (c, v) => c.Visible = v,
            false
        );
    }

    [Fact]
    public void Control_Enabled_DefaultTrue()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);

        Assert.True(control.Enabled);

        control.Dispose();
    }

    [Fact]
    public void Control_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Button(shell, SWT.PUSH),
            c => c.Enabled,
            (c, v) => c.Enabled = v,
            false
        );
    }

    [Fact]
    public void Control_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Control_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);

        Assert.Same(shell.Display, control.Display);

        control.Dispose();
    }

    [Fact]
    public void Control_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Control_SetVisible_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Button(shell, SWT.PUSH),
            c => c.Visible = true
        );
    }

    [Fact]
    public void Control_SetEnabled_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Button(shell, SWT.PUSH),
            c => c.Enabled = true
        );
    }

    [Fact]
    public void Control_GetVisible_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);
        control.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = control.Visible);
    }

    [Fact]
    public void Control_GetEnabled_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);
        control.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = control.Enabled);
    }

    [Fact]
    public void Control_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Button(shell, SWT.PUSH));
    }

    [Fact]
    public void Control_ParentDispose_ShouldDisposeControl()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);

        shell.Dispose();

        Assert.True(control.IsDisposed);
    }

    [Fact]
    public void Control_VisibleFalse_ThenTrue_ShouldWork()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);

        control.Visible = false;
        Assert.False(control.Visible);

        control.Visible = true;
        Assert.True(control.Visible);

        control.Dispose();
    }

    [Fact]
    public void Control_EnabledFalse_ThenTrue_ShouldWork()
    {
        using var shell = CreateTestShell();
        var control = new Button(shell, SWT.PUSH);

        control.Enabled = false;
        Assert.False(control.Enabled);

        control.Enabled = true;
        Assert.True(control.Enabled);

        control.Dispose();
    }
}
