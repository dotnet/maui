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

$builds
