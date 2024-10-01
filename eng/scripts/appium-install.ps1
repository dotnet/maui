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

PS> .\appium-install.ps1 '2.1.1' 2.7.2 2.25.1 4.30.2 1.6.1

This would install or update Appium version 2.1.1, the windows driver 2.7.2, the uiautomator2 driver with 2.25.1, the xcuitest driver with 4.30.2 and mac2 driver with 1.6.1

Versions for these steps are pinned and ideally stay in sync with the script that initializes the XAMBOT agents.
Find the script for that on the DevDiv Azure DevOps instance, Engineering team, BotDeploy.PackageGeneration repo.
#>

param
(
    [string] $appiumVersion = '2.11.4',
    [string] $windowsDriverVersion = '2.12.32',
    [string] $androidDriverVersion = '3.8.0',    
    [string] $iOSDriverVersion = '7.26.4',
    [string] $macDriverVersion = '1.19.1',
    [string] $logsDir = '../appium-logs'
)

Write-Output  "Welcome to the Appium installer"

Write-Output  "Node version"
node -v

$npmLogLevel = 'verbose'

# globally set npm loglevel
npm config set loglevel $npmLogLevel

# Create logs directory for npm logs if it doesn't exist
if (!(Test-Path $logsDir -PathType Container)) {
    New-Item -ItemType Directory -Path $logsDir
}

# Get our path to APPIUM_HOME
$AppiumHome = $env:APPIUM_HOME
Write-Output "APPIUM_HOME: $AppiumHome"

if ($AppiumHome) {
    if (Test-Path $AppiumHome) {
        Write-Output  "Removing existing APPIUM_HOME Cache..."
        Remove-Item -Path $AppiumHome -Recurse -Force
    }

    # Create the directory for appium home
    New-Item -ItemType Directory -Path $AppiumHome
}

# Check for an existing appium install version
$appiumCurrentVersion = ""
try { $appiumCurrentVersion = appium -v | Out-String } catch { }

if ($appiumCurrentVersion) {
    Write-Output  "Existing Appium version $appiumCurrentVersion"
} else {
    Write-Output  "No Appium version installed"
}

# If current version does not match the one we want, uninstall and install the new version
if ($appiumCurrentVersion -ne $appiumVersion) {
    Write-Output  "Uninstalling appium $appiumCurrentVersion"
    npm uninstall --logs-dir=$logsDir --loglevel $npmLogLevel -g appium
    Write-Output  "Uninstalled appium $appiumCurrentVersion"

    Write-Output  "Installing appium $appiumVersion"
    npm install --logs-dir=$logsDir --loglevel $npmLogLevel -g appium@$appiumVersion
    write-Output  "Installed appium $appiumVersion"   
}

$existingDrivers = appium driver list --installed --json  | ConvertFrom-Json
Write-Output "List of installed drivers $existingDrivers"

if ($IsWindows) {
    if ($existingDrivers.windows) {
        Write-Output  "Updating appium driver windows"
        appium driver update windows
        Write-Output  "Updated appium driver windows"
    } else {
        Write-Output  "Installing appium driver windows"
        appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
        Write-Output  "Installed appium driver windows"
    }
}

if ($IsMacOS) {

    if ($existingDrivers.xcuitest) {
        Write-Output  "Updating appium driver xcuitest"
        appium driver update xcuitest
        Write-Output  "Updated appium driver xcuitest"
    } else {
        Write-Output  "Installing appium driver xcuitest"
        appium driver install xcuitest@$iOSDriverVersion
        Write-Output  "Installed appium driver xcuitest"
    }
    
    if ($existingDrivers.mac2) {
        Write-Output  "Updating appium driver mac2"
        appium driver update mac2
        Write-Output  "Updated appium driver mac2"
    } else {
        Write-Output  "Installing appium driver mac2"
        appium driver install mac2@$macDriverVersion
        Write-Output  "Installed appium driver mac2"
    }
}

if ($existingDrivers.uiautomator2) {
    Write-Output  "Updating appium driver uiautomator2"
    appium driver update uiautomator2
    Write-Output  "Updated appium driver uiautomator2"
} else {
    Write-Output  "Installing appium driver uiautomator2"
    appium driver install uiautomator2@$androidDriverVersion
    Write-Output  "Installed appium driver uiautomator2"
}

$drivers = appium driver list --installed --json  | ConvertFrom-Json
Write-Output "List of installed drivers after cleaup $drivers"

Write-Output  "Check everything is installed correctly with appium doctor"

if ($IsWindows) {
    appium driver doctor windows || & { "ignore failure"; $global:LASTEXITCODE = 0 }
}
if ($IsMacOS) {
    appium driver doctor xcuitest || & { "ignore failure"; $global:LASTEXITCODE = 0 }
    appium driver doctor mac2 || & { "ignore failure"; $global:LASTEXITCODE = 0 }    
}
appium driver doctor uiautomator2 || & { "ignore failure"; $global:LASTEXITCODE = 0 }

Write-Output  "Done, thanks!"
