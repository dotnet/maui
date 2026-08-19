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

    Context 'bounded transient retries' {
        BeforeEach {
            $global:leakGhAttemptCount = 0
            $global:leakGhResponses = [System.Collections.Generic.Queue[object]]::new()
            function global:gh {
                param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
                $global:leakGhAttemptCount++
                $response = $global:leakGhResponses.Dequeue()
                $global:LASTEXITCODE = [int]$response.ExitCode
                if (-not [string]::IsNullOrWhiteSpace([string]$response.Stderr)) {
                    Write-Error ([string]$response.Stderr) -ErrorAction Continue
                }
                Write-Output ([string]$response.Stdout)
            }
        }

        AfterEach {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable leakGhAttemptCount, leakGhResponses `
                -Scope Global -ErrorAction SilentlyContinue
        }

        It 'retries a clearly transient failure and returns the later valid JSON' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 503: Service Unavailable'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"value":42}'
                })

            $result = Invoke-LeakGhJson `
                -Arguments @('api', 'test') `
                -RetryBaseDelaySeconds 0

            $result.value | Should -Be 42
            $global:leakGhAttemptCount | Should -Be 2
        }

        It 'fails closed after exhausting the bounded transient retry budget' {
            1..3 | ForEach-Object {
                $global:leakGhResponses.Enqueue([pscustomobject]@{
                        ExitCode = 1
                        Stderr = 'read: connection reset by peer'
                        Stdout = ''
                    })
            }

            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'test') `
                    -MaximumAttempts 3 `
                    -RetryBaseDelaySeconds 0
            } | Should -Throw '*failed with exit code 1 after 3 attempt(s)*'

            $global:leakGhAttemptCount | Should -Be 3
        }

        It 'does not retry a permanent gh failure' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 1
                    Stderr = 'HTTP 401: Bad credentials'
                    Stdout = ''
                })
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{"unexpected":true}'
                })

            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'test') `
                    -RetryBaseDelaySeconds 0
            } | Should -Throw '*failed with exit code 1 after 1 attempt(s)*'

            $global:leakGhAttemptCount | Should -Be 1
            $global:leakGhResponses.Count | Should -Be 1
        }

        It 'does not retry successful empty or invalid JSON responses' {
            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = ''
                })
            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'empty') `
                    -RetryBaseDelaySeconds 0
            } | Should -Throw '*returned an empty response*'
            $global:leakGhAttemptCount | Should -Be 1

            $global:leakGhResponses.Enqueue([pscustomobject]@{
                    ExitCode = 0
                    Stderr = ''
                    Stdout = '{'
                })
            {
                Invoke-LeakGhJson `
                    -Arguments @('api', 'invalid') `
                    -RetryBaseDelaySeconds 0
            } | Should -Throw '*returned invalid JSON*'
            $global:leakGhAttemptCount | Should -Be 2
        }
    }
}

Describe 'shared regular JSON file validation' {
    It 'reads a regular bounded JSON file' {
        $path = Join-Path $TestDrive 'valid.json'
        '{"value":42}' | Set-Content -LiteralPath $path

        (Read-RegularJsonFile -Path $path).value | Should -Be 42
    }

    It 'rejects missing, empty, oversized, and invalid JSON files' {
        {
            Read-RegularJsonFile -Path (Join-Path $TestDrive 'missing.json')
        } | Should -Throw '*Required JSON file is missing*'

        $emptyPath = Join-Path $TestDrive 'empty.json'
        Set-Content -LiteralPath $emptyPath -Value ''
        {
            Read-RegularJsonFile -Path $emptyPath
        } | Should -Throw '*empty or too large*'

        $oversizedPath = Join-Path $TestDrive 'oversized.json'
        Set-Content -LiteralPath $oversizedPath -Value ('x' * (1MB + 1)) -NoNewline
        {
            Read-RegularJsonFile -Path $oversizedPath
        } | Should -Throw '*empty or too large*'

        $invalidPath = Join-Path $TestDrive 'invalid.json'
        Set-Content -LiteralPath $invalidPath -Value '{'
        {
            Read-RegularJsonFile -Path $invalidPath
        } | Should -Throw '*Invalid JSON*'
    }

    It 'rejects a symbolic-link JSON file' {
        $target = Join-Path $TestDrive 'target.json'
        $link = Join-Path $TestDrive 'link.json'
        '{"value":42}' | Set-Content -LiteralPath $target
        New-Item -ItemType SymbolicLink -Path $link -Target $target | Out-Null

        {
            Read-RegularJsonFile -Path $link
        } | Should -Throw '*Refusing symbolic-link JSON file*'
    }

    It 'is defined only by the shared module used by both gates' {
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw

        (Get-Command Read-RegularJsonFile).ModuleName | Should -Be 'LeakWorkflowDedup'
        $fixGate | Should -Not -Match 'function Read-RegularJsonFile'
        $hunterGate | Should -Not -Match 'function Read-RegularJsonFile'
    }
}

Describe 'authoritative leak-fix branch scope' {
    It 'selects main and inflight/current as one scope while excluding release branches' {
        $selected = @(
            Select-LeakAuthoritativePullRequests `
                -PullRequests @(
                    (New-LeakPr -Number 41 -Title '[leak-fix] Fix First.Api leak' -Base 'main')
                    (New-LeakPr -Number 42 -Title '[leak-fix] Fix Second.Api leak' -Base 'inflight/current')
                    (New-LeakPr -Number 43 -Title '[leak-fix] Fix Third.Api leak' -Base 'release/10.0.1xx-sr9')
                ) `
                -Context 'test branch scope'
        )

        ($selected.number -join ',') | Should -Be '41,42'
    }

    It 'fails closed when baseRefName is missing or malformed' {
        $missing = [pscustomobject]@{
            number = 44
            title = '[leak-fix] Fix Missing.Api leak'
        }
        $malformed = New-LeakPr `
            -Number 45 `
            -Title '[leak-fix] Fix Malformed.Api leak' `
            -Base ' main '

        {
            Select-LeakAuthoritativePullRequests `
                -PullRequests @($missing) `
                -Context 'test branch scope'
        } | Should -Throw '*missing baseRefName*'
        {
            Select-LeakAuthoritativePullRequests `
                -PullRequests @($malformed) `
                -Context 'test branch scope'
        } | Should -Throw '*malformed baseRefName*'
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

    It 'rejects agent-authored different-mechanism overrides' {
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
        } | Should -Throw '*do not accept agent-authored different-mechanism overrides*'
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

    It 'accepts supported punctuation immediately after the anchored API' {
        @(
            '[leak-fix] Fix Picker.ItemsSource, clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource: clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource-clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource–clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource—clear stale subscriptions'
            '[leak-fix] Fix Picker.ItemsSource(clear stale subscriptions)'
            '[leak-fix] Fix Picker.ItemsSource.'
        ) | ForEach-Object {
            Get-CanonicalLeakApi -Title $_ | Should -Be 'Picker.ItemsSource'
        }
    }

    It 'preserves non-MAUI qualification to prevent namespace collisions' {
        $foo = Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Foo.Bar.CollectionView.ItemsSource leak'
        $baz = Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Baz.Qux.CollectionView.ItemsSource leak'

        $foo | Should -Be 'Foo.Bar.CollectionView.ItemsSource'
        $baz | Should -Be 'Baz.Qux.CollectionView.ItemsSource'
        $foo | Should -Not -Be $baz
    }

    It 'keeps short and legacy Microsoft.Maui-qualified keys stable' {
        Get-CanonicalLeakApi -Title '[leak-fix] Fix Picker.ItemsSource leak' |
            Should -Be 'Picker.ItemsSource'
        Get-CanonicalLeakApi `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.Picker.ItemsSource leak' |
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
        (Get-CanonicalLeakApi -Title '[leak-fix] Fix Picker.ItemsSource/Other retention') |
            Should -BeNullOrEmpty
    }

    It 'keeps legacy compatibility out of strict new-output parsing' {
        (Get-CanonicalLeakApi `
                -Title '[leak-scan] Shell BackButtonBehavior.Command leaks via ICommand') |
            Should -BeNullOrEmpty
        (Get-CanonicalLeakApi `
                -Title '[leak-fix] Fix Shell BackButtonBehavior.Command memory leak') |
            Should -BeNullOrEmpty
    }

    It 'recognizes the known Shell prefix only for existing issue and fix titles' {
        Get-CanonicalExistingLeakApi `
            -Title '[leak-scan] Shell BackButtonBehavior.Command leaks via ICommand' |
            Should -Be 'BackButtonBehavior.Command'
        Get-CanonicalExistingLeakApi `
            -Title '[leak-fix] Fix Shell BackButtonBehavior.Command memory leak' |
            Should -Be 'BackButtonBehavior.Command'
    }

    It 'does not scan URLs or arbitrary later identifiers in existing titles' {
        @(
            '[leak-scan] Investigate BackButtonBehavior.Command retention'
            '[leak-scan] Shell investigate BackButtonBehavior.Command retention'
            '[leak-scan] Shell https://github.com/dotnet/maui/issues/36345 BackButtonBehavior.Command'
            '[leak-fix] Fix Shell details at https://example.test/BackButtonBehavior.Command'
        ) | ForEach-Object {
            (Get-CanonicalExistingLeakApi -Title $_) | Should -BeNullOrEmpty
        }
    }
}

Describe 'trusted final duplicate gate' {
    It 'conservatively blocks same-API matches without an independent override' {
        $existing = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
            -Body 'Fixes #10'

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 100
    }

    It 'uses ordinal API identity so exact C# casing dedups while casing-only identifiers remain distinct' {
        $existing = New-LeakPr `
            -Number 100 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak'

        $exact = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()
        $caseVariant = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.gradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()

        $exact.Blocked | Should -BeTrue
        $caseVariant.Blocked | Should -BeFalse
    }

    It 'blocks the known legacy form when it appears on an existing fix' {
        $existing = New-LeakPr `
            -Number 104 `
            -Title '[leak-fix] Fix Shell BackButtonBehavior.Command memory leak'

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'BackButtonBehavior.Command' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($existing) `
            -OpenPullRequests @()

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 104
    }

    It 'blocks a same-API PR that appeared after Step 3' {
        $newOpen = New-LeakPr `
            -Number 101 `
            -Title '[leak-fix] Fix Microsoft.Maui.Controls.GradientBrush.GradientStops reset leak' `
            -Merged $false

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @() `
            -OpenPullRequests @($newOpen)

        $result.Blocked | Should -BeTrue
        $result.ApiMatches.number | Should -Be 101
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
            -OpenPullRequests @($releaseOpen)

        $result.Blocked | Should -BeFalse
        $result.DirectMatches.Count | Should -Be 0
        $result.ApiMatches.Count | Should -Be 0
    }

    It 'always blocks a direct issue reference' {
        $direct = New-LeakPr `
            -Number 102 `
            -Title '[leak-fix] Fix GradientBrush.GradientStops reset leak' `
            -Body "Fixes #20`nRefs: dotnet/maui#20"

        $result = Get-LeakFixFinalDedupResult `
            -IssueNumber 20 `
            -Api 'GradientBrush.GradientStops' `
            -Repository 'dotnet/maui' `
            -MergedPullRequests @($direct) `
            -OpenPullRequests @()

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
        $result.ApiMatches.number | Should -Be 100
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
    It 'memoizes ambiguous states at cycle and propagation return paths' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw

        ([regex]::Matches(
            $module,
            '\$memo\[\$PullRequestNumber\] = \$ambiguousState'
        )).Count | Should -Be 2
    }

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

Describe 'branch-scoped merged revert discovery' {
    BeforeEach {
        $script:discoveryCalls = [System.Collections.Generic.List[object]]::new()
        $script:discoveryRowsByBase = @{}
        function global:gh {
            param([Parameter(ValueFromRemainingArguments = $true)][string[]]$GhArgs)
            $global:LASTEXITCODE = 0
            $searchIndex = [Array]::IndexOf($GhArgs, '--search')
            $search = $GhArgs[$searchIndex + 1]
            $baseIndex = [Array]::IndexOf($GhArgs, '--base')
            $base = $GhArgs[$baseIndex + 1]
            $script:discoveryCalls.Add([pscustomobject]@{
                    Search = $search
                    Base = $base
                })
            $rows = if ($script:discoveryRowsByBase.ContainsKey($base)) {
                @($script:discoveryRowsByBase[$base])
            } else {
                @()
            }
            Write-Output (ConvertTo-Json -InputObject $rows -Depth 5)
        }
    }

    AfterAll {
        Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
    }

    It 'recursively discovers only explicit same-branch reverters of the target set' {
        $script:discoveryRowsByBase['main'] = @(
            New-LeakPr `
                -Number 200 `
                -Title 'Back out cleanup without Revert in the title' `
                -Body 'Reverts #100'
            New-LeakPr `
                -Number 201 `
                -Title 'Unrelated mention' `
                -Body 'Reverts were discussed for #100 but not performed'
            New-LeakPr `
                -Number 202 `
                -Title 'Other repository reference' `
                -Body 'Reverts other/repository#100'
            New-LeakPr `
                -Number 203 `
                -Title 'Wrong branch reference' `
                -Body 'Reverts #100' `
                -Base 'release/10.0.1xx-sr9'
            New-LeakPr `
                -Number 300 `
                -Title 'Restore prior behavior' `
                -Body 'Reverts dotnet/maui#200'
        )

        $result = @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @(
                    [pscustomobject]@{ number = 100; baseRefName = 'main' }
                )
        )

        $result.number | Should -Be @(200, 300)
        $script:discoveryCalls.Count | Should -Be 1
        $script:discoveryCalls[0].Search | Should -Be 'Reverts in:body'
        $script:discoveryCalls[0].Base | Should -Be 'main'
    }

    It 'fails closed when a branch-scoped snapshot reaches its result ceiling' {
        $script:discoveryRowsByBase['main'] = @(
            New-LeakPr -Number 200 -Title 'First' -Body 'Reverts #100'
            New-LeakPr -Number 201 -Title 'Second' -Body 'Reverts #100'
        )

        {
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @(
                    [pscustomobject]@{ number = 100; baseRefName = 'main' }
                ) `
                -SearchLimit 2
        } | Should -Throw "*Branch-scoped merged-revert search for 'main'*2-result ceiling*"
    }

    It 'keeps more than 30 initial seeds to one bounded branch snapshot query' {
        $targets = @(1000..1030 | ForEach-Object {
                [pscustomobject]@{ number = $_; baseRefName = 'main' }
            })

        $result = @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests $targets `
                -MaximumSearchQueries 1
        )

        $result.Count | Should -Be 0
        $script:discoveryCalls.Count | Should -Be 1
        $script:discoveryCalls[0].Search.Length | Should -BeLessOrEqual 256
    }

    It 'keeps more than 100 initial seeds and recursive reverters to one snapshot query' {
        $targets = @(1000..1100 | ForEach-Object {
                [pscustomobject]@{ number = $_; baseRefName = 'main' }
            })
        $script:discoveryRowsByBase['main'] = @(
            New-LeakPr -Number 2000 -Title 'Relevant revert' -Body 'Reverts #1000'
            New-LeakPr -Number 2001 -Title 'Recursive revert' -Body 'Reverts #2000'
        )

        $result = @(
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests $targets `
                -MaximumSearchQueries 1
        )

        $result.number | Should -Be @(2000, 2001)
        $script:discoveryCalls.Count | Should -Be 1
    }

    It 'fails closed before searching when distinct branches exceed the query budget' {
        {
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @(
                    [pscustomobject]@{ number = 100; baseRefName = 'main' }
                    [pscustomobject]@{
                        number = 101
                        baseRefName = 'inflight/current'
                    }
                ) `
                -MaximumSearchQueries 1
        } | Should -Throw '*requires 2 branch-scoped searches*1-query safety budget*'

        $script:discoveryCalls.Count | Should -Be 0
    }

    It 'fails closed before searching when the effective query exceeds its length ceiling' {
        {
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @(
                    [pscustomobject]@{
                        number = 100
                        baseRefName = ('long-branch-' + ('x' * 220))
                    }
                )
        } | Should -Throw '*exceeds GitHub*s 256-character Search API query ceiling*'

        $script:discoveryCalls.Count | Should -Be 0
    }

    It 'fails closed when recursive discovery exhausts the aggregate traversal budget' {
        $script:discoveryRowsByBase['main'] = @(
            New-LeakPr -Number 200 -Title 'First revert' -Body 'Reverts #100'
            New-LeakPr -Number 300 -Title 'Second revert' -Body 'Reverts #200'
            New-LeakPr -Number 400 -Title 'Third revert' -Body 'Reverts #300'
        )

        {
            Get-RelevantMergedLeakReverts `
                -Repository 'dotnet/maui' `
                -TargetPullRequests @(
                    [pscustomobject]@{ number = 100; baseRefName = 'main' }
                ) `
                -MaximumTraversalPullRequests 3
        } | Should -Throw '*exhausted the 3-PR aggregate traversal safety budget*'

        $script:discoveryCalls.Count | Should -Be 1
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

    It 'keeps source and trusted attempt-cap branch scope in parity' {
        $workflow = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw
        $gate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $stepStart = $workflow.IndexOf('# (d) Closed-unmerged attempts')
        $stepEnd = $workflow.IndexOf("`n" + '```', $stepStart)
        $step = $workflow.Substring($stepStart, $stepEnd - $stepStart)

        $step | Should -Match '--json number,title,body,baseRefName,mergedAt'
        $gate | Should -Match "'--json', 'number,title,body,baseRefName,mergedAt'"
        @($step, $gate) | ForEach-Object {
            $_ | Should -Match 'Select-LeakAuthoritativePullRequests'
            $_ | Should -Match 'one aggregate budget across both authoritative lanes'
        }
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
        ([regex]::Matches(
            $hunter,
            'Get-CanonicalLeakApi\.ps1 -Title "\$TITLE" -ExistingTitle'
        )).Count | Should -Be 2
        ([regex]::Matches(
            $fixer,
            'Get-CanonicalLeakApi\.ps1 -Title "\$TITLE" -ExistingTitle'
        )).Count | Should -Be 6
        $hunter | Should -Not -Match 'awk.*A-Za-z_'
        $fixer | Should -Not -Match 'awk.*A-Za-z_'
    }

    It 'allows pwsh for fixer agent bash calls' {
        $fixer = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../workflows/leak-fixer.md') -Raw

        $fixer | Should -Match '(?m)^  bash: \[[^\r\n]*"pwsh"\]$'
    }

    It 'defines shared rate-aware query and aggregate traversal budgets for every caller' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $wrapper = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Get-RelevantMergedLeakReverts.ps1'
        ) -Raw
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw

        $module | Should -Match '\$MaximumSearchQueries = 2'
        $module | Should -Match '\$MaximumTraversalPullRequests = 2000'
        @($wrapper, $fixGate, $hunterGate) | ForEach-Object {
            $_ | Should -Match 'Get-RelevantMergedLeakReverts'
            $_ | Should -Not -Match 'MaximumSearchQueries'
            $_ | Should -Not -Match 'MaximumTraversalPullRequests'
        }
    }

    It 'uses constant-size branch snapshots with exact local revert verification' {
        $module = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'LeakWorkflowDedup.psm1'
        ) -Raw
        $fixGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1'
        ) -Raw
        $hunterGate = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1'
        ) -Raw
        $hunter = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/daily-leak-hunter.md'
        ) -Raw
        $fixer = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../workflows/leak-fixer.md'
        ) -Raw

        $module | Should -Match "\`$searchQuery = 'Reverts in:body'"
        $module | Should -Match "'--base', \`$base"
        $module | Should -Match 'Get-LeakRevertTargets'
        $module | Should -Not -Match 'Reverts.*#\$\{targetNumber\}.*in:body'
        $fixGate | Should -Match 'Get-RelevantMergedLeakReverts'
        $hunterGate | Should -Match 'Get-RelevantMergedLeakReverts'
        $hunter | Should -Match 'Get-RelevantMergedLeakReverts\.ps1'
        $fixer | Should -Match 'Get-RelevantMergedLeakReverts\.ps1'
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
            $global:mockClosed = @()
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
                    if ($search -eq 'Reverts in:body') {
                        Write-Output (ConvertTo-Json -InputObject @($global:mockReverts) -Depth 5)
                    } else {
                        Write-Output (ConvertTo-Json -InputObject @($global:mockMerged) -Depth 5)
                    }
                } elseif ($state -eq 'closed') {
                    Write-Output (ConvertTo-Json -InputObject @($global:mockClosed) -Depth 5)
                } else {
                    Write-Output (ConvertTo-Json -InputObject @($global:mockOpen) -Depth 5)
                }
            }
        }

        AfterAll {
            Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
            Remove-Variable mockMerged, mockReverts, mockOpen, mockClosed, mockGhExitCode, mockGhStderr `
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

        It 'accepts supported punctuation after the canonical API' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix] Fix GradientBrush.GradientStops, clear reset subscriptions'
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

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

        It 'rejects the legacy form when an agent emits it as new PR output' {
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-fix] Fix Shell GradientBrush.GradientStops reset leak'
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

        It 'rejects an agent-authored different-mechanism state override' {
            @{
                issue_number = 20
                api = 'GradientBrush.GradientStops'
                repository = 'dotnet/maui'
                different_mechanism_prs = @(
                    @{
                        number = 501
                        basis = 'Agent-authored mechanism claim'
                    }
                )
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath (Join-Path $script:stateDirectory 'dedup-state.json')

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*do not accept agent-authored different-mechanism overrides*'
        }

        It 'blocks a live same-API match despite an agent-authored body disclosure' {
            $global:mockMerged = @(
                New-LeakPr `
                    -Number 501 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak' `
                    -Body 'Fixes #10'
            )
            $output = Get-Content -LiteralPath $script:agentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @"
Fixes #20
Refs: dotnet/maui#20

## Same-API comparisons
Same-API comparison: dotnet/maui#501 | Different mechanism: Agent-authored claim
"@
            $output | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:agentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*blocked PR creation*same-API match: 501*'
        }

        It 'aggregates main and inflight/current attempts while excluding release lanes' {
            $global:mockClosed = @(
                New-LeakPr -Number 601 -Title '[leak-fix] Fix Other.Api leak' `
                    -Body 'Fixes #20' -Merged $false
                New-LeakPr -Number 602 -Title '[leak-fix] Fix GradientBrush.GradientStops leak' `
                    -Body 'Fixes #10' -Base 'inflight/current' -Merged $false
                New-LeakPr -Number 603 -Title '[leak-fix] Fix Other.Api leak again' `
                    -Body 'Refs: dotnet/maui#20' -Merged $false
                New-LeakPr -Number 604 -Title '[leak-fix] Fix GradientBrush.GradientStops release leak' `
                    -Body 'Fixes #20' -Base 'release/10.0.1xx-sr9' -Merged $false
            )
            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*attempt-cap gate blocked PR creation: 3 closed-unmerged attempts*'
        }

        It 'fails closed when a closed attempt is missing baseRefName' {
            $global:mockClosed = @(
                [pscustomobject]@{
                    number = 605
                    title = '[leak-fix] Fix GradientBrush.GradientStops leak'
                    body = 'Fixes #20'
                    mergedAt = $null
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*missing baseRefName*'
        }

        It 'fails closed when a closed attempt has malformed baseRefName' {
            $global:mockClosed = @(
                New-LeakPr `
                    -Number 606 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops leak' `
                    -Body 'Fixes #20' `
                    -Base ' main ' `
                    -Merged $false
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakFixSafeOutputGate.ps1') `
                    -AgentOutputPath $script:agentOutput `
                    -StateDirectory $script:stateDirectory `
                    -Repository 'dotnet/maui'
            } | Should -Throw '*malformed baseRefName*'
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
                    -Title 'Back out the collection cleanup' `
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
                if ($search -eq 'Reverts in:body') {
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

        It 'rejects the legacy form when an agent emits it as new output' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan] Shell BackButtonBehavior.Command — reset leak'
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

        It 'keeps casing-only C# APIs distinct while the exact-casing batch contract still dedups' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items += [pscustomobject]@{
                type = 'create_issue'
                title = '[leak-scan] GradientBrush.gradientStops — distinct C# API casing'
                body = 'Second AI-generated leak report'
            }
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Not -Throw
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
            } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*701*"
        }

        It 'blocks agent-authored different-mechanism evidence for a same-API fix' {
            $global:mockHunterMerged = @(
                New-LeakPr `
                    -Number 701 `
                    -Title '[leak-fix] Fix GradientBrush.GradientStops teardown leak'
            )
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].body = @"
AI-generated leak report

## Same-API comparisons
Same-API comparison: dotnet/maui#701 | Different mechanism: Agent-authored claim
"@
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*701*"
        }

        It 'blocks a same-API open issue without accepting an override' {
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
            } | Should -Throw "*blocked issue creation for 'GradientBrush.GradientStops'*702*"
        }

        It 'blocks a legacy Shell-prefixed same-API open issue' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items[0].title =
                '[leak-scan] BackButtonBehavior.Command — reset leak'
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 36345
                    title = '[leak-scan] Shell BackButtonBehavior.Command leaks via strong ICommand'
                    body = 'Existing legacy scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/36345'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'BackButtonBehavior.Command'*36345*"
        }

        It 'rejects a mixed batch atomically when one item becomes stale' {
            $output = Get-Content -LiteralPath $script:hunterAgentOutput -Raw | ConvertFrom-Json
            $output.items += [pscustomobject]@{
                type = 'create_issue'
                title = '[leak-scan] Button.Clicked — event subscription leak'
                body = 'Second AI-generated leak report'
            }
            $output | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $script:hunterAgentOutput
            $global:mockHunterOpenIssues = @(
                [pscustomobject]@{
                    number = 703
                    title = '[leak-scan] Button.Clicked — existing event subscription leak'
                    body = 'Existing scanner issue'
                    url = 'https://github.com/dotnet/maui/issues/703'
                }
            )

            {
                & (Join-Path $PSScriptRoot 'Assert-LeakHunterSafeOutputGate.ps1') `
                    -AgentOutputPath $script:hunterAgentOutput `
                    -Repository 'dotnet/maui'
            } | Should -Throw "*blocked issue creation for 'Button.Clicked'*rejected atomically*"

            $unchanged = Get-Content -LiteralPath $script:hunterAgentOutput -Raw |
                ConvertFrom-Json
            @($unchanged.items).Count | Should -Be 2
            @($unchanged.items.title) | Should -Contain (
                '[leak-scan] GradientBrush.GradientStops — reset leak'
            )
            @($unchanged.items.title) | Should -Contain (
                '[leak-scan] Button.Clicked — event subscription leak'
            )
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
