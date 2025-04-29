# Component Governance Manifest (cgmanifest.json) Management

This document explains how to use the scripts to manage and update the `cgmanifest.json` file in the MAUI project.

## What is cgmanifest.json?

The Component Governance Manifest (`cgmanifest.json`) is a file that lists all the third-party components used in the project. It helps with tracking dependencies and their versions for security and compliance purposes.

## Automatic Generation

The project includes scripts to automatically generate and update the `cgmanifest.json` file with package versions from the `Versions.props` file:

### Using Cake Build Script

Run the `GenerateCgManifest` task:

```bash
# On macOS/Linux
./build.sh --target=GenerateCgManifest

# On Windows
.\build.ps1 -Target GenerateCgManifest
```

### Using Scripts Directly

You can also run the scripts directly:

```bash
# On macOS/Linux
./eng/scripts/update-cgmanifest.sh

# On Windows
pwsh -ExecutionPolicy Bypass -File .\eng\scripts\update-cgmanifest.ps1
```

### MSBuild Integration

The `CgManifest.targets` file provides MSBuild integration. You can include it in your project and run:

```bash
dotnet build -t:GenerateCgManifest
```

#### For CI Only

For CI builds, you can explicitly enable the cgmanifest.json generation:

```bash
dotnet build -p:GenerateCgManifest=true
```

This will:
1. Generate the cgmanifest.json file
2. Include it in the package (for Template projects)

#### For Development Builds

By default, the cgmanifest.json generation is disabled for normal development builds. 
This ensures your local builds are faster and don't unnecessarily modify the cgmanifest.json file.

If needed, you can manually enable it for a specific build:

```bash
dotnet build -p:UpdateCgManifestBeforeBuild=true
```

## Customizing Package Mappings

To add or modify package mappings, edit the relevant section in:

- PowerShell script: `eng/scripts/update-cgmanifest.ps1` (look for `$packageVersionMappings` hashtable)
- Bash script: `eng/scripts/update-cgmanifest.sh` (look for `PACKAGE_MAPPINGS` associative array)

## Manual Updates

If you need to manually add packages that aren't in `Versions.props`, you can edit the `cgmanifest.json` file directly. The update scripts will preserve any manually added entries and only update versions for packages it knows about.
