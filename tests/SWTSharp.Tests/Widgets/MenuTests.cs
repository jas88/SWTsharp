using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Menu widget.
/// </summary>
public class MenuTests : WidgetTestBase
{
    public MenuTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Menu_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Menu(shell, SWT.BAR));
    }

    [Fact]
    public void Menu_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Menu(shell, style),
            SWT.BAR,
            SWT.DROP_DOWN,
            SWT.POP_UP
        );
    }

    [Fact]
    public void Menu_AddMenuItem_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        Assert.NotNull(menuItem);
        Assert.Equal(1, menu.ItemCount);

        menu.Dispose();
    }

    [Fact]
    public void Menu_AddMultipleMenuItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        var item1 = new MenuItem(menu, SWT.PUSH);
        var item2 = new MenuItem(menu, SWT.PUSH);
        var item3 = new MenuItem(menu, SWT.SEPARATOR);

        Assert.Equal(3, menu.ItemCount);

        menu.Dispose();
    }

    [Fact]
    public void Menu_GetItem_ShouldReturnCorrectItem()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        var item1 = new MenuItem(menu, SWT.PUSH);
        var item2 = new MenuItem(menu, SWT.PUSH);

        var retrieved = menu.GetItem(1);
        Assert.Same(item2, retrieved);

        menu.Dispose();
    }

    [Fact]
    public void Menu_Items_ShouldReturnAllItems()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        var item1 = new MenuItem(menu, SWT.PUSH);
        var item2 = new MenuItem(menu, SWT.PUSH);

        var items = menu.Items;
        Assert.Equal(2, items.Length);
        Assert.Contains(item1, items);
        Assert.Contains(item2, items);

        menu.Dispose();
    }

    [Fact]
    public void Menu_Visible_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.POP_UP);

        menu.Visible = true;
        Assert.True(menu.Visible);

        menu.Visible = false;
        Assert.False(menu.Visible);

        menu.Dispose();
    }

    // Menu does not have Enabled property - it's a Widget, not a Control

    [Fact]
    public void Menu_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Menu(shell, SWT.BAR));
    }

    [Fact]
    public void Menu_Dispose_ShouldDisposeItems()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        menu.Dispose();

        Assert.True(menu.IsDisposed);
        Assert.True(menuItem.IsDisposed);
    }

    [Fact]
    public void Menu_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Menu(shell, SWT.BAR));
    }

    [Fact]
    public void Menu_GetItem_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        new MenuItem(menu, SWT.PUSH);
        menu.Dispose();

        Assert.Throws<SWTDisposedException>(() => menu.GetItem(0));
    }

    [Fact]
    public void Menu_InitialItemCount_ShouldBeZero()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        Assert.Equal(0, menu.ItemCount);

        menu.Dispose();
    }

    [Fact]
    public void Menu_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        Assert.Same(shell.Display, menu.Display);

        menu.Dispose();
    }

    [Fact]
    public void Menu_SubMenu_ShouldWork()
    {
        using var shell = CreateTestShell();
        var menuBar = new Menu(shell, SWT.BAR);
        var fileItem = new MenuItem(menuBar, SWT.CASCADE);
        var fileMenu = new Menu(shell, SWT.DROP_DOWN);

        fileItem.Menu = fileMenu;

        var openItem = new MenuItem(fileMenu, SWT.PUSH);

        Assert.Same(fileMenu, fileItem.Menu);
        Assert.Equal(1, fileMenu.ItemCount);

        menuBar.Dispose();
        fileMenu.Dispose();
    }

    [Fact]
    public void Menu_MixedItemTypes_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        new MenuItem(menu, SWT.PUSH);
        new MenuItem(menu, SWT.SEPARATOR);
        new MenuItem(menu, SWT.CHECK);
        new MenuItem(menu, SWT.RADIO);
        new MenuItem(menu, SWT.CASCADE);

        Assert.Equal(5, menu.ItemCount);

        menu.Dispose();
    }
}
