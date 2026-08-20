#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [AllowEmptyString()]
    [string]$Left,
    [Parameter(Mandatory = $true)]
    [AllowEmptyString()]
    [string]$Right
)

$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

if (Test-LeakApiIdentityMatch -Left $Left -Right $Right) {
    exit 0
}

exit 1
