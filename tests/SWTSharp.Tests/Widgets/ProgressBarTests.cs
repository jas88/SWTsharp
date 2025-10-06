using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for ProgressBar widget.
/// </summary>
public class ProgressBarTests : WidgetTestBase
{
    [Fact]
    public void ProgressBar_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new ProgressBar(shell, SWT.NONE));
    }

    [Fact]
    public void ProgressBar_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new ProgressBar(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL,
            SWT.INDETERMINATE,
            SWT.SMOOTH
        );
    }

    [Fact]
    public void ProgressBar_Minimum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ProgressBar(shell, SWT.NONE),
            p => p.Minimum,
            (p, v) => p.Minimum = v,
            10
        );
    }

    [Fact]
    public void ProgressBar_Maximum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ProgressBar(shell, SWT.NONE),
            p => p.Maximum,
            (p, v) => p.Maximum = v,
            200
        );
    }

    [Fact]
    public void ProgressBar_Selection_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ProgressBar(shell, SWT.NONE),
            p => p.Selection,
            (p, v) => p.Selection = v,
            50
        );
    }

    [Fact]
    public void ProgressBar_Selection_ClampedToMinimum()
    {
        using var shell = CreateTestShell();
        var progressBar = new ProgressBar(shell, SWT.NONE);

        progressBar.Minimum = 10;
        progressBar.Selection = 5;

        Assert.Equal(10, progressBar.Selection);

        progressBar.Dispose();
    }

    [Fact]
    public void ProgressBar_Selection_ClampedToMaximum()
    {
        using var shell = CreateTestShell();
        var progressBar = new ProgressBar(shell, SWT.NONE);

        progressBar.Maximum = 100;
        progressBar.Selection = 150;

        Assert.Equal(100, progressBar.Selection);

        progressBar.Dispose();
    }

    [Fact]
    public void ProgressBar_DefaultValues_ShouldBeCorrect()
    {
        using var shell = CreateTestShell();
        var progressBar = new ProgressBar(shell, SWT.NONE);

        Assert.Equal(0, progressBar.Minimum);
        Assert.Equal(100, progressBar.Maximum);
        Assert.Equal(0, progressBar.Selection);

        progressBar.Dispose();
    }

    [Fact]
    public void ProgressBar_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new ProgressBar(shell, SWT.NONE));
    }

    [Fact]
    public void ProgressBar_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new ProgressBar(shell, SWT.NONE));
    }

    [Fact]
    public void ProgressBar_SetSelection_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new ProgressBar(shell, SWT.NONE),
            p => p.Selection = 50
        );
    }

    [Fact]
    public void ProgressBar_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new ProgressBar(shell, SWT.NONE));
    }

    [Fact]
    public void ProgressBar_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ProgressBar(shell, SWT.NONE),
            p => p.Visible,
            (p, v) => p.Visible = v,
            false
        );
    }

    [Fact]
    public void ProgressBar_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ProgressBar(shell, SWT.NONE),
            p => p.Enabled,
            (p, v) => p.Enabled = v,
            false
        );
    }

    [Fact]
    public void ProgressBar_RangeChange_ShouldAdjustSelection()
    {
        using var shell = CreateTestShell();
        var progressBar = new ProgressBar(shell, SWT.NONE);

        progressBar.Maximum = 100;
        progressBar.Selection = 50;

        progressBar.Maximum = 40; // Selection should be clamped

        Assert.Equal(40, progressBar.Selection);

        progressBar.Dispose();
    }
}
