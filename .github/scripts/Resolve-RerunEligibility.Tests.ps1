BeforeAll {
    . "$PSScriptRoot/Resolve-RerunEligibility.ps1"

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
            [string]$Kind = 'issue-comment',
            [string]$AuthorAssociation = 'COLLABORATOR'
        )

        [pscustomobject]@{
            id         = $Id
            kind       = $Kind
            body       = $Body
            created_at = $CreatedAt
            updated_at = $UpdatedAt
            user       = New-TestUser -Login $Login -Type $Type
            author_association = $AuthorAssociation
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
    It 'normalizes app-style GitHub bot author logins' {
        Normalize-GitHubActorLogin 'app/dependabot' | Should -Be 'dependabot[bot]'
        Normalize-GitHubActorLogin ' dependabot[bot] ' | Should -Be 'dependabot[bot]'
        Normalize-GitHubActorLogin '' | Should -Be ''
    }

    It 'parses review command branch and platform options for reruns' {
        $parsed = ConvertFrom-ReviewCommand '/review -b feature/regression-check -p ios'

        $parsed | Should -Not -BeNullOrEmpty
        $parsed.Platform | Should -Be 'ios'
        $parsed.PipelineRef | Should -Be 'feature/regression-check'
    }

    It 'parses equals-form branch and platform options' {
        $parsed = ConvertFrom-ReviewCommand '/review --branch=refs/heads/feature/regression-check --platform=ios'

        $parsed | Should -Not -BeNullOrEmpty
        $parsed.Platform | Should -Be 'ios'
        $parsed.PipelineRef | Should -Be 'feature/regression-check'
    }

    It 'strips refs heads prefix when normalizing review pipeline refs' {
        Normalize-ReviewPipelineRef 'refs/heads/feature/regression-check' |
            Should -Be 'feature/regression-check'
    }

    It 'normalizes app-style GitHub actor logins to bot logins' {
        Normalize-GitHubActorLogin 'app/dependabot' | Should -Be 'dependabot[bot]'
        Normalize-GitHubActorLogin 'dev-user' | Should -Be 'dev-user'
    }

    Context 'review command option resolution (collaborator-permission trust)' {
        BeforeEach {
            # Trust is decided by CURRENT collaborator permission per login — the
            # same signal /review uses — not the comment's author_association.
            Mock Test-ReviewOptionLoginTrusted {
                param([string]$Login, [string]$Owner, [string]$Repo)
                return ($Login -in @('dev-user', 'trusted-user', 'maintainer'))
            }
        }

        It 'finds latest normal review command while ignoring rerun and tests commands' {
            $comments = @(
                New-TestComment -Id 1 -Body '/review -b old/ref -p android' -CreatedAt '2026-05-31T09:00:00Z'
                New-TestComment -Id 2 -Body '/review tests' -CreatedAt '2026-05-31T09:05:00Z'
                New-TestComment -Id 3 -Body '/review --platform ios --branch feature/regression-check' -CreatedAt '2026-05-31T09:10:00Z'
                New-TestComment -Id 4 -Body '/review rerun' -CreatedAt '2026-05-31T09:15:00Z'
            )

            $options = Get-LatestReviewCommandOptions -Comments $comments

            $options.Found | Should -BeTrue
            $options.Platform | Should -Be 'ios'
            $options.PipelineRef | Should -Be 'feature/regression-check'
            $options.CommentId | Should -Be 3
        }

        It 'ignores review command options from commenters without write access' {
            $comments = @(
                New-TestComment -Id 1 -Body '/review --branch=refs/pull/9999/merge --platform=windows' -CreatedAt '2026-05-31T09:00:00Z' -Login 'untrusted-user'
                New-TestComment -Id 2 -Body '/review -b feature/trusted -p ios' -CreatedAt '2026-05-31T09:05:00Z' -Login 'maintainer'
            )

            $options = Get-LatestReviewCommandOptions -Comments $comments

            $options.Found | Should -BeTrue
            $options.Platform | Should -Be 'ios'
            $options.PipelineRef | Should -Be 'feature/trusted'
            $options.CommentId | Should -Be 2
        }

        It 'does not use review command options when only untrusted comments exist' {
            $comments = @(
                New-TestComment -Id 1 -Body '/review --branch=refs/pull/9999/merge --platform=windows' -CreatedAt '2026-05-31T09:00:00Z' -Login 'untrusted-user'
            )

            $options = Get-LatestReviewCommandOptions -Comments $comments

            $options.Found | Should -BeFalse
            $options.PipelineRef | Should -Be 'main'
        }

        It 'uses the latest command from a currently-trusted maintainer' {
            $comments = @(
                New-TestComment -Id 1 -Body '/review -b feature/old -p ios' -CreatedAt '2026-05-31T09:00:00Z' -Login 'maintainer'
                New-TestComment -Id 2 -Body '/review -b feature/new -p windows' -CreatedAt '2026-05-31T09:10:00Z' -Login 'maintainer'
            )

            $options = Get-LatestReviewCommandOptions -Comments $comments

            $options.Found | Should -BeTrue
            $options.Platform | Should -Be 'windows'
            $options.PipelineRef | Should -Be 'feature/new'
            $options.CommentId | Should -Be 2
            $options.AuthorLogin | Should -Be 'maintainer'
        }

        It 'ignores all commands from a login that currently lacks write access' {
            $comments = @(
                New-TestComment -Id 5 -Body '/review -b feature/new -p windows' -CreatedAt '2026-05-31T10:00:00Z' -Login 'former-collaborator'
            )

            $options = Get-LatestReviewCommandOptions -Comments $comments

            $options.Found | Should -BeFalse
            $options.PipelineRef | Should -Be 'main'
        }

        It 'honors a command by collaborator permission even when author_association is NONE' {
            # The exact production failure: a maintainer's /review -b read as
            # author_association=CONTRIBUTOR under the bot token. Permission wins.
            $comments = @(
                New-TestComment -Id 1 -Body '/review -b feature/enhanced-reviewer -p android' -CreatedAt '2026-05-31T09:00:00Z' -Login 'maintainer' -AuthorAssociation 'NONE'
            )

            $options = Get-LatestReviewCommandOptions -Comments $comments

            $options.Found | Should -BeTrue
            $options.Platform | Should -Be 'android'
            $options.PipelineRef | Should -Be 'feature/enhanced-reviewer'
        }
    }

    Context 'Test-ReviewOptionLoginTrusted (collaborator-permission lookup)' {
        BeforeEach {
            Clear-ReviewOptionPermissionCache
            Mock Start-Sleep {}
        }

        It 'trusts write/maintain/admin permission' {
            Mock Get-CollaboratorPermissionResult { [pscustomobject]@{ ExitCode = 0; Permission = 'write'; StdErr = '' } }
            Test-ReviewOptionLoginTrusted -Login 'maintainer' | Should -BeTrue
        }

        It 'does not trust read or none permission' {
            Mock Get-CollaboratorPermissionResult { [pscustomobject]@{ ExitCode = 0; Permission = 'read'; StdErr = '' } }
            Test-ReviewOptionLoginTrusted -Login 'reader' | Should -BeFalse
        }

        It 'rejects an invalid (non-user) login without calling the API' {
            Mock Get-CollaboratorPermissionResult { throw 'API should not be called for an invalid login' }
            Test-ReviewOptionLoginTrusted -Login 'dotnet-maestro[bot]' | Should -BeFalse
            Test-ReviewOptionLoginTrusted -Login '' | Should -BeFalse
            Should -Invoke Get-CollaboratorPermissionResult -Times 0
        }

        It 'treats a definitive HTTP 404 as untrusted and caches it (one API call)' {
            Mock Get-CollaboratorPermissionResult { [pscustomobject]@{ ExitCode = 1; Permission = ''; StdErr = 'gh: Not Found (HTTP 404)' } }
            Test-ReviewOptionLoginTrusted -Login 'outsider' | Should -BeFalse
            Test-ReviewOptionLoginTrusted -Login 'outsider' | Should -BeFalse
            Should -Invoke Get-CollaboratorPermissionResult -Times 1
        }

        It 'retries a transient error and does not cache the undecided result' {
            Mock Get-CollaboratorPermissionResult { [pscustomobject]@{ ExitCode = 1; Permission = ''; StdErr = 'gh: Server Error (HTTP 503)' } }
            Test-ReviewOptionLoginTrusted -Login 'maintainer' -MaxAttempts 3 | Should -BeFalse
            Should -Invoke Get-CollaboratorPermissionResult -Times 3
            # Not cached: a later lookup tries again rather than staying downgraded.
            Test-ReviewOptionLoginTrusted -Login 'maintainer' -MaxAttempts 3 | Should -BeFalse
            Should -Invoke Get-CollaboratorPermissionResult -Times 6
        }

        It 'recovers when a transient error clears on a later attempt' {
            $script:permAttempt = 0
            Mock Get-CollaboratorPermissionResult {
                $script:permAttempt++
                if ($script:permAttempt -eq 1) {
                    [pscustomobject]@{ ExitCode = 1; Permission = ''; StdErr = 'gh: Server Error (HTTP 503)' }
                } else {
                    [pscustomobject]@{ ExitCode = 0; Permission = 'admin'; StdErr = '' }
                }
            }
            Test-ReviewOptionLoginTrusted -Login 'maintainer' -MaxAttempts 3 | Should -BeTrue
            Should -Invoke Get-CollaboratorPermissionResult -Times 2
        }
    }

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
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-new-comments-or-commits'
    }

    It 'accepts a non-command PR author comment after the latest AI Summary' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 2 -Body 'I pushed the requested update.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-author-comment-after-ai-summary'
    }

    It 'rejects a non-author maintainer comment after the latest AI Summary' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 2 -Body 'Could you please check the AI suggestions?' -CreatedAt '2026-05-31T09:45:00Z' -Login 'kubaflo' -Kind 'review'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z' -Login 'kubaflo'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-new-comments-or-commits'
    }

    It 'uses AI Summary creation time as the activity checkpoint when the summary was edited later' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T10:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 2 -Body 'I pushed the requested update before the summary edit.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-author-comment-after-ai-summary'
    }

    It 'selects the newest AI Summary by creation time instead of edit time' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody -Sha '1111111') -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T11:00:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 2 -Body (New-AISummaryBody -Sha '2222222') -CreatedAt '2026-05-31T10:00:00Z' -UpdatedAt '2026-05-31T10:00:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 3 -Body 'Follow-up after the latest summary.' -CreatedAt '2026-05-31T10:15:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:30:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha '2222222abcdef' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-author-comment-after-ai-summary'
    }

    It 'ignores forged AI Summary comments from non-bots' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -Login 'dev-user' -Type 'User'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-ai-summary'
    }

    It 'ignores AI Summary comments from bots outside the allowlisted logins' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -Login 'dependabot[bot]' -Type 'Bot'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-ai-summary'
    }

    It 'ignores AI Summary comments from github-actions[bot]' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody -Sha 'abcdef1') -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'github-actions[bot]' -Type 'Bot'
            New-TestComment -Id 2 -Body 'A meaningful follow-up after the summary.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-ai-summary'
    }

    It 'recognizes AI Summary comments from the MauiBot account' {
        $comments = @(
            New-TestComment -Id 4239128463 -Body (New-AISummaryBody -Sha '6e9af5b') -CreatedAt '2026-04-13T19:37:50Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 4658057491 -Body '@kubaflo , Addressed AI concerns.' -CreatedAt '2026-06-09T08:50:38Z'
            New-TestComment -Id 4659999999 -Body '/review rerun' -CreatedAt '2026-06-09T09:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 4659999999 -CurrentHeadSha '6e9af5bc8b5d0023400d653500951fb46df44170' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-author-comment-after-ai-summary'
    }

    It 'uses the first session marker from an AI Summary' {
        $body = @"
<!-- AI Summary -->
<!-- SESSION:1111111 START -->
old
<!-- SESSION:2222222 START -->
new
"@

        Get-LatestReviewedSha -AISummaryBody $body | Should -Be '1111111'
    }

    It 'does not count repeated rerun commands as evidence' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 9 -Body '/review rerun' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'no-new-comments-or-commits'
    }

    It 'accepts a non-command PR author comment after the previous rerun command' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 8 -Body '/review rerun' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 9 -Body 'Follow-up detail after rerun request.' -CreatedAt '2026-05-31T09:50:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-author-comment-after-previous-rerun'
    }

    It 'does not reuse old activity from before a previous rerun command' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
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
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User' -Kind 'review'
            New-TestComment -Id 2 -Body 'Follow-up after the review.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -PRAuthorLogin 'dev-user'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-author-comment-after-ai-summary'
    }

    It 'accepts a current head SHA that differs from the latest reviewed session' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody -Sha 'abcdef1') -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'fedcba9876543210'

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'new-head-commit'
    }

    It 'accepts a commit after the previous rerun command' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
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
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z' -Login 'MauiBot' -Type 'User'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123'

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'bot-comment'
    }

    It 'is idempotent when ready-for-rerun label already exists' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -CurrentLabels @('s/agent-ready-for-rerun')

        $result.Eligible | Should -BeTrue
        $result.Reason | Should -Be 'label-already-present'
    }

    It 'rejects rerun commands while a review is already in progress' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 2 -Body 'Please look at the latest push.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 10 -Body '/review rerun' -CreatedAt '2026-05-31T10:00:00Z'
        )

        $result = Resolve-RerunEligibility -Comments $comments -Commits @() -CurrentCommentId 10 -CurrentHeadSha 'abcdef123' -CurrentLabels @('s/agent-review-in-progress')

        $result.Eligible | Should -BeFalse
        $result.Reason | Should -Be 'review-in-progress'
    }

    It 'builds deterministic rerun context with new comments and commits' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 2 -Body 'New author context.' -CreatedAt '2026-05-31T09:45:00Z'
            New-TestComment -Id 11 -Body 'Reviewer reminder.' -CreatedAt '2026-05-31T09:46:00Z' -Login 'reviewer'
            New-TestComment -Id 3 -Body '/review rerun' -CreatedAt '2026-05-31T09:50:00Z'
        )
        $commits = @(
            New-TestCommit -Sha 'fedcba9876543210' -Date '2026-05-31T09:48:00Z'
        )

        $context = New-RerunContextMarkdown -Comments $comments -Commits $commits -CurrentHeadSha 'fedcba9876543210' -PRAuthorLogin 'dev-user' -CurrentLabels @('s/agent-review-in-progress')

        $context | Should -Match '# Rerun Context'
        $context | Should -Match 'New non-command author comments: 1'
        $context | Should -Match 'New commits: 1'
        $context | Should -Match '`s/agent-ready-for-rerun` present: false'
        $context | Should -Match '`s/agent-review-in-progress` present: true'
        $context | Should -Match 'New author context'
        $context | Should -Not -Match 'Reviewer reminder'
        $context | Should -Match 'fedcba9'
        $context | Should -Not -Match '\| .*\/review rerun'
    }

    It 'renders normalized app-style bot authors in rerun context' {
        $comments = @(
            New-TestComment -Id 1 -Body (New-AISummaryBody) -CreatedAt '2026-05-31T09:00:00Z' -UpdatedAt '2026-05-31T09:30:00Z' -Login 'MauiBot' -Type 'User'
            New-TestComment -Id 3 -Body '/review rerun' -CreatedAt '2026-05-31T09:50:00Z'
        )

        $context = New-RerunContextMarkdown -Comments $comments -Commits @() -CurrentHeadSha 'abcdef123' -PRAuthorLogin 'app/dependabot'

        $context | Should -Match 'PR author: dependabot\[bot\]'
        $context | Should -Match 'New non-command author comments: 0'
    }
}
