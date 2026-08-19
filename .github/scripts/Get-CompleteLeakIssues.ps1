#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Repository,
    [Parameter(Mandatory = $true)][string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

$issues = @(Get-CompleteLeakIssues -Repository $Repository)
ConvertTo-Json -InputObject $issues -Depth 5 |
    Set-Content -LiteralPath $OutputPath
