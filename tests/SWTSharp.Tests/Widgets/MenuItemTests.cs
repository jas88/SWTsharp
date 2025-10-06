using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for MenuItem widget.
/// </summary>
public class MenuItemTests : WidgetTestBase
{
    [Fact]
    public void MenuItem_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        Assert.NotNull(menuItem);
        AssertNotDisposed(menuItem);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);

        var styles = new[] { SWT.PUSH, SWT.CHECK, SWT.RADIO, SWT.SEPARATOR, SWT.CASCADE };
        foreach (var style in styles)
        {
            var item = new MenuItem(menu, style);
            Assert.NotNull(item);
            item.Dispose();
        }

        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Text_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        menuItem.Text = "File";
        Assert.Equal("File", menuItem.Text);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Text_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        menuItem.Text = string.Empty;
        Assert.Equal(string.Empty, menuItem.Text);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Text_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        menuItem.Text = null!;
        Assert.Equal(string.Empty, menuItem.Text);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Selection_Check_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.CHECK);

        Assert.False(menuItem.Selection);

        menuItem.Selection = true;
        Assert.True(menuItem.Selection);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Selection_Radio_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.RADIO);

        Assert.False(menuItem.Selection);

        menuItem.Selection = true;
        Assert.True(menuItem.Selection);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Enabled_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        menuItem.Enabled = false;
        Assert.False(menuItem.Enabled);

        menuItem.Enabled = true;
        Assert.True(menuItem.Enabled);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Menu_Cascade_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menuBar = new Menu(shell, SWT.BAR);
        var cascadeItem = new MenuItem(menuBar, SWT.CASCADE);
        var subMenu = new Menu(shell, SWT.DROP_DOWN);

        cascadeItem.Menu = subMenu;

        Assert.Same(subMenu, cascadeItem.Menu);

        cascadeItem.Dispose();
        menuBar.Dispose();
        subMenu.Dispose();
    }

    [Fact]
    public void MenuItem_Dispose_ShouldSetIsDisposed()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        AssertNotDisposed(menuItem);
        menuItem.Dispose();
        AssertDisposed(menuItem);

        menu.Dispose();
    }

    [Fact]
    public void MenuItem_SetText_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);
        menuItem.Dispose();

        Assert.Throws<SWTDisposedException>(() => menuItem.Text = "Test");

        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Data_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        var testData = new { Name = "Test", Value = 42 };
        menuItem.Data = testData;

        Assert.Same(testData, menuItem.Data);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_InitiallyEnabled()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        Assert.True(menuItem.Enabled);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_InitialText_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        Assert.Equal(string.Empty, menuItem.Text);

        menuItem.Dispose();
        menu.Dispose();
    }

    [Fact]
    public void MenuItem_Display_ShouldMatchMenu()
    {
        using var shell = CreateTestShell();
        var menu = new Menu(shell, SWT.BAR);
        var menuItem = new MenuItem(menu, SWT.PUSH);

        Assert.Same(shell.Display, menuItem.Display);

        menuItem.Dispose();
        menu.Dispose();
    }
}
