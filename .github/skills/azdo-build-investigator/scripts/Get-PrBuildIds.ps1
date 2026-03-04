<#
.SYNOPSIS
    Retrieves Azure DevOps build IDs associated with a GitHub PR.

.DESCRIPTION
    Queries GitHub PR checks and extracts the Azure DevOps build IDs,
    pipeline names, states, and links for each unique build.

.PARAMETER PrNumber
    The GitHub Pull Request number.

.PARAMETER Repo
    The GitHub repository in 'owner/repo' format. Defaults to 'dotnet/maui'.

.EXAMPLE
    ./Get-PrBuildIds.ps1 -PrNumber 33251

.EXAMPLE
    ./Get-PrBuildIds.ps1 -PrNumber 33251 -Repo "dotnet/maui"

.OUTPUTS
    Array of objects with Pipeline, BuildId, State, and Link properties.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PrNumber,

    [Parameter(Mandatory = $false)]
    [string]$Repo = "dotnet/maui"
)

$ErrorActionPreference = "Stop"

# Validate prerequisites
if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
    Write-Error "GitHub CLI (gh) is not installed. Install from https://cli.github.com/"
    exit 1
}

# Get PR checks from GitHub
$checksJson = gh pr checks $PrNumber --repo $Repo --json name,link,state 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to get PR checks: $checksJson"
    exit 1
}

$checks = $checksJson | ConvertFrom-Json

# Filter to Azure DevOps checks and extract build IDs
$builds = $checks | Where-Object { $_.link -match "dev\.azure\.com" } | ForEach-Object {
    $buildId = if ($_.link -match "buildId=(\d+)") { $matches[1] } else { $null }
    $pipeline = ($_.name -split " ")[0]
    
    [PSCustomObject]@{
        Pipeline = $pipeline
        BuildId  = $buildId
        State    = $_.state
        Link     = $_.link
    }
} | Sort-Object -Property Pipeline, BuildId -Unique

# Diagnose the no-build case — common when CI is skipped or not yet triggered
$noBuildRows = $builds | Where-Object { -not $_.BuildId }
if ($noBuildRows) {
    Write-Host ""
    Write-Host "⚠️  Some pipelines have no build ID — CI was not triggered for these:" -ForegroundColor Yellow
    foreach ($row in $noBuildRows) {
        Write-Host "   Pipeline : $($row.Pipeline)" -ForegroundColor Yellow
        Write-Host "   State    : $($row.State)" -ForegroundColor Yellow
        Write-Host "   Link     : $($row.Link)" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "   Likely causes:" -ForegroundColor Cyan
        Write-Host "     • PR only modifies path-excluded files (e.g. .github/**, docs/**)" -ForegroundColor Cyan
        Write-Host "     • Build has not been queued yet (maintainer trigger required)" -ForegroundColor Cyan
        Write-Host "     • PR is in draft state" -ForegroundColor Cyan
        Write-Host "   To check path filters: look for 'paths.exclude' in eng/pipelines/ci.yml" -ForegroundColor Cyan
        Write-Host ""
    }
}

$builds | Where-Object { $_.BuildId }
