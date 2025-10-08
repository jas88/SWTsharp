using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests;

[Collection("Display Tests")]
public class WidgetTests : TestBase
{
    public WidgetTests(DisplayFixture displayFixture) : base(displayFixture)
    {
    }

    [FactSkipOnMacOSCI]
    public void Display_Default_ShouldNotBeNull()
    {
        Assert.NotNull(Display);
    }

    [FactSkipOnMacOSCI]
    public void Display_Current_ShouldReturnSameAsDefault()
    {
        var current = Display.Current;
        Assert.Same(Display, current);
    }

    [FactSkipOnMacOSCI]
    public void Shell_Create_ShouldNotBeNull()
    {
        Shell? shell = null;
        RunOnUIThread(() =>
        {
            shell = new Shell(Display);
            Assert.NotNull(shell);
            Assert.Same(Display, shell.Display);
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Shell_Dispose_ShouldSetIsDisposed()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            Assert.False(shell.IsDisposed);
            shell.Dispose();
            Assert.True(shell.IsDisposed);
        });
    }

    [FactSkipOnMacOSCI]
    public void Shell_CheckWidget_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            shell.Dispose();
            Assert.Throws<SWTDisposedException>(() => shell.Text = "Test");
        });
    }

    [FactSkipOnMacOSCI]
    public void Shell_Text_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            shell.Text = "Test Window";
            Assert.Equal("Test Window", shell.Text);
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Button_Create_ShouldNotBeNull()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var button = new Button(shell, SWT.PUSH);
            Assert.NotNull(button);
            Assert.Same(shell, button.Parent);
            button.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Button_Text_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var button = new Button(shell, SWT.PUSH);
            button.Text = "Click Me";
            Assert.Equal("Click Me", button.Text);
            button.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Button_Selection_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var button = new Button(shell, SWT.CHECK);
            Assert.False(button.Selection);
            button.Selection = true;
            Assert.True(button.Selection);
            button.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Label_Text_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var label = new Label(shell, SWT.NONE);
            label.Text = "Hello World";
            Assert.Equal("Hello World", label.Text);
            label.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Text_Content_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var text = new Text(shell, SWT.SINGLE);
            text.TextContent = "Test input";
            Assert.Equal("Test input", text.TextContent);
            text.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Text_Append_ShouldAppendText()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var text = new Text(shell, SWT.MULTI);
            text.TextContent = "Hello";
            text.Append(" World");
            Assert.Equal("Hello World", text.TextContent);
            text.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Text_TextLimit_ShouldLimitText()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var text = new Text(shell, SWT.SINGLE);
            text.TextLimit = 5;
            text.TextContent = "1234567890";
            Assert.Equal("12345", text.TextContent);
            text.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Control_Visible_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var button = new Button(shell, SWT.PUSH);
            Assert.True(button.Visible);
            button.Visible = false;
            Assert.False(button.Visible);
            button.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Control_Enabled_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var button = new Button(shell, SWT.PUSH);
            Assert.True(button.Enabled);
            button.Enabled = false;
            Assert.False(button.Enabled);
            button.Dispose();
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void Widget_Data_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            var shell = new Shell();
            var testData = new { Name = "Test", Value = 42 };
            shell.Data = testData;
            Assert.Same(testData, shell.Data);
            shell.Dispose();
        });
    }

    [FactSkipOnMacOSCI]
    public void SWT_Platform_ShouldReturnValidPlatform()
    {
        var platform = Display.Platform;
        Assert.Contains(platform, new[] { SWT.PLATFORM_WIN32, SWT.PLATFORM_MACOSX, SWT.PLATFORM_LINUX });
    }

    [FactSkipOnMacOSCI]
    public void SWT_GetErrorMessage_ShouldReturnMessage()
    {
        var message = SWT.GetErrorMessage(SWT.ERROR_NULL_ARGUMENT);
        Assert.NotNull(message);
        Assert.Contains("null", message, StringComparison.OrdinalIgnoreCase);
    }
}
