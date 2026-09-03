#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Single source of truth for "build + deploy + run UI tests" with the same
    deploy/retry/recovery technique the Gate (verify-tests-fail.ps1) uses.

.DESCRIPTION
    Both the Gate (Phase 5) and STEP 3 (UI Test Execution Results) need to:
      1. Pre-boot a single shared device/simulator (Start-Emulator.ps1)
      2. Invoke BuildAndRunHostApp.ps1 with the booted UDID so it doesn't
         try to start its own device or race with another booted one
      3. Detect environment errors in the captured output (ADB broken pipe,
         XHarness exit 83, AOT loader crash, missing devices, etc.)
      4. Retry up to N times with a backoff sleep, rebooting the device on
         Android/iOS app-launch failures
      5. Return both the captured stdout (for downstream parsing) and the
         exit code, plus a flag indicating whether the persistent failure
         was an environment problem vs a real test failure.

    Until this script existed, STEP 3 just called BuildAndRunHostApp.ps1
    once with no preflight or retry, so a single ADB "Broken pipe" install
    failure would cause every NUnit test in the fixture to OneTimeSetUp-
    timeout and the AI summary would falsely report 100+ regressions.

    The Gate's verify-tests-fail.ps1 will be updated to delegate UI test
    runs to this script in a follow-up — for now it inlines the same logic
    in Invoke-TestRun + Invoke-TestRunWithRetry. The patterns and behaviour
    here are kept intentionally identical to those functions so consumers
    behave identically across both paths.

.PARAMETER Platform
    Target platform: android | ios | maccatalyst | catalyst | windows

.PARAMETER Category
    Optional category name to pass to BuildAndRunHostApp.ps1 -Category

.PARAMETER TestFilter
    Optional NUnit/xUnit filter to pass to BuildAndRunHostApp.ps1 -TestFilter

.PARAMETER MaxAttempts
    Maximum retry attempts on environment errors (default: 3)

.PARAMETER RetryDelaySeconds
    Sleep between retries (default: 30)

.PARAMETER DeviceUdid
    Optional pre-booted device UDID. When omitted, this script boots one via
    Start-Emulator.ps1 (Android/iOS only).

.PARAMETER LogFile
    Optional path to capture full stdout for downstream parsing.

.OUTPUTS
    Hashtable:
      Output      : raw output array (every captured element preserved
                    line-by-line — multi-line ErrorRecords are split so
                    downstream parsers see one element per actual line)
      ExitCode    : final attempt's $LASTEXITCODE
      Attempts    : number of attempts made
      EnvErrorHit : last env-error pattern matched (or $null if none)
      EnvErrorHistory : ordered list of every attempt's env-error (lets a caller
                    tell a crash-driven timeout — e.g. repeated app crashes then a
                    final 'timeout' — from a genuinely long-running one)
      DeviceUdid  : the device UDID used (caller may want to share/reset)
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)] [string] $Platform,
    [string]   $Category,
    [string]   $TestFilter,
    [int]      $MaxAttempts = 3,
    [int]      $RetryDelaySeconds = 30,
    [string]   $DeviceUdid,
    [string]   $LogFile,
    [string]   $RepoRoot,
    # Hard wall-clock budget for the WHOLE category (build + deploy + run,
    # across all retry attempts). 0 = unlimited (default / back-compat for the
    # Gate path). When > 0, each BuildAndRunHostApp.ps1 attempt runs in a child
    # process that is tree-killed (dotnet/gradle/adb/java descendants included)
    # the moment the budget is exhausted — so a hung build/run can never block
    # until the caller's AzDO task timeout. A hard-kill is treated as a
    # retryable environment error. See dotnet/maui deep-UI-test loop.
    [int]      $TimeoutMinutes = 0,
    # Idle (no-progress) timeout in minutes for a single BuildAndRunHostApp.ps1
    # attempt. 0 = disabled (default / back-compat). When > 0, an attempt that
    # emits NO new stdout/stderr for this long is tree-killed as a hang — this
    # catches a deadlocked build/test far faster than $TimeoutMinutes and lets
    # the wall-clock budget be raised so a large-but-progressing category (e.g.
    # the ~391-test CollectionView on the slow mac pool) can run to completion.
    [int]      $IdleTimeoutMinutes = 0
)

$ErrorActionPreference = 'Continue'

function ConvertTo-AzdoSafeConsole {
    param([string]$Text)

    # Captured build/test output is PR-controlled. Collapse line separators so an
    # embedded directive cannot create a new column-zero command, then defang both
    # Azure logging-command prefixes before writing the text back to the pipeline log.
    return ($Text -replace '[\r\n\f\v]+', ' ') -replace '##(?=\[|vso\[)', '## '
}

if (-not $RepoRoot) {
    $RepoRoot = git rev-parse --show-toplevel 2>$null
    if (-not $RepoRoot) { $RepoRoot = (Get-Location).Path }
}

$sharedUtils = Join-Path $PSScriptRoot 'shared-utils.ps1'
if (-not (Test-Path -LiteralPath $sharedUtils -PathType Leaf)) {
    throw "Shared UI-test utilities not found at $sharedUtils"
}
. $sharedUtils

# ── Hard-timeout helpers (only used when -TimeoutMinutes > 0) ──────────────
function Invoke-AndroidDiagnosticCommand {
    param(
        [string[]]$SerialArguments,
        [Parameter(Mandatory = $true)][string[]]$CommandArguments,
        [int]$TimeoutSeconds = 15
    )

    try {
        $result = Invoke-ProcessWithTimeout `
            -FilePath 'adb' `
            -ArgumentList @($SerialArguments + $CommandArguments) `
            -TimeoutSeconds $TimeoutSeconds
        if ($result.TimedOut) {
            Write-Host "  (adb $($CommandArguments -join ' ') exceeded ${TimeoutSeconds}s and was killed)"
        }
        return $result
    }
    catch {
        Write-Host "  (adb $($CommandArguments -join ' ') failed: $_)"
        return [pscustomobject]@{
            Output = @()
            ExitCode = -1
            TimedOut = $false
        }
    }
}

# Best-effort on-device diagnostics for a HUNG Android deep run. When the
# HostApp ANRs or never renders the gallery (the "GoToTestButton never appears"
# symptom), BuildAndRunHostApp hangs until the hard timeout and the tree-kill
# below tears the harness down (SIGKILL) before NUnit teardown can capture
# anything — leaving only appium.log with no logcat/screenshot of the stuck
# screen, so the hang is a black box. Capturing logcat + a screenshot + the UI
# hierarchy here is what makes these hangs diagnosable. Android-only and fully
# guarded: if adb is absent or no device is connected (iOS / Catalyst / Windows
# deep jobs) this is a silent no-op. Files use names the per-category loop
# already ships in the drop-deep-uitests artifact (android-device.log, *.png,
# *.xml).
function Save-AndroidHangDiagnostics {
    param(
        [string]$RepoRoot,
        [int]$Attempt,
        [int]$CommandTimeoutSeconds = 15
    )

    try {
        if (-not (Get-Command adb -ErrorAction SilentlyContinue)) { return }
        $serial = @()
        if ($env:DEVICE_UDID) { $serial = @('-s', $env:DEVICE_UDID) }
        # Only proceed if a device is actually online (keeps this a no-op off Android).
        $state = Invoke-AndroidDiagnosticCommand `
            -SerialArguments $serial `
            -CommandArguments @('get-state') `
            -TimeoutSeconds $CommandTimeoutSeconds
        if ($state.ExitCode -ne 0 -or (($state.Output -join '').Trim() -ne 'device')) { return }
        if (-not $RepoRoot) { $RepoRoot = (Get-Location).Path }
        $diagDir = Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/hang-diagnostics/attempt-$Attempt"
        New-Item -ItemType Directory -Force -Path $diagDir | Out-Null
        Write-Host "  Capturing bounded Android hang diagnostics for attempt $Attempt after process-tree termination…"
        # 1) logcat tail — android-device.log is in the loop's copy allowlist.
        $logcat = Invoke-AndroidDiagnosticCommand `
            -SerialArguments $serial `
            -CommandArguments @('logcat', '-d', '-v', 'time', '-t', '5000') `
            -TimeoutSeconds $CommandTimeoutSeconds
        if ($logcat.Output.Count -gt 0) {
            $logcat.Output | Out-File (Join-Path $diagDir 'android-device.log') -Encoding utf8
        }
        # 2) screenshot of the stuck screen — screencap to device then pull
        #    (avoids corrupting binary PNG bytes over a text stdout pipe).
        $remoteScreenshot = "/sdcard/maui-hang-$PID-$Attempt.png"
        $screenshot = Invoke-AndroidDiagnosticCommand `
            -SerialArguments $serial `
            -CommandArguments @('shell', 'screencap', '-p', $remoteScreenshot) `
            -TimeoutSeconds $CommandTimeoutSeconds
        if ($screenshot.ExitCode -eq 0) {
            [void](Invoke-AndroidDiagnosticCommand `
                -SerialArguments $serial `
                -CommandArguments @('pull', $remoteScreenshot, (Join-Path $diagDir 'hang-screenshot.png')) `
                -TimeoutSeconds $CommandTimeoutSeconds)
        }
        [void](Invoke-AndroidDiagnosticCommand `
            -SerialArguments $serial `
            -CommandArguments @('shell', 'rm', '-f', $remoteScreenshot) `
            -TimeoutSeconds $CommandTimeoutSeconds)
        # 3) UI hierarchy — reveals whether an ANR dialog or a blank page (no
        #    GoToTestButton) is on screen at the moment of the hang.
        $remoteHierarchy = "/sdcard/maui-hang-$PID-$Attempt.xml"
        $hierarchy = Invoke-AndroidDiagnosticCommand `
            -SerialArguments $serial `
            -CommandArguments @('shell', 'uiautomator', 'dump', $remoteHierarchy) `
            -TimeoutSeconds $CommandTimeoutSeconds
        if ($hierarchy.ExitCode -eq 0) {
            [void](Invoke-AndroidDiagnosticCommand `
                -SerialArguments $serial `
                -CommandArguments @('pull', $remoteHierarchy, (Join-Path $diagDir 'hang-window.xml')) `
                -TimeoutSeconds $CommandTimeoutSeconds)
        }
        [void](Invoke-AndroidDiagnosticCommand `
            -SerialArguments $serial `
            -CommandArguments @('shell', 'rm', '-f', $remoteHierarchy) `
            -TimeoutSeconds $CommandTimeoutSeconds)
    } catch {
        Write-Host "  (Android hang-diagnostic capture failed: $_)"
    }
}

# When a deep attempt fails, the child BuildAndRunHostApp.ps1 process had its
# stdout/stderr redirected to files (so this wrapper can watch them for idle
# detection — see Invoke-BuildScriptBounded). That means the pipeline console
# (the per-category loop log) only ever saw heartbeats, so a build or test
# failure looks like a *silent* `exit N` with no error text. Echo the tail of
# the captured output here so the real cause (an MSBuild `error XXNNNN`, a test
# assertion, an app crash) is visible directly in the log, without depending on
# the (often skipped) deep-uitests artifact download.
function Write-CapturedFailureOutput {
    param(
        [string[]] $Output,
        [int]      $ExitCode,
        [int]      $Attempt,
        [int]      $TailLines = 250
    )
    $lines = @($Output | ForEach-Object { "$_" })
    if ($lines.Count -eq 0) {
        Write-Host "  (attempt $Attempt exited $ExitCode but produced no captured output)" -ForegroundColor Yellow
        return
    }
    $omitted = $lines.Count - $TailLines
    $suffix  = if ($omitted -gt 0) { ", last $TailLines of $($lines.Count) lines" } else { "" }
    Write-Host "##[group]BuildAndRunHostApp attempt $Attempt output (exit $ExitCode$suffix)" -ForegroundColor Yellow
    if ($omitted -gt 0) {
        Write-Host "  … $omitted earlier line(s) omitted — see the published deep-uitests log for the full output …"
        $lines = $lines[-$TailLines..-1]
    }
    foreach ($l in $lines) { Write-Host (ConvertTo-AzdoSafeConsole $l) }
    Write-Host "##[endgroup]"
}

# Run BuildAndRunHostApp.ps1 in a child pwsh process with a hard wall-clock
# deadline. Returns @{ Output = [string[]]; ExitCode = int; TimedOut = bool }.
function Invoke-BuildScriptBounded {
    param(
        [string]    $ScriptPath,
        [hashtable] $Params,
        [int]       $TimeoutSeconds,
        # When > 0, tree-kill the child if it emits no new stdout/stderr for this
        # many seconds (a hang), even if $TimeoutSeconds has not elapsed. Lets the
        # wall-clock budget stay generous for slow-but-progressing categories.
        [int]       $IdleTimeoutSeconds = 0,
        # Extra files/dirs whose growth ALSO counts as progress for idle detection.
        # VSTest's `dotnet test` console output is block-buffered when stdout is
        # redirected to a file, so a healthy, actively-running category (especially
        # Windows/WinAppDriver) can go far longer than $IdleTimeoutSeconds without
        # the redirected stdout file growing — a FALSE "hang" that gets it killed
        # mid-run with zero results. The Appium log and the TRX/screenshot output
        # DO grow in real time (Appium writes its own log on every WinAppDriver
        # request; screenshots/TRX land as tests advance), so watching them next to
        # stdout keeps a genuinely-progressing run alive to the wall-clock budget.
        [string[]]  $LivenessPaths = @(),
        # When > 0, abort the child EARLY (before the wall/idle budget) once its
        # stdout shows this many "did not recover after crash-recovery attempts"
        # app-crash messages. A crash-looping app keeps emitting output (every
        # doomed fixture writes logcat + a screenshot every ~4 min), so the idle
        # detector never fires and a single `dotnet test` invocation grinds through
        # every remaining fixture — 48-157 identical env-failures — until the whole
        # category budget is spent (observed #36553: Button/Label/Layout each ate
        # ~99 min producing zero usable results). The app's own crash-recovery
        # (force-stop + relaunch) can NOT clear a wedged emulator; only the
        # per-attempt device reboot in the retry loop below can. Aborting early lets
        # that reboot actually run (attempt 1 no longer consumes the entire budget)
        # and frees the remaining time for the next category. A healthy run emits
        # ZERO of these, so any run reaching the threshold is unambiguously wedged.
        [int]       $CrashLoopAbortThreshold = 0,
        [int]       $Attempt = 1
    )
    $pwshExe = try { (Get-Process -Id $PID).Path } catch { $null }
    if (-not $pwshExe) { $pwshExe = 'pwsh' }
    $argList = @('-NoProfile', '-NonInteractive', '-File', $ScriptPath)
    foreach ($k in $Params.Keys) {
        $v = $Params[$k]
        if ($null -eq $v) { continue }
        if ($v -is [bool] -or $v -is [switch]) { if ($v) { $argList += "-$k" } }
        else { $argList += @("-$k", "$v") }
    }
    $outFile = [IO.Path]::GetTempFileName()
    $errFile = [IO.Path]::GetTempFileName()
    $timedOut = $false
    $exit = -1
    $start = Get-Date
    $proc = $null
    $outStream = $null
    $errStream = $null
    $stdoutCopy = $null
    $stderrCopy = $null
    $processStarted = $false
    try {
        $startInfo = [Diagnostics.ProcessStartInfo]::new()
        $startInfo.FileName = $pwshExe
        $startInfo.UseShellExecute = $false
        $startInfo.CreateNoWindow = $true
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        foreach ($argument in $argList) {
            [void]$startInfo.ArgumentList.Add([string]$argument)
        }

        $outStream = [IO.File]::Open(
            $outFile,
            [IO.FileMode]::Create,
            [IO.FileAccess]::Write,
            [IO.FileShare]::ReadWrite)
        $errStream = [IO.File]::Open(
            $errFile,
            [IO.FileMode]::Create,
            [IO.FileAccess]::Write,
            [IO.FileShare]::ReadWrite)
        $proc = [Diagnostics.Process]::new()
        $proc.StartInfo = $startInfo
        if (-not $proc.Start()) {
            throw "Failed to start bounded BuildAndRunHostApp process."
        }
        $processStarted = $true
        $stdoutCopy = $proc.StandardOutput.BaseStream.CopyToAsync($outStream)
        $stderrCopy = $proc.StandardError.BaseStream.CopyToAsync($errStream)
        $deadline = $start.AddSeconds($TimeoutSeconds)
        $lastBeat = $start
        # Progress tracking: a hung run stops writing to stdout/stderr, whereas a
        # slow-but-healthy category keeps emitting test results. We watch the
        # redirect files' byte length and reset the idle clock whenever they grow,
        # so a genuine hang is killed after $IdleTimeoutSeconds of silence while a
        # large category that is still producing output runs to the wall-clock cap.
        $lastProgressAt = $start
        $lastLen = -1L
        $killReason = $null
        # Crash-loop early-abort tracking (see $CrashLoopAbortThreshold above). The
        # signature is emitted by UtilExtensions.WaitForGoToTestButtonWithRecovery
        # once a fixture's OneTimeSetup exhausts the app's internal crash-recovery.
        $crashLoopSig    = 'did not recover after crash-recovery attempts'
        $crashLoopHits   = 0
        $lastCrashScanLen = -1L
        while (-not $proc.HasExited) {
            Start-Sleep -Seconds 5
            $now = Get-Date
            $curLen = 0L
            foreach ($f in @($outFile, $errFile)) {
                try { if (Test-Path $f) { $curLen += [int64](Get-Item $f -ErrorAction SilentlyContinue).Length } } catch { }
            }
            # Count growth of the live UI-test artifacts (Appium log, screenshots,
            # TRX) too, so a buffered-but-healthy `dotnet test` — whose console
            # output the OS holds back because stdout is redirected to a file — is
            # not mistaken for a hang. Appium appends to its log on every request,
            # so this grows continuously while tests actually run.
            foreach ($lp in $LivenessPaths) {
                try {
                    if (Test-Path -LiteralPath $lp -PathType Leaf) {
                        $curLen += [int64](Get-Item -LiteralPath $lp -ErrorAction SilentlyContinue).Length
                    } elseif (Test-Path -LiteralPath $lp -PathType Container) {
                        foreach ($cf in (Get-ChildItem -LiteralPath $lp -File -ErrorAction SilentlyContinue)) {
                            $curLen += [int64]$cf.Length
                        }
                    }
                } catch { }
            }
            if ($curLen -gt $lastLen) { $lastLen = $curLen; $lastProgressAt = $now }
            # Early-abort a wedged app: rescan stdout for the crash-recovery
            # signature only when the streams grew (cheap, and we stop the instant
            # the threshold is met). $outFile holds the child's dotnet-test stdout,
            # where each failed-fixture recovery prints the signature.
            if ($CrashLoopAbortThreshold -gt 0 -and $curLen -gt $lastCrashScanLen) {
                $lastCrashScanLen = $curLen
                try {
                    $so = ''
                    if (Test-Path $outFile) {
                        # Open share-read/write so we never contend with the child's
                        # redirected-stdout writer (matters on Windows; a no-op on the
                        # Linux android agents where this crash-loop actually occurs).
                        $fs = [IO.File]::Open($outFile, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
                        try { $sr = New-Object IO.StreamReader($fs); $so = $sr.ReadToEnd(); $sr.Dispose() } finally { $fs.Dispose() }
                    }
                    if ($so) {
                        $crashLoopHits = ([regex]::Matches($so, [regex]::Escape($crashLoopSig))).Count
                        if ($crashLoopHits -ge $CrashLoopAbortThreshold) { $killReason = 'crashloop'; break }
                    }
                } catch { }
            }
            if ($now -ge $deadline) { $killReason = 'budget'; break }
            if ($IdleTimeoutSeconds -gt 0 -and (($now - $lastProgressAt).TotalSeconds -ge $IdleTimeoutSeconds)) { $killReason = 'idle'; break }
            if (($now - $lastBeat).TotalSeconds -ge 120) {
                $elapsedMin = [int]($now - $start).TotalMinutes
                $idleMin    = [int]($now - $lastProgressAt).TotalMinutes
                $idleNote   = if ($IdleTimeoutSeconds -gt 0) { ", idle $idleMin min (kill at $([int]($IdleTimeoutSeconds/60)))" } else { "" }
                Write-Host "  … BuildAndRunHostApp still running ($elapsedMin min elapsed, wall budget $([int]($TimeoutSeconds/60)) min$idleNote)"
                $lastBeat = $now
            }
        }
        if (-not $proc.HasExited) {
            if ($killReason -eq 'idle') {
                Write-Host "##[warning]No test progress for $([int]($IdleTimeoutSeconds/60)) min — killing hung BuildAndRunHostApp process tree (pid $($proc.Id)) [stalled $([int]((Get-Date) - $start).TotalMinutes) min in]" -ForegroundColor Yellow
            } elseif ($killReason -eq 'crashloop') {
                Write-Host "##[warning]App crash-loop detected ($crashLoopHits× '$crashLoopSig') after $([int]((Get-Date) - $start).TotalMinutes) min — aborting this category early so the retry loop can reboot the device and reclaim the budget for the remaining categories. This is an ENVIRONMENT error (wedged emulator), NOT a PR-code failure." -ForegroundColor Yellow
            } else {
                Write-Host "##[warning]Hard timeout ($([int]($TimeoutSeconds/60)) min) reached — killing BuildAndRunHostApp process tree (pid $($proc.Id))" -ForegroundColor Yellow
            }
            Stop-ProcessTree -ProcessId $proc.Id
            Save-AndroidHangDiagnostics -RepoRoot $RepoRoot -Attempt $Attempt
            for ($i = 0; $i -lt 8 -and -not $proc.HasExited; $i++) { Start-Sleep -Seconds 2 }
            if ($killReason -eq 'crashloop') {
                # Leave $timedOut = $false so the caller runs its env-error scan over
                # the captured output — which already contains the crash signature —
                # and classifies this as the 'did not recover…' env-error → device
                # reboot + retry (the designed recovery), instead of the generic
                # 'timeout' path. A non-124, non-zero exit keeps it off both the
                # "success" (0) and the "hang/timeout" (124) branches.
                $exit = 1
            } else {
                $timedOut = $true
                $exit = 124
            }
        } else {
            $exit = $proc.ExitCode
        }
    } catch {
        Write-Host "⚠️ Bounded BuildAndRunHostApp invocation threw: $_" -ForegroundColor Yellow
        $exit = -1
        if ($processStarted -and -not $proc.HasExited) {
            try { Stop-ProcessTree -ProcessId $proc.Id } catch { }
        }
    } finally {
        if ($processStarted -and -not $proc.HasExited) {
            try { [void]$proc.WaitForExit(5000) } catch { }
        }
        if ($processStarted -and $proc.HasExited) {
            try { $proc.WaitForExit() } catch { }
            foreach ($copy in @($stdoutCopy, $stderrCopy)) {
                if ($copy) {
                    try { $copy.GetAwaiter().GetResult() } catch { }
                }
            }
        }
        foreach ($stream in @($outStream, $errStream)) {
            if ($stream) {
                try { $stream.Dispose() } catch { }
            }
        }
        if ($proc) {
            try { $proc.Dispose() } catch { }
        }
    }
    $out = @()
    foreach ($f in @($outFile, $errFile)) {
        if (Test-Path $f) {
            try { $out += Get-Content -Path $f -ErrorAction SilentlyContinue } catch { }
            Remove-Item $f -Force -ErrorAction SilentlyContinue
        }
    }
    return @{ Output = $out; ExitCode = $exit; TimedOut = $timedOut }
}

# Load shared env-error patterns (single source of truth).
$sharedPatternsScript = Join-Path $PSScriptRoot "Get-EnvErrorPatterns.ps1"
if (-not (Test-Path $sharedPatternsScript)) {
    throw "Get-EnvErrorPatterns.ps1 not found at $sharedPatternsScript — env-error retry requires the shared pattern file."
}
. $sharedPatternsScript
$envErrorPatterns = Get-EnvErrorPatterns
$ambiguousStartupPatterns = Get-AmbiguousStartupPatterns

# ── Step 1: pre-boot the device once (same as Gate's Invoke-TestRun) ──────
$bootedUdid = $DeviceUdid
$emulatorPlatform = switch ($Platform) {
    'catalyst'    { $null }
    'maccatalyst' { $null }
    'windows'     { $null }
    default       { $Platform }
}

if ($emulatorPlatform -and -not $bootedUdid) {
    Write-Host "🔹 Booting $Platform device/simulator (Start-Emulator.ps1)..." -ForegroundColor Cyan
    $startEmu = Join-Path $RepoRoot ".github/scripts/shared/Start-Emulator.ps1"
    if (Test-Path $startEmu) {
        try {
            $bootedUdid = & $startEmu -Platform $emulatorPlatform
            if ($LASTEXITCODE -eq 0 -and $bootedUdid) {
                Write-Host "✅ Device ready: $bootedUdid" -ForegroundColor Green
            } else {
                Write-Host "⚠️ Start-Emulator.ps1 returned exit $LASTEXITCODE; falling back to BuildAndRunHostApp internal device boot" -ForegroundColor Yellow
                $bootedUdid = $null
            }
        } catch {
            Write-Host "⚠️ Start-Emulator.ps1 threw: $_" -ForegroundColor Yellow
            $bootedUdid = $null
        }
    } else {
        Write-Host "⚠️ Start-Emulator.ps1 not found — letting BuildAndRunHostApp.ps1 boot its own device" -ForegroundColor Yellow
    }
}

# ── Step 2: build the BuildAndRunHostApp parameter set ────────────────────
$buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
if (-not (Test-Path $buildScript)) {
    throw "BuildAndRunHostApp.ps1 not found at: $buildScript"
}

$baseParams = @{ Platform = $Platform }
if ($Category)   { $baseParams.Category   = $Category }
if ($TestFilter) { $baseParams.TestFilter = $TestFilter }
if ($bootedUdid) { $baseParams.DeviceUdid = $bootedUdid }

# ── Step 3: retry loop on environment errors (same as Gate's
#    Invoke-TestRunWithRetry, including device reboot between attempts) ───
$attempts = 0
$lastOutput = @()
$lastExit = -1
$envHit = $null
$envErrorHistory = [System.Collections.Generic.List[string]]::new()
$overallDeadline = if ($TimeoutMinutes -gt 0) { (Get-Date).AddMinutes($TimeoutMinutes) } else { $null }
$idleTimeoutSec  = if ($IdleTimeoutMinutes -gt 0) { $IdleTimeoutMinutes * 60 } else { 0 }

# Files/dirs (besides the child's redirected stdout/stderr) whose growth proves the
# category is still making progress even when `dotnet test` console output is held
# back by OS stdout buffering. The Appium log + screenshots live directly in the
# UITests dir; TRX files land in its TestResults subdir. Watching these prevents a
# false idle-kill of an actively-running run (observed on Windows/WinAppDriver:
# #36561, #33007 — Appium logged requests up to the instant of the kill, yet the
# redirected stdout file had not grown, so the run was killed with zero results).
$uiLogsDir     = Join-Path $RepoRoot 'CustomAgentLogsTmp/UITests'
$livenessPaths = @($uiLogsDir, (Join-Path $uiLogsDir 'TestResults'))

for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    $attempts = $attempt
    $recoveryVerified = $false
    # Record the PREVIOUS attempt's env-error before it is reset below, so the
    # caller sees the full ORDERED history (e.g. two "did not recover after
    # crash-recovery attempts" app-crashes followed by a final 'timeout') and
    # not just the last one. This lets the deep classifier tell a crash-driven
    # timeout (raising the budget won't help — the app keeps crashing) from a
    # genuinely long-running one (where a bigger budget would).
    if ($attempt -gt 1 -and $envHit) { [void]$envErrorHistory.Add($envHit) }
    if ($overallDeadline -and (Get-Date) -ge $overallDeadline) {
        Write-Host "##[warning]Category time budget ($TimeoutMinutes min) exhausted — stopping before attempt $attempt." -ForegroundColor Yellow
        # The terminal reason for stopping here is the budget/timeout. Set it
        # unconditionally (the just-captured prior env-error is already in
        # $envErrorHistory) so the final EnvErrorHit is 'timeout' and the
        # post-loop capture below records 'timeout' — never a duplicate of the
        # prior attempt's error.
        $envHit = 'timeout'
        if ($lastExit -eq -1) { $lastExit = 124 }
        break
    }
    if ($attempt -gt 1) {
        Write-Host "↻ Attempt $attempt/$MaxAttempts after environment error '$envHit'" -ForegroundColor Yellow

        $recoveryBudgetSeconds = 180
        if ($overallDeadline) {
            $remainingForRecovery = [int]($overallDeadline - (Get-Date)).TotalSeconds - [Math]::Max(0, $RetryDelaySeconds)
            if ($remainingForRecovery -le 0) {
                Write-Host "##[warning]Category time budget exhausted before device recovery for attempt $attempt." -ForegroundColor Yellow
                $envHit = 'timeout'
                if ($lastExit -eq -1) { $lastExit = 124 }
                break
            }
            $recoveryBudgetSeconds = [Math]::Min($recoveryBudgetSeconds, $remainingForRecovery)
        }

        $resetScript = Join-Path $PSScriptRoot 'Reset-DeviceState.ps1'
        if (Test-Path -LiteralPath $resetScript -PathType Leaf) {
            $resetOutput = @(& $resetScript `
                -Platform $Platform `
                -DeviceUdid $bootedUdid `
                -BootTimeoutSeconds $recoveryBudgetSeconds `
                -PassThruStatus)
            $recoveryVerified = (
                $resetOutput.Count -eq 1 -and
                $resetOutput[0] -is [bool] -and
                $resetOutput[0]
            )
            if (-not $recoveryVerified) {
                Write-Host "##[warning]Device recovery was not verified; preserving the remaining retry budget." -ForegroundColor Yellow
            }
        } else {
            Write-Host "##[warning]Device recovery script is missing: $resetScript" -ForegroundColor Yellow
        }
        Start-Sleep -Seconds $RetryDelaySeconds
    }

    $envHit = $null
    Write-Host "▶ BuildAndRunHostApp.ps1 attempt $attempt/$MaxAttempts" -ForegroundColor Cyan
    if ($TimeoutMinutes -gt 0) {
        $remainingSec = [int]($overallDeadline - (Get-Date)).TotalSeconds
        if ($remainingSec -le 30) {
            Write-Host "##[warning]Category time budget ($TimeoutMinutes min) exhausted before attempt $attempt — stopping." -ForegroundColor Yellow
            $envHit = 'timeout'; $lastExit = 124
            break
        }
        $bounded = Invoke-BuildScriptBounded -ScriptPath $buildScript -Params $baseParams -TimeoutSeconds $remainingSec -IdleTimeoutSeconds $idleTimeoutSec -LivenessPaths $livenessPaths -CrashLoopAbortThreshold 10 -Attempt $attempt
        $lastOutput = $bounded.Output
        $lastExit = $bounded.ExitCode
        if ($bounded.TimedOut) {
            Write-Host "##[warning]Attempt $attempt hard-killed after exhausting the category time budget." -ForegroundColor Yellow
            $envHit = 'timeout'   # treat a hang as a retryable environment error
            if ($attempt -eq $MaxAttempts) { break }
            # A hang that trips the idle-kill BEFORE the wall budget leaves a
            # sliver of budget behind, and the current loop would spend it on a
            # doomed retry: observed on #36448 Material3 — attempt 1 hung (idle
            # climbed to 24 min) and was killed at 95 of 104 min, then attempt 2
            # got the leftover 8 min and was killed again → pure waste that also
            # starves the NEXT category. Only retry a timeout when a meaningful
            # chunk (>= the idle-kill window, ~25 min) remains — enough for a real
            # rebuild + test pass after the device reboot. Otherwise stop and let
            # the loop move on so later categories keep their share.
            $remainingAfterTimeout = if ($overallDeadline) { [int]($overallDeadline - (Get-Date)).TotalSeconds } else { [int]::MaxValue }
            $minRetrySec = if ($IdleTimeoutMinutes -gt 0) { $IdleTimeoutMinutes * 60 } else { 1500 }
            if ($remainingAfterTimeout -lt $minRetrySec) {
                Write-Host "##[warning]Only $([int]($remainingAfterTimeout/60)) min left after the timeout — not retrying (too little for a real attempt); moving on to the next category." -ForegroundColor Yellow
                break
            }
            continue
        }
    } else {
        $lastOutput = & $buildScript @baseParams 2>&1
        $lastExit = $LASTEXITCODE
    }

    if ($lastExit -eq 0) { break }

    # Same env-error scan as Get-TestResultFromOutput in the Gate.
    $joined = ($lastOutput | ForEach-Object { "$_" }) -join "`n"
    foreach ($p in $envErrorPatterns) {
        if ($joined -match $p) { $envHit = $p; break }
    }
    # Ambiguous HostApp-startup signatures mean EITHER a broken emulator OR a PR that
    # deterministically breaks startup. One recovery attempt (device reboot + fresh
    # rebuild/reinstall) settles it: if the SAME signature survives that clean-device
    # recovery, it is reproducible and must be reported as a real failure instead of
    # consuming the remaining retry budget and landing as INCONCLUSIVE.
    if ($recoveryVerified -and $envHit -and ($ambiguousStartupPatterns -contains $envHit) -and ($envErrorHistory -contains $envHit)) {
        Write-Host "⚠️ Ambiguous startup failure '$envHit' recurred after device recovery — treating as a deterministic (PR-caused) failure, not infrastructure." -ForegroundColor Yellow
        # Keep the signature in the ordered history (the deep classifier uses it to tell a
        # crash-driven run from a plain slow one), but clear EnvErrorHit so the caller
        # classifies this as a genuine failure.
        [void]$envErrorHistory.Add($envHit)
        $envHit = $null
    }
    if (-not $envHit) {
        # Real (non-env) failure — surface the captured build/test output so the
        # actual error is visible in this log. Only needed on the bounded (deep)
        # path, where the child's streams were redirected to files for idle
        # detection; the Gate path (TimeoutMinutes -le 0) captures with 2>&1 and
        # surfaces output through its own reporting.
        if ($TimeoutMinutes -gt 0) { Write-CapturedFailureOutput -Output $lastOutput -ExitCode $lastExit -Attempt $attempt }
        break   # real test failure — no point retrying
    }
    if ($attempt -eq $MaxAttempts) {
        Write-Host "⚠️ Env error '$envHit' persisted after $MaxAttempts attempts" -ForegroundColor Yellow
        if ($TimeoutMinutes -gt 0) { Write-CapturedFailureOutput -Output $lastOutput -ExitCode $lastExit -Attempt $attempt }
    }
}

# Capture the FINAL attempt's env-error (the loop-top capture only records
# prior attempts). Together these give the caller the full ordered history.
if ($envHit) { [void]$envErrorHistory.Add($envHit) }

# ── Normalize the captured output: PowerShell's `& cmd 2>&1` wraps multi-line
#    stderr blocks as single ErrorRecord/string elements with embedded \n.
#    The downstream Get-DotNetTestResults regex is anchored ^...$ (start/end
#    of STRING), so without splitting, a multi-line element gets misparsed
#    and a 100+-test fixture can collapse into one bogus result with all
#    names concatenated. We split each element here so every consumer sees
#    one true line per array element. ──
$normalized = @(
    $lastOutput | ForEach-Object {
        $s = "$_"
        if ($s.Contains("`n") -or $s.Contains("`r")) {
            $s -split "`r`n|`n|`r"
        } else {
            $s
        }
    }
)

if ($LogFile) {
    try {
        $dir = Split-Path -Parent $LogFile
        if ($dir -and -not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
        $normalized | Out-File -FilePath $LogFile -Encoding utf8
    } catch {
        Write-Host "⚠️ Failed to write $LogFile : $_" -ForegroundColor Yellow
    }
}

# ── Surface the TRX path so STEP 3 can parse authoritative test results ──
# BuildAndRunHostApp.ps1 prints a marker line `>>> TRX_RESULT_FILE: <path>`
# (matching the format `RunTestWithLocalDotNet` would have produced via
# Cake). Pull it out here so callers don't have to re-scan the output.
$trxResultFile = $null
foreach ($line in $normalized) {
    $s = "$line"
    if ($s -match '^\s*>>>\s*TRX_RESULT_FILE:\s*(.+?)\s*$') {
        $candidate = $matches[1].Trim()
        if (Test-Path $candidate) {
            $trxResultFile = $candidate
        }
    }
}

return @{
    Output        = $normalized
    ExitCode      = $lastExit
    Attempts      = $attempts
    EnvErrorHit   = $envHit
    EnvErrorHistory = @($envErrorHistory)
    DeviceUdid    = $bootedUdid
    TrxResultFile = $trxResultFile
}
