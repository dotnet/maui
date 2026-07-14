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
    [int]$BootTimeoutSeconds = 180
)

$ErrorActionPreference = 'Continue'
$p = $Platform.ToLowerInvariant()

function Wait-AndroidBootCompleted {
    param([string[]]$Serial, [int]$TimeoutSec)
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        try {
            $b = (& adb @Serial shell getprop sys.boot_completed 2>$null | Out-String).Trim()
            if ($b -eq '1') { return $true }
        } catch { }
        Start-Sleep -Seconds 3
    }
    return $false
}

try {
    if ($p -eq 'android') {
        if (-not (Get-Command adb -ErrorAction SilentlyContinue)) {
            Write-Host "adb not found — skipping device reset"
            return
        }
        $serial = @()
        if ($DeviceUdid) { $serial = @('-s', $DeviceUdid) }

        Write-Host "🔄 Rebooting Android emulator to reclaim a clean state before the next category…"
        & adb @serial reboot 2>$null | Out-Null
        & adb @serial wait-for-device 2>$null | Out-Null

        if (Wait-AndroidBootCompleted -Serial $serial -TimeoutSec $BootTimeoutSeconds) {
            Write-Host "  ✓ Emulator rebooted and boot completed."
            # Let the launcher settle, then make sure we are on the home screen
            # (not on a leftover system dialog) before the next category launches.
            Start-Sleep -Seconds 5
            try { & adb @serial shell input keyevent KEYCODE_HOME 2>$null | Out-Null } catch { }
        } else {
            Write-Host "##[warning]Emulator did not report sys.boot_completed within $BootTimeoutSeconds s — continuing anyway (the next category has its own retry/recovery)."
        }
    }
    elseif ($p -eq 'ios') {
        $sim = $DeviceUdid
        if (-not $sim) {
            try {
                $boot = & xcrun simctl list devices booted 2>$null |
                    Select-String -Pattern '\(([0-9A-Fa-f-]{36})\)' | Select-Object -First 1
                if ($boot) { $sim = $boot.Matches.Groups[1].Value }
            } catch { }
        }
        if ($sim) {
            Write-Host "🔄 Rebooting iOS simulator $sim to reclaim a clean state before the next category…"
            try {
                & xcrun simctl shutdown $sim 2>$null | Out-Null
                Start-Sleep -Seconds 3
                & xcrun simctl boot $sim 2>$null | Out-Null
                & xcrun simctl bootstatus $sim -b 2>$null | Out-Null
                Write-Host "  ✓ Simulator rebooted."
            } catch {
                Write-Host "(simctl reboot failed: $_)" -ForegroundColor DarkGray
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
