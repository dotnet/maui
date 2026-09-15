#!/usr/bin/env pwsh
#Requires -Modules Pester

# Regression coverage for the trusted build-evidence collector, for BOTH scanner
# twins (ci-status-main and ci-status-net11).
#
# The collector emits `failed_leaf_log_ids`, the set the manifest validator uses
# to forbid `signature-not-in-fetched-log` (an absence proof, satisfiable by any
# fabricated pattern) from covering a log that really failed. A DeviceTests
# submission log is required evidence even when the AzDO task is green, because
# the failure lives in Helix work items discovered AFTER the timeline
# classification loop. If those Helix failures are not folded back into
# `failed_leaf_log_ids`, a genuine device-test failure stays absence-skippable
# and the run goes green with nothing filed.
#
# These tests extract the collector from the COMPILED lock (not the .md source)
# and execute it under node, so they fail if the fold is dropped or if the lock
# stops being regenerated from source.

BeforeDiscovery {
    $script:NodeAvailable = $null -ne (Get-Command node -ErrorAction SilentlyContinue)

    . (Join-Path $PSScriptRoot 'CiScanTwins.Helpers.ps1')
    $script:CollectorTwins = @(Get-CiScanTwin)
}

BeforeAll {
    $script:HelixJobId = '0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0'

    function Get-CollectorSource {
        $lock = Get-Content -LiteralPath $script:LockPath -Raw
        $match = [regex]::Match(
            $lock,
            '(?m)^\s*- name: Freeze trusted scanner build evidence.*?^\s*script: ("(?:[^"\\]|\\.)*")\s*$',
            [System.Text.RegularExpressions.RegexOptions]::Singleline)
        if (-not $match.Success) {
            throw 'The compiled lock no longer contains the trusted build-evidence collector.'
        }

        # A YAML double-quoted scalar uses JSON string escapes, so the compiled
        # one-line body round-trips through ConvertFrom-Json unchanged.
        return ($match.Groups[1].Value | ConvertFrom-Json)
    }

    function Invoke-Collector {
        param(
            [Parameter(Mandatory = $true)][object[]]$Fixtures,
            [switch]$ConstantDeadletterContent
        )

        $runnerTemp = Join-Path $TestDrive 'runner-temp'
        $agentRoot = Join-Path $TestDrive 'agent-trusted'
        $fixturePath = Join-Path $TestDrive 'fixtures.json'
        $harness = Join-Path $TestDrive 'collector.js'
        New-Item -ItemType Directory -Force -Path $runnerTemp, $agentRoot | Out-Null
        Set-Content -LiteralPath $fixturePath -Value ($Fixtures | ConvertTo-Json -Depth 8 -AsArray)

        # Three substitutions, all unavoidable outside a real Actions run:
        #   * `${{ github.workflow_sha }}` is an Actions expression, not JS.
        #   * `agentRoot` is the hard-coded gh-aw runtime path; redirect it into
        #     TestDrive so the suite never writes outside its own sandbox.
        #   * Terminality retries stay live, but test fixtures need no wall-clock delay.
        $body = (Get-CollectorSource).
        Replace('${{ github.workflow_sha }}', ('a' * 40)).
        Replace(
            "const agentRoot = '/tmp/gh-aw/agent/trusted';",
            'const agentRoot = process.env.CI_SCAN_TEST_AGENT_ROOT;').
        Replace('await sleep(5000);', 'await sleep(0);')
        if ($ConstantDeadletterContent) {
            $needle = 'content: deadletterEvidenceLine,'
            if (($body.Split($needle).Count - 1) -ne 1) {
                throw 'The constant-deadletter-content mutation no longer matches the collector.'
            }
            $body = $body.Replace($needle, 'content: deadletterUrl.toString(),')
        }

        $script = @"
const fixtures = require($($fixturePath | ConvertTo-Json));
globalThis.fetch = async url => {
  const target = String(url);
  const fixture = fixtures.find(candidate => target.includes(candidate.match));
  if (!fixture) {
    throw new Error(``unmocked request: `${target}``);
  }
  return {
    ok: true,
    status: 200,
    json: async () => fixture.json,
    text: async () => String(fixture.text === undefined ? '' : fixture.text),
  };
};
(async () => {
$body
})().catch(error => {
  console.error(error && error.message ? error.message : String(error));
  process.exit(1);
});
"@
        Set-Content -LiteralPath $harness -Value $script

        $previousRunnerTemp = $env:RUNNER_TEMP
        $env:RUNNER_TEMP = $runnerTemp
        $env:CI_SCAN_TEST_AGENT_ROOT = $agentRoot
        try {
            $output = & node $harness 2>&1
            if ($LASTEXITCODE -ne 0) {
                throw "collector harness failed: $output"
            }
        } finally {
            $env:RUNNER_TEMP = $previousRunnerTemp
            Remove-Item Env:CI_SCAN_TEST_AGENT_ROOT -ErrorAction SilentlyContinue
        }

        return (Get-Content -LiteralPath (Join-Path $runnerTemp "$($script:ScannerId)/expected-builds.json") -Raw |
                ConvertFrom-Json)
    }

    function Get-CollectorEvidence {
        # Reads the evidence log the collector wrote for a scanned pipeline.
        # `Invoke-Collector` pins RUNNER_TEMP to this deterministic TestDrive
        # path, and the collector writes with fs.writeFileSync, so this always
        # returns the current run's content rather than a stale sibling test's.
        param(
            [Parameter(Mandatory = $true)][string]$Pipeline,
            [Parameter(Mandatory = $true)][string]$LogFileName
        )

        $evidencePath = Join-Path $TestDrive "runner-temp/$($script:ScannerId)/evidence/$Pipeline/$LogFileName"
        if (-not (Test-Path -LiteralPath $evidencePath)) {
            throw "The collector wrote no evidence at $evidencePath."
        }

        return (Get-Content -LiteralPath $evidencePath -Raw)
    }

    function New-DeviceTestsFixtures {
        param(
            [Parameter(Mandatory = $true)][string]$TaskResult,
            [Parameter(Mandatory = $true)][string]$WorkItemState,
            [Parameter(Mandatory = $true)][int]$WorkItemExitCode,
            [string]$ConsoleOutputUri = 'https://helix.blob.core.windows.net/console/Controls.DeviceTests.log',
            [object]$BuildFinishTime = (Get-Date).ToUniversalTime().ToString('o'),
            [int]$InitialWorkItemCount = 1,
            [int]$FinishedWorkItemCount = 1,
            [int]$UnscheduledWorkItemCount = 0,
            [int]$WaitingWorkItemCount = 0,
            [int]$RunningWorkItemCount = 0,
            [object[]]$WorkItems
        )

        $emptyBuilds = [pscustomobject]@{ value = @() }
        $submissionLog = @(
            'Helix Job: submitted',
            "https://helix.dot.net/api/jobs/$($script:HelixJobId)/workitems"
        ) -join "`n"
        if (-not $PSBoundParameters.ContainsKey('WorkItems')) {
            $WorkItems = @(
                [pscustomobject]@{
                    Name             = 'Controls.DeviceTests'
                    State            = $WorkItemState
                    ExitCode         = $WorkItemExitCode
                    ConsoleOutputUri = $ConsoleOutputUri
                }
            )
        }

        return @(
            # maui-pr and maui-pr-uitests have no recent build, so only the
            # DeviceTests pipeline contributes to the inventory under test.
            [pscustomobject]@{ match = 'definitions=302'; json = $emptyBuilds }
            [pscustomobject]@{ match = 'definitions=313'; json = $emptyBuilds }
            [pscustomobject]@{
                match = 'definitions=314'
                json  = [pscustomobject]@{
                    value = @(
                        [pscustomobject]@{
                            id           = 5000
                            finishTime   = $BuildFinishTime
                            status       = 'completed'
                            result       = 'succeeded'
                            sourceBranch = "refs/heads/$($script:ScannerBranch)"
                            definition   = [pscustomobject]@{ id = 314 }
                        }
                    )
                }
            }
            [pscustomobject]@{
                match = '/builds/5000/timeline'
                json  = [pscustomobject]@{
                    records = @(
                        [pscustomobject]@{
                            id       = 'record-1'
                            parentId = $null
                            type     = 'Task'
                            name     = 'DeviceTests Controls (Unix)'
                            result   = $TaskResult
                            log      = [pscustomobject]@{ id = 1001 }
                        }
                    )
                }
            }
            [pscustomobject]@{ match = '/builds/5000/logs/1001'; text = $submissionLog }
            [pscustomobject]@{
                match = "jobs/$($script:HelixJobId)/details"
                json  = [pscustomobject]@{
                    Finished             = '2026-07-28T17:16:15.8550000+00:00'
                    InitialWorkItemCount = $InitialWorkItemCount
                    WorkItems            = [pscustomobject]@{
                        Finished    = $FinishedWorkItemCount
                        Unscheduled = $UnscheduledWorkItemCount
                        Waiting     = $WaitingWorkItemCount
                        Running     = $RunningWorkItemCount
                    }
                }
            }
            [pscustomobject]@{
                match = "jobs/$($script:HelixJobId)/workitems"
                json  = $WorkItems
            }
            [pscustomobject]@{
                match = 'helix.blob.core.windows.net'
                text  = 'System.NullReferenceException in Microsoft.Maui.DeviceTests.ButtonTests'
            }
        )
    }
}

Describe 'trusted build-evidence collector: <_.Name>' -ForEach $script:CollectorTwins {
    BeforeAll {
        # Per-twin bindings; the file-level BeforeAll above only defines helpers,
        # because -ForEach data is not in scope there.
        $script:LockPath = $LockPath
        $script:ScannerId = $ScannerId
        $script:ScannerBranch = $Branch
    }

    It 'folds Helix-discovered failures into failed_leaf_log_ids in the compiled lock' {
        # Source-level guard so the contract is still enforced where node is
        # unavailable: the fold must live inside the Helix work-item loop, after
        # the failed console output is appended to the evidence file.
        $source = Get-CollectorSource
        $consoleAppend = $source.IndexOf('===== Helix console ')
        $fold = $source.IndexOf('failedLeafLogIds.add(logId);', $consoleAppend)
        $emit = $source.IndexOf('failed_leaf_log_ids: [...failedLeafLogIds]')
        $consoleAppend | Should -BeGreaterThan 0
        $fold | Should -BeGreaterThan $consoleAppend
        $emit | Should -BeGreaterThan $fold
    }

    It 'emits structured raw segments separately from synthetic provenance framing' -Skip:(-not $script:NodeAvailable) {
        Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Failed' `
                -WorkItemExitCode 1) | Out-Null

        $rendered = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.log'
        $raw = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.evidence.json' |
            ConvertFrom-Json

        $rendered | Should -Match '===== AzDO log 5000/1001 ====='
        $rendered | Should -Match '===== Helix console '
        $raw.schema_version | Should -Be 1
        $raw.pipeline | Should -Be 'maui-pr-devicetests'
        @($raw.segments.kind) | Should -Be @('azdo-log', 'helix-console')
        ($raw.segments.content -join "`n") | Should -Not -Match '===== (?:AzDO log|Helix console) '
    }

    It 'enforces structured evidence caps before writing either representation' {
        $source = Get-CollectorSource
        $segmentCap = $source.IndexOf('rawSegments.length > 200')
        $sizeCap = $source.IndexOf('structuredEvidence.length > 25_000_000')
        $renderedWrite = $source.IndexOf('evidence.join')
        $structuredWrite = $source.IndexOf('.evidence.json')

        $segmentCap | Should -BeGreaterThan 0
        $sizeCap | Should -BeGreaterThan $segmentCap
        $renderedWrite | Should -BeGreaterThan $sizeCap
        $structuredWrite | Should -BeGreaterThan $renderedWrite
    }

    It 'marks a green DeviceTests submission log as failed-leaf when its Helix work items failed' -Skip:(-not $script:NodeAvailable) {
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Failed' `
                -WorkItemExitCode 1)

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        $devicePipeline.status | Should -Be 'scanned'
        @($devicePipeline.required_log_ids) | Should -Be @(1001)
        # Absence alone must not cover this log: the failure is real, it just
        # surfaced through Helix rather than the AzDO timeline.
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)
    }

    It 'marks a green DeviceTests submission log as failed-leaf on a non-zero work-item exit code' -Skip:(-not $script:NodeAvailable) {
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeededWithIssues' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 3)

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)
    }

    It 'treats a deadletter console URI as a Helix failure in the compiled lock' {
        # Source-level guard for environments without node. The deadletter check
        # must be part of isFailure -- if it were only consulted after the
        # `continue`, a Finished/exit-0 deadletter would still be skipped.
        $source = Get-CollectorSource
        $deadletter = $source.IndexOf('helix-workitem-deadletter')
        $isFailure = $source.IndexOf('const isFailure')
        $continue = $source.IndexOf('if (!isFailure)')
        $deadletter | Should -BeGreaterThan 0
        $isFailure | Should -BeGreaterThan $deadletter
        $continue | Should -BeGreaterThan $isFailure
        $source.Substring($isFailure, $continue - $isFailure) | Should -Match 'isDeadletter'
    }

    It 'handles a deadletter before the blob-only console allowlist in the compiled lock' {
        # Source-level guard for environments without node. Production
        # deadletters are served from dotnet.github.io, not the blob host the
        # console fetch allows, so the deadletter branch MUST come first. If it
        # were ordered after the allowlist, every real deadletter would throw
        # `invalid console URL` and abort the scan before the fold below.
        $source = Get-CollectorSource
        $deadletterBranch = $source.IndexOf('if (isDeadletter)')
        $allowlist = $source.IndexOf("endsWith('.blob.core.windows.net')")
        $deadletterBranch | Should -BeGreaterThan 0
        $allowlist | Should -BeGreaterThan $deadletterBranch
    }

    It 'marks a deadlettered Helix work item as failed-leaf despite Finished/exit 0' -Skip:(-not $script:NodeAvailable) {
        # A deadlettered work item never ran, so Helix reports it terminal and
        # green. State/ExitCode therefore cannot see it, but the workflow's own
        # Helix reference calls a `helix-workitem-deadletter` console URI an
        # infra failure -- so it must not stay absence-skippable.
        #
        # This uses the REAL production deadletter URI, verified live against
        # Helix job a755e8d4-4f81-48be-8dbc-13e723054eb5 (work item
        # com.microsoft.maui.controls.devicetests-Signed, State=Finished,
        # ExitCode=-1). Its host is dotnet.github.io, NOT the blob host the
        # console fetch allows -- a blob-host fixture here passes the allowlist
        # and silently masks the production path. The fetch mock has no entry
        # for this URI, so the harness would throw `unmocked request` if the
        # collector ever tried to fetch the placeholder instead of skipping it.
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -ConsoleOutputUri 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt')

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        @($devicePipeline.required_log_ids) | Should -Be @(1001)
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)
    }

    It 'accepts the completed Helix response whose Unscheduled count remains cumulative' -Skip:(-not $script:NodeAvailable) {
        # Live response from job a755e8d4-4f81-48be-8dbc-13e723054eb5:
        # InitialWorkItemCount=5, Finished=6, Unscheduled=5, with six terminal
        # returned items. Unscheduled is not a pending count once the job has
        # Finished, so terminality must come from the returned work-item states.
        $terminalItems = @(
            [pscustomobject]@{
                Name = 'com.microsoft.maui.controls.devicetests-Signed'
                State = 'Finished'
                ExitCode = -1
                ConsoleOutputUri = 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'
            }
            [pscustomobject]@{
                Name = 'com.microsoft.maui.mauiblazorwebview.devicetests-Signed'
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = 'https://helix.blob.core.windows.net/console/MauiBlazorWebView.DeviceTests.log'
            }
            [pscustomobject]@{
                Name = 'com.microsoft.maui.graphics.devicetests-Signed'
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = 'https://helix.blob.core.windows.net/console/Graphics.DeviceTests.log'
            }
            [pscustomobject]@{
                Name = 'com.microsoft.maui.essentials.devicetests-Signed'
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = 'https://helix.blob.core.windows.net/console/Essentials.DeviceTests.log'
            }
            [pscustomobject]@{
                Name = 'com.microsoft.maui.core.devicetests-Signed'
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = 'https://helix.blob.core.windows.net/console/Core.DeviceTests.log'
            }
            [pscustomobject]@{
                Name = 'HelixController Work Queueing'
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = ''
            }
        )
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -InitialWorkItemCount 5 `
                -FinishedWorkItemCount 6 `
                -UnscheduledWorkItemCount 5 `
                -WorkItems $terminalItems)

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        $devicePipeline.status | Should -Be 'scanned'
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)
    }

    It 'rejects a Helix response that claims Finished while work items are waiting' -Skip:(-not $script:NodeAvailable) {
        {
            Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                    -TaskResult 'succeeded' `
                    -WorkItemState 'Finished' `
                    -WorkItemExitCode 0 `
                    -WaitingWorkItemCount 1)
        } | Should -Throw '*did not provide complete terminal work-item evidence*'
    }

    It 'rejects a Helix response that claims Finished while work items are running' -Skip:(-not $script:NodeAvailable) {
        {
            Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                    -TaskResult 'succeeded' `
                    -WorkItemState 'Finished' `
                    -WorkItemExitCode 0 `
                    -RunningWorkItemCount 1)
        } | Should -Throw '*did not provide complete terminal work-item evidence*'
    }

    It 'rejects a truthy but invalid AzDO finishTime' -Skip:(-not $script:NodeAvailable) {
        {
            Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                    -TaskResult 'succeeded' `
                    -WorkItemState 'Finished' `
                    -WorkItemExitCode 0 `
                    -BuildFinishTime 'not-a-date')
        } | Should -Throw '*invalid finishTime*'
    }

    It 'records stable work-item-bound deadletter evidence without fetching it' -Skip:(-not $script:NodeAvailable) {
        # The placeholder carries no diagnostics and is constant across Helix.
        # The countable evidence line therefore binds it to the trusted work-item
        # name while remaining stable across builds for legitimate recurrence.
        $inventory = Invoke-Collector `
            -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -ConsoleOutputUri 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt')
        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)

        $evidence = Get-CollectorEvidence -Pipeline 'maui-pr-devicetests' -LogFileName '5000-1001.log'
        $evidence | Should -Match 'Helix deadletter'
        $evidence | Should -Match ([regex]::Escape('https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'))
        # The blob-only console fetch must not have run for a deadletter.
        $evidence | Should -Not -Match 'Helix console '

        $raw = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.evidence.json' |
            ConvertFrom-Json
        $deadletter = @($raw.segments | Where-Object kind -eq 'helix-deadletter-uri')
        $deadletter.Count | Should -Be 1
        $deadletter[0].source | Should -Be "$($script:HelixJobId)/Controls.DeviceTests"
        $deadletter[0].content | Should -BeExactly (
            'Helix work item Controls.DeviceTests was deadlettered: ' +
            'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt')
    }

    It 'binds unrelated deadletters to distinct work-item evidence' -Skip:(-not $script:NodeAvailable) {
        $newDeadletter = {
            param([string]$Name)
            [pscustomobject]@{
                Name = $Name
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'
            }
        }

        Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -WorkItems @(& $newDeadletter 'android-emulator-boot')) | Out-Null
        $android = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.evidence.json' |
            ConvertFrom-Json

        Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -WorkItems @(& $newDeadletter 'ios-device-lost')) | Out-Null
        $ios = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.evidence.json' |
            ConvertFrom-Json

        $android.segments[-1].content | Should -Not -BeExactly $ios.segments[-1].content
        $android.segments[-1].content | Should -Match 'android-emulator-boot'
        $ios.segments[-1].content | Should -Match 'ios-device-lost'
    }

    It 'mutation "constant-deadletter-content": unrelated work items collapse to one identity' -Skip:(-not $script:NodeAvailable) {
        $newDeadletter = {
            param([string]$Name)
            [pscustomobject]@{
                Name = $Name
                State = 'Finished'
                ExitCode = 0
                ConsoleOutputUri = 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'
            }
        }

        Invoke-Collector `
            -ConstantDeadletterContent `
            -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -WorkItems @(& $newDeadletter 'android-emulator-boot')) | Out-Null
        $android = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.evidence.json' |
            ConvertFrom-Json

        Invoke-Collector `
            -ConstantDeadletterContent `
            -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -WorkItems @(& $newDeadletter 'ios-device-lost')) | Out-Null
        $ios = Get-CollectorEvidence `
            -Pipeline 'maui-pr-devicetests' `
            -LogFileName '5000-1001.evidence.json' |
            ConvertFrom-Json

        $android.segments[-1].content | Should -BeExactly $ios.segments[-1].content
        $android.segments[-1].content |
            Should -BeExactly 'https://dotnet.github.io/core-eng/helix-workitem-deadletter.txt'
    }

    It 'still marks a deadlettered work item on a blob-hosted URI as failed-leaf' -Skip:(-not $script:NodeAvailable) {
        # Helix has also served deadletter URIs from the blob host. That form
        # would pass the console allowlist, so guard that it takes the same
        # no-fetch deadletter path rather than being fetched as a real console.
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0 `
                -ConsoleOutputUri 'https://helix.blob.core.windows.net/helix-workitem-deadletter/Controls.DeviceTests.log')

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)
    }

    It 'leaves a clean DeviceTests submission log absence-skippable' -Skip:(-not $script:NodeAvailable) {
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'succeeded' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0)

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        # Still required evidence, but nothing failed, so an absence proof is a
        # legitimate way to cover it.
        @($devicePipeline.required_log_ids) | Should -Be @(1001)
        @($devicePipeline.failed_leaf_log_ids) | Should -BeNullOrEmpty
    }

    It 'still marks a failed DeviceTests submission task as failed-leaf' -Skip:(-not $script:NodeAvailable) {
        $inventory = Invoke-Collector -Fixtures (New-DeviceTestsFixtures `
                -TaskResult 'failed' `
                -WorkItemState 'Finished' `
                -WorkItemExitCode 0)

        $devicePipeline = @($inventory.pipelines | Where-Object { $_.name -eq 'maui-pr-devicetests' })[0]
        @($devicePipeline.failed_leaf_log_ids) | Should -Be @(1001)
    }
}
