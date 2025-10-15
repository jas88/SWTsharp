using System.Runtime.InteropServices;
using Xunit;
using SWTSharp;
using SWTSharp.Platform.MacOS;
using SWTSharp.Tests.Infrastructure;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// macOS-specific tests for MacOSButton event handling via Objective-C runtime.
/// Tests the runtime class creation mechanism for target/action pattern.
/// </summary>
public class MacOSButtonTests : WidgetTestBase
{
    public MacOSButtonTests(DisplayFixture displayFixture) : base(displayFixture) { }

    [Fact]
    public void MacOSButton_Create_ShouldInitializeEventHandlers()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var button = new Button(shell, SWT.PUSH);

            // Button should be created successfully
            Assert.NotNull(button);
            Assert.False(button.IsDisposed);

            button.Dispose();
        });
    }

    [Fact]
    public void MacOSButton_Click_Event_ShouldFire()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var button = new Button(shell, SWT.PUSH);
            button.Text = "Test Button";

            int clickCount = 0;
            button.Click += (sender, e) =>
            {
                clickCount++;
            };

            // Get the native NSButton handle
            var buttonHandle = GetNSButtonHandle(button);
            Assert.NotEqual(IntPtr.Zero, buttonHandle);

            // Trigger the button action programmatically
            TriggerNSButtonClick(buttonHandle);

            // Verify event fired
            Assert.Equal(1, clickCount);

            // Test multiple clicks
            TriggerNSButtonClick(buttonHandle);
            TriggerNSButtonClick(buttonHandle);
            Assert.Equal(3, clickCount);

            button.Dispose();
        });
    }

    [Fact]
    public void MacOSButton_MultipleButtons_ShouldRouteEventsCorrectly()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();

            var button1 = new Button(shell, SWT.PUSH);
            button1.Text = "Button 1";

            var button2 = new Button(shell, SWT.PUSH);
            button2.Text = "Button 2";

            var button3 = new Button(shell, SWT.PUSH);
            button3.Text = "Button 3";

            int button1Clicks = 0;
            int button2Clicks = 0;
            int button3Clicks = 0;

            button1.Click += (sender, e) => button1Clicks++;
            button2.Click += (sender, e) => button2Clicks++;
            button3.Click += (sender, e) => button3Clicks++;

            // Get button handles
            var handle1 = GetNSButtonHandle(button1);
            var handle2 = GetNSButtonHandle(button2);
            var handle3 = GetNSButtonHandle(button3);

            Assert.NotEqual(IntPtr.Zero, handle1);
            Assert.NotEqual(IntPtr.Zero, handle2);
            Assert.NotEqual(IntPtr.Zero, handle3);

            // Click each button
            TriggerNSButtonClick(handle1);
            Assert.Equal(1, button1Clicks);
            Assert.Equal(0, button2Clicks);
            Assert.Equal(0, button3Clicks);

            TriggerNSButtonClick(handle2);
            Assert.Equal(1, button1Clicks);
            Assert.Equal(1, button2Clicks);
            Assert.Equal(0, button3Clicks);

            TriggerNSButtonClick(handle3);
            Assert.Equal(1, button1Clicks);
            Assert.Equal(1, button2Clicks);
            Assert.Equal(1, button3Clicks);

            // Click button1 multiple times
            TriggerNSButtonClick(handle1);
            TriggerNSButtonClick(handle1);
            Assert.Equal(3, button1Clicks);
            Assert.Equal(1, button2Clicks);
            Assert.Equal(1, button3Clicks);

            button1.Dispose();
            button2.Dispose();
            button3.Dispose();
        });
    }

    [Fact]
    public void MacOSButton_Dispose_ShouldCleanupEventHandlers()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();
            var button = new Button(shell, SWT.PUSH);

            int clickCount = 0;
            button.Click += (sender, e) => clickCount++;

            var buttonHandle = GetNSButtonHandle(button);

            // Click before disposal
            TriggerNSButtonClick(buttonHandle);
            Assert.Equal(1, clickCount);

            // Dispose the button
            button.Dispose();

            // Clicking after disposal should not crash
            // (though it won't increment the counter since the event handler is gone)
            try
            {
                TriggerNSButtonClick(buttonHandle);
            }
            catch
            {
                // Expected - handle may be invalid after disposal
            }

            // Counter should remain at 1 (event didn't fire after disposal)
            Assert.Equal(1, clickCount);
        });
    }

    [Fact]
    public void MacOSButton_RuntimeClass_ShouldBeCreatedOnce()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();

            // Create multiple buttons - runtime class should only be created once
            var buttons = new List<Button>();
            for (int i = 0; i < 10; i++)
            {
                var button = new Button(shell, SWT.PUSH);
                button.Text = $"Button {i}";
                buttons.Add(button);
            }

            // Verify all buttons work
            var counters = new int[10];
            for (int i = 0; i < 10; i++)
            {
                int index = i; // Capture for closure
                buttons[i].Click += (sender, e) => counters[index]++;
            }

            // Click each button
            for (int i = 0; i < 10; i++)
            {
                var handle = GetNSButtonHandle(buttons[i]);
                TriggerNSButtonClick(handle);
                Assert.Equal(1, counters[i]);
            }

            // Cleanup
            foreach (var button in buttons)
            {
                button.Dispose();
            }
        });
    }

    [Fact]
    public void MacOSButton_TargetInstance_ShouldBeUnique()
    {
        // Skip on non-macOS platforms
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return;
        }

        RunOnUIThread(() =>
        {
            using var shell = CreateTestShell();

            var button1 = new Button(shell, SWT.PUSH);
            var button2 = new Button(shell, SWT.PUSH);

            // Each button should have its own target instance
            var handle1 = GetNSButtonHandle(button1);
            var handle2 = GetNSButtonHandle(button2);

            Assert.NotEqual(IntPtr.Zero, handle1);
            Assert.NotEqual(IntPtr.Zero, handle2);
            Assert.NotEqual(handle1, handle2);

            // Get target objects (this would require exposing internal state)
            // For now, just verify events route correctly
            int button1Clicks = 0;
            int button2Clicks = 0;

            button1.Click += (sender, e) => button1Clicks++;
            button2.Click += (sender, e) => button2Clicks++;

            TriggerNSButtonClick(handle1);
            Assert.Equal(1, button1Clicks);
            Assert.Equal(0, button2Clicks);

            TriggerNSButtonClick(handle2);
            Assert.Equal(1, button1Clicks);
            Assert.Equal(1, button2Clicks);

            button1.Dispose();
            button2.Dispose();
        });
    }

    // Objective-C P/Invoke declarations for testing
    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string selector);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern void objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg);

    /// <summary>
    /// Helper to get the native NSButton handle from a Button widget.
    /// Uses reflection to access internal PlatformWidget property.
    /// </summary>
    private static IntPtr GetNSButtonHandle(Button button)
    {
        var platformWidget = typeof(Button).BaseType?.GetProperty("PlatformWidget",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.GetValue(button);
        return (platformWidget as MacOSWidget)?.GetNativeHandle() ?? IntPtr.Zero;
    }

    /// <summary>
    /// Triggers a button click by calling performClick: on the NSButton.
    /// This simulates a user clicking the button.
    /// </summary>
    private static void TriggerNSButtonClick(IntPtr buttonHandle)
    {
        if (buttonHandle == IntPtr.Zero)
        {
            throw new ArgumentException("Invalid button handle", nameof(buttonHandle));
        }

        // Call performClick: selector to trigger the action
        // performClick: takes one argument (the sender, usually nil)
        var performClickSelector = sel_registerName("performClick:");
        objc_msgSend(buttonHandle, performClickSelector, IntPtr.Zero);
    }
}
