#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Apply a .NET MAUI PR build to your project.

.DESCRIPTION
    This script downloads and applies NuGet packages from a specific .NET MAUI pull request build
    to your local project. It automatically detects your project's target framework and updates
    the necessary package references.
    
    The script uses a hive-based approach, storing packages in: ~/.maui/hives/pr-<PR_NUMBER>/packages

.PARAMETER PrNumber
    The pull request number to apply. Required. Can be passed as positional parameter.

.PARAMETER ProjectPath
    The path to the .csproj file. If not specified, searches for a MAUI project in the current directory.

.EXAMPLE
    iex "& { $(irm https://raw.githubusercontent.com/dotnet/maui/main/eng/scripts/get-maui-pr.ps1) } 33002"

.EXAMPLE
    ./get-maui-pr.ps1 33002

.EXAMPLE
    ./get-maui-pr.ps1 -PrNumber 33002 -ProjectPath ./MyApp/MyApp.csproj

.NOTES
    This script requires:
    - .NET SDK installed
    - Internet connection to access GitHub and Azure DevOps APIs
    - A valid .NET MAUI project
    
    Repository Override:
    Set MAUI_REPO environment variable to point to a fork (e.g., 'myfork/maui')

    For more information about testing PR builds, visit:
    https://github.com/dotnet/maui/wiki/Testing-PR-Builds
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PrNumber,

    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = ""
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Configuration - Allow override via environment variable
$GitHubRepo = if ($env:MAUI_REPO) { $env:MAUI_REPO } else { "dotnet/maui" }
$AzureDevOpsOrg = "xamarin"
$AzureDevOpsProject = "public"
$PackageName = "Microsoft.Maui.Controls"

# Color output functions
function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Step {
    param([string]$Message)
    Write-Host "`n▶️  $Message" -ForegroundColor Blue
}

# Find MAUI project
function Find-MauiProject {
    param([string]$SearchPath)

    if ([string]::IsNullOrEmpty($SearchPath)) {
        $SearchPath = Get-Location
    }

    if (Test-Path $SearchPath -PathType Leaf) {
        if ($SearchPath -match '\.csproj$') {
            return $SearchPath
        }
        throw "The specified file is not a .csproj file: $SearchPath"
    }

    $projects = Get-ChildItem -Path $SearchPath -Filter "*.csproj" -File
    
    foreach ($project in $projects) {
        $content = Get-Content $project.FullName -Raw
        if ($content -match '<UseMaui>true</UseMaui>') {
            return $project.FullName
        }
    }

    throw "No .NET MAUI project found in $SearchPath. Make sure you're in a directory containing a MAUI project (.csproj with <UseMaui>true</UseMaui>)."
}

# Get PR information from GitHub
function Get-PullRequestInfo {
    param([int]$PrNumber)

    Write-Info "Fetching PR #$PrNumber information from GitHub..."
    
    try {
        $prUrl = "https://api.github.com/repos/$GitHubRepo/pulls/$PrNumber"
        $pr = Invoke-RestMethod -Uri $prUrl -Headers @{ "User-Agent" = "MAUI-PR-Script" }
        
        return @{
            Number = $pr.number
            Title = $pr.title
            State = $pr.state
            SHA = $pr.head.sha
            Ref = $pr.head.ref
        }
    }
    catch {
        throw "Failed to fetch PR information. Make sure PR #$PrNumber exists. Error: $_"
    }
}

# Get build information from GitHub Checks API
function Get-BuildInfo {
    param([string]$SHA)

    Write-Info "Looking for build artifacts for commit $($SHA.Substring(0, 7))..."
    
    try {
        $checksUrl = "https://api.github.com/repos/$GitHubRepo/commits/$SHA/check-runs"
        $response = Invoke-RestMethod -Uri $checksUrl -Headers @{ 
            "User-Agent" = "MAUI-PR-Script"
            "Accept" = "application/vnd.github.v3+json"
        }
        
        # Look for the main MAUI build check
        $buildCheck = $response.check_runs | Where-Object { 
            $_.name -eq "MAUI-public" -and $_.status -eq "completed"
        } | Select-Object -First 1
        
        if (-not $buildCheck) {
            throw "No completed build found for this PR. The build may still be in progress or may have failed."
        }
        
        if ($buildCheck.conclusion -ne "success") {
            Write-Warning "Build completed with status: $($buildCheck.conclusion)"
            $continue = Read-Host "Do you want to continue anyway? (y/N)"
            if ($continue -ne "y" -and $continue -ne "Y") {
                throw "Build was not successful. Aborting."
            }
        }
        
        # Extract build ID from details URL
        if ($buildCheck.details_url -match 'buildId=(\d+)') {
            $buildId = $Matches[1]
            Write-Success "Found build ID: $buildId"
            return $buildId
        }
        
        throw "Could not extract build ID from check run details."
    }
    catch {
        throw "Failed to get build information: $_"
    }
}

# Get artifacts from Azure DevOps
function Get-BuildArtifacts {
    param([string]$BuildId)

    Write-Info "Fetching artifacts from Azure DevOps build $BuildId..."
    
    try {
        $artifactsUrl = "https://dev.azure.com/$AzureDevOpsOrg/$AzureDevOpsProject/_apis/build/builds/$BuildId/artifacts?api-version=7.1"
        $response = Invoke-RestMethod -Uri $artifactsUrl -Headers @{ "User-Agent" = "MAUI-PR-Script" }
        
        # Look for nuget artifact
        $artifact = $response.value | Where-Object { $_.name -eq "nuget" } | Select-Object -First 1
        
        if (-not $artifact) {
            throw "No 'nuget' artifact found in build $BuildId"
        }
        
        return $artifact.resource.downloadUrl
    }
    catch {
        throw "Failed to get artifact information: $_"
    }
}

# Download and extract artifacts
function Get-Artifacts {
    param([string]$DownloadUrl, [string]$BuildId)

    # Use hive directory pattern like Aspire CLI
    $hiveDir = if ($IsWindows -or $env:OS -eq "Windows_NT") {
        Join-Path $env:USERPROFILE ".maui\hives\pr-$PrNumber"
    } else {
        Join-Path $env:HOME ".maui/hives/pr-$PrNumber"
    }
    $packagesDir = Join-Path $hiveDir "packages"
    $tempDir = $hiveDir
    $zipFile = Join-Path $tempDir "artifacts.zip"
    $extractDir = $packagesDir
    
    if (Test-Path $tempDir) {
        Write-Info "Cleaning up previous download..."
        Remove-Item $tempDir -Recurse -Force
    }
    
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    New-Item -ItemType Directory -Path $extractDir -Force | Out-Null
    
    Write-Info "Downloading artifacts (this may take a moment)..."
    try {
        Invoke-WebRequest -Uri $DownloadUrl -OutFile $zipFile -UseBasicParsing
        Write-Success "Downloaded artifacts"
        
        Write-Info "Extracting artifacts..."
        Expand-Archive -Path $zipFile -DestinationPath $extractDir -Force
        
        # Find the NuGet packages directory
        $nupkgDir = Get-ChildItem -Path $extractDir -Recurse -Directory | 
            Where-Object { (Get-ChildItem -Path $_.FullName -Filter "*.nupkg" -File).Count -gt 0 } |
            Select-Object -First 1
        
        if (-not $nupkgDir) {
            throw "Could not find NuGet packages in the extracted artifacts"
        }
        
        return $nupkgDir.FullName
    }
    catch {
        throw "Failed to download or extract artifacts: $_"
    }
}

# Get package version from directory
function Get-PackageVersion {
    param([string]$PackagesDir)

    $package = Get-ChildItem -Path $PackagesDir -Filter "$PackageName.*.nupkg" -File |
        Where-Object { $_.Name -notmatch '\.symbols\.nupkg$' } |
        Select-Object -First 1
    
    if (-not $package) {
        throw "Could not find $PackageName package in artifacts"
    }
    
    if ($package.Name -match "$PackageName\.(.+)\.nupkg") {
        return $Matches[1]
    }
    
    throw "Could not extract version from package filename: $($package.Name)"
}

# Detect target framework version
function Get-TargetFrameworkVersion {
    param([string]$ProjectPath)

    $content = Get-Content $ProjectPath -Raw
    
    # Look for TargetFramework or TargetFrameworks
    if ($content -match '<TargetFrameworks?>([^<]+)</TargetFrameworks?>') {
        $tfms = $Matches[1]
        
        # Extract .NET version (e.g., net9.0, net10.0)
        if ($tfms -match 'net(\d+)\.0') {
            $netVersion = [int]$Matches[1]
            return $netVersion
        }
    }
    
    throw "Could not determine target framework version from project file"
}

# Check if version matches target framework
function Test-VersionCompatibility {
    param([string]$Version, [int]$TargetNetVersion, [int]$PackageNetVersion)

    # PR builds are typically for the latest .NET version
    # Check if the package targets a newer .NET version than the project
    
    if ($Version -match 'preview' -or $Version -match 'ci\.') {
        if ($TargetNetVersion -lt $PackageNetVersion) {
            return $false
        }
    }
    
    return $true
}

# Extract .NET version from package version
function Get-PackageDotNetVersion {
    param([string]$Version)
    
    # Extract major version from package (e.g., "10.0.20-ci..." -> 10)
    if ($Version -match '^(\d+)\.') {
        return [int]$Matches[1]
    }
    
    # Default to current stable if can't determine
    return 9
}

# Update target frameworks
function Update-TargetFrameworks {
    param([string]$ProjectPath, [int]$NewNetVersion)

    $content = Get-Content $ProjectPath -Raw
    
    # Update all netX.0-* references (including in conditional TargetFrameworks)
    $content = $content -replace 'net\d+\.0-', "net$NewNetVersion.0-"
    
    Set-Content -Path $ProjectPath -Value $content -NoNewline
    Write-Success "Updated target frameworks to .NET $NewNetVersion.0"
    Write-Warning "You may need to update other package dependencies to match .NET $NewNetVersion.0"
}

# Create or update NuGet.config
function Update-NuGetConfig {
    param([string]$ProjectDir, [string]$PackagesDir)

    $nugetConfigPath = Join-Path $ProjectDir "NuGet.config"
    $sourceName = "maui-pr-build"
    
    if (Test-Path $nugetConfigPath) {
        Write-Info "Updating existing NuGet.config..."
        [xml]$config = Get-Content $nugetConfigPath
        
        # Ensure packageSources exists
        if (-not $config.configuration.packageSources) {
            $packageSources = $config.CreateElement("packageSources")
            $config.configuration.AppendChild($packageSources) | Out-Null
        }
        
        # Remove existing source with same name
        $existingSource = $config.configuration.packageSources.add | Where-Object { $_.key -eq $sourceName }
        if ($existingSource) {
            $config.configuration.packageSources.RemoveChild($existingSource) | Out-Null
        }
        
        # Add new source
        $add = $config.CreateElement("add")
        $add.SetAttribute("key", $sourceName)
        $add.SetAttribute("value", $PackagesDir)
        $config.configuration.packageSources.AppendChild($add) | Out-Null
        
        $config.Save($nugetConfigPath)
    }
    else {
        Write-Info "Creating new NuGet.config..."
        $config = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="$sourceName" value="$PackagesDir" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
"@
        Set-Content -Path $nugetConfigPath -Value $config
    }
    
    Write-Success "NuGet.config configured with local package source"
}

# Update project package reference
function Update-PackageReference {
    param([string]$ProjectPath, [string]$Version)

    $content = Get-Content $ProjectPath -Raw
    
    # Replace the version in PackageReference, handling both explicit version and $(MauiVersion)
    $pattern = "(<PackageReference\s+Include=`"$PackageName`"\s+Version=`")([^`"]+)(`"\s*/?)"
    $replacement = "`${1}$Version`${3}"
    
    if ($content -match $pattern) {
        $oldVersion = $Matches[2]
        $content = $content -replace $pattern, $replacement
        Set-Content -Path $ProjectPath -Value $content -NoNewline
        
        if ($oldVersion -eq '$(MauiVersion)') {
            Write-Success "Updated $PackageName from `$(MauiVersion) to explicit version $Version"
        } else {
            Write-Success "Updated $PackageName from $oldVersion to $Version"
        }
    }
    else {
        throw "Could not find $PackageName package reference in project file"
    }
}

# Main execution
try {
    Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        .NET MAUI PR Build Applicator                     ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

    Write-Step "Finding MAUI project"
    $projectPath = Find-MauiProject -SearchPath $ProjectPath
    $projectDir = Split-Path $projectPath -Parent
    $projectName = Split-Path $projectPath -Leaf
    Write-Success "Found project: $projectName"
    
    Write-Step "Fetching PR information"
    $prInfo = Get-PullRequestInfo -PrNumber $PrNumber
    Write-Info "PR #$($prInfo.Number): $($prInfo.Title)"
    Write-Info "State: $($prInfo.State)"
    
    if ($prInfo.State -ne "open" -and $prInfo.State -ne "closed") {
        Write-Warning "PR state is '$($prInfo.State)'. Continuing anyway..."
    }
    
    Write-Step "Detecting target framework"
    $targetNetVersion = Get-TargetFrameworkVersion -ProjectPath $projectPath
    Write-Info "Current target framework: .NET $targetNetVersion.0"
    
    Write-Step "Finding build artifacts"
    $buildId = Get-BuildInfo -SHA $prInfo.SHA
    
    Write-Step "Downloading artifacts"
    $downloadUrl = Get-BuildArtifacts -BuildId $buildId
    $packagesDir = Get-Artifacts -DownloadUrl $downloadUrl -BuildId $buildId
    
    Write-Step "Extracting package information"
    $version = Get-PackageVersion -PackagesDir $packagesDir
    Write-Success "Found package version: $version"
    
    # Get package .NET version
    $packageNetVersion = Get-PackageDotNetVersion -Version $version
    
    # Check compatibility
    $compatible = Test-VersionCompatibility -Version $version -TargetNetVersion $targetNetVersion -PackageNetVersion $packageNetVersion
    $willUpdateTfm = $false
    if (-not $compatible) {
        Write-Warning "This PR build may target a newer .NET version than your project"
        Write-Info "Your project targets: .NET $targetNetVersion.0"
        Write-Info "This PR build targets: .NET $packageNetVersion.0"
        
        $response = Read-Host "`nDo you want to update your project to .NET $packageNetVersion.0? (y/N)"
        if ($response -eq "y" -or $response -eq "Y") {
            $willUpdateTfm = $true
            Write-Warning "Note: You may need to manually update other package dependencies to versions compatible with .NET $packageNetVersion.0"
        }
        else {
            Write-Warning "Continuing without updating target framework. The package may not be compatible."
        }
    }
    
    # Confirmation prompt
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  CONFIRMATION" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "By continuing, you will apply the PR artifacts to your project." -ForegroundColor Cyan
    Write-Host ""
    Write-Warning "This should NOT be used in production and is for testing purposes only."
    Write-Host ""
    Write-Host "TIP: Create a separate Git branch for testing!" -ForegroundColor Cyan
    Write-Host "     git checkout -b test-pr-$PrNumber" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Please test the changes you are looking for, check for any side-effects," -ForegroundColor White
    Write-Host "and report your findings on:" -ForegroundColor White
    Write-Host "  https://github.com/dotnet/maui/pull/$PrNumber" -ForegroundColor Blue
    Write-Host ""
    Write-Host "Changes to be applied:" -ForegroundColor White
    Write-Host "  • Project: $projectName" -ForegroundColor Gray
    Write-Host "  • Package version: $version" -ForegroundColor Gray
    if ($willUpdateTfm) {
        $targetVersionForDisplay = if ($packageDotNetVersion) { "$packageDotNetVersion.0" } else { "10.0" }
        Write-Host "  • Target framework: Will be updated to .NET $targetVersionForDisplay" -ForegroundColor Gray
    }
    Write-Host ""
    
    $response = Read-Host "Do you want to continue? (y/N)"
    if ($response -ne "y" -and $response -ne "Y") {
        Write-Warning "Operation cancelled by user"
        exit 0
    }
    Write-Host ""
    
    if ($willUpdateTfm) {
        $targetNetVersionToApply = if ($packageDotNetVersion) { [int]$packageDotNetVersion } else { 10 }
        Update-TargetFrameworks -ProjectPath $projectPath -NewNetVersion $targetNetVersionToApply
        $targetNetVersion = $targetNetVersionToApply
    }
    
    Write-Step "Configuring NuGet sources"
    Update-NuGetConfig -ProjectDir $projectDir -PackagesDir $packagesDir
    
    Write-Step "Updating package reference"
    Update-PackageReference -ProjectPath $projectPath -Version $version
    
    # Get latest stable version for revert instructions
    try {
        $nugetResponse = Invoke-RestMethod -Uri "https://api.nuget.org/v3-flatcontainer/microsoft.maui.controls/index.json" -UseBasicParsing
        $stableVersions = $nugetResponse.versions | Where-Object { $_ -notmatch '-' } | Sort-Object -Descending
        $latestStable = $stableVersions[0]
    }
    catch {
        $latestStable = "X.Y.Z"
    }

    Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                                                           ║
║        ✅ Successfully applied PR #$PrNumber!                  ║
║                                                           ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

    Write-Info "Next steps:"
    Write-Host "  1. Run 'dotnet restore' to download the packages" -ForegroundColor White
    Write-Host "  2. Build and test your project with the PR changes" -ForegroundColor White
    Write-Host "  3. Report your findings on: https://github.com/dotnet/maui/pull/$PrNumber" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "Package: $PackageName $version"
    Write-Info "Local package source: $packagesDir"
    Write-Host ""
    
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  TO REVERT TO PRODUCTION VERSION" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "1. Edit $projectName and change the version:" -ForegroundColor White
    Write-Host "   From: Version=`"$version`"" -ForegroundColor Gray
    Write-Host "   To:   Version=`"X.Y.Z`"" -ForegroundColor Gray
    Write-Host "   (Check https://www.nuget.org/packages/$PackageName for latest)" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "2. In NuGet.config, remove or comment out the 'maui-pr-$PrNumber' source" -ForegroundColor White
    Write-Host ""
    Write-Host "3. Run: dotnet restore --force" -ForegroundColor White
    Write-Host ""
    Write-Host "TIP: Use a separate Git branch for testing PR builds!" -ForegroundColor Cyan
    Write-Host "     Then you can easily revert: git checkout main" -ForegroundColor Cyan
    Write-Host ""
    
}
catch {
    Write-Error "Failed to apply PR build: $_"
    Write-Host ""
    Write-Info "Troubleshooting tips:"
    Write-Host "  • Make sure you're in a directory containing a .NET MAUI project" -ForegroundColor Gray
    Write-Host "  • Verify that PR #$PrNumber exists: https://github.com/dotnet/maui/pull/$PrNumber" -ForegroundColor Gray
    Write-Host "  • Check if there's a completed build for this PR (look for green checkmarks)" -ForegroundColor Gray
    Write-Host "  • Check your internet connection" -ForegroundColor Gray
    Write-Host "  • Visit: https://github.com/dotnet/maui/wiki/Testing-PR-Builds" -ForegroundColor Gray
    
    exit 1
}
