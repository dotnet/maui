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

The `CgManifest.targets` file provides MSBuild integration.

#### Default Behavior

By default, the cgmanifest.json file is **always generated** during build, but it is **not included** in the NuGet package. This ensures the manifest is always up-to-date without affecting package contents.

#### Manual Generation

You can explicitly generate the manifest file:

```bash
dotnet build -t:GenerateCgManifest
```

#### For CI Builds

For CI builds where you want to include the cgmanifest.json in the package:

```bash
dotnet build -p:GenerateCgManifest=true
```
OR
```bash
dotnet pack -p:GenerateCgManifest=true
```

This will:
1. Generate the cgmanifest.json file (happens by default)
2. Include it in the package (for Template projects)

#### Disabling Generation

If needed, you can temporarily disable the automatic generation:

```bash
dotnet build -p:UpdateCgManifestBeforeBuild=false
```

## Customizing Package Mappings

To add or modify package mappings, edit the relevant section in:

- PowerShell script: `eng/scripts/update-cgmanifest.ps1` (look for `$packageVersionMappings` hashtable)
- Bash script: `eng/scripts/update-cgmanifest.sh` (look for `PACKAGE_MAPPINGS` associative array)

## Manual Updates

If you need to manually add packages that aren't in `Versions.props`, you can edit the `cgmanifest.json` file directly. The update scripts will preserve any manually added entries and only update versions for packages it knows about.
