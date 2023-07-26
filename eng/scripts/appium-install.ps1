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
    [string] $appiumVersion = '2.0.1',
    [string] $windowsDriverVersion = '2.9.2',
    [string] $androidDriverVersion = '2.29.2',
    [string] $iOSDriverVersion = '4.32.23',
    [string] $macDriverVersion = '1.6.3'
)

echo "Welcome to the Appium installer"
node -v
echo "Updating npm"
npm install -g npm
echo "Node version"
node -v
echo "Check if appium is installed"
npm list -g | grep appium

$modulesFolder = "/Users/builder/azdo/_work/_tool/node/20.3.1/x64/lib/node_modules/"
if (Test-Path -Path $modulesFolder ) {
    echo "node_modules exists!"
    rm -rf $modulesFolder
} else {
    echo "node_modules doesn't exist."
}
echo "Installing appium $appiumVersion"
npm install -g appium@$appiumVersion
appium -v

echo "Installing appium drivers windows $windowsDriverVersion, uiautomator2 $androidDriverVersion, xcuitest $iOSDriverVersion and mac2 $macDriverVersio"
$x = appium driver list --installed --json  | ConvertFrom-Json

if(!$x.windows)
{
    echo "Installing appium driver windows $windowsDriverVersion"
    appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
    echo "Installed appium driver windows"
}
else
{
    echo "Uninstalling appium driver windows"
    appium driver uninstall windows
    echo "Installing appium driver windows $windowsDriverVersion"
    appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
    echo "Installed appium driver windows"
}

if(!$x.uiautomator2)
{
    echo "Installing appium driver uiautomator2 $androidDriverVersion"
    appium driver install uiautomator2@$androidDriverVersion
    echo "Installed appium driver uiautomator2"
}
else
{
    echo "Uninstalling appium driver uiautomator2"
    appium driver uninstall uiautomator2
    echo "Installing appium driver uiautomator2 $androidDriverVersion"
    appium driver install uiautomator2@$androidDriverVersion
    echo "Installed appium driver uiautomator2"
}

if(!$x.xcuitest)
{
    echo "Installing appium driver xcuitest $iOSDriverVersion"
    appium driver install xcuitest@$iOSDriverVersion
    echo "Installed appium driver xcuitest"
}
else
{
    echo "Uninstalling appium driver xcuitest"
    appium driver uninstall xcuitest
    echo "Installing appium driver xcuitest $iOSDriverVersion"
    appium driver install xcuitest@$iOSDriverVersion
    echo "Installed appium driver xcuitest"
}

if(!$x.mac2)
{
    echo "Installing appium driver mac2 $macDriverVersion"
    appium driver install mac2@$macDriverVersion
    echo "Installed appium driver mac2"
}
else
{
    echo "Uninstalling appium driver mac2"
    appium driver uninstall mac2
    echo "Installing appium driver mac2 $macDriverVersion"
    appium driver install mac2@$macDriverVersion
    echo "Installed appium driver mac2"
}
echo "Done, thanks!"