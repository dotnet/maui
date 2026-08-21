<#
.SYNOPSIS
  Reset the shared UI-test device/simulator BETWEEN deep-UI-test categories.

.DESCRIPTION
  The deep per-category loop pre-boots ONE device/simulator (Start-Emulator.ps1)
  and reuses it for EVERY category. Over a long multi-category run — especially
  after a category is tree-killed on its time budget (wall-clock OR idle) — the
  shared Android emulator can degrade (memory exhaustion / a wedged system
  service) to the point where the HostApp starts crashing on launch: the Android
  "Controls.TestCases.HostApp keeps stopping" system dialog. Once that happens,
  EVERY subsequent fixture's OneTimeSetup times out waiting for the gallery
  ("Go To Test button") and the WHOLE next category is falsely reported failed
  (observed: Material3 0/338 after CollectionView was hard-killed at its budget).

  Rebooting the shared device between categories reclaims a clean state so each
  category starts fresh, eliminating this cross-category contamination.

  Best-effort by design: any failure here is swallowed (never throws) so a reset
  problem can NEVER block the deep run — the following category will still run
  and surface its own real result.

.PARAMETER Platform
  android | ios | catalyst | maccatalyst | windows

.PARAMETER DeviceUdid
  The shared device/simulator UDID (typically $env:DEVICE_UDID). Optional; on
  iOS the currently-booted simulator is auto-detected when omitted.

.PARAMETER BootTimeoutSeconds
  Max seconds to wait for the device to finish rebooting (default 180).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Platform,
    [string]$DeviceUdid,
    [ValidateRange(1, 3600)]
    [int]$BootTimeoutSeconds = 180
)

$ErrorActionPreference = 'Continue'
$p = $Platform.ToLowerInvariant()
$sharedUtils = Join-Path $PSScriptRoot 'shared-utils.ps1'
if (-not (Test-Path -LiteralPath $sharedUtils -PathType Leaf)) {
    Write-Host "##[warning]Device reset helper is missing: $sharedUtils"
    return
}
. $sharedUtils

function Get-RemainingTimeoutSeconds {
    param(
        [datetime]$Deadline,
        [int]$MaximumSeconds
    )

    $remaining = [int][Math]::Ceiling(($Deadline - (Get-Date)).TotalSeconds)
    if ($remaining -le 0) {
        return 0
    }

    return [Math]::Min($remaining, $MaximumSeconds)
}

function Invoke-DeviceCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [string[]]$ArgumentList = @(),
        [Parameter(Mandatory = $true)][datetime]$Deadline,
        [int]$MaximumSeconds = 30
    )

    $timeoutSeconds = Get-RemainingTimeoutSeconds -Deadline $Deadline -MaximumSeconds $MaximumSeconds
    if ($timeoutSeconds -le 0) {
        return [pscustomobject]@{
            Output = @()
            ExitCode = 124
            TimedOut = $true
        }
    }

    try {
        return Invoke-ProcessWithTimeout `
            -FilePath $FilePath `
            -ArgumentList $ArgumentList `
            -TimeoutSeconds $timeoutSeconds
    }
    catch {
        Write-Host "##[warning]Device command '$FilePath $($ArgumentList -join ' ')' failed: $_"
        return [pscustomobject]@{
            Output = @()
            ExitCode = -1
            TimedOut = $false
        }
    }
}

function Wait-AndroidBootCompleted {
    param(
        [string[]]$Serial,
        [datetime]$Deadline
    )

    while ((Get-Date) -lt $Deadline) {
        $probe = Invoke-DeviceCommand `
            -FilePath 'adb' `
            -ArgumentList @($Serial + @('shell', 'getprop', 'sys.boot_completed')) `
            -Deadline $Deadline `
            -MaximumSeconds 10
        if ($probe.ExitCode -eq 0 -and (($probe.Output -join '').Trim() -eq '1')) {
            return $true
        }

        $sleepSeconds = Get-RemainingTimeoutSeconds -Deadline $Deadline -MaximumSeconds 3
        if ($sleepSeconds -gt 0) {
            Start-Sleep -Seconds $sleepSeconds
        }
    }

    return $false
}

try {
    $resetDeadline = (Get-Date).AddSeconds($BootTimeoutSeconds)

    if ($p -eq 'android') {
        if (-not (Get-Command adb -ErrorAction SilentlyContinue)) {
            Write-Host "adb not found — skipping device reset"
            return
        }
        $serial = @()
        if ($DeviceUdid) { $serial = @('-s', $DeviceUdid) }

        Write-Host "🔄 Rebooting Android emulator to reclaim a clean state before the next category…"
        $reboot = Invoke-DeviceCommand `
            -FilePath 'adb' `
            -ArgumentList @($serial + @('reboot')) `
            -Deadline $resetDeadline `
            -MaximumSeconds 30
        if ($reboot.ExitCode -ne 0) {
            Write-Host "##[warning]adb reboot failed or timed out — continuing without a device reset."
            return
        }

        $wait = Invoke-DeviceCommand `
            -FilePath 'adb' `
            -ArgumentList @($serial + @('wait-for-device')) `
            -Deadline $resetDeadline `
            -MaximumSeconds $BootTimeoutSeconds
        if ($wait.ExitCode -ne 0) {
            Write-Host "##[warning]adb wait-for-device did not complete within the reset budget — continuing anyway."
            return
        }

        if (Wait-AndroidBootCompleted -Serial $serial -Deadline $resetDeadline) {
            Write-Host "  ✓ Emulator rebooted and boot completed."
            # Let the launcher settle, then make sure we are on the home screen
            # (not on a leftover system dialog) before the next category launches.
            $settleSeconds = Get-RemainingTimeoutSeconds -Deadline $resetDeadline -MaximumSeconds 5
            if ($settleSeconds -gt 0) {
                Start-Sleep -Seconds $settleSeconds
            }
            [void](Invoke-DeviceCommand `
                -FilePath 'adb' `
                -ArgumentList @($serial + @('shell', 'input', 'keyevent', 'KEYCODE_HOME')) `
                -Deadline $resetDeadline `
                -MaximumSeconds 10)
        } else {
            Write-Host "##[warning]Emulator did not report sys.boot_completed within $BootTimeoutSeconds s — continuing anyway (the next category has its own retry/recovery)."
        }
    }
    elseif ($p -eq 'ios') {
        $sim = $DeviceUdid
        if (-not $sim) {
            $booted = Invoke-DeviceCommand `
                -FilePath 'xcrun' `
                -ArgumentList @('simctl', 'list', 'devices', 'booted') `
                -Deadline $resetDeadline `
                -MaximumSeconds 20
            $boot = $booted.Output |
                Select-String -Pattern '\(([0-9A-Fa-f-]{36})\)' |
                Select-Object -First 1
            if ($boot) { $sim = $boot.Matches.Groups[1].Value }
        }
        if ($sim) {
            Write-Host "🔄 Rebooting iOS simulator $sim to reclaim a clean state before the next category…"
            $shutdown = Invoke-DeviceCommand `
                -FilePath 'xcrun' `
                -ArgumentList @('simctl', 'shutdown', $sim) `
                -Deadline $resetDeadline `
                -MaximumSeconds 30
            if ($shutdown.ExitCode -ne 0) {
                Write-Host "##[warning]simctl shutdown failed or timed out; attempting a bounded boot anyway."
            }

            $settleSeconds = Get-RemainingTimeoutSeconds -Deadline $resetDeadline -MaximumSeconds 3
            if ($settleSeconds -gt 0) {
                Start-Sleep -Seconds $settleSeconds
            }

            $bootResult = Invoke-DeviceCommand `
                -FilePath 'xcrun' `
                -ArgumentList @('simctl', 'boot', $sim) `
                -Deadline $resetDeadline `
                -MaximumSeconds 30
            if ($bootResult.ExitCode -ne 0) {
                Write-Host "##[warning]simctl boot failed or timed out — continuing without a completed reset."
                return
            }

            $bootStatusTimeout = Get-RemainingTimeoutSeconds `
                -Deadline $resetDeadline `
                -MaximumSeconds $BootTimeoutSeconds
            if ($bootStatusTimeout -le 0) {
                Write-Host "##[warning]No reset budget remains for simctl bootstatus — continuing anyway."
                return
            }

            $bootStatus = Invoke-DeviceCommand `
                -FilePath 'xcrun' `
                -ArgumentList @('simctl', 'bootstatus', $sim, '-b', '-t', "$bootStatusTimeout") `
                -Deadline $resetDeadline `
                -MaximumSeconds $bootStatusTimeout
            if ($bootStatus.ExitCode -eq 0) {
                Write-Host "  ✓ Simulator rebooted."
            } else {
                Write-Host "##[warning]simctl bootstatus failed or timed out — continuing anyway (the next category has its own retry/recovery)."
            }
        } else {
            Write-Host "No booted simulator UDID — skipping device reset"
        }
    }
    else {
        # catalyst / maccatalyst / windows run the HostApp as a fresh HOST process
        # per category (there is no shared VM/emulator to reboot), so the
        # cross-category device-degradation failure mode does not apply. Nothing
        # to reset here.
        Write-Host "Platform '$Platform' has no shared device to reset — skipping."
    }
}
catch {
    # Never let a reset problem block the run.
    Write-Host "##[warning]Device reset threw (non-fatal): $_"
}
