#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        . "$PSScriptRoot/Recover-MissedReviewCommands.ps1"
    } finally {
        $ErrorActionPreference = $previousErrorActionPreference
    }

    function New-RecoveryTestComment {
        param(
            [Int64]$Id,
            [string]$Body,
            [string]$CreatedAt,
            [int]$PRNumber = 37148,
            [string]$Login = 'maintainer',
            [string]$NodeId = "IC_$Id"
        )

        [pscustomobject]@{
            id = $Id
            node_id = $NodeId
            body = $Body
            created_at = $CreatedAt
            issue_url = "https://api.github.com/repos/dotnet/maui/issues/$PRNumber"
            user = [pscustomobject]@{ login = $Login }
        }
    }
}

Describe 'Select-ReviewCommandCandidates' {
    It 'selects only aged normal review commands and preserves parsed options' {
        $now = [datetimeoffset]'2026-08-06T22:00:00Z'
        $comments = @(
            New-RecoveryTestComment -Id 1 -Body '/review -b improved-reviewer -p android' -CreatedAt '2026-08-06T21:43:40Z'
            New-RecoveryTestComment -Id 2 -Body '/review ios' -CreatedAt '2026-08-06T21:55:00Z'
            New-RecoveryTestComment -Id 3 -Body '/review tests' -CreatedAt '2026-08-06T21:30:00Z'
            New-RecoveryTestComment -Id 4 -Body '/review rerun' -CreatedAt '2026-08-06T21:20:00Z'
            New-RecoveryTestComment -Id 5 -Body 'please review' -CreatedAt '2026-08-06T21:10:00Z'
            New-RecoveryTestComment -Id 6 -Body '/review windows' -CreatedAt '2026-08-05T20:00:00Z'
        )

        $result = @(Select-ReviewCommandCandidates `
            -Comments $comments `
            -LookbackHours 24 `
            -MinimumAgeMinutes 10 `
            -Now $now)

        $result.Count | Should -Be 1
        $result[0].CommentId | Should -Be 1
        $result[0].PRNumber | Should -Be 37148
        $result[0].Platform | Should -Be 'android'
        $result[0].PipelineRef | Should -Be 'improved-reviewer'
    }
}

Describe 'Invoke-MissedReviewCommandRecovery' {
    BeforeEach {
        $script:Now = [datetimeoffset]'2026-08-06T22:00:00Z'
        $script:Comment = New-RecoveryTestComment `
            -Id 5209319531 `
            -Body '/review -b improved-reviewer -p android' `
            -CreatedAt '2026-08-06T21:43:40Z'

        Mock Get-RecentIssueComments { @($script:Comment) }
        Mock Test-ReviewCommentIsMinimized { $false }
        Mock Test-ReviewCommentHasRecoveryMarker { $false }
        Mock Get-ReviewRecoveryPullRequest {
            [pscustomobject]@{ number = 37148; state = 'open' }
        }
        Mock Test-ReviewOptionLoginTrusted { $true }
        Mock Invoke-ReviewWorkflowDispatch
        Mock Add-ReviewRecoveryMarker { $true }
        Mock Hide-ReviewCommandComment { $true }
    }

    It 'dispatches an authorized unprocessed command with its original options' {
        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 1
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 1 -Exactly -ParameterFilter {
            $PRNumber -eq 37148 -and
            $Platform -eq 'android' -and
            $PipelineRef -eq 'improved-reviewer'
        }
        Should -Invoke Add-ReviewRecoveryMarker -Times 1 -Exactly -ParameterFilter {
            $CommentId -eq 5209319531
        }
        Should -Invoke Hide-ReviewCommandComment -Times 1 -Exactly -ParameterFilter {
            $NodeId -eq 'IC_5209319531'
        }
    }

    It 'does not dispatch minimized commands' {
        Mock Test-ReviewCommentIsMinimized { $true }

        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 0
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 0 -Exactly
        Should -Invoke Test-ReviewCommentHasRecoveryMarker -Times 0 -Exactly
    }

    It 'does not dispatch commands already marked by the recovery bot' {
        Mock Test-ReviewCommentHasRecoveryMarker { $true }

        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 0
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 0 -Exactly
    }

    It 'does not dispatch commands from users without current write access' {
        Mock Test-ReviewOptionLoginTrusted { $false }

        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 0
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 0 -Exactly
    }

    It 'keeps manual dry runs read-only' {
        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now -DryRun

        $result.Recovered.Count | Should -Be 1
        $result.Recovered[0].DryRun | Should -BeTrue
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 0 -Exactly
        Should -Invoke Add-ReviewRecoveryMarker -Times 0 -Exactly
        Should -Invoke Hide-ReviewCommandComment -Times 0 -Exactly
    }

    It 'requires at least one durable acknowledgement after dispatch' {
        Mock Add-ReviewRecoveryMarker { $false }
        Mock Hide-ReviewCommandComment { $false }

        { Invoke-MissedReviewCommandRecovery -Now $script:Now } |
            Should -Throw '*could not persist a recovery acknowledgement*'
    }

    It 'accepts minimized state when the reaction marker write fails' {
        Mock Add-ReviewRecoveryMarker { $false }

        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 1
        Should -Invoke Hide-ReviewCommandComment -Times 1 -Exactly
    }
}

Describe 'review trigger recovery workflow safety' {
    BeforeAll {
        $workflowPath = Join-Path $PSScriptRoot '..' 'workflows' 'review-trigger-recovery.yml' |
            Resolve-Path |
            Select-Object -ExpandProperty Path
        $script:RecoveryWorkflow = Get-Content -Raw -LiteralPath $workflowPath
    }

    It 'runs only on a schedule and never on a pull request or manual branch' {
        $script:RecoveryWorkflow | Should -Match "(?m)^    - cron: '\*/10 \* \* \* \*'$"
        $script:RecoveryWorkflow | Should -Not -Match 'pull_request_target'
        $script:RecoveryWorkflow | Should -Not -Match 'workflow_dispatch'
    }

    It 'pins checkout to trusted main' {
        $script:RecoveryWorkflow | Should -Match '(?m)^          ref: main$'
        $script:RecoveryWorkflow | Should -Match '(?m)^          persist-credentials: false$'
    }

    It 'uses only the permissions needed to poll, dispatch, and mark comments' {
        $script:RecoveryWorkflow | Should -Match '(?m)^      actions: write$'
        $script:RecoveryWorkflow | Should -Match '(?m)^      contents: read$'
        $script:RecoveryWorkflow | Should -Match '(?m)^      issues: write$'
        $script:RecoveryWorkflow | Should -Match '(?m)^      pull-requests: read$'
        $script:RecoveryWorkflow | Should -Not -Match 'id-token: write'
    }

    It 'supports a repository kill switch and calls the deterministic script' {
        $script:RecoveryWorkflow | Should -Match "vars\.REVIEW_TRIGGER_RECOVERY_DISABLED != 'true'"
        $script:RecoveryWorkflow | Should -Match '\./\.github/scripts/Recover-MissedReviewCommands\.ps1'
        $script:RecoveryWorkflow | Should -Match '(?m)^            -LookbackHours 24'
        $script:RecoveryWorkflow | Should -Match '(?m)^            -MinimumAgeMinutes 10'
        $script:RecoveryWorkflow | Should -Match '(?m)^            -MaxRecoveries 5'
    }
}
