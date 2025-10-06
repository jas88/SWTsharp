using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Table widget.
/// </summary>
public class TableTests : WidgetTestBase
{
    [Fact]
    public void Table_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Table(shell, SWT.NONE));
    }

    [Fact]
    public void Table_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Table(shell, style),
            SWT.NONE,
            SWT.SINGLE,
            SWT.MULTI,
            SWT.CHECK,
            SWT.FULL_SELECTION
        );
    }

    [Fact]
    public void Table_AddColumn_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);
        var column = new TableColumn(table, SWT.NONE);

        Assert.NotNull(column);
        Assert.Equal(1, table.ColumnCount);

        table.Dispose();
    }

    [Fact]
    public void Table_AddMultipleColumns_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);

        var col1 = new TableColumn(table, SWT.NONE);
        var col2 = new TableColumn(table, SWT.NONE);
        var col3 = new TableColumn(table, SWT.NONE);

        Assert.Equal(3, table.ColumnCount);

        table.Dispose();
    }

    [Fact]
    public void Table_AddItem_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);
        var item = new TableItem(table, SWT.NONE);

        Assert.NotNull(item);
        Assert.Equal(1, table.ItemCount);

        table.Dispose();
    }

    [Fact]
    public void Table_AddMultipleItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);

        var item1 = new TableItem(table, SWT.NONE);
        var item2 = new TableItem(table, SWT.NONE);
        var item3 = new TableItem(table, SWT.NONE);

        Assert.Equal(3, table.ItemCount);

        table.Dispose();
    }

    [Fact]
    public void Table_GetItem_ShouldReturnCorrectItem()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);

        var item1 = new TableItem(table, SWT.NONE);
        var item2 = new TableItem(table, SWT.NONE);

        var retrieved = table.GetItem(1);
        Assert.Same(item2, retrieved);

        table.Dispose();
    }

    [Fact]
    public void Table_GetColumn_ShouldReturnCorrectColumn()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);

        var col1 = new TableColumn(table, SWT.NONE);
        var col2 = new TableColumn(table, SWT.NONE);

        var retrieved = table.GetColumn(1);
        Assert.Same(col2, retrieved);

        table.Dispose();
    }

    [Fact]
    public void Table_HeaderVisible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Table(shell, SWT.NONE),
            t => t.HeaderVisible,
            (t, v) => t.HeaderVisible = v,
            true
        );
    }

    [Fact]
    public void Table_LinesVisible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Table(shell, SWT.NONE),
            t => t.LinesVisible,
            (t, v) => t.LinesVisible = v,
            true
        );
    }

    [Fact]
    public void Table_SelectionIndex_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.SINGLE);

        var item1 = new TableItem(table, SWT.NONE);
        var item2 = new TableItem(table, SWT.NONE);

        table.SetSelection(new[] { item2 });
        Assert.Single(table.Selection);
        Assert.Same(item2, table.Selection[0]);

        table.Dispose();
    }

    [Fact]
    public void Table_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Table(shell, SWT.NONE));
    }

    [Fact]
    public void Table_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Table(shell, SWT.NONE));
    }

    [Fact]
    public void Table_Dispose_ShouldDisposeItemsAndColumns()
    {
        using var shell = CreateTestShell();
        var table = new Table(shell, SWT.NONE);
        var column = new TableColumn(table, SWT.NONE);
        var item = new TableItem(table, SWT.NONE);

        table.Dispose();

        Assert.True(table.IsDisposed);
        Assert.True(column.IsDisposed);
        Assert.True(item.IsDisposed);
    }

    [Fact]
    public void Table_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Table(shell, SWT.NONE));
    }

    [Fact]
    public void Table_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Table(shell, SWT.NONE),
            t => t.Visible,
            (t, v) => t.Visible = v,
            false
        );
    }

    [Fact]
    public void Table_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Table(shell, SWT.NONE),
            t => t.Enabled,
            (t, v) => t.Enabled = v,
            false
        );
    }
}
