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

    [Fact]
    public void FileDialog_Create_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            Assert.NotNull(fileDialog);
        });
    }

    [Fact]
    public void FileDialog_Create_WithStyles_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styles = new[] { SWT.OPEN, SWT.SAVE, SWT.MULTI };

            foreach (var style in styles)
            {
                var fileDialog = new FileDialog(shell, style);
                Assert.NotNull(fileDialog);
            }
        });
    }

    [Fact]
    public void FileDialog_FileName_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            fileDialog.FileName = "test.txt";

            Assert.Equal("test.txt", fileDialog.FileName);
        });
    }

    [Fact]
    public void FileDialog_FileName_WithEmptyString_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            fileDialog.FileName = string.Empty;

            Assert.Equal(string.Empty, fileDialog.FileName);
        });
    }

    [Fact]
    public void FileDialog_FileName_WithNull_ShouldSetEmptyString()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            fileDialog.FileName = null!;

            Assert.Equal(string.Empty, fileDialog.FileName);
        });
    }

    [Fact]
    public void FileDialog_FilterExtensions_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            var extensions = new[] { "*.txt", "*.doc" };
            fileDialog.FilterExtensions = extensions;

            Assert.Equal(extensions, fileDialog.FilterExtensions);
        });
    }

    [Fact]
    public void FileDialog_FilterNames_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            var names = new[] { "Text Files", "Word Documents" };
            fileDialog.FilterNames = names;

            Assert.Equal(names, fileDialog.FilterNames);
        });
    }

    [Fact]
    public void FileDialog_FilterPath_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            fileDialog.FilterPath = "/home/user";

            Assert.Equal("/home/user", fileDialog.FilterPath);
        });
    }

    [Fact]
    public void FileDialog_InitialFileName_ShouldBeEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            Assert.Equal(string.Empty, fileDialog.FileName);
        });
    }

    [Fact]
    public void FileDialog_FilterExtensions_EmptyArray_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            fileDialog.FilterExtensions = Array.Empty<string>();

            Assert.Empty(fileDialog.FilterExtensions);
        });
    }

    [Fact]
    public void FileDialog_Parent_ShouldMatchShell()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            Assert.Same(shell, fileDialog.Parent);
        });
    }

    [Fact]
    public void FileDialog_Style_ShouldMatchConstructor()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell, SWT.OPEN);

            Assert.Equal(SWT.OPEN, fileDialog.Style);
        });
    }

    [Fact]
    public void FileDialog_Text_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var fileDialog = new FileDialog(shell);

            fileDialog.Text = "Select File";

            Assert.Equal("Select File", fileDialog.Text);
        });
    }
}
