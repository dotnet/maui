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
    [ValidateRange(1, 10000)]
    [int]$Limit = 300,
    [switch]$DryRun,
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'

$ReadyForRerunLabel = 's/agent-ready-for-rerun'
$RerunDeclinedLabel = 's/agent-rerun-declined'
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
    # skip. Surface the failure (including gh's stderr, which we no longer suppress)
    # so the per-PR try/catch records it as an error with actionable detail.
    $names = gh api "repos/$Owner/$Repo/issues/$Number/labels" --jq '.[].name'
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to fetch labels for #$Number (gh api exited $LASTEXITCODE)."
    }
    return @($names)
}

function Get-IssueCommentsForPR {
    param([int]$Number)

    $issueCommentsRaw = gh api "repos/$Owner/$Repo/issues/$Number/comments?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch issue comments for #$Number (gh api exited $LASTEXITCODE)." }
    return @($issueCommentsRaw | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'issue-comment' })
}

function Get-ReviewActivityForPR {
    param([int]$Number)

    # AI Summary is posted as a pull-request review, not an issue comment. Fetch
    # review activity before deciding whether the PR has ever been AI-reviewed.
    $reviewsRaw = gh api "repos/$Owner/$Repo/pulls/$Number/reviews?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch reviews for #$Number (gh api exited $LASTEXITCODE)." }
    $reviewCommentsRaw = gh api "repos/$Owner/$Repo/pulls/$Number/comments?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch review comments for #$Number (gh api exited $LASTEXITCODE)." }

    $reviews = @($reviewsRaw | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review' })
    $reviewComments = @($reviewCommentsRaw | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review-comment' })
    return @($reviews + $reviewComments)
}

function Get-CommitsForPR {
    param([int]$Number)

    # Fail loud (see Get-ActivityForPR) — a dropped commit list would mislead the
    # new-head-commit eligibility check.
    $commitsRaw = gh api "repos/$Owner/$Repo/pulls/$Number/commits?per_page=100" --paginate --jq '.[]'
    if ($LASTEXITCODE -ne 0) { throw "Failed to fetch commits for #$Number (gh api exited $LASTEXITCODE)." }
    return @($commitsRaw | ForEach-Object { $_ | ConvertFrom-Json })
}

function Get-LatestScannerDecline {
    param([object[]]$Comments)

    # New markers retain the scan-time activity checkpoint as Unix milliseconds.
    # This keeps activity that races the later decline write newer than the
    # checkpoint. Legacy head-only markers fall back to their creation time.
    $markerPattern = '<!--\s*agent-rerun-declined:([0-9a-fA-F]{40})(?::([0-9]{10,16}))?\s*-->'
    $markers = @($Comments | Where-Object {
        $_.kind -eq 'issue-comment' -and
        $_.user -and
        (Normalize-GitHubActorLogin ([string]$_.user.login)) -eq 'github-actions[bot]' -and
        ([string]$_.body) -match $markerPattern
    } | ForEach-Object {
        $match = [regex]::Match([string]$_.body, $markerPattern)
        $declinedAt = Get-ObjectDate $_ 'created_at'
        if ($match.Groups[2].Success) {
            $checkpointMilliseconds = [Int64]0
            if ([Int64]::TryParse($match.Groups[2].Value, [ref]$checkpointMilliseconds)) {
                try {
                    $declinedAt = [DateTimeOffset]::FromUnixTimeMilliseconds($checkpointMilliseconds)
                } catch {
                    # Preserve compatibility by using the marker creation time.
                }
            }
        }
        [pscustomobject]@{
            CommentId  = [Int64]$_.id
            DeclinedAt = $declinedAt
            HeadSha    = $match.Groups[1].Value.ToLowerInvariant()
        }
    } | Sort-Object @{ Expression = { $_.DeclinedAt }; Descending = $true }, @{ Expression = { $_.CommentId }; Descending = $true })

    if ($markers.Count -eq 0) {
        return $null
    }

    # Callers dereference DeclinedAt and HeadSha directly.
    return $markers[0]
}

function Invoke-AutoRerunCandidateScan {
    param(
        [string]$ScanOwner = $Owner,
        [string]$ScanRepo = $Repo,
        [int]$ScanLimit = $Limit,
        [switch]$ScanDryRun = $DryRun,
        [string]$ScanOutputPath = $OutputPath
    )

    $Owner = $ScanOwner
    $Repo = $ScanRepo
    $Limit = $ScanLimit
    $DryRun = $ScanDryRun
    $OutputPath = $ScanOutputPath

    # Fetch one sentinel item beyond the processing ceiling. This preserves the
    # safety bound while making truncation exact instead of guessing when Count == Limit.
    $fetchLimit = $Limit + 1
    $searchJson = gh pr list `
        --repo "$Owner/$Repo" `
        --state open `
        --limit $fetchLimit `
        --json number,title,url,headRefOid,isDraft,labels,author
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to list open PRs (gh pr list exited with code $LASTEXITCODE)."
    }
    $listedPRs = @($searchJson | ConvertFrom-Json)
    $truncated = $listedPRs.Count -gt $Limit
    $openPRs = @($listedPRs | Select-Object -First $Limit)

    if ($truncated) {
        Write-Host "::warning::Open PR scan truncated to the newest $Limit PR(s); at least $($listedPRs.Count) were returned. Older PRs were not evaluated."
    }

    Write-Host "Inspecting $($openPRs.Count) open PR(s) for autonomous rerun eligibility..."

    $labelEnsured = $false
    $decisions = @()
    $appliedCount = 0
    $applyFailureCount = 0

    foreach ($pr in $openPRs) {
        $number = [int]$pr.number
        $title = [string]$pr.title

        if ($pr.isDraft) {
            $decisions += [pscustomobject]@{ prNumber = $number; title = $title; eligible = $false; reason = 'draft'; applied = $false }
            continue
        }

        # Per-PR error isolation: a single malformed PR or transient API failure
        # must not abort the whole daily scan. Aggregate errors after the loop so
        # systemic failures cannot produce a misleading green run.
        try {
            # gh pr list already fetched labels, so avoid another Issues API call.
            $labels = @(@($pr.labels) | Where-Object { $_ } | ForEach-Object { $_.name })

            # Treat a stale in-progress label as absent so a wedged review can recover.
            $effectiveLabels = @($labels)
            if ($labels -contains $ReviewInProgressLabel -and (Test-AgentReviewInProgressIsStale -PRNumber $number -Owner $Owner -Repo $Repo)) {
                $effectiveLabels = @($labels | Where-Object { $_ -ne $ReviewInProgressLabel })
            }

            $issueComments = @(Get-IssueCommentsForPR -Number $number)
            $reviewActivity = @(Get-ReviewActivityForPR -Number $number)
            $activity = @($issueComments + $reviewActivity)
            $commits = @()
            if (Get-LatestAISummaryComment -Comments $activity) {
                $commits = @(Get-CommitsForPR -Number $number)
            }
            $rawAuthorLogin = if ($pr.author -and $pr.author.login) { [string]$pr.author.login } else { '' }
            $authorLogin = Normalize-GitHubActorLogin $rawAuthorLogin

            $result = Resolve-AutonomousRerunEligibility `
                -Comments $activity `
                -Commits $commits `
                -CurrentHeadSha $pr.headRefOid `
                -PRAuthorLogin $authorLogin `
                -CurrentLabels $effectiveLabels

            $alreadyPresent = @($labels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
            $hasDeclinedMarker = @($labels | Where-Object { $_ -eq $RerunDeclinedLabel }).Count -gt 0

            # Only a trusted scanner marker advances the checkpoint. It carries the
            # exact declined head so a push racing the marker write always re-qualifies,
            # even when the commit timestamp predates the marker timestamp.
            if (-not $alreadyPresent -and $hasDeclinedMarker) {
                $lastDecline = Get-LatestScannerDecline -Comments $issueComments
                if ($lastDecline) {
                    $result = Resolve-AutonomousRerunEligibility `
                        -Comments $activity `
                        -Commits $commits `
                        -CurrentHeadSha $pr.headRefOid `
                        -PRAuthorLogin $authorLogin `
                        -CurrentLabels $effectiveLabels `
                        -LastDeclinedAt $lastDecline.DeclinedAt.ToString('o') `
                        -LastDeclinedHeadSha $lastDecline.HeadSha
                } else {
                    Write-Host "::warning::PR #$number has $RerunDeclinedLabel but no trusted head marker; ignoring the stale marker."
                }
            }

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
                        if ($hasDeclinedMarker -and -not (Remove-Label -PRNumber $number -LabelName $RerunDeclinedLabel -Owner $Owner -Repo $Repo)) {
                            Write-Host "::warning::Applied $ReadyForRerunLabel to #$number but could not clear $RerunDeclinedLabel."
                        }
                        Write-Host "  ✅ Applied $ReadyForRerunLabel to #$number ($($result.Reason)): $title" -ForegroundColor Green
                    } else {
                        Write-Host "  ⚠️  Failed to apply $ReadyForRerunLabel to #$number" -ForegroundColor Yellow
                        Write-Host "::warning::Auto-rerun label application failed for PR #$number."
                        $applyFailureCount++
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
            Write-Host "  ⚠️  Skipping #$number due to evaluation error: $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host "::warning::Auto-rerun evaluation failed for PR #$number; see the preceding log line."
            $decisions += [pscustomobject]@{
                prNumber       = $number
                title          = $title
                eligible       = $false
                reason         = "error: $($_.Exception.Message)"
                alreadyPresent = $false
                applied        = $false
            }
        }
    }

    $eligibleCount = @($decisions | Where-Object { $_.eligible -and -not $_.alreadyPresent }).Count
    $errorCount = @($decisions | Where-Object { $_.reason -like 'error:*' }).Count
    $evaluatedCount = @($decisions | Where-Object { $_.reason -ne 'draft' }).Count
    $systemicThreshold = [Math]::Max(3, [Math]::Ceiling($evaluatedCount * 0.10))
    $systemicFailure = $evaluatedCount -gt 0 -and (
        $errorCount -eq $evaluatedCount -or
        $errorCount -ge $systemicThreshold
    )
    $shouldFail = ($DryRun -and $errorCount -gt 0) -or $systemicFailure -or $applyFailureCount -gt 0

    if ($errorCount -gt 0) {
        Write-Host "::warning::Autonomous rerun scan encountered $errorCount evaluation error(s) across $evaluatedCount evaluated PR(s)."
    }
    if ($applyFailureCount -gt 0) {
        Write-Host "::warning::Autonomous rerun scan failed to apply $ReadyForRerunLabel to $applyFailureCount eligible PR(s)."
    }

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
            generatedAt       = (Get-Date).ToUniversalTime().ToString('o')
            dryRun            = [bool]$DryRun
            applied           = $appliedCount
            applyFailures     = $applyFailureCount
            eligible          = $eligibleCount
            errors            = $errorCount
            evaluated         = $evaluatedCount
            systemicThreshold = $systemicThreshold
            systemicFailure   = [bool]$systemicFailure
            scan              = @{
                limit         = $Limit
                fetchedCount  = $openPRs.Count
                observedCount = $listedPRs.Count
                truncated     = [bool]$truncated
            }
            decisions         = @($decisions)
        } | ConvertTo-Json -Depth 10
        $summary | Set-Content -LiteralPath $OutputPath -Encoding UTF8
        Write-Host "Wrote decision summary to $OutputPath"
    }

    if ($shouldFail) {
        if ($applyFailureCount -gt 0) {
            throw "Autonomous rerun scan failed: $applyFailureCount label application failure(s); $errorCount of $evaluatedCount evaluated PR(s) had errors."
        }
        throw "Autonomous rerun scan failed: $errorCount of $evaluatedCount evaluated PR(s) had errors."
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

Invoke-AutoRerunCandidateScan `
    -ScanOwner $Owner `
    -ScanRepo $Repo `
    -ScanLimit $Limit `
    -ScanDryRun:$DryRun `
    -ScanOutputPath $OutputPath
