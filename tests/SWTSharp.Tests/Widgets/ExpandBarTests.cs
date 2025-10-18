using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for ExpandBar widget.
/// </summary>
public class ExpandBarTests : WidgetTestBase
{
    public ExpandBarTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void ExpandBar_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new ExpandBar(shell, SWT.NONE));
    }

    [Fact]
    public void ExpandBar_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new ExpandBar(shell, style),
            SWT.NONE,
            SWT.V_SCROLL
        );
    }

    [Fact]
    public void ExpandBar_ItemCount_Initial_ShouldBeZero()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            Assert.Equal(0, expandBar.ItemCount);
            Assert.Equal(0, expandBar.GetItemCount());

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_AddItem_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);
            var item = new ExpandItem(expandBar, SWT.NONE);

            Assert.NotNull(item);
            Assert.Equal(1, expandBar.ItemCount);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_AddMultipleItems_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);
            var item3 = new ExpandItem(expandBar, SWT.NONE);

            Assert.Equal(3, expandBar.ItemCount);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_GetItem_ShouldReturnCorrectItem()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);

            var retrieved = expandBar.GetItem(1);
            Assert.Same(item2, retrieved);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_GetItem_InvalidIndex_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            Assert.Throws<ArgumentOutOfRangeException>(() => expandBar.GetItem(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => expandBar.GetItem(-1));

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_GetItems_ShouldReturnAllItems()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);

            var items = expandBar.GetItems();
            Assert.Equal(2, items.Length);
            Assert.Same(item1, items[0]);
            Assert.Same(item2, items[1]);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_Items_Property_ShouldReturnAllItems()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);

            var items = expandBar.Items;
            Assert.Equal(2, items.Length);
            Assert.Same(item1, items[0]);
            Assert.Same(item2, items[1]);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_IndexOf_ShouldReturnCorrectIndex()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);

            Assert.Equal(0, expandBar.IndexOf(item1));
            Assert.Equal(1, expandBar.IndexOf(item2));

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_IndexOf_NotInList_ShouldReturnNegativeOne()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);
            var otherExpandBar = new ExpandBar(shell, SWT.NONE);
            var item = new ExpandItem(otherExpandBar, SWT.NONE);

            Assert.Equal(-1, expandBar.IndexOf(item));

            expandBar.Dispose();
            otherExpandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_IndexOf_Null_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            Assert.Throws<ArgumentNullException>(() => expandBar.IndexOf(null!));

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_Spacing_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ExpandBar(shell, SWT.NONE),
            bar => bar.Spacing,
            (bar, v) => bar.Spacing = v,
            10
        );
    }

    [Fact]
    public void ExpandBar_Spacing_GetSpacing_ShouldReturnValue()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            expandBar.Spacing = 15;
            Assert.Equal(15, expandBar.GetSpacing());

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_Spacing_SetSpacing_ShouldUpdateValue()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            expandBar.SetSpacing(20);
            Assert.Equal(20, expandBar.Spacing);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_Spacing_Negative_ShouldClampToZero()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);

            expandBar.Spacing = -5;
            Assert.Equal(0, expandBar.Spacing);

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new ExpandBar(shell, SWT.NONE));
    }

    [Fact]
    public void ExpandBar_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new ExpandBar(shell, SWT.NONE));
    }

    [Fact]
    public void ExpandBar_Dispose_ShouldDisposeItems()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);
            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);

            expandBar.Dispose();

            Assert.True(expandBar.IsDisposed);
            Assert.True(item1.IsDisposed);
            Assert.True(item2.IsDisposed);
        });
    }

    [Fact]
    public void ExpandBar_ItemDispose_ShouldRemoveFromBar()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);
            var item1 = new ExpandItem(expandBar, SWT.NONE);
            var item2 = new ExpandItem(expandBar, SWT.NONE);

            Assert.Equal(2, expandBar.ItemCount);

            item1.Dispose();

            Assert.Equal(1, expandBar.ItemCount);
            Assert.Same(item2, expandBar.GetItem(0));

            expandBar.Dispose();
        });
    }

    [Fact]
    public void ExpandBar_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new ExpandBar(shell, SWT.NONE));
    }

    [Fact]
    public void ExpandBar_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ExpandBar(shell, SWT.NONE),
            bar => bar.Visible,
            (bar, v) => bar.Visible = v,
            false
        );
    }

    [Fact]
    public void ExpandBar_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ExpandBar(shell, SWT.NONE),
            bar => bar.Enabled,
            (bar, v) => bar.Enabled = v,
            false
        );
    }

    [Fact]
    public void ExpandBar_GetItemCount_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);
            expandBar.Dispose();

            Assert.Throws<SWTDisposedException>(() => expandBar.GetItemCount());
        });
    }

    [Fact]
    public void ExpandBar_GetItems_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var expandBar = new ExpandBar(shell, SWT.NONE);
            expandBar.Dispose();

            Assert.Throws<SWTDisposedException>(() => expandBar.GetItems());
        });
    }
}
