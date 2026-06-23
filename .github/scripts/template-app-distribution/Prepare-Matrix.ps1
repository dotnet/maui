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

$variantDefinitions = [ordered]@{
    blank = [ordered]@{
        displayName = "MAUI Template"
        projectName = "MauiTemplateBlank"
        template = "maui"
        templateArgs = @()
        androidApplicationId = Get-EnvironmentOrDefault "TEMPLATE_APP_BLANK_ANDROID_APPLICATION_ID" "com.microsoft.maui.template.blank"
        iosBundleId = Get-EnvironmentOrDefault "TEMPLATE_APP_BLANK_IOS_BUNDLE_ID" "com.microsoft.maui.template.blank"
    }
    sample = [ordered]@{
        displayName = "MAUI Template Sample"
        projectName = "MauiTemplateSample"
        template = "maui"
        templateArgs = @("--sample-content")
        androidApplicationId = Get-EnvironmentOrDefault "TEMPLATE_APP_SAMPLE_ANDROID_APPLICATION_ID" "com.microsoft.maui.template.sample"
        iosBundleId = Get-EnvironmentOrDefault "TEMPLATE_APP_SAMPLE_IOS_BUNDLE_ID" "com.microsoft.maui.template.sample"
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
        runner = "ubuntu-latest"
        workload = "maui-android"
        targetFramework = "$DotNetTfm-android"
        runtimeIdentifier = "android-arm64"
    }
    ios = [ordered]@{
        runner = "macos-latest"
        workload = "maui-ios"
        targetFramework = "$DotNetTfm-ios"
        runtimeIdentifier = "ios-arm64"
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
        $applicationId = if ($platformName -eq "ios") { $variant.iosBundleId } else { $variant.androidApplicationId }

        if ([string]::IsNullOrWhiteSpace($applicationId)) {
            throw "Variant '$variantName' does not define an application identifier for '$platformName'."
        }

        $templateArgs = @(ConvertTo-StringArray $variant.templateArgs)
        $templateArgsJson = if ($templateArgs.Count -eq 0) {
            "[]"
        } else {
            ConvertTo-Json -InputObject $templateArgs -Compress
        }

        $matrix.include += [ordered]@{
            variant = $variantName
            platform = $platformName
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
