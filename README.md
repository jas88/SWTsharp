# SWTSharp

A C#/.NET port of the Java SWT (Standard Widget Toolkit) library, providing cross-platform GUI capabilities for .NET applications.

## Features

- **Cross-platform**: Supports Windows, macOS, and Linux (Windows fully implemented, others in progress)
- **Multi-target**: Compatible with .NET Standard 2.0, .NET 8.0, and .NET 9.0
- **SWT API compatibility**: Maintains API similarity with Java SWT for easier porting
- **Event-driven**: Full event handling system with typed listeners
- **Native look and feel**: Uses platform-specific UI controls

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

- ✅ Display - Connection to platform display system
- ✅ Shell - Top-level window
- ✅ Control - Base class for all controls
- ✅ Button - Push, check, radio, toggle buttons
- ✅ Label - Non-editable text labels
- ✅ Text - Single and multi-line text input
- 🚧 Composite - Container for other widgets (coming soon)
- 🚧 Menu - Menus and menu items (coming soon)
- 🚧 Table - Data tables (coming soon)
- 🚧 Tree - Hierarchical trees (coming soon)

## Platform Support

| Platform | Status | Notes |
|----------|--------|-------|
| Windows | ✅ Implemented | Full Win32 API support |
| macOS | 🚧 In Progress | Cocoa/AppKit integration planned |
| Linux | 🚧 In Progress | GTK integration planned |

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
│   └── SWTSharp/          # Main library
│       ├── Platform/      # Platform-specific implementations
│       └── Events/        # Event handling system
├── tests/
│   └── SWTSharp.Tests/    # Unit tests
├── samples/
│   └── SWTSharp.Sample/   # Example application
└── docs/                  # Documentation
```

## API Documentation

SWTSharp follows the Java SWT API design with C# conventions:

- Classes use PascalCase (e.g., `Button`, `Shell`)
- Properties instead of get/set methods (e.g., `shell.Text` instead of `shell.getText()`)
- C# events instead of listener interfaces
- Nullable reference types enabled
- IDisposable pattern for resource management

## Contributing

Contributions are welcome! Areas needing help:

- macOS platform implementation (Cocoa/AppKit)
- Linux platform implementation (GTK)
- Additional widgets (Composite, Menu, Table, Tree, etc.)
- Layout managers
- Graphics and drawing APIs
- More comprehensive tests

## License

Eclipse Public License 2.0 (EPL-2.0) - Same as Java SWT

## Acknowledgments

Based on the Eclipse SWT project: https://www.eclipse.org/swt/
