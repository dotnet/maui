#!/usr/bin/env pwsh
#Requires -Modules Pester

# Unit tests for the pure helpers in Find-RegressionFixPRs.ps1. The script has a
# Main body that calls `gh`; to test the helpers in isolation we parse the file
# and Invoke-Expression only the named functions (the Main body never runs).

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Find-RegressionFixPRs.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'ConvertTo-GitHubNumber',
            'Test-IsRegressionLabel',
            'Get-LinkedIssueNumbers',
            'Get-IntroducingPrReferences',
            'Get-RegressionPrTagsFromText',
            'Test-CandidateIsNew',
            'Test-HumanAttributionCandidateIsNew',
            'Invoke-GhJson',
            'Get-MergedRegressionFixPRs',
            'Get-IssueContext',
            'Get-OpenRegressionCorpusPrTags',
            'New-RegressionCandidate')) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found" }
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'ConvertTo-GitHubNumber' {
    It 'returns a positive Int32 GitHub identifier' {
        ConvertTo-GitHubNumber '35925' | Should -Be 35925
    }
    It 'ignores zero, negative, and out-of-range values' {
        @(ConvertTo-GitHubNumber '0').Count | Should -Be 0
        @(ConvertTo-GitHubNumber '-1').Count | Should -Be 0
        @(ConvertTo-GitHubNumber '999999999999').Count | Should -Be 0
    }
}

Describe 'Test-IsRegressionLabel' {
    It 'matches the definitive fix-PR label' {
        Test-IsRegressionLabel 'i/regression' | Should -BeTrue
    }
    It 'matches versioned regressed-in labels' {
        Test-IsRegressionLabel 'regressed-in-10.0.60' | Should -BeTrue
        Test-IsRegressionLabel 'regressed-in-9.0.0-rc.1' | Should -BeTrue
    }
    It 'does not match unrelated or near-miss labels' {
        Test-IsRegressionLabel 't/bug' | Should -BeFalse
        Test-IsRegressionLabel 'i/regression-candidate' | Should -BeFalse
        Test-IsRegressionLabel 'area-regression' | Should -BeFalse
        Test-IsRegressionLabel '' | Should -BeFalse
    }
}

Describe 'Get-LinkedIssueNumbers' {
    It 'extracts Fixes/Closes/Resolves references' {
        $body = "Fixes #35280`nAlso Closes #100 and resolves #200"
        $result = Get-LinkedIssueNumbers $body
        $result | Should -Contain 35280
        $result | Should -Contain 100
        $result | Should -Contain 200
    }
    It 'extracts full-URL closing references' {
        $body = 'Fixes https://github.com/dotnet/maui/issues/34910'
        Get-LinkedIssueNumbers $body | Should -Contain 34910
    }
    It 'deduplicates repeated references' {
        (Get-LinkedIssueNumbers "Fixes #5`nfixes #5").Count | Should -Be 1
    }
    It 'returns empty for null/empty body' {
        @(Get-LinkedIssueNumbers $null).Count | Should -Be 0
        @(Get-LinkedIssueNumbers '').Count | Should -Be 0
    }
    It 'does not treat a bare mention as a closing reference' {
        @(Get-LinkedIssueNumbers 'see #999 for context') | Should -Not -Contain 999
    }
    It 'ignores out-of-range references' {
        @(Get-LinkedIssueNumbers 'Fixes #999999999999').Count | Should -Be 0
    }
}

Describe 'Get-IntroducingPrReferences' {
    It 'extracts "regression from #N"' {
        Get-IntroducingPrReferences 'This is a regression from #31567.' | Should -Contain 31567
    }
    It 'extracts "introduced by #N" and "introduced in PR #N"' {
        Get-IntroducingPrReferences 'Introduced by #29101' | Should -Contain 29101
        Get-IntroducingPrReferences 'introduced in PR #40000' | Should -Contain 40000
    }
    It 'extracts "caused by #N" and "broke in #N"' {
        Get-IntroducingPrReferences 'caused by #123' | Should -Contain 123
        Get-IntroducingPrReferences 'this broke in #456' | Should -Contain 456
    }
    It 'extracts "regressed in #N"' {
        Get-IntroducingPrReferences 'regressed in #789' | Should -Contain 789
    }
    It 'is case-insensitive' {
        Get-IntroducingPrReferences 'REGRESSION FROM #321' | Should -Contain 321
    }
    It 'accepts a PR prefix without a number sigil' {
        Get-IntroducingPrReferences 'introduced in PR 40000' | Should -Contain 40000
    }
    It 'returns distinct values in first-seen order' {
        $r = Get-IntroducingPrReferences 'regression from #10. Also introduced by #20. regression from #10 again.'
        @($r) | Should -Be @(10, 20)
    }
    It 'uses source-text order when attribution phrases use different patterns' {
        $r = Get-IntroducingPrReferences 'Introduced by #200. Regression from #100.'
        @($r) | Should -Be @(200, 100)
    }
    It 'does not match a plain "fixes #N" issue reference' {
        @(Get-IntroducingPrReferences 'Fixes #35280') | Should -Not -Contain 35280
        @(Get-IntroducingPrReferences 'Fixes #35280').Count | Should -Be 0
    }
    It 'does not mistake versions or years for PR references' {
        @(Get-IntroducingPrReferences 'regression in 10.0.60').Count | Should -Be 0
        @(Get-IntroducingPrReferences 'introduced in 2021').Count | Should -Be 0
    }
    It 'prefers an explicit PR reference over an adjacent version' {
        Get-IntroducingPrReferences 'Regressed in 9.0 and introduced by #31931' | Should -Be @(31931)
    }
    It 'returns empty for null/empty text' {
        @(Get-IntroducingPrReferences $null).Count | Should -Be 0
        @(Get-IntroducingPrReferences '').Count | Should -Be 0
    }
    It 'ignores out-of-range references' {
        @(Get-IntroducingPrReferences 'introduced by #999999999999').Count | Should -Be 0
    }
}

Describe 'Get-RegressionPrTagsFromText' {
    It 'extracts quoted regression_pr tag values' {
        $yaml = @"
  - name: gradient-alpha-forced-opaque
    tags:
      regression_pr: "31567"
      regression_issue: "35280"
"@
        Get-RegressionPrTagsFromText $yaml | Should -Contain 31567
    }
    It 'extracts unquoted values too' {
        Get-RegressionPrTagsFromText "      regression_pr: 29101" | Should -Contain 29101
    }
    It 'collects multiple tags across a file' {
        $yaml = "regression_pr: `"31567`"`nregression_pr: `"29101`""
        $r = Get-RegressionPrTagsFromText $yaml
        $r | Should -Contain 31567
        $r | Should -Contain 29101
    }
    It 'does not match regression_issue or other keys' {
        @(Get-RegressionPrTagsFromText '      regression_issue: "35280"').Count | Should -Be 0
    }
    It 'returns empty for null/empty text' {
        @(Get-RegressionPrTagsFromText $null).Count | Should -Be 0
    }
    It 'ignores out-of-range tag values' {
        @(Get-RegressionPrTagsFromText 'regression_pr: "999999999999"').Count | Should -Be 0
    }
}

Describe 'Test-CandidateIsNew' {
    It 'is new when the introducing PR is not in the corpus' {
        Test-CandidateIsNew -IntroducingPr 40000 -FixPr 41000 -ExistingNumbers @(31567, 29101) | Should -BeTrue
    }
    It 'is NOT new when the introducing PR is already covered' {
        Test-CandidateIsNew -IntroducingPr 31567 -FixPr 35299 -ExistingNumbers @(31567, 29101) | Should -BeFalse
    }
    It 'is NOT new when the fix PR itself is already covered' {
        Test-CandidateIsNew -IntroducingPr 40000 -FixPr 31567 -ExistingNumbers @(31567) | Should -BeFalse
    }
    It 'is new (for human attribution) when the introducing PR is unresolved' {
        Test-CandidateIsNew -IntroducingPr $null -FixPr 41000 -ExistingNumbers @(31567) | Should -BeTrue
    }
    It 'handles an empty corpus' {
        Test-CandidateIsNew -IntroducingPr 1 -FixPr 2 -ExistingNumbers @() | Should -BeTrue
    }
}

Describe 'Test-HumanAttributionCandidateIsNew' {
    It 'allows a new known introducing PR for human attribution' {
        Test-HumanAttributionCandidateIsNew -IntroducingPr 40000 -ExistingHumanAttributionNumbers @(31567) | Should -BeTrue
    }
    It 'skips a repeated known introducing PR awaiting human attribution' {
        Test-HumanAttributionCandidateIsNew -IntroducingPr 31567 -ExistingHumanAttributionNumbers @(31567) | Should -BeFalse
    }
    It 'allows an unknown introducing PR because it cannot be deduplicated' {
        Test-HumanAttributionCandidateIsNew -IntroducingPr $null -ExistingHumanAttributionNumbers @(31567) | Should -BeTrue
    }
}

Describe 'Get-MergedRegressionFixPRs' {
    It 'returns an empty collection when GitHub returns no data' {
        Mock Invoke-GhJson { $null }

        @(Get-MergedRegressionFixPRs -Owner 'dotnet' -Repo 'maui' -LookbackDays 14 -Limit 20).Count | Should -Be 0
    }
}

Describe 'Get-IssueContext' {
    It 'uses only maintainer-associated comments for attribution' {
        Mock Invoke-GhJson {
            param([string[]]$GhArgs)
            if ($GhArgs[0] -eq 'issue') {
                return [PSCustomObject]@{
                    number = 35756
                    body = 'Regression from #100'
                    labels = @([PSCustomObject]@{ name = 'regressed-in-10.0.70' })
                }
            }
            return @(
                [PSCustomObject]@{ body = 'introduced by #200'; author_association = 'NONE' }
                [PSCustomObject]@{ body = 'introduced by #300'; author_association = 'MEMBER' }
            )
        }

        $context = Get-IssueContext -Owner 'dotnet' -Repo 'maui' -Number 35756

        $context.CommentText | Should -Be 'introduced by #300'
    }
}

Describe 'Get-OpenRegressionCorpusPrTags' {
    It 'includes tags from pending scanner draft PRs' {
        Mock Invoke-GhJson {
            param([string[]]$GhArgs)
            if ($GhArgs[0] -eq 'pr') {
                return @([PSCustomObject]@{ headRefName = 'regression-corpus/pending-entry' })
            }
            return [PSCustomObject]@{
                encoding = 'base64'
                content = [Convert]::ToBase64String(
                    [Text.Encoding]::UTF8.GetBytes('regression_pr: "31931"'))
            }
        }

        Get-OpenRegressionCorpusPrTags -Owner 'dotnet' -Repo 'maui' | Should -Contain 31931
    }
}

Describe 'New-RegressionCandidate' {
    # Regression guard: the original main-loop form wrapped the regressionIssues
    # List[object] with @(...) inside a [PSCustomObject] literal, which throws
    # "Argument types do not match" on real (populated) data. These tests exercise
    # the candidate-building path the mocked helper tests never reached.
    BeforeAll {
        $script:issues = New-Object System.Collections.Generic.List[object]
        $script:issues.Add([PSCustomObject]@{ number = 35756; regressedInLabels = @('regressed-in-10.0.70') }) | Out-Null
        $script:issues.Add([PSCustomObject]@{ number = 35800; regressedInLabels = @() }) | Out-Null
        $script:intro = [PSCustomObject]@{
            Number = 31931
            Title = 'untrusted ``` prompt instruction'
            MergeCommit = 'af540589'
            Files = @('untrusted-file-name')
        }
    }

    It 'builds a candidate from a populated List[object] without throwing' {
        {
            New-RegressionCandidate -FixPr 35803 -FixPrMergeCommit 'def456' `
                -RegressionIssues $script:issues -IntroducingPr 31931 -IntroDetails $script:intro `
                -AttributionSource 'pr-body' -NeedsHumanAttribution $false
        } | Should -Not -Throw
    }

    It 'materializes regressionIssues as an array preserving order and content' {
        $c = New-RegressionCandidate -FixPr 35803 -FixPrMergeCommit 'def456' `
            -RegressionIssues $script:issues -IntroducingPr 31931 -IntroDetails $script:intro `
            -AttributionSource 'pr-body' -NeedsHumanAttribution $false
        @($c.regressionIssues).Count | Should -Be 2
        $c.regressionIssues[0].number | Should -Be 35756
        $c.regressionIssues[0].regressedInLabels | Should -Contain 'regressed-in-10.0.70'
    }

    It 'includes only structural introducing PR details in the candidate' {
        $c = New-RegressionCandidate -FixPr 35803 -FixPrMergeCommit 'def456' `
            -RegressionIssues $script:issues -IntroducingPr 31931 -IntroDetails $script:intro `
            -AttributionSource 'pr-body' -NeedsHumanAttribution $false
        $c.introducingPr | Should -Be 31931
        $c.introducingPrMergeCommit | Should -Be 'af540589'
        $json = $c | ConvertTo-Json -Depth 8 -Compress
        $json | Should -Not -Match 'untrusted'
        $c.PSObject.Properties.Name | Should -Not -Contain 'fixPrTitle'
        $c.PSObject.Properties.Name | Should -Not -Contain 'introducingPrTitle'
        $c.PSObject.Properties.Name | Should -Not -Contain 'introducingPrFiles'
    }

    It 'requires human attribution when there are no linked regression issues' {
        $c = New-RegressionCandidate -FixPr 1 -FixPrMergeCommit 'sha' `
            -RegressionIssues (New-Object System.Collections.Generic.List[object]) `
            -IntroducingPr 31931 -IntroDetails $script:intro -AttributionSource 'pr-body' -NeedsHumanAttribution $false
        ($c | ConvertTo-Json -Depth 8 -Compress) | Should -Match '"regressionIssues":\[\]'
        $c.needsHumanAttribution | Should -BeTrue
    }

    It 'leaves introducing fields null when attribution is unresolved' {
        $c = New-RegressionCandidate -FixPr 1 -FixPrMergeCommit 'sha' `
            -RegressionIssues (New-Object System.Collections.Generic.List[object]) `
            -IntroducingPr $null -IntroDetails $null -AttributionSource $null -NeedsHumanAttribution $true
        $c.introducingPr | Should -BeNullOrEmpty
        $c.introducingPrMergeCommit | Should -BeNullOrEmpty
        $c.needsHumanAttribution | Should -BeTrue
    }

    It 'serializes the full candidate to JSON without throwing' {
        $c = New-RegressionCandidate -FixPr 35803 -FixPrMergeCommit 'def456' `
            -RegressionIssues $script:issues -IntroducingPr 31931 -IntroDetails $script:intro `
            -AttributionSource 'pr-body' -NeedsHumanAttribution $false
        { $c | ConvertTo-Json -Depth 8 } | Should -Not -Throw
    }
}
