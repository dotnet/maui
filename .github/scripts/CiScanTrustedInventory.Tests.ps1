#!/usr/bin/env pwsh
#Requires -Modules Pester

# Regression coverage for the ci-status-net11 trusted build-evidence collector.
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
}

BeforeAll {
    $script:LockPath = Join-Path $PSScriptRoot '../workflows/ci-status-net11.lock.yml'
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
            [Parameter(Mandatory = $true)][object[]]$Fixtures
        )

        $runnerTemp = Join-Path $TestDrive 'runner-temp'
        $agentRoot = Join-Path $TestDrive 'agent-trusted'
        $fixturePath = Join-Path $TestDrive 'fixtures.json'
        $harness = Join-Path $TestDrive 'collector.js'
        New-Item -ItemType Directory -Force -Path $runnerTemp, $agentRoot | Out-Null
        Set-Content -LiteralPath $fixturePath -Value ($Fixtures | ConvertTo-Json -Depth 8 -AsArray)

        # Two substitutions, both unavoidable outside a real Actions run:
        #   * `${{ github.workflow_sha }}` is an Actions expression, not JS.
        #   * `agentRoot` is the hard-coded gh-aw runtime path; redirect it into
        #     TestDrive so the suite never writes outside its own sandbox.
        $body = (Get-CollectorSource).
        Replace('${{ github.workflow_sha }}', ('a' * 40)).
        Replace(
            "const agentRoot = '/tmp/gh-aw/agent/trusted';",
            'const agentRoot = process.env.CI_SCAN_TEST_AGENT_ROOT;')

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

        return (Get-Content -LiteralPath (Join-Path $runnerTemp 'ci-scan-net11/expected-builds.json') -Raw |
                ConvertFrom-Json)
    }

    function New-DeviceTestsFixtures {
        param(
            [Parameter(Mandatory = $true)][string]$TaskResult,
            [Parameter(Mandatory = $true)][string]$WorkItemState,
            [Parameter(Mandatory = $true)][int]$WorkItemExitCode
        )

        $emptyBuilds = [pscustomobject]@{ value = @() }
        $submissionLog = @(
            'Helix Job: submitted',
            "https://helix.dot.net/api/jobs/$($script:HelixJobId)/workitems"
        ) -join "`n"

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
                            finishTime   = (Get-Date).ToUniversalTime().ToString('o')
                            status       = 'completed'
                            result       = 'succeeded'
                            sourceBranch = 'refs/heads/net11.0'
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
                    Finished             = $true
                    InitialWorkItemCount = 1
                    WorkItems            = [pscustomobject]@{
                        Finished    = 1
                        Unscheduled = 0
                        Waiting     = 0
                        Running     = 0
                    }
                }
            }
            [pscustomobject]@{
                match = "jobs/$($script:HelixJobId)/workitems"
                json  = @(
                    [pscustomobject]@{
                        Name             = 'Controls.DeviceTests'
                        State            = $WorkItemState
                        ExitCode         = $WorkItemExitCode
                        ConsoleOutputUri = 'https://helix.blob.core.windows.net/console/Controls.DeviceTests.log'
                    }
                )
            }
            [pscustomobject]@{
                match = 'helix.blob.core.windows.net'
                text  = 'System.NullReferenceException in Microsoft.Maui.DeviceTests.ButtonTests'
            }
        )
    }
}

Describe 'ci-status-net11 trusted build-evidence collector' {
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
