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
    [string] $appiumVersion = '2.0.0-beta.64',
    [string] $windowsDriverVersion = '2.7.2',
    [string] $androidDriverVersion = '2.25.1',
    [string] $iOSDriverVersion = '4.30.2',
    [string] $macDriverVersion = '1.6.1'
)

node -v
npm install -g npm
node -v
npm install -g appium@$appiumVersion
appium -v
$x = appium driver list --installed --json  | ConvertFrom-Json

if(!$x.windows)
{
    appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
}
else
{
    appium driver uninstall windows
    appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
}

if(!$x.uiautomator2)
{
    appium driver install uiautomator2@$androidDriverVersion
}
else
{
    appium driver uninstall uiautomator2
    appium driver install uiautomator2@$androidDriverVersion
}

if(!$x.xcuitest)
{
    appium driver install xcuitest@$iOSDriverVersion
}
else
{
    appium driver uninstall xcuitest
    appium driver install xcuitest@$iOSDriverVersion
}

if(!$x.mac2)
{
    appium driver install mac2@$macDriverVersion
}
else
{
    appium driver uninstall mac2
    appium driver install mac2@$macDriverVersion
}