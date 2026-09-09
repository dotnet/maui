#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Build-TemplateApp.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'Assert-EnvironmentValue',
        'Get-NewestBuildOutput',
        'Repair-AppleAdhocSignature',
        'Invoke-DotNetPublish',
        'Test-IsNet11OrLater',
        'Add-NativeAotArguments',
        'Get-BinlogConfiguration',
        'Invoke-NotaryTool',
        'Invoke-NotaryToolJson',
        'Write-NotarySubmissionDiagnostics',
        'Get-NotarizationTimeoutSeconds',
        'Get-NotaryTimestamp',
        'Wait-NotarySubmission',
        'Invoke-MacNotarization',
        'New-MacCatalystDeveloperIdSideload',
        'New-IosAdHocSideload'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }

    $testRoot = Join-Path $PSScriptRoot '.test-results'
    New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
    $projectPath = Join-Path $testRoot 'TestApp.csproj'
    Set-Content -Path $projectPath -Value '<Project Sdk="Microsoft.NET.Sdk" />'
    $projectFile = Get-Item $projectPath

    $installerScriptPath = Join-Path $PSScriptRoot 'Install-AppleSigningAssets.ps1'
    $installerTokens = $null
    $installerParseErrors = $null
    $installerAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $installerScriptPath,
        [ref]$installerTokens,
        [ref]$installerParseErrors
    )
    if ($installerParseErrors -and $installerParseErrors.Count -gt 0) {
        throw ($installerParseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $pairedEnvironmentFunction = $installerAst.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Assert-PairedEnvironmentValues'
    }, $true)
    if (-not $pairedEnvironmentFunction) {
        throw "Function 'Assert-PairedEnvironmentValues' not found"
    }
    Invoke-Expression $pairedEnvironmentFunction.Extent.Text

    $script:prepareMatrixScriptPath = Join-Path $PSScriptRoot 'Prepare-Matrix.ps1'
    $script:resolveDotNetSdkScriptPath = Join-Path $PSScriptRoot 'Resolve-DotNetSdk.ps1'
    $script:resolveSourceRefScriptPath = Join-Path $PSScriptRoot 'Resolve-SourceRef.ps1'
    $script:newTemplateScriptPath = Join-Path $PSScriptRoot 'New-TemplateApp.ps1'
    $script:sourcePackagesScriptPath = Join-Path $PSScriptRoot 'Source-Packages.ps1'
    $script:fastfilePath = Join-Path $PSScriptRoot 'fastlane/Fastfile'
    $script:workflowPath = Join-Path $PSScriptRoot '../../workflows/template-app-distribution.yml'
    $script:workflowText = Get-Content -Path $script:workflowPath -Raw
    $script:pwshPath = (Get-Command pwsh -ErrorAction Stop).Source
    $script:rubyPath = (Get-Command ruby -ErrorAction SilentlyContinue).Source
    $script:originalPath = $env:PATH
    . $script:sourcePackagesScriptPath
    $sourcePackagesTokens = $null
    $sourcePackagesParseErrors = $null
    $sourcePackagesAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:sourcePackagesScriptPath,
        [ref]$sourcePackagesTokens,
        [ref]$sourcePackagesParseErrors
    )
    if ($sourcePackagesParseErrors -and $sourcePackagesParseErrors.Count -gt 0) {
        throw ($sourcePackagesParseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }
    $payloadProofFunction = $sourcePackagesAst.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-AppPayloadProof'
    }, $true)
    if (-not $payloadProofFunction) {
        throw "Function 'Get-AppPayloadProof' not found"
    }
    $script:payloadProofFunctionText = $payloadProofFunction.Extent.Text
    Invoke-Expression $payloadProofFunction.Extent.Text
    $script:fixtureSourceSha = '0123456789abcdef0123456789abcdef01234567'
    $script:fixtureSourceVersion = '11.0.0-preview.1.25080.1'
    $script:requiredSourcePackageIds = @(
        'Microsoft.Maui.Controls',
        'Microsoft.Maui.Controls.Core',
        'Microsoft.Maui.Controls.Xaml',
        'Microsoft.Maui.Core',
        'Microsoft.Maui.Essentials',
        'Microsoft.Maui.Graphics',
        'Microsoft.Maui.Controls.Build.Tasks',
        'Microsoft.Maui.Resizetizer',
        'Microsoft.Maui.Sdk',
        'Microsoft.Maui.Templates.net10'
    )
    $script:testEnvironmentNames = @(
        'FAKE_DOTNET_MODE',
        'FAKE_MAUI_ASSEMBLY_DIRECTORY',
        'FAKE_CODESIGN_MODE',
        'FAKE_TESTFLIGHT_ERROR',
        'FAKE_TESTFLIGHT_GROUPS',
        'FAKE_SOURCE_ASSETS_MODE',
        'FAKE_SOURCE_MANIFEST_PATH',
        'FAKE_SOURCE_SHA',
        'FASTFILE_PATH',
        'GITHUB_RUN_ID',
        'GITHUB_SHA',
        'GITHUB_OUTPUT',
        'RUNNER_TEMP',
        'TEMPLATE_APP_VARIANTS_JSON',
        'ANDROID_KEYSTORE_PATH',
        'ANDROID_KEYSTORE_PASSWORD',
        'ANDROID_KEY_PASSWORD',
        'ANDROID_KEY_ALIAS',
        'ANDROID_SIGNING_KEY_ALIAS',
        'ANDROID_SIGNING_STORE_PASS',
        'ANDROID_SIGNING_KEY_PASS',
        'FAKE_ANDROID_SIGNING_ENV_LOG',
        'APPLE_DEVELOPERID_CERTIFICATE_BASE64',
        'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64',
        'IOS_ADHOC_CODESIGN_PROVISION',
        'IOS_CODESIGN_KEY',
        'IOS_CODESIGN_PROVISION',
        'APPLE_DEVELOPERID_CODESIGN_KEY',
        'APPLE_DEVELOPERID_CODESIGN_PROVISION',
        'TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS',
        'GH_TOKEN',
        'GITHUB_TOKEN',
        'COPILOT_GITHUB_TOKEN'
    )
    $script:originalEnvironment = @{}
    foreach ($name in $script:testEnvironmentNames) {
        $script:originalEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
    }

    function New-FakeSourcePackage(
        [string]$PackageDirectory,
        [string]$PackageId,
        [string]$Version,
        [string]$RepositoryCommit
    ) {
        New-Item -ItemType Directory -Path $PackageDirectory -Force | Out-Null
        $packagePath = Join-Path $PackageDirectory "$PackageId.$Version.nupkg"
        Remove-Item -Path $packagePath -Force -ErrorAction SilentlyContinue

        $archive = [System.IO.Compression.ZipFile]::Open(
            $packagePath,
            [System.IO.Compression.ZipArchiveMode]::Create
        )
        try {
            $entry = $archive.CreateEntry("$PackageId.nuspec")
            $writer = [System.IO.StreamWriter]::new($entry.Open())
            try {
                @"
<?xml version="1.0" encoding="utf-8"?>
<package>
  <metadata>
    <id>$PackageId</id>
    <version>$Version</version>
    <repository type="git" url="https://github.com/dotnet/maui" commit="$RepositoryCommit" />
  </metadata>
</package>
"@ | ForEach-Object { $writer.Write($_) }
            }
            finally {
                $writer.Dispose()
            }

            if ($PackageId -eq 'Microsoft.Maui.Sdk') {
                foreach ($sdkEntryPath in @('Sdk/Sdk.props', 'Sdk/Sdk.targets')) {
                    $sdkEntry = $archive.CreateEntry($sdkEntryPath)
                    $sdkWriter = [System.IO.StreamWriter]::new($sdkEntry.Open())
                    try {
                        "<Project />" | ForEach-Object { $sdkWriter.Write($_) }
                    }
                    finally {
                        $sdkWriter.Dispose()
                    }
                }
            }
        }
        finally {
            $archive.Dispose()
        }

        return $packagePath
    }

    function Save-SourcePackageManifest($Fixture, $Manifest) {
        $Manifest | ConvertTo-Json -Depth 20 | Set-Content -Path $Fixture.ManifestPath -Encoding utf8
    }

    function Get-SourcePackageManifestObject($Fixture) {
        return Get-Content -Path $Fixture.ManifestPath -Raw | ConvertFrom-Json
    }

    function Get-TemplatePackageEntry($Fixture) {
        return @($Fixture.Packages | Where-Object { $_['id'] -like 'Microsoft.Maui.Templates*' })[0]
    }

    function New-SourcePackageFixture {
        param(
            [string]$SourceSha = $script:fixtureSourceSha,
            [string]$Version = $script:fixtureSourceVersion,
            [string[]]$PackageIds = $script:requiredSourcePackageIds
        )

        $fixtureRoot = Join-Path $testRoot ([guid]::NewGuid().ToString("N"))
        $packageDirectory = Join-Path $fixtureRoot 'packages'
        New-Item -ItemType Directory -Path $packageDirectory -Force | Out-Null

        foreach ($packageId in $PackageIds) {
            New-FakeSourcePackage `
                -PackageDirectory $packageDirectory `
                -PackageId $packageId `
                -Version $Version `
                -RepositoryCommit $SourceSha | Out-Null
        }

        $packages = foreach ($packageId in $PackageIds) {
            $packagePath = Join-Path $packageDirectory "$packageId.$Version.nupkg"
            Get-SourcePackageInfo -Path $packagePath -SourceSha $SourceSha
        }
        $manifest = [ordered]@{
            version = $Version
            sourceSha = $SourceSha
            packages = @($packages)
        }
        $manifestPath = Join-Path $packageDirectory 'source-packages.json'
        $manifest | ConvertTo-Json -Depth 20 | Set-Content -Path $manifestPath -Encoding utf8

        return [pscustomobject]@{
            Root = $fixtureRoot
            PackageDirectory = $packageDirectory
            ManifestPath = $manifestPath
            SourceSha = $SourceSha
            Version = $Version
            TemplatePackagePath = Join-Path $packageDirectory "$((@($PackageIds | Where-Object { $_ -like 'Microsoft.Maui.Templates*' })[0])).$Version.nupkg"
            Packages = @($packages)
            Manifest = $manifest
        }
    }

    function Initialize-FakeMauiPayloadAssemblies {
        param([string]$SourceSha = $script:fixtureSourceSha)

        $assemblyDirectory = Join-Path $testRoot "payload-assemblies-$SourceSha"
        New-Item -ItemType Directory -Path $assemblyDirectory -Force | Out-Null

        foreach ($assemblyName in @('Microsoft.Maui.dll', 'Microsoft.Maui.Controls.dll', 'Microsoft.Maui.Graphics.dll')) {
            $assemblyPath = Join-Path $assemblyDirectory $assemblyName
            if (Test-Path $assemblyPath) {
                continue
            }

            $namespaceSuffix = $SourceSha.Substring(0, 8)
            $typeSuffix = ($assemblyName -replace '[^A-Za-z0-9]', '')
            Add-Type -TypeDefinition @"
using System.Reflection;
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+$SourceSha")]
namespace MauiFixture_$namespaceSuffix {
    public sealed class Marker_$typeSuffix {}
}
"@ -Language CSharp -OutputAssembly $assemblyPath
        }

        return $assemblyDirectory
    }

    function Add-ZipEntryFromFile($Archive, [string]$EntryPath, [string]$SourcePath) {
        $entry = $Archive.CreateEntry($EntryPath.Replace('\', '/'))
        $entryStream = $entry.Open()
        try {
            $fileStream = [System.IO.File]::OpenRead($SourcePath)
            try {
                $fileStream.CopyTo($entryStream)
            }
            finally {
                $fileStream.Dispose()
            }
        }
        finally {
            $entryStream.Dispose()
        }
    }

    function Add-ZipEntryFromText($Archive, [string]$EntryPath, [string]$Content) {
        $entry = $Archive.CreateEntry($EntryPath.Replace('\', '/'))
        $writer = [System.IO.StreamWriter]::new($entry.Open())
        try {
            $writer.Write($Content)
        }
        finally {
            $writer.Dispose()
        }
    }

    function New-FakePayloadArchive(
        [string]$Path,
        $Manifest,
        [string]$SourceSha = $script:fixtureSourceSha,
        [switch]$OmitRequiredAssembly
    ) {
        New-Item -ItemType Directory -Path (Split-Path $Path -Parent) -Force | Out-Null
        Remove-Item -Path $Path -Force -ErrorAction SilentlyContinue

        $archive = [System.IO.Compression.ZipFile]::Open(
            $Path,
            [System.IO.Compression.ZipArchiveMode]::Create
        )
        try {
            $prefix = if ([System.IO.Path]::GetExtension($Path) -ieq '.ipa') { 'Payload/TestApp.app' } else { '' }
            foreach ($assemblyName in @('Microsoft.Maui.dll', 'Microsoft.Maui.Controls.dll', 'Microsoft.Maui.Graphics.dll')) {
                if ($OmitRequiredAssembly -and $assemblyName -eq 'Microsoft.Maui.Graphics.dll') {
                    continue
                }

                $sourcePath = Join-Path $script:fakeMauiAssemblyDirectory $assemblyName
                if ($SourceSha -ne $script:fixtureSourceSha) {
                    $alternateDirectory = Initialize-FakeMauiPayloadAssemblies -SourceSha $SourceSha
                    $sourcePath = Join-Path $alternateDirectory $assemblyName
                }

                $entryPath = if ([string]::IsNullOrWhiteSpace($prefix)) { $assemblyName } else { "$prefix/$assemblyName" }
                Add-ZipEntryFromFile -Archive $archive -EntryPath $entryPath -SourcePath $sourcePath
            }

            $depsLibraries = [ordered]@{}
            foreach ($package in $Manifest.packages) {
                $depsLibraries["$($package.id)/$($package.version)"] = @{}
            }
            $depsJson = [ordered]@{ libraries = $depsLibraries } | ConvertTo-Json -Depth 10
            $depsPath = if ([string]::IsNullOrWhiteSpace($prefix)) { 'TestApp.deps.json' } else { "$prefix/TestApp.deps.json" }
            Add-ZipEntryFromText -Archive $archive -EntryPath $depsPath -Content $depsJson
        }
        finally {
            $archive.Dispose()
        }

        return Get-Item $Path
    }

    function Write-SourcePackageAssetsFile(
        [string]$AssetsPath,
        $Manifest,
        [ValidateSet('match', 'hash-mismatch', 'mixed-stable', 'missing-essential')]
        [string]$Mode = 'match'
    ) {
        $libraries = [ordered]@{}
        foreach ($package in $Manifest.packages) {
            $libraries["$($package.id)/$($package.version)"] = @{
                sha512 = $package.sha512
            }
        }

        switch ($Mode) {
            'hash-mismatch' {
                $libraries["Microsoft.Maui.Controls/$($Manifest.version)"] = @{
                    sha512 = 'hash-mismatch'
                }
            }
            'mixed-stable' {
                $null = $libraries.Remove("Microsoft.Maui.Controls/$($Manifest.version)")
                $libraries['Microsoft.Maui.Controls/8.0.100'] = @{
                    sha512 = ($Manifest.packages | Where-Object id -EQ 'Microsoft.Maui.Controls' | Select-Object -ExpandProperty sha512)
                }
            }
            'missing-essential' {
                $null = $libraries.Remove("Microsoft.Maui.Resizetizer/$($Manifest.version)")
            }
        }

        New-Item -ItemType Directory -Path (Split-Path $AssetsPath -Parent) -Force | Out-Null
        [ordered]@{
            version = 3
            targets = [ordered]@{
                'net11.0' = [ordered]@{}
            }
            libraries = $libraries
        } | ConvertTo-Json -Depth 10 | Set-Content -Path $AssetsPath -Encoding utf8

        return $AssetsPath
    }

    $script:fakeMauiAssemblyDirectory = Initialize-FakeMauiPayloadAssemblies
    $script:buildTemplateAppHarnessPath = Join-Path $testRoot 'Build-TemplateApp.harness.ps1'
    @"
param()
`$ErrorActionPreference = 'Stop'
. '$script:sourcePackagesScriptPath'
if (-not (Get-Command Get-AppPayloadProof -ErrorAction SilentlyContinue)) {
$script:payloadProofFunctionText
}
& '$scriptPath' @args
exit `$LASTEXITCODE
"@ | Set-Content -Path $script:buildTemplateAppHarnessPath -Encoding utf8

    $script:fakeCommandDirectory = Join-Path $testRoot 'fake-commands'
    New-Item -ItemType Directory -Path $script:fakeCommandDirectory -Force | Out-Null
    $fakeDotNetScriptPath = Join-Path $script:fakeCommandDirectory 'fake-dotnet.ps1'
    @'
$ErrorActionPreference = "Stop"

function Get-ArgumentValue([string[]]$Arguments, [string]$Name) {
    for ($index = 0; $index -lt $Arguments.Count - 1; $index++) {
        if ($Arguments[$index] -eq $Name) {
            return $Arguments[$index + 1]
        }
    }

    return $null
}

function Get-FakeSourceManifest {
    if ([string]::IsNullOrWhiteSpace($env:FAKE_SOURCE_MANIFEST_PATH) -or -not (Test-Path $env:FAKE_SOURCE_MANIFEST_PATH)) {
        return $null
    }

    return Get-Content -Path $env:FAKE_SOURCE_MANIFEST_PATH -Raw | ConvertFrom-Json -AsHashtable
}

function Write-FakeProjectAssets([string]$ProjectDirectory, $Manifest) {
    if (-not $Manifest) {
        return
    }

    $libraries = [ordered]@{}
    foreach ($package in $Manifest.packages) {
        $libraries["$($package.id)/$($package.version)"] = @{
            sha512 = $package.sha512
        }
    }

    switch ($env:FAKE_SOURCE_ASSETS_MODE) {
        "hash-mismatch" {
            $key = "Microsoft.Maui.Controls/$($Manifest.version)"
            $libraries[$key] = @{ sha512 = 'hash-mismatch' }
        }
        "mixed-stable" {
            $null = $libraries.Remove("Microsoft.Maui.Controls/$($Manifest.version)")
            $libraries["Microsoft.Maui.Controls/8.0.100"] = @{
                sha512 = ($Manifest.packages | Where-Object { $_.id -eq 'Microsoft.Maui.Controls' } | Select-Object -ExpandProperty sha512)
            }
        }
        "missing-essential" {
            $null = $libraries.Remove("Microsoft.Maui.Resizetizer/$($Manifest.version)")
        }
    }

    $assets = [ordered]@{
        version = 3
        targets = [ordered]@{
            'net11.0' = [ordered]@{}
        }
        libraries = $libraries
    }

    $assetsPath = Join-Path $ProjectDirectory 'obj/project.assets.json'
    New-Item -ItemType Directory -Path (Split-Path $assetsPath -Parent) -Force | Out-Null
    $assets | ConvertTo-Json -Depth 10 | Set-Content -Path $assetsPath -Encoding utf8
}

function Write-FakeAppBundle([string]$AppPath, $Manifest) {
    New-Item -ItemType Directory -Path $AppPath -Force | Out-Null
    Set-Content -Path (Join-Path $AppPath 'Info.plist') -Value 'fake app' -Encoding utf8
    foreach ($assemblyName in @('Microsoft.Maui.dll', 'Microsoft.Maui.Controls.dll', 'Microsoft.Maui.Graphics.dll')) {
        Copy-Item -Path (Join-Path $env:FAKE_MAUI_ASSEMBLY_DIRECTORY $assemblyName) `
            -Destination (Join-Path $AppPath $assemblyName) -Force
    }

    $depsLibraries = [ordered]@{}
    foreach ($package in $Manifest.packages) {
        $depsLibraries["$($package.id)/$($package.version)"] = @{}
    }
    [ordered]@{
        libraries = $depsLibraries
    } | ConvertTo-Json -Depth 10 | Set-Content -Path (Join-Path $AppPath 'TestApp.deps.json') -Encoding utf8
}

function Add-ArchiveFileEntry($Archive, [string]$EntryPath, [string]$SourcePath) {
    $entry = $Archive.CreateEntry($EntryPath.Replace('\', '/'))
    $targetStream = $entry.Open()
    try {
        $sourceStream = [System.IO.File]::OpenRead($SourcePath)
        try {
            $sourceStream.CopyTo($targetStream)
        }
        finally {
            $sourceStream.Dispose()
        }
    }
    finally {
        $targetStream.Dispose()
    }
}

function Add-ArchiveTextEntry($Archive, [string]$EntryPath, [string]$Content) {
    $entry = $Archive.CreateEntry($EntryPath.Replace('\', '/'))
    $writer = [System.IO.StreamWriter]::new($entry.Open())
    try {
        $writer.Write($Content)
    }
    finally {
        $writer.Dispose()
    }
}

function Write-FakeArchive([string]$ArchivePath, [string]$Prefix, $Manifest) {
    New-Item -ItemType Directory -Path (Split-Path $ArchivePath -Parent) -Force | Out-Null
    Remove-Item -Path $ArchivePath -Force -ErrorAction SilentlyContinue
    $archive = [System.IO.Compression.ZipFile]::Open(
        $ArchivePath,
        [System.IO.Compression.ZipArchiveMode]::Create
    )
    try {
        foreach ($assemblyName in @('Microsoft.Maui.dll', 'Microsoft.Maui.Controls.dll', 'Microsoft.Maui.Graphics.dll')) {
            $entryPath = if ([string]::IsNullOrWhiteSpace($Prefix)) { $assemblyName } else { "$Prefix/$assemblyName" }
            Add-ArchiveFileEntry -Archive $archive -EntryPath $entryPath `
                -SourcePath (Join-Path $env:FAKE_MAUI_ASSEMBLY_DIRECTORY $assemblyName)
        }

        $depsLibraries = [ordered]@{}
        foreach ($package in $Manifest.packages) {
            $depsLibraries["$($package.id)/$($package.version)"] = @{}
        }
        $depsPath = if ([string]::IsNullOrWhiteSpace($Prefix)) { 'TestApp.deps.json' } else { "$Prefix/TestApp.deps.json" }
        Add-ArchiveTextEntry -Archive $archive -EntryPath $depsPath -Content (
            ([ordered]@{ libraries = $depsLibraries } | ConvertTo-Json -Depth 10)
        )
    }
    finally {
        $archive.Dispose()
    }
}

$outputPath = $null
$runtimeIdentifier = $null
$binlogPaths = @()
$projectPath = $args | Where-Object { $_ -like "*.csproj" } | Select-Object -First 1
$sourceManifest = Get-FakeSourceManifest
for ($index = 0; $index -lt $args.Count; $index++) {
    if ($args[$index] -eq "-o" -and $index + 1 -lt $args.Count) {
        $outputPath = $args[$index + 1]
    }
    if ($args[$index] -eq "-r" -and $index + 1 -lt $args.Count) {
        $runtimeIdentifier = $args[$index + 1]
    }
    if ($args[$index].StartsWith("/bl:")) {
        $binlogPath = $args[$index].Substring(4)
        New-Item -ItemType Directory -Path (Split-Path $binlogPath -Parent) -Force | Out-Null
        $binlogPaths += $binlogPath
    }
}
$argumentText = $args -join "`n"
foreach ($binlogPath in $binlogPaths) {
    Set-Content -Path $binlogPath -Value $argumentText
}
if (-not [string]::IsNullOrWhiteSpace($env:FAKE_ANDROID_SIGNING_ENV_LOG)) {
    @(
        "alias=$env:ANDROID_SIGNING_KEY_ALIAS",
        "storePass=$env:ANDROID_SIGNING_STORE_PASS",
        "keyPass=$env:ANDROID_SIGNING_KEY_PASS"
    ) -join "`n" | Add-Content -Path $env:FAKE_ANDROID_SIGNING_ENV_LOG
}

if ($sourceManifest -and $projectPath -and $args[0] -in @('build', 'publish')) {
    Write-FakeProjectAssets -ProjectDirectory (Split-Path $projectPath -Parent) -Manifest $sourceManifest
}

if ($args[0] -eq 'new') {
    if ($args.Count -ge 2 -and $args[1] -eq 'install') {
        exit 0
    }

    $projectName = Get-ArgumentValue -Arguments $args -Name '-n'
    $projectDirectory = Get-ArgumentValue -Arguments $args -Name '-o'
    if ([string]::IsNullOrWhiteSpace($projectDirectory)) {
        throw "fake dotnet new requires -o"
    }
    if ([string]::IsNullOrWhiteSpace($projectName)) {
        $projectName = 'TestApp'
    }

    $projectFilePath = Join-Path $projectDirectory "$projectName.csproj"
    New-Item -ItemType Directory -Path $projectDirectory, (Join-Path $projectDirectory 'Platforms/iOS'), (Join-Path $projectDirectory 'Platforms/MacCatalyst') -Force | Out-Null
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net11.0-android;net11.0-ios</TargetFrameworks>
    <ApplicationTitle>Template Title</ApplicationTitle>
    <ApplicationId>com.example.template</ApplicationId>
    <ApplicationDisplayVersion>0.1</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Maui.Controls" Version="8.0.0" />
    <PackageReference Include="Microsoft.Maui.Graphics">
      <Version>8.0.0</Version>
    </PackageReference>
  </ItemGroup>
</Project>
"@ | Set-Content -Path $projectFilePath -Encoding utf8
    '<plist><dict><key>ITSAppUsesNonExemptEncryption</key><true/></dict></plist>' |
        Set-Content -Path (Join-Path $projectDirectory 'Platforms/iOS/Info.plist') -Encoding utf8
    '<plist><dict><key>ITSAppUsesNonExemptEncryption</key><true/></dict></plist>' |
        Set-Content -Path (Join-Path $projectDirectory 'Platforms/MacCatalyst/Info.plist') -Encoding utf8
    '<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui" />' |
        Set-Content -Path (Join-Path $projectDirectory 'MainPage.xaml') -Encoding utf8
    exit 0
}

switch ($env:FAKE_DOTNET_MODE) {
    "android-success" {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        if ($argumentText -match "AndroidPackageFormat=apk") {
            Write-FakeArchive -ArchivePath (Join-Path $outputPath "TestApp-Signed.apk") -Prefix '' -Manifest $sourceManifest
        } elseif ($argumentText -match "AndroidPackageFormat=aab") {
            Write-FakeArchive -ArchivePath (Join-Path $outputPath "TestApp.aab") -Prefix 'base/root' -Manifest $sourceManifest
        }
    }
    "ios-device-only" {
        if ($runtimeIdentifier -eq "ios-arm64") {
            $projectDirectory = Split-Path $projectPath -Parent
            $appPath = Join-Path $projectDirectory "bin/$runtimeIdentifier/TestApp.app"
            Write-FakeAppBundle -AppPath $appPath -Manifest $sourceManifest
        }
    }
    "ios-publish-success" {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        Write-FakeArchive -ArchivePath (Join-Path $outputPath "TestApp.ipa") -Prefix 'Payload/TestApp.app' -Manifest $sourceManifest
    }
    "ios-store-overwrite" {
        $projectDirectory = Split-Path $projectPath -Parent
        $projectIpa = Join-Path $projectDirectory "bin/$runtimeIdentifier/TestApp.ipa"
        New-Item -ItemType Directory -Path (Split-Path $projectIpa -Parent), $outputPath -Force | Out-Null
        if ($argumentText -match "CodesignProvision=Ad Hoc Profile") {
            Set-Content -Path $projectIpa -Value "ad-hoc"
            Set-Content -Path (Join-Path $outputPath "TestApp.ipa") -Value "ad-hoc"
        } else {
            Set-Content -Path $projectIpa -Value "app-store"
        }
    }
    "ios-adhoc-failure" {
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        if ($argumentText -match "CodesignProvision=Ad Hoc Profile") {
            exit 23
        }
        Write-FakeArchive -ArchivePath (Join-Path $outputPath "TestApp.ipa") -Prefix 'Payload/TestApp.app' -Manifest $sourceManifest
    }
}

exit 0
'@ | Set-Content -Path $fakeDotNetScriptPath -Encoding utf8

    if ($IsWindows) {
        @"
@echo off
pwsh -NoLogo -NoProfile -File "$fakeDotNetScriptPath" %*
exit /b %ERRORLEVEL%
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'dotnet.cmd') -Encoding ascii
    } else {
        @"
#!/bin/sh
exec pwsh -NoLogo -NoProfile -File "$fakeDotNetScriptPath" "`$@"
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'dotnet') -Encoding utf8NoBOM
        & chmod +x (Join-Path $script:fakeCommandDirectory 'dotnet')
    }

    $fakeCodesignScriptPath = Join-Path $script:fakeCommandDirectory 'fake-codesign.ps1'
    @'
$target = $args[-1]
if ($env:FAKE_CODESIGN_MODE -eq "nested-failure" -and $target -match "\.(dylib|so)$") {
    exit 17
}
if ($env:FAKE_CODESIGN_MODE -eq "bundle-failure" -and (Test-Path -Path $target -PathType Container)) {
    exit 19
}
exit 0
'@ | Set-Content -Path $fakeCodesignScriptPath -Encoding utf8

    if ($IsWindows) {
        @"
@echo off
pwsh -NoLogo -NoProfile -File "$fakeCodesignScriptPath" %*
exit /b %ERRORLEVEL%
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'codesign.cmd') -Encoding ascii
    } else {
        @"
#!/bin/sh
exec pwsh -NoLogo -NoProfile -File "$fakeCodesignScriptPath" "`$@"
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'codesign') -Encoding utf8NoBOM
        & chmod +x (Join-Path $script:fakeCommandDirectory 'codesign')
    }

    $fakeDittoScriptPath = Join-Path $script:fakeCommandDirectory 'fake-ditto.ps1'
    @'
$ErrorActionPreference = "Stop"

if ($args[0] -eq '-c') {
    $sourcePath = $args[-2]
    $destinationPath = $args[-1]
    Remove-Item -Path $destinationPath -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path $sourcePath -DestinationPath $destinationPath -Force
    exit 0
}

$source = $args[-2]
$destination = $args[-1]
if (Test-Path $destination) {
    Remove-Item -Path $destination -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
Copy-Item -Path $source -Destination $destination -Recurse -Force
exit 0
'@ | Set-Content -Path $fakeDittoScriptPath -Encoding utf8

    if ($IsWindows) {
        @"
@echo off
pwsh -NoLogo -NoProfile -File "$fakeDittoScriptPath" %*
exit /b %ERRORLEVEL%
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'ditto.cmd') -Encoding ascii
    } else {
        @"
#!/bin/sh
exec pwsh -NoLogo -NoProfile -File "$fakeDittoScriptPath" "`$@"
"@ | Set-Content -Path (Join-Path $script:fakeCommandDirectory 'ditto') -Encoding utf8NoBOM
        & chmod +x (Join-Path $script:fakeCommandDirectory 'ditto')
    }

    $pathSeparator = [System.IO.Path]::PathSeparator
    $env:PATH = "$($script:fakeCommandDirectory)$pathSeparator$($env:PATH)"

    $script:fastfileHarnessPath = Join-Path $testRoot 'fastfile-harness.rb'
    @'
$lanes = {}

module UI
  def self.user_error!(message)
    raise message
  end

  def self.important(message)
    puts message
  end

  def self.error(message)
    warn message
  end
end

def default_platform(*_args)
end

def platform(*_args)
  yield
end

def desc(*_args)
end

def lane(name, &block)
  $lanes[name] = block
end

def app_store_connect_api_key(**_kwargs)
  {}
end

def upload_to_play_store(**_kwargs)
end

def upload_to_testflight(*_args)
  raise ENV.fetch("FAKE_TESTFLIGHT_ERROR")
end

load ENV.fetch("FASTFILE_PATH")

options = {
  app_identifier: "com.example.test",
  api_key_id: "key",
  issuer_id: "issuer",
  api_private_key_path: "key.p8",
  ipa: "TestApp.ipa",
  groups: ENV.fetch("FAKE_TESTFLIGHT_GROUPS", "")
}

begin
  $lanes.fetch(:template_app_testflight).call(options)
  puts "lane succeeded"
rescue => error
  warn error.message
  exit 42
end
'@ | Set-Content -Path $script:fastfileHarnessPath -Encoding utf8

    function New-BuildTestCase {
        $caseRoot = Join-Path $testRoot ([guid]::NewGuid().ToString("N"))
        $projectRoot = Join-Path $caseRoot 'project'
        $outputRoot = Join-Path $caseRoot 'output'
        $buildRoot = Join-Path $caseRoot 'build'
        $runnerTemp = Join-Path $caseRoot 'runner-temp'
        New-Item -ItemType Directory -Path $caseRoot, $projectRoot, $outputRoot, $buildRoot, $runnerTemp -Force | Out-Null
        $sourceFixture = New-SourcePackageFixture
        $nuGetConfigPath = Join-Path $caseRoot 'NuGet.base.config'
        @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="private-feed" value="https://example.invalid/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="Legacy.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
"@ | Set-Content -Path $nuGetConfigPath -Encoding utf8
        Set-Content -Path (Join-Path $projectRoot 'TestApp.csproj') -Value '<Project Sdk="Microsoft.NET.Sdk" />'

        return [pscustomobject]@{
            Root = $caseRoot
            ProjectRoot = $projectRoot
            OutputRoot = $outputRoot
            BuildRoot = $buildRoot
            RunnerTemp = $runnerTemp
            GitHubOutput = Join-Path $caseRoot 'github-output.txt'
            NuGetConfigPath = $nuGetConfigPath
            SourceFixture = $sourceFixture
            SourceManifestPath = $sourceFixture.ManifestPath
            SourceSha = $sourceFixture.SourceSha
            TemplatePackagePath = $sourceFixture.TemplatePackagePath
        }
    }

    function Invoke-ExternalPowerShell([string]$FilePath, [string[]]$Arguments) {
        $output = @(& $script:pwshPath -NoLogo -NoProfile -File $FilePath @Arguments 2>&1)
        return [pscustomobject]@{
            ExitCode = $LASTEXITCODE
            Output = ($output | Out-String)
        }
    }

    function Read-GitHubOutputValues([string]$Path) {
        $outputValues = @{}
        foreach ($line in Get-Content -Path $Path) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        return $outputValues
    }

    function Invoke-BuildTemplateApp(
        $TestCase,
        [string]$Platform,
        [string]$TargetFramework,
        [string]$RuntimeIdentifier,
        [switch]$Publish,
        [switch]$CreateBinlog
    ) {
        $arguments = @(
            '-ProjectPath', $TestCase.ProjectRoot,
            '-Platform', $Platform,
            '-TargetFramework', $TargetFramework,
            '-RuntimeIdentifier', $RuntimeIdentifier,
            '-OutputPath', $TestCase.OutputRoot,
            '-AppDisplayVersion', '11.0',
            '-AppBuildNumber', '1',
            '-SourceManifestPath', $TestCase.SourceManifestPath,
            '-SourceSha', $TestCase.SourceSha
        )
        if ($Publish) {
            $arguments += '-Publish'
        }
        if ($CreateBinlog) {
            $arguments += '-CreateBinlog'
        }

        $env:FAKE_SOURCE_MANIFEST_PATH = $TestCase.SourceManifestPath
        $env:FAKE_SOURCE_SHA = $TestCase.SourceSha
        return Invoke-ExternalPowerShell $script:buildTemplateAppHarnessPath $arguments
    }

    function Invoke-NewTemplateApp(
        $TestCase,
        [string]$Variant = 'sample',
        [string]$ProjectName = 'TestApp',
        [string]$Template = 'maui',
        [string]$DotNetTfm = 'net11.0',
        [string]$TargetFramework = 'net11.0-android',
        [string]$ApplicationId = 'com.example.generated',
        [string]$DisplayName = 'Generated App'
    ) {
        $env:FAKE_SOURCE_MANIFEST_PATH = $TestCase.SourceManifestPath
        $env:FAKE_SOURCE_SHA = $TestCase.SourceSha
        return Invoke-ExternalPowerShell $script:newTemplateScriptPath @(
            '-TemplatePackagePath', $TestCase.TemplatePackagePath,
            '-BuildRoot', $TestCase.BuildRoot,
            '-Variant', $Variant,
            '-ProjectName', $ProjectName,
            '-Template', $Template,
            '-TemplateArgsJson', '[]',
            '-DotNetTfm', $DotNetTfm,
            '-TargetFramework', $TargetFramework,
            '-ApplicationId', $ApplicationId,
            '-DisplayName', $DisplayName,
            '-DotNetSdk', '11.0.100',
            '-AppDisplayVersion', '11.0',
            '-AppBuildNumber', '1',
            '-NuGetConfigPath', $TestCase.NuGetConfigPath,
            '-SourceManifestPath', $TestCase.SourceManifestPath,
            '-SourceSha', $TestCase.SourceSha
        )
    }

    function Invoke-PrepareMatrix([string]$Variants, [string]$Platforms) {
        return Invoke-ExternalPowerShell $script:prepareMatrixScriptPath @(
            '-Variants', $Variants,
            '-Platforms', $Platforms,
            '-DotNetTfm', 'net11.0'
        )
    }

    function New-SourceRefTestRepository(
        [switch]$UntrustedHead,
        [string]$HeadBranchName = 'feature/untrusted'
    ) {
        $repositoryPath = Join-Path $testRoot ([guid]::NewGuid().ToString("N"))
        New-Item -ItemType Directory -Path $repositoryPath -Force | Out-Null

        & git -C $repositoryPath init --quiet --initial-branch=main
        & git -C $repositoryPath config user.name 'Template App Tests'
        & git -C $repositoryPath config user.email 'template-app-tests@example.invalid'
        & git -C $repositoryPath config commit.gpgsign false
        Set-Content -Path (Join-Path $repositoryPath 'source.txt') -Value 'trusted'
        & git -C $repositoryPath add source.txt
        & git -C $repositoryPath commit --quiet -m 'trusted source'
        $trustedSha = (& git -C $repositoryPath rev-parse HEAD).Trim()
        & git -C $repositoryPath update-ref refs/remotes/origin/main $trustedSha

        if ($UntrustedHead) {
            & git -C $repositoryPath switch --quiet -c $HeadBranchName
            Set-Content -Path (Join-Path $repositoryPath 'source.txt') -Value 'untrusted'
            & git -C $repositoryPath commit --quiet -am 'untrusted source'
            $headSha = (& git -C $repositoryPath rev-parse HEAD).Trim()
            & git -C $repositoryPath update-ref "refs/remotes/origin/$HeadBranchName" $headSha
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create source-ref test repository."
        }

        return $repositoryPath
    }

    function Invoke-ResolveSourceRef(
        [string]$RepositoryPath,
        [string]$SourceRef,
        [bool]$Publish,
        [string]$TrustedPublishBranches = ''
    ) {
        $arguments = @(
            '-RepositoryPath', $RepositoryPath,
            '-SourceRef', $SourceRef,
            '-WorkflowRef', 'refs/heads/main',
            '-DefaultBranch', 'main',
            "-Publish:$($Publish.ToString().ToLowerInvariant())"
        )
        if (-not [string]::IsNullOrWhiteSpace($TrustedPublishBranches)) {
            $arguments += @('-TrustedPublishBranches', $TrustedPublishBranches)
        }

        return Invoke-ExternalPowerShell $script:resolveSourceRefScriptPath $arguments
    }

    function Invoke-FastfileHarness {
        $output = @(& $script:rubyPath $script:fastfileHarnessPath 2>&1)
        return [pscustomobject]@{
            ExitCode = $LASTEXITCODE
            Output = ($output | Out-String)
        }
    }

    function Reset-BuildTestEnvironment {
        foreach ($name in $script:testEnvironmentNames) {
            [Environment]::SetEnvironmentVariable($name, $null)
        }
        $env:FASTFILE_PATH = $script:fastfilePath
        $env:FAKE_MAUI_ASSEMBLY_DIRECTORY = $script:fakeMauiAssemblyDirectory
    }
}

AfterAll {
    $env:PATH = $script:originalPath
    foreach ($name in $script:testEnvironmentNames) {
        [Environment]::SetEnvironmentVariable($name, $script:originalEnvironment[$name])
    }
    Remove-Item -Path $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'dotnet SDK resolution' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'emits a complete SDK version and derived target framework from global.json' {
        $case = New-BuildTestCase
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        @{
            tools = @{
                dotnet = '11.0.100-preview.1.25120.13'
            }
        } | ConvertTo-Json -Depth 3 | Set-Content -Path (Join-Path $case.Root 'global.json')

        $result = Invoke-ExternalPowerShell $script:resolveDotNetSdkScriptPath @(
            '-RepositoryPath', $case.Root,
            '-DotNetSdk', 'global-json'
        )

        $result.ExitCode | Should -Be 0 -Because $result.Output
        Get-Content -Path $case.GitHubOutput | Should -Contain 'dotnet_sdk=11.0.100-preview.1.25120.13'
        Get-Content -Path $case.GitHubOutput | Should -Contain 'dotnet_tfm=net11.0'
    }

    It 'rejects malformed source-derived SDK version <SdkVersion>' -ForEach @(
        @{ SdkVersion = '11.0' }
        @{ SdkVersion = '11.0.100"; Write-Host compromised; "' }
        @{ SdkVersion = "11.0.100`nforged_output=true" }
    ) {
        $case = New-BuildTestCase
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        @{
            sdk = @{
                version = $SdkVersion
            }
        } | ConvertTo-Json -Depth 3 | Set-Content -Path (Join-Path $case.Root 'global.json')

        $result = Invoke-ExternalPowerShell $script:resolveDotNetSdkScriptPath @(
            '-RepositoryPath', $case.Root,
            '-DotNetSdk', 'global-json'
        )

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'complete SDK'
        Test-Path $case.GitHubOutput | Should -BeFalse
    }
}

Describe 'source ref trust resolution' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'allows a trusted branch at the protected workflow ref' {
        $repositoryPath = New-SourceRefTestRepository
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef 'main' `
            -Publish $true

        $result.ExitCode | Should -Be 0 -Because $result.Output
        Get-Content -Path $env:GITHUB_OUTPUT | Should -Contain 'trusted=true'
    }

    It 'rejects an untrusted branch for protected publishing' {
        $repositoryPath = New-SourceRefTestRepository -UntrustedHead
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef 'feature/untrusted' `
            -Publish $true

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Publishing requires a trusted source_ref'
        Test-Path $env:GITHUB_OUTPUT | Should -BeFalse
    }

    It 'rejects a release-shaped branch that is absent from the controlled allowlist' {
        $branch = 'release/11.0.1xx-preview7'
        $repositoryPath = New-SourceRefTestRepository `
            -UntrustedHead `
            -HeadBranchName $branch
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef $branch `
            -Publish $true

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'exact TEMPLATE_APP_TRUSTED_PUBLISH_BRANCHES entry'
        Test-Path $env:GITHUB_OUTPUT | Should -BeFalse
    }

    It 'allows an exact administrator-configured publish branch' {
        $branch = 'release/11.0.1xx-preview7'
        $repositoryPath = New-SourceRefTestRepository `
            -UntrustedHead `
            -HeadBranchName $branch
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef $branch `
            -Publish $true `
            -TrustedPublishBranches $branch

        $result.ExitCode | Should -Be 0 -Because $result.Output
        Get-Content -Path $env:GITHUB_OUTPUT | Should -Contain 'trusted=true'
    }

    It 'rejects wildcard entries in the controlled publish branch allowlist' {
        $repositoryPath = New-SourceRefTestRepository
        $env:GITHUB_OUTPUT = Join-Path $repositoryPath 'github-output.txt'

        $result = Invoke-ResolveSourceRef `
            -RepositoryPath $repositoryPath `
            -SourceRef 'main' `
            -Publish $true `
            -TrustedPublishBranches 'release/*'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'valid exact branch name'
        Test-Path $env:GITHUB_OUTPUT | Should -BeFalse
    }
}

Describe 'ad-hoc signature repair' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'fails closed when a nested library cannot be signed' {
        $appBundle = Join-Path $testRoot ([guid]::NewGuid().ToString('N') + '.app')
        New-Item -ItemType Directory -Path $appBundle -Force | Out-Null
        New-Item -ItemType File -Path (Join-Path $appBundle 'libbroken.dylib') -Force | Out-Null
        $env:FAKE_CODESIGN_MODE = 'nested-failure'

        {
            Repair-AppleAdhocSignature $appBundle
        } | Should -Throw '*nested library*failed with exit code 17*'
    }

    It 'fails closed when the final bundle signature fails' {
        $appBundle = Join-Path $testRoot ([guid]::NewGuid().ToString('N') + '.app')
        New-Item -ItemType Directory -Path $appBundle -Force | Out-Null
        $env:FAKE_CODESIGN_MODE = 'bundle-failure'

        {
            Repair-AppleAdhocSignature $appBundle
        } | Should -Throw '*failed with exit code 19*'
    }
}

Describe 'bounded Mac notarization polling' {
    BeforeEach {
        Reset-BuildTestEnvironment
        Mock Start-Sleep {}
        Mock Write-NotarySubmissionDiagnostics {}
    }

    It 'uses a workflow-controlled timeout with a bounded default' {
        Get-NotarizationTimeoutSeconds | Should -Be 1800

        $env:TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS = '90'
        Get-NotarizationTimeoutSeconds | Should -Be 90

        $env:TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS = '7201'
        { Get-NotarizationTimeoutSeconds } | Should -Throw '*integer from 1 through 7200*'
    }

    It 'performs a final poll at the configured deadline' {
        $origin = [DateTimeOffset]::Parse('2026-01-01T00:00:00Z')
        $script:notaryTimestamps = @($origin, $origin, $origin.AddSeconds(15))
        $script:notaryTimestampIndex = 0
        Mock Get-NotaryTimestamp {
            $timestamp = $script:notaryTimestamps[$script:notaryTimestampIndex]
            $script:notaryTimestampIndex++
            return $timestamp
        }
        $script:notaryPollCount = 0
        Mock Invoke-NotaryToolJson {
            $script:notaryPollCount++
            [pscustomobject]@{
                status = if ($script:notaryPollCount -lt 3) { 'In Progress' } else { 'Accepted' }
            }
        }

        Wait-NotarySubmission `
            -SubmissionId 'submission-accepted' `
            -CredentialArguments @('--key', 'private-key-path') `
            -TimeoutSeconds 30 `
            -PollIntervalSeconds 15

        Should -Invoke Invoke-NotaryToolJson -Times 3 -Exactly
        Should -Invoke Start-Sleep -Times 2 -Exactly `
            -ParameterFilter { $Seconds -eq 15 }
        Should -Invoke Write-NotarySubmissionDiagnostics -Times 0 -Exactly
    }

    It 'reports the submission log when Apple rejects the submission' {
        Mock Invoke-NotaryToolJson {
            [pscustomobject]@{ status = 'Invalid' }
        }

        {
            Wait-NotarySubmission `
                -SubmissionId 'submission-invalid' `
                -CredentialArguments @('--key', 'private-key-path') `
                -TimeoutSeconds 30 `
                -PollIntervalSeconds 15
        } | Should -Throw "*failed with status 'Invalid'*"

        Should -Invoke Write-NotarySubmissionDiagnostics -Times 1 -Exactly `
            -ParameterFilter { $SubmissionId -eq 'submission-invalid' }
    }

    It 'reports the last status and submission log at the deadline' {
        $origin = [DateTimeOffset]::Parse('2026-01-01T00:00:00Z')
        $script:notaryTimestamps = @(
            $origin,
            $origin,
            $origin.AddSeconds(15),
            $origin.AddSeconds(30)
        )
        $script:notaryTimestampIndex = 0
        Mock Get-NotaryTimestamp {
            $timestamp = $script:notaryTimestamps[$script:notaryTimestampIndex]
            $script:notaryTimestampIndex++
            return $timestamp
        }
        Mock Invoke-NotaryToolJson {
            [pscustomobject]@{ status = 'In Progress' }
        }

        {
            Wait-NotarySubmission `
                -SubmissionId 'submission-timeout' `
                -CredentialArguments @('--key', 'private-key-path') `
                -TimeoutSeconds 30 `
                -PollIntervalSeconds 15
        } | Should -Throw "*did not complete within 30 seconds*Last status: 'In Progress'*"

        Should -Invoke Invoke-NotaryToolJson -Times 3 -Exactly
        Should -Invoke Start-Sleep -Times 2 -Exactly `
            -ParameterFilter { $Seconds -eq 15 }
        Should -Invoke Write-NotarySubmissionDiagnostics -Times 1 -Exactly `
            -ParameterFilter { $SubmissionId -eq 'submission-timeout' }
    }

    It 'submits without the unbounded notarytool wait option' {
        (Get-Command Invoke-MacNotarization).Definition | Should -Not -Match '--wait'
        (Get-Command Invoke-MacNotarization).Definition | Should -Match 'Wait-NotarySubmission'
    }
}

Describe 'optional Apple sideload signing' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'preserves the no-secret iOS fallback' {
        $result = New-IosAdHocSideload `
            -ProjectFile $projectFile `
            -TargetFramework 'net11.0-ios' `
            -Configuration 'Release' `
            -RuntimeIdentifier 'ios-arm64' `
            -OutputPath $testRoot `
            -AppDisplayVersion '11.0' `
            -AppBuildNumber '1'

        $result | Should -BeNullOrEmpty
    }

    It 'fails when a configured iOS ad-hoc publish fails' {
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'AdHoc Profile'
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $binlogPath = Join-Path $testRoot 'ios-adhoc-build.binlog'
        $script:publishArguments = $null
        Mock Invoke-DotNetPublish {
            param($Arguments)
            $script:publishArguments = $Arguments
            throw 'simulated ad-hoc publish failure'
        }

        {
            New-IosAdHocSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-ios' `
                -Configuration 'Release' `
                -RuntimeIdentifier 'ios-arm64' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -BinlogArguments @("/bl:$binlogPath")
        } | Should -Throw '*simulated ad-hoc publish failure*'

        $script:publishArguments | Should -Contain "/bl:$binlogPath"
    }

    It 'fails when Developer ID signing is only partially configured' {
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = 'Developer ID Application'

        {
            New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-maccatalyst' `
                -Configuration 'Release' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -RuntimeIdentifier 'maccatalyst-arm64'
        } | Should -Throw '*partially configured*'
    }

    It 'fails when a configured Developer ID publish fails' {
        $env:APPLE_DEVELOPERID_CODESIGN_KEY = 'Developer ID Application'
        $env:APPLE_DEVELOPERID_CODESIGN_PROVISION = 'Developer ID Profile'
        $binlogPath = Join-Path $testRoot 'maccatalyst-developer-id-build.binlog'
        $script:publishArguments = $null
        Mock Invoke-DotNetPublish {
            param($Arguments)
            $script:publishArguments = $Arguments
            throw 'simulated Developer ID publish failure'
        }

        {
            New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework 'net11.0-maccatalyst' `
                -Configuration 'Release' `
                -OutputPath $testRoot `
                -AppDisplayVersion '11.0' `
                -AppBuildNumber '1' `
                -RuntimeIdentifier 'maccatalyst-arm64' `
                -BinlogArguments @("/bl:$binlogPath")
        } | Should -Throw '*simulated Developer ID publish failure*'

        $script:publishArguments | Should -Contain "/bl:$binlogPath"
    }
}

Describe 'Developer ID installer configuration' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'accepts Developer ID assets when both are absent' {
        Assert-PairedEnvironmentValues `
            'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
            'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
            'Developer ID sideload signing' |
            Should -BeFalse
    }

    It 'rejects a Developer ID certificate without its profile' {
        $env:APPLE_DEVELOPERID_CERTIFICATE_BASE64 = 'certificate'

        {
            Assert-PairedEnvironmentValues `
                'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
                'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
                'Developer ID sideload signing'
        } | Should -Throw '*partially configured*'
    }

    It 'rejects a Developer ID profile without its certificate' {
        $env:APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64 = 'profile'

        {
            Assert-PairedEnvironmentValues `
                'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
                'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
                'Developer ID sideload signing'
        } | Should -Throw '*partially configured*'
    }

    It 'accepts Developer ID assets when both are present' {
        $env:APPLE_DEVELOPERID_CERTIFICATE_BASE64 = 'certificate'
        $env:APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64 = 'profile'

        Assert-PairedEnvironmentValues `
            'APPLE_DEVELOPERID_CERTIFICATE_BASE64' `
            'APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64' `
            'Developer ID sideload signing' |
            Should -BeTrue
    }
}

Describe 'custom template variant validation' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'accepts a custom variant with safe project name <ProjectName>' -ForEach @(
        @{ ProjectName = 'CustomApp' }
        @{ ProjectName = 'Custom.App_2-Preview' }
    ) {
        param($ProjectName)

        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            'Custom_Variant-2' = @{
                displayName = 'Custom App'
                projectName = $ProjectName
                template = 'maui'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom_variant-2' 'android'

        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match '"variant":"custom_variant-2"'
        $result.Output | Should -Match ([regex]::Escape("""projectName"":""$ProjectName"""))
    }

    It 'rejects a custom variant without a template' {
        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            custom = @{
                displayName = 'Custom App'
                projectName = 'CustomApp'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "Variant 'custom' does not define required field 'template'"
    }

    It 'rejects a custom variant without a project name' {
        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            custom = @{
                displayName = 'Custom App'
                template = 'maui'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "Variant 'custom' does not define required field 'projectName'"
    }

    It 'rejects unsafe project name <Case>' -ForEach @(
        @{ Case = 'parent path escape'; ProjectName = '../escape' }
        @{ Case = 'forward-slash path'; ProjectName = 'nested/name' }
        @{ Case = 'backslash path'; ProjectName = 'nested\name' }
        @{ Case = 'quote injection'; ProjectName = 'Bad"Name' }
        @{ Case = 'newline injection'; ProjectName = "Bad`nName" }
    ) {
        param($ProjectName)

        $env:TEMPLATE_APP_VARIANTS_JSON = @{
            custom = @{
                displayName = 'Custom App'
                projectName = $ProjectName
                template = 'maui'
                androidApplicationId = 'com.example.custom'
            }
        } | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'custom' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Invalid project name'
    }

    It 'rejects an unsafe custom variant name <Name>' -ForEach @(
        @{ Name = '../escape' }
        @{ Name = 'nested/name' }
        @{ Name = 'nested\name' }
        @{ Name = 'custom.variant' }
    ) {
        param($Name)

        $customDefinitions = @{}
        $customDefinitions[$Name] = @{
            displayName = 'Custom App'
            projectName = 'CustomApp'
            template = 'maui'
            androidApplicationId = 'com.example.custom'
        }
        $env:TEMPLATE_APP_VARIANTS_JSON = $customDefinitions | ConvertTo-Json -Compress

        $result = Invoke-PrepareMatrix 'all' 'android'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Invalid custom template app variant name'
    }
}

Describe 'Android artifact safety' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'requires an APK before assigning the sideload output' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'no-artifacts'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Android APK publish completed but no APK artifact was found'
        if (Test-Path $case.GitHubOutput) {
            Get-Content -Path $case.GitHubOutput -Raw | Should -Not -Match 'sideload_package_path='
        }
    }

    It 'writes end-to-end provenance for a dry-run APK payload' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:GITHUB_OUTPUT = $case.GitHubOutput

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64'

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = Read-GitHubOutputValues $case.GitHubOutput
        $outputValues.provenance_path | Should -Not -BeNullOrEmpty
        Test-Path $outputValues.provenance_path | Should -BeTrue

        $provenance = Get-Content -Path $outputValues.provenance_path -Raw | ConvertFrom-Json
        $provenance.sourceSha | Should -Be $case.SourceSha
        $provenance.platform | Should -Be 'android'
        @($provenance.resolutions).Count | Should -Be 1
        $provenance.resolutions[0].description | Should -Be 'Android APK publish'
        $provenance.resolutions[0].resolution.packages.id | Should -Contain 'Microsoft.Maui.Controls'
        @($provenance.payloads).Count | Should -Be 1
        $payload = $provenance.payloads[0]
        $payload.file | Should -Match '\.apk$'
        @($payload.assemblies | ForEach-Object { [System.IO.Path]::GetFileName($_.path) }) |
            Should -Contain 'Microsoft.Maui.dll'
        @($payload.assemblies | ForEach-Object { [System.IO.Path]::GetFileName($_.path) }) |
            Should -Contain 'Microsoft.Maui.Controls.dll'
        @($payload.assemblies | ForEach-Object { [System.IO.Path]::GetFileName($_.path) }) |
            Should -Contain 'Microsoft.Maui.Graphics.dll'
    }

    It 'emits installable APK, store AAB, and distinct binlog outputs' {
        $case = New-BuildTestCase
        $keystorePath = Join-Path $case.Root 'test.keystore'
        Set-Content -Path $keystorePath -Value 'fake keystore'
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:ANDROID_KEYSTORE_PATH = $keystorePath
        $env:ANDROID_KEYSTORE_PASSWORD = 'password'
        $env:ANDROID_KEY_ALIAS = 'alias'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = Read-GitHubOutputValues $case.GitHubOutput

        $outputValues.package_path | Should -Match '\.aab$'
        $outputValues.sideload_package_path | Should -Match '\.apk$'
        $outputValues.provenance_path | Should -Not -BeNullOrEmpty
        $outputValues.binlog_path | Should -Be (Join-Path $case.OutputRoot 'build.binlog')
        $outputValues.store_binlog_path | Should -Be (Join-Path $case.OutputRoot 'store-build.binlog')
        Test-Path $outputValues.package_path | Should -BeTrue
        Test-Path $outputValues.sideload_package_path | Should -BeTrue
        Test-Path $outputValues.provenance_path | Should -BeTrue
        Test-Path $outputValues.binlog_path | Should -BeTrue
        Test-Path $outputValues.store_binlog_path | Should -BeTrue
    }

    It 'keeps every configured Android signing secret out of APK and AAB binlog command lines' {
        $case = New-BuildTestCase
        $keystorePath = Join-Path $case.Root 'test.keystore'
        $signingEnvironmentLog = Join-Path $case.Root 'signing-environment.log'
        Set-Content -Path $keystorePath -Value 'fake keystore'
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:FAKE_ANDROID_SIGNING_ENV_LOG = $signingEnvironmentLog
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:ANDROID_KEYSTORE_PATH = $keystorePath
        $env:ANDROID_KEYSTORE_PASSWORD = 'store-password-secret'
        $env:ANDROID_KEY_PASSWORD = 'key-password-secret'
        $env:ANDROID_KEY_ALIAS = 'signing-alias-secret'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $binlogCommandLines = @(
            Get-Content -Path (Join-Path $case.OutputRoot 'build.binlog') -Raw
            Get-Content -Path (Join-Path $case.OutputRoot 'store-build.binlog') -Raw
        )
        foreach ($commandLine in $binlogCommandLines) {
            $commandLine | Should -Match '(?s)-p\s+AndroidSigningKeyAlias=env:ANDROID_SIGNING_KEY_ALIAS'
            $commandLine | Should -Match '(?s)-p\s+AndroidSigningStorePass=env:ANDROID_SIGNING_STORE_PASS'
            $commandLine | Should -Match '(?s)-p\s+AndroidSigningKeyPass=env:ANDROID_SIGNING_KEY_PASS'
            $commandLine | Should -Not -Match 'signing-alias-secret|store-password-secret|key-password-secret'
        }

        $signingEnvironment = Get-Content -Path $signingEnvironmentLog -Raw
        $signingEnvironment | Should -Match 'alias=signing-alias-secret'
        $signingEnvironment | Should -Match 'storePass=store-password-secret'
        $signingEnvironment | Should -Match 'keyPass=key-password-secret'
    }

    It 'fails the build when the packaged Android payload was built from a different source commit' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:FAKE_MAUI_ASSEMBLY_DIRECTORY = Initialize-FakeMauiPayloadAssemblies -SourceSha 'fedcba9876543210fedcba9876543210fedcba98'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'Packaged assembly'
        $result.Output | Should -Match 'not from'
        if (Test-Path $case.GitHubOutput) {
            (Read-GitHubOutputValues $case.GitHubOutput).ContainsKey('package_path') | Should -BeFalse
        }
    }
}

Describe 'iOS dry-run artifact safety' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'keeps a device IPA when simulator artifact discovery fails' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-device-only'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64'

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = Read-GitHubOutputValues $case.GitHubOutput

        $outputValues.package_path | Should -Match '\.ipa$'
        $outputValues.sideload_package_path | Should -Be $outputValues.package_path
        $outputValues.provenance_path | Should -Not -BeNullOrEmpty
        Test-Path $outputValues.package_path | Should -BeTrue
        Test-Path $outputValues.provenance_path | Should -BeTrue

        $provenance = Get-Content -Path $outputValues.provenance_path -Raw | ConvertFrom-Json
        $provenance.sourceSha | Should -Be $case.SourceSha
        $provenance.platform | Should -Be 'ios'
        @($provenance.resolutions.description) | Should -Contain 'iOS simulator build'
        @($provenance.resolutions.description) | Should -Contain 'iOS unsigned device build'
        @($provenance.payloads).Count | Should -Be 1
        $payload = $provenance.payloads[0]
        $payload.file | Should -Match '\.ipa$'
        @($payload.assemblies | ForEach-Object { [System.IO.Path]::GetFileName($_.path) }) |
            Should -Contain 'Microsoft.Maui.dll'
        @($payload.assemblies | ForEach-Object { [System.IO.Path]::GetFileName($_.path) }) |
            Should -Contain 'Microsoft.Maui.Controls.dll'
        @($payload.assemblies | ForEach-Object { [System.IO.Path]::GetFileName($_.path) }) |
            Should -Contain 'Microsoft.Maui.Graphics.dll'
    }
}

Describe 'publish binlogs' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'uses a distinct store binlog for an Android publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'android' `
            -Publish `
            -CreateBinlog

        $configuration.BuildPath | Should -Be (Join-Path $testRoot 'build.binlog')
        $configuration.StorePath | Should -Be (Join-Path $testRoot 'store-build.binlog')
        $configuration.BuildArguments | Should -Contain "/bl:$($configuration.BuildPath)"
        $configuration.StoreArguments | Should -Contain "/bl:$($configuration.StorePath)"
    }

    It 'uses a dedicated binlog for an iOS ad-hoc publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'ios' `
            -Publish `
            -CreateBinlog

        $configuration.SideloadPath | Should -Be (Join-Path $testRoot 'ios-adhoc-build.binlog')
        $configuration.SideloadArguments | Should -Contain "/bl:$($configuration.SideloadPath)"
    }

    It 'uses a dedicated binlog for a Mac Catalyst Developer ID publish' {
        $configuration = Get-BinlogConfiguration `
            -OutputPath $testRoot `
            -Platform 'maccatalyst' `
            -Publish `
            -CreateBinlog

        $configuration.SideloadPath | Should -Be (Join-Path $testRoot 'maccatalyst-developer-id-build.binlog')
        $configuration.SideloadArguments | Should -Contain "/bl:$($configuration.SideloadPath)"
    }

    It 'emits both primary and ad-hoc iOS binlogs from the publish path' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-publish-success'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $env:IOS_CODESIGN_PROVISION = 'App Store Profile'
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'Ad Hoc Profile'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.binlog_path | Should -Be (Join-Path $case.OutputRoot 'build.binlog')
        $outputValues.sideload_binlog_path | Should -Be (Join-Path $case.OutputRoot 'ios-adhoc-build.binlog')
        Test-Path $outputValues.binlog_path | Should -BeTrue
        Test-Path $outputValues.sideload_binlog_path | Should -BeTrue
    }

    It 'preserves the App Store IPA before the ad-hoc publish can overwrite project outputs' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-store-overwrite'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $env:IOS_CODESIGN_PROVISION = 'App Store Profile'
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'Ad Hoc Profile'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64' `
            -Publish

        $result.ExitCode | Should -Be 0 -Because $result.Output
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.package_path | Should -Be (Join-Path $case.OutputRoot 'store/TestApp.ipa')
        Get-Content -Path $outputValues.package_path -Raw | Should -Match '^app-store'
        Get-Content -Path (Join-Path $case.ProjectRoot 'bin/ios-arm64/TestApp.ipa') -Raw | Should -Match '^ad-hoc'
        Get-Content -Path $outputValues.sideload_package_path -Raw | Should -Match '^ad-hoc'
    }

    It 'preserves the ad-hoc binlog output when the secondary publish fails' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'ios-adhoc-failure'
        $env:GITHUB_OUTPUT = $case.GitHubOutput
        $env:RUNNER_TEMP = $case.RunnerTemp
        $env:IOS_CODESIGN_KEY = 'Apple Distribution'
        $env:IOS_CODESIGN_PROVISION = 'App Store Profile'
        $env:IOS_ADHOC_CODESIGN_PROVISION = 'Ad Hoc Profile'

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'ios' `
            -TargetFramework 'net11.0-ios' `
            -RuntimeIdentifier 'ios-arm64' `
            -Publish `
            -CreateBinlog

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match 'iOS ad-hoc publish failed with exit code 23'
        $outputValues = @{}
        foreach ($line in Get-Content -Path $case.GitHubOutput) {
            $name, $value = $line -split '=', 2
            $outputValues[$name] = $value
        }

        $outputValues.sideload_binlog_path | Should -Be (Join-Path $case.OutputRoot 'ios-adhoc-build.binlog')
        Test-Path $outputValues.sideload_binlog_path | Should -BeTrue
    }
}

Describe 'source package trust' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'rejects a source package whose nuspec commit no longer matches the pinned source sha' {
        $fixture = New-SourcePackageFixture
        New-FakeSourcePackage `
            -PackageDirectory $fixture.PackageDirectory `
            -PackageId 'Microsoft.Maui.Controls' `
            -Version $fixture.Version `
            -RepositoryCommit 'fedcba9876543210fedcba9876543210fedcba98' | Out-Null

        {
            Read-SourcePackageManifest -Path $fixture.ManifestPath -SourceSha $fixture.SourceSha
        } | Should -Throw '*Source provenance mismatch*'
    }

    It 'rejects a source package manifest whose recorded hashes do not match the nupkg' {
        $fixture = New-SourcePackageFixture
        $manifest = Get-SourcePackageManifestObject $fixture
        $manifest.packages[0].sha512 = 'hash-mismatch'
        Save-SourcePackageManifest -Fixture $fixture -Manifest $manifest

        {
            Read-SourcePackageManifest -Path $fixture.ManifestPath -SourceSha $fixture.SourceSha
        } | Should -Throw '*(sha512)*'
    }

    It 'rejects a source package manifest that omits a required package' {
        $fixture = New-SourcePackageFixture -PackageIds (
            $script:requiredSourcePackageIds | Where-Object { $_ -ne 'Microsoft.Maui.Sdk' }
        )

        {
            Read-SourcePackageManifest -Path $fixture.ManifestPath -SourceSha $fixture.SourceSha
        } | Should -Throw "*Required source-built package 'Microsoft.Maui.Sdk' is missing.*"
    }

    It 'rejects a source package manifest with multiple source-built template packages' {
        $fixture = New-SourcePackageFixture -PackageIds (
            $script:requiredSourcePackageIds + 'Microsoft.Maui.Templates.net11'
        )

        {
            Read-SourcePackageManifest -Path $fixture.ManifestPath -SourceSha $fixture.SourceSha
        } | Should -Throw '*Expected exactly one source-built template package.*'
    }
}

Describe 'template source package configuration' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'writes fail-closed NuGet source mapping and MauiVersion for generated template apps' {
        $case = New-BuildTestCase
        $env:GITHUB_SHA = 'workflowcommit0123456789012345678901234567890'
        $env:GITHUB_RUN_ID = '4242'

        $result = Invoke-NewTemplateApp -TestCase $case -ProjectName 'GeneratedApp'

        $result.ExitCode | Should -Be 0 -Because $result.Output

        $projectRoot = Join-Path $case.BuildRoot 'sample'
        $projectDirectory = Join-Path $projectRoot 'GeneratedApp'
        $projectFilePath = Join-Path $projectDirectory 'GeneratedApp.csproj'
        $generatedNuGetConfigPath = Join-Path $projectRoot 'NuGet.config'
        $generatedPropsPath = Join-Path $projectRoot 'Directory.Build.props'
        $generatedSdkDirectory = Join-Path $projectRoot '.maui-sdk'
        $generatedSdkTargetsPath = Join-Path $generatedSdkDirectory 'Sdk/Sdk.targets'
        $generatedSdkPropsPath = Join-Path $generatedSdkDirectory 'Sdk/Sdk.props'
        $sourceManifestCopyPath = Join-Path $projectDirectory 'source-packages.json'
        $sourceProvenancePath = Join-Path $projectDirectory 'Resources/Raw/source-provenance.json'

        Test-Path $projectFilePath | Should -BeTrue
        Test-Path $generatedNuGetConfigPath | Should -BeTrue
        Test-Path $generatedPropsPath | Should -BeTrue
        Test-Path $generatedSdkTargetsPath | Should -BeTrue
        Test-Path $generatedSdkPropsPath | Should -BeTrue
        Test-Path $sourceManifestCopyPath | Should -BeTrue
        Test-Path $sourceProvenancePath | Should -BeTrue

        [xml]$generatedNuGetConfig = Get-Content -Path $generatedNuGetConfigPath -Raw
        @($generatedNuGetConfig.configuration.packageSources.add | ForEach-Object key) |
            Should -Contain 'maui-source'
        $mauiMapping = @($generatedNuGetConfig.configuration.packageSourceMapping.packageSource |
            Where-Object key -EQ 'maui-source')
        $mauiMapping.Count | Should -Be 1
        @($mauiMapping[0].package | ForEach-Object pattern) | Should -Be @('Microsoft.Maui.*')
        @($generatedNuGetConfig.configuration.packageSourceMapping.packageSource |
            Where-Object key -EQ 'nuget.org').package.pattern |
            Should -Contain '*'

        [xml]$directoryBuildProps = Get-Content -Path $generatedPropsPath -Raw
        $directoryBuildProps.Project.PropertyGroup.MauiVersion | Should -Be $case.SourceFixture.Version

        [xml]$projectXml = Get-Content -Path $projectFilePath -Raw
        $projectXml.Project.PropertyGroup.MauiVersion | Should -Be $case.SourceFixture.Version
        $projectXml.Project.PropertyGroup.SkipMauiWorkloadManifest | Should -Be 'true'
        $controlsReference = @($projectXml.Project.ItemGroup.PackageReference | Where-Object Include -EQ 'Microsoft.Maui.Controls')
        $graphicsReference = @($projectXml.Project.ItemGroup.PackageReference | Where-Object Include -EQ 'Microsoft.Maui.Graphics')
        $sdkImport = @($projectXml.Project.Import | Where-Object Project -EQ $generatedSdkTargetsPath)
        $sdkImport.Count | Should -Be 1
        $controlsReference[0].Version | Should -Be $case.SourceFixture.Version
        $graphicsReference[0].Version | Should -Be $case.SourceFixture.Version
        $graphicsReference[0].SelectSingleNode('Version') | Should -BeNullOrEmpty

        $sourceManifestCopy = Get-Content -Path $sourceManifestCopyPath -Raw | ConvertFrom-Json
        $sourceManifestCopy.sourceSha | Should -Be $case.SourceSha
        @($sourceManifestCopy.packages).Count | Should -Be 10

        $sourceProvenance = Get-Content -Path $sourceProvenancePath -Raw | ConvertFrom-Json
        $templatePackage = Get-TemplatePackageEntry $case.SourceFixture
        $sourceProvenance.sourceSha | Should -Be $case.SourceSha
        $sourceProvenance.frameworkVersion | Should -Be $case.SourceFixture.Version
        $sourceProvenance.template.sha512 | Should -Be $templatePackage['sha512']
        $sourceProvenance.workflowCommit | Should -Be $env:GITHUB_SHA
        $sourceProvenance.runId | Should -Be $env:GITHUB_RUN_ID
    }
}

Describe 'source-built restore verification' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'fails closed when a build resolves a stable Microsoft.Maui package instead of the pinned source package' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:FAKE_SOURCE_ASSETS_MODE = 'mixed-stable'
        $env:GITHUB_OUTPUT = $case.GitHubOutput

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "Resolved 'Microsoft\.Maui\.Controls/8\.0\.100'"
        $result.Output | Should -Match 'not the pinned source-built package'
    }

    It 'fails closed when required source-built packages are missing from restore assets' {
        $case = New-BuildTestCase
        $env:FAKE_DOTNET_MODE = 'android-success'
        $env:FAKE_SOURCE_ASSETS_MODE = 'missing-essential'
        $env:GITHUB_OUTPUT = $case.GitHubOutput

        $result = Invoke-BuildTemplateApp `
            -TestCase $case `
            -Platform 'android' `
            -TargetFramework 'net11.0-android' `
            -RuntimeIdentifier 'android-arm64'

        $result.ExitCode | Should -Not -Be 0
        $result.Output | Should -Match "Required resolved source package 'Microsoft\.Maui\.Resizetizer' is missing"
    }
}

Describe 'packaged payload provenance' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'keeps direct payload helper compatibility when called without a manifest' {
        $fixture = New-SourcePackageFixture
        $archive = New-FakePayloadArchive `
            -Path (Join-Path $fixture.Root 'TestApp-no-manifest.apk') `
            -Manifest $fixture.Manifest

        $proof = Get-AppPayloadProof -Path $archive.FullName -SourceSha $fixture.SourceSha

        $proof.file | Should -Be 'TestApp-no-manifest.apk'
        $proof.assemblies.Count | Should -BeGreaterThan 0
        $proof.dependencies | Should -Contain "Microsoft.Maui.Controls/$($fixture.Version)"
    }

    It 'reads informational source sha and dependency provenance from packaged MAUI archives' {
        $fixture = New-SourcePackageFixture
        $assetsPath = Write-SourcePackageAssetsFile `
            -AssetsPath (Join-Path $fixture.Root 'obj/project.assets.json') `
            -Manifest $fixture.Manifest
        $null = Test-SourcePackageAssets -AssetsPath $assetsPath -Manifest $fixture.Manifest

        $archive = New-FakePayloadArchive `
            -Path (Join-Path $fixture.Root 'TestApp-Signed.apk') `
            -Manifest $fixture.Manifest

        $proof = Get-AppPayloadProof -Path $archive.FullName -SourceSha $fixture.SourceSha -Manifest $fixture.Manifest

        $proof.file | Should -Be 'TestApp-Signed.apk'
        @($proof.assemblies.path | ForEach-Object { [System.IO.Path]::GetFileName($_) }) |
            Should -Contain 'Microsoft.Maui.Controls.dll'
        $proof.assemblies.informationalVersion | Should -Contain "1.0.0+$($fixture.SourceSha)"
        $proof.dependencies | Should -Contain "Microsoft.Maui.Controls/$($fixture.Version)"
    }

    It 'rejects manifest-verified payloads that omit a required MAUI assembly' {
        $fixture = New-SourcePackageFixture
        $assetsPath = Write-SourcePackageAssetsFile `
            -AssetsPath (Join-Path $fixture.Root 'obj/project.assets.json') `
            -Manifest $fixture.Manifest
        $null = Test-SourcePackageAssets -AssetsPath $assetsPath -Manifest $fixture.Manifest

        $archive = New-FakePayloadArchive `
            -Path (Join-Path $fixture.Root 'TestApp-missing-graphics.ipa') `
            -Manifest $fixture.Manifest `
            -OmitRequiredAssembly

        {
            Get-AppPayloadProof -Path $archive.FullName -SourceSha $fixture.SourceSha -Manifest $fixture.Manifest
        } | Should -Throw "*Required MAUI assembly 'Microsoft.Maui.Graphics.dll' is missing*"
    }

    It 'rejects packaged MAUI assemblies that were not built from the pinned source sha' {
        $fixture = New-SourcePackageFixture
        $assetsPath = Write-SourcePackageAssetsFile `
            -AssetsPath (Join-Path $fixture.Root 'obj/project.assets.json') `
            -Manifest $fixture.Manifest
        $null = Test-SourcePackageAssets -AssetsPath $assetsPath -Manifest $fixture.Manifest

        $archive = New-FakePayloadArchive `
            -Path (Join-Path $fixture.Root 'TestApp.ipa') `
            -Manifest $fixture.Manifest `
            -SourceSha 'fedcba9876543210fedcba9876543210fedcba98'

        {
            Get-AppPayloadProof -Path $archive.FullName -SourceSha $fixture.SourceSha -Manifest $fixture.Manifest
        } | Should -Throw '*not from*'
    }
}

Describe 'workflow test gate' {
    It 'runs the behavioral suite before matrix preparation' {
        $scriptTestsJob = [regex]::Match(
            $script:workflowText,
            '(?ms)^  script-tests:[ \t]*\r?\n.*?(?=^  [A-Za-z0-9_-]+:[ \t]*\r?$|\z)'
        )

        $scriptTestsJob.Success | Should -BeTrue
        $scriptTestsJob.Value | Should -Match '(?s)Invoke-Pester.*?-CI'
        $script:workflowText | Should -Match (
            '(?ms)^  prepare:.*?^\s{4}needs: script-tests\s*$')
        $script:workflowText | Should -Not -Match (
            '(?ms)uses:\s*actions/checkout@v4\s+with:\s+' +
            'ref:\s*\$\{\{\s*github\.ref\s*\}\}')
        [regex]::Matches(
            $script:workflowText,
            'ref:\s*\$\{\{\s*github\.sha\s*\}\}').Count |
            Should -Be 4
    }

    It 'passes source-derived SDK outputs to PowerShell through environment variables' {
        $script:workflowText | Should -Not -Match (
            '-DotNet(?:Sdk|Tfm)\s+"\$\{\{\s*(?:steps\.sdk|needs\.prepare)\.outputs\.dotnet_')
        $script:workflowText | Should -Not -Match (
            '\$displayVersion\s*=\s*"\$\{\{\s*steps\.sdk\.outputs\.dotnet_tfm')
        [regex]::Matches($script:workflowText, '-DotNetSdk "\$env:DOTNET_SDK"').Count |
            Should -Be 2
        [regex]::Matches($script:workflowText, '-DotNetTfm "\$env:DOTNET_TFM"').Count |
            Should -Be 3
    }

    It 'passes the administrator-controlled publish branch allowlist to the trust gate' {
        $script:workflowText | Should -Match (
            'TRUSTED_PUBLISH_BRANCHES:\s*\$\{\{\s*vars\.TEMPLATE_APP_TRUSTED_PUBLISH_BRANCHES\s*\}\}')
        $script:workflowText | Should -Match (
            '-TrustedPublishBranches "\$env:TRUSTED_PUBLISH_BRANCHES"')
    }

    It 'passes generated project paths to PowerShell through environment variables' {
        $script:workflowText | Should -Not -Match (
            '-ProjectPath\s+"\$\{\{\s*steps\.app\.outputs\.project_path\s*\}\}"')
        [regex]::Matches(
            $script:workflowText,
            'PROJECT_PATH:\s*\$\{\{\s*steps\.app\.outputs\.project_path\s*\}\}').Count |
            Should -Be 2
        [regex]::Matches($script:workflowText, '-ProjectPath "\$env:PROJECT_PATH"').Count |
            Should -Be 2
    }

    It 'serializes publish runs across source refs while preserving dry-run concurrency' {
        $expectedGroup = "group: `${{ github.workflow }}-`${{ inputs.publish && 'publish' || format('dry-run-{0}', inputs.source_ref) }}"
        $script:workflowText | Should -Match ([regex]::Escape($expectedGroup))
        $script:workflowText | Should -Match 'TEMPLATE_APP_NOTARIZATION_TIMEOUT_SECONDS:.*1800'
    }

    It 'sets realistic deterministic timeouts for every job' {
        $expectedTimeouts = [ordered]@{
            'script-tests' = 15
            'prepare' = 30
            'dry-run-build' = 120
            'publish' = 180
        }

        foreach ($jobName in $expectedTimeouts.Keys) {
            $jobBlock = [regex]::Match(
                $script:workflowText,
                "(?ms)^  $([regex]::Escape($jobName)):\s*\r?\n.*?(?=^  [A-Za-z0-9_-]+:\s*\r?$|\z)"
            )
            $jobBlock.Success | Should -BeTrue -Because "job '$jobName' must exist"
            $jobBlock.Value | Should -Match (
                "(?m)^\s{4}timeout-minutes:\s+$($expectedTimeouts[$jobName])\s*$")
        }
    }

    It 'includes the workflow attempt in every uploaded artifact name' {
        $artifactNames = [regex]::Matches(
            $script:workflowText,
            '(?m)^\s+name:\s+(template-app-(?:dryrun|publish)[^\r\n]+)$'
        )

        $artifactNames.Count | Should -Be 2
        foreach ($artifactName in $artifactNames) {
            $artifactName.Groups[1].Value | Should -Match (
                '\$\{\{\s*github\.run_attempt\s*\}\}')
        }
    }

    It 'does not create or upload binlogs from secret-bearing publish jobs' {
        $publishJob = [regex]::Match(
            $script:workflowText,
            '(?ms)^  publish:\s*\r?\n.*?(?=^  [A-Za-z0-9_-]+:\s*\r?$|\z)'
        )
        $dryRunJob = [regex]::Match(
            $script:workflowText,
            '(?ms)^  dry-run-build:\s*\r?\n.*?(?=^  [A-Za-z0-9_-]+:\s*\r?$|\z)'
        )

        $publishJob.Success | Should -BeTrue
        $dryRunJob.Success | Should -BeTrue
        $publishJob.Value | Should -Not -Match '(?m)^\s+-CreateBinlog'
        $publishJob.Value | Should -Not -Match 'template-app-publish-binlog'
        $publishJob.Value | Should -Not -Match 'steps\.build\.outputs\.(?:binlog_path|store_binlog_path|sideload_binlog_path)'
        $dryRunJob.Value | Should -Match '(?m)^\s+-CreateBinlog'
        $dryRunJob.Value | Should -Match 'steps\.build\.outputs\.binlog_path'
    }
}

Describe 'template metadata replacement' {
    BeforeAll {
        $newTemplateScriptPath = Join-Path $PSScriptRoot 'New-TemplateApp.ps1'
        $newTemplateTokens = $null
        $newTemplateParseErrors = $null
        $newTemplateAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $newTemplateScriptPath,
            [ref]$newTemplateTokens,
            [ref]$newTemplateParseErrors
        )
        if ($newTemplateParseErrors -and $newTemplateParseErrors.Count -gt 0) {
            throw ($newTemplateParseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
        }

        foreach ($functionName in @('ConvertTo-XmlEscaped', 'Set-ProjectElementValue')) {
            $function = $newTemplateAst.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)

            if (-not $function) {
                throw "Function '$functionName' not found"
            }

            Invoke-Expression $function.Extent.Text
        }
    }

    It 'treats dollar signs as literal replacement text' {
        $content = '<ApplicationTitle>Old</ApplicationTitle>'

        Set-ProjectElementValue $content 'ApplicationTitle' 'Cash $$ App $&' |
            Should -Be '<ApplicationTitle>Cash $$ App $&amp;</ApplicationTitle>'
    }
}

Describe 'TestFlight error handling' {
    BeforeEach {
        Reset-BuildTestEnvironment
    }

    It 'fails when external groups cannot receive a build due to a beta-review conflict' {
        $env:FAKE_TESTFLIGHT_ERROR = 'Another build is in review'
        $env:FAKE_TESTFLIGHT_GROUPS = 'External Testers'

        $result = Invoke-FastfileHarness

        $result.ExitCode | Should -Be 42
        $result.Output | Should -Match 'requested external TestFlight groups did not receive it'
    }

    It 'allows an upload-only beta-review conflict when no external groups were requested' {
        $env:FAKE_TESTFLIGHT_ERROR = 'Another build is in review'
        $env:FAKE_TESTFLIGHT_GROUPS = ''

        $result = Invoke-FastfileHarness

        $result.ExitCode | Should -Be 0
        $result.Output | Should -Match 'no external distribution was requested'
        $result.Output | Should -Match 'lane succeeded'
    }

    It 'fails instead of reporting a processing timeout as successful' {
        $env:FAKE_TESTFLIGHT_ERROR = 'BuildWatcher exceeded processing timeout'
        $env:FAKE_TESTFLIGHT_GROUPS = 'External Testers'

        $result = Invoke-FastfileHarness

        $result.ExitCode | Should -Be 42
        $result.Output | Should -Match 'requested TestFlight distribution could not be completed'
    }
}
