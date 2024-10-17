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

#Update git config
Write-Output git config –global core.autocrlf false

# Read the existing file
[xml]$xmlDoc = Get-Content $xmlFileName

if ($androidVersion)
{
    $androidVersionNode = $xmlDoc.SelectNodes("//Project//PropertyGroup//MicrosoftAndroidSdkWindowsPackageVersion")[0]
    $androidVersionNode."#text" = $androidVersion
}

if ($iOSVersion)
{
    $iosVersionNode = $xmlDoc.SelectNodes("//Project//PropertyGroup//MicrosoftiOSSdkPackageVersion")[0]
    $iosVersionNode."#text" = $iOSVersion
}

if ($tvOSVersion)
{
    $tvOSVersionNode = $xmlDoc.SelectNodes("//Project//PropertyGroup//MicrosofttvOSSdkPackageVersion")[0]
    $tvOSVersionNode."#text" = $tvOSVersion
}

if ($macCatalystVersion)
{
    $macCatalystVersionNode = $xmlDoc.SelectNodes("//Project//PropertyGroup//MicrosoftMacCatalystSdkPackageVersion")[0]
    $macCatalystVersionNode."#text" = $macCatalystVersion
}

if ($macVersion)
{
    $macVersionNode = $xmlDoc.SelectNodes("//Project//PropertyGroup//MicrosoftmacOSSdkPackageVersion")[0]
    $macVersionNode."#text" = $macVersion
}


Write-Output("Android version: " + $xmlDoc.Project.PropertyGroup.MicrosoftAndroidSdkWindowsPackageVersion)
Write-Output("iOS version: " + $xmlDoc.Project.PropertyGroup.MicrosoftiOSSdkPackageVersion)
Write-Output("tvOS version: " + $xmlDoc.Project.PropertyGroup.MicrosofttvOSSdkPackageVersion)
Write-Output("MacCatalyst version: " + $xmlDoc.Project.PropertyGroup.MicrosoftMacCatalystSdkPackageVersion)
Write-Output("Mac version: " + $xmlDoc.Project.PropertyGroup.MicrosoftmacOSSdkPackageVersion)

$xmlDoc.Save($xmlFileName)