using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Scale widget.
/// </summary>
public class ScaleTests : WidgetTestBase
{
    public ScaleTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [FactSkipOnMacOSCI]
    public void Scale_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Scale(shell, SWT.NONE));
    }

    [FactSkipOnMacOSCI]
    public void Scale_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Scale(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Minimum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.Minimum,
            (s, v) => s.Minimum = v,
            10
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Maximum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.Maximum,
            (s, v) => s.Maximum = v,
            200
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Selection_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.Selection,
            (s, v) => s.Selection = v,
            50
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Increment_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.Increment,
            (s, v) => s.Increment = v,
            5
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_PageIncrement_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.PageIncrement,
            (s, v) => s.PageIncrement = v,
            10
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Selection_ClampedToMinimum()
    {
        using var shell = CreateTestShell();
        var scale = new Scale(shell, SWT.NONE);

        scale.Minimum = 10;
        scale.Selection = 5;

        Assert.Equal(10, scale.Selection);

        scale.Dispose();
    }

    [FactSkipOnMacOSCI]
    public void Scale_Selection_ClampedToMaximum()
    {
        using var shell = CreateTestShell();
        var scale = new Scale(shell, SWT.NONE);

        scale.Maximum = 100;
        scale.Selection = 150;

        Assert.Equal(100, scale.Selection);

        scale.Dispose();
    }

    [FactSkipOnMacOSCI]
    public void Scale_DefaultValues_ShouldBeCorrect()
    {
        using var shell = CreateTestShell();
        var scale = new Scale(shell, SWT.NONE);

        Assert.Equal(0, scale.Minimum);
        Assert.Equal(100, scale.Maximum);
        Assert.Equal(0, scale.Selection);

        scale.Dispose();
    }

    [FactSkipOnMacOSCI]
    public void Scale_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Scale(shell, SWT.NONE));
    }

    [FactSkipOnMacOSCI]
    public void Scale_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Scale(shell, SWT.NONE));
    }

    [FactSkipOnMacOSCI]
    public void Scale_SetSelection_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Scale(shell, SWT.NONE),
            s => s.Selection = 50
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Scale(shell, SWT.NONE));
    }

    [FactSkipOnMacOSCI]
    public void Scale_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.Visible,
            (s, v) => s.Visible = v,
            false
        );
    }

    [FactSkipOnMacOSCI]
    public void Scale_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Scale(shell, SWT.NONE),
            s => s.Enabled,
            (s, v) => s.Enabled = v,
            false
        );
    }
}
