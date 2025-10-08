using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for DirectoryDialog dialog.
/// </summary>
public class DirectoryDialogTests : TestBase
{
    public DirectoryDialogTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.NotNull(directoryDialog);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.NONE };

        foreach (var style in styles)
        {
            var directoryDialog = new DirectoryDialog(shell, style);
            Assert.NotNull(directoryDialog);
        }
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_FilterPath_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.FilterPath = "/home/user";

        Assert.Equal("/home/user", directoryDialog.FilterPath);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_FilterPath_WithEmptyString_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.FilterPath = string.Empty;

        Assert.Equal(string.Empty, directoryDialog.FilterPath);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_FilterPath_WithNull_ShouldSetEmptyString()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.FilterPath = null!;

        Assert.Equal(string.Empty, directoryDialog.FilterPath);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_Message_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.Message = "Select a directory";

        Assert.Equal("Select a directory", directoryDialog.Message);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_InitialFilterPath_ShouldBeEmpty()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.Equal(string.Empty, directoryDialog.FilterPath);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_MessageUpdate_ShouldWork()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.Message = "First";
        Assert.Equal("First", directoryDialog.Message);

        directoryDialog.Message = "Second";
        Assert.Equal("Second", directoryDialog.Message);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_Parent_ShouldMatchShell()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        Assert.Same(shell, directoryDialog.Parent);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_Style_ShouldMatchConstructor()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell, SWT.NONE);

        Assert.Equal(SWT.NONE, directoryDialog.Style);
    }

    [FactSkipOnMacOSCI]
    public void DirectoryDialog_Text_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var directoryDialog = new DirectoryDialog(shell);

        directoryDialog.Text = "Choose Directory";

        Assert.Equal("Choose Directory", directoryDialog.Text);
    }
}
