# GitHub Copilot Development Environment Instructions

This document provides specific guidance for GitHub Copilot when working on the .NET MAUI repository. It serves as context for understanding the project structure, development workflow, and best practices.

## Repository Overview

**.NET MAUI** is a cross-platform framework for creating mobile and desktop applications with C# and XAML. This repository contains the core framework code that enables development for Android, iOS, iPadOS, macOS, and Windows from a single shared codebase.

### Key Technologies
- **.NET 9.0.105** (current version per global.json)
- **C#** and **XAML** for application development
- **Cake build system** for compilation and packaging
- **MSBuild** with custom build tasks
- **xUnit** for testing

## Development Environment Setup

### Prerequisites

#### Linux Development (Current Environment)
```bash
# Install .NET 9 SDK
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# Verify installation
dotnet --version  # Should show 9.0.105 or newer
```

#### Additional Requirements
- **OpenJDK 17** for Android development
- **VS Code** with .NET MAUI Dev Kit extension
- **Android SDK** for Android development
- **Platform-specific workloads** (installed via dotnet workload restore)

### Initial Repository Setup

1. **Clone and navigate to repository:**
   ```bash
   git clone https://github.com/dotnet/maui.git
   cd maui
   ```

2. **Restore tools and build tasks (REQUIRED before opening IDE):**
   ```bash
   dotnet tool restore
   dotnet build ./Microsoft.Maui.BuildTasks.slnf
   ```

3. **Install platform workloads:**
   ```bash
   dotnet workload restore
   ```

## Project Structure

### Important Directories
- `src/Core/` - Core MAUI framework code
- `src/Controls/` - UI controls and components
- `src/Essentials/` - Platform APIs and essentials
- `src/TestUtils/` - Testing utilities and infrastructure
- `docs/` - Development documentation
- `eng/` - Build engineering and tooling
- `.github/` - GitHub workflows and configuration

### Solution Files
- `Microsoft.Maui.sln` - Main solution file
- `Microsoft.Maui-windows.slnf` - Windows-specific solution filter
- `Microsoft.Maui-mac.slnf` - macOS-specific solution filter
- `Microsoft.Maui-vscode.sln` - VS Code optimized solution
- `Microsoft.Maui.BuildTasks.slnf` - Build tasks solution (must build first)

### Sample Projects
- `src/Controls/samples/Maui.Controls.Sample` - Full gallery sample
- `src/Controls/samples/Maui.Controls.Sample.Sandbox` - Empty project for testing/reproduction
- `src/Essentials/samples/Essentials.Sample` - Essentials API demonstrations

## Development Workflow

### IDE Setup

#### VS Code (Recommended for Linux)
1. Open repository root folder in VS Code
2. Install the ".NET MAUI" extension by Microsoft
3. Wait for IntelliSense to initialize (may take several minutes)
4. Use Command Palette (`Ctrl+Shift+P`):
   - "Pick Device" to select target platform
   - "Pick Startup Project" to select project to run

#### Visual Studio (Windows)
- Open `Microsoft.Maui-windows.slnf` in Visual Studio 2022 v17.12+

### Building

#### Using Cake (Recommended)
```bash
# Build everything
dotnet cake

# Clean build (removes obj/bin folders)
dotnet cake --clean

# Target specific platforms
dotnet cake --target=VS --workloads=global --android --ios

# Pack NuGet packages
dotnet cake --target=dotnet-pack
```

#### Using dotnet CLI
```bash
# Build specific projects
dotnet build src/Core/src/Microsoft.Maui.csproj
dotnet build Microsoft.Maui.sln
```

### Testing and Debugging

#### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests

# Run specific test with parameters
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests --logger "console;verbosity=diagnostic" --filter "Name=Build\(%22maui%22,%22net7.0%22,%22Debug%22,False\)"
```

#### Debugging
1. Use the Sandbox project (`src/Controls/samples/Controls.Sample.Sandbox`) for reproduction
2. Add your reproduction code to the Sandbox project
3. Set breakpoints in MAUI source code
4. Select Sandbox as startup project in VS Code
5. Debug normally - breakpoints in MAUI source will be hit

**Note:** Do not commit changes to the Sandbox project in PRs.

### Local Development with Branch-Specific .NET

For compatibility with specific branches:
```bash
# Use branch-specific .NET version
./dotnet-local.sh build    # Linux/macOS
./dotnet-local.cmd build   # Windows
```

## Platform-Specific Development

### Android
- Requires Android SDK and OpenJDK 17
- Use `--android` flag with Cake builds
- Android SDK Manager available via: `android` command (after dotnet tool restore)

### iOS (requires macOS)
- Requires Xcode installation
- Use `--ios` flag with Cake builds
- Pair to Mac required when developing on Windows

### Windows
- Requires Windows SDK
- Use `--windows` flag with Cake builds
- Open `Microsoft.Maui-windows.slnf` in Visual Studio

### macOS/Mac Catalyst
- Requires Xcode installation
- Use `--catalyst` flag with Cake builds

## Common Commands

```bash
# Clean everything (use when switching branches)
git clean -xdf

# Restore packages and tools
dotnet restore
dotnet tool restore

# Build build tasks (required first)
dotnet build ./Microsoft.Maui.BuildTasks.slnf

# Install/update workloads
dotnet workload restore
dotnet workload update

# Generate API documentation
dotnet cake --target=dotnet-pack-docs
```

## Troubleshooting

### IntelliSense Issues
- Reload VS Code window
- Unload/reload `maui.core` and `maui.controls` projects
- Wait for background tasks to complete

### Build Issues
- Run `dotnet cake --clean` to clean obj/bin folders
- Use `git clean -xdf` as last resort (loses uncommitted changes)
- Ensure build tasks are built first: `dotnet build ./Microsoft.Maui.BuildTasks.slnf`

### Platform Issues
- Run `git clean -xdf` when changing/adding platforms
- Verify required SDKs are installed
- Check workload installation: `dotnet workload list`

## Contribution Guidelines

### Branching
- `main` - For bug fixes without API changes
- `net10.0` - For new features and API changes

### Documentation
- Update XML documentation for public APIs
- Follow existing code documentation patterns
- Update relevant docs in `docs/` folder when needed

### Testing
- Add tests for new functionality
- Ensure existing tests pass
- Use Integration tests for end-to-end scenarios

## Additional Resources

- [Development Guide](.github/DEVELOPMENT.md)
- [Development Tips](docs/DevelopmentTips.md)
- [Contributing Guidelines](.github/CONTRIBUTING.md)
- [Testing Wiki](https://github.com/dotnet/maui/wiki/Testing)
- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui)

---

**Note for Future Updates:** This document should be expanded as new development patterns, tools, or workflows are discovered. Add sections for specific scenarios, debugging techniques, or tooling as they become relevant to the development process.