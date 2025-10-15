using System.Runtime.InteropServices;
using Xunit;
using SWTSharp;
using SWTSharp.Platform.MacOS;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// macOS-specific tests for MacOSComposite widget using NSView via Objective-C runtime.
/// Tests container creation, child management, bounds, visibility, and disposal.
/// </summary>
public class MacOSCompositeTests : WidgetTestBase
{
    public MacOSCompositeTests(DisplayFixture displayFixture) : base(displayFixture) { }

    #region Creation Tests

    [Fact]
    public void MacOSComposite_Create_ShouldInitialize()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            // Composite should be created successfully
            Assert.NotNull(composite);
            Assert.False(composite.IsDisposed);

            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_Create_WithBorder_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.BORDER);

            Assert.NotNull(composite);
            Assert.False(composite.IsDisposed);

            composite.Dispose();
        });
    }

    #endregion

    #region Child Management Tests

    [Fact]
    public void MacOSComposite_Children_InitiallyEmpty_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            Assert.Empty(composite.Children);

            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_AddChild_ShouldIncreaseChildCount()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);
            var button = new Button(composite, SWT.PUSH);

            Assert.Single(composite.Children);
            Assert.Contains(button, composite.Children);

            button.Dispose();
            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_AddMultipleChildren_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            var button1 = new Button(composite, SWT.PUSH);
            var button2 = new Button(composite, SWT.PUSH);
            var label = new Label(composite, SWT.NONE);

            Assert.Equal(3, composite.Children.Length);
            Assert.Contains(button1, composite.Children);
            Assert.Contains(button2, composite.Children);
            Assert.Contains(label, composite.Children);

            label.Dispose();
            button2.Dispose();
            button1.Dispose();
            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_RemoveChild_ShouldDecreaseChildCount()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);
            var button = new Button(composite, SWT.PUSH);

            Assert.Single(composite.Children);

            button.Dispose();
            Assert.Empty(composite.Children);

            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_NestedComposites_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var outerComposite = new Composite(shell, SWT.NONE);
            var innerComposite = new Composite(outerComposite, SWT.NONE);
            var button = new Button(innerComposite, SWT.PUSH);

            Assert.Single(outerComposite.Children);
            Assert.Contains(innerComposite, outerComposite.Children);
            Assert.Single(innerComposite.Children);
            Assert.Contains(button, innerComposite.Children);

            button.Dispose();
            innerComposite.Dispose();
            outerComposite.Dispose();
        });
    }

    #endregion

    #region Bounds and Visibility Tests

    [Fact]
    public void MacOSComposite_SetBounds_ShouldUpdateBounds()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            composite.SetBounds(10, 20, 100, 50);
            var bounds = composite.GetBounds();

            Assert.Equal(10, bounds.X);
            Assert.Equal(20, bounds.Y);
            Assert.Equal(100, bounds.Width);
            Assert.Equal(50, bounds.Height);

            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_Visibility_GetSet_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            // Default should be visible
            Assert.True(composite.Visible);

            composite.Visible = false;
            Assert.False(composite.Visible);

            composite.Visible = true;
            Assert.True(composite.Visible);

            composite.Dispose();
        });
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void MacOSComposite_Dispose_ShouldCleanup()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            composite.Dispose();

            Assert.True(composite.IsDisposed);
        });
    }

    [Fact]
    public void MacOSComposite_Dispose_ShouldDisposeChildren()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);
            var button = new Button(composite, SWT.PUSH);

            composite.Dispose();

            Assert.True(composite.IsDisposed);
            Assert.True(button.IsDisposed);
        });
    }

    [Fact]
    public void MacOSComposite_DoubleDispose_ShouldNotThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            composite.Dispose();
            composite.Dispose(); // Should not throw
        });
    }

    [Fact]
    public void MacOSComposite_AccessAfterDispose_ShouldThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);
            composite.Dispose();

            Assert.Throws<SWTDisposedException>(() => composite.SetBounds(0, 0, 100, 100));
            Assert.Throws<SWTDisposedException>(() => _ = composite.GetBounds());
            Assert.Throws<SWTDisposedException>(() => _ = composite.Children);
        });
    }

    #endregion

    #region Platform-Specific Tests

    [Fact]
    public void MacOSComposite_NSViewHandle_ShouldBeValid()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            var handle = GetNSViewHandle(composite);
            Assert.NotEqual(IntPtr.Zero, handle);

            composite.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_NSViewHandle_ShouldBeUnique()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite1 = new Composite(shell, SWT.NONE);
            var composite2 = new Composite(shell, SWT.NONE);

            var handle1 = GetNSViewHandle(composite1);
            var handle2 = GetNSViewHandle(composite2);

            Assert.NotEqual(IntPtr.Zero, handle1);
            Assert.NotEqual(IntPtr.Zero, handle2);
            Assert.NotEqual(handle1, handle2);

            composite1.Dispose();
            composite2.Dispose();
        });
    }

    [Fact]
    public void MacOSComposite_PlatformWidget_ShouldBeCreated()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite = new Composite(shell, SWT.NONE);

            // Verify platform widget exists
            Assert.NotNull(composite.PlatformWidget);

            composite.Dispose();
        });
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MacOSComposite_ComplexHierarchy_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var composite1 = new Composite(shell, SWT.NONE);
            var composite2 = new Composite(shell, SWT.NONE);

            var button1 = new Button(composite1, SWT.PUSH);
            var label1 = new Label(composite1, SWT.NONE);

            var button2 = new Button(composite2, SWT.PUSH);
            var label2 = new Label(composite2, SWT.NONE);

            Assert.Equal(2, composite1.Children.Length);
            Assert.Equal(2, composite2.Children.Length);

            Assert.Contains(button1, composite1.Children);
            Assert.Contains(label1, composite1.Children);
            Assert.Contains(button2, composite2.Children);
            Assert.Contains(label2, composite2.Children);

            label2.Dispose();
            button2.Dispose();
            label1.Dispose();
            button1.Dispose();
            composite2.Dispose();
            composite1.Dispose();
        });
    }

    #endregion

    /// <summary>
    /// Helper to get the native NSView handle from a Composite widget.
    /// Uses reflection to access internal PlatformWidget property.
    /// </summary>
    private static IntPtr GetNSViewHandle(Composite composite)
    {
        var platformWidget = typeof(Composite).BaseType?.BaseType?.GetProperty("PlatformWidget",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(composite);
        return (platformWidget as MacOSWidget)?.GetNativeHandle() ?? IntPtr.Zero;
    }
}
