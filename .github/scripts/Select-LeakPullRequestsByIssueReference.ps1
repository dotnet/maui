#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$InputPath,
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [Parameter(Mandatory = $true)][int]$IssueNumber,
    [Parameter(Mandatory = $true)][string]$Repository
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

$pullRequests = @(
    Read-RegularJsonFile `
        -Path $InputPath `
        -MaximumBytes 128MB
)
$matches = @(
    Select-LeakPullRequestsReferencingIssue `
        -PullRequests $pullRequests `
        -IssueNumber $IssueNumber `
        -Repository $Repository
)
ConvertTo-Json -InputObject $matches -Depth 5 |
    Set-Content -LiteralPath $OutputPath
