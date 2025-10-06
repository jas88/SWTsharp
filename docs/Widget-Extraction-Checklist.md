# Widget Extraction Checklist

Use this checklist when extracting a widget from the main platform file into a partial class.

## Pre-Extraction Analysis

- [ ] **Identify widget methods** - List all methods for this widget from IPlatform.cs
- [ ] **Count lines** - Verify extraction will be 200-500 lines
- [ ] **Check dependencies** - Note any shared constants or helper methods
- [ ] **Review existing tests** - Ensure widget has test coverage
- [ ] **Create feature branch** - `feature/refactor-platform-{widget}`

## File Creation

- [ ] **Create new file** - `{Platform}Platform_{Widget}.cs`
- [ ] **Add file header**
```csharp
using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// {Platform} {Widget} implementation - partial class extension
/// </summary>
internal partial class {Platform}Platform
{
```

## Code Extraction

- [ ] **Copy constants** - All widget-specific constants (check for WS_*, SS_*, etc.)
- [ ] **Copy P/Invoke declarations** - Any widget-specific native API calls
- [ ] **Copy structures** - Any widget-specific data structures
- [ ] **Extract methods** - All public interface methods for this widget
- [ ] **Include helper methods** - Any private methods used only by this widget
- [ ] **Preserve comments** - Copy all XML documentation and code comments
- [ ] **Check NET8 conditionals** - Ensure LibraryImport vs DllImport is correct

## Constants to Check

### Win32 Common Constants (might need copying)
- [ ] Window Styles: `WS_CHILD`, `WS_VISIBLE`, `WS_BORDER`, etc.
- [ ] Extended Styles: `WS_EX_CLIENTEDGE`, `WS_EX_STATICEDGE`, etc.
- [ ] Messages: `WM_SETTEXT`, `WM_GETTEXT`, `WM_COMMAND`, etc.
- [ ] Common messages: `BM_*`, `CB_*`, `LB_*`, `TVM_*`, `LVM_*`, etc.
- [ ] Other flags: `SWP_NOMOVE`, `SWP_NOSIZE`, etc.

### macOS Common Constants
- [ ] Objective-C selectors and methods
- [ ] NSView/NSControl constants
- [ ] Event handling constants

### Linux Common Constants
- [ ] GTK widget flags
- [ ] GtkWidget/GtkContainer constants
- [ ] Signal names

## Verification Steps

- [ ] **Compile check** - Build succeeds for all platforms
- [ ] **No duplicate code** - Verify code removed from main file
- [ ] **Interface compliance** - All IPlatform methods implemented
- [ ] **Run tests** - All existing tests still pass
- [ ] **Check line count** - New file is 200-500 lines
- [ ] **Verify formatting** - Code follows project style guidelines

## Testing Checklist

- [ ] **Unit tests pass** - Run widget-specific unit tests
- [ ] **Integration tests pass** - Run platform integration tests
- [ ] **Manual testing** - Create sample app using the widget
- [ ] **Cross-platform** - Test on Windows, macOS, and Linux if possible
- [ ] **Edge cases** - Test null values, empty strings, invalid indices, etc.

## Documentation

- [ ] **XML comments** - Verify all public methods have XML documentation
- [ ] **Platform notes** - Document platform-specific behavior
- [ ] **Update changelog** - Note widget extraction in CHANGELOG.md
- [ ] **Code comments** - Add inline comments for complex logic

## Cleanup Main File

- [ ] **Remove extracted constants** - If only used by this widget
- [ ] **Remove extracted methods** - All widget methods removed
- [ ] **Remove unused P/Invoke** - If declarations only used by this widget
- [ ] **Update line count** - Verify main file reduction
- [ ] **Verify no broken references** - No missing method errors

## Common Patterns to Follow

### Method Organization
```csharp
// 1. Constants (organized by category)
private const uint WIDGET_STYLE_1 = 0x0001;
private const uint WIDGET_MESSAGE_1 = 0x0100;

// 2. P/Invoke declarations (if needed)
#if NET8_0_OR_GREATER
    [LibraryImport(User32, ...)]
    private static partial IntPtr NativeMethod(...);
#else
    [DllImport(User32, ...)]
    private static extern IntPtr NativeMethod(...);
#endif

// 3. Public interface methods (in IPlatform order)
public IntPtr CreateWidget(IntPtr parent, int style)
{
    // Implementation
}

// 4. Helper methods (private)
private void HelperMethod()
{
    // Implementation
}
```

### Error Handling
```csharp
if (handle == IntPtr.Zero)
{
    int error = Marshal.GetLastWin32Error();
    throw new InvalidOperationException($"Failed to create {widget}. Error: {error}");
}
```

### Parameter Validation
```csharp
if (handle == IntPtr.Zero)
{
    throw new ArgumentException("Invalid handle", nameof(handle));
}
```

## Platform-Specific Considerations

### Win32
- [ ] **Window classes** - Check if custom window class needed
- [ ] **SendMessage calls** - Verify message constants included
- [ ] **Subclassing** - Note if widget requires subclassing
- [ ] **Common controls** - Check if needs `InitCommonControlsEx`

### macOS
- [ ] **Objective-C runtime** - Verify selector strings correct
- [ ] **Delegates** - Ensure delegate references kept alive (SEC-001)
- [ ] **Memory management** - Check retain/release patterns
- [ ] **AppKit classes** - Verify NS* class names correct

### Linux
- [ ] **GTK version** - Check GTK3 vs GTK4 compatibility
- [ ] **Signal handlers** - Verify signal names and signatures
- [ ] **GObject references** - Check g_object_ref/unref patterns
- [ ] **Widget hierarchy** - Verify parent-child relationships

## Common Mistakes to Avoid

❌ **DON'T**
- Copy constants used by multiple widgets (keep in main file)
- Remove shared helper methods (keep in main file)
- Change method signatures or behavior
- Skip testing on all three platforms
- Forget to remove code from main file
- Mix widget concerns in single file

✅ **DO**
- Keep each file focused on single widget type
- Preserve all XML documentation
- Maintain consistent code style
- Test thoroughly after extraction
- Document platform-specific quirks
- Follow existing partial file patterns

## Final Review

- [ ] **Code review** - Get peer review before merging
- [ ] **CI/CD passes** - All automated checks pass
- [ ] **Documentation complete** - README and comments updated
- [ ] **Performance check** - No regression in benchmarks
- [ ] **Memory leaks** - Run memory profiler if applicable

## Sign-Off

- [ ] **Developer**: Code complete and tested
- [ ] **Reviewer**: Code reviewed and approved
- [ ] **QA**: Manual testing passed
- [ ] **Merge**: PR merged to main branch

---

## Example: Extracting the Slider Widget

```bash
# 1. Create feature branch
git checkout -b feature/refactor-platform-slider

# 2. Create new file
touch src/SWTSharp/Platform/Win32Platform_Slider.cs
touch src/SWTSharp/Platform/MacOSPlatform_Slider.cs
touch src/SWTSharp/Platform/LinuxPlatform_Slider.cs

# 3. Extract code (use template above)
# - Copy Slider constants
# - Copy CreateSlider, SetSliderValues, ConnectSliderChanged
# - Include any Slider-specific P/Invoke

# 4. Remove from main files
# - Delete Slider code from Win32Platform.cs
# - Delete Slider code from MacOSPlatform.cs
# - Delete Slider code from LinuxPlatform.cs

# 5. Test
dotnet build
dotnet test

# 6. Commit and push
git add .
git commit -m "refactor: Extract Slider widget to partial classes"
git push origin feature/refactor-platform-slider

# 7. Create PR and get review
```

## Template Files

See existing examples:
- `Win32Platform_Combo.cs` - Complex widget with many methods
- `Win32Platform_Label.cs` - Simple widget with few methods

---

**Use this checklist for every widget extraction to ensure consistency and quality!**
