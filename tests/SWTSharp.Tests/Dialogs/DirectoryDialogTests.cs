using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for DirectoryDialog dialog.
/// </summary>
public class DirectoryDialogTests : WidgetTestBase
{
    [Fact]
    public void DirectoryDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.NotNull(directoryDialog);
        AssertNotDisposed(directoryDialog);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.NONE };

        foreach (var style in styles)
        {
            var directoryDialog = new DirectoryDialog(shell, style);
            Assert.NotNull(directoryDialog);
            directoryDialog.Dispose();
        }
    }

    [Fact]
    public void DirectoryDialog_FilterPath_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.FilterPath = "/home/user";

        Assert.Equal("/home/user", directoryDialog.FilterPath);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_FilterPath_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.FilterPath = string.Empty;

        Assert.Equal(string.Empty, directoryDialog.FilterPath);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_FilterPath_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.FilterPath = null!;

        Assert.Equal(string.Empty, directoryDialog.FilterPath);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_Message_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.Message = "Select a directory";

        Assert.Equal("Select a directory", directoryDialog.Message);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_Dispose_ShouldSetIsDisposed()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        AssertNotDisposed(directoryDialog);
        directoryDialog.Dispose();
        AssertDisposed(directoryDialog);
    }

    [Fact]
    public void DirectoryDialog_SetFilterPath_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);
        directoryDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => directoryDialog.FilterPath = "/test");
    }

    [Fact]
    public void DirectoryDialog_GetFilterPath_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);
        directoryDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = directoryDialog.FilterPath);
    }

    [Fact]
    public void DirectoryDialog_Data_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        var testData = new { Name = "Test", Value = 42 };
        directoryDialog.Data = testData;

        Assert.Same(testData, directoryDialog.Data);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_InitialFilterPath_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.Equal(string.Empty, directoryDialog.FilterPath);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.Same(shell.Display, directoryDialog.Display);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_IsDisposed_InitiallyFalse()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.False(directoryDialog.IsDisposed);

        directoryDialog.Dispose();
    }

    [Fact]
    public void DirectoryDialog_MultipleDispose_ShouldNotThrow()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.Dispose();
        directoryDialog.Dispose(); // Should not throw

        Assert.True(directoryDialog.IsDisposed);
    }

    [Fact]
    public void DirectoryDialog_MessageUpdate_ShouldWork()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.Message = "First";
        Assert.Equal("First", directoryDialog.Message);

        directoryDialog.Message = "Second";
        Assert.Equal("Second", directoryDialog.Message);

        directoryDialog.Dispose();
    }
}
