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
}

Describe 'effective recursive revert state' {
    It 'keeps an unreverted fix active' {
        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequestNumbers @(100) `
                -MergedRevertPullRequests @()
        ).Count | Should -Be 0
    }

    It 'excludes a fix after one active revert' {
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body 'Reverts dotnet/maui#100'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequestNumbers @(100) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'reinstates a fix after its revert is itself reverted' {
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert leak fix' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 300 -Title 'Revert the revert' -Body 'Reverts dotnet/maui#200'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequestNumbers @(100) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }

    It 'handles a deeper odd effective chain' {
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 300 -Title 'Revert A again' -Body 'Reverts dotnet/maui#200'
            New-LeakPr -Number 400 -Title 'Revert A re-revert' -Body 'Reverts dotnet/maui#300'
        )

        Get-EffectiveRevertedPullRequestNumbers `
            -Repository 'dotnet/maui' `
            -FixPullRequestNumbers @(100) `
            -MergedRevertPullRequests $reverts |
            Should -Be @(100)
    }

    It 'combines multiple active sibling reverts by parity' {
        $reverts = @(
            New-LeakPr -Number 200 -Title 'Revert A' -Body 'Reverts dotnet/maui#100'
            New-LeakPr -Number 201 -Title 'Revert B' -Body 'Reverts dotnet/maui#100'
        )

        @(
            Get-EffectiveRevertedPullRequestNumbers `
                -Repository 'dotnet/maui' `
                -FixPullRequestNumbers @(100) `
                -MergedRevertPullRequests $reverts
        ).Count | Should -Be 0
    }
}

Describe 'workflow enforcement boundary' {
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
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:LASTEXITCODE = $global:mockGhExitCode
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
            Remove-Variable mockMerged, mockReverts, mockOpen, mockGhExitCode -Scope Global -ErrorAction SilentlyContinue
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

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*failed with exit code 1*'
        }

        It 'allows a live same-API match only after a persisted mechanism decision' {
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
            } | Should -Not -Throw
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
}
