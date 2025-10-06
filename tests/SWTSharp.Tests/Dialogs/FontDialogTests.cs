using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Graphics;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for FontDialog dialog.
/// </summary>
public class FontDialogTests : WidgetTestBase
{
    [Fact]
    public void FontDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.NotNull(fontDialog);
        AssertNotDisposed(fontDialog);

        fontDialog.Dispose();
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
            fontDialog.Dispose();
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

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_FontData_WithBold_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var fontData = new FontData("Times New Roman", 14, SWT.BOLD);
        fontDialog.FontData = fontData;

        Assert.Equal(fontData, fontDialog.FontData);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_FontData_WithItalic_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var fontData = new FontData("Courier", 10, SWT.ITALIC);
        fontDialog.FontData = fontData;

        Assert.Equal(fontData, fontDialog.FontData);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_RGB_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var rgb = new RGB(255, 0, 0);
        fontDialog.RGB = rgb;

        Assert.Equal(rgb, fontDialog.RGB);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_Dispose_ShouldSetIsDisposed()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        AssertNotDisposed(fontDialog);
        fontDialog.Dispose();
        AssertDisposed(fontDialog);
    }

    [Fact]
    public void FontDialog_SetFontData_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);
        fontDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() =>
            fontDialog.FontData = new FontData("Arial", 12, SWT.NORMAL));
    }

    [Fact]
    public void FontDialog_GetFontData_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);
        fontDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = fontDialog.FontData);
    }

    [Fact]
    public void FontDialog_Data_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        var testData = new { Name = "Test", Value = 42 };
        fontDialog.Data = testData;

        Assert.Same(testData, fontDialog.Data);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.Same(shell.Display, fontDialog.Display);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_IsDisposed_InitiallyFalse()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.False(fontDialog.IsDisposed);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_MultipleDispose_ShouldNotThrow()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        fontDialog.Dispose();
        fontDialog.Dispose(); // Should not throw

        Assert.True(fontDialog.IsDisposed);
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

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_ParentDispose_ShouldNotAffectDialog()
    {
        var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        shell.Dispose();

        AssertNotDisposed(fontDialog);

        fontDialog.Dispose();
    }

    [Fact]
    public void FontDialog_InitialFontData_ShouldNotBeNull()
    {
        using var shell = CreateTestShell();
        var fontDialog = new FontDialog(shell);

        Assert.NotNull(fontDialog.FontData);

        fontDialog.Dispose();
    }
}
