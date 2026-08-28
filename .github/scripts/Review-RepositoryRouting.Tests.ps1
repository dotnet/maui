#Requires -Modules Pester

Set-StrictMode -Version Latest

BeforeAll {
    $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
    $script:PipelinePath = Join-Path $script:RepoRoot 'eng/pipelines/ci-copilot.yml'
    $script:PipelineSource = Get-Content -Raw -LiteralPath $script:PipelinePath

    $script:RoutedPowerShellPaths = [ordered]@{
        Review = Join-Path $script:RepoRoot '.github/scripts/Review-PR.ps1'
        Summary = Join-Path $script:RepoRoot '.github/scripts/post-ai-summary-comment.ps1'
        Inline = Join-Path $script:RepoRoot '.github/scripts/post-inline-review.ps1'
        Finalize = Join-Path $script:RepoRoot '.github/scripts/apply-pr-finalize.ps1'
        DetectTests = Join-Path $script:RepoRoot '.github/scripts/shared/Detect-TestsInDiff.ps1'
        PrepareUi = Join-Path $script:RepoRoot '.github/scripts/shared/Prepare-UITestFailureAnalysis.ps1'
        Cleanup = Join-Path $script:RepoRoot '.github/scripts/shared/Remove-StaleMauiBotComments.ps1'
        Verify = Join-Path $script:RepoRoot '.github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1'
        DetectUi = Join-Path $script:RepoRoot 'eng/scripts/detect-ui-test-categories.ps1'
    }

    $script:RoutedPowerShellSources = [ordered]@{}
    foreach ($entry in $script:RoutedPowerShellPaths.GetEnumerator()) {
        $script:RoutedPowerShellSources[$entry.Key] =
            Get-Content -Raw -LiteralPath $entry.Value
    }

    function Get-PowerShellAst {
        param([Parameter(Mandatory = $true)][string]$Source)

        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseInput(
            $Source,
            [ref]$tokens,
            [ref]$errors)
        if ($errors -and $errors.Count -gt 0) {
            throw ($errors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
        }
        return $ast
    }

    function Get-UnqualifiedGhPrCommands {
        param([Parameter(Mandatory = $true)][string]$Source)

        $ast = Get-PowerShellAst -Source $Source
        return @($ast.FindAll({
            $args[0] -is [System.Management.Automation.Language.CommandAst] -and
            $args[0].GetCommandName() -eq 'gh' -and
            $args[0].Extent.Text -match '(?s)(?:^|\s)gh\s+pr\s+(?:view|list|diff|comment)\b' -and
            $args[0].Extent.Text -notmatch '(?s)\s--repo\s+'
        }, $true))
    }

    function Get-HardcodedCurrentPrRoutes {
        param([Parameter(Mandatory = $true)][string]$Source)

        return @([regex]::Matches(
            $Source,
            '(?i)repos/dotnet/maui/(?:pulls|issues)/(?:\$(?:PRNumber|prNumber)|\$\{(?:PR_NUM|PARAM_PR_NUMBER)\})'))
    }

    function Get-HardcodedGhRepositoryArguments {
        param([Parameter(Mandatory = $true)][string]$Source)

        return @([regex]::Matches(
            $Source,
            '(?i)(?:--repo[''"]?\s+[''"]dotnet/maui[''"]|[''"]--repo[''"]\s*,\s*[''"]dotnet/maui[''"])'))
    }

    function Get-SelectorlessGhPrViewCommands {
        param([Parameter(Mandatory = $true)][string]$Source)

        $ast = Get-PowerShellAst -Source $Source
        return @($ast.FindAll({
            if ($args[0] -isnot [System.Management.Automation.Language.CommandAst] -or
                $args[0].GetCommandName() -ne 'gh') {
                return $false
            }

            $parts = @($args[0].CommandElements | ForEach-Object { $_.Extent.Text })
            return $parts.Count -ge 3 -and
                $parts[1] -eq 'pr' -and
                $parts[2] -eq 'view' -and
                ($parts.Count -eq 3 -or $parts[3] -match '^-')
        }, $true))
    }
}

Describe 'Repository-routing invariant scanners' {
    It 'fires on an unqualified gh PR lookup' {
        $bad = 'gh pr view $PRNumber --json title'

        @(Get-UnqualifiedGhPrCommands -Source $bad).Count | Should -Be 1
    }

    It 'fires on a hard-coded current-PR REST endpoint' {
        $bad = 'gh api "repos/dotnet/maui/pulls/$PRNumber"'

        @(Get-HardcodedCurrentPrRoutes -Source $bad).Count | Should -Be 1
    }

    It 'fires on a hard-coded gh repository argument in an inline pipeline command' {
        $bad = "-Arguments @('pr', 'comment', `$prNumber, '--repo', 'dotnet/maui')"
        $good = "-Arguments @('pr', 'comment', `$prNumber, '--repo', `$reviewRepository)"

        @(Get-HardcodedGhRepositoryArguments -Source $bad).Count | Should -Be 1
        @(Get-HardcodedGhRepositoryArguments -Source $good).Count | Should -Be 0
    }

    It 'fires on gh pr view without a pull request or branch selector' {
        $bad = 'gh pr view --repo kubaflo/maui --json number'
        $good = 'gh pr view $PRNumber --repo kubaflo/maui --json number'

        @(Get-SelectorlessGhPrViewCommands -Source $bad).Count | Should -Be 1
        @(Get-SelectorlessGhPrViewCommands -Source $good).Count | Should -Be 0
    }
}

Describe 'Every current-PR PowerShell boundary follows the selected repository' {
    It 'qualifies every gh pr command and proves the scan reached real commands' {
        $allCommands = @()
        foreach ($source in $script:RoutedPowerShellSources.Values) {
            $ast = Get-PowerShellAst -Source $source
            $allCommands += @($ast.FindAll({
                $args[0] -is [System.Management.Automation.Language.CommandAst] -and
                $args[0].GetCommandName() -eq 'gh' -and
                $args[0].Extent.Text -match '(?s)(?:^|\s)gh\s+pr\s+(?:view|list|diff|comment)\b'
            }, $true))
        }

        @($allCommands).Count | Should -BeGreaterOrEqual 9
        foreach ($command in $allCommands) {
            $command.Extent.Text | Should -Match '(?s)\s--repo\s+\$Repository\b'
        }

        $unqualified = @()
        foreach ($source in $script:RoutedPowerShellSources.Values) {
            $unqualified += @(Get-UnqualifiedGhPrCommands -Source $source)
        }
        @($unqualified).Count | Should -Be 0

        $selectorless = @()
        foreach ($source in $script:RoutedPowerShellSources.Values) {
            $selectorless += @(Get-SelectorlessGhPrViewCommands -Source $source)
        }
        @($selectorless).Count | Should -Be 0
    }

    It 'uses repository variables for a non-vacuous set of REST mutations and reads' {
        $allSource = $script:RoutedPowerShellSources.Values -join "`n"
        $selectedRoutes = [regex]::Matches(
            $allSource,
            'repos/\$(?:Repository|Repo)/(?:pulls|issues)/\$')

        @($selectedRoutes).Count | Should -BeGreaterOrEqual 16
        @(Get-HardcodedCurrentPrRoutes -Source $allSource).Count | Should -Be 0
    }

    It 'propagates repository identity through nested cleanup helpers' {
        $cleanup = $script:RoutedPowerShellSources.Cleanup
        $summary = $script:RoutedPowerShellSources.Summary
        $review = $script:RoutedPowerShellSources.Review

        $cleanup | Should -Match '(?s)Get-GitHubIssueComments\s+-PRNumber \$PRNumber\s+-Repository \$Repository'
        $cleanup | Should -Match '(?s)Get-GitHubPullRequestReviews\s+-PRNumber \$PRNumber\s+-Repository \$Repository'
        $cleanup | Should -Match '(?s)Dismiss-MauiBotPullRequestReview\s+.*?-Repository \$Repository'
        $summary | Should -Match '(?s)Hide-StaleMauiBotIssueComments\s+.*?-Repository \$Repository'
        $summary | Should -Match '(?s)Hide-StaleMauiBotPullRequestReviews\s+.*?-Repository \$Repository'
        $review | Should -Match '(?s)Dismiss-StaleMauiBotTryFixReviews\s+.*?-Repository \$Repository'
    }
}

Describe 'Review-PR immutable repository snapshot' {
    BeforeAll {
        $script:ReviewSource = $script:RoutedPowerShellSources.Review
    }

    It 'reads and fetches the selected repository, then rejects a different fetched head' {
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '-Arguments @(''api'', "repos/$Repository/pulls/$PRNumber")'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '$reviewRepositoryUrl = "https://github.com/$Repository.git"'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            'git fetch $reviewRepositoryUrl "pull/$PRNumber/head:$tempBranch"'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            'if ($reviewedPrHeadSha -ne $expectedPrHeadSha)'))
        $script:ReviewSource | Should -Match 'PR head advanced while setup was resolving'
    }

    It 'freezes the selected repository current target-branch tip instead of the PR historical base' {
        $script:ReviewSource | Should -Match ([regex]::Escape(
            'Get-FetchedRepositoryBranchSha `'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '-RepositoryUrl $reviewRepositoryUrl `'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '[string]$ResolvedBaseCommit = '''''))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '[string]$ResolvedBaseRef = '''''))
        $script:ReviewSource | Should -Not -Match ([regex]::Escape(
            '$prInfo.base.sha'))
    }

    It 'persists and revalidates repository, base, head, and review-tree identities' {
        $script:ReviewSource | Should -Match '(?s)\[ordered\]@\{\s*repository = \$Repository\s*baseRefName = \$baseRefName\s*baseSha = \$reviewedBaseSha\s*prHeadSha = \$reviewedPrHeadSha\s*reviewTreeSha = \$reviewedTreeSha'
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '[string]$reviewSnapshot.repository -ne $Repository'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '[string]$reviewSnapshot.baseSha -notmatch ''^[0-9a-fA-F]{40}$'''))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '[string]$reviewSnapshot.prHeadSha -notmatch ''^[0-9a-fA-F]{40}$'''))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '[string]$reviewSnapshot.reviewTreeSha -notmatch ''^[0-9a-fA-F]{40}$'''))
    }

    It 'passes repository and immutable base through analysis, gates, posting, and metadata updates' {
        $script:ReviewSource | Should -Match '(?s)& \$regressionScript\s+.*?-Repo \$Repository\s+.*?-BaseCommit \(\[string\]\$reviewSnapshot\.baseSha\)'
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '-File $detectScript -PrNumber "$PRNumber" -Repository $Repository -BaseCommit ([string]$reviewSnapshot.baseSha)'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '-File "$verifyScript" -Platform $gatePlatform -PRNumber $PRNumber -Repository $Repository -BaseBranch ([string]$reviewSnapshot.baseSha)'))
        $script:ReviewSource | Should -Match ([regex]::Escape(
            '$reviewOutput = & $reviewScript -PRNumber $PRNumber -Repository $Repository'))
        $script:ReviewSource | Should -Match '(?s)PRNumber = \$PRNumber\s*ContentFile = \$finalizeContent\s*WinnerFile = \$finalizeWinner\s*Repo = \$Repository'
        $script:ReviewSource | Should -Match '(?s)-ExpectedHeadSha \$ReviewedCommit\s+.*?-Owner \$repositoryOwner\s+.*?-Repo \$repositoryName'
    }
}

Describe 'Selected-repository helper contracts' {
    It 'keeps changed-test and UI-failure discovery on the selected repository' {
        $detect = $script:RoutedPowerShellSources.DetectTests
        $prepare = $script:RoutedPowerShellSources.PrepareUi

        $detect | Should -Match ([regex]::Escape(
            'gh api "repos/$Repository/pulls/$PRNumber/files"'))
        $detect | Should -Match ([regex]::Escape(
            'gh pr list --repo $Repository --head $currentBranch --json baseRefName --limit 1'))
        $detect | Should -Match ([regex]::Escape(
            'gh pr diff $PRNumber --repo $Repository --name-only'))

        @([regex]::Matches(
            $prepare,
            'gh pr diff \$PRNumber --repo \$Repository')).Count | Should -Be 2
    }

    It 'fails closed while binding the changed-test gate to the immutable selected base' {
        $verify = $script:RoutedPowerShellSources.Verify

        $verify | Should -Match ([regex]::Escape(
            'gh pr view $PRNumber --repo $Repository --json baseRefName'))
        $verify | Should -Match ([regex]::Escape(
            'git fetch "https://github.com/$Repository.git" "refs/heads/$detectedBase" --no-tags'))
        $verify | Should -Match ([regex]::Escape(
            '$detectedBaseSha = ([string](git rev-parse FETCH_HEAD 2>$null)).Trim()'))
        $verify | Should -Match 'throw "Could not fetch the current target branch ''\$detectedBase'' for \$Repository#\$PRNumber\.'
        $verify | Should -Match ([regex]::Escape(
            'git update-ref "refs/remotes/origin/$detectedBase" $detectedBaseSha'))
        $verify | Should -Match 'throw "Could not bind origin/\$detectedBase to immutable base \$detectedBaseSha\.'
    }

    It 'does not replace selected manual-review base state from pipeline origin' {
        $detectUi = $script:RoutedPowerShellSources.DetectUi

        $detectUi | Should -Match ([regex]::Escape(
            '$repoName = $Repository'))
        $detectUi | Should -Match ([regex]::Escape(
            'Invoke-Git fetch _detect_base "refs/heads/$TargetBranch" --no-tags --depth=200'))
        $detectUi | Should -Match ([regex]::Escape(
            '$baseSha = ([string](& git rev-parse FETCH_HEAD 2>$null)).Trim()'))
        $detectUi | Should -Match ([regex]::Escape(
            'Invoke-Git fetch _detect_base $baseSha --no-tags --depth=200'))
        $detectUi | Should -Match ([regex]::Escape(
            'Invoke-Git update-ref refs/remotes/origin/$TargetBranch $baseSha'))
        $detectUi | Should -Not -Match ([regex]::Escape(
            '$baseSha = [string]$pr.base.sha'))
        $detectUi | Should -Match '(?s)if \(\$isManualPrTest -and \$baseSha -match.*?Invoke-Git update-ref "refs/remotes/origin/\$targetBranch" \$baseSha\s*\}\s*else\s*\{\s*Invoke-Git fetch origin'
    }

    It 'uses selected-repository commit links and finalizer endpoints' {
        $summary = $script:RoutedPowerShellSources.Summary
        $finalize = $script:RoutedPowerShellSources.Finalize

        $summary | Should -Match ([regex]::Escape(
            '"https://github.com/$Repository/commit/$commitFull"'))
        $summary | Should -Match ([regex]::Escape(
            '"https://github.com/$Repository/commit/$currentHeadSha"'))
        $finalize | Should -Match ([regex]::Escape(
            '$prOutput = @(& gh api "repos/$Repo/pulls/$PRNumber"'))
        $finalize | Should -Match ([regex]::Escape(
            '$ghArgs = @(''api'', "repos/$Repo/pulls/$PRNumber", ''--method'', ''PATCH'''))
    }
}

Describe 'Pipeline review routing' {
    It 'declares and validates the repository parameter before using it' {
        $script:PipelineSource | Should -Match '(?s)- name: ReviewRepository\s+displayName:.*?\s+type: string\s+default: ''dotnet/maui'''
        $script:PipelineSource | Should -Match 'ReviewRepository must use a valid owner/name form'
        $script:PipelineSource | Should -Match 'PARAM_REVIEW_REPOSITORY: \$\{\{ parameters\.ReviewRepository \}\}'
    }

    It 'routes both metadata resolvers and immutable fetches through the selected repository' {
        $script:PipelineSource | Should -Match 'gh pr view "\$\{PARAM_PR_NUMBER\}" --repo "\$\{REVIEW_REPOSITORY\}" --json baseRefName'
        $script:PipelineSource | Should -Match 'https://api\.github\.com/repos/\$\{REVIEW_REPOSITORY\}/pulls/\$\{PARAM_PR_NUMBER\}'
        $script:PipelineSource | Should -Match 'baseRefName,headRefOid'
        $script:PipelineSource | Should -Not -Match 'baseRefOid'
        $script:PipelineSource | Should -Match 'git fetch "\$\{REVIEW_REPOSITORY_URL\}" "refs/heads/\$\{BASE_REF\}" --no-tags'
        $script:PipelineSource | Should -Match 'BASE_SHA=\$\(git rev-parse FETCH_HEAD\)'
        $script:PipelineSource | Should -Match 'git fetch "\$\{REVIEW_REPOSITORY_URL\}" "pull/\$\{PARAM_PR_NUMBER\}/head" --no-tags'
        $script:PipelineSource | Should -Match ([regex]::Escape('-ResolvedBaseCommit "$(resolvedBaseSha)"'))
        $script:PipelineSource | Should -Match ([regex]::Escape('-ResolvedBaseRef "$(resolvedBaseRef)"'))
        $script:PipelineSource | Should -Match 'PARAM_BASE_COMMIT: \$\(resolvedBaseSha\)'
    }

    It 'passes repository identity to every Review-PR phase and validates the Setup snapshot' {
        @([regex]::Matches(
            $script:PipelineSource,
            '-Repository "\$\{\{ parameters\.ReviewRepository \}\}"')).Count |
            Should -BeGreaterOrEqual 4
        $script:PipelineSource | Should -Match '\{0\}\|\{1\}\|\{2\}\|\{3\}.*snapshot\.repository'
        $script:PipelineSource | Should -Match '\[ "\$REVIEWED_REPOSITORY" != "\$\{\{ parameters\.ReviewRepository \}\}" \]'
    }

    It 'routes Stage 3 review, labels, fallback cleanup, and lock cleanup together' {
        $selectedRoutes = [regex]::Matches(
            $script:PipelineSource,
            'repos/(?:\$\{REVIEW_REPOSITORY\}|\$reviewRepository)/(?:pulls|issues)/')
        @($selectedRoutes).Count | Should -BeGreaterOrEqual 10
        @(Get-HardcodedCurrentPrRoutes -Source $script:PipelineSource).Count | Should -Be 0
        @(Get-HardcodedGhRepositoryArguments -Source $script:PipelineSource).Count | Should -Be 0

        $script:PipelineSource | Should -Match 'commit_id = "\$\(reviewedPrHeadSha\)"'
        $script:PipelineSource | Should -Match '(?s)Apply-AgentLabels\s+.*?-Owner \$reviewOwner\s+.*?-Repo \$reviewRepo'
        $cleanupCalls = @($script:PipelineSource -split '\r?\n' |
            Where-Object { $_ -match 'Hide-StaleMauiBotIssueComments' })
        @($cleanupCalls).Count | Should -BeGreaterOrEqual 3
        @($cleanupCalls | Where-Object { $_ -notmatch '-Repository\s+' }).Count | Should -Be 0
        $script:PipelineSource | Should -Match 'templateParameters\.ReviewRepository // "dotnet/maui"'
        $script:PipelineSource | Should -Match 'repos/\$\{REVIEW_REPOSITORY\}/issues/\$\{PR_NUM\}/labels/s%2Fagent-review-in-progress'
    }
}
