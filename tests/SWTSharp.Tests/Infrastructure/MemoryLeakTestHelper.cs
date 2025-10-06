using System.Diagnostics;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// Helper class for memory leak detection tests.
/// </summary>
public static class MemoryLeakTestHelper
{
    /// <summary>
    /// Runs a test action multiple times and checks for memory leaks.
    /// </summary>
    /// <param name="action">The action to test</param>
    /// <param name="iterations">Number of iterations to run</param>
    /// <param name="maxMemoryIncreaseMB">Maximum allowed memory increase in MB</param>
    public static void AssertNoMemoryLeak(Action action, int iterations = 1000, int maxMemoryIncreaseMB = 50)
    {
        // Force initial GC to get baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var initialMemory = GC.GetTotalMemory(true);

        // Run the action multiple times
        for (int i = 0; i < iterations; i++)
        {
            action();

            // Periodic GC to prevent false positives
            if (i % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        // Force final GC
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncreaseMB = (finalMemory - initialMemory) / (1024.0 * 1024.0);

        Assert.True(
            memoryIncreaseMB < maxMemoryIncreaseMB,
            $"Memory leak detected: {memoryIncreaseMB:F2} MB increase after {iterations} iterations (max allowed: {maxMemoryIncreaseMB} MB)"
        );
    }

    /// <summary>
    /// Monitors memory during a long-running operation.
    /// </summary>
    public static MemoryMonitor StartMonitoring()
    {
        return new MemoryMonitor();
    }
}

/// <summary>
/// Monitors memory usage during test execution.
/// </summary>
public class MemoryMonitor : IDisposable
{
    private readonly long _initialMemory;
    private readonly Stopwatch _stopwatch;
    private bool _disposed;

    public MemoryMonitor()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        _initialMemory = GC.GetTotalMemory(false);
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Gets the current memory usage in MB.
    /// </summary>
    public double CurrentMemoryMB => GC.GetTotalMemory(false) / (1024.0 * 1024.0);

    /// <summary>
    /// Gets the memory increase since monitoring started in MB.
    /// </summary>
    public double MemoryIncreaseMB => (GC.GetTotalMemory(false) - _initialMemory) / (1024.0 * 1024.0);

    /// <summary>
    /// Gets the elapsed time since monitoring started.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Dispose()
    {
        if (!_disposed)
        {
            _stopwatch.Stop();
            _disposed = true;
        }
    }
}
