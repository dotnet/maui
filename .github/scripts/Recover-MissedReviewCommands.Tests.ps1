#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:RecoverScriptPath = Join-Path $PSScriptRoot 'Recover-MissedReviewCommands.ps1'
    $previousErrorActionPreference = $ErrorActionPreference
    try {
        . $script:RecoverScriptPath
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

Describe 'recovery script initialization' {
    It 'preserves custom repository parameters when importing shared command helpers' {
        $state = & {
            . $script:RecoverScriptPath -Owner 'example-owner' -Repo 'example-repo'
            [pscustomobject]@{
                Owner = $Owner
                Repo = $Repo
            }
        }

        $state.Owner | Should -Be 'example-owner'
        $state.Repo | Should -Be 'example-repo'
    }
}

Describe 'Select-ReviewCommandCandidates' {
    It 'selects only aged normal review commands and preserves parsed options' {
        $now = [datetimeoffset]'2026-08-06T22:00:00Z'
        $comments = @(
            New-RecoveryTestComment -Id 1 -Body '/review -b improved-reviewer -p android' -CreatedAt '2026-08-06T21:30:00Z'
            New-RecoveryTestComment -Id 2 -Body '/review ios' -CreatedAt '2026-08-06T21:55:00Z'
            New-RecoveryTestComment -Id 3 -Body '/review tests' -CreatedAt '2026-08-06T21:30:00Z'
            New-RecoveryTestComment -Id 4 -Body '/review rerun' -CreatedAt '2026-08-06T21:20:00Z'
            New-RecoveryTestComment -Id 5 -Body 'please review' -CreatedAt '2026-08-06T21:10:00Z'
            New-RecoveryTestComment -Id 6 -Body '/review windows' -CreatedAt '2026-08-05T20:00:00Z'
        )

        $result = @(Select-ReviewCommandCandidates `
            -Comments $comments `
            -LookbackHours 24 `
            -MinimumAgeMinutes 25 `
            -Now $now)

        $result.Count | Should -Be 1
        $result[0].CommentId | Should -Be 1
        $result[0].PRNumber | Should -Be 37148
        $result[0].Platform | Should -Be 'android'
        $result[0].PipelineRef | Should -Be 'improved-reviewer'
    }

    It 'selects the exact PR 36572 command whose issue_comment webhook was missed' {
        $now = [datetimeoffset]'2026-08-06T22:30:00Z'
        $comment = New-RecoveryTestComment `
            -Id 5209393546 `
            -PRNumber 36572 `
            -Body '/review -b improved-reviewer -p android' `
            -CreatedAt '2026-08-06T21:57:59Z'

        $result = @(Select-ReviewCommandCandidates `
            -Comments @($comment) `
            -LookbackHours 24 `
            -MinimumAgeMinutes 25 `
            -Now $now)

        $result.Count | Should -Be 1
        $result[0].CommentId | Should -Be 5209393546
        $result[0].PRNumber | Should -Be 36572
        $result[0].Platform | Should -Be 'android'
        $result[0].PipelineRef | Should -Be 'improved-reviewer'
    }

    It 'does not replay comments created before the recovery workflow was deployed' {
        $now = [datetimeoffset]'2026-08-06T22:30:00Z'
        $comment = New-RecoveryTestComment `
            -Id 5209393546 `
            -PRNumber 36572 `
            -Body '/review -b improved-reviewer -p android' `
            -CreatedAt '2026-08-06T21:57:59Z'

        $result = @(Select-ReviewCommandCandidates `
            -Comments @($comment) `
            -LookbackHours 24 `
            -MinimumAgeMinutes 25 `
            -NotBefore ([datetimeoffset]'2026-08-06T22:00:00Z') `
            -Now $now)

        $result.Count | Should -Be 0
    }
}

Describe 'Get-ReviewRecoveryDeploymentEpoch' {
    It 'uses the first scheduled run as the rollout lower bound' {
        Mock Invoke-ReviewRecoveryGhApi {
            param([string[]]$Arguments)

            if ($Arguments[0] -match '/commits/first-run$') {
                return [pscustomobject]@{
                    commit = [pscustomobject]@{
                        committer = [pscustomobject]@{ date = '2026-08-06T21:58:00Z' }
                    }
                }
            }

            [pscustomobject]@{
                total_count = 2
                workflow_runs = @(
                    [pscustomobject]@{
                        created_at = '2026-08-06T22:10:00Z'
                        head_sha = 'second-run'
                    }
                    [pscustomobject]@{
                        created_at = '2026-08-06T22:00:00Z'
                        head_sha = 'first-run'
                    }
                )
            }
        }

        $result = Get-ReviewRecoveryDeploymentEpoch `
            -Owner 'dotnet' `
            -Repo 'maui' `
            -LookbackHours 24 `
            -Now ([datetimeoffset]'2026-08-06T22:30:00Z')

        $result | Should -Be ([datetimeoffset]'2026-08-06T21:58:00Z')
        Should -Invoke Invoke-ReviewRecoveryGhApi -Times 2 -Exactly
        Should -Invoke Invoke-ReviewRecoveryGhApi -Times 1 -Exactly -ParameterFilter {
            $Arguments[0] -match '/commits/first-run$'
        }
    }

    It 'reads the oldest page while deployment is inside the lookback' {
        Mock Invoke-ReviewRecoveryGhApi {
            param([string[]]$Arguments)

            if ($Arguments[0] -match 'page=1$') {
                return [pscustomobject]@{
                    total_count = 201
                    workflow_runs = @(
                        [pscustomobject]@{ created_at = '2026-08-08T10:00:00Z' }
                    )
                }
            }
            if ($Arguments[0] -match '/commits/oldest-run$') {
                return [pscustomobject]@{
                    commit = [pscustomobject]@{
                        committer = [pscustomobject]@{ date = '2026-08-06T23:58:00Z' }
                    }
                }
            }

            return [pscustomobject]@{
                total_count = 201
                workflow_runs = @(
                    [pscustomobject]@{
                        created_at = '2026-08-07T00:00:00Z'
                        head_sha = 'oldest-run'
                    }
                )
            }
        }

        $result = Get-ReviewRecoveryDeploymentEpoch `
            -Owner 'dotnet' `
            -Repo 'maui' `
            -LookbackHours 24 `
            -Now ([datetimeoffset]'2026-08-08T10:30:00Z')

        $result | Should -Be ([datetimeoffset]'2026-08-06T23:58:00Z')
        Should -Invoke Invoke-ReviewRecoveryGhApi -Times 3 -Exactly
        Should -Invoke Invoke-ReviewRecoveryGhApi -Times 1 -Exactly -ParameterFilter {
            $Arguments[0] -match 'page=3$'
        }
    }

    It 'drops the rollout lower bound after more than one lookback of possible runs' {
        Mock Invoke-ReviewRecoveryGhApi {
            [pscustomobject]@{
                total_count = 289
                workflow_runs = @()
            }
        }

        $result = Get-ReviewRecoveryDeploymentEpoch `
            -Owner 'dotnet' `
            -Repo 'maui' `
            -LookbackHours 24

        $result | Should -Be ([datetimeoffset]::MinValue)
        Should -Invoke Invoke-ReviewRecoveryGhApi -Times 1 -Exactly
    }
}

Describe 'Test-ReviewCommentHasRecoveryMarker' {
    It 'finds a recovery marker after the first page of reactions' {
        $firstPage = @(
            1..100 | ForEach-Object {
                [pscustomobject]@{
                    content = 'heart'
                    user = [pscustomobject]@{ login = "user-$_" }
                }
            }
        )
        $marker = [pscustomobject]@{
            content = 'rocket'
            user = [pscustomobject]@{ login = 'github-actions[bot]' }
        }

        Mock Invoke-ReviewRecoveryGhApi {
            param([string[]]$Arguments)

            if ($Arguments[0] -match 'page=1$') {
                return $firstPage
            }

            return @($marker)
        }

        Test-ReviewCommentHasRecoveryMarker `
            -Owner 'dotnet' `
            -Repo 'maui' `
            -CommentId 12345 |
            Should -BeTrue

        Should -Invoke Invoke-ReviewRecoveryGhApi -Times 2 -Exactly
    }
}

Describe 'Invoke-MissedReviewCommandRecovery' {
    BeforeEach {
        $script:Now = [datetimeoffset]'2026-08-06T22:00:00Z'
        $script:Comment = New-RecoveryTestComment `
            -Id 5209319531 `
            -Body '/review -b improved-reviewer -p android' `
            -CreatedAt '2026-08-06T21:30:00Z'

        Mock Get-RecentIssueComments { @($script:Comment) }
        Mock Test-ReviewCommentIsMinimized { $false }
        Mock Test-ReviewCommentHasRecoveryMarker { $false }
        Mock Get-ReviewRecoveryPullRequest {
            [pscustomobject]@{ number = 37148; state = 'open' }
        }
        Mock Test-ReviewOptionLoginTrusted { $true }
        Mock Invoke-ReviewWorkflowDispatch
    }

    It 'dispatches an authorized unprocessed command with its original options' {
        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 1
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 1 -Exactly -ParameterFilter {
            $PRNumber -eq 37148 -and
            $Platform -eq 'android' -and
            $PipelineRef -eq 'improved-reviewer' -and
            $CommentId -eq 5209319531 -and
            $CommentNodeId -eq 'IC_5209319531'
        }
        $result.Recovered[0].AcknowledgementPending | Should -BeTrue
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
        $result.Recovered[0].AcknowledgementPending | Should -BeFalse
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 0 -Exactly
    }

    It 'passes the deployment epoch through candidate selection' {
        $result = Invoke-MissedReviewCommandRecovery `
            -Now $script:Now `
            -NotBefore ([datetimeoffset]'2026-08-06T21:45:00Z')

        $result.Recovered.Count | Should -Be 0
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 0 -Exactly
    }

    It 'dispatches the full batch while the serialized workflow owns acknowledgement' {
        $secondComment = New-RecoveryTestComment `
            -Id 5209319532 `
            -Body '/review ios' `
            -CreatedAt '2026-08-06T21:29:00Z'
        Mock Get-RecentIssueComments { @($script:Comment, $secondComment) }

        $result = Invoke-MissedReviewCommandRecovery -Now $script:Now

        $result.Recovered.Count | Should -Be 2
        @($result.Recovered | Where-Object AcknowledgementPending).Count | Should -Be 2
        Should -Invoke Invoke-ReviewWorkflowDispatch -Times 2 -Exactly
    }

    It 'does not acknowledge in the scanner before the serialized trigger workflow runs' {
        $scriptText = Get-Content -Raw -LiteralPath $script:RecoverScriptPath
        $scriptText | Should -Not -Match 'function Add-ReviewRecoveryMarker'
        $scriptText | Should -Not -Match 'function Hide-ReviewCommandComment'
        $scriptText | Should -Match 'serialized review-trigger workflow will acknowledge'
    }
}

Describe 'Invoke-ReviewWorkflowDispatch' {
    It 'carries the source comment identity into workflow_dispatch' {
        $script:DispatchPayload = $null
        Mock Invoke-ReviewRecoveryGhApi {
            param([string[]]$Arguments)
            $inputIndex = [Array]::IndexOf($Arguments, '--input')
            $script:DispatchPayload = Get-Content -Raw -LiteralPath $Arguments[$inputIndex + 1] |
                ConvertFrom-Json
        }

        Invoke-ReviewWorkflowDispatch `
            -Owner 'dotnet' `
            -Repo 'maui' `
            -PRNumber 37148 `
            -Platform 'android' `
            -PipelineRef 'improved-reviewer' `
            -CommentId 5209319531 `
            -CommentNodeId 'IC_5209319531'

        $script:DispatchPayload.inputs.source_comment_id | Should -Be '5209319531'
        $script:DispatchPayload.inputs.source_comment_node_id | Should -Be 'IC_5209319531'
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

    It 'uses only the permissions needed to poll and dispatch' {
        $script:RecoveryWorkflow | Should -Match '(?m)^      actions: write$'
        $script:RecoveryWorkflow | Should -Match '(?m)^      contents: read$'
        $script:RecoveryWorkflow | Should -Match '(?m)^      issues: read$'
        $script:RecoveryWorkflow | Should -Not -Match '(?m)^      issues: write$'
        $script:RecoveryWorkflow | Should -Match '(?m)^      pull-requests: read$'
        $script:RecoveryWorkflow | Should -Not -Match 'id-token: write'
    }

    It 'supports a repository kill switch and calls the deterministic script' {
        $script:RecoveryWorkflow | Should -Match "vars\.REVIEW_TRIGGER_RECOVERY_DISABLED != 'true'"
        $script:RecoveryWorkflow | Should -Match '\./\.github/scripts/Recover-MissedReviewCommands\.ps1'
        $script:RecoveryWorkflow | Should -Match '(?m)^            -LookbackHours 24'
        $script:RecoveryWorkflow | Should -Match '(?m)^            -MinimumAgeMinutes 25'
        $script:RecoveryWorkflow | Should -Match '(?m)^            -MaxRecoveries 5'
    }
}
