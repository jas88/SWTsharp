using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Graphics;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for ColorDialog dialog.
/// </summary>
public class ColorDialogTests : WidgetTestBase
{
    [Fact]
    public void ColorDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        Assert.NotNull(colorDialog);
        AssertNotDisposed(colorDialog);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_Create_WithStyles_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var styles = new[] { SWT.NONE };

        foreach (var style in styles)
        {
            var colorDialog = new ColorDialog(shell, style);
            Assert.NotNull(colorDialog);
            colorDialog.Dispose();
        }
    }

    [Fact]
    public void ColorDialog_RGB_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var rgb = new RGB(255, 128, 64);
        colorDialog.RGB = rgb;

        Assert.Equal(rgb, colorDialog.RGB);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_RGB_WithBlack_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var black = new RGB(0, 0, 0);
        colorDialog.RGB = black;

        Assert.Equal(black, colorDialog.RGB);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_RGB_WithWhite_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var white = new RGB(255, 255, 255);
        colorDialog.RGB = white;

        Assert.Equal(white, colorDialog.RGB);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_Dispose_ShouldSetIsDisposed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        AssertNotDisposed(colorDialog);
        colorDialog.Dispose();
        AssertDisposed(colorDialog);
    }

    [Fact]
    public void ColorDialog_SetRGB_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);
        colorDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => colorDialog.RGB = new RGB(0, 0, 0));
    }

    [Fact]
    public void ColorDialog_GetRGB_AfterDispose_ShouldThrow()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);
        colorDialog.Dispose();

        Assert.Throws<SWTDisposedException>(() => _ = colorDialog.RGB);
    }

    [Fact]
    public void ColorDialog_Data_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var testData = new { Name = "Test", Value = 42 };
        colorDialog.Data = testData;

        Assert.Same(testData, colorDialog.Data);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_Display_ShouldMatchParent()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        Assert.Same(shell.Display, colorDialog.Display);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_IsDisposed_InitiallyFalse()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        Assert.False(colorDialog.IsDisposed);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_MultipleDispose_ShouldNotThrow()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        colorDialog.Dispose();
        colorDialog.Dispose(); // Should not throw

        Assert.True(colorDialog.IsDisposed);
    }

    [Fact]
    public void ColorDialog_RGBUpdate_ShouldWork()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var color1 = new RGB(255, 0, 0);
        colorDialog.RGB = color1;
        Assert.Equal(color1, colorDialog.RGB);

        var color2 = new RGB(0, 255, 0);
        colorDialog.RGB = color2;
        Assert.Equal(color2, colorDialog.RGB);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_ParentDispose_ShouldNotAffectDialog()
    {
        var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        shell.Dispose();

        AssertNotDisposed(colorDialog);

        colorDialog.Dispose();
    }

    [Fact]
    public void ColorDialog_InitialRGB_ShouldNotBeNull()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        Assert.NotNull(colorDialog.RGB);

        colorDialog.Dispose();
    }
}
