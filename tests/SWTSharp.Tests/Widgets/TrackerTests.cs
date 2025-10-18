using Xunit;
using SWTSharp;
using SWTSharp.Tests.Infrastructure;
using SWTSharp.Graphics;

namespace SWTSharp.Tests.Widgets;

/// <summary>
/// Comprehensive unit tests for Tracker widget.
/// </summary>
public class TrackerTests : TestBase
{
    public TrackerTests(DisplayFixture displayFixture) : base(displayFixture) { }

    #region Creation Tests

    [Fact]
    public void Tracker_Create_WithStyle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);
            Assert.NotNull(tracker);
        });
    }

    [Fact]
    public void Tracker_Create_WithParent_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            using var tracker = new Tracker(shell, SWT.RESIZE);
            Assert.NotNull(tracker);
        });
    }

    [Fact]
    public void Tracker_Create_WithDifferentStyles_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            int[] styles = { SWT.NONE, SWT.RESIZE, SWT.LEFT, SWT.RIGHT, SWT.UP, SWT.DOWN };

            foreach (var style in styles)
            {
                using var tracker = new Tracker(style);
                Assert.NotNull(tracker);
            }
        });
    }

    [Fact]
    public void Tracker_Create_WithNullParent_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(null, SWT.RESIZE);
            Assert.NotNull(tracker);
        });
    }

    #endregion

    #region Rectangles Property Tests

    [Fact]
    public void Tracker_Rectangles_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            var rectangles = new[]
            {
                new Rectangle(10, 10, 100, 100),
                new Rectangle(150, 150, 50, 50)
            };

            tracker.Rectangles = rectangles;
            var result = tracker.Rectangles;

            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal(10, result[0].X);
            Assert.Equal(10, result[0].Y);
            Assert.Equal(100, result[0].Width);
            Assert.Equal(100, result[0].Height);
        });
    }

    [Fact]
    public void Tracker_Rectangles_WithEmptyArray_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Rectangles = Array.Empty<Rectangle>();
            var result = tracker.Rectangles;

            Assert.NotNull(result);
            Assert.Empty(result);
        });
    }

    [Fact]
    public void Tracker_Rectangles_WithNull_ShouldSetEmptyArray()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Rectangles = null!;
            var result = tracker.Rectangles;

            Assert.NotNull(result);
            Assert.Empty(result);
        });
    }

    [Fact]
    public void Tracker_Rectangles_DefaultShouldBeEmpty()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            var result = tracker.Rectangles;

            Assert.NotNull(result);
            Assert.Empty(result);
        });
    }

    [Fact]
    public void Tracker_Rectangles_SingleRectangle_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.RESIZE);

            var rectangle = new Rectangle(50, 50, 200, 150);
            tracker.Rectangles = new[] { rectangle };

            var result = tracker.Rectangles;

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(50, result[0].X);
            Assert.Equal(50, result[0].Y);
            Assert.Equal(200, result[0].Width);
            Assert.Equal(150, result[0].Height);
        });
    }

    #endregion

    #region Stippled Property Tests

    [Fact]
    public void Tracker_Stippled_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Stippled = true;
            Assert.True(tracker.Stippled);

            tracker.Stippled = false;
            Assert.False(tracker.Stippled);
        });
    }

    [Fact]
    public void Tracker_Stippled_DefaultShouldBeFalse()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            Assert.False(tracker.Stippled);
        });
    }

    #endregion

    #region CursorType Property Tests

    [Fact]
    public void Tracker_CursorType_ShouldGetAndSet()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.CursorType = SWT.CURSOR_ARROW;
            Assert.Equal(SWT.CURSOR_ARROW, tracker.CursorType);

            tracker.CursorType = SWT.CURSOR_CROSS;
            Assert.Equal(SWT.CURSOR_CROSS, tracker.CursorType);
        });
    }

    [Fact]
    public void Tracker_CursorType_DefaultShouldBeNone()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            Assert.Equal(SWT.NONE, tracker.CursorType);
        });
    }

    #endregion

    #region Open/Close Tests

    [Fact]
    public void Tracker_Close_WithoutOpen_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            // Should not throw even if never opened
            tracker.Close();
        });
    }

    [Fact]
    public void Tracker_Close_MultipleTimes_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Close();
            tracker.Close();
            tracker.Close();

            // Multiple closes should be safe
        });
    }

    #endregion

    #region Event Tests

    [Fact]
    public void Tracker_MouseMove_CanBeSubscribed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            bool eventFired = false;
            tracker.MouseMove += (sender, e) => eventFired = true;

            // Event subscription should work without throwing
            Assert.NotNull(tracker);
        });
    }

    [Fact]
    public void Tracker_Resize_CanBeSubscribed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.RESIZE);

            bool eventFired = false;
            tracker.Resize += (sender, e) => eventFired = true;

            // Event subscription should work without throwing
            Assert.NotNull(tracker);
        });
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Tracker_Dispose_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);

            tracker.Dispose();

            // Verify no exception thrown
        });
    }

    [Fact]
    public void Tracker_Dispose_MultipleTimes_ShouldNotThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);

            tracker.Dispose();
            tracker.Dispose();
            tracker.Dispose();

            // Multiple disposes should be safe
        });
    }

    [Fact]
    public void Tracker_GetRectangles_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = tracker.Rectangles);
        });
    }

    [Fact]
    public void Tracker_SetRectangles_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
                tracker.Rectangles = new[] { new Rectangle(0, 0, 10, 10) });
        });
    }

    [Fact]
    public void Tracker_GetStippled_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = tracker.Stippled);
        });
    }

    [Fact]
    public void Tracker_SetStippled_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tracker.Stippled = true);
        });
    }

    [Fact]
    public void Tracker_GetCursorType_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _ = tracker.CursorType);
        });
    }

    [Fact]
    public void Tracker_SetCursorType_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tracker.CursorType = SWT.CURSOR_ARROW);
        });
    }

    [Fact]
    public void Tracker_Open_AfterDispose_ShouldThrow()
    {
        RunOnUIThread(() =>
        {
            var tracker = new Tracker(SWT.NONE);
            tracker.Dispose();

            Assert.Throws<ObjectDisposedException>(() => tracker.Open());
        });
    }

    [Fact]
    public void Tracker_Dispose_WithParent_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var tracker = new Tracker(shell, SWT.RESIZE);

            tracker.Dispose();

            // Verify no exception thrown
        });
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public void Tracker_MultipleRectangles_ShouldHandleCorrectly()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            var rectangles = new[]
            {
                new Rectangle(0, 0, 50, 50),
                new Rectangle(100, 100, 75, 75),
                new Rectangle(200, 200, 100, 100),
                new Rectangle(350, 350, 25, 25)
            };

            tracker.Rectangles = rectangles;
            var result = tracker.Rectangles;

            Assert.Equal(4, result.Length);
            Assert.Equal(50, result[0].Width);
            Assert.Equal(75, result[1].Width);
            Assert.Equal(100, result[2].Width);
            Assert.Equal(25, result[3].Width);
        });
    }

    [Fact]
    public void Tracker_PropertyChanges_BeforeOpen_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.RESIZE);

            tracker.Rectangles = new[] { new Rectangle(10, 10, 100, 100) };
            tracker.Stippled = true;
            tracker.CursorType = SWT.CURSOR_SIZEALL;

            Assert.Single(tracker.Rectangles);
            Assert.True(tracker.Stippled);
            Assert.Equal(SWT.CURSOR_SIZEALL, tracker.CursorType);
        });
    }

    [Fact]
    public void Tracker_UsingStatement_ShouldDisposeCorrectly()
    {
        RunOnUIThread(() =>
        {
            Tracker? tracker;
            using (tracker = new Tracker(SWT.NONE))
            {
                tracker.Rectangles = new[] { new Rectangle(0, 0, 50, 50) };
            }

            // After using block, should be disposed
            Assert.Throws<ObjectDisposedException>(() => _ = tracker.Rectangles);
        });
    }

    #endregion

    #region Bounds and Sizing Tests

    [Fact]
    public void Tracker_Rectangle_ZeroSize_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Rectangles = new[] { new Rectangle(10, 10, 0, 0) };
            var result = tracker.Rectangles;

            Assert.Single(result);
            Assert.Equal(0, result[0].Width);
            Assert.Equal(0, result[0].Height);
        });
    }

    [Fact]
    public void Tracker_Rectangle_NegativeCoordinates_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Rectangles = new[] { new Rectangle(-50, -50, 100, 100) };
            var result = tracker.Rectangles;

            Assert.Single(result);
            Assert.Equal(-50, result[0].X);
            Assert.Equal(-50, result[0].Y);
        });
    }

    [Fact]
    public void Tracker_Rectangle_LargeValues_ShouldSucceed()
    {
        RunOnUIThread(() =>
        {
            using var tracker = new Tracker(SWT.NONE);

            tracker.Rectangles = new[] { new Rectangle(0, 0, 10000, 10000) };
            var result = tracker.Rectangles;

            Assert.Single(result);
            Assert.Equal(10000, result[0].Width);
            Assert.Equal(10000, result[0].Height);
        });
    }

    #endregion
}
