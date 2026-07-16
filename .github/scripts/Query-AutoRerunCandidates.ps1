#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Autonomously applies the s/agent-ready-for-rerun label to open PRs that have
    genuinely new PR-author activity since their last AI review.

.DESCRIPTION
    Driver for the PR Review Queue workflow. Enumerates open, non-draft PRs and
    evaluates each with Resolve-AutonomousRerunEligibility — the deterministic,
    AI-free counterpart of the `/review rerun` eligibility check. A PR qualifies
    only when it already carries a MauiBot AI Summary AND has a new commit, a new
    non-command author comment, or a head SHA that differs from the last reviewed
    SHA since that summary. Eligible PRs are labelled s/agent-ready-for-rerun,
    which the hourly rerun-review-scanner then picks up and re-reviews.

    No AI is used and untrusted text is never inspected semantically. PRs that
    were never AI-reviewed do not qualify. PRs that already carry the label, or
    that have a non-stale s/agent-review-in-progress label, are skipped.

.PARAMETER Owner
    Repository owner (default: dotnet).

.PARAMETER Repo
    Repository name (default: maui).

.PARAMETER Limit
    Maximum number of open PRs to inspect (default: 300).

.PARAMETER DryRun
    Evaluate and report without applying any labels.

.PARAMETER OutputPath
    Optional path to write a JSON summary of the decisions.

.EXAMPLE
    ./Query-AutoRerunCandidates.ps1 -DryRun
    # Read-only: classify open PRs without applying labels.
#>

param(
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [int]$Limit = 300,
    [switch]$DryRun,
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$ReadyForRerunLabel = 's/agent-ready-for-rerun'
$ReviewInProgressLabel = 's/agent-review-in-progress'

# Pass -Owner/-Repo through: Resolve-RerunEligibility.ps1 has its own $Owner/$Repo params
# defaulting to dotnet/maui, so dot-sourcing it without arguments would reset THIS script's
# $Owner/$Repo to the defaults (mirrors the correct pattern in Query-RerunReadyPRs.ps1).
. "$PSScriptRoot/Resolve-RerunEligibility.ps1" -Owner $Owner -Repo $Repo
. "$PSScriptRoot/shared/Update-AgentLabels.ps1"

# Derive the label description/color from the shared canonical definition so this script
# and Update-AgentLabels.ps1 can't drift and repeatedly re-PATCH each other's metadata.
$rerunLabelDef = $AllLabelDefs[$ReadyForRerunLabel]
$ReadyForRerunLabelDescription = $rerunLabelDef.Description
$ReadyForRerunLabelColor = $rerunLabelDef.Color

function Get-IssueLabels {
    param([int]$Number)

    # Don't silently treat an API failure as "no labels" — that would drop a real
    # s/agent-ready-for-rerun / in-progress label and cause a spurious re-label or
    # skip. Surface the failure so the per-PR try/catch records it as an error.
    $names = gh api "repos/$Owner/$Repo/issues/$Number/labels" --jq '.[].name' 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to fetch labels for #$Number (gh api exited $LASTEXITCODE)."
    }
    return @($names)
}

function Get-ActivityForPR {
    param([int]$Number)

    # Fail loud on API errors — same contract as Get-IssueLabels. A transient/auth/rate-limit
    # failure must NOT be treated as "no activity", which would silently mark a PR ineligible
    # and drop a real rerun candidate. Capture raw output first so $LASTEXITCODE reflects
    # `gh api` (not the trailing ForEach-Object in a pipeline). The per-PR try/catch records
    # the throw as an 'error' decision and continues.
    $issueCommentsRaw = gh api "repos/$Owner/$Repo/issues/$Number/comments?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch issue comments for #$Number (gh api exited $LASTEXITCODE)." }
    $reviewsRaw = gh api "repos/$Owner/$Repo/pulls/$Number/reviews?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch reviews for #$Number (gh api exited $LASTEXITCODE)." }
    $reviewCommentsRaw = gh api "repos/$Owner/$Repo/pulls/$Number/comments?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch review comments for #$Number (gh api exited $LASTEXITCODE)." }

    $issueComments = @($issueCommentsRaw | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'issue-comment' })
    $reviews = @($reviewsRaw | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review' })
    $reviewComments = @($reviewCommentsRaw | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review-comment' })
    return @($issueComments + $reviews + $reviewComments)
}

function Get-CommitsForPR {
    param([int]$Number)

    # Fail loud (see Get-ActivityForPR) — a dropped commit list would mislead the
    # new-head-commit eligibility check.
    $commitsRaw = gh api "repos/$Owner/$Repo/pulls/$Number/commits?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch commits for #$Number (gh api exited $LASTEXITCODE)." }
    return @($commitsRaw | ForEach-Object { $_ | ConvertFrom-Json })
}

$searchJson = gh pr list `
    --repo "$Owner/$Repo" `
    --state open `
    --limit $Limit `
    --json number,title,url,headRefOid,isDraft,labels,author
if ($LASTEXITCODE -ne 0) {
    throw "Failed to list open PRs (gh pr list exited with code $LASTEXITCODE)."
}
$openPRs = @($searchJson | ConvertFrom-Json)

# Warn if we hit the fetch cap — the scan would silently miss eligible PRs beyond it.
if ($openPRs.Count -ge $Limit) {
    Write-Host "  ⚠️  Fetched $($openPRs.Count) open PR(s), which meets the -Limit cap of $Limit; some open PRs may be excluded. Raise -Limit." -ForegroundColor Yellow
}

Write-Host "Inspecting $($openPRs.Count) open PR(s) for autonomous rerun eligibility..."

$labelEnsured = $false
$decisions = @()
$appliedCount = 0

foreach ($pr in $openPRs) {
    $number = [int]$pr.number
    $title = [string]$pr.title

    if ($pr.isDraft) {
        $decisions += [pscustomobject]@{ prNumber = $number; title = $title; eligible = $false; reason = 'draft'; applied = $false }
        continue
    }

    # Per-PR error isolation: a single malformed PR (e.g. a null/absent comment
    # created_at that throws in the eligibility date comparison, or a transient API
    # error) must not abort the whole daily scan. Record it as an error decision and
    # continue with the remaining PRs.
    try {
    # gh pr list (above) already fetched each PR's labels via --json labels, so derive the initial
    # label set from that search snapshot instead of an extra Issues-API call per PR. Get-IssueLabels
    # is reserved for the post-apply verification path below (and only when the add reports failure).
    $labels = @(@($pr.labels) | Where-Object { $_ } | ForEach-Object { $_.name })

    # Treat a stale in-progress label as absent so a wedged review can still be
    # re-detected — the same staleness rule the rerun scanner uses.
    $effectiveLabels = @($labels)
    if ($labels -contains $ReviewInProgressLabel -and (Test-AgentReviewInProgressIsStale -PRNumber $number -Owner $Owner -Repo $Repo)) {
        $effectiveLabels = @($labels | Where-Object { $_ -ne $ReviewInProgressLabel })
    }

    $activity = @(Get-ActivityForPR -Number $number)
    $commits = @(Get-CommitsForPR -Number $number)
    $rawAuthorLogin = if ($pr.author -and $pr.author.login) { [string]$pr.author.login } else { '' }
    $authorLogin = Normalize-GitHubActorLogin $rawAuthorLogin

    $result = Resolve-AutonomousRerunEligibility `
        -Comments $activity `
        -Commits $commits `
        -CurrentHeadSha $pr.headRefOid `
        -PRAuthorLogin $authorLogin `
        -CurrentLabels $effectiveLabels

    $alreadyPresent = @($labels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
    $applied = $false

    if ($result.Eligible -and -not $alreadyPresent) {
        if ($DryRun) {
            Write-Host "  [dry-run] Would label #$number ($($result.Reason)): $title"
        } else {
            if (-not $labelEnsured) {
                Ensure-LabelExists `
                    -LabelName $ReadyForRerunLabel `
                    -Description $ReadyForRerunLabelDescription `
                    -Color $ReadyForRerunLabelColor `
                    -Owner $Owner `
                    -Repo $Repo
                $labelEnsured = $true
            }

            $addSucceeded = Add-Label -PRNumber $number -LabelName $ReadyForRerunLabel -Owner $Owner -Repo $Repo
            $labelIsPresent = $false
            if (-not $addSucceeded) {
                # Safety net only when the POST reported failure (eventual consistency / already-present).
                # A verify-read failure must not discard a known-successful apply, so don't let it throw
                # here — otherwise a transient 502 on this second /labels call (plausible across up to
                # 300 PRs/run) would record a live label as applied=false and undercount $appliedCount.
                try {
                    $updatedLabels = @(Get-IssueLabels -Number $number)
                    $labelIsPresent = @($updatedLabels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
                } catch {
                    Write-Host "  ⚠️  Could not verify label state for #$($number): $($_.Exception.Message)" -ForegroundColor Yellow
                }
            }
            if ($addSucceeded -or $labelIsPresent) {
                $applied = $true
                $appliedCount++
                Write-Host "  ✅ Applied $ReadyForRerunLabel to #$number ($($result.Reason)): $title" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️  Failed to apply $ReadyForRerunLabel to #$number" -ForegroundColor Yellow
            }
        }
    } elseif ($result.Eligible -and $alreadyPresent) {
        Write-Host "  ⏭️  #$number already has $ReadyForRerunLabel — skipping"
    }

    $decisions += [pscustomobject]@{
        prNumber       = $number
        title          = $title
        eligible       = [bool]$result.Eligible
        reason         = [string]$result.Reason
        alreadyPresent = $alreadyPresent
        applied        = $applied
    }
    } catch {
        Write-Host "  ⚠️  Skipping #$number — evaluation error: $($_.Exception.Message)" -ForegroundColor Yellow
        $decisions += [pscustomobject]@{
            prNumber       = $number
            title          = $title
            eligible       = $false
            reason         = "error: $($_.Exception.Message)"
            alreadyPresent = $false
            applied        = $false
        }
        continue
    }
}

$eligibleCount = @($decisions | Where-Object { $_.eligible -and -not $_.alreadyPresent }).Count
if ($DryRun) {
    Write-Host "Autonomous rerun scan complete: $eligibleCount PR(s) eligible (dry-run, no labels applied)."
} else {
    Write-Host "Autonomous rerun scan complete: applied $ReadyForRerunLabel to $appliedCount PR(s)."
}

if ($OutputPath) {
    $outputDir = Split-Path -Parent $OutputPath
    if ($outputDir) {
        New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    }
    $summary = @{
        generatedAt = (Get-Date).ToUniversalTime().ToString('o')
        dryRun      = [bool]$DryRun
        applied     = $appliedCount
        eligible    = $eligibleCount
        decisions   = @($decisions)
    } | ConvertTo-Json -Depth 10
    $summary | Set-Content -LiteralPath $OutputPath -Encoding UTF8
    Write-Host "Wrote decision summary to $OutputPath"
}
