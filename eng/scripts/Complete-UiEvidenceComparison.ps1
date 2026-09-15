#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$RawEvidenceRoot,

    [Parameter(Mandatory = $true)]
    [string]$PayloadRoot,

    [Parameter(Mandatory = $true)]
    [string]$RunnerPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

$raw = (Resolve-Path -LiteralPath $RawEvidenceRoot).Path
$payload = (Resolve-Path -LiteralPath $PayloadRoot).Path
$output = [IO.Path]::GetFullPath($OutputDirectory)
Remove-Item -LiteralPath $output -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $output | Out-Null
foreach ($item in @(Get-ChildItem -LiteralPath $raw -Force -Recurse)) {
    if (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "Raw UI evidence cannot contain reparse points."
    }
}
Get-ChildItem -LiteralPath $raw -Force | Copy-Item -Destination $output -Recurse -Force
Copy-Item -LiteralPath (Join-Path $payload "request.json") `
    -Destination (Join-Path $output "request.json") `
    -Force
Copy-Item -LiteralPath (Join-Path $payload "scenarios.json") `
    -Destination (Join-Path $output "scenarios.json") `
    -Force
Copy-Item -LiteralPath (Join-Path $payload "payload-manifest.json") `
    -Destination (Join-Path $output "payload-manifest.json") `
    -Force
Copy-Item -LiteralPath (Join-Path $payload "devflow-feed\devflow-manifest.json") `
    -Destination (Join-Path $output "devflow-manifest.json") `
    -Force

$comparisonPath = Join-Path $output "comparison-summary.json"
& dotnet $RunnerPath compare `
    --request (Join-Path $output "request.json") `
    --registry (Join-Path $output "scenarios.json") `
    --payload-manifest (Join-Path $payload "payload-manifest.json") `
    --runs-root $output `
    --output $comparisonPath
if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $comparisonPath)) {
    throw "UI evidence comparison failed with exit code $LASTEXITCODE."
}

& (Join-Path $PSScriptRoot "Seal-UiEvidence.ps1") `
    -Root $output `
    -RequestManifestPath (Join-Path $output "request.json")
if ($LASTEXITCODE -ne 0) {
    throw "UI evidence sealing failed with exit code $LASTEXITCODE."
}
