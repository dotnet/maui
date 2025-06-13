# GitHub Copilot Development Environment Instructions

This document provides specific guidance for GitHub Copilot when working on the .NET MAUI repository. It serves as context for understanding the project structure, development workflow, and best practices.

## Repository Overview

**.NET MAUI** is a cross-platform framework for creating mobile and desktop applications with C# and XAML. This repository contains the core framework code that enables development for Android, iOS, iPadOS, macOS, and Windows from a single shared codebase.

### Key Technologies
- **.NET SDK** - Version depends on the branch:
  - **main branch**: Use the latest stable version of .NET to build
  - **net10 branch**: Use the latest .NET 10 SDK
  - **net11 branch**: Use the latest .NET 11 SDK
  - **etc.**: Each feature branch correlates to its respective .NET version
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

## Project Structure

### Important Directories
- `src/Core/` - Core MAUI framework code
- `src/Controls/` - UI controls and components
- `src/Essentials/` - Platform APIs and essentials
- `src/TestUtils/` - Testing utilities and infrastructure
- `docs/` - Development documentation
- `eng/` - Build engineering and tooling
- `.github/` - GitHub workflows and configuration

### Platform-Specific Code Organization
- **Android** specific code is inside folders labeled `Android`
- **iOS** specific code is inside folders labeled `iOS`
- **MacCatalyst** specific code is inside folders named `MacCatalyst`
- **Windows** specific code is inside folders named `Windows`

### Solution Files
- `Microsoft.Maui.sln` - Main solution file
- `Microsoft.Maui-windows.slnf` - Windows-specific solution filter
- `Microsoft.Maui-mac.slnf` - macOS-specific solution filter
- `Microsoft.Maui-vscode.sln` - VS Code optimized solution
- `Microsoft.Maui.BuildTasks.slnf` - Build tasks solution (must build first)

### Sample Projects
```
├── Controls 
│   ├── samples
│   │   ├── Maui.Controls.Sample
│   │   ├── Maui.Controls.Sample.Sandbox
├── Essentials 
│   ├── samples
│   │   ├── Essentials.Sample
├── BlazorWebView 
│   ├── samples
│   │   ├── BlazorWinFormsApp
│   │   ├── BlazorWpfApp
```

- `src/Controls/samples/Maui.Controls.Sample` - Full gallery sample with all controls and features
- `src/Controls/samples/Maui.Controls.Sample.Sandbox` - Empty project for testing/reproduction
- `src/Essentials/samples/Essentials.Sample` - Essentials API demonstrations (non-UI MAUI APIs)
- `src/BlazorWebView/samples/` - BlazorWebView sample applications

## Development Workflow

### IDE Setup

#### VS Code (Recommended for Linux/macOS)
1. Open repository root folder in VS Code
2. Install the ".NET MAUI Dev Kit" extension from the marketplace: https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-maui
3. Wait for IntelliSense to initialize (may take several minutes to fully process the solution)
4. Use Command Palette (`Ctrl+Shift+P`):
   - "Pick Device" to select target platform
   - "Pick Startup Project" to select project to run

**Note:** IntelliSense takes considerable time to fully process the solution. If experiencing issues, unload/reload the `maui.core` and `maui.controls` projects to resolve problems.

#### Visual Studio (Windows)
- Install Visual Studio 2022 v17.12 or newer
- Follow [these steps](https://learn.microsoft.com/dotnet/maui/get-started/installation?tabs=vswin) to include MAUI workload
- Open `Microsoft.Maui-windows.slnf` in Visual Studio from the repository root

### Building

#### Using Cake (Recommended)
```bash
# Build everything
dotnet cake

# Clean build (removes obj/bin folders)
dotnet cake --clean

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
- Install missing Android SDKs via [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk)
- Android SDK Manager available via: `android` command (after dotnet tool restore)

### iOS (requires macOS)
- Requires current stable Xcode installation from [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
- Pair to Mac required when developing on Windows

### Windows
- Requires Windows SDK
- Open `Microsoft.Maui-windows.slnf` in Visual Studio

### macOS/Mac Catalyst
- Requires Xcode installation

## Common Commands

```bash
# Clean everything (use when switching branches)
git clean -xdf

# Restore packages and tools
dotnet restore
dotnet tool restore

# Build build tasks (required first)
dotnet build ./Microsoft.Maui.BuildTasks.slnf

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

## Contribution Guidelines

### Files to Never Commit
- **Never** check in changes to `cgmanifest.json` files
- **Never** check in changes to `templatestrings.json` files
- These files are automatically generated and should not be modified manually

### Branching
- `main` - For bug fixes without API changes
- `net10.0` - For new features and API changes

**Note:** The main branch is always pinned to the latest stable release of the .NET SDK, regardless of whether it's a long-term support (LTS) release. Ensure you have that version installed to build the codebase.

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