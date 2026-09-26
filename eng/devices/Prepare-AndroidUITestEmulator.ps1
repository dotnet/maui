[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$AdbPath,

    [Parameter(Mandatory)]
    [ValidatePattern('^emulator-[0-9]+$')]
    [string]$Serial,

    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string]$AvdName
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-Adb {
    param([string[]]$AdbArguments)

    $output = @(& $AdbPath -s $Serial @AdbArguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw "adb -s $Serial $($AdbArguments -join ' ') failed with exit code ${LASTEXITCODE}: $($output -join [Environment]::NewLine)"
    }
    return $output
}

$name = @(Invoke-Adb -AdbArguments @('emu', 'avd', 'name'))
if ($name.Count -ne 2 -or $name[0] -cne $AvdName -or $name[1] -cne 'OK') {
    throw "Refusing to configure $Serial. Expected AVD '$AvdName', received '$($name -join ', ')'."
}

$packages = @(Invoke-Adb -AdbArguments @('shell', 'pm', 'list', 'packages', '--user', '0'))
if ($packages.Count -eq 0 -or @($packages | Where-Object { $_ -notlike 'package:*' }).Count -ne 0) {
    throw "Could not read installed packages on ${Serial}: $($packages -join [Environment]::NewLine)"
}

$searchPackage = 'com.google.android.googlequicksearchbox'
if ($packages -cnotcontains "package:$searchPackage") {
    Write-Host "Google Search is not installed on $Serial; no autostart suppression is needed."
    return
}

# Search can open a Play Services update over HostApp; leave Play Services, Play Store and Maps intact.
$disabled = @(Invoke-Adb -AdbArguments @('shell', 'pm', 'disable-user', '--user', '0', $searchPackage))
if ($disabled -cnotcontains "Package $searchPackage new state: disabled-user") {
    throw "Google Search disable-user did not confirm success on ${Serial}: $($disabled -join [Environment]::NewLine)"
}

$disabledPackages = @(Invoke-Adb -AdbArguments @('shell', 'pm', 'list', 'packages', '-d', '--user', '0', $searchPackage))
if ($disabledPackages.Count -ne 1 -or $disabledPackages[0] -cne "package:$searchPackage") {
    throw "Google Search is still enabled on $Serial after disable-user: $($disabledPackages -join [Environment]::NewLine)"
}

Write-Host "Google Search autostart is disabled for user 0 on $Serial ($AvdName)."
