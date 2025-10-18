using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for DateTime widget.
/// </summary>
public class DateTimeTests : WidgetTestBase
{
    public DateTimeTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void DateTime_Create_ShouldSucceed()
    {
        AssertWidgetCreation(shell => new DateTime(shell, SWT.DATE));
    }

    [Fact]
    public void DateTime_Create_WithStyles_ShouldSucceed()
    {
        AssertWidgetStyles(
            (shell, style) => new DateTime(shell, style),
            SWT.DATE,
            SWT.TIME,
            SWT.CALENDAR,
            SWT.DATE | SWT.SHORT,
            SWT.DATE | SWT.MEDIUM,
            SWT.DATE | SWT.LONG,
            SWT.TIME | SWT.SHORT,
            SWT.TIME | SWT.MEDIUM,
            SWT.TIME | SWT.LONG
        );
    }

    [Fact]
    public void DateTime_InitialValue_ShouldBeCurrentDateTime()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);
            var now = System.DateTime.Now;

            // Year should be current year
            Assert.Equal(now.Year, dateTime.Year);

            // Month should be current month (0-based)
            Assert.Equal(now.Month - 1, dateTime.Month);

            // Day should be current day
            Assert.Equal(now.Day, dateTime.Day);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Year_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            dateTime.Year = 2023;
            Assert.Equal(2023, dateTime.Year);

            dateTime.Year = 2025;
            Assert.Equal(2025, dateTime.Year);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Year_InvalidValue_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Year = 1751);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Year = 10000);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Year = -1);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Month_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            dateTime.Month = 0; // January
            Assert.Equal(0, dateTime.Month);

            dateTime.Month = 11; // December
            Assert.Equal(11, dateTime.Month);

            dateTime.Month = 5; // June
            Assert.Equal(5, dateTime.Month);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Month_InvalidValue_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Month = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Month = 12);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Month = 100);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Day_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            dateTime.Day = 1;
            Assert.Equal(1, dateTime.Day);

            dateTime.Day = 15;
            Assert.Equal(15, dateTime.Day);

            dateTime.Day = 31;
            Assert.Equal(31, dateTime.Day);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Day_InvalidValue_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Day = 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Day = 32);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Day = -1);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Hours_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            dateTime.Hours = 0;
            Assert.Equal(0, dateTime.Hours);

            dateTime.Hours = 12;
            Assert.Equal(12, dateTime.Hours);

            dateTime.Hours = 23;
            Assert.Equal(23, dateTime.Hours);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Hours_InvalidValue_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Hours = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Hours = 24);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Hours = 100);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Minutes_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            dateTime.Minutes = 0;
            Assert.Equal(0, dateTime.Minutes);

            dateTime.Minutes = 30;
            Assert.Equal(30, dateTime.Minutes);

            dateTime.Minutes = 59;
            Assert.Equal(59, dateTime.Minutes);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Minutes_InvalidValue_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Minutes = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Minutes = 60);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Minutes = 100);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Seconds_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            dateTime.Seconds = 0;
            Assert.Equal(0, dateTime.Seconds);

            dateTime.Seconds = 30;
            Assert.Equal(30, dateTime.Seconds);

            dateTime.Seconds = 59;
            Assert.Equal(59, dateTime.Seconds);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Seconds_InvalidValue_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Seconds = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Seconds = 60);
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.Seconds = 100);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_SetDate_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            dateTime.SetDate(2024, 5, 15); // June 15, 2024

            Assert.Equal(2024, dateTime.Year);
            Assert.Equal(5, dateTime.Month);
            Assert.Equal(15, dateTime.Day);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_SetDate_InvalidValues_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.SetDate(1000, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.SetDate(2024, 12, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.SetDate(2024, 0, 32));

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_SetTime_ShouldWork()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            dateTime.SetTime(14, 30, 45);

            Assert.Equal(14, dateTime.Hours);
            Assert.Equal(30, dateTime.Minutes);
            Assert.Equal(45, dateTime.Seconds);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_SetTime_InvalidValues_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.SetTime(24, 0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.SetTime(0, 60, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => dateTime.SetTime(0, 0, 60));

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_GetDate_ShouldReturnCorrectValues()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            dateTime.SetDate(2023, 11, 25);
            var (year, month, day) = dateTime.GetDate();

            Assert.Equal(2023, year);
            Assert.Equal(11, month);
            Assert.Equal(25, day);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_GetTime_ShouldReturnCorrectValues()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);

            dateTime.SetTime(16, 45, 30);
            var (hours, minutes, seconds) = dateTime.GetTime();

            Assert.Equal(16, hours);
            Assert.Equal(45, minutes);
            Assert.Equal(30, seconds);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Value_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            var testDate = new System.DateTime(2024, 6, 15, 14, 30, 45);
            dateTime.Value = testDate;

            Assert.Equal(2024, dateTime.Year);
            Assert.Equal(5, dateTime.Month); // 0-based, so June = 5
            Assert.Equal(15, dateTime.Day);
            Assert.Equal(14, dateTime.Hours);
            Assert.Equal(30, dateTime.Minutes);
            Assert.Equal(45, dateTime.Seconds);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_Parent_ShouldBeCorrect()
    {
        AssertControlParent(shell => new DateTime(shell, SWT.DATE));
    }

    [Fact]
    public void DateTime_Dispose_ShouldSetIsDisposed()
    {
        AssertWidgetDisposal(shell => new DateTime(shell, SWT.DATE));
    }

    [Fact]
    public void DateTime_SetYear_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new DateTime(shell, SWT.DATE),
            dt => dt.Year = 2024
        );
    }

    [Fact]
    public void DateTime_GetYear_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);
            dateTime.Dispose();

            Assert.Throws<SWTDisposedException>(() => _ = dateTime.Year);
        });
    }

    [Fact]
    public void DateTime_SetDate_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new DateTime(shell, SWT.DATE),
            dt => dt.SetDate(2024, 0, 1)
        );
    }

    [Fact]
    public void DateTime_SetTime_AfterDispose_ShouldThrow()
    {
        AssertThrowsAfterDisposal(
            shell => new DateTime(shell, SWT.TIME),
            dt => dt.SetTime(12, 0, 0)
        );
    }

    [Fact]
    public void DateTime_Data_ShouldGetAndSet()
    {
        AssertWidgetData(shell => new DateTime(shell, SWT.DATE));
    }

    [Fact]
    public void DateTime_Visible_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new DateTime(shell, SWT.DATE),
            dt => dt.Visible,
            (dt, v) => dt.Visible = v,
            false
        );
    }

    [Fact]
    public void DateTime_Enabled_ShouldGetAndSet()
    {
        AssertPropertyGetSet(
            shell => new DateTime(shell, SWT.DATE),
            dt => dt.Enabled,
            (dt, v) => dt.Enabled = v,
            false
        );
    }

    [Fact]
    public void DateTime_Display_ShouldMatchParent()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);

            Assert.Same(shell.Display, dateTime.Display);

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_SelectionChanged_ShouldFireOnDateChange()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.DATE);
            bool eventFired = false;

            dateTime.SelectionChanged += (sender, e) =>
            {
                eventFired = true;
            };

            dateTime.Year = 2024;

            Assert.True(eventFired, "SelectionChanged event should fire when date changes");

            dateTime.Dispose();
        });
    }

    [Fact]
    public void DateTime_SelectionChanged_ShouldFireOnTimeChange()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var dateTime = new DateTime(shell, SWT.TIME);
            bool eventFired = false;

            dateTime.SelectionChanged += (sender, e) =>
            {
                eventFired = true;
            };

            dateTime.Hours = 14;

            Assert.True(eventFired, "SelectionChanged event should fire when time changes");

            dateTime.Dispose();
        });
    }
}
