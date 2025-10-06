using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Tree widget.
/// </summary>
public class TreeTests : WidgetTestBase
{
    [Fact]
    public void Tree_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Tree(shell, SWT.NONE));
    }

    [Fact]
    public void Tree_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Tree(shell, style),
            SWT.NONE,
            SWT.SINGLE,
            SWT.MULTI,
            SWT.CHECK
        );
    }

    [Fact]
    public void Tree_AddRootItem_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);
        var item = new TreeItem(tree, SWT.NONE);

        Assert.NotNull(item);
        Assert.Equal(1, tree.ItemCount);

        tree.Dispose();
    }

    [Fact]
    public void Tree_AddMultipleRootItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);

        var item1 = new TreeItem(tree, SWT.NONE);
        var item2 = new TreeItem(tree, SWT.NONE);
        var item3 = new TreeItem(tree, SWT.NONE);

        Assert.Equal(3, tree.ItemCount);

        tree.Dispose();
    }

    [Fact]
    public void Tree_AddChildItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);

        var parent = new TreeItem(tree, SWT.NONE);
        var child1 = new TreeItem(parent, SWT.NONE);
        var child2 = new TreeItem(parent, SWT.NONE);

        Assert.Equal(2, parent.ItemCount);

        tree.Dispose();
    }

    [Fact]
    public void Tree_GetItem_ShouldReturnCorrectItem()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);

        var item1 = new TreeItem(tree, SWT.NONE);
        var item2 = new TreeItem(tree, SWT.NONE);

        var retrieved = tree.GetItem(1);
        Assert.Same(item2, retrieved);

        tree.Dispose();
    }

    [Fact]
    public void Tree_SelectionIndex_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.SINGLE);

        new TreeItem(tree, SWT.NONE);
        new TreeItem(tree, SWT.NONE);

        tree.SelectionIndex = 1;
        Assert.Equal(1, tree.SelectionIndex);

        tree.Dispose();
    }

    [Fact]
    public void Tree_Selection_ShouldReturnSelectedItem()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.SINGLE);

        var item1 = new TreeItem(tree, SWT.NONE);
        var item2 = new TreeItem(tree, SWT.NONE);

        tree.SelectionIndex = 1;
        Assert.Same(item2, tree.Selection);

        tree.Dispose();
    }

    [Fact]
    public void Tree_Items_ShouldReturnRootItems()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);

        var item1 = new TreeItem(tree, SWT.NONE);
        var item2 = new TreeItem(tree, SWT.NONE);

        var items = tree.Items;
        Assert.Equal(2, items.Length);
        Assert.Contains(item1, items);
        Assert.Contains(item2, items);

        tree.Dispose();
    }

    [Fact]
    public void Tree_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Tree(shell, SWT.NONE));
    }

    [Fact]
    public void Tree_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Tree(shell, SWT.NONE));
    }

    [Fact]
    public void Tree_Dispose_ShouldDisposeItems()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);
        var item = new TreeItem(tree, SWT.NONE);

        tree.Dispose();

        Assert.True(tree.IsDisposed);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void Tree_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Tree(shell, SWT.NONE));
    }

    [Fact]
    public void Tree_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Tree(shell, SWT.NONE),
            t => t.Visible,
            (t, v) => t.Visible = v,
            false
        );
    }

    [Fact]
    public void Tree_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Tree(shell, SWT.NONE),
            t => t.Enabled,
            (t, v) => t.Enabled = v,
            false
        );
    }

    [Fact]
    public void Tree_InitialItemCount_ShouldBeZero()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);

        Assert.Equal(0, tree.ItemCount);

        tree.Dispose();
    }

    [Fact]
    public void Tree_NestedHierarchy_ShouldWork()
    {
        using var shell = CreateTestShell();
        var tree = new Tree(shell, SWT.NONE);

        var root = new TreeItem(tree, SWT.NONE);
        var child = new TreeItem(root, SWT.NONE);
        var grandchild = new TreeItem(child, SWT.NONE);

        Assert.Equal(1, tree.ItemCount);
        Assert.Equal(1, root.ItemCount);
        Assert.Equal(1, child.ItemCount);

        tree.Dispose();
    }
}
