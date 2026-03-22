# Component Governance Manifest (cgmanifest.json)

This document explains how to manage, update, and include the `cgmanifest.json` file in the MAUI project.

## What is cgmanifest.json?

The Component Governance Manifest (`cgmanifest.json`) is a file that lists all the third-party components used in the project. It helps with tracking dependencies and their versions for security and compliance purposes.

## Automatic Generation

The project includes scripts to automatically generate and update the `cgmanifest.json` file with package versions from the `Versions.props` file.

### Using Cake Build Script

Run the `GenerateCgManifest` task:

```bash
dotnet tool resotre
dotnet cake --target=GenerateCgManifest --workloads=global
```

### Using Scripts Directly

You can also run the PowerShell script directly (works on both Windows and macOS/Linux):

```bash
# On all platforms (Windows/macOS/Linux)
pwsh -ExecutionPolicy Bypass -File ./eng/scripts/update-cgmanifest.ps1
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

#### Disabling Generation

If needed, you can temporarily disable the automatic generation:

```bash
dotnet build -p:UpdateCgManifestBeforeBuild=false
```

## Including CG Manifest in CI Builds

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

### Azure DevOps Pipeline

Add the following step to your YAML pipeline to include the cgmanifest.json file in the package:

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Pack with CG Manifest'
  inputs:
    command: 'pack'
    packagesToPack: 'src/Templates/src/Microsoft.Maui.Templates.csproj'
    packDirectory: '$(Build.ArtifactStagingDirectory)/nuget'
    arguments: '-p:GenerateCgManifest=true'
```

### GitHub Actions

For GitHub Actions workflows, add this step:

```yaml
- name: Pack with CG Manifest
  run: dotnet pack src/Templates/src/Microsoft.Maui.Templates.csproj -p:GenerateCgManifest=true -o $GITHUB_WORKSPACE/artifacts/nuget
```

## Customizing Package Mappings

To add or modify package mappings, edit the PowerShell script: `eng/scripts/update-cgmanifest.ps1` (look for `$packageVersionMappings` hashtable).

## Special Handling for Multiple Versions

The script has special handling for packages that need multiple versions to be included in the manifest:

- **CommunityToolkit.Maui**: Both the current version (`CommunityToolkitMauiPackageVersion`) and previous version (`CommunityToolkitMauiPreviousPackageVersion`) from `Versions.props` are included.

To add similar handling for other packages, modify the script to add special case handling like that implemented for CommunityToolkit.Maui.

## Manual Updates

If you need to manually add packages that aren't in `Versions.props`, you can edit the `cgmanifest.json` file directly. The update scripts preserve manually added entries and only update versions for packages it knows about.

## Verifying the Package

To verify that the cgmanifest.json file is included in the package:

```bash
# On macOS/Linux
find ./artifacts/packages -name "Microsoft.Maui.Templates*.nupkg" | xargs -I{} unzip -l {} | grep cgmanifest.json

# On Windows
foreach ($pkg in (Get-ChildItem -Path ./artifacts/packages -Filter "Microsoft.Maui.Templates*.nupkg")) { 
  Add-Type -AssemblyName System.IO.Compression.FileSystem
  $zip = [System.IO.Compression.ZipFile]::OpenRead($pkg.FullName)
  $zip.Entries | Where-Object { $_.Name -eq "cgmanifest.json" -or $_.FullName -like "*cgmanifest.json" }
  $zip.Dispose()
}
```
