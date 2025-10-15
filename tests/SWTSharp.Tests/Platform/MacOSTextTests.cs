using System.Runtime.InteropServices;
using Xunit;
using SWTSharp;
using SWTSharp.Platform.MacOS;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// macOS-specific tests for MacOSText widget using NSTextField via Objective-C runtime.
/// Tests text input, selection, and read-only functionality.
/// </summary>
public class MacOSTextTests : WidgetTestBase
{
    public MacOSTextTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void MacOSText_Create_ShouldInitialize()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            // Text widget should be created successfully
            Assert.NotNull(text);
            Assert.False(text.IsDisposed);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_GetText_InitialState_ShouldBeEmpty()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            Assert.Equal(string.Empty, text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_SetText_ShouldUpdateContent()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "Hello World";
            Assert.Equal("Hello World", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_SetText_WithUnicode_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "Hello 世界 🌍";
            Assert.Equal("Hello 世界 🌍", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_SetText_MultipleChanges_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "First";
            Assert.Equal("First", text.TextContent);

            text.TextContent = "Second";
            Assert.Equal("Second", text.TextContent);

            text.TextContent = "Third";
            Assert.Equal("Third", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_SetText_Empty_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "Test";
            text.TextContent = "";
            Assert.Equal("", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_SetText_Null_ShouldConvertToEmpty()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            text.TextContent = null!;
            Assert.Equal("", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_ReadOnly_ShouldPreventEditing()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.READ_ONLY);

            text.TextContent = "Read Only";
            Assert.Equal("Read Only", text.TextContent);

            // Verify NSTextField is not editable
            var handle = GetNSTextFieldHandle(text);
            Assert.NotEqual(IntPtr.Zero, handle);

            // Query isEditable selector
            var isEditableSel = sel_registerName("isEditable");
            bool isEditable = objc_msgSend_bool(handle, isEditableSel);
            Assert.False(isEditable);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_Editable_ShouldAllowEditing()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            // Verify NSTextField is editable by default
            var handle = GetNSTextFieldHandle(text);
            Assert.NotEqual(IntPtr.Zero, handle);

            var isEditableSel = sel_registerName("isEditable");
            bool isEditable = objc_msgSend_bool(handle, isEditableSel);
            Assert.True(isEditable);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_MultiLine_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.MULTI);

            Assert.NotNull(text);
            text.TextContent = "Line 1\nLine 2\nLine 3";
            Assert.Equal("Line 1\nLine 2\nLine 3", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_Password_ShouldCreate()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.PASSWORD);

            Assert.NotNull(text);
            text.TextContent = "secret";
            // Password fields still store the actual text
            Assert.Equal("secret", text.TextContent);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_Dispose_ShouldCleanup()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);
            text.TextContent = "Test";

            text.Dispose();

            Assert.True(text.IsDisposed);
        });
    }

    [Fact]
    public void MacOSText_DoubleDispose_ShouldNotThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            text.Dispose();
            text.Dispose(); // Should not throw
        });
    }

    [Fact]
    public void MacOSText_AccessAfterDispose_ShouldThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);
            text.Dispose();

            Assert.Throws<SWTDisposedException>(() => text.TextContent = "test");
            Assert.Throws<SWTDisposedException>(() => _ = text.TextContent);
        });
    }

    [Fact]
    public void MacOSText_MultipleTextFields_ShouldWorkIndependently()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();

            var text1 = new Text(shell, SWT.SINGLE);
            var text2 = new Text(shell, SWT.SINGLE);
            var text3 = new Text(shell, SWT.SINGLE);

            text1.TextContent = "Field 1";
            text2.TextContent = "Field 2";
            text3.TextContent = "Field 3";

            Assert.Equal("Field 1", text1.TextContent);
            Assert.Equal("Field 2", text2.TextContent);
            Assert.Equal("Field 3", text3.TextContent);

            // Change one field
            text2.TextContent = "Modified";
            Assert.Equal("Field 1", text1.TextContent);
            Assert.Equal("Modified", text2.TextContent);
            Assert.Equal("Field 3", text3.TextContent);

            text1.Dispose();
            text2.Dispose();
            text3.Dispose();
        });
    }

    [Fact]
    public void MacOSText_NSTextFieldHandle_ShouldBeValid()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text = new Text(shell, SWT.SINGLE);

            var handle = GetNSTextFieldHandle(text);
            Assert.NotEqual(IntPtr.Zero, handle);

            text.Dispose();
        });
    }

    [Fact]
    public void MacOSText_NSTextFieldHandle_ShouldBeUnique()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var text1 = new Text(shell, SWT.SINGLE);
            var text2 = new Text(shell, SWT.SINGLE);

            var handle1 = GetNSTextFieldHandle(text1);
            var handle2 = GetNSTextFieldHandle(text2);

            Assert.NotEqual(IntPtr.Zero, handle1);
            Assert.NotEqual(IntPtr.Zero, handle2);
            Assert.NotEqual(handle1, handle2);

            text1.Dispose();
            text2.Dispose();
        });
    }

    // Objective-C P/Invoke declarations for testing
    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern bool objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    /// <summary>
    /// Helper to get the native NSTextField handle from a Text widget.
    /// Uses reflection to access internal PlatformWidget property.
    /// </summary>
    private static IntPtr GetNSTextFieldHandle(Text text)
    {
        var platformWidget = typeof(Text).BaseType?.GetProperty("PlatformWidget",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(text);
        return (platformWidget as MacOSWidget)?.GetNativeHandle() ?? IntPtr.Zero;
    }
}
