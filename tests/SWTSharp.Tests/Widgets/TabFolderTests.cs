using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for TabFolder widget.
/// </summary>
public class TabFolderTests : WidgetTestBase
{
    public TabFolderTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void TabFolder_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new TabFolder(shell, SWT.NONE));
    }

    [Fact]
    public void TabFolder_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new TabFolder(shell, style),
            SWT.NONE,
            SWT.TOP,
            SWT.BOTTOM
        );
    }

    [Fact]
    public void TabFolder_AddTabItem_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);
            var tabItem = new TabItem(tabFolder, SWT.NONE);

            Assert.NotNull(tabItem);
            Assert.Equal(1, tabFolder.ItemCount);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_AddMultipleTabItems_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);

            var tab1 = new TabItem(tabFolder, SWT.NONE);
            var tab2 = new TabItem(tabFolder, SWT.NONE);
            var tab3 = new TabItem(tabFolder, SWT.NONE);

            Assert.Equal(3, tabFolder.ItemCount);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_GetItem_ShouldReturnCorrectItem()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);

            var tab1 = new TabItem(tabFolder, SWT.NONE);
            var tab2 = new TabItem(tabFolder, SWT.NONE);

            var retrieved = tabFolder.GetItem(1);
            Assert.Same(tab2, retrieved);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_SelectionIndex_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);

            new TabItem(tabFolder, SWT.NONE);
            new TabItem(tabFolder, SWT.NONE);

            tabFolder.SelectionIndex = 1;
            Assert.Equal(1, tabFolder.SelectionIndex);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_Selection_ShouldReturnSelectedItem()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);

            var tab1 = new TabItem(tabFolder, SWT.NONE);
            var tab2 = new TabItem(tabFolder, SWT.NONE);

            tabFolder.SelectionIndex = 1;
            Assert.Same(tab2, tabFolder.Selection);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_Items_ShouldReturnAllItems()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);

            var tab1 = new TabItem(tabFolder, SWT.NONE);
            var tab2 = new TabItem(tabFolder, SWT.NONE);

            var items = tabFolder.Items;
            Assert.Equal(2, items.Length);
            Assert.Contains(tab1, items);
            Assert.Contains(tab2, items);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new TabFolder(shell, SWT.NONE));
    }

    [Fact]
    public void TabFolder_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new TabFolder(shell, SWT.NONE));
    }

    [Fact]
    public void TabFolder_Dispose_ShouldDisposeItems()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);
            var tabItem = new TabItem(tabFolder, SWT.NONE);

            tabFolder.Dispose();

            Assert.True(tabFolder.IsDisposed);
            Assert.True(tabItem.IsDisposed);
        });
    }

    [Fact]
    public void TabFolder_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new TabFolder(shell, SWT.NONE));
    }

    [Fact]
    public void TabFolder_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new TabFolder(shell, SWT.NONE),
            t => t.Visible,
            (t, v) => t.Visible = v,
            false
        );
    }

    [Fact]
    public void TabFolder_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new TabFolder(shell, SWT.NONE),
            t => t.Enabled,
            (t, v) => t.Enabled = v,
            false
        );
    }

    [Fact]
    public void TabFolder_InitialItemCount_ShouldBeZero()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tabFolder = new TabFolder(shell, SWT.NONE);

            Assert.Equal(0, tabFolder.ItemCount);

            tabFolder.Dispose();
        });
    }

    [Fact]
    public void TabFolder_SetSelectionIndex_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new TabFolder(shell, SWT.NONE),
            t => t.SelectionIndex = 0
        );
    }
}
