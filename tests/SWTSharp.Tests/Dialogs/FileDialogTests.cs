using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for FileDialog dialog.
/// </summary>
public class FileDialogTests : TestBase
{
    public FileDialogTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [FactSkipOnMacOSCI]
    public void FileDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.NotNull(fileDialog);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.OPEN, SWT.SAVE, SWT.MULTI };

        foreach (var style in styles)
        {
            var fileDialog = new FileDialog(shell, style);
            Assert.NotNull(fileDialog);
        }
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FileName_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FileName = "test.txt";

        Assert.Equal("test.txt", fileDialog.FileName);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FileName_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FileName = string.Empty;

        Assert.Equal(string.Empty, fileDialog.FileName);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FileName_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FileName = null!;

        Assert.Equal(string.Empty, fileDialog.FileName);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FilterExtensions_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        var extensions = new[] { "*.txt", "*.doc" };
        fileDialog.FilterExtensions = extensions;

        Assert.Equal(extensions, fileDialog.FilterExtensions);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FilterNames_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        var names = new[] { "Text Files", "Word Documents" };
        fileDialog.FilterNames = names;

        Assert.Equal(names, fileDialog.FilterNames);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FilterPath_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FilterPath = "/home/user";

        Assert.Equal("/home/user", fileDialog.FilterPath);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_InitialFileName_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.Equal(string.Empty, fileDialog.FileName);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_FilterExtensions_EmptyArray_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.FilterExtensions = Array.Empty<string>();

        Assert.Empty(fileDialog.FilterExtensions);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_Parent_ShouldMatchShell()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        Assert.Same(shell, fileDialog.Parent);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_Style_ShouldMatchConstructor()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell, SWT.OPEN);

        Assert.Equal(SWT.OPEN, fileDialog.Style);
    }

    [FactSkipOnMacOSCI]
    public void FileDialog_Text_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fileDialog = new FileDialog(shell);

        fileDialog.Text = "Select File";

        Assert.Equal("Select File", fileDialog.Text);
    }
}
