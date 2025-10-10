using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Slider widget.
/// </summary>
public class SliderTests : WidgetTestBase
{
    public SliderTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Slider_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Slider(shell, SWT.NONE));
    }

    [Fact]
    public void Slider_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Slider(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL
        );
    }

    [Fact]
    public void Slider_Minimum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.Minimum,
            (s, v) => s.Minimum = v,
            10
        );
    }

    [Fact]
    public void Slider_Maximum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.Maximum,
            (s, v) => s.Maximum = v,
            200
        );
    }

    [Fact]
    public void Slider_Selection_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.Selection,
            (s, v) => s.Selection = v,
            50
        );
    }

    [Fact]
    public void Slider_Increment_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.Increment,
            (s, v) => s.Increment = v,
            5
        );
    }

    [Fact]
    public void Slider_PageIncrement_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.PageIncrement,
            (s, v) => s.PageIncrement = v,
            10
        );
    }

    [Fact]
    public void Slider_Thumb_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.Thumb,
            (s, v) => s.Thumb = v,
            20
        );
    }

    [Fact]
    public void Slider_Selection_ClampedToMinimum()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var slider = new Slider(shell, SWT.NONE);

            slider.Minimum = 10;
            slider.Selection = 5;

            Assert.Equal(10, slider.Selection);

            slider.Dispose();
        });
    }

    [Fact]
    public void Slider_Selection_ClampedToMaximum()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var slider = new Slider(shell, SWT.NONE);

            slider.Maximum = 100;
            slider.Selection = 150;

            Assert.Equal(100, slider.Selection);

            slider.Dispose();
        });
    }

    [Fact]
    public void Slider_DefaultValues_ShouldBeCorrect()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var slider = new Slider(shell, SWT.NONE);

            Assert.Equal(0, slider.Minimum);
            Assert.Equal(100, slider.Maximum);
            Assert.Equal(0, slider.Selection);

            slider.Dispose();
        });
    }

    [Fact]
    public void Slider_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Slider(shell, SWT.NONE));
    }

    [Fact]
    public void Slider_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Slider(shell, SWT.NONE));
    }

    [Fact]
    public void Slider_SetSelection_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Slider(shell, SWT.NONE),
            s => s.Selection = 50
        );
    }

    [Fact]
    public void Slider_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Slider(shell, SWT.NONE));
    }

    [Fact]
    public void Slider_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Slider(shell, SWT.NONE),
            s => s.Visible,
            (s, v) => s.Visible = v,
            false
        );
    }
}
