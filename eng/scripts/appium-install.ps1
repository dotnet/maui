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
    [string] $appiumVersion = '',
    [string] $windowsDriverVersion = '',
    [string] $androidDriverVersion = '',
    [string] $iOSDriverVersion = '',
    [string] $macDriverVersion = '',
    [string] $logsDir = '../appium-logs'
)

# By default, versions should be read from /eng/Versions.props
$getLocation = $PSScriptRoot
$versionPropsPath = [IO.Path]::Combine($getLocation, '..', 'Versions.props')

Write-Output "Checking $versionPropsPath for versions..."

if (Test-Path $versionPropsPath)
{
    Write-Output "Reading versions from Version.props..."
    [xml]$versionProps = Get-Content $versionPropsPath
    $versionPropsAppiumVersion = $versionProps.Project.PropertyGroup.AppiumVersion | Where-Object { $_ -ne $null } | Select-Object -Last 1
    if ($null -ne $versionPropsAppiumVersion) {
        $appiumVersion = $versionPropsAppiumVersion
    }

    $versionPropsWindowsDriverVersion = $versionProps.Project.PropertyGroup.AppiumWindowsDriverVersion | Where-Object { $_ -ne $null } | Select-Object -Last 1
    if ($null -ne $versionPropsWindowsDriverVersion) {
        $windowsDriverVersion = $versionPropsWindowsDriverVersion
    }

    $versionPropsUIAutomator2DriverVersion = $versionProps.Project.PropertyGroup.AppiumUIAutomator2DriverVersion | Where-Object { $_ -ne $null } | Select-Object -Last 1
    if ($null -ne $versionPropsUIAutomator2DriverVersion) {
        $androidDriverVersion = $versionPropsUIAutomator2DriverVersion
    }

    $versionPropsXCUItestDriverVersion = $versionProps.Project.PropertyGroup.AppiumXCUITestDriverVersion | Where-Object { $_ -ne $null } | Select-Object -Last 1
    if ($null -ne $versionPropsXCUItestDriverVersion) {
        $iOSDriverVersion = $versionPropsXCUItestDriverVersion
    }

    $versionPropsMac2DriverVersion = $versionProps.Project.PropertyGroup.AppiumMac2DriverVersion | Where-Object { $_ -ne $null } | Select-Object -Last 1
    if ($null -ne $versionPropsMac2DriverVersion) {
        $macDriverVersion = $versionPropsMac2DriverVersion
    }
}
else {
    throw "The version.props file was not found at path: $versionPropsPath"
}

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
try { $appiumCurrentVersion = (appium -v | Out-String).Trim() -replace "`r", "" -replace "`n", "" } catch { Write-Debug "Problem retrieving current Appium version" }

if ($appiumCurrentVersion) {
    Write-Output  "Existing Appium version $appiumCurrentVersion"
} else {
    Write-Output  "No Appium version installed"
}

# Check if we found a version of appium at all
$missingAppium = [string]::IsNullOrEmpty($appiumCurrentVersion)

# If current version does not match the one we want, uninstall and install the new version
if ($missingAppium -or ($appiumCurrentVersion -ne $appiumVersion)) {
    if (-not $missingAppium) {
        Write-Output  "Uninstalling appium $appiumCurrentVersion"
        npm uninstall --logs-dir=$logsDir --loglevel $npmLogLevel -g appium
        Write-Output  "Uninstalled appium $appiumCurrentVersion"
    }

    Write-Output  "Installing appium $appiumVersion"
    npm install --logs-dir=$logsDir --loglevel $npmLogLevel -g appium@$appiumVersion
    write-Output  "Installed appium $appiumVersion"
}

# Clean npm cache, just in case
Write-Output  "Cleaning npm cache"
npm cache clean --force

$existingDrivers = appium driver list --installed --json  | ConvertFrom-Json
Write-Output "List of installed drivers $existingDrivers"

$exitCode = 0

function Check-ExitCode {
    param (
        [int]$code
    )
    if ($code -ne 0 -and $exitCode -eq 0) {
        $exitCode = $code
    }
}

if ($IsWindows) {
    if ($existingDrivers.windows) {
        Write-Output  "Updating appium driver windows"
        appium driver update windows
        Check-ExitCode $LASTEXITCODE
        Write-Output  "Updated appium driver windows"
    } else {
        Write-Output  "Installing appium driver windows"
        appium driver install --source=npm appium-windows-driver@$windowsDriverVersion
        Check-ExitCode $LASTEXITCODE
        Write-Output  "Installed appium driver windows"
    }
}

if ($IsMacOS) {

    if ($existingDrivers.xcuitest) {
        Write-Output  "Updating appium driver xcuitest"
        appium driver update xcuitest
        Check-ExitCode $LASTEXITCODE
        Write-Output  "Updated appium driver xcuitest"
    } else {
        Write-Output  "Installing appium driver xcuitest"
        appium driver install xcuitest@$iOSDriverVersion
        Check-ExitCode $LASTEXITCODE
        Write-Output  "Installed appium driver xcuitest"
    }
    if ($existingDrivers.mac2) {
        Write-Output  "Updating appium driver mac2"
        appium driver update mac2
        Check-ExitCode $LASTEXITCODE
        Write-Output  "Updated appium driver mac2"
    } else {
        Write-Output  "Installing appium driver mac2"
        appium driver install mac2@$macDriverVersion
        Check-ExitCode $LASTEXITCODE
        Write-Output  "Installed appium driver mac2"
    }
}

if ($existingDrivers.uiautomator2) {
    Write-Output  "Updating appium driver uiautomator2"
    appium driver update uiautomator2
    Check-ExitCode $LASTEXITCODE
    Write-Output  "Updated appium driver uiautomator2"
} else {
    Write-Output  "Installing appium driver uiautomator2"
    appium driver install uiautomator2@$androidDriverVersion
    Check-ExitCode $LASTEXITCODE
    Write-Output  "Installed appium driver uiautomator2"
}

$drivers = appium driver list --installed --json  | ConvertFrom-Json
Write-Output "List of installed drivers after cleaup $drivers"

Write-Output  "Check everything is installed correctly with appium doctor"

if ($IsWindows) {
    appium driver doctor windows
}
if ($IsMacOS) {
    appium driver doctor xcuitest
    appium driver doctor mac2
}

appium driver doctor uiautomator2

if ($exitCode -ne 0) {
    throw "One or more Appium driver installations failed. Please check the logs for more information."
}

Write-Output  "Done, thanks!"
