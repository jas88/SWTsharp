// Example: Enable diagnostic logging with environment variable SWTSHARP_DEBUG=1
// This demonstrates the comprehensive logging added to help debug platform-specific issues

using System;
using SWTSharp;

class DiagnosticLoggingDemo
{
    static void Main()
    {
        Console.WriteLine("=== SWTSharp Diagnostic Logging Demo ===");
        Console.WriteLine("Set environment variable: SWTSHARP_DEBUG=1");
        Console.WriteLine();

        // Check if logging is enabled
        var debug = Environment.GetEnvironmentVariable("SWTSHARP_DEBUG");
        if (debug == "1")
        {
            Console.WriteLine("Diagnostic logging is ENABLED");
        }
        else
        {
            Console.WriteLine("Diagnostic logging is DISABLED");
            Console.WriteLine("To enable, run with: SWTSHARP_DEBUG=1");
        }
        Console.WriteLine();

        try
        {
            // Create a display - this will trigger platform initialization
            Console.WriteLine("Creating Display...");
            var display = new Display();

            // Create a shell - this will show window creation
            Console.WriteLine("Creating Shell...");
            var shell = new Shell(display);
            shell.SetText("Diagnostic Logging Demo");
            shell.SetSize(400, 300);

            // Create a button - this will show widget creation
            Console.WriteLine("Creating Button...");
            var button = new Button(shell, SWT.PUSH);
            button.SetText("Test Button");
            button.SetBounds(10, 10, 100, 30);

            Console.WriteLine();
            Console.WriteLine("Widgets created successfully!");
            Console.WriteLine("Check the output above for diagnostic logs (if enabled)");

            // Clean up
            shell.Dispose();
            display.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        Console.WriteLine();
        Console.WriteLine("=== Demo Complete ===");
    }
}
