# Display.ExecuteOnMainThread() - macOS Main Thread Execution

## Overview

`Display.ExecuteOnMainThread()` provides a way to execute code on the **actual process main thread** on macOS, using Grand Central Dispatch (GCD). This is required for NSWindow creation and other AppKit operations that must run on the process's first thread.

## API

```csharp
public void ExecuteOnMainThread(Action action)
```

**Parameters:**
- `action`: The action to execute on the main thread

**Behavior by Platform:**
- **macOS**: Uses `dispatch_sync_f()` with `dispatch_get_main_queue()` to execute on the process's first thread
- **Windows**: Executes directly (no main thread requirement)
- **Linux/GTK**: Executes directly (no main thread requirement)

## Usage Example

```csharp
using SWTSharp;

// Get the Display instance
var display = Display.Default;

// Create a Shell on the macOS main thread
Shell shell = null;
display.ExecuteOnMainThread(() =>
{
    shell = new Shell(display);
    shell.Text = "Main Thread Window";
    shell.SetSize(400, 300);
    shell.Open();
});

// The shell is now created and visible
display.Run();
```

## Unit Testing with xUnit

This is particularly useful for xUnit tests on macOS, where tests run on background threads:

```csharp
[Fact]
public void TestShellCreation()
{
    // xUnit runs this on a background thread
    var display = Display.Default;

    Shell shell = null;

    // Execute the shell creation on the main thread
    display.ExecuteOnMainThread(() =>
    {
        shell = new Shell(display);
        shell.Text = "Test Shell";
    });

    // Assert on the results
    Assert.NotNull(shell);
    Assert.Equal("Test Shell", shell.Text);

    // Cleanup (also on main thread)
    display.ExecuteOnMainThread(() => shell.Dispose());
}
```

## How It Works (macOS)

1. **GCD Main Queue**: Uses `dispatch_get_main_queue()` to get the process's main dispatch queue
2. **Synchronous Dispatch**: Uses `dispatch_sync_f()` to execute and wait for completion
3. **Exception Handling**: Captures exceptions from the main thread and re-throws them on the calling thread
4. **Thread Safety**: Uses GCHandle to prevent garbage collection during dispatch

## Implementation Details

### macOS (MacOSPlatform.cs)

```csharp
[DllImport(LibSystem)]
private static extern IntPtr dispatch_get_main_queue();

[DllImport(LibSystem)]
private static extern void dispatch_sync_f(IntPtr queue, IntPtr context, IntPtr work);

public void ExecuteOnMainThread(Action action)
{
    var mainQueue = dispatch_get_main_queue();
    // ... GCHandle management and dispatch_sync_f call
}
```

### Linux/Windows

These platforms don't have a main thread requirement, so the action executes directly:

```csharp
public void ExecuteOnMainThread(Action action)
{
    action(); // Just execute directly
}
```

## Comparison with SyncExec()

| Feature | `SyncExec()` | `ExecuteOnMainThread()` |
|---------|--------------|-------------------------|
| Target thread | Display's UI thread | Process main thread (macOS) |
| macOS compatibility | ❌ Fails if UI thread != main thread | ✅ Always uses main thread |
| xUnit compatibility | ❌ Creates widgets on wrong thread | ✅ Works correctly |
| Platform-specific | No | Yes (macOS only) |
| Use case | Normal UI threading | macOS-required operations |

## When to Use

**Use `ExecuteOnMainThread()`:**
- ✅ Creating NSWindow on macOS
- ✅ xUnit tests that create widgets
- ✅ Any AppKit operation requiring main thread
- ✅ Cross-platform code that needs to work in test harnesses

**Use `SyncExec()`:**
- ✅ Normal UI thread marshalling in production apps
- ✅ When Display is initialized on the main thread
- ✅ Cross-thread communication in running applications

## Notes

- **Blocking**: `ExecuteOnMainThread()` blocks the calling thread until completion
- **Exception Propagation**: Exceptions thrown in the action are propagated to the caller
- **macOS Only**: The main thread distinction only matters on macOS; other platforms execute directly
- **Not Needed in Production**: Production macOS apps typically initialize Display on the main thread, so `SyncExec()` works fine

## Related

- `Display.SyncExec()` - Execute on Display's UI thread
- `Display.AsyncExec()` - Execute asynchronously on UI thread
- `Display.IsValidThread()` - Check if on Display's UI thread
