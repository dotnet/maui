#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Fix-MilestoneDrift.ps1.
    Tests the pure functions (milestone mapping, matching, linked-issue extraction)
    without hitting GitHub or Git.

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

    It 'returns false (and warns, does not throw) when gh search fails' {
        $script:_searchExit = 1
        { Test-MilestoneValidForIssue -IssueNumber 42 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue } |
            Should -Not -Throw
        Test-MilestoneValidForIssue -IssueNumber 42 -Milestone '.NET 10 SR6' -WarningAction SilentlyContinue | Should -BeFalse
    }

    It 'caches lookups so a second call does not re-query gh' {
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes #34490","url":"u"}]'
        $script:_prShipped[501] = @('.NET 10 SR6')

        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue
        Test-MilestoneValidForIssue -IssueNumber 34490 -Milestone '.NET 10 SR6' | Should -BeTrue

        # gh search should have been hit exactly once
        Assert-MockCalled Invoke-GhCli -Times 1 -Exactly -Scope It -ParameterFilter {
            @($Arguments)[0] -eq 'search' -and @($Arguments)[1] -eq 'prs'
        }
    }

    It 'matches a fix verb that uses an owner/repo prefix (org/repo#NNN)' {
        # Real-world bodies sometimes write "Fixes dotnet/maui#34490"
        $script:_searchJson = '[{"number":501,"title":"Fix bug","body":"Fixes dotnet/maui#34490","url":"u"}]'
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
