using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Combo widget.
/// </summary>
public class ComboTests : WidgetTestBase
{
    public ComboTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Combo_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Combo(shell, SWT.NONE));
    }

    [Fact]
    public void Combo_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Combo(shell, style),
            SWT.NONE,
            SWT.DROP_DOWN,
            SWT.READ_ONLY,
            SWT.SIMPLE
        );
    }

    [Fact]
    public void Combo_Add_SingleItem_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);

        combo.Add("Item 1");

        Assert.Equal(1, combo.ItemCount);
        Assert.Contains("Item 1", combo.Items);

        combo.Dispose();
    }

    [Fact]
    public void Combo_Add_MultipleItems_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);

        combo.Add("Item 1");
        combo.Add("Item 2");
        combo.Add("Item 3");

        Assert.Equal(3, combo.ItemCount);

        combo.Dispose();
    }

    [Fact]
    public void Combo_Remove_ShouldRemoveItem()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);

        combo.Add("Item 1");
        combo.Add("Item 2");
        combo.Remove(0);

        Assert.Equal(1, combo.ItemCount);
        Assert.DoesNotContain("Item 1", combo.Items);

        combo.Dispose();
    }

    [Fact]
    public void Combo_RemoveAll_ShouldClearCombo()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);

        combo.Add("Item 1");
        combo.Add("Item 2");
        combo.RemoveAll();

        Assert.Equal(0, combo.ItemCount);

        combo.Dispose();
    }

    [Fact]
    public void Combo_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Combo(shell, SWT.NONE),
            c => c.Text,
            (c, v) => c.Text = v,
            "Test Text"
        );
    }

    [Fact]
    public void Combo_SelectionIndex_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.READ_ONLY);

        combo.Add("Item 1");
        combo.Add("Item 2");

        combo.Select(1);

        Assert.Equal(1, combo.SelectionIndex);

        combo.Dispose();
    }

    [Fact]
    public void Combo_Items_ShouldReturnAllItems()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);

        combo.Add("A");
        combo.Add("B");
        combo.Add("C");

        var items = combo.Items;
        Assert.Equal(3, items.Length);

        combo.Dispose();
    }

    [Fact]
    public void Combo_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Combo(shell, SWT.NONE));
    }

    [Fact]
    public void Combo_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Combo(shell, SWT.NONE));
    }

    [Fact]
    public void Combo_Add_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);
        combo.Dispose();

        Assert.Throws<SWTDisposedException>(() => combo.Add("Item"));
    }

    [Fact]
    public void Combo_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Combo(shell, SWT.NONE));
    }

    [Fact]
    public void Combo_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Combo(shell, SWT.NONE),
            c => c.Visible,
            (c, v) => c.Visible = v,
            false
        );
    }

    [Fact]
    public void Combo_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Combo(shell, SWT.NONE),
            c => c.Enabled,
            (c, v) => c.Enabled = v,
            false
        );
    }

    [Fact]
    public void Combo_TextLimit_ShouldLimitText()
    {
        using var shell = CreateTestShell();
        var combo = new Combo(shell, SWT.NONE);

        combo.TextLimit = 5;
        combo.Text = "1234567890";

        Assert.Equal("12345", combo.Text);

        combo.Dispose();
    }
}
