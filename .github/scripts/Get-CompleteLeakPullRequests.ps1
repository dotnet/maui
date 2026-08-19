#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Repository,
    [Parameter(Mandatory = $true)]
    [ValidateSet('OPEN', 'CLOSED', 'MERGED')]
    [string]$State,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

$pullRequests = @(
    Get-CompleteLeakPullRequests `
        -Repository $Repository `
        -State $State
)
ConvertTo-Json -InputObject $pullRequests -Depth 5 |
    Set-Content -LiteralPath $OutputPath
