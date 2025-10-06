# SWTSharp Packaging Guide

## Building the NuGet Package

### Development Build

```bash
# Build for all target frameworks
dotnet build -c Release

# Create NuGet package
dotnet pack -c Release

# Package will be created at:
# src/SWTSharp/bin/Release/SWTSharp.0.1.0.nupkg
```

### Testing the Package Locally

```bash
# Add local package source
dotnet nuget add source ./src/SWTSharp/bin/Release -n SWTSharpLocal

# Install in a test project
dotnet add package SWTSharp --version 0.1.0 --source SWTSharpLocal
```

### Package Contents

The NuGet package includes:
- `lib/netstandard2.0/SWTSharp.dll` - .NET Standard 2.0 compatible
- `lib/net8.0/SWTSharp.dll` - .NET 8.0 optimized
- `lib/net9.0/SWTSharp.dll` - .NET 9.0 optimized

## Publishing to NuGet.org

### Prerequisites

1. Create an account at https://www.nuget.org
2. Generate an API key from your account settings
3. Store the API key securely

### Publishing Steps

```bash
# Set API key (first time only)
dotnet nuget push --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Push package to NuGet.org
dotnet nuget push src/SWTSharp/bin/Release/SWTSharp.0.1.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Version Management

### Updating Version

Edit `src/SWTSharp/SWTSharp.csproj`:

```xml
<Version>0.2.0</Version>
```

### Semantic Versioning

- **MAJOR** (x.0.0): Breaking API changes
- **MINOR** (0.x.0): New features, backwards compatible
- **PATCH** (0.0.x): Bug fixes, backwards compatible

Example progression:
- `0.1.0` - Initial release (current)
- `0.2.0` - Add new widgets (Composite, Menu)
- `0.2.1` - Bug fixes
- `1.0.0` - Production-ready, stable API

## Package Metadata

Current configuration in `SWTSharp.csproj`:

```xml
<PropertyGroup>
  <PackageId>SWTSharp</PackageId>
  <Version>0.1.0</Version>
  <Authors>SWTSharp Contributors</Authors>
  <Description>A C#/.NET port of the Java SWT (Standard Widget Toolkit) library with multi-platform support</Description>
  <PackageTags>swt;ui;widgets;cross-platform;gui</PackageTags>
  <RepositoryUrl>https://github.com/jas88/swtsharp</RepositoryUrl>
  <PackageLicenseExpression>EPL-2.0</PackageLicenseExpression>
</PropertyGroup>
```

## Pre-Release Versions

For alpha/beta releases:

```xml
<Version>0.2.0-alpha.1</Version>
<Version>0.2.0-beta.1</Version>
<Version>0.2.0-rc.1</Version>
```

Install pre-release versions:

```bash
dotnet add package SWTSharp --version 0.2.0-alpha.1 --prerelease
```

## Multi-Platform Distribution

The package automatically includes platform-specific code through conditional compilation. When installed:

- Windows: Uses Win32Platform implementation
- macOS: Will use MacOSPlatform (when implemented)
- Linux: Will use LinuxPlatform (when implemented)

## Package Size Optimization

Current package size: ~50-100 KB (estimated)

To minimize size:
- Enable IL trimming for .NET 6+
- Use PublishTrimmed in consumer apps
- Avoid large dependencies

## Continuous Integration

### GitHub Actions Example

```yaml
name: Build and Publish

on:
  release:
    types: [created]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 9.0.x
      - name: Restore dependencies
        run: dotnet restore
      - name: Build
        run: dotnet build -c Release --no-restore
      - name: Test
        run: dotnet test -c Release --no-build
      - name: Pack
        run: dotnet pack -c Release --no-build
      - name: Publish to NuGet
        run: dotnet nuget push **/*.nupkg --api-key ${{secrets.NUGET_API_KEY}} --source https://api.nuget.org/v3/index.json
```

## Symbol Packages

To include debugging symbols:

```xml
<PropertyGroup>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

This creates `SWTSharp.0.1.0.snupkg` for Source Link debugging.

## README in Package

Add a package README:

```xml
<PropertyGroup>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>

<ItemGroup>
  <None Include="../../README.md" Pack="true" PackagePath="/"/>
</ItemGroup>
```

## Verification Checklist

Before publishing:

- [ ] All tests passing
- [ ] Version number updated
- [ ] README updated
- [ ] CHANGELOG updated
- [ ] License file included
- [ ] API documentation complete
- [ ] Sample code tested
- [ ] Package metadata correct
- [ ] No debug code or secrets
- [ ] Tested on all target frameworks
