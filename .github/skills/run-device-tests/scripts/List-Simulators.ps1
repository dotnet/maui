<#
.SYNOPSIS
    Lists available iOS simulators.

.DESCRIPTION
    Queries xcrun simctl to list available iOS simulators, optionally filtering
    by iOS version.

.PARAMETER iOSVersion
    Optional iOS version filter (e.g., "26", "18.4"). Shows all if not specified.

.PARAMETER BootedOnly
    If specified, only shows booted simulators.

.EXAMPLE
    ./List-Simulators.ps1

.EXAMPLE
    ./List-Simulators.ps1 -iOSVersion 26

.EXAMPLE
    ./List-Simulators.ps1 -BootedOnly
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$iOSVersion,

    [Parameter(Mandatory = $false)]
    [switch]$BootedOnly
)

$ErrorActionPreference = "Stop"

# Check for xcrun
if (-not (Get-Command "xcrun" -ErrorAction SilentlyContinue)) {
    Write-Error "xcrun is not available. Xcode must be installed."
    exit 1
}

Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Available iOS Simulators" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Get simulators as JSON
$jsonOutput = & xcrun simctl list devices available --json 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to list simulators: $jsonOutput"
    exit 1
}

$simulators = $jsonOutput | ConvertFrom-Json

# Process devices
$devices = @()

foreach ($runtime in $simulators.devices.PSObject.Properties) {
    $runtimeName = $runtime.Name
    
    # Extract iOS version from runtime identifier
    # Format: com.apple.CoreSimulator.SimRuntime.iOS-26-2 -> 26.2
    if ($runtimeName -match "iOS-(\d+)-(\d+)") {
        $version = "$($matches[1]).$($matches[2])"
    } elseif ($runtimeName -match "iOS-(\d+)") {
        $version = $matches[1]
    } else {
        continue # Skip non-iOS runtimes
    }
    
    # Filter by version if specified
    if ($iOSVersion -and -not $version.StartsWith($iOSVersion)) {
        continue
    }
    
    foreach ($device in $runtime.Value) {
        # Filter booted only if specified
        if ($BootedOnly -and $device.state -ne "Booted") {
            continue
        }
        
        $devices += [PSCustomObject]@{
            Name       = $device.name
            UDID       = $device.udid
            State      = $device.state
            iOSVersion = $version
            Runtime    = $runtimeName
        }
    }
}

# Sort by iOS version (descending) then name
$devices = $devices | Sort-Object -Property @{Expression = {[version]$_.iOSVersion}; Descending = $true}, Name

if ($devices.Count -eq 0) {
    if ($iOSVersion) {
        Write-Host "No simulators found for iOS $iOSVersion" -ForegroundColor Yellow
    } else {
        Write-Host "No iOS simulators found" -ForegroundColor Yellow
    }
    exit 0
}

# Display results
$currentVersion = ""
foreach ($device in $devices) {
    if ($device.iOSVersion -ne $currentVersion) {
        $currentVersion = $device.iOSVersion
        Write-Host ""
        Write-Host "── iOS $currentVersion ──" -ForegroundColor Yellow
    }
    
    $stateColor = if ($device.State -eq "Booted") { "Green" } else { "Gray" }
    $stateIndicator = if ($device.State -eq "Booted") { "●" } else { "○" }
    
    Write-Host "  $stateIndicator " -NoNewline -ForegroundColor $stateColor
    Write-Host "$($device.Name)" -NoNewline
    Write-Host " ($($device.UDID))" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Total: $($devices.Count) simulators" -ForegroundColor Cyan
if ($BootedOnly) {
    Write-Host "  (showing booted only)" -ForegroundColor Gray
}
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

# Output for programmatic use
$devices
