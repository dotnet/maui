#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts the AI review summary as a GitHub Pull Request review.

.DESCRIPTION
    Creates a new PR review per run, identified by <!-- AI Summary --> marker.
    Before posting a fresh review, older generated AI Summary artifacts are
    hidden as outdated. The replacement review contains only the latest review
    session, keyed by the current HEAD commit SHA.

    After posting, the PR author is @-mentioned so they know to review.

    Content is auto-loaded from PRAgent phase files:
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/gate/content.md          (always shown first, open)
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/{pre-flight,try-fix,report}/content.md
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/expert-pr-eval/content.md
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/pre-flight/code-review.md (legacy fallback)

    Gate is included as a section inside this unified review body — the script may
    be called by Review-PR.ps1 twice per run: once after the gate completes
    (gate-only update) and once after the review phases finish (full update).

    Any standalone legacy "<!-- AI Gate -->" comment from older versions of
    the script is hidden before the fresh review is posted to avoid duplicates.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER DryRun
    Print review body instead of posting

.EXAMPLE
    ./post-ai-summary-comment.ps1 -PRNumber 12345

.EXAMPLE
    ./post-ai-summary-comment.ps1 -PRNumber 12345 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    # Repository holding the pull request, in `owner/name` form. Defaults to the
    # upstream project so every existing caller keeps its behaviour. The bot's own
    # fix pull requests live on the testing fork, and a summary that always posted
    # to dotnet/maui would either 404 or, far worse, attach a review to whichever
    # unrelated upstream pull request happens to carry the same number.
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})/[A-Za-z0-9._-]+$')]
    [string]$Repository = 'dotnet/maui',

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    # Trusted gate verdict supplied by the pipeline (Gate task output variable),
    # NOT read from the agent-writable PRAgent worktree. Used to veto an APPROVE
    # review over a FAILED gate. Empty/omitted (local/manual runs that never post
    # APPROVE) is treated as the non-blocking 'SKIPPED' sentinel.
    [Parameter(Mandatory = $false)]
    # TIMEDOUT is a pipeline-supplied sentinel meaning the Gate task itself did not finish
    # (stopped by its 150-min hang-safety timeout, or it produced no verdict). It renders an
    # honest "gate did not complete" section and vetoes APPROVE (the fix was not verified).
    [ValidateSet('PASSED', 'SKIPPED', 'INCONCLUSIVE', 'FAILED', 'TIMEDOUT', '')]
    [string]$TrustedGateResult = '',

    # Optional review/deep-run platform supplied by the pipeline (${{ parameters.Platform }}).
    # Used ONLY as a fallback for the Platform status chip when the summary content carries no
    # "**Platform:**" line — e.g. a deep-only re-run with no code-review phase, where the
    # deep clearly ran on a platform but nothing in the text names it (dotnet/maui#35606 rendered
    # "Platform Unknown"). A full review still prefers the code-review-derived platform. Empty for
    # local/manual runs → behaves exactly as before.
    [Parameter(Mandatory = $false)]
    [string]$Platform = '',

    # Immutable PR head captured by the trusted Setup task. When the live PR
    # advances during a run, the summary remains bound to this reviewed commit
    # and is downgraded to an informational COMMENT.
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^$|^[0-9a-fA-F]{40}$')]
    [string]$ReviewedCommit = ''
)

$ErrorActionPreference = "Stop"
$MARKER = "<!-- AI Summary -->"

. (Join-Path $PSScriptRoot 'shared/Escape-Html.ps1')

$commentCleanupScript = Join-Path $PSScriptRoot "shared/Remove-StaleMauiBotComments.ps1"
if (Test-Path $commentCleanupScript) {
    . $commentCleanupScript
}

# ============================================================================
# LOAD PHASE CONTENT
# ============================================================================

Write-Host "ℹ️  Loading phase content for PR #$PRNumber..." -ForegroundColor Cyan

$RepoRoot = git rev-parse --show-toplevel 2>$null
$PRAgentDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
if (-not (Test-Path $PRAgentDir)) {
    if ($RepoRoot) {
        $PRAgentDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    }
}

if (-not (Test-Path $PRAgentDir)) {
    throw "No PRAgent directory found at: $PRAgentDir"
}

$phases = [ordered]@{
    "pre-flight"       = @{ Files = @("pre-flight/content.md");                                  Title = "📋 Pre-Flight — Context & Validation" }
    "code-review"      = @{ Files = @("expert-pr-eval/content.md", "pre-flight/code-review.md"); Title = "🔬 Code Review — Deep Analysis" }
    "try-fix"          = @{ Files = @("try-fix/content.md");                                     Title = "🛠️ Try-Fix — Analysis & Comparison" }
    "pr-finalize"      = @{ Files = @("pr-finalize/content.md");                                 Title = "📝 PR Finalize — Recommended Title & Description" }
    "report"           = @{ Files = @("report/content.md");                                      Title = "🏁 Report — Final Recommendation" }
    "regression-check" = @{ Files = @("regression-check/content.md");                            Title = "🔗 Regression Cross-Reference" }
    # Keep the potentially very large UI-test details last so they cannot hide the
    # expert-review sections if a final defensive truncation is ever needed.
    "uitests"          = @{ Files = @("uitests/content.md");                                     Title = "📱 UI Tests" }
}

function Get-FirstPhaseContent {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Root,

        [Parameter(Mandatory = $true)]
        [string[]]$RelativePaths
    )

    foreach ($relativePath in $RelativePaths) {
        $filePath = Join-Path $Root $relativePath
        if (-not (Test-Path -LiteralPath $filePath)) {
            continue
        }

        $content = Get-Content -LiteralPath $filePath -Raw -Encoding UTF8
        if (-not [string]::IsNullOrWhiteSpace($content)) {
            return [pscustomobject]@{
                Path    = $filePath
                Content = $content
            }
        }
    }

    return $null
}

function Test-PhaseContentIsNoOp {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PhaseKey,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $normalized = ($Content -replace "`r`n", "`n").Trim()

    switch ($PhaseKey) {
        "uitests" {
            return (
                $normalized -match '^No UI test categories needed for this PR \(no UI-relevant changes\)\.?$' -or
                $normalized -match '^Full UI test matrix will run \(no specific categories detected from PR changes\)\.?$'
            )
        }

        "regression-check" {
            $withoutHeading = ($normalized -replace '(?m)^##\s+.*Regression Cross-Reference\s*\n+', '').Trim()
            return (
                $withoutHeading -match '^(?:●|🟢)\s+No implementation files modified\s+[—-]\s+skipping regression cross-reference\.\s*$' -or
                $withoutHeading -match '^(?:●|🟢)\s+No regression risks detected\.\s+No labeled bug-fix PRs in the last \d+ months touched the modified files\.\s*$'
            )
        }
        "pr-finalize" {
            # Keep-as-is verdict: the PR's existing title/description are already good, so
            # omit the "PR Finalize — Recommended Title & Description" section entirely (no copy-paste
            # artifact is needed). Tolerant of an optional "**Assessment:**" prefix and any
            # trailing optional notes the agent may add.
            return (
                $normalized -match '✅\s*Current title and description accurately reflect the change\s*[—-]\s*recommend keeping as-is'
            )
        }
        default {
            return $false
        }
    }
}

function New-MissingAgentPhaseContent {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('pre-flight', 'code-review', 'try-fix', 'report')]
        [string]$PhaseKey
    )

    $phaseName = switch ($PhaseKey) {
        'pre-flight' { 'Pre-Flight' }
        'code-review' { 'Code Review' }
        'try-fix' { 'Try-Fix' }
        'report' { 'Report / Final Recommendation' }
    }

    return @"
⚠️ **$phaseName did not produce output on this run.**

The Copilot expert-review task ended before this phase was persisted, usually because the review-stage time budget expired or the CI agent encountered a transient authentication/runtime problem. Earlier completed sections remain valid, but this review is **incomplete** without this phase.

**Next step:** re-comment ``/review`` to retry on a fresh agent. If this repeats across runs, a maintainer should inspect the reviewer token and Task 3 logs.
"@
}

function Get-AuthoritativeGateContent {
    param(
        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [string]$GateContent = '',

        [Parameter(Mandatory = $false)]
        [AllowEmptyString()]
        [string]$TrustedGateResult = ''
    )

    $trustedVerdict = $TrustedGateResult.Trim().ToUpperInvariant()

    # PASSED content can retain its detailed report. Every non-pass verdict comes from the
    # trusted Gate task output and must agree with the agent-writable display artifact.
    if ($trustedVerdict -notin @('FAILED', 'SKIPPED', 'INCONCLUSIVE', 'TIMEDOUT')) {
        return $GateContent
    }

    # A TIMEDOUT verdict means the Gate task was killed before its trusted wrapper could
    # publish a result. Any gate/content.md left behind is necessarily partial or stale.
    if ($trustedVerdict -eq 'TIMEDOUT') {
        return @'
### Gate Result: TIMEDOUT — test verification did not finish

The automated **test-verification gate** did not complete on this run. It was stopped by the pipeline's **hang-safety timeout** (the gate is capped at 150 min to catch an emulator/simulator boot or an Appium hang that would otherwise run to the job limit), or it could not produce a verdict.

- This is almost always a transient **infrastructure** issue on the CI agent — **not** a problem with your PR.
- Because the gate could not finish, **the fix was not verified by tests** on this run, so this review is **not eligible for APPROVE**.
- The rest of the review below (expert analysis and findings) ran as usual.

**Next step:** re-comment `/review` to retry the gate on a fresh agent.
'@
    }

    $displayedVerdicts = @(
        [regex]::Matches(
            $GateContent,
            '(?im)Gate Result:\s*(?:\S+\s*)?(FAILED|PASSED|SKIPPED|INCONCLUSIVE|TIMEDOUT)') |
            ForEach-Object { $_.Groups[1].Value.ToUpperInvariant() } |
            Select-Object -Unique
    )
    if ($displayedVerdicts.Count -eq 1 -and $displayedVerdicts[0] -eq $trustedVerdict) {
        return $GateContent
    }

    switch ($trustedVerdict) {
        'FAILED' {
            return @'
### Gate Result: ❌ FAILED

The trusted **test-verification gate** reported a failure. Its detailed display artifact was
missing or contradicted this trusted pipeline verdict, so that agent-writable content was not
used in the review summary.

Inspect the trusted **Gate** task logs for the failing build or test evidence. This run is not
eligible for approval.
'@
        }
        'SKIPPED' {
            return @'
### Gate Result: ⚠️ SKIPPED

The trusted **test-verification gate** skipped test execution for this change. It did not build
or run the selected tests, so this verdict does not establish that the PR build is healthy.

Review any independent CI or Deep UI results shown below before merging.
'@
        }
        'INCONCLUSIVE' {
            return @'
### Gate Result: ⚠️ INCONCLUSIVE

The trusted **test-verification gate** could not produce a definitive pass/fail result. Its
detailed display artifact was missing or contradicted this trusted pipeline verdict, so that
agent-writable content was not used in the review summary.

Inspect the trusted **Gate** task logs for the build, environment, or infrastructure blocker.
'@
        }
    }
}

function Limit-MarkdownContent {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [ValidateRange(512, 65500)]
        [int]$MaxChars,

        [Parameter(Mandatory = $true)]
        [string]$SectionName
    )

    if ($Content.Length -le $MaxChars) {
        return $Content
    }

    $notice = "`n`n_This $SectionName section was shortened to keep every required review section visible. Full details remain available in the pipeline build artifacts._"
    $keep = [Math]::Max(0, $MaxChars - $notice.Length - 256)

    while ($true) {
        $candidate = $Content.Substring(0, [Math]::Min($keep, $Content.Length)).TrimEnd()
        $lastNewline = $candidate.LastIndexOf("`n")
        if ($lastNewline -gt [Math]::Floor($candidate.Length * 0.8)) {
            $candidate = $candidate.Substring(0, $lastNewline).TrimEnd()
        }

        $suffix = ""
        if ((([regex]::Matches($candidate, '(?m)^```')).Count % 2) -ne 0) {
            $codeFence = [string][char]96 * 3
            $suffix += "`n$codeFence"
        }

        $openDetails = ([regex]::Matches($candidate, '(?i)<details(?:\s|>)')).Count
        $closedDetails = ([regex]::Matches($candidate, '(?i)</details>')).Count
        $unclosedDetails = [Math]::Max(0, $openDetails - $closedDetails)

        $result = $candidate + $suffix + $notice
        if ($unclosedDetails -gt 0) {
            $result += "`n" + (("</details>`n" * $unclosedDetails).TrimEnd())
        }

        if ($result.Length -le $MaxChars -or $keep -eq 0) {
            return $result
        }

        $keep = [Math]::Max(0, $keep - ($result.Length - $MaxChars) - 64)
    }
}

function Get-AIReviewEvent {
    param([string]$ReportContent)

    if ([string]::IsNullOrWhiteSpace($ReportContent)) {
        return 'COMMENT'
    }

    $normalized = $ReportContent -replace "`r`n", "`n"
    if ($normalized -match '(?im)^\s*(?:##\s*)?(?:✅\s*)?Final\s+Recommendation:\s*APPROVE\s*$') {
        return 'APPROVE'
    }

    if ($normalized -match '(?im)^\s*(?:##\s*)?(?:⚠️\s*)?Final\s+Recommendation:\s*REQUEST\s+CHANGES\s*$') {
        return 'REQUEST_CHANGES'
    }

    return 'COMMENT'
}

function Add-MissingUITestResultsNote {
    # The UI Tests section starts as a bare "Detected UI test categories: X" placeholder written
    # during pre-flight; the RunDeepUITests stage is supposed to append real results. When the
    # platform-pool run produces nothing (most often because the PR build failed or the deep
    # stage was skipped), that placeholder is posted as-is — an empty, confusing section. Append
    # a short explanation so the empty section explains itself. No-op for content that already
    # has results, or for the "no categories"/"full matrix" placeholders.
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Content,

        [Parameter(Mandatory = $false)]
        [ValidateSet('PASSED', 'SKIPPED', 'INCONCLUSIVE', 'FAILED', 'TIMEDOUT', '')]
        [string]$TrustedGateResult = ''
    )

    if ([string]::IsNullOrWhiteSpace($Content)) { return $Content }
    if ($Content -notmatch '(?im)Detected UI test categories') { return $Content }
    # Already has deep/in-process results — leave it alone.
    if ($Content -match '(?im)DEEP_UITESTS_BEGIN' -or
        $Content -match '(?im)Deep UI tests' -or
        $Content -match '(?im)UI Test Execution Results' -or
        $Content -match '(?im)\b\d+\s+passed\b') {
        return $Content
    }

    # Tailor the guidance to the trusted pipeline gate outcome instead of trying to
    # discover it in UI-phase content. PASSED is the only verdict that proves the PR
    # build completed successfully. SKIPPED runs no build/tests, while INCONCLUSIVE
    # and TIMEDOUT do not establish build health. FAILED (or an unknown/absent verdict)
    # keeps the build/gate guidance.
    $gateState = $TrustedGateResult.Trim().ToUpperInvariant()

    if ($gateState -eq 'TIMEDOUT') {
        $note = @'

> [!WARNING]
> **No UI test results were produced for the detected categories.** The trusted gate timed
> out before producing a definitive result, and the deep UI stage also returned no results.
> This is usually transient **infrastructure**, but the PR build was not proven either way;
> inspect the **Gate** section and push again after any confirmed build issue is addressed.
'@
    } elseif ($gateState -eq 'SKIPPED') {
        $note = @'

> [!WARNING]
> **No UI test results were produced for the detected categories.** The trusted gate was
> skipped before build/test verification, so it does not prove that the PR build is healthy.
> The deep UI stage also returned no results; inspect the independent CI checks and retry the
> review if those categories still need coverage.
'@
    } elseif ($gateState -eq 'INCONCLUSIVE') {
        $note = @'

> [!WARNING]
> **No UI test results were produced for the detected categories.** The trusted gate was
> inconclusive and did not establish whether the PR build was healthy. The deep UI stage also
> returned no results, usually because of a build, environment, or infrastructure interruption;
> inspect the **Gate** section and independent CI checks before retrying.
'@
    } elseif ($gateState -notin @('PASSED', 'SKIPPED', 'INCONCLUSIVE')) {
        $note = @'

> [!WARNING]
> **No UI test results were produced for the detected categories.** The platform-pool run
> returned no results — most often because the PR build failed (see the **Gate** section) or
> the deep UI test stage was skipped. Fix the build/gate issues and push again; the review
> re-runs on new commits (a maintainer can also re-run it).
'@
    } else {
        # PASSED is the only trusted verdict that proves the PR build was not the blocker.
        $note = @'

> [!WARNING]
> **No UI test results were produced for the detected categories.** The PR build itself was
> fine — the deep UI stage was skipped or interrupted on **infrastructure** (the
> merge-for-testing step, emulator/simulator boot, or an Appium hang), not by this PR's code.
> This is usually transient; the review re-runs on new commits (a maintainer can also re-run it).
'@
    }
    return ($Content.TrimEnd() + [Environment]::NewLine + $note)
}

function ConvertTo-TitleCase {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $Value
    }

    $trimmed = $Value.Trim()
    switch -Regex ($trimmed) {
        '(?i)^android$' { return 'Android' }
        '(?i)^ios$' { return 'iOS' }
        '(?i)^(mac)?catalyst$' { return 'MacCatalyst' }
        '(?i)^windows$' { return 'Windows' }
        '(?i)^all$' { return 'All' }
    }

    return (Get-Culture).TextInfo.ToTitleCase($trimmed.ToLowerInvariant())
}

function ConvertTo-ShieldsSegment {
    param([string]$Value)

    $encoded = [uri]::EscapeDataString($Value)
    return ($encoded -replace '-', '--' -replace '_', '__')
}

function New-StatusChip {
    param(
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$Color
    )

    $labelSegment = ConvertTo-ShieldsSegment $Label
    $valueSegment = ConvertTo-ShieldsSegment $Value
    $alt = "$Label $Value" -replace '"', '&quot;'
    return "  <img alt=`"$alt`" src=`"https://img.shields.io/badge/$labelSegment-$valueSegment-$Color`?labelColor=30363d&style=flat-square`">"
}

function Get-GateStatus {
    param([string]$GateContent)

    if ([string]::IsNullOrWhiteSpace($GateContent)) {
        return 'Unknown'
    }

    # A FAILED gate may actually be a mixed / inconclusive (partial) outcome: the gate's
    # verification report flags these via its failure classifications ("regression in
    # another test" = one test FAIL→PASS but another fails both; "test does not reproduce
    # the bug" = passes with and without the fix). Surface those as 'Partial' rather than a
    # flat 'Failed'. A SKIPPED gate means no runnable tests were detected → 'No Tests'.
    # INCONCLUSIVE means the tests could not be built/run (build or env error) → 'Inconclusive'.
    # TIMEDOUT means the gate task itself was stopped by its hang-safety timeout (or produced no
    # verdict at all) → 'Timed Out': the fix was NOT verified, but this is an infra outcome, not a
    # real test failure, so it renders teal like Inconclusive rather than red.
    $isPartial = ($GateContent -match '(?i)Regression in another test' -or
                  $GateContent -match '(?i)Test does not reproduce the bug')

    if ($GateContent -match '(?im)Gate Result:\s*(?:\S+\s*)?(FAILED|PASSED|SKIPPED|INCONCLUSIVE|TIMEDOUT)') {
        switch ($Matches[1].ToUpperInvariant()) {
            'PASSED'       { return 'Passed' }
            'SKIPPED'      { return 'No Tests' }
            'INCONCLUSIVE' { return 'Inconclusive' }
            'TIMEDOUT'     { return 'Timed Out' }
            'FAILED'       { if ($isPartial) { return 'Partial' } else { return 'Failed' } }
        }
    }

    if ($GateContent -match '(?i)\binconclusive\b') { return 'Inconclusive' }
    if ($GateContent -match '(?i)\btimed[\s-]?out\b') { return 'Timed Out' }
    if ($isPartial) { return 'Partial' }
    if ($GateContent -match '(?i)\bfailed\b') { return 'Failed' }
    if ($GateContent -match '(?i)\bpassed\b') { return 'Passed' }
    if ($GateContent -match '(?i)no tests were detected|\bskipped\b') { return 'No Tests' }
    return 'Unknown'
}

function Get-ConfidenceStatus {
    param([string[]]$Contents)

    foreach ($content in $Contents) {
        if ([string]::IsNullOrWhiteSpace($content)) {
            continue
        }

        if ($content -match '(?im)\*\*Confidence:\*\*\s*(high|medium|low|unknown)') {
            return ConvertTo-TitleCase $Matches[1]
        }
        if ($content -match '(?im)^Confidence:\s*(high|medium|low|unknown)') {
            return ConvertTo-TitleCase $Matches[1]
        }
    }

    return 'Unknown'
}

function Get-PlatformStatus {
    param([string[]]$Contents)

    foreach ($content in $Contents) {
        if ([string]::IsNullOrWhiteSpace($content)) {
            continue
        }

        if ($content -match '(?im)\*\*Platform:\*\*\s*([A-Za-z, /]+)') {
            return ConvertTo-TitleCase (($Matches[1] -split '[,/]')[0])
        }
        if ($content -match '(?im)\*\*Platforms Affected:\*\*\s*([A-Za-z, /]+)') {
            return ConvertTo-TitleCase (($Matches[1] -split '[,/]')[0])
        }
    }

    return 'Unknown'
}

function New-StatusChipRow {
    param(
        [string]$GateStatus,
        [string]$Confidence,
        [string]$Platform
    )

    $gateColor = switch ($GateStatus) {
        'Passed'       { '1a7f37' }   # green
        'Partial'      { 'bf8700' }   # amber — mixed/inconclusive
        'Inconclusive' { '0e7490' }   # teal — could not build/run (infra), not a real fail (avoid purple ~ GitHub "merged")
        'Timed Out'    { '0e7490' }   # teal — gate stopped by its hang-safety timeout (infra), fix unverified
        'No Tests'     { '57606a' }   # neutral gray — nothing to verify
        'Unknown'      { '57606a' }   # neutral gray — gate did not run / no verdict (deep-only rerun); absence of data, NOT a failure
        'Failed'       { 'd1242f' }   # red
        default        { '57606a' }   # any unrecognized status renders neutral gray — red is reserved for a confirmed Failed gate only
    }
    $confidenceColor = switch ($Confidence) {
        'High' { '0969da' }
        'Medium' { 'bf8700' }
        'Low' { 'd1242f' }
        default { '57606a' }
    }
    $platformColor = if ($Platform -eq 'Unknown') { '57606a' } else { '1f6feb' }  # blue (avoid purple ~ GitHub "merged")

    $chips = @(
        (New-StatusChip -Label 'Gate' -Value $GateStatus -Color $gateColor),
        (New-StatusChip -Label 'Confidence' -Value $Confidence -Color $confidenceColor),
        (New-StatusChip -Label 'Platform' -Value $Platform -Color $platformColor)
    )

    return @"
<p align="left">
$($chips -join "`n")
</p>
"@
}

function New-FutureActionSection {
    param(
        [Parameter(Mandatory = $true)][string]$PRAgentDir
    )

    $winnerFile = Join-Path $PRAgentDir "winner.json"
    if (-not (Test-Path $winnerFile)) {
        return @"
---

<details>
<summary><strong>🧭 Next Steps</strong> — review latest findings</summary>
<br/>

No alternative fix was selected for this run. Review the session findings and CI results before merging.

</details>
"@
    }

    try {
        $winner = Get-Content -Raw -LiteralPath $winnerFile -Encoding UTF8 | ConvertFrom-Json
    } catch {
        return @"
---

<details>
<summary><strong>🧭 Next Steps</strong> — review latest findings</summary>
<br/>

The workflow could not parse the fix-selection result. Review the session findings and CI results before merging.

</details>
"@
    }

    $selected = [string]$winner.winner
    if ([string]::IsNullOrWhiteSpace($selected) -or $selected -eq 'pr') {
        return @"
---

<details>
<summary><strong>🧭 Next Steps</strong> — review latest findings</summary>
<br/>

No alternative fix was selected for this run. Review the session findings and CI results before merging.

</details>
"@
    }

    $rationale = if ($winner.summary) { [string]$winner.summary } else { "Automated review identified a stronger candidate fix." }
    $safeRationale = Escape-Html $rationale
    $safeSelected = Escape-Html $selected

    if ($selected -eq 'pr-plus-reviewer') {
        return @"
---

<details>
<summary><strong>🧭 Next Steps</strong> — reviewer changes required</summary>
<br/>

**The reviewer-enhanced candidate identified changes that are not yet in the submitted PR.**

**Why:** $safeRationale

Address the actionable findings in this review before merging.

</details>
"@
    }

    $diff = [string]$winner.candidateDiff
    $truncated = $false

    if ([string]::IsNullOrWhiteSpace($diff)) {
        $diff = "Candidate diff was not available in winner.json."
    } else {
        $maxDiffBytes = 55KB
        $marker = "`n... [truncated]"
        $markerBytes = [System.Text.Encoding]::UTF8.GetByteCount($marker)
        $budget = $maxDiffBytes - $markerBytes
        if ([System.Text.Encoding]::UTF8.GetByteCount($diff) -gt $maxDiffBytes) {
            $lo = 0
            $hi = $diff.Length
            while ($lo -lt $hi) {
                $mid = [int](($lo + $hi + 1) / 2)
                $bytes = [System.Text.Encoding]::UTF8.GetByteCount($diff.Substring(0, $mid))
                if ($bytes -le $budget) { $lo = $mid } else { $hi = $mid - 1 }
            }
            $diff = $diff.Substring(0, $lo) + $marker
            $truncated = $true
        }
    }

    $maxBacktickRun = 0
    foreach ($m in [regex]::Matches($diff, '`+')) {
        if ($m.Length -gt $maxBacktickRun) { $maxBacktickRun = $m.Length }
    }
    $fenceLen = [Math]::Max(4, $maxBacktickRun + 1)
    $fence = '`' * $fenceLen
    $truncatedNote = if ($truncated) { "`n_The diff was truncated to fit GitHub's review body limit._" } else { "" }

    return @"
---

<details>
<summary><strong>🧭 Next Steps</strong> — alternative fix proposed (<code>$safeSelected</code>)</summary>
<br/>

**Automated review — alternative fix proposed**

The expert-reviewer evaluation compared the PR fix against automatically generated candidates and selected <code>$safeSelected</code> as the strongest fix.

**Why:** $safeRationale

Please consider applying the candidate diff below (or use it as guidance). Once you push an update, this workflow will re-trigger and re-evaluate.

<details><summary>Candidate diff (<code>$safeSelected</code>)</summary>

${fence}diff
$diff
$fence

</details>
$truncatedNote

</details>
"@
}

function Test-WinnerRequiresPRChanges {
    param(
        [Parameter(Mandatory = $true)][string]$PRAgentDir
    )

    $winnerFile = Join-Path $PRAgentDir "winner.json"
    if (-not (Test-Path $winnerFile)) {
        return $false
    }

    try {
        $winner = Get-Content -Raw -LiteralPath $winnerFile -Encoding UTF8 | ConvertFrom-Json
        $winnerName = [string]$winner.winner
        if ([string]::IsNullOrWhiteSpace($winnerName)) {
            return $false
        }

        return ($winner.isPRFix -eq $false) -or ($winnerName -match '(?i)^(pr-plus-reviewer|try-fix(?:-|$))')
    } catch {
        return $false
    }
}

function Test-RunValidationFailed {
    param(
        [Parameter(Mandatory = $true)][string]$PRAgentDir,
        [Parameter(Mandatory = $true)][string]$TrustedGateResult
    )

    # Gate: key off the TRUSTED gate verdict passed in by the pipeline (derived from the
    # Gate task's process exit code / its freshly-written staging file, captured BEFORE the
    # untrusted CopilotReview phase runs and frozen as an Azure output variable). Do NOT read
    # gate/gate-result.txt or gate/content.md from $PRAgentDir — both live in the agent-writable
    # worktree/artifact, so a prompt-injected review agent could overwrite a real FAILED gate
    # with "PASSED" before this trusted posting step and bypass the APPROVE veto.
    # FAILED = a real test regression; TIMEDOUT = the gate never finished (fix unverified) —
    # both must veto an APPROVE. INCONCLUSIVE/SKIPPED stay non-blocking sentinels.
    if ($TrustedGateResult -match '(?im)^\s*(FAILED|TIMEDOUT)\s*$') { return $true }

    # UI tests: the pipeline render writes "❌ **Deep UI tests** — N passed, M failed …" with no
    # "Result:" line, so detect the failure icon on a bold test header or a non-zero "N failed"
    # count ("marked failed by TRX" on the passing branch has no digit immediately before
    # "failed", so it does not match). Skip the "no UI tests needed" no-op placeholder.
    $uiFile = Join-Path $PRAgentDir 'uitests/content.md'
    if (Test-Path -LiteralPath $uiFile) {
        $uiContent = Get-Content -Raw -LiteralPath $uiFile -Encoding UTF8
        if (-not (Test-PhaseContentIsNoOp -PhaseKey 'uitests' -Content $uiContent) -and
            ($uiContent -match '(?im)❌\s*\*\*[^*\n]*tests\*\*' -or $uiContent -match '(?im)\b[1-9]\d*\s+failed\b')) {
            return $true
        }
    }

    return $false
}

function Test-DeepUITestsHadNoSignal {
    # True when the deep-UI run produced NO positive signal — every category hit a
    # OneTimeSetUp/fixture-setup failure (HostApp crash / platform-pool infra) and nothing
    # passed or regularly failed. The pipeline renders this as ⚠️ "… could not run:
    # OneTimeSetUp/fixture setup failure …" with no "N passed" — so it escapes both the ❌ and
    # "N failed" veto. An APPROVE over such a run is too generous (zero deep-UI tests actually
    # completed); callers soften APPROVE→COMMENT (not REQUEST_CHANGES — a crash may be a flake).
    param([Parameter(Mandatory = $true)][string]$PRAgentDir)

    $uiFile = Join-Path $PRAgentDir 'uitests/content.md'
    if (-not (Test-Path -LiteralPath $uiFile)) { return $false }
    $uiContent = Get-Content -Raw -LiteralPath $uiFile -Encoding UTF8
    if ([string]::IsNullOrWhiteSpace($uiContent)) { return $false }

    # Match every "no-signal" render the pipeline emits when regularFailed==0 and nothing
    # passed: fixture setup failure, HostApp crash, or a selected category with zero runnable
    # tests. appCrashCategories takes priority over setup failures in the render, so the crash
    # header — the originally-flagged escape — must be matched explicitly.
    $noSignalHeader = ($uiContent -match '(?im)could not run:\s*OneTimeSetUp/fixture setup failure') -or
                      ($uiContent -match '(?im)the HostApp crashed mid-run, so .*could not complete') -or
                      ($uiContent -match '(?im)\b(?:category|categories) reported 0 tests\.')
    # Require no completed-test signal at all (no positive "N passed", no non-zero
    # "N failed"). A zero-test headline legitimately says "0 passed, 0 failed";
    # those zero counts must not masquerade as positive execution signal.
    return ($noSignalHeader -and
            $uiContent -notmatch '(?im)\b[1-9]\d*\s+passed\b' -and
            $uiContent -notmatch '(?im)\b[1-9]\d*\s+failed\b')
}

function Test-ExpertReviewIsBlocking {
    <#
    .SYNOPSIS
        True when the expert code-review artifact carries a blocking verdict.
    .DESCRIPTION
        The expert reviewer writes its verdict to expert-pr-eval/content.md (older runs
        used pre-flight/code-review.md). That verdict is now rendered into the posted
        summary, so a formal APPROVE over a NEEDS_CHANGES/NEEDS_DISCUSSION expert verdict
        makes the review visibly self-contradictory. Only the FIRST artifact that carries a
        usable verdict is consulted (current wins over legacy), matching the precedence in
        Get-OutcomeFromCodeReviewVerdict (Update-AgentLabels.ps1) so the review event and
        the derived outcome label can never disagree. Any read/parse issue returns $false so
        a missing/garbled artifact never invents a blocking verdict.
    #>
    param([Parameter(Mandatory = $true)][string]$PRAgentDir)

    foreach ($rel in @('expert-pr-eval/content.md', 'pre-flight/code-review.md')) {
        $file = Join-Path $PRAgentDir $rel
        if (-not (Test-Path -LiteralPath $file)) { continue }
        $content = $null
        try { $content = Get-Content -Raw -LiteralPath $file -Encoding UTF8 -ErrorAction Stop } catch { continue }
        if ([string]::IsNullOrWhiteSpace($content)) { continue }

        $verdict = $null
        if ($content -match '(?im)Verdict:\s*\**\s*(LGTM|APPROVE|NEEDS[ _]?CHANGES|NEEDS[ _]?DISCUSSION|REQUEST[ _]?CHANGES)') {
            $verdict = $Matches[1]
        }
        elseif ($content -match '(?im)^[ \t]*#{1,6}[ \t]+(?:Initial[ \t]+)?Verdict[^\r\n]*(?:\r?\n[ \t]*)+\**[ \t]*(LGTM|APPROVE|NEEDS[ _]?CHANGES|NEEDS[ _]?DISCUSSION|REQUEST[ _]?CHANGES)\b') {
            $verdict = $Matches[1]
        }
        if ($verdict) {
            return ($verdict -notmatch '(?i)^(LGTM|APPROVE)')
        }
    }

    return $false
}

function Get-AIReviewEventForRun {
    param(
        [string]$ReportContent,

        [Parameter(Mandatory = $true)]
        [string]$PRAgentDir,

        [string]$TrustedGateResult
    )

    # Fail closed: the APPROVE veto must never run against an absent gate signal. Callers must
    # pass the trusted pipeline-supplied verdict explicitly (local/manual callers pass a
    # non-blocking sentinel such as 'SKIPPED').
    if ([string]::IsNullOrWhiteSpace($TrustedGateResult)) {
        throw "TrustedGateResult is required: the APPROVE veto must key off the trusted pipeline gate verdict, not the agent-writable worktree."
    }

    $reviewEvent = Get-AIReviewEvent -ReportContent $ReportContent

    # A pr-plus-reviewer or try-fix winner means the submitted PR still needs the
    # winning changes. This machine-readable result vetoes an accidental prose APPROVE.
    if (Test-WinnerRequiresPRChanges -PRAgentDir $PRAgentDir) {
        return 'REQUEST_CHANGES'
    }

    # Validation veto: never post an APPROVE review over a failed gate / device-test validation,
    # even when the report body recommends APPROVE (the report can be stale vs. current-run results).
    if ($reviewEvent -eq 'APPROVE' -and (Test-RunValidationFailed -PRAgentDir $PRAgentDir -TrustedGateResult $TrustedGateResult)) {
        return 'REQUEST_CHANGES'
    }

    # Expert-verdict veto: the expert code-review section is rendered into the same summary, so
    # approving over a NEEDS_CHANGES/NEEDS_DISCUSSION expert verdict posts a self-contradictory
    # review (blocking findings shown, formal approval granted). The expert verdict is the more
    # specific signal, so it wins over the Report LLM's prose recommendation.
    if ($reviewEvent -eq 'APPROVE' -and (Test-ExpertReviewIsBlocking -PRAgentDir $PRAgentDir)) {
        return 'REQUEST_CHANGES'
    }

    # Soften (not veto) a positive APPROVE when the deep-UI run produced no passing signal at
    # all — every category crashed / hit a setup failure. COMMENT is neutral; the crash may be
    # an infra flake rather than a PR regression, so REQUEST_CHANGES would be too harsh.
    if ($reviewEvent -eq 'APPROVE' -and (Test-DeepUITestsHadNoSignal -PRAgentDir $PRAgentDir)) {
        return 'COMMENT'
    }

    return $reviewEvent
}

function Invoke-PostPullRequestReview {
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [Parameter(Mandatory = $true)]
        [string]$Body,

        [Parameter(Mandatory = $true)]
        [ValidateSet('APPROVE', 'REQUEST_CHANGES', 'COMMENT')]
        [string]$Event,

        [Parameter(Mandatory = $false)]
        [ValidatePattern('^$|^[0-9a-fA-F]{40}$')]
        [string]$CommitSha = ''
    )

    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        $payload = [ordered]@{ body = $Body; event = $Event }
        if (-not [string]::IsNullOrWhiteSpace($CommitSha)) {
            $payload['commit_id'] = $CommitSha
        }
        $payload |
            ConvertTo-Json -Depth 10 |
            Set-Content -Path $tempFile -Encoding UTF8

        $response = gh api --method POST "repos/$Repository/pulls/$PRNumber/reviews" --input $tempFile 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "POST review failed (exit code $LASTEXITCODE): $response"
        }

        return (($response -join [Environment]::NewLine) | ConvertFrom-Json)
    } finally {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    }
}

# ─── Gate content (rendered first, collapsed) ───
$gateSection = $null
$gateContent = $null
$gateFilePath = Join-Path $PRAgentDir "gate/content.md"
if (Test-Path $gateFilePath) {
    $gateContent = Get-Content $gateFilePath -Raw -Encoding UTF8
    if (-not [string]::IsNullOrWhiteSpace($gateContent)) {
        Write-Host "  ✅ gate ($((Get-Item $gateFilePath).Length) bytes)" -ForegroundColor Green
    } else {
        Write-Host "  ⏭️  gate (empty)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ⏭️  gate (not found)" -ForegroundColor Gray
}

$hadPersistedGateContent = -not [string]::IsNullOrWhiteSpace($gateContent)
$gateContent = Get-AuthoritativeGateContent -GateContent $gateContent -TrustedGateResult $TrustedGateResult

if ($TrustedGateResult -match '(?i)^\s*TIMEDOUT\s*$') {
    if ($hadPersistedGateContent) {
        Write-Host "  ⏱️  gate (discarded partial content; trusted verdict is TIMEDOUT)" -ForegroundColor Yellow
    } else {
        Write-Host "  ⏱️  gate (synthesized TIMEDOUT section — gate did not complete)" -ForegroundColor Yellow
    }
}

if (-not [string]::IsNullOrWhiteSpace($gateContent)) {
    $gateOpen = if ($TrustedGateResult -match '(?i)^\s*TIMEDOUT\s*$') { ' open' } else { '' }
    $gateSection = @"
<details$gateOpen>
<summary><strong>🚦 Gate — Test Before & After Fix</strong></summary>
<br/>

$gateContent

</details>
"@
}

$phaseSections = @()
$phaseContentByKey = [ordered]@{}
$phaseTitleByKey = @{}

foreach ($key in $phases.Keys) {
    $phase = $phases[$key]
    $phaseContent = Get-FirstPhaseContent -Root $PRAgentDir -RelativePaths $phase.Files

    if ($phaseContent) {
        $filePath = $phaseContent.Path
        $content = $phaseContent.Content
        if (Test-PhaseContentIsNoOp -PhaseKey $key -Content $content) {
            Write-Host "  ⏭️  $key (no actionable content)" -ForegroundColor Gray
            continue
        }

        # For uitests, annotate the "detected categories but no results" placeholder so an
        # empty section explains itself instead of showing only the detected categories.
        if ($key -eq "uitests") {
            $content = Add-MissingUITestResultsNote `
                -Content $content `
                -TrustedGateResult $TrustedGateResult
        }
        $phaseContentByKey[$key] = $content
        Write-Host "  ✅ $key ($((Get-Item -LiteralPath $filePath).Length) bytes)" -ForegroundColor Green
        # For uitests, make title dynamic: "UI Tests — Cat1, Cat2"
        $phaseTitle = $phase.Title
        if ($key -eq "uitests") {
            $catMatch = [regex]::Match($content, 'Detected UI test categories:\*\*\s*`{1,2}([^`]+)`{1,2}')
            if ($catMatch.Success) {
                $phaseTitle = "$($phase.Title) — $($catMatch.Groups[1].Value)"
            }
        }
        $phaseTitleByKey[$key] = $phaseTitle
    } else {
        Write-Host "  ⏭️  $key (not found)" -ForegroundColor Gray
    }
}

# Keep every expected expert-review section visible. Task 3 can persist pre-flight,
# code-review, and try-fix output but still hit its time budget before report/content.md;
# previously that silently removed the Report section and made a partial review look complete.
$hadActualPhaseContent = $phaseContentByKey.Count -gt 0
$agentPhaseKeys = @('pre-flight', 'code-review', 'try-fix', 'report')
foreach ($key in $agentPhaseKeys) {
    if (-not $phaseContentByKey.Contains($key)) {
        $phaseContentByKey[$key] = New-MissingAgentPhaseContent -PhaseKey $key
        $phaseTitleByKey[$key] = $phases[$key].Title
        Write-Host "  ℹ️ Added explicit $key placeholder (phase output missing)" -ForegroundColor Yellow
    }
}

foreach ($key in $phases.Keys) {
    if (-not $phaseContentByKey.Contains($key)) {
        continue
    }

    $phaseSections += @"
<details>
<summary><strong>$($phaseTitleByKey[$key])</strong></summary>
<br/>

$($phaseContentByKey[$key])

</details>
"@
}

if (-not $gateSection) {
    # Reliability guard: in the deferred Stage-3 deep-results post, the PRAgent phase content
    # (gate/content.md, code-review/content.md, …) can be absent even though the pipeline DID
    # run and handed us a real trusted gate verdict — e.g. the content dir was not carried into
    # the Stage-3 job, or the earlier review phase produced no files. Previously this hard-threw
    # (exit 1), which FAILED the Post stage AND posted nothing: the Task-4 fallback notice never
    # fires because Task-4 already deferred (aiSummaryReviewId='DEFERRED', not empty), so the PR
    # got no summary at all (build 14829982, PR #36657: TRX deep results present, gate verdict
    # INCONCLUSIVE, but every phase file "not found"). Rather than crash, synthesize a minimal
    # gate section from the trusted verdict so the PR ALWAYS gets a summary (deep results are
    # folded in below as usual). Only hard-throw when there is genuinely nothing — no phase
    # content AND no trusted verdict (a local/manual misconfiguration).
    if (-not [string]::IsNullOrWhiteSpace($TrustedGateResult)) {
        $verdictUpper = $TrustedGateResult.ToUpperInvariant()
        Write-Host "  ⚠️  No phase content found, but a trusted gate verdict ('$verdictUpper') was supplied — synthesizing a minimal gate section so the PR still gets a summary." -ForegroundColor Yellow
        $gateContent = @"
### Gate Result: $verdictUpper — detailed report unavailable

The automated **test-verification gate** produced a **$verdictUpper** verdict, but its detailed per-test report could not be attached to this summary on this run (the review's phase content was not available when the deep results were posted). This is an **infrastructure** hiccup in assembling the report — **not** a problem with your PR.

- The trusted gate verdict above is authoritative for the review decision.
- Any deep UI test results for this run are shown below.

**Next step:** re-comment ``/review`` to get a full report on a fresh agent.
"@
        $gateSection = @"
<details open>
<summary><strong>🚦 Gate — Test Before & After Fix</strong></summary>
<br/>

$gateContent

</details>
"@
    } elseif (-not $hadActualPhaseContent) {
        throw "No gate or phase content found. Ensure at least one of gate/content.md or {phase}/content.md exists in $PRAgentDir."
    }
}

# The trusted gate verdict comes from the pipeline (Gate task output variable). For
# local/manual invocations that never post APPROVE, fall back to the non-blocking 'SKIPPED'
# sentinel so the veto is a no-op rather than reading any agent-writable worktree file.
$effectiveGateResult = if ([string]::IsNullOrWhiteSpace($TrustedGateResult)) { 'SKIPPED' } else { $TrustedGateResult }
$reviewEvent = Get-AIReviewEventForRun -ReportContent $phaseContentByKey['report'] -PRAgentDir $PRAgentDir -TrustedGateResult $effectiveGateResult

# ============================================================================
# FETCH PR METADATA (commit + author)
# ============================================================================

try {
    $prMetadata = gh api "repos/$Repository/pulls/$PRNumber" --jq '{author: .user.login, head: .head.sha}' 2>$null | ConvertFrom-Json
} catch {
    Write-Host "⚠️ Failed to fetch current PR metadata: $_" -ForegroundColor Yellow
    $prMetadata = $null
}
$currentHeadSha = if ($prMetadata) { [string]$prMetadata.head } else { '' }
$commitFull = if (-not [string]::IsNullOrWhiteSpace($ReviewedCommit)) { $ReviewedCommit } else { $currentHeadSha }
$commitSha7 = if ($commitFull.Length -ge 7) { $commitFull.Substring(0, 7) } else { "unknown" }
$commitUrl = if ($commitFull) { "https://github.com/dotnet/maui/commit/$commitFull" } else { "#" }
$prAuthor = if ($prMetadata) { [string]$prMetadata.author } else { $null }

$snapshotNotice = $null
if (-not [string]::IsNullOrWhiteSpace($ReviewedCommit)) {
    if ([string]::IsNullOrWhiteSpace($currentHeadSha)) {
        $reviewEvent = 'COMMENT'
        $snapshotNotice = @"
> [!WARNING]
> This run reviewed commit [``$commitSha7``]($commitUrl), but the current PR head could not be verified while posting. The result is informational and no current-head approval or change request was applied.
"@
    } elseif (-not $currentHeadSha.Equals($ReviewedCommit, [StringComparison]::OrdinalIgnoreCase)) {
        $currentHeadSha7 = $currentHeadSha.Substring(0, [Math]::Min(7, $currentHeadSha.Length))
        $currentHeadUrl = "https://github.com/dotnet/maui/commit/$currentHeadSha"
        $reviewEvent = 'COMMENT'
        $snapshotNotice = @"
> [!WARNING]
> This run reviewed commit [``$commitSha7``]($commitUrl), but the PR advanced to [``$currentHeadSha7``]($currentHeadUrl) while it was running. These results are informational; re-run ``/review`` for the current head.
"@
    }
}
$reviewCommitForApi = if ($snapshotNotice) { '' } else { $commitFull }
Write-Host "  🧾 PR review event: $reviewEvent (trusted gate: $effectiveGateResult; reviewed commit: $commitSha7)" -ForegroundColor Cyan

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm UTC")

# ============================================================================
# BUILD NEW SESSION BLOCK
# ============================================================================

# Combine gate (always first) with phases (collapsed). When only one
# kind of content is available, the session still renders cleanly.
$sessionParts = @()
if ($snapshotNotice)          { $sessionParts += $snapshotNotice }
if ($gateSection)            { $sessionParts += $gateSection }
if ($phaseSections.Count -gt 0) { $sessionParts += ($phaseSections -join "`n`n---`n`n") }
$phaseContent = $sessionParts -join "`n`n---`n`n"

$sessionMarkerStart = "<!-- SESSION:$commitSha7 START -->"
$sessionMarkerEnd = "<!-- SESSION:$commitSha7 END -->"

$newSessionBlock = @"
$sessionMarkerStart
<details>
<summary><strong>🗂️ Review Sessions</strong> — click to expand</summary>
<br/>

$phaseContent

</details>
$sessionMarkerEnd
"@

# ============================================================================
# FIND EXISTING AI SUMMARY ARTIFACTS & BUILD FINAL BODY
# ============================================================================

Write-Host "Checking for existing AI Summary artifacts..." -ForegroundColor Yellow
$existingCommentIds = @()
$existingReviewIds = @()
$existingBodies = @()

$existingRaw = gh api "repos/$Repository/issues/$PRNumber/comments" --paginate 2>$null
if ($existingRaw) {
    try {
        $allComments = $existingRaw | ConvertFrom-Json
        $existingObjs = @($allComments | Where-Object { $_.body -and $_.body.Contains($MARKER) })
        if ($existingObjs.Count -gt 0) {
            $existingCommentIds = @($existingObjs | ForEach-Object { $_.id })
            $existingBodies = @($existingObjs | ForEach-Object { [string]$_.body })
            Write-Host "✓ Found existing AI Summary issue comment(s): $($existingCommentIds -join ', ')" -ForegroundColor Green
        }

        if (Get-Command Get-GitHubPullRequestReviews -ErrorAction SilentlyContinue) {
            $existingReviewObjs = @(Get-GitHubPullRequestReviews -PRNumber $PRNumber | Where-Object { $_.body -and $_.body.Contains($MARKER) })
            if ($existingReviewObjs.Count -gt 0) {
                $existingReviewIds = @($existingReviewObjs | ForEach-Object { $_.id })
                $existingBodies += @($existingReviewObjs | ForEach-Object { [string]$_.body })
                Write-Host "✓ Found existing AI Summary review(s): $($existingReviewIds -join ', ')" -ForegroundColor Green
            }
        }
    } catch {
        Write-Host "⚠️ Could not parse comments: $_" -ForegroundColor Yellow
    }
}

$authorPing = ""
if ($prAuthor) {
    $authorPing = "> @$prAuthor — new AI review results are available based on commit <a href=`"$commitUrl`"><code>$commitSha7</code></a>."
}

$summaryContent = @($gateContent) + @($phaseContentByKey.Values)
$resolvedPlatform = Get-PlatformStatus -Contents $summaryContent
# Fall back to the pipeline-supplied review/deep platform when the content names none
# (e.g. a deep-only rerun with no code-review phase) so the chip shows the real platform
# instead of a misleading "Unknown" (dotnet/maui#35606).
if ($resolvedPlatform -eq 'Unknown' -and -not [string]::IsNullOrWhiteSpace($Platform)) {
    $resolvedPlatform = ConvertTo-TitleCase $Platform
}
$statusChipRow = New-StatusChipRow `
    -GateStatus (Get-GateStatus -GateContent $gateContent) `
    -Confidence (Get-ConfidenceStatus -Contents $summaryContent) `
    -Platform $resolvedPlatform
$futureActionSection = New-FutureActionSection -PRAgentDir $PRAgentDir

$commentBody = @"
$MARKER

## AI Review Summary

$authorPing

$statusChipRow

---

$newSessionBlock

$futureActionSection
"@

# Clean up excessive blank lines
$commentBody = $commentBody -replace "`n{4,}", "`n`n`n"

Write-Host "  ✅ Built review body ($($commentBody.Length) chars)" -ForegroundColor Green

# GitHub caps both PR-review bodies AND issue-comment bodies at 65,536 characters. A body over
# that limit makes every POST path fail with HTTP 422 "Body is too long". Rebuild oversized
# summaries with per-section budgets first so Gate, every expert phase, applicable UI Tests, and
# Next Steps all remain present. The final substring fallback below is only a last-resort guard.
$githubBodyMaxChars = 65500
if ($commentBody.Length -gt $githubBodyMaxChars) {
    Write-Host "  ℹ Review body exceeded $githubBodyMaxChars chars; compacting large sections while preserving all headings." -ForegroundColor Yellow

    $compactBudgets = @{
        'pre-flight'       = 4000
        'code-review'      = 6500
        'try-fix'          = 5000
        'pr-finalize'      = 3000
        'report'           = 4000
        'regression-check' = 2500
        'uitests'          = 12000
    }

    $compactSessionParts = @()
    if ($gateSection) {
        $compactGateContent = Limit-MarkdownContent -Content $gateContent -MaxChars 7000 -SectionName 'Gate'
        $compactGateOpen = if ($gateContent -match '(?i)TIMEDOUT|detailed report unavailable') { ' open' } else { '' }
        $compactSessionParts += @"
<details$compactGateOpen>
<summary><strong>🚦 Gate — Test Before & After Fix</strong></summary>
<br/>

$compactGateContent

</details>
"@
    }

    $compactPhaseSections = @()
    foreach ($key in $phases.Keys) {
        if (-not $phaseContentByKey.Contains($key)) {
            continue
        }

        $compactContent = Limit-MarkdownContent `
            -Content $phaseContentByKey[$key] `
            -MaxChars $compactBudgets[$key] `
            -SectionName $key

        $compactPhaseSections += @"
<details>
<summary><strong>$($phaseTitleByKey[$key])</strong></summary>
<br/>

$compactContent

</details>
"@
    }

    if ($compactPhaseSections.Count -gt 0) {
        $compactSessionParts += ($compactPhaseSections -join "`n`n---`n`n")
    }

    $compactPhaseContent = $compactSessionParts -join "`n`n---`n`n"
    $compactSessionBlock = @"
$sessionMarkerStart
<details>
<summary><strong>🗂️ Review Sessions</strong> — click to expand</summary>
<br/>

$compactPhaseContent

</details>
$sessionMarkerEnd
"@
    $compactFutureActionSection = Limit-MarkdownContent -Content $futureActionSection -MaxChars 4000 -SectionName 'Next Steps'
    $commentBody = @"
$MARKER

## AI Review Summary

$authorPing

$statusChipRow

---

$compactSessionBlock

$compactFutureActionSection
"@
    $commentBody = $commentBody -replace "`n{4,}", "`n`n`n"
    Write-Host "  ✅ Compacted review body to $($commentBody.Length) chars with all required sections preserved." -ForegroundColor Green
}

if ($commentBody.Length -gt $githubBodyMaxChars) {
    $truncationNotice = "`n`n---`n`nℹ **Summary truncated** — this report exceeded GitHub's 65,536-character limit. See the full deep-test results and analysis in the pipeline build artifacts."
    $keep = $githubBodyMaxChars - $truncationNotice.Length
    if ($keep -lt 0) { $keep = 0 }
    $commentBody = $commentBody.Substring(0, $keep)
    # If truncation left an unbalanced fenced code block open, close it so markdown stays valid.
    $codeFence = [string][char]96 * 3
    if ((([regex]::Matches($commentBody, '(?m)^```')).Count % 2) -ne 0) {
        $commentBody += "`n" + $codeFence
    }
    $commentBody += $truncationNotice
    Write-Host "  ℹ Review body exceeded $githubBodyMaxChars chars; truncated to $($commentBody.Length) chars." -ForegroundColor Yellow
}

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "Review event: $reviewEvent" -ForegroundColor Cyan
    Write-Host "=== COMMENT PREVIEW ===" -ForegroundColor Cyan
    Write-Host $commentBody
    Write-Host "=== END PREVIEW ===" -ForegroundColor Cyan
    exit 0
}

# ============================================================================
# HIDE STALE GENERATED ARTIFACTS, THEN POST REVIEW
# ============================================================================

# ============================================================================
# HIDE STALE GENERATED ARTIFACTS, THEN POST
# ============================================================================
#
# The pipeline's posting token (GH_COMMENT_TOKEN, a GitHub App token) can CREATE PR
# reviews but CANNOT update / dismiss / minimize them (PUT + dismiss both return HTTP 404
# in-pipeline, though they succeed with a full-permission PAT), so posting the AI Summary
# as a REVIEW every build stacks them indefinitely (observed 40+ on one PR). Issue
# comments, by contrast, ARE editable by this token. So for the common COMMENT verdict
# (no formal veto) we post/UPDATE a single AI-Summary ISSUE COMMENT in place — it never
# stacks. Formal APPROVE / CHANGES_REQUESTED verdicts still post a review (they carry the
# review state and are far less frequent). Any failure in the comment path falls back to
# posting a review, so the worst case is the previous behavior.
$review = $null
$postedEvent = $reviewEvent

if ($reviewEvent -eq 'COMMENT') {
    # "Mark the previous one as outdated, then post a new summary." MauiBot's token CAN
    # minimizeComment (collapse as outdated) but CANNOT unminimizeComment (FORBIDDEN — proven
    # in-pipeline: "MauiBot does not have the correct permissions to execute UnminimizeComment").
    # So we must NOT reuse+PATCH a comment that a prior sweep may have collapsed (we could never
    # un-hide it → the fresh summary would stay invisible). Instead: collapse EVERY prior
    # AI-Summary issue comment (and stale notices) as outdated, then post a brand-new comment.
    # This only uses the permission MauiBot has, and gives one visible summary above a stack of
    # collapsed "outdated" ones — the behavior maintainers expect.
    if (Get-Command Hide-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
        Hide-StaleMauiBotIssueComments `
            -PRNumber $PRNumber `
            -IncludeAISummary `
            -IncludeLegacyGate `
            -IncludeMergeConflict `
            -IncludeTryFix `
            -IncludeReviewIncomplete `
            -Reason "superseded by a newer AI Summary"
    }
    # Best-effort collapse of any stale AI-Summary REVIEWS (from before the issue-comment design).
    if (Get-Command Hide-StaleMauiBotPullRequestReviews -ErrorAction SilentlyContinue) {
        Hide-StaleMauiBotPullRequestReviews -PRNumber $PRNumber -IncludeAISummary -IncludeTryFix -Reason "superseded by a newer AI Summary" -DismissFormalReviews
    }

    try {
        $bodyTmp = New-TemporaryFile
        @{ body = $commentBody } | ConvertTo-Json -Depth 6 | Set-Content $bodyTmp.FullName -Encoding UTF8
        Write-Host "Posting a new AI Summary issue comment (previous ones collapsed as outdated)..." -ForegroundColor Yellow
        $cRaw = gh api --method POST "repos/$Repository/issues/$PRNumber/comments" --input $bodyTmp.FullName 2>&1
        Remove-Item $bodyTmp.FullName -ErrorAction SilentlyContinue
        if ($LASTEXITCODE -eq 0) {
            $review = $cRaw | ConvertFrom-Json
            $postedEvent = 'COMMENT'
            Write-Host "✅ New AI Summary issue comment posted (ID: $($review.id))" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Issue-comment post failed; falling back to a review. $cRaw" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ Issue-comment path threw; falling back to a review: $_" -ForegroundColor Yellow
    }
}

if (-not $review) {
    # Formal verdict (APPROVE / CHANGES_REQUESTED) OR the issue-comment path failed:
    # post a PR review (the previous behavior).
    if (Get-Command Hide-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
        Hide-StaleMauiBotIssueComments `
            -PRNumber $PRNumber `
            -IncludeAISummary `
            -IncludeLegacyGate `
            -IncludeMergeConflict `
            -IncludeTryFix `
            -IncludeReviewIncomplete `
            -Reason "stale generated PR review artifact"
    }

    if (Get-Command Hide-StaleMauiBotPullRequestReviews -ErrorAction SilentlyContinue) {
        Hide-StaleMauiBotPullRequestReviews `
            -PRNumber $PRNumber `
            -IncludeAISummary `
            -IncludeTryFix `
            -Reason "stale generated PR review" `
            -DismissFormalReviews
    }

    Write-Host "Creating new AI Summary PR review ($reviewEvent)..." -ForegroundColor Yellow
    $postedEvent = $reviewEvent
    try {
        $review = Invoke-PostPullRequestReview -PRNumber $PRNumber -Body $commentBody -Event $postedEvent -CommitSha $reviewCommitForApi
    } catch {
        if ($postedEvent -eq 'COMMENT') {
            throw
        }

        Write-Host "⚠️ Formal $postedEvent review was rejected; retrying as COMMENT: $_" -ForegroundColor Yellow
        $postedEvent = 'COMMENT'
        $review = Invoke-PostPullRequestReview -PRNumber $PRNumber -Body $commentBody -Event $postedEvent -CommitSha $reviewCommitForApi
    }
}

$reviewId = [string]$review.id
$reviewNodeId = [string]$review.node_id

if (-not [string]::IsNullOrWhiteSpace($reviewId)) {
    Set-Content -Path (Join-Path $PRAgentDir "ai-summary-review-id.txt") -Value $reviewId -Encoding UTF8
}
if (-not [string]::IsNullOrWhiteSpace($reviewNodeId)) {
    Set-Content -Path (Join-Path $PRAgentDir "ai-summary-review-node-id.txt") -Value $reviewNodeId -Encoding UTF8
}

Write-Host "✅ AI Summary PR review posted (ID: $reviewId, event: $postedEvent)" -ForegroundColor Green
Write-Output "AI_SUMMARY_REVIEW_ID=$reviewId"
Write-Output "AI_SUMMARY_REVIEW_NODE_ID=$reviewNodeId"
Write-Output "AI_SUMMARY_REVIEW_EVENT=$postedEvent"
