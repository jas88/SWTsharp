using Xunit;
using SWTSharp;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Base class for widget-specific tests with common widget testing patterns.
/// </summary>
public abstract class WidgetTestBase : TestBase
{
    /// <summary>
    /// Tests that a widget can be created without throwing exceptions.
    /// </summary>
    protected void AssertWidgetCreation<T>(Func<Shell, T> factory) where T : Widget
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);

        Assert.NotNull(widget);
        AssertNotDisposed(widget);
        Assert.Same(shell.Display, widget.Display);

        widget.Dispose();
        AssertDisposed(widget);
    }

    /// <summary>
    /// Tests that a control has correct parent relationship.
    /// </summary>
    protected void AssertControlParent<T>(Func<Shell, T> factory) where T : Control
    {
        using var shell = CreateTestShell();
        var control = factory(shell);

        Assert.NotNull(control);
        Assert.Same(shell, control.Parent);

        control.Dispose();
    }

    /// <summary>
    /// Tests widget disposal behavior.
    /// </summary>
    protected void AssertWidgetDisposal<T>(Func<Shell, T> factory) where T : Widget
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);

        AssertNotDisposed(widget);
        Assert.False(widget.IsDisposed);

        widget.Dispose();

        AssertDisposed(widget);
        Assert.True(widget.IsDisposed);
    }

    /// <summary>
    /// Tests that operations throw after widget disposal.
    /// </summary>
    protected void AssertThrowsAfterDisposal<T>(Func<Shell, T> factory, Action<T> operation) where T : Widget
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);
        widget.Dispose();

        Assert.Throws<SWTDisposedException>(() => operation(widget));
    }

    /// <summary>
    /// Tests a property getter and setter.
    /// </summary>
    protected void AssertPropertyGetSet<T, TProp>(
        Func<Shell, T> factory,
        Func<T, TProp> getter,
        Action<T, TProp> setter,
        TProp testValue) where T : Widget
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);

        setter(widget, testValue);
        var actualValue = getter(widget);

        Assert.Equal(testValue, actualValue);

        widget.Dispose();
    }

    /// <summary>
    /// Tests that a widget can be created with different style bits.
    /// </summary>
    protected void AssertWidgetStyles<T>(Func<Shell, int, T> factory, params int[] styles) where T : Widget
    {
        using var shell = CreateTestShell();

        foreach (var style in styles)
        {
            var widget = factory(shell, style);
            Assert.NotNull(widget);
            AssertNotDisposed(widget);
            widget.Dispose();
        }
    }

    /// <summary>
    /// Tests widget data property.
    /// </summary>
    protected void AssertWidgetData<T>(Func<Shell, T> factory) where T : Widget
    {
        using var shell = CreateTestShell();
        var widget = factory(shell);

        var testData = new { Name = "Test", Value = 42 };
        widget.Data = testData;

        Assert.Same(testData, widget.Data);

        widget.Dispose();
    }
}
