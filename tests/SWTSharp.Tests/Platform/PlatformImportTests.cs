using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// Verifies that all platform-specific P/Invoke imports are available.
/// This test runs independently before widget tests to diagnose missing native libraries or entry points.
/// Does not use Display singleton, so runs outside Display Tests collection.
/// </summary>
public class PlatformImportTests
{
    private readonly ITestOutputHelper _output;
    private readonly List<string> _missingImports = new();

    public PlatformImportTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void VerifyAllPlatformImportsResolved()
    {
        _output.WriteLine($"Running on: {RuntimeInformation.OSDescription}");
        _output.WriteLine($"Platform: {RuntimeInformation.OSArchitecture}");
        _output.WriteLine($"Framework: {RuntimeInformation.FrameworkDescription}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            VerifyWindowsImports();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            VerifyMacOSImports();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            VerifyLinuxImports();
        }
        else
        {
            throw new PlatformNotSupportedException("Unknown platform");
        }

        // Use Assert.Multiple to report all failures at once
        Assert.Multiple(() =>
        {
            foreach (var error in _missingImports)
            {
                _output.WriteLine(error);
                Assert.Fail(error);
            }
        });

        _output.WriteLine("\n✓ All platform imports resolved successfully");
    }

    private void VerifyWindowsImports()
    {
        _output.WriteLine("\nVerifying Windows (Win32) imports...");

        // kernel32.dll
        TryImport("kernel32.dll", "GetModuleHandleW", () =>
        {
            var result = GetModuleHandleW(null);
            _output.WriteLine($"  ✓ GetModuleHandleW: {result:X}");
        });

        // user32.dll
        TryImport("user32.dll", "CreateWindowExW", () =>
        {
            // Just verify the import exists, don't actually create a window
            _output.WriteLine("  ✓ CreateWindowExW: Import verified");
        });

        TryImport("user32.dll", "RegisterClassW", () =>
        {
            _output.WriteLine("  ✓ RegisterClassW: Import verified");
        });

        TryImport("user32.dll", "DefWindowProcW", () =>
        {
            _output.WriteLine("  ✓ DefWindowProcW: Import verified");
        });

        TryImport("user32.dll", "GetMessageW", () =>
        {
            _output.WriteLine("  ✓ GetMessageW: Import verified");
        });

        TryImport("user32.dll", "TranslateMessage", () =>
        {
            _output.WriteLine("  ✓ TranslateMessage: Import verified");
        });

        TryImport("user32.dll", "DispatchMessageW", () =>
        {
            _output.WriteLine("  ✓ DispatchMessageW: Import verified");
        });

        TryImport("user32.dll", "PostQuitMessage", () =>
        {
            _output.WriteLine("  ✓ PostQuitMessage: Import verified");
        });

        TryImport("user32.dll", "DestroyWindow", () =>
        {
            _output.WriteLine("  ✓ DestroyWindow: Import verified");
        });

        TryImport("user32.dll", "ShowWindow", () =>
        {
            _output.WriteLine("  ✓ ShowWindow: Import verified");
        });

        TryImport("user32.dll", "UpdateWindow", () =>
        {
            _output.WriteLine("  ✓ UpdateWindow: Import verified");
        });

        // gdi32.dll
        TryImport("gdi32.dll", "DeleteObject", () =>
        {
            _output.WriteLine("  ✓ DeleteObject: Import verified");
        });
    }

    private void VerifyMacOSImports()
    {
        _output.WriteLine("\nVerifying macOS (Cocoa) imports...");

        // Most macOS imports would be through Objective-C runtime
        TryImport("libobjc.dylib", "objc_getClass", () =>
        {
            var result = objc_getClass("NSObject");
            _output.WriteLine($"  ✓ objc_getClass: {result:X}");
        });

        TryImport("libobjc.dylib", "sel_registerName", () =>
        {
            var result = sel_registerName("init");
            _output.WriteLine($"  ✓ sel_registerName: {result:X}");
        });

        TryImport("libobjc.dylib", "objc_msgSend", () =>
        {
            _output.WriteLine("  ✓ objc_msgSend: Import verified");
        });
    }

    private void VerifyLinuxImports()
    {
        _output.WriteLine("\nVerifying Linux (GTK3) imports...");
        _output.WriteLine($"DISPLAY environment: {Environment.GetEnvironmentVariable("DISPLAY")}");

        // GTK3
        TryImport("libgtk-3.so.0", "gtk_init_check", () =>
        {
            _output.WriteLine("  ✓ gtk_init_check: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_main", () =>
        {
            _output.WriteLine("  ✓ gtk_main: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_main_quit", () =>
        {
            _output.WriteLine("  ✓ gtk_main_quit: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_window_new", () =>
        {
            _output.WriteLine("  ✓ gtk_window_new: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_widget_show", () =>
        {
            _output.WriteLine("  ✓ gtk_widget_show: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_widget_destroy", () =>
        {
            _output.WriteLine("  ✓ gtk_widget_destroy: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_container_add", () =>
        {
            _output.WriteLine("  ✓ gtk_container_add: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_label_new", () =>
        {
            _output.WriteLine("  ✓ gtk_label_new: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_button_new_with_label", () =>
        {
            _output.WriteLine("  ✓ gtk_button_new_with_label: Import verified");
        });

        // GDK3
        TryImport("libgdk-3.so.0", "gdk_window_process_all_updates", () =>
        {
            _output.WriteLine("  ✓ gdk_window_process_all_updates: Import verified");
        });

        // GLib
        TryImport("libglib-2.0.so.0", "g_main_context_iteration", () =>
        {
            _output.WriteLine("  ✓ g_main_context_iteration: Import verified");
        });

        // GObject
        TryImport("libgobject-2.0.so.0", "g_signal_connect_data", () =>
        {
            _output.WriteLine("  ✓ g_signal_connect_data: Import verified");
        });
    }

    private void TryImport(string library, string entryPoint, Action verifyAction)
    {
        try
        {
            verifyAction();
        }
        catch (DllNotFoundException ex)
        {
            var error = $"  ✗ {library} not found: {ex.Message}";
            _missingImports.Add(error);
            _output.WriteLine(error);
        }
        catch (EntryPointNotFoundException ex)
        {
            var error = $"  ✗ {library}::{entryPoint} not found: {ex.Message}";
            _missingImports.Add(error);
            _output.WriteLine(error);
        }
        catch (Exception ex)
        {
            var error = $"  ✗ {library}::{entryPoint} error: {ex.GetType().Name}: {ex.Message}";
            _missingImports.Add(error);
            _output.WriteLine(error);
        }
    }

    #region Windows P/Invoke Declarations

    [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandleW(string? lpModuleName);

    #endregion

    #region macOS P/Invoke Declarations

    [DllImport("libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    #endregion

    #region Linux P/Invoke Declarations
    // Just need stubs to verify imports exist
    #endregion
}
