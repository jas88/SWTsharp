using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for MessageBox dialog.
/// </summary>
public class MessageBoxTests : WidgetTestBase
{
    [Fact]
    public void MessageBox_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.NotNull(messageBox);
        AssertNotDisposed(messageBox);

        messageBox.Dispose();
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
            messageBox.Dispose();
        }
    }

    [Fact]
    public void MessageBox_Message_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = "Test message";

        Assert.Equal("Test message", messageBox.Message);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_Message_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = string.Empty;

        Assert.Equal(string.Empty, messageBox.Message);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_Message_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Message = null!;

        Assert.Equal(string.Empty, messageBox.Message);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_Dispose_ShouldSetIsDisposed()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        AssertNotDisposed(messageBox);
        messageBox.Dispose();
        AssertDisposed(messageBox);
    }

    [Fact]
    public void MessageBox_SetMessage_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);
        messageBox.Dispose();

        Assert.Throws<SWTDisposedException>(() => messageBox.Message = "Test");
    }

    [Fact]
    public void MessageBox_GetMessage_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);
        messageBox.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = messageBox.Message);
    }

    [Fact]
    public void MessageBox_Data_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        var testData = new { Name = "Test", Value = 42 };
        messageBox.Data = testData;

        Assert.Same(testData, messageBox.Data);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_InitialMessage_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.Equal(string.Empty, messageBox.Message);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.Same(shell.Display, messageBox.Display);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_MultipleDispose_ShouldNotThrow()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        messageBox.Dispose();
        messageBox.Dispose(); // Should not throw

        Assert.True(messageBox.IsDisposed);
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

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_ParentDispose_ShouldNotAffectMessageBox()
    {
        var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        shell.Dispose();

        // MessageBox should still be valid after parent disposal
        AssertNotDisposed(messageBox);

        messageBox.Dispose();
    }

    [Fact]
    public void MessageBox_IsDisposed_InitiallyFalse()
    {
        using var shell = CreateTestShell();
        var messageBox = new MessageBox(shell);

        Assert.False(messageBox.IsDisposed);

        messageBox.Dispose();
    }
}
