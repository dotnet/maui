#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$SourceDirectory,

    [Parameter(Mandatory = $true)]
    [string]$ExpectedCommit
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

Assert-UiEvidenceSha $ExpectedCommit "ExpectedCommit"
$root = (Resolve-Path -LiteralPath $SourceDirectory).Path
$manifestPath = Join-Path $root "devflow-source-manifest.json"
$manifest = Read-UiEvidenceJson $manifestPath
if ($manifest.schemaVersion -ne 1 -or
    [string]$manifest.commit -ne $ExpectedCommit.ToLowerInvariant() -or
    [string]$manifest.compatibility.dependencyBaseline -ne "dotnet-maui" -or
    $manifest.compatibility.windowsNativeUiAutomationDisabled -ne $true -or
    $manifest.compatibility.sourceMauiProjectReferencesEnabled -ne $true) {
    throw "DevFlow source manifest is invalid."
}

$sealed = @{}
foreach ($entry in @($manifest.files)) {
    $relativePath = Normalize-UiEvidenceRepositoryPath ([string]$entry.relativePath)
    $key = $relativePath.ToLowerInvariant()
    if ($sealed.ContainsKey($key)) {
        throw "DevFlow source manifest contains duplicate path '$relativePath'."
    }
    $sealed[$key] = $true

    $path = [IO.Path]::GetFullPath((Join-Path $root $relativePath))
    if (-not $path.StartsWith(
        $root.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
        throw "DevFlow source path escapes the source root."
    }
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "DevFlow source file is missing: $relativePath"
    }
    $file = Get-Item -LiteralPath $path
    if (($file.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw "DevFlow source contains a reparse point: $relativePath"
    }
    if ([long]$entry.sizeBytes -ne $file.Length -or [string]$entry.sha256 -ne (Get-UiEvidenceSha256 $path)) {
        throw "DevFlow source file changed: $relativePath"
    }
}

foreach ($file in @(Get-ChildItem -LiteralPath $root -File -Recurse)) {
    if ($file.FullName -eq $manifestPath) {
        continue
    }
    $relativePath = Get-UiEvidenceRelativePath $root $file.FullName
    if (-not $sealed.ContainsKey($relativePath.ToLowerInvariant())) {
        throw "DevFlow source contains an unsealed file: $relativePath"
    }
}

foreach ($requiredPath in @(
    "src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core/Microsoft.Maui.DevFlow.Agent.Core.csproj",
    "src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core/DevFlowAgentService.cs",
    "src/DevFlow/Microsoft.Maui.DevFlow.Agent.Core/LayoutDiagnostics/VisualTreeWalker.LayoutDiagnostics.cs"
)) {
    if (-not $sealed.ContainsKey($requiredPath.ToLowerInvariant())) {
        throw "DevFlow source is missing required file '$requiredPath'."
    }
}

Write-Host "DevFlow source validation passed."
