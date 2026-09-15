#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,

    [Parameter(Mandatory = $true)]
    [string]$RequestPath,

    [Parameter(Mandatory = $true)]
    [ValidateSet("base", "head")]
    [string]$Variant,

    [Parameter(Mandatory = $true)]
    [string]$DevFlowManifestPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

$root = (Resolve-Path -LiteralPath $ArtifactRoot).Path
$request = Read-UiEvidenceJson $RequestPath
$devFlow = Read-UiEvidenceJson $DevFlowManifestPath
$expectedCommit = if ($Variant -eq "base") {
    [string]$request.baseCommitSha
}
else {
    [string]$request.headCommitSha
}

if ($request.platform -eq "android") {
    $candidates = @(
        Get-ChildItem -LiteralPath $root -File -Recurse -Filter "*-Signed.apk" |
            Where-Object { $_.FullName -match 'Controls\.TestCases\.HostApp' }
    )
    if ($candidates.Count -eq 0) {
        $candidates = @(
            Get-ChildItem -LiteralPath $root -File -Recurse -Filter "*.apk" |
                Where-Object { $_.FullName -match 'Controls\.TestCases\.HostApp' }
        )
    }
}
elseif ($request.platform -eq "windows") {
    $candidates = @(
        Get-ChildItem -LiteralPath $root -File -Recurse -Filter "Controls.TestCases.HostApp.exe" |
            Where-Object { $_.FullName -match '[\\/]win-x64[\\/]' }
    )
    $publishedCandidates = @($candidates | Where-Object { $_.FullName -match '[\\/]publish[\\/]' })
    if ($publishedCandidates.Count -eq 1) {
        $candidates = $publishedCandidates
    }
}
else {
    throw "Unsupported UI evidence platform '$($request.platform)'."
}

if ($candidates.Count -ne 1) {
    $found = if ($candidates.Count -eq 0) { "<none>" } else { $candidates.FullName -join [Environment]::NewLine }
    throw "Expected exactly one $Variant UI evidence app, found $($candidates.Count):$([Environment]::NewLine)$found"
}

$app = $candidates[0]
$appRoot = Get-Item -LiteralPath $app.DirectoryName
if (($appRoot.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
    throw "UI evidence app root cannot be a reparse point."
}
$appFiles = @(
    Get-ChildItem -LiteralPath $app.DirectoryName -File -Recurse |
        Sort-Object FullName |
        ForEach-Object {
            if (($_.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "UI evidence app cannot contain a reparse point: $($_.FullName)"
            }
            [PSCustomObject][ordered]@{
                relativePath = Get-UiEvidenceRelativePath $app.DirectoryName $_.FullName
                sizeBytes = [long]$_.Length
                sha256 = Get-UiEvidenceSha256 $_.FullName
            }
        }
)
$appDirectoryDigest = Get-UiEvidenceStringSha256 (@(
    $appFiles | ForEach-Object {
        "$($_.relativePath):$($_.sizeBytes):$($_.sha256)"
    }
) -join "`n")
$metadata = [PSCustomObject][ordered]@{
    schemaVersion = 1
    requestKey = [string]$request.requestKey
    repository = [string]$request.repository
    pullRequestNumber = [int]$request.pullRequestNumber
    variant = $Variant
    platform = [string]$request.platform
    scenarioId = [string]$request.scenarioId
    commitSha = $expectedCommit
    harnessSha = [string]$request.harnessSha
    registrySha256 = [string]$request.registrySha256
    runtimeVariant = "mono"
    sdkVersion = (& dotnet --version).Trim()
    devFlowCommit = [string]$devFlow.commit
    devFlowPackageVersion = [string]$devFlow.packageVersion
    appRelativePath = Get-UiEvidenceRelativePath $root $app.FullName
    appRootRelativePath = Get-UiEvidenceRelativePath $root $app.DirectoryName
    appSizeBytes = [long]$app.Length
    appSha256 = Get-UiEvidenceSha256 $app.FullName
    appDirectorySha256 = $appDirectoryDigest
    appFiles = $appFiles
}
Write-UiEvidenceJson $metadata $OutputPath
