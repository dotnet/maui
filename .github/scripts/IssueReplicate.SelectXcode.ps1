#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [ValidateSet('None', 'Azure', 'GitHub')][string]$Provider = 'None'
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
$sdk = Get-IssueReplicateIOSSdkVersion -RepoRoot $RepoRoot
$choices = @(Get-ChildItem -LiteralPath /Applications -Directory -Filter 'Xcode_*.app' | ForEach-Object {
    if ($_.Name -cmatch "^Xcode_($([regex]::Escape($sdk))(?:\.[0-9]+)?)\.app$") {
        [pscustomobject]@{ Path = $_.FullName; Version = [version]$Matches[1] }
    }
} | Sort-Object Version -Descending)
if ($choices.Count -lt 1) { throw "The fresh runner lacks Xcode for the pinned iOS $sdk SDK." }
$selected = $choices[0]
$developer = Join-Path $selected.Path 'Contents/Developer'
if (-not (Test-Path -LiteralPath $developer -PathType Container)) { throw 'The selected Xcode is incomplete.' }
Write-Host "Pinned iOS SDK $sdk; selected Xcode $($selected.Version)."
switch ($Provider) {
    'Azure' {
        Write-Host "##vso[task.setvariable variable=XCODE]$($selected.Version)"
        Write-Host "##vso[task.setvariable variable=REQUIRED_XCODE]$($selected.Version)"
    }
    'GitHub' {
        if (-not $env:GITHUB_ENV) { throw 'The GitHub environment file is unavailable.' }
        [IO.File]::AppendAllText($env:GITHUB_ENV, "DEVELOPER_DIR=$developer`n", [Text.UTF8Encoding]::new($false))
    }
    default { $developer }
}
