#!/usr/bin/env pwsh

$script:MauiBotCommentAuthors = @(
    'MauiBot',
    'maui-bot',
    'maui-bot[bot]',
    'github-actions[bot]'
)

$script:AiSummaryCommentMarker = '<!-- AI Summary -->'
$script:AiGateCommentMarker = '<!-- AI Gate -->'
$script:MergeConflictCommentMarker = '<!-- MAUI_BOT_MERGE_CONFLICT -->'
$script:TryFixCommentMarker = '<!-- MAUI_BOT_TRY_FIX -->'

function Test-IsMauiBotCommentAuthor {
    param([object]$Comment)

    $login = [string]$Comment.user.login
    if ([string]::IsNullOrWhiteSpace($login)) {
        return $false
    }

    return @($script:MauiBotCommentAuthors | Where-Object { $_ -ieq $login }).Count -gt 0
}

function Test-IsMergeConflictCommentBody {
    param([string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return $false
    }

    return $Body.Contains($script:MergeConflictCommentMarker) -or
        ($Body.Contains('**Merge Conflict Detected**') -and $Body.Contains('This PR has merge conflicts with its target branch.'))
}

function Test-IsTryFixCommentBody {
    param([string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return $false
    }

    return $Body.Contains($script:TryFixCommentMarker) -or
        ($Body.Contains('Automated review') -and $Body.Contains('alternative fix proposed')) -or
        ($Body.Contains('try-fix-') -and $Body.Contains('Candidate diff'))
}

function Get-GitHubIssueComments {
    param([Parameter(Mandatory = $true)][int]$PRNumber)

    $raw = gh api "repos/dotnet/maui/issues/$PRNumber/comments?per_page=100" --paginate 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($raw)) {
        return @()
    }

    try {
        return @($raw | ConvertFrom-Json)
    } catch {
        Write-Host "  Warning: could not parse PR comments for cleanup: $_" -ForegroundColor Yellow
        return @()
    }
}

function Remove-StaleMauiBotIssueComments {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [switch]$IncludeAISummary,
        [switch]$IncludeLegacyGate,
        [switch]$IncludeMergeConflict,
        [switch]$IncludeTryFix,

        [string]$Reason = 'stale MauiBot comment',
        [switch]$DryRun
    )

    $comments = Get-GitHubIssueComments -PRNumber $PRNumber
    if (-not $comments -or $comments.Count -eq 0) {
        return
    }

    $staleComments = @()
    foreach ($comment in $comments) {
        $body = [string]$comment.body
        if ([string]::IsNullOrWhiteSpace($body)) {
            continue
        }

        $matchesGeneratedMarker =
            ($IncludeAISummary -and $body.Contains($script:AiSummaryCommentMarker)) -or
            ($IncludeLegacyGate -and $body.Contains($script:AiGateCommentMarker))

        $matchesBotOnlyContent =
            (Test-IsMauiBotCommentAuthor $comment) -and (
                ($IncludeMergeConflict -and (Test-IsMergeConflictCommentBody $body)) -or
                ($IncludeTryFix -and (Test-IsTryFixCommentBody $body))
            )

        if ($matchesGeneratedMarker -or $matchesBotOnlyContent) {
            $staleComments += $comment
        }
    }

    foreach ($comment in $staleComments) {
        if ($DryRun) {
            Write-Host "  [DryRun] Would delete $Reason (comment ID: $($comment.id))" -ForegroundColor Magenta
            continue
        }

        try {
            Write-Host "  Deleting $Reason (comment ID: $($comment.id))..." -ForegroundColor Gray
            $deleteOutput = gh api --method DELETE "repos/dotnet/maui/issues/comments/$($comment.id)" 2>&1
            if ($LASTEXITCODE -ne 0) {
                throw "DELETE failed (exit code $LASTEXITCODE): $deleteOutput"
            }
        } catch {
            Write-Host "  Warning: could not delete $Reason comment $($comment.id): $_" -ForegroundColor Yellow
        }
    }
}

function Get-GitHubPullRequestReviews {
    param([Parameter(Mandatory = $true)][int]$PRNumber)

    $raw = gh api "repos/dotnet/maui/pulls/$PRNumber/reviews?per_page=100" --paginate 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($raw)) {
        return @()
    }

    try {
        return @($raw | ConvertFrom-Json)
    } catch {
        Write-Host "  Warning: could not parse PR reviews for cleanup: $_" -ForegroundColor Yellow
        return @()
    }
}

function Dismiss-StaleMauiBotTryFixReviews {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [string]$Reason = 'superseded MauiBot try-fix review',
        [switch]$DryRun
    )

    $reviews = Get-GitHubPullRequestReviews -PRNumber $PRNumber
    if (-not $reviews -or $reviews.Count -eq 0) {
        return
    }

    $staleReviews = @($reviews | Where-Object {
        (Test-IsMauiBotCommentAuthor $_) -and
        ([string]$_.state -ieq 'CHANGES_REQUESTED') -and
        (Test-IsTryFixCommentBody ([string]$_.body))
    })

    foreach ($review in $staleReviews) {
        if ($DryRun) {
            Write-Host "  [DryRun] Would dismiss $Reason (review ID: $($review.id))" -ForegroundColor Magenta
            continue
        }

        $tmp = New-TemporaryFile
        try {
            @{ message = 'Superseded by a newer MauiBot review run.' } |
                ConvertTo-Json -Compress |
                Set-Content -LiteralPath $tmp -Encoding UTF8 -NoNewline

            Write-Host "  Dismissing $Reason (review ID: $($review.id))..." -ForegroundColor Gray
            $dismissOutput = gh api --method PUT "repos/dotnet/maui/pulls/$PRNumber/reviews/$($review.id)/dismissals" --input $tmp.FullName 2>&1
            if ($LASTEXITCODE -ne 0) {
                throw "dismissal failed (exit code $LASTEXITCODE): $dismissOutput"
            }
        } catch {
            Write-Host "  Warning: could not dismiss $Reason review $($review.id): $_" -ForegroundColor Yellow
        } finally {
            Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
        }
    }
}
