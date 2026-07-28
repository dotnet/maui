#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds bounded exact-label issue evidence and open-PR watch context for the CI-fixer.
#>

param(
    [int]$MaxPRs = 20,
    [ValidateRange(1, 50)]
    [int]$MaxIssues = 20,
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [string]$OutputPath = "CustomAgentLogsTmp/CiFixScanner/candidates.json",
    [string]$TitlePrefix = '[ci-fix]',
    [string]$IssueLabel = 'ci-scan',
    # The twin workflow's label. Issues carrying BOTH labels are owned by the twin and are
    # excluded here deterministically, because the agent has no live issue tools and so
    # cannot enforce that ownership rule from the prompt.
    [AllowEmptyString()]
    [string]$ExcludeIssueLabel = '',
    [AllowEmptyString()]
    [string]$IssueNumber = '',
    [ValidateRange(64, 1024)]
    [int]$MaxIssueTitleChars = 256,
    [ValidateRange(256, 16384)]
    [int]$MaxIssueBodyChars = 12000,
    # The base branch this workflow instance owns. Each ci-status-fix twin watches ONLY
    # PRs targeting its own base (main -> 'main', net11 -> 'net11.0'). See the baseRefName
    # guard in the candidate loop for why this is load-bearing, not cosmetic.
    [string]$BaseBranch = 'main'
)

$ErrorActionPreference = 'Stop'
# Pin native-command error handling OFF so `& gh ... 2>&1` in Invoke-GhCommand never
# throws at invocation on a non-zero exit (404 on an orphaned SHA, transient rate-limit).
# The whole prefetch's graceful degradation depends on the -AllowFailure/-AllowNotFound
# paths returning $null on expected failures rather than terminating; a future runner
# image or profile that flips this preference to $true would bypass them and crash the
# loop.
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

# The prefetch runs before the agent, so a transient GitHub API outage would otherwise
# suppress the entire scheduled sweep. Keep retries bounded and preserve the existing
# fail-closed result after the final attempt.
$TransientGhHttpStatusCodes = @(429, 500, 502, 503, 504)
$MaxTransientGhAttempts = 4
$TransientGhRetryBaseDelaySeconds = 2

function Test-IsTransientGhFailure {
    param([AllowEmptyString()][string]$Detail)

    foreach ($statusCode in $TransientGhHttpStatusCodes) {
        if ($Detail -match "(?i)\bHTTP $statusCode\b") {
            return $true
        }
    }

    return $false
}

function Test-IsGhNotFoundFailure {
    param([AllowEmptyString()][string]$Detail)

    # Only a CONFIRMED 404 means "this issue is genuinely not in scope". Auth
    # (401/403), rate-limit (429), server (5xx) and network failures must NOT be
    # collapsed into "not found" — on the discovery path that would silently look
    # like "no ci-fix work" and skip the whole sweep. Match the HTTP code, never
    # gh's prose, so a body containing the words "Not Found" cannot fake it.
    return [bool]($Detail -match '(?i)\bHTTP 404\b')
}

function Invoke-GhCommand {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description,
        # Degrade to $null on ANY non-transient failure. Reserved for callers that
        # track their own "known" flag and fail closed from it (Get-HeadCheckState,
        # Get-PullRequestBody).
        [switch]$AllowFailure,
        # Degrade to $null ONLY on a confirmed HTTP 404, and propagate everything
        # else. Use this on discovery reads, where an empty result is indistinguishable
        # from "nothing to do" and therefore must never be produced by an auth,
        # rate-limit, server or network failure.
        [switch]$AllowNotFound
    )

    for ($attempt = 1; $attempt -le $MaxTransientGhAttempts; $attempt++) {
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

        if ($exitCode -eq 0) {
            return $stdoutText
        }

        $message = "gh $Description failed with exit code $exitCode."
        $detail = (@($stderrText, $stdoutText) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join ' '
        if (-not [string]::IsNullOrWhiteSpace($detail)) {
            $message = "$message Output: $detail"
        }

        if ((Test-IsTransientGhFailure -Detail $detail) -and ($attempt -lt $MaxTransientGhAttempts)) {
            $delaySeconds = $TransientGhRetryBaseDelaySeconds * [Math]::Pow(2, $attempt - 1)
            Write-Warning "$message Retrying in $delaySeconds second(s) ($attempt/$MaxTransientGhAttempts)."
            Start-Sleep -Seconds $delaySeconds
            continue
        }

        if ($AllowNotFound -and (Test-IsGhNotFoundFailure -Detail $detail)) {
            Write-Warning $message
            return $null
        }

        if ($AllowFailure) {
            Write-Warning $message
            return $null
        }

        throw $message
    }

    throw "gh $Description exhausted its retry budget unexpectedly."
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

function Resolve-IssueScopeNumber {
    param([AllowEmptyString()][string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    $parsed = [int]0
    if (-not [int]::TryParse($Value, [ref]$parsed) -or $parsed -le 0) {
        throw "IssueNumber must be empty or a positive Int32 issue number."
    }

    return $parsed
}

function ConvertTo-BoundedUntrustedText {
    param(
        [AllowNull()][string]$Value,
        [Parameter(Mandatory = $true)][int]$MaxChars
    )

    $original = if ($null -eq $Value) { '' } else { [string]$Value }
    $sanitized = $original.Replace("`r`n", "`n").Replace("`r", "`n")
    $sanitized = [regex]::Replace(
        $sanitized,
        "[\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]",
        ' '
    )

    $truncated = $sanitized.Length -gt $MaxChars
    if ($truncated) {
        $length = $MaxChars
        if ($length -gt 0 -and [char]::IsHighSurrogate($sanitized[$length - 1])) {
            $length--
        }
        $sanitized = $sanitized.Substring(0, $length)
    }

    return [pscustomobject]@{
        text           = $sanitized
        truncated      = [bool]$truncated
        originalLength = [int]$original.Length
    }
}

function Test-IssueHasExactLabel {
    param(
        [AllowNull()][object[]]$Labels,
        [Parameter(Mandatory = $true)][string]$ExactLabel
    )

    foreach ($label in @($Labels)) {
        $name = if ($label -is [string]) { [string]$label } elseif ($label.name) { [string]$label.name } else { '' }
        if ($name.Equals($ExactLabel, [StringComparison]::OrdinalIgnoreCase)) {
            return $true
        }
    }

    return $false
}

function ConvertTo-CiFixIssueEvidence {
    param(
        [AllowNull()][object[]]$Issues,
        [Parameter(Mandatory = $true)][string]$ExactLabel,
        [AllowNull()][string]$ExcludeLabel,
        [Parameter(Mandatory = $true)][int]$Limit,
        [Parameter(Mandatory = $true)][int]$TitleMaxChars,
        [Parameter(Mandatory = $true)][int]$BodyMaxChars
    )

    $evidence = @()
    $excludedIssueNumbers = @()
    $eligibleCount = 0
    $seenIssueNumbers = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($issue in @($Issues)) {
        if ($null -eq $issue -or
            ([string]$issue.state).ToLowerInvariant() -ne 'open' -or
            $issue.pull_request -or
            -not (Test-IssueHasExactLabel -Labels @($issue.labels) -ExactLabel $ExactLabel)) {
            continue
        }

        $number = [int]0
        if (-not [int]::TryParse([string]$issue.number, [ref]$number) -or $number -le 0) {
            continue
        }
        if (-not $seenIssueNumbers.Add($number)) {
            continue
        }

        # The twin workflow owns dual-labelled issues. Decide ownership deterministically
        # here: the agent cannot see raw labels (the `issues` toolset is deliberately
        # removed), so a prompt-level "skip if it also carries <twin label>" rule would be
        # unenforceable and both twins could open a PR for the same issue.
        if (-not [string]::IsNullOrWhiteSpace($ExcludeLabel) -and
            (Test-IssueHasExactLabel -Labels @($issue.labels) -ExactLabel $ExcludeLabel)) {
            $excludedIssueNumbers += $number
            continue
        }

        # Count every eligible issue, but only materialize up to $Limit, so the caller can
        # report a truthful backlog total instead of implying the batch is the whole queue.
        $eligibleCount++
        if ($evidence.Count -ge $Limit) {
            continue
        }

        $title = ConvertTo-BoundedUntrustedText -Value ([string]$issue.title) -MaxChars $TitleMaxChars
        $body = ConvertTo-BoundedUntrustedText -Value ([string]$issue.body -as [string]) -MaxChars $BodyMaxChars
        $evidence += [pscustomobject]@{
            issueNumber       = $number
            url               = [string]$issue.html_url
            state             = 'open'
            exactLabel        = $ExactLabel
            title             = $title.text
            body              = $body.text
            titleTruncated    = [bool]$title.truncated
            bodyTruncated     = [bool]$body.truncated
            titleOriginalChars = [int]$title.originalLength
            bodyOriginalChars = [int]$body.originalLength
            createdAt         = [string]$issue.created_at
            updatedAt         = [string]$issue.updated_at
            untrusted         = $true
        }
    }

    return [pscustomobject]@{
        items = @($evidence)
        totalMatched = $eligibleCount
        truncated = ($eligibleCount -gt @($evidence).Count)
        excludedDualLabelled = @($excludedIssueNumbers)
    }
}

function Get-CiFixIssueEvidence {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryOwner,
        [Parameter(Mandatory = $true)][string]$RepositoryName,
        [Parameter(Mandatory = $true)][string]$ExactLabel,
        [AllowNull()][string]$ExcludeLabel,
        [AllowNull()][Nullable[int]]$ScopedIssueNumber,
        [AllowNull()][object[]]$PriorityIssueNumbers,
        [Parameter(Mandatory = $true)][int]$Limit,
        [Parameter(Mandatory = $true)][int]$TitleMaxChars,
        [Parameter(Mandatory = $true)][int]$BodyMaxChars
    )

    if ([string]::IsNullOrWhiteSpace($ExactLabel) -or
        $ExactLabel.Length -gt 100 -or
        $ExactLabel -notmatch '^[A-Za-z0-9][A-Za-z0-9 ._:/+()-]*$') {
        throw "IssueLabel must be a non-empty GitHub label name of at most 100 safe characters."
    }

    if (-not [string]::IsNullOrWhiteSpace($ExcludeLabel) -and
        ($ExcludeLabel.Length -gt 100 -or
        $ExcludeLabel -notmatch '^[A-Za-z0-9][A-Za-z0-9 ._:/+()-]*$')) {
        throw "ExcludeLabel must be a GitHub label name of at most 100 safe characters."
    }

    if ($null -ne $ScopedIssueNumber) {
        # A stale or mistyped dispatch number must produce the documented
        # "skipped: dispatch issue_number not an in-scope ci-scan issue" record, not a hard
        # failure of the whole pre-activation job before the agent ever runs. Only a
        # confirmed 404 qualifies: -AllowNotFound propagates auth/rate-limit/5xx/network
        # failures so a transient blip can never masquerade as "not in scope".
        $issueJson = Invoke-GhCommand `
            -Arguments @('api', "repos/$RepositoryOwner/$RepositoryName/issues/$ScopedIssueNumber") `
            -Description "read scoped issue #$ScopedIssueNumber" `
            -AllowNotFound
        $issues = if ([string]::IsNullOrWhiteSpace($issueJson)) { @() } else { @(ConvertFrom-Json $issueJson) }
    }
    else {
        $issues = @()
        $seenPriorityNumbers = [System.Collections.Generic.HashSet[int]]::new()
        foreach ($candidateNumber in @($PriorityIssueNumbers)) {
            $number = [int]0
            if (-not [int]::TryParse([string]$candidateNumber, [ref]$number) -or
                $number -le 0 -or
                -not $seenPriorityNumbers.Add($number) -or
                $seenPriorityNumbers.Count -gt $Limit) {
                continue
            }

            # A watch issue that was deleted/transferred (404) is legitimately gone, so
            # skip it. Anything else (auth, rate-limit, 5xx, network) must fail the
            # prefetch instead of silently dropping a watched issue from the snapshot.
            $priorityJson = Invoke-GhCommand `
                -Arguments @('api', "repos/$RepositoryOwner/$RepositoryName/issues/$number") `
                -Description "read priority watch issue #$number" `
                -AllowNotFound
            if (-not [string]::IsNullOrWhiteSpace($priorityJson)) {
                $issues += ConvertFrom-Json $priorityJson
            }
        }

        # Page to completion, oldest-first. `sort=updated&direction=desc` with a single
        # `per_page=$Limit` page silently stranded the oldest still-open issues forever:
        # the batch is capped, the agent has no live issue tools to compensate, and every
        # issue the agent touches bubbles back to the top of the window. Ascending
        # creation order makes the cap drain the backlog FIFO instead.
        $issueLines = Invoke-GhCommand `
            -Arguments @(
                'api', '--method', 'GET', "repos/$RepositoryOwner/$RepositoryName/issues",
                '-f', 'state=open',
                '-f', "labels=$ExactLabel",
                '-f', 'sort=created',
                '-f', 'direction=asc',
                '-f', 'per_page=100',
                '--paginate',
                '--jq', '.[]'
            ) `
            -Description "list open issues with exact label '$ExactLabel'"
        $issues += @(ConvertFrom-JsonLines -JsonLines $issueLines)
    }

    return ConvertTo-CiFixIssueEvidence `
        -Issues $issues `
        -ExactLabel $ExactLabel `
        -ExcludeLabel $ExcludeLabel `
        -Limit $Limit `
        -TitleMaxChars $TitleMaxChars `
        -BodyMaxChars $BodyMaxChars
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
    # onto them. Scoping to $BaseBranch mirrors the workflow handler-base contract
    # and create-PR `allowed-base-branches` pin onto the watch/advance path so the
    # two twins never cross-drive each other's PRs. An empty/unexpected base fails
    # closed (skipped).
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
$scopedIssueNumber = Resolve-IssueScopeNumber -Value $IssueNumber
$issueEvidence = Get-CiFixIssueEvidence `
    -RepositoryOwner $Owner `
    -RepositoryName $Repo `
    -ExactLabel $IssueLabel `
    -ExcludeLabel $ExcludeIssueLabel `
    -ScopedIssueNumber $scopedIssueNumber `
    -PriorityIssueNumbers @($candidates | ForEach-Object { $_.refsIssue }) `
    -Limit $MaxIssues `
    -TitleMaxChars $MaxIssueTitleChars `
    -BodyMaxChars $MaxIssueBodyChars
$issueEvidenceItems = @($issueEvidence.items)

$outputDir = Split-Path -Parent $OutputPath
if ($outputDir) {
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
}

$json = [ordered]@{
    schemaVersion = 2
    generatedAt = (Get-Date).ToUniversalTime().ToString('o')
    repository = "$Owner/$Repo"
    issueEvidence = [ordered]@{
        authoritative = $true
        exactLabel = $IssueLabel
        excludeLabel = $ExcludeIssueLabel
        scopedIssueNumber = $scopedIssueNumber
        maxIssues = $MaxIssues
        titleMaxChars = $MaxIssueTitleChars
        bodyMaxChars = $MaxIssueBodyChars
        count = $issueEvidenceItems.Count
        # totalMatched counts every eligible open exact-label issue, not just the bounded
        # batch. truncated=true means work remains beyond this run's window; it must never
        # be read as "no other candidates exist".
        totalMatched = [int]$issueEvidence.totalMatched
        truncated = [bool]$issueEvidence.truncated
        excludedDualLabelled = @($issueEvidence.excludedDualLabelled)
        issues = @($issueEvidenceItems)
    }
    anyActionable = [bool]$anyActionable
    candidates = @($candidates)
} | ConvertTo-Json -Depth 20
$json | Set-Content -LiteralPath $OutputPath -Encoding UTF8

Write-Host "Wrote $($candidates.Count) ci-fix candidate(s) and $($issueEvidenceItems.Count) exact-label issue(s) (anyActionable=$anyActionable) to $OutputPath"
Write-Output $json
