using System.Runtime.InteropServices;
using Xunit;
using SWTSharp;
using SWTSharp.Platform.MacOS;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// macOS-specific tests for MacOSCombo widget using NSComboBox via Objective-C runtime.
/// Tests combo creation, item management, selection, and text functionality.
/// </summary>
public class MacOSComboTests : WidgetTestBase
{
    public MacOSComboTests(DisplayFixture displayFixture) : base(displayFixture) { }

    #region Creation Tests

    [Fact]
    public void MacOSCombo_Create_WithDropDown_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            Assert.NotNull(combo);
            Assert.False(combo.IsDisposed);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Create_WithReadOnly_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.READ_ONLY);

            Assert.NotNull(combo);
            Assert.False(combo.IsDisposed);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Create_WithSimple_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.SIMPLE);

            Assert.NotNull(combo);
            Assert.False(combo.IsDisposed);

            combo.Dispose();
        });
    }

    #endregion

    #region Item Management Tests

    [Fact]
    public void MacOSCombo_Items_InitiallyEmpty_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            Assert.Equal(0, combo.ItemCount);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Add_SingleItem_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Item 1");

            Assert.Equal(1, combo.ItemCount);
            Assert.Equal("Item 1", combo.GetItem(0));

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Add_MultipleItems_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Item 1");
            combo.Add("Item 2");
            combo.Add("Item 3");

            Assert.Equal(3, combo.ItemCount);
            Assert.Equal("Item 1", combo.GetItem(0));
            Assert.Equal("Item 2", combo.GetItem(1));
            Assert.Equal("Item 3", combo.GetItem(2));

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_RemoveAll_ShouldClearItems()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Item 1");
            combo.Add("Item 2");
            combo.Add("Item 3");

            Assert.Equal(3, combo.ItemCount);

            combo.RemoveAll();

            Assert.Equal(0, combo.ItemCount);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Items_WithUnicode_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Hello 世界");
            combo.Add("Test 🌍");

            Assert.Equal(2, combo.ItemCount);
            Assert.Equal("Hello 世界", combo.GetItem(0));
            Assert.Equal("Test 🌍", combo.GetItem(1));

            combo.Dispose();
        });
    }

    #endregion

    #region Selection Tests

    [Fact]
    public void MacOSCombo_SelectionIndex_InitiallyNegative_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            Assert.Equal(-1, combo.SelectionIndex);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Select_ValidIndex_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Item 1");
            combo.Add("Item 2");
            combo.Add("Item 3");

            combo.Select(1);

            Assert.Equal(1, combo.SelectionIndex);
            Assert.Equal("Item 2", combo.Text);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_Text_GetSet_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Item 1");
            combo.Add("Item 2");
            combo.Add("Item 3");

            combo.Text = "Item 2";

            Assert.Equal("Item 2", combo.Text);
            Assert.Equal(1, combo.SelectionIndex);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_DeselectAll_ShouldResetSelection()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Add("Item 1");
            combo.Add("Item 2");
            combo.Add("Item 3");

            combo.Select(1);
            Assert.Equal(1, combo.SelectionIndex);

            combo.DeselectAll();
            Assert.Equal(-1, combo.SelectionIndex);

            combo.Dispose();
        });
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void MacOSCombo_Dispose_ShouldCleanup()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Dispose();

            Assert.True(combo.IsDisposed);
        });
    }

    [Fact]
    public void MacOSCombo_DoubleDispose_ShouldNotThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            combo.Dispose();
            combo.Dispose(); // Should not throw
        });
    }

    [Fact]
    public void MacOSCombo_AccessAfterDispose_ShouldThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);
            combo.Dispose();

            Assert.Throws<SWTDisposedException>(() => combo.Add("Item"));
            Assert.Throws<SWTDisposedException>(() => _ = combo.ItemCount);
            Assert.Throws<SWTDisposedException>(() => _ = combo.Text);
        });
    }

    #endregion

    #region Platform-Specific Tests

    [Fact]
    public void MacOSCombo_PlatformWidget_ShouldBeCreated()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            // Verify platform widget exists
            Assert.NotNull(combo.PlatformWidget);

            combo.Dispose();
        });
    }

    [Fact]
    public void MacOSCombo_NSComboBoxHandle_ShouldBeValid()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            var handle = GetNSComboBoxHandle(combo);
            Assert.NotEqual(IntPtr.Zero, handle);

            combo.Dispose();
        });
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MacOSCombo_CompleteWorkflow_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var combo = new Combo(shell, SWT.DROP_DOWN);

            // Add items
            combo.Add("Apple");
            combo.Add("Banana");
            combo.Add("Cherry");

            Assert.Equal(3, combo.ItemCount);

            // Select an item
            combo.Select(1);
            Assert.Equal("Banana", combo.Text);

            // Change selection
            combo.Text = "Cherry";
            Assert.Equal(2, combo.SelectionIndex);

            // Clear items
            combo.RemoveAll();
            Assert.Equal(0, combo.ItemCount);
            Assert.Equal(-1, combo.SelectionIndex);

            combo.Dispose();
        });
    }

    #endregion

    /// <summary>
    /// Helper to get the native NSComboBox handle from a Combo widget.
    /// Uses reflection to access internal PlatformWidget property.
    /// </summary>
    private static IntPtr GetNSComboBoxHandle(Combo combo)
    {
        var platformWidget = typeof(Combo).BaseType?.BaseType?.GetProperty("PlatformWidget",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(combo);
        return (platformWidget as MacOSWidget)?.GetNativeHandle() ?? IntPtr.Zero;
    }
}
