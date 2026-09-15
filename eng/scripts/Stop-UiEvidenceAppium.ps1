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
if ($state.started -eq $true -and $null -ne $state.processId) {
    $process = Get-Process -Id ([int]$state.processId) -ErrorAction SilentlyContinue
    if ($null -ne $process) {
        try {
            $process.Kill($true)
        }
        catch {
        }
        finally {
            $process.Dispose()
        }
    }
}
