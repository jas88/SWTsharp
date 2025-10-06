# Getting Started with SWTSharp Implementation

## For New Team Members

Welcome to the SWTSharp project! This guide will help you get started contributing to the implementation.

---

## Prerequisites

### Required Knowledge
- **C# and .NET**: Proficiency in C# 12.0 and .NET 8.0/9.0
- **Platform APIs**: Familiarity with at least one of:
  - Windows: Win32 API, P/Invoke
  - macOS: Objective-C, Cocoa/AppKit
  - Linux: C, GTK+ 3.0
- **Git**: Version control basics
- **Testing**: Unit testing with xUnit or NUnit

### Recommended Knowledge
- **Java SWT**: Understanding of Java SWT API is very helpful
- **UI Programming**: Cross-platform UI development experience
- **Native Interop**: P/Invoke, DllImport, marshalling
- **Graphics Programming**: GDI, Core Graphics, or Cairo experience

---

## Development Environment Setup

### 1. Install Required Tools

#### All Platforms
```bash
# Install .NET SDK 9.0 (or 8.0)
# Download from: https://dotnet.microsoft.com/download

# Verify installation
dotnet --version
```

#### Windows Developers
- Visual Studio 2022 (Community or higher)
- Windows SDK (latest)
- Git for Windows

#### macOS Developers
```bash
# Install Xcode and command line tools
xcode-select --install

# Install .NET SDK for macOS
# Download from: https://dotnet.microsoft.com/download
```

#### Linux Developers
```bash
# Install .NET SDK
sudo apt-get update
sudo apt-get install dotnet-sdk-9.0

# Install GTK development packages
sudo apt-get install libgtk-3-dev

# Install other dependencies
sudo apt-get install build-essential git
```

---

### 2. Clone the Repository

```bash
git clone https://github.com/jas88/swtsharp.git
cd swtsharp
```

---

### 3. Build the Project

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run sample application
dotnet run --project samples/SWTSharp.Sample
```

---

### 4. Verify Platform Support

```bash
# Check which platform you're on
dotnet run --project samples/SWTSharp.Sample -- --platform-info
```

Expected output:
- Windows: `Platform: win32`
- macOS: `Platform: macosx`
- Linux: `Platform: linux`

---

## Project Structure

```
swtsharp/
├── src/
│   └── SWTSharp/              # Main library
│       ├── Platform/          # Platform-specific code
│       │   ├── IPlatform.cs   # Platform interface
│       │   ├── Win32Platform.cs
│       │   ├── MacOSPlatform.cs
│       │   └── LinuxPlatform.cs
│       ├── Events/            # Event handling
│       ├── Widget.cs          # Base widget class
│       ├── Control.cs         # Base control class
│       ├── Shell.cs           # Window/shell
│       ├── Button.cs          # Button widget
│       ├── Label.cs           # Label widget
│       ├── Text.cs            # Text input widget
│       └── ... (more widgets)
├── tests/
│   └── SWTSharp.Tests/        # Unit tests
├── samples/
│   └── SWTSharp.Sample/       # Example application
└── docs/
    ├── implementation-plan/   # THIS DOCUMENT
    └── api/                   # API documentation
```

---

## Finding Work to Do

### 1. Check the Roadmap
See `/docs/implementation-plan/roadmap.md` for the full implementation plan.

### 2. Check Current Phase
See `/docs/implementation-plan/quick-reference.md` for quick status.

### 3. Check GitHub Issues
Look for issues tagged with:
- `good-first-issue`: Great for new contributors
- `help-wanted`: Areas needing contributors
- `phase-1`, `phase-2`, etc.: Organized by implementation phase
- `windows`, `macos`, `linux`: Platform-specific work

### 4. Check Team Assignments
See the project wiki or team coordination channel for current assignments.

---

## Implementation Guidelines

### Coding Standards

#### 1. Follow C# Conventions
```csharp
// ✅ Good: PascalCase for public members
public class Button : Control
{
    public string Text { get; set; }
    public event EventHandler? Click;
}

// ❌ Bad: Java naming conventions
public class Button : Control
{
    public string getText() { ... }  // Don't use Java-style getters
    public void setText(string text) { ... }
}
```

#### 2. Use Nullable Reference Types
```csharp
// ✅ Good: Explicit nullability
public class Control : Widget
{
    private Control? _parent;  // Can be null
    private string _text = string.Empty;  // Never null

    public Control? Parent => _parent;
}
```

#### 3. Resource Management
```csharp
// ✅ Good: IDisposable pattern
public class Widget : IDisposable
{
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                _data = null;
            }
            // Free unmanaged resources
            if (Handle != IntPtr.Zero)
            {
                // Platform-specific cleanup
            }
            _disposed = true;
        }
    }
}
```

#### 4. Thread Safety
```csharp
// ✅ Good: Check thread access
protected void CheckWidget()
{
    if (_disposed)
        throw new SWTDisposedException("Widget has been disposed");

    if (_display != null && !_display.IsValidThread())
        throw new SWTInvalidThreadException("Invalid thread access");
}

// Use CheckWidget() at start of every public method
public string Text
{
    get
    {
        CheckWidget();
        return _text;
    }
}
```

---

### Platform-Specific Implementation

#### Pattern: Platform Abstraction

```csharp
// 1. Define platform-independent API in widget
public class Button : Control
{
    private string _text = string.Empty;

    public string Text
    {
        get
        {
            CheckWidget();
            return _text;
        }
        set
        {
            CheckWidget();
            _text = value ?? string.Empty;
            UpdateText();  // Calls platform-specific code
        }
    }

    private void UpdateText()
    {
        if (Handle != IntPtr.Zero)
        {
            PlatformFactory.Instance.SetControlText(Handle, _text);
        }
    }
}

// 2. Implement in platform layer
// Platform/Win32Platform.cs
internal class Win32Platform : IPlatform
{
    public void SetControlText(IntPtr handle, string text)
    {
        SetWindowText(handle, text);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool SetWindowText(IntPtr hWnd, string text);
}
```

---

### Testing Guidelines

#### 1. Unit Tests for Each Widget

```csharp
using Xunit;
using SWTSharp;

public class ButtonTests
{
    [Fact]
    public void Button_SetText_UpdatesText()
    {
        // Arrange
        var display = Display.Default;
        var shell = new Shell(display);
        var button = new Button(shell, SWT.PUSH);

        // Act
        button.Text = "Click Me";

        // Assert
        Assert.Equal("Click Me", button.Text);

        // Cleanup
        button.Dispose();
        shell.Dispose();
    }

    [Fact]
    public void Button_ThrowsWhenDisposed()
    {
        // Arrange
        var display = Display.Default;
        var shell = new Shell(display);
        var button = new Button(shell, SWT.PUSH);
        button.Dispose();

        // Act & Assert
        Assert.Throws<SWTDisposedException>(() => button.Text = "Test");
    }
}
```

#### 2. Platform-Specific Tests

```csharp
public class WindowsPlatformTests
{
    [Fact]
    [PlatformSpecific(TestPlatforms.Windows)]
    public void Windows_CreateButton_CreatesWin32Control()
    {
        // Test Windows-specific behavior
    }
}

public class MacOSPlatformTests
{
    [Fact]
    [PlatformSpecific(TestPlatforms.macOS)]
    public void MacOS_CreateButton_CreatesNSButton()
    {
        // Test macOS-specific behavior
    }
}
```

---

## Common Tasks

### Task 1: Implement a New Widget Style

**Example: Add ARROW style support to Button**

```csharp
// 1. Update Button.cs
public class Button : Control
{
    public Button(Control parent, int style) : base(parent, style)
    {
        if ((style & SWT.ARROW) != 0)
        {
            CreateArrowButton(style);
        }
        else
        {
            CreateWidget(style);
        }
    }

    private void CreateArrowButton(int style)
    {
        // Determine arrow direction from style
        int direction = SWT.RIGHT; // default
        if ((style & SWT.UP) != 0) direction = SWT.UP;
        else if ((style & SWT.DOWN) != 0) direction = SWT.DOWN;
        else if ((style & SWT.LEFT) != 0) direction = SWT.LEFT;

        // Create platform-specific arrow button
        Handle = PlatformFactory.Instance.CreateArrowButton(
            Parent?.Handle ?? IntPtr.Zero,
            Style,
            direction
        );
    }
}

// 2. Update IPlatform.cs
internal interface IPlatform
{
    IntPtr CreateArrowButton(IntPtr parentHandle, int style, int direction);
}

// 3. Implement in Win32Platform.cs
internal class Win32Platform : IPlatform
{
    public IntPtr CreateArrowButton(IntPtr parentHandle, int style, int direction)
    {
        // Map SWT direction to Win32 constants
        uint arrowStyle = direction switch
        {
            SWT.UP => 0x0040,    // BS_ARROW + upward
            SWT.DOWN => 0x0080,  // BS_ARROW + downward
            SWT.LEFT => 0x0100,  // BS_ARROW + left
            SWT.RIGHT => 0x0200, // BS_ARROW + right
            _ => 0x0200
        };

        return CreateWindowEx(/* ... */);
    }
}

// 4. Add tests
[Fact]
public void Button_ArrowStyle_CreatesCorrectly()
{
    var button = new Button(shell, SWT.ARROW | SWT.UP);
    Assert.True((button.Style & SWT.ARROW) != 0);
}
```

---

### Task 2: Add Platform Implementation

**Example: Implement Button for macOS**

```csharp
// Platform/MacOSPlatform.cs
using System.Runtime.InteropServices;

internal class MacOSPlatform : IPlatform
{
    public IntPtr CreateButton(IntPtr parentHandle, int style, string text)
    {
        // Create NSButton via Objective-C runtime
        IntPtr nsButtonClass = objc_getClass("NSButton");
        IntPtr button = objc_msgSend(nsButtonClass, sel_registerName("alloc"));
        button = objc_msgSend(button, sel_registerName("init"));

        // Set button type based on style
        if ((style & SWT.PUSH) != 0)
        {
            objc_msgSend(button, sel_registerName("setButtonType:"), 0); // NSMomentaryLightButton
        }
        else if ((style & SWT.CHECK) != 0)
        {
            objc_msgSend(button, sel_registerName("setButtonType:"), 3); // NSSwitchButton
        }
        // ... more button types

        // Set title
        if (!string.IsNullOrEmpty(text))
        {
            IntPtr nsString = CreateNSString(text);
            objc_msgSend(button, sel_registerName("setTitle:"), nsString);
        }

        // Add to parent view if provided
        if (parentHandle != IntPtr.Zero)
        {
            objc_msgSend(parentHandle, sel_registerName("addSubview:"), button);
        }

        return button;
    }

    // Objective-C runtime imports
    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.dylib")]
    private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, int arg1);

    // Helper to create NSString
    private IntPtr CreateNSString(string text)
    {
        // Implementation...
    }
}
```

---

### Task 3: Implement a Layout Manager

**Example: Implement FillLayout**

```csharp
// Layouts/FillLayout.cs
namespace SWTSharp.Layouts;

public class FillLayout : Layout
{
    public int Type { get; set; } = SWT.HORIZONTAL;
    public int MarginWidth { get; set; } = 0;
    public int MarginHeight { get; set; } = 0;
    public int Spacing { get; set; } = 0;

    protected override Point ComputeSize(
        Composite composite,
        int wHint,
        int hHint,
        bool flushCache)
    {
        var children = composite.GetChildren();
        var count = children.Length;

        if (count == 0)
            return new Point(MarginWidth * 2, MarginHeight * 2);

        // Compute size based on children
        int maxWidth = 0, maxHeight = 0;
        foreach (var child in children)
        {
            var size = child.ComputeSize(SWT.DEFAULT, SWT.DEFAULT, flushCache);
            maxWidth = Math.Max(maxWidth, size.X);
            maxHeight = Math.Max(maxHeight, size.Y);
        }

        if (Type == SWT.HORIZONTAL)
        {
            int width = maxWidth * count + Spacing * (count - 1) + MarginWidth * 2;
            int height = maxHeight + MarginHeight * 2;
            return new Point(width, height);
        }
        else
        {
            int width = maxWidth + MarginWidth * 2;
            int height = maxHeight * count + Spacing * (count - 1) + MarginHeight * 2;
            return new Point(width, height);
        }
    }

    protected override void Layout(Composite composite, bool flushCache)
    {
        var children = composite.GetChildren();
        var count = children.Length;

        if (count == 0) return;

        var clientArea = composite.GetClientArea();
        int x = clientArea.X + MarginWidth;
        int y = clientArea.Y + MarginHeight;
        int availableWidth = clientArea.Width - MarginWidth * 2;
        int availableHeight = clientArea.Height - MarginHeight * 2;

        if (Type == SWT.HORIZONTAL)
        {
            int childWidth = (availableWidth - Spacing * (count - 1)) / count;
            foreach (var child in children)
            {
                child.SetBounds(x, y, childWidth, availableHeight);
                x += childWidth + Spacing;
            }
        }
        else // VERTICAL
        {
            int childHeight = (availableHeight - Spacing * (count - 1)) / count;
            foreach (var child in children)
            {
                child.SetBounds(x, y, availableWidth, childHeight);
                y += childHeight + Spacing;
            }
        }
    }
}

// Add to Composite.cs
public class Composite : Control
{
    private Layout? _layout;

    public Layout? Layout
    {
        get => _layout;
        set
        {
            _layout = value;
            if (_layout != null)
            {
                DoLayout();
            }
        }
    }

    public void DoLayout()
    {
        if (_layout != null)
        {
            _layout.Layout(this, false);
        }
    }
}
```

---

## Debugging Tips

### Enable Detailed Logging

```csharp
// Add to your app startup
Environment.SetEnvironmentVariable("SWT_DEBUG", "1");
```

### Debug Native Handles

```csharp
// Windows: Use Spy++ to inspect window handles
// macOS: Use Xcode's View Debugger
// Linux: Use GTK Inspector (Ctrl+Shift+I in GTK apps)
```

### Memory Leak Detection

```bash
# Use dotnet-counters
dotnet-counters monitor --process-id <pid>

# Use dotMemory or PerfView for detailed analysis
```

---

## Contribution Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/phase2-composite-implementation
```

### 2. Make Changes

- Follow coding standards
- Add tests for new functionality
- Update documentation

### 3. Test Locally

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~ButtonTests"

# Test on specific platform
dotnet test --filter "PlatformSpecific=Windows"
```

### 4. Commit Changes

```bash
git add .
git commit -m "feat(composite): implement child management system

- Add AddChild and RemoveChild methods
- Implement tab order traversal
- Add focus management
- Platform-specific rendering for Windows

Closes #123"
```

### 5. Create Pull Request

- Describe what you implemented
- Reference related issues
- Include screenshots for UI changes
- Request review from platform experts

---

## Resources

### Documentation
- **Java SWT**: https://www.eclipse.org/swt/
- **SWT Snippets**: https://www.eclipse.org/swt/snippets/
- **Win32 API**: https://docs.microsoft.com/en-us/windows/win32/
- **AppKit**: https://developer.apple.com/documentation/appkit
- **GTK**: https://docs.gtk.org/gtk3/

### Tools
- **P/Invoke Interop Assistant**: https://www.pinvoke.net/
- **Spy++**: Inspect Windows handles
- **Xcode View Debugger**: Inspect Cocoa views
- **GTK Inspector**: Inspect GTK widgets

### Communication
- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: Questions and design discussions
- **Team Slack/Discord**: Daily coordination (if applicable)

---

## FAQ

**Q: Which phase should I start with?**
A: Check the roadmap. Currently, Phase 1 (completing basic widgets) is in progress. Phase 2 (Composite) is the next critical phase.

**Q: I only know Windows development. Can I still help?**
A: Absolutely! Windows is our reference platform. Focus on Windows implementation, and others will port to macOS/Linux.

**Q: How do I test platform-specific code?**
A: Use platform-specific test attributes. Tests will be skipped on other platforms automatically.

**Q: What if I find a bug in existing code?**
A: Create a GitHub issue with reproduction steps. If you can fix it, submit a PR!

**Q: How closely should I follow Java SWT?**
A: API should be similar, but use C# conventions (properties instead of getters, events instead of listeners, etc.). When in doubt, ask!

**Q: Can I add features not in Java SWT?**
A: Yes, but discuss first. Additional features should be clearly marked as extensions.

---

## Next Steps

1. ✅ Read this guide
2. ✅ Set up development environment
3. ✅ Build and run the project
4. ✅ Read the roadmap (`/docs/implementation-plan/roadmap.md`)
5. ✅ Find an issue to work on (GitHub Issues or roadmap)
6. ✅ Introduce yourself to the team
7. ✅ Start coding!

---

**Welcome to the team! Let's build something great together!**

*Last Updated: 2025-10-05*
*Version: 1.0*
