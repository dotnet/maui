#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$Variants,

    [Parameter(Mandatory)]
    [string]$Platforms,

    [Parameter(Mandatory)]
    [string]$DotNetTfm
)

$ErrorActionPreference = "Stop"

function Split-InputList([string]$Value) {
    return @($Value.Split(',', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim().ToLowerInvariant() })
}

function Get-EnvironmentOrDefault([string]$Name, [string]$DefaultValue) {
    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $DefaultValue
    }

    return $value
}

function Get-DefaultIdentifierPrefix {
    $prefix = [Environment]::GetEnvironmentVariable("TEMPLATE_APP_IDENTIFIER_PREFIX")
    if (-not [string]::IsNullOrWhiteSpace($prefix)) {
        return $prefix.Trim().TrimEnd(".").ToLowerInvariant()
    }

    $owner = [Environment]::GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER")
    if ([string]::IsNullOrWhiteSpace($owner)) {
        $owner = "maui"
    }

    $ownerSegment = $owner.ToLowerInvariant() -replace "[^a-z0-9]+", ""
    if ([string]::IsNullOrWhiteSpace($ownerSegment)) {
        $ownerSegment = "maui"
    }

    return "com.$ownerSegment.maui.template"
}

function ConvertTo-StringArray($Value) {
    if ($null -eq $Value) {
        return @()
    }

    if ($Value -is [array]) {
        return @($Value | ForEach-Object { [string]$_ })
    }

    return @([string]$Value)
}

function Merge-VariantDefinition($Definitions, [string]$Name, $Definition) {
    if (-not $Definitions.Contains($Name)) {
        $Definitions[$Name] = [ordered]@{}
    }

    foreach ($property in $Definition.PSObject.Properties) {
        $Definitions[$Name][$property.Name] = $property.Value
    }
}

$identifierPrefix = Get-DefaultIdentifierPrefix
$blankDefaultIdentifier = "$identifierPrefix.blank"
$blankIosBundleId = Get-EnvironmentOrDefault "TEMPLATE_APP_BLANK_IOS_BUNDLE_ID" $blankDefaultIdentifier
$blankMacCatalystBundleId = Get-EnvironmentOrDefault "TEMPLATE_APP_BLANK_MACCATALYST_BUNDLE_ID" $blankIosBundleId
$sampleDefaultIdentifier = "$identifierPrefix.sample"
$sampleIosBundleId = Get-EnvironmentOrDefault "TEMPLATE_APP_SAMPLE_IOS_BUNDLE_ID" $sampleDefaultIdentifier
$sampleMacCatalystBundleId = Get-EnvironmentOrDefault "TEMPLATE_APP_SAMPLE_MACCATALYST_BUNDLE_ID" $sampleIosBundleId

$variantDefinitions = [ordered]@{
    blank = [ordered]@{
        displayName = "MAUI Template"
        projectName = "MauiTemplateBlank"
        template = "maui"
        templateArgs = @()
        androidApplicationId = Get-EnvironmentOrDefault "TEMPLATE_APP_BLANK_ANDROID_APPLICATION_ID" $blankDefaultIdentifier
        iosBundleId = $blankIosBundleId
        maccatalystBundleId = $blankMacCatalystBundleId
        windowsApplicationId = Get-EnvironmentOrDefault "TEMPLATE_APP_BLANK_WINDOWS_APPLICATION_ID" $blankDefaultIdentifier
    }
    sample = [ordered]@{
        displayName = "MAUI Template Sample"
        projectName = "MauiTemplateSample"
        template = "maui"
        templateArgs = @("--sample-content")
        androidApplicationId = Get-EnvironmentOrDefault "TEMPLATE_APP_SAMPLE_ANDROID_APPLICATION_ID" $sampleDefaultIdentifier
        iosBundleId = $sampleIosBundleId
        maccatalystBundleId = $sampleMacCatalystBundleId
        windowsApplicationId = Get-EnvironmentOrDefault "TEMPLATE_APP_SAMPLE_WINDOWS_APPLICATION_ID" $sampleDefaultIdentifier
    }
}

if (-not [string]::IsNullOrWhiteSpace($env:TEMPLATE_APP_VARIANTS_JSON)) {
    $customDefinitions = $env:TEMPLATE_APP_VARIANTS_JSON | ConvertFrom-Json
    foreach ($property in $customDefinitions.PSObject.Properties) {
        Merge-VariantDefinition $variantDefinitions $property.Name.ToLowerInvariant() $property.Value
    }
}

$platformDefinitions = [ordered]@{
    android = [ordered]@{
        artifactPlatform = "android"
        runner = "ubuntu-latest"
        workload = "maui-android"
        targetFramework = "$DotNetTfm-android"
        runtimeIdentifier = "android-arm64"
    }
    ios = [ordered]@{
        artifactPlatform = "ios"
        runner = "macos-15"
        workload = "maui-ios"
        targetFramework = "$DotNetTfm-ios"
        runtimeIdentifier = "ios-arm64"
    }
    maccatalyst = [ordered]@{
        artifactPlatform = "macos"
        runner = "macos-15"
        workload = "maui-maccatalyst"
        targetFramework = "$DotNetTfm-maccatalyst"
        runtimeIdentifier = ""
    }
    windows = [ordered]@{
        artifactPlatform = "windows"
        runner = "windows-latest"
        workload = "maui-windows"
        targetFramework = "$DotNetTfm-windows10.0.19041.0"
        runtimeIdentifier = "win-x64"
    }
}

$selectedVariants = Split-InputList $Variants
if ($selectedVariants.Count -eq 0 -or $selectedVariants -contains "all") {
    $selectedVariants = @($variantDefinitions.Keys)
}

$selectedPlatforms = Split-InputList $Platforms
if ($selectedPlatforms.Count -eq 0 -or $selectedPlatforms -contains "all") {
    $selectedPlatforms = @($platformDefinitions.Keys)
}

$matrix = [ordered]@{
    include = @()
}

foreach ($variantName in $selectedVariants) {
    if (-not $variantDefinitions.Contains($variantName)) {
        throw "Unknown template app variant '$variantName'. Known variants: $($variantDefinitions.Keys -join ', ')"
    }

    $variant = $variantDefinitions[$variantName]

    foreach ($platformName in $selectedPlatforms) {
        if (-not $platformDefinitions.Contains($platformName)) {
            throw "Unknown platform '$platformName'. Known platforms: $($platformDefinitions.Keys -join ', ')"
        }

        $platform = $platformDefinitions[$platformName]
        $applicationId = switch ($platformName) {
            "ios" { $variant.iosBundleId }
            "maccatalyst" {
                if ([string]::IsNullOrWhiteSpace($variant.maccatalystBundleId)) {
                    $variant.iosBundleId
                } else {
                    $variant.maccatalystBundleId
                }
            }
            "windows" {
                if ([string]::IsNullOrWhiteSpace($variant.windowsApplicationId)) {
                    $variant.androidApplicationId
                } else {
                    $variant.windowsApplicationId
                }
            }
            default { $variant.androidApplicationId }
        }

        if ([string]::IsNullOrWhiteSpace($applicationId)) {
            throw "Variant '$variantName' does not define an application identifier for '$platformName'."
        }

        $templateArgs = @(ConvertTo-StringArray $variant.templateArgs)
        $maccatalystBundleId = if ([string]::IsNullOrWhiteSpace($variant.maccatalystBundleId)) {
            $variant.iosBundleId
        } else {
            $variant.maccatalystBundleId
        }
        $windowsApplicationId = if ([string]::IsNullOrWhiteSpace($variant.windowsApplicationId)) {
            $variant.androidApplicationId
        } else {
            $variant.windowsApplicationId
        }
        $templateArgsJson = if ($templateArgs.Count -eq 0) {
            "[]"
        } else {
            ConvertTo-Json -InputObject $templateArgs -Compress
        }

        $matrix.include += [ordered]@{
            variant = $variantName
            platform = $platformName
            artifactPlatform = $platform.artifactPlatform
            runner = $platform.runner
            workload = $platform.workload
            targetFramework = $platform.targetFramework
            runtimeIdentifier = $platform.runtimeIdentifier
            displayName = [string]$variant.displayName
            projectName = [string]$variant.projectName
            template = [string]$variant.template
            templateArgsJson = $templateArgsJson
            applicationId = [string]$applicationId
            androidApplicationId = [string]$variant.androidApplicationId
            iosBundleId = [string]$variant.iosBundleId
            maccatalystBundleId = [string]$maccatalystBundleId
            windowsApplicationId = [string]$windowsApplicationId
        }
    }
}

if ($matrix.include.Count -eq 0) {
    throw "The selected variants/platforms produced an empty build matrix."
}

$matrixJson = $matrix | ConvertTo-Json -Compress -Depth 10
Write-Host "Matrix: $matrixJson"

if ($env:GITHUB_OUTPUT) {
    "matrix=$matrixJson" >> $env:GITHUB_OUTPUT
}
