#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Query-AutoRerunCandidates.ps1'
    $workflowPath = Join-Path $PSScriptRoot '../workflows/pr-review-queue.yml'
    $reviewableQueryTestsPath = Join-Path $PSScriptRoot 'Query-ReviewablePRs.Tests.ps1'
    $scannerPath = Join-Path $PSScriptRoot '../workflows/rerun-review-scanner.md'
    $reviewTriggerPath = Join-Path $PSScriptRoot '../workflows/review-trigger.yml'
    $labelHelperPath = Join-Path $PSScriptRoot 'shared/Update-AgentLabels.ps1'
    $outputDir = Join-Path $PSScriptRoot '../../CustomAgentLogsTmp/QueryAutoRerunTests'
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

    . $scriptPath -Owner 'test-owner' -Repo 'test-repo'

    function New-TestPR {
        param(
            [int]$Number,
            [bool]$IsDraft = $false,
            [string[]]$Labels = @(),
            [string]$HeadSha = '2222222abcdef'
        )

        [pscustomobject]@{
            number     = $Number
            title      = "PR $Number"
            url        = "https://example.test/$Number"
            headRefOid = $HeadSha
            isDraft    = $IsDraft
            labels     = @($Labels | ForEach-Object { [pscustomobject]@{ name = $_ } })
            author     = [pscustomobject]@{ login = 'dev-user' }
        }
    }

    function ConvertTo-GhLines {
        param([object[]]$Items)
        return @($Items | ForEach-Object { $_ | ConvertTo-Json -Depth 10 -Compress })
    }

    function New-DeclineMarkerComment {
        param(
            [int64]$Id,
            [string]$CreatedAt,
            [string]$HeadSha,
            [Int64]$ActivityCheckpoint = 0,
            [string]$Login = 'github-actions[bot]'
        )

        $checkpointSuffix = if ($ActivityCheckpoint -gt 0) { ":$ActivityCheckpoint" } else { '' }
        [pscustomobject]@{
            id = $Id
            body = "<!-- agent-rerun-declined:$HeadSha$checkpointSuffix -->"
            created_at = $CreatedAt
            updated_at = $CreatedAt
            user = [pscustomobject]@{ login = $Login; type = 'Bot' }
            author_association = 'NONE'
        }
    }

    function Invoke-TestScan {
        Invoke-AutoRerunCandidateScan `
            -ScanOwner 'test-owner' `
            -ScanRepo 'test-repo' `
            -ScanLimit $script:Limit `
            -ScanDryRun:$script:DryRun `
            -ScanOutputPath $script:OutputPath
    }
}

AfterAll {
    Remove-Item -LiteralPath $outputDir -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Query-AutoRerunCandidates' {
    BeforeEach {
        $script:Owner = 'test-owner'
        $script:Repo = 'test-repo'
        $script:Limit = 5
        $script:DryRun = $true
        $script:OutputPath = Join-Path $outputDir "$([Guid]::NewGuid().ToString('N')).json"
        $script:prList = @()
        $script:issueComments = @()
        $script:reviews = @()
        $script:reviewComments = @()
        $script:commits = @()
        $script:failedPRs = @()
        $script:prListFailure = $false
        $script:ghCalls = @()

        Mock gh {
            param(
                [Parameter(ValueFromRemainingArguments = $true)]
                [string[]]$GhArgs
            )

            $command = $GhArgs -join ' '
            $script:ghCalls += $command
            $global:LASTEXITCODE = 0

            if ($command -match '^pr list ') {
                if ($script:prListFailure) {
                    $global:LASTEXITCODE = 1
                    return @()
                }
                return ($script:prList | ConvertTo-Json -Depth 10 -Compress)
            }

            $numberMatch = [regex]::Match($command, '/(?:issues|pulls)/(\d+)/')
            $number = if ($numberMatch.Success) { [int]$numberMatch.Groups[1].Value } else { 0 }
            if ($script:failedPRs -contains $number) {
                $global:LASTEXITCODE = 1
                return @()
            }

            if ($command -match '/issues/\d+/comments') { return ConvertTo-GhLines $script:issueComments }
            if ($command -match '/pulls/\d+/reviews') { return ConvertTo-GhLines $script:reviews }
            if ($command -match '/pulls/\d+/comments') { return ConvertTo-GhLines $script:reviewComments }
            if ($command -match '/pulls/\d+/commits') { return ConvertTo-GhLines $script:commits }
            throw "Unexpected gh call: $command"
        }
    }

Context 'bounded scan metadata' {
    It 'processes only Limit items and reports exact truncation using one sentinel item' {
        $script:prList = @(1..6 | ForEach-Object { New-TestPR -Number $_ -IsDraft $true })

        $messages = Invoke-TestScan 6>&1 | Out-String

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $messages | Should -Match '::warning::Open PR scan truncated'
        $summary.scan.limit | Should -Be 5
        $summary.scan.fetchedCount | Should -Be 5
        $summary.scan.observedCount | Should -Be 6
        $summary.scan.truncated | Should -BeTrue
        $summary.decisions.Count | Should -Be 5
        @($script:ghCalls | Where-Object { $_ -match '^pr list .*--limit 6' }).Count | Should -Be 1
    }

    It 'keeps pull-request validation tokenless and test-only' {
        $workflow = Get-Content -Raw -LiteralPath $workflowPath
        $validateJob = [regex]::Match($workflow, '(?s)^  validate:.*\z', 'Multiline').Value

        $validateJob | Should -Match 'Validate queue scripts without GitHub credentials'
        $validateJob | Should -Match 'Query-ReviewablePRs\.Tests\.ps1'
        $validateJob | Should -Match 'Query-AutoRerunCandidates\.Tests\.ps1'
        $validateJob | Should -Match 'Update-AgentLabels\.Tests\.ps1'
        $validateJob | Should -Not -Match 'GH_TOKEN|github\.token'
        $validateJob | Should -Not -Match 'pull-requests:\s+read|issues:\s+read'
        Test-Path -LiteralPath $reviewableQueryTestsPath | Should -BeTrue
    }

    It 'keeps the scheduled queue job at pull-request read permission' {
        $workflow = Get-Content -Raw -LiteralPath $workflowPath
        $generateJob = [regex]::Match($workflow, '(?s)  generate-report:.*?^  validate:', 'Multiline').Value
        $generateJob | Should -Match 'pull-requests:\s+read'
        $generateJob | Should -Not -Match 'pull-requests:\s+write'
    }

    It 'passes systemic and label-application failures through the workflow boundary' {
        $workflow = Get-Content -Raw -LiteralPath $workflowPath
        $autoLabelStep = [regex]::Match(
            $workflow,
            '(?s)      - name: Auto-label rerun-ready PRs.*?(?=      - name: Upload auto-rerun decisions)').Value

        $autoLabelStep | Should -Match 'pwsh \.github/scripts/Query-AutoRerunCandidates\.ps1'
        $autoLabelStep | Should -Not -Match 'set \+e|rc=\$\?|Auto-rerun labeling step failed'
    }
}

Context 'API request bounding' {
    It 'checks PR reviews before classifying a PR without an AI Summary' {
        $script:prList = @(New-TestPR -Number 10)

        Invoke-TestScan

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.decisions[0].reason | Should -Be 'no-ai-summary'
        @($script:ghCalls | Where-Object { $_ -match '/issues/10/comments' }).Count | Should -Be 1
        @($script:ghCalls | Where-Object { $_ -match '/pulls/10/reviews' }).Count | Should -Be 1
        @($script:ghCalls | Where-Object { $_ -match '/pulls/10/comments' }).Count | Should -Be 1
        @($script:ghCalls | Where-Object { $_ -match '/pulls/10/commits' }).Count | Should -Be 0
    }

    It 'finds the producer AI Summary in a mocked PR review and fetches commits' {
        $script:prList = @(New-TestPR -Number 11 -HeadSha '2222222abcdef')
        $script:reviews = @(
            [pscustomobject]@{
                id = 100
                body = "<!-- AI Summary -->`n<!-- SESSION:1111111 START -->"
                submitted_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            }
        )

        Invoke-TestScan

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.decisions[0].eligible | Should -BeTrue
        $summary.decisions[0].reason | Should -Be 'new-head-commit'
        @($script:ghCalls | Where-Object { $_ -match '/pulls/11/reviews' }).Count | Should -Be 1
        @($script:ghCalls | Where-Object { $_ -match '/pulls/11/commits' }).Count | Should -Be 1
    }
}

Context 'error aggregation' {
    It 'fails when the open PR query fails' {
        $script:prListFailure = $true

        { Invoke-TestScan } | Should -Throw '*Failed to list open PRs*'
    }

    It 'fails a dry-run after writing structured error details' {
        $script:prList = @(1..2 | ForEach-Object { New-TestPR -Number $_ })
        $script:failedPRs = @(1, 2)

        { Invoke-TestScan } | Should -Throw '*2 of 2 evaluated PR(s) had errors*'

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.errors | Should -Be 2
        $summary.evaluated | Should -Be 2
        $summary.systemicFailure | Should -BeTrue
        @($summary.decisions | Where-Object reason -like 'error:*').Count | Should -Be 2
    }

    It 'keeps an isolated scheduled error non-fatal but visible' {
        $script:DryRun = $false
        $script:prList = @(1..4 | ForEach-Object { New-TestPR -Number $_ })
        $script:failedPRs = @(1)

        { Invoke-TestScan } | Should -Not -Throw

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.errors | Should -Be 1
        $summary.systemicFailure | Should -BeFalse
    }

    It 'fails a scheduled scan when evaluation failures are systemic' {
        $script:DryRun = $false
        $script:prList = @(1..3 | ForEach-Object { New-TestPR -Number $_ })
        $script:failedPRs = @(1, 2, 3)

        { Invoke-TestScan } | Should -Throw '*3 of 3 evaluated PR(s) had errors*'

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.systemicFailure | Should -BeTrue
    }

    It 'fails after writing the summary when an eligible label cannot be applied or verified' {
        $script:DryRun = $false
        $script:prList = @(New-TestPR -Number 1)
        $script:issueComments = @(
            [pscustomobject]@{
                id = 1
                body = "<!-- AI Summary -->`n<!-- SESSION:1111111 START -->"
                created_at = '2026-05-31T09:00:00Z'
                updated_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            }
        )
        Mock Ensure-LabelExists {}
        Mock Add-Label { $false }
        Mock Get-IssueLabels { @() }

        { Invoke-TestScan } | Should -Throw '*1 label application failure(s)*'

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.applyFailures | Should -Be 1
        $summary.applied | Should -Be 0
        $summary.decisions[0].eligible | Should -BeTrue
        $summary.decisions[0].applied | Should -BeFalse
    }
}

Context 'scanner decline checkpoint' {
    It 'does not treat generic ready-label removals as scanner declines' {
        $script:prList = @(New-TestPR -Number 6)
        $script:issueComments = @(
            [pscustomobject]@{
                id = 1
                body = "<!-- AI Summary -->`n<!-- SESSION:1111111 START -->"
                created_at = '2026-05-31T09:00:00Z'
                updated_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            }
        )

        Invoke-TestScan

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.decisions[0].eligible | Should -BeTrue
        $summary.decisions[0].reason | Should -Be 'new-head-commit'
    }

    It 'sorts explicit scanner markers newest-first and re-evaluates eligibility against that checkpoint' {
        $script:prList = @(
            New-TestPR -Number 7 -Labels @('s/agent-rerun-declined') -HeadSha '2222222222222222222222222222222222222222'
        )
        $script:issueComments = @(
            [pscustomobject]@{
                id = 1
                body = "<!-- AI Summary -->`n<!-- SESSION:1111111 START -->"
                created_at = '2026-05-31T09:00:00Z'
                updated_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            },
            [pscustomobject]@{
                id = 2
                body = 'I pushed the update.'
                created_at = '2026-05-31T09:45:00Z'
                updated_at = '2026-05-31T09:45:00Z'
                user = [pscustomobject]@{ login = 'dev-user'; type = 'User' }
                author_association = 'CONTRIBUTOR'
            },
            (New-DeclineMarkerComment -Id 3 -CreatedAt '2026-05-31T08:00:00Z' -HeadSha 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'),
            (New-DeclineMarkerComment -Id 4 -CreatedAt '2026-05-31T10:00:00Z' -HeadSha '2222222222222222222222222222222222222222'),
            (New-DeclineMarkerComment -Id 5 -CreatedAt '2026-05-31T09:30:00Z' -HeadSha 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb')
        )

        Invoke-TestScan

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.decisions[0].eligible | Should -BeFalse
        $summary.decisions[0].reason | Should -Be 'declined-state-unchanged'
    }

    It 're-qualifies a head that changed while the scanner was persisting its decline marker' {
        $script:prList = @(
            New-TestPR -Number 8 -Labels @('s/agent-rerun-declined') -HeadSha '3333333333333333333333333333333333333333'
        )
        $script:issueComments = @(
            [pscustomobject]@{
                id = 1
                body = "<!-- AI Summary -->`n<!-- SESSION:1111111 START -->"
                created_at = '2026-05-31T09:00:00Z'
                updated_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            },
            (New-DeclineMarkerComment -Id 2 -CreatedAt '2026-05-31T10:00:00Z' -HeadSha '2222222222222222222222222222222222222222')
        )

        Invoke-TestScan

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.decisions[0].eligible | Should -BeTrue
        $summary.decisions[0].reason | Should -Be 'new-head-commit'
    }

    It 'keeps a comment-only activity race newer than the scan-time decline checkpoint' {
        $script:prList = @(
            New-TestPR -Number 9 -Labels @('s/agent-rerun-declined') -HeadSha '2222222222222222222222222222222222222222'
        )
        $script:reviews = @(
            [pscustomobject]@{
                id = 1
                body = "<!-- AI Summary -->`n<!-- SESSION:2222222 START -->"
                submitted_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            }
        )
        $script:issueComments = @(
            [pscustomobject]@{
                id = 2
                body = 'Follow-up posted while the scanner was deciding.'
                created_at = '2026-05-31T09:45:00Z'
                updated_at = '2026-05-31T09:45:00Z'
                user = [pscustomobject]@{ login = 'dev-user'; type = 'User' }
                author_association = 'CONTRIBUTOR'
            },
            (New-DeclineMarkerComment `
                -Id 3 `
                -CreatedAt '2026-05-31T10:00:00Z' `
                -HeadSha '2222222222222222222222222222222222222222' `
                -ActivityCheckpoint 1780219800000)
        )

        Invoke-TestScan

        $summary = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $summary.decisions[0].eligible | Should -BeTrue
        $summary.decisions[0].reason | Should -Be 'new-author-comment-after-ai-summary'
    }

    It 'persists skip markers before consuming ready labels and clears them before trigger dispatch' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath
        $scanner.IndexOf('const declineRecorded = await markDeclined(prNumber, liveHeadSha, a.activityCheckpoint, a.activityKey);') |
            Should -BeLessThan $scanner.IndexOf('if (!declineRecorded) { continue; }')
        $scanner.IndexOf('if (!declineRecorded) { continue; }') |
            Should -BeLessThan $scanner.IndexOf("await react(a.rerunCommentId, '-1');")
        $scanner.IndexOf("await react(a.rerunCommentId, '-1');") |
            Should -BeLessThan $scanner.IndexOf('await removeReadyLabel(prNumber);')
        $scanner.IndexOf('await clearDeclined(prNumber);') |
            Should -BeLessThan $scanner.IndexOf('await github.rest.actions.createWorkflowDispatch({')
    }

    It 'makes decline marking idempotent across partial failures' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath
        $markDeclined = [regex]::Match(
            $scanner,
            '(?s)async function markDeclined\(prNumber, headSha, activityCheckpoint, activityKey\).*?async function clearDeclined'
        ).Value

        $markDeclined | Should -Match 'const cycleMarker = `<!-- agent-rerun-declined-cycle:\$\{headSha\}:\$\{activityKey\} -->`;'
        $markDeclined | Should -Match 'const markerText = `<!-- agent-rerun-declined:\$\{headSha\}:\$\{activityCheckpoint\} -->\\n\$\{cycleMarker\}`;'
        $markDeclined | Should -Match '\.includes\(cycleMarker\)'
        $markDeclined | Should -Match 'nodes\{id body author\{login\}\}'
        $markDeclined | Should -Match "comment\.author\?\.login === 'github-actions\[bot\]'"
        $markDeclined | Should -Match 'comments\(last:100,before:\$before\)'
        $markDeclined | Should -Match 'pageInfo\{hasPreviousPage startCursor\}'
        $markDeclined | Should -Match 'const maxMarkerPages = 5;'
        $markDeclined | Should -Match 'markerPage < maxMarkerPages'
        $markDeclined | Should -Match 'a duplicate checkpoint is safer than an unbounded history walk'

        $headSha = '2222222222222222222222222222222222222222'
        $priorActivityKey = 'a' * 64
        $newActivityKey = 'b' * 64
        $priorCycleMarker = "<!-- agent-rerun-declined-cycle:${headSha}:${priorActivityKey} -->"
        $retryCycleMarker = "<!-- agent-rerun-declined-cycle:${headSha}:${priorActivityKey} -->"
        $newActivityCycleMarker = "<!-- agent-rerun-declined-cycle:${headSha}:${newActivityKey} -->"

        $retryCycleMarker | Should -Be $priorCycleMarker
        $newActivityCycleMarker | Should -Not -Be $priorCycleMarker
        $markDeclined.IndexOf('if (alreadyLabelled && existingMarker)') |
            Should -BeLessThan $markDeclined.IndexOf('issues.createComment({')
        $markDeclined.IndexOf('if (existingMarker)') |
            Should -BeLessThan $markDeclined.IndexOf('issues.createComment({')
        $markDeclined | Should -Match 'rest\.issues\.addLabels'
    }

    It 'revalidates queue and in-progress labels immediately before decline side effects' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath
        $markDeclined = [regex]::Match(
            $scanner,
            '(?s)async function markDeclined\(prNumber, headSha, activityCheckpoint, activityKey\).*?async function clearDeclined'
        ).Value

        ([regex]::Matches($markDeclined, 'await getCurrentLabelNames\(prNumber\)')).Count |
            Should -BeGreaterOrEqual 2
        $finalLabelRead = $markDeclined.LastIndexOf('await getCurrentLabelNames(prNumber)')
        $markerLookup = $markDeclined.IndexOf('const maxMarkerPages = 5;')
        $createMarker = $markDeclined.IndexOf('issues.createComment({')
        $applyDeclined = $markDeclined.IndexOf('rest.issues.addLabels')

        $finalLabelRead | Should -BeGreaterThan $markerLookup
        $finalLabelRead | Should -BeLessThan $createMarker
        $markDeclined | Should -Match '!currentLabels\.has\(readyLabel\)'
        $markDeclined | Should -Match "currentLabels\.has\('s/agent-review-in-progress'\)"
        $applyDeclined | Should -BeGreaterThan $finalLabelRead
        $scanner | Should -Match 'if \(!declineRecorded\) \{ continue; \}'
    }

    It 'keeps PR-authored narrative out of the agent decision prompt' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath

        $scanner | Should -Match 'fixed-schema candidates'
        $scanner | Should -Match 'PR-authored titles,\s+comments, command bodies, commit messages'
        $scanner | Should -Match 'must not be fetched or inspected'
        $scanner | Should -Not -Match 'safe and useful enough'
    }

    It 'recovers when concurrent decline-label creation returns 422' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath
        $ensureDeclinedLabel = [regex]::Match(
            $scanner,
            '(?s)async function ensureDeclinedLabel\(\).*?async function markDeclined'
        ).Value

        $ensureDeclinedLabel | Should -Match 'if \(createError\.status !== 422\) \{ throw createError; \}'
        ([regex]::Matches($ensureDeclinedLabel, 'rest\.issues\.getLabel')).Count | Should -Be 2
        $ensureDeclinedLabel.LastIndexOf('rest.issues.getLabel') |
            Should -BeGreaterThan $ensureDeclinedLabel.IndexOf('rest.issues.createLabel')
        $ensureDeclinedLabel.LastIndexOf('await syncDeclinedLabel(existing);') |
            Should -BeGreaterThan $ensureDeclinedLabel.LastIndexOf('rest.issues.getLabel')
    }

    It 'keeps advisory decline cleanup best-effort before dispatch' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath
        $clearDeclined = [regex]::Match(
            $scanner,
            '(?s)async function clearDeclined\(prNumber\).*?\n\s+for \(const a of actions\)'
        ).Value

        $clearDeclined | Should -Match 'core\.warning'
        $clearDeclined | Should -Not -Match 'throw new Error'
        $scanner.IndexOf('await clearDeclined(prNumber);') |
            Should -BeLessThan $scanner.IndexOf('await github.rest.actions.createWorkflowDispatch({')
    }

    It 'clears the decline label in the shared review entrypoint before acquiring the review lock' {
        $reviewTrigger = Get-Content -Raw -LiteralPath $reviewTriggerPath
        $reviewTrigger.IndexOf('$declineCleared = Clear-AgentRerunDeclined') |
            Should -BeLessThan $reviewTrigger.IndexOf('$locked = Set-AgentReviewInProgress')
        $reviewTrigger | Should -Match '::warning::Could not clear s/agent-rerun-declined'
        $reviewTrigger | Should -Not -Match 'throw "Failed to clear s/agent-rerun-declined'
    }

    It 'keeps scanner decline-label metadata aligned with the shared definition' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath
        $labelHelper = Get-Content -Raw -LiteralPath $labelHelperPath

        $scanner | Should -Match ([regex]::Escape('agent-rerun-declined:'))
        foreach ($value in @(
            's/agent-rerun-declined',
            'AI rerun scanner declined the current PR state; new author activity is required',
            'D4C5F9'
        )) {
            $scanner | Should -Match ([regex]::Escape($value))
            $labelHelper | Should -Match ([regex]::Escape($value))
        }
    }

    It 'documents the autonomous queue as the rerun-label source' {
        $scanner = Get-Content -Raw -LiteralPath $scannerPath

        $scanner | Should -Match 'daily PR Review Queue workflow applies this label autonomously'
        $scanner | Should -Match 'The `/review rerun` comment command is intentionally unsupported'
        ([regex]::Matches($scanner, '/review rerun')).Count | Should -Be 1
    }
}
}
