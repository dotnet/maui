<#
.SYNOPSIS

Install dependencies for Appium UITests

.DESCRIPTION

This will install or update npm, appium and the following drivers: appium-windows-driver, uiautomator2, xcuitest and mac2

.PARAMETER appiumVersion

The Appium version to install

.PARAMETER windowsDriverVersion

The windows driver version to update or install

.PARAMETER androidDriverVersion

The uiautomator2 driver version to update or install

.PARAMETER iOSDriverVersion

The xcuitest driver version to update or install

.PARAMETER macDriverVersion

The mac2 driver version to update or install


.EXAMPLE

PS> .\appium-install.ps1 '2.0.0-beta.61' 2.7.2 2.25.1 4.30.2 1.6.1

This would install or update Appium version 2.0.0-beta.61, the windows driver 2.7.2, the uiautomator2 driver with 2.25.1, the xcuitest driver with 4.30.2 and mac2 driver with 1.6.1
#>


param
(
    [string] $appiumVersion = '2.1.0',
    [string] $windowsDriverVersion = '2.10.1',
    [string] $androidDriverVersion = '2.29.4',
    [string] $iOSDriverVersion = '4.34.0',
    [string] $macDriverVersion = '1.7.2'
)

Write-Output  "Welcome to the Appium installer"
node -v

Write-Output  "Updating npm"
npm install -g npm

Write-Output  "Node version"
node -v

Write-Output  "Installing appium $appiumVersion"
npm install -g appium@$appiumVersion
write-Output  "Installed appium"
appium -v

$existingDrivers = appium driver list --installed --json  | ConvertFrom-Json
Write-Output "List of installed drivers $x"
if ($existingDrivers.windows) {
    Write-Output  "Uninstalling appium driver windows"
    appium driver uninstall windows
    Write-Output  "Unistalled appium driver windows"
}

if ($existingDrivers.uiautomator2) {
    Write-Output  "Uninstalling appium driver uiautomator2"
    appium driver uninstall uiautomator2
    Write-Output  "Unistalled appium driver uiautomator2"
}

if ($existingDrivers.xcuitest) {
    Write-Output  "Uninstalling appium driver xcuitest"
    appium driver uninstall xcuitest
    Write-Output  "Unistalled appium driver xcuitest"
}

if ($existingDrivers.mac2) {
    Write-Output  "Uninstalling appium driver mac2"
    appium driver uninstall mac2
    Write-Output  "Unistalled appium driver mac2"
}

$drivers = appium driver list --installed --json  | ConvertFrom-Json
Write-Output "List of installed drivers $drivers"

Write-Output  "Going to install appium drivers windows $windowsDriverVersion, uiautomator2 $androidDriverVersion, xcuitest $iOSDriverVersion and mac2 $macDriverVersio"

Write-Output  "Installing appium driver windows $windowsDriverVersion"
appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
Write-Output  "Installed appium driver windows"

Write-Output  "Installing appium driver uiautomator2 $androidDriverVersion"
appium driver install uiautomator2@$androidDriverVersion
Write-Output  "Installed appium driver uiautomator2"

Write-Output  "Installing appium driver xcuitest $iOSDriverVersion"
appium driver install xcuitest@$iOSDriverVersion
Write-Output  "Installed appium driver xcuitest"

Write-Output  "Installing appium driver mac2 $macDriverVersion"
appium driver install mac2@$macDriverVersion
Write-Output  "Installed appium driver mac2"

Write-Output  "Done, thanks!"