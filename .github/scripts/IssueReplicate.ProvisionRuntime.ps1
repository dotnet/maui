#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [Parameter(Mandatory)][ValidatePattern('^[0-9]+\.[0-9]+$')][string]$SdkVersion
)

$ErrorActionPreference = 'Stop'
if (-not $IsMacOS) { throw 'iOS runtime provisioning requires a hosted macOS runner.' }
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
. (Join-Path $RepoRoot '.github/scripts/shared/shared-utils.ps1')
$developer = & (Join-Path $PSScriptRoot 'IssueReplicate.SelectXcode.ps1') `
    -RepoRoot $RepoRoot -SdkVersion $SdkVersion
$env:DEVELOPER_DIR = $developer
if ($env:GITHUB_ENV) {
    [IO.File]::AppendAllText($env:GITHUB_ENV, "DEVELOPER_DIR=$developer`n", [Text.UTF8Encoding]::new($false))
} elseif ($env:SYSTEM_COLLECTIONURI -cmatch '^https://dev\.azure\.com/dnceng-public/') {
    Write-Host "##vso[task.setvariable variable=DEVELOPER_DIR]$developer"
}
$runtimeId = "com.apple.CoreSimulator.SimRuntime.iOS-$($SdkVersion.Replace('.', '-'))"
function Test-InstalledRuntime {
    $inventory = Invoke-ProcessWithTimeout -FilePath 'xcrun' -TimeoutSeconds 30 `
        -ArgumentList @('simctl', 'list', 'runtimes', 'available', '--json')
    if ($inventory.TimedOut -or $inventory.OutputDrainTimedOut -or $inventory.ExitCode -ne 0) {
        throw 'Could not inspect the hosted iOS runtime inventory within its deadline.'
    }
    $record = ($inventory.Output -join "`n") | ConvertFrom-Json
    return @($record.runtimes | Where-Object {
        $_.identifier -ceq $runtimeId -and $_.version -ceq $SdkVersion -and $_.isAvailable -eq $true
    }).Count -eq 1
}
if (-not (Test-InstalledRuntime)) {
    $download = Invoke-ProcessWithTimeout -FilePath 'xcodebuild' -TimeoutSeconds 1200 `
        -ArgumentList @('-downloadPlatform', 'iOS', '-buildVersion', $SdkVersion, '-architectureVariant', 'arm64')
    foreach ($row in @($download.Output | Select-Object -Last 60)) {
        Write-Host ($row.ToString().Replace("`r", '') -replace '##vso\[[^]]*\]', '')
    }
    if ($download.TimedOut -or $download.OutputDrainTimedOut -or $download.ExitCode -ne 0) {
        throw "Could not provision the exact author-build iOS $SdkVersion runtime within its twenty-minute deadline."
    }
}
if (-not (Test-InstalledRuntime)) { throw "The author-build iOS $SdkVersion runtime is unavailable; refusing a substitution." }
Write-Host "Author-build toolchain: Xcode at $developer; exact available runtime $runtimeId."
