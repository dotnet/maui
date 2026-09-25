#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$Root,

    [Parameter(Mandatory = $false)]
    [string]$ExpectedRequestKey,

    [Parameter(Mandatory = $false)]
    [string]$ExpectedHeadCommitSha,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

try {
    $resolvedRoot = Assert-UiEvidenceLocalPath $Root
    $sealPath = Resolve-UiEvidenceChildPath $resolvedRoot "evidence-seal.json"
    $seal = Read-UiEvidenceJson $sealPath

    if ($seal.schemaVersion -ne 1 -or $seal.sealed -ne $true -or $seal.algorithm -ne "sha256") {
        throw "Evidence seal metadata is invalid."
    }
    if ($ExpectedRequestKey -and [string]$seal.requestKey -ne $ExpectedRequestKey) {
        throw "Evidence request key does not match the expected request."
    }
    if ($ExpectedHeadCommitSha) {
        Assert-UiEvidenceSha $ExpectedHeadCommitSha "ExpectedHeadCommitSha"
        if ([string]$seal.headCommitSha -ne $ExpectedHeadCommitSha.ToLowerInvariant()) {
            throw "Evidence head commit does not match the expected head."
        }
    }

    $sealedPaths = @{}
    $inventoryLines = New-Object System.Collections.Generic.List[string]
    $totalBytes = [long]0
    foreach ($entry in @($seal.files)) {
        $relativePath = Normalize-UiEvidenceRepositoryPath ([string]$entry.relativePath)
        if ($sealedPaths.ContainsKey($relativePath.ToLowerInvariant())) {
            throw "Evidence seal contains duplicate path '$relativePath'."
        }
        $sealedPaths[$relativePath.ToLowerInvariant()] = $true

        $fullPath = Resolve-UiEvidenceChildPath $resolvedRoot $relativePath
        if (-not (Test-Path -LiteralPath $fullPath -PathType Leaf)) {
            throw "Sealed evidence file is missing: $relativePath"
        }

        $file = Get-Item -LiteralPath $fullPath -Force
        if (($file.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
            throw "Sealed evidence file is a reparse point: $relativePath"
        }
        if ([long]$entry.sizeBytes -ne $file.Length) {
            throw "Evidence file size changed: $relativePath"
        }
        $actualHash = Get-UiEvidenceSha256 $fullPath
        if ([string]$entry.sha256 -ne $actualHash) {
            throw "Evidence file hash changed: $relativePath"
        }

        $totalBytes += $file.Length
        $inventoryLines.Add("${relativePath}:$($file.Length):$actualHash")
    }

    foreach ($file in @(Get-UiEvidenceFiles $resolvedRoot)) {
        if ($file.FullName -eq $sealPath) {
            continue
        }
        $relativePath = Get-UiEvidenceRelativePath $resolvedRoot $file.FullName
        if (-not $sealedPaths.ContainsKey($relativePath.ToLowerInvariant())) {
            throw "Evidence bundle contains an unsealed file: $relativePath"
        }
    }

    if ($totalBytes -ne [long]$seal.totalBytes) {
        throw "Evidence total size does not match the seal."
    }
    $actualSealDigest = Get-UiEvidenceStringSha256 (@($inventoryLines) -join "`n")
    if ([string]$seal.sealDigest -ne $actualSealDigest) {
        throw "Evidence inventory digest does not match the seal."
    }

    $result = [PSCustomObject][ordered]@{
        schemaVersion = 1
        valid = $true
        requestKey = [string]$seal.requestKey
        headCommitSha = [string]$seal.headCommitSha
        fileCount = @($seal.files).Count
        totalBytes = $totalBytes
        errors = @()
    }
    if ($OutputPath) {
        Write-UiEvidenceJson $result $OutputPath
    }
    else {
        $result | ConvertTo-Json -Depth 8
    }
    exit 0
}
catch {
    $result = [PSCustomObject][ordered]@{
        schemaVersion = 1
        valid = $false
        requestKey = $null
        headCommitSha = $null
        fileCount = 0
        totalBytes = 0
        errors = @($_.Exception.Message)
    }
    if ($OutputPath) {
        Write-UiEvidenceJson $result $OutputPath
    }
    else {
        $result | ConvertTo-Json -Depth 8
    }
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 2
}
