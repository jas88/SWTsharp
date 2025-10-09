# SWTSharp

[![CI/CD Pipeline](https://github.com/jas88/SWTsharp/actions/workflows/ci.yml/badge.svg)](https://github.com/jas88/SWTsharp/actions/workflows/ci.yml)
<!-- [![codecov](https://codecov.io/gh/jas88/SWTsharp/branch/main/graph/badge.svg)](https://codecov.io/gh/jas88/SWTsharp) -->
<!-- Coverage temporarily disabled: xUnit runner compatibility issues with macOS Thread 1 requirement. See issue #TBD for details. -->
[![NuGet](https://img.shields.io/nuget/v/SWTSharp.svg)](https://www.nuget.org/packages/SWTSharp/)
[![License](https://img.shields.io/badge/License-EPL%202.0-blue.svg)](https://www.eclipse.org/legal/epl-2.0/)

A C#/.NET port of the Java SWT (Standard Widget Toolkit) library, providing cross-platform GUI capabilities for .NET applications.

## Features

- **Cross-platform**: Supports Windows, macOS, and Linux with platform-native implementations
- **Multi-target**: Compatible with .NET Standard 2.0, .NET 8.0, and .NET 9.0
- **SWT API compatibility**: Maintains API similarity with Java SWT for easier porting
- **Event-driven**: Full event handling system with typed listeners and adapters
- **Native look and feel**: Uses platform-specific UI controls (Win32, Cocoa/AppKit, GTK)
- **Comprehensive widget set**: 25+ widgets including containers, controls, and dialogs
- **Layout managers**: Multiple layout options (Grid, Form, Fill, Row, Stack)
- **Graphics support**: Drawing, fonts, colors, and images

## Installation

```bash
dotnet add package SWTSharp
```

Or build from source:

```bash
git clone https://github.com/jas88/swtsharp
cd swtsharp
dotnet restore
dotnet build

# Install Git hooks (optional but recommended for contributors)
./scripts/install-hooks.sh
```

## Quick Start

```csharp
using SWTSharp;

// Create display and main window
var display = Display.Default;
var shell = new Shell(display);
shell.Text = "My Application";
shell.SetSize(400, 300);

// Create widgets
var label = new Label(shell, SWT.NONE);
label.Text = "Hello, SWTSharp!";
label.SetBounds(10, 10, 200, 25);

var button = new Button(shell, SWT.PUSH);
button.Text = "Click Me";
button.SetBounds(10, 50, 100, 30);
button.Click += (sender, e) => {
    Console.WriteLine("Button clicked!");
};

// Open window and run event loop
shell.Open();
while (!shell.IsDisposed)
{
    if (!display.ReadAndDispatch())
        display.Sleep();
}

display.Dispose();
```

## Supported Widgets

### Core Components
- ✅ **Display** - Connection to platform display system
- ✅ **Shell** - Top-level window with title bar and controls
- ✅ **Widget** - Base class for all UI components
- ✅ **Control** - Base class for all interactive controls

### Containers
- ✅ **Composite** - Container for organizing child widgets
- ✅ **Group** - Container with border and optional title
- ✅ **Canvas** - Custom drawing surface
- ✅ **TabFolder** - Tabbed container with TabItems

### Input Controls
- ✅ **Button** - Push, check, radio, and toggle buttons
- ✅ **Text** - Single and multi-line text input
- ✅ **Combo** - Dropdown list with text input
- ✅ **List** - Scrollable list box
- ✅ **Spinner** - Numeric input with up/down arrows
- ✅ **Scale** - Slider control for value selection
- ✅ **Slider** - Horizontal or vertical slider

### Display Controls
- ✅ **Label** - Non-editable text and image labels
- ✅ **ProgressBar** - Progress indicator (horizontal/vertical)

### Complex Widgets
- ✅ **Table** - Multi-column data table with TableColumn and TableItem
- ✅ **Tree** - Hierarchical tree view with TreeItem
- ✅ **ToolBar** - Toolbar with ToolItems
- ✅ **Menu** - Menus and menu bars
- ✅ **MenuItem** - Individual menu items

### Dialogs
- ✅ **MessageBox** - Standard message dialogs
- ✅ **FileDialog** - File open/save dialogs
- ✅ **DirectoryDialog** - Folder selection dialog
- ✅ **ColorDialog** - Color picker dialog
- ✅ **FontDialog** - Font selection dialog

### Layout Managers
- ✅ **FillLayout** - Simple fill layout
- ✅ **GridLayout** - Grid-based layout with GridData
- ✅ **FormLayout** - Attachment-based layout with FormData
- ✅ **RowLayout** - Flow layout with RowData
- ✅ **StackLayout** - Single visible child layout

### Graphics
- ✅ **GC** - Graphics context for drawing
- ✅ **Color** - Color management (RGB)
- ✅ **Font** - Font handling with FontData
- ✅ **Image** - Image loading and display
- ✅ **Device** - Graphics device abstraction
- ✅ **Resource** - Base class for disposable resources

## Platform Support

| Platform | Status | Implementation | Notes |
|----------|--------|---------------|-------|
| Windows | ✅ Complete | Win32 API | Full native Win32 widget support |
| macOS | ✅ Complete | Cocoa/AppKit | Native macOS controls with ObjC runtime |
| Linux | ✅ Complete | GTK 3 | Native GTK widgets with X11 display |

All three platforms support:
- Complete widget set (25+ widgets)
- Event handling system
- Graphics and drawing
- Layout managers
- Dialogs and file choosers
- Platform-specific safe handles
- Thread-safe dispatching

## Building

```bash
# Restore dependencies
dotnet restore

# Build library
dotnet build

# Run tests
dotnet test

# Run sample
dotnet run --project samples/SWTSharp.Sample

# Create NuGet package
dotnet pack -c Release
```

## Project Structure

```
swtsharp/
├── src/
│   └── SWTSharp/              # Main library
│       ├── Platform/          # Platform-specific implementations
│       │   ├── Win32/         # Windows Win32 API
│       │   ├── MacOS/         # macOS Cocoa/AppKit
│       │   ├── Linux/         # Linux GTK
│       │   └── SafeHandles/   # Platform-safe resource handles
│       ├── Events/            # Event handling (19 event types)
│       ├── Layout/            # Layout managers (5 types)
│       ├── Dialogs/           # Standard dialogs (6 types)
│       └── Graphics/          # Graphics and drawing APIs
├── tests/
│   └── SWTSharp.Tests/        # Comprehensive unit tests
├── samples/
│   └── SWTSharp.Sample/       # Example application
└── .github/
    └── workflows/             # CI/CD with multi-platform testing
```

## API Documentation

SWTSharp follows the Java SWT API design with C# conventions:

- Classes use PascalCase (e.g., `Button`, `Shell`)
- Properties instead of get/set methods (e.g., `shell.Text` instead of `shell.getText()`)
- C# events instead of listener interfaces
- Nullable reference types enabled
- IDisposable pattern for resource management

## Testing

The project includes comprehensive unit tests across all platforms:

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run platform-specific tests
dotnet test --filter "Platform=Windows"
dotnet test --filter "Platform=macOS"
dotnet test --filter "Platform=Linux"
```

CI/CD pipeline runs tests on:
- ✅ Windows (windows-latest)
- ✅ macOS (macos-latest)
- ✅ Linux (ubuntu-latest with Xvfb)

## Contributing

Contributions are welcome! Current focus areas:

- Additional widget features and refinements
- Performance optimizations
- Accessibility support
- More comprehensive tests and examples
- Documentation improvements
- Bug fixes and edge case handling

## License

Eclipse Public License 2.0 (EPL-2.0) - Same as Java SWT

## Acknowledgments

Based on the Eclipse SWT project: https://www.eclipse.org/swt/
