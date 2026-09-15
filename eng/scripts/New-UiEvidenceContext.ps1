#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$ChangedFilesPath,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HarnessSha,

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $false)]
    [string]$RegistryPath = (Join-Path $PSScriptRoot "..\ui-evidence\scenarios.json"),

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

Assert-UiEvidenceSha $BaseCommitSha "BaseCommitSha"
Assert-UiEvidenceSha $HeadCommitSha "HeadCommitSha"
Assert-UiEvidenceSha $HarnessSha "HarnessSha"
foreach ($inputPath in @($ChangedFilesPath, $RegistryPath)) {
    if (-not (Test-Path -LiteralPath $inputPath -PathType Leaf)) {
        throw "UI evidence input file does not exist: $inputPath"
    }
}

$output = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path -LiteralPath $output) {
    $item = Get-Item -LiteralPath $output -Force
    if (-not $item.PSIsContainer -or
        ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or
        @(Get-ChildItem -LiteralPath $output -Force).Count -ne 0) {
        throw "OutputDirectory must be a new or empty, non-linked directory."
    }
}
New-Item -ItemType Directory -Path $output -Force | Out-Null

$selectionPath = Join-Path $output "selection.json"
$requestsPath = Join-Path $output "requests.json"
$registryCopy = Join-Path $output "scenarios.json"
Copy-Item -LiteralPath $RegistryPath -Destination $registryCopy

& pwsh -NoProfile -File (Join-Path $PSScriptRoot "Select-UiEvidenceScenarios.ps1") `
    -ChangedFilesPath $ChangedFilesPath `
    -BaseCommitSha $BaseCommitSha `
    -HeadCommitSha $HeadCommitSha `
    -HarnessSha $HarnessSha `
    -Repository $Repository `
    -PullRequestNumber $PullRequestNumber `
    -RegistryPath $registryCopy `
    -OutputPath $selectionPath
$selectionExit = $LASTEXITCODE
if ($selectionExit -notin @(0, 3)) {
    throw "UI evidence selection failed with exit code $selectionExit."
}

& pwsh -NoProfile -File (Join-Path $PSScriptRoot "New-UiEvidenceRequests.ps1") `
    -SelectionPath $selectionPath `
    -OutputPath $requestsPath
if ($LASTEXITCODE -ne $selectionExit) {
    throw "UI evidence request generation failed with exit code $LASTEXITCODE."
}

$selection = Read-UiEvidenceJson $selectionPath
$requests = @(Read-UiEvidenceJson $requestsPath)
$selection.requests = $requests
Write-UiEvidenceJson $selection $selectionPath

Write-Host "Prepared $($requests.Count) local request(s): $($selection.selectionStatus). No apps were built or run."
exit $selectionExit
