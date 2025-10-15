using Xunit;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.Platform;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// Focused tests for Label widget on Linux.
/// Tests label text display, alignment, and separator functionality.
/// </summary>
[Collection("Cross-Platform Tests")]
public class LinuxLabelTests : TestBase
{
    public LinuxLabelTests(DisplayFixture fixture) : base(fixture) { }

    #region Creation Tests

    [LinuxFact]
    public void Label_Create_WithNoneStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);
            Assert.NotNull(label);
            Assert.False(label.IsDisposed);
        });
    }

    [LinuxFact]
    public void Label_Create_WithLeftStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.LEFT);
            Assert.NotNull(label);
        });
    }

    [LinuxFact]
    public void Label_Create_WithCenterStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.CENTER);
            Assert.NotNull(label);
        });
    }

    [LinuxFact]
    public void Label_Create_WithRightStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.RIGHT);
            Assert.NotNull(label);
        });
    }

    [LinuxFact]
    public void Label_Create_WithSeparatorHorizontal_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.SEPARATOR | SWT.HORIZONTAL);
            Assert.NotNull(label);
        });
    }

    [LinuxFact]
    public void Label_Create_WithSeparatorVertical_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.SEPARATOR | SWT.VERTICAL);
            Assert.NotNull(label);
        });
    }

    #endregion

    #region Text Property Tests

    [LinuxFact]
    public void Label_Text_GetSet_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            label.Text = "Test Label";
            Assert.Equal("Test Label", label.Text);
        });
    }

    [LinuxFact]
    public void Label_Text_SetEmpty_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            label.Text = "";
            Assert.Equal("", label.Text);
        });
    }

    [LinuxFact]
    public void Label_Text_SetNull_ShouldConvertToEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            label.Text = null!;
            Assert.Equal("", label.Text);
        });
    }

    [LinuxFact]
    public void Label_Text_WithUnicode_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            label.Text = "Hello 世界 🌍";
            Assert.Equal("Hello 世界 🌍", label.Text);
        });
    }

    [LinuxFact]
    public void Label_Text_MultipleChanges_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            label.Text = "First";
            Assert.Equal("First", label.Text);

            label.Text = "Second";
            Assert.Equal("Second", label.Text);

            label.Text = "Third";
            Assert.Equal("Third", label.Text);
        });
    }

    #endregion

    #region Disposal Tests

    [LinuxFact]
    public void Label_Dispose_ShouldCleanup()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var label = new Label(shell, SWT.NONE);

            label.Dispose();

            Assert.True(label.IsDisposed);
        });
    }

    [LinuxFact]
    public void Label_DoubleDispose_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var label = new Label(shell, SWT.NONE);

            label.Dispose();
            label.Dispose(); // Should not throw
        });
    }

    [LinuxFact]
    public void Label_AccessAfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var label = new Label(shell, SWT.NONE);
            label.Dispose();

            Assert.Throws<SWTDisposedException>(() => label.Text = "test");
            Assert.Throws<SWTDisposedException>(() => _ = label.Text);
        });
    }

    #endregion

    #region Platform-Specific Tests

    [LinuxFact]
    public void Label_PlatformWidget_ShouldBeGtkLabel()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            // Verify platform widget is created
            Assert.NotNull(label.PlatformWidget);

            // Verify it's an IPlatformTextWidget
            Assert.True(label.PlatformWidget is IPlatformTextWidget);
        });
    }

    [LinuxFact]
    public void Label_PlatformWidget_ShouldBeCreated()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label = new Label(shell, SWT.NONE);

            // Verify platform widget exists
            Assert.NotNull(label.PlatformWidget);
        });
    }

    #endregion

    #region Integration Tests

    [LinuxFact]
    public void Label_MultipleLabels_ShouldWorkIndependently()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var label1 = new Label(shell, SWT.LEFT);
            using var label2 = new Label(shell, SWT.CENTER);
            using var label3 = new Label(shell, SWT.RIGHT);

            label1.Text = "Label 1";
            label2.Text = "Label 2";
            label3.Text = "Label 3";

            Assert.Equal("Label 1", label1.Text);
            Assert.Equal("Label 2", label2.Text);
            Assert.Equal("Label 3", label3.Text);

            // Change one label
            label2.Text = "Modified";
            Assert.Equal("Label 1", label1.Text);
            Assert.Equal("Modified", label2.Text);
            Assert.Equal("Label 3", label3.Text);
        });
    }

    [LinuxFact]
    public void Label_AllAlignments_ShouldCreate()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);

            using var leftLabel = new Label(shell, SWT.LEFT);
            using var centerLabel = new Label(shell, SWT.CENTER);
            using var rightLabel = new Label(shell, SWT.RIGHT);

            leftLabel.Text = "Left";
            centerLabel.Text = "Center";
            rightLabel.Text = "Right";

            Assert.Equal("Left", leftLabel.Text);
            Assert.Equal("Center", centerLabel.Text);
            Assert.Equal("Right", rightLabel.Text);
        });
    }

    [LinuxFact]
    public void Label_AllStyles_ShouldCreate()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);

            using var normal = new Label(shell, SWT.NONE);
            using var left = new Label(shell, SWT.LEFT);
            using var center = new Label(shell, SWT.CENTER);
            using var right = new Label(shell, SWT.RIGHT);
            using var hSep = new Label(shell, SWT.SEPARATOR | SWT.HORIZONTAL);
            using var vSep = new Label(shell, SWT.SEPARATOR | SWT.VERTICAL);

            normal.Text = "Normal";
            left.Text = "Left";
            center.Text = "Center";
            right.Text = "Right";

            Assert.Equal("Normal", normal.Text);
            Assert.Equal("Left", left.Text);
            Assert.Equal("Center", center.Text);
            Assert.Equal("Right", right.Text);
        });
    }

    #endregion
}
