using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SWTSharp.TestHost;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Module initializer that starts the UI thread dispatcher before any tests run.
/// This ensures all UI operations happen on a single dedicated thread across all platforms.
/// On macOS, that thread runs CFRunLoop. On Windows/Linux, it runs a custom dispatch queue.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        var threadId = Thread.CurrentThread.ManagedThreadId;
        var msg = $"[ModuleInitializer] Running on Thread {threadId}";
        Console.WriteLine(msg);

        // Use cross-platform temp path (works on Windows, macOS, Linux)
        var logPath = Path.Combine(Path.GetTempPath(), "test-thread-log.txt");
        File.AppendAllText(logPath, msg + "\n");

        // Note: On macOS, MainThreadDispatcher initialization and validation
        // happens in DisplayFixture constructor (after Program.Main runs).
        // Module initializers run too early (before Program.Main) to check this.
    }
}
