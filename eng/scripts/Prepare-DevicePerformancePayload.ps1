#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Packages separately built base/head device-test apps into one Helix payload.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseArtifacts,

    [Parameter(Mandatory = $true)]
    [string]$HeadArtifacts,

    [Parameter(Mandatory = $true)]
    [ValidateSet("ios", "maccatalyst", "android", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ExpectedScenario,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$Repository,

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('\A(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?(?:\[bot\])?)?\z')]
    [string]$PullRequestAuthor = "",

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$HarnessSha,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AzdoBuildId,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AzdoBuildUrl,

    [Parameter(Mandatory = $true)]
    [string]$OutputArchive,

    [Parameter(Mandatory = $true)]
    [string]$MetadataOut
)

$ErrorActionPreference = "Stop"

function Test-IsLink($item)
{
    return (($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0 -or
        -not [string]::IsNullOrWhiteSpace([string]$item.LinkType))
}

function Assert-SafeDirectoryLinks([IO.DirectoryInfo]$directory)
{
    if (Test-IsLink $directory)
    {
        throw "Device performance app root cannot be a symbolic link or reparse point: $($directory.FullName)"
    }

    $root = [IO.Path]::GetFullPath($directory.FullName)
    $rootPrefix = $root + [IO.Path]::DirectorySeparatorChar
    $pathComparison = if ($IsWindows) { [StringComparison]::OrdinalIgnoreCase } else { [StringComparison]::Ordinal }
    foreach ($entry in Get-ChildItem -LiteralPath $root -Force -Recurse)
    {
        if (-not (Test-IsLink $entry))
        {
            continue
        }

        try
        {
            $target = $entry.ResolveLinkTarget($true)
        }
        catch
        {
            throw "Device performance app contains an invalid symbolic link or reparse point: $($entry.FullName)"
        }

        if ($null -eq $target)
        {
            throw "Device performance app contains an unresolved symbolic link or reparse point: $($entry.FullName)"
        }

        $targetPath = [IO.Path]::GetFullPath($target.FullName)
        if (-not [string]::Equals($targetPath, $root, $pathComparison) -and
            -not $targetPath.StartsWith($rootPrefix, $pathComparison))
        {
            throw "Device performance app contains a symbolic link or reparse point outside its artifact tree: $($entry.FullName)"
        }
    }
}

function Find-ControlsDeviceTestApp([string]$root, [string]$variant) {
    if (-not (Test-Path $root))
    {
        throw "$variant artifact directory does not exist: $root"
    }

    if ($Platform -eq "android")
    {
        $matches = @(
            Get-ChildItem $root -File -Recurse -Filter "*-Signed.apk" |
                Where-Object { $_.FullName -match 'Controls\.DeviceTests' }
        )
        if ($matches.Count -eq 0)
        {
            $matches = @(
                Get-ChildItem $root -File -Recurse -Filter "*.apk" |
                    Where-Object { $_.FullName -match 'Controls\.DeviceTests' }
            )
        }
    }
    elseif ($Platform -eq "windows")
    {
        $executables = @(
            Get-ChildItem $root -File -Recurse -Filter "Microsoft.Maui.Controls.DeviceTests.exe" |
                Where-Object {
                    $_.FullName -match '[\\/]Release[\\/]' -and
                    $_.Directory.Name -eq "publish"
                }
        )
        $matches = @($executables | ForEach-Object { $_.Directory } | Sort-Object FullName -Unique)
    }
    else
    {
        $platformPattern = if ($Platform -eq "ios") { '-ios' } else { '-maccatalyst' }
        $matches = @(
            Get-ChildItem $root -Directory -Recurse -Filter "*.app" |
                Where-Object {
                    $_.FullName -match 'Controls\.DeviceTests' -and
                    $_.FullName -match $platformPattern -and
                    $_.FullName -match '[\\/]Release[\\/]'
                }
        )
    }

    if ($matches.Count -ne 1)
    {
        $found = if ($matches.Count -eq 0) { "<none>" } else { $matches.FullName -join [Environment]::NewLine }
        throw "Expected exactly one $variant Controls.DeviceTests $Platform app, found $($matches.Count):$([Environment]::NewLine)$found"
    }

    return $matches[0]
}

function Copy-App($app, [string]$destinationDirectory) {
    New-Item -ItemType Directory -Force -Path $destinationDirectory | Out-Null
    $destination = Join-Path $destinationDirectory $app.Name

    if ($app -is [IO.DirectoryInfo])
    {
        Assert-SafeDirectoryLinks $app
        Copy-Item $app.FullName -Destination $destination -Recurse -Force
    }
    else
    {
        if (Test-IsLink $app)
        {
            throw "Device performance app cannot be a symbolic link or reparse point: $($app.FullName)"
        }
        Copy-Item $app.FullName -Destination $destination -Force
    }

    return $destination
}

function Read-BuildMetadata(
    [string]$root,
    [string]$expectedVariant,
    [string]$expectedCommitSha
) {
    $matches = @(Get-ChildItem $root -File -Recurse -Filter "device-performance-build-metadata.json")
    if ($matches.Count -ne 1)
    {
        throw "Expected exactly one $expectedVariant build metadata file, found $($matches.Count)."
    }
    if (Test-IsLink $matches[0])
    {
        throw "$expectedVariant build metadata cannot be a symbolic link or reparse point."
    }

    $metadata = Get-Content $matches[0].FullName -Raw | ConvertFrom-Json
    $expectedValues = [ordered]@{
        schemaVersion = 1
        repository = $Repository
        pullRequestNumber = $PullRequestNumber
        variant = $expectedVariant
        platform = $Platform
        commitSha = $expectedCommitSha
        harnessSha = $HarnessSha
    }

    foreach ($entry in $expectedValues.GetEnumerator())
    {
        if ([string]$metadata.($entry.Key) -ne [string]$entry.Value)
        {
            throw "$expectedVariant build metadata $($entry.Key) '$($metadata.($entry.Key))' does not match expected '$($entry.Value)'."
        }
    }

    $expectedRuntimeVariant = if ($Platform -eq "windows") { "coreclr" } else { "mono" }
    if ([string]$metadata.runtimeVariant -ne $expectedRuntimeVariant)
    {
        throw "$expectedVariant build metadata runtimeVariant '$($metadata.runtimeVariant)' does not match expected '$expectedRuntimeVariant'."
    }
    if ([string]$metadata.sdkVersion -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?$')
    {
        throw "$expectedVariant build metadata contains an invalid SDK version."
    }

    return $metadata
}

$baseBuildMetadata = Read-BuildMetadata $BaseArtifacts "base" $BaseCommitSha
$headBuildMetadata = Read-BuildMetadata $HeadArtifacts "head" $HeadCommitSha
$baseApp = Find-ControlsDeviceTestApp $BaseArtifacts "base"
$headApp = Find-ControlsDeviceTestApp $HeadArtifacts "head"
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-payload-" + [Guid]::NewGuid().ToString("N"))
$payloadRoot = Join-Path $temporaryRoot "payload"

try
{
    $baseDestination = Copy-App $baseApp (Join-Path $payloadRoot "base")
    $headDestination = Copy-App $headApp (Join-Path $payloadRoot "head")

    $archiveDirectory = Split-Path -Parent $OutputArchive
    if ($archiveDirectory -and -not (Test-Path $archiveDirectory))
    {
        New-Item -ItemType Directory -Force -Path $archiveDirectory | Out-Null
    }
    Remove-Item $OutputArchive -Force -ErrorAction SilentlyContinue

    if ($IsMacOS -and $Platform -ne "android")
    {
        & /usr/bin/ditto -c -k --sequesterRsrc --keepParent $payloadRoot $OutputArchive
        if ($LASTEXITCODE -ne 0)
        {
            throw "ditto failed to create the Apple performance payload."
        }
    }
    else
    {
        Compress-Archive -Path $payloadRoot -DestinationPath $OutputArchive -Force
    }

    $baseRelativePath = if ($Platform -eq "windows") {
        "payload/base/publish/Microsoft.Maui.Controls.DeviceTests.exe"
    } else {
        "payload/base/$($baseApp.Name)"
    }
    $headRelativePath = if ($Platform -eq "windows") {
        "payload/head/publish/Microsoft.Maui.Controls.DeviceTests.exe"
    } else {
        "payload/head/$($headApp.Name)"
    }
    $metadata = [PSCustomObject]@{
        schemaVersion = 2
        repository = $Repository
        pullRequestNumber = $PullRequestNumber
        pullRequestAuthor = $PullRequestAuthor
        platform = $Platform
        expectedScenario = $ExpectedScenario
        baseCommitSha = $BaseCommitSha
        headCommitSha = $HeadCommitSha
        harnessSha = $HarnessSha
        azdoBuildId = $AzdoBuildId
        azdoBuildUrl = $AzdoBuildUrl
        baseRuntimeVariant = $baseBuildMetadata.runtimeVariant
        headRuntimeVariant = $headBuildMetadata.runtimeVariant
        baseSdkVersion = $baseBuildMetadata.sdkVersion
        headSdkVersion = $headBuildMetadata.sdkVersion
        expectedVariantRuns = 2
        baseAppRelativePath = $baseRelativePath
        headAppRelativePath = $headRelativePath
        archive = (Resolve-Path $OutputArchive).Path
    }

    $metadataDirectory = Split-Path -Parent $MetadataOut
    if ($metadataDirectory -and -not (Test-Path $metadataDirectory))
    {
        New-Item -ItemType Directory -Force -Path $metadataDirectory | Out-Null
    }
    ConvertTo-Json -InputObject $metadata -Depth 6 |
        Set-Content -Path $MetadataOut -Encoding UTF8

    Write-Host "Created device performance payload: $OutputArchive"
    Write-Host "Base app: $baseRelativePath"
    Write-Host "Head app: $headRelativePath"
    exit 0
}
finally
{
    Remove-Item $temporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
}
