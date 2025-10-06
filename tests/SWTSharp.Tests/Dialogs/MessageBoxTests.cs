using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for MessageBox dialog.
/// </summary>
public class MessageBoxTests : TestBase
{
    [Fact]
    public void MessageBox_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.NotNull(messageBox);
    }

    [Fact]
    public void MessageBox_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.OK, SWT.CANCEL, SWT.YES | SWT.NO, SWT.ICON_ERROR, SWT.ICON_INFORMATION };

        foreach (var style in styles)
        {
            var messageBox = new MessageBox(shell, style);
            Assert.NotNull(messageBox);
        }
    }

    [Fact]
    public void MessageBox_Message_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = "Test message";

        Assert.Equal("Test message", messageBox.Message);
    }

    [Fact]
    public void MessageBox_Message_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = string.Empty;

        Assert.Equal(string.Empty, messageBox.Message);
    }

    [Fact]
    public void MessageBox_Message_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = null!;

        Assert.Equal(string.Empty, messageBox.Message);
    }

    [Fact]
    public void MessageBox_InitialMessage_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.Equal(string.Empty, messageBox.Message);
    }

    [Fact]
    public void MessageBox_MessageUpdate_ShouldWork()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = "First";
        Assert.Equal("First", messageBox.Message);

        messageBox.Message = "Second";
        Assert.Equal("Second", messageBox.Message);
    }

    [Fact]
    public void MessageBox_Parent_ShouldMatchShell()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.Same(shell, messageBox.Parent);
    }

    [Fact]
    public void MessageBox_Style_ShouldMatchConstructor()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell, SWT.OK | SWT.ICON_INFORMATION);

        Assert.Equal(SWT.OK | SWT.ICON_INFORMATION, messageBox.Style);
    }

    [Fact]
    public void MessageBox_Text_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Text = "Dialog Title";

        Assert.Equal("Dialog Title", messageBox.Text);
    }
}
