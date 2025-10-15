using System.Runtime.InteropServices;
using Xunit;
using SWTSharp;
using SWTSharp.Platform.MacOS;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// macOS-specific tests for MacOSLabel widget using NSTextField (non-editable) via Objective-C runtime.
/// Tests label text display and alignment functionality.
/// </summary>
public class MacOSLabelTests : WidgetTestBase
{
    public MacOSLabelTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void MacOSLabel_Create_ShouldInitialize()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            // Label should be created successfully
            Assert.NotNull(label);
            Assert.False(label.IsDisposed);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_GetText_InitialState_ShouldBeEmpty()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            Assert.Equal(string.Empty, label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_SetText_ShouldUpdateContent()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            label.Text = "Test Label";
            Assert.Equal("Test Label", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_SetText_WithUnicode_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            label.Text = "Hello 世界 🌍";
            Assert.Equal("Hello 世界 🌍", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_SetText_MultipleChanges_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            label.Text = "First";
            Assert.Equal("First", label.Text);

            label.Text = "Second";
            Assert.Equal("Second", label.Text);

            label.Text = "Third";
            Assert.Equal("Third", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_SetText_Empty_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            label.Text = "Test";
            label.Text = "";
            Assert.Equal("", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_SetText_Null_ShouldConvertToEmpty()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            label.Text = null!;
            Assert.Equal("", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_Alignment_Left_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.LEFT);

            label.Text = "Left aligned";
            Assert.Equal("Left aligned", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_Alignment_Center_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.CENTER);

            label.Text = "Center aligned";
            Assert.Equal("Center aligned", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_Alignment_Right_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.RIGHT);

            label.Text = "Right aligned";
            Assert.Equal("Right aligned", label.Text);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_Separator_Horizontal_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.SEPARATOR | SWT.HORIZONTAL);

            Assert.NotNull(label);
            Assert.False(label.IsDisposed);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_Separator_Vertical_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.SEPARATOR | SWT.VERTICAL);

            Assert.NotNull(label);
            Assert.False(label.IsDisposed);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_Dispose_ShouldCleanup()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);
            label.Text = "Test";

            label.Dispose();

            Assert.True(label.IsDisposed);
        });
    }

    [Fact]
    public void MacOSLabel_DoubleDispose_ShouldNotThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            label.Dispose();
            label.Dispose(); // Should not throw
        });
    }

    [Fact]
    public void MacOSLabel_AccessAfterDispose_ShouldThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);
            label.Dispose();

            Assert.Throws<SWTDisposedException>(() => label.Text = "test");
            Assert.Throws<SWTDisposedException>(() => _ = label.Text);
        });
    }

    [Fact]
    public void MacOSLabel_MultipleLabels_ShouldWorkIndependently()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();

            var label1 = new Label(shell, SWT.NONE);
            var label2 = new Label(shell, SWT.CENTER);
            var label3 = new Label(shell, SWT.RIGHT);

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

            label1.Dispose();
            label2.Dispose();
            label3.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_NSTextFieldHandle_ShouldBeValid()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label = new Label(shell, SWT.NONE);

            var handle = GetNSTextFieldHandle(label);
            Assert.NotEqual(IntPtr.Zero, handle);

            label.Dispose();
        });
    }

    [Fact]
    public void MacOSLabel_NSTextFieldHandle_ShouldBeUnique()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var label1 = new Label(shell, SWT.NONE);
            var label2 = new Label(shell, SWT.NONE);

            var handle1 = GetNSTextFieldHandle(label1);
            var handle2 = GetNSTextFieldHandle(label2);

            Assert.NotEqual(IntPtr.Zero, handle1);
            Assert.NotEqual(IntPtr.Zero, handle2);
            Assert.NotEqual(handle1, handle2);

            label1.Dispose();
            label2.Dispose();
        });
    }

    /// <summary>
    /// Helper to get the native NSTextField handle from a Label widget.
    /// Uses reflection to access internal PlatformWidget property.
    /// </summary>
    private static IntPtr GetNSTextFieldHandle(Label label)
    {
        var platformWidget = typeof(Label).BaseType?.GetProperty("PlatformWidget",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(label);
        return (platformWidget as MacOSWidget)?.GetNativeHandle() ?? IntPtr.Zero;
    }
}
