<#
.SYNOPSIS

Updates the Versions.props variables.

.DESCRIPTION

This will update the Android and iOS versions .

.PARAMETER xmlFileName

Path to the Versions.props file

.PARAMETER androidVersion

The Android version to update on the file

.PARAMETER iOSVersion

The iOS version to update on the file

.PARAMETER tvOSVersion

The tvOS version to update on the file

.PARAMETER macCatalystVersion

The MacCatalyst version to update on the file

.PARAMETER macVersion

The Mac version to update on the file

.

.EXAMPLE

PS> .\update-version-props.ps1 ~\dotnet\maui\eng\Versions.props 35.0.0 17.0.00 17.0.00 17.0.00 17.0.00

This would update Android with version 35.0.0 and iOS, tvOS, MacCatalyst and Mac with 17.0.00
#>

param
(
    [string] $xmlFileName,
    [string] $androidVersion,
    [string] $iOSVersion,
    [string] $tvOSVersion,
    [string] $macCatalystVersion,
    [string] $macVersion
)

function Set-SDKVersions {
    param(
        [ValidateNotNullOrEmpty()]
        [xml]
        $XmlDoc,
        [ValidateNotNullOrEmpty()]
        [string]
        $Node,

        [ValidateNotNullOrEmpty()]
        [string]
        $Version
    )

}

#Update git config
$gitConfig = git config –global core.autocrlf false
Write-Output $gitConfig

function Set-SDKVersion {
    param(
        [ValidateNotNullOrEmpty()]
        [xml]
        $XmlDoc,
        [ValidateNotNullOrEmpty()]
        [string]
        $NodePath,

        [ValidateNotNullOrEmpty()]
        [string]
        $Version
    )
    $node = $XmlDoc.SelectNodes($NodePath)[0]

    if (-not $node) {
        Write-Error "Could not find node for $NodePath"
        return $false
    }
    $node."#text" = $Version
    return $true
}

$sdkMaps = @{
    "MicrosoftAndroidSdkWindowsPackageVersion" = $androidVersion;
    "MicrosoftiOSSdkPackageVersion" = $iOSVersion;
    "MicrosofttvOSSdkPackageVersion" = $tvOSVersion;
    "MicrosoftMacCatalystSdkPackageVersion" = $macCatalystVersion;
    "MicrosoftmacOSSdkPackageVersion" = $macVersion;
}

# Read the existing file
[xml]$xmlDoc = Get-Content $xmlFileName

foreach ($sdkMap in $sdkMaps.GetEnumerator()) {
    $node = $xmlDoc.SelectNodes("//Project//PropertyGroup//$($sdkMap.Key)")[0]
    if (-not $node) {
        Write-Error "Could not find node for $($sdkMap.Key)"
        exit 1
    } else {
        $node."#text" = $sdkMap.Value
    }
}

Write-Output("New Android version: " + $xmlDoc.Project.PropertyGroup.MicrosoftAndroidSdkWindowsPackageVersion)
Write-Output("New iOS version: " + $xmlDoc.Project.PropertyGroup.MicrosoftiOSSdkPackageVersion)
Write-Output("New tvOS version: " + $xmlDoc.Project.PropertyGroup.MicrosofttvOSSdkPackageVersion)
Write-Output("New MacCatalyst version: " + $xmlDoc.Project.PropertyGroup.MicrosoftMacCatalystSdkPackageVersion)
Write-Output("New Mac version: " + $xmlDoc.Project.PropertyGroup.MicrosoftmacOSSdkPackageVersion)

$xmlDoc.Save($xmlFileName)
