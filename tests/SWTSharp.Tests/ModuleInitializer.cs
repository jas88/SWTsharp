using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SWTSharp.Tests;

/// <summary>
/// Module initializer that runs before any other code in the test assembly.
/// NOTE: On macOS, the dispatcher is now initialized in Program.Main() to ensure
/// Thread 1 remains available. This module initializer is kept for logging only.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Console.WriteLine($"Module Initializer: Running on Thread {Thread.CurrentThread.ManagedThreadId}");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine("Module Initializer: macOS detected - Main thread dispatcher will be started by Program.Main()");
        }
    }
}
