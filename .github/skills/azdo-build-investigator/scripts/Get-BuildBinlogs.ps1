<#
.SYNOPSIS
    Downloads .binlog files from an Azure DevOps build's Container artifacts.

.DESCRIPTION
    Container-type build artifacts (e.g., "Windows_NT_Build Windows (Debug)_Attempt1")
    cannot be downloaded via `az pipelines runs artifact download`. This script uses
    the ADO File Container API with a Bearer token to list and download .binlog files.

    Requires: az CLI logged in (`az login`) to get an access token.

.PARAMETER BuildId
    The Azure DevOps build ID.

.PARAMETER OutputDir
    Directory to download binlogs to. Defaults to /tmp/maui-binlogs.

.PARAMETER ArtifactName
    Filter to a specific artifact by name (supports wildcards). Defaults to all Container artifacts.

.EXAMPLE
    # Download all binlogs from a build
    ./Get-BuildBinlogs.ps1 -BuildId 1325582

    # Download from a specific artifact
    ./Get-BuildBinlogs.ps1 -BuildId 1325582 -ArtifactName "*Windows*Build*"

    # Custom output directory
    ./Get-BuildBinlogs.ps1 -BuildId 1325582 -OutputDir ~/Downloads/binlogs
#>
param(
    [Parameter(Mandatory = $true)]
    [int]$BuildId,

    [string]$OutputDir = "/tmp/maui-binlogs",

    [string]$ArtifactName = "*"
)

$ErrorActionPreference = "Stop"

$Org = "https://dev.azure.com/dnceng-public"
$Project = "public"
$ApiVersion = "7.1"
$ContainerApiVersion = "5.0-preview"

# --- Get auth token ---
Write-Host "Getting auth token from az CLI..."
try {
    $tokenJson = az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 2>&1
    if ($LASTEXITCODE -ne 0) { throw "az account get-access-token failed" }
    $token = ($tokenJson | ConvertFrom-Json).accessToken
} catch {
    Write-Error @"

ERROR: Could not get Azure DevOps access token.

Make sure you are logged in to az CLI:
  az login
  az extension add --name azure-devops   # optional

Then retry.
"@
    exit 1
}

$authHeaders = @{ Authorization = "Bearer $token"; Accept = "application/json" }

# --- List artifacts for the build ---
Write-Host "Listing artifacts for build $BuildId..."
$artifactsUrl = "$Org/$Project/_apis/build/builds/$BuildId/artifacts?api-version=$ApiVersion"
$artifacts = (Invoke-RestMethod -Uri $artifactsUrl -Headers $authHeaders).value

if (-not $artifacts) {
    Write-Warning "No artifacts found for build $BuildId"
    exit 0
}

# Filter to Container-type artifacts matching the name filter
$containerArtifacts = $artifacts | Where-Object {
    $_.resource.type -eq "Container" -and $_.name -like $ArtifactName
}

if (-not $containerArtifacts) {
    Write-Warning "No Container-type artifacts found matching '$ArtifactName'"
    Write-Host "Available artifacts:"
    $artifacts | ForEach-Object { Write-Host "  [$($_.resource.type)] $($_.name)" }
    exit 0
}

Write-Host "Found $($containerArtifacts.Count) Container artifact(s) to search for binlogs."

# --- Process each container artifact ---
$downloadCount = 0
foreach ($artifact in $containerArtifacts) {
    $artifactNameClean = $artifact.name
    Write-Host "`n=== $artifactNameClean ==="

    # Extract container ID from resource.data (format: #/CONTAINERID/)
    $containerId = $artifact.resource.data -replace '^#/(\d+)/?$', '$1'
    if (-not $containerId -or $containerId -eq $artifact.resource.data) {
        Write-Warning "  Could not parse container ID from: $($artifact.resource.data)"
        continue
    }
    Write-Host "  Container ID: $containerId"

    # List files in this container path
    $listUrl = "$Org/_apis/resources/Containers/${containerId}?itemPath=$([Uri]::EscapeDataString($artifactNameClean))&api-version=$ContainerApiVersion"
    try {
        $containerItems = Invoke-RestMethod -Uri $listUrl -Headers $authHeaders
    } catch {
        Write-Warning "  Failed to list container contents: $_"
        continue
    }

    $binlogFiles = $containerItems.value | Where-Object { $_.itemType -eq "file" -and $_.path -match '\.binlog$' }

    if (-not $binlogFiles) {
        Write-Host "  No .binlog files found in this artifact."
        $containerItems.value | Where-Object { $_.itemType -eq "file" } | Select-Object -First 5 | ForEach-Object {
            Write-Host "    (sample) $($_.path)"
        }
        continue
    }

    Write-Host "  Found $($binlogFiles.Count) .binlog file(s)"

    # Download each binlog
    foreach ($file in $binlogFiles) {
        $fileName = Split-Path $file.path -Leaf
        $outPath = Join-Path $OutputDir $fileName

        # Create output directory if needed
        if (-not (Test-Path $OutputDir)) {
            New-Item -ItemType Directory -Path $OutputDir | Out-Null
        }

        $downloadUrl = "$Org/_apis/resources/Containers/${containerId}?itemPath=$([Uri]::EscapeDataString($file.path))&api-version=$ContainerApiVersion&`$format=file"
        Write-Host "  Downloading: $fileName"

        try {
            Invoke-WebRequest -Uri $downloadUrl -Headers $authHeaders -OutFile $outPath
            $sizeMB = [Math]::Round((Get-Item $outPath).Length / 1MB, 1)
            Write-Host "    -> $outPath ($sizeMB MB)"
            $downloadCount++
        } catch {
            Write-Warning "    Failed to download $fileName : $_"
        }
    }
}

Write-Host ""
if ($downloadCount -gt 0) {
    Write-Host "Downloaded $downloadCount .binlog file(s) to: $OutputDir"
    Write-Host ""
    Write-Host "Analyze with binlogtool:"
    Write-Host "  binlogtool search `"$OutputDir/*.binlog`" `"error`""
    Write-Host "  binlogtool search `"$OutputDir/*.binlog`" `"error CS`"    # C# compiler errors"
    Write-Host "  binlogtool search `"$OutputDir/*.binlog`" `"error NU`"    # NuGet errors"
    Write-Host "  binlogtool reconstruct `"$OutputDir/*.binlog`" > /tmp/build.log"
    Write-Host ""
    Write-Host "Clean up when done:"
    Write-Host "  rm -rf $OutputDir"
} else {
    Write-Warning "No binlog files were downloaded."
}
