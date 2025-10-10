using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Group widget.
/// </summary>
public class GroupTests : WidgetTestBase
{
    public GroupTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void Group_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new Group(shell, SWT.NONE));
    }

    [Fact]
    public void Group_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new Group(shell, style),
            SWT.NONE,
            SWT.SHADOW_ETCHED_IN,
            SWT.SHADOW_ETCHED_OUT,
            SWT.SHADOW_IN,
            SWT.SHADOW_OUT
        );
    }

    [Fact]
    public void Group_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Group(shell, SWT.NONE),
            g => g.Text,
            (g, v) => g.Text = v,
            "Group Title"
        );
    }

    [Fact]
    public void Group_Text_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => new Group(shell, SWT.NONE),
            g => g.Text,
            (g, v) => g.Text = v,
            string.Empty
        );
    }

    [Fact]
    public void Group_Text_WithNull_ShouldSetEmptyString()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var group = new Group(shell, SWT.NONE);

            group.Text = null!;

            Assert.Equal(string.Empty, group.Text);

            group.Dispose();
        });
    }

    [Fact]
    public void Group_AddChildren_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var group = new Group(shell, SWT.NONE);

            var button = new Button(group, SWT.PUSH);
            var label = new Label(group, SWT.NONE);

            Assert.Same(group, button.Parent);
            Assert.Same(group, label.Parent);

            group.Dispose();
        });
    }

    [Fact]
    public void Group_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new Group(shell, SWT.NONE));
    }

    [Fact]
    public void Group_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new Group(shell, SWT.NONE));
    }

    [Fact]
    public void Group_Dispose_ShouldDisposeChildren()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var group = new Group(shell, SWT.NONE);
            var button = new Button(group, SWT.PUSH);

            group.Dispose();

            Assert.True(group.IsDisposed);
            Assert.True(button.IsDisposed);
        });
    }

    [Fact]
    public void Group_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new Group(shell, SWT.NONE),
            g => g.Text = "Test"
        );
    }

    [Fact]
    public void Group_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new Group(shell, SWT.NONE));
    }

    [Fact]
    public void Group_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Group(shell, SWT.NONE),
            g => g.Visible,
            (g, v) => g.Visible = v,
            false
        );
    }

    [Fact]
    public void Group_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new Group(shell, SWT.NONE),
            g => g.Enabled,
            (g, v) => g.Enabled = v,
            false
        );
    }

    [Fact]
    public void Group_InitialText_ShouldBeEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var group = new Group(shell, SWT.NONE);

            Assert.Equal(string.Empty, group.Text);

            group.Dispose();
        });
    }

    [Fact]
    public void Group_Display_ShouldMatchParent()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var group = new Group(shell, SWT.NONE);

            Assert.Same(shell.Display, group.Display);

            group.Dispose();
        });
    }
}
