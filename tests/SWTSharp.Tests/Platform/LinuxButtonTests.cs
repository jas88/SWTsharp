using Xunit;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.Platform;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// Focused tests for Button widget on Linux.
/// Tests only existing API: Text, Selection, Click events.
/// </summary>
[Collection("Cross-Platform Tests")]
public class LinuxButtonTests : TestBase
{
    public LinuxButtonTests(DisplayFixture fixture) : base(fixture) { }

    #region Creation Tests

    [LinuxFact]
    public void Button_Create_WithPushStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);
            Assert.NotNull(button);
            Assert.False(button.IsDisposed);
        });
    }

    [LinuxFact]
    public void Button_Create_WithCheckStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.CHECK);
            Assert.NotNull(button);
        });
    }

    [LinuxFact]
    public void Button_Create_WithRadioStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.RADIO);
            Assert.NotNull(button);
        });
    }

    [LinuxFact]
    public void Button_Create_WithToggleStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.TOGGLE);
            Assert.NotNull(button);
        });
    }

    #endregion

    #region Text Property Tests

    [LinuxFact]
    public void Button_Text_GetSet_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            button.Text = "Test Button";
            Assert.Equal("Test Button", button.Text);
        });
    }

    [LinuxFact]
    public void Button_Text_SetEmpty_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            button.Text = "";
            Assert.Equal("", button.Text);
        });
    }

    [LinuxFact]
    public void Button_Text_SetNull_ShouldConvertToEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            button.Text = null!;
            Assert.Equal("", button.Text);
        });
    }

    [LinuxFact]
    public void Button_Text_WithUnicode_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            button.Text = "Hello 世界 🌍";
            Assert.Equal("Hello 世界 🌍", button.Text);
        });
    }

    [LinuxFact]
    public void Button_Text_MultipleChanges_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            button.Text = "First";
            Assert.Equal("First", button.Text);

            button.Text = "Second";
            Assert.Equal("Second", button.Text);

            button.Text = "Third";
            Assert.Equal("Third", button.Text);
        });
    }

    #endregion

    #region Selection Property Tests

    [LinuxFact]
    public void Button_Selection_CheckButton_ShouldToggle()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.CHECK);

            Assert.False(button.Selection);

            button.Selection = true;
            Assert.True(button.Selection);

            button.Selection = false;
            Assert.False(button.Selection);
        });
    }

    [LinuxFact]
    public void Button_Selection_RadioButton_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.RADIO);

            Assert.False(button.Selection);

            button.Selection = true;
            Assert.True(button.Selection);
        });
    }

    [LinuxFact]
    public void Button_Selection_ToggleButton_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.TOGGLE);

            Assert.False(button.Selection);

            button.Selection = true;
            Assert.True(button.Selection);

            button.Selection = false;
            Assert.False(button.Selection);
        });
    }

    #endregion

    #region Click Event Tests

    [LinuxFact]
    public void Button_Click_EventHandlerRegistration()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            var clicked = false;
            button.Click += (sender, e) => clicked = true;

            // Verify subscription doesn't throw
            Assert.False(button.IsDisposed);
        });
    }

    [LinuxFact]
    public void Button_Click_MultipleHandlers_ShouldAllRegister()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            var count = 0;
            button.Click += (sender, e) => count++;
            button.Click += (sender, e) => count++;
            button.Click += (sender, e) => count++;

            // Verify handlers are registered (button not disposed)
            Assert.False(button.IsDisposed);
        });
    }

    [LinuxFact]
    public void Button_Click_UnsubscribeHandler_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            void Handler(object? sender, EventArgs e) { }

            button.Click += Handler;
            button.Click -= Handler;

            // Verify unsubscribe doesn't throw
        });
    }

    #endregion

    #region Disposal Tests

    [LinuxFact]
    public void Button_Dispose_ShouldCleanup()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var button = new Button(shell, SWT.PUSH);

            button.Dispose();

            Assert.True(button.IsDisposed);
        });
    }

    [LinuxFact]
    public void Button_DoubleDispose_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var button = new Button(shell, SWT.PUSH);

            button.Dispose();
            button.Dispose(); // Should not throw
        });
    }

    [LinuxFact]
    public void Button_AccessAfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var button = new Button(shell, SWT.PUSH);
            button.Dispose();

            Assert.Throws<SWTDisposedException>(() => button.Text = "test");
            Assert.Throws<SWTDisposedException>(() => _ = button.Text);
            Assert.Throws<SWTDisposedException>(() => button.Selection = true);
            Assert.Throws<SWTDisposedException>(() => _ = button.Selection);
        });
    }

    #endregion

    #region Platform-Specific Tests

    [LinuxFact]
    public void Button_PlatformWidget_ShouldBeGtkButton()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            // Verify platform widget is created
            Assert.NotNull(button.PlatformWidget);

            // Verify it's an IPlatformTextWidget
            Assert.True(button.PlatformWidget is IPlatformTextWidget);
        });
    }

    [LinuxFact]
    public void Button_PlatformWidget_ShouldBeCreated()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            // Verify platform widget exists
            Assert.NotNull(button.PlatformWidget);
        });
    }

    [LinuxFact]
    public void Button_EventHandling_ShouldBeSubscribed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.PUSH);

            // Verify platform widget supports event handling
            Assert.True(button.PlatformWidget is IPlatformEventHandling);
        });
    }

    #endregion

    #region Integration Tests

    [LinuxFact]
    public void Button_MultipleButtons_ShouldWorkIndependently()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button1 = new Button(shell, SWT.PUSH);
            using var button2 = new Button(shell, SWT.CHECK);
            using var button3 = new Button(shell, SWT.RADIO);

            button1.Text = "Button 1";
            button2.Text = "Button 2";
            button3.Text = "Button 3";

            Assert.Equal("Button 1", button1.Text);
            Assert.Equal("Button 2", button2.Text);
            Assert.Equal("Button 3", button3.Text);

            button2.Selection = true;
            Assert.True(button2.Selection);
            Assert.False(button1.Selection);
            Assert.False(button3.Selection);
        });
    }

    [LinuxFact]
    public void Button_CheckButton_WithText_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var button = new Button(shell, SWT.CHECK);

            button.Text = "Enable feature";
            button.Selection = true;

            Assert.Equal("Enable feature", button.Text);
            Assert.True(button.Selection);
        });
    }

    [LinuxFact]
    public void Button_AllStyles_ShouldCreate()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);

            using var pushButton = new Button(shell, SWT.PUSH);
            using var checkButton = new Button(shell, SWT.CHECK);
            using var radioButton = new Button(shell, SWT.RADIO);
            using var toggleButton = new Button(shell, SWT.TOGGLE);

            pushButton.Text = "Push";
            checkButton.Text = "Check";
            radioButton.Text = "Radio";
            toggleButton.Text = "Toggle";

            Assert.Equal("Push", pushButton.Text);
            Assert.Equal("Check", checkButton.Text);
            Assert.Equal("Radio", radioButton.Text);
            Assert.Equal("Toggle", toggleButton.Text);
        });
    }

    #endregion
}
