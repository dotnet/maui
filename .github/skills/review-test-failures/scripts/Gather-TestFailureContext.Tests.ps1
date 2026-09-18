#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for the PURE functions in Gather-TestFailureContext.ps1 — the deterministic
    /review tests gather script that gates merge verdicts.

.DESCRIPTION
    Gather-TestFailureContext.ps1 is a param'd, self-executing script (it does real GitHub/AzDO/
    Helix I/O on load), so these tests AST-extract only the pure, side-effect-free functions and
    dot-source them — the same pattern Run-DeviceTests.Tests.ps1 uses. No network, no auth.

    Functions covered:
      - Get-HelixWorkItemCounts      (Phase 1: anonymous /workitems completeness + fail counting)
      - Get-XUnitFailures            (Phase 2: parse xUnit v2 TestResults XML)
      - Get-ConsoleFailureReason     (Phase 2: extract crash/timeout reason from console log)
      - New-DeviceWorkItemFailureRecords (Phase 2: classify ONE failed work item into records)

    CORE INVARIANT under test: NEVER A FALSE GREEN. A non-zero-ExitCode device-test work item must
    ALWAYS yield at least one capping record, and any incompleteness (running items, short counts,
    unreadable result files, a killed/hung/crashed run, or a non-zero exit with zero named failures)
    must keep the verdict at "needs human investigation".

.EXAMPLE
    Invoke-Pester ./Gather-TestFailureContext.Tests.ps1
    Invoke-Pester ./Gather-TestFailureContext.Tests.ps1 -Output Detailed
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Gather-TestFailureContext.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }
    $script:gatherAst = $ast

    foreach ($functionName in @(
            'ConvertTo-Array',
            'Get-ObjectValue',
            'Get-GatherRequestTimeoutSeconds',
            'Invoke-ProcessWithGatherDeadline',
            'Invoke-GhJson',
            'Invoke-JsonUrl',
            'Invoke-TextUrl',
            'Get-PinnedPrDiff',
            'Get-AzDoApiBase',
            'Get-HttpStatusCode',
            'Invoke-AzDoJsonWithProjectFallback',
            'Get-RequiredReviewPipelines',
            'Get-ChecksForBuildDiscovery',
            'Get-BuildHeadEvidence',
            'Get-RecentSourceBranchBuilds',
            'Get-PipelineHistory',
            'Get-BuildLogTestFailures',
            'Get-TestFailuresFromLog',
            'Get-PublicBuildFailureEvidence',
            'Get-BoundedFailureText',
            'Get-HeaderValue',
            'Get-AzDoTestRuns',
            'Get-AzDoFailedTestResultsByBuild',
            'Get-VisualSnapshotInfo',
            'Select-VisualAttachments',
            'Get-VisualEnvironmentHintFromLog',
            'Resolve-VisualEnvironmentName',
            'Get-HelixWorkItemCounts',
            'Get-XUnitFailures',
            'Get-ConsoleFailureReason',
            'New-DeviceWorkItemFailureRecords',
            'Get-AggregatedBaseLegMap',
            'Get-PlatformFromText',
            'Get-ErrorFingerprint',
            'Get-BuildErrorSignature',
            'Test-IsTransientBuildErrorCode',
            'Get-BuildErrorsFromLog',
            'Get-VisualEvidenceBudgetDecision',
            'Get-BoundedVisualDeadline',
            'Get-VisualRequestTimeoutSeconds'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }

    function New-HistoryBuildFixture {
        param(
            [int]$Id = 99,
            [int]$DefinitionId = 302,
            [string]$Branch = 'refs/pull/123/merge',
            [string]$SourceVersion = ('b' * 40),
            [string]$Status = 'completed',
            [string]$Result = 'failed',
            [string]$FinishTime = '2026-09-17T09:59:00Z'
        )
        return [pscustomobject]@{
            id = $Id
            definition = [pscustomobject]@{ id = $DefinitionId; name = 'maui-pr' }
            sourceBranch = $Branch
            sourceVersion = $SourceVersion
            status = $Status
            result = $Result
            queueTime = '2026-09-17T08:00:00Z'
            finishTime = $FinishTime
            _links = [pscustomobject]@{ web = [pscustomobject]@{ href = "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$Id" } }
        }
    }

    function New-CurrentBuildFixture {
        param([int]$DefinitionId = 302, [string]$Name = 'maui-pr', [bool]$Verified = $true)
        return [ordered]@{
            id = $DefinitionId
            org = 'dnceng-public'
            project = 'public'
            checkNames = @($Name)
            accessible = $true
            timelineReadable = $true
            headEvidence = [ordered]@{ verified = $Verified; error = if (-not $Verified) { 'Earlier PR revision.' } else { $null } }
            metadata = [ordered]@{
                definitionId = $DefinitionId
                definitionName = $Name
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = ('a' * 40)
                queueTime = '2026-09-17T10:00:00Z'
                status = 'completed'
                result = 'failed'
                webUrl = "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$DefinitionId"
            }
        }
    }
}

Describe 'Visual snapshot evidence helpers' {
    It 'parses a snapshot difference and its percentage' {
        $info = Get-VisualSnapshotInfo -Message @'
VisualTestUtils.VisualTestFailedException :
Snapshot different than baseline: EntryClearButtonColorShouldUpdateOnThemeChange.png (2.08% difference)
If the correct baseline has changed, update it.
'@
        $info.kind | Should -Be 'different'
        $info.snapshotFileName | Should -Be 'EntryClearButtonColorShouldUpdateOnThemeChange.png'
        $info.description | Should -Be '2.08% difference'
        $info.differencePercent | Should -Be 2.08
    }

    It 'parses a missing baseline and preserves a repository path hint' {
        $info = Get-VisualSnapshotInfo -Message @'
Baseline snapshot not yet created: /agent/_work/1/s/src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/NewSnapshot.png
Ensure new snapshot is correct.
'@
        $info.kind | Should -Be 'missing-baseline'
        $info.snapshotFileName | Should -Be 'NewSnapshot.png'
        $info.baselinePathHint | Should -Be 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/NewSnapshot.png'
    }

    It 'rejects an unsafe snapshot filename from untrusted test output' {
        Get-VisualSnapshotInfo -Message 'Snapshot different than baseline: ../../payload.png (1.00% difference)' | Should -BeNullOrEmpty
    }

    It 'selects the highest complete visual retry and ignores teardown screenshots' {
        $attachments = @(
            [pscustomobject]@{ id = 7; fileName = 'Sample-diff.png'; size = 10; url = 'https://dev.azure.com/o/p/_apis/test/Runs/1/Results/2/Attachments/7' },
            [pscustomobject]@{ id = 9; fileName = 'Sample-diff[1].png'; size = 10; url = 'https://dev.azure.com/o/p/_apis/test/Runs/1/Results/2/Attachments/9' },
            [pscustomobject]@{ id = 13; fileName = 'Sample-iOS-UITestBaseTearDown-ScreenShot-guid.png'; size = 10; url = 'https://dev.azure.com/o/p/_apis/test/Runs/1/Results/2/Attachments/13' },
            [pscustomobject]@{ id = 15; fileName = 'Sample.png'; size = 10; url = 'https://dev.azure.com/o/p/_apis/test/Runs/1/Results/2/Attachments/15' },
            [pscustomobject]@{ id = 17; fileName = 'Sample[1].png'; size = 10; url = 'https://dev.azure.com/o/p/_apis/test/Runs/1/Results/2/Attachments/17' }
        )

        $selected = Select-VisualAttachments -Attachments $attachments -SnapshotFileName 'Sample.png'
        $selected.selectedRetry | Should -Be 1
        $selected.actual.id | Should -Be 17
        $selected.diff.id | Should -Be 9
        $selected.candidateCount | Should -Be 2
    }

    It 'keeps an actual-only attachment for a missing baseline' {
        $selected = Select-VisualAttachments -Attachments @(
            [pscustomobject]@{ id = 4; fileName = 'NewSnapshot.png'; size = 10; url = 'https://dev.azure.com/o/p/_apis/test/Runs/1/Results/2/Attachments/4' }
        ) -SnapshotFileName 'NewSnapshot.png'

        $selected.actual.id | Should -Be 4
        $selected.diff | Should -BeNullOrEmpty
    }

    It 'maps current UI runtime logs to snapshot environment directories' {
        $ios = Get-VisualEnvironmentHintFromLog -Text 'Running TestCases.iOS.Tests --device="ios-simulator-64" --apiversion="26.0"'
        $ios.platform | Should -Be 'ios'
        $ios.environmentName | Should -Be 'ios-26'

        $android = Get-VisualEnvironmentHintFromLog -Text 'Running TestCases.Android.Tests --device="android-emulator-64" --apiversion="36"'
        $android.platform | Should -Be 'android'
        $android.environmentName | Should -Be 'android-notch-36'

        $mac = Get-VisualEnvironmentHintFromLog -Text 'Running TestCases.Mac.Tests for maccatalyst'
        $mac.environmentName | Should -Be 'mac'
    }

    It 'does not reuse a sampled environment when platform hint coverage was incomplete' {
        Resolve-VisualEnvironmentName `
            -Hints @([pscustomobject]@{ platform = 'ios'; environmentName = 'ios-26' }) `
            -Platform 'ios' `
            -ResultText 'TestCases.iOS.Tests' `
            -IncompletePlatforms @('ios') |
            Should -BeNullOrEmpty
    }

    It 'does not reuse a sampled environment when an unsampled leg has unknown platform' {
        Resolve-VisualEnvironmentName `
            -Hints @([pscustomobject]@{ platform = 'ios'; environmentName = 'ios-26' }) `
            -Platform 'ios' `
            -ResultText 'TestCases.iOS.Tests' `
            -IncompletePlatforms @('unknown') |
            Should -BeNullOrEmpty
    }

    It 'prefers a result-level environment hint even when build-log sampling was incomplete' {
        Resolve-VisualEnvironmentName `
            -Hints @([pscustomobject]@{ platform = 'ios'; environmentName = 'ios-26' }) `
            -Platform 'ios' `
            -ResultText 'Running TestCases.iOS.Tests --apiversion="26.0"' `
            -IncompletePlatforms @('ios') |
            Should -Be 'ios-26'
    }
}

Describe 'Shared gather request deadline' {
    It 'caps ordinary JSON requests by the remaining overall gather budget' {
        $priorDeadline = Get-Variable -Name GatherHardDeadline -Scope Script -ErrorAction SilentlyContinue
        $script:GatherHardDeadline = (Get-Date).AddSeconds(3)
        Mock Invoke-WebRequest {
            return [pscustomobject]@{
                Content = '{}'
                StatusCode = 200
            }
        }
        try {
            Invoke-JsonUrl -Url 'https://dev.azure.com/dnceng-public/public/_apis/example' | Out-Null
            Should -Invoke Invoke-WebRequest -Times 1 -Exactly -ParameterFilter {
                $TimeoutSec -gt 0 -and $TimeoutSec -le 3
            }
        }
        finally {
            if ($null -eq $priorDeadline) {
                Remove-Variable -Name GatherHardDeadline -Scope Script -ErrorAction SilentlyContinue
            }
            else {
                $script:GatherHardDeadline = $priorDeadline.Value
            }
        }
    }

    It 'terminates a child process that exceeds the remaining gather timeout' {
        $priorDeadline = Get-Variable -Name GatherHardDeadline -Scope Script -ErrorAction SilentlyContinue
        $script:GatherHardDeadline = (Get-Date).AddSeconds(2)
        try {
            {
                Invoke-ProcessWithGatherDeadline `
                    -FileName 'pwsh' `
                    -Arguments @('-NoLogo', '-NoProfile', '-Command', 'Start-Sleep -Seconds 5') `
                    -RequestedTimeoutSec 1
            } | Should -Throw '*exceeded*timeout*'
        }
        finally {
            if ($null -eq $priorDeadline) {
                Remove-Variable -Name GatherHardDeadline -Scope Script -ErrorAction SilentlyContinue
            }
            else {
                $script:GatherHardDeadline = $priorDeadline.Value
            }
        }
    }
}

Describe 'Untrusted failure text bounds' {
    It 'caps long messages while preserving short text' {
        Get-BoundedFailureText -Text 'short' -MaxChars 20 | Should -Be 'short'
        $bounded = Get-BoundedFailureText -Text ('x' * 10000) -MaxChars 100
        $bounded.Length | Should -Be 100
        $bounded | Should -Match '\[truncated\]$'
    }
}

Describe 'Get-VisualEvidenceBudgetDecision (elapsed-only visual budget accounting)' {
    It 'reports remaining budget and does not trip while visual time is under budget' {
        $d = Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds 250
        $d.remainingSeconds | Should -Be 350
        $d.exhausted | Should -BeFalse
    }

    It 'trips exactly at the budget boundary' {
        (Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds 600).exhausted | Should -BeTrue
        (Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds 601).exhausted | Should -BeTrue
        (Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds 599).exhausted | Should -BeFalse
    }

    It 'does not let interleaved nonvisual work between uitests builds consume the budget' {
        # Two maui-pr-uitests builds each spend 250s in visual discovery, with a NON-uitests build
        # doing a very long (5000s) timeline/log/Helix read BETWEEN them. Because only visual-scan
        # time is accumulated into $elapsed (the loop adds to it solely in the discovery finally),
        # the nonvisual build must not advance the budget, so the SECOND uitests build still scans.
        # This is the exact regression the absolute wall-clock deadline caused.
        $elapsed = 0.0

        $build1 = Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds $elapsed
        $build1.exhausted | Should -BeFalse   # first uitests build scans
        $elapsed += 250                        # charge only its visual-discovery time

        # Non-uitests build: 5000s of nonvisual processing. The loop NEVER adds this to $elapsed.
        # (Modeled by leaving $elapsed unchanged.)

        $build2 = Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds $elapsed
        $build2.exhausted | Should -BeFalse   # second uitests build STILL scans (250 < 600)
        $build2.remainingSeconds | Should -Be 350
    }

    It 'trips a later uitests build once accumulated visual time exceeds the budget' {
        $elapsed = 0.0
        (Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds $elapsed).exhausted | Should -BeFalse
        $elapsed += 400
        (Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds $elapsed).exhausted | Should -BeFalse
        $elapsed += 400   # 800s of accumulated visual time now exceeds the 600s budget
        (Get-VisualEvidenceBudgetDecision -BudgetSeconds 600 -ElapsedSeconds $elapsed).exhausted | Should -BeTrue
    }
}

Describe 'Get-BoundedVisualDeadline (overall gather finalization reserve)' {
    It 'preserves the visual-only quota when the gather deadline is farther away' {
        $start = [datetime]'2026-07-23T00:00:00Z'
        Get-BoundedVisualDeadline `
            -VisualStart $start `
            -RemainingVisualSeconds 600 `
            -GatherHardDeadline $start.AddSeconds(900) |
            Should -Be $start.AddSeconds(600)
    }

    It 'caps a late visual scan at the overall gather deadline' {
        $start = [datetime]'2026-07-23T00:17:00Z'
        $gatherDeadline = [datetime]'2026-07-23T00:18:00Z'
        Get-BoundedVisualDeadline `
            -VisualStart $start `
            -RemainingVisualSeconds 600 `
            -GatherHardDeadline $gatherDeadline |
            Should -Be $gatherDeadline
    }
}

Describe 'Get-VisualRequestTimeoutSeconds (per-request timeout capped by remaining visual budget)' {
    It 'returns the default for an unbudgeted deadline sentinel' {
        Get-VisualRequestTimeoutSeconds -Deadline ([datetime]::MaxValue) | Should -Be 100
    }

    It 'returns the full default when the deadline is far away' {
        $t = Get-VisualRequestTimeoutSeconds -Deadline (Get-Date).AddSeconds(500)
        $t | Should -Be 100
    }

    It 'caps the timeout to the remaining budget when less than the default' {
        # ~30s left: the request must not be allowed its full 100s default, which would overrun the
        # shared deadline by ~70s (and the following attachments request could add another ~100s).
        $t = Get-VisualRequestTimeoutSeconds -Deadline (Get-Date).AddSeconds(30)
        $t | Should -BeLessOrEqual 30
        $t | Should -BeGreaterThan 0
    }

    It 'never returns below the minimum for a tiny-but-positive remainder' {
        # A sub-second remainder still issues ONE bounded request (>=1s); the caller's own deadline
        # recheck is what stops the loop, not a zero/negative timeout that would throw.
        $t = Get-VisualRequestTimeoutSeconds -Deadline (Get-Date).AddMilliseconds(200)
        $t | Should -Be 1
    }

    It 'never returns below the minimum once the deadline has already passed' {
        $t = Get-VisualRequestTimeoutSeconds -Deadline (Get-Date).AddSeconds(-50)
        $t | Should -Be 1
    }

    It 'honors custom default and minimum bounds' {
        (Get-VisualRequestTimeoutSeconds -Deadline (Get-Date).AddSeconds(999) -DefaultTimeoutSec 60) | Should -Be 60
        (Get-VisualRequestTimeoutSeconds -Deadline (Get-Date).AddSeconds(-1) -MinimumTimeoutSec 5) | Should -Be 5
    }
}

Describe 'Get-AzDoFailedTestResultsByBuild request budgeting' {
    BeforeEach {
        Mock Invoke-WebRequest {
            return [pscustomobject]@{
                Content = '{"value":[]}'
                Headers = @{}
            }
        }
    }

    It 'caps the first page request by the remaining visual budget' {
        Get-AzDoFailedTestResultsByBuild `
            -Org 'dnceng-public' `
            -Project 'public' `
            -BuildId 123 `
            -Deadline ((Get-Date).AddSeconds(3)) | Out-Null

        Should -Invoke -CommandName Invoke-WebRequest -Times 1 -Exactly -ParameterFilter {
            $TimeoutSec -gt 0 -and $TimeoutSec -le 3
        }
    }

    It 'still issues a bounded first page when the deadline is near-expiry (sub-second remaining)' {
        # Near-expiry regression: the deadline has NOT yet passed when the first page's top-of-loop
        # guard runs, so exactly one request must fire -- but its timeout has to be clamped to the
        # minimum (1s) rather than the 100s default, otherwise a first page issued with a few hundred
        # milliseconds of budget left could overrun the shared visual deadline by ~100s.
        Get-AzDoFailedTestResultsByBuild `
            -Org 'dnceng-public' `
            -Project 'public' `
            -BuildId 123 `
            -Deadline ((Get-Date).AddMilliseconds(300)) | Out-Null

        Should -Invoke -CommandName Invoke-WebRequest -Times 1 -Exactly -ParameterFilter {
            $TimeoutSec -eq 1
        }
    }

    It 'keeps the default timeout for the unbudgeted deadline sentinel' {
        Get-AzDoFailedTestResultsByBuild `
            -Org 'dnceng-public' `
            -Project 'public' `
            -BuildId 123 | Out-Null

        Should -Invoke -CommandName Invoke-WebRequest -Times 1 -Exactly -ParameterFilter {
            $TimeoutSec -eq 100
        }
    }

    It 'does not issue a request after the visual budget is exhausted' {
        $result = Get-AzDoFailedTestResultsByBuild `
            -Org 'dnceng-public' `
            -Project 'public' `
            -BuildId 123 `
            -Deadline ((Get-Date).AddSeconds(-1))

        Should -Invoke -CommandName Invoke-WebRequest -Times 0 -Exactly
        $result.truncated | Should -BeTrue
    }
}

Describe 'Get-AzDoTestRuns overall deadline enforcement' {
    It 'returns an incomplete result without a request after the gather deadline' {
        Mock Invoke-WebRequest {
            throw 'request should not run'
        }

        $result = Get-AzDoTestRuns `
            -BaseUrl 'https://dev.azure.com/dnceng-public/public' `
            -BuildId 123 `
            -Deadline ((Get-Date).AddSeconds(-1))

        Should -Invoke Invoke-WebRequest -Times 0 -Exactly
        $result.truncated | Should -BeTrue
        $result.deadlineExhausted | Should -BeTrue
    }
}

Describe 'Get-HelixWorkItemCounts (anonymous /workitems completeness + fail counting)' {
    It 'confirms a finished, full, all-pass job as NOT unverified, 0 failures' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount 10 -JobFinished '2026-06-27T10:00:00Z'
        $r.sawCount | Should -BeTrue
        $r.totalFail | Should -Be 0
        $r.unverified | Should -BeFalse
    }

    It 'counts non-zero ExitCodes as failures and surfaces their names' {
        $mixed = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "ok$_"; State = 'Finished'; ExitCode = 0 } })
        $mixed += @(1..3 | ForEach-Object { [pscustomobject]@{ Name = "bad$_"; State = 'Finished'; ExitCode = 1 } })
        $r = Get-HelixWorkItemCounts -WorkItems $mixed -InitialWorkItemCount 13 -JobFinished '2026-06-27T10:00:00Z'
        $r.totalFail | Should -Be 3
        @($r.failedNames).Count | Should -Be 3
        (@($r.failedNames) | Where-Object { $_ -like 'bad*' }).Count | Should -Be 3
        $r.unverified | Should -BeFalse
    }

    It 'marks a still-running job unverified (items not Finished + no job finish)' {
        $running = @(1..2 | ForEach-Object { [pscustomobject]@{ Name = "run$_"; State = 'Running'; ExitCode = $null } })
        $running += @(1..5 | ForEach-Object { [pscustomobject]@{ Name = "done$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $running -InitialWorkItemCount 7 -JobFinished $null
        $r.unverified | Should -BeTrue
        $r.totalFail | Should -Be 0
    }

    It 'counts a hidden fail AND stays unverified when a sibling item is still running' {
        $mix2 = @(
            [pscustomobject]@{ Name = 'f1'; State = 'Finished'; ExitCode = 2 },
            [pscustomobject]@{ Name = 'r1'; State = 'Running'; ExitCode = $null }
        )
        $r = Get-HelixWorkItemCounts -WorkItems $mix2 -InitialWorkItemCount 2 -JobFinished '2026-06-27T10:00:00Z'
        $r.totalFail | Should -Be 1
        $r.unverified | Should -BeTrue
    }

    It 'marks a short set unverified (fewer returned than InitialWorkItemCount) even when all pass' {
        $short = @(1..3 | ForEach-Object { [pscustomobject]@{ Name = "s$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $short -InitialWorkItemCount 5 -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
    }

    It 'treats an empty array as unverified with no observed count' {
        $r = Get-HelixWorkItemCounts -WorkItems @() -InitialWorkItemCount 0 -JobFinished '2026-06-27T10:00:00Z'
        $r.sawCount | Should -BeFalse
        $r.unverified | Should -BeTrue
    }

    It 'treats $null work items as unverified with no observed count' {
        $r = Get-HelixWorkItemCounts -WorkItems $null -InitialWorkItemCount $null -JobFinished $null
        $r.sawCount | Should -BeFalse
        $r.unverified | Should -BeTrue
    }

    It 'treats a string error body as unverified' {
        $r = Get-HelixWorkItemCounts -WorkItems '404 Not Found' -InitialWorkItemCount $null -JobFinished $null
        $r.unverified | Should -BeTrue
    }

    It 'treats a non-integer ExitCode as unverified (cannot interpret), not a silent pass' {
        $weird = @([pscustomobject]@{ Name = 'w1'; State = 'Finished'; ExitCode = 'abc' })
        $r = Get-HelixWorkItemCounts -WorkItems $weird -InitialWorkItemCount 1 -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
        $r.totalFail | Should -Be 0
    }

    It 'marks unverified when the job-finished timestamp is blank' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount 10 -JobFinished '   '
        $r.unverified | Should -BeTrue
    }

    It 'handles IDictionary work items via Get-ObjectValue (member access alone would miss them)' {
        $dictItems = @(
            ([ordered]@{ Name = 'd1'; State = 'Finished'; ExitCode = 0 }),
            ([ordered]@{ Name = 'd2'; State = 'Finished'; ExitCode = 1 })
        )
        $r = Get-HelixWorkItemCounts -WorkItems $dictItems -InitialWorkItemCount 2 -JobFinished '2026-06-27T10:00:00Z'
        $r.totalFail | Should -Be 1
        $r.unverified | Should -BeFalse
    }

    It 'marks unverified when InitialWorkItemCount is null (completeness unknowable)' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount $null -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
        $r.totalFail | Should -Be 0
        $r.sawCount | Should -BeTrue
    }

    It 'marks unverified when InitialWorkItemCount is non-integer' {
        $allPass = @(1..10 | ForEach-Object { [pscustomobject]@{ Name = "wi$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $allPass -InitialWorkItemCount 'Unknown' -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeTrue
    }

    It 'confirms when actual count EXCEEDS planned (retry republished more items, not a shortfall)' {
        $seven = @(1..7 | ForEach-Object { [pscustomobject]@{ Name = "x$_"; State = 'Finished'; ExitCode = 0 } })
        $r = Get-HelixWorkItemCounts -WorkItems $seven -InitialWorkItemCount 6 -JobFinished '2026-06-27T10:00:00Z'
        $r.unverified | Should -BeFalse
        $r.totalFail | Should -Be 0
    }
}

Describe 'Get-XUnitFailures (parse xUnit v2 TestResults XML)' {
    It 'parses mixed Pass/Fail/Skip and captures the failure message' {
        $xmlMixed = @'
<assemblies>
  <assembly name="A.dll" total="4" passed="2" failed="1" skipped="1">
    <collection>
      <test name="N.T.Pass1" type="N.T" method="Pass1" result="Pass" />
      <test name="N.T.Pass2" type="N.T" method="Pass2" result="Pass" />
      <test name="N.T.Skipped" type="N.T" method="Skipped" result="Skip" />
      <test name="N.T.Boom" type="N.T" method="Boom" result="Fail">
        <failure exception-type="Xunit.Sdk.EqualException">
          <message><![CDATA[Assert.Equal() Failure]]></message>
          <stack-trace><![CDATA[at N.T.Boom()]]></stack-trace>
        </failure>
      </test>
    </collection>
  </assembly>
</assemblies>
'@
        $x = Get-XUnitFailures -Xml $xmlMixed
        $x.parsed | Should -BeTrue
        $x.total | Should -Be 4
        $x.passed | Should -Be 2
        $x.skipped | Should -Be 1
        $x.failed | Should -Be 1
        @($x.failedTests).Count | Should -Be 1
        $x.failedTests[0].name | Should -Be 'N.T.Boom'
        $x.failedTests[0].message | Should -Match 'Assert.Equal'
    }

    It 'parses an all-pass run with zero failed tests' {
        $xmlClean = '<assemblies><assembly total="2" passed="2" failed="0" skipped="0"><collection><test name="a" result="Pass"/><test name="b" result="Pass"/></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlClean
        $x.parsed | Should -BeTrue
        $x.failed | Should -Be 0
        @($x.failedTests).Count | Should -Be 0
    }

    It 'returns parsed=$false on malformed XML (caller must NOT trust a zero count)' {
        $x = Get-XUnitFailures -Xml '<assemblies><assembly broken'
        $x.parsed | Should -BeFalse
        $x.failed | Should -Be 0
    }

    It 'returns parsed=$false on empty/whitespace input' {
        $x = Get-XUnitFailures -Xml '   '
        $x.parsed | Should -BeFalse
    }

    It 'aggregates failures across multiple assemblies' {
        $xmlMulti = '<assemblies><assembly><collection><test name="f1" result="Fail"><failure><message>boom1</message></failure></test></collection></assembly><assembly><collection><test name="f2" result="Fail"><failure><message>boom2</message></failure></test><test name="p" result="Pass"/></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlMulti
        $x.failed | Should -Be 2
        (@($x.failedTests | ForEach-Object { $_.name }) -join ',') | Should -Be 'f1,f2'
    }

    It 'tolerates a UTF-8 BOM + xml declaration (live Helix per-category files are served this way)' {
        # Regression guard for the byte[]/BOM decode bug found in live validation: Azure blob
        # serves TestResults-*.xml as application/octet-stream, so the downloaded text can carry
        # a leading BOM that would otherwise make XmlDocument.LoadXml throw.
        $xmlBom = ([char]0xFEFF) + '<?xml version="1.0" encoding="utf-8"?>' + "`n" + '<assemblies><assembly total="1" passed="1" failed="0" skipped="0"><collection><test name="ok" result="Pass"/></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlBom
        $x.parsed | Should -BeTrue
        $x.passed | Should -Be 1
    }

    It 'counts an assembly-level error node (fixture/cleanup crash) with no failed-test element as a named failure' {
        # xUnit v2 records class/collection-fixture failures and unhandled cleanup exceptions as
        # <errors><error> nodes that carry no <test> element. Missing these would let a PR-introduced
        # fixture crash vanish while a sibling test dismisses on base -> a false green.
        $xmlErr = @'
<assemblies>
  <assembly name="A.dll" total="1" passed="1" failed="0" skipped="0" errors="1">
    <errors>
      <error type="assembly-cleanup" name="A.dll">
        <failure exception-type="System.InvalidOperationException">
          <message><![CDATA[fixture teardown threw]]></message>
        </failure>
      </error>
    </errors>
    <collection>
      <test name="A.Ok" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@
        $x = Get-XUnitFailures -Xml $xmlErr
        $x.parsed | Should -BeTrue
        $x.failed | Should -Be 1
        @($x.failedTests).Count | Should -Be 1
        $x.failedTests[0].name | Should -Be 'A.dll'
        $x.failedTests[0].message | Should -Match 'teardown'
    }

    It 'exposes declaredFailed > failed when the file asserts more failures than we could extract (truncation/schema drift)' {
        # The assembly attribute claims 3 failures but only one <test result="Fail"> is present -> the
        # file is truncated/partial. declaredFailed must exceed the extracted count so the caller can
        # flag the read as incomplete and cap to NHI instead of trusting the single name.
        $xmlShort = '<assemblies><assembly total="5" passed="2" failed="3" skipped="0"><collection><test name="only.one" result="Fail"><failure><message>boom</message></failure></test></collection></assembly></assemblies>'
        $x = Get-XUnitFailures -Xml $xmlShort
        $x.parsed | Should -BeTrue
        $x.failed | Should -Be 1
        $x.declaredFailed | Should -Be 3
        ($x.declaredFailed -gt $x.failed) | Should -BeTrue
    }

    It 'SECURITY: returns parsed=$false on a DOCTYPE (DtdProcessing=Prohibit blocks billion-laughs / XXE)' {
        $xmlDtd = @'
<?xml version="1.0"?>
<!DOCTYPE assemblies [
  <!ENTITY a "aaaaaaaaaa">
  <!ENTITY b "&a;&a;&a;&a;&a;&a;&a;&a;&a;&a;">
]>
<assemblies><assembly total="1" passed="0" failed="1"><collection><test name="x" result="Fail"><failure><message>&b;</message></failure></test></collection></assembly></assemblies>
'@
        $x = Get-XUnitFailures -Xml $xmlDtd
        # A blocked DTD parse caps to NHI (parsed=$false -> opaque fallback), never a false green.
        $x.parsed | Should -BeFalse
        $x.failed | Should -Be 0
    }
}

Describe 'Get-ConsoleFailureReason (extract crash/timeout reason from console log)' {
    It 'flags a timeout/hang as incomplete and captures the [FAIL] line' {
        $conTimeout = @'
Starting app for category discovery...
Waiting for category discovery (timeout: 60 seconds)...
[FAIL] Timeout waiting for HybridWebView test results after 480 seconds
Test execution completed with exit code: 1
'@
        $c = Get-ConsoleFailureReason -Console $conTimeout
        $c.isIncomplete | Should -BeTrue
        $c.isTimeout | Should -BeTrue
        $c.reason | Should -Match 'HybridWebView'
    }

    It 'flags an unhandled-exception crash as incomplete' {
        $conCrash = "Running tests...`nUnhandled exception. System.AccessViolationException`nProcess was killed"
        $c = Get-ConsoleFailureReason -Console $conCrash
        $c.isCrash | Should -BeTrue
        $c.isIncomplete | Should -BeTrue
    }

    It 'does NOT flag a clean run as incomplete' {
        $conClean = "Running tests...`nAll tests passed`nDone"
        $c = Get-ConsoleFailureReason -Console $conClean
        $c.isIncomplete | Should -BeFalse
        [string]::IsNullOrEmpty($c.reason) | Should -BeTrue
    }

    It 'does NOT flag empty console output as incomplete' {
        $c = Get-ConsoleFailureReason -Console ''
        $c.isIncomplete | Should -BeFalse
    }

    It 'does NOT over-cap a cleanly-completed Windows run that merely had failing tests' {
        # run-windows-devicetests.cmd:481 echoes "Test execution completed with exit code: 1"
        # UNCONDITIONALLY for any non-zero run, and "[FAIL] <test>" is a NAMED failure, not an
        # incomplete run. Neither may set isIncomplete, or every failed Windows work item would cap to
        # NHI and a clean named failure could never flow through to base/known-issue attribution.
        $conNamed = @'
Running Microsoft.Maui.DeviceTests...
[FAIL] Microsoft.Maui.DeviceTests.ButtonTests.SomeRealTest
ERROR: At least 1 test(s) failed
Test execution completed with exit code: 1
'@
        $c = Get-ConsoleFailureReason -Console $conNamed
        $c.isIncomplete | Should -BeFalse
        # the [FAIL]/exit-code lines are still captured for the human-readable reason
        $c.reason | Should -Match 'SomeRealTest'
    }

    It 'still flags a genuine timeout even when the unconditional exit-code line is present' {
        # The category-timeout marker (:wait_for_result) and the exit-code tail co-occur; the timeout
        # marker -- not the exit-code line -- must be what trips isIncomplete.
        $conTo = @'
[FAIL] Timeout waiting for HybridWebView test results after 480 seconds
Test execution completed with exit code: 1
'@
        $c = Get-ConsoleFailureReason -Console $conTo
        $c.isIncomplete | Should -BeTrue
    }

    It 'flags a total wipeout ("All test processes may have crashed") as incomplete' {
        $c = Get-ConsoleFailureReason -Console "Launching categories...`nAll test processes may have crashed`nTest execution completed with exit code: 1"
        $c.isIncomplete | Should -BeTrue
    }
}

Describe 'New-DeviceWorkItemFailureRecords (classify ONE failed work item — never a false green)' {
    BeforeEach {
        $script:ctx = @{ platform = 'windows'; buildId = 1; buildDefinition = 'maui-pr-devicetests'; helixJobId = 'job1'; helixWorkItem = 'WI' }
        $script:conOk = [ordered]@{ reason = ''; isTimeout = $false; isCrash = $false; isIncomplete = $false }
        $script:trxFail = [ordered]@{ parsed = $true; total = 3; passed = 2; failed = 1; skipped = 0; failedTests = @([ordered]@{ name = 'Ns.Real.Test'; type = 'Ns.Real'; method = 'Test'; message = 'boom' }) }
    }

    It 'A. emits only NAMED records for a cleanly-completed run with real failures' {
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
        $recs[0]['testName'] | Should -Be 'Ns.Real.Test'
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 0
    }

    It 'B. emits ONE incomplete capping record for an infra hang (0 failed + timeout + dump)' {
        $trxHang = [ordered]@{ parsed = $true; total = 713; passed = 713; failed = 0; skipped = 30; failedTests = @() }
        $conHang = [ordered]@{ reason = '[FAIL] Timeout waiting for HybridWebView'; isTimeout = $true; isCrash = $false; isIncomplete = $true }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $trxHang -Console $conHang -HasDump $true -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-workitem-incomplete'
        $recs[0]['crashDump'] | Should -BeTrue
        $recs[0]['message'] | Should -Match 'HybridWebView'
        $recs[0]['message'] | Should -Match '713 passed'
    }

    It 'C. emits BOTH named AND incomplete records when named failures coincide with a mid-run crash' {
        $conCrash2 = [ordered]@{ reason = 'crash'; isTimeout = $false; isCrash = $true; isIncomplete = $true }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $conCrash2 -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 1
    }

    It 'D. emits an incomplete record when no result file was readable' {
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $null -Console $null -HasDump $false -AnyResultFile $false -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-workitem-incomplete'
        $recs[0]['message'] | Should -Match 'no test-results file'
    }

    It 'E. SAFETY: a non-zero-exit work item with ZERO named failures STILL caps (never trust 0-fail as clean)' {
        $trxZero = [ordered]@{ parsed = $true; total = 5; passed = 5; failed = 0; skipped = 0; failedTests = @() }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $trxZero -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-workitem-incomplete'
    }

    It 'F. SAFETY: a crash DUMP alongside named failures STILL caps, even with a clean console' {
        # A SIGSEGV/abort can flush a partial result file then kill the run, so named failures + a dump
        # means the PR regression may be among the tests that never ran. The dump alone must force the
        # incomplete cap regardless of what the console says.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $true -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['crashDump'] | Should -BeTrue
        $inc[0]['message'] | Should -Match 'crash dump'
    }

    It 'G. SAFETY: a crash SIGNAL (isCrash) with isIncomplete=$false STILL caps alongside named failures' {
        # Console may detect a crash token (core dumped / .dmp / segfault) without setting isIncomplete;
        # isCrash alone must force the cap so a mid-run native crash can never be greened.
        $conCrashOnly = [ordered]@{ reason = ''; isTimeout = $false; isCrash = $true; isIncomplete = $false }
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $conCrashOnly -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 1
    }

    It 'H. SAFETY: ResultReadIncomplete (a result file overflowed/failed to read) STILL caps alongside named failures' {
        # Even with a clean console, no dump, and real named failures, a partial result-file read means
        # an unseen failure could be the PR's -> cap to NHI.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -ResultReadIncomplete $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['message'] | Should -Match 'could not be fully read'
    }

    It 'I. a fully-clean completed run (no dump, no crash, complete read) emits ONLY named records' {
        # Confirms the new guards did not over-cap: a clean run with real failures and no incompleteness
        # signal still dismisses through the normal named path (so base/known-issue dismissal still works).
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -ResultReadIncomplete $false -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
        @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' }).Count | Should -Be 0
    }

    It 'J. SAFETY: a NEGATIVE work-item ExitCode (WorkItemCrashed) STILL caps alongside named failures' {
        # A signal-killed work item (e.g. Helix ExitCode -4 on PR #36161) can flush a partial TRX with
        # real named failures before dying. A clean console + complete-looking read must NOT let it
        # green -- the negative exit code alone forces the incomplete cap.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -WorkItemCrashed $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['message'] | Should -Match 'terminated abnormally'
    }

    It 'K. WorkItemCrashed defaults to $false so a clean named run is unaffected' {
        # The crash defense must be OPT-IN: omitting -WorkItemCrashed (a normal positive-exit failure)
        # leaves the clean named-failure flow-through intact.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
    }

    It 'L. SAFETY: a TRUNCATED console (ConsoleReadIncomplete) STILL caps alongside named failures' {
        # The timeout/crash markers print at the TAIL, so a truncated read can drop them while the head
        # still shows named '[FAIL]' lines. A truncated console on a failed work item must therefore cap
        # to NHI -- the unread tail could hold the marker proving the run never finished.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -ConsoleReadIncomplete $true -Context $script:ctx)
        $recs.Count | Should -Be 2
        @($recs | Where-Object { $_['source'] -eq 'helix-trx' }).Count | Should -Be 1
        $inc = @($recs | Where-Object { $_['source'] -eq 'helix-workitem-incomplete' })
        $inc.Count | Should -Be 1
        $inc[0]['message'] | Should -Match 'console log was truncated'
    }

    It 'M. ConsoleReadIncomplete defaults to $false so a clean named run is unaffected' {
        # The truncation guard must be OPT-IN: omitting -ConsoleReadIncomplete (a console that fit under
        # the read cap) leaves the clean named-failure flow-through intact.
        $recs = @(New-DeviceWorkItemFailureRecords -Trx $script:trxFail -Console $script:conOk -HasDump $false -AnyResultFile $true -Context $script:ctx)
        $recs.Count | Should -Be 1
        $recs[0]['source'] | Should -Be 'helix-trx'
    }
}

Describe 'Get-AggregatedBaseLegMap (multi-build base leg diff — network-free via pre-seeded cache)' {
    It 'stops base timeline sampling after the overall gather deadline' {
        $result = Get-AggregatedBaseLegMap `
            -Org 'dnceng-public' `
            -Project 'public' `
            -BaseBuilds @([pscustomobject]@{ id = 100 }) `
            -Cache @{} `
            -Deadline ((Get-Date).AddSeconds(-1))

        $result.truncated | Should -BeTrue
        $result.sampledBuilds | Should -Be 0
    }

    # The aggregator only calls Get-TimelineRecordResultMap on a CACHE MISS, so pre-seeding $Cache with
    # entries keyed "org|project|buildId" is a fully network-free seam: each case supplies its own base
    # single-build leg maps and asserts the green/red tallies that decide whether a PR leg is a clean
    # 'regressed-vs-base' or a base flake. A cache MISS would call the (here-undefined)
    # Get-TimelineRecordResultMap and throw, so a clean return also proves no network was attempted.
    # Single-build cache entry shape mirrors Get-TimelineRecordResultMap: { accessible; records:
    # normName -> { name; hasFailed; hasSucceeded } }.

    It 'counts a leg GREEN across all sampled base builds (all-green -> greenCount=N, failedCount=0)' {
        $cache = @{
            'o|p|101' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
            'o|p|102' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
        }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 101 }, [ordered]@{ id = 102 }) -Cache $cache
        $agg.accessible | Should -BeTrue
        $agg.sampledBuilds | Should -Be 2
        $agg.records['leg a'].greenCount | Should -Be 2
        $agg.records['leg a'].failedCount | Should -Be 0
    }

    It 'tallies a leg red on some base builds and green on others (green-plus-red)' {
        $cache = @{
            'o|p|201' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $true; hasSucceeded = $false } } }
            'o|p|202' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
            'o|p|203' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
        }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 201 }, [ordered]@{ id = 202 }, [ordered]@{ id = 203 }) -Cache $cache
        $agg.sampledBuilds | Should -Be 3
        $agg.records['leg a'].greenCount | Should -Be 2
        $agg.records['leg a'].failedCount | Should -Be 1
    }

    It 'counts a retry-then-pass base build as RED for that build (hasFailed wins over hasSucceeded)' {
        # A base build where the leg failed one attempt but a retry later passed still carries a
        # base-branch flake -> it must count RED, never GREEN, so it cannot mask a base flake and let a
        # matching PR-red occurrence be read as a clean regression.
        $cache = @{ 'o|p|301' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $true; hasSucceeded = $true } } } }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 301 }) -Cache $cache
        $agg.records['leg a'].failedCount | Should -Be 1
        $agg.records['leg a'].greenCount | Should -Be 0
    }

    It 'skips an INACCESSIBLE base build (not counted, not sampled)' {
        $cache = @{
            'o|p|401' = [ordered]@{ accessible = $false; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
            'o|p|402' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } }
        }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 401 }, [ordered]@{ id = 402 }) -Cache $cache
        $agg.sampledBuilds | Should -Be 1
        $agg.records['leg a'].greenCount | Should -Be 1
        $agg.baseBuildIds.Count | Should -Be 1
        $agg.baseBuildIds[0] | Should -Be 402
    }

    It 'returns accessible=$false when NO base build was readable' {
        $cache = @{ 'o|p|501' = [ordered]@{ accessible = $false; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } } }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 501 }) -Cache $cache
        $agg.accessible | Should -BeFalse
        $agg.sampledBuilds | Should -Be 0
    }

    It 'REUSES the shared cache across calls (a base id fetched once serves later PR builds — network-free)' {
        # The outer loop shares ONE $baseRecordMapCache across PR builds; a base id read for one PR
        # build must be reused for the next without a second fetch (a cache MISS would call the
        # undefined Get-TimelineRecordResultMap and throw).
        $cache = @{ 'o|p|601' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } } }
        $agg1 = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 601 }) -Cache $cache
        $agg2 = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 601 }) -Cache $cache
        $agg1.records['leg a'].greenCount | Should -Be 1
        $agg2.records['leg a'].greenCount | Should -Be 1
        $cache.Keys.Count | Should -Be 1
    }

    It 'ignores base builds with a non-positive id' {
        $cache = @{ 'o|p|701' = [ordered]@{ accessible = $true; records = @{ 'leg a' = [ordered]@{ name = 'Leg A'; hasFailed = $false; hasSucceeded = $true } } } }
        $agg = Get-AggregatedBaseLegMap -Org 'o' -Project 'p' -BaseBuilds @([ordered]@{ id = 0 }, [ordered]@{ id = 701 }) -Cache $cache
        $agg.sampledBuilds | Should -Be 1
        $agg.baseBuildIds.Count | Should -Be 1
        $agg.baseBuildIds[0] | Should -Be 701
    }
}

Describe 'Test-IsTransientBuildErrorCode (transient infra vs deterministic toolchain boundary)' {
    It 'classifies restore/network + file-lock codes as transient' {
        Test-IsTransientBuildErrorCode -Signature 'NU1301'  | Should -BeTrue
        Test-IsTransientBuildErrorCode -Signature 'MSB3021' | Should -BeTrue
        Test-IsTransientBuildErrorCode -Signature 'MSB3027' | Should -BeTrue
    }

    It 'classifies deterministic compiler/toolchain codes as NOT transient' {
        Test-IsTransientBuildErrorCode -Signature 'CS0246'  | Should -BeFalse   # C# compile error
        Test-IsTransientBuildErrorCode -Signature 'NU1101'  | Should -BeFalse   # package not found (deterministic)
        Test-IsTransientBuildErrorCode -Signature 'MSB4018' | Should -BeFalse   # task failed unexpectedly (deterministic)
        Test-IsTransientBuildErrorCode -Signature 'Failed to load assembly' | Should -BeFalse
        Test-IsTransientBuildErrorCode -Signature 'CrossGen/R2R'            | Should -BeFalse
    }

    It 'treats an empty/whitespace signature as NOT transient' {
        Test-IsTransientBuildErrorCode -Signature ''    | Should -BeFalse
        Test-IsTransientBuildErrorCode -Signature '   ' | Should -BeFalse
    }
}

Describe 'Pinned PR patch evidence' -Tag 'EvidenceCollection' {
        BeforeEach {
            $script:patchText = "diff --git a/Test.cs b/Test.cs`n--- a/Test.cs`n+++ b/Test.cs`n@@ -1 +1 @@`n-old`n+new`n"
            Mock Invoke-ProcessWithGatherDeadline {
                return @{ exitCode = 0; stdout = $script:patchText; stderr = '' }
            }
        }

        It 'reads actual patch text using only the captured immutable base and head' {
            $diff = Get-PinnedPrDiff -Repository 'dotnet/maui' -BaseRefOid ('b' * 40) -HeadRefOid ('a' * 40)
            $diff.text | Should -BeExactly $script:patchText
            $diff.headRefOid | Should -Be ('a' * 40)
            $diff.baseRefOid | Should -Be ('b' * 40)
            $diff.truncated | Should -BeFalse
            $diff.error | Should -BeNullOrEmpty
            Should -Invoke Invoke-ProcessWithGatherDeadline -Times 1 -Exactly -ParameterFilter {
                $FileName -eq 'gh' -and $Arguments[0] -eq 'api' -and
                $Arguments[1] -eq "repos/dotnet/maui/compare/$('b' * 40)...$('a' * 40)" -and
                $Arguments -contains 'Accept: application/vnd.github.diff'
            }
        }

        It 'marks bounded patch text as truncated rather than implying omitted code is unchanged' {
            $diff = Get-PinnedPrDiff -Repository 'dotnet/maui' -BaseRefOid ('b' * 40) -HeadRefOid ('a' * 40) -MaxChars 60
            $diff.text.Length | Should -Be 60
            $diff.text | Should -Match '\[truncated\]$'
            $diff.truncated | Should -BeTrue
        }

        It 'retains explicit errors for an inaccessible patch' {
            Mock Invoke-ProcessWithGatherDeadline { return @{ exitCode = 1; stdout = ''; stderr = 'HTTP 404' } }
            $diff = Get-PinnedPrDiff -Repository 'dotnet/maui' -BaseRefOid ('b' * 40) -HeadRefOid ('a' * 40)
            $diff.error | Should -Match '404'
            $diff.text | Should -BeNullOrEmpty
        }

        It 'does not accept a missing immutable head or an empty patch as coverage' {
            $missing = Get-PinnedPrDiff -Repository 'dotnet/maui' -BaseRefOid ('b' * 40) -HeadRefOid ''
            $missing.error | Should -Match 'immutable'
            Should -Invoke Invoke-ProcessWithGatherDeadline -Times 0 -Exactly
            $script:patchText = ''
            (Get-PinnedPrDiff -Repository 'dotnet/maui' -BaseRefOid ('b' * 40) -HeadRefOid ('a' * 40)).error | Should -Match 'no patch text'
        }

        It 'reports the shared deadline instead of falling back to a moving PR diff' {
            Mock Invoke-ProcessWithGatherDeadline { throw 'Overall gather deadline was exhausted.' }
            (Get-PinnedPrDiff -Repository 'dotnet/maui' -BaseRefOid ('b' * 40) -HeadRefOid ('a' * 40)).error | Should -Match 'deadline'
            Should -Invoke Invoke-ProcessWithGatherDeadline -Times 1 -Exactly
        }
    }

    Describe 'Required pipeline discovery and build provenance' -Tag 'EvidenceCollection' {
        BeforeEach {
            Mock Invoke-GhJson { throw 'No unexpected GitHub requests.' }
        }

        It 'includes all three green pipeline slots even when a check filter selected only an unrelated check' {
            $checks = @(
                @{ name = 'maui-pr'; conclusion = 'SUCCESS'; detailsUrl = '?buildId=302' },
                @{ name = 'maui-pr-devicetests (iOS)'; conclusion = 'SUCCESS'; detailsUrl = '?buildId=314' },
                @{ name = 'maui-pr-uitests / Android'; conclusion = 'SUCCESS'; detailsUrl = '?buildId=313' }
            )
            $discovered = @(Get-ChecksForBuildDiscovery -Checks $checks -InterestingChecks @(@{ name = 'other'; detailsUrl = '?buildId=1' }))
            $discovered.Count | Should -Be 4
            $discovered.name | Should -Contain 'maui-pr'
            $discovered.name | Should -Contain 'maui-pr-devicetests (iOS)'
            $discovered.name | Should -Contain 'maui-pr-uitests / Android'
            @(Get-RequiredReviewPipelines).definitionId | Should -Be @(302, 314, 313)
        }

        It 'does not require a failing check when manual build IDs are the only extra input' {
            $checks = @(@{ name = 'maui-pr'; detailsUrl = '?buildId=302' })
            @(Get-ChecksForBuildDiscovery -Checks $checks -InterestingChecks @()).Count | Should -Be 1
        }

        It 'verifies an exact sourceVersion without a network query' {
            $build = New-HistoryBuildFixture -SourceVersion ('a' * 40)
            $proof = Get-BuildHeadEvidence -Build $build -Repository 'dotnet/maui' -PrNumber 123 -HeadRefOid ('a' * 40)
            $proof.verified | Should -BeTrue
            $proof.method | Should -Be 'sourceVersion'
            Should -Invoke Invoke-GhJson -Times 0 -Exactly
        }

        It 'accepts the PR source SHA for a merge build on this PR branch' {
            $build = New-HistoryBuildFixture
            $build | Add-Member triggerInfo @{ 'pr.sourceSha' = ('a' * 40) }
            $proof = Get-BuildHeadEvidence -Build $build -Repository 'dotnet/maui' -PrNumber 123 -HeadRefOid ('a' * 40)
            $proof.verified | Should -BeTrue
            $proof.method | Should -Be 'triggerInfo.pr.sourceSha'
        }

        It 'checks immutable merge parents when trigger metadata does not prove the head' {
            Mock Invoke-GhJson { return @{ sha = ('b' * 40); parents = @(@{ sha = ('c' * 40) }, @{ sha = ('a' * 40) }) } }
            $proof = Get-BuildHeadEvidence -Build (New-HistoryBuildFixture) -Repository 'dotnet/maui' -PrNumber 123 -HeadRefOid ('a' * 40)
            $proof.verified | Should -BeTrue
            $proof.method | Should -Be 'merge-parent'
            Should -Invoke Invoke-GhJson -Times 1 -Exactly -ParameterFilter {
                $Arguments[1] -eq "repos/dotnet/maui/git/commits/$('b' * 40)"
            }
        }

        It 'does not substitute an old revision or a different PR branch for the captured head' {
            $build = New-HistoryBuildFixture -Branch 'refs/pull/999/merge'
            $build | Add-Member triggerInfo @{ 'pr.sourceSha' = ('a' * 40) }
            $proof = Get-BuildHeadEvidence -Build $build -Repository 'dotnet/maui' -PrNumber 123 -HeadRefOid ('a' * 40)
            $proof.verified | Should -BeFalse
            $proof.error | Should -Match 'earlier PR revision'
            Should -Invoke Invoke-GhJson -Times 0 -Exactly
        }

        It 'reports unreadable merge provenance explicitly' {
            Mock Invoke-GhJson { throw 'HTTP 404 expired commit' }
            $proof = Get-BuildHeadEvidence -Build (New-HistoryBuildFixture) -Repository 'dotnet/maui' -PrNumber 123 -HeadRefOid ('a' * 40)
            $proof.verified | Should -BeFalse
            $proof.error | Should -Match '404'
        }
    }

    Describe 'Previous completed source-branch build selection' -Tag 'EvidenceCollection' {
        BeforeEach {
            $script:historyApiBuilds = @(1..6 | ForEach-Object {
                New-HistoryBuildFixture -Id (100 - $_) -FinishTime ([datetimeoffset]::Parse('2026-09-17T10:00:00Z').AddMinutes(-$_).ToString('o'))
            })
            Mock Invoke-AzDoJsonWithProjectFallback { return @{ value = @{ value = $script:historyApiBuilds }; error = $null } }
            $script:historyArguments = @{
                Org = 'dnceng-public'; Project = 'public'; DefinitionId = 302
                SourceBranch = 'refs/pull/123/merge'; CurrentBuildId = 100; BeforeQueueTime = '2026-09-17T10:00:00Z'
            }
        }

        It 'selects exactly five most recent completed runs before the current queue time by default' {
            $history = Get-RecentSourceBranchBuilds @historyArguments
            $history.builds.id | Should -Be @(99, 98, 97, 96, 95)
            $history.error | Should -BeNullOrEmpty
            Should -Invoke Invoke-AzDoJsonWithProjectFallback -Times 1 -Exactly -ParameterFilter {
                $RelativePath -match 'definitions=302&branchName=refs%2Fpull%2F123%2Fmerge' -and
                $RelativePath -match 'statusFilter=completed&maxTime=' -and
                $RelativePath -match 'queryOrder=finishTimeDescending&\$top=6&'
            }
        }

        It 'excludes the current build, other branches/definitions, unfinished runs, and time-boundary violations' {
            $script:historyApiBuilds += @(
                (New-HistoryBuildFixture -Id 100),
                (New-HistoryBuildFixture -Id 201 -DefinitionId 313),
                (New-HistoryBuildFixture -Id 202 -Branch 'refs/heads/main'),
                (New-HistoryBuildFixture -Id 203 -Status 'inProgress'),
                (New-HistoryBuildFixture -Id 204 -FinishTime '2026-09-17T10:00:00Z'),
                (New-HistoryBuildFixture -Id 205 -FinishTime '2026-09-17T10:01:00Z'),
                (New-HistoryBuildFixture -Id 206 -Branch 'refs/Pull/123/merge')
            )
            $queuedLater = New-HistoryBuildFixture -Id 207
            $queuedLater.queueTime = '2026-09-17T10:01:00Z'
            $script:historyApiBuilds += $queuedLater
            (Get-RecentSourceBranchBuilds @historyArguments).builds.id | Should -Be @(99, 98, 97, 96, 95)
        }

        It 'keeps older PR SHAs and separate reruns rather than filtering by the current commit' {
            $script:historyApiBuilds[0].sourceVersion = ('c' * 40)
            $history = Get-RecentSourceBranchBuilds @historyArguments
            $history.builds[0].sourceVersion | Should -Be ('c' * 40)
            $history.builds[1].sourceVersion | Should -Be ('b' * 40)
            $history.builds[2].sourceVersion | Should -Be ('b' * 40)
            $history.builds[1].id | Should -Not -Be $history.builds[2].id
            $history.builds[0].finishTime | Should -Not -BeNullOrEmpty
            $history.builds[0].url | Should -Match 'buildId=99'
        }

        It 'records a short history window explicitly instead of treating it as five clean runs' {
            $script:historyApiBuilds = @($script:historyApiBuilds | Select-Object -First 2)
            $history = Get-RecentSourceBranchBuilds @historyArguments
            $history.builds.Count | Should -Be 2
            $history.error | Should -Match 'Only 2 of 5'
            $history.builds[0].complete | Should -BeFalse
        }

        It 'reports unreadable, missing, or expired history metadata' {
            Mock Invoke-AzDoJsonWithProjectFallback { return @{ value = $null; error = 'HTTP 404 expired build metadata' } }
            $history = Get-RecentSourceBranchBuilds @historyArguments
            $history.builds.Count | Should -Be 0
            $history.error | Should -Match '404'
        }

        It 'issues no network request with an invalid anchor or exhausted deadline' {
            $script:historyArguments.BeforeQueueTime = ''
            (Get-RecentSourceBranchBuilds @historyArguments).error | Should -Match 'queue time'
            $script:historyArguments.BeforeQueueTime = '2026-09-17T10:00:00Z'
            (Get-RecentSourceBranchBuilds @historyArguments -Deadline ((Get-Date).AddSeconds(-1))).error | Should -Match 'deadline'
            Should -Invoke Invoke-AzDoJsonWithProjectFallback -Times 0 -Exactly
        }
    }

Describe 'Get-BuildErrorsFromLog (deterministicBuildError boundary — one-green-base shortcut gate)' {
    It 'flags a transient NuGet restore/network code (NU1301) as NON-deterministic' {
        $r = @(Get-BuildErrorsFromLog -Lines @('##[error]error NU1301: Unable to load the service index for source https://pkgs.dev.azure.com/x/index.json') -LogId 10 -RecordName 'Build_iOS')
        $r.Count | Should -Be 1
        $r[0].deterministicBuildError | Should -BeFalse
    }

    It 'flags transient MSBuild file-lock codes (MSB3021 / MSB3027) as NON-deterministic' {
        $r1 = @(Get-BuildErrorsFromLog -Lines @('error MSB3021: Unable to copy file "a.dll" to "b.dll". The process cannot access the file because it is being used by another process.') -LogId 11 -RecordName 'Build_Android')
        $r1[0].deterministicBuildError | Should -BeFalse
        $r2 = @(Get-BuildErrorsFromLog -Lines @('error MSB3027: Could not copy "a.dll" to "b.dll". Exceeded retry count of 10. Failed. The file is locked by: "dotnet".') -LogId 12 -RecordName 'Build_Android')
        $r2[0].deterministicBuildError | Should -BeFalse
    }

    It 'keeps a genuine deterministic compile break (CS0246) as deterministic' {
        $r = @(Get-BuildErrorsFromLog -Lines @('Foo.cs(12,5): error CS0246: The type or namespace name ''Bar'' could not be found') -LogId 13 -RecordName 'Build_Windows')
        $r.Count | Should -Be 1
        $r[0].deterministicBuildError | Should -BeTrue
    }

    It 'does NOT blanket-exclude the NU/MSB prefixes (NU1101, MSB4018 stay deterministic)' {
        $rNu = @(Get-BuildErrorsFromLog -Lines @('error NU1101: Unable to find package Foo. No packages exist with this id.') -LogId 14 -RecordName 'Build_iOS')
        $rNu[0].deterministicBuildError | Should -BeTrue
        $rMsb = @(Get-BuildErrorsFromLog -Lines @('error MSB4018: The "GenerateResource" task failed unexpectedly.') -LogId 15 -RecordName 'Build_iOS')
        $rMsb[0].deterministicBuildError | Should -BeTrue
    }

    It 'keeps a native crash NON-deterministic (unchanged behavior)' {
        $r = @(Get-BuildErrorsFromLog -Lines @('Process terminated. Segmentation fault (core dumped)') -LogId 16 -RecordName 'Run_iOS')
        $r.Count | Should -Be 1
        $r[0].deterministicBuildError | Should -BeFalse
    }
}

Describe 'Historical log and outcome completeness' -Tag 'EvidenceCollection' {
    BeforeEach {
        $script:logBuild = New-HistoryBuildFixture
        $script:timelineRecords = @(@{
            id = 'task'; name = 'Run Android tests'; type = 'Task'; state = 'completed'
            result = 'failed'; log = @{ id = 7 }; issues = @()
        })
        $script:historyLogText = "Failed Controls.Test [1 ms]`nExpected: InvalidOperationException`nActual: different result"
        Mock Invoke-AzDoJsonWithProjectFallback {
            if ($RelativePath -match '/timeline\?') {
                return @{ value = @{ records = $script:timelineRecords }; error = $null }
            }
            return @{ value = $script:logBuild; baseUrl = 'https://dev.azure.com/dnceng-public/public'; error = $null }
        }
        Mock Invoke-TextUrl { return $script:historyLogText }
    }

    It 'reuses the existing parser and retains exact extracted messages and outcome records' {
        $evidence = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence -FailureSource 'azdo-history-log'
        $evidence.failures[0].message | Should -BeExactly "Expected: InvalidOperationException`nActual: different result"
        $evidence.failures[0].source | Should -Be 'azdo-history-log'
        $evidence.outcomes[0].result | Should -Be 'failed'
        $evidence.readLogCount | Should -Be 1
        $evidence.complete | Should -BeTrue
    }

    It 'preserves the legacy baseline source and failure fields by default' {
        $evidence = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99
        $evidence.failures[0].source | Should -Be 'azdo-baseline-log'
        $evidence.failures[0].testName | Should -Be 'Controls.Test'
        $evidence.totalFailedRecords | Should -Be 1
        $evidence.inspectedLogCount | Should -Be 1
    }

    It 'requires readable timeline outcomes rather than successful metadata alone' {
        $script:logBuild.result = 'succeeded'
        $script:timelineRecords = @()
        $empty = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence
        $empty.complete | Should -BeFalse
        $empty.incompleteReasons -join ' ' | Should -Match 'No readable timeline'
        $script:timelineRecords = @(@{ name = 'Build'; type = 'Task'; state = 'completed'; result = 'succeeded' })
        (Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence).complete | Should -BeTrue
        Should -Invoke Invoke-TextUrl -Times 0 -Exactly
    }

    It 'keeps expired timelines and unreadable logs incomplete rather than clean zeroes' {
        Mock Invoke-AzDoJsonWithProjectFallback {
            if ($RelativePath -match '/timeline\?') { return @{ value = $null; error = 'HTTP 404 timeline expired' } }
            return @{ value = $script:logBuild; baseUrl = 'https://dev.azure.com/dnceng-public/public'; error = $null }
        }
        $evidence = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence
        $evidence.complete | Should -BeFalse
        $evidence.error | Should -Match 'expired'
        Should -Invoke Invoke-TextUrl -Times 0 -Exactly
    }

    It 'reports a partial log read even when other failures were extractable' {
        Mock Invoke-TextUrl { throw 'HTTP 404 expired log' }
        $evidence = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence
        $evidence.complete | Should -BeFalse
        $evidence.error | Should -Match 'could not be read'
        $evidence.readLogCount | Should -Be 0
    }

    It 'reports failed-log sampling and error-extraction caps explicitly' {
        $script:timelineRecords = @(1..3 | ForEach-Object {
            @{ name = "Task $_"; type = 'Task'; state = 'completed'; result = 'failed'; log = @{ id = $_ } }
        })
        $sampled = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence -MaxLogs 1
        $sampled.complete | Should -BeFalse
        $sampled.incompleteReasons -join ' ' | Should -Match 'first 1 of 3'
        $script:timelineRecords = @($script:timelineRecords[0])
        $script:historyLogText = (1..7 | ForEach-Object { "error CS0246: The type 'MissingType$_' could not be found" }) -join "`n"
        $capped = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence
        $capped.complete | Should -BeFalse
        $capped.incompleteReasons -join ' ' | Should -Match 'extraction cap'
        $capped.failures.Count | Should -Be 5
    }

    It 'keeps missing task logs, unexplained failures, and canceled outcomes incomplete' {
        $script:timelineRecords[0].log = $null
        $missing = Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence
        $missing.complete | Should -BeFalse
        $missing.incompleteReasons -join ' ' | Should -Match 'no readable log reference'
        $script:timelineRecords[0].log = @{ id = 7 }
        $script:historyLogText = 'No recognizable failure output'
        (Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence).complete | Should -BeFalse
        $script:logBuild.result = 'canceled'
        (Get-BuildLogTestFailures -Org 'dnceng-public' -Project 'public' -BuildId 99 -IncludeOutcomeEvidence).incompleteReasons -join ' ' | Should -Match 'complete run'
    }
}

Describe 'Bounded public failure detail evidence' -Tag 'EvidenceCollection' {
    BeforeEach {
        $script:failureMessage = "Expected: Handler_1`r`nActual: Handler_2; exact 'quoted' assertion."
        $script:failureStack = "at Controls.Test()`r`n  at Runner.Execute()"
        Mock Get-AzDoFailedTestResultsByBuild {
            return @{ results = @(@{ runId = 42; id = 7 }); truncated = $false }
        }
        Mock Invoke-JsonUrl {
            return @{
                testCaseTitle = 'Controls.Test'; automatedTestName = 'Controls.Test'; outcome = 'Failed'
                errorMessage = $script:failureMessage; stackTrace = $script:failureStack; testRun = @{ name = 'Android' }
            }
        }
    }

    It 'preserves the exact failure message and stack, with stable build/run/result references' {
        $evidence = Get-PublicBuildFailureEvidence -Org 'dnceng-public' -Project 'public' -BuildId 99 -DefinitionName 'maui-pr-uitests'
        $evidence.readable | Should -BeTrue
        $evidence.error | Should -BeNullOrEmpty
        $evidence.failures[0].message | Should -BeExactly $script:failureMessage
        $evidence.failures[0].stackTrace | Should -BeExactly $script:failureStack
        $evidence.failures[0].messageFingerprint | Should -Not -BeNullOrEmpty
        $evidence.failures[0].buildId | Should -Be 99
        $evidence.failures[0].runId | Should -Be 42
        $evidence.failures[0].resultId | Should -Be 7
        $evidence.failures[0].resultUrl | Should -Match 'Runs/42/Results/7'
    }

    It 'marks result/page limits and truncated failure text as incomplete evidence' {
        Mock Get-AzDoFailedTestResultsByBuild {
            return @{ results = @(@{ runId = 42; id = 7 }, @{ runId = 42; id = 8 }); truncated = $true }
        }
        $script:failureMessage = 'x' * 5000
        $evidence = Get-PublicBuildFailureEvidence -Org 'dnceng-public' -Project 'public' -BuildId 99 -MaxResults 1
        $evidence.truncated | Should -BeTrue
        $evidence.totalResults | Should -Be 2
        $evidence.failures.Count | Should -Be 1
        $evidence.failures[0].message.Length | Should -Be 4000
        $evidence.failures[0].messageTruncated | Should -BeTrue
        $evidence.error | Should -Match 'truncated'
        Should -Invoke Invoke-JsonUrl -Times 1 -Exactly
    }

    It 'retains the coverage error when details are unreadable rather than calling zero failures clean' {
        Mock Invoke-JsonUrl { throw 'HTTP 404 result expired' }
        $evidence = Get-PublicBuildFailureEvidence -Org 'dnceng-public' -Project 'public' -BuildId 99
        $evidence.failures.Count | Should -Be 0
        $evidence.totalResults | Should -Be 1
        $evidence.error | Should -Match 'expired'
    }

    It 'rejects failure detail from a different build' {
        Mock Invoke-JsonUrl { return @{ outcome = 'Failed'; build = @{ id = 999 }; errorMessage = 'not this build' } }
        $evidence = Get-PublicBuildFailureEvidence -Org 'dnceng-public' -Project 'public' -BuildId 99
        $evidence.failures.Count | Should -Be 0
        $evidence.error | Should -Match 'different build'
    }

    It 'returns explicit missing evidence without a request when the gather budget is exhausted' {
        $evidence = Get-PublicBuildFailureEvidence -Org 'dnceng-public' -Project 'public' -BuildId 99 -Deadline ((Get-Date).AddSeconds(-1))
        $evidence.readable | Should -BeFalse
        $evidence.error | Should -Match 'deadline'
        Should -Invoke Get-AzDoFailedTestResultsByBuild -Times 0 -Exactly
        Should -Invoke Invoke-JsonUrl -Times 0 -Exactly
    }
}

Describe 'Invalid failed-result collections' -Tag 'EvidenceCollection' {
    It 'rejects empty or schema-missing API responses' -TestCases @(@{ Content = '' }, @{ Content = '{}' }) {
        param($Content)
        $script:invalidResultContent = $Content
        Mock Invoke-WebRequest { return @{ Content = $script:invalidResultContent; Headers = @{} } }
        { Get-AzDoFailedTestResultsByBuild -Org 'dnceng-public' -Project 'public' -BuildId 99 } |
            Should -Throw '*no readable result collection*'
    }
}

Describe 'Three-pipeline history coverage' -Tag 'EvidenceCollection' {
    BeforeEach {
        $script:historyBuildResult = 'failed'
        $script:currentBuilds = @(
            (New-CurrentBuildFixture),
            (New-CurrentBuildFixture -DefinitionId 314 -Name 'maui-pr-devicetests'),
            (New-CurrentBuildFixture -DefinitionId 313 -Name 'maui-pr-uitests')
        )
        Mock Get-RecentSourceBranchBuilds {
            return @{
                error = $null
                builds = @(1..$Top | ForEach-Object {
                    [ordered]@{
                        id = $DefinitionId * 100 + $_; url = "https://example.invalid/build/$_"
                        sourceVersion = if ($_ -eq 1) { 'c' * 40 } else { 'b' * 40 }
                        queueTime = '2026-09-17T08:00:00Z'; finishTime = '2026-09-17T09:00:00Z'; result = $script:historyBuildResult
                        failures = @(); outcomes = @(); failureResultCount = $null; complete = $false; note = 'Not yet inspected.'
                    }
                })
            }
        }
        Mock Get-BuildLogTestFailures {
            return @{
                failures = @(@{ testName = 'Controls.Test'; message = "Expected: original`r`nActual: changed"; buildId = $BuildId; source = 'azdo-history-log' })
                outcomes = @(@{ name = 'Run tests'; state = 'completed'; result = 'failed' })
                complete = $true; incompleteReasons = @(); error = $null
            }
        }
        Mock Get-PublicBuildFailureEvidence {
            return @{ failures = @(); readable = $true; truncated = $false; totalResults = 0; error = $null }
        }
    }

    It 'collects five runs per definition without combining older PR SHAs or reruns' {
        $history = Get-PipelineHistory -Builds $currentBuilds
        $history.requestedBuildCount | Should -Be 5
        $history.pipelines.Count | Should -Be 3
        $history.pipelines.definitionId | Should -Be @(302, 314, 313)
        foreach ($pipeline in $history.pipelines) {
            $pipeline.branch | Should -Be 'refs/pull/123/merge'
            $pipeline.currentHeadVerified | Should -BeTrue
            $pipeline.builds.Count | Should -Be 5
            $pipeline.builds[0].sourceVersion | Should -Be ('c' * 40)
            $pipeline.builds[1].sourceVersion | Should -Be ('b' * 40)
            $pipeline.builds[2].sourceVersion | Should -Be ('b' * 40)
            $pipeline.builds[1].id | Should -Not -Be $pipeline.builds[2].id
            $pipeline.builds[0].failures[0].message | Should -BeExactly "Expected: original`r`nActual: changed"
        }
        Should -Invoke Get-RecentSourceBranchBuilds -Times 3 -Exactly -ParameterFilter {
            $SourceBranch -eq 'refs/pull/123/merge' -and $BeforeQueueTime -eq '2026-09-17T10:00:00Z' -and $Top -eq 5
        }
        Should -Invoke Get-BuildLogTestFailures -Times 15 -Exactly -ParameterFilter { $IncludeOutcomeEvidence -and $FailureSource -eq 'azdo-history-log' }
    }

    It 'never treats a green device build with an empty failure list as a complete passing run' {
        $script:historyBuildResult = 'succeeded'
        Mock Get-BuildLogTestFailures { return @{ failures = @(); outcomes = @(); complete = $true; incompleteReasons = @(); error = $null } }
        $history = Get-PipelineHistory -Builds $currentBuilds
        $device = $history.pipelines | Where-Object { $_.definitionId -eq 314 }
        $device.builds[0].result | Should -Be 'succeeded'
        $device.builds[0].complete | Should -BeFalse
        $device.builds[0].failures.Count | Should -Be 0
        $device.builds[0].note | Should -Match 'not passing device-test evidence'
    }

    It 'does not relabel current failures solely because the same failure appears in PR history' {
        $script:currentBuilds[0].testResults = @(@{
            testName = 'Controls.Test'; message = "Expected: original`r`nActual: changed"
            alsoFailsOnBaseline = $false; deterministicAttribution = 'indeterminate'
        })
        $before = $currentBuilds | ConvertTo-Json -Depth 20 -Compress
        Get-PipelineHistory -Builds $currentBuilds | Out-Null
        ($currentBuilds | ConvertTo-Json -Depth 20 -Compress) | Should -BeExactly $before
    }

    It 'keeps explicit missing and inaccessible rows when only one pipeline could be inspected' {
        $unreadable = @{ id = 314; org = 'dnceng-public'; project = 'public'; checkNames = @('maui-pr-devicetests'); accessible = $false; error = 'HTTP 403' }
        $history = Get-PipelineHistory -Builds @($currentBuilds[2], $unreadable)
        $history.pipelines.Count | Should -Be 3
        $history.pipelines[0].error | Should -Match 'No current build'
        $history.pipelines[1].error | Should -Match 'inaccessible.*403'
        $history.pipelines[2].currentBuildId | Should -Be 313
    }

    It 'does not use a manually supplied stale build as verified current-head coverage' {
        $stale = New-CurrentBuildFixture -Verified $false
        $history = Get-PipelineHistory -Builds @($stale)
        $history.pipelines[0].currentBuildId | Should -Be 302
        $history.pipelines[0].currentHeadVerified | Should -BeFalse
        $history.pipelines[0].error | Should -Match 'captured PR head'
        $history.pipelines[0].builds.Count | Should -Be 0
        Should -Invoke Get-RecentSourceBranchBuilds -Times 0 -Exactly
    }

    It 'selects a verified build ahead of a newer stale manual input and retains current timeline gaps' {
        $stale = New-CurrentBuildFixture -Verified $false
        $stale.id = 999
        $stale.metadata.queueTime = '2026-09-17T11:00:00Z'
        $script:currentBuilds[0].timelineReadable = $false
        $history = Get-PipelineHistory -Builds @($stale, $currentBuilds[0])
        $history.pipelines[0].currentBuildId | Should -Be 302
        $history.pipelines[0].currentTimelineReadable | Should -BeFalse
        $history.pipelines[0].currentError | Should -Match 'timeline is unreadable'
    }

    It 'preserves failures but marks expired or incomplete evidence as incomplete' {
        Mock Get-BuildLogTestFailures {
            return @{ failures = @(@{ testName = 'Controls.Test'; message = 'Exact failure' }); outcomes = @(); complete = $false; incompleteReasons = @(); error = 'Logs expired' }
        }
        Mock Get-PublicBuildFailureEvidence {
            return @{ failures = @(); readable = $false; truncated = $true; totalResults = 0; error = 'Result list unavailable' }
        }
        $history = Get-PipelineHistory -Builds @($currentBuilds[0])
        $entry = $history.pipelines[0].builds[0]
        $entry.complete | Should -BeFalse
        $entry.failures[0].message | Should -BeExactly 'Exact failure'
        $entry.failureResultCount | Should -BeNullOrEmpty
        $entry.note | Should -Match 'Logs expired'
        $entry.note | Should -Match 'empty list is not a clean run'
    }

    It 'retains every discovered historical row when the evidence-read deadline is exhausted' {
        $history = Get-PipelineHistory -Builds $currentBuilds -Deadline ((Get-Date).AddSeconds(-1))
        foreach ($pipeline in $history.pipelines) {
            $pipeline.builds.Count | Should -Be 5
            foreach ($build in $pipeline.builds) {
                $build.complete | Should -BeFalse
                $build.note | Should -Match 'deadline'
            }
        }
        Should -Invoke Get-BuildLogTestFailures -Times 0 -Exactly
        Should -Invoke Get-PublicBuildFailureEvidence -Times 0 -Exactly
    }
}

Describe 'Visual opt-out keeps ordinary UI failure evidence' -Tag 'EvidenceCollection' {
    It 'runs the real gather-loop guards without invoking visual discovery or attachments' {
        $SkipVisualEvidence = $true
        $build = @{ definition = @{ name = 'maui-pr-uitests' } }
        $buildRef = @{ org = 'dnceng-public'; project = 'public'; buildId = 313 }
        $buildSummary = @{ testResults = @(); publicTestEvidence = $null }
        $allLogFailures = New-Object System.Collections.Generic.List[object]
        $allUnexplainedLegs = New-Object System.Collections.Generic.List[object]
        $gatherHardDeadline = [datetime]::MaxValue
        Mock Get-PublicBuildFailureEvidence {
            return @{ readable = $true; truncated = $false; error = $null; failures = @(@{ testName = 'Ordinary.UI.Test'; message = 'Exact nonvisual assertion' }) }
        }
        Mock Get-VisualEvidenceBudgetDecision { throw 'Visual discovery must not start.' }
        Mock Get-AzDoFailedTestResultsByBuild { throw 'Visual-only results query must not start.' }
        Mock Select-VisualAttachments { throw 'Attachments must not be inspected.' }
        $guards = @($script:gatherAst.FindAll({
            param($node)
            if ($node -isnot [System.Management.Automation.Language.IfStatementAst]) { return $false }
            $condition = $node.Clauses[0].Item1.Extent.Text
            return $condition.Contains('$SkipVisualEvidence') -and $condition.Contains('$build.definition.name')
        }, $true))
        $guards.Count | Should -Be 4
        foreach ($guard in $guards) { Invoke-Expression $guard.Extent.Text }
        $buildSummary.testResults[0].testName | Should -Be 'Ordinary.UI.Test'
        $allLogFailures.Count | Should -Be 1
        $allUnexplainedLegs.Count | Should -Be 0
        Should -Invoke Get-PublicBuildFailureEvidence -Times 1 -Exactly
        Should -Invoke Get-VisualEvidenceBudgetDecision -Times 0 -Exactly
        Should -Invoke Get-AzDoFailedTestResultsByBuild -Times 0 -Exactly
        Should -Invoke Select-VisualAttachments -Times 0 -Exactly
    }
}

Describe 'Serialized evidence contract' -Tag 'EvidenceCollection' {
    It 'keeps pinned patch and three history slots beside the existing gate and baseline fields' {
        $pr = @{ number = 123; headRefOid = ('a' * 40); baseRefOid = ('b' * 40) }
        $prDiff = @{ text = 'actual patch'; truncated = $false; error = $null; headRefOid = $pr.headRefOid; baseRefOid = $pr.baseRefOid }
        $history = @{ requestedBuildCount = 5; pipelines = @(
            @{ name = 'maui-pr'; definitionId = 302; builds = @(); error = 'Not run' },
            @{ name = 'maui-pr-devicetests'; definitionId = 314; builds = @(); error = 'Inaccessible' },
            @{ name = 'maui-pr-uitests'; definitionId = 313; builds = @(); error = 'Unverified head' }
        ) }
        $changedFiles = @('Test.cs')
        $buildRefsById = [ordered]@{}
        $visualEvidenceArray = @()
        $visualEvidenceLimitations = New-Object System.Collections.Generic.List[string]
        $limitations = New-Object System.Collections.Generic.List[string]
        $gate = @{ unchanged = 'legacy gate' }
        $baselineSummaryArray = @(@{ unchanged = 'legacy baseline' })
        $SkipVisualEvidence = $true
        $contextAssignment = $script:gatherAst.EndBlock.Statements | Where-Object {
            $_ -is [System.Management.Automation.Language.AssignmentStatementAst] -and
            $_.Left.Extent.Text -eq '$context'
        }
        $context = Invoke-Expression $contextAssignment.Right.Extent.Text
        $serialized = $context | ConvertTo-Json -Depth 100 | ConvertFrom-Json
        $serialized.scope.diff.text | Should -BeExactly 'actual patch'
        $serialized.scope.diff.headRefOid | Should -Be $pr.headRefOid
        $serialized.history.requestedBuildCount | Should -Be 5
        $serialized.history.pipelines.definitionId | Should -Be @(302, 314, 313)
        $serialized.history.pipelines.error | Should -Be @('Not run', 'Inaccessible', 'Unverified head')
        $serialized.gate.unchanged | Should -Be 'legacy gate'
        $serialized.baselineSummary[0].unchanged | Should -Be 'legacy baseline'
        $serialized.visualEvidence.skipped | Should -BeTrue
    }

    It 'defaults to five historical runs and retains opt-in visual skipping' {
        $parameters = $script:gatherAst.ParamBlock.Parameters
        ($parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'HistoryBuilds' }).DefaultValue.Value | Should -Be 5
        $skip = $parameters | Where-Object { $_.Name.VariablePath.UserPath -eq 'SkipVisualEvidence' }
        $skip.StaticType | Should -Be ([System.Management.Automation.SwitchParameter])
        $skip.DefaultValue | Should -BeNullOrEmpty
    }
}

Describe 'Markdown evidence pointer' -Tag 'EvidenceCollection', 'MarkdownEvidence' {
    It 'writes the adjacent JSON link and evidence fields before legacy sections' {
        $PrNumber = 123
        $context = @{ generatedAtUtc = '2026-09-17T10:00:00Z' }
        $ContextMarkdownPath = Join-Path $TestDrive 'context.md'
        $statements = $script:gatherAst.EndBlock.Statements
        $start = $statements | Where-Object {
            $_ -is [System.Management.Automation.Language.AssignmentStatementAst] -and $_.Left.Extent.Text -eq '$md'
        }
        $end = $statements | Where-Object { $_.Extent.Text -eq '$md.Add("## Merge-readiness gate (deterministic)")' }
        $write = $statements | Where-Object {
            $_.Extent.Text.StartsWith('$md -join') -and $_.Extent.Text.Contains('Set-Content -Path $ContextMarkdownPath')
        }
        $start | Should -Not -BeNullOrEmpty
        $end | Should -Not -BeNullOrEmpty
        $write | Should -Not -BeNullOrEmpty
        foreach ($statement in $statements | Where-Object {
            $_.Extent.StartOffset -ge $start.Extent.StartOffset -and $_.Extent.EndOffset -le $end.Extent.EndOffset
        }) {
            Invoke-Expression $statement.Extent.Text
        }
        Invoke-Expression $write.Extent.Text
        $markdown = Get-Content -Path $ContextMarkdownPath -Raw
        $markdown | Should -Match '\[context\.json\]\(context\.json\)'
        $markdown | Should -Match '`scope\.diff` contains the PR patch'
        $markdown | Should -Match '`history` contains the previous completed runs.*same source branch'
        $markdown | Should -Match 'five requested by default'
        $markdown | Should -Match '(?m)^## Merge-readiness gate \(deterministic\)'
    }
}
