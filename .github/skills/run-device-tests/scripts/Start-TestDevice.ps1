[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('android', 'ios', 'catalyst', 'windows')]
    [string] $Platform,
    [Parameter(Mandatory)]
    [string] $ResultsDirectory,
    [Parameter(Mandatory)]
    [string] $DeviceDirectory,
    [string] $ApiVersion = 'latest',
    [int] $ExpectedOSMajor = 0,
    [string] $ExpectedArchitecture = '',
    [bool] $RequireWindows11 = $false
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

function Get-CompatibleIosDeviceType {
    param([version]$RuntimeVersion, [object[]]$DeviceTypes)

    $RuntimeVersion = [version]::new($RuntimeVersion.Major, $RuntimeVersion.Minor, [Math]::Max(0, $RuntimeVersion.Build))
    $compatible = @($DeviceTypes | Where-Object {
        $_.productFamily -eq 'iPhone' -and
        [version]$_.minRuntimeVersionString -le $RuntimeVersion -and
        [version]$_.maxRuntimeVersionString -ge $RuntimeVersion
    } | Sort-Object { [version]$_.minRuntimeVersionString } -Descending)
    $preferred = $compatible | Where-Object identifier -EQ 'com.apple.CoreSimulator.SimDeviceType.iPhone-16' |
        Select-Object -First 1
    if ($preferred) { return $preferred.identifier }
    if (!$compatible.Count) { throw "No installed iPhone device type supports iOS $RuntimeVersion." }
    return $compatible[0].identifier
}

$baseDirectory = (Get-Location).Path
$ResultsDirectory = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($ResultsDirectory)) {
    $ResultsDirectory
} else { Join-Path $baseDirectory $ResultsDirectory }))
$DeviceDirectory = [IO.Path]::GetFullPath($(if ([IO.Path]::IsPathRooted($DeviceDirectory)) {
    $DeviceDirectory
} else { Join-Path $baseDirectory $DeviceDirectory }))
New-Item -ItemType Directory -Force -Path $ResultsDirectory | Out-Null
$statePath = Join-Path $ResultsDirectory 'device.json'
if ($Platform -in @('ios', 'catalyst') -and !$IsMacOS) {
    throw "$Platform tests require a macOS execution agent."
}
if ($Platform -eq 'windows' -and !$IsWindows) {
    throw 'Windows tests require a Windows execution agent.'
}
if (Test-Path -LiteralPath $statePath) {
    throw "Refusing to reuse device state '$statePath'."
}
if ($ExpectedArchitecture -and
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() -ine $ExpectedArchitecture) {
    throw "This lane requires $ExpectedArchitecture, not $([Runtime.InteropServices.RuntimeInformation]::OSArchitecture)."
}
if ($ExpectedOSMajor) {
    $version = if ($IsMacOS) { [version](& sw_vers -productVersion) } else { [Environment]::OSVersion.Version }
    if ($version.Major -ne $ExpectedOSMajor) {
        throw "This lane requires OS major $ExpectedOSMajor, not $version."
    }
}

$state = @{ Platform = $Platform; DeviceDirectory = $DeviceDirectory }
function Save-State {
    $state | ConvertTo-Json | Set-Content -LiteralPath $statePath
}
Save-State

if ($Platform -eq 'android') {
    $sdk = if ($env:ANDROID_SDK_ROOT) { $env:ANDROID_SDK_ROOT } else { $env:ANDROID_HOME }
    if (!$sdk) { throw 'ANDROID_SDK_ROOT or ANDROID_HOME must identify the provisioned SDK.' }
    $sdk = [IO.Path]::GetFullPath($sdk)
    $adb = Join-Path $sdk 'platform-tools/adb'
    $manager = Join-Path $sdk 'cmdline-tools/latest/bin/sdkmanager'
    $avdmanager = Join-Path $sdk 'cmdline-tools/latest/bin/avdmanager'
    $emulator = Join-Path $sdk 'emulator/emulator'
    $architecture = if ([Runtime.InteropServices.RuntimeInformation]::OSArchitecture -eq 'Arm64') { 'arm64-v8a' } else { 'x86_64' }
    $image = "system-images;android-$ApiVersion;google_apis;$architecture"
    & $manager "--sdk_root=$sdk" --install 'platform-tools' 'emulator' $image

    $name = "maui-$([guid]::NewGuid().ToString('N'))"
    $deviceRoot = Join-Path $DeviceDirectory $name
    $avdHome = Join-Path $deviceRoot 'avd'
    New-Item -ItemType Directory -Path $deviceRoot | Out-Null
    New-Item -ItemType Directory -Force -Path $avdHome | Out-Null
    $env:ANDROID_AVD_HOME = $avdHome
    $state.AvdHome = $avdHome
    $state.DeviceRoot = $deviceRoot
    $state.AvdName = $name
    $state.Adb = $adb
    Save-State
    'no' | & $avdmanager create avd --force --name $name --package $image --path (Join-Path $avdHome "$name.avd")

    # Reserve both emulator ports until immediately before launching this job's emulator.
    $listeners = @()
    foreach ($candidate in (5554..5680 | Where-Object { $_ % 2 -eq 0 } | Sort-Object { Get-Random })) {
        try {
            $listeners = @(
                [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, $candidate),
                [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, ($candidate + 1))
            )
            $listeners | ForEach-Object Start
            $port = $candidate
            break
        }
        catch {
            $listeners | ForEach-Object Stop
            $listeners = @()
        }
    }
    if (!$port) { throw 'No unused Android emulator port pair is available.' }
    $state.Device = "emulator-$port"
    Save-State
    $listeners | ForEach-Object Stop
    $process = Start-Process -FilePath $emulator -ArgumentList @(
        '-avd', $name, '-port', $port, '-no-window', '-no-audio', '-no-boot-anim',
        '-no-snapshot', '-gpu', 'swiftshader_indirect'
    ) -RedirectStandardOutput (Join-Path $ResultsDirectory 'emulator.stdout.log') `
      -RedirectStandardError (Join-Path $ResultsDirectory 'emulator.stderr.log') -PassThru
    $state.ProcessId = $process.Id
    $state.ProcessStartTime = $process.StartTime.ToUniversalTime().ToString('O')
    Save-State

    $deadline = [DateTime]::UtcNow.AddMinutes(10)
    while ([DateTime]::UtcNow -lt $deadline) {
        $process.Refresh()
        if ($process.HasExited) { throw "Emulator exited before boot (exit $($process.ExitCode))." }
        $PSNativeCommandUseErrorActionPreference = $false
        $booted = & $adb -s $state.Device shell getprop sys.boot_completed 2>$null
        $bootExit = $LASTEXITCODE
        $PSNativeCommandUseErrorActionPreference = $true
        if ($bootExit -eq 0 -and "$booted".Trim() -eq '1') {
            $avdName = "$(& $adb -s $state.Device shell getprop ro.boot.qemu.avd_name)".Trim()
            if (!$avdName) {
                $avdName = "$(& $adb -s $state.Device shell getprop ro.kernel.qemu.avd_name)".Trim()
            }
            if ($avdName -ne $state.AvdName) {
                throw "Emulator serial $($state.Device) belongs to a different AVD."
            }
            break
        }
        Start-Sleep -Seconds 5
    }
    if ("$booted".Trim() -ne '1') { throw 'The job-owned Android emulator did not boot within ten minutes.' }
    & $adb -s $state.Device shell input keyevent 82
    Write-Host "##vso[task.setvariable variable=TestDevice]$($state.Device)"
    Write-Host "##vso[task.setvariable variable=TestRuntimeIdentifier]android-$([Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant())"
}
elseif ($Platform -eq 'ios') {
    $runtimes = (& xcrun simctl list runtimes --json | ConvertFrom-Json).runtimes
    $runtime = $runtimes | Where-Object {
        $_.isAvailable -and $_.identifier -like '*.iOS-*' -and
        ($ApiVersion -eq 'latest' -or $_.version -eq $ApiVersion -or
            ($ApiVersion -match '^\d+$' -and ([version]$_.version).Major -eq [int]$ApiVersion))
    } | Sort-Object { [version]$_.version } -Descending | Select-Object -First 1
    if (!$runtime) { throw "Required iOS simulator runtime '$ApiVersion' is not installed." }
    $deviceTypes = (& xcrun simctl list devicetypes --json | ConvertFrom-Json).devicetypes
    $deviceType = Get-CompatibleIosDeviceType -RuntimeVersion $runtime.version -DeviceTypes $deviceTypes
    $state.SimulatorName = "maui-$([guid]::NewGuid().ToString('N'))"
    $state.Device = (& xcrun simctl create $state.SimulatorName $deviceType $runtime.identifier).Trim()
    $simulatorId = [guid]::Empty
    if (![guid]::TryParseExact($state.Device, 'D', [ref]$simulatorId)) { throw "Unexpected simulator UDID '$($state.Device)'." }
    Save-State
    & xcrun simctl boot $state.Device
    & xcrun simctl bootstatus $state.Device -b
    Write-Host "##vso[task.setvariable variable=TestDevice]$($state.Device)"
    Write-Host "##vso[task.setvariable variable=TestRuntimeIdentifier]iossimulator-$([Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant())"
}
elseif ($Platform -eq 'catalyst') {
    Write-Host "##vso[task.setvariable variable=TestRuntimeIdentifier]maccatalyst-$([Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLowerInvariant())"
}
else {
    if (![Environment]::UserInteractive) { throw 'Windows UI device tests require an interactive desktop agent.' }
    if ($RequireWindows11) {
        $os = Get-CimInstance Win32_OperatingSystem
        if ($os.ProductType -ne 1 -or [int]$os.BuildNumber -lt 22000) {
            throw 'This lane requires Windows 11 client, not a Windows Server build agent.'
        }
    }
    Write-Host '##vso[task.setvariable variable=TestRuntimeIdentifier]win-x64'
}
