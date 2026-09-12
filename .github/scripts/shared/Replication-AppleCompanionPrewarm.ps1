#!/usr/bin/env pwsh

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:ReplicationAppleCompanionProjectRelativePath =
'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
$script:ReplicationAppleCompanionAssetsRelativePath =
'artifacts/obj/Controls.DeviceTests/project.assets.json'
$script:ReplicationAppleCompanionEntitlementsRelativePath =
'source-overrides/ReplicationMacCatalystControlsDeviceTests.entitlements'
$script:ReplicationAppleCompanionTargetFrameworks = @(
    'net10.0-ios'
    'net10.0-maccatalyst'
)
$script:ReplicationAppleCompanionRuntimeIdentifiers = [ordered]@{
    'net10.0-ios' = 'iossimulator-arm64'
    'net10.0-maccatalyst' = 'maccatalyst-arm64'
}

function Assert-ReplicationAppleCompanionNoLinkPath {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$Description,
        [switch]$Leaf
    )

    $fullRoot = [IO.Path]::GetFullPath($Root).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $fullPath = [IO.Path]::GetFullPath($Path)
    $prefix = "$fullRoot$([IO.Path]::DirectorySeparatorChar)"
    if ($fullPath -cne $fullRoot -and
        -not $fullPath.StartsWith($prefix, [StringComparison]::Ordinal)) {
        throw "$Description escapes its fixed root."
    }
    if (-not (Test-Path -LiteralPath $fullRoot -PathType Container)) {
        throw "$Description requires an existing root directory."
    }

    $current = $fullPath
    while ($true) {
        if (-not (Test-Path -LiteralPath $current)) {
            throw "$Description does not exist."
        }
        $item = Get-Item -LiteralPath $current -Force -ErrorAction Stop
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "$Description must be a regular no-link path."
        }
        if ($current -ceq $fullRoot) {
            break
        }
        $parent = Split-Path -Parent $current
        if ([string]::IsNullOrEmpty($parent) -or $parent -ceq $current) {
            throw "$Description escapes its fixed root."
        }
        $current = $parent
    }

    $target = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
    if ($Leaf -and $target.PSIsContainer) {
        throw "$Description must be a bounded regular no-link file."
    }
    if (-not $Leaf -and -not $target.PSIsContainer) {
        throw "$Description must be a regular no-link directory."
    }
    return $target.FullName
}

function Get-ReplicationAppleCompanionPrewarmPlan {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$TrustedRoot,
        [Parameter(Mandatory = $true)][string]$PackagesPath
    )

    $repositoryRoot = Assert-ReplicationAppleCompanionNoLinkPath `
        -Path $RepositoryRoot `
        -Root $RepositoryRoot `
        -Description 'Apple companion repository root'
    $trustedRoot = Assert-ReplicationAppleCompanionNoLinkPath `
        -Path $TrustedRoot `
        -Root $TrustedRoot `
        -Description 'Apple companion trusted root'
    $projectPath = Assert-ReplicationAppleCompanionNoLinkPath `
        -Path (Join-Path $repositoryRoot (
            $script:ReplicationAppleCompanionProjectRelativePath)) `
        -Root $repositoryRoot `
        -Description 'Apple companion Controls.DeviceTests project' `
        -Leaf
    $entitlementsPath = Assert-ReplicationAppleCompanionNoLinkPath `
        -Path (Join-Path $trustedRoot (
            $script:ReplicationAppleCompanionEntitlementsRelativePath)) `
        -Root $trustedRoot `
        -Description 'Apple companion Catalyst entitlements' `
        -Leaf
    if ([string]::IsNullOrWhiteSpace($PackagesPath)) {
        throw 'Apple companion packages path is required.'
    }
    $packagesPath = [IO.Path]::GetFullPath($PackagesPath)

    $platformProperties = @(
        '-p:IncludeIosTargetFrameworks=true'
        '-p:IncludeMacCatalystTargetFrameworks=true'
        '-p:IncludeAndroidTargetFrameworks=false'
        '-p:IncludeWindowsTargetFrameworks=false'
        '-p:IncludeMacOSTargetFrameworks=false'
        '-p:IncludeTizenTargetFrameworks=false'
    )
    $restoreArguments = @(
        'restore'
        $projectPath
        '--packages'
        $packagesPath
        '--force-evaluate'
    ) + $platformProperties
    $commonBuildArguments = @(
        'build'
        $projectPath
        '--configuration'
        'Debug'
        '--no-restore'
        "-p:RestorePackagesPath=$packagesPath"
    ) + $platformProperties
    $iosArguments = $commonBuildArguments + @(
        '--framework'
        'net10.0-ios'
        '--runtime'
        'iossimulator-arm64'
        '-p:CodesignRequireProvisioningProfile=false'
    )
    $catalystArguments = $commonBuildArguments + @(
        '--framework'
        'net10.0-maccatalyst'
        '--runtime'
        'maccatalyst-arm64'
        '-p:CodesignRequireProvisioningProfile=false'
        "-p:CodesignEntitlements=$entitlementsPath"
        '-p:CodesignKey=-'
        '-p:MtouchDebug=false'
        '-p:UseSystemResourceKeys=false'
    )

    return [pscustomobject]@{
        ProjectPath = $projectPath
        AssetsPath = Join-Path $repositoryRoot (
            $script:ReplicationAppleCompanionAssetsRelativePath)
        EntitlementsPath = $entitlementsPath
        TargetFrameworks = @($script:ReplicationAppleCompanionTargetFrameworks)
        TargetPairs = @(
            'net10.0-ios/iossimulator-arm64'
            'net10.0-maccatalyst/maccatalyst-arm64'
        )
        Commands = @(
            [pscustomobject]@{
                Name = 'restore-dual-apple-graph'
                LogName = 'prewarm-restore-dual-apple-graph.log'
                Kind = 'restore'
                TargetFramework = $null
                RuntimeIdentifier = $null
                TimeoutSeconds = 600
                Arguments = $restoreArguments
            }
            [pscustomobject]@{
                Name = 'build-ios-simulator-no-restore'
                LogName = 'prewarm-build-ios-simulator-no-restore.log'
                Kind = 'build'
                TargetFramework = 'net10.0-ios'
                RuntimeIdentifier = 'iossimulator-arm64'
                TimeoutSeconds = 600
                Arguments = $iosArguments
            }
            [pscustomobject]@{
                Name = 'build-maccatalyst-no-restore'
                LogName = 'prewarm-build-maccatalyst-no-restore.log'
                Kind = 'build'
                TargetFramework = 'net10.0-maccatalyst'
                RuntimeIdentifier = 'maccatalyst-arm64'
                TimeoutSeconds = 600
                Arguments = $catalystArguments
            }
        )
    }
}

function Assert-ReplicationAppleCompanionAssets {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [string]$ExpectedSha256
    )

    $repositoryRoot = Assert-ReplicationAppleCompanionNoLinkPath `
        -Path $RepositoryRoot `
        -Root $RepositoryRoot `
        -Description 'Apple companion repository root'
    $assetsPath = Join-Path $repositoryRoot (
        $script:ReplicationAppleCompanionAssetsRelativePath)
    $assetsPath = Assert-ReplicationAppleCompanionNoLinkPath `
        -Path $assetsPath `
        -Root $repositoryRoot `
        -Description 'Apple companion project.assets.json' `
        -Leaf
    $item = Get-Item -LiteralPath $assetsPath -Force -ErrorAction Stop
    if ($item.Length -le 0 -or $item.Length -gt 16MB) {
        throw 'Apple companion project.assets.json must be a bounded regular no-link file.'
    }
    $sha256 = (Get-FileHash -LiteralPath $assetsPath -Algorithm SHA256).
    Hash.ToLowerInvariant()
    if (-not [string]::IsNullOrEmpty($ExpectedSha256) -and
        $ExpectedSha256 -cnotmatch '^[0-9a-f]{64}$') {
        throw 'Apple companion expected assets digest is malformed.'
    }
    if (-not [string]::IsNullOrEmpty($ExpectedSha256) -and
        $sha256 -cne $ExpectedSha256) {
        throw 'Apple companion project.assets.json changed after the graph restore.'
    }

    try {
        $assets = Get-Content -LiteralPath $assetsPath -Raw -ErrorAction Stop |
            ConvertFrom-Json -AsHashtable -Depth 100 -ErrorAction Stop
    } catch {
        throw 'Apple companion project.assets.json is not valid bounded JSON.'
    }
    if ($assets -isnot [Collections.IDictionary] -or
        -not $assets.Contains('targets') -or
        $assets['targets'] -isnot [Collections.IDictionary] -or
        -not $assets.Contains('project') -or
        $assets['project'] -isnot [Collections.IDictionary]) {
        throw 'Apple companion project.assets.json is missing its required object structure.'
    }
    $project = $assets['project']
    if (-not $project.Contains('restore') -or
        $project['restore'] -isnot [Collections.IDictionary] -or
        -not $project.Contains('frameworks') -or
        $project['frameworks'] -isnot [Collections.IDictionary]) {
        throw 'Apple companion project.assets.json is missing required framework declarations.'
    }
    $restoreMetadata = $project['restore']
    if (-not $restoreMetadata.Contains('originalTargetFrameworks') -or
        $restoreMetadata['originalTargetFrameworks'] -is [string] -or
        $restoreMetadata['originalTargetFrameworks'] -isnot [Collections.IEnumerable]) {
        throw 'Apple companion project.assets.json is missing required restore framework declarations.'
    }

    $targetKeys = @($assets['targets'].Keys)
    $originalFrameworks = @($restoreMetadata['originalTargetFrameworks'])
    $resolvedTargetPairs = [Collections.Generic.List[string]]::new()
    foreach ($framework in $script:ReplicationAppleCompanionTargetFrameworks) {
        if ($originalFrameworks -cnotcontains $framework) {
            throw "Apple companion assets are missing required original framework '$framework'."
        }
        $frameworkRecords = @($project['frameworks'].GetEnumerator() | Where-Object {
                $_.Value -is [Collections.IDictionary] -and
                $_.Value.Contains('targetAlias') -and
                [string]$_.Value['targetAlias'] -ceq $framework
            })
        if ($frameworkRecords.Count -ne 1) {
            throw "Apple companion assets are missing required framework alias '$framework'."
        }
        $frameworkKey = [string]$frameworkRecords[0].Key
        $frameworkPattern = if ($framework -ceq 'net10.0-ios') {
            '^net10\.0-ios(?:\d+\.\d+)?$'
        } else {
            '^net10\.0-maccatalyst(?:\d+\.\d+)?$'
        }
        if ($frameworkKey -cnotmatch $frameworkPattern) {
            throw "Apple companion assets contain an invalid framework key for '$framework'."
        }
        if ($targetKeys -cnotcontains $frameworkKey) {
            throw "Apple companion assets are missing required target '$frameworkKey'."
        }
        $targetPair = "$frameworkKey/$(
            $script:ReplicationAppleCompanionRuntimeIdentifiers[$framework])"
        if ($targetKeys -cnotcontains $targetPair) {
            throw "Apple companion assets are missing required target '$targetPair'."
        }
        $resolvedTargetPairs.Add($targetPair)
    }

    return [pscustomobject]@{
        AssetsSha256 = $sha256
        TargetPairs = @($resolvedTargetPairs)
        OriginalTargetFrameworks = @($script:ReplicationAppleCompanionTargetFrameworks)
        Frameworks = @($script:ReplicationAppleCompanionTargetFrameworks)
    }
}
