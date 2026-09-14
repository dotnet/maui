#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$PayloadRoot,

    [Parameter(Mandatory = $true)]
    [string]$RunnerPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $false)]
    [string]$LogDirectory = (Join-Path ([IO.Path]::GetTempPath()) "maui-ui-evidence-runner-logs"),

    [Parameter(Mandatory = $false)]
    [string]$AppiumUrl = "http://127.0.0.1:4723/wd/hub",

    [Parameter(Mandatory = $false)]
    [string]$DeviceId,

    [Parameter(Mandatory = $false)]
    [switch]$Headless,

    [Parameter(Mandatory = $false)]
    [switch]$DevFlow,

    [Parameter(Mandatory = $false)]
    [switch]$CaptureOnly
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

$payload = (Resolve-Path -LiteralPath $PayloadRoot).Path
$manifest = Read-UiEvidenceJson (Join-Path $payload "payload-manifest.json")
$requestPath = Join-Path $payload "request.json"
$request = Read-UiEvidenceJson $requestPath
$registryPath = Join-Path $payload "scenarios.json"
$output = [IO.Path]::GetFullPath($OutputDirectory)
$logs = [IO.Path]::GetFullPath($LogDirectory)
Remove-Item -LiteralPath $output -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $logs -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $output, $logs | Out-Null
Copy-Item -LiteralPath $requestPath -Destination (Join-Path $output "request.json")
Copy-Item -LiteralPath $registryPath -Destination (Join-Path $output "scenarios.json")
Copy-Item -LiteralPath (Join-Path $payload "payload-manifest.json") -Destination (Join-Path $output "payload-manifest.json")
Copy-Item -LiteralPath (Join-Path $payload "devflow-feed\devflow-manifest.json") -Destination (Join-Path $output "devflow-manifest.json")

function Assert-VariantPayload {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("base", "head")]
        [string]$Variant
    )

    $root = Join-Path $payload "$Variant\app"
    $entries = if ($Variant -eq "base") { @($manifest.baseFiles) } else { @($manifest.headFiles) }
    $expectedDigest = if ($Variant -eq "base") {
        [string]$manifest.baseAppDirectorySha256
    }
    else {
        [string]$manifest.headAppDirectorySha256
    }
    $sealed = @{}
    $inventory = @()
    foreach ($entry in $entries) {
        $relativePath = Normalize-UiEvidenceRepositoryPath ([string]$entry.relativePath)
        $key = $relativePath.ToLowerInvariant()
        if ($sealed.ContainsKey($key)) {
            throw "$Variant payload contains duplicate file '$relativePath'."
        }
        $sealed[$key] = $true
        $path = [IO.Path]::GetFullPath((Join-Path $root $relativePath))
        if (-not $path.StartsWith(
            $root.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar,
            [StringComparison]::OrdinalIgnoreCase)) {
            throw "$Variant payload file escapes the app root."
        }
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "$Variant payload file is missing: $relativePath"
        }
        $file = Get-Item -LiteralPath $path
        $actualHash = Get-UiEvidenceSha256 $path
        if ([long]$entry.sizeBytes -ne $file.Length -or [string]$entry.sha256 -ne $actualHash) {
            throw "$Variant payload file changed: $relativePath"
        }
        $inventory += "${relativePath}:$($file.Length):$actualHash"
    }
    foreach ($file in @(Get-ChildItem -LiteralPath $root -File -Recurse)) {
        $relativePath = Get-UiEvidenceRelativePath $root $file.FullName
        if (-not $sealed.ContainsKey($relativePath.ToLowerInvariant())) {
            throw "$Variant payload contains an unsealed file: $relativePath"
        }
    }
    if ((Get-UiEvidenceStringSha256 ($inventory -join "`n")) -ne $expectedDigest) {
        throw "$Variant payload directory digest is invalid."
    }
}

$plan = @(
    [PSCustomObject]@{ Directory = "base-run1"; Variant = "base"; VariantRun = 1; Sequence = 1; Commit = [string]$request.baseCommitSha; App = [string]$manifest.baseAppRelativePath },
    [PSCustomObject]@{ Directory = "head-run1"; Variant = "head"; VariantRun = 1; Sequence = 2; Commit = [string]$request.headCommitSha; App = [string]$manifest.headAppRelativePath },
    [PSCustomObject]@{ Directory = "head-run2"; Variant = "head"; VariantRun = 2; Sequence = 3; Commit = [string]$request.headCommitSha; App = [string]$manifest.headAppRelativePath },
    [PSCustomObject]@{ Directory = "base-run2"; Variant = "base"; VariantRun = 2; Sequence = 4; Commit = [string]$request.baseCommitSha; App = [string]$manifest.baseAppRelativePath }
)
Write-UiEvidenceJson $plan (Join-Path $output "run-plan.json")

foreach ($run in $plan) {
    Assert-VariantPayload $run.Variant
    $runOutput = Join-Path $output $run.Directory
    New-Item -ItemType Directory -Force -Path $runOutput | Out-Null
    $arguments = @(
        $RunnerPath,
        "run",
        "--platform", [string]$request.platform,
        "--app", (Join-Path $payload $run.App),
        "--scenario", [string]$request.scenarioId,
        "--registry", $registryPath,
        "--output", $runOutput,
        "--variant", $run.Variant,
        "--run-ordinal", "$($run.VariantRun)",
        "--sequence-ordinal", "$($run.Sequence)",
        "--commit-sha", $run.Commit,
        "--harness-sha", [string]$request.harnessSha,
        "--request-key", [string]$request.requestKey,
        "--appium-url", $AppiumUrl
    )
    if ($DeviceId) {
        $arguments += @("--device-id", $DeviceId)
    }
    if ($Headless) {
        $arguments += "--headless"
    }
    if ($DevFlow) {
        $arguments += @("--devflow", "--devflow-port", "9223")
    }

    $logPath = Join-Path $logs "$($run.Directory).log"
    & dotnet @arguments *> $logPath
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $runOutput "run-result.json"))) {
        throw "UI evidence runner failed for $($run.Directory) with exit code $exitCode."
    }
    Assert-VariantPayload $run.Variant
}

if ($CaptureOnly) {
    Write-Host "Captured four UI evidence runs; compare them separately with trusted comparison tooling."
    exit 0
}

$comparisonPath = Join-Path $output "comparison-summary.json"
& dotnet $RunnerPath compare `
    --request (Join-Path $output "request.json") `
    --registry (Join-Path $output "scenarios.json") `
    --payload-manifest (Join-Path $output "payload-manifest.json") `
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
