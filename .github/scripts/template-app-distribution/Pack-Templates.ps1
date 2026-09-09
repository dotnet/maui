#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$RepositoryPath,

    [Parameter(Mandatory)]
    [ValidatePattern('^[a-z0-9.-]+$')]
    [string]$VersionSuffix,

    [Parameter(Mandatory)]
    [ValidatePattern('^[0-9a-f]{40}$')]
    [string]$SourceSha,

    [Parameter(Mandatory)]
    [ValidateSet('android', 'ios', 'maccatalyst', 'windows')]
    [string]$Platform,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [Parameter(Mandatory)]
    [string]$DotNetCliHome,

    [Parameter(Mandatory)]
    [string]$NuGetPackages
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/Source-Packages.ps1"

$actualSha = (& git -C $RepositoryPath rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $actualSha -ne $SourceSha) {
    throw "Source checkout does not match pinned commit $SourceSha."
}
$changes = & git -C $RepositoryPath status --porcelain --untracked-files=no
if ($LASTEXITCODE -ne 0 -or $changes) {
    throw "Source checkout must be unmodified before building packages."
}
[xml]$versions = Get-Content (Join-Path $RepositoryPath 'eng/Versions.props') -Raw
$versionParts = foreach ($name in @('MajorVersion', 'MinorVersion', 'PatchVersion')) {
    $value = $versions.SelectSingleNode("/Project/PropertyGroup/$name").InnerText
    if ($value -notmatch '^\d+$') { throw "Invalid source version component '$name'." }
    $value
}
$packageVersion = "$($versionParts -join '.')-$VersionSuffix"

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
New-Item -ItemType Directory -Path $DotNetCliHome -Force | Out-Null
New-Item -ItemType Directory -Path $NuGetPackages -Force | Out-Null

$env:DOTNET_CLI_HOME = $DotNetCliHome
$env:NUGET_PACKAGES = $NuGetPackages

$properties = @(
    '-p:Configuration=Release',
    "-p:PackageVersion=$packageVersion",
    "-p:RepositoryCommit=$SourceSha",
    "-p:SourceRevisionId=$SourceSha",
    '-p:CI=true',
    '-p:GenerateCgManifest=false',
    '-p:SymbolPackageFormat=snupkg',
    '-p:IncludePreviousTfms=false',
    '-p:ValidateXcodeVersion=false'
)
foreach ($entry in @{ Android = 'android'; Ios = 'ios'; MacCatalyst = 'maccatalyst'; MacOS = 'macos'; Windows = 'windows'; Tizen = 'tizen' }.GetEnumerator()) {
    $enabled = ($entry.Value -eq $Platform).ToString().ToLowerInvariant()
    $properties += "-p:Include$($entry.Key)TargetFrameworks=$enabled"
}

Push-Location $RepositoryPath
try {
    Invoke-DistributionDotNet (@('build', 'Microsoft.Maui.BuildTasks.slnf', '-p:BuildTaskOnlyBuild=true') + $properties) 'Source build tasks'
    $solution = if ($IsWindows) { 'eng/Microsoft.Maui.Packages.slnf' } else { 'eng/Microsoft.Maui.Packages-mac.slnf' }
    Invoke-DistributionDotNet (@('msbuild', $solution, '-restore', '-t:Pack', '-maxcpucount:2') + $properties) 'Source framework and template pack'
} finally {
    Pop-Location
}

$shipping = Join-Path $RepositoryPath 'artifacts/packages/Release/Shipping'
$packages = @(Get-SourcePackageFiles $shipping)
if ($packages.Count -eq 0) { throw "No source-built MAUI packages were produced." }
$manifest = [ordered]@{
    sourceSha = $SourceSha
    version = $packageVersion
    platform = $Platform
    packages = @()
}
foreach ($package in $packages) {
    $info = Get-SourcePackageInfo $package.FullName $SourceSha
    if ($info.version -ne $packageVersion) { throw "Mixed package versions in source build: $($info.id)." }
    Copy-Item $package.FullName $OutputPath -Force
    $manifest.packages += $info
}
$manifestPath = Join-Path $OutputPath 'source-packages.json'
$manifest | ConvertTo-Json -Depth 20 | Set-Content $manifestPath -Encoding utf8
$validatedManifest = Read-SourcePackageManifest $manifestPath $SourceSha
$template = $validatedManifest.packages | Where-Object id -Like 'Microsoft.Maui.Templates*'
Write-Host "Source MAUI $packageVersion and templates built from $SourceSha."

if ($env:GITHUB_OUTPUT) {
    "template_package_path=$(Join-Path $OutputPath $template.file)" >> $env:GITHUB_OUTPUT
    "source_manifest_path=$manifestPath" >> $env:GITHUB_OUTPUT
    "source_packages_path=$OutputPath" >> $env:GITHUB_OUTPUT
}
