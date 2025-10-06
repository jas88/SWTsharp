using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for FileDialog dialog.
/// </summary>
public class FileDialogTests : WidgetTestBase
{
    [Fact]
    public void FileDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.NotNull(fileDialog);
        AssertNotDisposed(fileDialog);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.OPEN, SWT.SAVE, SWT.MULTI };

        foreach (var style in styles)
        {
            var fileDialog = new FileDialog(shell, style);
            Assert.NotNull(fileDialog);
            fileDialog.Dispose();
        }
    }

    [Fact]
    public void FileDialog_FileName_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FileName = "test.txt";

        Assert.Equal("test.txt", fileDialog.FileName);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_FileName_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FileName = string.Empty;

        Assert.Equal(string.Empty, fileDialog.FileName);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_FileName_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FileName = null!;

        Assert.Equal(string.Empty, fileDialog.FileName);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_FilterExtensions_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        var extensions = new[] { "*.txt", "*.doc" };
        fileDialog.FilterExtensions = extensions;

        Assert.Equal(extensions, fileDialog.FilterExtensions);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_FilterNames_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        var names = new[] { "Text Files", "Word Documents" };
        fileDialog.FilterNames = names;

        Assert.Equal(names, fileDialog.FilterNames);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_FilterPath_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FilterPath = "/home/user";

        Assert.Equal("/home/user", fileDialog.FilterPath);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_Dispose_ShouldSetIsDisposed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        AssertNotDisposed(fileDialog);
        fileDialog.Dispose();
        AssertDisposed(fileDialog);
    }

    [Fact]
    public void FileDialog_SetFileName_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);
        fileDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => fileDialog.FileName = "test.txt");
    }

    [Fact]
    public void FileDialog_GetFileName_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);
        fileDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = fileDialog.FileName);
    }

    [Fact]
    public void FileDialog_Data_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        var testData = new { Name = "Test", Value = 42 };
        fileDialog.Data = testData;

        Assert.Same(testData, fileDialog.Data);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_InitialFileName_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.Equal(string.Empty, fileDialog.FileName);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.Same(shell.Display, fileDialog.Display);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_FilterExtensions_EmptyArray_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FilterExtensions = Array.Empty<string>();

        Assert.Empty(fileDialog.FilterExtensions);

        fileDialog.Dispose();
    }

    [Fact]
    public void FileDialog_IsDisposed_InitiallyFalse()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.False(fileDialog.IsDisposed);

        fileDialog.Dispose();
    }
}
