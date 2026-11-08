#!/usr/bin/env pwsh
#Requires -Version 7.0

function Get-ReadinessReleaseState {
    param([string]$RepositoryPath = (Get-Location).Path)

    $statePath = Join-Path $RepositoryPath 'eng/release-rollover.json'
    if (-not (Test-Path -LiteralPath $statePath)) { return $null }
    # Use the trusted validator, not a second implementation of its state schema.
    $resolver = Join-Path $PSScriptRoot '../../../../eng/scripts/Get-ReleaseRolloverPlan.ps1'
    $json = & $resolver -StatePath $statePath -ResolveStateOnly
    return ($json | ConvertFrom-Json -ErrorAction Stop)
}

function Resolve-ReadinessBranchRoles {
    param(
        [Parameter(Mandatory)][int]$Major,
        [string]$RepositoryPath = (Get-Location).Path,
        [string]$SourceBranch
    )

    $state = Get-ReadinessReleaseState -RepositoryPath $RepositoryPath
    $configured = $null -ne $state -and $state.releaseBand -match "^$Major\.0\."
    if (-not $SourceBranch) {
        if ($configured) {
            $SourceBranch = $state.fixBranch
        } else {
            if (-not (Get-Command Get-MainBranchForVersion -ErrorAction SilentlyContinue)) {
                Import-Module (Join-Path $PSScriptRoot '../../../scripts/shared/MauiReleaseVersioning.psm1')
            }
            $SourceBranch = Get-MainBranchForVersion -Major $Major -Repo $RepositoryPath
        }
    }

    [pscustomobject]@{
        configured = $configured
        configurationOnly = $true
        sourceBranch = $SourceBranch
        featureBranch = if ($configured) { $state.featureBranch } else { 'main' }
        releaseBand = if ($configured) { $state.releaseBand } else { "$Major.0.1xx" }
        currentSr = if ($configured) { $state.currentSr } else { $null }
        releaseBranch = if ($configured) { "release/$($state.releaseBand)-sr$($state.currentSr)" } else { $null }
        candidateBranch = if ($configured) { $state.candidateBranch } else { $null }
        inflightBranch = if ($configured) { $state.inflightBranch } else { 'inflight/current' }
        nightlyBranch = if ($configured) { $state.nightlyBranch } else { 'inflight/current' }
        nightlyMode = if ($configured) { $state.nightlyMode } else { 'legacy' }
        stateSha256 = if ($configured) { $state.stateSha256 } else { $null }
    }
}

function Get-ReadinessNightlyParameters {
    param([Parameter(Mandatory)]$Roles, [Parameter(Mandatory)][int]$Major,
          [string]$RepositoryPath = (Get-Location).Path,
          [AllowEmptyString()][string]$VersionContent)

    if (-not $Roles.configured) { return @{} }
    $ref = "origin/$($Roles.nightlyBranch)"
    if ($PSBoundParameters.ContainsKey('VersionContent')) {
        $xml = $VersionContent
    } else {
        $content = & git -C $RepositoryPath show "${ref}:eng/Versions.props" 2>$null
        if ($LASTEXITCODE -ne 0) { throw "Cannot read active nightly lane '$ref'; feed freshness is UNKNOWN." }
        $xml = $content -join "`n"
    }
    if ($xml -notmatch '<MajorVersion>(\d+)</MajorVersion>' -or [int]$Matches[1] -ne $Major) {
        throw "Active nightly lane '$ref' does not identify .NET $Major; feed freshness is UNKNOWN."
    }
    if ($xml -notmatch '<PatchVersion>(\d+)</PatchVersion>') {
        throw "Active nightly lane '$ref' has no PatchVersion; feed freshness is UNKNOWN."
    }
    $patch = [int]$Matches[1]
    if ($Roles.nightlyMode -eq 'versioned') {
        $expectedPatch = ([int]$Roles.currentSr + 1) * 10
        if ($patch -lt $expectedPatch -or $patch -ge ($expectedPatch + 10)) {
            throw "Active nightly lane '$ref' declares PatchVersion $patch; expected $expectedPatch..$($expectedPatch + 9) for SR$([int]$Roles.currentSr + 1). Version advance is unverified; feed freshness is UNKNOWN."
        }
    }
    $versionBase = "$Major.0.$patch"
    @{
        InflightPrefixRegex = '^' + [regex]::Escape($versionBase) + '-ci\.inflight\.'
        RequireInflight = $true
    }
}
