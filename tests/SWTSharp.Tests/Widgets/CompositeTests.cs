using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Composite widget.
/// </summary>
public class CompositeTests : WidgetTestBase
{
    [Fact]
    public void Composite_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Composite(shell, SWT.NONE));
    }

    [Fact]
    public void Composite_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Composite(shell, style),
            SWT.NONE,
            SWT.BORDER,
            SWT.H_SCROLL,
            SWT.V_SCROLL
        );
    }

    [Fact]
    public void Composite_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Composite(shell, SWT.NONE));
    }

    [Fact]
    public void Composite_Dispose_ShouldDisposeChildren()
    {
        using var shell = CreateTestShell();
        var composite = new Composite(shell, SWT.NONE);
        var button = new Button(composite, SWT.PUSH);

        composite.Dispose();

        Assert.True(composite.IsDisposed);
        Assert.True(button.IsDisposed);
    }

    [Fact]
    public void Composite_AddChild_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var composite = new Composite(shell, SWT.NONE);
        var button = new Button(composite, SWT.PUSH);

        Assert.Same(composite, button.Parent);

        button.Dispose();
        composite.Dispose();
    }

    [Fact]
    public void Composite_AddMultipleChildren_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var composite = new Composite(shell, SWT.NONE);
        var button1 = new Button(composite, SWT.PUSH);
        var button2 = new Button(composite, SWT.PUSH);
        var label = new Label(composite, SWT.NONE);

        Assert.Same(composite, button1.Parent);
        Assert.Same(composite, button2.Parent);
        Assert.Same(composite, label.Parent);

        composite.Dispose();
    }

    [Fact]
    public void Composite_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Composite(shell, SWT.NONE),
            c => c.Visible,
            (c, v) => c.Visible = v,
            false
        );
    }

    [Fact]
    public void Composite_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Composite(shell, SWT.NONE),
            c => c.Enabled,
            (c, v) => c.Enabled = v,
            false
        );
    }

    [Fact]
    public void Composite_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Composite(shell, SWT.NONE));
    }

    [Fact]
    public void Composite_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Composite(shell, SWT.NONE));
    }

    [Fact]
    public void Composite_AfterDispose_PropertyAccess_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Composite(shell, SWT.NONE),
            c => c.Visible = true
        );
    }

    [Fact]
    public void Composite_NestedComposites_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var composite1 = new Composite(shell, SWT.NONE);
        var composite2 = new Composite(composite1, SWT.NONE);
        var composite3 = new Composite(composite2, SWT.NONE);

        Assert.Same(shell, composite1.Parent);
        Assert.Same(composite1, composite2.Parent);
        Assert.Same(composite2, composite3.Parent);

        composite1.Dispose();
    }

    [Fact]
    public void Composite_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var composite = new Composite(shell, SWT.NONE);

        Assert.Same(shell.Display, composite.Display);

        composite.Dispose();
    }

    [Fact]
    public void Composite_InitiallyVisible()
    {
        using var shell = CreateTestShell();
        var composite = new Composite(shell, SWT.NONE);

        Assert.True(composite.Visible);

        composite.Dispose();
    }

    [Fact]
    public void Composite_InitiallyEnabled()
    {
        using var shell = CreateTestShell();
        var composite = new Composite(shell, SWT.NONE);

        Assert.True(composite.Enabled);

        composite.Dispose();
    }
}
