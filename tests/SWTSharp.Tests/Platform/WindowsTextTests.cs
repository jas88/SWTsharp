using Xunit;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.Platform;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// Focused tests for Text widget on Windows.
/// Tests text input, selection, read-only, and text limit functionality.
/// </summary>
[Collection("Cross-Platform Tests")]
public class WindowsTextTests : TestBase
{
    public WindowsTextTests(DisplayFixture fixture) : base(fixture) { }

    #region Creation Tests

    [WindowsFact]
    public void Text_Create_WithSingleStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);
            Assert.NotNull(text);
            Assert.False(text.IsDisposed);
        });
    }

    [WindowsFact]
    public void Text_Create_WithMultiStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.MULTI);
            Assert.NotNull(text);
        });
    }

    [WindowsFact]
    public void Text_Create_WithPasswordStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.PASSWORD);
            Assert.NotNull(text);
        });
    }

    [WindowsFact]
    public void Text_Create_WithReadOnlyStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.READ_ONLY);
            Assert.NotNull(text);
        });
    }

    #endregion

    #region Text Content Tests

    [WindowsFact]
    public void Text_GetText_InitialState_ShouldBeEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);
            Assert.Equal(string.Empty, text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_SetText_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "Hello World";
            Assert.Equal("Hello World", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_SetText_Empty_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "";
            Assert.Equal("", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_SetText_Null_ShouldConvertToEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextContent = null!;
            Assert.Equal("", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_SetText_WithUnicode_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "Hello 世界 🌍";
            Assert.Equal("Hello 世界 🌍", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_SetText_MultipleChanges_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextContent = "First";
            Assert.Equal("First", text.TextContent);

            text.TextContent = "Second";
            Assert.Equal("Second", text.TextContent);

            text.TextContent = "Third";
            Assert.Equal("Third", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_Append_ShouldAppendText()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.MULTI);

            text.TextContent = "Hello";
            text.Append(" World");
            Assert.Equal("Hello World", text.TextContent);
        });
    }

    #endregion

    #region Text Limit Tests

    [WindowsFact]
    public void Text_TextLimit_ShouldLimitText()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextLimit = 5;
            text.TextContent = "1234567890";
            Assert.Equal("12345", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_TextLimit_ZeroShouldRemoveLimit()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            text.TextLimit = 0;
            text.TextContent = "This is a long text without limit";
            Assert.Equal("This is a long text without limit", text.TextContent);
        });
    }

    #endregion

    #region Multi-Line Tests

    [WindowsFact]
    public void Text_MultiLine_WithNewlines_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.MULTI);

            text.TextContent = "Line 1\nLine 2\nLine 3";
            Assert.Equal("Line 1\nLine 2\nLine 3", text.TextContent);
        });
    }

    [WindowsFact]
    public void Text_MultiLine_WithWrap_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.MULTI | SWT.WRAP);

            text.TextContent = "This is a long line that should wrap";
            Assert.Equal("This is a long line that should wrap", text.TextContent);
        });
    }

    #endregion

    #region Disposal Tests

    [WindowsFact]
    public void Text_Dispose_ShouldCleanup()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var text = new Text(shell, SWT.SINGLE);

            text.Dispose();

            Assert.True(text.IsDisposed);
        });
    }

    [WindowsFact]
    public void Text_DoubleDispose_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var text = new Text(shell, SWT.SINGLE);

            text.Dispose();
            text.Dispose(); // Should not throw
        });
    }

    [WindowsFact]
    public void Text_AccessAfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            var text = new Text(shell, SWT.SINGLE);
            text.Dispose();

            Assert.Throws<SWTDisposedException>(() => text.TextContent = "test");
            Assert.Throws<SWTDisposedException>(() => _ = text.TextContent);
            Assert.Throws<SWTDisposedException>(() => text.Append("test"));
        });
    }

    #endregion

    #region Platform-Specific Tests

    [WindowsFact]
    public void Text_PlatformWidget_ShouldBeWin32Text()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            // Verify platform widget is created
            Assert.NotNull(text.PlatformWidget);

            // Verify it's an IPlatformTextInput
            Assert.True(text.PlatformWidget is IPlatformTextInput);
        });
    }

    [WindowsFact]
    public void Text_PlatformWidget_ShouldBeCreated()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text = new Text(shell, SWT.SINGLE);

            // Verify platform widget exists
            Assert.NotNull(text.PlatformWidget);
        });
    }

    #endregion

    #region Integration Tests

    [WindowsFact]
    public void Text_MultipleTextFields_ShouldWorkIndependently()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);
            using var text1 = new Text(shell, SWT.SINGLE);
            using var text2 = new Text(shell, SWT.MULTI);
            using var text3 = new Text(shell, SWT.PASSWORD);

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
        });
    }

    [WindowsFact]
    public void Text_AllStyles_ShouldCreate()
    {
        RunOnUIThread(() =>
        {
            using var shell = new Shell(Display);

            using var single = new Text(shell, SWT.SINGLE);
            using var multi = new Text(shell, SWT.MULTI);
            using var password = new Text(shell, SWT.PASSWORD);
            using var readOnly = new Text(shell, SWT.READ_ONLY);

            single.TextContent = "Single";
            multi.TextContent = "Multi";
            password.TextContent = "Pass";
            readOnly.TextContent = "ReadOnly";

            Assert.Equal("Single", single.TextContent);
            Assert.Equal("Multi", multi.TextContent);
            Assert.Equal("Pass", password.TextContent);
            Assert.Equal("ReadOnly", readOnly.TextContent);
        });
    }

    #endregion
}
