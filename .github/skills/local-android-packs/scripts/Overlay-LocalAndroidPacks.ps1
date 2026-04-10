<#
.SYNOPSIS
    Overlay locally built dotnet/android packs onto the provisioned .NET SDK.

.DESCRIPTION
    Replaces provisioned Android workload packs with locally built pack directories
    from a dotnet/android build. This lets you test local android changes against a
    MAUI build without waiting for dependency flow.

    The script backs up everything it modifies and can fully restore via -Restore.

.PARAMETER AndroidSrcPath
    Path to the local dotnet/android repository root.

.PARAMETER Configuration
    Build configuration to use for locating packs. Default: Release.
    The local packs are expected at: <AndroidSrcPath>/bin/<Configuration>/lib/packs/

.PARAMETER Restore
    Undo a previous overlay by restoring the original manifest and removing
    overlaid pack directories.

.EXAMPLE
    # Overlay Release packs
    ./Overlay-LocalAndroidPacks.ps1 -AndroidSrcPath ~/repos/android

.EXAMPLE
    # Overlay Debug packs
    ./Overlay-LocalAndroidPacks.ps1 -AndroidSrcPath ~/repos/android -Configuration Debug

.EXAMPLE
    # Restore original packs
    ./Overlay-LocalAndroidPacks.ps1 -Restore
#>

[CmdletBinding(DefaultParameterSetName = 'Overlay')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Overlay')]
    [string]$AndroidSrcPath,

    [Parameter(ParameterSetName = 'Overlay')]
    [string]$Configuration = 'Release',

    [Parameter(Mandatory = $true, ParameterSetName = 'Restore')]
    [switch]$Restore
)

$ErrorActionPreference = 'Stop'

# Dot-source shared utilities for Write-Step, Write-Info, Write-Success, Write-Warn
. "$PSScriptRoot/../../../scripts/shared/shared-utils.ps1"

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

# Script is at .github/skills/local-android-packs/scripts/ — repo root is 4 levels up
$RepoRoot = (Resolve-Path "$PSScriptRoot/../../../..").Path
$DotnetRoot = Join-Path $RepoRoot '.dotnet'
$BackupDir = Join-Path $DotnetRoot '.android-packs-backup'
$BackupMetadataFile = Join-Path $BackupDir 'backup-metadata.json'

# Manifest entry names to skip during overlay
$SkipPacks = @('Microsoft.Android.Sdk.net10', 'Microsoft.Android.Templates')

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function ConvertFrom-JsonSafe {
    <#
    .SYNOPSIS
        Parse JSON that may contain trailing commas (common in workload manifests).
        PowerShell versions before 7.5 are strict about trailing commas in JSON;
        this provides backward compatibility by stripping them before parsing.
    #>
    param([string]$JsonText)
    $cleaned = $JsonText -replace ',\s*([\]\}])', '$1'
    return $cleaned | ConvertFrom-Json
}

function Find-AndroidManifest {
    <#
    .SYNOPSIS
        Locate the Android WorkloadManifest.json under .dotnet/sdk-manifests/.
        Returns the path to the manifest file. The manifest may be directly in
        the manifest dir or inside a version subdirectory.
    #>
    param([string]$DotnetRoot)

    $manifestsRoot = Join-Path $DotnetRoot 'sdk-manifests'
    if (-not (Test-Path $manifestsRoot)) {
        throw "No sdk-manifests directory found at: $manifestsRoot"
    }

    $candidates = Get-ChildItem -Path $manifestsRoot -Directory | Sort-Object Name -Descending
    foreach ($band in $candidates) {
        $androidDir = Join-Path $band.FullName 'microsoft.net.sdk.android'
        if (-not (Test-Path $androidDir)) { continue }

        # Manifest can be directly in the dir or in a version subdir
        $directManifest = Join-Path $androidDir 'WorkloadManifest.json'
        if (Test-Path $directManifest) {
            return $directManifest
        }

        # Check version subdirectories (e.g., 36.1.2/)
        $versionDirs = Get-ChildItem -Path $androidDir -Directory | Sort-Object Name -Descending
        foreach ($vdir in $versionDirs) {
            $versionedManifest = Join-Path $vdir.FullName 'WorkloadManifest.json'
            if (Test-Path $versionedManifest) {
                return $versionedManifest
            }
        }
    }

    throw "No Android WorkloadManifest.json found under: $manifestsRoot. Run './build.sh --restore' to provision the SDK."
}

function Get-PlatformSdkAlias {
    <#
    .SYNOPSIS
        Return the platform-specific SDK pack name based on the current OS.
    #>
    if ($IsMacOS) { return 'Microsoft.Android.Sdk.Darwin' }
    if ($IsLinux) { return 'Microsoft.Android.Sdk.Linux' }
    if ($IsWindows) { return 'Microsoft.Android.Sdk.Windows' }
    # Fallback: try to detect from runtime
    throw "Cannot determine platform SDK alias — unsupported OS."
}

function Get-LocalPackVersion {
    <#
    .SYNOPSIS
        Detect the local build version by examining the version subdirectory
        inside any pack directory under the local packs path.
    #>
    param([string]$LocalPacksPath)

    $packDirs = Get-ChildItem -Path $LocalPacksPath -Directory
    foreach ($packDir in $packDirs) {
        $versionDirs = Get-ChildItem -Path $packDir.FullName -Directory | Sort-Object Name -Descending
        if ($versionDirs.Count -gt 0) {
            return $versionDirs[0].Name
        }
    }

    throw "Could not detect local pack version — no version subdirectories found under: $LocalPacksPath"
}

function Get-ManifestNet11Packs {
    <#
    .SYNOPSIS
        Return the list of pack entries in the manifest that should be overlaid
        (net11 packs only, excluding skip list). Returns an array of pack names.
    #>
    param([PSCustomObject]$Manifest)

    $result = @()
    $installedVersion = $Manifest.version

    $packMembers = $Manifest.packs | Get-Member -MemberType NoteProperty
    foreach ($member in $packMembers) {
        $packName = $member.Name

        # Skip packs in the skip list
        if ($packName -in $SkipPacks) { continue }

        # Only overlay packs whose version matches the manifest's top-level version
        # (this filters out net10 backcompat packs that have a different version)
        $packInfo = $Manifest.packs.$packName
        if ($packInfo.version -ne $installedVersion) { continue }

        $result += $packName
    }

    return $result
}

# ---------------------------------------------------------------------------
# Restore flow
# ---------------------------------------------------------------------------

function Invoke-Restore {
    Write-Step "Restoring original Android packs..."

    if (-not (Test-Path $BackupMetadataFile)) {
        throw "No backup metadata found at: $BackupMetadataFile`nNothing to restore."
    }

    $metadata = Get-Content -Path $BackupMetadataFile -Raw | ConvertFrom-Json
    Write-Info "Backup from: $($metadata.timestamp)"
    Write-Info "Original version: $($metadata.originalVersion)"
    Write-Info "Local version: $($metadata.localVersion)"

    $errors = @()

    # Step 1: Remove overlaid pack directories
    foreach ($pack in $metadata.overlaidPacks) {
        $localVersionDir = Join-Path $DotnetRoot "packs/$($pack.targetPackName)/$($metadata.localVersion)"
        try {
            if (Test-Path $localVersionDir) {
                Write-Info "  Removing $($pack.targetPackName)/$($metadata.localVersion)"
                Remove-Item -Path $localVersionDir -Recurse -Force
            }
            else {
                Write-Warn "  Local version directory not found (already removed?): $localVersionDir"
            }
        }
        catch {
            $errors += "Failed to remove $($pack.targetPackName): $_"
        }
    }

    # Step 2: Restore original manifest
    $manifestBackup = Join-Path $BackupDir 'WorkloadManifest.json'
    if (Test-Path $manifestBackup) {
        $manifestPath = $metadata.manifestPath
        Write-Info "  Restoring manifest: $manifestPath"
        try {
            Copy-Item -Path $manifestBackup -Destination $manifestPath -Force
        }
        catch {
            $errors += "Failed to restore manifest: $_"
        }
    }
    else {
        $errors += "Manifest backup not found: $manifestBackup"
    }

    # Step 3: Clean up backup directory
    try {
        Remove-Item -Path $BackupDir -Recurse -Force
    }
    catch {
        $errors += "Failed to remove backup directory: $_"
    }

    # Summary
    if ($errors.Count -gt 0) {
        Write-Warn "Restore completed with errors:"
        foreach ($err in $errors) {
            Write-Warn "  $err"
        }
    }
    else {
        Write-Success "Restore complete. All packs reverted to version $($metadata.originalVersion)."
    }
}

# ---------------------------------------------------------------------------
# Overlay flow
# ---------------------------------------------------------------------------

function Invoke-Overlay {
    param(
        [string]$AndroidSrcPath,
        [string]$Configuration
    )

    $localPacksPath = Join-Path $AndroidSrcPath "bin/$Configuration/lib/packs"

    # --- Step 1: Validate inputs ---
    Write-Step "Validating inputs..."

    if (-not (Test-Path $AndroidSrcPath)) {
        throw "Android source path does not exist: $AndroidSrcPath"
    }

    if (-not (Test-Path $localPacksPath)) {
        throw "Local packs directory not found: $localPacksPath`nVerify that dotnet/android was built with configuration '$Configuration'."
    }

    if (-not (Test-Path $DotnetRoot)) {
        throw ".dotnet directory not found at: $DotnetRoot`nRun './build.sh --restore' to provision the SDK."
    }

    $manifestPath = Find-AndroidManifest -DotnetRoot $DotnetRoot
    Write-Info "Manifest: $manifestPath"

    # Check for existing overlay
    if (Test-Path $BackupMetadataFile) {
        throw "A previous overlay is still active (found $BackupMetadataFile).`nRun with -Restore first to revert, then re-run the overlay."
    }

    # --- Step 2: Read manifest and detect versions ---
    Write-Step "Reading manifest and detecting versions..."

    $manifestRaw = Get-Content -Path $manifestPath -Raw
    $manifest = ConvertFrom-JsonSafe -JsonText $manifestRaw
    $installedVersion = $manifest.version
    Write-Info "Installed version: $installedVersion"

    $platformSdkAlias = Get-PlatformSdkAlias
    Write-Info "Platform SDK alias: $platformSdkAlias"

    $localVersion = Get-LocalPackVersion -LocalPacksPath $localPacksPath
    Write-Info "Local build version: $localVersion"

    if ($localVersion -eq $installedVersion) {
        throw "Local build version ($localVersion) matches the installed version." + `
              " Overlay would delete the existing packs and leave the SDK broken on restore." + `
              " Build dotnet/android at a different version, or modify the version suffix before overlaying."
    }

    $net11Packs = Get-ManifestNet11Packs -Manifest $manifest
    Write-Info "Net11 manifest packs found: $($net11Packs.Count)"

    # --- Step 3: Determine which packs are available locally ---
    Write-Step "Scanning local packs..."

    $localPackDirs = Get-ChildItem -Path $localPacksPath -Directory

    # Build a lookup: local pack dir name → local pack dir path
    $localPackLookup = @{}
    foreach ($dir in $localPackDirs) {
        $localPackLookup[$dir.Name] = $dir.FullName
    }

    # Determine which manifest packs can actually be overlaid (exist locally)
    $overlayablePacks = @()
    $skippedPacks = @()

    foreach ($manifestPackName in $net11Packs) {
        # Determine which local directory corresponds to this manifest entry
        if ($manifestPackName -eq 'Microsoft.Android.Sdk.net11') {
            # SDK pack uses platform alias both locally and in .dotnet/packs/
            $localDirName = $platformSdkAlias
            $targetPackName = $platformSdkAlias
        }
        else {
            # All other packs: directory name matches manifest pack name
            $localDirName = $manifestPackName
            $targetPackName = $manifestPackName
        }

        # Check if the local pack exists
        if (-not $localPackLookup.ContainsKey($localDirName)) {
            $skippedPacks += @{ ManifestName = $manifestPackName; Reason = "Not found in local build" }
            continue
        }

        $localPackDir = $localPackLookup[$localDirName]

        # Verify the local pack has a version subdirectory
        $versionDirs = Get-ChildItem -Path $localPackDir -Directory | Sort-Object Name -Descending
        if ($versionDirs.Count -eq 0) {
            $skippedPacks += @{ ManifestName = $manifestPackName; Reason = "No version subdirectory" }
            continue
        }

        $overlayablePacks += @{
            manifestPackName = $manifestPackName
            targetPackName   = $targetPackName
            localDirName     = $localDirName
            localVersionDir  = $versionDirs[0].FullName
        }
    }

    Write-Info "Packs available to overlay: $($overlayablePacks.Count)"
    if ($skippedPacks.Count -gt 0) {
        Write-Info "Packs skipped (not in local build): $($skippedPacks.Count)"
    }

    if ($overlayablePacks.Count -eq 0) {
        throw "No local packs match manifest entries. Verify that dotnet/android was built with configuration '$Configuration'."
    }

    # --- Step 4: Create backup ---
    Write-Step "Creating backup..."

    if (-not (Test-Path $BackupDir)) {
        New-Item -Path $BackupDir -ItemType Directory -Force | Out-Null
    }
    Copy-Item -Path $manifestPath -Destination (Join-Path $BackupDir 'WorkloadManifest.json') -Force

    # Write backup metadata immediately so auto-rollback works even if overlay fails partway through
    $metadata = @{
        originalVersion = $installedVersion
        localVersion    = $localVersion
        manifestPath    = $manifestPath
        timestamp       = (Get-Date -Format 'o')
        config          = $Configuration
        overlaidPacks   = @()
    }
    $metadata | ConvertTo-Json -Depth 5 | Set-Content -Path $BackupMetadataFile

    # --- Step 5: Patch manifest (only for packs confirmed to exist locally) ---
    Write-Step "Patching manifest..."

    $manifest.version = $localVersion
    foreach ($pack in $overlayablePacks) {
        $packInfo = $manifest.packs.($pack.manifestPackName)
        if ($packInfo -and $packInfo.version -eq $installedVersion) {
            $packInfo.version = $localVersion
        }
    }

    $manifest | ConvertTo-Json -Depth 10 | Set-Content -Path $manifestPath

    # --- Step 6: Place packs ---
    Write-Step "Placing packs..."

    $overlaidPacks = @()

    foreach ($pack in $overlayablePacks) {
        # Target path in .dotnet/packs/
        $targetDir = Join-Path $DotnetRoot "packs/$($pack.targetPackName)/$localVersion"

        try {
            # Copy the pack
            if (Test-Path $targetDir) {
                Write-Warn "  Target already exists, replacing: $($pack.targetPackName)/$localVersion"
                Remove-Item -Path $targetDir -Recurse -Force
            }

            New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
            Copy-Item -Path "$($pack.localVersionDir)/*" -Destination $targetDir -Recurse -Force
        }
        catch {
            # Clean up partial directory immediately — metadata hasn't been updated
            # for this pack yet, so auto-rollback wouldn't know to remove it.
            Remove-Item -Path $targetDir -Recurse -Force -ErrorAction SilentlyContinue
            throw  # re-throw to trigger outer catch for manifest rollback
        }

        # Only reached on success — track this pack for rollback
        $overlaidPacks += @{
            manifestPackName = $pack.manifestPackName
            targetPackName   = $pack.targetPackName
            localDirName     = $pack.localDirName
        }

        # Update metadata incrementally so rollback can clean up partial overlays on failure
        $metadata.overlaidPacks = $overlaidPacks
        $metadata | ConvertTo-Json -Depth 5 | Set-Content -Path $BackupMetadataFile

        Write-Info "  $($pack.targetPackName) : $installedVersion → $localVersion"
    }

    # --- Step 7: Summary ---
    Write-Host ""
    Write-Success "=== Overlay Complete ==="
    Write-Host ""
    Write-Host "  Installed version : $installedVersion"
    Write-Host "  Local version     : $localVersion"
    Write-Host "  Packs overlaid    : $($overlaidPacks.Count)"
    Write-Host ""

    foreach ($p in $overlaidPacks) {
        Write-Host "    ✅ $($p.targetPackName)" -ForegroundColor Green
    }

    if ($skippedPacks.Count -gt 0) {
        Write-Host ""
        Write-Host "  Packs skipped     : $($skippedPacks.Count)" -ForegroundColor Yellow
        foreach ($s in $skippedPacks) {
            Write-Host "    ⏭️  $($s.ManifestName) — $($s.Reason)" -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-Host "  To restore original packs:" -ForegroundColor Yellow
    Write-Host "    $($MyInvocation.MyCommand.Name) -Restore" -ForegroundColor Yellow
    Write-Host ""
}

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

try {
    Write-Step "Android Packs Overlay"
    Write-Info "Repo root: $RepoRoot"
    Write-Info "DOTNET_ROOT: $DotnetRoot"

    if ($Restore) {
        Invoke-Restore
    }
    else {
        $resolvedAndroidPath = (Resolve-Path $AndroidSrcPath).Path
        Invoke-Overlay -AndroidSrcPath $resolvedAndroidPath -Configuration $Configuration
    }
}
catch {
    # On overlay failure, attempt automatic rollback if backup exists
    if (-not $Restore -and (Test-Path $BackupMetadataFile)) {
        Write-Warn "Overlay failed — attempting automatic rollback..."
        try {
            Invoke-Restore
            Write-Info "Automatic rollback succeeded."
        }
        catch {
            Write-Warn "Automatic rollback also failed: $_"
            Write-Warn "Manual cleanup may be required. Check $BackupDir for backup files."
        }
    }

    Write-Host ""
    Write-Error "Fatal: $_"
    exit 1
}
