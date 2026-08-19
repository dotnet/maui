#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    Import-Module (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1') -Force

    function New-LeakPr {
        param(
            [int]$Number,
            [string]$Title,
            [string]$Body = '',
            [string]$Base = 'main',
            [bool]$Merged = $true
        )

        [pscustomobject]@{
            number      = $Number
            title       = $Title
            body        = $Body
            baseRefName = $Base
            mergedAt    = if ($Merged) { '2026-08-10T00:00:00Z' } else { $null }
            url         = "https://github.com/dotnet/maui/pull/$Number"
        }
    }
}

Describe 'native gh invocation' {
    It 'pins native-command errors to the structured exit-code path' {
        $module = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1')
        $helper = [regex]::Match(
            $module,
            '(?s)function Invoke-LeakGhJson \{.*?\n\}'
        ).Value

        $preferenceIndex = $helper.IndexOf('$PSNativeCommandUseErrorActionPreference = $false')
        $invokeIndex = $helper.IndexOf('$output = & gh @Arguments 2>&1')

        $preferenceIndex | Should -BeGreaterOrEqual 0
        $invokeIndex | Should -BeGreaterOrEqual 0
        $preferenceIndex | Should -BeLessThan $invokeIndex
    }
}

Describe 'fresh-shell de-dup state' {
    It 'fails closed when persisted identity does not match the requested PR' {
        $state = [pscustomobject]@{
            issue_number = 42
            api = 'Picker.ItemsSource'
            repository = 'dotnet/maui'
            different_mechanism_prs = @()
        }

        {
            Assert-LeakDedupState `
                -State $state `
                -IssueNumber 43 `
                -Api 'Picker.ItemsSource' `
                -Repository 'dotnet/maui'
        } | Should -Throw '*does not match PR issue*'
    }

    It 'rejects malformed different-mechanism decisions' {
        $state = [pscustomobject]@{
            issue_number = 42
            api = 'Picker.ItemsSource'
            repository = 'dotnet/maui'
            different_mechanism_prs = @(
                [pscustomobject]@{ number = 100; basis = 'too short' }
            )
        }

        {
            Assert-LeakDedupState `
                -State $state `
                -IssueNumber 42 `
                -Api 'Picker.ItemsSource' `
                -Repository 'dotnet/maui'
        } | Should -Throw '*lacks a specific basis*'
    }

    It 'rejects a comparison basis that cannot fit the structured body disclosure' {
        $state = [pscustomobject]@{
            issue_number = 42
            api = 'Picker.ItemsSource'
            repository = 'dotnet/maui'
            different_mechanism_prs = @(
                [pscustomobject]@{
                    number = 100
                    basis = "Existing teardown path|different reset path"
                }
            )
        }

        {
            Assert-LeakDedupState `
                -State $state `
                -IssueNumber 42 `
                -Api 'Picker.ItemsSource' `
                -Repository 'dotnet/maui'
        } | Should -Throw '*invalid basis format*'
    }
}

Describe 'canonical leak API title parsing' {
    It 'extracts the API only from the anchored leak-scan title position' {
        Get-CanonicalLeakApi `
            -Title '[leak-scan] Microsoft.Maui.Controls.Picker.ItemsSource — collection retention' |
            Should -Be 'Picker.ItemsSource'
    }

    It 'extracts the API only from the anchored leak-fix title position' {
        Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.Picker.ItemsSource memory leak' |
            Should -Be 'Picker.ItemsSource'
    }

    It 'rejects a URL before an otherwise valid API' {
        (Get-CanonicalLeakApi `
                -Title '[leak-fix] Investigate https://github.com/dotnet/maui/issues/123 for Picker.ItemsSource') |
            Should -BeNullOrEmpty
    }

    It 'rejects an earlier namespace token in a malformed title' {
        (Get-CanonicalLeakApi `
                -Title '[leak-fix] Investigate Microsoft.Maui.Controls before Picker.ItemsSource') |
            Should -BeNullOrEmpty
    }

    It 'rejects tagged titles that do not follow the expected title grammar' {
        (Get-CanonicalLeakApi -Title '[leak-fix] Picker.ItemsSource memory leak') |
            Should -BeNullOrEmpty
        (Get-CanonicalLeakApi -Title '[leak-scan] Investigate Picker.ItemsSource retention') |
            Should -BeNullOrEmpty
    }
}

Describe 'mechanism-aware final duplicate gate' {
    It 'preserves an approved same-API different-mechanism exception' {
        $existing = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
            -Body 'Fixes #10'

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @() `
            -ApprovedDifferentMechanismPullRequests @(100)

        $result.Blocked | Should -BeFalse
    }

    It 'blocks a same-API PR that appeared after Step 3 until its mechanism is compared' {
        $newOpen = New-LeakPr `
            -Number 101 `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.GradientBrush.GradientStops reset leak' `
            -Merged $false

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @() `
            -OpenPullRequests @($newOpen) `
            -ApprovedDifferentMechanismPullRequests @()

        $result.Blocked | Should -BeTrue
        $result.UnapprovedApiMatches.number | Should -Be 101
    }

    It 'ignores a same-API open PR targeting an unrelated release branch' {
        $releaseOpen = New-LeakPr `
            -Number 103 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
            -Body "Fixes #20`nRefs: dotnet/maui#20" `
            -Base 'release/10.0.1xx-sr9' `
            -Merged $false

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @() `
            -OpenPullRequests @($releaseOpen) `
            -ApprovedDifferentMechanismPullRequests @()

        $result.Blocked | Should -BeFalse
        $result.DirectMatches.Count | Should -Be 0
        $result.ApiMatches.Count | Should -Be 0
    }

    It 'always blocks a direct issue reference even when the PR was approved by API' {
        $direct = New-LeakPr `
            -Number 102 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
            -Body "Fixes #20`nRefs: dotnet/maui#20"

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($direct) `
            -OpenPullRequests @() `
            -ApprovedDifferentMechanismPullRequests @(102)

        $result.Blocked | Should -BeTrue
        $result.DirectMatches.number | Should -Be 102
    }

    It 'does not treat an effectively reverted merged fix as a duplicate' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Restore collection cleanup' `
            -Body "Fixes #20`nRefs: dotnet/maui#20"
        $revert = New-LeakPr `
            -Number 200 `
            -Title 'Revert leak fix' `
            -Body 'Reverts dotnet/maui#100'

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($fix) `
            -OpenPullRequests @() `
            -MergedRevertPullRequests @($revert)

        $result.Blocked | Should -BeFalse
        $result.EffectivelyReverted | Should -Be @(100)
    }

    It 'blocks a cyclic fix conservatively while still resolving unrelated candidates' {
        $cyclicFix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Body 'Fixes #10'
        $unrelatedFix = New-LeakPr `
            -Number 110 `
            -Title '[leak-fix] Fix ListView.RefreshCommand leak' `
            -Body 'Fixes #11'
        $reverts = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Revert Picker fix and cyclic peer' `
                -Body "Reverts #100`nReverts #300"
            New-LeakPr `
                -Number 300 `
                -Title 'Revert cyclic peer' `
                -Body 'Reverts #200'
            New-LeakPr `
                -Number 210 `
                -Title 'Revert unrelated fix' `
                -Body 'Reverts #110'
        )

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'Picker.ItemsSource' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($cyclicFix, $unrelatedFix) `
            -OpenPullRequests @() `
            -MergedRevertPullRequests $reverts

        $result.Blocked | Should -BeTrue
        $result.UnapprovedApiMatches.number | Should -Be 100
        $result.EffectivelyReverted | Should -Be @(110)
    }

    It 'honors a definite terminal reverter alongside a cycle-entangled sibling' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Body 'Fixes #10'
        $reverts = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Cycle-entangled sibling' `
                -Body "Reverts #100`nReverts #300"
            New-LeakPr `
                -Number 300 `
                -Title 'Cycle peer' `
                -Body 'Reverts #200'
            New-LeakPr `
                -Number 201 `
                -Title 'Definite terminal sibling' `
                -Body 'Reverts #100'
        )

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'Picker.ItemsSource' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($fix) `
            -OpenPullRequests @() `
            -MergedRevertPullRequests $reverts

        $result.Blocked | Should -BeFalse
        $result.EffectivelyReverted | Should -Be @(100)
    }
}

Describe 'effective recursive revert state' {
    It 'keeps an unreverted fix active' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @()
        ).Count | Should -Be 0
    }

    It 'excludes a fix after one active revert' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body 'Reverts dotnet/maui#100'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'accepts a repository-local revert reference' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body 'Reverts #100'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'accepts markdown formatting around a repository-local revert reference' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body '> - **Reverts #100**'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'rejects a revert reference qualified to another repository' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert unrelated fix' -Body 'Reverts dotnet/runtime#100'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'reinstates a fix after its revert is itself reverted' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 300 -Title 'Revert the revert' -Body 'Reverts dotnet/maui#200'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'handles a deeper odd effective chain' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 300 -Title 'Revert A again' -Body 'Reverts dotnet/maui#200'
            New-LeakPr -Number 400 -Title 'Revert A re-revert' -Body 'Reverts dotnet/maui#300'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'keeps a fix reverted when multiple independent sibling reverts remain active' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 201 -Title 'Revert B' -Body 'Reverts dotnet/maui#100'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests $reverts
        ) | Should -Be @(100)
    }

    It 'keeps a fix reverted while any independent sibling revert remains active' {
        $fix = New-LeakPr -Number 100 -Title '[leak-fix] Fix Picker.ItemsSource leak'
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 201 -Title 'Revert B' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 300 -Title 'Restore only A' -Body 'Reverts dotnet/maui#200'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($fix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'ignores a servicing-branch revert of a main fix' {
        $fix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Base main
        $releaseRevert = New-LeakPr `
            -Number 200 `
            -Title 'Revert leak fix for servicing' `
            -Body 'Reverts dotnet/maui#100' `
            -Base 'release/10.0.1xx-sr9'

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequests @($fix) `
                -MergedRevertPullRequests @($releaseRevert)
        ).Count | Should -Be 0
    }

    It 'scopes main and inflight revert chains independently' {
        $mainFix = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix Picker.ItemsSource leak' `
            -Base main
        $inflightFix = New-LeakPr `
            -Number 110 `
            -Title '[leak-fix] Fix ListView.RefreshCommand leak' `
            -Base 'inflight/current'
        $reverts = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Revert main fix' `
                -Body 'Reverts dotnet/maui#100' `
                -Base main
            New-LeakPr `
                -Number 210 `
                -Title 'Unrelated main revert of inflight PR number' `
                -Body 'Reverts dotnet/maui#110' `
                -Base main
            New-LeakPr `
                -Number 220 `
                -Title 'Revert inflight fix' `
                -Body 'Reverts dotnet/maui#110' `
                -Base 'inflight/current'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequests @($mainFix, $inflightFix) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100, 110)
    }
}

Describe 'workflow enforcement boundary' {
    It 'fails closed when the early merged-fix search reaches the GitHub Search API ceiling' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw
        $stepStart = $workflow.IndexOf('# (a) Exact [leak-fix] PRs already MERGED')
        $stepEnd = $workflow.IndexOf('# Canonicalize every merged PR title', $stepStart)
        $step = $workflow.Substring($stepStart, $stepEnd - $stepStart)

        $rawWrite = $step.IndexOf('> /tmp/gh-aw/agent/merged-leak-fix-prs-raw.json')
        $ceilingCheck = $step.IndexOf('if test "$MERGED_RAW_COUNT" -ge 1000')
        $filteredWrite = $step.IndexOf('> /tmp/gh-aw/agent/merged-leak-fix-prs.json')

        $step | Should -Match '--state merged --limit 1000'
        ($rawWrite -ge 0) | Should -BeTrue
        ($ceilingCheck -gt $rawWrite) | Should -BeTrue
        ($filteredWrite -gt $ceilingCheck) | Should -BeTrue
    }

    It 'uses the full Search API window and fails closed for open leak-scan issue de-dup' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw
        $stepStart = $workflow.IndexOf("# This workflow's own open [leak-scan] issues")
        $stepEnd = $workflow.IndexOf('# Exact [leak-fix] PRs already MERGED', $stepStart)
        $step = $workflow.Substring($stepStart, $stepEnd - $stepStart)

        $rawWrite = $step.IndexOf('> /tmp/gh-aw/agent/my-open-leakscan.json')
        $ceilingCheck = $step.IndexOf('if test "$OPEN_LEAKSCAN_COUNT" -ge 1000')
        $dedupRead = $step.IndexOf("jq -r '.[].title")

        $step | Should -Match '--state open --label agentic-workflows --limit 1000'
        ($rawWrite -ge 0) | Should -BeTrue
        ($ceilingCheck -gt $rawWrite) | Should -BeTrue
        ($dedupRead -gt $ceilingCheck) | Should -BeTrue
    }

    It 'wires the final check into safe-output steps rather than prompt-only enforcement' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw

        $workflow | Should -Match '(?s)safe-outputs:.*steps:.*Assert-LeakFixSafeOutputGate\.ps1'
        $workflow | Should -Match 'dedup-state\.json'
        $workflow | Should -Match 'github\.event\.repository\.default_branch'
        $workflow | Should -Match 'RUNNER_TEMP/leak-fix-safe-output'
        $workflow | Should -Not -Match 'run: \.github/scripts/Assert-LeakFixSafeOutputGate\.ps1'
        $workflow | Should -Match 'refusing unsupported empty-API de-dup before build/test work'
        ([regex]::Matches(
            $workflow,
            'select\(\.baseRefName == "main" or \.baseRefName == "inflight/current"\)'
        )).Count | Should -BeGreaterOrEqual 3
    }

    It 'wires a trusted final live refresh into the hunter safe-output boundary' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw
        $lock = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.lock.yml') -Raw

        $workflow | Should -Match '(?s)safe-outputs:.*steps:.*Assert-LeakHunterSafeOutputGate\.ps1.*create-issue:'
        $workflow | Should -Match '(?s)jobs:\s+safe_outputs:\s+permissions:\s+pull-requests: read'
        $workflow | Should -Match 'github\.event\.repository\.default_branch'
        $workflow | Should -Match 'GITHUB_WORKSPACE.*trusted-leak-hunter'
        $workflow | Should -Match 'persist-credentials: false'
        $workflow | Should -Not -Match 'run: \.github/scripts/Assert-LeakHunterSafeOutputGate\.ps1'
        $workflow | Should -Match "contains\(needs\.agent\.outputs\.output_types, 'create_issue'\)"
        $lock | Should -Match '(?ms)^  safe_outputs:.*?^    permissions:.*?^      pull-requests: read$'
    }

    It 'documents recursive any-active-reverter semantics' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw

        $workflow | Should -Match 'any active same-branch direct reverter'
        $workflow | Should -Match 'independent sibling reverts\s+never cancel each other'
        $workflow | Should -Not -Match 'combined by parity|combine by parity'
    }

    It 'keeps hunter batch instructions aligned with the canonical-API gate' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw

        $workflow | Should -Match 'at most\s+one output per canonical rooting API in the current batch'
        $workflow | Should -Match 'defer the others to a later run'
        $workflow | Should -Not -Match 'distinct mechanisms on one API are separate leaks'
    }

    It 'uses the shared anchored API parser in every workflow parser path' {
        $hunter = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md') -Raw
        $fixer = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw

        ([regex]::Matches($hunter, 'Get-CanonicalLeakApi\.ps1')).Count | Should -Be 2
        ([regex]::Matches($fixer, 'Get-CanonicalLeakApi\.ps1')).Count | Should -Be 6
        $hunter | Should -Not -Match 'awk.*A-Za-z_'
        $fixer | Should -Not -Match 'awk.*A-Za-z_'
    }

    Context 'safe-output gate script' {
        BeforeEach {
            $script:agentOutput = Join-Path $TestDrive 'agent_output.json'
            $script:stateDirectory = Join-Path $TestDrive 'agent'
            New-Item -ItemType Directory -Path $script:stateDirectory -Force | Out-Null
            @{
                items = @(
                    @{
                        type = 'create_pull_request'
                        title = '[leak-fix] Fix GradientBrush.GradientStops reset leak'
                        body = "Fixes #20`nRefs: dotnet/maui#20"
                        branch = 'leak-fix/issue-20'
                    }
                )
            } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @()
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')

            $global:mockMerged = @()
            $global:mockReverts = @()
            $global:mockOpen = @()
            $global:mockGhExitCode = 0
            $global:mockGhStderr = ''
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:LASTEXITCODE = $global:mockGhExitCode
                if (-not [string]::IsNullOrWhiteSpace($global:mockGhStderr)) {
                    Write-Error $global:mockGhStderr -ErrorAction Continue
                }
                if ($global:mockGhExitCode -ne 0) {
                    Write-Output 'mock gh failure'
                    return
                }
                $stateIndex = [Array]::IndexOf($GhArgs, '--state')
                $state = $GhArgs[$stateIndex + 1]
                $searchIndex = [Array]::IndexOf($GhArgs, '--search')
                $search = $GhArgs[$searchIndex + 1]
                if ($state -eq 'merged') {
                    if ($search -eq '"Revert" in:title') {
                        Write-Output (ConvertTo-Json -InputObject @($global:mockReverts) -Depth 5)
                    } else {
                        Write-Output (ConvertTo-Json -InputObject @($global:mockMerged) -Depth 5)
                    }
                } else {
                    Write-Output (ConvertTo-Json -InputObject @($global:mockOpen) -Depth 5)
                }
            }
        }

        AfterAll {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable mockMerged, mockReverts, mockOpen, mockGhExitCode, mockGhStderr `
                -Scope Global -ErrorAction SilentlyContinue
        }

        It 'rejects an untagged create-pull-request title' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title = 'Fix GradientBrush.GradientStops reset leak'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*must start with the literal*prefix*'
        }

        It 'accepts a tagged create-pull-request title' {
            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects a tagged title whose API is not in the expected position' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix] Investigate https://github.com/dotnet/maui/issues/20 for GradientBrush.GradientStops'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*Could not derive a canonical Type.Member*'
        }

        It 'accepts an additional exact-repository Refs citation for an API-match PR' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = "Fixes #20`nRefs: dotnet/maui#20`nRefs: dotnet/maui#501"
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'fails closed before mutation when live metadata has a direct issue match' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 500 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Body "Fixes #20`nRefs: dotnet/maui#20"
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*direct issue-reference match*'
        }

        It 'fails closed when the final GitHub fetch fails' {
            $global:mockGhExitCode = 1
            $global:mockGhStderr = "auth warning`n$([char]27)[31mred"

            $message = try {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
                throw 'Expected the gh failure to stop the gate.'
            } catch {
                $_.Exception.Message
            }

            $message | Should -Match 'failed with exit code 1'
            $message | Should -Match 'Output: auth warning'
            $message | Should -Not -Match "[`r`n$([char]27)]"
        }

        It 'parses successful JSON without mixing benign gh stderr into stdout' {
            $global:mockGhStderr = 'benign gh warning'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects a live same-API decision that is absent from the PR body' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 501 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            )
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @(
                    @{
                        number = 501
                        basis = 'Existing PR fixes detach teardown; this issue fixes Reset unsubscription.'
                    }
                )
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*structured same-API disclosure*#501*'
        }

        It 'allows a live same-API match when the body discloses the exact persisted basis' {
            $basis = 'Existing PR fixes detach teardown; this issue fixes Reset unsubscription.'
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 501 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            )
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @(
                    @{
                        number = 501
                        basis = $basis
                    }
                )
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @"
Fixes #20
Refs: dotnet/maui#20

## Same-API comparisons
Same-API comparison: dotnet/maui#501 | Different mechanism: $basis
"@
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects a same-API disclosure whose basis differs from persisted state' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 501 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            )
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @(
                    @{
                        number = 501
                        basis = 'Existing PR fixes detach teardown; this issue fixes Reset unsubscription.'
                    }
                )
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @'
Fixes #20
Refs: dotnet/maui#20
Same-API comparison: dotnet/maui#501 | Different mechanism: These mechanisms are definitely unrelated.
'@
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*does not match the persisted comparison basis*'
        }

        It 'allows a release-only open PR even when it directly references the issue' {
            $global:mockOpen = @(
                New-LeakPr `
                    -Number 502 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Body "Fixes #20`nRefs: dotnet/maui#20" `
                    -Base 'release/10.0.1xx-sr9' `
                    -Merged $false
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'allows a re-file after the matching merged fix was effectively reverted' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 503 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
                    -Body "Fixes #20`nRefs: dotnet/maui#20"
            )
            $global:mockReverts = @(
                New-LeakPr `
                    -Number 504 `
                    -Title 'Revert leak fix' `
                    -Body 'Reverts dotnet/maui#503'
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }
    }

    Context 'hunter safe-output gate script' {
        BeforeEach {
            $script:hunterAgentOutput = Join-Path $TestDrive 'hunter_agent_output.json'
            @{
                items = @(
                    @{
                        type = 'create_issue'
                        title = '[leak-scan] GradientBrush.GradientStops — reset leak'
                        body = 'AI-generated leak report'
                    }
                )
            } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:hunterAgentOutput

            $global:mockHunterOpenIssues = @()
            $global:mockHunterMerged = @()
            $global:mockHunterReverts = @()
            $global:mockHunterGhExitCode = 0
            $global:mockHunterGhStderr = ''
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:LASTEXITCODE = $global:mockHunterGhExitCode
                if (-not [string]::IsNullOrWhiteSpace($global:mockHunterGhStderr)) {
                    Write-Error $global:mockHunterGhStderr -ErrorAction Continue
                }
                if ($global:mockHunterGhExitCode -ne 0) {
                    Write-Output 'mock gh failure'
                    return
                }
                if ($GhArgs[0] -eq 'issue') {
                    Write-Output (ConvertTo-Json -InputObject @($global:mockHunterOpenIssues) -Depth 5)
                    return
                }
                $searchIndex = [Array]::IndexOf($GhArgs, '--search')
                $search = $GhArgs[$searchIndex + 1]
                if ($search -eq '"Revert" in:title') {
                    Write-Output (ConvertTo-Json -InputObject @($global:mockHunterReverts) -Depth 5)
                } else {
                    Write-Output (ConvertTo-Json -InputObject @($global:mockHunterMerged) -Depth 5)
                }
            }
        }

        AfterAll {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable mockHunterOpenIssues, mockHunterMerged, mockHunterReverts, `
                mockHunterGhExitCode, mockHunterGhStderr `
                -Scope Global -ErrorAction SilentlyContinue
        }

        It 'accepts issue emission when the final live refresh has no match' {
            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'rejects a malformed issue title instead of deriving a later API token' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan] Investigate Microsoft.Maui.Controls before GradientBrush.GradientStops'
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*Could not derive a canonical Type.Member*'
        }

        It 'rejects differently titled issues for the same canonical API in one output batch' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items += [pscustomobject]@{
                type = 'create_issue'
                title = '[leak-scan] GradientBrush.GradientStops — detach teardown leak'
                body = 'Second AI-generated leak report'
            }
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*same canonical API 'GradientBrush.GradientStops'*"
        }

        It 'blocks issue emission when a matching fix merged after the pre-agent snapshot' {
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 701 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak'
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*structured pull-request override*#701*'
        }

        It 'allows a distinct same-API mechanism with bounded human-visible evidence' {
            $basis = 'Existing PR fixes detach teardown; this issue proves Reset unsubscription.'
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 701 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak'
            )
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @"
AI-generated leak report

## Same-API comparisons
Same-API comparison: dotnet/maui#701 | Different mechanism: $basis
"@
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'requires a separate structured comparison for a same-API open issue' {
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 702
                    title = '[leak-scan] GradientBrush.GradientStops — teardown leak'
                    body = 'Existing scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/702'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*structured issue override*#702*'
        }

        It 'parses successful hunter JSON without mixing benign gh stderr into stdout' {
            $global:mockHunterGhStderr = 'benign gh warning'

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
        }

        It 'fails closed when a hunter gh query fails' {
            $global:mockHunterGhExitCode = 1

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*failed with exit code 1*'
        }
    }
}
