using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SWTSharp.TestHost;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Module initializer to set up MainThreadDispatcher before any tests run.
/// This runs automatically when the test assembly loads, whether via dotnet test or dotnet run.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.WriteLine($"[ModuleInitializer] Running on macOS - Thread {Thread.CurrentThread.ManagedThreadId}");

            // On macOS, we need MainThreadDispatcher running on Thread 1
            // When running via dotnet test, we're already on Thread 1
            if (Thread.CurrentThread.ManagedThreadId == 1)
            {
                // Start dispatcher in background mode - it will service requests without blocking
                MainThreadDispatcher.Initialize();
                Console.WriteLine("[ModuleInitializer] MainThreadDispatcher initialized on Thread 1");

                // Hook into MacOSPlatform
                SWTSharp.Platform.MacOSPlatform.CustomMainThreadExecutor = MainThreadDispatcher.Invoke;
                Console.WriteLine("[ModuleInitializer] Hooked MacOSPlatform.CustomMainThreadExecutor");
            }
            else
            {
                Console.WriteLine($"[ModuleInitializer] WARNING: Not on Thread 1, tests may fail");
            }
        }
    }
}
