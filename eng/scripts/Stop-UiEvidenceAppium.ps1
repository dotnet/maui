#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$StatePath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

if (-not (Test-Path -LiteralPath $StatePath -PathType Leaf)) {
    exit 0
}
$state = Read-UiEvidenceJson $StatePath
if ($state.schemaVersion -ne 2 -or [string]::IsNullOrWhiteSpace([string]$state.processStartedAtUtc)) {
    throw "Appium state does not establish process ownership; refusing to stop a process."
}
if ($state.started -eq $true -and $null -ne $state.processId) {
    $process = Get-Process -Id ([int]$state.processId) -ErrorAction SilentlyContinue
    if ($null -ne $process) {
        try {
            $expectedStart = if ($state.processStartedAtUtc -is [DateTime]) {
                $state.processStartedAtUtc.ToUniversalTime()
            }
            else {
                [DateTime]::Parse(
                    [string]$state.processStartedAtUtc, [Globalization.CultureInfo]::InvariantCulture,
                    [Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
            }
            if ($process.StartTime.ToUniversalTime() -ne $expectedStart) {
                throw "Appium process identity changed; refusing to stop an unrelated process."
            }
            $process.Kill($true)
            if (-not $process.WaitForExit(10000)) {
                throw "The owned Appium process did not exit."
            }
        }
        finally {
            $process.Dispose()
        }
    }
}
