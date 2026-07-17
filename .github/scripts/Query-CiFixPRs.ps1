#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds bounded context for open ci-fix PRs watched by the ci-status-fix gh-aw workflow.
#>

param(
    [int]$MaxPRs = 20,
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [string]$OutputPath = "CustomAgentLogsTmp/CiFixScanner/candidates.json",
    [string]$TitlePrefix = '[ci-fix]',
    # The base branch this workflow instance owns. Each ci-status-fix twin watches ONLY
    # PRs targeting its own base (main -> 'main', net11 -> 'net11.0'). See the baseRefName
    # guard in the candidate loop for why this is load-bearing, not cosmetic.
    [string]$BaseBranch = 'main'
)

$ErrorActionPreference = 'Stop'
# Pin native-command error handling OFF so `& gh ... 2>&1` in Invoke-GhCommand never
# throws at invocation on a non-zero exit (404 on an orphaned SHA, transient rate-limit).
# The whole prefetch's graceful degradation depends on the -AllowFailure path returning
# $null on expected failures rather than terminating; a future runner image or profile
# that flips this preference to $true would bypass -AllowFailure and crash the loop.
$PSNativeCommandUseErrorActionPreference = $false

$BotLogins = @(
    'github-actions[bot]',
    'github-actions',
    # 'web-flow' is deliberately absent. It represents GitHub web UI operations, never a
    # ci-fixer attempt, so its commits must not consume the bot's attempt budget.
    'app/github-actions',
    'dotnet-maestro[bot]',
    'azure-pipelines[bot]',
    'dotnet-policy-service[bot]',
    # dotnet-bot, MauiBot and maui-bot are MAUI/dotnet automation accounts whose logins
    # do NOT carry the '[bot]' suffix, so classify them as bot actors for attempt accounting
    # and Track C response-marker filtering.
    # The repo posts CI/review automation as 'maui-bot' / 'MauiBot' (see
    # .github/scripts/shared/Remove-StaleMauiBotComments.ps1 and the ci-copilot pipeline);
    # 'mauibot' covers 'MauiBot' case-insensitively, but the hyphenated 'maui-bot' login is
    # a distinct string and must be listed explicitly. Compared case-insensitively
    # (Test-IsHumanLogin lowercases the login first).
    'dotnet-bot',
    'mauibot',
    'maui-bot',
    'maui-bot[bot]'
)

# NOTE: 'action_required' is deliberately EXCLUDED. That conclusion means a human
# must act (an Actions approval gate, or an integration awaiting a manual run) —
# it reports status=completed, so treating it as a failure would let a settled head
# fall into the red -> classify/advance path (Step 3.5 gate 4) and push a fix on top
# of CI that was never actually exercised. For this workflow — whose whole round-1
# premise is "CI waits for a maintainer /azp run" — action_required belongs with the
# WAIT states. Get-HeadCheckState routes it to 'neutral' (Step 3.5 gate 2) EVEN WHEN
# other legs are green, so a build-green / uitests-not-yet-run head is never surfaced
# as a validated green (which would invite merging a fix whose gated legs never ran).
#
# 'startup_failure' and 'stale' are GraphQL CheckConclusionState values, NOT part of
# the REST check-runs 'conclusion' enum this script actually reads via
# /commits/{sha}/check-runs (success/failure/neutral/cancelled/skipped/timed_out/
# action_required/null). They are listed here DEFENSIVELY: they are a zero-cost no-op
# while this endpoint never emits them, but if GitHub ever surfaces them through the
# REST check-runs API (or the fetch is switched to GraphQL), both are aborts that
# produced no genuine pass and must be kept OUT of the 'success' bucket — treated as
# failures alongside 'cancelled'/'timed_out'. Gate 4 + Step 4.7 bucket (a) then
# classify such a leg as an infra flake (annotate, no attempt burned) — never a
# caused-by-fix advance. Mirrors dotnet/maui's review-test-failures skill, which
# groups cancelled/timed_out/startup_failure/stale as aborted-failing checks.
$FailureCheckConclusions = @('failure', 'timed_out', 'cancelled', 'startup_failure', 'stale')
$FailureStatusStates = @('failure', 'error')

# The attempt ceiling is a FIXED workflow contract — it is NEVER read from the
# mutable 'ci-fix-attempts: N/M' PR-body marker denominator. Trusting M from the
# body would let a corrupted/injected denominator (e.g. 1/9999) raise the cap, or a
# stale marker slip past it. Get-CiFixMarkers pins attemptMax to this constant, and
# the actionable gate counts attempts as max(marker numerator, bot-commit count) so
# the safety bound never depends solely on an LLM-authored body marker.
$AttemptMax = 10

function Invoke-GhCommand {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description,
        [switch]$AllowFailure
    )

    $output = & gh @Arguments 2>&1
    $exitCode = $LASTEXITCODE

    # `2>&1` folds gh's stderr into the pipeline as ErrorRecord objects while real
    # stdout stays as strings. Separate the two by type so a success-path caller
    # never receives a stderr line (gh progress/deprecation/rate-limit notices)
    # concatenated into the JSON it is about to parse. On failure, both streams are
    # surfaced in the exception/warning message for diagnosability.
    $stdoutText = (@($output | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] }) |
        ForEach-Object { $_.ToString() }) -join "`n"
    $stderrText = (@($output | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] }) |
        ForEach-Object { $_.ToString() }) -join "`n"

    if ($exitCode -ne 0) {
        $message = "gh $Description failed with exit code $exitCode."
        $detail = (@($stderrText, $stdoutText) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join ' '
        if (-not [string]::IsNullOrWhiteSpace($detail)) {
            $message = "$message Output: $detail"
        }

        if ($AllowFailure) {
            Write-Warning $message
            return $null
        }

        throw $message
    }

    return $stdoutText
}

function ConvertFrom-JsonLines {
    param([AllowNull()][string]$JsonLines)

    if ([string]::IsNullOrWhiteSpace($JsonLines)) {
        return @()
    }

    $items = @()
    foreach ($line in ($JsonLines -split "`n")) {
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        $items += ($line | ConvertFrom-Json)
    }

    return @($items)
}

function Test-IsHumanLogin {
    param([AllowNull()][string]$Login)

    if ([string]::IsNullOrWhiteSpace($Login)) {
        return $false
    }

    $normalized = $Login.Trim().ToLowerInvariant()
    if ($BotLogins -contains $normalized) {
        return $false
    }

    if ($normalized.EndsWith('[bot]', [StringComparison]::OrdinalIgnoreCase)) {
        return $false
    }

    return $true
}

function Get-PullRequestBody {
    param([int]$Number)

    $bodyJson = Invoke-GhCommand `
        -Arguments @('pr', 'view', "$Number", '--repo', "$Owner/$Repo", '--json', 'body') `
        -Description "read PR #$Number body" `
        -AllowFailure

    if ($null -eq $bodyJson) {
        return [pscustomobject]@{ Succeeded = $false; Body = '' }
    }

    $body = ($bodyJson | ConvertFrom-Json).body
    if ($null -eq $body) {
        $body = ''
    }

    return [pscustomobject]@{ Succeeded = $true; Body = [string]$body }
}

function Get-CiFixMarkers {
    param([AllowNull()][string]$Body)

    $refsIssue = $null
    $attempt = $null

    if ($Body -match 'Refs:\s*dotnet/maui#(\d+)') {
        # [long]::TryParse, NOT a [long]/[int] cast: the regex '(\d+)' is unbounded, so a
        # malformed body with an oversized issue number throws a terminating
        # OverflowException on a direct cast under $ErrorActionPreference='Stop' — [int]
        # above ~2.1B, [long] above ~9.2e18 — aborting the ENTIRE prefetch (candidates.json
        # never written → the watch loop stalls for every PR). TryParse fails this one
        # marker closed to $null (treated as "no Refs") instead of killing the whole run.
        $parsedRefs = [long]0
        if ([long]::TryParse($Matches[1], [ref]$parsedRefs)) { $refsIssue = $parsedRefs }
    }

    # Parse ONLY the numerator (attempts made). The denominator is deliberately
    # ignored: the ceiling is the fixed $AttemptMax constant, never the mutable body.
    if ($Body -match 'ci-fix-attempts:\s*(\d+)\s*/\s*\d+') {
        # [long]::TryParse, NOT a [long]/[int] cast: the attempt marker lives in the PR
        # body, which the LLM can rewrite (update_pull_request) and any triager can edit,
        # and the regex '(\d+)' is unbounded. A crafted marker such as
        # `ci-fix-attempts: 999...(>Int64)/10` overflows a direct [long] cast, throws under
        # $ErrorActionPreference='Stop', and — with no try/catch on this path — aborts the
        # whole prefetch, stalling EVERY watched PR. TryParse fails closed to $null; a null
        # marker contributes 0 downstream, so the trustworthy bot-commit floor governs.
        # Mirrors the TryParse review-id hardening in Get-CiFixEngagement.
        $parsedAttempt = [long]0
        if ([long]::TryParse($Matches[1], [ref]$parsedAttempt)) { $attempt = $parsedAttempt }
    }

    return [pscustomobject]@{
        refsIssue  = $refsIssue
        attempt    = $attempt
        attemptMax = $AttemptMax
    }
}

function Get-HeadCheckState {
    param([string]$HeadSha)

    $checkRunsKnown = $true
    $statusesKnown = $true

    $checkRunLines = Invoke-GhCommand `
        -Arguments @('api', "repos/$Owner/$Repo/commits/$HeadSha/check-runs?per_page=100", '--paginate', '--jq', '.check_runs[]') `
        -Description "read check-runs for $HeadSha" `
        -AllowFailure
    if ($null -eq $checkRunLines) {
        $checkRunsKnown = $false
        $checkRuns = @()
    }
    else {
        $checkRuns = @(ConvertFrom-JsonLines -JsonLines $checkRunLines)
    }

    # The combined-status endpoint returns its .statuses[] array paginated at 30 by
    # default; request per_page=100 so a repo with many legacy commit-status contexts
    # is still read in full. (A single object is returned, so no --paginate/JSON-lines
    # handling is needed the way check-runs above require.)
    $statusJson = Invoke-GhCommand `
        -Arguments @('api', "repos/$Owner/$Repo/commits/$HeadSha/status?per_page=100") `
        -Description "read commit statuses for $HeadSha" `
        -AllowFailure
    if ($null -eq $statusJson) {
        $statusesKnown = $false
        $statuses = @()
        $combinedState = ''
    }
    else {
        $statusResult = $statusJson | ConvertFrom-Json
        $statuses = @($statusResult.statuses)
        # GitHub computes the combined-status top-level .state over EVERY context (all
        # pages), so it is 'failure' iff at least one legacy commit-status actually
        # failed. Fold ONLY the 'failure' value into failure detection as a
        # pagination-proof backstop: if a repo ever exceeds the 100-context page size
        # and the failing context lands beyond page 1 of .statuses[], this still catches
        # the red. It is a strict true-positive — .state is never 'failure' unless a
        # context failed. Deliberately do NOT treat .state -eq 'pending' as unsettled:
        # the combined-status API returns 'pending' whenever there are ZERO legacy
        # statuses, which is the norm for check-run-only heads (every MAUI commit), so
        # keying settledness off it would wedge the loop permanently.
        $combinedState = ([string]$statusResult.state).ToLowerInvariant()
    }

    $failedCheckRuns = @(
        $checkRuns |
            Where-Object { $_.conclusion -and ($FailureCheckConclusions -contains ([string]$_.conclusion).ToLowerInvariant()) } |
            ForEach-Object {
                [pscustomobject]@{
                    name       = [string]$_.name
                    conclusion = [string]$_.conclusion
                }
            }
    )

    if (-not $checkRunsKnown -or -not $statusesKnown) {
        return [pscustomobject]@{
            Succeeded         = $false
            checksSettled     = $false
            overallConclusion = 'unknown'
            failedLegs        = @($failedCheckRuns)
        }
    }

    $unsettledCheckRuns = @($checkRuns | Where-Object { ([string]$_.status).ToLowerInvariant() -ne 'completed' })
    $pendingStatuses = @($statuses | Where-Object { ([string]$_.state).ToLowerInvariant() -eq 'pending' })
    $failedStatuses = @($statuses | Where-Object { $FailureStatusStates -contains ([string]$_.state).ToLowerInvariant() })
    # Failing commit statuses (common for AzDO/third-party integrations that report
    # via the combined-status API rather than check-runs) are folded into failedLegs
    # alongside failed check-runs, so downstream "which leg failed?" classification
    # (Step 3.5 gate 4) sees every failing signal — not just the check-run ones.
    $failedStatusLegs = @(
        $failedStatuses |
            ForEach-Object {
                [pscustomobject]@{
                    name       = [string]$_.context
                    conclusion = [string]$_.state
                }
            }
    )
    $successfulCheckRuns = @($checkRuns | Where-Object { $_.conclusion -and ([string]$_.conclusion).ToLowerInvariant() -eq 'success' })
    $successfulStatuses = @($statuses | Where-Object { ([string]$_.state).ToLowerInvariant() -eq 'success' })
    # A head can report status=completed yet still be waiting on a human: an
    # 'action_required' check-run (a manual-approval gate, or MAUI's /azp-gated
    # uitests/devicetests legs that stay action_required until a maintainer triggers
    # them) means CI was never fully exercised. It is deliberately absent from
    # $FailureCheckConclusions, so it must be detected explicitly here to keep such a
    # head OUT of the 'success' bucket below.
    $actionRequiredCheckRuns = @($checkRuns | Where-Object { $_.conclusion -and ([string]$_.conclusion).ToLowerInvariant() -eq 'action_required' })

    $checksSettled = ($unsettledCheckRuns.Count -eq 0) -and ($pendingStatuses.Count -eq 0)
    $overallConclusion = 'neutral'
    if ($failedCheckRuns.Count -gt 0 -or $failedStatuses.Count -gt 0 -or $combinedState -eq 'failure') {
        $overallConclusion = 'failure'
    }
    elseif (-not $checksSettled) {
        $overallConclusion = 'pending'
    }
    elseif ($actionRequiredCheckRuns.Count -gt 0) {
        # Settled-but-waiting-on-human: route to WAIT (Step 3.5 gate 2) EVEN WHEN other
        # legs are green, so a build-green / uitests-not-yet-run head is never
        # mis-surfaced as a validated green. 'neutral' (not 'pending') preserves the
        # invariant that 'pending' means checksSettled == false.
        $overallConclusion = 'neutral'
    }
    elseif ($successfulCheckRuns.Count -gt 0 -or $successfulStatuses.Count -gt 0) {
        $overallConclusion = 'success'
    }

    return [pscustomobject]@{
        Succeeded         = $true
        checksSettled     = [bool]$checksSettled
        overallConclusion = $overallConclusion
        failedLegs        = @($failedCheckRuns + $failedStatusLegs)
    }
}

function Get-PullRequestWatchState {
    param([int]$Number)

    $allSucceeded = $true

    $issueCommentLines = Invoke-GhCommand `
        -Arguments @('api', "repos/$Owner/$Repo/issues/$Number/comments?per_page=100", '--paginate', '--jq', '.[]') `
        -Description "read issue comments for PR #$Number" `
        -AllowFailure
    if ($null -eq $issueCommentLines) {
        $allSucceeded = $false
        $issueComments = @()
    }
    else {
        $issueComments = @(ConvertFrom-JsonLines -JsonLines $issueCommentLines)
    }

    $commitLines = Invoke-GhCommand `
        -Arguments @('api', "repos/$Owner/$Repo/pulls/$Number/commits?per_page=100", '--paginate', '--jq', '.[]') `
        -Description "read commits for PR #$Number" `
        -AllowFailure
    if ($null -eq $commitLines) {
        $allSucceeded = $false
        $commits = @()
    }
    else {
        $commits = @(ConvertFrom-JsonLines -JsonLines $commitLines)
    }

    # Pagination-proof Track C dedup set. Every Track C response comment (APPLY and
    # PUSH-BACK alike) embeds `<!-- ci-fix-track-c-responded: <review-id> -->`.
    # Collecting the answered review-ids HERE — from the fully `--paginate`d
    # $issueComments above — gives the agent a durable "already answered" set so a
    # pure-decline (which advances no commit and bumps no marker) is never re-declined
    # every scheduled run. The agent previously re-fetched page 1 of comments itself,
    # which silently missed markers beyond the first 100 on a busy PR (issue comments
    # are returned oldest-first, so a recent marker lands on the LAST page).
    $respondedTrackCReviewIds = @(
        $issueComments |
            # Match the Track C response marker from ANY non-human commenter, not a single
            # hardcoded login. Today no safe-outputs.github-app is configured, so the Track C
            # add-comment posts as 'github-actions[bot]'; but if a PAT/App token is ever wired
            # up for safe-outputs (e.g. so pushes fire CI), the response comment would post
            # under a different [bot] login and an '-eq github-actions[bot]' filter would
            # silently go empty — resurrecting the pure-decline re-comment loop. Keying off
            # Test-IsHumanLogin keeps this idempotency guard robust to that identity change.
            # The marker is only ever written by this workflow's own Track C R4 emit, so
            # widening to non-human commenters admits no spoofed markers from human reviewers.
            Where-Object { $_.user -and $_.body -and -not (Test-IsHumanLogin -Login ([string]$_.user.login)) } |
            ForEach-Object {
                # [long]::TryParse, NOT a [long]/[int] cast: GitHub review ids already exceed
                # Int32.MaxValue (~2.1B; live dotnet/maui review ids are ~4.6B), and the regex
                # '(\d+)' is unbounded so a hand-edited marker can exceed Int64 too. A direct
                # cast throws a terminating OverflowException under $ErrorActionPreference='Stop'
                # ([int] above ~2.1B, [long] above ~9.2e18) which — with no try/catch on this
                # path — aborts the entire prefetch on the SECOND cycle after any Track C
                # response (the marker this loop itself writes), so candidates.json is never
                # emitted and the whole watch loop dies permanently. TryParse fails a malformed
                # id closed (emit nothing) so it is simply not counted as answered.
                $parsedReviewId = [long]0
                if (([string]$_.body -match 'ci-fix-track-c-responded:\s*(\d+)') -and
                    [long]::TryParse($Matches[1], [ref]$parsedReviewId)) { $parsedReviewId }
            } |
            Sort-Object -Unique
    )

    # Append-only floor for the attempt counter: every push the workflow makes is a
    # bot-authored commit on the PR branch. Counting them gives an authoritative lower
    # bound that CANNOT be rewound, so a stale/dropped body-marker bump can never let
    # the loop push past $AttemptMax. Author-based (not committer-based) to avoid
    # counting maintainer "Update branch" merge commits (committed by web-flow) as
    # attempts.
    $botCommitCount = @(
        $commits | Where-Object {
            $authorLogin = if ($_.author -and $_.author.login) { [string]$_.author.login } else { '' }
            ($authorLogin -ne '') -and -not (Test-IsHumanLogin -Login $authorLogin)
        }
    ).Count

    return [pscustomobject]@{
        Succeeded                = $allSucceeded
        botCommitCount           = [int]$botCommitCount
        respondedTrackCReviewIds = @($respondedTrackCReviewIds)
    }
}

$searchJson = Invoke-GhCommand `
    -Arguments @(
        'pr', 'list',
        '--repo', "$Owner/$Repo",
        '--state', 'open',
        '--search', "`"$TitlePrefix`" in:title",
        '--limit', "$MaxPRs",
        '--json', 'number,title,url,baseRefName,headRefName,headRefOid,isDraft,labels'
    ) `
    -Description 'list open ci-fix PRs'
$searchResult = if ([string]::IsNullOrWhiteSpace($searchJson)) { @() } else { @($searchJson | ConvertFrom-Json) }

$candidates = @()
foreach ($pr in @($searchResult)) {
    $number = [int]$pr.number
    $title = [string]$pr.title
    $baseRefName = [string]$pr.baseRefName
    $headRefName = [string]$pr.headRefName
    $labels = @($pr.labels | ForEach-Object { [string]$_.name })

    if ($title.IndexOf("${TitlePrefix}[needs-human]", [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        continue
    }

    if (-not $title.StartsWith("$TitlePrefix ", [StringComparison]::OrdinalIgnoreCase)) {
        continue
    }

    if ($headRefName -notlike 'ci-fix/*') {
        continue
    }

    # Base-branch partition (load-bearing): each ci-status-fix twin watches ONLY PRs that
    # target its own base branch. The main twin searches "[ci-fix]" and the net11 twin
    # searches "[ci-fix-net11]", but title-prefix alone is NOT a safe partition across the
    # SHARED `ci-fix/**` branch namespace: net11 historically opened its PRs under the plain
    # "[ci-fix]" prefix (renamed to "[ci-fix-net11]" in this change), so open legacy net11
    # PRs still carry "[ci-fix]" + base net11.0. Without this guard the main twin's
    # "[ci-fix]" search would ADOPT those net11.0-based PRs and push main-based fix commits
    # onto them. Scoping to $BaseBranch mirrors the create-PR `base-branch` /
    # `allowed-base-branches` pin onto the watch/advance path so the two twins never
    # cross-drive each other's PRs. An empty/unexpected base fails closed (skipped).
    if (-not $baseRefName.Equals($BaseBranch, [StringComparison]::OrdinalIgnoreCase)) {
        continue
    }

    if ($labels -notcontains 'agentic-workflows') {
        continue
    }

    $bodyResult = Get-PullRequestBody -Number $number
    $markers = Get-CiFixMarkers -Body $bodyResult.Body
    $checkState = Get-HeadCheckState -HeadSha ([string]$pr.headRefOid)
    $watchState = Get-PullRequestWatchState -Number $number

    $dataComplete = $bodyResult.Succeeded -and $checkState.Succeeded -and $watchState.Succeeded
    # Authoritative attempt counter: the higher of the (possibly stale) body-marker
    # numerator and the append-only bot-commit floor, gated by the fixed $AttemptMax
    # constant. A null marker contributes 0, so the bot-commit floor governs on its own.
    # Clamp the marker to $attemptMax with LONG math BEFORE narrowing to [int]:
    # Get-CiFixMarkers returns $markers.attempt as a [long] because the PR body is
    # attacker-editable, so a crafted `ci-fix-attempts: 99999999999/10` survives parsing.
    # A direct [int] cast here would throw a terminating OverflowException under
    # $ErrorActionPreference='Stop' and — with no try/catch on this path — abort the whole
    # prefetch, stalling EVERY watched PR (candidates.json never written). Min() caps it at
    # $attemptMax (10) first, which is also the correct semantic: a marker at/over the cap
    # simply means "no attempts remain", so the actionable gate below treats the PR as done.
    $markerAttempt = if ($null -eq $markers.attempt) { 0 } else {
        [int][Math]::Min([long]$markers.attempt, [long]$markers.attemptMax)
    }
    $effectiveAttempt = [Math]::Max($markerAttempt, [int]$watchState.botCommitCount)
    $actionable = $dataComplete -and
        $checkState.checksSettled -and
        ($checkState.overallConclusion -eq 'failure') -and
        ($effectiveAttempt -lt [int]$markers.attemptMax)

    $candidates += [pscustomobject]@{
        prNumber          = $number
        title             = $title
        url               = [string]$pr.url
        headRefName       = $headRefName
        headSha           = [string]$pr.headRefOid
        isDraft           = [bool]$pr.isDraft
        refsIssue         = $markers.refsIssue
        attempt           = $markers.attempt
        attemptMax        = [int]$markers.attemptMax
        botCommitCount    = [int]$watchState.botCommitCount
        effectiveAttempt  = [int]$effectiveAttempt
        respondedTrackCReviewIds = @($watchState.respondedTrackCReviewIds)
        checksSettled     = [bool]$checkState.checksSettled
        overallConclusion = [string]$checkState.overallConclusion
        failedLegs        = @($checkState.failedLegs)
        # dataComplete is false when ANY prefetch source (PR body, head check-state,
        # or watch state) hit an API error. The agent MUST treat an incomplete candidate
        # as "wait" because its attempt count or Track C response dedup may be incomplete.
        dataComplete      = [bool]$dataComplete
        actionable        = [bool]$actionable
    }
}

$anyActionable = @($candidates | Where-Object { $_.actionable }).Count -gt 0

$outputDir = Split-Path -Parent $OutputPath
if ($outputDir) {
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
}

$json = [ordered]@{
    generatedAt   = (Get-Date).ToUniversalTime().ToString('o')
    anyActionable = [bool]$anyActionable
    candidates    = @($candidates)
} | ConvertTo-Json -Depth 20
$json | Set-Content -LiteralPath $OutputPath -Encoding UTF8

Write-Host "Wrote $($candidates.Count) ci-fix candidate(s) (anyActionable=$anyActionable) to $OutputPath"
Write-Output $json
