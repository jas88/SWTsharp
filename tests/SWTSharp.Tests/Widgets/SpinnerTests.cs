using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Spinner widget.
/// </summary>
public class SpinnerTests : WidgetTestBase
{
    [Fact]
    public void Spinner_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Spinner(shell, SWT.NONE));
    }

    [Fact]
    public void Spinner_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Spinner(shell, style),
            SWT.NONE,
            SWT.READ_ONLY,
            SWT.WRAP
        );
    }

    [Fact]
    public void Spinner_Minimum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Minimum,
            (s, v) => s.Minimum = v,
            10
        );
    }

    [Fact]
    public void Spinner_Maximum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Maximum,
            (s, v) => s.Maximum = v,
            200
        );
    }

    [Fact]
    public void Spinner_Selection_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Selection,
            (s, v) => s.Selection = v,
            50
        );
    }

    [Fact]
    public void Spinner_Increment_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Increment,
            (s, v) => s.Increment = v,
            5
        );
    }

    [Fact]
    public void Spinner_PageIncrement_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.PageIncrement,
            (s, v) => s.PageIncrement = v,
            10
        );
    }

    [Fact]
    public void Spinner_Selection_ClampedToMinimum()
    {
        using var shell = CreateTestShell();
        var spinner = new Spinner(shell, SWT.NONE);

        spinner.Minimum = 10;
        spinner.Selection = 5;

        Assert.Equal(10, spinner.Selection);

        spinner.Dispose();
    }

    [Fact]
    public void Spinner_Selection_ClampedToMaximum()
    {
        using var shell = CreateTestShell();
        var spinner = new Spinner(shell, SWT.NONE);

        spinner.Maximum = 100;
        spinner.Selection = 150;

        Assert.Equal(100, spinner.Selection);

        spinner.Dispose();
    }

    [Fact]
    public void Spinner_DefaultValues_ShouldBeCorrect()
    {
        using var shell = CreateTestShell();
        var spinner = new Spinner(shell, SWT.NONE);

        Assert.Equal(0, spinner.Minimum);
        Assert.Equal(100, spinner.Maximum);
        Assert.Equal(0, spinner.Selection);

        spinner.Dispose();
    }

    [Fact]
    public void Spinner_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Spinner(shell, SWT.NONE));
    }

    [Fact]
    public void Spinner_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Spinner(shell, SWT.NONE));
    }

    [Fact]
    public void Spinner_SetSelection_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Selection = 50
        );
    }

    [Fact]
    public void Spinner_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Spinner(shell, SWT.NONE));
    }

    [Fact]
    public void Spinner_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Visible,
            (s, v) => s.Visible = v,
            false
        );
    }

    [Fact]
    public void Spinner_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Spinner(shell, SWT.NONE),
            s => s.Enabled,
            (s, v) => s.Enabled = v,
            false
        );
    }
}
