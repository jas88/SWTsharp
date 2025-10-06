using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Canvas widget.
/// </summary>
public class CanvasTests : WidgetTestBase
{
    [Fact]
    public void Canvas_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Canvas(shell, SWT.NONE));
    }

    [Fact]
    public void Canvas_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Canvas(shell, style),
            SWT.NONE,
            SWT.BORDER
        );
    }

    [Fact]
    public void Canvas_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Canvas(shell, SWT.NONE));
    }

    [Fact]
    public void Canvas_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Canvas(shell, SWT.NONE));
    }

    [Fact]
    public void Canvas_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Canvas(shell, SWT.NONE));
    }

    [Fact]
    public void Canvas_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Canvas(shell, SWT.NONE),
            c => c.Visible,
            (c, v) => c.Visible = v,
            false
        );
    }

    [Fact]
    public void Canvas_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Canvas(shell, SWT.NONE),
            c => c.Enabled,
            (c, v) => c.Enabled = v,
            false
        );
    }

    [Fact]
    public void Canvas_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);

        Assert.Same(shell.Display, canvas.Display);

        canvas.Dispose();
    }

    [Fact]
    public void Canvas_SetVisible_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Canvas(shell, SWT.NONE),
            c => c.Visible = true
        );
    }

    [Fact]
    public void Canvas_GetVisible_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);
        canvas.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = canvas.Visible);
    }

    [Fact]
    public void Canvas_InitiallyVisible()
    {
        using var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);

        Assert.True(canvas.Visible);

        canvas.Dispose();
    }

    [Fact]
    public void Canvas_InitiallyEnabled()
    {
        using var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);

        Assert.True(canvas.Enabled);

        canvas.Dispose();
    }

    [Fact]
    public void Canvas_ParentDispose_ShouldDisposeCanvas()
    {
        using var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);

        shell.Dispose();

        Assert.True(canvas.IsDisposed);
    }

    [Fact]
    public void Canvas_MultipleDispose_ShouldNotThrow()
    {
        var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);

        canvas.Dispose();
        canvas.Dispose(); // Should not throw

        Assert.True(canvas.IsDisposed);
        shell.Dispose();
    }

    [Fact]
    public void Canvas_IsComposite_ShouldAcceptChildren()
    {
        using var shell = CreateTestShell();
        var canvas = new Canvas(shell, SWT.NONE);
        var button = new Button(canvas, SWT.PUSH);

        Assert.Same(canvas, button.Parent);

        canvas.Dispose();
    }
}
