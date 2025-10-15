using System.Runtime.InteropServices;
using System.Linq;
using Xunit;
using SWTSharp;
using SWTSharp.Platform.MacOS;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// macOS-specific tests for MacOSList widget using NSTableView via Objective-C runtime.
/// Tests list creation, item management, single and multi-selection functionality.
/// </summary>
public class MacOSListTests : WidgetTestBase
{
    public MacOSListTests(DisplayFixture displayFixture) : base(displayFixture) { }

    #region Creation Tests

    [Fact]
    public void MacOSList_Create_WithSingle_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            Assert.NotNull(list);
            Assert.False(list.IsDisposed);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Create_WithMulti_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.MULTI);

            Assert.NotNull(list);
            Assert.False(list.IsDisposed);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Create_WithBorder_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.BORDER);

            Assert.NotNull(list);
            Assert.False(list.IsDisposed);

            list.Dispose();
        });
    }

    #endregion

    #region Item Management Tests

    [Fact]
    public void MacOSList_Items_InitiallyEmpty_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            Assert.Equal(0, list.ItemCount);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Add_SingleItem_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Add("Item 1");

            Assert.Equal(1, list.ItemCount);
            Assert.Equal("Item 1", list.GetItem(0));

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Add_MultipleItems_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            Assert.Equal(3, list.ItemCount);
            Assert.Equal("Item 1", list.GetItem(0));
            Assert.Equal("Item 2", list.GetItem(1));
            Assert.Equal("Item 3", list.GetItem(2));

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_RemoveAll_ShouldClearItems()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            Assert.Equal(3, list.ItemCount);

            list.RemoveAll();

            Assert.Equal(0, list.ItemCount);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Items_WithUnicode_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Add("Hello 世界");
            list.Add("Test 🌍");

            Assert.Equal(2, list.ItemCount);
            Assert.Equal("Hello 世界", list.GetItem(0));
            Assert.Equal("Test 🌍", list.GetItem(1));

            list.Dispose();
        });
    }

    #endregion

    #region Single Selection Tests

    [Fact]
    public void MacOSList_SelectionIndex_InitiallyNegative_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            Assert.Equal(-1, list.SelectionIndex);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Select_ValidIndex_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            list.Select(1);

            Assert.Equal(1, list.SelectionIndex);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_Deselect_ShouldResetSelection()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            list.Select(1);
            Assert.Equal(1, list.SelectionIndex);

            list.Deselect(1);
            Assert.Equal(-1, list.SelectionIndex);

            list.Dispose();
        });
    }

    #endregion

    #region Multi-Selection Tests

    [Fact]
    public void MacOSList_SelectionIndices_InitiallyEmpty_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.MULTI);

            Assert.Empty(list.SelectionIndices);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_MultiSelect_ShouldSucceed()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.MULTI);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");
            list.Add("Item 4");

            // Select multiple items by calling Select for each
            list.Select(1);
            list.Select(3);

            var indices = list.SelectionIndices;
            Assert.Equal(2, indices.Length);
            Assert.Contains(1, indices);
            Assert.Contains(3, indices);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_SelectAll_ShouldSelectAll()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.MULTI);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            list.SelectAll();

            var indices = list.SelectionIndices;
            Assert.Equal(3, indices.Length);
            Assert.Contains(0, indices);
            Assert.Contains(1, indices);
            Assert.Contains(2, indices);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_DeselectAll_ShouldClearSelection()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.MULTI);

            list.Add("Item 1");
            list.Add("Item 2");
            list.Add("Item 3");

            list.SelectAll();
            Assert.Equal(3, list.SelectionIndices.Length);

            list.DeselectAll();
            Assert.Empty(list.SelectionIndices);

            list.Dispose();
        });
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void MacOSList_Dispose_ShouldCleanup()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Dispose();

            Assert.True(list.IsDisposed);
        });
    }

    [Fact]
    public void MacOSList_DoubleDispose_ShouldNotThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            list.Dispose();
            list.Dispose(); // Should not throw
        });
    }

    [Fact]
    public void MacOSList_AccessAfterDispose_ShouldThrow()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);
            list.Dispose();

            Assert.Throws<SWTDisposedException>(() => list.Add("Item"));
            Assert.Throws<SWTDisposedException>(() => _ = list.ItemCount);
            Assert.Throws<SWTDisposedException>(() => _ = list.SelectionIndices);
        });
    }

    #endregion

    #region Platform-Specific Tests

    [Fact]
    public void MacOSList_PlatformWidget_ShouldBeCreated()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            // Verify platform widget exists
            Assert.NotNull(list.PlatformWidget);

            list.Dispose();
        });
    }

    [Fact]
    public void MacOSList_NSTableViewHandle_ShouldBeValid()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.SINGLE);

            var handle = GetNSTableViewHandle(list);
            Assert.NotEqual(IntPtr.Zero, handle);

            list.Dispose();
        });
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void MacOSList_CompleteWorkflow_ShouldWork()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var list = new List(shell, SWT.MULTI);

            // Add items
            list.Add("Apple");
            list.Add("Banana");
            list.Add("Cherry");
            list.Add("Date");

            Assert.Equal(4, list.ItemCount);

            // Select multiple items
            list.Select(0);
            list.Select(2);
            var indices = list.SelectionIndices;
            Assert.Equal(2, indices.Length);
            Assert.Contains(0, indices);
            Assert.Contains(2, indices);

            // Select all
            list.SelectAll();
            Assert.Equal(4, list.SelectionIndices.Length);

            // Clear selection
            list.DeselectAll();
            Assert.Empty(list.SelectionIndices);

            // Clear items
            list.RemoveAll();
            Assert.Equal(0, list.ItemCount);

            list.Dispose();
        });
    }

    #endregion

    /// <summary>
    /// Helper to get the native NSTableView handle from a List widget.
    /// Uses reflection to access internal PlatformWidget property.
    /// </summary>
    private static IntPtr GetNSTableViewHandle(List list)
    {
        var platformWidget = typeof(List).BaseType?.BaseType?.GetProperty("PlatformWidget",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(list);
        return (platformWidget as MacOSWidget)?.GetNativeHandle() ?? IntPtr.Zero;
    }
}
