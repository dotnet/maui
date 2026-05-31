BeforeAll {
    . "$PSScriptRoot/Resolve-RerunEligibility.ps1" -PRNumber 1 -CurrentCommentId 1

    function New-TestUser {
        param(
            [string]$Login = 'dev-user',
            [string]$Type = 'User'
        )

        [pscustomobject]@{
            login = $Login
            type  = $Type
        }
    }

    function New-TestComment {
        param(
            [int64]$Id,
            [string]$Body,
            [string]$CreatedAt,
            [string]$UpdatedAt = $CreatedAt,
            [string]$Login = 'dev-user',
            [string]$Type = 'User',
            [string]$Kind = 'issue-comment'
        )

        [pscustomobject]@{
            id         = $Id
            kind       = $Kind
            body       = $Body
            created_at = $CreatedAt
            updated_at = $UpdatedAt
            user       = New-TestUser -Login $Login -Type $Type
        }
    }

    function New-TestCommit {
        param(
            [string]$Sha,
            [string]$Date
        )

        [pscustomobject]@{
            sha    = $Sha
            commit = [pscustomobject]@{
                author    = [pscustomobject]@{ date = $Date }
                committer = [pscustomobject]@{ date = $Date }
            }
        }
    }

    function New-AISummaryBody {
        param([string]$Sha = 'abcdef1')

        @"
<!-- AI Summary -->

## AI Review Summary

<!-- SESSION:$Sha START -->
<details>
<summary><strong>Review Sessions</strong> — click to expand</summary>
</details>
<!-- SESSION:$Sha END -->
"@
    }
}

Describe 'Resolve-RerunEligibility' {
    It 'rejects commands when no AI Summary exists' {
        $comments = @(
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-ai-summary'
    }

    It 'rejects a rerun command when there are no new comments or commits' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-new-comments-or-commits'
    }

    It 'accepts a non-command comment after the latest AI Summary' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 2 -Body 'I pushed the requested update.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-comment-after-ai-summary'
    }

    It 'does not count repeated rerun commands as evidence' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 9 -Body '/review rerun' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-new-comments-or-commits'
    }

    It 'accepts a non-command comment after the previous rerun command' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 8 -Body '/review rerun' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 9 -Body 'Follow-up detail after rerun request.' -CreatedAt '2026-05-31T09:50:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-comment-after-previous-rerun'
    }

    It 'does not reuse old activity from before a previous rerun command' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 7 -Body 'Old follow-up before the first rerun.' -CreatedAt '2026-05-31T09:40:00Z'
            New-TestComment -Id 8 -Body '/review rerun' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-new-comments-or-commits'
    }

    It 'finds AI Summary content posted as a PR review' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot' -Kind 'review'
            New-TestComment -Id 2 -Body 'Follow-up after the review.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-comment-after-ai-summary'
    }

    It 'accepts a current head SHA that differs from the latest reviewed session' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody -Sha 'abcdef1') -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'fedcba9876543210'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-head-commit'
    }

    It 'accepts a commit after the previous rerun command' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 8 -Body '/review rerun' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )
        $commits = @(
            New-TestCommit -Sha 'abcdef123' -Date '2026-05-31T09:50:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits $commits -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-commit-after-previous-rerun'
    }

    It 'rejects bot rerun comments' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z' -Login 'maui-bot' -Type 'Bot'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'bot-comment'
    }

    It 'is idempotent when ready-for-rerun label already exists' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'maui-bot' -Type 'Bot'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -CurrentLabels @('s/agent-ready-for-rerun')

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'label-already-present'
    }
}
