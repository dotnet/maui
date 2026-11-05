[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $ResultsDirectory
)

$ErrorActionPreference = 'Stop'
$statePath = Join-Path $ResultsDirectory 'device.json'
if (!(Test-Path -LiteralPath $statePath)) { return }
$state = Get-Content -LiteralPath $statePath -Raw | ConvertFrom-Json
$PSNativeCommandUseErrorActionPreference = $false
if ($state.Platform -eq 'android' -and $state.Device) {
    $killFailed = $false
    $name = "$(& $state.Adb -s $state.Device shell getprop ro.boot.qemu.avd_name 2>$null)".Trim()
    if (!$name -and $LASTEXITCODE -eq 0) {
        $name = "$(& $state.Adb -s $state.Device shell getprop ro.kernel.qemu.avd_name 2>$null)".Trim()
    }
    if ($LASTEXITCODE -eq 0 -and $name -eq $state.AvdName) {
        & $state.Adb -s $state.Device logcat -d *> (Join-Path $ResultsDirectory 'logcat.log')
        & $state.Adb -s $state.Device emu kill
        $killFailed = $LASTEXITCODE -ne 0
    }
    if ($state.ProcessId) {
        $process = Get-Process -Id $state.ProcessId -ErrorAction SilentlyContinue
        if ($process -and $process.ProcessName -match '^(emulator|qemu)' -and
            $process.StartTime.ToUniversalTime().ToString('O') -eq $state.ProcessStartTime) {
            Stop-Process -Id $state.ProcessId -ErrorAction Stop
            if (!$process.WaitForExit(30000)) {
                throw "Could not stop the job-owned emulator PID $($state.ProcessId)."
            }
        }
    }
    if ($killFailed) { throw "Could not stop the job-owned AVD '$($state.AvdName)'." }
    if ($state.AvdHome -and (Test-Path -LiteralPath $state.AvdHome)) {
        Remove-Item -LiteralPath $state.AvdHome -Recurse -Force
    }
}
elseif ($state.Platform -eq 'ios' -and $state.Device) {
    $crashes = Join-Path $HOME "Library/Developer/CoreSimulator/Devices/$($state.Device)/data/Library/Logs/CrashReporter"
    if (Test-Path -LiteralPath $crashes) {
        Copy-Item -LiteralPath $crashes -Destination (Join-Path $ResultsDirectory 'crashes') -Recurse
    }
    & xcrun simctl spawn $state.Device log show --last 15m --style compact *> (Join-Path $ResultsDirectory 'simulator.log')
    & xcrun simctl shutdown $state.Device
    & xcrun simctl delete $state.Device
    if ($LASTEXITCODE -ne 0) { throw "Could not delete job-owned simulator $($state.Device)." }
}
