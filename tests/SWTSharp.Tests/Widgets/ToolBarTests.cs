using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for ToolBar widget.
/// </summary>
public class ToolBarTests : WidgetTestBase
{
    [Fact]
    public void ToolBar_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new ToolBar(shell, SWT.NONE));
    }

    [Fact]
    public void ToolBar_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new ToolBar(shell, style),
            SWT.NONE,
            SWT.HORIZONTAL,
            SWT.VERTICAL,
            SWT.FLAT
        );
    }

    [Fact]
    public void ToolBar_AddToolItem_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);
        var toolItem = new ToolItem(toolBar, SWT.PUSH);

        Assert.NotNull(toolItem);
        Assert.Equal(1, toolBar.ItemCount);

        toolBar.Dispose();
    }

    [Fact]
    public void ToolBar_AddMultipleToolItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);

        var item1 = new ToolItem(toolBar, SWT.PUSH);
        var item2 = new ToolItem(toolBar, SWT.PUSH);
        var item3 = new ToolItem(toolBar, SWT.SEPARATOR);

        Assert.Equal(3, toolBar.ItemCount);

        toolBar.Dispose();
    }

    [Fact]
    public void ToolBar_GetItem_ShouldReturnCorrectItem()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);

        var item1 = new ToolItem(toolBar, SWT.PUSH);
        var item2 = new ToolItem(toolBar, SWT.PUSH);

        var retrieved = toolBar.GetItem(1);
        Assert.Same(item2, retrieved);

        toolBar.Dispose();
    }

    [Fact]
    public void ToolBar_Items_ShouldReturnAllItems()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);

        var item1 = new ToolItem(toolBar, SWT.PUSH);
        var item2 = new ToolItem(toolBar, SWT.PUSH);

        var items = toolBar.Items;
        Assert.Equal(2, items.Length);
        Assert.Contains(item1, items);
        Assert.Contains(item2, items);

        toolBar.Dispose();
    }

    [Fact]
    public void ToolBar_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new ToolBar(shell, SWT.NONE));
    }

    [Fact]
    public void ToolBar_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new ToolBar(shell, SWT.NONE));
    }

    [Fact]
    public void ToolBar_Dispose_ShouldDisposeItems()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);
        var toolItem = new ToolItem(toolBar, SWT.PUSH);

        toolBar.Dispose();

        Assert.True(toolBar.IsDisposed);
        Assert.True(toolItem.IsDisposed);
    }

    [Fact]
    public void ToolBar_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new ToolBar(shell, SWT.NONE));
    }

    [Fact]
    public void ToolBar_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ToolBar(shell, SWT.NONE),
            t => t.Visible,
            (t, v) => t.Visible = v,
            false
        );
    }

    [Fact]
    public void ToolBar_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new ToolBar(shell, SWT.NONE),
            t => t.Enabled,
            (t, v) => t.Enabled = v,
            false
        );
    }

    [Fact]
    public void ToolBar_InitialItemCount_ShouldBeZero()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);

        Assert.Equal(0, toolBar.ItemCount);

        toolBar.Dispose();
    }

    [Fact]
    public void ToolBar_GetItem_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);
        new ToolItem(toolBar, SWT.PUSH);
        toolBar.Dispose();

        Assert.Throws<SWTDisposedException>(() => toolBar.GetItem(0));
    }

    [Fact]
    public void ToolBar_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);

        Assert.Same(shell.Display, toolBar.Display);

        toolBar.Dispose();
    }

    [Fact]
    public void ToolBar_MixedItemTypes_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var toolBar = new ToolBar(shell, SWT.NONE);

        new ToolItem(toolBar, SWT.PUSH);
        new ToolItem(toolBar, SWT.SEPARATOR);
        new ToolItem(toolBar, SWT.CHECK);
        new ToolItem(toolBar, SWT.RADIO);

        Assert.Equal(4, toolBar.ItemCount);

        toolBar.Dispose();
    }
}
