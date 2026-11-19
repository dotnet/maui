#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [Parameter(Mandatory)][ValidatePattern('^[0-9]+\.[0-9]+$')][string]$SdkVersion,
    [Parameter(Mandatory)][string]$SdkPackDirectory
)

$ErrorActionPreference = 'Stop'
if (-not $IsMacOS) { throw 'iOS runtime provisioning requires a hosted macOS runner.' }
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
. (Join-Path $RepoRoot '.github/scripts/shared/shared-utils.ps1')
$developer = & (Join-Path $PSScriptRoot 'IssueReplicate.SelectXcode.ps1') `
    -RepoRoot $RepoRoot -SdkVersion $SdkVersion -SdkPackDirectory $SdkPackDirectory
$env:DEVELOPER_DIR = $developer
$selection = Invoke-ProcessWithTimeout -FilePath 'sudo' -TimeoutSeconds 30 `
    -ArgumentList @('-n', 'xcode-select', '-s', $developer)
if ($selection.TimedOut -or $selection.OutputDrainTimedOut -or $selection.ExitCode -ne 0) {
    throw "Could not select the author-build Xcode without prompting: exit=$($selection.ExitCode), timeout=$($selection.TimedOut), drainTimeout=$($selection.OutputDrainTimedOut)."
}
$cache = Invoke-ProcessWithTimeout -FilePath 'xcrun' -TimeoutSeconds 30 -ArgumentList @('--kill-cache')
if ($cache.TimedOut -or $cache.OutputDrainTimedOut -or $cache.ExitCode -ne 0) {
    throw "Could not clear the author-build Xcode command cache: exit=$($cache.ExitCode), timeout=$($cache.TimedOut), drainTimeout=$($cache.OutputDrainTimedOut)."
}
if ($env:GITHUB_ENV) {
    [IO.File]::AppendAllText($env:GITHUB_ENV, "DEVELOPER_DIR=$developer`n", [Text.UTF8Encoding]::new($false))
}
elseif ($env:SYSTEM_COLLECTIONURI -cmatch '^https://dev\.azure\.com/dnceng-public/') {
    Write-Host "##vso[task.setvariable variable=DEVELOPER_DIR]$developer"
}
Initialize-IssueReplicateIOSRuntime -RepoRoot $RepoRoot -SdkVersion $SdkVersion
$runtimeId = "com.apple.CoreSimulator.SimRuntime.iOS-$($SdkVersion.Replace('.', '-'))"
Write-Host "Author-build toolchain: Xcode at $developer; exact available runtime $runtimeId."
