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
    [string]   $RepoRoot
)

$ErrorActionPreference = 'Continue'

if (-not $RepoRoot) {
    $RepoRoot = git rev-parse --show-toplevel 2>$null
    if (-not $RepoRoot) { $RepoRoot = (Get-Location).Path }
}

# Load shared env-error patterns (single source of truth).
$sharedPatternsScript = Join-Path $PSScriptRoot "Get-EnvErrorPatterns.ps1"
if (-not (Test-Path $sharedPatternsScript)) {
    throw "Get-EnvErrorPatterns.ps1 not found at $sharedPatternsScript — env-error retry requires the shared pattern file."
}
. $sharedPatternsScript
$envErrorPatterns = Get-EnvErrorPatterns

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

for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
    $attempts = $attempt
    if ($attempt -gt 1) {
        Write-Host "↻ Attempt $attempt/$MaxAttempts after environment error '$envHit'" -ForegroundColor Yellow

        # Same recovery as Gate's Invoke-TestRunWithRetry
        if ($Platform -eq 'android') {
            try {
                Write-Host "🔄 adb reboot to recover" -ForegroundColor Yellow
                if ($bootedUdid) {
                    & adb -s $bootedUdid reboot 2>$null | Out-Null
                    & adb -s $bootedUdid wait-for-device 2>$null | Out-Null
                } else {
                    & adb reboot 2>$null | Out-Null
                    & adb wait-for-device 2>$null | Out-Null
                }
            } catch {
                Write-Host "(adb reboot failed: $_)" -ForegroundColor DarkGray
            }
        } elseif ($Platform -in @('ios','catalyst','maccatalyst')) {
            $sim = $bootedUdid
            if (-not $sim) {
                try {
                    $boot = & xcrun simctl list devices booted 2>$null | Select-String -Pattern '\(([0-9A-F-]{36})\)' | Select-Object -First 1
                    if ($boot) { $sim = $boot.Matches.Groups[1].Value }
                } catch { }
            }
            if ($sim) {
                try {
                    Write-Host "🔄 simctl shutdown/boot $sim" -ForegroundColor Yellow
                    & xcrun simctl shutdown $sim 2>$null | Out-Null
                    Start-Sleep -Seconds 5
                    & xcrun simctl boot $sim 2>$null | Out-Null
                } catch {
                    Write-Host "(simctl reboot failed: $_)" -ForegroundColor DarkGray
                }
            }
        }
        Start-Sleep -Seconds $RetryDelaySeconds
    }

    $envHit = $null
    Write-Host "▶ BuildAndRunHostApp.ps1 attempt $attempt/$MaxAttempts" -ForegroundColor Cyan
    $lastOutput = & $buildScript @baseParams 2>&1
    $lastExit = $LASTEXITCODE

    if ($lastExit -eq 0) { break }

    # Same env-error scan as Get-TestResultFromOutput in the Gate.
    $joined = ($lastOutput | ForEach-Object { "$_" }) -join "`n"
    foreach ($p in $envErrorPatterns) {
        if ($joined -match $p) { $envHit = $p; break }
    }
    if (-not $envHit) { break }   # real test failure — no point retrying
    if ($attempt -eq $MaxAttempts) {
        Write-Host "⚠️ Env error '$envHit' persisted after $MaxAttempts attempts" -ForegroundColor Yellow
    }
}

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
    DeviceUdid    = $bootedUdid
    TrxResultFile = $trxResultFile
}
