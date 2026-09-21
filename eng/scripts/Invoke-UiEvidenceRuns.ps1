#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$PayloadRoot,

    [Parameter(Mandatory = $true)]
    [string]$RunnerPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $false)]
    [string]$LogDirectory = (Join-Path ([IO.Path]::GetTempPath()) ("maui-ui-evidence-runner-logs-" + [Guid]::NewGuid().ToString("N"))),

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

$payload = Assert-UiEvidenceLocalPath $PayloadRoot
Get-UiEvidenceFiles $payload | Out-Null
$manifest = Read-UiEvidenceJson (Join-Path $payload "payload-manifest.json")
$requestPath = Join-Path $payload "request.json"
$request = Read-UiEvidenceJson $requestPath
$registryPath = Join-Path $payload "scenarios.json"
if ($request.platform -eq "android" -and [string]::IsNullOrWhiteSpace($DeviceId)) {
    throw "Android UI evidence requires an explicit DeviceId."
}
if ($manifest.schemaVersion -ne 1 -or $manifest.requestKey -cne $request.requestKey) {
    throw "UI evidence payload does not match the request."
}
if ((Get-UiEvidenceSha256 $registryPath) -cne [string]$request.registrySha256) {
    throw "UI evidence registry hash does not match the request."
}
$output = New-UiEvidenceOutputDirectory $OutputDirectory @($payload)
$logs = New-UiEvidenceOutputDirectory $LogDirectory @($payload, $output)
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
        $path = Resolve-UiEvidenceChildPath $root $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "$Variant payload file is missing: $relativePath"
        }
        $file = Get-Item -LiteralPath $path -Force
        $actualHash = Get-UiEvidenceSha256 $path
        if ([long]$entry.sizeBytes -ne $file.Length -or [string]$entry.sha256 -ne $actualHash) {
            throw "$Variant payload file changed: $relativePath"
        }
        $inventory += "${relativePath}:$($file.Length):$actualHash"
    }
    foreach ($file in @(Get-UiEvidenceFiles $root)) {
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
    $appPath = Resolve-UiEvidenceChildPath $payload $run.App
    $variantRoot = Join-Path $payload "$($run.Variant)\app"
    Get-UiEvidenceRelativePath $variantRoot $appPath | Out-Null
    $runOutput = Join-Path $output $run.Directory
    New-Item -ItemType Directory -Force -Path $runOutput | Out-Null
    $arguments = @(
        $RunnerPath,
        "run",
        "--platform", [string]$request.platform,
        "--app", $appPath,
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
