#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [Parameter(Mandatory)][string]$InstallDirectory,
    [Parameter(Mandatory)][string]$InputDirectory,
    [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'IssueReplicate.Core.ps1')
if (Test-Path -LiteralPath $InstallDirectory) {
    throw 'The author sample SDK must be installed in a new directory, separate from verification.'
}
$sdkVersion = (Get-Content -Raw -LiteralPath (Join-Path $RepoRoot 'global.json') | ConvertFrom-Json).tools.dotnet
$manifest = Get-Content -Raw -LiteralPath (Join-Path $InputDirectory 'manifest.json') | ConvertFrom-Json
$zip = Join-Path $InputDirectory 'sample.zip'
if ($manifest.schemaVersion -ne 1 -or $manifest.platform -cne $Platform -or
    (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant() -cne $manifest.sampleSha256) {
    throw 'Sample SDK provisioning requires the unchanged platform-bound author archive.'
}
$inspection = Join-Path ([IO.Path]::GetTempPath()) "issue-sample-sdk-$([guid]::NewGuid().ToString('N'))"
try {
    Assert-IssueReplicateZip -Path $zip -ExtractTo $inspection
    $selection = Get-IssueReplicateSampleProject -Directory $inspection -Platform $Platform
    $directory = $selection.Project.Directory
    $root = [IO.Path]::GetFullPath($inspection)
    while ($directory -and ($directory.FullName -ceq $root -or
        $directory.FullName.StartsWith($root + [IO.Path]::DirectorySeparatorChar, [StringComparison]::Ordinal))) {
        $globalPath = Join-Path $directory.FullName 'global.json'
        if (Test-Path -LiteralPath $globalPath -PathType Leaf) {
            $global = Get-Item -LiteralPath $globalPath
            if ($global.Length -gt 8000) { throw 'The author SDK declaration exceeds the input bound.' }
            $authorSdk = (Get-Content -Raw -LiteralPath $globalPath | ConvertFrom-Json).sdk.version
            if ($authorSdk) { $sdkVersion = $authorSdk }
            break
        }
        $directory = $directory.Parent
    }
    $tfmVersion = [version]([regex]::Match($selection.TargetFramework, '^net([0-9]+\.[0-9]+)-').Groups[1].Value)
    if ($sdkVersion -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[A-Za-z0-9.-]+)?$' -or
        [version]($sdkVersion.Split('-')[0]) -lt $tfmVersion) {
        throw "The pinned SDK cannot build the unchanged author target $($selection.TargetFramework); select a matching netN.0 branch."
    }
    Write-Host "Unchanged author target $($selection.TargetFramework); isolated SDK distribution $sdkVersion."
} finally {
    if (Test-Path -LiteralPath $inspection -PathType Container) {
        Remove-Item -LiteralPath $inspection -Recurse -Force
    }
}
if ($sdkVersion -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[A-Za-z0-9.-]+)?$') {
    throw 'The pinned public SDK distribution version is invalid.'
}
$installers = @('temp/dotnet-install.sh', 'bin/dotnet-install.sh' | ForEach-Object {
    Join-Path $RepoRoot $_
} | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })
if ($installers.Count -ne 1) {
    throw 'Pinned repository SDK provisioning must complete before sample SDK installation.'
}
$installer = $installers[0]

# Repository provisioning removes the stock MAUI manifests to build MAUI from source.
$feed = if ($sdkVersion -match '-') { @('--azure-feed', 'https://ci.dot.net/public') } else { @() }
& bash $installer --version $sdkVersion --install-dir $InstallDirectory --no-path @feed
if ($LASTEXITCODE -ne 0) { throw 'Clean public sample SDK installation failed.' }
$dotnet = Join-Path $InstallDirectory 'dotnet'
Push-Location $InstallDirectory
try {
    & $dotnet workload install "maui-$Platform" --skip-manifest-update `
        --configfile (Join-Path $RepoRoot 'NuGet.config') --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw 'Public MAUI sample workload installation failed.' }
} finally { Pop-Location }
if ($Platform -eq 'ios') {
    $iosPacks = @(Get-ChildItem -LiteralPath (Join-Path $InstallDirectory 'packs') -Directory |
        Where-Object { $_.Name -cmatch "^Microsoft\.iOS\.Sdk\.net$($tfmVersion.Major)\.$($tfmVersion.Minor)_([0-9]+\.[0-9]+)$" })
    if ($iosPacks.Count -ne 1) { throw 'The isolated author SDK must declare exactly one matching iOS SDK pack.' }
    $packVersions = @(Get-ChildItem -LiteralPath $iosPacks[0].FullName -Directory)
    if ($packVersions.Count -ne 1) { throw 'The isolated author SDK must contain exactly one matching installed iOS pack version.' }
    $iosVersion = [regex]::Match($iosPacks[0].Name, '_([0-9]+\.[0-9]+)$').Groups[1].Value
    & (Join-Path $PSScriptRoot 'IssueReplicate.ProvisionRuntime.ps1') `
        -RepoRoot $RepoRoot -SdkVersion $iosVersion -SdkPackDirectory $packVersions[0].FullName
}
exit 0
