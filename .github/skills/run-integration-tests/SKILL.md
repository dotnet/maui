---
name: run-integration-tests
description: "Build, pack, and run .NET MAUI integration tests locally. Validates templates, samples, and end-to-end scenarios using the local workload."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires Windows for WindowsTemplates category. macOS for macOSTemplates, RunOniOS, RunOnAndroid.
---

# Run Integration Tests Skill

Build the MAUI product, install local workloads, and run integration tests.

## When to Use

- User asks to "run integration tests"
- User asks to "test templates locally"
- User asks to "validate MAUI build with templates"
- User wants to verify changes don't break template scenarios
- User asks to run specific test categories (WindowsTemplates, Samples, Build, Blazor, etc.)

## Available Test Categories

| Category | Platform | Description |
|----------|----------|-------------|
| `Build` | All | Basic template build tests |
| `WindowsTemplates` | Windows | Windows-specific template scenarios |
| `macOSTemplates` | macOS | macOS-specific scenarios |
| `Blazor` | All | Blazor hybrid templates |
| `MultiProject` | All | Multi-project templates |
| `Samples` | All | Sample project builds |
| `AOT` | macOS | Native AOT compilation |
| `RunOnAndroid` | macOS | Build, install, run on Android emulator |
| `RunOniOS` | macOS | iOS simulator tests |

## Scripts

All scripts are in `.github/skills/run-integration-tests/scripts/`

### Run Integration Tests (Full Workflow)

```powershell
# Run with specific category
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "WindowsTemplates"

# Run with Release configuration
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "Samples" -Configuration "Release"

# Run with custom test filter
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -TestFilter "FullyQualifiedName~BuildSample"

# Skip build step (if already built)
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "Build" -SkipBuild
```

## Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-Category` | No | - | Test category to run (WindowsTemplates, Samples, Build, etc.) |
| `-TestFilter` | No | - | Custom NUnit test filter expression |
| `-Configuration` | No | Debug | Build configuration (Debug/Release) |
| `-SkipBuild` | No | false | Skip build/pack step if already done |
| `-SkipInstall` | No | false | Skip workload installation if already done |
| `-ResultsDirectory` | No | artifacts/integration-tests | Directory for test results |

## Workflow Steps

The script performs these steps:

1. **Build & Pack**: `.\build.cmd -restore -pack -configuration $Configuration`
2. **Install Workloads**: `.dotnet\dotnet build .\src\DotNet\DotNet.csproj -t:Install -c $Configuration`
3. **Extract Version**: Reads MAUI_PACKAGE_VERSION from installed packs
4. **Run Tests**: `.dotnet\dotnet test ... -filter "Category=$Category"`

## Example Usage

```powershell
# Run WindowsTemplates tests
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "WindowsTemplates"

# Run Samples tests
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "Samples"

# Run multiple categories
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -TestFilter "Category=Build|Category=Blazor"
```

## Prerequisites

- Windows for WindowsTemplates
- .NET SDK (version from global.json)
- Sufficient disk space for build artifacts

## Output

- Test results in TRX format at `<ResultsDirectory>/`
- Build logs in `artifacts/` directory
- Console output with test pass/fail summary

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "MAUI_PACKAGE_VERSION was not set" | Ensure build step completed successfully |
| Template not found | Workload installation may have failed |
| Build failures | Check `artifacts/log/` for detailed build logs |
| "Cannot proceed with locked .dotnet folder" | Kill processes using `.dotnet`: `Get-Process \| Where-Object { $_.Path -like "*\.dotnet\*" } \| ForEach-Object { Stop-Process -Id $_.Id -Force }` |
