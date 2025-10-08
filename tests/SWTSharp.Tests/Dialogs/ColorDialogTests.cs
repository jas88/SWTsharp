using Xunit;
using SWTSharp;
using SWTSharp.Dialogs;
using SWTSharp.Graphics;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Dialogs;

/// <summary>
/// Comprehensive unit tests for ColorDialog dialog.
/// </summary>
public class ColorDialogTests : TestBase
{
    public ColorDialogTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void ColorDialog_Create_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        Assert.NotNull(colorDialog);
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
    }

    [Fact]
    public void ColorDialog_RGB_WithBlack_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var black = new RGB(0, 0, 0);
        colorDialog.RGB = black;

        Assert.Equal(black, colorDialog.RGB);
    }

    [Fact]
    public void ColorDialog_RGB_WithWhite_ShouldSucceed()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var white = new RGB(255, 255, 255);
        colorDialog.RGB = white;

        Assert.Equal(white, colorDialog.RGB);
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
    }

    [Fact]
    public void ColorDialog_InitialRGB_ShouldHaveDefaultValue()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        // RGB is a value type, verify it has a valid default value
        Assert.NotEqual(default(RGB), colorDialog.RGB);
    }

    [Fact]
    public void ColorDialog_RGBs_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        var customColors = new[] {
            new RGB(255, 0, 0),
            new RGB(0, 255, 0),
            new RGB(0, 0, 255)
        };

        colorDialog.RGBs = customColors;

        Assert.Equal(customColors, colorDialog.RGBs);
    }

    [Fact]
    public void ColorDialog_Text_ShouldGetAndSet()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        colorDialog.Text = "Select a Color";

        Assert.Equal("Select a Color", colorDialog.Text);
    }

    [Fact]
    public void ColorDialog_Parent_ShouldMatchShell()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell);

        Assert.Same(shell, colorDialog.Parent);
    }

    [Fact]
    public void ColorDialog_Style_ShouldMatchConstructor()
    {
        using var shell = CreateTestShell();
        var colorDialog = new ColorDialog(shell, SWT.NONE);

        Assert.Equal(SWT.NONE, colorDialog.Style);
    }
}
