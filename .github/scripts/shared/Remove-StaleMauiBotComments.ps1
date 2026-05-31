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

    if ($Body.Contains($script:AiSummaryCommentMarker)) {
        return $false
    }

    return $Body.Contains($script:TryFixCommentMarker) -or
        ($Body.Contains('Automated review') -and $Body.Contains('alternative fix proposed')) -or
        ($Body.Contains('try-fix-') -and $Body.Contains('Candidate diff'))
}

function Test-IsAISummaryCommentBody {
    param([string]$Body)

    if ([string]::IsNullOrWhiteSpace($Body)) {
        return $false
    }

    return $Body.Contains($script:AiSummaryCommentMarker)
}

function Test-ShouldPreserveMauiBotArtifact {
    param(
        [object]$Artifact,
        [string[]]$PreserveNodeIds = @(),
        [string[]]$PreserveIds = @()
    )

    $nodeId = [string]$Artifact.node_id
    $id = [string]$Artifact.id

    return (
        (-not [string]::IsNullOrWhiteSpace($nodeId) -and $PreserveNodeIds -contains $nodeId) -or
        (-not [string]::IsNullOrWhiteSpace($id) -and $PreserveIds -contains $id)
    )
}

function Invoke-GitHubMinimizeComment {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SubjectNodeId,

        [ValidateSet('SPAM', 'ABUSE', 'OFF_TOPIC', 'OUTDATED', 'DUPLICATE', 'RESOLVED', 'LOW_QUALITY')]
        [string]$Classifier = 'OUTDATED',

        [string]$Reason = 'stale MauiBot artifact',

        [switch]$DryRun
    )

    if ([string]::IsNullOrWhiteSpace($SubjectNodeId)) {
        Write-Host "  Warning: cannot hide $Reason because node_id is empty" -ForegroundColor Yellow
        return $false
    }

    if ($DryRun) {
        Write-Host "  [DryRun] Would hide $Reason (node_id: $SubjectNodeId, classifier: $Classifier)" -ForegroundColor Magenta
        return $true
    }

    $query = @'
mutation MinimizeComment($subjectId: ID!, $classifier: ReportedContentClassifiers!) {
  minimizeComment(input: { subjectId: $subjectId, classifier: $classifier }) {
    minimizedComment {
      isMinimized
      minimizedReason
    }
  }
}
'@

    try {
        Write-Host "  Hiding $Reason (node_id: $SubjectNodeId, classifier: $Classifier)..." -ForegroundColor Gray
        $output = gh api graphql `
            -f query="$query" `
            -F subjectId="$SubjectNodeId" `
            -F classifier="$Classifier" 2>&1

        if ($LASTEXITCODE -ne 0) {
            throw "minimizeComment failed (exit code $LASTEXITCODE): $output"
        }

        return $true
    } catch {
        Write-Host "  Warning: could not hide $Reason with node_id ${SubjectNodeId}: $_" -ForegroundColor Yellow
        return $false
    }
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

function Hide-StaleMauiBotIssueComments {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [switch]$IncludeAISummary,
        [switch]$IncludeLegacyGate,
        [switch]$IncludeMergeConflict,
        [switch]$IncludeTryFix,

        [string[]]$PreserveNodeIds = @(),
        [string[]]$PreserveIds = @(),

        [ValidateSet('SPAM', 'ABUSE', 'OFF_TOPIC', 'OUTDATED', 'DUPLICATE', 'RESOLVED', 'LOW_QUALITY')]
        [string]$Classifier = 'OUTDATED',

        [string]$Reason = 'stale MauiBot comment',
        [switch]$DryRun
    )

    $comments = Get-GitHubIssueComments -PRNumber $PRNumber
    if (-not $comments -or $comments.Count -eq 0) {
        return
    }

    $staleComments = @()
    foreach ($comment in $comments) {
        if (Test-ShouldPreserveMauiBotArtifact -Artifact $comment -PreserveNodeIds $PreserveNodeIds -PreserveIds $PreserveIds) {
            continue
        }

        $body = [string]$comment.body
        if ([string]::IsNullOrWhiteSpace($body)) {
            continue
        }

        $matchesGeneratedMarker =
            ($IncludeAISummary -and (Test-IsAISummaryCommentBody $body)) -or
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
        Invoke-GitHubMinimizeComment `
            -SubjectNodeId ([string]$comment.node_id) `
            -Classifier $Classifier `
            -Reason "$Reason comment $($comment.id)" `
            -DryRun:$DryRun | Out-Null
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

        [string[]]$PreserveNodeIds = @(),
        [string[]]$PreserveIds = @(),

        [ValidateSet('SPAM', 'ABUSE', 'OFF_TOPIC', 'OUTDATED', 'DUPLICATE', 'RESOLVED', 'LOW_QUALITY')]
        [string]$Classifier = 'OUTDATED',

        [string]$Reason = 'stale MauiBot comment',
        [switch]$DryRun
    )

    Hide-StaleMauiBotIssueComments `
        -PRNumber $PRNumber `
        -IncludeAISummary:$IncludeAISummary `
        -IncludeLegacyGate:$IncludeLegacyGate `
        -IncludeMergeConflict:$IncludeMergeConflict `
        -IncludeTryFix:$IncludeTryFix `
        -PreserveNodeIds $PreserveNodeIds `
        -PreserveIds $PreserveIds `
        -Classifier $Classifier `
        -Reason $Reason `
        -DryRun:$DryRun
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

function Dismiss-MauiBotPullRequestReview {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [Parameter(Mandatory = $true)]
        [object]$Review,

        [string]$Reason = 'Superseded by a newer MauiBot review run.',
        [switch]$DryRun
    )

    if ($DryRun) {
        Write-Host "  [DryRun] Would dismiss stale review ID $($Review.id)" -ForegroundColor Magenta
        return $true
    }

    $tmp = New-TemporaryFile
    try {
        @{ message = $Reason } |
            ConvertTo-Json -Compress |
            Set-Content -LiteralPath $tmp -Encoding UTF8 -NoNewline

        Write-Host "  Dismissing stale review ID $($Review.id)..." -ForegroundColor Gray
        $dismissOutput = gh api --method PUT "repos/dotnet/maui/pulls/$PRNumber/reviews/$($Review.id)/dismissals" --input $tmp.FullName 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "dismissal failed (exit code $LASTEXITCODE): $dismissOutput"
        }

        return $true
    } catch {
        Write-Host "  Warning: could not dismiss review $($Review.id): $_" -ForegroundColor Yellow
        return $false
    } finally {
        Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
    }
}

function Hide-StaleMauiBotPullRequestReviews {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [switch]$IncludeAISummary,
        [switch]$IncludeTryFix,

        [string[]]$PreserveNodeIds = @(),
        [string[]]$PreserveIds = @(),

        [ValidateSet('SPAM', 'ABUSE', 'OFF_TOPIC', 'OUTDATED', 'DUPLICATE', 'RESOLVED', 'LOW_QUALITY')]
        [string]$Classifier = 'OUTDATED',

        [string]$Reason = 'stale MauiBot review',
        [switch]$DismissChangesRequested,
        [switch]$DismissFormalReviews,
        [switch]$DryRun
    )

    $reviews = Get-GitHubPullRequestReviews -PRNumber $PRNumber
    if (-not $reviews -or $reviews.Count -eq 0) {
        return
    }

    $staleReviews = @()
    foreach ($review in $reviews) {
        if (Test-ShouldPreserveMauiBotArtifact -Artifact $review -PreserveNodeIds $PreserveNodeIds -PreserveIds $PreserveIds) {
            continue
        }

        $body = [string]$review.body
        if ([string]::IsNullOrWhiteSpace($body) -or -not (Test-IsMauiBotCommentAuthor $review)) {
            continue
        }

        $matchesReview =
            ($IncludeAISummary -and (Test-IsAISummaryCommentBody $body)) -or
            ($IncludeTryFix -and (Test-IsTryFixCommentBody $body))

        if ($matchesReview) {
            $staleReviews += $review
        }
    }

    foreach ($review in $staleReviews) {
        Invoke-GitHubMinimizeComment `
            -SubjectNodeId ([string]$review.node_id) `
            -Classifier $Classifier `
            -Reason "$Reason review $($review.id)" `
            -DryRun:$DryRun | Out-Null

        $reviewState = [string]$review.state
        $shouldDismiss =
            ($DismissFormalReviews -and $reviewState -in @('APPROVED', 'CHANGES_REQUESTED')) -or
            ($DismissChangesRequested -and $reviewState -ieq 'CHANGES_REQUESTED')

        if ($shouldDismiss) {
            Dismiss-MauiBotPullRequestReview `
                -PRNumber $PRNumber `
                -Review $review `
                -Reason 'Superseded by a newer MauiBot review run.' `
                -DryRun:$DryRun | Out-Null
        }
    }
}

function Dismiss-StaleMauiBotTryFixReviews {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [string[]]$PreserveNodeIds = @(),
        [string[]]$PreserveIds = @(),

        [string]$Reason = 'superseded MauiBot try-fix review',
        [switch]$DryRun
    )

    Hide-StaleMauiBotPullRequestReviews `
        -PRNumber $PRNumber `
        -IncludeTryFix `
        -PreserveNodeIds $PreserveNodeIds `
        -PreserveIds $PreserveIds `
        -Classifier OUTDATED `
        -Reason $Reason `
        -DismissChangesRequested `
        -DryRun:$DryRun
}
