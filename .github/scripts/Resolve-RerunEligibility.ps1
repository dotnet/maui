#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Determines whether a /review rerun request should mark a PR ready for rerun.

.DESCRIPTION
    This script is intentionally deterministic: it never uses AI and never
    inspects untrusted text semantically. A rerun is eligible only when there is
    new PR activity after the previous AI Summary or previous /review rerun:
    a new non-command comment, or a new commit.
#>

param(
    [int]$PRNumber = 0,

    [Int64]$CurrentCommentId = 0,

    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',

    [string]$ContextOutputPath,

    [switch]$ApplyLabel
)

$ErrorActionPreference = 'Stop'
$AISummaryMarker = '<!-- AI Summary -->'
$ReadyForRerunLabel = 's/agent-ready-for-rerun'
$ReviewInProgressLabel = 's/agent-review-in-progress'
$ReadyForRerunLabelDescription = 'AI review has new PR activity and is ready for rerun'
$ReadyForRerunLabelColor = '5319E7'
$AISummaryAuthorLogins = @(
    'MauiBot'
)

function ConvertTo-DateTimeOffset {
    param([Parameter(Mandatory = $true)]$Value)

    if ($Value -is [datetimeoffset]) {
        return $Value
    }
    if ($Value -is [datetime]) {
        return [datetimeoffset]$Value
    }
    return [datetimeoffset]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::AssumeUniversal)
}

function Test-RerunCommand {
    param([string]$Body)

    return ([string]$Body).Trim() -match '(?i)^/review\s+rerun\s*$'
}

function Test-AISummaryCommentAuthor {
    param([object]$Comment)

    $login = if ($Comment.user) { [string]$Comment.user.login } else { '' }
    if ([string]::IsNullOrWhiteSpace($login)) {
        return $false
    }

    return @($AISummaryAuthorLogins | Where-Object { $_ -ieq $login }).Count -gt 0
}

function Normalize-ReviewPipelineRef {
    param([string]$Value)

    $pipelineRef = if ([string]::IsNullOrWhiteSpace($Value)) { 'main' } else { ([string]$Value).Trim() }
    $pipelineRef = $pipelineRef -replace '^refs/heads/', ''
    $pipelineRef = $pipelineRef -replace '[^a-zA-Z0-9/_.\-]', ''
    if ([string]::IsNullOrWhiteSpace($pipelineRef)) {
        return 'main'
    }
    if ($pipelineRef -match '\.\.' -or $pipelineRef -match '//' -or $pipelineRef.EndsWith('/') -or $pipelineRef.StartsWith('/')) {
        return 'main'
    }
    return $pipelineRef
}

function ConvertFrom-ReviewCommand {
    param([string]$Body)

    $trimmed = ([string]$Body).Trim()
    if ($trimmed -notmatch '(?i)^/review(\s|$)') {
        return $null
    }
    if ($trimmed -match '(?i)^/review\s+(rerun|tests)(\s|$)') {
        return $null
    }

    $validPlatforms = @('android', 'ios', 'catalyst', 'windows')
    $argsText = [regex]::Replace($trimmed, '(?i)^/review\s*', '')
    $tokens = @()
    if (-not [string]::IsNullOrWhiteSpace($argsText)) {
        $tokens = @($argsText -split '\s+' | Where-Object { $_ })
    }

    $platform = ''
    $pipelineRef = 'main'
    for ($i = 0; $i -lt $tokens.Count; $i++) {
        $token = [string]$tokens[$i]
        if ($token -match '^(--branch|-b)=(.*)$') {
            $pipelineRef = Normalize-ReviewPipelineRef $Matches[2]
            continue
        }
        if ($token -match '^(--branch|-b)$') {
            if ($i + 1 -lt $tokens.Count -and -not ([string]$tokens[$i + 1]).StartsWith('--')) {
                $pipelineRef = Normalize-ReviewPipelineRef $tokens[$i + 1]
                $i++
            }
            continue
        }
        if ($token -match '^(--platform|-p)=(.*)$') {
            $candidate = $Matches[2].ToLowerInvariant()
            if ($validPlatforms -contains $candidate) {
                $platform = $candidate
            }
            continue
        }
        if ($token -match '^(--platform|-p)$') {
            if ($i + 1 -lt $tokens.Count -and -not ([string]$tokens[$i + 1]).StartsWith('--')) {
                $candidate = ([string]$tokens[$i + 1]).ToLowerInvariant()
                if ($validPlatforms -contains $candidate) {
                    $platform = $candidate
                }
                $i++
            }
            continue
        }

        $candidate = $token.ToLowerInvariant()
        if (-not $platform -and $validPlatforms -contains $candidate) {
            $platform = $candidate
        }
    }

    return [pscustomobject]@{
        IsReviewCommand = $true
        Platform        = $platform
        PipelineRef     = $pipelineRef
        Body            = $trimmed
    }
}

function Test-ReviewCommandOptionsAllowed {
    param(
        $Comment,
        [AllowNull()][string[]]$AllowedAuthorLogins = $null
    )

    # The comment's own author_association is always required to be in the trusted set,
    # so that a login which was trusted on an earlier comment cannot carry that trust
    # forward to a later comment posted after access changed.
    $association = if ($Comment.author_association) { [string]$Comment.author_association } else { '' }
    if ($association -notin @('OWNER', 'MEMBER', 'COLLABORATOR')) {
        return $false
    }

    if ($null -ne $AllowedAuthorLogins) {
        if (-not $Comment.user -or [string]::IsNullOrWhiteSpace($Comment.user.login)) {
            return $false
        }

        $login = ([string]$Comment.user.login).ToLowerInvariant()
        $allowed = @($AllowedAuthorLogins | ForEach-Object { ([string]$_).ToLowerInvariant() })
        return $allowed -contains $login
    }

    return $true
}

function Get-LatestReviewCommandOptions {
    param(
        [object[]]$Comments,
        [AllowNull()][string[]]$AllowedAuthorLogins = $null
    )

    $reviewCommands = @($Comments | Where-Object {
        $_.kind -eq 'issue-comment' -and
        (Test-ReviewCommandOptionsAllowed -Comment $_ -AllowedAuthorLogins $AllowedAuthorLogins) -and
        (ConvertFrom-ReviewCommand $_.body)
    } | Sort-Object @{ Expression = { Get-ObjectDate $_ 'created_at' }; Descending = $true }, @{ Expression = { [Int64]$_.id }; Descending = $true })

    if ($reviewCommands.Count -eq 0) {
        return [pscustomobject]@{
            Found       = $false
            Platform    = ''
            PipelineRef = 'main'
            CommentId   = $null
            Body        = ''
        }
    }

    $latest = $reviewCommands[0]
    $parsed = ConvertFrom-ReviewCommand $latest.body
    return [pscustomobject]@{
        Found       = $true
        Platform    = [string]$parsed.Platform
        PipelineRef = [string]$parsed.PipelineRef
        CommentId   = [Int64]$latest.id
        Body        = [string]$parsed.Body
        AuthorLogin  = if ($latest.user) { [string]$latest.user.login } else { '' }
    }
}

function Get-ObjectDate {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$PropertyName
    )

    $value = $Object.$PropertyName
    if ($null -eq $value) {
        return $null
    }

    return ConvertTo-DateTimeOffset $value
}

function Get-LatestAISummaryComment {
    param([object[]]$Comments)

    return @($Comments |
        Where-Object {
            $_.body -and
            ([string]$_.body).Contains($AISummaryMarker) -and
            (Test-AISummaryCommentAuthor $_)
        } |
        Sort-Object @{ Expression = { Get-ObjectDate $_ 'created_at' }; Descending = $true }, @{ Expression = { [Int64]$_.id }; Descending = $true } |
        Select-Object -First 1)
}

function Get-LatestRerunCommentBefore {
    param(
        [object[]]$Comments,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId
    )

    $current = @($Comments | Where-Object { [Int64]$_.id -eq $CurrentCommentId } | Select-Object -First 1)
    if (-not $current) {
        return $null
    }

    $currentCreatedAt = Get-ObjectDate $current 'created_at'
    return @($Comments |
        Where-Object {
            [Int64]$_.id -ne $CurrentCommentId -and
            (Test-RerunCommand $_.body) -and
            (Get-ObjectDate $_ 'created_at') -lt $currentCreatedAt
        } |
        Sort-Object @{ Expression = { Get-ObjectDate $_ 'created_at' }; Descending = $true }, @{ Expression = { [Int64]$_.id }; Descending = $true } |
        Select-Object -First 1)
}

function Get-LatestRerunComment {
    param([object[]]$Comments)

    return @($Comments |
        Where-Object { Test-RerunCommand $_.body } |
        Sort-Object @{ Expression = { Get-ObjectDate $_ 'created_at' }; Descending = $true }, @{ Expression = { [Int64]$_.id }; Descending = $true } |
        Select-Object -First 1)
}

function Get-LatestReviewedSha {
    param([string]$AISummaryBody)

    if ([string]::IsNullOrWhiteSpace($AISummaryBody)) {
        return $null
    }

    $matches = [regex]::Matches($AISummaryBody, '<!--\s*SESSION:([0-9a-fA-F]{7,40})\s+START\s*-->')
    if ($matches.Count -eq 0) {
        return $null
    }

    return $matches[0].Groups[1].Value.ToLowerInvariant()
}

function Test-CommentIsEvidence {
    param(
        [Parameter(Mandatory = $true)]$Comment,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId
    )

    if ([Int64]$Comment.id -eq $CurrentCommentId) {
        return $false
    }
    if (Test-RerunCommand $Comment.body) {
        return $false
    }
    if ($Comment.user -and $Comment.user.type -eq 'Bot') {
        return $false
    }
    if (Test-AISummaryCommentAuthor $Comment) {
        return $false
    }

    return $true
}

function Test-HasEvidenceCommentAfter {
    param(
        [object[]]$Comments,
        [Parameter(Mandatory = $true)][datetimeoffset]$Checkpoint,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId
    )

    return [bool]@($Comments | Where-Object {
        (Test-CommentIsEvidence -Comment $_ -CurrentCommentId $CurrentCommentId) -and
        (Get-ObjectDate $_ 'created_at') -gt $Checkpoint
    } | Select-Object -First 1)
}

function Test-HasCommitAfter {
    param(
        [object[]]$Commits,
        [Parameter(Mandatory = $true)][datetimeoffset]$Checkpoint
    )

    return [bool]@($Commits | Where-Object {
        $date = $null
        if ($_.commit -and $_.commit.committer -and $_.commit.committer.date) {
            $date = ConvertTo-DateTimeOffset $_.commit.committer.date
        } elseif ($_.commit -and $_.commit.author -and $_.commit.author.date) {
            $date = ConvertTo-DateTimeOffset $_.commit.author.date
        }

        $date -and $date -gt $Checkpoint
    } | Select-Object -First 1)
}

function Get-CommitDate {
    param($Commit)

    if ($Commit.commit -and $Commit.commit.committer -and $Commit.commit.committer.date) {
        return ConvertTo-DateTimeOffset $Commit.commit.committer.date
    }
    if ($Commit.commit -and $Commit.commit.author -and $Commit.commit.author.date) {
        return ConvertTo-DateTimeOffset $Commit.commit.author.date
    }
    return $null
}

function Test-HeadDiffersFromReviewedSha {
    param(
        [string]$CurrentHeadSha,
        [string]$LatestReviewedSha
    )

    if ([string]::IsNullOrWhiteSpace($CurrentHeadSha) -or [string]::IsNullOrWhiteSpace($LatestReviewedSha)) {
        return $false
    }

    return -not $CurrentHeadSha.ToLowerInvariant().StartsWith($LatestReviewedSha.ToLowerInvariant())
}

function ConvertTo-RerunActivityItem {
    param(
        [Parameter(Mandatory = $true)]$Item,
        [Parameter(Mandatory = $true)][string]$Kind
    )

    $createdAt = $Item.created_at
    if ($Kind -eq 'review') {
        $createdAt = $Item.submitted_at
    }

    $updatedAt = $Item.updated_at
    if ($null -eq $updatedAt) {
        $updatedAt = $createdAt
    }

    return [pscustomobject]@{
        id         = [Int64]$Item.id
        kind       = $Kind
        body       = [string]$Item.body
        created_at = $createdAt
        updated_at = $updatedAt
        user       = $Item.user
        author_association = $Item.author_association
    }
}

function Format-MarkdownCell {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ''
    }

    $singleLine = ($Value -replace '\r?\n', ' ').Trim()
    if ($singleLine.Length -gt 180) {
        $singleLine = $singleLine.Substring(0, 177) + '...'
    }

    return ($singleLine -replace '\|', '\|')
}

function New-RerunContextMarkdown {
    param(
        [object[]]$Comments,
        [object[]]$Commits,
        [string]$CurrentHeadSha,
        [object[]]$CurrentLabels = @()
    )

    $latestSummary = Get-LatestAISummaryComment -Comments $Comments
    $latestRerun = Get-LatestRerunComment -Comments $Comments
    $checkpointRerun = if ($latestRerun) { Get-LatestRerunCommentBefore -Comments $Comments -CurrentCommentId ([Int64]$latestRerun.id) } else { $null }
    $readyLabelPresent = @($CurrentLabels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
    $inProgressLabelPresent = @($CurrentLabels | Where-Object { $_ -eq $ReviewInProgressLabel }).Count -gt 0

    $latestReviewedSha = if ($latestSummary) { Get-LatestReviewedSha -AISummaryBody $latestSummary.body } else { $null }
    $summaryCreatedAt = if ($latestSummary) { Get-ObjectDate $latestSummary 'created_at' } else { $null }

    $checkpoint = $summaryCreatedAt
    $checkpointReason = if ($latestSummary) { 'latest AI Summary' } else { 'none' }
    if ($checkpointRerun) {
        $checkpointRerunCreatedAt = Get-ObjectDate $checkpointRerun 'created_at'
        if (-not $checkpoint -or $checkpointRerunCreatedAt -gt $checkpoint) {
            $checkpoint = $checkpointRerunCreatedAt
            $checkpointReason = 'previous /review rerun'
        }
    }

    $evidenceComments = @()
    if ($checkpoint) {
        $evidenceComments = @($Comments | Where-Object {
            (Test-CommentIsEvidence -Comment $_ -CurrentCommentId 0) -and
            (Get-ObjectDate $_ 'created_at') -gt $checkpoint
        } | Sort-Object @{ Expression = { Get-ObjectDate $_ 'created_at' }; Descending = $false }, @{ Expression = { [Int64]$_.id }; Descending = $false })
    }

    $newCommits = @()
    if ($checkpoint) {
        $newCommits = @($Commits | Where-Object {
            $date = Get-CommitDate $_
            $date -and $date -gt $checkpoint
        } | Sort-Object @{ Expression = { Get-CommitDate $_ }; Descending = $false })
    }

    $headDiffers = Test-HeadDiffersFromReviewedSha -CurrentHeadSha $CurrentHeadSha -LatestReviewedSha $latestReviewedSha
    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('# Rerun Context')
    $lines.Add('')
    $lines.Add('This file was generated deterministically before pre-flight. No AI was used to decide or summarize this context.')
    $lines.Add('')
    $lines.Add('## Checkpoint')
    $lines.Add('')
    if ($latestSummary) {
        $lines.Add("- Latest AI Summary: $($latestSummary.kind) `#$($latestSummary.id)` created $($summaryCreatedAt.ToString('u'))")
    } else {
        $lines.Add('- Latest AI Summary: not found')
    }
    if ($latestRerun) {
        $lines.Add("- Latest `/review rerun`: comment `#$($latestRerun.id)` created $((Get-ObjectDate $latestRerun 'created_at').ToString('u'))")
    } else {
        $lines.Add('- Latest `/review rerun`: not found')
    }
    if ($checkpointRerun) {
        $lines.Add("- Previous `/review rerun` checkpoint: comment `#$($checkpointRerun.id)` created $((Get-ObjectDate $checkpointRerun 'created_at').ToString('u'))")
    }
    if ($checkpoint) {
        $lines.Add("- Activity checkpoint: $checkpointReason at $($checkpoint.ToString('u'))")
    } else {
        $lines.Add('- Activity checkpoint: none')
    }
    $lines.Add("- Latest reviewed SHA: $(if ($latestReviewedSha) { $latestReviewedSha } else { 'unknown' })")
    $lines.Add("- Current head SHA: $(if ($CurrentHeadSha) { $CurrentHeadSha } else { 'unknown' })")
    $lines.Add("- Current head differs from latest reviewed SHA: $($headDiffers.ToString().ToLowerInvariant())")
    $lines.Add("- ``$ReadyForRerunLabel`` present: $($readyLabelPresent.ToString().ToLowerInvariant())")
    $lines.Add("- ``$ReviewInProgressLabel`` present: $($inProgressLabelPresent.ToString().ToLowerInvariant())")
    $lines.Add('')
    $lines.Add('## New activity since checkpoint')
    $lines.Add('')
    $lines.Add("- New non-command comments: $($evidenceComments.Count)")
    $lines.Add("- New commits: $($newCommits.Count)")
    $lines.Add('')

    if ($evidenceComments.Count -gt 0) {
        $lines.Add('### New comments')
        $lines.Add('')
        $lines.Add('| Kind | Author | Created | Body |')
        $lines.Add('|---|---|---|---|')
        foreach ($comment in $evidenceComments) {
            $author = if ($comment.user) { [string]$comment.user.login } else { '' }
            $createdAt = (Get-ObjectDate $comment 'created_at').ToString('u')
            $lines.Add("| $($comment.kind) | $(Format-MarkdownCell $author) | $createdAt | $(Format-MarkdownCell $comment.body) |")
        }
        $lines.Add('')
    }

    if ($newCommits.Count -gt 0) {
        $lines.Add('### New commits')
        $lines.Add('')
        $lines.Add('| SHA | Author | Date | Message |')
        $lines.Add('|---|---|---|---|')
        foreach ($commit in $newCommits) {
            $sha = if ($commit.sha) { ([string]$commit.sha).Substring(0, [Math]::Min(7, ([string]$commit.sha).Length)) } else { '' }
            $author = if ($commit.commit -and $commit.commit.author) { [string]$commit.commit.author.name } else { '' }
            $date = Get-CommitDate $commit
            $message = if ($commit.commit -and $commit.commit.message) { ([string]$commit.commit.message -split "`n")[0] } else { '' }
            $lines.Add("| $sha | $(Format-MarkdownCell $author) | $(if ($date) { $date.ToString('u') } else { '' }) | $(Format-MarkdownCell $message) |")
        }
        $lines.Add('')
    }

    if ($evidenceComments.Count -eq 0 -and $newCommits.Count -eq 0 -and -not $headDiffers) {
        $lines.Add('No new deterministic activity was found since the checkpoint.')
        $lines.Add('')
    }

    return ($lines -join "`n")
}

function Resolve-RerunEligibility {
    param(
        [object[]]$Comments,
        [object[]]$Commits,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId,
        [string]$CurrentHeadSha,
        [object[]]$CurrentLabels = @()
    )

    $current = @($Comments | Where-Object { [Int64]$_.id -eq $CurrentCommentId } | Select-Object -First 1)
    if (-not $current) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'current-comment-not-found'; Label = $ReadyForRerunLabel }
    }

    if (-not (Test-RerunCommand $current.body)) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'not-rerun-command'; Label = $ReadyForRerunLabel }
    }

    if ($current.user -and ($current.user.type -eq 'Bot' -or (Test-AISummaryCommentAuthor $current))) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'bot-comment'; Label = $ReadyForRerunLabel }
    }

    if (@($CurrentLabels | Where-Object { $_ -eq $ReviewInProgressLabel }).Count -gt 0) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'review-in-progress'; Label = $ReadyForRerunLabel }
    }

    $latestSummary = Get-LatestAISummaryComment -Comments $Comments
    if (-not $latestSummary) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'no-ai-summary'; Label = $ReadyForRerunLabel }
    }

    if (@($CurrentLabels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0) {
        return [pscustomobject]@{ Eligible = $true; Reason = 'label-already-present'; Label = $ReadyForRerunLabel }
    }

    $summaryCreatedAt = Get-ObjectDate $latestSummary 'created_at'
    $latestReviewedSha = Get-LatestReviewedSha -AISummaryBody $latestSummary.body
    $previousRerun = Get-LatestRerunCommentBefore -Comments $Comments -CurrentCommentId $CurrentCommentId
    $checkpoint = $summaryCreatedAt
    $checkpointReason = 'ai-summary'
    if ($previousRerun) {
        $previousRerunCreatedAt = Get-ObjectDate $previousRerun 'created_at'
        if ($previousRerunCreatedAt -gt $checkpoint) {
            $checkpoint = $previousRerunCreatedAt
            $checkpointReason = 'previous-rerun'
        }
    }

    if ($checkpointReason -eq 'ai-summary' -and (Test-HeadDiffersFromReviewedSha -CurrentHeadSha $CurrentHeadSha -LatestReviewedSha $latestReviewedSha)) {
        return [pscustomobject]@{ Eligible = $true; Reason = 'new-head-commit'; Label = $ReadyForRerunLabel }
    }

    if (Test-HasEvidenceCommentAfter -Comments $Comments -Checkpoint $checkpoint -CurrentCommentId $CurrentCommentId) {
        $reason = if ($checkpointReason -eq 'previous-rerun') { 'new-comment-after-previous-rerun' } else { 'new-comment-after-ai-summary' }
        return [pscustomobject]@{ Eligible = $true; Reason = $reason; Label = $ReadyForRerunLabel }
    }

    if (Test-HasCommitAfter -Commits $Commits -Checkpoint $checkpoint) {
        $reason = if ($checkpointReason -eq 'previous-rerun') { 'new-commit-after-previous-rerun' } else { 'new-commit-after-ai-summary' }
        return [pscustomobject]@{ Eligible = $true; Reason = $reason; Label = $ReadyForRerunLabel }
    }

    return [pscustomobject]@{ Eligible = $false; Reason = 'no-new-comments-or-commits'; Label = $ReadyForRerunLabel }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

if ($PRNumber -le 0) {
    throw "PRNumber is required when running Resolve-RerunEligibility.ps1 directly."
}

$issueComments = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/comments?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'issue-comment' })
$reviews = @(gh api "repos/$Owner/$Repo/pulls/$PRNumber/reviews?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review' })
$reviewComments = @(gh api "repos/$Owner/$Repo/pulls/$PRNumber/comments?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review-comment' })
$comments = @($issueComments + $reviews + $reviewComments)
$pr = gh api "repos/$Owner/$Repo/pulls/$PRNumber" | ConvertFrom-Json
$commits = @(gh api "repos/$Owner/$Repo/pulls/$PRNumber/commits?per_page=100" --paginate --jq '.[]' | ForEach-Object { $_ | ConvertFrom-Json })
$labels = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" --jq '.[].name' 2>$null)

if ($pr.state -ne 'open') {
    throw "PR #$PRNumber is not open (state: $($pr.state))"
}

if ($ContextOutputPath) {
    $context = New-RerunContextMarkdown `
        -Comments $comments `
        -Commits $commits `
        -CurrentHeadSha $pr.head.sha `
        -CurrentLabels $labels
    $contextDir = Split-Path -Parent $ContextOutputPath
    if ($contextDir) {
        New-Item -ItemType Directory -Force -Path $contextDir | Out-Null
    }
    $context | Set-Content -LiteralPath $ContextOutputPath -Encoding UTF8
    Write-Host "Wrote rerun context: $ContextOutputPath"
    if ($env:GITHUB_OUTPUT) {
        "context_output_path=$ContextOutputPath" >> $env:GITHUB_OUTPUT
    }
    if ($CurrentCommentId -eq 0 -and -not $ApplyLabel) {
        exit 0
    }
}

if ($CurrentCommentId -eq 0) {
    throw "CurrentCommentId is required unless only writing ContextOutputPath."
}

$result = Resolve-RerunEligibility `
    -Comments $comments `
    -Commits $commits `
    -CurrentCommentId $CurrentCommentId `
    -CurrentHeadSha $pr.head.sha `
    -CurrentLabels $labels

Write-Host "Rerun eligibility: $($result.Eligible) ($($result.Reason))"

if ($env:GITHUB_OUTPUT) {
    "eligible=$($result.Eligible.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
    "reason=$($result.Reason)" >> $env:GITHUB_OUTPUT
    "label=$($result.Label)" >> $env:GITHUB_OUTPUT
}

if ($ApplyLabel -and $result.Eligible) {
    . "$PSScriptRoot/shared/Update-AgentLabels.ps1"
    Ensure-LabelExists `
        -LabelName $ReadyForRerunLabel `
        -Description $ReadyForRerunLabelDescription `
        -Color $ReadyForRerunLabelColor `
        -Owner $Owner `
        -Repo $Repo

    $alreadyPresent = @($labels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
    if ($alreadyPresent) {
        Write-Host "  ✅ Already present: $ReadyForRerunLabel" -ForegroundColor Green
    } else {
        $addSucceeded = Add-Label -PRNumber $PRNumber -LabelName $ReadyForRerunLabel -Owner $Owner -Repo $Repo
        $updatedLabels = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" --jq '.[].name' 2>$null)
        $labelIsPresent = @($updatedLabels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
        if ($addSucceeded -or $labelIsPresent) {
            Write-Host "  ✅ Applied: $ReadyForRerunLabel" -ForegroundColor Green
        } else {
            throw "Failed to apply label: $ReadyForRerunLabel"
        }
    }
}
