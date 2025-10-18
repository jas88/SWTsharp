using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for ScrollBar widget.
/// </summary>
public class ScrollBarTests : WidgetTestBase
{
    public ScrollBarTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void ScrollBar_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new ScrollBar(shell, SWT.NONE));
    }

    [Fact]
    public void ScrollBar_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new ScrollBar(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL
        );
    }

    [Fact]
    public void ScrollBar_Create_Horizontal_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new ScrollBar(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void ScrollBar_Create_Vertical_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new ScrollBar(shell, SWT.VERTICAL));
    }

    [Fact]
    public void ScrollBar_DefaultStyle_ShouldBeHorizontal()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            // When neither HORIZONTAL nor VERTICAL is specified, HORIZONTAL should be used
            Assert.True((scrollBar.Style & SWT.HORIZONTAL) != 0);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_DefaultValues_ShouldBeCorrect()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            Assert.Equal(0, scrollBar.Minimum);
            Assert.Equal(100, scrollBar.Maximum);
            Assert.Equal(0, scrollBar.Selection);
            Assert.Equal(1, scrollBar.Increment);
            Assert.Equal(10, scrollBar.PageIncrement);
            Assert.Equal(10, scrollBar.Thumb);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Minimum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Minimum,
            (sb, v) => sb.Minimum = v,
            10
        );
    }

    [Fact]
    public void ScrollBar_Maximum_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Maximum,
            (sb, v) => sb.Maximum = v,
            200
        );
    }

    [Fact]
    public void ScrollBar_Selection_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Selection,
            (sb, v) => sb.Selection = v,
            50
        );
    }

    [Fact]
    public void ScrollBar_Increment_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Increment,
            (sb, v) => sb.Increment = v,
            5
        );
    }

    [Fact]
    public void ScrollBar_PageIncrement_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.PageIncrement,
            (sb, v) => sb.PageIncrement = v,
            20
        );
    }

    [Fact]
    public void ScrollBar_Thumb_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Thumb,
            (sb, v) => sb.Thumb = v,
            15
        );
    }

    [Fact]
    public void ScrollBar_Selection_ClampedToMinimum()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Minimum = 10;
            scrollBar.Selection = 5;

            Assert.Equal(10, scrollBar.Selection);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Selection_ClampedToMaximum()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Maximum = 100;
            scrollBar.Selection = 150;

            Assert.Equal(100, scrollBar.Selection);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_SetSelection_ShouldClampValue()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Minimum = 0;
            scrollBar.Maximum = 100;

            scrollBar.SetSelection(-10);
            Assert.Equal(0, scrollBar.Selection);

            scrollBar.SetSelection(150);
            Assert.Equal(100, scrollBar.Selection);

            scrollBar.SetSelection(50);
            Assert.Equal(50, scrollBar.Selection);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_GetSelection_ShouldReturnCurrentValue()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Selection = 75;
            Assert.Equal(75, scrollBar.GetSelection());

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_SetValues_ShouldUpdateAllProperties()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.SetValues(50, 0, 200, 20, 2, 15);

            Assert.Equal(50, scrollBar.Selection);
            Assert.Equal(0, scrollBar.Minimum);
            Assert.Equal(200, scrollBar.Maximum);
            Assert.Equal(20, scrollBar.Thumb);
            Assert.Equal(2, scrollBar.Increment);
            Assert.Equal(15, scrollBar.PageIncrement);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_SetValues_ShouldClampSelection()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            // Selection outside range should be clamped
            scrollBar.SetValues(250, 10, 100, 10, 1, 10);

            Assert.Equal(100, scrollBar.Selection); // Clamped to maximum

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_IsEnabled_WhenMaximumGreaterThanMinimum()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Minimum = 0;
            scrollBar.Maximum = 100;

            Assert.True(scrollBar.IsEnabled);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_IsEnabled_WhenMaximumEqualsMinimum()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Minimum = 50;
            scrollBar.Maximum = 50;

            Assert.False(scrollBar.IsEnabled);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Increment_IgnoresNonPositiveValues()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Increment = 5;
            Assert.Equal(5, scrollBar.Increment);

            scrollBar.Increment = 0;
            Assert.Equal(5, scrollBar.Increment); // Should remain unchanged

            scrollBar.Increment = -1;
            Assert.Equal(5, scrollBar.Increment); // Should remain unchanged

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_PageIncrement_IgnoresNonPositiveValues()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.PageIncrement = 20;
            Assert.Equal(20, scrollBar.PageIncrement);

            scrollBar.PageIncrement = 0;
            Assert.Equal(20, scrollBar.PageIncrement); // Should remain unchanged

            scrollBar.PageIncrement = -5;
            Assert.Equal(20, scrollBar.PageIncrement); // Should remain unchanged

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Thumb_IgnoresNonPositiveValues()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Thumb = 15;
            Assert.Equal(15, scrollBar.Thumb);

            scrollBar.Thumb = 0;
            Assert.Equal(15, scrollBar.Thumb); // Should remain unchanged

            scrollBar.Thumb = -10;
            Assert.Equal(15, scrollBar.Thumb); // Should remain unchanged

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_MinimumChange_AdjustsSelectionIfNeeded()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Selection = 20;
            scrollBar.Minimum = 50; // Setting minimum above current selection

            Assert.Equal(50, scrollBar.Selection); // Selection should be adjusted

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_MaximumChange_AdjustsSelectionIfNeeded()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);

            scrollBar.Selection = 80;
            scrollBar.Maximum = 50; // Setting maximum below current selection

            Assert.Equal(50, scrollBar.Selection); // Selection should be adjusted

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Bounds_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.HORIZONTAL);

            scrollBar.SetBounds(10, 20, 200, 20);

            var bounds = scrollBar.GetBounds();
            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(200, bounds.Width);
            Assert.Equal(20, bounds.Height);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Size_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.VERTICAL);

            scrollBar.SetSize(20, 300);

            var size = scrollBar.GetSize();
            Assert.Equal(20, size.X);
            Assert.Equal(300, size.Y);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new ScrollBar(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void ScrollBar_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new ScrollBar(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void ScrollBar_SetSelection_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Selection = 50
        );
    }

    [Fact]
    public void ScrollBar_GetSelection_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.NONE);
            scrollBar.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = scrollBar.Selection);
        });
    }

    [Fact]
    public void ScrollBar_SetMinimum_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Minimum = 10
        );
    }

    [Fact]
    public void ScrollBar_SetMaximum_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new ScrollBar(shell, SWT.NONE),
            sb => sb.Maximum = 200
        );
    }

    [Fact]
    public void ScrollBar_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new ScrollBar(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void ScrollBar_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.HORIZONTAL),
            sb => sb.Visible,
            (sb, v) => sb.Visible = v,
            false
        );
    }

    [Fact]
    public void ScrollBar_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ScrollBar(shell, SWT.HORIZONTAL),
            sb => sb.Enabled,
            (sb, v) => sb.Enabled = v,
            false
        );
    }

    [Fact]
    public void ScrollBar_Display_ShouldMatchParent()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.HORIZONTAL);

            Assert.Same(shell.Display, scrollBar.Display);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_HorizontalScrollBar_StyleCheck()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.HORIZONTAL);

            Assert.True((scrollBar.Style & SWT.HORIZONTAL) != 0);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_VerticalScrollBar_StyleCheck()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar = new ScrollBar(shell, SWT.VERTICAL);

            Assert.True((scrollBar.Style & SWT.VERTICAL) != 0);

            scrollBar.Dispose();
        });
    }

    [Fact]
    public void ScrollBar_MultipleInstances_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var scrollBar1 = new ScrollBar(shell, SWT.HORIZONTAL);
            var scrollBar2 = new ScrollBar(shell, SWT.VERTICAL);

            scrollBar1.Selection = 25;
            scrollBar2.Selection = 75;

            Assert.Equal(25, scrollBar1.Selection);
            Assert.Equal(75, scrollBar2.Selection);

            scrollBar1.Dispose();
            scrollBar2.Dispose();
        });
    }
}
