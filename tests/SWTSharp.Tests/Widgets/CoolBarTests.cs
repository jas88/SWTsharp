using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for CoolBar widget.
/// </summary>
public class CoolBarTests : WidgetTestBase
{
    public CoolBarTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void CoolBar_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new CoolBar(shell, SWT.NONE));
    }

    [Fact]
    public void CoolBar_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new CoolBar(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL,
            SWT.FLAT,
            SWT.HORIZONTAL | SWT.FLAT,
            SWT.VERTICAL | SWT.FLAT
        );
    }

    [Fact]
    public void CoolBar_DefaultStyle_ShouldBeHorizontalAndFlat()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            Assert.True(coolBar.IsHorizontal, "Default coolbar should be HORIZONTAL");
            Assert.True(coolBar.IsFlat, "Default coolbar should be FLAT");

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_HorizontalStyle_ShouldBeHorizontal()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.HORIZONTAL);

            Assert.True(coolBar.IsHorizontal);
            Assert.False(coolBar.IsVertical);

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_VerticalStyle_ShouldBeVertical()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.VERTICAL);

            Assert.True(coolBar.IsVertical);
            Assert.False(coolBar.IsHorizontal);

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_InitialItemCount_ShouldBeZero()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            Assert.Equal(0, coolBar.ItemCount);
            Assert.Equal(0, coolBar.GetItemCount());
            Assert.Empty(coolBar.Items);

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_AddItem_ShouldIncreaseCount()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            var item = new CoolItem(coolBar, SWT.NONE);

            Assert.Equal(1, coolBar.ItemCount);
            Assert.Single(coolBar.Items);

            item.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_AddMultipleItems_ShouldIncreaseCount()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            var item1 = new CoolItem(coolBar, SWT.NONE);
            var item2 = new CoolItem(coolBar, SWT.NONE);
            var item3 = new CoolItem(coolBar, SWT.NONE);

            Assert.Equal(3, coolBar.ItemCount);
            Assert.Equal(3, coolBar.Items.Length);

            item3.Dispose();
            item2.Dispose();
            item1.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_GetItem_ShouldReturnCorrectItem()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            var item1 = new CoolItem(coolBar, SWT.NONE);
            var item2 = new CoolItem(coolBar, SWT.NONE);

            Assert.Same(item1, coolBar.GetItem(0));
            Assert.Same(item2, coolBar.GetItem(1));

            item2.Dispose();
            item1.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_GetItem_InvalidIndex_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            var item = new CoolItem(coolBar, SWT.NONE);

            Assert.Throws<ArgumentOutOfRangeException>(() => coolBar.GetItem(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => coolBar.GetItem(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => coolBar.GetItem(100));

            item.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_IndexOf_ShouldReturnCorrectIndex()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            var item1 = new CoolItem(coolBar, SWT.NONE);
            var item2 = new CoolItem(coolBar, SWT.NONE);

            Assert.Equal(0, coolBar.IndexOf(item1));
            Assert.Equal(1, coolBar.IndexOf(item2));

            item2.Dispose();
            item1.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_IndexOf_WithNull_ShouldReturnMinusOne()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            Assert.Equal(-1, coolBar.IndexOf(null!));

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_IndexOf_ItemNotInCoolBar_ShouldReturnMinusOne()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar1 = new CoolBar(shell, SWT.NONE);
            var coolBar2 = new CoolBar(shell, SWT.NONE);
            var item = new CoolItem(coolBar2, SWT.NONE);

            Assert.Equal(-1, coolBar1.IndexOf(item));

            item.Dispose();
            coolBar2.Dispose();
            coolBar1.Dispose();
        });
    }

    [Fact]
    public void CoolBar_Locked_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new CoolBar(shell, SWT.NONE),
            cb => cb.Locked,
            (cb, v) => cb.Locked = v,
            true
        );
    }

    [Fact]
    public void CoolBar_Locked_InitialValue_ShouldBeFalse()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            Assert.False(coolBar.Locked);

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_SetLocked_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            coolBar.SetLocked(true);
            Assert.True(coolBar.Locked);

            coolBar.SetLocked(false);
            Assert.False(coolBar.Locked);

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new CoolBar(shell, SWT.NONE));
    }

    [Fact]
    public void CoolBar_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new CoolBar(shell, SWT.NONE));
    }

    [Fact]
    public void CoolBar_Dispose_ShouldDisposeItems()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            var item1 = new CoolItem(coolBar, SWT.NONE);
            var item2 = new CoolItem(coolBar, SWT.NONE);

            coolBar.Dispose();

            Assert.True(item1.IsDisposed);
            Assert.True(item2.IsDisposed);
        });
    }

    [Fact]
    public void CoolBar_GetItemCount_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            coolBar.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = coolBar.ItemCount);
        });
    }

    [Fact]
    public void CoolBar_GetItems_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            coolBar.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = coolBar.Items);
        });
    }

    [Fact]
    public void CoolBar_SetLocked_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new CoolBar(shell, SWT.NONE),
            cb => cb.Locked = true
        );
    }

    [Fact]
    public void CoolBar_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new CoolBar(shell, SWT.NONE));
    }

    [Fact]
    public void CoolBar_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new CoolBar(shell, SWT.NONE),
            cb => cb.Visible,
            (cb, v) => cb.Visible = v,
            false
        );
    }

    [Fact]
    public void CoolBar_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new CoolBar(shell, SWT.NONE),
            cb => cb.Enabled,
            (cb, v) => cb.Enabled = v,
            false
        );
    }

    [Fact]
    public void CoolBar_Display_ShouldMatchParent()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);

            Assert.Same(shell.Display, coolBar.Display);

            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_ItemWithControl_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            var button = new Button(shell, SWT.PUSH);
            var item = new CoolItem(coolBar, SWT.NONE);

            item.Control = button;

            Assert.Same(button, item.Control);

            item.Dispose();
            button.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_ItemPreferredSize_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            var item = new CoolItem(coolBar, SWT.NONE);

            item.SetPreferredSize(100, 50);

            Assert.Equal(100, item.PreferredWidth);
            Assert.Equal(50, item.PreferredHeight);

            item.Dispose();
            coolBar.Dispose();
        });
    }

    [Fact]
    public void CoolBar_ItemMinimumSize_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var coolBar = new CoolBar(shell, SWT.NONE);
            var item = new CoolItem(coolBar, SWT.NONE);

            item.SetMinimumSize(50, 25);

            Assert.Equal(50, item.MinimumWidth);
            Assert.Equal(25, item.MinimumHeight);

            item.Dispose();
            coolBar.Dispose();
        });
    }
}
