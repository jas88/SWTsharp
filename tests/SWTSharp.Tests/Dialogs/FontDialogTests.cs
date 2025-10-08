using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Graphics;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for FontDialog dialog.
/// </summary>
public class FontDialogTests : TestBase
{
    public FontDialogTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void FontDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.NotNull(fontDialog);
    }

    [Fact]
    public void FontDialog_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.NONE };

        foreach (var style in styles)
        {
            var fontDialog = new FontDialog(shell, style);
            Assert.NotNull(fontDialog);
        }
    }

    [Fact]
    public void FontDialog_FontData_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var fontData = new FontData("Arial", 12, SWT.NORMAL);
        fontDialog.FontData = fontData;

        Assert.Equal(fontData, fontDialog.FontData);
    }

    [Fact]
    public void FontDialog_FontData_WithBold_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var fontData = new FontData("Times New Roman", 14, SWT.BOLD);
        fontDialog.FontData = fontData;

        Assert.Equal(fontData, fontDialog.FontData);
    }

    [Fact]
    public void FontDialog_FontData_WithItalic_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var fontData = new FontData("Courier", 10, SWT.ITALIC);
        fontDialog.FontData = fontData;

        Assert.Equal(fontData, fontDialog.FontData);
    }

    [Fact]
    public void FontDialog_RGB_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var rgb = new RGB(255, 0, 0);
        fontDialog.RGB = rgb;

        Assert.Equal(rgb, fontDialog.RGB);
    }

    [Fact]
    public void FontDialog_FontDataUpdate_ShouldWork()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var font1 = new FontData("Arial", 12, SWT.NORMAL);
        fontDialog.FontData = font1;
        Assert.Equal(font1, fontDialog.FontData);

        var font2 = new FontData("Times", 14, SWT.BOLD);
        fontDialog.FontData = font2;
        Assert.Equal(font2, fontDialog.FontData);
    }

    [Fact]
    public void FontDialog_InitialFontData_ShouldNotBeNull()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.NotNull(fontDialog.FontData);
    }

    [Fact]
    public void FontDialog_Parent_ShouldMatchShell()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.Same(shell, fontDialog.Parent);
    }

    [Fact]
    public void FontDialog_Style_ShouldMatchConstructor()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell, SWT.NONE);

        Assert.Equal(SWT.NONE, fontDialog.Style);
    }

    [Fact]
    public void FontDialog_Text_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        fontDialog.Text = "Select Font";

        Assert.Equal("Select Font", fontDialog.Text);
    }
}
