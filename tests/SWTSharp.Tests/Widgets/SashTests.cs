using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Sash widget.
/// </summary>
public class SashTests : WidgetTestBase
{
    public SashTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Sash_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Sash(shell, SWT.NONE));
    }

    [Fact]
    public void Sash_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Sash(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL
        );
    }

    [Fact]
    public void Sash_Create_Horizontal_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Sash(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void Sash_Create_Vertical_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Sash(shell, SWT.VERTICAL));
    }

    [Fact]
    public void Sash_DefaultStyle_ShouldBeHorizontal()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.NONE);

            // When neither HORIZONTAL nor VERTICAL is specified, HORIZONTAL should be used
            Assert.True((sash.Style & SWT.HORIZONTAL) != 0);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_Position_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Sash(shell, SWT.HORIZONTAL),
            s => s.Position,
            (s, v) => s.Position = v,
            100
        );
    }

    [Fact]
    public void Sash_Position_MultipleUpdates_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            sash.Position = 50;
            Assert.Equal(50, sash.Position);

            sash.Position = 100;
            Assert.Equal(100, sash.Position);

            sash.Position = 0;
            Assert.Equal(0, sash.Position);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_SetPosition_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            sash.SetPosition(75);
            Assert.Equal(75, sash.GetPosition());

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_GetPosition_ShouldReturnCurrentPosition()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.VERTICAL);

            sash.Position = 200;
            Assert.Equal(200, sash.GetPosition());

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_InitialPosition_ShouldBeZero()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            Assert.Equal(0, sash.Position);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_Bounds_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            sash.SetBounds(10, 20, 100, 5);

            var bounds = sash.GetBounds();
            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(100, bounds.Width);
            Assert.Equal(5, bounds.Height);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_Size_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.VERTICAL);

            sash.SetSize(5, 200);

            var size = sash.GetSize();
            Assert.Equal(5, size.X);
            Assert.Equal(200, size.Y);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Sash(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void Sash_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Sash(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void Sash_SetPosition_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Sash(shell, SWT.HORIZONTAL),
            s => s.Position = 100
        );
    }

    [Fact]
    public void Sash_GetPosition_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);
            sash.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = sash.Position);
        });
    }

    [Fact]
    public void Sash_SetPositionMethod_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Sash(shell, SWT.HORIZONTAL),
            s => s.SetPosition(50)
        );
    }

    [Fact]
    public void Sash_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Sash(shell, SWT.HORIZONTAL));
    }

    [Fact]
    public void Sash_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Sash(shell, SWT.HORIZONTAL),
            s => s.Visible,
            (s, v) => s.Visible = v,
            false
        );
    }

    [Fact]
    public void Sash_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Sash(shell, SWT.HORIZONTAL),
            s => s.Enabled,
            (s, v) => s.Enabled = v,
            false
        );
    }

    [Fact]
    public void Sash_SashMoved_EventHandler_ShouldNotCrash()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            bool eventFired = false;
            sash.SashMoved += (sender, e) =>
            {
                eventFired = true;
            };

            // We can't programmatically trigger the event easily,
            // but we can verify the handler is attached without crashing
            Assert.False(eventFired);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_Display_ShouldMatchParent()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            Assert.Same(shell.Display, sash.Display);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_HorizontalSash_StyleCheck()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.HORIZONTAL);

            Assert.True((sash.Style & SWT.HORIZONTAL) != 0);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_VerticalSash_StyleCheck()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash = new Sash(shell, SWT.VERTICAL);

            Assert.True((sash.Style & SWT.VERTICAL) != 0);

            sash.Dispose();
        });
    }

    [Fact]
    public void Sash_MultipleInstances_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var sash1 = new Sash(shell, SWT.HORIZONTAL);
            var sash2 = new Sash(shell, SWT.VERTICAL);

            sash1.Position = 50;
            sash2.Position = 100;

            Assert.Equal(50, sash1.Position);
            Assert.Equal(100, sash2.Position);

            sash1.Dispose();
            sash2.Dispose();
        });
    }
}
