<#
.SYNOPSIS
    Selects and runs the minimum safe set of tests for a PR using Copilot CLI.

.DESCRIPTION
    This script:
    1. Fetches PR metadata (changed files, title, description, labels)
    2. Runs deterministic file-path → test mapping
    3. Invokes Copilot CLI with the smart-test-selector agent for AI refinement
    4. Outputs the selected test scope as JSON
    5. Optionally executes the selected tests

    Falls back to ALL tests if Copilot CLI fails or confidence is low.

.PARAMETER PRNumber
    The GitHub PR number to analyze.

.PARAMETER Platform
    Target platform for UI/device tests. Default: 'android'.
    Valid values: android, ios, all

.PARAMETER RunTests
    If specified, actually builds and runs the selected tests.
    Without this flag, only outputs the selection.

.PARAMETER ForceAll
    If specified, skips analysis and runs ALL tests.

.PARAMETER OutputFile
    Path to write the selection JSON. Default: selection.json in artifacts dir.

.PARAMETER LogFile
    If provided, captures all output via Start-Transcript.

.EXAMPLE
    .\Select-SmartTests.ps1 -PRNumber 33432
    Analyzes PR #33432 and outputs selected test scope.

.EXAMPLE
    .\Select-SmartTests.ps1 -PRNumber 33432 -Platform ios -RunTests
    Analyzes PR #33432 and runs selected tests on iOS.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'all')]
    [string]$Platform = 'android',

    [Parameter(Mandatory = $false)]
    [switch]$RunTests,

    [Parameter(Mandatory = $false)]
    [switch]$ForceAll,

    [Parameter(Mandatory = $false)]
    [string]$OutputFile,

    [Parameter(Mandatory = $false)]
    [string]$LogFile
)

$ErrorActionPreference = 'Stop'

if ($LogFile) {
    $logDir = Split-Path $LogFile -Parent
    if ($logDir -and -not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Start-Transcript -Path $LogFile -Force | Out-Null
}

# ──────────────────────────────────────────────────────
# CONFIGURATION
# ──────────────────────────────────────────────────────

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) {
    Write-Error "Not in a git repository"
    exit 1
}

# Artifacts directory
$ArtifactsDir = if ($env:BUILD_ARTIFACTSTAGINGDIRECTORY) {
    "$env:BUILD_ARTIFACTSTAGINGDIRECTORY/smart-tests"
} else {
    "$RepoRoot/CustomAgentLogsTmp/SmartTests"
}
if (-not (Test-Path $ArtifactsDir)) {
    New-Item -ItemType Directory -Path $ArtifactsDir -Force | Out-Null
}

if (-not $OutputFile) {
    $OutputFile = Join-Path $ArtifactsDir "selection.json"
}

# ──────────────────────────────────────────────────────
# HELPERS
# ──────────────────────────────────────────────────────

function Extract-JsonFromOutput {
    param([string]$RawOutput)
    # Try code block first (```json ... ```)
    $codeBlock = [regex]::Match($RawOutput, '```(?:json)?\s*(\{[\s\S]*?\})\s*```')
    if ($codeBlock.Success) { return $codeBlock.Groups[1].Value }
    # Fall back to first balanced { ... } (non-greedy via lazy quantifier won't work for nested, so find first valid JSON)
    $firstBrace = $RawOutput.IndexOf('{')
    if ($firstBrace -ge 0) {
        $depth = 0; $inStr = $false; $escape = $false
        for ($i = $firstBrace; $i -lt $RawOutput.Length; $i++) {
            $c = $RawOutput[$i]
            if ($escape) { $escape = $false; continue }
            if ($c -eq '\') { $escape = $true; continue }
            if ($c -eq '"') { $inStr = -not $inStr; continue }
            if ($inStr) { continue }
            if ($c -eq '{') { $depth++ } elseif ($c -eq '}') { $depth--; if ($depth -eq 0) { return $RawOutput.Substring($firstBrace, $i - $firstBrace + 1) } }
        }
    }
    return $null
}

# ──────────────────────────────────────────────────────
# RESILIENCE: DEVICE HEALTH, FLAKY TESTS, RETRY, AI ANALYSIS
# ──────────────────────────────────────────────────────

function Test-DeviceHealth {
    param(
        [Parameter(Mandatory)]
        [ValidateSet("android", "ios", "catalyst")]
        [string]$Platform
    )

    $deadline = (Get-Date).AddSeconds(180)
    $issues = [System.Collections.Generic.List[string]]::new()
    $actions = [System.Collections.Generic.List[string]]::new()
    $deviceId = ""

    function Test-TimedOut { return (Get-Date) -ge $deadline }

    function Clear-AppiumPort {
        try {
            $pids = bash -c "lsof -i :4723 2>/dev/null | grep LISTEN | awk '{print `$2}'" 2>$null
            if ($pids) {
                foreach ($p in ($pids -split "`n" | Where-Object { $_ -match '^\d+$' })) {
                    Write-Host "  ⚠️ Killing stale Appium process (PID $p) on port 4723"
                    kill -9 $p 2>$null
                    $actions.Add("Killed stale Appium PID $p")
                }
                Start-Sleep -Seconds 1
                $stillListening = bash -c "lsof -i :4723 2>/dev/null | grep LISTEN" 2>$null
                if ($stillListening) { $issues.Add("Port 4723 still occupied after kill"); return $false }
            }
            Write-Host "  ✅ Appium port 4723 is free"
            return $true
        } catch {
            $issues.Add("Failed to check Appium port: $_")
            return $false
        }
    }

    if ($Platform -eq "android") {
        if (-not (Get-Command "adb" -ErrorAction SilentlyContinue)) {
            $issues.Add("adb not found in PATH")
            return @{ Healthy = $false; Platform = $Platform; DeviceId = ""; Issues = $issues.ToArray(); Actions = $actions.ToArray() }
        }

        $runningDevices = adb devices 2>$null | Select-String "^emulator-\d+\s+device"
        if ($runningDevices.Count -gt 0) {
            $deviceId = ($runningDevices[0].Line -split '\s+')[0]
            Write-Host "  ✅ Android emulator: $deviceId"
            $bootDone = adb -s $deviceId shell getprop sys.boot_completed 2>$null
            if ($bootDone -notmatch "1") {
                Write-Host "  ⚠️ Waiting for boot..."
                $actions.Add("Waited for emulator boot")
                while (-not (Test-TimedOut)) {
                    Start-Sleep -Seconds 5
                    $bootDone = adb -s $deviceId shell getprop sys.boot_completed 2>$null
                    if ($bootDone -match "1") { Write-Host "  ✅ Boot completed"; break }
                }
                if ($bootDone -notmatch "1") { $issues.Add("Emulator did not finish booting") }
            }
        } else {
            Write-Host "  ⚠️ No emulator — trying Start-Emulator.ps1"
            $actions.Add("Starting emulator via Start-Emulator.ps1")
            $startScript = Join-Path $RepoRoot ".github/scripts/shared/Start-Emulator.ps1"
            if (Test-Path $startScript) {
                try {
                    & pwsh $startScript -Platform android 2>&1 | ForEach-Object { Write-Host "     $_" }
                    $runningDevices = adb devices 2>$null | Select-String "^emulator-\d+\s+device"
                    if ($runningDevices.Count -gt 0) {
                        $deviceId = ($runningDevices[0].Line -split '\s+')[0]
                        Write-Host "  ✅ Emulator started: $deviceId"
                    } else { $issues.Add("Start-Emulator.ps1 ran but no emulator appeared") }
                } catch { $issues.Add("Start-Emulator.ps1 failed: $_") }
            } else { $issues.Add("Start-Emulator.ps1 not found and no running emulator") }
        }
        if (-not (Test-TimedOut)) { if (-not (Clear-AppiumPort)) { $issues.Add("Could not free Appium port") } }
    }
    elseif ($Platform -eq "ios" -or $Platform -eq "catalyst") {
        if (-not (Get-Command "xcrun" -ErrorAction SilentlyContinue)) {
            $issues.Add("xcrun not found")
            return @{ Healthy = $false; Platform = $Platform; DeviceId = ""; Issues = $issues.ToArray(); Actions = $actions.ToArray() }
        }

        $bootedRaw = xcrun simctl list devices booted --json 2>$null | ConvertFrom-Json
        $bootedDevices = @()
        if ($bootedRaw -and $bootedRaw.devices) {
            $bootedDevices = $bootedRaw.devices.PSObject.Properties.Value | ForEach-Object { $_ } | Where-Object { $_.state -eq "Booted" }
        }

        if ($bootedDevices.Count -gt 0) {
            $sim = $bootedDevices | Select-Object -First 1
            $deviceId = $sim.udid
            Write-Host "  ✅ Booted simulator: $($sim.name) ($deviceId)"
        } else {
            Write-Host "  ⚠️ No booted simulator — booting one"
            $actions.Add("Booting simulator")
            $allRaw = xcrun simctl list devices available --json 2>$null | ConvertFrom-Json
            $targetSim = $null
            if ($allRaw -and $allRaw.devices) {
                $allSims = $allRaw.devices.PSObject.Properties | ForEach-Object {
                    $_.Value | Where-Object { $_.isAvailable -eq $true }
                }
                $targetSim = $allSims | Where-Object { $_.name -eq "iPhone Xs" } | Select-Object -First 1
                if (-not $targetSim) { $targetSim = $allSims | Where-Object { $_.name -match "^iPhone" } | Select-Object -First 1 }
            }
            if ($targetSim) {
                xcrun simctl boot $targetSim.udid 2>$null
                $actions.Add("Booted $($targetSim.name)")
                $bootWait = 0
                while ($bootWait -lt 60 -and -not (Test-TimedOut)) {
                    Start-Sleep -Seconds 3; $bootWait += 3
                    $dev = xcrun simctl list devices --json 2>$null | ConvertFrom-Json
                    $check = $dev.devices.PSObject.Properties.Value | ForEach-Object { $_ } | Where-Object { $_.udid -eq $targetSim.udid }
                    if ($check.state -eq "Booted") { $deviceId = $targetSim.udid; Write-Host "  ✅ Simulator booted"; break }
                }
                if (-not $deviceId) { $issues.Add("Simulator did not boot in time") }
            } else { $issues.Add("No available iPhone simulator found") }
        }

        # Verify responsiveness
        if ($deviceId -and -not (Test-TimedOut)) {
            $responsive = $false
            for ($r = 0; $r -lt 3; $r++) {
                $null = xcrun simctl spawn $deviceId launchctl print system 2>&1
                if ($LASTEXITCODE -eq 0) { $responsive = $true; break }
                Start-Sleep -Seconds 3
            }
            if (-not $responsive) { $issues.Add("Simulator booted but unresponsive") }
        }
        if (-not (Test-TimedOut)) { if (-not (Clear-AppiumPort)) { $issues.Add("Could not free Appium port") } }
    }

    $healthy = ($issues.Count -eq 0) -and ($deviceId -ne "")
    if ($healthy) { Write-Host "  ✅ Device health: PASSED" } else { Write-Host "  ❌ Device health: FAILED — $($issues -join '; ')" }
    return @{ Healthy = $healthy; Platform = $Platform; DeviceId = $deviceId; Issues = $issues.ToArray(); Actions = $actions.ToArray() }
}

function Get-KnownFlakyTests {
    param(
        [string]$SearchPath,
        [ValidateSet('Android', 'iOS', 'MacCatalyst', 'Windows')]
        [string]$Platform
    )
    if (-not $SearchPath) { $SearchPath = Join-Path $RepoRoot 'src/Controls/tests/TestCases.Shared.Tests' }
    if (-not (Test-Path $SearchPath)) { return @{} }

    $attrPlatformMap = @{
        'FlakyTest'                                = @('Android', 'iOS', 'MacCatalyst', 'Windows')
        'FailsOnAllPlatformsWhenRunningOnXamarinUITest' = @('Android', 'iOS', 'MacCatalyst', 'Windows')
        'FailsOnAllPlatforms'                      = @('Android', 'iOS', 'MacCatalyst', 'Windows')
        'FailsOnAndroidWhenRunningOnXamarinUITest' = @('Android')
        'FailsOnAndroid'                           = @('Android')
        'FailsOnIOSWhenRunningOnXamarinUITest'     = @('iOS')
        'FailsOnIOS'                               = @('iOS')
        'FailsOnMacWhenRunningOnXamarinUITest'     = @('MacCatalyst')
        'FailsOnMac'                               = @('MacCatalyst')
        'FailsOnWindowsWhenRunningOnXamarinUITest' = @('Windows')
        'FailsOnWindows'                           = @('Windows')
    }

    $attrNames = ($attrPlatformMap.Keys | Sort-Object -Descending { $_.Length }) -join '|'
    $pattern = '^\s*\[(?<attr>' + $attrNames + ')(?:\((?<args>[^]]*)\))?\]'
    $classPattern = '^\s*(?:public\s+)?(?:partial\s+)?class\s+(?<name>\w+)'
    $results = @{}

    foreach ($file in (Get-ChildItem -Path $SearchPath -Filter '*.cs' -Recurse -ErrorAction SilentlyContinue)) {
        try { $lines = Get-Content -Path $file.FullName -ErrorAction Stop } catch { continue }
        $currentClass = $null; $inBlock = $false
        foreach ($line in $lines) {
            if ($inBlock) { if ($line -match '\*/') { $inBlock = $false }; continue }
            if ($line -match '/\*' -and $line -notmatch '\*/') { $inBlock = $true; continue }
            if ($line -match '^\s*//') { continue }
            if ($line -match $classPattern) { $currentClass = $Matches['name'] }
            if ($line -match $pattern) {
                $attrName = $Matches['attr']
                $msg = $null; if ($Matches['args'] -and $Matches['args'] -match '"([^"]*)"') { $msg = $Matches[1] }
                $plats = $attrPlatformMap[$attrName]
                $cls = if ($currentClass) { $currentClass } else { [IO.Path]::GetFileNameWithoutExtension($file.Name) }
                if (-not $results.ContainsKey($cls)) { $results[$cls] = @() }
                $results[$cls] += [PSCustomObject]@{ Attribute = $attrName; Message = $msg; Platforms = $plats }
            }
        }
    }

    if ($Platform) {
        $filtered = @{}
        foreach ($key in $results.Keys) {
            $matching = @($results[$key] | Where-Object { $_.Platforms -contains $Platform })
            if ($matching.Count -gt 0) { $filtered[$key] = $matching }
        }
        return $filtered
    }
    return $results
}

function Invoke-TestWithRetry {
    param(
        [string]$TestCommand,
        [string[]]$TestArgs,
        [string]$TrxOutputDir,
        [string]$TestLabel,
        [int]$MaxRetries = 2,
        [hashtable]$KnownFlaky = @{}
    )

    $allResults = @{
        Label = $TestLabel
        Attempts = @()
        FinalExitCode = 0
        FailedTests = @()
        PassedOnRetry = @()
        ConsistentFailures = @()
    }

    # Attempt 1: Run full test suite
    $trxPath = Join-Path $TrxOutputDir "${TestLabel}_attempt1.trx"
    Write-Host "  🧪 [$TestLabel] Attempt 1..." -ForegroundColor Cyan
    $output = & $TestCommand @TestArgs --logger "trx;LogFileName=$trxPath" 2>&1
    $exitCode = $LASTEXITCODE
    $allResults.Attempts += @{ Number = 1; ExitCode = $exitCode; TrxPath = $trxPath; Output = ($output | Out-String) }

    if ($exitCode -eq 0) {
        Write-Host "  ✅ [$TestLabel] Passed on first attempt" -ForegroundColor Green
        return $allResults
    }

    # Parse TRX to find failed tests
    $failedTestNames = @()
    if (Test-Path $trxPath) {
        try {
            [xml]$trx = Get-Content $trxPath
            $ns = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }
            $failedResults = Select-Xml -Xml $trx -XPath "//t:UnitTestResult[@outcome='Failed']" -Namespace $ns
            $failedTestNames = @($failedResults | ForEach-Object { $_.Node.testName })
            Write-Host "  ⚠️ [$TestLabel] $($failedTestNames.Count) test(s) failed" -ForegroundColor Yellow
        } catch {
            Write-Host "  ⚠️ [$TestLabel] Could not parse TRX — retrying all" -ForegroundColor Yellow
        }
    }

    if ($failedTestNames.Count -eq 0) {
        # Couldn't parse individual failures — report as-is
        $allResults.FinalExitCode = $exitCode
        $allResults.ConsistentFailures = @(@{ TestName = "(unknown — TRX parse failed)"; FailedAttempts = @(1); Error = ($output | Out-String).Substring(0, [Math]::Min(2000, ($output | Out-String).Length)) })
        return $allResults
    }

    # Track which attempts each test failed on
    $perTestFailures = @{}
    foreach ($name in $failedTestNames) { $perTestFailures[$name] = @(1) }

    # Retry only failed tests
    for ($retry = 2; $retry -le ($MaxRetries + 1); $retry++) {
        $retryFilter = ($failedTestNames | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
        $retryTrx = Join-Path $TrxOutputDir "${TestLabel}_attempt${retry}.trx"
        Write-Host "  🔄 [$TestLabel] Retry $($retry-1)/$MaxRetries — $($failedTestNames.Count) test(s)..." -ForegroundColor Yellow

        $retryArgs = @($TestArgs) + @('--filter', $retryFilter, '--logger', "trx;LogFileName=$retryTrx")
        $retryOutput = & $TestCommand @retryArgs 2>&1
        $retryExit = $LASTEXITCODE
        $allResults.Attempts += @{ Number = $retry; ExitCode = $retryExit; TrxPath = $retryTrx; Output = ($retryOutput | Out-String) }

        if ($retryExit -eq 0) {
            Write-Host "  ✅ [$TestLabel] All failed tests passed on retry $($retry-1)" -ForegroundColor Green
            $allResults.PassedOnRetry = $failedTestNames
            $allResults.FinalExitCode = 0
            return $allResults
        }

        # Parse retry TRX for still-failing tests
        if (Test-Path $retryTrx) {
            try {
                [xml]$retryTrxXml = Get-Content $retryTrx
                $ns = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }
                $stillFailed = Select-Xml -Xml $retryTrxXml -XPath "//t:UnitTestResult[@outcome='Failed']" -Namespace $ns
                $stillFailedNames = @($stillFailed | ForEach-Object { $_.Node.testName })
                $nowPassed = $failedTestNames | Where-Object { $_ -notin $stillFailedNames }
                $allResults.PassedOnRetry += $nowPassed
                # Track per-test failure attempts
                foreach ($name in $stillFailedNames) {
                    if (-not $perTestFailures.ContainsKey($name)) { $perTestFailures[$name] = @() }
                    $perTestFailures[$name] += $retry
                }
                $failedTestNames = $stillFailedNames
            } catch { }
        }
    }

    # Build final failure report
    foreach ($name in $failedTestNames) {
        $errorMsg = ""
        # Try to extract error from last TRX
        $lastTrx = $allResults.Attempts[-1].TrxPath
        if (Test-Path $lastTrx) {
            try {
                [xml]$trxXml = Get-Content $lastTrx
                $ns = @{ t = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010' }
                $testResult = Select-Xml -Xml $trxXml -XPath "//t:UnitTestResult[@testName='$name']" -Namespace $ns | Select-Object -First 1
                if ($testResult) {
                    $errorInfo = $testResult.Node.SelectSingleNode('t:Output/t:ErrorInfo', (New-Object System.Xml.XmlNamespaceManager($trxXml.NameTable)))
                    if (-not $errorInfo) {
                        $nsmgr = New-Object System.Xml.XmlNamespaceManager($trxXml.NameTable)
                        $nsmgr.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
                        $errorInfo = $testResult.Node.SelectSingleNode('t:Output/t:ErrorInfo', $nsmgr)
                    }
                    if ($errorInfo) { $errorMsg = $errorInfo.InnerText.Substring(0, [Math]::Min(1000, $errorInfo.InnerText.Length)) }
                }
            } catch { }
        }

        $isKnownFlaky = $false
        foreach ($cls in $KnownFlaky.Keys) {
            if ($name -match $cls) { $isKnownFlaky = $true; break }
        }

        $failedAttempts = if ($perTestFailures.ContainsKey($name)) { $perTestFailures[$name] } else { @(1) }

        $allResults.ConsistentFailures += @{
            TestName = $name
            FailedAttempts = $failedAttempts
            TotalAttempts = $allResults.Attempts.Count
            Error = $errorMsg
            IsKnownFlaky = $isKnownFlaky
        }
    }

    $allResults.FinalExitCode = 1
    $allResults.FailedTests = $failedTestNames
    Write-Host "  ❌ [$TestLabel] $($failedTestNames.Count) test(s) still failing after retries" -ForegroundColor Red
    return $allResults
}

function Invoke-FailureAnalysis {
    param(
        [array]$TestResults,
        [string[]]$ChangedFiles,
        [string]$PRTitle,
        [hashtable]$KnownFlaky,
        [hashtable]$DeviceHealth,
        [string]$Platform
    )

    # Build the failure summary for the AI
    $failureSummary = @()
    foreach ($result in $TestResults) {
        if ($result.FinalExitCode -eq 0) { continue }
        foreach ($failure in $result.ConsistentFailures) {
            $failureSummary += @{
                testName = $failure.TestName
                label = $result.Label
                error = $failure.Error
                isKnownFlaky = $failure.IsKnownFlaky
                passedOnRetry = $failure.TestName -in $result.PassedOnRetry
                totalAttempts = $result.Attempts.Count
            }
        }
    }

    if ($failureSummary.Count -eq 0) {
        return @{
            verdict = 'PASS'
            summary = 'All tests passed (some on retry)'
            failures = @()
            blockerCount = 0; flakyCount = 0; infraCount = 0; unrelatedCount = 0
            retryRecommendation = 'none'
        }
    }

    # Build flaky list summary for AI
    $flakyListStr = ""
    foreach ($cls in $KnownFlaky.Keys) {
        $attrs = ($KnownFlaky[$cls] | ForEach-Object { "[$($_.Attribute)]" }) -join ', '
        $flakyListStr += "  $cls — $attrs`n"
    }

    $prompt = @"
Analyze these test failures from a CI run for PR "$PRTitle".

Platform: $Platform
Device health: $(if ($DeviceHealth.Healthy) { 'HEALTHY' } else { "ISSUES: $($DeviceHealth.Issues -join '; ')" })

Failed tests (JSON):
$($failureSummary | ConvertTo-Json -Depth 5)

PR changed files:
$($ChangedFiles -join "`n")

Known flaky/failing tests:
$flakyListStr

Classify each failure and return ONLY a JSON object matching the test-failure-analyzer agent schema.
"@

    Write-Host "  🤖 Invoking AI failure analysis..." -ForegroundColor Cyan

    try {
        $rawOutput = & copilot --agent test-failure-analyzer -p $prompt 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($rawOutput)) {
            Write-Host "  ⚠️ AI analysis unavailable — using heuristic classification" -ForegroundColor Yellow
            return $null
        }

        $jsonStr = Extract-JsonFromOutput -RawOutput $rawOutput
        if ($jsonStr) {
            $parsed = $jsonStr | ConvertFrom-Json
            Write-Host "  ✅ AI analysis complete: $($parsed.summary)" -ForegroundColor Green
            return $parsed
        }
    } catch {
        Write-Host "  ⚠️ AI analysis failed: $_ — using heuristic classification" -ForegroundColor Yellow
    }
    return $null
}

function Get-HeuristicClassification {
    param(
        [array]$TestResults,
        [string[]]$ChangedFiles,
        [hashtable]$KnownFlaky,
        [hashtable]$DeviceHealth
    )

    $failures = @()
    $blockers = 0; $flaky = 0; $infra = 0; $unrelated = 0

    $infraPatterns = @('AppiumServer', 'socket hang up', 'ECONNREFUSED', 'session not created',
                       'device not found', 'boot_completed', 'emulator', 'XCTest')

    foreach ($result in $TestResults) {
        if ($result.FinalExitCode -eq 0) { continue }
        foreach ($f in $result.ConsistentFailures) {
            $classification = 'REGRESSION'
            $recommendation = 'BLOCK_MERGE'
            $confidence = 'medium'

            # Check infrastructure errors
            $isInfra = $false
            foreach ($pat in $infraPatterns) {
                if ($f.Error -match [regex]::Escape($pat)) { $isInfra = $true; break }
            }
            if (-not $DeviceHealth.Healthy) { $isInfra = $true }

            if ($isInfra) {
                $classification = 'INFRASTRUCTURE'; $recommendation = 'RETRY'; $confidence = 'high'; $infra++
            } elseif ($f.IsKnownFlaky -or ($f.TestName -in $result.PassedOnRetry)) {
                $classification = 'FLAKY'; $recommendation = 'IGNORE'; $confidence = 'high'; $flaky++
            } else {
                # Check if test correlates with PR changes
                $correlates = $false
                foreach ($file in $ChangedFiles) {
                    $parts = $file -split '/'
                    foreach ($part in $parts) {
                        if ($part -and $f.TestName -match $part) { $correlates = $true; break }
                    }
                    if ($correlates) { break }
                }
                if ($correlates) {
                    $classification = 'REGRESSION'; $recommendation = 'BLOCK_MERGE'; $confidence = 'high'; $blockers++
                } else {
                    $classification = 'UNRELATED'; $recommendation = 'INVESTIGATE'; $confidence = 'low'; $unrelated++
                }
            }

            $failures += @{
                testName = $f.TestName
                classification = $classification
                confidence = $confidence
                recommendation = $recommendation
                reasoning = "Heuristic classification"
            }
        }
    }

    $verdict = if ($blockers -gt 0) { 'FAIL' } elseif ($infra -gt 0) { 'RETRY' } else { 'PASS' }
    return @{
        failures = $failures
        verdict = $verdict
        summary = "$blockers regression(s), $flaky flaky, $infra infra, $unrelated unrelated"
        blockerCount = $blockers; flakyCount = $flaky; infraCount = $infra; unrelatedCount = $unrelated
        retryRecommendation = if ($infra -gt 0) { 'retry_infra_only' } else { 'none' }
    }
}

function Format-TestReport {
    param(
        [int]$PRNumber,
        [string]$PRTitle,
        [hashtable]$Selection,
        [array]$TestResults,
        $FailureAnalysis,
        [hashtable]$DeviceHealth,
        [string]$Platform
    )

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("## 🤖 Smart Test Results — PR #$PRNumber")
    [void]$sb.AppendLine("")

    # Selection summary
    [void]$sb.AppendLine("<details><summary>📋 Test Selection</summary>")
    [void]$sb.AppendLine("")
    $uiDisp = if ($Selection.uiTestCategories -contains 'ALL') { 'ALL' } else { $Selection.uiTestCategories -join ', ' }
    $unitDisp = if ($Selection.unitTestProjects -contains 'ALL') { 'ALL' } else { $Selection.unitTestProjects -join ', ' }
    [void]$sb.AppendLine("| Suite | Selected |")
    [void]$sb.AppendLine("|-------|----------|")
    [void]$sb.AppendLine("| UI Tests | $uiDisp |")
    [void]$sb.AppendLine("| Unit Tests | $unitDisp |")
    [void]$sb.AppendLine("| Platform | $Platform |")
    [void]$sb.AppendLine("| Confidence | $($Selection.confidence) |")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("</details>")
    [void]$sb.AppendLine("")

    # Device health
    if (-not $DeviceHealth.Healthy) {
        [void]$sb.AppendLine("⚠️ **Device Health Issues**: $($DeviceHealth.Issues -join '; ')")
        [void]$sb.AppendLine("")
    }

    # Results
    $totalPassed = ($TestResults | Where-Object { $_.FinalExitCode -eq 0 }).Count
    $totalFailed = ($TestResults | Where-Object { $_.FinalExitCode -ne 0 }).Count
    $totalRetried = ($TestResults | ForEach-Object { $_.PassedOnRetry } | Measure-Object).Count

    [void]$sb.AppendLine("### Results")
    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("| Metric | Count |")
    [void]$sb.AppendLine("|--------|-------|")
    [void]$sb.AppendLine("| ✅ Passed | $totalPassed suite(s) |")
    [void]$sb.AppendLine("| ❌ Failed | $totalFailed suite(s) |")
    [void]$sb.AppendLine("| 🔄 Passed on retry | $totalRetried test(s) |")
    [void]$sb.AppendLine("")

    # Failure analysis
    if ($FailureAnalysis -and $FailureAnalysis.failures.Count -gt 0) {
        $verdict = switch ($FailureAnalysis.verdict) {
            'PASS'  { '✅ PASS — all failures are flaky or unrelated' }
            'FAIL'  { '❌ FAIL — regressions detected' }
            'RETRY' { '🔄 RETRY — infrastructure issues' }
            default { "⚠️ $($FailureAnalysis.verdict)" }
        }
        [void]$sb.AppendLine("### Verdict: $verdict")
        [void]$sb.AppendLine("")
        [void]$sb.AppendLine("$($FailureAnalysis.summary)")
        [void]$sb.AppendLine("")

        if ($FailureAnalysis.blockerCount -gt 0) {
            [void]$sb.AppendLine("#### 🔴 Regressions (block merge)")
            [void]$sb.AppendLine("")
            foreach ($f in ($FailureAnalysis.failures | Where-Object { $_.classification -eq 'REGRESSION' })) {
                $name = if ($f.testName) { $f.testName } else { $f['testName'] }
                $reason = if ($f.reasoning) { $f.reasoning } else { $f['reasoning'] }
                [void]$sb.AppendLine("- **$name** — $reason")
            }
            [void]$sb.AppendLine("")
        }

        $otherCount = $FailureAnalysis.flakyCount + $FailureAnalysis.infraCount + $FailureAnalysis.unrelatedCount
        if ($otherCount -gt 0) {
            [void]$sb.AppendLine("<details><summary>ℹ️ Non-blocking failures ($otherCount)</summary>")
            [void]$sb.AppendLine("")
            foreach ($f in ($FailureAnalysis.failures | Where-Object { $_.classification -ne 'REGRESSION' })) {
                $name = if ($f.testName) { $f.testName } else { $f['testName'] }
                $cls = if ($f.classification) { $f.classification } else { $f['classification'] }
                [void]$sb.AppendLine("- [$cls] **$name**")
            }
            [void]$sb.AppendLine("")
            [void]$sb.AppendLine("</details>")
        }
    }

    return $sb.ToString()
}

# ──────────────────────────────────────────────────────
# DETERMINISTIC FILE-PATH → TEST MAPPING
# ──────────────────────────────────────────────────────

# Map source paths to UI test categories
$PathToCategoryMap = [ordered]@{
    'src/Controls/src/Core/ActivityIndicator'       = @('ActivityIndicator')
    'src/Controls/src/Core/Border'                  = @('Border')
    'src/Controls/src/Core/BoxView'                 = @('BoxView')
    'src/Controls/src/Core/Brush'                   = @('Brush')
    'src/Controls/src/Core/Button'                  = @('Button')
    'src/Controls/src/Core/Cells'                   = @('Cells')
    'src/Controls/src/Core/CheckBox'                = @('CheckBox')
    'src/Controls/src/Core/DatePicker'              = @('DatePicker')
    'src/Controls/src/Core/DragAndDrop'             = @('DragAndDrop')
    'src/Controls/src/Core/Editor'                  = @('Editor')
    'src/Controls/src/Core/Entry'                   = @('Entry')
    'src/Controls/src/Core/FlyoutPage'              = @('FlyoutPage')
    'src/Controls/src/Core/Frame'                   = @('Frame')
    'src/Controls/src/Core/GraphicsView'            = @('GraphicsView')
    'src/Controls/src/Core/Image/'                  = @('Image')
    'src/Controls/src/Core/ImageButton'             = @('ImageButton')
    'src/Controls/src/Core/IndicatorView'           = @('IndicatorView')
    'src/Controls/src/Core/Label'                   = @('Label')
    'src/Controls/src/Core/Layout'                  = @('Layout')
    'src/Controls/src/Core/LegacyLayouts'           = @('Layout')
    'src/Controls/src/Core/ListView'                = @('ListView')
    'src/Controls/src/Core/NavigationPage'          = @('Navigation')
    'src/Controls/src/Core/Page'                    = @('Page')
    'src/Controls/src/Core/Picker'                  = @('Picker')
    'src/Controls/src/Core/ProgressBar'             = @('ProgressBar')
    'src/Controls/src/Core/RadioButton'             = @('RadioButton')
    'src/Controls/src/Core/RefreshView'             = @('RefreshView')
    'src/Controls/src/Core/ScrollView'              = @('ScrollView')
    'src/Controls/src/Core/SearchBar'               = @('SearchBar')
    'src/Controls/src/Core/Shape'                   = @('Shape')
    'src/Controls/src/Core/Shapes'                  = @('Shape')
    'src/Controls/src/Core/Shell'                   = @('Shell')
    'src/Controls/src/Core/Slider'                  = @('Slider')
    'src/Controls/src/Core/Stepper'                 = @('Stepper')
    'src/Controls/src/Core/SwipeView'               = @('SwipeView')
    'src/Controls/src/Core/Switch'                  = @('Switch')
    'src/Controls/src/Core/TabbedPage'              = @('TabbedPage')
    'src/Controls/src/Core/TableView'               = @('TableView')
    'src/Controls/src/Core/TimePicker'              = @('TimePicker')
    'src/Controls/src/Core/Toolbar'                 = @('ToolbarItem')
    'src/Controls/src/Core/TitleBar'                = @('TitleView')
    'src/Controls/src/Core/WebView'                 = @('WebView')
    'src/Controls/src/Core/HybridWebView'           = @('WebView')
    'src/Controls/src/Core/Window'                  = @('Window')
    'src/Controls/src/Core/ContentView'             = @('Page')
    'src/Controls/src/Core/ContentPage'             = @('Page')
    'src/Controls/src/Core/BindableLayout'          = @('Layout')
    'src/Controls/src/Core/CarouselView'            = @('CarouselView')
    'src/Controls/src/Core/Handlers/Items'          = @('CollectionView', 'CarouselView')
    'src/Controls/src/Core/Items'                   = @('CollectionView', 'CarouselView')
    'src/Controls/src/Core/Handlers/Shell'          = @('Shell')
    'src/Controls/src/Core/Handlers/Shapes'         = @('Shape')
    'src/Controls/src/Core/Platform/GestureManager' = @('Gestures', 'DragAndDrop')
    'src/Controls/src/Core/Platform/AlertManager'   = @('DisplayAlert', 'DisplayPrompt', 'ActionSheet')
    'src/Controls/src/Core/Platform/ModalNavigationManager' = @('Navigation', 'Page')
    'src/Controls/src/Core/Interactivity'           = @('Gestures')
    'src/Controls/src/Core/Application'             = @('Lifecycle', 'Window')
    'src/Controls/src/Core/AutomationProperties'    = @('Accessibility')
    'src/BlazorWebView'                             = @('WebView')
}

# Map source paths to unit test projects
$PathToUnitTestMap = [ordered]@{
    'src/Controls/src/'            = @('Controls.Core.UnitTests')
    'src/Controls/tests/Core.UnitTests' = @('Controls.Core.UnitTests')
    'src/Controls/tests/Xaml.UnitTests' = @('Controls.Xaml.UnitTests')
    'src/Controls/tests/SourceGen.UnitTests' = @('SourceGen.UnitTests')
    'src/Core/src/'                = @('Core.UnitTests')
    'src/Core/tests/UnitTests'     = @('Core.UnitTests')
    'src/Essentials/'              = @('Essentials.UnitTests')
    'src/Graphics/'                = @()  # No unit tests, only device tests
}

# Map source paths to device test projects
$PathToDeviceTestMap = [ordered]@{
    'src/Controls/src/'            = @('Controls.DeviceTests')
    'src/Controls/tests/DeviceTests' = @('Controls.DeviceTests')
    'src/Core/src/'                = @('Core.DeviceTests')
    'src/Core/tests/DeviceTests'   = @('Core.DeviceTests')
    'src/Essentials/'              = @('Essentials.DeviceTests')
    'src/Graphics/'                = @('Graphics.DeviceTests')
}

# Paths that trigger ALL tests
$InfrastructurePaths = @(
    'eng/'
    'Directory.Build'
    'global.json'
    'NuGet.config'
    'build.cake'
    'build.ps1'
    'build.sh'
    'build.cmd'
)

# File patterns for platform detection
$PlatformPatterns = @{
    'android'  = @('.android.cs', '/Android/', '/Platforms/Android/')
    'ios'      = @('.ios.cs', '/iOS/', '/Platforms/iOS/')
    'catalyst' = @('.maccatalyst.cs', '/MacCatalyst/', '/Platforms/MacCatalyst/')
    'windows'  = @('.windows.cs', '/Windows/', '/Platforms/Windows/')
}

# ──────────────────────────────────────────────────────
# HELPER FUNCTIONS
# ──────────────────────────────────────────────────────

function Get-DeterministicSelection {
    param([string[]]$ChangedFiles)

    $uiCategories = [System.Collections.Generic.HashSet[string]]::new()
    $unitProjects = [System.Collections.Generic.HashSet[string]]::new()
    $deviceProjects = [System.Collections.Generic.HashSet[string]]::new()
    $integrationCategories = [System.Collections.Generic.HashSet[string]]::new()
    $platforms = [System.Collections.Generic.HashSet[string]]::new()
    $isAllRequired = $false
    $unmatchedFiles = @()

    foreach ($file in $ChangedFiles) {
        $matched = $false

        # Check infrastructure paths first
        foreach ($infraPath in $InfrastructurePaths) {
            if ($file -like "$infraPath*" -or $file -like "*/$infraPath*") {
                $isAllRequired = $true
                $matched = $true
                break
            }
        }
        if ($isAllRequired) { break }

        # Check UI category mapping
        foreach ($entry in $PathToCategoryMap.GetEnumerator()) {
            if ($file -like "$($entry.Key)*") {
                foreach ($cat in $entry.Value) { [void]$uiCategories.Add($cat) }
                $matched = $true
                break
            }
        }

        # Check unit test mapping
        foreach ($entry in $PathToUnitTestMap.GetEnumerator()) {
            if ($file -like "$($entry.Key)*") {
                foreach ($proj in $entry.Value) { [void]$unitProjects.Add($proj) }
                $matched = $true
                break
            }
        }

        # Check device test mapping
        foreach ($entry in $PathToDeviceTestMap.GetEnumerator()) {
            if ($file -like "$($entry.Key)*") {
                foreach ($proj in $entry.Value) { [void]$deviceProjects.Add($proj) }
                $matched = $true
                break
            }
        }

        # Check for template changes → integration tests
        if ($file -like 'src/Templates/*') {
            [void]$integrationCategories.Add('Build')
            [void]$integrationCategories.Add('Blazor')
            $matched = $true
        }

        # Core framework changes → ALL UI tests
        if ($file -like 'src/Core/src/*') {
            $isAllRequired = $true
            break
        }

        # Detect platforms
        foreach ($entry in $PlatformPatterns.GetEnumerator()) {
            foreach ($pattern in $entry.Value) {
                if ($file -like "*$pattern*") {
                    [void]$platforms.Add($entry.Key)
                }
            }
        }

        if (-not $matched) {
            $unmatchedFiles += $file
        }
    }

    # If no platforms detected, it's shared code → all platforms
    if ($platforms.Count -eq 0 -and -not $isAllRequired) {
        [void]$platforms.Add('ALL')
    }

    # Always include safety-net categories if we have any UI categories
    if ($uiCategories.Count -gt 0) {
        [void]$uiCategories.Add('Lifecycle')
        [void]$uiCategories.Add('ViewBaseTests')
    }

    $confidence = if ($isAllRequired) { "low" }
                  elseif ($unmatchedFiles.Count -gt ($ChangedFiles.Count / 2)) { "medium" }
                  else { "high" }

    return @{
        IsAllRequired          = $isAllRequired
        UICategories           = [string[]]$uiCategories
        UnitProjects           = [string[]]$unitProjects
        DeviceProjects         = [string[]]$deviceProjects
        IntegrationCategories  = [string[]]$integrationCategories
        Platforms              = [string[]]$platforms
        UnmatchedFiles         = $unmatchedFiles
        Confidence             = $confidence
    }
}

function Invoke-CopilotSelection {
    param(
        [string[]]$ChangedFiles,
        [hashtable]$Baseline,
        [string]$PRTitle,
        [string]$PRBody,
        [string]$PRLabels
    )

    # Build prompt for Copilot CLI
    $changedFilesStr = $ChangedFiles -join "`n"
    $baselineStr = "Deterministic baseline:`n"
    $baselineStr += "  UI Categories: $($Baseline.UICategories -join ', ')`n"
    $baselineStr += "  Unit Tests: $($Baseline.UnitProjects -join ', ')`n"
    $baselineStr += "  Device Tests: $($Baseline.DeviceProjects -join ', ')`n"
    $baselineStr += "  Integration: $($Baseline.IntegrationCategories -join ', ')`n"
    $baselineStr += "  Platforms: $($Baseline.Platforms -join ', ')`n"
    $baselineStr += "  Confidence: $($Baseline.Confidence)`n"
    if ($Baseline.UnmatchedFiles.Count -gt 0) {
        $baselineStr += "  Unmatched files: $($Baseline.UnmatchedFiles -join ', ')`n"
    }

    $prompt = @"
Analyze PR #$PRNumber and select the minimum safe set of tests.

PR Title: $PRTitle
PR Labels: $PRLabels
PR Description (first 2000 chars):
$($PRBody.Substring(0, [Math]::Min(2000, $PRBody.Length)))

Changed files (one per line):
$changedFilesStr

$baselineStr

You MAY add to the baseline but SHOULD NOT remove from it.
Respond with ONLY a JSON object matching the schema in your agent definition.
"@

    Write-Host "  🤖 Invoking Copilot CLI with smart-test-selector agent..." -ForegroundColor Cyan

    $copilotArgs = @(
        "--agent", "smart-test-selector",
        "-p", $prompt
    )

    try {
        $rawOutput = & copilot @copilotArgs 2>/dev/null
        $exitCode = $LASTEXITCODE

        if ($exitCode -ne 0 -or [string]::IsNullOrWhiteSpace($rawOutput)) {
            Write-Host "  ⚠️ Copilot CLI returned exit code $exitCode" -ForegroundColor Yellow
            return $null
        }

        Write-Host "  ✅ Copilot CLI responded" -ForegroundColor Green

        # Extract JSON from response (Copilot may wrap in markdown code blocks)
        $jsonStr = Extract-JsonFromOutput -RawOutput $rawOutput
        if (-not $jsonStr) {
            Write-Host "  ⚠️ Could not extract JSON from Copilot response" -ForegroundColor Yellow
            Write-Host "  Raw output: $($rawOutput.Substring(0, [Math]::Min(500, $rawOutput.Length)))" -ForegroundColor Gray
            return $null
        }

        $parsed = $jsonStr | ConvertFrom-Json
        return $parsed
    }
    catch {
        Write-Host "  ⚠️ Copilot invocation failed: $_" -ForegroundColor Yellow
        return $null
    }
}

function Merge-Selections {
    param(
        [hashtable]$Baseline,
        $CopilotResult
    )

    # Start with baseline
    $uiCategories = [System.Collections.Generic.HashSet[string]]::new()
    $unitProjects = [System.Collections.Generic.HashSet[string]]::new()
    $deviceProjects = [System.Collections.Generic.HashSet[string]]::new()
    $integrationCategories = [System.Collections.Generic.HashSet[string]]::new()
    $platforms = [System.Collections.Generic.HashSet[string]]::new()

    foreach ($cat in $Baseline.UICategories) { [void]$uiCategories.Add($cat) }
    foreach ($proj in $Baseline.UnitProjects) { [void]$unitProjects.Add($proj) }
    foreach ($proj in $Baseline.DeviceProjects) { [void]$deviceProjects.Add($proj) }
    foreach ($cat in $Baseline.IntegrationCategories) { [void]$integrationCategories.Add($cat) }
    foreach ($p in $Baseline.Platforms) { [void]$platforms.Add($p) }

    # Merge Copilot additions (additive only)
    if ($CopilotResult) {
        if ($CopilotResult.uiTestCategories) {
            foreach ($cat in $CopilotResult.uiTestCategories) {
                if ($cat -eq 'ALL') { return @{ IsAllRequired = $true } }
                [void]$uiCategories.Add($cat)
            }
        }
        if ($CopilotResult.unitTestProjects) {
            foreach ($proj in $CopilotResult.unitTestProjects) {
                if ($proj -eq 'ALL') { $unitProjects = [System.Collections.Generic.HashSet[string]]@('ALL'); break }
                [void]$unitProjects.Add($proj)
            }
        }
        if ($CopilotResult.deviceTestProjects) {
            foreach ($proj in $CopilotResult.deviceTestProjects) {
                if ($proj -eq 'ALL') { $deviceProjects = [System.Collections.Generic.HashSet[string]]@('ALL'); break }
                [void]$deviceProjects.Add($proj)
            }
        }
        if ($CopilotResult.integrationTestCategories) {
            foreach ($cat in $CopilotResult.integrationTestCategories) {
                if ($cat -eq 'ALL') { $integrationCategories = [System.Collections.Generic.HashSet[string]]@('ALL'); break }
                [void]$integrationCategories.Add($cat)
            }
        }
        if ($CopilotResult.platforms) {
            foreach ($p in $CopilotResult.platforms) {
                [void]$platforms.Add($p)
            }
        }
    }

    return @{
        IsAllRequired          = $false
        UICategories           = [string[]]$uiCategories
        UnitProjects           = [string[]]$unitProjects
        DeviceProjects         = [string[]]$deviceProjects
        IntegrationCategories  = [string[]]$integrationCategories
        Platforms              = [string[]]$platforms
        Confidence             = if ($CopilotResult.confidence) { $CopilotResult.confidence } else { $Baseline.Confidence }
        Reasoning              = if ($CopilotResult.reasoning) { $CopilotResult.reasoning } else { "Deterministic mapping only" }
    }
}

# ──────────────────────────────────────────────────────
# MAIN EXECUTION
# ──────────────────────────────────────────────────────

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Smart Test Selector                             ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  PR:        #$PRNumber                                          ║" -ForegroundColor Cyan
Write-Host "║  Platform:  $Platform                                        ║" -ForegroundColor Cyan
Write-Host "║  RunTests:  $RunTests                                      ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Step 1: Verify prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$ghVersion = gh --version 2>$null | Select-Object -First 1
if (-not $ghVersion) {
    Write-Error "GitHub CLI (gh) is not installed."
    exit 1
}
Write-Host "  ✅ GitHub CLI: $ghVersion" -ForegroundColor Green

$copilotVersion = copilot --version 2>$null
if (-not $copilotVersion) {
    Write-Host "  ⚠️ Copilot CLI not installed — will use deterministic mapping only" -ForegroundColor Yellow
    $hasCopilot = $false
} else {
    Write-Host "  ✅ Copilot CLI: $copilotVersion" -ForegroundColor Green
    $hasCopilot = $true
}

# Step 2: Fetch PR metadata
Write-Host ""
Write-Host "📥 Fetching PR #$PRNumber metadata..." -ForegroundColor Yellow

$prJson = gh pr view $PRNumber --json title,body,labels,files,headRefName,baseRefName 2>$null
if (-not $prJson) {
    Write-Error "PR #$PRNumber not found or not accessible."
    exit 1
}
try {
    $prData = $prJson | ConvertFrom-Json
} catch {
    Write-Error "Failed to parse PR data: $_"
    exit 1
}
Write-Host "  ✅ PR: $($prData.title)" -ForegroundColor Green
Write-Host "  ✅ Base: $($prData.baseRefName) ← $($prData.headRefName)" -ForegroundColor Green

$changedFiles = @($prData.files | ForEach-Object { $_.path })
$prLabels = ($prData.labels | ForEach-Object { $_.name }) -join ', '

Write-Host "  ✅ Changed files: $($changedFiles.Count)" -ForegroundColor Green
if ($prLabels) {
    Write-Host "  ✅ Labels: $prLabels" -ForegroundColor Green
}

# Step 3: ForceAll shortcut
if ($ForceAll) {
    Write-Host ""
    Write-Host "⚡ ForceAll specified — selecting ALL tests" -ForegroundColor Yellow
    $finalSelection = @{
        uiTestCategories         = @('ALL')
        unitTestProjects         = @('ALL')
        deviceTestProjects       = @('ALL')
        integrationTestCategories = @('ALL')
        platforms                = @('ALL')
        confidence               = 'high'
        reasoning                = 'ForceAll parameter specified'
        fallback                 = $true
    }
} else {
    # Step 4: Deterministic mapping
    Write-Host ""
    Write-Host "🔍 Running deterministic file-path mapping..." -ForegroundColor Yellow

    $baseline = Get-DeterministicSelection -ChangedFiles $changedFiles

    if ($baseline.IsAllRequired) {
        Write-Host "  ⚠️ Infrastructure/core changes detected — selecting ALL" -ForegroundColor Yellow
        $finalSelection = @{
            uiTestCategories         = @('ALL')
            unitTestProjects         = @('ALL')
            deviceTestProjects       = @('ALL')
            integrationTestCategories = @('ALL')
            platforms                = @('ALL')
            confidence               = 'low'
            reasoning                = 'Infrastructure or core framework changes require full test suite'
            fallback                 = $true
        }
    } else {
        Write-Host "  📊 Baseline:" -ForegroundColor Gray
        Write-Host "     UI Categories:  $($baseline.UICategories -join ', ')" -ForegroundColor Gray
        Write-Host "     Unit Tests:     $($baseline.UnitProjects -join ', ')" -ForegroundColor Gray
        Write-Host "     Device Tests:   $($baseline.DeviceProjects -join ', ')" -ForegroundColor Gray
        Write-Host "     Integration:    $($baseline.IntegrationCategories -join ', ')" -ForegroundColor Gray
        Write-Host "     Platforms:      $($baseline.Platforms -join ', ')" -ForegroundColor Gray
        Write-Host "     Confidence:     $($baseline.Confidence)" -ForegroundColor Gray
        if ($baseline.UnmatchedFiles.Count -gt 0) {
            Write-Host "     Unmatched:      $($baseline.UnmatchedFiles.Count) files" -ForegroundColor Gray
        }

        # Step 5: AI refinement via Copilot CLI
        $copilotResult = $null
        if ($hasCopilot -and ($baseline.UnmatchedFiles.Count -gt 0 -or $baseline.Confidence -ne 'high')) {
            Write-Host ""
            Write-Host "🤖 Requesting AI refinement..." -ForegroundColor Yellow
            $copilotResult = Invoke-CopilotSelection `
                -ChangedFiles $changedFiles `
                -Baseline $baseline `
                -PRTitle $prData.title `
                -PRBody ($prData.body ?? '') `
                -PRLabels $prLabels
        } elseif ($hasCopilot) {
            Write-Host ""
            Write-Host "  ✅ High confidence — skipping AI refinement" -ForegroundColor Green
        }

        # Step 6: Merge selections (additive)
        $merged = Merge-Selections -Baseline $baseline -CopilotResult $copilotResult

        if ($merged.IsAllRequired) {
            $finalSelection = @{
                uiTestCategories         = @('ALL')
                unitTestProjects         = @('ALL')
                deviceTestProjects       = @('ALL')
                integrationTestCategories = @('ALL')
                platforms                = @('ALL')
                confidence               = 'low'
                reasoning                = 'AI analysis expanded to ALL'
                fallback                 = $true
            }
        } else {
            $finalSelection = @{
                uiTestCategories         = if ($merged.UICategories.Count -gt 0) { $merged.UICategories } else { @() }
                unitTestProjects         = if ($merged.UnitProjects.Count -gt 0) { $merged.UnitProjects } else { @() }
                deviceTestProjects       = if ($merged.DeviceProjects.Count -gt 0) { $merged.DeviceProjects } else { @() }
                integrationTestCategories = if ($merged.IntegrationCategories.Count -gt 0) { $merged.IntegrationCategories } else { @() }
                platforms                = if ($merged.Platforms.Count -gt 0) { $merged.Platforms } else { @('ALL') }
                confidence               = $merged.Confidence
                reasoning                = $merged.Reasoning
                fallback                 = $false
            }
        }
    }
}

# Step 7: Output results
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║  TEST SELECTION RESULTS                                   ║" -ForegroundColor Green
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green

$uiDisplay = if ($finalSelection.uiTestCategories -contains 'ALL') { 'ALL' } else { $finalSelection.uiTestCategories -join ', ' }
$unitDisplay = if ($finalSelection.unitTestProjects -contains 'ALL') { 'ALL' } else { $finalSelection.unitTestProjects -join ', ' }
$deviceDisplay = if ($finalSelection.deviceTestProjects -contains 'ALL') { 'ALL' } else { $finalSelection.deviceTestProjects -join ', ' }
$integDisplay = if ($finalSelection.integrationTestCategories -contains 'ALL') { 'ALL' }
                elseif ($finalSelection.integrationTestCategories.Count -eq 0) { '(none)' }
                else { $finalSelection.integrationTestCategories -join ', ' }
$platDisplay = if ($finalSelection.platforms -contains 'ALL') { 'ALL' } else { $finalSelection.platforms -join ', ' }

Write-Host "║  UI Tests:     $uiDisplay" -ForegroundColor White
Write-Host "║  Unit Tests:   $unitDisplay" -ForegroundColor White
Write-Host "║  Device Tests: $deviceDisplay" -ForegroundColor White
Write-Host "║  Integration:  $integDisplay" -ForegroundColor White
Write-Host "║  Platforms:    $platDisplay" -ForegroundColor White
Write-Host "║  Confidence:   $($finalSelection.confidence)" -ForegroundColor White
Write-Host "║  Reasoning:    $($finalSelection.reasoning)" -ForegroundColor White
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# Write selection JSON
$selectionJson = @{
    prNumber                 = $PRNumber
    prTitle                  = $prData.title
    changedFileCount         = $changedFiles.Count
    uiTestCategories         = $finalSelection.uiTestCategories
    unitTestProjects         = $finalSelection.unitTestProjects
    deviceTestProjects       = $finalSelection.deviceTestProjects
    integrationTestCategories = $finalSelection.integrationTestCategories
    platforms                = $finalSelection.platforms
    confidence               = $finalSelection.confidence
    reasoning                = $finalSelection.reasoning
    fallback                 = $finalSelection.fallback
    timestamp                = (Get-Date -Format 'o')
} | ConvertTo-Json -Depth 5

$selectionJson | Out-File -FilePath $OutputFile -Encoding utf8
Write-Host "📄 Selection written to: $OutputFile" -ForegroundColor Gray

# Set Azure DevOps output variables if running in pipeline
if ($env:BUILD_BUILDID) {
    $uiCsv = $finalSelection.uiTestCategories -join ','
    $unitCsv = $finalSelection.unitTestProjects -join ','
    $deviceCsv = $finalSelection.deviceTestProjects -join ','
    $integCsv = $finalSelection.integrationTestCategories -join ','
    $platCsv = $finalSelection.platforms -join ','

    Write-Host "##vso[task.setvariable variable=SmartTests_UICategories;isOutput=true]$uiCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_UnitProjects;isOutput=true]$unitCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_DeviceProjects;isOutput=true]$deviceCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_IntegrationCategories;isOutput=true]$integCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_Platforms;isOutput=true]$platCsv"
    Write-Host "##vso[task.setvariable variable=SmartTests_Confidence;isOutput=true]$($finalSelection.confidence)"
    Write-Host "##vso[task.setvariable variable=SmartTests_Fallback;isOutput=true]$($finalSelection.fallback)"
}

# Step 8: Run tests if requested
if ($RunTests) {
    Write-Host ""
    Write-Host "🚀 Running selected tests with resilience..." -ForegroundColor Yellow
    Write-Host ""

    # Load known flaky tests
    $platformForFlaky = switch ($Platform) {
        'android' { 'Android' }
        'ios'     { 'iOS' }
        default   { $null }
    }
    Write-Host "📋 Loading known flaky/failing test annotations..." -ForegroundColor Yellow
    $knownFlaky = Get-KnownFlakyTests -Platform $platformForFlaky
    Write-Host "  ✅ Found $($knownFlaky.Count) test class(es) with annotations" -ForegroundColor Green

    # Pre-test device health check (for UI tests)
    $deviceHealth = @{ Healthy = $true; Platform = $Platform; DeviceId = ""; Issues = @(); Actions = @() }
    if ($finalSelection.uiTestCategories.Count -gt 0) {
        Write-Host ""
        Write-Host "🏥 Running pre-test device health check..." -ForegroundColor Yellow
        $healthPlatform = if ($Platform -eq 'all') { 'android' } else { $Platform }
        $deviceHealth = Test-DeviceHealth -Platform $healthPlatform

        if (-not $deviceHealth.Healthy) {
            Write-Host "  ⚠️ Device health issues detected — tests may be unreliable" -ForegroundColor Yellow
            Write-Host "  ⚠️ Issues: $($deviceHealth.Issues -join '; ')" -ForegroundColor Yellow
        }
    }

    $allTestResults = @()
    $testsFailed = $false

    # Run unit tests with structured retry
    if ($finalSelection.unitTestProjects.Count -gt 0) {
        Write-Host ""
        Write-Host "📦 Running unit tests..." -ForegroundColor Yellow

        $unitProjectPaths = @{
            'Core.UnitTests'            = 'src/Core/tests/UnitTests/Core.UnitTests.csproj'
            'Controls.Core.UnitTests'   = 'src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj'
            'Controls.Xaml.UnitTests'   = 'src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj'
            'SourceGen.UnitTests'       = 'src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj'
            'Essentials.UnitTests'      = 'src/Essentials/test/UnitTests/Essentials.UnitTests.csproj'
        }

        $projectsToRun = if ($finalSelection.unitTestProjects -contains 'ALL') {
            $unitProjectPaths.Keys
        } else {
            $finalSelection.unitTestProjects
        }

        foreach ($projName in $projectsToRun) {
            if ($unitProjectPaths.ContainsKey($projName)) {
                $projPath = Join-Path $RepoRoot $unitProjectPaths[$projName]
                $result = Invoke-TestWithRetry `
                    -TestCommand 'dotnet' `
                    -TestArgs @('test', $projPath, '--no-restore') `
                    -TrxOutputDir $ArtifactsDir `
                    -TestLabel $projName `
                    -MaxRetries 2 `
                    -KnownFlaky $knownFlaky
                $allTestResults += $result
                if ($result.FinalExitCode -ne 0) { $testsFailed = $true }
            }
        }
    }

    # Run UI tests (using BuildAndRunHostApp.ps1 pattern)
    if ($finalSelection.uiTestCategories.Count -gt 0) {
        Write-Host ""
        Write-Host "🖥️ Running UI tests..." -ForegroundColor Yellow

        $categoryCsv = if ($finalSelection.uiTestCategories -contains 'ALL') {
            ''
        } else {
            $finalSelection.uiTestCategories -join ','
        }

        $platformsToTest = if ($finalSelection.platforms -contains 'ALL') {
            @($Platform)
        } elseif ($Platform -eq 'all') {
            $finalSelection.platforms
        } else {
            @($Platform)
        }

        foreach ($plat in $platformsToTest) {
            $uiArgs = @('-Platform', $plat)
            if ($categoryCsv) { $uiArgs += @('-Category', $categoryCsv) }
            $scriptPath = Join-Path $RepoRoot '.github/scripts/BuildAndRunHostApp.ps1'

            if (Test-Path $scriptPath) {
                Write-Host "  🧪 Running UI tests on $plat..." -ForegroundColor Cyan
                & pwsh $scriptPath @uiArgs 2>&1 | ForEach-Object { Write-Host "     $_" }
                $uiExitCode = $LASTEXITCODE

                $uiResult = @{
                    Label = "UITests-$plat"
                    Attempts = @(@{ Number = 1; ExitCode = $uiExitCode; TrxPath = ''; Output = '' })
                    FinalExitCode = $uiExitCode
                    FailedTests = @()
                    PassedOnRetry = @()
                    ConsistentFailures = @()
                }

                if ($uiExitCode -ne 0) {
                    Write-Host "  ❌ UI tests on $plat FAILED" -ForegroundColor Red
                    $uiResult.ConsistentFailures += @{
                        TestName = "UITests-$plat (aggregate)"
                        Error = "UI test suite exited with code $uiExitCode"
                        IsKnownFlaky = $false
                    }
                    $testsFailed = $true
                } else {
                    Write-Host "  ✅ UI tests on $plat PASSED" -ForegroundColor Green
                }
                $allTestResults += $uiResult
            } else {
                Write-Host "  ⚠️ BuildAndRunHostApp.ps1 not found — skipping UI tests" -ForegroundColor Yellow
            }
        }
    }

    # Step 9: Failure analysis (only if tests failed)
    $failureAnalysis = $null
    if ($testsFailed) {
        Write-Host ""
        Write-Host "🔬 Analyzing failures..." -ForegroundColor Yellow

        if ($hasCopilot) {
            $failureAnalysis = Invoke-FailureAnalysis `
                -TestResults $allTestResults `
                -ChangedFiles $changedFiles `
                -PRTitle $prData.title `
                -KnownFlaky $knownFlaky `
                -DeviceHealth $deviceHealth `
                -Platform $Platform
        }

        # Fall back to heuristic if AI unavailable
        if (-not $failureAnalysis) {
            $failureAnalysis = Get-HeuristicClassification `
                -TestResults $allTestResults `
                -ChangedFiles $changedFiles `
                -KnownFlaky $knownFlaky `
                -DeviceHealth $deviceHealth
        }

        # Display analysis
        Write-Host ""
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
        Write-Host "║  FAILURE ANALYSIS                                         ║" -ForegroundColor Yellow
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Yellow
        Write-Host "║  Verdict:    $($failureAnalysis.verdict)" -ForegroundColor White
        Write-Host "║  Regressions: $($failureAnalysis.blockerCount)  Flaky: $($failureAnalysis.flakyCount)  Infra: $($failureAnalysis.infraCount)  Unrelated: $($failureAnalysis.unrelatedCount)" -ForegroundColor White
        Write-Host "║  Summary:   $($failureAnalysis.summary)" -ForegroundColor White
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

        # Save analysis to artifacts
        $analysisJson = $failureAnalysis | ConvertTo-Json -Depth 10
        $analysisJson | Out-File -FilePath (Join-Path $ArtifactsDir "failure-analysis.json") -Encoding utf8
    }

    # Step 10: Generate and post PR comment
    Write-Host ""
    Write-Host "📝 Generating test report..." -ForegroundColor Yellow

    $report = Format-TestReport `
        -PRNumber $PRNumber `
        -PRTitle $prData.title `
        -Selection $finalSelection `
        -TestResults $allTestResults `
        -FailureAnalysis $failureAnalysis `
        -DeviceHealth $deviceHealth `
        -Platform $Platform

    $report | Out-File -FilePath (Join-Path $ArtifactsDir "test-report.md") -Encoding utf8
    Write-Host "  ✅ Report saved to test-report.md" -ForegroundColor Green

    # Post PR comment if GH_TOKEN is available
    if ($env:GH_TOKEN -or $env:GH_COMMENT_TOKEN) {
        Write-Host "  📤 Posting report to PR #$PRNumber..." -ForegroundColor Cyan
        $reportPath = Join-Path $ArtifactsDir "test-report.md"
        try {
            gh pr comment $PRNumber --body-file $reportPath --edit-last 2>$null
            if ($LASTEXITCODE -ne 0) {
                gh pr comment $PRNumber --body-file $reportPath 2>$null
            }
            Write-Host "  ✅ PR comment posted" -ForegroundColor Green
        } catch {
            Write-Host "  ⚠️ Could not post PR comment: $_" -ForegroundColor Yellow
        }
    }

    # Final exit code based on analysis verdict
    if ($testsFailed) {
        if ($failureAnalysis -and $failureAnalysis.verdict -ne 'FAIL') {
            Write-Host ""
            Write-Host "⚠️ Tests had failures but analysis says: $($failureAnalysis.verdict)" -ForegroundColor Yellow
            Write-Host "  $($failureAnalysis.summary)" -ForegroundColor Yellow
            # Exit 0 if all failures are flaky/infra/unrelated (no real regressions)
            if ($failureAnalysis.blockerCount -eq 0) {
                Write-Host "  ✅ No regressions detected — treating as PASS" -ForegroundColor Green
                # Still exit non-zero but with warning — let the pipeline decide
            }
        }
        Write-Host ""
        Write-Host "❌ Test run completed with failures. See analysis above." -ForegroundColor Red
        exit 1
    } else {
        Write-Host ""
        Write-Host "✅ All selected tests passed!" -ForegroundColor Green
    }
}

if ($LogFile) {
    Stop-Transcript | Out-Null
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
exit 0
