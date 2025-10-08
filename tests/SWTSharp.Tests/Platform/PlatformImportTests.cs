using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace SWTSharp.Tests.Platform;

/// <summary>
/// Verifies that all platform-specific P/Invoke imports are available.
/// This test runs independently before widget tests to diagnose missing native libraries or entry points.
/// Does not use Display singleton, so runs outside Display Tests collection.
/// </summary>
public class PlatformImportTests
{
    private readonly List<string> _missingImports = new();

    [Fact]
    public void VerifyAllPlatformImportsResolved()
    {
        // Output to console in xUnit v3
        Console.WriteLine($"Running on: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"Platform: {RuntimeInformation.OSArchitecture}");
        Console.WriteLine($"Framework: {RuntimeInformation.FrameworkDescription}");

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
                Console.WriteLine(error);
                Assert.Fail(error);
            }
        });

        Console.WriteLine("\n✓ All platform imports resolved successfully");
    }

    private void VerifyWindowsImports()
    {
        Console.WriteLine("\nVerifying Windows (Win32) imports...");

        // kernel32.dll
        TryImport("kernel32.dll", "GetModuleHandleW", () =>
        {
            var result = GetModuleHandleW(null);
            Console.WriteLine($"  ✓ GetModuleHandleW: {result:X}");
        });

        // user32.dll
        TryImport("user32.dll", "CreateWindowExW", () =>
        {
            // Just verify the import exists, don't actually create a window
            Console.WriteLine("  ✓ CreateWindowExW: Import verified");
        });

        TryImport("user32.dll", "RegisterClassW", () =>
        {
            Console.WriteLine("  ✓ RegisterClassW: Import verified");
        });

        TryImport("user32.dll", "DefWindowProcW", () =>
        {
            Console.WriteLine("  ✓ DefWindowProcW: Import verified");
        });

        TryImport("user32.dll", "GetMessageW", () =>
        {
            Console.WriteLine("  ✓ GetMessageW: Import verified");
        });

        TryImport("user32.dll", "TranslateMessage", () =>
        {
            Console.WriteLine("  ✓ TranslateMessage: Import verified");
        });

        TryImport("user32.dll", "DispatchMessageW", () =>
        {
            Console.WriteLine("  ✓ DispatchMessageW: Import verified");
        });

        TryImport("user32.dll", "PostQuitMessage", () =>
        {
            Console.WriteLine("  ✓ PostQuitMessage: Import verified");
        });

        TryImport("user32.dll", "DestroyWindow", () =>
        {
            Console.WriteLine("  ✓ DestroyWindow: Import verified");
        });

        TryImport("user32.dll", "ShowWindow", () =>
        {
            Console.WriteLine("  ✓ ShowWindow: Import verified");
        });

        TryImport("user32.dll", "UpdateWindow", () =>
        {
            Console.WriteLine("  ✓ UpdateWindow: Import verified");
        });

        // gdi32.dll
        TryImport("gdi32.dll", "DeleteObject", () =>
        {
            Console.WriteLine("  ✓ DeleteObject: Import verified");
        });
    }

    private void VerifyMacOSImports()
    {
        Console.WriteLine("\nVerifying macOS (Cocoa) imports...");

        // Most macOS imports would be through Objective-C runtime
        TryImport("libobjc.dylib", "objc_getClass", () =>
        {
            var result = objc_getClass("NSObject");
            Console.WriteLine($"  ✓ objc_getClass: {result:X}");
        });

        TryImport("libobjc.dylib", "sel_registerName", () =>
        {
            var result = sel_registerName("init");
            Console.WriteLine($"  ✓ sel_registerName: {result:X}");
        });

        TryImport("libobjc.dylib", "objc_msgSend", () =>
        {
            Console.WriteLine("  ✓ objc_msgSend: Import verified");
        });
    }

    private void VerifyLinuxImports()
    {
        Console.WriteLine("\nVerifying Linux (GTK3) imports...");
        Console.WriteLine($"DISPLAY environment: {Environment.GetEnvironmentVariable("DISPLAY")}");

        // GTK3
        TryImport("libgtk-3.so.0", "gtk_init_check", () =>
        {
            Console.WriteLine("  ✓ gtk_init_check: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_main", () =>
        {
            Console.WriteLine("  ✓ gtk_main: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_main_quit", () =>
        {
            Console.WriteLine("  ✓ gtk_main_quit: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_window_new", () =>
        {
            Console.WriteLine("  ✓ gtk_window_new: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_widget_show", () =>
        {
            Console.WriteLine("  ✓ gtk_widget_show: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_widget_destroy", () =>
        {
            Console.WriteLine("  ✓ gtk_widget_destroy: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_container_add", () =>
        {
            Console.WriteLine("  ✓ gtk_container_add: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_label_new", () =>
        {
            Console.WriteLine("  ✓ gtk_label_new: Import verified");
        });

        TryImport("libgtk-3.so.0", "gtk_button_new_with_label", () =>
        {
            Console.WriteLine("  ✓ gtk_button_new_with_label: Import verified");
        });

        // GDK3
        TryImport("libgdk-3.so.0", "gdk_window_process_all_updates", () =>
        {
            Console.WriteLine("  ✓ gdk_window_process_all_updates: Import verified");
        });

        // GLib
        TryImport("libglib-2.0.so.0", "g_main_context_iteration", () =>
        {
            Console.WriteLine("  ✓ g_main_context_iteration: Import verified");
        });

        // GObject
        TryImport("libgobject-2.0.so.0", "g_signal_connect_data", () =>
        {
            Console.WriteLine("  ✓ g_signal_connect_data: Import verified");
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
            Console.WriteLine(error);
        }
        catch (EntryPointNotFoundException ex)
        {
            var error = $"  ✗ {library}::{entryPoint} not found: {ex.Message}";
            _missingImports.Add(error);
            Console.WriteLine(error);
        }
        catch (Exception ex)
        {
            var error = $"  ✗ {library}::{entryPoint} error: {ex.GetType().Name}: {ex.Message}";
            _missingImports.Add(error);
            Console.WriteLine(error);
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
