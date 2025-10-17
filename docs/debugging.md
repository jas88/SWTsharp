# Debugging Platform-Specific Issues

## Diagnostic Logging

SWTSharp includes comprehensive diagnostic logging to help debug platform-specific issues during development and CI testing.

### Enabling Diagnostic Logging

Set the environment variable `SWTSHARP_DEBUG=1` to enable detailed logging:

**Windows (PowerShell):**
```powershell
$env:SWTSHARP_DEBUG="1"
dotnet test
```

**macOS/Linux (Bash):**
```bash
export SWTSHARP_DEBUG=1
dotnet test
```

**GitHub Actions CI:**
```yaml
- name: Run tests
  env:
    SWTSHARP_DEBUG: 1
  run: dotnet test
```

### What Gets Logged

When diagnostic logging is enabled, you'll see detailed output including:

#### Platform Detection (PlatformFactory)
- Operating system details
- Architecture information
- .NET framework version
- Which platform implementation was created

#### Win32Platform (Windows)
- Window class registration (success/failure with error codes)
- Module handle extraction
- Widget creation (type, parent handle, style flags)
- Initialization steps

#### LinuxPlatform (GTK)
- GTK initialization status
- Window creation (GtkWindow handle)
- Container creation (GtkFixed handle)
- Widget creation (parent handle, style flags)

#### MacOSPlatform (Cocoa)
- Thread detection (pthread_main_np)
- ObjCRuntime initialization
- Selector registration
- Main thread execution routing
- NSWindow creation
- Widget creation with handles

### Example Output

```
[PlatformFactory] Detecting platform...
[PlatformFactory] OS: macOS 14.0 25.0.0 Darwin Kernel Version 25.0.0
[PlatformFactory] Architecture: Arm64
[PlatformFactory] Framework: .NET 9.0.0
[PlatformFactory] Creating MacOSPlatform
[macOS] MacOSPlatform constructor called
[macOS] Thread check - pthread_main_np(): 1
[macOS] ObjCRuntime initialized
[macOS] Initializing MacOSPlatform...
[macOS] Initializing widget selectors...
[macOS] Initialization complete
[macOS] Creating window. Style: 0x0, Title: 'Test Window'
[macOS] Executing window creation on thread 1
[macOS] NSWindow created successfully: 0x1234567890ABCDEF
```

### Using Logs for Debugging

1. **Platform Detection Issues**: Check the PlatformFactory logs to verify the correct platform is detected
2. **Window Creation Failures**: Look for window creation logs and handle values (0x0 indicates failure)
3. **Widget Parent Issues**: Verify parent handle extraction shows non-zero values
4. **macOS Thread Issues**: Check pthread_main_np() returns 1 during UI operations
5. **GTK Container Issues**: Ensure container creation succeeds and is added to window
6. **Win32 Registration**: Verify window class registration succeeds or properly handles ERROR_CLASS_ALREADY_EXISTS

### Disabling Logging

To disable diagnostic logging, either unset the environment variable or set it to any value other than "1":

```bash
unset SWTSHARP_DEBUG
# or
export SWTSHARP_DEBUG=0
```

### Performance Impact

Diagnostic logging has minimal performance impact:
- Single environment variable check per platform initialization
- Conditional Console.WriteLine calls only when enabled
- No overhead when disabled (checks are compile-time optimized)

This makes it safe to leave the logging infrastructure in production code.
