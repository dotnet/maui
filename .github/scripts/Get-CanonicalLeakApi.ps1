#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [AllowEmptyString()]
    [string]$Title,
    [switch]$ExistingTitle
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

$api = if ($ExistingTitle) {
    Get-CanonicalExistingLeakApi -Title $Title
} else {
    Get-CanonicalLeakApi -Title $Title
}
if (-not [string]::IsNullOrWhiteSpace($api)) {
    Write-Output $api
}
