using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SWTSharp.Tests.Infrastructure;

/// <summary>
/// xUnit collection definition to ensure all tests using Display run serially.
/// This is necessary because Display is a singleton that can only be associated with one thread.
/// </summary>
[CollectionDefinition("Display Tests", DisableParallelization = true)]
public class DisplayCollection : ICollectionFixture<DisplayFixture>
{
}

/// <summary>
/// Shared fixture that creates a single UI thread and Display for all tests.
/// Implements IAsyncLifetime for proper async initialization/disposal pattern.
/// </summary>
/// <remarks>
/// This fixture is designed to work with the [Collection("Display Tests")] and
/// [Collection("GUI Tests")] patterns where GUI tests run serially and share
/// a Display instance for performance.
///
/// The Display is created once when the first test in the collection runs,
/// and disposed after the last test completes.
/// </remarks>
public class DisplayFixture : IAsyncLifetime
{
    private Thread _uiThread = null!;
    private bool _disposed;
    private BlockingCollection<Action>? _actionQueue;
    private Thread? _dispatcherThread;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Gets the shared Display instance for GUI tests.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if accessed before InitializeAsync or after DisposeAsync.</exception>
    public Display Display { get; private set; } = null!;

    /// <summary>
    /// Initializes the fixture by creating the shared Display instance.
    /// Called by xUnit before the first test in the collection runs.
    /// </summary>
    public Task InitializeAsync()
    {
        Console.WriteLine($"DisplayFixture: Current thread = {Thread.CurrentThread.ManagedThreadId}");

        // On macOS, tests MUST run through the custom test runner (Program.Main)
        // which initializes MainThreadDispatcher on Thread 1
        // If MainThreadDispatcher is not initialized, we're being run by 'dotnet test'
        // which won't work on macOS - skip all tests by throwing
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (!SWTSharp.TestHost.MainThreadDispatcher.IsInitialized)
            {
                var message =
                    "macOS tests must run through custom test runner.\n" +
                    "Use: dotnet run --project tests/SWTSharp.Tests\n" +
                    "NOT: dotnet test\n" +
                    "\n" +
                    "The MacOSRunnerTests.MacOS_Tests_Should_Run_Through_Custom_Runner test\n" +
                    "will automatically launch the custom runner when you use 'dotnet test'.";

                Console.Error.WriteLine($"ERROR: {message}");
                throw new InvalidOperationException(message);
            }
        }

        // On macOS, Display must be created on Thread 1 (the main thread)
        // Use MainThreadDispatcher to ensure Display is created on Thread 1
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SWTSharp.TestHost.MainThreadDispatcher.Invoke(() =>
            {
                Display = Display.Default;
                _uiThread = Thread.CurrentThread;
                Console.WriteLine($"DisplayFixture: Display created on Thread {Thread.CurrentThread.ManagedThreadId}");
            });

            // Hook Display.AsyncExec to use MainThreadDispatcher
            Display.SetAsyncExecutor(SWTSharp.TestHost.MainThreadDispatcher.Invoke);
            Console.WriteLine("DisplayFixture: Set custom async executor to use MainThreadDispatcher");
        }
        else
        {
            // For Windows/Linux: Create a dedicated UI thread with an action queue
            // This ensures SyncExec works when tests run on different threads than fixture init
            _actionQueue = new BlockingCollection<Action>();
            _cts = new CancellationTokenSource();
            var displayReady = new ManualResetEventSlim(false);

            _dispatcherThread = new Thread(() =>
            {
                Display = Display.Default;
                _uiThread = Thread.CurrentThread;
                Console.WriteLine($"DisplayFixture: Display created on Thread {Thread.CurrentThread.ManagedThreadId}");
                displayReady.Set();

                // Process actions from the queue
                try
                {
                    foreach (var action in _actionQueue.GetConsumingEnumerable(_cts.Token))
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"DisplayFixture: Error executing action: {ex.Message}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown
                }
            })
            {
                Name = "SWTSharp UI Thread",
                IsBackground = true
            };
            _dispatcherThread.Start();

            // Wait for Display to be created
            displayReady.Wait(TimeSpan.FromSeconds(10));
            if (Display == null)
            {
                throw new InvalidOperationException("Failed to create Display on UI thread within timeout");
            }

            // Hook Display.AsyncExec to use our action queue
            Display.SetAsyncExecutor(action => _actionQueue.Add(action));
            Console.WriteLine("DisplayFixture: Set custom async executor to use action queue dispatcher");
        }

        var displayThread = Display.GetType().GetField("_thread", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(Display) as Thread;
        Console.WriteLine($"DisplayFixture: Display thread = {displayThread?.ManagedThreadId ?? -1}");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the fixture by disposing the shared Display instance.
    /// Called by xUnit after the last test in the collection completes.
    /// </summary>
    public Task DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;

            // Cleanup all shells
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // On macOS, cleanup on the main thread via MainThreadDispatcher
                    var shells = Display.GetShells();
                    foreach (var shell in shells)
                    {
                        shell?.Dispose();
                    }
                }
                else if (_actionQueue != null)
                {
                    // On Windows/Linux, dispatch cleanup to the UI thread
                    var cleanupDone = new ManualResetEventSlim(false);
                    _actionQueue.Add(() =>
                    {
                        try
                        {
                            var shells = Display.GetShells();
                            foreach (var shell in shells)
                            {
                                shell?.Dispose();
                            }
                        }
                        finally
                        {
                            cleanupDone.Set();
                        }
                    });
                    cleanupDone.Wait(TimeSpan.FromSeconds(5));

                    // Shut down the dispatcher thread
                    _cts?.Cancel();
                    _actionQueue.CompleteAdding();
                    _dispatcherThread?.Join(TimeSpan.FromSeconds(2));
                }
            }
            catch
            {
                // Swallow exceptions during disposal
            }
        }

        return Task.CompletedTask;
    }
}
