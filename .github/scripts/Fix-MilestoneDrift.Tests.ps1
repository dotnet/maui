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
