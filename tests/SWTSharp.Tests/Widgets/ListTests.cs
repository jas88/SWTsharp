using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for List widget.
/// </summary>
public class ListTests : WidgetTestBase
{
    [Fact]
    public void List_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new List(shell, SWT.NONE));
    }

    [Fact]
    public void List_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new List(shell, style),
            SWT.NONE,
            SWT.SINGLE,
            SWT.MULTI
        );
    }

    [Fact]
    public void List_Add_SingleItem_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);

        list.Add("Item 1");

        Assert.Equal(1, list.ItemCount);
        Assert.Contains("Item 1", list.Items);

        list.Dispose();
    }

    [Fact]
    public void List_Add_MultipleItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);

        list.Add("Item 1");
        list.Add("Item 2");
        list.Add("Item 3");

        Assert.Equal(3, list.ItemCount);
        Assert.Contains("Item 1", list.Items);
        Assert.Contains("Item 2", list.Items);
        Assert.Contains("Item 3", list.Items);

        list.Dispose();
    }

    [Fact]
    public void List_Remove_ShouldRemoveItem()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);

        list.Add("Item 1");
        list.Add("Item 2");
        list.Remove(0);

        Assert.Equal(1, list.ItemCount);
        Assert.Contains("Item 2", list.Items);
        Assert.DoesNotContain("Item 1", list.Items);

        list.Dispose();
    }

    [Fact]
    public void List_RemoveAll_ShouldClearList()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);

        list.Add("Item 1");
        list.Add("Item 2");
        list.RemoveAll();

        Assert.Equal(0, list.ItemCount);
        Assert.Empty(list.Items);

        list.Dispose();
    }

    [Fact]
    public void List_SelectionIndex_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.SINGLE);

        list.Add("Item 1");
        list.Add("Item 2");

        list.SelectionIndex = 1;

        Assert.Equal(1, list.SelectionIndex);

        list.Dispose();
    }

    [Fact]
    public void List_SelectionIndices_MultiSelect_ShouldWork()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.MULTI);

        list.Add("Item 1");
        list.Add("Item 2");
        list.Add("Item 3");

        list.Select(0);
        list.Select(2);

        var selections = list.SelectionIndices;
        Assert.Contains(0, selections);
        Assert.Contains(2, selections);

        list.Dispose();
    }

    [Fact]
    public void List_Items_ShouldReturnAllItems()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);

        list.Add("A");
        list.Add("B");
        list.Add("C");

        var items = list.Items;
        Assert.Equal(3, items.Length);
        Assert.Equal("A", items[0]);
        Assert.Equal("B", items[1]);
        Assert.Equal("C", items[2]);

        list.Dispose();
    }

    [Fact]
    public void List_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new List(shell, SWT.NONE));
    }

    [Fact]
    public void List_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new List(shell, SWT.NONE));
    }

    [Fact]
    public void List_Add_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);
        list.Dispose();

        Assert.Throws<SWTDisposedException>(() => list.Add("Item"));
    }

    [Fact]
    public void List_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new List(shell, SWT.NONE));
    }

    [Fact]
    public void List_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new List(shell, SWT.NONE),
            l => l.Visible,
            (l, v) => l.Visible = v,
            false
        );
    }

    [Fact]
    public void List_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new List(shell, SWT.NONE),
            l => l.Enabled,
            (l, v) => l.Enabled = v,
            false
        );
    }

    [Fact]
    public void List_InitialItemCount_ShouldBeZero()
    {
        using var shell = CreateTestShell();
        var list = new List(shell, SWT.NONE);

        Assert.Equal(0, list.ItemCount);

        list.Dispose();
    }
}
