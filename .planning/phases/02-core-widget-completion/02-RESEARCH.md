# Phase 2: Core Widget Completion - Research

**Researched:** 2026-01-30
**Domain:** Cross-platform GUI widget implementation, native resource management, P/Invoke patterns
**Confidence:** HIGH

## Summary

Phase 2 completes existing widget implementations in SWTSharp to achieve zero TODOs, full platform coverage, proper resource disposal, and zero compiler warnings. The primary technical challenges are: (1) SafeHandle resource management across three native platforms (Win32, Cocoa/AppKit, GTK3), (2) implementing functional dialogs that return values matching Java SWT semantics, (3) completing Linux-specific widgets (Slider, Spinner) with GTK3 P/Invoke, and (4) resolving 30+ compiler warnings including missing XML documentation.

The codebase already has solid architecture: IPlatformWidget interfaces abstract platform differences, SafeHandle subclasses wrap native handles, and the platform factory pattern enables runtime platform selection. What's needed is systematic completion of 199 TODO items, proper SafeHandle.ReleaseHandle() implementations, full XML documentation coverage, and Linux widget parity.

Eclipse SWT disposal patterns dictate: "If you create the object, you must dispose of it." SafeHandle is .NET's solution for native resource management - the CLR guarantees ReleaseHandle() runs during finalization, even during abrupt AppDomain unloads. The pattern is proven: Win32WindowHandle already implements this correctly with DestroyWindow in ReleaseHandle().

**Primary recommendation:** Use widget-by-widget completion strategy (Button → Label → Text → Shell → Composite for core widgets), implement SafeHandle.ReleaseHandle() for all platform handles using platform-appropriate native cleanup (DestroyWindow for Win32, CFRelease for Cocoa, g_object_unref for GTK), add comprehensive XML documentation following Microsoft's recommended tags, and verify disposal with both unit tests (SafeHandle isolation) and integration tests (widget lifecycle).

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| .NET 9.0 | 9.0 | Multi-platform runtime | Latest LTS with Native AOT support, multi-targeting to netstandard2.0/net8.0/net9.0 |
| SafeHandle | Built-in | Native resource management | Microsoft-recommended pattern for P/Invoke handle cleanup; critical finalizer guarantees |
| P/Invoke | Built-in | Platform interop | Standard .NET mechanism for calling native Win32/Cocoa/GTK APIs |
| xUnit | 2.9.3 | Testing framework | From Phase 1; event-based sync via TaskCompletionSource |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| NSubstitute | 5.3.0 | Mocking | Unit testing platform interfaces in isolation |
| GUITestBase | Phase 1 | Widget test infrastructure | Integration testing with Display lifecycle management |
| EventSyncHelpers | Phase 1 | Async test synchronization | Event-based waiting without polling |
| Coverlet | 6.0.4 | Code coverage | Multi-platform coverage collection (from Phase 1) |

### Platform-Specific APIs
| Platform | API | Purpose | P/Invoke Library |
|----------|-----|---------|-----------------|
| Windows | Win32 API | Native widgets | user32.dll, gdi32.dll, comctl32.dll |
| macOS | Cocoa/AppKit | Native widgets | Objective-C runtime via libobjc.dylib |
| Linux | GTK3 | Native widgets | libgtk-3.so.0, libgdk-3.so.0 |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| SafeHandle | Manual finalization | SafeHandle provides critical finalizer guarantees; manual finalizers are complex and error-prone |
| Direct P/Invoke | Wrapper libraries (GtkSharp, etc.) | Direct P/Invoke maintains control and avoids dependency on wrapper maintenance; existing codebase already uses this pattern |
| Widget-by-widget | Category-parallel | Widget-by-widget delivers verifiable completion; matches CONTEXT.md decision |

**Installation:**
```bash
# Already available in .NET SDK 9.0
# No additional packages required beyond Phase 1 test infrastructure
```

## Architecture Patterns

### Recommended Project Structure
```
src/SWTSharp/
├── [Widget].cs                     # Public widget API (Button.cs, Label.cs, etc.)
├── Platform/
│   ├── IPlatformWidget.cs         # Base platform interface
│   ├── IPlatform[Feature].cs      # Feature interfaces (IPlatformTextWidget, etc.)
│   ├── Win32/
│   │   ├── Win32[Widget].cs       # Win32 widget implementation
│   │   └── Win32Platform_*.cs     # Platform partial classes
│   ├── MacOS/
│   │   ├── MacOS[Widget].cs       # Cocoa widget implementation
│   │   └── MacOSPlatform_*.cs     # Platform partial classes
│   ├── Linux/
│   │   ├── Linux[Widget].cs       # GTK widget implementation (NEEDS: Slider, Spinner)
│   │   └── LinuxPlatform_*.cs     # Platform partial classes
│   └── SafeHandles/
│       ├── Win32/Win32[Type]Handle.cs    # Win32 SafeHandles
│       ├── MacOS/MacOS[Type]Handle.cs    # Cocoa SafeHandles
│       └── Linux/Linux[Type]Handle.cs    # GTK SafeHandles
└── Dialogs/
    └── [Dialog].cs                # Dialog implementations (FileDialog, etc.)

tests/SWTSharp.Tests/
├── Infrastructure/
│   ├── GUITestBase.cs             # From Phase 1
│   └── EventSyncHelpers.cs        # From Phase 1
├── [Widget]Tests.cs               # Widget integration tests
└── Platform/
    └── SafeHandles/
        └── [Handle]Tests.cs       # SafeHandle unit tests
```

### Pattern 1: SafeHandle Implementation for Native Resources

**What:** SafeHandle-derived classes that wrap native platform handles and implement ReleaseHandle() with platform-appropriate cleanup

**When to use:** For all native handles (windows, menus, fonts, graphics contexts, images)

**Example:**
```csharp
// Source: Existing Win32WindowHandle.cs (CORRECT PATTERN)
public sealed class Win32WindowHandle : SafeWindowHandle
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    protected override bool ReleaseHandle()
    {
        // CRITICAL: No resource allocation in ReleaseHandle
        // CRITICAL: Must handle all failure cases
        // DestroyWindow returns non-zero on success
        return DestroyWindow(handle);
    }
}
```

**Platform-Specific Cleanup Functions:**
```csharp
// Win32: DestroyWindow, DeleteObject, DeleteDC
protected override bool ReleaseHandle()
{
    return DestroyWindow(handle);  // user32.dll
}

// macOS: CFRelease for Core Foundation, objc_msgSend for NSObject release
protected override bool ReleaseHandle()
{
    objc_msgSend(handle, sel_release);  // libobjc.dylib
    return true;
}

// GTK: g_object_unref
protected override bool ReleaseHandle()
{
    g_object_unref(handle);  // libgtk-3.so.0
    return true;
}
```

**Reference:** [Microsoft Learn: SafeHandle](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.safehandle)

### Pattern 2: Dialog Implementation with Java SWT Semantics

**What:** Dialog classes that show platform-native dialogs and return results matching Java SWT API

**When to use:** FileDialog, ColorDialog, FontDialog, MessageBox

**Example:**
```csharp
// Source: Java SWT API pattern + existing FileDialog.cs structure
public class FileDialog : Dialog
{
    /// <summary>
    /// Opens the file dialog and returns the selected file path.
    /// </summary>
    /// <returns>Selected file path, or null if cancelled</returns>
    public string? Open()
    {
        CheckWidget();

        // Platform-specific dialog implementation
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return OpenWin32Dialog();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OpenCocoaDialog();
        }
        else
        {
            return OpenGTKDialog();
        }
    }

    private string? OpenWin32Dialog()
    {
        // Use GetOpenFileName/GetSaveFileName from comdlg32.dll
        // Return null on cancel, file path on success
    }

    private string? OpenCocoaDialog()
    {
        // Use NSOpenPanel/NSSavePanel
        // Return null on cancel, file path on success
    }

    private string? OpenGTKDialog()
    {
        // Use gtk_file_chooser_dialog_new + gtk_dialog_run
        // Return null on cancel, file path on success
    }
}
```

**Java SWT Return Semantics (from CONTEXT.md):**
- FileDialog.open() → `string?` (null on cancel)
- ColorDialog.open() → `RGB?` (null on cancel)
- FontDialog.open() → `FontData?` (null on cancel)
- MessageBox.open() → `int` (SWT.OK, SWT.CANCEL, etc.)

**Reference:** [Eclipse SWT Dialogs](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/guide/swt_widgets.htm)

### Pattern 3: IPlatformWidget Interface Implementation

**What:** Platform-specific widget classes implementing IPlatformWidget and feature interfaces

**When to use:** For all widgets (Button, Label, Text, Shell, Composite, etc.)

**Example:**
```csharp
// Pattern from existing MacOSButton.cs and Win32Button.cs
internal class Win32Button : IPlatformTextWidget, IPlatformEventHandling
{
    private readonly Win32WindowHandle _handle;

    public Win32Button(IntPtr parent, int style)
    {
        // Create native button via CreateWindowEx
        _handle = Win32WindowHandle.Create(/* ... */);
    }

    public void SetText(string text)
    {
        // Use SetWindowText Win32 API
        SetWindowText(_handle.DangerousGetHandle(), text);
    }

    public string GetText()
    {
        // Use GetWindowText Win32 API
    }

    public void Dispose()
    {
        _handle.Dispose();  // Triggers ReleaseHandle()
    }
}
```

### Pattern 4: GTK3 Widget Implementation for Linux

**What:** GTK3-based widget implementations using direct P/Invoke to libgtk-3.so.0

**When to use:** Linux platform widgets, especially missing Slider and Spinner

**Example:**
```csharp
// Source: Existing LinuxPlatform_Slider.cs (PARTIAL - needs completion)
internal partial class LinuxPlatform
{
    [DllImport("libgtk-3.so.0")]
    private static extern IntPtr gtk_scale_new_with_range(
        GtkOrientation orientation, double min, double max, double step);

    [DllImport("libgtk-3.so.0")]
    private static extern void gtk_range_set_value(IntPtr range, double value);

    public IntPtr CreateSlider(IntPtr parent, int style)
    {
        GtkOrientation orientation = (style & SWT.VERTICAL) != 0
            ? GtkOrientation.Vertical
            : GtkOrientation.Horizontal;

        IntPtr scale = gtk_scale_new_with_range(orientation, 0, 100, 1);
        gtk_scale_set_draw_value(scale, false);  // Hide numeric value
        gtk_widget_show(scale);

        // TODO in codebase: Connect "value-changed" signal
        return scale;
    }
}
```

**GTK Signal Connection Pattern:**
```csharp
// For event handling
[DllImport("libgobject-2.0.so.0")]
private static extern ulong g_signal_connect_data(
    IntPtr instance,
    string detailed_signal,
    IntPtr c_handler,
    IntPtr data,
    IntPtr destroy_data,
    int connect_flags);
```

**Reference:** [GTK3 Documentation](https://docs.gtk.org/gtk3/)

### Pattern 5: XML Documentation Coverage

**What:** Comprehensive XML documentation comments following Microsoft recommended tags

**When to use:** All public types and members to resolve CS1591 warnings

**Example:**
```csharp
/// <summary>
/// Represents a button control.
/// Buttons can be push buttons, check buttons, radio buttons, or toggle buttons.
/// </summary>
/// <remarks>
/// Button styles are specified via the <see cref="SWT"/> style constants:
/// <list type="bullet">
/// <item><description><see cref="SWT.PUSH"/> - Standard push button (default)</description></item>
/// <item><description><see cref="SWT.CHECK"/> - Checkbox button</description></item>
/// <item><description><see cref="SWT.RADIO"/> - Radio button</description></item>
/// <item><description><see cref="SWT.TOGGLE"/> - Toggle button</description></item>
/// </list>
/// </remarks>
public class Button : Control
{
    /// <summary>
    /// Gets or sets the button's text.
    /// </summary>
    /// <value>The button text, or an empty string if no text is set.</value>
    /// <exception cref="SWTException">
    /// Thrown if the widget has been disposed or if the operation fails.
    /// </exception>
    public string Text { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="parent">The parent composite widget.</param>
    /// <param name="style">
    /// The style bits. Valid styles are <see cref="SWT.PUSH"/>,
    /// <see cref="SWT.CHECK"/>, <see cref="SWT.RADIO"/>, and <see cref="SWT.TOGGLE"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="parent"/> is <c>null</c>.
    /// </exception>
    public Button(Control parent, int style) { }
}
```

**Required Tags:**
- `<summary>` - Brief description (third-person singular verb: "Represents...", "Gets or sets...")
- `<param>` - Parameter descriptions
- `<returns>` - Return value description
- `<exception>` - Exceptions that can be thrown
- `<remarks>` - Additional details beyond summary
- `<value>` - For properties
- `<see cref=""/>` - Cross-references to other types/members

**Reference:** [Microsoft Learn: Recommended XML Tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags)

### Anti-Patterns to Avoid

- **Allocating resources in ReleaseHandle():** ReleaseHandle() may run on finalizer thread during AppDomain shutdown. Must not allocate memory or call managed code that could fail.
- **Disposing SafeHandles twice:** Widget disposal should call SafeHandle.Dispose() once; SafeHandle ensures ReleaseHandle() runs exactly once.
- **Returning managed exceptions from ReleaseHandle():** Must catch all exceptions; return false on failure, true on success.
- **Using IntPtr directly instead of SafeHandle:** SafeHandle prevents handle recycling attacks and premature GC reclamation.
- **Polling for async operations in tests:** Use TaskCompletionSource with TaskCreationOptions.RunContinuationsAsynchronously (from Phase 1 EventSyncHelpers).
- **Missing XML documentation:** Compiler warning CS1591 indicates missing documentation; suppress nothing, document everything public.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Native handle lifetime | Manual finalizers | SafeHandle subclasses | Critical finalizer guarantees; handles recycle attacks; CLR-guaranteed cleanup |
| Async test synchronization | Thread.Sleep polling | TaskCompletionSource | Event-driven, deterministic, no race conditions (Phase 1 pattern) |
| Platform detection | Custom environment checks | RuntimeInformation.IsOSPlatform() | Standard .NET API; reliable across platforms |
| Dialog results | Custom result types | Java SWT semantics (null for cancel) | API compatibility requirement; matches user expectations |
| XML documentation | Custom doc formats | Triple-slash comments with tags | Compiler-verified; IDE IntelliSense integration; DocFX compatible |

**Key insight:** SafeHandle is complex but proven. The CLR guarantees ReleaseHandle() execution during finalization, even during abrupt AppDomain unloads. Manual finalizers cannot provide this guarantee and are error-prone. The existing Win32WindowHandle pattern is correct - replicate for all platform handles.

## Common Pitfalls

### Pitfall 1: Incomplete ReleaseHandle() Implementation
**What goes wrong:** SafeHandle subclass exists but ReleaseHandle() throws NotImplementedException or returns true without cleanup
**Why it happens:** Developer creates SafeHandle skeleton but doesn't implement platform-specific cleanup
**How to avoid:** For each SafeHandle:
1. Identify platform-appropriate cleanup function (DestroyWindow, CFRelease, g_object_unref)
2. P/Invoke the cleanup function in ReleaseHandle()
3. Return the success result (Win32 BOOL, or always true for reference-counted cleanup)
4. Write unit test that verifies handle is invalid after disposal
**Warning signs:** Memory leaks in long-running tests; handle exhaustion; resource contention

### Pitfall 2: Dialog Returns Non-Null on Cancel
**What goes wrong:** FileDialog.Open() returns empty string instead of null when user cancels
**Why it happens:** Platform dialog APIs return success/cancel separately from result string
**How to avoid:**
- Win32: Check OPENFILENAME.lpstrFile only if GetOpenFileName returns TRUE
- Cocoa: Check NSOpenPanel.runModal() result (NSModalResponseOK vs NSModalResponseCancel)
- GTK: Check gtk_dialog_run() result (GTK_RESPONSE_ACCEPT vs GTK_RESPONSE_CANCEL)
- Return null explicitly for all cancel paths
**Warning signs:** User cancels dialog but code treats it as selection; empty file paths cause errors

### Pitfall 3: Missing XML Documentation Causes CS1591 Warnings
**What goes wrong:** Public API members lack XML documentation; build generates CS1591 warnings
**Why it happens:** Developer adds public member but forgets documentation; copy-paste without docs
**How to avoid:**
- Enable CS1591 checking in .csproj (remove from NoWarn)
- Use IDE quick-fix to generate documentation stub (Visual Studio: Alt+Enter; Rider: Alt+Enter)
- Follow template: `<summary>` in third-person singular, `<param>` for each parameter, `<returns>` for return value
- Document exceptions using `<exception cref=""/>`
**Warning signs:** CS1591 warnings in build output; IntelliSense shows "No documentation available"

### Pitfall 4: GTK Widget Not Shown (gtk_widget_show Required)
**What goes wrong:** GTK widget is created but invisible; appears missing
**Why it happens:** GTK widgets default to hidden; must call gtk_widget_show() after creation
**How to avoid:**
- After gtk_[widget_type]_new(), always call gtk_widget_show(widget)
- For containers, call gtk_widget_show_all(container) to show container and children
- Existing LinuxPlatform_Slider.cs shows correct pattern
**Warning signs:** Widget APIs return success but nothing appears on screen; layout seems broken

### Pitfall 5: P/Invoke String Marshaling Issues
**What goes wrong:** Strings passed to/from native code are corrupted; crashes occur
**Why it happens:** Default marshaling is platform-dependent; GTK uses UTF-8, Win32 uses UTF-16
**How to avoid:**
- Win32: Use `CharSet = CharSet.Unicode` or `CharSet.Auto` for Unicode functions
- GTK: Use `[MarshalAs(UnmanagedType.LPStr)]` for UTF-8 strings
- Cocoa: Convert manually using CFStringCreateWithCharacters or UTF8Encoding
- Add explicit marshaling attributes to all P/Invoke string parameters
**Warning signs:** Garbled text in widgets; access violations; platform-specific failures

### Pitfall 6: Widget Disposal Without Handle Cleanup
**What goes wrong:** Widget.Dispose() is called but native handle leaks
**Why it happens:** Widget doesn't call PlatformWidget.Dispose() which triggers SafeHandle.ReleaseHandle()
**How to avoid:**
- Override Dispose(bool disposing) in widget classes
- Call PlatformWidget?.Dispose() in Dispose(true) path
- Call base.Dispose(disposing) at end
- Verify with integration tests using GUITestBase disposal checking (Phase 1)
**Warning signs:** Memory usage increases over test runs; resource exhaustion in long-running apps

## Code Examples

Verified patterns from official sources and existing codebase:

### SafeHandle ReleaseHandle Implementation (Win32)
```csharp
// Source: Microsoft Learn + Existing Win32WindowHandle.cs
public sealed class Win32WindowHandle : SafeWindowHandle
{
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyWindow(IntPtr hWnd);

    /// <summary>
    /// Executes the code required to free the Win32 window handle.
    /// </summary>
    /// <returns>true if the handle is released successfully; otherwise, false.</returns>
    protected override bool ReleaseHandle()
    {
        // CRITICAL: In a CER (Constrained Execution Region), we must not throw exceptions
        // DestroyWindow returns non-zero on success
        return DestroyWindow(handle);
    }
}
```

### SafeHandle ReleaseHandle Implementation (macOS)
```csharp
// Pattern for Cocoa NSObject release
public sealed class MacOSWindowHandle : SafeWindowHandle
{
    [DllImport("libobjc.dylib")]
    private static extern void objc_msgSend(IntPtr receiver, IntPtr selector);

    private static readonly IntPtr sel_release = GetSelector("release");

    protected override bool ReleaseHandle()
    {
        // Release NSWindow via Objective-C runtime
        objc_msgSend(handle, sel_release);
        return true;  // Reference counting, always succeeds
    }
}
```

### SafeHandle ReleaseHandle Implementation (GTK)
```csharp
// Pattern for GTK GObject unref
public sealed class LinuxWindowHandle : SafeWindowHandle
{
    [DllImport("libgtk-3.so.0")]
    private static extern void g_object_unref(IntPtr object);

    protected override bool ReleaseHandle()
    {
        // Decrement reference count on GObject
        g_object_unref(handle);
        return true;  // Reference counting, always succeeds
    }
}
```

### Dialog Implementation Pattern (FileDialog Win32)
```csharp
// Source: Win32 common dialogs API
private string? OpenWin32Dialog()
{
    var ofn = new OPENFILENAME
    {
        lStructSize = Marshal.SizeOf<OPENFILENAME>(),
        hwndOwner = Parent?.Handle ?? IntPtr.Zero,
        lpstrFile = new string('\0', 260),  // MAX_PATH
        nMaxFile = 260,
        lpstrFilter = BuildFilterString(),
        nFilterIndex = FilterIndex + 1,
        lpstrInitialDir = FilterPath,
        Flags = OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST
    };

    bool result = (Style & SWT.SAVE) != 0
        ? GetSaveFileName(ref ofn)
        : GetOpenFileName(ref ofn);

    if (!result)
    {
        return null;  // User cancelled
    }

    FileName = ofn.lpstrFile;
    FilterIndex = ofn.nFilterIndex - 1;
    return FileName;
}
```

### GTK Signal Connection for Events
```csharp
// Source: GTK3 signal connection pattern
private void ConnectSliderEvents(IntPtr widget)
{
    // Create GCHandle to prevent callback collection
    var callback = new Action<IntPtr, IntPtr>((sender, data) =>
    {
        // Handle value-changed signal
        double value = gtk_range_get_value(sender);
        OnSelectionChanged(new EventArgs());
    });

    var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
    _eventHandles[widget] = GCHandle.Alloc(callback);  // Prevent GC

    g_signal_connect_data(
        widget,
        "value-changed",
        callbackPtr,
        IntPtr.Zero,
        IntPtr.Zero,
        0);
}
```

### XML Documentation Template
```csharp
// Source: Microsoft Learn recommended tags
/// <summary>
/// Represents a selectable user interface object that displays a range of numeric values.
/// </summary>
/// <remarks>
/// Sliders are typically used to allow users to select a value from a continuous range.
/// The slider position represents the current selection, which can be modified by
/// dragging the thumb or clicking the track.
/// </remarks>
public class Slider : Control
{
    /// <summary>
    /// Gets or sets the minimum value of the slider range.
    /// </summary>
    /// <value>The minimum value. Default is 0.</value>
    /// <exception cref="SWTException">
    /// Thrown if the widget has been disposed.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the value is greater than <see cref="Maximum"/>.
    /// </exception>
    public int Minimum { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Slider"/> class.
    /// </summary>
    /// <param name="parent">The parent composite widget. Cannot be <c>null</c>.</param>
    /// <param name="style">
    /// The style bits. Valid styles are <see cref="SWT.HORIZONTAL"/>
    /// and <see cref="SWT.VERTICAL"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="parent"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="SWTException">
    /// Thrown if widget creation fails.
    /// </exception>
    public Slider(Control parent, int style) : base(parent, style) { }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual finalizers | SafeHandle critical finalizers | .NET 2.0 (2005) | SafeHandle guarantees cleanup even during AppDomain unload |
| HandleRef for P/Invoke | SafeHandle | .NET 2.0 (2005) | SafeHandle prevents handle recycling attacks |
| Thread.Sleep polling | TaskCompletionSource event-based | .NET 4.0+ | Deterministic, race-free async testing |
| Suppressing CS1591 | Comprehensive XML docs | Ongoing best practice | IDE IntelliSense, DocFX generation, API discoverability |
| GtkSharp wrapper | Direct GTK P/Invoke | Project decision | Control over bindings, no wrapper maintenance dependency |

**Deprecated/outdated:**
- **HandleRef:** Superseded by SafeHandle (MS recommendation: "Use SafeHandle, not HandleRef")
- **Manual finalizers:** Complex and error-prone; SafeHandle is safer
- **GtkSharp for GTK3:** Maintenance uncertain; direct P/Invoke gives full control
- **Polling in tests:** Event-based synchronization is deterministic

## Open Questions

None - all technical approaches are validated in existing codebase:

1. **SafeHandle pattern:** Win32WindowHandle proves pattern correctness
2. **Platform interfaces:** IPlatformWidget architecture is established
3. **Test infrastructure:** Phase 1 delivered GUITestBase and EventSyncHelpers
4. **GTK P/Invoke:** LinuxPlatform_Slider.cs shows partial implementation; needs completion

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: SafeHandle](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.safehandle) - SafeHandle API and critical finalizer
- [Microsoft Learn: Reliability Best Practices](https://learn.microsoft.com/en-us/dotnet/framework/performance/reliability-best-practices) - SafeHandle usage guidelines
- [Microsoft Learn: Implementing Dispose](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose) - SafeHandle disposal pattern
- [Microsoft Learn: Recommended XML Tags](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/xmldoc/recommended-tags) - XML documentation standards
- [Eclipse SWT Widgets](https://help.eclipse.org/latest/topic/org.eclipse.platform.doc.isv/guide/swt_widgets.htm) - SWT disposal patterns and lifecycle
- [GTK3 Documentation](https://docs.gtk.org/gtk3/) - GTK widget APIs and reference counting
- Existing SWTSharp codebase - Win32WindowHandle, IPlatformWidget, GUITestBase patterns

### Secondary (MEDIUM confidence)
- [GTK Widget Management](https://developer-old.gnome.org/gtkmm-tutorial/stable/sec-memory-widgets.html.en) - GTK lifecycle and make_managed()
- [SWT Tutorial (Vogella)](https://www.vogella.com/tutorials/SWT/article.html) - SWT widget patterns
- [C# XML Comments Best Practices](https://blog.rsuter.com/best-practices-for-writing-xml-documentation-phrases-in-c/) - Documentation conventions

### Tertiary (LOW confidence)
- None - all findings verified with authoritative sources

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - .NET 9.0, SafeHandle, P/Invoke are proven; Phase 1 infrastructure exists
- Architecture: HIGH - Existing codebase demonstrates all patterns; Win32WindowHandle is reference implementation
- Pitfalls: HIGH - Verified from Microsoft documentation and existing codebase TODOs

**Research date:** 2026-01-30
**Valid until:** 90 days (stable .NET/SWT APIs; Phase 1 foundation is fresh)
