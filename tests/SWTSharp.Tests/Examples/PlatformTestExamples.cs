using Xunit;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.Events;

namespace SWTSharp.Tests.Examples;

/// <summary>
/// Example tests demonstrating platform-specific test infrastructure.
/// These are real working examples that can be used as templates.
/// </summary>
[Collection("Cross-Platform Tests")]
public class PlatformTestExamples : TestBase
{
    public PlatformTestExamples(DisplayFixture fixture) : base(fixture) { }

    #region Cross-Platform Tests

    [Fact]
    public void Example_BasicWidget_ShouldWork()
    {
        // This test runs on all platforms
        var shell = CreateTestShell();

        RunOnUIThread(() =>
        {
            shell.Text = "Test Shell";
            shell.SetSize(300, 200);
        });

        AssertNotDisposed(shell);
        Assert.Equal("Test Shell", shell.Text);
    }

    [Theory]
    [InlineData(SWT.PUSH, "Push Button")]
    [InlineData(SWT.CHECK, "Check Button")]
    [InlineData(SWT.RADIO, "Radio Button")]
    public void Example_ButtonStyles_ShouldWork(int style, string text)
    {
        // Theory tests work across platforms
        var shell = CreateTestShell();
        Button? button = null;

        RunOnUIThread(() =>
        {
            button = new Button(shell, style);
            button.Text = text;
        });

        Assert.NotNull(button);
        Assert.Equal(text, button.Text);
    }

    #endregion

    #region Platform-Specific Tests

    [WindowsFact]
    public void Example_WindowsOnly_ShouldSkipOnOtherPlatforms()
    {
        // This test only runs on Windows
        // Automatically skipped on Linux and macOS
        var shell = CreateTestShell();

        RunOnUIThread(() =>
        {
            shell.Text = "Windows Test";
        });

        Assert.Equal("Windows Test", shell.Text);
    }

    [LinuxFact]
    public void Example_LinuxOnly_ShouldSkipOnOtherPlatforms()
    {
        // This test only runs on Linux
        // Automatically skipped on Windows and macOS
        var shell = CreateTestShell();

        RunOnUIThread(() =>
        {
            shell.Text = "Linux Test";
        });

        Assert.Equal("Linux Test", shell.Text);
    }

    [MacOSFact]
    public void Example_MacOSOnly_ShouldSkipOnOtherPlatforms()
    {
        // This test only runs on macOS
        // Automatically skipped on Windows and Linux
        var shell = CreateTestShell();

        RunOnUIThread(() =>
        {
            shell.Text = "macOS Test";
        });

        Assert.Equal("macOS Test", shell.Text);
    }

    #endregion

    #region Skip Pattern Tests

    [FactSkipPlatform("macOS")]
    public void Example_SkipOnMacOS_ShouldRunOnWindowsAndLinux()
    {
        // This test runs on Windows and Linux only
        // Skipped on macOS
        var shell = CreateTestShell();
        Assert.NotNull(shell);
    }

    [FactSkipPlatform("Windows", "Linux")]
    public void Example_SkipOnMultiple_ShouldRunOnMacOSOnly()
    {
        // This test runs on macOS only
        // Skipped on Windows and Linux
        var shell = CreateTestShell();
        Assert.NotNull(shell);
    }

    [FactOnlyPlatform("Windows", "Linux")]
    public void Example_OnlyOnMultiple_ShouldSkipMacOS()
    {
        // This test runs on Windows and Linux only
        // Alternative syntax to SkipPlatform
        var shell = CreateTestShell();
        Assert.NotNull(shell);
    }

    #endregion

    #region TestHelpers Examples

    [Fact]
    public void Example_UsingTestHelpers_CreateWidgets()
    {
        var shell = CreateTestShell();

        // Create widgets using helpers
        var button = TestHelpers.CreateTestButton(shell, "Test Button");
        var label = TestHelpers.CreateTestLabel(shell, "Test Label");
        var composite = TestHelpers.CreateTestComposite(shell);

        // Assert widgets are created
        TestHelpers.AssertNotDisposed(button);
        TestHelpers.AssertNotDisposed(label);
        TestHelpers.AssertNotDisposed(composite);
    }

    [Fact]
    public void Example_UsingTestHelpers_EventTesting()
    {
        var shell = CreateTestShell();
        var button = TestHelpers.CreateTestButton(shell, "Click Me");

        bool eventFired = false;

        RunOnUIThread(() =>
        {
            button.Click += (s, e) => eventFired = true;
            button.NotifyListeners(SWT.Selection, new Event());
        });

        // Wait for event to fire
        TestHelpers.AssertCondition(
            () => eventFired,
            "Button click event should fire"
        );
    }

    [Fact]
    public void Example_UsingTestHelpers_WaitForCondition()
    {
        var shell = CreateTestShell();
        bool condition = false;

        // Simulate async operation
        RunOnUIThread(() =>
        {
            System.Threading.Tasks.Task.Delay(100).ContinueWith(_ =>
            {
                condition = true;
            });
        });

        // Wait for condition with timeout
        TestHelpers.AssertCondition(
            () => condition,
            System.TimeSpan.FromSeconds(1),
            "Condition should become true within 1 second"
        );
    }

    [Fact]
    public void Example_UsingTestHelpers_MeasurePerformance()
    {
        var shell = CreateTestShell();

        // Measure UI operation time
        var elapsed = TestHelpers.MeasureUITime(Display, () =>
        {
            for (int i = 0; i < 100; i++)
            {
                var button = new Button(shell, SWT.PUSH);
                button.Text = $"Button {i}";
            }
        });

        // Assert performance requirements
        Assert.True(elapsed < System.TimeSpan.FromSeconds(1),
            $"Creating 100 buttons should take less than 1 second, took {elapsed.TotalMilliseconds}ms");
    }

    #endregion

    #region Disposal Testing

    [Fact]
    public void Example_Disposal_WidgetShouldDispose()
    {
        var shell = CreateTestShell();
        Button? button = null;

        RunOnUIThread(() =>
        {
            button = new Button(shell, SWT.PUSH);
        });

        Assert.NotNull(button);
        TestHelpers.AssertNotDisposed(button);

        RunOnUIThread(() =>
        {
            button.Dispose();
        });

        TestHelpers.AssertDisposed(button);
    }

    [Fact]
    public void Example_Disposal_ParentDisposesChildren()
    {
        var shell = CreateTestShell();
        Button? button = null;

        RunOnUIThread(() =>
        {
            button = new Button(shell, SWT.PUSH);
        });

        Assert.NotNull(button);
        TestHelpers.AssertNotDisposed(button);

        RunOnUIThread(() =>
        {
            shell.Dispose();
        });

        // Both shell and button should be disposed
        TestHelpers.AssertDisposed(shell);
        TestHelpers.AssertDisposed(button);
    }

    #endregion
}
