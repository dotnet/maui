<#
.SYNOPSIS

Runs adb commands to launch a Xamarin.Android project and time its
overall startup.

.DESCRIPTION

This will build & deploy the project, and by default launch the app 10
times. adb logcat messages are parsed and an average displayed.

.PARAMETER adb

Path to the adb executable. Will locate a default one installed by
Visual Studio 2019 and fall back to using the one found in $env:PATH.

.PARAMETER msbuild

Path to the msbuild executable. Will locate a default one installed by
Visual Studio 2019 and fall back to using the one found in $env:PATH.

.PARAMETER project

Path to a Xamarin.Android csproj file. Defaults to
HelloForms.Android.csproj in this repository.

.PARAMETER package

The package name of the app, found in AndroidManifest.xml. This value
is needed to launch or restart the app.

.PARAMETER configuration

The $(Configuration) MSBuild property. Defaults to 'Debug'.

.PARAMETER extra

Additional command-line arguments to pass to MSBuild. For example,
'-extra /p:AotAssemblies=True' would enable AOT.

.PARAMETER xamarinformsversion
The Xamarin.Forms version to run hello world on

.PARAMETER sleep

The number of seconds to wait between each app launch. Defaults to 3,
but you may need to increase for larger apps.

.PARAMETER iterations

The number of times to launch the app for profiling. Defaults to 10.

.PARAMETER androidapi

The android api to use when creating the emulator

.EXAMPLE

PS> profile-android.ps1

Will build and deploy HelloForms.Android.csproj, launch the
application 10 times and print the average time taken.

.EXAMPLE

PS> profile-android.ps1 -project .\Path\To\MyApp.csproj -package com.mycompany.myapp -iterations 3
Launching: com.mycompany.myapp
Launching: com.mycompany.myapp
Launching: com.mycompany.myapp
12-12 09:13:15.593  1876  1898 I ActivityManager: Displayed com.mycompany.myapp/crc6450e568c951913723.MainActivity: +1s501ms
12-12 09:13:19.696  1876  1898 I ActivityManager: Displayed com.mycompany.myapp/crc6450e568c951913723.MainActivity: +1s475ms
12-12 09:13:23.863  1876  1898 I ActivityManager: Displayed com.mycompany.myapp/crc6450e568c951913723.MainActivity: +1s564ms
Average(ms): 1513.33333333333

This would launch a different app with 3 iterations.

.EXAMPLE

PS> profile-android.ps1 -configuration Release -extra /p:AotAssemblies=True -androidapi android-28

This would test the app using a Release build and AOT.
#>

param
(
    [string] $adb,
    [string] $msbuild,
    [string] $project,
    [string] $package,
    [string] $configuration = 'Debug',
    [string] $extra,
    [string] $xamarinformsversion = '4.7.0.1351',
    [string] $androidapi = 'android-28',
    [int] $sleep = 3,
    [int] $iterations = 10
)

Function Get-Devices{
    Param ($emulators, $androidapi)

    if (-not $emulator)
    {
        $emulator = 'C:\Program Files (x86)\Android\android-sdk\emulator\emulator.exe'
        if (-not (Test-Path $emulator))
        {
            Write-Host "We don't have emulator"
            # emulator should be in $PATH on macOS
            if($IsMacOS)
            {
                Write-Host "We are running macos"
                $emulator = $null
            }
        }
    }

    $devices = & $adb devices
    $noDevices = [string]::IsNullOrWhiteSpace(($devices -replace "List of Devices attached",""))

    if ($noDevices)
    {
        Write-Host "No devices found"
        if ($emulator)
        {
            Write-Host "Listing emulators"
            & $emulator -list-avds
            $emulators = & $emulator -list-avds
            $emulatorsArray =$emulators.Split(" ")
            Write-Host "number of emulators" $emulatorsArray.length
            if($emulatorsArray.length -gt 0)
            {
                $index = 0
                $emulatorToRun = $emulatorsArray[ $index]
                Write-Host "Starting emulator $index from list - $emulatorToRun "
                Start-Process -FilePath $emulator -ArgumentList "-avd $emulatorToRun"
                Start-Job -ScriptBlock {
                    & $emulator -avd $emulatorToRun
                }
            }
            else {
                Write-Host "no emulators found"
            }
        }
        elseif ($IsMacOS)
        {
            Write-Host "start emulator.sh"
            Start-Process -FilePath emulator.sh $androidapi
        }
    }
    else {
        & $adb devices
    }
}

Function Build-App{
    Param ($project, $package)

    if (-not $msbuild)
    {
        $msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
        if (-not (Test-Path $msbuild))
        {
            # msbuild should be in $PATH on macOS
            $msbuild = 'msbuild'
        }
    }
    if (-not $flutter)
    {
        # flutter should be in $PATH
        $flutter = 'flutter'
    }

    $isFlutter = -not $project.EndsWith("csproj")
    if(-not $isFlutter)
    {
        & $msbuild $project /v:minimal /nologo /restore /t:Clean,Install /p:Configuration=$configuration /p:XamarinFormsVersion=$xamarinformsversion $extra
    }
    else
    {
        Set-Location $project
        Write-Host "Building flutter: $package"
        & $flutter build apk --release
        & $flutter install
    }
}

$ErrorActionPreference = 'Stop'

# Input validation
if (-not $adb)
{
    $adb = 'C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe'
    if (-not (Test-Path $adb))
    {
        Write-Host "doesnt exist adb"
        # adb should be in $PATH on macOS
        $adb = 'adb'
    }
}

#Check if we have devices, or start a emulator
Get-Devices $emulator $androidapi

#Build App
Build-App $project $package

#We need a large logcat buffer
& $adb logcat -G 30M
& $adb logcat -c

for ($i = 1; $i -le $iterations; $i++)
{
    Write-Host "Launching: $package"
    & $adb shell am force-stop $package
    & $adb shell monkey -p $package 1
   # & $adb shell am start -n "$package/$package.MainActivity"
    Start-Sleep -Seconds $sleep
}

# Log message of the form:
# 12-12 09:08:36.974  1876  1898 I ActivityManager: Displayed com.mycompany.myapp/crc6450e568c951913723.MainActivity: +1s540ms

$log = & $adb logcat -d | Select-String -Pattern 'Activity.*Manager.+Displayed'
if ($log.Count -eq 0)
{
    Write-Error "No ActivityManager messages found"
}

$sum = 0;
[System.Collections.ArrayList] $times = @()
foreach ($line in $log)
{
    if ($line -match "((?<seconds>\d+)s)?(?<milliseconds>\d+)ms(\s+\(total.+\))?$")
    {
        $seconds = [int]$Matches.seconds
        $milliseconds = [int]$Matches.milliseconds
        $time = $seconds * 1000 + $milliseconds
        $times.Add($time) > $null
        $sum += $time
        Write-Host $line
    }
    else
    {
        Write-Error "No timing found for line: $line"
    }
}
$mean = $sum / $log.Count
$variance = 0
if ($log.Count -ne 1)
{
    foreach ($time in $times)
    {
        $variance += ($time - $mean) * ($time - $mean) / ($log.Count - 1)
    }
}
$stdev = [math]::Sqrt($variance)
$stderr = $stdev / [math]::Sqrt($log.Count)

Write-Host "Average(ms): $mean"
Write-Host "Std Err(ms): $stderr"
Write-Host "Std Dev(ms): $stdev"
