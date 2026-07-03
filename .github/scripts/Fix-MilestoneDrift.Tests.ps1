#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Fix-MilestoneDrift.ps1.
    Most tests cover the pure functions (milestone mapping, matching, linked-issue
    extraction) and never touch GitHub or Git. One block — 'Get-RefinedReleaseMilestone
    — git integration (unmocked)' — builds a disposable LOCAL git repo in a temp dir to
    exercise the real `git tag -l` / `git merge-base --is-ancestor` plumbing end-to-end;
    it still never touches GitHub (no `gh`, no network) and is skipped if git is absent.

.EXAMPLE
    Invoke-Pester ./Fix-MilestoneDrift.Tests.ps1
    Invoke-Pester ./Fix-MilestoneDrift.Tests.ps1 -Output Detailed

.NOTES
    === LIVE VALIDATION GUIDE ===

    The following functions require git and GitHub API access and cannot be unit tested
    with Pester alone. When modifying these functions, run the dry-run commands below
    to validate correctness before merging.

    Functions requiring live validation:
      - Invoke-AnalyzeSinglePr (version detection, release branch lookup, fallback logic)
      - Invoke-AnalyzeRelease (tag-based batch analysis)
      - Find-ReleaseBranchForCommit (git ancestry checks against release branches)
      - Get-VersionFromGitRef (reads Versions.props from git refs, fetches missing commits)
      - Get-MainBranchForVersion (reads Versions.props from origin/main)

    Dry-run validation commands (run from repo root with gh CLI authenticated):

      # 1. PR merged to inflight/current — should read from origin/main (all branches feed into main)
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -PrNumber 34228 -RepoPath . -Verbose
      # Expected: "Version from Versions.props on origin/main: 10.0.70", milestone = .NET 10 SR7

      # 2. PR merged to main, already on a release branch — should use release branch
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -PrNumber 34620 -RepoPath . -Verbose
      # Expected: "Found in release branch: release/10.0.1xx-sr6 → .NET 10 SR6"

      # 3. PR merged to net11.0 — should read from origin/net11.0, not origin/main
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -PrNumber 34969 -RepoPath . -Verbose
      # Expected: reads from origin/net11.0, milestone is .NET 11.0-preview{N}

      # 4. PR merged to net11.0, on a preview release branch — should use release branch
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -PrNumber 30132 -RepoPath . -Verbose
      # Expected: "Found in release branch: release/11.0.1xx-preview3 → .NET 11.0-preview3"

      # 5. PR merged to a release branch directly — should read from that branch
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -PrNumber 35016 -RepoPath . -Verbose
      # Expected: reads from origin/release/10.0.1xx-sr6, milestone is .NET 10 SR6

      # 6. Tag-based reconciliation for a preview release
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -Tag '11.0.0-preview.3.26203.7' -RepoPath . -Verbose
      # Expected: finds preview2 as previous tag, scans ~234 PRs, skips ~180 merge-ups, checks ~53

      # 7. Tag-based reconciliation for a stable release
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -Tag 10.0.50 -RepoPath . -Output /dev/null -Verbose
      # Expected: finds 10.0.41 as previous tag, scans ~78 PRs, all .NET 10

      # 8. URL-form linked-issue discovery — covers the `Get-LinkedIssues` parser branch
      #    that extracts `Fixes https://github.com/dotnet/maui/issues/N` (the `#N` shorthand
      #    is the same-repo path; the URL form is the cross-origin form that an external
      #    contributor or copy-pasted-from-browser link will use).
      pwsh -File .github/scripts/Fix-MilestoneDrift.ps1 -PrNumber 35662 -RepoPath . -Verbose
      # Expected: report says `Issues checked: 1` (issue #35615 was found via the URL form).
      # If the URL branch ever regresses, `Issues checked` will drop to 0 instead.
      # Note: this dry-run does NOT exercise Test-MilestoneValidForIssue because PR 35662
      # and issue #35615 share a milestone (.NET 10 SR9), so Test-AndRecordCorrection
      # short-circuits before calling the validator. The Pester test at the
      # `'matches a fix verb that uses the full URL form (issues/N)'` It-block covers
      # the validator's URL-form OR query directly.

    Key things to verify after changes:
      - inflight/* and darc/* PRs read from origin/main (they feed into main)
      - net11.0 PRs read from origin/net11.0 (never from origin/main)
      - PRs on release branches get the milestone from the branch name (not Versions.props)
      - Preview tags in tag mode find the correct previous tag (preview2 → preview3, not full history)
      - Rebased/cherry-picked PRs are found via commit message grep when ancestry fails
#>

BeforeAll {
    . "$PSScriptRoot/Fix-MilestoneDrift.ps1"
}

Describe 'Resolve-MergedAfterCutoff' {
    It 'defaults to 2026-01-01 UTC when value is "<Value>"' -ForEach @(
        @{ Value = $null }
        @{ Value = '' }
        @{ Value = '   ' }
    ) {
        $result = Resolve-MergedAfterCutoff $Value
        $result | Should -Be ([datetime]::new(2026, 1, 1, 0, 0, 0, [System.DateTimeKind]::Utc))
        $result.Kind | Should -Be ([System.DateTimeKind]::Utc)
    }

    It 'parses a date-only value "<Value>" as UTC midnight' -ForEach @(
        @{ Value = '2025-01-01'; Year = 2025; Month = 1;  Day = 1 }
        @{ Value = '2024-06-15'; Year = 2024; Month = 6;  Day = 15 }
        @{ Value = '2020-12-31'; Year = 2020; Month = 12; Day = 31 }
    ) {
        $result = Resolve-MergedAfterCutoff $Value
        $result.Year  | Should -Be $Year
        $result.Month | Should -Be $Month
        $result.Day   | Should -Be $Day
        $result.Hour  | Should -Be 0
        $result.Kind  | Should -Be ([System.DateTimeKind]::Utc)
    }

    It 'parses an ISO-8601 value with explicit UTC offset' {
        $result = Resolve-MergedAfterCutoff '2025-06-01T12:30:00Z'
        $result.Kind | Should -Be ([System.DateTimeKind]::Utc)
        $result | Should -Be ([datetime]::new(2025, 6, 1, 12, 30, 0, [System.DateTimeKind]::Utc))
    }

    It 'normalizes a non-UTC offset to UTC' {
        # 2025-06-01T00:00:00+05:00 == 2025-05-31T19:00:00Z
        $result = Resolve-MergedAfterCutoff '2025-06-01T00:00:00+05:00'
        $result.Kind | Should -Be ([System.DateTimeKind]::Utc)
        $result | Should -Be ([datetime]::new(2025, 5, 31, 19, 0, 0, [System.DateTimeKind]::Utc))
    }

    It 'throws a clear error for unparseable value "<Value>"' -ForEach @(
        @{ Value = 'garbage' }
        @{ Value = 'not-a-date' }
        @{ Value = '2025-13-99' }
        @{ Value = '13/13/2025' }
    ) {
        { Resolve-MergedAfterCutoff $Value } | Should -Throw "*Invalid -MergedAfter value*"
    }
}

Describe 'Get-PrInfo — merged-after cutoff enforcement' {
    BeforeAll {
        # Build a GitHub-pulls-API-shaped object (ConvertFrom-Json style) for the mock.
        function New-FakePr {
            param([string]$MergedAt, [int]$Number = 42)
            [pscustomobject]@{
                title            = "PR $Number"
                html_url         = "https://github.com/dotnet/maui/pull/$Number"
                body             = ''
                merged_at        = $MergedAt
                milestone        = $null
                base             = [pscustomobject]@{ ref = 'net11.0' }
                merge_commit_sha = 'deadbeef'
            }
        }
    }

    AfterAll {
        # Restore the default cutoff so later Describes are unaffected.
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff ''
    }

    It 'skips a PR merged before the cutoff (returns a pre-cutoff sentinel, not $null)' {
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff ''   # 2026-01-01
        Mock Invoke-GhApi { New-FakePr -MergedAt '2025-05-01T00:00:00Z' -Number 100 }
        $result = Get-PrInfo 100
        $result | Should -BeOfType [hashtable]
        $result.SkippedPreCutoff | Should -BeTrue
        $result.Number | Should -Be 100
    }

    It 'includes a PR merged on/after the cutoff (returns the object, no skip sentinel)' {
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff ''   # 2026-01-01
        Mock Invoke-GhApi { New-FakePr -MergedAt '2026-03-01T00:00:00Z' -Number 101 }
        $pr = Get-PrInfo 101
        $pr | Should -Not -BeNullOrEmpty
        $pr.Number | Should -Be 101
        $pr.ContainsKey('SkippedPreCutoff') | Should -BeFalse
    }

    It 'includes a PR merged exactly at the cutoff boundary (strict less-than)' {
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff ''   # 2026-01-01T00:00:00Z
        Mock Invoke-GhApi { New-FakePr -MergedAt '2026-01-01T00:00:00Z' -Number 102 }
        Get-PrInfo 102 | Should -Not -BeNullOrEmpty
    }

    It 'a lowered cutoff lets an older PR through (the configurable use case)' {
        # Same 2025 PR that the default cutoff skips is now processed.
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff '2024-01-01'
        Mock Invoke-GhApi { New-FakePr -MergedAt '2025-05-01T00:00:00Z' -Number 103 }
        $pr = Get-PrInfo 103
        $pr | Should -Not -BeNullOrEmpty
        $pr.Number | Should -Be 103
    }

    It 'a raised cutoff skips a PR that the default would include' {
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff '2026-06-01'
        Mock Invoke-GhApi { New-FakePr -MergedAt '2026-03-01T00:00:00Z' -Number 104 }
        $result = Get-PrInfo 104
        $result.SkippedPreCutoff | Should -BeTrue
        $result.Number | Should -Be 104
    }

    It 'never skips an unmerged PR (no merged_at) regardless of cutoff' {
        $script:MergedAfterCutoff = Resolve-MergedAfterCutoff ''
        Mock Invoke-GhApi { New-FakePr -MergedAt $null -Number 105 }
        Get-PrInfo 105 | Should -Not -BeNullOrEmpty
    }
}

Describe 'Invoke-AnalyzeRelease — pre-cutoff skips are accounted separately from errors' {
    BeforeEach {
        # Minimal mocks so Invoke-AnalyzeRelease reaches the PR loop without touching git/gh.
        Mock ConvertTo-Milestone { '.NET 10 SR8' }
        Mock Get-AllTags { @('10.0.70', '10.0.80') }
        Mock Initialize-MilestoneValidationContext { }
        Mock Get-MainBranchForVersion { 'net10.0' }
        Mock Get-AllMilestones { @{ '.NET 10 SR8' = 999 } }
        Mock Find-MatchingMilestone { @{ Number = 999; Title = '.NET 10 SR8' } }
        Mock Get-PrNumbersBetweenTags { @(100, 101) }
        # Defensive: only reached for real (non-skipped, non-null) PRs — never hit in these tests.
        Mock Test-PrBelongsToVersion { $true }
        Mock Test-AndRecordCorrection { }
        Mock Get-LinkedIssues { @() }
    }

    It 'counts an all-pre-cutoff cohort as skipped, not as errors (no spurious failure)' {
        # Regression: a cohort whose PRs all predate the cutoff must NOT be reported as
        # "0 PRs checked, N errors" (which makes the top-level script throw a red run).
        Mock Get-PrInfo {
            param([int]$PrNum)
            return @{ SkippedPreCutoff = $true; Number = $PrNum }
        }
        $report = Invoke-AnalyzeRelease '10.0.80' '10.0.70' '.'
        $report.PrsSkippedPreCutoff | Should -Be 2
        $report.PrsChecked          | Should -Be 0
        $report.Errors.Count        | Should -Be 0   # the top-level throw guard keys off Errors.Count
    }

    It 'still records a genuine fetch failure as an error, distinct from a pre-cutoff skip' {
        Mock Get-PrInfo {
            param([int]$PrNum)
            if ($PrNum -eq 101) { return $null }                   # real fetch failure
            return @{ SkippedPreCutoff = $true; Number = $PrNum }   # pre-cutoff skip
        }
        $report = Invoke-AnalyzeRelease '10.0.80' '10.0.70' '.'
        $report.PrsSkippedPreCutoff | Should -Be 1
        $report.Errors.Count        | Should -Be 1
        $report.Errors[0]           | Should -BeLike '*Failed to fetch PR #101*'
    }
}

Describe 'ConvertTo-Milestone' {
    It 'maps GA tag "<Tag>" to "<Expected>"' -ForEach @(
        @{ Tag = '10.0.0';  Expected = '.NET 10.0 GA' }
        @{ Tag = '9.0.0';   Expected = '.NET 9.0 GA' }
    ) {
        ConvertTo-Milestone $Tag | Should -Be $Expected
    }

    It 'maps SR tag "<Tag>" to "<Expected>"' -ForEach @(
        @{ Tag = '10.0.10'; Expected = '.NET 10 SR1' }
        @{ Tag = '10.0.11'; Expected = '.NET 10 SR1.1' }
        @{ Tag = '10.0.20'; Expected = '.NET 10 SR2' }
        @{ Tag = '10.0.31'; Expected = '.NET 10 SR3.1' }
        @{ Tag = '10.0.40'; Expected = '.NET 10 SR4' }
        @{ Tag = '10.0.41'; Expected = '.NET 10 SR4.1' }
        @{ Tag = '10.0.50'; Expected = '.NET 10 SR5' }
        @{ Tag = '9.0.82';  Expected = '.NET 9 SR8.2' }
        @{ Tag = '9.0.90';  Expected = '.NET 9 SR9' }
        @{ Tag = '10.0.100'; Expected = '.NET 10 SR10' }
        @{ Tag = '10.0.101'; Expected = '.NET 10 SR10.1' }
    ) {
        ConvertTo-Milestone $Tag | Should -Be $Expected
    }

    It 'maps early patch "<Tag>" to SR1' -ForEach @(
        @{ Tag = '10.0.1' }
        @{ Tag = '10.0.5' }
        @{ Tag = '10.0.9' }
    ) {
        ConvertTo-Milestone $Tag | Should -Be '.NET 10.0 SR1'
    }

    It 'returns $null for non-SR tags' -ForEach @(
        @{ Tag = '10.0.0-preview.7.25406.3' }
        @{ Tag = 'not-a-tag' }
        @{ Tag = '' }
    ) {
        ConvertTo-Milestone $Tag | Should -BeNullOrEmpty
    }

    It 'maps preview "<Tag>" with label "<Label>" iter <Iter> to "<Expected>"' -ForEach @(
        @{ Tag = '11.0.0'; Label = 'preview'; Iter = 1; Expected = '.NET 11.0-preview1' }
        @{ Tag = '11.0.0'; Label = 'preview'; Iter = 3; Expected = '.NET 11.0-preview3' }
        @{ Tag = '11.0.0'; Label = 'preview'; Iter = 7; Expected = '.NET 11.0-preview7' }
        @{ Tag = '12.0.0'; Label = 'rc';      Iter = 1; Expected = '.NET 12.0-rc1' }
        @{ Tag = '12.0.0'; Label = 'rc';      Iter = 2; Expected = '.NET 12.0-rc2' }
    ) {
        ConvertTo-Milestone $Tag $Label $Iter | Should -Be $Expected
    }

    It 'maps to GA when no pre-release label' {
        ConvertTo-Milestone '11.0.0' $null 0 | Should -Be '.NET 11.0 GA'
        ConvertTo-Milestone '11.0.0' | Should -Be '.NET 11.0 GA'
    }
}

Describe 'Test-MilestoneMatch' {
    It 'exact match' {
        Test-MilestoneMatch '.NET 10 SR5' '.NET 10 SR5' | Should -BeTrue
    }

    It 'normalized match: ".NET 10.0 SR4" matches ".NET 10 SR4"' {
        Test-MilestoneMatch '.NET 10.0 SR4' '.NET 10 SR4' | Should -BeTrue
    }

    It 'normalized match: ".NET 10 SR4" matches ".NET 10.0 SR4"' {
        Test-MilestoneMatch '.NET 10 SR4' '.NET 10.0 SR4' | Should -BeTrue
    }

    It 'GA normalized match: ".NET 10.0 GA" matches ".NET 10 GA"' {
        Test-MilestoneMatch '.NET 10.0 GA' '.NET 10 GA' | Should -BeTrue
    }

    It 'GA normalized match: ".NET 10 GA" matches ".NET 10.0 GA"' {
        Test-MilestoneMatch '.NET 10 GA' '.NET 10.0 GA' | Should -BeTrue
    }

    It 'sub-patch ".NET 10 SR5.1" does NOT match ".NET 10 SR5" (distinct milestones)' {
        Test-MilestoneMatch '.NET 10 SR5.1' '.NET 10 SR5' | Should -BeFalse
    }

    It 'sub-patch ".NET 10 SR4.1" exact match' {
        Test-MilestoneMatch '.NET 10 SR4.1' '.NET 10 SR4.1' | Should -BeTrue
    }

    It 'sub-patch with normalization: ".NET 10.0 SR4.1" matches ".NET 10 SR4.1"' {
        Test-MilestoneMatch '.NET 10.0 SR4.1' '.NET 10 SR4.1' | Should -BeTrue
    }

    It 'does not match different SR numbers' {
        Test-MilestoneMatch '.NET 10 SR4' '.NET 10 SR5' | Should -BeFalse
    }

    It 'does not match null actual' {
        Test-MilestoneMatch $null '.NET 10 SR5' | Should -BeFalse
    }

    It 'does not match empty actual' {
        Test-MilestoneMatch '' '.NET 10 SR5' | Should -BeFalse
    }

    It 'does not match completely different milestones' {
        Test-MilestoneMatch 'Backlog' '.NET 10 SR5' | Should -BeFalse
        Test-MilestoneMatch '.NET 9 Servicing' '.NET 10 SR5' | Should -BeFalse
        Test-MilestoneMatch '.NET 11 Planning' '.NET 10 SR5' | Should -BeFalse
    }
}

Describe 'Find-MatchingMilestone' {
    BeforeAll {
        $script:milestones = @{
            '.NET 10.0 GA'    = 102
            '.NET 10.0 SR1'   = 99
            '.NET 10.0 SR2'   = 107
            '.NET 10.0 SR3'   = 109
            '.NET 10.0 SR4'   = 110
            '.NET 10 SR5'     = 113
            '.NET 10 SR6'     = 115
            '.NET 10.0 SR8'   = 117
        }
    }

    It 'direct match for ".NET 10 SR5"' {
        $result = Find-MatchingMilestone '.NET 10 SR5' $milestones
        $result.Title  | Should -Be '.NET 10 SR5'
        $result.Number | Should -Be 113
    }

    It 'normalized match: ".NET 10 SR4" resolves to ".NET 10.0 SR4"' {
        $result = Find-MatchingMilestone '.NET 10 SR4' $milestones
        $result.Title  | Should -Be '.NET 10.0 SR4'
        $result.Number | Should -Be 110
    }

    It 'normalized match: ".NET 10 SR1" resolves to ".NET 10.0 SR1"' {
        $result = Find-MatchingMilestone '.NET 10 SR1' $milestones
        $result.Title  | Should -Be '.NET 10.0 SR1'
        $result.Number | Should -Be 99
    }

    It 'alt format: ".NET 10.0 SR8" resolves from ".NET 10 SR8"' {
        $result = Find-MatchingMilestone '.NET 10 SR8' $milestones
        $result.Title  | Should -Be '.NET 10.0 SR8'
        $result.Number | Should -Be 117
    }

    It 'returns $null for non-existent milestone' {
        Find-MatchingMilestone '.NET 10 SR99' $milestones | Should -BeNullOrEmpty
    }
}

Describe 'Find-PreviousTag' {
    BeforeAll {
        $script:tags = @(
            '10.0.0', '10.0.1', '10.0.10', '10.0.11',
            '10.0.20', '10.0.30', '10.0.31',
            '10.0.40', '10.0.41', '10.0.50',
            '9.0.82', '9.0.90',
            '11.0.0-preview.1.26107', '11.0.0-preview.2.26152.10', '11.0.0-preview.3.26203.7'
        )
    }

    It 'stable: "<Tag>" → "<Expected>"' -ForEach @(
        @{ Tag = '10.0.50';  Expected = '10.0.41' }
        @{ Tag = '10.0.41';  Expected = '10.0.40' }
        @{ Tag = '10.0.40';  Expected = '10.0.31' }
        @{ Tag = '10.0.20';  Expected = '10.0.11' }
        @{ Tag = '10.0.10';  Expected = '10.0.1' }
        @{ Tag = '10.0.1';   Expected = '10.0.0' }
        @{ Tag = '9.0.90';   Expected = '9.0.82' }
    ) {
        Find-PreviousTag $Tag $tags | Should -Be $Expected
    }

    It 'preview: "<Tag>" → "<Expected>"' -ForEach @(
        @{ Tag = '11.0.0-preview.3.26203.7';  Expected = '11.0.0-preview.2.26152.10' }
        @{ Tag = '11.0.0-preview.2.26152.10'; Expected = '11.0.0-preview.1.26107' }
    ) {
        Find-PreviousTag $Tag $tags | Should -Be $Expected
    }

    It 'returns $null when no previous exists' {
        Find-PreviousTag '10.0.0' $tags | Should -BeNullOrEmpty
        Find-PreviousTag '11.0.0-preview.1.26107' $tags | Should -BeNullOrEmpty
    }

    It 'only considers same major version' {
        Find-PreviousTag '9.0.82' $tags | Should -Not -Match '^10\.'
    }
}

Describe 'Get-LinkedIssues' {
    It 'extracts "Fixes #NNNNN"' {
        $result = Get-LinkedIssues "Fixes #12345" "Some title"
        $result | Should -Contain 12345
    }

    It 'extracts "Closes #NNNNN" and "Resolves #NNNNN"' {
        $result = Get-LinkedIssues "Closes #111`nResolves #222" "Title"
        $result | Should -Contain 111
        $result | Should -Contain 222
    }

    It 'extracts past-tense variants' {
        $result = Get-LinkedIssues "Fixed #333`nClosed #444`nResolved #555" "Title"
        $result | Should -Contain 333
        $result | Should -Contain 444
        $result | Should -Contain 555
    }

    It 'extracts bare "close" and "resolve" forms' {
        $result = Get-LinkedIssues "Close #777`nResolve #888" "Title"
        $result | Should -Contain 777
        $result | Should -Contain 888
    }

    It 'extracts full GitHub issue URLs with fixing keyword' {
        $result = Get-LinkedIssues "Fixes https://github.com/dotnet/maui/issues/33800" "Title"
        $result | Should -Contain 33800
    }

    It 'ignores bare GitHub issue URLs without fixing keyword' {
        $result = @(Get-LinkedIssues "See https://github.com/dotnet/maui/issues/33800" "Title")
        $result | Should -HaveCount 0
    }

    It 'ignores informational URL references' {
        $result = @(Get-LinkedIssues "I believe a previously closed issue maybe the same thing happening:`nhttps://github.com/dotnet/maui/issues/17549" "Title")
        $result | Should -HaveCount 0
    }

    It 'extracts from title' {
        $result = Get-LinkedIssues "" "Fix issue fixes #99999"
        $result | Should -Contain 99999
    }

    It 'deduplicates' {
        $result = @(Get-LinkedIssues "Fixes #100`nAlso fixes #100`nResolves https://github.com/dotnet/maui/issues/100" "Title")
        $result | Should -HaveCount 1
        $result[0] | Should -Be 100
    }

    It 'returns empty for no references' {
        $result = Get-LinkedIssues "No issues here" "Just a title"
        $result | Should -HaveCount 0
    }

    It 'handles case insensitivity' {
        $result = Get-LinkedIssues "FIXES #555 and RESOLVES #666" "Title"
        $result | Should -Contain 555
        $result | Should -Contain 666
    }

    It 'extracts cross-repo `Fixes dotnet/maui#NNNNN` form' {
        # External contributors frequently use the cross-repo shorthand even within the
        # same repo. Pre-fix, this regex required `#` immediately after the verb, so the
        # `dotnet/maui` prefix made it silently ignored — meaning issues that should have
        # been included in milestone-correction sweeps were skipped.
        $result = Get-LinkedIssues "Fixes dotnet/maui#34490" "Title"
        $result | Should -Contain 34490
    }

    It 'extracts dotnet/maui cross-repo form combined with other syntaxes' {
        $result = @(Get-LinkedIssues "Closes dotnet/maui#1`nResolves #2`nFixes dotnet/maui#3" "Title")
        $result | Should -Contain 1
        $result | Should -Contain 2
        $result | Should -Contain 3
        $result | Should -HaveCount 3
    }

    It 'DOES NOT cross-pollute from other repos — `Fixes dotnet/runtime#42` must NOT be extracted as MAUI #42' {
        # Round-2 review caught this: an over-broad `[A-Za-z0-9_\-./]+?` prefix would
        # silently treat any cross-repo reference as a MAUI issue. The validator would
        # then clobber dotnet/maui#42's milestone based on a fix in dotnet/runtime#42.
        $result = @(Get-LinkedIssues "Fixes dotnet/runtime#42" "Title")
        $result | Should -HaveCount 0
    }

    It 'DOES NOT cross-pollute from xamarin-style references' {
        $result = @(Get-LinkedIssues "Fixes xamarin/Xamarin.Forms#1234" "Title")
        $result | Should -HaveCount 0
    }

    It 'DOES NOT cross-pollute from dotnet/runtime even when other syntaxes are present' {
        $result = @(Get-LinkedIssues "Fixes dotnet/runtime#100`nResolves dotnet/maui#200`nCloses #300" "Title")
        $result | Should -Not -Contain 100
        $result | Should -Contain 200
        $result | Should -Contain 300
        $result | Should -HaveCount 2
    }
}

Describe 'ConvertBranchToMilestone' {
    It 'maps GA branch' {
        ConvertBranchToMilestone 'release/10.0.1xx' | Should -Be '.NET 10.0 GA'
    }

    It 'maps SR branches' {
        ConvertBranchToMilestone 'release/10.0.1xx-sr1' | Should -Be '.NET 10 SR1'
        ConvertBranchToMilestone 'release/10.0.1xx-sr5' | Should -Be '.NET 10 SR5'
        ConvertBranchToMilestone 'release/10.0.1xx-sr10' | Should -Be '.NET 10 SR10'
    }

    It 'maps preview branches' {
        ConvertBranchToMilestone 'release/11.0.1xx-preview1' | Should -Be '.NET 11.0-preview1'
        ConvertBranchToMilestone 'release/11.0.1xx-preview3' | Should -Be '.NET 11.0-preview3'
        ConvertBranchToMilestone 'release/11.0.1xx-preview7' | Should -Be '.NET 11.0-preview7'
    }

    It 'maps RC branches' {
        ConvertBranchToMilestone 'release/12.0.1xx-rc1' | Should -Be '.NET 12.0-rc1'
        ConvertBranchToMilestone 'release/12.0.1xx-rc2' | Should -Be '.NET 12.0-rc2'
    }

    It 'returns $null for non-release branches' {
        ConvertBranchToMilestone 'main' | Should -BeNullOrEmpty
        ConvertBranchToMilestone 'net11.0' | Should -BeNullOrEmpty
        ConvertBranchToMilestone 'feature/something' | Should -BeNullOrEmpty
    }
}

Describe 'Get-PatchVersion' {
    It '"<Tag>" → <Expected>' -ForEach @(
        @{ Tag = '10.0.50';  Expected = 50 }
        @{ Tag = '10.0.0';   Expected = 0 }
        @{ Tag = '10.0.100'; Expected = 100 }
        @{ Tag = 'invalid';  Expected = 0 }
    ) {
        Get-PatchVersion $Tag | Should -Be $Expected
    }
}

Describe 'Test-IsReleaseTag' {
    It 'accepts valid .NET 10 stable tags' {
        Test-IsReleaseTag '10.0.50' 10 | Should -BeTrue
        Test-IsReleaseTag '10.0.0' 10  | Should -BeTrue
    }

    It 'accepts preview/RC tags for same major' {
        Test-IsReleaseTag '11.0.0-preview.3.26203.7' 11 | Should -BeTrue
        Test-IsReleaseTag '10.0.0-rc.1.25424.2' 10 | Should -BeTrue
    }

    It 'rejects wrong major version' {
        Test-IsReleaseTag '9.0.82' 10  | Should -BeFalse
        Test-IsReleaseTag '11.0.0-preview.3.26203.7' 10 | Should -BeFalse
    }

    It 'rejects non-release tags' {
        Test-IsReleaseTag 'not-a-tag' 10 | Should -BeFalse
    }
}

Describe 'Test-PrBelongsToVersion' {
    Context 'MainBranch = main (e.g. .NET 10)' {
        It 'allows PRs targeting main' {
            Test-PrBelongsToVersion 'main' 'main' 10 | Should -BeTrue
        }

        It 'allows PRs targeting inflight branches' {
            Test-PrBelongsToVersion 'inflight/current' 'main' 10 | Should -BeTrue
        }

        It 'allows PRs targeting release branches for same version' {
            Test-PrBelongsToVersion 'release/10.0.50' 'main' 10 | Should -BeTrue
        }

        It 'allows PRs targeting darc branches' {
            Test-PrBelongsToVersion 'darc/main-abc123' 'main' 10 | Should -BeTrue
        }

        It 'rejects PRs targeting net11.0' {
            Test-PrBelongsToVersion 'net11.0' 'main' 10 | Should -BeFalse
        }

        It 'rejects PRs targeting net12.0' {
            Test-PrBelongsToVersion 'net12.0' 'main' 10 | Should -BeFalse
        }

        It 'allows PRs targeting feature branches' {
            Test-PrBelongsToVersion 'feature/my-feature' 'main' 10 | Should -BeTrue
        }

        It 'allows null/empty base ref' {
            Test-PrBelongsToVersion $null 'main' 10 | Should -BeTrue
            Test-PrBelongsToVersion '' 'main' 10 | Should -BeTrue
        }
    }

    Context 'MainBranch = net11.0 (e.g. .NET 11)' {
        It 'allows PRs targeting net11.0' {
            Test-PrBelongsToVersion 'net11.0' 'net11.0' 11 | Should -BeTrue
        }

        It 'rejects PRs targeting main (those are .NET 10)' {
            Test-PrBelongsToVersion 'main' 'net11.0' 11 | Should -BeFalse
        }

        It 'rejects PRs targeting inflight (feeds into main, not net11.0)' {
            Test-PrBelongsToVersion 'inflight/current' 'net11.0' 11 | Should -BeFalse
        }

        It 'rejects PRs targeting net10.0' {
            Test-PrBelongsToVersion 'net10.0' 'net11.0' 11 | Should -BeFalse
        }

        It 'allows PRs targeting release/11.x branches' {
            Test-PrBelongsToVersion 'release/11.0.10' 'net11.0' 11 | Should -BeTrue
        }

        It 'rejects PRs targeting release/10.x branches' {
            Test-PrBelongsToVersion 'release/10.0.50' 'net11.0' 11 | Should -BeFalse
        }
    }
}

Describe 'Close-LinkedIssue' {
    BeforeEach {
        $script:_ghViewState = 'OPEN'
        $script:_ghViewExit  = 0
        $script:_ghCloseExit = 0
        $script:_lastCloseComment = $null

        Mock Invoke-GhCli {
            # `$Arguments` is the splat array because Invoke-GhCli declares it as
            # [Parameter(ValueFromRemainingArguments)]. Be tolerant of either shape.
            $a = @($Arguments)

            if ($a.Count -ge 2 -and $a[0] -eq 'issue' -and $a[1] -eq 'view') {
                $global:LASTEXITCODE = $script:_ghViewExit
                if ($script:_ghViewExit -ne 0) { return 'simulated gh view failure' }
                return "{`"state`":`"$($script:_ghViewState)`",`"number`":42,`"title`":`"test`"}"
            }

            if ($a.Count -ge 2 -and $a[0] -eq 'issue' -and $a[1] -eq 'close') {
                $global:LASTEXITCODE = $script:_ghCloseExit
                # Capture the comment text for assertion convenience.
                $commentIndex = [array]::IndexOf($a, '--comment')
                if ($commentIndex -ge 0 -and $commentIndex + 1 -lt $a.Count) {
                    $script:_lastCloseComment = $a[$commentIndex + 1]
                }
                if ($script:_ghCloseExit -ne 0) { return 'simulated gh close failure' }
                return ''
            }

            $global:LASTEXITCODE = 0
            return ''
        }
    }

    It 'no-ops when the issue is already closed (no gh issue close call)' {
        $script:_ghViewState = 'CLOSED'

        Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef 'net11.0' -Apply $true

        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'view'
        }
        Assert-MockCalled Invoke-GhCli -Times 0 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'close'
        }
    }

    It 'no-ops when the issue is already closed in lowercase state' {
        $script:_ghViewState = 'closed'

        Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef 'net11.0' -Apply $true

        Assert-MockCalled Invoke-GhCli -Times 0 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'close'
        }
    }

    It 'calls gh issue close with the correct args when issue is open and -Apply is set' {
        $script:_ghViewState = 'OPEN'

        Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef 'net11.0' -Apply $true

        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            $a = @($Arguments)
            ($a[0] -eq 'issue') -and ($a[1] -eq 'close') -and
            ($a -contains '42') -and ($a -contains 'dotnet/maui') -and
            ($a -contains '--reason') -and ($a -contains 'completed') -and ($a -contains '--comment')
        }
        # Comment text should reference the PR and the base ref
        $script:_lastCloseComment | Should -Not -BeNullOrEmpty
        $script:_lastCloseComment | Should -Match '#100'
        $script:_lastCloseComment | Should -Match 'net11\.0'
    }

    It 'does NOT call gh issue close in dry-run mode (Apply = $false)' {
        $script:_ghViewState = 'OPEN'

        Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef 'net11.0' -Apply $false

        Assert-MockCalled Invoke-GhCli -Times 0 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'close'
        }
        # But we DID check the issue state (the view call)
        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'view'
        }
    }

    It 'warns and returns when gh issue view fails (no close call, no throw)' {
        $script:_ghViewExit = 1

        { Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef 'net11.0' -Apply $true -WarningAction SilentlyContinue } |
            Should -Not -Throw

        Assert-MockCalled Invoke-GhCli -Times 0 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'close'
        }
    }

    It 'warns and returns when gh issue close fails (does not throw)' {
        $script:_ghViewState = 'OPEN'
        $script:_ghCloseExit = 1

        { Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef 'net11.0' -Apply $true -WarningAction SilentlyContinue } |
            Should -Not -Throw

        # We did attempt the close even though it failed
        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'close'
        }
    }

    It 'falls back to a placeholder branch label when BaseRef is empty' {
        $script:_ghViewState = 'OPEN'

        Close-LinkedIssue -IssueNumber 42 -PrNumber 100 -BaseRef '' -Apply $true

        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'issue' -and @($Arguments)[1] -eq 'close'
        }
        $script:_lastCloseComment | Should -Match 'unknown branch'
    }
}

Describe 'Get-MilestoneSortKey' {
    It 'returns null for null/empty/whitespace' {
        Get-MilestoneSortKey $null   | Should -Be $null
        Get-MilestoneSortKey ''      | Should -Be $null
        Get-MilestoneSortKey '   '   | Should -Be $null
    }

    It 'returns null for non-release placeholder milestones' {
        Get-MilestoneSortKey 'Backlog'             | Should -Be $null
        Get-MilestoneSortKey '.NET 11 Planning'    | Should -Be $null
        Get-MilestoneSortKey 'Future'              | Should -Be $null
        Get-MilestoneSortKey 'Triage'              | Should -Be $null
    }

    It 'orders preview/rc/GA/SR within a major correctly' {
        $p1  = Get-MilestoneSortKey '.NET 11.0-preview1'
        $p3  = Get-MilestoneSortKey '.NET 11.0-preview3'
        $rc1 = Get-MilestoneSortKey '.NET 11.0-rc1'
        $ga  = Get-MilestoneSortKey '.NET 11.0 GA'
        $sr1 = Get-MilestoneSortKey '.NET 11 SR1'

        ($p1 -lt $p3) | Should -BeTrue
        ($p3 -lt $rc1) | Should -BeTrue
        ($rc1 -lt $ga) | Should -BeTrue
        ($ga -lt $sr1) | Should -BeTrue
    }

    It 'orders SR sub-patches between SRs (SR4 < SR4.1 < SR5)' {
        $sr4   = Get-MilestoneSortKey '.NET 10 SR4'
        $sr4_1 = Get-MilestoneSortKey '.NET 10 SR4.1'
        $sr5   = Get-MilestoneSortKey '.NET 10 SR5'

        ($sr4 -lt $sr4_1) | Should -BeTrue
        ($sr4_1 -lt $sr5) | Should -BeTrue
    }

    It 'orders earlier majors before later majors' {
        $net10_sr6 = Get-MilestoneSortKey '.NET 10 SR6'
        $net11_p1  = Get-MilestoneSortKey '.NET 11.0-preview1'
        ($net10_sr6 -lt $net11_p1) | Should -BeTrue
    }

    It 'accepts the `.NET 10.0 GA` form (optional `.0` between major and GA) — production milestones use this naming' {
        # Production has BOTH `.NET 10 SR4`-style and `.NET 10.0 SR4`-style names live —
        # silently scoring the .0 form as null caused the validator to fail open and
        # treat valid milestones as un-comparable. See PR #35858 finding B.
        $ga      = Get-MilestoneSortKey '.NET 10.0 GA'
        $sr1     = Get-MilestoneSortKey '.NET 10.0 SR1'
        $sr2_1   = Get-MilestoneSortKey '.NET 10.0 SR2.1'
        $sr4     = Get-MilestoneSortKey '.NET 10.0 SR4'
        $ga      | Should -Not -Be $null
        $sr1     | Should -Not -Be $null
        $sr2_1   | Should -Not -Be $null
        $sr4     | Should -Not -Be $null
        ($ga -lt $sr1)     | Should -BeTrue
        ($sr1 -lt $sr2_1)  | Should -BeTrue
        ($sr2_1 -lt $sr4)  | Should -BeTrue
    }

    It 'treats `.NET 10 SR4` and `.NET 10.0 SR4` as equal (alternate spellings of the same release)' {
        $a = Get-MilestoneSortKey '.NET 10 SR4'
        $b = Get-MilestoneSortKey '.NET 10.0 SR4'
        $a | Should -Not -Be $null
        $b | Should -Not -Be $null
        $a | Should -Be $b
    }
}

Describe 'Compare-MauiMilestone' {
    It 'returns -1 when A is earlier (.NET 10 SR6 < .NET 11.0-preview3)' {
        Compare-MauiMilestone '.NET 10 SR6' '.NET 11.0-preview3' | Should -Be -1
    }

    It 'returns 1 when A is later (.NET 11.0-preview3 > .NET 10 SR6)' {
        Compare-MauiMilestone '.NET 11.0-preview3' '.NET 10 SR6' | Should -Be 1
    }

    It 'returns 0 when both are the same milestone' {
        Compare-MauiMilestone '.NET 11.0-preview3' '.NET 11.0-preview3' | Should -Be 0
    }

    It 'returns null when either side is non-comparable (Backlog/Planning/none)' {
        Compare-MauiMilestone 'Backlog' '.NET 11.0-preview3' | Should -Be $null
        Compare-MauiMilestone '.NET 11.0-preview3' '.NET 11 Planning' | Should -Be $null
        Compare-MauiMilestone $null '.NET 11.0-preview3' | Should -Be $null
        Compare-MauiMilestone '' '.NET 11.0-preview3' | Should -Be $null
    }

    It 'returns -1 for preview before rc (.NET 11.0-preview7 < .NET 11.0-rc1)' {
        Compare-MauiMilestone '.NET 11.0-preview7' '.NET 11.0-rc1' | Should -Be -1
    }
}

Describe 'Test-MilestoneValidForIssue' {
    BeforeEach {
        # Clear caches between tests so each starts fresh.
        $script:milestoneValidationCache = @{}

        # Default search response: empty array.
        $script:_searchJson = '[]'
        $script:_searchExit = 0
        # Default shipped-in mapping: { PrNumber => @(milestone-names) }.
        # Empty means "no PR has shipped in any milestone we ask about".
        $script:_prShipped = @{}

        Mock Invoke-GhCli {
            $a = @($Arguments)
            if ($a.Count -ge 2 -and $a[0] -eq 'search' -and $a[1] -eq 'prs') {
                $global:LASTEXITCODE = $script:_searchExit
                if ($script:_searchExit -ne 0) { return 'simulated gh search failure' }
                return $script:_searchJson
            }
            $global:LASTEXITCODE = 0
            return ''
        }

        # Mock the commit-in-tag check so tests don't need a real git repo / tags.
        Mock Test-PrShippedInMilestone {
            param([int]$PrNumber, [string]$Milestone)
            if ($script:_prShipped.ContainsKey($PrNumber)) {
                return ($script:_prShipped[$PrNumber] -contains $Milestone)
            }
            return $false
        }
    }

    It 'returns false when no PRs reference the issue' {
        $script:_searchJson = '[]'
        Test-MilestoneValidForIssue -IssueNumber 9999 -Milestone '.NET 10 SR6' | Should -BeFalse
    }

    It 'returns false for null/empty milestone (no validation needed)' {
        Test-MilestoneValidForIssue -IssueNumber 42 -Milestone $null | Should -BeFalse
        Test-MilestoneValidForIssue -IssueNumber 42 -Milestone ''   | Should -BeFalse
    }

    It 'returns true when a linking fix-PR commit is in the milestone tag' {
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes #34490","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'returns false when a PR mentions the issue but does not use a fix verb' {
        # Just a casual mention "#34490" — should not count as a fix
        $script:_searchJson = '[{"number":888,"title":"Related work","body":"See #34490 for context","url":"u"}]'
        $script:_prShipped[888] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeFalse
    }

    It 'returns false when the fixing PR did not ship in the milestone tag' {
        # PR fixes the issue but its commit is only in a later release (e.g. SR7.1).
        # This is the key tightening: trusting the PR''s milestone field alone is not enough.
        $script:_searchJson = '[{"number":513,"title":"net11 fix","body":"Fixes #34490","url":"u"}]'
        $script:_prShipped[513] = @('.NET 10 SR7.1')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeFalse
    }

    It 'returns false when fixing PR has shipped in nothing yet' {
        $script:_searchJson = '[{"number":513,"title":"net11 fix","body":"Fixes #34490","url":"u"}]'
        # 513 is in no tag

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeFalse
    }

    It 'returns true if ANY of multiple linking PRs validates the milestone' {
        # Two PRs link the issue; only the second has shipped in the matching milestone.
        $script:_searchJson = '[{"number":513,"title":"net11 fix","body":"Fixes #34490","url":"u"},{"number":501,"title":"sr6 fix","body":"Fixes #34490","url":"u"}]'
        $script:_prShipped[513] = @('.NET 11.0-preview3')
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'returns $null (uncertain, not $false) when gh search fails — caller must skip rather than clobber valid earlier milestone' {
        $script:_searchExit = 1
        { Test-MilestoneValidForIssue -IssueNumber 42 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue } |
            Should -Not -Throw
        $result = Test-MilestoneValidForIssue -IssueNumber 42 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue
        $result | Should -BeNullOrEmpty
        # Specifically $null, not $false — $false would mean "no linking PR ships here" (definitive)
        # and would cause Test-AndRecordCorrection to queue a destructive correction.
        ($null -eq $result) | Should -BeTrue
    }

    It 'does NOT cache $null — retries on a later call so a transient gh failure doesn''t permanently disable KEEP for the run' {
        # First call: gh fails → return $null (uncertain). DO NOT cache.
        $script:_searchExit = 1
        Test-MilestoneValidForIssue -IssueNumber 7 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue | Should -BeNullOrEmpty

        # Second call: gh succeeds with a real match → must return $true, not the cached $null.
        $script:_searchExit = 0
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes #7","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')
        Test-MilestoneValidForIssue -IssueNumber 7 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'caches lookups so a second call does not re-query gh' {
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes #34490","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue

        # Single OR query covers both forms in one API call → first call hits gh once,
        # second is fully cached. Total: 1 search call across 2 invocations.
        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'search' -and @($Arguments)[1] -eq 'prs'
        }
    }

    It 'uses a single OR query covering both `#N` and `issues/N` linking forms' {
        # Verify the actual query string includes the OR clause — pre-fix this was two
        # separate API calls, which doubled rate-limit pressure and could discard
        # positive evidence on partial failure.
        $script:_searchJson = '[]'
        Test-MilestoneValidForIssue -IssueNumber 99 -Milestone '.NET 10 SR6' | Out-Null
        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            $a = @($Arguments)
            $a[0] -eq 'search' -and $a[1] -eq 'prs' -and
            $a[2] -match '#99 in:title,body OR issues/99 in:body'
        }
    }

    It 'matches a fix verb that uses an owner/repo prefix (org/repo#NNN)' {
        # Real-world bodies sometimes write "Fixes dotnet/maui#34490"
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes dotnet/maui#34490","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'matches a fix verb that uses the full URL form (issues/N)' {
        # When a PR body links the issue ONLY via the URL form (`Fixes https://.../issues/N`)
        # — never the `#N` shorthand — the original `#N in:body` query missed it entirely.
        # The supplementary `issues/N in:body` query covers that case.
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes https://github.com/dotnet/maui/issues/34490","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'matches a fix verb when the issue number is only in the PR title (not body)' {
        # The first query `#N in:title,body` covers title-only linking — pre-fix the query
        # was `in:body` only, missing title-only links.
        $script:_searchJson = '[{"number":501,"title":"Fix #34490 in renderer","body":"Some unrelated body.","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
    }
}

Describe 'Test-MilestoneValidForPr' {
    BeforeEach {
        $script:_prShippedForPr = @{}
        Mock Test-PrShippedInMilestone {
            param([int]$PrNumber, [string]$Milestone)
            if ($script:_prShippedForPr.ContainsKey($PrNumber)) {
                return ($script:_prShippedForPr[$PrNumber] -contains $Milestone)
            }
            return $false
        }
    }

    It 'returns false for null/empty milestone' {
        Test-MilestoneValidForPr -PrNumber 100 -Milestone $null | Should -BeFalse
        Test-MilestoneValidForPr -PrNumber 100 -Milestone ''    | Should -BeFalse
    }

    It 'returns true when the PR commit is in a tag mapping to the milestone (cherry-pick case)' {
        # PR #34527 originally shipped in 10.0.60 (SR6) and was later cherry-picked
        # to 10.0.80 (SR8). When auditing SR8, we should KEEP it on SR6.
        $script:_prShippedForPr[34527] = @('.NET 10 SR6', '.NET 10 SR7', '.NET 10 SR8')

        Test-MilestoneValidForPr -PrNumber 34527 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'returns false when the PR has only shipped in the target milestone (not in the earlier one)' {
        # PR was milestoned for SR7 but actually only landed in SR8.
        $script:_prShippedForPr[35008] = @('.NET 10 SR8')

        Test-MilestoneValidForPr -PrNumber 35008 -Milestone '.NET 10 SR7' | Should -BeFalse
    }

    It 'returns false when the PR has shipped nowhere yet (in flight)' {
        # No entry — Test-PrShippedInMilestone returns false.
        Test-MilestoneValidForPr -PrNumber 99999 -Milestone '.NET 10 SR7' | Should -BeFalse
    }
}

Describe 'Test-AndRecordCorrection — earliest-release-wins guard' {
    BeforeEach {
        # Mock the underlying validators so the test focuses on dispatch logic.
        Mock Test-MilestoneValidForIssue {
            param([int]$IssueNumber, [string]$Milestone)
            if ($script:_validForIssue.ContainsKey("$IssueNumber|$Milestone")) {
                return $script:_validForIssue["$IssueNumber|$Milestone"]
            }
            return $false
        }
        Mock Test-MilestoneValidForPr {
            param([int]$PrNumber, [string]$Milestone)
            if ($script:_validForPr.ContainsKey("$PrNumber|$Milestone")) {
                return $script:_validForPr["$PrNumber|$Milestone"]
            }
            return $false
        }
        $script:_validForIssue = @{}
        $script:_validForPr = @{}
        # Helper inlined in each It (Pester 5 doesn't carry function defs out of BeforeEach).
        $script:_newReport = {
            @{
                TotalPrs       = 1
                PrsChecked     = 0
                IssuesChecked  = 0
                AlreadyCorrect = 0
                Corrections    = [System.Collections.ArrayList]::new()
                Errors         = [System.Collections.ArrayList]::new()
            }
        }
    }

    It 'deduplicates the SAME issue across multiple linking PRs in the Kept bucket' {
        # The same issue can be discovered via multiple linking PRs during a tag-range walk
        # (e.g. a backport PR and the original PR both reference the issue). Pre-fix, each
        # touch appended a new row, polluting the report.
        $script:_validForIssue['34490|.NET 10 SR6'] = $true
        $report = & $script:_newReport

        $resolvedMs = @{ Number = 999; Title = '.NET 10 SR8' }
        Test-AndRecordCorrection 'issue' 34490 'Bug' 'u1' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 501 $report
        Test-AndRecordCorrection 'issue' 34490 'Bug' 'u1' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 502 $report
        Test-AndRecordCorrection 'issue' 34490 'Bug' 'u1' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 503 $report

        $report.ContainsKey('Kept') | Should -BeTrue
        $report.Kept.Count | Should -Be 1
        $report.Kept[0].Number | Should -Be 34490
    }

    It 'does NOT collapse Kept entries for different issues that share a milestone' {
        $script:_validForIssue['34490|.NET 10 SR6'] = $true
        $script:_validForIssue['34491|.NET 10 SR6'] = $true
        $report = & $script:_newReport
        $resolvedMs = @{ Number = 999; Title = '.NET 10 SR8' }

        Test-AndRecordCorrection 'issue' 34490 'BugA' 'u1' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 501 $report
        Test-AndRecordCorrection 'issue' 34491 'BugB' 'u2' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 502 $report

        $report.Kept.Count | Should -Be 2
    }

    It 'when validator returns $null (uncertain) — does NOT queue a correction (defensive)' {
        # Earlier milestone + validator inconclusive ==> we MUST keep the current milestone.
        # Pre-fix, $null was coerced to $false and a destructive correction got queued.
        $script:_validForIssue['34490|.NET 10 SR6'] = $null
        $report = & $script:_newReport
        $resolvedMs = @{ Number = 999; Title = '.NET 10 SR8' }

        Test-AndRecordCorrection 'issue' 34490 'Bug' 'u1' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 501 $report `
            -WarningAction SilentlyContinue

        $report.Corrections.Count | Should -Be 0
        # Also NOT counted as kept — we don't have positive evidence either way.
        if ($report.ContainsKey('Kept')) { $report.Kept.Count | Should -Be 0 }
    }

    It 'when validator returns $false (no linking PR shipped there) — DOES queue a correction' {
        # This is the legitimate clobber path: earlier milestone was wrong, no fix actually
        # shipped there, the target milestone is correct. Pre-fix and post-fix both work.
        $script:_validForIssue['34490|.NET 10 SR6'] = $false
        $report = & $script:_newReport
        $resolvedMs = @{ Number = 999; Title = '.NET 10 SR8' }

        Test-AndRecordCorrection 'issue' 34490 'Bug' 'u1' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 501 $report

        $report.Corrections.Count | Should -Be 1
        $report.Corrections[0].Number | Should -Be 34490
    }

    It 'PR-side KEEP path also dedups identical (PR, milestone) pairs (cherry-pick case)' {
        # A cherry-picked PR can match in multiple linking searches. Same dedup applies.
        $script:_validForPr['34527|.NET 10 SR6'] = $true
        $report = & $script:_newReport
        $resolvedMs = @{ Number = 999; Title = '.NET 10 SR8' }

        Test-AndRecordCorrection 'pr' 34527 'Cherry' 'u' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 0 $report
        Test-AndRecordCorrection 'pr' 34527 'Cherry' 'u' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 0 $report

        $report.Kept.Count | Should -Be 1
    }
}

Describe 'Invoke-AnalyzeSinglePr — validation context seeding' {
    BeforeEach {
        # Stand up the bare minimum mocks so Invoke-AnalyzeSinglePr can run start-to-finish
        # without touching git, gh, or the file system.
        $script:_initCalled = $false
        $script:_initArgs = $null
        Mock Initialize-MilestoneValidationContext {
            param([string]$RepoPath, [string[]]$AllTags, [int]$Major)
            $script:_initCalled = $true
            $script:_initArgs = @{ RepoPath = $RepoPath; AllTags = $AllTags; Major = $Major }
        }
        Mock Get-PrInfo {
            return @{
                Number         = 42
                Title          = 'Test PR'
                Url            = 'u'
                Milestone      = ''
                BaseRef        = 'net11.0'
                MergedAt       = '2025-01-01T00:00:00Z'
                MergeCommitSha = 'abc123'
                Body           = ''
            }
        }
        Mock Get-AllTags { return @('10.0.60', '10.0.70', '11.0.0-preview.3') }
        # Drive the "found in release branch" fast-path so $expectedMs is set early and
        # we don't fall through to Find-TagContainingPr (which needs real git history).
        Mock Get-VersionFromGitRef { return @{ Tag = '11.0.0'; PreLabel = 'preview'; PreIter = 3 } }
        Mock Find-ReleaseBranchForCommit { return @{ Branch = 'release/11.0.1xx-preview3'; Milestone = '.NET 11.0-preview3' } }
        Mock ConvertBranchToMilestone { return '.NET 11.0-preview3' }
        Mock Get-MainBranchForVersion { return 'net11.0' }
        Mock Get-AllMilestones { return @(@{ Number = 99; Title = '.NET 11.0-preview3' }) }
        Mock Find-MatchingMilestone { return @{ Number = 99; Title = '.NET 11.0-preview3' } }
        Mock Test-PrBelongsToVersion { return $true }
        Mock Get-CurrentMajorVersion { return 11 }
        Mock Get-LinkedIssues { return @() }
        Mock Test-AndRecordCorrection { }
    }

    It 'seeds the validation context BEFORE Test-AndRecordCorrection is reached (so KEEP guard is not dead code)' {
        # Pre-fix: Initialize-MilestoneValidationContext was never called in single-PR mode.
        # That meant $script:validationAllTags stayed $null, Get-TagsForMilestone returned @(),
        # Test-PrShippedInMilestone always returned $false, and Test-MilestoneValidForIssue
        # never returned $true — making the entire KEEP branch unreachable in the live
        # workflow path (the path the cron + auto-trigger actually exercise).
        Invoke-AnalyzeSinglePr -PrNum 42 -ReleaseTag '' -Repo '.' | Out-Null

        $script:_initCalled | Should -BeTrue
        $script:_initArgs.Major | Should -Be 11
        # The tags array we passed in must reach the initializer (so KEEP queries have data to work with).
        $script:_initArgs.AllTags | Should -Contain '10.0.60'
    }
}

Describe 'Get-TagsForMilestone — cross-major filter' {
    BeforeEach {
        # Seed validation context as a live workflow run would: target major = 11.
        # We MUST be able to look up tags for a .NET 10 milestone (the cross-major
        # KEEP scenario), even though validationMajor is set to 11.
        Initialize-MilestoneValidationContext `
            -RepoPath '.' `
            -AllTags @('10.0.50', '10.0.60', '10.0.61', '10.0.70', '10.0.71', '10.0.80', '11.0.0', '11.0.1') `
            -Major 11
    }

    It 'returns 10.x tags when looking up `.NET 10 SR6` while validationMajor=11 (cross-major KEEP scenario)' {
        # Pre-fix this returned @() because the `Test-IsReleaseTag $tag 11` filter
        # dropped every 10.x tag → Test-PrShippedInMilestone returned $false →
        # earliest-release-wins KEEP guard silently clobbered the SR6 milestone.
        $tags = Get-TagsForMilestone -Milestone '.NET 10 SR6'
        $tags | Should -Contain '10.0.60'
        $tags | Should -Not -Contain '11.0.0'
        $tags | Should -Not -Contain '10.0.70'  # 10.0.70 maps to .NET 10 SR7, not SR6
    }

    It 'returns 10.x tags when looking up `.NET 10 SR7.1` (sub-patch)' {
        $tags = Get-TagsForMilestone -Milestone '.NET 10 SR7.1'
        $tags | Should -Contain '10.0.71'
    }

    It 'returns 11.x tags when looking up `.NET 11.0 GA` (same-major case still works)' {
        $tags = Get-TagsForMilestone -Milestone '.NET 11.0 GA'
        $tags | Should -Contain '11.0.0'
    }

    It 'returns empty for non-comparable milestones' {
        Get-TagsForMilestone -Milestone 'Backlog' | Should -BeNullOrEmpty
        Get-TagsForMilestone -Milestone '.NET 11 Planning' | Should -BeNullOrEmpty
    }

    It 'caches results per-milestone' {
        $first  = Get-TagsForMilestone -Milestone '.NET 10 SR6'
        $second = Get-TagsForMilestone -Milestone '.NET 10 SR6'
        $first.Count | Should -Be $second.Count
    }
}

Describe 'Test-PrShippedInMilestone — tristate on git failure' {
    BeforeEach {
        Initialize-MilestoneValidationContext `
            -RepoPath '.' `
            -AllTags @('10.0.60', '10.0.61') `
            -Major 10
    }

    It 'returns $true when commit is in any matching tag' {
        # Both 10.0.60 and 10.0.61 map to ".NET 10 SR6" and ".NET 10 SR6.1" respectively.
        # For SR6, only 10.0.60 matches. Mock Get-PrsInTag directly to avoid HashSet plumbing.
        Mock Get-PrsInTag {
            $set = [System.Collections.Generic.HashSet[int]]::new()
            [void]$set.Add(101); [void]$set.Add(102)
            return ,$set
        }
        Test-PrShippedInMilestone -PrNumber 101 -Milestone '.NET 10 SR6' | Should -BeTrue
    }

    It 'returns $false when ALL git reads succeed and no matching tag contains the PR' {
        Mock Get-PrsInTag {
            $set = [System.Collections.Generic.HashSet[int]]::new()
            [void]$set.Add(999)
            return ,$set
        }
        Test-PrShippedInMilestone -PrNumber 42 -Milestone '.NET 10 SR6' | Should -BeFalse
    }

    It 'returns $null (uncertain) when git read fails AND no other tag confirmed the PR' {
        # Get-PrsInTag returns $null on git failure. Caller must NOT treat this as
        # $false (which would clobber a possibly-valid earlier milestone).
        Mock Get-PrsInTag { return $null }
        $result = Test-PrShippedInMilestone -PrNumber 42 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue
        ($null -eq $result) | Should -BeTrue
    }

    It 'returns $true even when some other tag''s git read failed — definitive evidence wins over uncertainty' {
        # Seed two tags both mapping to SR6 by extending the cache to include another SR6 tag.
        # We simulate one tag failing and the other returning the PR.
        Initialize-MilestoneValidationContext `
            -RepoPath '.' `
            -AllTags @('10.0.60', '10.0.60-rc.1') `
            -Major 10
        $callCount = 0
        Mock Get-PrsInTag {
            $script:callCount++
            if ($script:callCount -eq 1) { return $null }
            $set = [System.Collections.Generic.HashSet[int]]::new()
            [void]$set.Add(101)
            return ,$set
        }
        # Force both tags into the .NET 10 SR6 bucket by also mocking Get-TagsForMilestone.
        Mock Get-TagsForMilestone { return @('10.0.60', '10.0.60-rc.1') }
        Test-PrShippedInMilestone -PrNumber 101 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue | Should -BeTrue
    }
}

Describe 'Test-AndRecordCorrection — PR-side tristate propagation' {
    BeforeEach {
        Mock Test-MilestoneValidForPr {
            param([int]$PrNumber, [string]$Milestone)
            if ($script:_prValid.ContainsKey("$PrNumber|$Milestone")) {
                return $script:_prValid["$PrNumber|$Milestone"]
            }
            return $false
        }
        $script:_prValid = @{}
    }

    It 'PR-side $null from validator → SKIP correction (mirrors issue-side defensive behavior)' {
        # The PR-side KEEP path was previously fail-closed-on-$false even when the underlying
        # git read failed. Now Test-MilestoneValidForPr is tristate too, and the same
        # defensive skip applies.
        $script:_prValid['34527|.NET 10 SR6'] = $null
        $report = @{
            TotalPrs       = 1
            PrsChecked     = 0
            IssuesChecked  = 0
            AlreadyCorrect = 0
            Corrections    = [System.Collections.ArrayList]::new()
            Errors         = [System.Collections.ArrayList]::new()
        }
        $resolvedMs = @{ Number = 999; Title = '.NET 10 SR8' }

        Test-AndRecordCorrection 'pr' 34527 'Cherry' 'u' '.NET 10 SR6' '.NET 10 SR8' $resolvedMs 0 $report `
            -WarningAction SilentlyContinue

        $report.Corrections.Count | Should -Be 0
        if ($report.ContainsKey('Kept')) { $report.Kept.Count | Should -Be 0 }
    }
}

Describe 'Get-PrsInTag — unary-comma preserves HashSet on cache hit' {
    BeforeEach {
        Initialize-MilestoneValidationContext `
            -RepoPath '.' `
            -AllTags @('10.0.60') `
            -Major 10
    }

    It 'Test-PrShippedInMilestone works on a cache hit for a tag with 1 reachable PR' {
        # Pre-fix: the cache-hit path returned the HashSet directly. PowerShell unwrapped
        # the 1-element HashSet enumerable to Int32 on the way back to the caller, so the
        # second Test-PrShippedInMilestone call threw "Int32 does not contain method Contains".
        # GPT-5.5 round-3 caught this.
        Mock Get-PrNumbersReachableFromTag { return @(101) }
        Test-PrShippedInMilestone -PrNumber 101 -Milestone '.NET 10 SR6' | Should -BeTrue
        # Second call hits the cache — must still produce a definitive answer.
        Test-PrShippedInMilestone -PrNumber 101 -Milestone '.NET 10 SR6' | Should -BeTrue
        Test-PrShippedInMilestone -PrNumber 999 -Milestone '.NET 10 SR6' | Should -BeFalse
    }

    It 'Test-PrShippedInMilestone returns $false (not $null) on a cache hit for a tag with 0 PRs' {
        # Pre-fix: a cached empty HashSet got unwrapped to $null on the second read, which
        # the tristate code (legitimately) treats as "git failure / uncertain" — incorrectly
        # converting a deterministic empty read into a permanent skip-this-correction state.
        Mock Get-PrNumbersReachableFromTag { return @() }
        Test-PrShippedInMilestone -PrNumber 42 -Milestone '.NET 10 SR6' | Should -BeFalse
        # Second call (cache hit) — must still be a definitive false, not the uncertain $null.
        $second = Test-PrShippedInMilestone -PrNumber 42 -Milestone '.NET 10 SR6'
        $second | Should -BeFalse
        ($null -eq $second) | Should -BeFalse
    }

    It 'Get-PrNumbersReachableFromTag is invoked exactly once per tag even across many lookups' {
        # Verifies the cache is hit (which is the whole point of returning the cached
        # HashSet — performance — and is precisely why the unwrap bug matters).
        Mock Get-PrNumbersReachableFromTag { return @(101, 102, 103) }
        Test-PrShippedInMilestone -PrNumber 101 -Milestone '.NET 10 SR6' | Should -BeTrue
        Test-PrShippedInMilestone -PrNumber 102 -Milestone '.NET 10 SR6' | Should -BeTrue
        Test-PrShippedInMilestone -PrNumber 103 -Milestone '.NET 10 SR6' | Should -BeTrue
        Test-PrShippedInMilestone -PrNumber 999 -Milestone '.NET 10 SR6' | Should -BeFalse
        Assert-MockCalled Get-PrNumbersReachableFromTag -Times 1 -Exactly
    }
}

Describe 'Get-RefinedReleaseMilestone — SR sub-patch precision' {
    # The bug: a commit that lands on an SR release branch AFTER the base SR
    # shipped actually goes out in a later sub-patch (10.0.71 = SR7.1), but the
    # branch name alone (release/10.0.1xx-sr7) only yields the base SR (SR7).
    # Get-RefinedReleaseMilestone fixes this by resolving the EARLIEST SR-family
    # tag that contains the commit. These tests mock the tag list and the
    # commit-in-tag ancestry check so no real git repo / tags are required.

    It 'refines to the earliest sub-patch tag that contains the commit (SR7 → SR7.1)' {
        # Family tags 10.0.70/71/72 all exist; commit shipped in .71 (and is
        # therefore also in .72), but NOT in the base .70. Earliest containing = .71.
        Mock Get-AllTags { return @('10.0.60', '10.0.70', '10.0.71', '10.0.72', '11.0.0') }
        Mock Test-CommitInTag { return ($Tag -in @('10.0.71', '10.0.72')) }
        Get-RefinedReleaseMilestone '.NET 10 SR7' 'abc123' '.' | Should -Be '.NET 10 SR7.1'
    }

    It 'keeps the base SR when the commit shipped in the base drop (SR7 stays SR7)' {
        # Commit is contained in the base .70 tag → earliest containing is .70 → SR7.
        Mock Get-AllTags { return @('10.0.70', '10.0.71') }
        Mock Test-CommitInTag { return $true }  # contained in everything, incl. base
        Get-RefinedReleaseMilestone '.NET 10 SR7' 'abc123' '.' | Should -Be '.NET 10 SR7'
    }

    It 'uses the next sub-patch after the latest shipped tag when not yet tagged (SR7 → SR7.2)' {
        # .70 and .71 already shipped but none contains this (untagged) commit, so
        # it goes out in the next drop after the latest shipped family tag → .72.
        Mock Get-AllTags { return @('10.0.70', '10.0.71') }
        Mock Test-CommitInTag { return $false }
        Get-RefinedReleaseMilestone '.NET 10 SR7' 'abc123' '.' | Should -Be '.NET 10 SR7.2'
    }

    It 'never crosses the family boundary: SR7 exhausted at .79 falls back to base SR (not SR8)' {
        # Degenerate state — all 10 SR7 drops (.70..79) shipped and none contains the
        # commit. The next patch (.80) belongs to SR8, which the SR7 branch never
        # ships, so the refiner must NOT predict SR8; it falls back to the base SR7.
        Mock Get-AllTags { return @('10.0.70', '10.0.71', '10.0.72', '10.0.73', '10.0.74', '10.0.75', '10.0.76', '10.0.77', '10.0.78', '10.0.79') }
        Mock Test-CommitInTag { return $false }
        Get-RefinedReleaseMilestone '.NET 10 SR7' 'abc123' '.' | Should -Be '.NET 10 SR7'
    }

    It 'returns the base SR unchanged when no family tags have shipped yet' {
        # Branch exists but no 10.0.7x tag yet → base SR is the best we can say.
        Mock Get-AllTags { return @('10.0.60', '10.0.61') }
        Mock Test-CommitInTag { return $false }
        Get-RefinedReleaseMilestone '.NET 10 SR7' 'abc123' '.' | Should -Be '.NET 10 SR7'
    }

    It 'does not let an adjacent SR family leak in (SR1 vs SR10 boundary)' {
        # SR10 family is 10.0.100..10.0.109; the SR1-era 10.0.10 tag must be ignored.
        Mock Get-AllTags { return @('10.0.10', '10.0.100', '10.0.101') }
        Mock Test-CommitInTag { return ($Tag -in @('10.0.101')) }
        Get-RefinedReleaseMilestone '.NET 10 SR10' 'abc123' '.' | Should -Be '.NET 10 SR10.1'
    }

    It 'returns non-SR milestones unchanged without touching tags' -ForEach @(
        @{ Milestone = '.NET 11.0-preview3' }
        @{ Milestone = '.NET 11.0-rc1' }
        @{ Milestone = '.NET 11.0 GA' }
    ) {
        Mock Get-AllTags { throw 'should not be called for non-SR milestones' }
        Mock Test-CommitInTag { throw 'should not be called for non-SR milestones' }
        Get-RefinedReleaseMilestone $Milestone 'abc123' '.' | Should -Be $Milestone
    }

    It 'returns the input unchanged for empty milestone or empty commit' {
        Mock Get-AllTags { throw 'should not be called' }
        Get-RefinedReleaseMilestone '' 'abc123' '.' | Should -Be ''
        Get-RefinedReleaseMilestone '.NET 10 SR7' '' '.' | Should -Be '.NET 10 SR7'
    }

    It 'falls back to the base SR when the ancestry check hits a real git error' {
        # Test-CommitInTag throws on a git failure (exit >1), distinct from a clean
        # "not an ancestor". The refiner must NOT turn that into a speculative
        # sub-patch (e.g. SR7.2) — it catches the error and returns the base SR so a
        # transient failure can only ever lose precision, never assign a wrong drop.
        Mock Get-AllTags { return @('10.0.70', '10.0.71') }
        Mock Test-CommitInTag { throw 'git merge-base --is-ancestor failed (exit 128)' }
        Get-RefinedReleaseMilestone '.NET 10 SR7' 'abc123' '.' | Should -Be '.NET 10 SR7'
    }
}

Describe 'Get-OnBranchShaFromLog — grep fallback subject precision' {
    # The grep fallback feeds this `git log --format='%H%x1f%s'` output (full SHA,
    # 0x1f Unit Separator, subject). Only a subject that ENDS with the squash token
    # "(#PrNum)" is a genuine squash-merge / cherry-pick of that PR; body-only
    # mentions and quoted reverts must be rejected so the milestone is refined from
    # the right commit. Input is oldest-first (git --reverse).
    BeforeAll {
        $script:US   = [char]0x1f
        $script:Sha1 = '1111111111111111111111111111111111111111'
        $script:Sha2 = '2222222222222222222222222222222222222222'
    }

    It 'returns the SHA of a genuine squash-merge subject (trailing token)' {
        $lines = @("$Sha1$US" + 'Fix crash on startup (#35694)')
        Get-OnBranchShaFromLog $lines 35694 | Should -Be $Sha1
    }

    It 'ignores a PR number that appears only in the commit body, not the subject' {
        # --grep matched on the body, but the emitted subject has no trailing token.
        $lines = @("$Sha1$US" + 'Unrelated change with no PR token in the subject')
        Get-OnBranchShaFromLog $lines 35694 | Should -BeNullOrEmpty
    }

    It 'ignores a revert that merely quotes the original PR number mid-subject' {
        # Searching for #100: the revert subject ends with (#200), not (#100).
        $lines = @("$Sha1$US" + 'Revert "Fix X (#100)" (#200)')
        Get-OnBranchShaFromLog $lines 100 | Should -BeNullOrEmpty
    }

    It 'matches the revert itself when searching for the revert PR number' {
        $lines = @("$Sha1$US" + 'Revert "Fix X (#100)" (#200)')
        Get-OnBranchShaFromLog $lines 200 | Should -Be $Sha1
    }

    It 'returns the FIRST genuine match (oldest-first input wins)' {
        # Two real squash subjects for the same PR (introduce, then re-apply). The
        # --reverse ordering means the original introduction is selected.
        $lines = @(
            "$Sha1$US" + 'Original fix (#42)',
            "$Sha2$US" + 'Re-apply fix (#42)'
        )
        Get-OnBranchShaFromLog $lines 42 | Should -Be $Sha1
    }

    It 'tolerates trailing whitespace after the token' {
        $lines = @("$Sha1$US" + 'Fix thing (#42)   ')
        Get-OnBranchShaFromLog $lines 42 | Should -Be $Sha1
    }

    It 'does not partial-match a longer PR number that shares the prefix' {
        # Searching #42 must not match (#420).
        $lines = @("$Sha1$US" + 'Some fix (#420)')
        Get-OnBranchShaFromLog $lines 42 | Should -BeNullOrEmpty
    }

    It 'returns null for empty input' {
        Get-OnBranchShaFromLog @() 42 | Should -BeNullOrEmpty
    }
}

# ---------------------------------------------------------------------------
# Git-backed integration block (no mocks, no GitHub).
#
# Everything above mocks Get-AllTags / Test-CommitInTag so the resolution logic
# can be tested in isolation. This block instead builds a throwaway LOCAL git
# repo in a temp dir and calls the REAL, unmocked helpers, so the actual
# `git tag -l` and `git merge-base --is-ancestor` plumbing is exercised
# end-to-end. It never touches GitHub (no `gh`, no network) and never mutates
# the checkout it runs from — every git command is scoped with `git -C $tmp`.
# Skipped cleanly when git is not on PATH.
# ---------------------------------------------------------------------------
Describe 'Get-RefinedReleaseMilestone — git integration (unmocked)' -Skip:(-not (Get-Command git -ErrorAction SilentlyContinue)) {

    BeforeAll {
        $script:tmp = Join-Path ([IO.Path]::GetTempPath()) "miletest-$(New-Guid)"
        New-Item -ItemType Directory -Path $script:tmp -Force | Out-Null

        # Disposable repo with a deterministic identity; nothing global is touched.
        git -C $script:tmp init -q
        git -C $script:tmp config user.email 'milestone-test@example.invalid'
        git -C $script:tmp config user.name  'Milestone Test'
        git -C $script:tmp config commit.gpgsign false

        # Build a linear history of empty commits and tag the SR-family drops:
        #   c0 -> 10.0.70 (SR7 base)
        #   c1 -> 10.0.71 (SR7.1)   <-- the commit under test
        #   c2 -> 10.0.72 (SR7.2)
        #   c3 -> (untagged, ships in the NEXT drop)
        git -C $script:tmp commit -q --allow-empty -m 'SR7 base drop'
        $script:shaBase = (git -C $script:tmp rev-parse HEAD).Trim()
        git -C $script:tmp tag '10.0.70'

        git -C $script:tmp commit -q --allow-empty -m 'fix shipped in SR7.1 (#35694)'
        $script:shaFix = (git -C $script:tmp rev-parse HEAD).Trim()
        git -C $script:tmp tag '10.0.71'

        git -C $script:tmp commit -q --allow-empty -m 'SR7.2 drop'
        git -C $script:tmp tag '10.0.72'

        git -C $script:tmp commit -q --allow-empty -m 'not yet tagged'
        $script:shaUntagged = (git -C $script:tmp rev-parse HEAD).Trim()
    }

    AfterAll {
        if ($script:tmp -and (Test-Path $script:tmp)) {
            Remove-Item -Recurse -Force $script:tmp -ErrorAction SilentlyContinue
        }
    }

    It 'resolves a commit to the EARLIEST family tag that contains it (SR7.1, not SR7.2)' {
        # shaFix is contained in 10.0.71 AND 10.0.72, but the earliest wins.
        Get-RefinedReleaseMilestone '.NET 10 SR7' $script:shaFix $script:tmp |
            Should -Be '.NET 10 SR7.1'
    }

    It 'resolves a commit that only shipped in the base drop to the base SR (SR7)' {
        # shaBase is the commit tagged 10.0.70 — earliest containing family tag.
        Get-RefinedReleaseMilestone '.NET 10 SR7' $script:shaBase $script:tmp |
            Should -Be '.NET 10 SR7'
    }

    It 'predicts the next sub-patch for a commit not yet in any family tag (SR7.3)' {
        # shaUntagged sits after 10.0.72; base SR already shipped, so it lands in
        # the next drop: latest family tag .72 -> predict .73 -> SR7.3.
        Get-RefinedReleaseMilestone '.NET 10 SR7' $script:shaUntagged $script:tmp |
            Should -Be '.NET 10 SR7.3'
    }

    It 'leaves a non-SR milestone untouched even when family tags exist' {
        Get-RefinedReleaseMilestone '.NET 10 Preview 3' $script:shaFix $script:tmp |
            Should -Be '.NET 10 Preview 3'
    }

    It 'Test-CommitInTag returns $true when the commit IS contained in the tag' {
        Test-CommitInTag $script:shaFix '10.0.71' $script:tmp | Should -BeTrue
    }

    It 'Test-CommitInTag returns $false when the commit is NOT contained (exit 1)' {
        # shaFix shipped in .71 — it is not an ancestor of the earlier .70 tag.
        Test-CommitInTag $script:shaFix '10.0.70' $script:tmp | Should -BeFalse
    }

    It 'Test-CommitInTag THROWS on a git error (bad object, exit > 1)' {
        # A well-formed but non-existent 40-hex SHA makes git fatal (exit 128),
        # which must surface as an exception rather than a silent "not contained".
        $bogus = 'deadbeef' * 5   # 40 hex chars, resolves to nothing
        { Test-CommitInTag $bogus '10.0.70' $script:tmp } | Should -Throw
    }
}
