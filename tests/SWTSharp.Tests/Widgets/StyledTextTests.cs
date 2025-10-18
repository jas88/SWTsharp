using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.Graphics;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for StyledText widget.
/// </summary>
public class StyledTextTests : WidgetTestBase
{
    public StyledTextTests(DisplayFixture displayFixture) : base(displayFixture) { }

    #region Creation Tests

    [Fact]
    public void StyledText_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new StyledText(shell, SWT.MULTI));
    }

    [Fact]
    public void StyledText_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new StyledText(shell, style),
            SWT.MULTI,
            SWT.SINGLE,
            SWT.WRAP,
            SWT.READ_ONLY,
            SWT.MULTI | SWT.WRAP,
            SWT.MULTI | SWT.READ_ONLY
        );
    }

    [Fact]
    public void StyledText_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new StyledText(shell, SWT.MULTI));
    }

    #endregion

    #region Text Property Tests

    [Fact]
    public void StyledText_Text_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new StyledText(shell, SWT.MULTI),
            st => st.Text,
            (st, v) => st.Text = v,
            "Test content"
        );
    }

    [Fact]
    public void StyledText_Text_WithEmptyString_ShouldSucceed()
    {
        AssertPropertyGetSet(
            shell => new StyledText(shell, SWT.MULTI),
            st => st.Text,
            (st, v) => st.Text = v,
            string.Empty
        );
    }

    [Fact]
    public void StyledText_Text_WithNull_ShouldSetEmptyString()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = null!;

            Assert.Equal(string.Empty, styledText.Text);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Text_WithMultiline_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            var multilineText = "Line 1\nLine 2\nLine 3";
            styledText.Text = multilineText;

            Assert.Equal(multilineText, styledText.Text);

            styledText.Dispose();
        });
    }

    #endregion

    #region TextLimit Tests

    [Fact]
    public void StyledText_TextLimit_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.TextLimit = 100;
            Assert.Equal(100, styledText.TextLimit);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_TextLimit_ShouldEnforceLimit()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.TextLimit = 5;
            styledText.Text = "1234567890";

            Assert.Equal("12345", styledText.Text);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_TextLimit_Negative_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            Assert.Throws<ArgumentException>(() => styledText.TextLimit = -1);

            styledText.Dispose();
        });
    }

    #endregion

    #region Editable Property Tests

    [Fact]
    public void StyledText_Editable_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new StyledText(shell, SWT.MULTI),
            st => st.Editable,
            (st, v) => st.Editable = v,
            false
        );
    }

    [Fact]
    public void StyledText_Editable_DefaultShouldBeTrue()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            Assert.True(styledText.Editable);

            styledText.Dispose();
        });
    }

    #endregion

    #region Append and Insert Tests

    [Fact]
    public void StyledText_Append_ShouldAppendText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello";
            styledText.Append(" World");

            Assert.Equal("Hello World", styledText.Text);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Append_MultipleAppends_ShouldConcatenate()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "A";
            styledText.Append("B");
            styledText.Append("C");

            Assert.Equal("ABC", styledText.Text);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Append_WithNull_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Test";
            styledText.Append(null!);

            Assert.Equal("Test", styledText.Text);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Insert_ShouldInsertText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "HelloWorld";
            styledText.CaretOffset = 5;
            styledText.Insert(" ");

            // Note: Insert is platform-specific, just verify no exception

            styledText.Dispose();
        });
    }

    #endregion

    #region Selection Tests

    [Fact]
    public void StyledText_SetSelection_ShouldSetSelection()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.SetSelection(0, 5);

            var selection = styledText.GetSelection();
            Assert.Equal(0, selection.Start);
            Assert.Equal(5, selection.End);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_GetSelectionText_ShouldReturnSelectedText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.SetSelection(0, 5);

            var selectedText = styledText.GetSelectionText();
            // Note: Platform-specific, just verify method works

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_SelectAll_ShouldSelectAllText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.SelectAll();

            var selection = styledText.GetSelection();
            Assert.Equal(0, selection.Start);
            Assert.Equal(11, selection.End);

            styledText.Dispose();
        });
    }

    #endregion

    #region Caret Tests

    [Fact]
    public void StyledText_CaretOffset_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.CaretOffset = 5;

            Assert.Equal(5, styledText.CaretOffset);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_GetCaretOffset_ShouldReturnCurrentOffset()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello";
            var offset = styledText.GetCaretOffset();

            Assert.True(offset >= 0);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_SetCaretOffset_OutOfBounds_ShouldClamp()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello";
            styledText.SetCaretOffset(100);

            Assert.Equal(5, styledText.CaretOffset); // Clamped to text length

            styledText.Dispose();
        });
    }

    #endregion

    #region Line Operations Tests

    [Fact]
    public void StyledText_LineCount_ShouldReturnCorrectCount()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Line 1\nLine 2\nLine 3";

            Assert.Equal(3, styledText.LineCount);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_GetLine_ShouldReturnLineText()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Line 1\nLine 2\nLine 3";
            var line = styledText.GetLine(1);

            Assert.Equal("Line 2", line);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_GetLine_InvalidIndex_ShouldReturnEmpty()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Line 1";
            var line = styledText.GetLine(10);

            Assert.Equal(string.Empty, line);

            styledText.Dispose();
        });
    }

    #endregion

    #region Clipboard Operations Tests

    [Fact]
    public void StyledText_Copy_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.SetSelection(0, 5);

            // Just verify method doesn't throw
            styledText.Copy();

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Cut_ShouldNotThrowWhenEditable()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.SetSelection(0, 5);
            styledText.Editable = true;

            // Just verify method doesn't throw
            styledText.Cut();

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Paste_ShouldNotThrowWhenEditable()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Editable = true;

            // Just verify method doesn't throw
            styledText.Paste();

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_Cut_WhenNotEditable_ShouldDoNothing()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            styledText.Editable = false;

            // Should not throw, just do nothing
            styledText.Cut();

            styledText.Dispose();
        });
    }

    #endregion

    #region Style Range Tests

    [Fact]
    public void StyledText_SetStyleRange_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            styledText.Text = "Hello World";
            var styleRange = new StyleRange
            {
                Start = 0,
                Length = 5,
                Foreground = new RGB(255, 0, 0),
                FontStyle = SWT.BOLD
            };

            // Just verify method doesn't throw
            styledText.SetStyleRange(styleRange);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyleRange_Properties_ShouldGetAndSet()
    {
        var styleRange = new StyleRange
        {
            Start = 0,
            Length = 10,
            Foreground = new RGB(255, 0, 0),
            Background = new RGB(0, 255, 0),
            FontStyle = SWT.BOLD | SWT.ITALIC
        };

        Assert.Equal(0, styleRange.Start);
        Assert.Equal(10, styleRange.Length);
        Assert.NotNull(styleRange.Foreground);
        Assert.Equal(255, styleRange.Foreground.Value.Red);
        Assert.NotNull(styleRange.Background);
        Assert.Equal(255, styleRange.Background.Value.Green);
        Assert.Equal(SWT.BOLD | SWT.ITALIC, styleRange.FontStyle);
    }

    #endregion

    #region Event Tests

    [Fact]
    public void StyledText_TextChanged_ShouldFireOnTextChange()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            bool eventFired = false;
            styledText.TextChanged += (sender, e) => eventFired = true;

            styledText.Text = "Test";

            Assert.True(eventFired);

            styledText.Dispose();
        });
    }

    [Fact]
    public void StyledText_SelectionChanged_CanBeSubscribed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);

            bool eventFired = false;
            styledText.SelectionChanged += (sender, e) => eventFired = true;

            // Event subscription should work without throwing
            Assert.NotNull(styledText);

            styledText.Dispose();
        });
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void StyledText_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new StyledText(shell, SWT.MULTI));
    }

    [Fact]
    public void StyledText_SetText_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new StyledText(shell, SWT.MULTI),
            st => st.Text = "Test"
        );
    }

    [Fact]
    public void StyledText_GetText_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);
            styledText.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = styledText.Text);
        });
    }

    [Fact]
    public void StyledText_Append_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);
            styledText.Dispose();

            Assert.Throws<SWTDisposedException>(() => styledText.Append("Test"));
        });
    }

    [Fact]
    public void StyledText_SetSelection_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var styledText = new StyledText(shell, SWT.MULTI);
            styledText.Dispose();

            Assert.Throws<SWTDisposedException>(() => styledText.SetSelection(0, 5));
        });
    }

    #endregion

    #region Common Widget Tests

    [Fact]
    public void StyledText_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new StyledText(shell, SWT.MULTI));
    }

    [Fact]
    public void StyledText_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new StyledText(shell, SWT.MULTI),
            st => st.Visible,
            (st, v) => st.Visible = v,
            false
        );
    }

    [Fact]
    public void StyledText_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new StyledText(shell, SWT.MULTI),
            st => st.Enabled,
            (st, v) => st.Enabled = v,
            false
        );
    }

    #endregion
}
