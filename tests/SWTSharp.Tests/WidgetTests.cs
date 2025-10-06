using Xunit;
using SWTSharp;

namespace SWTSharp.Tests;

public class WidgetTests
{
    [Fact]
    public void Display_Default_ShouldNotBeNull()
    {
        var display = Display.Default;
        Assert.NotNull(display);
    }

    [Fact]
    public void Display_Current_ShouldReturnSameAsDefault()
    {
        var display = Display.Default;
        var current = Display.Current;
        Assert.Same(display, current);
    }

    [Fact]
    public void Shell_Create_ShouldNotBeNull()
    {
        var display = Display.Default;
        var shell = new Shell(display);
        Assert.NotNull(shell);
        Assert.Same(display, shell.Display);
        shell.Dispose();
    }

    [Fact]
    public void Shell_Dispose_ShouldSetIsDisposed()
    {
        var shell = new Shell();
        Assert.False(shell.IsDisposed);
        shell.Dispose();
        Assert.True(shell.IsDisposed);
    }

    [Fact]
    public void Shell_CheckWidget_AfterDispose_ShouldThrow()
    {
        var shell = new Shell();
        shell.Dispose();
        Assert.Throws<SWTDisposedException>(() => shell.Text = "Test");
    }

    [Fact]
    public void Shell_Text_ShouldGetAndSet()
    {
        var shell = new Shell();
        shell.Text = "Test Window";
        Assert.Equal("Test Window", shell.Text);
        shell.Dispose();
    }

    [Fact]
    public void Button_Create_ShouldNotBeNull()
    {
        var shell = new Shell();
        var button = new Button(shell, SWT.PUSH);
        Assert.NotNull(button);
        Assert.Same(shell, button.Parent);
        button.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Button_Text_ShouldGetAndSet()
    {
        var shell = new Shell();
        var button = new Button(shell, SWT.PUSH);
        button.Text = "Click Me";
        Assert.Equal("Click Me", button.Text);
        button.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Button_Selection_ShouldGetAndSet()
    {
        var shell = new Shell();
        var button = new Button(shell, SWT.CHECK);
        Assert.False(button.Selection);
        button.Selection = true;
        Assert.True(button.Selection);
        button.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Label_Text_ShouldGetAndSet()
    {
        var shell = new Shell();
        var label = new Label(shell, SWT.NONE);
        label.Text = "Hello World";
        Assert.Equal("Hello World", label.Text);
        label.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Text_Content_ShouldGetAndSet()
    {
        var shell = new Shell();
        var text = new Text(shell, SWT.SINGLE);
        text.TextContent = "Test input";
        Assert.Equal("Test input", text.TextContent);
        text.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Text_Append_ShouldAppendText()
    {
        var shell = new Shell();
        var text = new Text(shell, SWT.MULTI);
        text.TextContent = "Hello";
        text.Append(" World");
        Assert.Equal("Hello World", text.TextContent);
        text.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Text_TextLimit_ShouldLimitText()
    {
        var shell = new Shell();
        var text = new Text(shell, SWT.SINGLE);
        text.TextLimit = 5;
        text.TextContent = "1234567890";
        Assert.Equal("12345", text.TextContent);
        text.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Control_Visible_ShouldGetAndSet()
    {
        var shell = new Shell();
        var button = new Button(shell, SWT.PUSH);
        Assert.True(button.Visible);
        button.Visible = false;
        Assert.False(button.Visible);
        button.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Control_Enabled_ShouldGetAndSet()
    {
        var shell = new Shell();
        var button = new Button(shell, SWT.PUSH);
        Assert.True(button.Enabled);
        button.Enabled = false;
        Assert.False(button.Enabled);
        button.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Widget_Data_ShouldGetAndSet()
    {
        var shell = new Shell();
        var testData = new { Name = "Test", Value = 42 };
        shell.Data = testData;
        Assert.Same(testData, shell.Data);
        shell.Dispose();
    }

    [Fact]
    public void SWT_Platform_ShouldReturnValidPlatform()
    {
        var platform = Display.Platform;
        Assert.Contains(platform, new[] { SWT.PLATFORM_WIN32, SWT.PLATFORM_MACOSX, SWT.PLATFORM_LINUX });
    }

    [Fact]
    public void SWT_GetErrorMessage_ShouldReturnMessage()
    {
        var message = SWT.GetErrorMessage(SWT.ERROR_NULL_ARGUMENT);
        Assert.NotNull(message);
        Assert.Contains("null", message, StringComparison.OrdinalIgnoreCase);
    }
}
