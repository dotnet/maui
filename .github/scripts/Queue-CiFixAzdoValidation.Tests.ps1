#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Queue-CiFixAzdoValidation.ps1'
    $script:MaxHttpAttempts = 4
    $script:RetryBaseDelaySeconds = 2
    $script:DispatcherBudgetPrefix = '[dispatcher-budget-exhausted]'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object Message) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Get-ObjectPropertyValue',
            'Get-DispatcherElapsedSeconds',
            'Get-DispatcherRemainingSeconds',
            'Test-IsDispatcherBudgetException',
            'Get-DispatcherHttpTimeoutSeconds',
            'Invoke-DispatcherSleep',
            'Test-CiFixPrFingerprint',
            'Get-CiFixPipelineDefinitions',
            'Get-CiFixContextFromPullRequest',
            'Get-GitHubApiHeaders',
            'Get-FixturePullRequestDetail',
            'Get-FixtureCommit',
            'Get-CiFixMergePairDiagnostic',
            'Resolve-VerifiedCiFixContext',
            'Get-CiFixEventContext',
            'Test-TrustedCiFixWorkflowRun',
            'Get-OpenCiFixContexts',
            'Test-IsTransientHttpException',
            'Get-HttpStatusCode',
            'Invoke-WithHttpRetry',
            'Get-AzdoToken',
            'Find-AzdoDuplicateBuild',
            'Get-AzdoDuplicateBuild',
            'New-AzdoQueueRequest',
            'Invoke-AzdoPipelineQueue',
            'Write-CiFixJobSummary',
            'New-CiFixFailureResult',
            'Invoke-CiFixQueueWork')) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) {
            throw "Function '$functionName' not found"
        }
        Invoke-Expression $function.Extent.Text
    }

    function New-TestPullRequest {
        param(
            [int]$Number = 123,
            [string]$Repository = 'dotnet/maui',
            [string]$HeadRepository = 'dotnet/maui',
            [string]$Author = 'github-actions[bot]',
            [string]$HeadRef = 'ci-fix/issue-123',
            [string]$BaseRef = 'main',
            [string]$Title = '[ci-fix] Repair CI (refs #123)',
            [string[]]$Labels = @('agentic-workflows'),
            [string]$State = 'open',
            [string]$HeadSha = '1111111111111111111111111111111111111111',
            [string]$MergeSha = '2222222222222222222222222222222222222222'
        )

        return [pscustomobject]@{
            number = $Number
            id = 456789 + $Number
            state = $State
            draft = $true
            title = $Title
            merge_commit_sha = $MergeSha
            user = [pscustomobject]@{ login = $Author }
            labels = @($Labels | ForEach-Object { [pscustomobject]@{ name = $_ } })
            base = [pscustomobject]@{
                ref = $BaseRef
                repo = [pscustomobject]@{ full_name = $Repository }
            }
            head = [pscustomobject]@{
                ref = $HeadRef
                sha = $HeadSha
                repo = [pscustomobject]@{ full_name = $HeadRepository }
            }
        }
    }

    function New-TestEvent {
        param(
            [string]$Action = 'synchronize',
            [string]$Repository = 'dotnet/maui',
            [string]$HeadRepository = 'dotnet/maui',
            [string]$Author = 'github-actions[bot]',
            [string]$HeadRef = 'ci-fix/issue-123',
            [string]$BaseRef = 'main',
            [string]$Title = '[ci-fix] Repair CI (refs #123)',
            [string[]]$Labels = @('agentic-workflows'),
            [string]$State = 'open',
            [string]$EventLabel = 'agentic-workflows'
        )

        return [pscustomobject]@{
            action = $Action
            label = [pscustomobject]@{ name = $EventLabel }
            pull_request = New-TestPullRequest `
                -Repository $Repository `
                -HeadRepository $HeadRepository `
                -Author $Author `
                -HeadRef $HeadRef `
                -BaseRef $BaseRef `
                -Title $Title `
                -Labels $Labels `
                -State $State
        }
    }

    function New-TestMergeCommit {
        param(
            [string]$MergeSha = '2222222222222222222222222222222222222222',
            [string]$HeadSha = '1111111111111111111111111111111111111111',
            [string]$BaseParentSha = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
        )

        return [pscustomobject]@{
            sha = $MergeSha
            parents = @(
                [pscustomobject]@{ sha = $BaseParentSha },
                [pscustomobject]@{ sha = $HeadSha }
            )
        }
    }

    function New-TestPullRequestFixture {
        param([Parameter(Mandatory = $true)][object[]]$PullRequests)

        $details = [ordered]@{}
        $commits = [ordered]@{}
        foreach ($pullRequest in $PullRequests) {
            $number = [string]$pullRequest.number
            $details[$number] = $pullRequest
            if ([string]$pullRequest.merge_commit_sha -match '^[0-9a-fA-F]{40}$') {
                $commits[[string]$pullRequest.merge_commit_sha] = New-TestMergeCommit `
                    -MergeSha ([string]$pullRequest.merge_commit_sha) `
                    -HeadSha ([string]$pullRequest.head.sha)
            }
        }

        return [pscustomobject]@{
            pullRequests = $PullRequests
            pullRequestDetails = [pscustomobject]$details
            commits = [pscustomobject]$commits
        }
    }
}

Describe 'Test-CiFixPrFingerprint' {
    It 'accepts the exact main automated PR fingerprint' {
        Test-CiFixPrFingerprint `
            -Repository dotnet/maui `
            -HeadRepository dotnet/maui `
            -Title '[ci-fix] Repair CI' `
            -BaseRef main `
            -HeadRef ci-fix/issue-123 `
            -AuthorLogin 'github-actions[bot]' `
            -Labels @('agentic-workflows') | Should -BeTrue
    }

    It 'accepts the exact net11 automated PR fingerprint' {
        Test-CiFixPrFingerprint `
            -Repository dotnet/maui `
            -HeadRepository dotnet/maui `
            -Title '[ci-fix-net11] Repair CI' `
            -BaseRef net11.0 `
            -HeadRef ci-fix/issue-123 `
            -AuthorLogin 'github-actions[bot]' `
            -Labels @('agentic-workflows') | Should -BeTrue
    }

    It 'fails closed for each mismatched trust attribute' -ForEach @(
        @{ Repository = 'fork/maui'; HeadRepository = 'dotnet/maui'; Author = 'github-actions[bot]'; Head = 'ci-fix/issue-123'; Base = 'main'; Title = '[ci-fix] Repair'; Labels = @('agentic-workflows') }
        @{ Repository = 'dotnet/maui'; HeadRepository = 'fork/maui'; Author = 'github-actions[bot]'; Head = 'ci-fix/issue-123'; Base = 'main'; Title = '[ci-fix] Repair'; Labels = @('agentic-workflows') }
        @{ Repository = 'dotnet/maui'; HeadRepository = 'dotnet/maui'; Author = 'attacker'; Head = 'ci-fix/issue-123'; Base = 'main'; Title = '[ci-fix] Repair'; Labels = @('agentic-workflows') }
        @{ Repository = 'dotnet/maui'; HeadRepository = 'dotnet/maui'; Author = 'github-actions[bot]'; Head = 'feature/issue-123'; Base = 'main'; Title = '[ci-fix] Repair'; Labels = @('agentic-workflows') }
        @{ Repository = 'dotnet/maui'; HeadRepository = 'dotnet/maui'; Author = 'github-actions[bot]'; Head = 'ci-fix/issue-123'; Base = 'main'; Title = '[ci-fix-net11] Repair'; Labels = @('agentic-workflows') }
        @{ Repository = 'dotnet/maui'; HeadRepository = 'dotnet/maui'; Author = 'github-actions[bot]'; Head = 'ci-fix/issue-123'; Base = 'net11.0'; Title = '[ci-fix] Repair'; Labels = @('agentic-workflows') }
        @{ Repository = 'dotnet/maui'; HeadRepository = 'dotnet/maui'; Author = 'github-actions[bot]'; Head = 'ci-fix/issue-123'; Base = 'main'; Title = '[ci-fix] Repair'; Labels = @() }
    ) {
        Test-CiFixPrFingerprint `
            -Repository $Repository `
            -HeadRepository $HeadRepository `
            -Title $Title `
            -BaseRef $Base `
            -HeadRef $Head `
            -AuthorLogin $Author `
            -Labels $Labels | Should -BeFalse
    }
}

Describe 'trusted workflow configuration' {
    It 'pins checkout to the reviewed v7.0.1 commit in the id-token job' {
        $workflow = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../workflows/ci-fix-azdo-validation.yml')

        $workflow | Should -Match 'actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7\.0\.1'
        $workflow | Should -Match 'ref: \$\{\{ github\.sha \}\}'
        $workflow | Should -Not -Match 'github\.event\.pull_request\.base\.sha'
        $workflow | Should -Match 'persist-credentials: false'
        $workflow | Should -Match 'id-token: write'
        $workflow | Should -Match 'timeout-minutes: 10'
        $workflow | Should -Match 'Queue-CiFixAzdoValidation\.ps1 -DispatcherBudgetSeconds 420'
    }
}

Describe 'Get-CiFixEventContext' {
    It 'admits opened, reopened, synchronize, and matching labeled events' -ForEach @(
        @{ Action = 'opened' }
        @{ Action = 'reopened' }
        @{ Action = 'synchronize' }
        @{ Action = 'labeled' }
    ) {
        $context = Get-CiFixEventContext `
            -Event (New-TestEvent -Action $Action) `
            -Repository dotnet/maui `
            -EventName pull_request_target

        $context.PullRequestNumber | Should -Be 123
    }

    It 'ignores a labeled event for any other label' {
        $context = Get-CiFixEventContext `
            -Event (New-TestEvent -Action labeled -EventLabel unrelated) `
            -Repository dotnet/maui `
            -EventName pull_request_target

        $context | Should -BeNullOrEmpty
    }

    It 'ignores an otherwise valid event until the required label exists' {
        $context = Get-CiFixEventContext `
            -Event (New-TestEvent -Labels @()) `
            -Repository dotnet/maui `
            -EventName pull_request_target

        $context | Should -BeNullOrEmpty
    }

    It 'rejects any event type other than pull_request_target' {
        {
            Get-CiFixEventContext `
                -Event (New-TestEvent) `
                -Repository dotnet/maui `
                -EventName pull_request
        } | Should -Throw "*Unexpected event 'pull_request'*"
    }

    It 'ignores an unrelated PR before requiring a merge SHA' {
        $event = New-TestEvent -Author attacker
        $event.pull_request.merge_commit_sha = $null

        $context = Get-CiFixEventContext `
            -Event $event `
            -Repository dotnet/maui `
            -EventName pull_request_target

        $context | Should -BeNullOrEmpty
    }

    It 'defers an eligible PR whose merge commit is not available yet' {
        $event = New-TestEvent
        $event.pull_request.merge_commit_sha = $null

        $context = Get-CiFixEventContext `
            -Event $event `
            -Repository dotnet/maui `
            -EventName pull_request_target `
            -WarningAction SilentlyContinue

        $context | Should -BeNullOrEmpty
    }
}

Describe 'Test-TrustedCiFixWorkflowRun' {
    It 'accepts only the trusted default-branch fixer completions' -ForEach @(
        @{ Name = 'CI Failure Fixer (main)'; Path = '.github/workflows/ci-status-fix.lock.yml' }
        @{ Name = 'CI Failure Fixer (net11.0)'; Path = '.github/workflows/ci-status-fix-net11.lock.yml' }
    ) {
        $event = [pscustomobject]@{
            action = 'completed'
            repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
            workflow_run = [pscustomobject]@{
                name = $Name
                path = $Path
                head_branch = 'main'
                head_repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
            }
        }

        Test-TrustedCiFixWorkflowRun -Event $event -Repository dotnet/maui | Should -BeTrue
    }

    It 'rejects a matching workflow name from a non-main ref' {
        $event = [pscustomobject]@{
            action = 'completed'
            repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
            workflow_run = [pscustomobject]@{
                name = 'CI Failure Fixer (main)'
                path = '.github/workflows/ci-status-fix.lock.yml'
                head_branch = 'feature/untrusted'
                head_repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
            }
        }

        Test-TrustedCiFixWorkflowRun -Event $event -Repository dotnet/maui | Should -BeFalse
    }
}

Describe 'Get-OpenCiFixContexts' {
    BeforeEach {
        Mock Invoke-DispatcherSleep {}
    }

    It 'enumerates a top-level REST Object[] and keeps both eligible PRs' {
        $script:restPullRequests = @(
            (New-TestPullRequest -Number 123),
            (New-TestPullRequest `
                -Number 124 `
                -BaseRef net11.0 `
                -HeadRef ci-fix/issue-124 `
                -Title '[ci-fix-net11] Repair CI (refs #124)' `
                -HeadSha '3333333333333333333333333333333333333333' `
                -MergeSha '4444444444444444444444444444444444444444'),
            (New-TestPullRequest -Number 125 -Author attacker -HeadRef feature/unrelated)
        )
        $script:restDetails = @{
            123 = $script:restPullRequests[0]
            124 = $script:restPullRequests[1]
        }
        Mock Invoke-WithHttpRetry {
            if ($OperationName -like 'GitHub open pull request query page *') {
                return ,$script:restPullRequests
            }
            if ($OperationName -match 'pull request #(?<number>\d+) detail') {
                return $script:restDetails[[int]$Matches.number]
            }
            if ($OperationName -match 'test merge commit (?<sha>[0-9a-f]{40})') {
                $detail = $script:restDetails.Values |
                    Where-Object merge_commit_sha -EQ $Matches.sha |
                    Select-Object -First 1
                return New-TestMergeCommit -MergeSha $Matches.sha -HeadSha $detail.head.sha
            }
            throw "Unexpected operation '$OperationName'."
        }

        $contexts = @(
            Get-OpenCiFixContexts `
                -Repository dotnet/maui `
                -GitHubToken token `
                -FixturePath ''
        )

        $contexts.Count | Should -Be 2
        $contexts.PullRequestNumber | Should -Be @(123, 124)
        Should -Invoke Invoke-WithHttpRetry -Times 5 -Exactly
        Should -Invoke Invoke-WithHttpRetry -Times 0 -Exactly -ParameterFilter {
            $OperationName -match 'pull request #125 detail'
        }
    }

    It 'handles an empty REST array' {
        Mock Invoke-WithHttpRetry { return ,@() }

        $contexts = @(
            Get-OpenCiFixContexts `
                -Repository dotnet/maui `
                -GitHubToken token `
                -FixturePath ''
        )

        $contexts.Count | Should -Be 0
        Should -Invoke Invoke-WithHttpRetry -Times 1 -Exactly
    }

    It 'uses the enumerated count to fetch the next page at the 100 item boundary' {
        $script:page = 0
        $script:fullPage = @(
            1..100 | ForEach-Object {
                New-TestPullRequest -Number (1000 + $_) -Author attacker -HeadRef "feature/unrelated-$_"
            }
        )
        Mock Invoke-WithHttpRetry {
            $script:page++
            if ($script:page -eq 1) {
                return ,$script:fullPage
            }
            return ,@()
        }

        $contexts = @(
            Get-OpenCiFixContexts `
                -Repository dotnet/maui `
                -GitHubToken token `
                -FixturePath ''
        )

        $contexts.Count | Should -Be 0
        Should -Invoke Invoke-WithHttpRetry -Times 2 -Exactly
    }

    It 'refreshes a stale list response and verifies the current head and test merge pair' {
        $listPullRequest = New-TestPullRequest `
            -HeadSha '1111111111111111111111111111111111111111' `
            -MergeSha '2222222222222222222222222222222222222222'
        $staleDetailPullRequest = New-TestPullRequest `
            -HeadSha '3333333333333333333333333333333333333333' `
            -MergeSha '2222222222222222222222222222222222222222'
        $freshDetailPullRequest = New-TestPullRequest `
            -HeadSha '3333333333333333333333333333333333333333' `
            -MergeSha '4444444444444444444444444444444444444444'
        $script:detailRefresh = 0
        Mock Invoke-WithHttpRetry {
            if ($OperationName -like 'GitHub open pull request query page *') {
                return ,@($listPullRequest)
            }
            if ($OperationName -like '*detail refresh') {
                $script:detailRefresh++
                if ($script:detailRefresh -eq 1) {
                    return $staleDetailPullRequest
                }
                return $freshDetailPullRequest
            }
            if ($OperationName -like 'GitHub test merge commit 2222*') {
                return New-TestMergeCommit `
                    -MergeSha '2222222222222222222222222222222222222222' `
                    -HeadSha '1111111111111111111111111111111111111111'
            }
            if ($OperationName -like 'GitHub test merge commit 4444*') {
                return New-TestMergeCommit `
                    -MergeSha '4444444444444444444444444444444444444444' `
                    -HeadSha '3333333333333333333333333333333333333333'
            }
            throw "Unexpected operation '$OperationName'."
        }

        $contexts = @(Get-OpenCiFixContexts -Repository dotnet/maui -GitHubToken token -FixturePath '')

        $contexts.Count | Should -Be 1
        $contexts[0].HeadSha | Should -Be '3333333333333333333333333333333333333333'
        $contexts[0].MergeSha | Should -Be '4444444444444444444444444444444444444444'
        Should -Invoke Invoke-DispatcherSleep -Times 1 -Exactly
    }

    It 'fails closed after bounded refreshes when the test merge remains stale' {
        $pullRequest = New-TestPullRequest
        Mock Invoke-WithHttpRetry {
            if ($OperationName -like 'GitHub open pull request query page *') {
                return ,@($pullRequest)
            }
            if ($OperationName -like '*detail refresh') {
                return $pullRequest
            }
            if ($OperationName -like 'GitHub test merge commit *') {
                return New-TestMergeCommit `
                    -MergeSha $pullRequest.merge_commit_sha `
                    -HeadSha '3333333333333333333333333333333333333333'
            }
            throw "Unexpected operation '$OperationName'."
        }

        $contexts = @(Get-OpenCiFixContexts -Repository dotnet/maui -GitHubToken token -FixturePath '')

        $contexts.Count | Should -Be 1
        $contexts[0].VerificationError |
            Should -BeLike "*PR #123 head '1111111111111111111111111111111111111111' did not obtain a verified test merge after 4 attempts*source parent '3333333333333333333333333333333333333333' does not match head*No Azure DevOps validation was queued or deduplicated*"

        Should -Invoke Invoke-WithHttpRetry -Times 4 -Exactly -ParameterFilter {
            $OperationName -like '*detail refresh'
        }
        Should -Invoke Invoke-WithHttpRetry -Times 4 -Exactly -ParameterFilter {
            $OperationName -like 'GitHub test merge commit *'
        }
        Should -Invoke Invoke-DispatcherSleep -Times 3 -Exactly
    }

    It 'uses an individually refreshed revocation as authoritative' {
        $listPullRequest = New-TestPullRequest
        $revokedPullRequest = New-TestPullRequest -Labels @()
        Mock Invoke-WithHttpRetry {
            if ($OperationName -like 'GitHub open pull request query page *') {
                return ,@($listPullRequest)
            }
            if ($OperationName -like '*detail refresh') {
                return $revokedPullRequest
            }
            throw "Unexpected operation '$OperationName'."
        }

        $contexts = @(Get-OpenCiFixContexts -Repository dotnet/maui -GitHubToken token -FixturePath '')

        $contexts.Count | Should -Be 0
        Should -Invoke Invoke-WithHttpRetry -Times 0 -Exactly -ParameterFilter {
            $OperationName -like 'GitHub test merge commit *'
        }
    }

    It 'stops stale merge refreshes when the shared dispatcher budget expires' {
        $pullRequest = New-TestPullRequest
        Mock Invoke-WithHttpRetry {
            if ($OperationName -like 'GitHub open pull request query page *') {
                return ,@($pullRequest)
            }
            if ($OperationName -like '*detail refresh') {
                return $pullRequest
            }
            if ($OperationName -like 'GitHub test merge commit *') {
                return New-TestMergeCommit `
                    -MergeSha $pullRequest.merge_commit_sha `
                    -HeadSha '3333333333333333333333333333333333333333'
            }
            throw "Unexpected operation '$OperationName'."
        }
        Mock Invoke-DispatcherSleep {
            throw "$($script:DispatcherBudgetPrefix) No retry time remains."
        }

        {
            Get-OpenCiFixContexts -Repository dotnet/maui -GitHubToken token -FixturePath ''
        } | Should -Throw '*dispatcher-budget-exhausted*'

        Should -Invoke Invoke-WithHttpRetry -Times 1 -Exactly -ParameterFilter {
            $OperationName -like '*detail refresh'
        }
        Should -Invoke Invoke-WithHttpRetry -Times 1 -Exactly -ParameterFilter {
            $OperationName -like 'GitHub test merge commit *'
        }
    }

    It 'rejects malformed test merge metadata before a context can be returned' -ForEach @(
        @{
            Name = 'wrong commit identity'
            Commit = [pscustomobject]@{
                sha = '5555555555555555555555555555555555555555'
                parents = @(
                    [pscustomobject]@{ sha = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' },
                    [pscustomobject]@{ sha = '1111111111111111111111111111111111111111' }
                )
            }
            Expected = '*requested merge*returned commit*'
        },
        @{
            Name = 'missing source parent'
            Commit = [pscustomobject]@{
                sha = '2222222222222222222222222222222222222222'
                parents = @([pscustomobject]@{ sha = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' })
            }
            Expected = '*has 1 parent(s), expected exactly 2*'
        },
        @{
            Name = 'wrong source parent'
            Commit = [pscustomobject]@{
                sha = '2222222222222222222222222222222222222222'
                parents = @(
                    [pscustomobject]@{ sha = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' },
                    [pscustomobject]@{ sha = '3333333333333333333333333333333333333333' }
                )
            }
            Expected = '*source parent*does not match head*'
        },
        @{
            Name = 'malformed base parent'
            Commit = [pscustomobject]@{
                sha = '2222222222222222222222222222222222222222'
                parents = @(
                    [pscustomobject]@{ sha = '' },
                    [pscustomobject]@{ sha = '1111111111111111111111111111111111111111' }
                )
            }
            Expected = '*has an invalid first parent*'
        }
    ) {
        $pullRequest = New-TestPullRequest
        Mock Invoke-WithHttpRetry {
            if ($OperationName -like 'GitHub open pull request query page *') {
                return ,@($pullRequest)
            }
            if ($OperationName -like '*detail refresh') {
                return $pullRequest
            }
            if ($OperationName -like 'GitHub test merge commit *') {
                return $Commit
            }
            throw "Unexpected operation '$OperationName'."
        }

        $contexts = @(Get-OpenCiFixContexts -Repository dotnet/maui -GitHubToken token -FixturePath '')

        $contexts.Count | Should -Be 1
        $contexts[0].VerificationError | Should -BeLike $Expected

        Should -Invoke Invoke-DispatcherSleep -Times 3 -Exactly
    }
}

Describe 'Get-AzdoToken' {
    BeforeEach {
        $script:oldTenant = $env:AZDO_TRIGGER_TENANT_ID
        $script:oldClient = $env:AZDO_TRIGGER_CLIENT_ID
        $script:oldRequestToken = $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN
        $script:oldRequestUrl = $env:ACTIONS_ID_TOKEN_REQUEST_URL
        $env:AZDO_TRIGGER_TENANT_ID = 'tenant'
        $env:AZDO_TRIGGER_CLIENT_ID = 'client'
        $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN = 'request-token'
        $env:ACTIONS_ID_TOKEN_REQUEST_URL = 'https://example.test/oidc?x=1'

        Mock Write-Host {}
        Mock Invoke-WithHttpRetry {
            if ($OperationName -eq 'GitHub OIDC token request') {
                return [pscustomobject]@{ value = 'oidc-token' }
            }
            if ($OperationName -eq 'Azure AD token exchange') {
                return [pscustomobject]@{ access_token = 'azdo-token' }
            }
            throw "Unexpected operation $OperationName"
        }
    }

    AfterEach {
        $env:AZDO_TRIGGER_TENANT_ID = $script:oldTenant
        $env:AZDO_TRIGGER_CLIENT_ID = $script:oldClient
        $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN = $script:oldRequestToken
        $env:ACTIONS_ID_TOKEN_REQUEST_URL = $script:oldRequestUrl
    }

    It 'returns exactly one success-stream value containing only the access token' {
        $result = @(Get-AzdoToken)

        $result.Count | Should -Be 1
        $result[0] | Should -BeExactly 'azdo-token'
        Should -Invoke Write-Host -Times 2 -Exactly
    }
}

Describe 'New-AzdoQueueRequest' {
    It 'queues the PR merge ref and merge commit while preserving the source head identity' {
        $context = Get-CiFixEventContext `
            -Event (New-TestEvent) `
            -Repository dotnet/maui `
            -EventName pull_request_target

        $request = New-AzdoQueueRequest -DefinitionId 302 -Context $context

        $request.definition.id | Should -Be 302
        $request.reason | Should -Be 'pullRequest'
        $request.sourceBranch | Should -Be 'refs/pull/123/merge'
        $request.sourceVersion | Should -Be '2222222222222222222222222222222222222222'
        $request.triggerInfo.'pr.sourceSha' | Should -Be '1111111111111111111111111111111111111111'
        $request.triggerInfo.'pr.targetBranch' | Should -Be 'main'
        $request.triggerInfo.'pr.number' | Should -Be '123'
        $request.triggerInfo.'pr.providerId' | Should -Be 'github'
    }

    It 'sets the bare net11 target branch in trigger metadata' {
        $context = Get-CiFixEventContext `
            -Event (New-TestEvent `
                -BaseRef net11.0 `
                -Title '[ci-fix-net11] Repair CI' `
                -HeadRef ci-fix/issue-124) `
            -Repository dotnet/maui `
            -EventName pull_request_target

        $request = New-AzdoQueueRequest -DefinitionId 302 -Context $context

        $request.triggerInfo.'pr.targetBranch' | Should -Be 'net11.0'
    }

    It 'serializes producer-equivalent system pull request parameters for both target branches' {
        foreach ($targetBranch in @('main', 'net11.0')) {
            $title = if ($targetBranch -eq 'main') { '[ci-fix] Repair CI' } else { '[ci-fix-net11] Repair CI' }
            $headRef = if ($targetBranch -eq 'main') { 'ci-fix/issue-123' } else { 'ci-fix/issue-124' }
            $context = Get-CiFixEventContext `
                -Event (New-TestEvent -BaseRef $targetBranch -Title $title -HeadRef $headRef) `
                -Repository dotnet/maui `
                -EventName pull_request_target
            $request = New-AzdoQueueRequest -DefinitionId 302 -Context $context
            $outerJson = $request | ConvertTo-Json -Depth 10 -Compress
            $outerRequest = $outerJson | ConvertFrom-Json -Depth 10

            $outerRequest.parameters.GetType() | Should -Be ([string])
            $parameters = $outerRequest.parameters | ConvertFrom-Json
            @($parameters.PSObject.Properties.Name | Sort-Object) | Should -Be @(
                'system.pullRequest.isFork',
                'system.pullRequest.mergedAt',
                'system.pullRequest.pullRequestId',
                'system.pullRequest.pullRequestNumber',
                'system.pullRequest.sourceBranch',
                'system.pullRequest.sourceCommitId',
                'system.pullRequest.sourceRepositoryUri',
                'system.pullRequest.targetBranch',
                'system.pullRequest.targetBranchName'
            )

            $parameters.'system.pullRequest.pullRequestId' | Should -Be '456912'
            $parameters.'system.pullRequest.pullRequestNumber' | Should -Be '123'
            $parameters.'system.pullRequest.mergedAt' | Should -Be ''
            $parameters.'system.pullRequest.sourceBranch' | Should -Be $headRef
            $parameters.'system.pullRequest.targetBranch' | Should -Be $targetBranch
            $parameters.'system.pullRequest.targetBranchName' | Should -Be $targetBranch
            $parameters.'system.pullRequest.sourceRepositoryUri' | Should -Be 'https://github.com/dotnet/maui'
            $parameters.'system.pullRequest.sourceCommitId' | Should -Be '1111111111111111111111111111111111111111'
            $parameters.'system.pullRequest.isFork' | Should -Be 'False'
            foreach ($property in $parameters.PSObject.Properties) {
                $property.Value.GetType() | Should -Be ([string])
            }
            $outerRequest.sourceVersion | Should -Be '2222222222222222222222222222222222222222'
            $parameters.'system.pullRequest.sourceCommitId' | Should -Not -Be $outerRequest.sourceVersion
        }
    }

    It 'defines all three MAUI validation pipelines' {
        $pipelines = @(Get-CiFixPipelineDefinitions)
        $pipelines.Name | Should -Be @('maui-pr', 'maui-pr-uitests', 'maui-pr-devicetests')
        $pipelines.DefinitionId | Should -Be @(302, 313, 314)
    }
}

Describe 'Find-AzdoDuplicateBuild' {
    It 'deduplicates any prior build for the same pipeline PR head regardless of completion state' {
        $builds = @(
            [pscustomobject]@{
                id = 9001
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
                status = 'completed'
                result = 'failed'
                triggerInfo = [pscustomobject]@{
                    'pr.number' = '123'
                    'pr.sourceSha' = '1111111111111111111111111111111111111111'
                }
            }
        )

        $duplicate = Find-AzdoDuplicateBuild `
            -Builds $builds `
            -PullRequestNumber 123 `
            -HeadSha '1111111111111111111111111111111111111111' `
            -MergeSha '2222222222222222222222222222222222222222'

        $duplicate.id | Should -Be 9001
    }

    It 'does not deduplicate an older head on the same PR' {
        $builds = @(
            [pscustomobject]@{
                id = 9001
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
                triggerInfo = [pscustomobject]@{
                    'pr.number' = '123'
                    'pr.sourceSha' = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
                }
            }

        )

        Find-AzdoDuplicateBuild `
            -Builds $builds `
            -PullRequestNumber 123 `
            -HeadSha '1111111111111111111111111111111111111111' `
            -MergeSha '2222222222222222222222222222222222222222' |
            Should -BeNullOrEmpty
    }

    It 'deduplicates legacy metadata by the exact merge commit' {
        $builds = @(
            [pscustomobject]@{
                id = 9002
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = '2222222222222222222222222222222222222222'
                triggerInfo = [pscustomobject]@{}
            }
        )

        $duplicate = Find-AzdoDuplicateBuild `
            -Builds $builds `
            -PullRequestNumber 123 `
            -HeadSha '1111111111111111111111111111111111111111' `
            -MergeSha '2222222222222222222222222222222222222222'

        $duplicate.id | Should -Be 9002
    }

    It 'does not let a previous-head build deduplicate a current verified head through an old merge SHA' {
        $builds = @(
            [pscustomobject]@{
                id = 9003
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = '2222222222222222222222222222222222222222'
                triggerInfo = [pscustomobject]@{
                    'pr.number' = '123'
                    'pr.sourceSha' = '1111111111111111111111111111111111111111'
                }
            }
        )

        Find-AzdoDuplicateBuild `
            -Builds $builds `
            -PullRequestNumber 123 `
            -HeadSha '3333333333333333333333333333333333333333' `
            -MergeSha '4444444444444444444444444444444444444444' |
            Should -BeNullOrEmpty
    }
}

Describe 'Invoke-AzdoPipelineQueue retry safety' {
    BeforeAll {
        $queueFunctionDefinitions = @(
            'Get-ObjectPropertyValue',
            'Get-DispatcherElapsedSeconds',
            'Get-DispatcherRemainingSeconds',
            'Test-IsDispatcherBudgetException',
            'Get-DispatcherHttpTimeoutSeconds',
            'Invoke-DispatcherSleep',
            'Get-CiFixPipelineDefinitions',
            'Test-IsTransientHttpException',
            'Get-HttpStatusCode',
            'Invoke-WithHttpRetry',
            'Find-AzdoDuplicateBuild',
            'Get-AzdoDuplicateBuild',
            'New-AzdoQueueRequest',
            'Invoke-AzdoPipelineQueue',
            'Write-CiFixJobSummary',
            'New-CiFixFailureResult',
            'Invoke-CiFixQueueWork'
        ) | ForEach-Object {
            $functionName = $_
            $ast.Find({
                    $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                    $args[0].Name -eq $functionName
                }, $true).Extent.Text
        }
        $script:queueTestModule = New-Module -Name QueueCiFixAzdoValidationTest -ScriptBlock {
            param([string[]]$FunctionDefinitions)

            $script:AzureDevOpsOrganization = 'dnceng-public'
            $script:AzureDevOpsProject = 'public'
            $script:TransientHttpStatusCodes = @(408, 429, 500, 502, 503, 504)
            $script:MaxHttpAttempts = 4
            $script:RetryBaseDelaySeconds = 2
            $script:DispatcherBudgetSeconds = 480
            $script:DispatcherStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            $script:DispatcherBudgetPrefix = '[dispatcher-budget-exhausted]'
            foreach ($definition in $FunctionDefinitions) {
                Invoke-Expression $definition
            }
        } -ArgumentList (, $queueFunctionDefinitions)
        Import-Module $script:queueTestModule -Prefix QueueTest -Force
    }

    AfterAll {
        Remove-Module $script:queueTestModule -Force
    }

    Context 'behavioral paths' {
        BeforeEach {
            $script:queueContext = [pscustomobject]@{
                PullRequestNumber = 123
                PullRequestId = 456789
                Draft = $true
                Title = '[ci-fix] Repair CI (refs #123)'
                BaseRef = 'main'
                HeadRef = 'ci-fix/issue-123'
                HeadSha = '1111111111111111111111111111111111111111'
                MergeSha = '2222222222222222222222222222222222222222'
            }
            Mock Start-Sleep {} -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-DispatcherElapsedSeconds { return 0 } -ModuleName QueueCiFixAzdoValidationTest
        }

        It 'returns a successful POST without duplicate reconciliation' {
            $script:postedBuild = [pscustomobject]@{ id = 7001 }
            Mock Invoke-RestMethod { return $script:postedBuild } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild {
                throw 'Duplicate lookup must not run after a successful POST.'
            } -ModuleName QueueCiFixAzdoValidationTest

            $result = Invoke-QueueTestAzdoPipelineQueue `
                -DefinitionId 302 `
                -Context $script:queueContext `
                -AuthToken test-token

            $result.Build.id | Should -Be 7001
            $result.Reconciled | Should -BeFalse
            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
        }

        It 'reconciles a timed-out POST to the exact build without retrying the POST' {
            $script:duplicateCalls = 0
            $script:correlatedBuild = [pscustomobject]@{ id = 7002 }
            Mock Invoke-RestMethod {
                throw [System.TimeoutException]::new('queue response timed out')
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild {
                $script:duplicateCalls++
                if ($script:duplicateCalls -eq 2) {
                    return $script:correlatedBuild
                }
                return $null
            } -ModuleName QueueCiFixAzdoValidationTest

            $result = Invoke-QueueTestAzdoPipelineQueue `
                -DefinitionId 302 `
                -Context $script:queueContext `
                -AuthToken test-token

            $result.Build.id | Should -Be 7002
            $result.Reconciled | Should -BeTrue
            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly
            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly
        }

        It 'recognizes the PowerShell timeout exception chain and reconciles exactly once' {
            $socket = [System.Net.Sockets.SocketException]::new([System.Net.Sockets.SocketError]::TimedOut)
            $io = [System.IO.IOException]::new('socket timed out', $socket)
            $innerCanceled = [System.Threading.Tasks.TaskCanceledException]::new('socket read canceled', $io)
            $timeout = [System.TimeoutException]::new('request timed out', $innerCanceled)
            $canceled = [System.Threading.Tasks.TaskCanceledException]::new('request canceled', $timeout)
            $script:exactBuild = [pscustomobject]@{
                id = 7004
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = '2222222222222222222222222222222222222222'
                triggerInfo = [pscustomobject]@{
                    'pr.number' = '123'
                    'pr.sourceSha' = '1111111111111111111111111111111111111111'
                }
            }
            $script:wrongBuild = [pscustomobject]@{
                id = 7005
                sourceBranch = 'refs/pull/123/merge'
                sourceVersion = 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa'
                triggerInfo = [pscustomobject]@{
                    'pr.number' = '123'
                    'pr.sourceSha' = 'bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb'
                }
            }
            Mock Invoke-RestMethod {
                if ($Method -eq 'Post') {
                    throw $canceled
                }
                return [pscustomobject]@{ value = @($script:wrongBuild, $script:exactBuild) }
            } -ModuleName QueueCiFixAzdoValidationTest

            (Test-QueueTestIsTransientHttpException -Exception $canceled) | Should -BeTrue
            $result = Invoke-QueueTestAzdoPipelineQueue `
                -DefinitionId 302 `
                -Context $script:queueContext `
                -AuthToken test-token

            $result.Build.id | Should -Be 7004
            $result.Reconciled | Should -BeTrue
            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Get' }
        }

        It 'keeps nested timeout POST acceptance uncertain when the exact reconciliation read fails' {
            $socket = [System.Net.Sockets.SocketException]::new([System.Net.Sockets.SocketError]::TimedOut)
            $io = [System.IO.IOException]::new('socket timed out', $socket)
            $innerCanceled = [System.Threading.Tasks.TaskCanceledException]::new('socket read canceled', $io)
            $timeout = [System.TimeoutException]::new('request timed out', $innerCanceled)
            $canceled = [System.Threading.Tasks.TaskCanceledException]::new('request canceled', $timeout)
            Mock Invoke-RestMethod {
                if ($Method -eq 'Post') {
                    throw $canceled
                }
                throw [System.InvalidOperationException]::new('duplicate query unavailable')
            } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestAzdoPipelineQueue `
                    -DefinitionId 302 `
                    -Context $script:queueContext `
                    -AuthToken test-token
            } | Should -Throw '*may have been accepted*exact reconciliation failed: duplicate query unavailable*POST was issued exactly once*acceptance remains uncertain*'

            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Get' }
        }

        It 'does not classify plain cancellation or authentication failures as transient' {
            (Test-QueueTestIsTransientHttpException -Exception ([System.Threading.Tasks.TaskCanceledException]::new('caller canceled'))) | Should -BeFalse
            $unauthorized = [System.Net.Http.HttpRequestException]::new(
                'unauthorized',
                $null,
                [System.Net.HttpStatusCode]::Unauthorized)
            (Test-QueueTestIsTransientHttpException -Exception $unauthorized) | Should -BeFalse
            $wrappedUnauthorized = [System.Net.Http.HttpRequestException]::new(
                'unauthorized',
                [System.TimeoutException]::new('inner timeout'),
                [System.Net.HttpStatusCode]::Unauthorized)
            (Test-QueueTestIsTransientHttpException -Exception $wrappedUnauthorized) | Should -BeFalse
        }

        It 'reconciles a 5xx POST to the exact build without retrying the POST' {
            $script:correlatedBuild = [pscustomobject]@{ id = 7003 }
            Mock Invoke-RestMethod {
                $exception = [System.Exception]::new('service unavailable')
                $exception | Add-Member -NotePropertyName StatusCode -NotePropertyValue 503
                throw $exception
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild {
                return $script:correlatedBuild
            } -ModuleName QueueCiFixAzdoValidationTest

            $result = Invoke-QueueTestAzdoPipelineQueue `
                -DefinitionId 313 `
                -Context $script:queueContext `
                -AuthToken test-token

            $result.Build.id | Should -Be 7003
            $result.Reconciled | Should -BeTrue
            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly
            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly
        }

        It 'throws after bounded reconciliation when no exact build appears' {
            Mock Invoke-RestMethod {
                throw [System.TimeoutException]::new('queue response timed out')
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild { return $null } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestAzdoPipelineQueue `
                    -DefinitionId 314 `
                    -Context $script:queueContext `
                    -AuthToken test-token
            } | Should -Throw '*may have been accepted*no exact correlated build appeared after reconciliation*POST was issued exactly once*acceptance remains uncertain*'

            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 4 -Exactly
            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 4 -Exactly
        }

        It 'preserves possible acceptance when reconciliation itself fails' {
            Mock Invoke-RestMethod {
                throw [System.TimeoutException]::new('queue response timed out')
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild {
                throw [System.InvalidOperationException]::new('duplicate lookup unavailable')
            } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestAzdoPipelineQueue `
                    -DefinitionId 302 `
                    -Context $script:queueContext `
                    -AuthToken test-token
            } | Should -Throw '*may have been accepted*exact reconciliation failed: duplicate lookup unavailable*POST was issued exactly once*acceptance remains uncertain*'

            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter {
                $Method -eq 'Post'
            }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly
        }

        It 'throws a nontransient POST failure without reconciliation or retry' {
            Mock Invoke-RestMethod {
                throw [System.InvalidOperationException]::new('invalid request')
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild {
                throw 'Duplicate lookup must not run for a nontransient failure.'
            } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestAzdoPipelineQueue `
                    -DefinitionId 302 `
                    -Context $script:queueContext `
                    -AuthToken test-token
            } | Should -Throw '*invalid request*'

            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter { $Method -eq 'Post' }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
        }

        It 'caps HTTP timeouts and retry sleeps to the shared remaining budget' {
            Mock Get-DispatcherElapsedSeconds { return 455 } -ModuleName QueueCiFixAzdoValidationTest

            Get-QueueTestDispatcherHttpTimeoutSeconds -OperationName 'bounded request' |
                Should -Be 25

            Mock Get-DispatcherElapsedSeconds { return 475 } -ModuleName QueueCiFixAzdoValidationTest
            Invoke-QueueTestDispatcherSleep -OperationName 'bounded sleep' -RequestedSeconds 10

            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter {
                $Seconds -eq 4
            }
        }

        It 'prevents a queue POST after the shared budget expires' {
            Mock Get-DispatcherElapsedSeconds { return 480 } -ModuleName QueueCiFixAzdoValidationTest
            Mock Invoke-RestMethod { throw 'HTTP must not run after budget expiry.' } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestAzdoPipelineQueue `
                    -DefinitionId 302 `
                    -Context $script:queueContext `
                    -AuthToken test-token
            } | Should -Throw '*dispatcher-budget-exhausted*before*queue POST*'

            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
        }

        It 'prevents nested HTTP retries from overrunning the shared budget' {
            $script:elapsedCalls = 0
            Mock Get-DispatcherElapsedSeconds {
                $script:elapsedCalls++
                if ($script:elapsedCalls -le 2) { return 470 }
                return 479.5
            } -ModuleName QueueCiFixAzdoValidationTest
            $script:operationCalls = 0

            {
                Invoke-QueueTestWithHttpRetry -OperationName 'nested retry' -Operation {
                    param($timeoutSeconds)
                    $script:operationCalls++
                    $timeoutSeconds | Should -Be 10
                    throw [System.TimeoutException]::new('transient')
                }
            } | Should -Throw '*dispatcher-budget-exhausted*'

            $script:operationCalls | Should -Be 1
            Should -Invoke Start-Sleep -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter {
                $Seconds -eq 2
            }
        }

        It 'keeps an ambiguous POST uncertain when reconciliation exhausts the budget' {
            Mock Invoke-RestMethod {
                throw [System.TimeoutException]::new('queue response timed out')
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Invoke-DispatcherSleep {
                throw '[dispatcher-budget-exhausted] no reconciliation time remains'
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Get-AzdoDuplicateBuild {
                throw 'Duplicate lookup must not run after sleep exhausts the budget.'
            } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestAzdoPipelineQueue `
                    -DefinitionId 302 `
                    -Context $script:queueContext `
                    -AuthToken test-token
            } | Should -Throw '*may have been accepted*POST was issued exactly once*acceptance remains uncertain*'

            Should -Invoke Invoke-RestMethod -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly -ParameterFilter {
                $Method -eq 'Post'
            }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
        }

        It 'preserves completed results and marks all remaining work failed after budget exhaustion' {
            $secondContext = $script:queueContext.PSObject.Copy()
            $secondContext.PullRequestNumber = 124
            $secondContext.PullRequestId = 456913
            $secondContext.HeadSha = '3333333333333333333333333333333333333333'
            $secondContext.MergeSha = '4444444444444444444444444444444444444444'
            $script:queueCalls = 0
            Mock Get-AzdoDuplicateBuild { return $null } -ModuleName QueueCiFixAzdoValidationTest
            Mock Invoke-AzdoPipelineQueue {
                $script:queueCalls++
                if ($script:queueCalls -eq 1) {
                    return [pscustomobject]@{
                        Build = [pscustomobject]@{ id = 8001 }
                        Reconciled = $false
                    }
                }
                throw '[dispatcher-budget-exhausted] queue budget expired before the next POST'
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Write-CiFixJobSummary {} -ModuleName QueueCiFixAzdoValidationTest

            $results = @(
                Invoke-QueueTestCiFixQueueWork `
                    -Contexts @($script:queueContext, $secondContext) `
                    -AuthToken test-token
            )

            $results.Count | Should -Be 6
            $results[0].Outcome | Should -Be 'queued'
            $results[0].BuildId | Should -Be 8001
            @($results[1..5].Outcome | Sort-Object -Unique) | Should -Be @('failed')
            $results[1].Error | Should -Match 'dispatcher-budget-exhausted'
            $results[1].Error | Should -Match 'queue budget expired before the next POST'
            foreach ($result in $results[2..5]) {
                $result.Error | Should -Match "PR #$($result.PullRequestNumber) pipeline '$([regex]::Escape($result.Name))' was not processed"
                $result.Error | Should -Match 'No queue POST was attempted for this work item'
                $result.Error | Should -Match "while processing PR #123 pipeline 'maui-pr-uitests'"
                $result.Error | Should -Not -Match 'may have been accepted'
            }
            Should -Invoke Invoke-AzdoPipelineQueue -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly
            Should -Invoke Write-CiFixJobSummary -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly -ParameterFilter {
                @($Results).Count -eq 3
            }
        }

        It 'keeps ambiguous uncertainty on only the submitted POST and skips every later item accurately' {
            $secondContext = $script:queueContext.PSObject.Copy()
            $secondContext.PullRequestNumber = 124
            $secondContext.PullRequestId = 456913
            $secondContext.HeadSha = '3333333333333333333333333333333333333333'
            $secondContext.MergeSha = '4444444444444444444444444444444444444444'
            Mock Get-AzdoDuplicateBuild { return $null } -ModuleName QueueCiFixAzdoValidationTest
            Mock Invoke-AzdoPipelineQueue {
                throw '[dispatcher-budget-exhausted] Azure DevOps queue request for definition 302 may have been accepted, but reconciliation expired. The POST was issued exactly once and acceptance remains uncertain.'
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Write-CiFixJobSummary {} -ModuleName QueueCiFixAzdoValidationTest

            $results = @(
                Invoke-QueueTestCiFixQueueWork `
                    -Contexts @($script:queueContext, $secondContext) `
                    -AuthToken test-token
            )

            $results.Count | Should -Be 6
            $results[0].Error | Should -Match 'definition 302 may have been accepted'
            $results[0].Error | Should -Match 'acceptance remains uncertain'
            foreach ($result in $results[1..5]) {
                $result.Error | Should -Match "PR #$($result.PullRequestNumber) pipeline '$([regex]::Escape($result.Name))' was not processed"
                $result.Error | Should -Match 'No queue POST was attempted for this work item'
                $result.Error | Should -Not -Match 'may have been accepted'
                $result.Error | Should -Not -Match 'acceptance remains uncertain'
            }
            Should -Invoke Invoke-AzdoPipelineQueue -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 1 -Exactly
            Should -Invoke Write-CiFixJobSummary -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly
        }

        It 'isolates an unverifiable PR while processing every pipeline for the next verified head' {
            $unverifiedContext = $script:queueContext.PSObject.Copy()
            $unverifiedContext | Add-Member `
                -NotePropertyName VerificationError `
                -NotePropertyValue "Eligible CI-fix PR #123 head '$($unverifiedContext.HeadSha)' did not obtain a verified test merge. No Azure DevOps validation was queued or deduplicated for this PR."
            $verifiedContext = $script:queueContext.PSObject.Copy()
            $verifiedContext.PullRequestNumber = 124
            $verifiedContext.PullRequestId = 456913
            $verifiedContext.BaseRef = 'net11.0'
            $verifiedContext.HeadRef = 'ci-fix/issue-124'
            $verifiedContext.HeadSha = '3333333333333333333333333333333333333333'
            $verifiedContext.MergeSha = '4444444444444444444444444444444444444444'
            Mock Get-AzdoDuplicateBuild { return $null } -ModuleName QueueCiFixAzdoValidationTest
            Mock Invoke-AzdoPipelineQueue {
                return [pscustomobject]@{
                    Build = [pscustomobject]@{ id = 8000 + $DefinitionId }
                    Reconciled = $false
                }
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Write-CiFixJobSummary {} -ModuleName QueueCiFixAzdoValidationTest

            $results = @(
                Invoke-QueueTestCiFixQueueWork `
                    -Contexts @($unverifiedContext, $verifiedContext) `
                    -AuthToken test-token
            )

            $results.Count | Should -Be 6
            @($results[0..2].Outcome | Select-Object -Unique) | Should -Be @('failed')
            foreach ($result in $results[0..2]) {
                $result.PullRequestNumber | Should -Be 123
                $result.Error | Should -Match 'did not obtain a verified test merge'
                $result.Error | Should -Match 'No Azure DevOps validation was queued or deduplicated'
            }
            @($results[3..5].Outcome | Select-Object -Unique) | Should -Be @('queued')
            @($results[3..5].PullRequestNumber | Select-Object -Unique) | Should -Be @(124)
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 3 -Exactly -ParameterFilter {
                $PullRequestNumber -eq 124
            }
            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly -ParameterFilter {
                $PullRequestNumber -eq 123
            }
            Should -Invoke Invoke-AzdoPipelineQueue -ModuleName QueueCiFixAzdoValidationTest -Times 3 -Exactly -ParameterFilter {
                $Context.PullRequestNumber -eq 124
            }
            Should -Invoke Invoke-AzdoPipelineQueue -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly -ParameterFilter {
                $Context.PullRequestNumber -eq 123
            }
            Should -Invoke Write-CiFixJobSummary -ModuleName QueueCiFixAzdoValidationTest -Times 2 -Exactly
        }

        It 'rejects an empty token before verified work can make any Azure request' {
            Mock Get-AzdoDuplicateBuild {
                throw 'Azure duplicate lookup must not run without authentication.'
            } -ModuleName QueueCiFixAzdoValidationTest
            Mock Invoke-AzdoPipelineQueue {
                throw 'Azure queue POST must not run without authentication.'
            } -ModuleName QueueCiFixAzdoValidationTest

            {
                Invoke-QueueTestCiFixQueueWork `
                    -Contexts @($script:queueContext) `
                    -AuthToken ''
            } | Should -Throw '*authentication is required before processing verified PR #123*'

            Should -Invoke Get-AzdoDuplicateBuild -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
            Should -Invoke Invoke-AzdoPipelineQueue -ModuleName QueueCiFixAzdoValidationTest -Times 0 -Exactly
        }
    }
}

Describe 'event payload validation' {
    It 'uses an event-neutral error for a missing payload file' {
        $missingEventPath = Join-Path $TestDrive 'missing-event.json'

        $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
            -EventPath $missingEventPath `
            -Repository dotnet/maui `
            -EventName workflow_run `
            -DryRun 2>&1

        $LASTEXITCODE | Should -Not -Be 0
        $output -join [Environment]::NewLine |
            Should -Match 'GITHUB_EVENT_PATH must identify a supported GitHub event payload file\.'
    }
}

Describe 'entrypoint verification failure routing' {
    BeforeEach {
        $script:oldStepSummary = $env:GITHUB_STEP_SUMMARY
        Mock Start-Sleep {}
    }

    AfterEach {
        $env:GITHUB_STEP_SUMMARY = $script:oldStepSummary
    }

    It 'retains one unverifiable PR while producing all dry-run payloads for the next verified PR' {
        $eventPath = Join-Path $TestDrive 'mixed-event.json'
        $fixturePath = Join-Path $TestDrive 'mixed-fixture.json'
        $stdoutPath = Join-Path $TestDrive 'mixed-stdout.json'
        $summaryPath = Join-Path $TestDrive 'mixed-summary.md'
        $env:GITHUB_STEP_SUMMARY = $summaryPath
        New-TestEvent | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath

        $unverifiedPullRequest = New-TestPullRequest -Number 123
        $verifiedPullRequest = New-TestPullRequest `
            -Number 124 `
            -BaseRef net11.0 `
            -HeadRef ci-fix/issue-124 `
            -Title '[ci-fix-net11] Repair CI (refs #124)' `
            -HeadSha '3333333333333333333333333333333333333333' `
            -MergeSha '4444444444444444444444444444444444444444'
        $fixture = New-TestPullRequestFixture -PullRequests @(
            $unverifiedPullRequest,
            $verifiedPullRequest
        )
        $fixture.commits.'2222222222222222222222222222222222222222' = New-TestMergeCommit `
            -MergeSha '2222222222222222222222222222222222222222' `
            -HeadSha '5555555555555555555555555555555555555555'
        $fixture | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $fixturePath

        {
            & $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $fixturePath `
                -DryRun > $stdoutPath
        } | Should -Throw '*3 of 6 Azure DevOps validation pipelines failed before dry-run queue payload generation*'

        $results = @(Get-Content -Raw -LiteralPath $stdoutPath | ConvertFrom-Json -Depth 20)
        $results.Count | Should -Be 6
        @($results[0..2].PullRequestNumber | Select-Object -Unique) | Should -Be @(123)
        @($results[0..2].Outcome | Select-Object -Unique) | Should -Be @('failed')
        @($results[3..5].PullRequestNumber | Select-Object -Unique) | Should -Be @(124)
        @($results[3..5].Outcome | Select-Object -Unique) | Should -Be @('dry-run')
        $summary = Get-Content -Raw -LiteralPath $summaryPath
        $summary | Should -Match 'dotnet/maui#123'
        $summary | Should -Match 'dotnet/maui#124'
        Should -Invoke Start-Sleep -Times 3 -Exactly
    }

    It 'retains all-unverifiable results without attempting OIDC or Azure work' {
        $eventPath = Join-Path $TestDrive 'unverifiable-event.json'
        $fixturePath = Join-Path $TestDrive 'unverifiable-fixture.json'
        $stdoutPath = Join-Path $TestDrive 'unverifiable-stdout.json'
        $summaryPath = Join-Path $TestDrive 'unverifiable-summary.md'
        $env:GITHUB_STEP_SUMMARY = $summaryPath
        New-TestEvent | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath

        $pullRequest = New-TestPullRequest
        $fixture = New-TestPullRequestFixture -PullRequests @($pullRequest)
        $fixture.commits.'2222222222222222222222222222222222222222' = New-TestMergeCommit `
            -MergeSha '2222222222222222222222222222222222222222' `
            -HeadSha '5555555555555555555555555555555555555555'
        $fixture | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $fixturePath

        {
            & $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $fixturePath `
                -DryRun > $stdoutPath
        } | Should -Throw '*3 of 3 Azure DevOps validation pipelines failed before dry-run queue payload generation*'

        $results = @(Get-Content -Raw -LiteralPath $stdoutPath | ConvertFrom-Json -Depth 20)
        $results.Count | Should -Be 3
        @($results.Outcome | Select-Object -Unique) | Should -Be @('failed')
        foreach ($result in $results) {
            $result.Error | Should -Match 'did not obtain a verified test merge'
            $result.Error | Should -Match 'No Azure DevOps validation was queued or deduplicated'
        }
        (Get-Content -Raw -LiteralPath $summaryPath) | Should -Match 'dotnet/maui#123'
        Should -Invoke Start-Sleep -Times 3 -Exactly
    }
}

Describe 'full-script offline reconciliation' {
    It 'reconciles multiple realistic workflow_run PRs and ignores unrelated PRs' {
        $eventPath = [System.IO.Path]::GetTempFileName()
        $pullRequestsPath = [System.IO.Path]::GetTempFileName()
        try {
            [pscustomobject]@{
                action = 'completed'
                repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
                workflow_run = [pscustomobject]@{
                    name = 'CI Failure Fixer (main)'
                    path = '.github/workflows/ci-status-fix.lock.yml'
                    head_branch = 'main'
                    head_repository = [pscustomobject]@{ full_name = 'dotnet/maui' }
                }
            } | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath

            $fixturePullRequests = @(
                (New-TestPullRequest -Number 123),
                (New-TestPullRequest `
                    -Number 124 `
                    -BaseRef net11.0 `
                    -HeadRef ci-fix/issue-124 `
                    -Title '[ci-fix-net11] Repair CI (refs #124)' `
                    -HeadSha '3333333333333333333333333333333333333333' `
                    -MergeSha '4444444444444444444444444444444444444444'),
                (New-TestPullRequest -Number 125 -Author attacker -HeadRef feature/unrelated)
            )
            New-TestPullRequestFixture -PullRequests $fixturePullRequests |
                ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $pullRequestsPath

            $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName workflow_run `
                -PullRequestsFixturePath $pullRequestsPath `
                -DryRun

            $LASTEXITCODE | Should -Be 0
            $results = @($output -join [Environment]::NewLine | ConvertFrom-Json -Depth 20)
            $results.Count | Should -Be 6
            @($results.PullRequestNumber | Sort-Object -Unique) | Should -Be @(123, 124)
            @($results.Request.triggerInfo.'pr.targetBranch' | Sort-Object -Unique) | Should -Be @('main', 'net11.0')
        }
        finally {
            Remove-Item -LiteralPath $eventPath, $pullRequestsPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'makes a surviving pull_request_target event reconcile another eligible PR head' {
        $eventPath = [System.IO.Path]::GetTempFileName()
        $pullRequestsPath = [System.IO.Path]::GetTempFileName()
        try {
            New-TestEvent | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath
            $fixturePullRequests = @(
                (New-TestPullRequest -Number 123),
                (New-TestPullRequest `
                    -Number 124 `
                    -BaseRef net11.0 `
                    -HeadRef ci-fix/issue-124 `
                    -Title '[ci-fix-net11] Repair CI (refs #124)' `
                    -HeadSha '3333333333333333333333333333333333333333' `
                    -MergeSha '4444444444444444444444444444444444444444')
            )
            New-TestPullRequestFixture -PullRequests $fixturePullRequests |
                ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $pullRequestsPath

            $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $pullRequestsPath `
                -DryRun

            $LASTEXITCODE | Should -Be 0
            $results = @($output -join [Environment]::NewLine | ConvertFrom-Json -Depth 20)
            $results.Count | Should -Be 6
            @($results.PullRequestNumber | Sort-Object -Unique) | Should -Be @(123, 124)
        }
        finally {
            Remove-Item -LiteralPath $eventPath, $pullRequestsPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'makes an unrelated surviving pull_request_target event reconcile eligible heads' {
        $eventPath = [System.IO.Path]::GetTempFileName()
        $pullRequestsPath = [System.IO.Path]::GetTempFileName()
        try {
            New-TestEvent `
                -Author contributor `
                -HeadRef feature/unrelated `
                -Title 'Unrelated PR' `
                -Labels @() |
                ConvertTo-Json -Depth 10 |
                Set-Content -LiteralPath $eventPath
            $fixturePullRequests = @(
                (New-TestPullRequest -Number 123),
                (New-TestPullRequest -Number 125 -Author contributor -HeadRef feature/unrelated -Labels @())
            )
            New-TestPullRequestFixture -PullRequests $fixturePullRequests |
                ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $pullRequestsPath

            $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $pullRequestsPath `
                -DryRun

            $LASTEXITCODE | Should -Be 0
            $results = @($output -join [Environment]::NewLine | ConvertFrom-Json -Depth 20)
            $results.Count | Should -Be 3
            @($results.PullRequestNumber | Sort-Object -Unique) | Should -Be @(123)
        }
        finally {
            Remove-Item -LiteralPath $eventPath, $pullRequestsPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'does not queue a stale event when live eligibility was revoked' {
        $eventPath = [System.IO.Path]::GetTempFileName()
        $pullRequestsPath = [System.IO.Path]::GetTempFileName()
        try {
            New-TestEvent | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath
            $fixturePullRequests = @(
                New-TestPullRequest -Labels @()
            )
            New-TestPullRequestFixture -PullRequests $fixturePullRequests |
                ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $pullRequestsPath

            $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $pullRequestsPath `
                -DryRun

            $LASTEXITCODE | Should -Be 0
            @($output) | Should -Be @('No eligible automated CI-fix pull request heads require reconciliation.')
        }
        finally {
            Remove-Item -LiteralPath $eventPath, $pullRequestsPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'uses the live head and merge SHAs instead of a stale event snapshot' {
        $eventPath = [System.IO.Path]::GetTempFileName()
        $pullRequestsPath = [System.IO.Path]::GetTempFileName()
        try {
            New-TestEvent | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath
            $fixturePullRequests = @(
                New-TestPullRequest `
                    -HeadSha '3333333333333333333333333333333333333333' `
                    -MergeSha '4444444444444444444444444444444444444444'
            )
            New-TestPullRequestFixture -PullRequests $fixturePullRequests |
                ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $pullRequestsPath

            $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $pullRequestsPath `
                -DryRun

            $LASTEXITCODE | Should -Be 0
            $results = @($output -join [Environment]::NewLine | ConvertFrom-Json -Depth 20)
            $results.Count | Should -Be 3
            @($results.Request.triggerInfo.'pr.sourceSha' | Sort-Object -Unique) |
                Should -Be @('3333333333333333333333333333333333333333')
            @($results.Request.sourceVersion | Sort-Object -Unique) | Should -Be @('4444444444444444444444444444444444444444')
        }
        finally {
            Remove-Item -LiteralPath $eventPath, $pullRequestsPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'refreshes a stale list merge and queues only the verified current head pair from fixture metadata' {
        $eventPath = [System.IO.Path]::GetTempFileName()
        $pullRequestsPath = [System.IO.Path]::GetTempFileName()
        try {
            New-TestEvent | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $eventPath
            $listPullRequest = New-TestPullRequest `
                -HeadSha '1111111111111111111111111111111111111111' `
                -MergeSha '2222222222222222222222222222222222222222'
            $detailPullRequest = New-TestPullRequest `
                -HeadSha '3333333333333333333333333333333333333333' `
                -MergeSha '4444444444444444444444444444444444444444'
            [pscustomobject]@{
                pullRequests = @($listPullRequest)
                pullRequestDetails = [pscustomobject]@{
                    '123' = $detailPullRequest
                }
                commits = [pscustomobject]@{
                    '4444444444444444444444444444444444444444' = New-TestMergeCommit `
                        -MergeSha '4444444444444444444444444444444444444444' `
                        -HeadSha '3333333333333333333333333333333333333333'
                }
            } | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $pullRequestsPath

            $output = & pwsh -NoLogo -NoProfile -File $scriptPath `
                -EventPath $eventPath `
                -Repository dotnet/maui `
                -EventName pull_request_target `
                -PullRequestsFixturePath $pullRequestsPath `
                -DryRun

            $LASTEXITCODE | Should -Be 0
            $results = @($output -join [Environment]::NewLine | ConvertFrom-Json -Depth 20)
            $results.Count | Should -Be 3
            @($results.Request.triggerInfo.'pr.sourceSha' | Sort-Object -Unique) |
                Should -Be @('3333333333333333333333333333333333333333')
            @($results.Request.sourceVersion | Sort-Object -Unique) |
                Should -Be @('4444444444444444444444444444444444444444')
        }
        finally {
            Remove-Item -LiteralPath $eventPath, $pullRequestsPath -Force -ErrorAction SilentlyContinue
        }
    }
}
