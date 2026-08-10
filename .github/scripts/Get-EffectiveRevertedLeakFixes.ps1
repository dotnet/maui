#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Repository,
    [Parameter(Mandatory = $true)][string]$MergedFixTsvPath,
    [Parameter(Mandatory = $true)][string]$MergedRevertsJsonPath,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

$fixNumbers = @(
    Get-Content -LiteralPath $MergedFixTsvPath |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object {
            $fields = $_ -split "`t", 3
            if ($fields.Count -lt 2 -or $fields[1] -notmatch '^[1-9][0-9]*$') {
                throw "Malformed merged-fix TSV row: $_"
            }
            [int]$fields[1]
        }
)

$reverts = @(
    Get-Content -LiteralPath $MergedRevertsJsonPath -Raw |
        ConvertFrom-Json |
        Where-Object { $null -ne $_.mergedAt }
)

$effectiveReverted = @(
    Get-EffectiveRevertedPullRequestNumbers `
        -Repository $Repository `
        -FixPullRequestNumbers $fixNumbers `
        -MergedRevertPullRequests $reverts
)

Set-Content -LiteralPath $OutputPath -Value $effectiveReverted
