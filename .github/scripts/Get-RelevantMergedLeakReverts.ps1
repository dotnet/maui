#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Repository,
    [Parameter(Mandatory = $true)][string]$MergedFixTsvPath,
    [Parameter(Mandatory = $true)][string]$MergedPullRequestsJsonPath,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

$fixPullRequests = @(
    Get-Content -LiteralPath $MergedFixTsvPath |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        ForEach-Object {
            $fields = $_ -split "`t", 4
            if ($fields.Count -lt 3 -or
                $fields[1] -notmatch '^[1-9][0-9]*$' -or
                [string]::IsNullOrWhiteSpace($fields[2])) {
                throw "Malformed merged-fix TSV row: $_"
            }
            [pscustomobject]@{
                number = [int]$fields[1]
                baseRefName = $fields[2]
            }
        }
)

$mergedPullRequests = @(
    Read-RegularJsonFile `
        -Path $MergedPullRequestsJsonPath `
        -MaximumBytes 128MB
)
$reverts = @(
    Get-RelevantMergedLeakReverts `
        -Repository $Repository `
        -TargetPullRequests $fixPullRequests `
        -MergedPullRequests $mergedPullRequests
)
ConvertTo-Json -InputObject @($reverts) -Depth 5 |
    Set-Content -LiteralPath $OutputPath
