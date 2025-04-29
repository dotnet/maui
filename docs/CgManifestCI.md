# CG Manifest for CI Buil- name: Pack with CG Manifest
  run: dotnet pack src/Templates/src/Microsoft.Maui.Templates.csproj -p:GenerateCgManifest=true -o $GITHUB_WORKSPACE/artifacts/nuget
```

## Local Testing of CI Build

To test the CI build process locally, run this command:

```bash
# Pack with cgmanifest.json included in the package
dotnet pack src/Templates/src/Microsoft.Maui.Templates.csproj -p:GenerateCgManifest=true -o ./artifacts/packagesprovides instructions for including the Component Governance manifest in your CI pipeline packages.

## Azure DevOps Pipeline

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

Note: The cgmanifest.json file is always generated during build, so you don't need a separate step to generate it.

## GitHub Actions

For GitHub Actions workflows, add this step:

```yaml
- name: Pack with CG Manifest
  run: dotnet pack src/Templates/src/Microsoft.Maui.Templates.csproj -p:GenerateCgManifest=true -o $GITHUB_WORKSPACE/artifacts/nuget
  
- name: Pack with CG Manifest
  run: dotnet pack src/Templates/src/Microsoft.Maui.Templates.csproj -p:GenerateCgManifest=true -o $GITHUB_WORKSPACE/artifacts/nuget
```

## Local Testing of CI Build

To test the CI build process locally, run these commands:

```bash
# Generate the cgmanifest.json file
dotnet build src/Templates/src/Microsoft.Maui.Templates.csproj -t:GenerateCgManifest

# Pack with cgmanifest.json included
dotnet pack src/Templates/src/Microsoft.Maui.Templates.csproj -p:GenerateCgManifest=true -o ./artifacts/packages
```

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
